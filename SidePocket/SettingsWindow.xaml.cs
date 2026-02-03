using System.Windows;

namespace SidePocket
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            try
            {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/resources/icon.ico"));
            }
            catch { }
            
            this.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) this.Close(); };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HotKeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            
            // 忽略单独的修饰键
            if (e.Key == System.Windows.Input.Key.System || e.Key == System.Windows.Input.Key.LWin || e.Key == System.Windows.Input.Key.RWin || 
                e.Key == System.Windows.Input.Key.LeftCtrl || e.Key == System.Windows.Input.Key.RightCtrl ||
                e.Key == System.Windows.Input.Key.LeftAlt || e.Key == System.Windows.Input.Key.RightAlt ||
                e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift)
            {
                return;
            }

            // 这里简单处理显示，实际逻辑需要保存修饰键状态
            string keyText = e.Key.ToString();
            if (e.Key == System.Windows.Input.Key.Oem3) keyText = "~";
            HotKeyTextBox.Text = keyText;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            HotKeyTextBox.Text = "~";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 实际保存设置到配置并刷新 MainWindow 的热键
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
