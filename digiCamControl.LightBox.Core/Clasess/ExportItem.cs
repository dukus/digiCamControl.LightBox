using System.Windows.Controls;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class ExportItem
    {
        private ContentControl _control;
        public string Name { get; set; }
        public string Id { get; set; }
        public string Icon { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public ContentControl Control
        {
            get
            {
                if (_control == null)
                {
                    foreach (var plugin in ServiceProvider.Instance.ExportPlugins)
                    {
                        if (plugin.Id == Id)
                        {
                            _control = plugin.GetConfig(this);
                        }
                    }
                }
                return _control;
            }
            
        }

        public ValueItemCollection Variables { get; set; }

        public ExportItem()
        {
            Variables = new ValueItemCollection();
        }
    }
}
