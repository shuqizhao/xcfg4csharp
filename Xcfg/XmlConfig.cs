using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Xcfg
{
    public class XmlConfig
    {
        public string Name { get; set; }

        [XmlAttribute("majorVersion")]
        public int Major { get; set; }

        [XmlAttribute("minorVersion")]
        public int Minor { get; set; }
    }
    public class XmlConfig<T> : XmlConfig where T : XmlConfig, new()
    {
        protected XmlConfig()
        {

        }
        public static T Instance
        {
            get
            {
                var cfgName = GetCfgName();
                return (T)XmlConfigManager.GetXmlConfig(cfgName);
            }
            set
            {
                var cfgName = GetCfgName();
                if (value != null)
                    XmlConfigManager.AddXmlConfig(cfgName, value);
            }
        }

        private static string GetCfgName()
        {
            var cfgName = "";
            var attrs = typeof(T).GetCustomAttributes(false);
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
                cfgName = typeof(T).Name;
            }
            return cfgName;
        }

        static XmlConfig()
        {
            var cfgFolder = Helper.GetAppCfgFolder();
            if (!Directory.Exists(cfgFolder))
            {
                Directory.CreateDirectory(cfgFolder);
            }

            var cfgName = GetCfgName();
            var cfgPath = cfgFolder + "/" + cfgName + ".config";
            Instance = LoadLocalCfg(cfgPath);
            var majorVersion = 1;
            var minorVersion = 0;
            if (Instance != null)
            {
                majorVersion = Instance.Major;
                minorVersion = Instance.Minor;
            }
            var rcs = Helper.GetRemoteConfigSectionParam(cfgName, majorVersion, minorVersion);
            if (rcs != null)
            {
                var sucess = Helper.DownloadRemoteCfg("", rcs.DownloadUrl, cfgPath);
                if (sucess)
                {
                    Instance = LoadLocalCfg(cfgPath);
                }
            }
        }


        private static T LoadLocalCfg(string cfgPath)
        {
            if (!File.Exists(cfgPath))
            {
                return default(T);
            }
            var xmlStr = File.ReadAllText(cfgPath, Encoding.UTF8);
            return Helper.DeserializeFromXml<T>(xmlStr);
        }
    }
}
