using HidSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using GastroWaga.Services;
using GastroWaga.UI;


namespace GastroWaga
{
    public partial class MainWindow : Window
    {
        HidDevice? _device;
        HidStream? _stream;
        CancellationTokenSource? _cts;

        double _grossGrams = 0;
        double _lastSampleGrams = 0;
        DateTime _lastChange = DateTime.UtcNow;
        const double StableThresholdGrams = 1.0;
        const int StableMs = 300;

        double _tareGrams = 0;
        string _unitDisplay = "kg";

        public MainWindow() { InitializeComponent(); }

        public MainWindow()
        {
            InitializeComponent();
            CurrentSessionText.Text = AppState.CurrentSessionName ?? "(brak)";
        }

        // handler
        private void OpenSessions_Click(object sender, RoutedEventArgs e)
        {
            var w = new SessionsWindow { Owner = this };
            w.ShowDialog();
            CurrentSessionText.Text = AppState.CurrentSessionName ?? "(brak)";
        }

        private void SetStatus(string text, bool isError = false)
        {
            if (StatusText == null) return;
            StatusText.Text = text;
            try { StatusText.Foreground = isError ? Brushes.OrangeRed : Brushes.Gray; } catch { }
        }

        private void UpdateDisplay(double grossGrams, bool? stable, bool overload)
        {
            _grossGrams = grossGrams;
            double net = Math.Max(0, _grossGrams - _tareGrams);

            string formatted = _unitDisplay == "kg" ? $"{net / 1000.0:0.000} kg" : $"{Math.Round(net, 0)} g";
            if (WeightText != null) WeightText.Text = formatted;
            if (TareText != null) TareText.Text = $"{_tareGrams:0} g";

            if (overload) SetStatus("Przeciążenie (OVLD)", true);
            else if (stable == true || IsSoftStable(grossGrams)) SetStatus("Odczyt stabilny");
            else SetStatus("Odczyt niestabilny...");
        }

        private bool IsSoftStable(double gramsNow)
        {
            var now = DateTime.UtcNow;
            if (Math.Abs(gramsNow - _lastSampleGrams) > StableThresholdGrams)
            {
                _lastSampleGrams = gramsNow;
                _lastChange = now;
            }
            return (now - _lastChange).TotalMilliseconds >= StableMs;
        }

