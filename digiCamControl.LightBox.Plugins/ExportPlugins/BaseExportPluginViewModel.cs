using System;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight;

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


        public void SetBaseDefault()
        {
            Resize = false;
            Width = 500;
            Height = 500;
            ImageSource = 1;
        }
    }
}

