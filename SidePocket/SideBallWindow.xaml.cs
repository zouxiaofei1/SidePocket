using System;
using System.Windows;
using System.Windows.Input;

namespace SidePocket
{
    public partial class SideBallWindow : Window
    {
        private IntPtr _targetHwnd;

        public SideBallWindow(IntPtr targetHwnd, System.Windows.Media.ImageSource? icon)
        {
            InitializeComponent();
            _targetHwnd = targetHwnd;
            if (icon != null)
            {
                AppIcon.Source = icon;
            }

            this.SourceInitialized += (s, e) =>
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                int exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TOOLWINDOW);
            };
        }

        private bool _isDragging = false;
        private double _initialMouseY;
        private double _initialWindowTop;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                
                // 使用 Win32 GetCursorPos 获取屏幕绝对坐标，避免 WPF PointToScreen 在高 DPI 或窗口移动时的抖动
                NativeMethods.GetCursorPos(out var pt);
                _initialMouseY = pt.Y;
                _initialWindowTop = this.Top;
                
                this.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Border_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDragging)
            {
                NativeMethods.GetCursorPos(out var pt);
                
                // 获取当前 DPI 缩放比例
                var source = PresentationSource.FromVisual(this);
                double scaleY = 1.0;
                if (source?.CompositionTarget != null)
                {
                    scaleY = source.CompositionTarget.TransformToDevice.M22;
                }

                double deltaY = (pt.Y - _initialMouseY) / scaleY;
                
                // 仅更新 Top，锁定 Left 保持在屏幕右侧
                this.Top = _initialWindowTop + deltaY;
            }
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            this.ReleaseMouseCapture();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 双击恢复窗口
            NativeMethods.ShowWindow(_targetHwnd, NativeMethods.SW_SHOW);
            NativeMethods.ShowWindow(_targetHwnd, NativeMethods.SW_RESTORE);
            this.Close();
        }
    }
}
