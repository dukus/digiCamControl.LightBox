using System;
using digiCamControl.LightBox.Core.Clasess;
using GalaSoft.MvvmLight.Command;
using WPFFolderBrowser;

namespace digiCamControl.LightBox.Plugins.ExportPlugins
{
    public class CopyPluginViewModel:BaseExportPluginViewModel
    {
        public RelayCommand BrowseFolderCommand { get; set; }

        public string Folder
        {
            get { return ExportItem.Variables["Folder"]?.ToString(); }
            set
            {
                ExportItem.Variables["Folder"] = value;
            }
        }

        public string FileNameTemplate
        {
            get { return ExportItem.Variables["FileNameTemplate"]?.ToString(); }
            set
            {
                ExportItem.Variables["FileNameTemplate"] = value;
            }
        }
        public CopyPluginViewModel()
        {
            ExportItem = new ExportItem();
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
        }

        private void BrowseFolder()
        {
            WPFFolderBrowserDialog dlg = new WPFFolderBrowserDialog();
            dlg.InitialDirectory = Folder;
            if (dlg.ShowDialog() == true)
            {
                Folder = dlg.InitialDirectory;
            }
        }

        public CopyPluginViewModel(ExportItem item)
        {
            ExportItem = item;
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
        }

        public void SetDefault()
        {
            SetBaseDefault();
            Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            FileNameTemplate = "{SessionName}\\{SessionCounter:D4}\\DSC_{ImageNumber:D4}.jpg";
        }
    }
}
