using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xcfg
{
    internal class XmlConfigManager
    {
        private static readonly Dictionary<string, XmlConfig> _dic = new Dictionary<string, XmlConfig>();
        private static readonly object _lockObj = new object();

        static XmlConfigManager()
        {
            var task = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(5 * 1000);

                    var dicCount = 0;

                    lock (_lockObj)
                    {
                        dicCount = _dic.Count;
                    }

                    if (dicCount > 0)
                    {
                        var rcfg = new RemoteConfigSectionCollection();
                        rcfg.Application = Helper.GetAppName();
                        rcfg.Machine = Environment.MachineName;
                        rcfg.Environment = Helper.GetEnvironment();
                        rcfg.Sections = new RemoteConfigSection[dicCount];

                        lock (_lockObj)
                        {
                            var i = 0;

                            foreach (var o in _dic)
                            {
                                var rcs = new RemoteConfigSection();
                                rcs.SectionName = o.Key;
                                rcs.MajorVersion = o.Value.Major;
                                rcs.MinorVersion = o.Value.Minor;
                                rcfg.Sections[i] = rcs;
                                i++;
                            }
                        }

                        var rcfgResult = Helper.GetServerVersions(rcfg);

                        if (rcfgResult.Sections == null || rcfgResult.Sections.Length == 0)
                        {
                            Console.WriteLine("...no change");
                        }
                        else
                        {
                            Console.WriteLine("...has change");
                            var cfgFolder = Helper.GetAppCfgFolder();
                            foreach (var v in rcfgResult.Sections)
                            {
                                var cfgPath = cfgFolder + "/" + _dic[v.SectionName].Name + ".config";
                                var sucess = Helper.DownloadRemoteCfg(v.SectionName, v.DownloadUrl, cfgPath);
                                if (sucess)
                                {
                                    var xmlStr = File.ReadAllText(cfgPath, Encoding.UTF8);
                                    var xmlConfig = Helper.DeserializeFromXml(xmlStr, _dic[v.SectionName].GetType());
                                    if (xmlConfig != null)
                                    {
                                        AddXmlConfig(_dic[v.SectionName].Name, xmlConfig);
                                    }
                                }
                            }
                        }
                    }
                }
            });
            task.Start();
        }

        internal static void AddXmlConfig(string name, XmlConfig xmlConfig)
        {
            lock (_lockObj)
            {
                xmlConfig.Name = name;
                _dic[name.ToLower()] = xmlConfig;
            }
        }

        internal static XmlConfig GetXmlConfig(string name)
        {
            lock (_lockObj)
            {
                name = name.ToLower();
                if (_dic.ContainsKey(name))
                {
                    return _dic[name];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
