using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Xcfg
{
    public class RemoteConfigSectionCollection
    {
        [XmlAttribute("machine")]
        public string Machine { get; set; }

        [XmlAttribute("application")]
        public string Application { get; set; }

        [XmlAttribute("env")]
        public string Environment { get; set; }

        [XmlElement("section")]
        public RemoteConfigSection[] Sections { get; set; }
    }
}
