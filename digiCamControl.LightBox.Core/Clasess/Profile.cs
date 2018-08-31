using System;
using System.Collections.ObjectModel;
using System.IO;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using Newtonsoft.Json;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class Profile
    {
        public string Name { get; set; }
        public string SessionName { get; set; }
        public int SessionCounter { get; set; }
        [JsonIgnore]
        public AsyncObservableCollection<FileItem> Files { get; set; }
        public ValueItemCollection Variables { get; set; }
        public AsyncObservableCollection<ExportItem> ExportItems { get; set; }

        public Profile()
        {
            Files = new AsyncObservableCollection<FileItem>();
            Variables = new ValueItemCollection();
            ExportItems = new AsyncObservableCollection<ExportItem>();
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                string file = Path.Combine(Settings.Instance.ProfileFolder, Name + ".json");
                Utils.CreateFolder(file);
                File.WriteAllText(file,json);
            }
            catch (Exception e)
            {
                Log.Error("Error to save session", e);
            }
        }

        /// <summary>
        /// Will load session information from a file
        /// If file not fund or error uccured a new session object will be returned
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Profile Load(string filename)
        {
            try
            {
                return JsonConvert.DeserializeObject<Profile>(File.ReadAllText(filename));
            }
            catch (Exception e)
            {
                Log.Error("Error to load sesion", e);
            }
            return new Profile();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
