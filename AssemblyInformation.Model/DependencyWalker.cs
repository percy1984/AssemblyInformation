using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AssemblyInformation.Model
{
    public class DependencyWalker
    {
        private readonly Dictionary<string, Binary> assemblyMap = new Dictionary<string, Binary>();

        public event EventHandler<ReferringAssemblyStatusChangeEventArgs> ReferringAssemblyStatusChanged = delegate { };

        public IEnumerable<Binary> FindDependencies(AssemblyName assemblyName, bool recursive, out List<string> loadErrors)
        {
            loadErrors = new List<string>();
            assemblyMap.Clear();
            var dependencies = new List<Binary>();

            var assembly = FindAssembly(assemblyName);
            if (null == assembly)
            {
                loadErrors.Add("Failed to load: " + assemblyName.FullName);
            }
            else
            {
                FindDependencies(assembly, recursive);
                dependencies.AddRange(assemblyMap.Values.OrderBy(p => p.FullName));
            }

            foreach (var dependency in dependencies)
            {
                Trace.WriteLine($"{dependency.DisplayName} => {dependency.IsSystemBinary}");
            }

            return dependencies;
        }

        public IEnumerable<Binary> FindDependencies(Assembly assembly, bool recursive, out List<string> loadErrors)
        {
            loadErrors = new List<string>();
            assemblyMap.Clear();
            FindDependencies(assembly, recursive);
            var dependencies = new List<Binary>(assemblyMap.Values).OrderBy(p => p.FullName).ToList();

            foreach (var dependency in dependencies)
            {
                Trace.WriteLine(String.Format("{0} => {1}", dependency.DisplayName, dependency.IsSystemBinary));
            }

            return dependencies;
        }

        public IEnumerable<string> FindReferringAssemblies(Assembly testAssembly, string directory, bool recursive)
        {
            var referringAssemblies = new List<string>();
            var binaries = new List<string>();
            try
            {
                ReferringAssemblyStatusChanged(this, new ReferringAssemblyStatusChangeEventArgs { StatusText = "Finding all binaries" });
                FindAssemblies(new DirectoryInfo(directory), binaries, recursive);
            }
            catch (Exception)
            {
                UpdateProgress(Resource.FailedToListBinaries, -2);
                return null;
            }

            if (binaries.Count == 0)
            {
                return referringAssemblies;
            }

            var baseDirPathLength = directory.Length;
            if (!directory.EndsWith("\\"))
            {
                baseDirPathLength++;
            }

            var i = 0;
            foreach (var binary in binaries)
            {
                var message = String.Format(Resource.AnalyzingAssembly, Path.GetFileName(binary));
                var progress = (i++ * 100) / binaries.Count;
                if (progress == 100)
                {
                    progress = 99;
                }

                if (!UpdateProgress(message, progress))
                {
                    return referringAssemblies;
                }

                try
                {
                    var assembly = Assembly.LoadFile(binary);
                    var dw = new DependencyWalker();
                    var dependencies = dw.FindDependencies(assembly, false, out var loadErrors);
                    if (null == dependencies)
                    {
                        continue;
                    }

                    if (
                        dependencies.Any(p => string.Compare(p.FullName, testAssembly.FullName, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        referringAssemblies.Add(binary.Remove(0, baseDirPathLength));
                    }

                    loadErrors.AddRange(loadErrors);
                }
                catch (ArgumentException)
                {
                }
                catch (FileLoadException)
                {
                }
                catch (FileNotFoundException)
                {
                }
                catch (BadImageFormatException)
                {
                }
            }

            return referringAssemblies.OrderBy(p => p);
        }

        private static Assembly FindAssembly(AssemblyName assName)
        {
            var retryCount = 0;
            Assembly assembly = null;
            var assemblyName = assName.FullName;

            while (retryCount < 2)
            {
                retryCount++;
                try
                {
                    if (!File.Exists(assemblyName))
                    {
                        assembly = Assembly.Load(assemblyName);
                    }
                    else
                    {
                        var fileInfo = new FileInfo(assemblyName);
                        assembly = Assembly.LoadFile(fileInfo.FullName);
                    }

                    break;
                }
                catch (FileNotFoundException)
                {
                    if (assemblyName.EndsWith(".dll"))
                    {
                        continue;
                    }

                    var parts = assemblyName.Split(',');
                    if (parts.Length > 0)
                    {
                        var name = parts[0].Trim() + ".dll";
                        assemblyName = name;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (IOException)
                {
                }
                catch (BadImageFormatException)
                {
                }
            }

            return assembly;
        }

        private void FindAssemblies(DirectoryInfo directoryInfo, List<string> binaries, bool recursive)
        {
            var message = string.Format(Resource.AnalyzingFolder, directoryInfo.Name);
            if (!UpdateProgress(message, -1))
            {
                return;
            }

            binaries.AddRange(directoryInfo.GetFiles("*.dll").Select(fileInfo => fileInfo.FullName));
            binaries.AddRange(directoryInfo.GetFiles("*.exe").Select(fileInfo => fileInfo.FullName));

            if (!recursive) return;

            foreach (var directory in directoryInfo.GetDirectories())
            {
                FindAssemblies(directory, binaries, true);
            }
        }

        private void FindDependencies(Assembly assembly, bool recursive)
        {
            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                var name = referencedAssembly.FullName;

                if (assemblyMap.ContainsKey(name))
                {
                    continue;
                }

                assemblyMap[name] = new Binary(referencedAssembly);

                if (AssemblyInformationLoader.SystemAssemblies.Count(p => referencedAssembly.FullName.StartsWith(p)) > 0)
                {
                    assemblyMap[name].IsSystemBinary = true;
                    continue;
                }

                if (!recursive) continue;
                var referredAssembly = FindAssembly(referencedAssembly);

                if (null == referredAssembly) continue;
                assemblyMap[name] = new Binary(referencedAssembly, referredAssembly);
                FindDependencies(referredAssembly, true);
            }
        }

        private bool UpdateProgress(string message, int progress)
        {
            if (null == ReferringAssemblyStatusChanged) return true;
            var eventArg = new ReferringAssemblyStatusChangeEventArgs { StatusText = message, Progress = progress };
            ReferringAssemblyStatusChanged(this, eventArg);
            return !eventArg.Cancel;

        }
    }
}
