using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using digiCamControl.LightBox.Core.Clasess;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins
{
    public class CapturePanel: IPanelItem
    {
        public CapturePanelViewModel ViewModel { get; set; }

        public CapturePanel()
        {
            Panel = new CapturePanelView();
            ViewModel = (CapturePanelViewModel) Panel.DataContext;
            
        }

        public string Name => "Capture";
        public string Id => "{D65AD198-1641-491B-872A-3C4EA367456A}";
        public string Icon => "CameraIris";
        public ContentControl Panel { get; }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
