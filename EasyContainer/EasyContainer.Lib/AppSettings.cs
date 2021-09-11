namespace EasyContainer.Lib
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public static class AppSettings
    {
        public static string GetAppSettingsPath()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            string path;
            if (string.IsNullOrEmpty(env))
                path = Path.Combine(AppContext.BaseDirectory, "appSettings.Development.json");
            else if (env == "Production")
                path = Path.Combine(AppContext.BaseDirectory, "appSettings.json");
            else
                path = Path.Combine(AppContext.BaseDirectory, $"appSettings.{env}.json");

            return path;
        }

        // TODO: Add support for nested settings
        public static void AddOrUpdateAppSetting<T>(string key, T value)
        {
            var filePath = GetAppSettingsPath();
            var json = File.ReadAllText(filePath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json);

            var sectionPath = key.Split(":")[0];

            if (!string.IsNullOrEmpty(sectionPath))
            {
                var keyPath = key.Split(":")[1];
                jsonObj[sectionPath][keyPath] = value;
            }
            else
            {
                jsonObj[sectionPath] = value; // if no sectionpath just set the value
            }

            string output =
                JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(filePath, output);
        }
    }
}