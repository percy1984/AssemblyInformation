using System.Diagnostics;
using System.Reflection;

namespace AssemblyInformation.Model
{
    public interface IAssemblyInformationLoader
    {
        Assembly Assembly { get; }
        string AssemblyFullName { get; }
        string AssemblyKind { get; }
        DebuggableAttribute.DebuggingModes? DebuggingFlags { get; }
        bool EditAndContinueEnabled { get; }
        string FrameworkVersion { get; }
        bool IgnoreSymbolStoreSequencePoints { get; }
        bool JitOptimized { get; }

        /// <summary>
        /// True if in Debugging mode, false if not.
        /// </summary>
        bool JitTrackingEnabled { get; }

        string TargetProcessor { get; }
    }
}