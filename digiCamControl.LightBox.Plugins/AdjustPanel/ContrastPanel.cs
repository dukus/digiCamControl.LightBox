using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{

    public class ContrastPanel : IPanelItem
    {
        public string Name => "ContrastBrightnes";
        public string Id => "{B18025B2-7FB2-4441-B8BA-D1D75A4F2F49}";
        public string Icon => "ContrastBox";
        public ContentControl Panel { get; }

        public ContrastPanel()
        {
            Panel = new ContrastPanelView();
        }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
