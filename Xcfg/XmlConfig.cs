using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Xcfg
{
    public class XmlConfig<T> where T : new()
    {
        protected XmlConfig()
        {
            
        }
        public static T Instance { get; private set; }

        static XmlConfig()
        {
            Instance = new T();
            var cfgFolder = Helper.GetAppCfgFolder();
            if (!Directory.Exists(cfgFolder))
            {
                Directory.CreateDirectory(cfgFolder);
            }
            var cfgName = "";
            var attrs = Instance.GetType().GetCustomAttributes(false);
            foreach (var typeAttribute in attrs)
            {
                if (typeAttribute is XmlRootAttribute)
                {
                    var ta = typeAttribute as XmlRootAttribute;
                    cfgName = ta.ElementName;
                }
            }
            if (string.IsNullOrWhiteSpace(cfgName))
            {
                cfgName = Instance.GetType().Name;
            }
            var cfgPath = cfgFolder + "/" + cfgName + ".config";
            Instance = LoadLocalCfg(cfgPath, 1, 0);
            if (Instance != null)
            {

            }
            else
            {
                
            }
        }

        public static T GetRemoteConfigSectionParam(string cfgName)
        {
            
        }

        [XmlAttribute("major")]
        public int Major { get; set; }

        [XmlAttribute("minor")]
        public int Minor { get; set; }

        public static T LoadLocalCfg(string cfgPath,int major,int minor)
        {
            if(!File.Exists(cfgPath))
            {
                return default(T);
            }
            return Helper.DeserializeFromXml<T>(cfgPath);
        }
    }
}
