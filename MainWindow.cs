using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PKSAnalyzer
{
    public class MainWindow : Window
    {
        private readonly ListBox interfacesListBox;
        private readonly TextBox interfaceInfoTextBox;
        private readonly TextBox urlTextBox;
        private readonly TextBox resultTextBox;
        private readonly ListBox historyListBox;
        private readonly Button analyzeButton;
        private readonly string historyFilePath;
        private readonly List<string> history = new List<string>();

        public MainWindow()
        {
            Title = "Анализатор сетевых подключений";
            Width = 1100;
            Height = 720;
            MinWidth = 900;
            MinHeight = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "url_history.txt");

            var rootGrid = new Grid();
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftPanel = new DockPanel { Margin = new Thickness(10) };
            Grid.SetColumn(leftPanel, 0);

            var refreshButton = new Button
            {
                Content = "Обновить список",
                Height = 32,
                Margin = new Thickness(0, 0, 0, 10)
            };
            refreshButton.Click += RefreshButton_Click;
            DockPanel.SetDock(refreshButton, Dock.Top);
            leftPanel.Children.Add(refreshButton);

            var interfacesGroup = new GroupBox
            {
                Header = "Сетевые интерфейсы",
                Margin = new Thickness(0)
            };

            interfacesListBox = new ListBox
            {
                Margin = new Thickness(8),
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
            interfacesListBox.SelectionChanged += InterfacesListBox_SelectionChanged;

            interfacesGroup.Content = interfacesListBox;
            leftPanel.Children.Add(interfacesGroup);

            var rightGrid = new Grid { Margin = new Thickness(0, 10, 10, 10) };
            Grid.SetColumn(rightGrid, 1);
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(220) });
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(160) });

            var interfaceInfoGroup = new GroupBox
            {
                Header = "Информация о выбранном интерфейсе",
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(interfaceInfoGroup, 0);

            interfaceInfoTextBox = new TextBox
            {
                Margin = new Thickness(8),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            interfaceInfoGroup.Content = interfaceInfoTextBox;

            var urlGroup = new GroupBox
            {
                Header = "Проверка URL / URI",
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(urlGroup, 1);

            var urlGrid = new Grid();
            urlGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            urlGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            urlGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            urlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            urlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            urlTextBox = new TextBox
            {
                Margin = new Thickness(8, 8, 8, 4),
                Height = 30,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(urlTextBox, 0);
            Grid.SetColumn(urlTextBox, 0);

            analyzeButton = new Button
            {
                Content = "Проверить",
                Margin = new Thickness(0, 8, 8, 4),
                Height = 30
            };
            analyzeButton.Click += AnalyzeButton_Click;
            Grid.SetRow(analyzeButton, 0);
            Grid.SetColumn(analyzeButton, 1);

            var hintText = new TextBlock
            {
                Margin = new Thickness(8, 0, 8, 6),
                Text = "Пример: https://ya.ru/search?q=test#top"
            };
            Grid.SetRow(hintText, 1);
            Grid.SetColumnSpan(hintText, 2);

            resultTextBox = new TextBox
            {
                Margin = new Thickness(8, 0, 8, 8),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                AcceptsReturn = true
            };
            Grid.SetRow(resultTextBox, 2);
            Grid.SetColumnSpan(resultTextBox, 2);

            urlGrid.Children.Add(urlTextBox);
            urlGrid.Children.Add(analyzeButton);
            urlGrid.Children.Add(hintText);
            urlGrid.Children.Add(resultTextBox);
            urlGroup.Content = urlGrid;

            var historyGroup = new GroupBox
            {
                Header = "История проверенных URL"
            };
            Grid.SetRow(historyGroup, 2);

            historyListBox = new ListBox
            {
                Margin = new Thickness(8)
            };
            historyListBox.MouseDoubleClick += HistoryListBox_MouseDoubleClick;
            historyGroup.Content = historyListBox;

            rightGrid.Children.Add(interfaceInfoGroup);
            rightGrid.Children.Add(urlGroup);
            rightGrid.Children.Add(historyGroup);

            rootGrid.Children.Add(leftPanel);
            rootGrid.Children.Add(rightGrid);

            Content = rootGrid;

            LoadHistory();
            LoadNetworkInterfaces();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadNetworkInterfaces();
        }

        private void InterfacesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = interfacesListBox.SelectedItem as InterfaceItem;
            interfaceInfoTextBox.Text = item == null ? string.Empty : BuildInterfaceInfo(item.NetworkInterface);
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var input = urlTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введите URL или URI для проверки.", "Пустой ввод", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            analyzeButton.IsEnabled = false;
            resultTextBox.Text = "Проверка...";

            try
            {
                resultTextBox.Text = await AnalyzeUrlAsync(input);
                SaveToHistory(input);
            }
            catch (Exception ex)
            {
                resultTextBox.Text = "Ошибка при анализе: " + ex.Message;
            }
            finally
            {
                analyzeButton.IsEnabled = true;
            }
        }

        private void HistoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (historyListBox.SelectedItem == null)
            {
                return;
            }

            urlTextBox.Text = historyListBox.SelectedItem.ToString();
        }

        private void LoadNetworkInterfaces()
        {
            interfacesListBox.Items.Clear();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .OrderBy(nic => nic.Name)
                .ToList();

            foreach (var networkInterface in interfaces)
            {
                interfacesListBox.Items.Add(new InterfaceItem(networkInterface));
            }

            if (interfacesListBox.Items.Count > 0)
            {
                interfacesListBox.SelectedIndex = 0;
            }
            else
            {
                interfaceInfoTextBox.Text = "Сетевые интерфейсы не найдены.";
            }
        }

        private string BuildInterfaceInfo(NetworkInterface networkInterface)
        {
            var builder = new StringBuilder();
            var properties = networkInterface.GetIPProperties();

            builder.AppendLine("Имя: " + networkInterface.Name);
            builder.AppendLine("Описание: " + networkInterface.Description);
            builder.AppendLine("Тип интерфейса: " + networkInterface.NetworkInterfaceType);
            builder.AppendLine("Состояние подключения: " + networkInterface.OperationalStatus);
            builder.AppendLine("Скорость соединения: " + FormatSpeed(networkInterface.Speed));
            builder.AppendLine("MAC-адрес: " + FormatMacAddress(networkInterface.GetPhysicalAddress()));
            builder.AppendLine();

            builder.AppendLine("IP-адреса:");

            if (properties.UnicastAddresses.Count == 0)
            {
                builder.AppendLine("  Нет данных");
            }
            else
            {
                foreach (var addressInfo in properties.UnicastAddresses)
                {
                    builder.AppendLine("  Адрес: " + addressInfo.Address);

                    if (addressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        builder.AppendLine("  Маска подсети: " + (addressInfo.IPv4Mask == null ? "-" : addressInfo.IPv4Mask.ToString()));
                    }

                    builder.AppendLine("  Тип адреса: " + GetAddressType(addressInfo.Address));
                    builder.AppendLine();
                }
            }

            builder.AppendLine("DNS-серверы:");
            if (properties.DnsAddresses.Count == 0)
            {
                builder.AppendLine("  Нет данных");
            }
            else
            {
                foreach (var dnsAddress in properties.DnsAddresses)
                {
                    builder.AppendLine("  " + dnsAddress);
                }
            }

            return builder.ToString().TrimEnd();
        }

        private async Task<string> AnalyzeUrlAsync(string input)
        {
            Uri uri;
            if (!Uri.TryCreate(input, UriKind.Absolute, out uri))
            {
                return "Некорректный URL/URI. Пример правильного формата: https://site.ru/page?id=1";
            }

            var builder = new StringBuilder();
            builder.AppendLine("Исходная строка: " + input);
            builder.AppendLine("Схема (протокол): " + EmptyToDash(uri.Scheme));
            builder.AppendLine("Хост: " + EmptyToDash(uri.Host));
            builder.AppendLine("Порт: " + (uri.IsDefaultPort ? "по умолчанию" : uri.Port.ToString()));
            builder.AppendLine("Путь: " + EmptyToDash(uri.AbsolutePath));
            builder.AppendLine("Параметры запроса: " + EmptyToDash(uri.Query));
            builder.AppendLine("Фрагмент: " + EmptyToDash(uri.Fragment));

            if (!string.IsNullOrWhiteSpace(uri.Query))
            {
                builder.AppendLine();
                builder.AppendLine("Разбор параметров:");

                foreach (var parameter in ParseQuery(uri.Query))
                {
                    builder.AppendLine("  " + parameter);
                }
            }

            if (string.IsNullOrWhiteSpace(uri.Host))
            {
                return builder.ToString().TrimEnd();
            }

            builder.AppendLine();
            builder.AppendLine("Проверка доступности хоста:");

            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(uri.Host, 1500);

                    if (reply.Status == IPStatus.Success)
                    {
                        builder.AppendLine("  Ping: доступен");
                        builder.AppendLine("  Время отклика: " + reply.RoundtripTime + " мс");
                        builder.AppendLine("  IP хоста: " + reply.Address);
                    }
                    else
                    {
                        builder.AppendLine("  Ping: недоступен (" + reply.Status + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                builder.AppendLine("  Ping: ошибка - " + ex.Message);
            }

            builder.AppendLine();
            builder.AppendLine("DNS-информация:");

            try
            {
                var addresses = await Task.Run(() => Dns.GetHostAddresses(uri.Host));

                if (addresses.Length == 0)
                {
                    builder.AppendLine("  Адреса не найдены");
                }
                else
                {
                    foreach (var address in addresses)
                    {
                        builder.AppendLine("  " + address + " [" + GetAddressType(address) + "]");
                    }
                }

                var hostEntry = await Task.Run(() => Dns.GetHostEntry(uri.Host));
                builder.AppendLine("  Каноническое имя: " + EmptyToDash(hostEntry.HostName));
            }
            catch (Exception ex)
            {
                builder.AppendLine("  Ошибка DNS: " + ex.Message);
            }

            return builder.ToString().TrimEnd();
        }

        private void LoadHistory()
        {
            history.Clear();

            if (File.Exists(historyFilePath))
            {
                history.AddRange(File.ReadAllLines(historyFilePath, Encoding.UTF8)
                    .Where(line => !string.IsNullOrWhiteSpace(line)));
            }

            UpdateHistoryList();
        }

        private void SaveToHistory(string url)
        {
            history.RemoveAll(item => string.Equals(item, url, StringComparison.OrdinalIgnoreCase));
            history.Insert(0, url);

            while (history.Count > 15)
            {
                history.RemoveAt(history.Count - 1);
            }

            File.WriteAllLines(historyFilePath, history, Encoding.UTF8);
            UpdateHistoryList();
        }

        private void UpdateHistoryList()
        {
            historyListBox.ItemsSource = null;
            historyListBox.ItemsSource = history.ToList();
        }

        private static string FormatMacAddress(PhysicalAddress address)
        {
            var bytes = address.GetAddressBytes();
            return bytes.Length == 0 ? "-" : string.Join("-", bytes.Select(b => b.ToString("X2")));
        }

        private static string FormatSpeed(long speed)
        {
            if (speed <= 0)
            {
                return "-";
            }

            var mbps = speed / 1000d / 1000d;
            return mbps.ToString("0.##") + " Мбит/с";
        }

        private static string EmptyToDash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private static IEnumerable<string> ParseQuery(string query)
        {
            var cleanedQuery = query.TrimStart('?');

            if (string.IsNullOrWhiteSpace(cleanedQuery))
            {
                yield return "-";
                yield break;
            }

            var parts = cleanedQuery.Split('&');

            foreach (var part in parts)
            {
                var pieces = part.Split(new[] { '=' }, 2);
                var key = Uri.UnescapeDataString(pieces[0]);
                var value = pieces.Length > 1 ? Uri.UnescapeDataString(pieces[1]) : string.Empty;
                yield return key + " = " + value;
            }
        }

        private static string GetAddressType(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
            {
                return "loopback";
            }

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                var bytes = address.GetAddressBytes();

                if (bytes[0] == 10)
                {
                    return "локальный";
                }

                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                {
                    return "локальный";
                }

                if (bytes[0] == 192 && bytes[1] == 168)
                {
                    return "локальный";
                }

                if (bytes[0] == 169 && bytes[1] == 254)
                {
                    return "локальный";
                }

                return "публичный";
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || IsUniqueLocalIpv6(address))
                {
                    return "локальный";
                }

                return "публичный";
            }

            return "неизвестно";
        }

        private static bool IsUniqueLocalIpv6(IPAddress address)
        {
            var bytes = address.GetAddressBytes();
            return bytes.Length > 0 && (bytes[0] & 0xFE) == 0xFC;
        }

        private sealed class InterfaceItem
        {
            private readonly NetworkInterface networkInterface;

            public InterfaceItem(NetworkInterface networkInterface)
            {
                this.networkInterface = networkInterface;
            }

            public NetworkInterface NetworkInterface
            {
                get { return networkInterface; }
            }

            public override string ToString()
            {
                return NetworkInterface.Name + " (" + NetworkInterface.NetworkInterfaceType + ")";
            }
        }
    }
}
