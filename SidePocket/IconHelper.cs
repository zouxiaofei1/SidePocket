using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SidePocket
{
    public static class IconHelper
    {
        public static ImageSource? GetIcon(IntPtr hwnd)
        {
            try
            {
                IntPtr hIcon = NativeMethods.SendMessage(hwnd, NativeMethods.WM_GETICON, NativeMethods.ICON_BIG, 0);
                if (hIcon == IntPtr.Zero)
                {
                    hIcon = NativeMethods.SendMessage(hwnd, NativeMethods.WM_GETICON, NativeMethods.ICON_SMALL, 0);
                }
                if (hIcon == IntPtr.Zero)
                {
                    hIcon = NativeMethods.GetClassLongPtr(hwnd, NativeMethods.GCL_HICON);
                }
                if (hIcon == IntPtr.Zero)
                {
                    hIcon = NativeMethods.GetClassLongPtr(hwnd, NativeMethods.GCL_HICONSM);
                }

                if (hIcon != IntPtr.Zero)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(
                        hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch
            {
                // Fallback or ignore
            }
            return null;
        }
    }
}
