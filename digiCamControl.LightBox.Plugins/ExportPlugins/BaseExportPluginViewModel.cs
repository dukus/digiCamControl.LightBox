using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;
using ImageMagick;

namespace digiCamControl.LightBox.Plugins.ExportPlugins
{
    public class BaseExportPluginViewModel:ViewModelBase
    {
        public ExportItem ExportItem { get; set; }


        public int Width
        {
            get { return ExportItem.Variables.GetInt("Width"); }
            set
            {
                ExportItem.Variables["Width"] = value;
            }
        }

        public int ImageSource
        {
            get { return ExportItem.Variables.GetInt("ImageSource"); }
            set
            {
                ExportItem.Variables["ImageSource"] = value;
            }
        }


        public int Height
        {
            get { return ExportItem.Variables.GetInt("Height"); }
            set
            {
                ExportItem.Variables["Height"] = value;
            }
        }

        public bool Resize
        {
            get { return ExportItem.Variables.GetBool("Resize"); }
            set
            {
                ExportItem.Variables["Resize"] = value;
                RaisePropertyChanged();
            }
        }

        public IMagickImage GetImage(ExportItem item, FileItem fileItem, Profile profile)
        {
            IMagickImage magickImage = null;
            switch (ImageSource)
            {
                // original
                case 0:
                    magickImage = new MagickImage(fileItem.TempFile);
                    break;
                // edited
                case 1:
                    magickImage = new MagickImage(fileItem.TempFile);
                    magickImage = ServiceProvider.Instance.PreAdjustPlugins.Aggregate(magickImage, (current, plugin) => plugin.Execute(current, profile.Variables));
                    magickImage = ServiceProvider.Instance.AdjustPlugins.Aggregate(magickImage, (current, plugin) => plugin.Execute(current, fileItem.Variables));
                    break;
                // combined
                case 2:
                    break;

            }

            if (Resize)
            {
                MagickGeometry geometry = new MagickGeometry(Width, Height);
                geometry.IgnoreAspectRatio = false;
                magickImage.Resize(geometry);
            }
            return magickImage;
        }

        public Dictionary<string, object> GetFormatValues(ExportItem item, FileItem fileItem, Profile profile)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("ImageNumber", fileItem.ImageNumber);
            values.Add("SessionName", profile.SessionName);
            values.Add("SessionCounter", profile.SessionCounter);
            return values;
        }


        public void SetBaseDefault()
        {
            Resize = false;
            Width = 500;
            Height = 500;
            ImageSource = 1;
        }
    }
}

