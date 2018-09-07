using System;
using digiCamControl.LightBox.Core.Clasess;

namespace digiCamControl.LightBox.Plugins.ExportPlugins
{
    public class CopyPluginViewModel:BaseExportPluginViewModel
    {
        public string Folder
        {
            get { return ExportItem.Variables["Folder"].ToString(); }
            set
            {
                ExportItem.Variables["Folder"] = value;
            }
        }

        public string FileNameTemplate
        {
            get { return ExportItem.Variables["FileNameTemplate"].ToString(); }
            set
            {
                ExportItem.Variables["FileNameTemplate"] = value;
            }
        }
        public CopyPluginViewModel()
        {
            ExportItem = new ExportItem();
        }

        public CopyPluginViewModel(ExportItem item)
        {
            ExportItem = item;
        }

        public void SetDefault()
        {
            SetBaseDefault();
            Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            FileNameTemplate = "{SessionName}\\DSC_{ImageNumber:D2}.jpg";
        }
    }
}
