using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace SidePocket
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            NativeMethods.SetProcessDPIAware();
            ApplyTheme();
            SystemEvents.UserPreferenceChanged += (s, args) =>
            {
                if (args.Category == UserPreferenceCategory.General)
                {
                    ApplyTheme();
                }
            };
            base.OnStartup(e);
        }

        public static void ApplyTheme()
        {
            bool isDark = IsDarkMode();
            var resources = System.Windows.Application.Current.Resources;

            void UpdateBrush(string brushKey, string colorHex)
            {
                var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
                if (resources.Contains(brushKey))
                {
                    if (resources[brushKey] is SolidColorBrush brush)
                    {
                        if (brush.IsFrozen)
                        {
                            resources[brushKey] = new SolidColorBrush(color);
                        }
                        else
                        {
                            brush.Color = color;
                        }
                    }
                }
                else
                {
                    resources[brushKey] = new SolidColorBrush(color);
                }
            }

            if (isDark)
            {
                UpdateBrush("WindowBackgroundBrush", "#1C1C1C");
                UpdateBrush("CardBackgroundBrush", "#2D2D2D");
                UpdateBrush("HeaderTextBrush", "#AAAAAA");
                UpdateBrush("MainTextBrush", "#FFFFFF");
                UpdateBrush("SubTextBrush", "#BBBBBB");
                UpdateBrush("BorderBrush", "#33FFFFFF");
                UpdateBrush("RecorderBackgroundBrush", "#3D3D3D");
                UpdateBrush("HoverBackgroundBrush", "#22FFFFFF");
            }
            else
            {
                UpdateBrush("WindowBackgroundBrush", "#F3F3F3");
                UpdateBrush("CardBackgroundBrush", "#FFFFFF");
                UpdateBrush("HeaderTextBrush", "#666666");
                UpdateBrush("MainTextBrush", "#000000");
                UpdateBrush("SubTextBrush", "#666666");
                UpdateBrush("BorderBrush", "#11000000");
                UpdateBrush("RecorderBackgroundBrush", "#F9F9F9");
                UpdateBrush("HoverBackgroundBrush", "#11000000");
            }
        }

        private static bool IsDarkMode()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue("AppsUseLightTheme");
                        if (value is int i)
                        {
                            return i == 0;
                        }
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
