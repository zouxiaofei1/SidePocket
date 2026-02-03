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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
