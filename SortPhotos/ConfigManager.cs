using System;
using System.IO;
using System.Configuration;

namespace SortPhotos.Logic
{
    public static class ConfigManager
    {
        private const string DefaultScanPathKey = "DefaultScanPath";

        /// <summary>
        /// Gets the default scan path from configuration
        /// </summary>
        public static string GetDefaultScanPath()
        {
            string path = ConfigurationManager.AppSettings[DefaultScanPathKey];

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                // Use My Pictures folder as fallback
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "");
            }

            return path;
        }

        /// <summary>
        /// Saves the default scan path to configuration
        /// </summary>
        public static void SaveDefaultScanPath(string path)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.AppSettings.Settings[DefaultScanPathKey] == null)
            {
                config.AppSettings.Settings.Add(DefaultScanPathKey, path);
            }
            else
            {
                config.AppSettings.Settings[DefaultScanPathKey].Value = path;
            }

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}