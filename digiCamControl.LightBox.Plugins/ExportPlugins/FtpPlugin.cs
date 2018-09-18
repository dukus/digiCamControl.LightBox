using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins.ExportPlugins
{
    public class FtpPlugin : FtpPluginViewModel, IExportPlugin
    {
        public string Name => "Ftp";
        public string Icon => "FolderNetwork";
        public string Description => "Copy images to a specified FTP location";
        public string Id => "{B9422DAA-3F7D-4717-82E8-8584CCFE8057}";

        public bool Export(ExportItem item, FileItem fileItem, Profile profile)
        {
            return true;
        }

        public ContentControl GetConfig(ExportItem item)
        {
            return new FtpPluginView { DataContext = new FtpPluginViewModel(item) };
        }

        public ExportItem GetDefault()
        {
            ExportItem = new ExportItem();
            SetDefault();
            return ExportItem;
        }
    }
}
