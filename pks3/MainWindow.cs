using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PKS3
{
    public class MainWindow : Window
    {
        private readonly object syncRoot = new object();
        private readonly List<LogEntry> logs = new List<LogEntry>();
        private readonly Dictionary<string, string> messages = new Dictionary<string, string>();
        private readonly Dictionary<string, int> minuteStats = new Dictionary<string, int>();
        private readonly Dictionary<string, int> hourStats = new Dictionary<string, int>();
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string logFilePath;

        private HttpListener listener;
        private DateTime serverStartedAt = DateTime.MinValue;
        private int totalRequests;
        private int getRequests;
        private int postRequests;
        private int outgoingRequests;
        private double totalProcessingMilliseconds;

        private TextBox portTextBox;
        private Button serverButton;
        private TextBlock serverStatusText;
        private TextBox statisticsTextBox;
        private DataGrid trafficGrid;
        private TextBox trafficChartTextBox;
        private ComboBox logMethodFilterComboBox;
        private ComboBox logStatusFilterComboBox;
        private TextBox logsTextBox;
        private TextBox clientUrlTextBox;
        private ComboBox clientMethodComboBox;
        private TextBox clientBodyTextBox;
        private Button sendButton;
        private TextBox responseTextBox;

        public MainWindow()
        {
            Title = "PKS3 - Монитор HTTP-запросов";
            Width = 1300;
            Height = 850;
            MinWidth = 1100;
            MinHeight = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
            httpClient.Timeout = TimeSpan.FromSeconds(20);

            BuildInterface();
            UpdateStatistics();
            RefreshTrafficView();
            RefreshLogsView();
        }

        protected override void OnClosed(EventArgs e)
        {
            StopServer();
            httpClient.Dispose();
            base.OnClosed(e);
        }

        private void BuildInterface()
        {
            var rootGrid = new Grid();
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(320) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.15, GridUnitType.Star) });
            rootGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var serverGroup = new GroupBox
            {
                Header = "Сервер и аналитика",
                Margin = new Thickness(10, 10, 5, 5),
                Content = BuildServerPanel()
            };
            Grid.SetRow(serverGroup, 0);
            Grid.SetColumn(serverGroup, 0);

            var clientGroup = new GroupBox
            {
                Header = "HTTP-клиент",
                Margin = new Thickness(5, 10, 10, 5),
                Content = BuildClientPanel()
            };
            Grid.SetRow(clientGroup, 0);
            Grid.SetColumn(clientGroup, 1);

            var logsGroup = new GroupBox
            {
                Header = "Логи запросов и ответов",
                Margin = new Thickness(10, 5, 10, 10),
                Content = BuildLogsPanel()
            };
            Grid.SetRow(logsGroup, 1);
            Grid.SetColumnSpan(logsGroup, 2);

            rootGrid.Children.Add(serverGroup);
            rootGrid.Children.Add(clientGroup);
            rootGrid.Children.Add(logsGroup);

            Content = rootGrid;
        }

        private UIElement BuildServerPanel()
        {
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(170) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var topPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(topPanel, 0);

            topPanel.Children.Add(new TextBlock
            {
                Text = "Порт:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });

            portTextBox = new TextBox
            {
                Text = "8080",
                Width = 100,
                Height = 28,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            topPanel.Children.Add(portTextBox);

            serverButton = new Button
            {
                Content = "Запустить сервер",
                Width = 170,
                Height = 30
            };
            serverButton.Click += ServerButton_Click;
            topPanel.Children.Add(serverButton);

            serverStatusText = new TextBlock
            {
                Text = "Сервер не запущен.",
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = Brushes.DarkRed
            };
            Grid.SetRow(serverStatusText, 1);

            statisticsTextBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(statisticsTextBox, 2);

            var bottomGrid = new Grid();
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star) });
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(bottomGrid, 3);

            trafficGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                Margin = new Thickness(0, 0, 10, 0),
                HeadersVisibility = DataGridHeadersVisibility.Column,
                CanUserAddRows = false
            };
            trafficGrid.Columns.Add(new DataGridTextColumn { Header = "Тип", Binding = new Binding("PeriodType") });
            trafficGrid.Columns.Add(new DataGridTextColumn { Header = "Период", Binding = new Binding("Period") });
            trafficGrid.Columns.Add(new DataGridTextColumn { Header = "Запросов", Binding = new Binding("Count") });
            Grid.SetColumn(trafficGrid, 0);

            trafficChartTextBox = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                AcceptsReturn = true
            };
            Grid.SetColumn(trafficChartTextBox, 1);

            bottomGrid.Children.Add(trafficGrid);
            bottomGrid.Children.Add(trafficChartTextBox);

            grid.Children.Add(topPanel);
            grid.Children.Add(serverStatusText);
            grid.Children.Add(statisticsTextBox);
            grid.Children.Add(bottomGrid);

            return grid;
        }

        private UIElement BuildClientPanel()
        {
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(140) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(new TextBlock
            {
                Text = "URL:",
                Margin = new Thickness(0, 0, 0, 5)
            });

            clientUrlTextBox = new TextBox
            {
                Text = "http://localhost:8080/",
                Height = 28,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 22, 0, 10)
            };
            Grid.SetRow(clientUrlTextBox, 0);
            grid.Children.Add(clientUrlTextBox);

            var methodPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(methodPanel, 1);

            methodPanel.Children.Add(new TextBlock
            {
                Text = "Метод:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });

            clientMethodComboBox = new ComboBox
            {
                Width = 120,
                Height = 28,
                Margin = new Thickness(0, 0, 12, 0)
            };
            clientMethodComboBox.Items.Add("GET");
            clientMethodComboBox.Items.Add("POST");
            clientMethodComboBox.SelectedIndex = 0;
            clientMethodComboBox.SelectionChanged += ClientMethodComboBox_SelectionChanged;
            methodPanel.Children.Add(clientMethodComboBox);

            sendButton = new Button
            {
                Content = "Отправить запрос",
                Width = 160,
                Height = 30
            };
            sendButton.Click += SendButton_Click;
            methodPanel.Children.Add(sendButton);

            clientBodyTextBox = new TextBox
            {
                Text = "{\r\n  \"message\": \"Привет\"\r\n}",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(clientBodyTextBox, 2);

            grid.Children.Add(methodPanel);
            grid.Children.Add(clientBodyTextBox);

            var responseLabel = new TextBlock
            {
                Text = "Ответ:",
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(responseLabel, 3);

            responseTextBox = new TextBox
            {
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(responseTextBox, 4);

            grid.Children.Add(responseLabel);
            grid.Children.Add(responseTextBox);

            return grid;
        }

        private UIElement BuildLogsPanel()
        {
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var filterPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(filterPanel, 0);

            filterPanel.Children.Add(new TextBlock
            {
                Text = "Метод:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });

            logMethodFilterComboBox = new ComboBox
            {
                Width = 140,
                Height = 28,
                Margin = new Thickness(0, 0, 14, 0)
            };
            logMethodFilterComboBox.Items.Add("Все");
            logMethodFilterComboBox.Items.Add("GET");
            logMethodFilterComboBox.Items.Add("POST");
            logMethodFilterComboBox.SelectedIndex = 0;
            logMethodFilterComboBox.SelectionChanged += LogFilter_SelectionChanged;
            filterPanel.Children.Add(logMethodFilterComboBox);

            filterPanel.Children.Add(new TextBlock
            {
                Text = "Статус:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            });

            logStatusFilterComboBox = new ComboBox
            {
                Width = 140,
                Height = 28
            };
            logStatusFilterComboBox.Items.Add("Все");
            logStatusFilterComboBox.Items.Add("Успех");
            logStatusFilterComboBox.Items.Add("Ошибка");
            logStatusFilterComboBox.SelectedIndex = 0;
            logStatusFilterComboBox.SelectionChanged += LogFilter_SelectionChanged;
            filterPanel.Children.Add(logStatusFilterComboBox);

            logsTextBox = new TextBox
            {
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Grid.SetRow(logsTextBox, 1);

            grid.Children.Add(filterPanel);
            grid.Children.Add(logsTextBox);

            return grid;
        }

        private void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (listener != null && listener.IsListening)
            {
                StopServer();
                return;
            }

            int port;
            if (!int.TryParse(portTextBox.Text.Trim(), out port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Введите корректный порт от 1 до 65535.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                StartServer(port);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось запустить сервер: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StopServer();
            }
        }

        private void StartServer(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();

            serverStartedAt = DateTime.Now;
            serverButton.Content = "Остановить сервер";
            serverStatusText.Text = "Сервер запущен: http://localhost:" + port + "/";
            serverStatusText.Foreground = Brushes.DarkGreen;

            if (string.IsNullOrWhiteSpace(clientUrlTextBox.Text) || clientUrlTextBox.Text.Contains("localhost:8080"))
            {
                clientUrlTextBox.Text = "http://localhost:" + port + "/";
            }

            AddLog("SYSTEM", "SERVER", 200, "Сервер запущен на порту " + port, true);
            Task.Run((Func<Task>)ListenLoopAsync);
            UpdateStatistics();
        }

        private void StopServer()
        {
            if (listener == null)
            {
                return;
            }

            try
            {
                if (listener.IsListening)
                {
                    listener.Stop();
                }
            }
            catch
            {
            }

            try
            {
                listener.Close();
            }
            catch
            {
            }

            listener = null;
            serverButton.Content = "Запустить сервер";
            serverStatusText.Text = "Сервер остановлен.";
            serverStatusText.Foreground = Brushes.DarkRed;
            AddLog("SYSTEM", "SERVER", 200, "Сервер остановлен.", true);
            UpdateStatistics();
        }

        private async Task ListenLoopAsync()
        {
            while (listener != null && listener.IsListening)
            {
                HttpListenerContext context;

                try
                {
                    context = await listener.GetContextAsync();
                }
                catch
                {
                    break;
                }

                ThreadPool.QueueUserWorkItem(async state =>
                {
                    await HandleRequestAsync((HttpListenerContext)state);
                }, context);
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var method = request.HttpMethod.ToUpperInvariant();
            var url = request.Url == null ? "/" : request.Url.ToString();
            var body = await ReadRequestBodyAsync(request);
            var headers = BuildHeadersText(request);

            int statusCode = 200;
            bool success = true;
            string responseText;

            var writeErrorResponse = false;

            try
            {
                if (method == "GET")
                {
                    var response = CreateServerStateResponse();
                    responseText = SerializeJson(response);
                    await WriteJsonResponseAsync(context.Response, responseText, 200);
                }
                else if (method == "POST")
                {
                    var messageRequest = DeserializeJson<MessageRequest>(body);
                    if (messageRequest == null || string.IsNullOrWhiteSpace(messageRequest.Message))
                    {
                        statusCode = 400;
                        success = false;
                        responseText = SerializeJson(new ErrorResponse { Error = "Ожидался JSON вида { \"message\": \"текст\" }." });
                        await WriteJsonResponseAsync(context.Response, responseText, statusCode);
                    }
                    else
                    {
                        var id = Guid.NewGuid().ToString("N");
                        lock (syncRoot)
                        {
                            messages[id] = messageRequest.Message;
                        }

                        responseText = SerializeJson(new MessageResponse { Id = id, Message = "Сообщение сохранено." });
                        await WriteJsonResponseAsync(context.Response, responseText, 200);
                    }
                }
                else
                {
                    statusCode = 405;
                    success = false;
                    responseText = SerializeJson(new ErrorResponse { Error = "Поддерживаются только GET и POST." });
                    await WriteJsonResponseAsync(context.Response, responseText, statusCode);
                }
            }
            catch (Exception ex)
            {
                statusCode = 500;
                success = false;
                responseText = SerializeJson(new ErrorResponse { Error = ex.Message });
                writeErrorResponse = true;
            }
            finally
            {
                stopwatch.Stop();
            }

            if (writeErrorResponse)
            {
                await WriteJsonResponseAsync(context.Response, responseText, statusCode);
            }

            RegisterIncomingRequest(method, stopwatch.Elapsed.TotalMilliseconds);

            var logText = "URL: " + url +
                          "\r\nЗаголовки:\r\n" + headers +
                          "\r\nТело запроса:\r\n" + (string.IsNullOrWhiteSpace(body) ? "<пусто>" : body) +
                          "\r\nОтвет:\r\n" + responseText +
                          "\r\nВремя обработки: " + stopwatch.ElapsedMilliseconds + " мс";

            AddLog(method, "SERVER", statusCode, logText, success);
        }

        private void RegisterIncomingRequest(string method, double milliseconds)
        {
            lock (syncRoot)
            {
                totalRequests++;
                totalProcessingMilliseconds += milliseconds;

                if (method == "GET")
                {
                    getRequests++;
                }
                else if (method == "POST")
                {
                    postRequests++;
                }

                var minuteKey = DateTime.Now.ToString("dd.MM HH:mm");
                var hourKey = DateTime.Now.ToString("dd.MM HH:00");

                if (!minuteStats.ContainsKey(minuteKey))
                {
                    minuteStats[minuteKey] = 0;
                }

                if (!hourStats.ContainsKey(hourKey))
                {
                    hourStats[hourKey] = 0;
                }

                minuteStats[minuteKey]++;
                hourStats[hourKey]++;
            }

            Dispatcher.Invoke(() =>
            {
                UpdateStatistics();
                RefreshTrafficView();
            });
        }

        private ServerStateResponse CreateServerStateResponse()
        {
            lock (syncRoot)
            {
                return new ServerStateResponse
                {
                    TotalRequests = totalRequests,
                    GetRequests = getRequests,
                    PostRequests = postRequests,
                    AverageProcessingMs = totalRequests == 0 ? 0 : Math.Round(totalProcessingMilliseconds / totalRequests, 2),
                    Uptime = serverStartedAt == DateTime.MinValue ? "00:00:00" : (DateTime.Now - serverStartedAt).ToString(@"hh\:mm\:ss"),
                    MessagesCount = messages.Count
                };
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return string.Empty;
            }

            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task WriteJsonResponseAsync(HttpListenerResponse response, string json, int statusCode)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        private string BuildHeadersText(HttpListenerRequest request)
        {
            var builder = new StringBuilder();

            foreach (var key in request.Headers.AllKeys)
            {
                builder.AppendLine(key + ": " + request.Headers[key]);
            }

            return builder.Length == 0 ? "<нет заголовков>" : builder.ToString().TrimEnd();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var url = clientUrlTextBox.Text.Trim();
            var method = clientMethodComboBox.SelectedItem == null ? "GET" : clientMethodComboBox.SelectedItem.ToString();
            var body = clientBodyTextBox.Text;

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Введите URL.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            sendButton.IsEnabled = false;
            responseTextBox.Text = "Запрос выполняется...";

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var request = new HttpRequestMessage(new HttpMethod(method), url);

                if (method == "POST")
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                var response = await httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                stopwatch.Stop();

                lock (syncRoot)
                {
                    outgoingRequests++;
                }

                responseTextBox.Text = "Код: " + (int)response.StatusCode + " " + response.ReasonPhrase + "\r\n\r\n" + responseBody;

                var logText = "URL: " + url +
                              "\r\nТело запроса:\r\n" + (method == "POST" ? body : "<нет тела>") +
                              "\r\nОтвет:\r\n" + responseBody +
                              "\r\nВремя выполнения: " + stopwatch.ElapsedMilliseconds + " мс";

                AddLog(method, "CLIENT", (int)response.StatusCode, logText, response.IsSuccessStatusCode);
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                responseTextBox.Text = "Ошибка: " + ex.Message;
                AddLog(method, "CLIENT", 0, "URL: " + url + "\r\nОшибка клиента: " + ex.Message, false);
            }
            finally
            {
                sendButton.IsEnabled = true;
            }
        }

        private void ClientMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var method = clientMethodComboBox.SelectedItem == null ? "GET" : clientMethodComboBox.SelectedItem.ToString();
            clientBodyTextBox.IsEnabled = method == "POST";
        }

        private void LogFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshLogsView();
        }

        private void AddLog(string method, string source, int statusCode, string text, bool success)
        {
            var entry = new LogEntry
            {
                Time = DateTime.Now,
                Method = method,
                Source = source,
                StatusCode = statusCode,
                Success = success,
                Text = text
            };

            lock (syncRoot)
            {
                logs.Add(entry);
                File.AppendAllText(logFilePath, entry.ToDisplayString() + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
            }

            Dispatcher.Invoke(RefreshLogsView);
        }

        private void UpdateStatistics()
        {
            int localTotalRequests;
            int localGetRequests;
            int localPostRequests;
            int localOutgoingRequests;
            int localMessagesCount;
            double averageMs;
            string uptime;

            lock (syncRoot)
            {
                localTotalRequests = totalRequests;
                localGetRequests = getRequests;
                localPostRequests = postRequests;
                localOutgoingRequests = outgoingRequests;
                localMessagesCount = messages.Count;
                averageMs = localTotalRequests == 0 ? 0 : Math.Round(totalProcessingMilliseconds / localTotalRequests, 2);
                uptime = serverStartedAt == DateTime.MinValue ? "00:00:00" : (DateTime.Now - serverStartedAt).ToString(@"hh\:mm\:ss");
            }

            statisticsTextBox.Text =
                "Входящих запросов: " + localTotalRequests + "\r\n" +
                "GET-запросов: " + localGetRequests + "\r\n" +
                "POST-запросов: " + localPostRequests + "\r\n" +
                "Исходящих запросов клиента: " + localOutgoingRequests + "\r\n" +
                "Среднее время обработки: " + averageMs + " мс\r\n" +
                "Сохраненных сообщений: " + localMessagesCount + "\r\n" +
                "Время работы сервера: " + uptime + "\r\n" +
                "Файл логов: " + logFilePath;
        }

        private void RefreshTrafficView()
        {
            List<TrafficRow> rows;
            string chartText;

            lock (syncRoot)
            {
                rows = minuteStats
                    .OrderByDescending(x => x.Key)
                    .Take(8)
                    .Select(x => new TrafficRow { PeriodType = "Минута", Period = x.Key, Count = x.Value })
                    .Concat(hourStats
                        .OrderByDescending(x => x.Key)
                        .Take(8)
                        .Select(x => new TrafficRow { PeriodType = "Час", Period = x.Key, Count = x.Value }))
                    .ToList();

                var chartLines = new List<string>();
                chartLines.Add("Минутный график:");
                chartLines.AddRange(BuildChartLines("М", minuteStats));
                chartLines.Add(string.Empty);
                chartLines.Add("Часовой график:");
                chartLines.AddRange(BuildChartLines("Ч", hourStats));
                chartText = string.Join("\r\n", chartLines);
            }

            trafficGrid.ItemsSource = null;
            trafficGrid.ItemsSource = rows;
            trafficChartTextBox.Text = chartText;
        }

        private IEnumerable<string> BuildChartLines(string prefix, Dictionary<string, int> source)
        {
            if (source.Count == 0)
            {
                return new[] { "Пока нет данных." };
            }

            return source
                .OrderByDescending(x => x.Key)
                .Take(8)
                .Select(x => prefix + " " + x.Key + " | " + new string('#', Math.Max(1, x.Value)) + " (" + x.Value + ")");
        }

        private void RefreshLogsView()
        {
            var selectedMethod = logMethodFilterComboBox == null || logMethodFilterComboBox.SelectedItem == null ? "Все" : logMethodFilterComboBox.SelectedItem.ToString();
            var selectedStatus = logStatusFilterComboBox == null || logStatusFilterComboBox.SelectedItem == null ? "Все" : logStatusFilterComboBox.SelectedItem.ToString();
            List<LogEntry> filteredLogs;

            lock (syncRoot)
            {
                filteredLogs = logs
                    .Where(x => selectedMethod == "Все" || x.Method == selectedMethod || x.Method == "SYSTEM")
                    .Where(x => selectedStatus == "Все" ||
                                (selectedStatus == "Успех" && x.Success) ||
                                (selectedStatus == "Ошибка" && !x.Success))
                    .OrderByDescending(x => x.Time)
                    .Take(100)
                    .ToList();
            }

            if (logsTextBox != null)
            {
                logsTextBox.Text = filteredLogs.Count == 0
                    ? "Логи пока отсутствуют."
                    : string.Join("\r\n\r\n", filteredLogs.Select(x => x.ToDisplayString()));
            }
        }

        private string SerializeJson<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, value);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private T DeserializeJson<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    return serializer.ReadObject(stream) as T;
                }
            }
            catch
            {
                return null;
            }
        }

        private sealed class LogEntry
        {
            public DateTime Time { get; set; }
            public string Method { get; set; }
            public string Source { get; set; }
            public int StatusCode { get; set; }
            public bool Success { get; set; }
            public string Text { get; set; }

            public string ToDisplayString()
            {
                return "[" + Time.ToString("dd.MM.yyyy HH:mm:ss") + "] " +
                       Source + " " + Method + " | Код: " + StatusCode + " | " +
                       (Success ? "Успех" : "Ошибка") + "\r\n" + Text;
            }
        }

        private sealed class TrafficRow
        {
            public string PeriodType { get; set; }
            public string Period { get; set; }
            public int Count { get; set; }
        }

        [DataContract]
        private sealed class MessageRequest
        {
            [DataMember(Name = "message")]
            public string Message { get; set; }
        }

        [DataContract]
        private sealed class MessageResponse
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "message")]
            public string Message { get; set; }
        }

        [DataContract]
        private sealed class ErrorResponse
        {
            [DataMember(Name = "error")]
            public string Error { get; set; }
        }

        [DataContract]
        private sealed class ServerStateResponse
        {
            [DataMember(Name = "totalRequests")]
            public int TotalRequests { get; set; }

            [DataMember(Name = "getRequests")]
            public int GetRequests { get; set; }

            [DataMember(Name = "postRequests")]
            public int PostRequests { get; set; }

            [DataMember(Name = "averageProcessingMs")]
            public double AverageProcessingMs { get; set; }

            [DataMember(Name = "uptime")]
            public string Uptime { get; set; }

            [DataMember(Name = "messagesCount")]
            public int MessagesCount { get; set; }
        }
    }
}
