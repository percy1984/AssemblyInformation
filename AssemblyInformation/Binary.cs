using System.Reflection;

namespace AssemblyInformation
{
    internal class Binary
    {
        public Binary(AssemblyName assemblyName, string fullPath = null, bool isSystemBinary = false)
        {
            FullName = assemblyName.FullName;
            DisplayName = assemblyName.Name;
            FullPath = fullPath;
            IsSystemBinary = isSystemBinary;
        }

        public Binary(AssemblyName assemblyName, Assembly assembly)
            : this(assemblyName, assembly.Location, assembly.GlobalAssemblyCache)
        {
        }

        public string DisplayName { get; private set; }

        public string FullName { get; private set; }

        public string FullPath { get; private set; }

        public bool IsSystemBinary { get; set; }
    }
}
