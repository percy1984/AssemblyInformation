using System.Collections.Generic;
using System.ComponentModel;
using AssemblyInformation.Model;

namespace AssemblyInformation.ViewModel
{
    public class AssemblyInformationVm : INotifyPropertyChanged
    {
        public bool IsBuildInReleaseMode { get; set; }

        public bool IsOptimized { get; set; }

        public bool IsEditAndContinueEnabled { get; set; }

        public string FrameworkVersion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string AssemblyKind { get; set; }

        public string TargetProcessor { get; set; }

        public string FullAssemblyName { get; set; }

        public ICollection<ReferenceInfo> DirectReferences { get; set; }
        public ICollection<ReferenceInfo> DirectIndirectReferences { get; set; }
        public ICollection<ReferenceInfo> ReferringAssemblies { get; set; }

        public AssemblyInformationVm(IAssemblyInformationLoader ail)
        {
            IsBuildInReleaseMode = !ail.JitTrackingEnabled;
            IsOptimized = ail.JitOptimized;
            IsEditAndContinueEnabled = ail.EditAndContinueEnabled;
            FrameworkVersion = ail.FrameworkVersion;
            AssemblyKind = ail.AssemblyKind;
            TargetProcessor = ail.TargetProcessor;
            FullAssemblyName = ail.AssemblyFullName;
        }
    }
}