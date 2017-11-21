using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Xcfg.Console
{
    [XmlRoot("connectionStrings")]
    public class ConnectionStrings:XmlConfig<ConnectionStrings>
    {
        [XmlElement("add")]
        public ConnectionString[] Conns { get; set; }
    }

    public class ConnectionString
    {
        [XmlAttribute("name")]
       public string Name { get; set; }

        [XmlAttribute("providerName")]
        public string ProviderName { get; set; }

        [XmlAttribute("connectionString")]
        public string ConnectionStr { get; set; }
    }
}
