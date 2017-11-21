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
        private static Dictionary<string, XmlConfig> dic = new Dictionary<string, XmlConfig>();
        private static object lockObj = new object();
        static XmlConfigManager()
        {
            var task = new Task(() =>
              {
                  while (true)
                  {
                      Thread.Sleep(5 * 1000);

                      if (dic.Count > 0)
                      {
                          var rcfg = new RemoteConfigSectionCollection();

                          lock (lockObj)
                          {

                              rcfg.Application = Helper.GetAppName();

                              rcfg.Machine = Environment.MachineName;

                              rcfg.Environment = Helper.GetEnvironment();

                              rcfg.Sections = new RemoteConfigSection[dic.Count];
                              var i = 0;
                              foreach (var o in dic)
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
                                  var cfgPath = cfgFolder + "/" + dic[v.SectionName].Name + ".config";
                                  var sucess = Helper.DownloadRemoteCfg(v.SectionName, v.DownloadUrl, cfgPath);
                                  if (sucess)
                                  {
                                      var xmlStr = File.ReadAllText(cfgPath, Encoding.UTF8);
                                      var xmlConfig = Helper.DeserializeFromXml(xmlStr, dic[v.SectionName].GetType());
                                      if (xmlConfig != null)
                                      {
                                          AddXmlConfig(dic[v.SectionName].Name, xmlConfig);
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
            lock (lockObj)
            {
                xmlConfig.Name = name;
                dic[name.ToLower()] = xmlConfig;
            }
        }

        internal static XmlConfig GetXmlConfig(string name)
        {
            lock (lockObj)
            {
                name = name.ToLower();
                if (dic.ContainsKey(name))
                {
                    return dic[name];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
