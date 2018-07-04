using System;
using System.IO;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class Settings
    {
        private static Settings _instance;

        public static Settings Instance => _instance ?? (_instance = new Settings());

        public string DataFolder { get; set; }
        public string TempFolder { get; set; }

        public Settings()
        {
            DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                ServiceProvider.AppName);
            TempFolder = Path.Combine(DataFolder, "Temp");
        }
    }
}
