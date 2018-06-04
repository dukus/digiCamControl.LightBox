using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins
{
    public class LiveViewPanel:IPanelItem
    {
        public string Name => "Live View";
        public string Id => "{4711ADD7-310F-454D-A647-1E48F24E067B}";
        public string Icon => "ViewCarousel";
        public ContentControl Panel { get; }
        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
