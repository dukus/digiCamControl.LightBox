using System;
using System.IO;
using System.Threading;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
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
            Values = new ValueItemCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProfile"/> class.
        /// </summary>
        /// <param name="cameraDevice">The camera device for  default saved values</param>
        public CameraProfile(ICameraDevice cameraDevice)
        {
            Id = Guid.NewGuid().ToString();
            Values = new ValueItemCollection();
            if (cameraDevice != null)
            {
                Values["Mode"] = cameraDevice.Mode.Value;
                Values["ShutterSpeed"] = cameraDevice.ShutterSpeed.Value;
                Values["FNumber"] = cameraDevice.FNumber.Value;
                Values["IsoNumber"] = cameraDevice.IsoNumber.Value;
                Values["WhiteBalance"] = cameraDevice.WhiteBalance.Value;
                Values["CompressionSetting"] = cameraDevice.CompressionSetting.Value;
                Values["FocusMode"] = cameraDevice.FocusMode.Value;
                Values["ExposureMeteringMode"] = cameraDevice.ExposureMeteringMode.Value;
            }
        }

        public void Set(ICameraDevice cameraDevice)
        {
            SetValue(cameraDevice.Mode,"Mode");
            SetValue(cameraDevice.ShutterSpeed, "ShutterSpeed");
            SetValue(cameraDevice.FNumber, "FNumber");
            SetValue(cameraDevice.IsoNumber, "IsoNumber");
            SetValue(cameraDevice.WhiteBalance, "WhiteBalance");
            SetValue(cameraDevice.CompressionSetting, "CompressionSetting");
            SetValue(cameraDevice.FocusMode, "FocusMode");
            SetValue(cameraDevice.ExposureMeteringMode, "ExposureMeteringMode");
        }

        private void SetValue(PropertyValue<long> prop,string name)
        {
            if (!string.IsNullOrWhiteSpace(Values.GetString(name)))
            {
                try
                {
                    if (prop.Values.Contains(Values.GetString(name)))
                    {
                        prop.Value = Values.GetString(name);
                        Thread.Sleep(250);
                    }
                    else
                    {
                        Log.Debug("Wrong value " + Values.GetString(name) + " for " + name);
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Unable to set property " + name);
                }
            }
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
                Log.Error("Error to save profile", e);
            }
        }

        /// <summary>
        /// Deletes profile information from disk.
        /// </summary>
        public void Delete()
        {
            try
            {
                
                string file = Path.Combine(Settings.Instance.CameraProfileFolder, Id + ".json");
                Utils.DeleteFile(file);
            }
            catch (Exception e)
            {
                Log.Error("Error to delete profile", e);
            }
        }

    }
}
