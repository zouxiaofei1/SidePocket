using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Diagnostics;

namespace SidePocket
{
    public partial class SettingsWindow : Window
    {
        private FullConfig _tempConfig;
        private Border? _activeRecorder = null;
        private const string RUN_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "SidePocket";

        public SettingsWindow()
        {
            InitializeComponent();
            try
            {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/resources/icon.ico"));
            }
            catch { }
            
            _tempConfig = new FullConfig
            {
                PocketHotKey = new HotKeyConfig { Modifiers = ConfigManager.Current.PocketHotKey.Modifiers, Key = ConfigManager.Current.PocketHotKey.Key },
                RestoreHotKey = new HotKeyConfig { Modifiers = ConfigManager.Current.RestoreHotKey.Modifiers, Key = ConfigManager.Current.RestoreHotKey.Key }
            };
            
            UpdateDisplay();
            CheckAutoStartStatus();
            this.KeyDown += (s, e) => { if (e.Key == Key.Escape && _activeRecorder == null) this.Close(); };
        }

        private void CheckAutoStartStatus()
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue(APP_NAME);
                        if (AutoStartCheck != null)
                        {
                            AutoStartCheck.IsChecked = value != null;
                        }
                    }
                }
            }
            catch { }
        }

        private void AutoStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                {
                    if (key != null)
                    {
                        if (AutoStartCheck.IsChecked == true)
                        {
                            string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                            if (!string.IsNullOrEmpty(appPath))
                            {
                                // Wrap path in quotes to handle spaces
                                key.SetValue(APP_NAME, $"\"{appPath}\" --autostart");
                            }
                        }
                        else
                        {
                            key.DeleteValue(APP_NAME, false);
                        }
                        ToastNotification.Show("开机启动设置已保存", this);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("设置开机启动失败: " + ex.Message);
            }
        }

        private void UpdateDisplay()
        {
            PocketHotKeyDisplay.Text = _tempConfig.PocketHotKey.ToString();
            RestoreHotKeyDisplay.Text = _tempConfig.RestoreHotKey.ToString();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HotKeyRecorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                e.Handled = true; // 阻止事件冒泡到窗口拖动处理器
                StartRecording(border);
            }
        }

        private void StartRecording(Border border)
        {
            _activeRecorder = border;
            _activeRecorder.Focus();
            InputMethod.SetIsInputMethodEnabled(_activeRecorder, false);
            _activeRecorder.Background = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["HoverBackgroundBrush"];
            
            if (_activeRecorder.Tag?.ToString() == "Pocket")
                PocketHotKeyDisplay.Text = "请按下快捷键...";
            else
                RestoreHotKeyDisplay.Text = "请按下快捷键...";
        }

        private void HotKeyRecorder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_activeRecorder != null && _activeRecorder == sender)
            {
                _activeRecorder.Background = (System.Windows.Media.Brush)System.Windows.Application.Current.Resources["RecorderBackgroundBrush"];
                _activeRecorder = null;
            }
            UpdateDisplay();
        }

        private void HotKeyRecorder_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_activeRecorder == null) return;

            e.Handled = true;

            var key = e.Key;
            if (key == Key.System) key = e.SystemKey;
            if (key == Key.ImeProcessed) key = e.ImeProcessedKey;

            var modifiers = Keyboard.Modifiers;

            if (key == Key.Escape)
            {
                _activeRecorder.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                return;
            }

            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LWin || key == Key.RWin)
            {
                if (_activeRecorder.Tag?.ToString() == "Pocket")
                    PocketHotKeyDisplay.Text = GetModifiersText(modifiers) + "...";
                else
                    RestoreHotKeyDisplay.Text = GetModifiersText(modifiers) + "...";
                return;
            }

            if (modifiers == ModifierKeys.None)
            {
                if (_activeRecorder.Tag?.ToString() == "Pocket")
                    PocketHotKeyDisplay.Text = "需要组合键 (Ctrl/Alt/Shift/Win)";
                else
                    RestoreHotKeyDisplay.Text = "需要组合键 (Ctrl/Alt/Shift/Win)";
                return;
            }

            var targetConfig = _activeRecorder.Tag?.ToString() == "Pocket" ? _tempConfig.PocketHotKey : _tempConfig.RestoreHotKey;
            targetConfig.Modifiers = modifiers;
            targetConfig.Key = key;

            _activeRecorder.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            AutoSave();
        }

        private void AutoSave()
        {
            ConfigManager.Save(_tempConfig);
            if (System.Windows.Application.Current.MainWindow is MainWindow main)
            {
                main.RegisterHotKey();
            }
            ToastNotification.Show("快捷键已保存", this);
        }

        private string GetModifiersText(ModifierKeys modifiers)
        {
            var sb = new System.Text.StringBuilder();
            if ((modifiers & ModifierKeys.Windows) != 0) sb.Append("Win + ");
            if ((modifiers & ModifierKeys.Control) != 0) sb.Append("Ctrl + ");
            if ((modifiers & ModifierKeys.Alt) != 0) sb.Append("Alt + ");
            if ((modifiers & ModifierKeys.Shift) != 0) sb.Append("Shift + ");
            return sb.ToString();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _tempConfig = new FullConfig();
            UpdateDisplay();
            AutoSave();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
