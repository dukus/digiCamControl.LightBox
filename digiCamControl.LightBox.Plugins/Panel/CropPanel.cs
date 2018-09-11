using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins
{
    public class CropPanel: IPanelItem
    {
        public CropPanel()
        {
            Panel = new CropPanelView();
        }

        public string Name => "Crop";
        public string Id => "{2F72798A-00B9-4F42-9227-1E1C49498663}";
        public string Icon => "Crop";
        public ContentControl Panel { get; }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
