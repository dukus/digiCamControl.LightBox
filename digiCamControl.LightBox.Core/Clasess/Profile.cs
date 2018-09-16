using System;
using System.Collections.ObjectModel;
using System.IO;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class Profile:ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public string SessionName { get; set; }
        public int SessionCounter { get; set; }
        public string Id { get; set; }
        public string CameraProfileId { get; set; }


        
        public AsyncObservableCollection<FileItem> Files { get; set; }
        public ValueItemCollection Variables { get; set; }
        public AsyncObservableCollection<ExportItem> ExportItems { get; set; }


        public Profile()
        {
            Files = new AsyncObservableCollection<FileItem>();
            Variables = new ValueItemCollection();
            ExportItems = new AsyncObservableCollection<ExportItem>();
            Id = Guid.NewGuid().ToString();
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                string file = GetFileName();
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
        public static Profile Load(string filename)
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

        public string GetFileName()
        {
            return Path.Combine(Settings.Instance.ProfileFolder, Id + ".json");
        }

        public override string ToString()
        {
            return Name;
        }

        public void CleanUp()
        {
            try
            {
                foreach (FileItem item in Files)
                {
                    item.CleanUp();
                }
                Files.Clear();
            }
            catch (Exception e)
            {
                Log.Debug("Cleanup error", e);
            }
        }
    }
}
