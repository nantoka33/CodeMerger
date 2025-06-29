using System.IO;
using System.Text.Json;

namespace CodeMerger
{
    public class Settings
    {
        public List<string> ExcludeFiles { get; set; } = new();
    }

    public static class SettingsService
    {
        private static readonly string AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeMergerApp");
        private static readonly string SettingsPath = Path.Combine(AppDir, "settings.json");

        public static Settings Load()
        {
            if (!File.Exists(SettingsPath)) return new Settings { ExcludeFiles = new List<string> { "App.xaml", "App.xaml.cs", "AssemblyInfo.cs" } };
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }

        public static void Save(Settings settings)
        {
            Directory.CreateDirectory(AppDir);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
    }
}
