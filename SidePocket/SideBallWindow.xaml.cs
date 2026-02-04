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
            this.Loaded += SideBallWindow_Loaded;
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
        private bool _hasMoved = false;
        private double _initialMouseY;
        private double _initialWindowTop;
        private System.Windows.Point _startPoint;
        private DateTime _startTime;

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isDragging)
            {
                NativeMethods.GetCursorPos(out var pt);
                
                double diffY = Math.Abs(pt.Y - _startPoint.Y);
                if (diffY > 5) // 增加阈值，减少误触
                {
                    _hasMoved = true;
                }

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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _isDragging = true;
            _hasMoved = false;

            // 使用 Win32 GetCursorPos 获取屏幕绝对坐标，避免 WPF PointToScreen 在高 DPI 或窗口移动时的抖动
            NativeMethods.GetCursorPos(out var pt);
            _initialMouseY = pt.Y;
            _initialWindowTop = this.Top;
            _startPoint = new System.Windows.Point(pt.X, pt.Y);
            _startTime = DateTime.Now;

            this.CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_isDragging)
            {
                bool wasMoved = _hasMoved;
                _isDragging = false;
                this.ReleaseMouseCapture();

                if (!wasMoved && (DateTime.Now - _startTime).TotalMilliseconds < 500)
                {
                    RestoreWindow();
                }
            }
        }

        protected override void OnLostMouseCapture(System.Windows.Input.MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            _isDragging = false;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 已改为通过 OnMouseLeftButtonDown 处理
        }

        private void Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 如果鼠标离开且没有捕获鼠标，重置状态（可选）
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 保留双击作为兜底
            RestoreWindow();
        }

        private void SideBallWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Resources["RippleStoryboard"] is System.Windows.Media.Animation.Storyboard sb)
            {
                sb.Begin(this);
            }
        }

        private void RestoreWindow()
        {
            if (this.Resources["ExitStoryboard"] is System.Windows.Media.Animation.Storyboard sb)
            {
                sb.Completed += (s, e) =>
                {
                    NativeMethods.ShowWindow(_targetHwnd, NativeMethods.SW_SHOW);
                    NativeMethods.ShowWindow(_targetHwnd, NativeMethods.SW_RESTORE);
                    NativeMethods.SetForegroundWindow(_targetHwnd);
                    this.Close();
                };
                sb.Begin(this);
            }
            else
            {
                NativeMethods.ShowWindow(_targetHwnd, NativeMethods.SW_SHOW);
                NativeMethods.ShowWindow(_targetHwnd, NativeMethods.SW_RESTORE);
                NativeMethods.SetForegroundWindow(_targetHwnd);
                this.Close();
            }
        }
    }
}
