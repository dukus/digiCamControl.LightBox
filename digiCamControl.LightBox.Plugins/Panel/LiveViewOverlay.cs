using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins.Panel
{
    public class LiveViewOverlay: IPanelItem
    {
        public string Name => "Overlay";
        public string Id => "{D8D840DD-C6E4-43B4-ADAA-B7FADA1634D5}";
        public string Icon => "Grid";
        public ContentControl Panel { get; }

        public LiveViewOverlay()
        {
            Panel = new LiveViewOverlayView();
        }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
