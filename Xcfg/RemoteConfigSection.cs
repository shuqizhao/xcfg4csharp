using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Xcfg
{
    public class RemoteConfigSection
    {
        [XmlAttribute("name")]
        public string SectionName { get;set;}

        [XmlAttribute("majorVerion")]
        public int MajorVersion { get; set; }

        [XmlAttribute("minorVerion")]
        public int MinorVersion { get; set; }

        [XmlAttribute("downloadUrl")]
        public string DownloadUrl { get; set; }
    }
}
