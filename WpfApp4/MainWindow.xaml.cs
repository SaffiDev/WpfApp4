using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp4
{
    public partial class MainWindow
    {
        private Process? _process1;
        private DispatcherTimer? _timer;

        // Импорт функций Windows API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

        // Константа для сообщения WM_SETTEXT
        private const uint WmSettext = 0x000C;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _process1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mspaint.exe", 
                        UseShellExecute = true
                    }
                };
                _process1.Start();

                // Время га запуск
                if (!_process1.WaitForInputIdle(5000))
                {
                    label1.Content = "Paint не ответил timely";
                    return;
                }

                // Проверка заголовка через MainWindowTitle
                label1.Content = $"Запущен процесс: {_process1.ProcessName}\nID: {_process1.Id}\nЗаголовок: {_process1.MainWindowTitle}";
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;

                StartTimer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска Paint: {ex.Message}");
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _timer.Stop();

                if (_process1 != null && !_process1.HasExited)
                {
                    _process1.Kill();
                    _process1.WaitForExit(3000);
                    _process1.Close();
                }

                label1.Content = "Процесс остановлен";
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки: {ex.Message}");
            }
        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, _) =>
            {
                try
                {
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");
                    IntPtr windowHandle;

                    // Класс окна для Paint — "MSPaintApp"
                    string className = "MSPaintApp";
                    windowHandle = FindWindow(className, null);

                    if (windowHandle != IntPtr.Zero)
                    {
                        SendMessage(windowHandle, WmSettext, IntPtr.Zero, currentTime); // Только время
                        if (_process1 != null)
                            label1.Content =
                                $"Paint работает.\nID: {_process1.Id}\nЗаголовок окна обновлен: {currentTime}";
                    }
                    else
                    {
                        if (_process1 != null)
                            label1.Content =
                                $"Окно Paint не найдено. ID: {_process1.Id}\nЗаголовок: {_process1.MainWindowTitle}";
                    }

                    if (_process1 != null && _process1.HasExited)
                    {
                        _timer.Stop();
                        label1.Content = "Paint завершил работу";
                        StartButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка в таймере: {ex.Message}");
                }
            };
            _timer.Start();
        }
    }
}