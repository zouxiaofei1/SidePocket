using System;
using System.Windows;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace SidePocket
{
    public partial class MainWindow : Window
    {
        private const int POCKET_HOTKEY_ID = 9000;
        private const int RESTORE_HOTKEY_ID = 9001;
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

            // 检查是否是手动启动（非命令行参数，或特定参数）
            // Windows 开机自启动通常不带额外参数，但我们可以通过 Environment.CommandLine 或自定义逻辑判断
            // 简单的做法：如果没有参数，或者不是通过注册表特定路径启动
            bool isAutoStart = false;
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.ToLower() == "--autostart")
                {
                    isAutoStart = true;
                    break;
                }
            }

            if (!isAutoStart)
            {
                ToastNotification.Show("SidePocket 已运行", this);
            }
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
            _notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
        }

        private void NotifyIcon_MouseDoubleClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                var settings = new SettingsWindow();
                settings.Show();
                var helper = new WindowInteropHelper(settings);
                NativeMethods.SetForegroundWindow(helper.Handle);
                settings.Activate();
                settings.Focus();
            }
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

        public void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            NativeMethods.UnregisterHotKey(helper.Handle, POCKET_HOTKEY_ID);
            NativeMethods.UnregisterHotKey(helper.Handle, RESTORE_HOTKEY_ID);

            // Register Pocket HotKey
            RegisterSingleHotKey(helper.Handle, POCKET_HOTKEY_ID, ConfigManager.Current.PocketHotKey);
            // Register Restore HotKey
            RegisterSingleHotKey(helper.Handle, RESTORE_HOTKEY_ID, ConfigManager.Current.RestoreHotKey);
        }

        private void RegisterSingleHotKey(IntPtr hwnd, int id, HotKeyConfig config)
        {
            uint modifiers = 0;
            if ((config.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0) modifiers |= (uint)NativeMethods.MOD_ALT;
            if ((config.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0) modifiers |= (uint)NativeMethods.MOD_CONTROL;
            if ((config.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0) modifiers |= (uint)NativeMethods.MOD_SHIFT;
            if ((config.Modifiers & System.Windows.Input.ModifierKeys.Windows) != 0) modifiers |= (uint)NativeMethods.MOD_WIN;

            uint vk = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(config.Key);

            if (!NativeMethods.RegisterHotKey(hwnd, id, modifiers, vk))
            {
                System.Windows.MessageBox.Show($"热键 {config} 注册失败，可能被占用");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == POCKET_HOTKEY_ID)
                {
                    PocketActiveWindow();
                    handled = true;
                }
                else if (id == RESTORE_HOTKEY_ID)
                {
                    PocketManager.RestoreLast();
                    handled = true;
                }
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

            // 过滤系统窗口（桌面、任务栏等）
            var className = new System.Text.StringBuilder(256);
            NativeMethods.GetClassName(targetHwnd, className, className.Capacity);
            string cls = className.ToString();
            
            // 常见的桌面和系统 UI 类名
            if (cls == "Progman" || cls == "WorkerW" || cls == "Shell_TrayWnd" || cls == "Shell_SecondaryTrayWnd" || 
                cls == "NotifyIconOverflowWindow" || cls == "TrayNotifyWnd" || cls == "Windows.UI.Core.CoreWindow")
                return;

            // 2. 获取图标
            var icon = IconHelper.GetIcon(targetHwnd);
            
            // 如果获取不到图标，且不是已知的可收纳窗口（可选逻辑：判断是否是普通窗口样式）
            // 用户提到：通过图标不存在或者为默认判定
            if (icon == null)
                return;

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

        private bool _isClosing = false;
        protected override void OnClosed(EventArgs e)
        {
            if (_isClosing) return;
            _isClosing = true;

            // 恢复所有收纳的窗口
            PocketManager.RestoreAll();

            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            _source?.RemoveHook(HwndHook);
            var helper = new WindowInteropHelper(this);
            NativeMethods.UnregisterHotKey(helper.Handle, POCKET_HOTKEY_ID);
            NativeMethods.UnregisterHotKey(helper.Handle, RESTORE_HOTKEY_ID);
            
            base.OnClosed(e);

            // 强制退出进程，确保所有后台线程和窗口被清理
            Environment.Exit(0);
        }
    }
}
