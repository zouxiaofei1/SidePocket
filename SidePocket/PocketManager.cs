using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SidePocket
{
    public static class PocketManager
    {
        private static readonly Dictionary<IntPtr, SideBallWindow> _pocketedWindows = new Dictionary<IntPtr, SideBallWindow>();
        private static int _ballCount = 0;

        public static bool IsPocketed(IntPtr hwnd)
        {
            return _pocketedWindows.ContainsKey(hwnd);
        }

        public static void Add(IntPtr hwnd, ImageSource? icon)
        {
            if (IsPocketed(hwnd)) return;

            var ball = new SideBallWindow(hwnd, icon);
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            ball.Left = screenWidth - 36;
            ball.Top = (screenHeight / 3) + (_ballCount * 38);
            _ballCount++;

            ball.Closed += (s, e) => {
                _pocketedWindows.Remove(hwnd);
                _ballCount--;
                RearrangeBalls();
            };

            _pocketedWindows[hwnd] = ball;
            ball.Show();
        }

        private static void RearrangeBalls()
        {
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            int i = 0;
            foreach (var ball in _pocketedWindows.Values)
            {
                ball.Top = (screenHeight / 3) + (i * 38);
                i++;
            }
            _ballCount = i;
        }

        public static void RestoreAll()
        {
            var windows = new List<IntPtr>(_pocketedWindows.Keys);
            foreach (var hwnd in windows)
            {
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_SHOW);
                NativeMethods.ShowWindow(hwnd, NativeMethods.SW_RESTORE);
                if (_pocketedWindows.TryGetValue(hwnd, out var ball))
                {
                    ball.Close();
                }
            }
        }
    }
}
