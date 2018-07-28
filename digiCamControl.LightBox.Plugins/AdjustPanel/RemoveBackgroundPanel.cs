using System;
using System.Windows.Controls;
using digiCamControl.LightBox.Core.Interfaces;

namespace digiCamControl.LightBox.Plugins.AdjustPanel
{
    public class RemoveBackgroundPanel: IPanelItem
    {
        public string Name => "RemoveBackground";
        public string Id => "{219B00B1-24E5-496A-857F-77C04C972DBA}";
        public string Icon => "FoodVariant";
        public ContentControl Panel { get; }

        public RemoveBackgroundPanel()
        {
            Panel = new RemoveBackgroundPanelView();
        }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
