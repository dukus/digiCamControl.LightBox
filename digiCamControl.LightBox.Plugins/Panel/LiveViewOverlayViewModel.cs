using System;
using System.Collections.Generic;
using System.IO;
using CameraControl.Devices;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

namespace digiCamControl.LightBox.Plugins.Panel
{
    public class LiveViewOverlayViewModel : ViewModelBase,IInit
    {
        //last captured file
        private string _lastFile="";
        public List<string> Overlays { get; set; }
        public Profile Profile => ServiceProvider.Instance.Profile;
        

        public string SelectedOverlay
        {
            get { return Profile.Variables.GetString("SelectedOverlay", "(None)"); }
            set
            {
                Profile.Variables["SelectedOverlay"] = value;
                var file = Path.Combine(Settings.Instance.OverlayFolder,
                    Profile.Variables.GetString("SelectedOverlay") + ".png");
                if (File.Exists(file))
                {
                    Profile.Variables["OverlayFileName"] = file;
                }
                else
                {
                    switch (Profile.Variables.GetString("SelectedOverlay"))
                    {
                        case "(Browse)":
                            Profile.Variables["OverlayFileName"] = LiveViewOverlayFile;
                            break;
                        case "(Last Captured)":
                            Profile.Variables["OverlayFileName"] = _lastFile;
                            break;
                        default:
                            Profile.Variables["OverlayFileName"] = "";
                            break;
                    }
                }
                RaisePropertyChanged(() => BrowseVisible);
            }
        }

        public bool BrowseVisible => Profile.Variables.GetString("SelectedOverlay") == "(Browse)";

        public double LiveViewOverlayTransparency
        {
            get { return Profile.Variables.GetDouble("LiveViewOverlayTransparency", 1); }
            set
            {
                Profile.Variables["LiveViewOverlayTransparency"] = value;
                RaisePropertyChanged(() => LiveViewOverlayTransparency);
            }
        }

        public string LiveViewOverlayFile
        {
            get { return Profile.Variables.GetString("LiveViewOverlayFile"); }
            set
            {
                Profile.Variables["LiveViewOverlayFile"] = value;
                Profile.Variables["OverlayFileName"] = LiveViewOverlayFile;
                RaisePropertyChanged(() => LiveViewOverlayFile);
            }
        }

        public RelayCommand BrowseFolderCommand { get; set; }


        public LiveViewOverlayViewModel()
        {
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            Overlays = new List<string> {"(None)", "(Browse)", "(Last Captured)" };
            try
            {
                var files = Directory.GetFiles(Settings.Instance.OverlayFolder, "*.png");
                foreach (var file in files)
                {
                    Overlays.Add(Path.GetFileNameWithoutExtension(file));
                }
                if (Overlays.Count > 0)
                    SelectedOverlay = Overlays[0];
            }
            catch (Exception e)
            {
                Log.Debug("Unable to load overlay list ", e);
            }
        }

        private void BrowseFolder()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                FileName = LiveViewOverlayFile,
                Filter = "Png files (*.png)|*.png|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                LiveViewOverlayFile = dialog.FileName;
            }
        }

        public void Init()
        {
            ServiceProvider.Instance.Message += Instance_Message;
        }

        private void Instance_Message(object sender, MessageArgs message)
        {
            if (message.Message == Messages.ImageCaptured)
            {
                _lastFile = message.ParamString;
                if (Profile.Variables.GetString("SelectedOverlay") == "(Last Captured)")
                    Profile.Variables["OverlayFileName"] = _lastFile;
            }
        }

        public void UnInit()
        {
            ServiceProvider.Instance.Message -= Instance_Message;
        }
    }
}
