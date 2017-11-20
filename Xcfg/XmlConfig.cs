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
        public static T Instance { get; protected set; }

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
                var rcs = GetRemoteConfigSectionParam(cfgName);
            }
        }

        public static RemoteConfigSection GetRemoteConfigSectionParam(string cfgName)
        {
            var rcfg = new RemoteConfigSectionCollection();
            rcfg.Application = Helper.GetAppName();
            rcfg.Machine = Environment.MachineName;
            rcfg.Environment = Helper.GetEnvironment();
            rcfg.Sections = new RemoteConfigSection[1];
            rcfg.Sections[0] = new RemoteConfigSection { SectionName = cfgName.ToLower(), MajorVersion = 1, MinorVersion = 0 };
            var rcfgResult = GetServerVersions(rcfg);
            if (rcfgResult == null || rcfgResult.Sections.Length == 0)
            {
                return null;
            }
            else
            {
                return rcfgResult.Sections[0];
            }
        }

        private static RemoteConfigSectionCollection GetServerVersions(RemoteConfigSectionCollection rcfg)
        {
            var bytes = Helper.SerializeToXml(rcfg);
            var url = Helper.GetRemoteCfgUrl();
            var xmlStr = Helper.HttpPost(url, Encoding.UTF8.GetString(bytes));
            return Helper.DeserializeFromXml<RemoteConfigSectionCollection>(xmlStr);
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