        private static List<HidDevice> SafeEnumerateDymo()
        {
            var list = DeviceList.Local;
            var result = new List<HidDevice>();

            try
            {
                result.AddRange(list.GetHidDevices(vendorID: 0x0922)
                    .Where(d => new[] { 0x8003, 0x8004, 0x8005, 0x8009 }.Contains(d.ProductID)));
            }
            catch { }

            if (result.Count == 0)
            {
                foreach (var d in list.GetHidDevices())
                {
                    bool ok = false;
                    try
                    {
                        var rd = d.GetReportDescriptor();
                        if (rd != null && rd.DeviceItems != null)
                        {
                            ok = rd.DeviceItems.Any(di =>
                            {
                                try
                                {
                                    var values = di.Usages?.GetAllValues();
                                    if (values == null) return false;
                                    return values.Any(u => (((u >> 16) & 0xFFFF) == 0x008D));
                                }
                                catch { return false; }
                            });
                        }
                        if (!ok)
                        {
                            string name;
                            try { name = d.GetFriendlyName() ?? ""; } catch { name = ""; }
                            ok = name.IndexOf("dymo", StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                    }
                    catch { ok = false; }

                    if (ok) result.Add(d);
                }
            }
            return result;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatus("Szukam wagi DYMO...");
                var candidates = SafeEnumerateDymo();
                _device = candidates.FirstOrDefault();

                if (_device == null) { SetStatus("Nie wykryto wagi DYMO. Podłącz USB i spróbuj ponownie.", true); return; }
                if (!_device.TryOpen(out _stream)) { SetStatus("Znaleziono wagę, ale nie mogę otworzyć połączenia HID.", true); return; }

                _cts = new CancellationTokenSource();
                SetStatus("Połączono z wagą.");
                _ = Task.Run(() => ReadLoop(_cts.Token));
            }
            catch (Exception ex) { SetStatus("Błąd: " + ex.Message, true); }
        }

        private void Disconnect_Click(object? sender, RoutedEventArgs? e)
        {
            try
            {
                _cts?.Cancel();
                _stream?.Dispose();
                _stream = null;
                _device = null;
                _grossGrams = 0;
                if (WeightText != null) WeightText.Text = _unitDisplay == "kg" ? "0.000 kg" : "0 g";
                SetStatus("Rozłączono");
            }
            catch { }
        }

        private async Task ReadLoop(CancellationToken ct)
        {
            var deviceLocal = _device;
            var streamLocal = _stream;
            if (deviceLocal == null || streamLocal == null) return;

            int len; try { len = Math.Max(64, deviceLocal.GetMaxInputReportLength()); } catch { len = 64; }
            var buf = new byte[len];

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    int n = await streamLocal.ReadAsync(buf, 0, buf.Length, ct);
                    if (n <= 0) continue;

                    if (TryParseDymoReport(buf, n, out double grams, out bool? stable, out bool overload))
                    {
                        try { Dispatcher.Invoke(() => UpdateDisplay(grams, stable, overload)); } catch { }
                    }
                }
                catch (OperationCanceledException) { }
                catch (IOException)
                {
                    Dispatcher.Invoke(() => { SetStatus("Utracono połączenie z wagą (I/O).", true); Disconnect_Click(null, null); });
                    break;
                }
                catch (ObjectDisposedException)
                {
                    Dispatcher.Invoke(() => SetStatus("Połączenie zamknięte.", true));
                    break;
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => SetStatus("Błąd odczytu: " + ex.Message, true));
                    await Task.Delay(200, ct);
                }
            }
        }

        private bool TryParseDymoReport(byte[] data, int count, out double grams, out bool? stable, out bool overload)
        {
            grams = 0; stable = null; overload = false;

            int offset = (count >= 6 && data[0] != 0) ? 1 : 0;
            if (count < offset + 5) return false;

            byte flags = data[offset + 0];
            byte unit = data[offset + 1];
            sbyte exponent = unchecked((sbyte)data[offset + 2]);
            ushort raw = (ushort)(data[offset + 3] | (data[offset + 4] << 8));

            double value = raw * Math.Pow(10, exponent);
            grams = (unit == 0x0B) ? value * 28.349523125 : value;

            if (flags == 0x02) stable = true;
            if (flags == 0x06) overload = true;

            return true;
        }

        // ===== Zdarzenia UI: jednostki i TARE =====
        private void UnitCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (UnitCombo?.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                _unitDisplay = item.Content?.ToString() == "g" ? "g" : "kg";
                UpdateDisplay(_grossGrams, null, false);
            }
        }

        private void TareFromCurrent_Click(object sender, RoutedEventArgs e)
        {
            _tareGrams = Math.Max(0, _grossGrams);
            UpdateDisplay(_grossGrams, null, false);
        }

        private void TareClear_Click(object sender, RoutedEventArgs e)
        {
            _tareGrams = 0;
            UpdateDisplay(_grossGrams, null, false);
        }

        private void TarePresetCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TarePresetCombo?.SelectedItem is System.Windows.Controls.ComboBoxItem item &&
                double.TryParse(item.Tag?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double g))
            {
                _tareGrams = Math.Max(0, g);
                UpdateDisplay(_grossGrams, null, false);
            }
        }

        private void TareCustomSet_Click(object sender, RoutedEventArgs e)
        {
            var txt = (TareCustomText?.Text ?? "").Replace(",", ".").Trim();
            if (double.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out double g))
            {
                _tareGrams = Math.Max(0, g);
                UpdateDisplay(_grossGrams, null, false);
            }
            else
            {
                MessageBox.Show("Podaj liczbę w gramach, np. 12 lub 250.",
                                "Błędna wartość", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
