using System.ComponentModel;
using AssemblyInformation.Model;

namespace AssemblyInformation.ViewModel
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBuildInReleaseMode { get; set; }

        public bool IsOptimized { get; set; }

        public bool IsEditAndContinueEnabled { get; set; }

        public MainWindowVm(AssemblyInformationLoader ail)
        {
            IsBuildInReleaseMode = !ail.JitTrackingEnabled;
            IsOptimized = ail.JitOptimized;
            IsEditAndContinueEnabled = ail.EditAndContinueEnabled;
        }
    }
}
