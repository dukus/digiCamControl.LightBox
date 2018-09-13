using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraControl.Devices;
using Newtonsoft.Json;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class CameraProfile
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public ValueItemCollection Values { get; set; }


        public CameraProfile()
        {
            Id = Guid.NewGuid().ToString();
        }

        public static CameraProfile Load(string file)
        {
            try
            {
                return JsonConvert.DeserializeObject<CameraProfile>(File.ReadAllText(file));
            }
            catch (Exception e)
            {
                Log.Error("Error to load camera profile", e);
            }
            return new CameraProfile();
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                string file = Path.Combine(Settings.Instance.CameraProfileFolder, Id + ".json"); 
                Utils.CreateFolder(file);
                File.WriteAllText(file, json);
            }
            catch (Exception e)
            {
                Log.Error("Error to save session", e);
            }
        }

    }
}
