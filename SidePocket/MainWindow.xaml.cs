using System;
using System.Windows;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace SidePocket
{
    public partial class MainWindow : Window
    {
        private const int HOTKEY_ID = 9000;
        private HwndSource? _source;
        private Forms.NotifyIcon? _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
            // 确保窗口不可见但不影响句柄创建
            this.Width = 0;
            this.Height = 0;
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = false;
            this.Opacity = 0;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // 获取当前隐藏主窗口的句柄，用于注册热键消息钩子
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);

            RegisterHotKey();
            InitEdgeTrigger();
            InitNotifyIcon();
        }

        private void InitNotifyIcon()
        {
            _notifyIcon = new Forms.NotifyIcon();
            try
            {
                var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/resources/icon.ico")).Stream;
                _notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "SidePocket";
            _notifyIcon.MouseDown += NotifyIcon_MouseDown;
        }

        private void NotifyIcon_MouseDown(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Right)
            {
                var menu = new FluentContextMenu();
                
                // 强制设置宽度和高度策略
                menu.Width = 160; 
                
                var cursor = Forms.Cursor.Position;
                
                // 获取当前主屏幕的 DPI 缩放，用于像素到 DIU 的转换
                double scaleX = 1.0, scaleY = 1.0;
                var presentationSource = PresentationSource.FromVisual(this);
                if (presentationSource?.CompositionTarget != null)
                {
                    scaleX = presentationSource.CompositionTarget.TransformToDevice.M11;
                    scaleY = presentationSource.CompositionTarget.TransformToDevice.M22;
                }

                // 确保菜单出现在鼠标左上方，并考虑到边距和 DPI
                // 我们根据截图微调偏移量：cursor.X / scaleX 已经是鼠标所在的逻辑坐标
                // 160 是菜单逻辑宽度，我们减去它使菜单右边缘对齐鼠标
                // 向上偏移量 145 是根据菜单内容高度估算的逻辑高度
                menu.Left = (cursor.X / scaleX) - menu.Width + 10; // 稍微向右偏一点点避免被鼠标挡住
                menu.Top = (cursor.Y / scaleY) - 145; 
                
                menu.Show();
                
                // 重要：在显示后调用 SetForegroundWindow，确保菜单能正常失去焦点关闭并响应点击
                var helper = new WindowInteropHelper(menu);
                NativeMethods.SetForegroundWindow(helper.Handle);
                
                menu.Activate();
                menu.Focus();
            }
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            // 注册 Win + ~ (VK_OEM_3)
            if (!NativeMethods.RegisterHotKey(helper.Handle, HOTKEY_ID, NativeMethods.MOD_WIN, NativeMethods.VK_OEM_3))
            {
                System.Windows.MessageBox.Show("热键注册失败，可能被占用");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                PocketActiveWindow();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private EdgeTriggerWindow? _trigger;

        public void PocketActiveWindow()
        {
            // 1. 获取当前激活窗口
            IntPtr targetHwnd = NativeMethods.GetForegroundWindow();

            if (targetHwnd == IntPtr.Zero || IsOwnWindow(targetHwnd))
                return; // 不要把自己收纳了

            if (PocketManager.IsPocketed(targetHwnd))
                return; // 已经收纳过了

            // 2. 获取图标
            var icon = IconHelper.GetIcon(targetHwnd);

            // 3. 隐藏目标窗口
            NativeMethods.ShowWindow(targetHwnd, NativeMethods.SW_HIDE);

            // 4. 管理收纳
            PocketManager.Add(targetHwnd, icon);
        }

        private bool IsOwnWindow(IntPtr hwnd)
        {
            var helper = new WindowInteropHelper(this);
            if (hwnd == helper.Handle) return true;
            
            // 检查是否是其他的辅助窗口
            foreach (Window win in System.Windows.Application.Current.Windows)
            {
                if (new WindowInteropHelper(win).Handle == hwnd) return true;
            }
            return false;
        }

        private void InitEdgeTrigger()
        {
            _trigger = new EdgeTriggerWindow();
            _trigger.Show();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source?.RemoveHook(HwndHook);
            var helper = new WindowInteropHelper(this);
            NativeMethods.UnregisterHotKey(helper.Handle, HOTKEY_ID);
            base.OnClosed(e);
        }
    }
}