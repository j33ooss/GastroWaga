using System.Windows;
using System.Windows.Threading;

namespace GastroWaga
{
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Błąd (nieobsłużony)", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // nie zamykaj aplikacji
        }
    }
}
