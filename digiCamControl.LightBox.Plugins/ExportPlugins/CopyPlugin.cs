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
    public class CopyPlugin : CopyPluginViewModel, IExportPlugin
    {
        public string Name => "Disk";
        public string Icon => "Folder";
        public string Description => "Copy images to a specified folder";
        public string Id => "{F1EE5D0E-9D09-4C66-8CA7-B1B59F3E70DA}";

        public bool Export(ExportItem item)
        {
            return true;
        }

        public ContentControl GetConfig(ExportItem item)
        {
            return new CopyPluginView {DataContext = new CopyPluginViewModel(item)};
        }

        public ExportItem GetDefault()
        {
            ExportItem = new ExportItem();
            SetDefault();
            return ExportItem;
        }
    }
}
