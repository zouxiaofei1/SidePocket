using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SidePocket
{
    public partial class EdgeTriggerWindow : Window
    {
        private PocketButtonWindow? _pocketButton;
        private DispatcherTimer _hideTimer;

        public EdgeTriggerWindow()
        {
            InitializeComponent();
            _hideTimer = new DispatcherTimer();
            _hideTimer.Interval = TimeSpan.FromSeconds(2);
            _hideTimer.Tick += HideTimer_Tick;

            this.SourceInitialized += (s, e) =>
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                int exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TOOLWINDOW);
            };
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ShowPocketButton();
        }

        private void ShowPocketButton()
        {
            if (_pocketButton == null)
            {
                _pocketButton = new PocketButtonWindow();
                _pocketButton.Left = 12; // Move slightly away from edge
                _pocketButton.Top = 10;
                _pocketButton.Closed += (s, e) => _pocketButton = null;
            }
            _pocketButton.Show();
            _hideTimer.Stop();
            _hideTimer.Start();
        }

        private void HideTimer_Tick(object? sender, EventArgs e)
        {
            if (_pocketButton != null && !_pocketButton.IsMouseOver)
            {
                _pocketButton.Hide();
                _hideTimer.Stop();
            }
        }
    }
}
