using System.Text.Json;
using System.Text.Json.Serialization;

namespace CLua
{

    public static class ConfigManager
    {

        // Source Generator context - přidej PŘED ConfigManager třídu
        private static string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private static Dictionary<string, string> configData = new Dictionary<string, string>();

        static ConfigManager()
        {
            LoadConfig(); // Načteme data při startu
        }

        private static void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                try
                {
                    //configData = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(configPath));
                    // configData = JsonSerializer.Deserialize(
                    //    File.ReadAllText(configPath),
                    //    JsonContext.Default.DictionaryStringString
                    //) ?? new Dictionary<string, string>();
                    configData = JsonSerializer.Deserialize<Dictionary<string, string>>(
                          File.ReadAllText(configPath),
                          JsonHelper.serializerOptions
                      ) ?? new Dictionary<string, string>();
                }
                catch
                {
                    //Console.WriteLine("⚠️ Chyba při načítání configu. Použit výchozí nastavení.");
                    configData = new Dictionary<string, string>();
                }
            }
        }

        private static void SaveConfig()
        {
            //File.WriteAllText(configPath, JsonSerializer.Serialize(configData, new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            //}));

            //var json = JsonSerializer.Serialize(
            //    configData,
            //    JsonContext.Default.DictionaryStringString
            //);
            //File.WriteAllText(configPath, json, System.Text.Encoding.UTF8);

            var json = JsonSerializer.Serialize(configData, JsonHelper.serializerOptions);
            File.WriteAllText(configPath, json, System.Text.Encoding.UTF8);
        }

        public static void SaveSetting(string key, string value)
        {
            configData[key] = value;
            SaveConfig();
        }

        public static string LoadSetting(string key, string defaultValue = "")
        {
            //return configData.ContainsKey(key) ? configData[key] : defaultValue;
            return configData.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}