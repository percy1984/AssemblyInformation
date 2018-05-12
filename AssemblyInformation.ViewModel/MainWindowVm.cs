using System.ComponentModel;
using System.Reflection;
using AssemblyInformation.Model;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace AssemblyInformation.ViewModel
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand LoadAssemblyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var fileDialog = new OpenFileDialog();

                    if (fileDialog.ShowDialog() == true)
                    {
                        AssemblyInfoVm = new AssemblyInformationVm(new AssemblyInformationLoader(Assembly.LoadFrom(fileDialog.FileName)));
                    }
                });
            }
        }

        public AssemblyInformationVm AssemblyInfoVm { get; private set; }

    }
}
