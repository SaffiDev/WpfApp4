using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace WpfApp4
{
    public partial class MainWindow : Window
    {
        private Process process1;
        private DispatcherTimer timer;

        // Импорт функций Windows API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        // Константа для сообщения WM_SETTEXT
        private const uint WM_SETTEXT = 0x000C;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                process1 = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mspaint.exe", 
                        UseShellExecute = true
                    }
                };
                process1.Start();
                
                if (!process1.WaitForInputIdle(5000))
                {
                    label1.Content = "Paint не ответил timely";
                    return;
                }

                label1.Content = $"Запущен процесс: {process1.ProcessName}\nID: {process1.Id}";
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
                timer?.Stop();

                if (process1 != null && !process1.HasExited)
                {
                    process1.Kill();
                    process1.WaitForExit(3000);
                    process1.Close();
                }

                label1.Content = "Процесс остановлен";
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                Title = "MainWindow";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка остановки: {ex.Message}");
            }
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                try
                {
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");
                    IntPtr windowHandle;

                    // Класс окна для Paint — точно "MSPaintApp"
                    string className = "MSPaintApp";
                    windowHandle = FindWindow(className, null);

                    if (windowHandle != IntPtr.Zero)
                    {
                        SendMessage(windowHandle, WM_SETTEXT, IntPtr.Zero, $"Paint - Время: {currentTime}"); 
                        label1.Content = $"Paint работает.\nID: {process1.Id}\nЗаголовок окна обновлен: Paint - Время: {currentTime}";
                    }
                    else
                    {
                        label1.Content = $"Окно Paint не найдено. ID: {process1.Id}";
                    }

                    if (process1.HasExited)
                    {
                        timer.Stop();
                        label1.Content = "Paint завершил работу";
                        StartButton.IsEnabled = true;
                        StopButton.IsEnabled = false;
                        Title = "MainWindow";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка в таймере: {ex.Message}");
                }
            };
            timer.Start();
        }
    }
}