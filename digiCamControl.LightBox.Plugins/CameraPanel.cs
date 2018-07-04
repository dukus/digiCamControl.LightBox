using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins
{
    public class CameraPanel:IPanelItem
    {
        public string Name => "Camera";
        public string Id => "{7C3C2D81-7BC7-4AE5-9856-CB6E0B4D4A43}";
        public string Icon => "Camera";
        public ContentControl Panel { get; }

        public CameraPanel()
        {
            Panel = new CameraPanelView();
        }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
