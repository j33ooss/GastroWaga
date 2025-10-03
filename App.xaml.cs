using GastroWaga.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace GastroWaga
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await DbInitializer.EnsureCreatedAsync();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Błąd (nieobsłużony)", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
