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

        public AssemblyInformationVm(IAssemblyInformationLoader ail)
        {
            IsBuildInReleaseMode = !ail.JitTrackingEnabled;
            IsOptimized = ail.JitOptimized;
            IsEditAndContinueEnabled = ail.EditAndContinueEnabled;
            FrameworkVersion = ail.FrameworkVersion;
        }
    }
}