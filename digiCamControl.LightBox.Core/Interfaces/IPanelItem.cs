using System.Windows.Controls;

namespace digiCamControl.LightBox.Core.Interfaces
{
    public interface IPanelItem
    {
        string Name { get;  }
        string Id { get;  }
        string Icon { get;  }
        ContentControl Panel { get;  }
        bool Execute();
    }
}