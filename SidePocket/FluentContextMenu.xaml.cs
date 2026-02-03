using System;
using System.Windows;

namespace SidePocket
{
    public partial class FluentContextMenu : Window
    {
        public FluentContextMenu()
        {
            InitializeComponent();
            try
            {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/resources/icon.ico"));
            }
            catch { }
        }

        private void RestoreAll_Click(object sender, RoutedEventArgs e)
        {
            PocketManager.RestoreAll();
            this.Close();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.Show();
            this.Close();
        }

        private void Logo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Settings_Click(sender, e);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // 通过关闭主窗口来触发有序退出
            System.Windows.Application.Current.MainWindow.Close();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
