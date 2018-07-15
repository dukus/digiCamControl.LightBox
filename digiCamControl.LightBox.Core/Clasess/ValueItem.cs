using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class ValueItem
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }

        public override string ToString()
        {
            return Name + "=" + Value;
        }
    }
}
