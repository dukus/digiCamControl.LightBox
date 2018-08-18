using System.Windows.Controls;
using digiCamControl.LightBox.Core.Clasess;

namespace digiCamControl.LightBox.Core.Interfaces
{
    public interface IExportPlugin
    {
        string Name { get;  }
        string Icon { get;  }
        string Description { get;  }
        string Id { get; }
        bool Export(ExportItem item);
        ContentControl GetConfig(ExportItem item);
        /// <summary>
        /// Return a instancee of a default settings for a plugin
        /// </summary>
        /// <returns></returns>
        ExportItem GetDefault();
    }
}