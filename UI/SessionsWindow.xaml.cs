using GastroWaga.Domain.Entities;
using GastroWaga.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GastroWaga.UI
{
    public partial class SessionsWindow : Window
    {
        readonly SessionService _svc = new SessionService();

        public SessionsWindow()
        {
            InitializeComponent();
            Loaded += SessionsWindow_Loaded;
        }

        private async void SessionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshLists();
        }

        private async Task RefreshLists()
        {
            OpenGrid.ItemsSource = await _svc.GetOpenAsync();
            ClosedGrid.ItemsSource = await _svc.GetClosedAsync();
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            var name = NameBox.Text?.Trim() ?? "";
            var wh = WarehouseBox.Text?.Trim() ?? "";
            var user = UserBox.Text?.Trim() ?? "";
            var s = await _svc.CreateAsync(name, wh, user);
            MessageBox.Show($"Utworzono sesję:\n{s.Name}", "Sesja", MessageBoxButton.OK, MessageBoxImage.Information);
            await RefreshLists();
        }

        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            var tab = Tabs.SelectedItem as System.Windows.Controls.TabItem;
            if (tab?.Header?.ToString() == "Robocze")
            {
                if (OpenGrid.SelectedItem is Session s)
                {
                    AppState.CurrentSessionId = s.Id;
                    AppState.CurrentSessionName = s.Name;
                    MessageBox.Show($"Kontynuujesz: {s.Name}", "Sesja", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else MessageBox.Show("Wybierz sesję z listy Robocze.", "Sesja", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Kontynuować można tylko sesje Robocze.", "Sesja", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            if (OpenGrid.SelectedItem is Session s)
            {
                await _svc.CloseAsync(s.Id);
                await RefreshLists();
            }
            else MessageBox.Show("Wybierz sesję do zamknięcia (lista Robocze).", "Sesja", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private async void Reopen_Click(object sender, RoutedEventArgs e)
        {
            if (ClosedGrid.SelectedItem is Session s)
            {
                await _svc.ReopenAsync(s.Id);
                await RefreshLists();
            }
            else MessageBox.Show("Wybierz sesję do ponownego otwarcia (lista Zamknięte).", "Sesja", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private async void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            var s = OpenGrid.SelectedItem as Session ?? ClosedGrid.SelectedItem as Session;
            if (s == null) { MessageBox.Show("Wybierz sesję do duplikacji.", "Sesja", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            var copy = await _svc.DuplicateAsync(s.Id);
            await RefreshLists();
            MessageBox.Show($"Utworzono: {copy.Name}", "Sesja", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    }
}
