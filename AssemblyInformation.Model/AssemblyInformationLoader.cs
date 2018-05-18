using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace AssemblyInformation.Model
{
    public class AssemblyInformationLoader : IAssemblyInformationLoader
    {
        public static readonly List<string> SystemAssemblies = new List<string>()
        {
            "System",
            "mscorlib",
            "Windows",
            "PresentationCore",
            "PresentationFramework",
            "Microsoft.VisualC"
        };

        private static readonly Dictionary<ImageFileMachine, string> ImageFileMachineNames =
            new Dictionary<ImageFileMachine, string>();

        private static readonly Dictionary<PortableExecutableKinds, string> PortableExecutableKindsNames =
            new Dictionary<PortableExecutableKinds, string>();

        static AssemblyInformationLoader()
        {
            PortableExecutableKindsNames[PortableExecutableKinds.ILOnly] =
                "Contains only Microsoft intermediate language (MSIL), and is therefore neutral with respect to 32-bit or 64-bit platforms.";
            PortableExecutableKindsNames[PortableExecutableKinds.NotAPortableExecutableImage] =
                "Not in portable executable (PE) file format.";
            PortableExecutableKindsNames[PortableExecutableKinds.PE32Plus] = "Requires a 64-bit platform.";
            PortableExecutableKindsNames[PortableExecutableKinds.Required32Bit] =
                "Can be run on a 32-bit platform, or in the 32-bit Windows on Windows (WOW) environment on a 64-bit platform.";
            PortableExecutableKindsNames[PortableExecutableKinds.Unmanaged32Bit] = "Contains pure unmanaged code.";
            PortableExecutableKindsNames[PortableExecutableKinds.Preferred32Bit] = "Platform-agnostic, but 32-bit preferred.";

            ImageFileMachineNames[ImageFileMachine.I386] = "Targets a 32-bit Intel processor.";
            ImageFileMachineNames[ImageFileMachine.IA64] = "Targets a 64-bit Intel processor.";
            ImageFileMachineNames[ImageFileMachine.AMD64] = "Targets a 64-bit AMD processor.";
        }

        public AssemblyInformationLoader(Assembly assembly)
        {
            Assembly = assembly;
            LoadInformation();
        }

        public Assembly Assembly { get; }

        public string AssemblyFullName { get; private set; }

        public string AssemblyKind { get; private set; }

        public DebuggableAttribute.DebuggingModes? DebuggingFlags { get; private set; }

        public bool EditAndContinueEnabled { get; private set; }

        public string FrameworkVersion { get; private set; }

        public bool IgnoreSymbolStoreSequencePoints { get; private set; }

        public bool JitOptimized { get; private set; }

        /// <summary>
        /// True if in Debugging mode, false if not.
        /// </summary>
        public bool JitTrackingEnabled { get; private set; }

        public string TargetProcessor { get; private set; }

        private void LoadInformation()
        {
            DetermineExecutableKind();

            DetermineDebuggingAttributes();

            AssemblyFullName = Assembly.FullName;

            DetermineFrameworkVersion();
        }

        private void DetermineExecutableKind()
        {
            var modules = Assembly.GetModules(false);
            if (modules.Length <= 0) return;

            modules[0].GetPEKind(out var portableExecutableKinds, out var imageFileMachine);

            foreach (PortableExecutableKinds kind in Enum.GetValues(typeof(PortableExecutableKinds)))
            {
                if ((portableExecutableKinds & kind) == kind && kind != PortableExecutableKinds.NotAPortableExecutableImage)
                {
                    if (!String.IsNullOrEmpty(AssemblyKind))
                    {
                        AssemblyKind += Environment.NewLine;
                    }

                    AssemblyKind += "- " + PortableExecutableKindsNames[kind];
                }
            }

            TargetProcessor = ImageFileMachineNames[imageFileMachine];

            // Any CPU builds are reported as 32bit.
            // 32bit builds will have more value for PortableExecutableKinds
            if (imageFileMachine == ImageFileMachine.I386 && portableExecutableKinds == PortableExecutableKinds.ILOnly)
            {
                TargetProcessor = "AnyCPU";
            }
        }

        private void DetermineDebuggingAttributes()
        {
            var debugAttribute = Assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().FirstOrDefault();
            if (debugAttribute != null)
            {
                JitTrackingEnabled = debugAttribute.IsJITTrackingEnabled;
                JitOptimized = !debugAttribute.IsJITOptimizerDisabled;
                IgnoreSymbolStoreSequencePoints =
                    (debugAttribute.DebuggingFlags &
                     DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints) !=
                    DebuggableAttribute.DebuggingModes.None;
                EditAndContinueEnabled =
                    (debugAttribute.DebuggingFlags &
                     DebuggableAttribute.DebuggingModes.EnableEditAndContinue) != DebuggableAttribute.DebuggingModes.None;

                DebuggingFlags = debugAttribute.DebuggingFlags;
            }
            else
            {
                // No DebuggableAttribute means IsJITTrackingEnabled=false, IsJITOptimizerDisabled=false, IgnoreSymbolStoreSequencePoints=false, EnableEditAndContinue=false
                JitTrackingEnabled = false;
                JitOptimized = true;
                IgnoreSymbolStoreSequencePoints = false;
                EditAndContinueEnabled = false;
                DebuggingFlags = null;
            }
        }

        private void DetermineFrameworkVersion()
        {
            FrameworkVersion = Assembly.ImageRuntimeVersion;

            var attributes = Assembly.GetCustomAttributes(true);
            var targetFrameworkAttribute = attributes.OfType<TargetFrameworkAttribute>().FirstOrDefault();

            if (targetFrameworkAttribute != null)
            {
                FrameworkVersion = targetFrameworkAttribute.FrameworkDisplayName;
            }
        }
    }
}