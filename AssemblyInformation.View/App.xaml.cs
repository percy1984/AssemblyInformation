using System.Windows;
using AssemblyInformation.Model;
using AssemblyInformation.ViewModel;
using StructureMap;

namespace AssemblyInformation.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var container = new Container();
            container.Configure(r => r.For<IAssemblyInformationLoader>().Use<AssemblyInformationLoader>());
            var mainWindow = container.GetInstance<AssemblyInformationV>();
            mainWindow.DataContext = container.GetInstance<MainWindowVm>();
            mainWindow.Show();
        }
    }
}
