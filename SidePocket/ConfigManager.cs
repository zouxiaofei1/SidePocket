using System.Text.Json;
using System.IO;
using System.Windows.Input;

namespace SidePocket
{
    public class HotKeyConfig
    {
        public ModifierKeys Modifiers { get; set; } = ModifierKeys.Windows;
        public Key Key { get; set; } = Key.Oem3;

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            if ((Modifiers & ModifierKeys.Windows) != 0) sb.Append("Win + ");
            if ((Modifiers & ModifierKeys.Control) != 0) sb.Append("Ctrl + ");
            if ((Modifiers & ModifierKeys.Alt) != 0) sb.Append("Alt + ");
            if ((Modifiers & ModifierKeys.Shift) != 0) sb.Append("Shift + ");
            
            string keyText = Key.ToString();
            if (Key == Key.Oem3) keyText = "~";
            else if (Key >= Key.D0 && Key <= Key.D9) keyText = Key.ToString().Substring(1);
            else if (Key >= Key.NumPad0 && Key <= Key.NumPad9) keyText = "Num" + Key.ToString().Substring(6);
            
            sb.Append(keyText);
            return sb.ToString();
        }
    }

    public class FullConfig
    {
        public HotKeyConfig PocketHotKey { get; set; } = new HotKeyConfig { Modifiers = ModifierKeys.Windows, Key = Key.Oem3 };
        public HotKeyConfig RestoreHotKey { get; set; } = new HotKeyConfig { Modifiers = ModifierKeys.Windows | ModifierKeys.Shift, Key = Key.Oem3 };
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "xfsoftware", "sidepocket", "settings.json");
        
        public static FullConfig Current { get; private set; } = new FullConfig();

        static ConfigManager()
        {
            Load();
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<FullConfig>(json);
                    if (config != null) Current = config;
                }
            }
            catch { }
        }

        public static void Save(FullConfig config)
        {
            try
            {
                Current = config;
                string json = JsonSerializer.Serialize(config);
                string? directory = Path.GetDirectoryName(ConfigPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
