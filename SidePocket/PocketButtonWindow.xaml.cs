using System;
using System.Windows;
using System.Windows.Input;

namespace SidePocket
{
    public partial class PocketButtonWindow : Window
    {
        public PocketButtonWindow()
        {
            InitializeComponent();
            this.SourceInitialized += (s, e) =>
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                int exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TOOLWINDOW);
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 通过主窗口逻辑执行收纳
            ((MainWindow)System.Windows.Application.Current.MainWindow).PocketActiveWindow();
            this.Hide();
        }
    }
}
