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
        [XmlAttribute]
        public int IntValue { get; set; }

        [XmlText]
        public string StrValue { get; set; }
    }
}
