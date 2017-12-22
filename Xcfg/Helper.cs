using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
#endif
namespace Xcfg
{
    public static class Helper
    {
        public const string Product = "prod";
        public const string Dev = "dev";
        public const string Testing = "testing";
        public const string Labs = "labs";

#if NETSTANDARD2_0
        public static IConfigurationRoot JsonConfigurationManager
        {
            get
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                return builder.Build();
            }
        }
#endif

        public static string GetCfgFolder()
        {
            var dd = System.Environment.OSVersion;

            if (dd.VersionString.Contains("Windows"))
            {
                return "c:/xcfg.configs";
            }
            else
            {
                return "/usr/local/etc/xcfg.configs";
            }
        }

        public static string GetAppName()
        {
            string appName = ConfigurationManager.AppSettings["appname"] ?? "";
#if NETSTANDARD2_0
            appName = JsonConfigurationManager["appname"] ?? "";
#endif
            if (string.IsNullOrWhiteSpace(appName))
            {
                string filePath = System.AppDomain.CurrentDomain.BaseDirectory;
                appName = filePath.Replace("\\", "_");
                appName = appName.Replace(":", "_");
            }
            return appName;
        }

        public static string GetEnvironment()
        {
            string environment = ConfigurationManager.AppSettings["environment"] ?? "";
#if NETSTANDARD2_0
            environment = JsonConfigurationManager["environment"] ?? "";
#endif
            switch (environment.ToLower())
            {
                case Product:
                    return Product;
                case Labs:
                    return Labs;
                case Testing:
                    return Testing;
                case Dev:
                    return Dev;
                default:
                    return Dev;
            }
        }

        public static string GetAppCfgFolder()
        {
            return GetCfgFolder() + "/" + GetAppName() + "/" + GetEnvironment();
        }


        /// <summary>     
        /// XML序列化某一类型到指定的文件   
        /// /// </summary>   
        /// /// <param name="filePath"></param>   
        /// /// <param name="obj"></param>  
        /// /// <param name="type"></param>   
        public static byte[] SerializeToXml<T>(T obj)
        {
            byte[] bytes = null;
            try
            {
                using (System.IO.MemoryStream writer = new System.IO.MemoryStream())
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                    xs.Serialize(writer, obj);
                    bytes = writer.ToArray();
                }
            }
            catch (Exception ex) { }
            return bytes;
        }

        public static string GetRemoteCfgUrl()
        {
            return GetRemoteCfgShortUrl() + "/ConfigVersionHandler.ashx";
        }

        public static T DeserializeFromXml<T>(string xmlStr)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(xmlStr);
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                T ret = (T)xs.Deserialize(new MemoryStream(bytes));
                return ret;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static XmlConfig DeserializeFromXml(string xmlStr, Type type)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(xmlStr);
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(type);
                var ret = (XmlConfig)xs.Deserialize(new MemoryStream(bytes));
                return ret;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetRemoteCfgShortUrl()
        {
            var host = ConfigurationManager.AppSettings["remote_cfg_host"] ?? "";
            var port = ConfigurationManager.AppSettings["remote_cfg_port"] ?? "";
#if NETSTANDARD2_0
            host = JsonConfigurationManager["remote_cfg_host"] ?? "";
            port = JsonConfigurationManager["remote_cfg_port"] ?? "";
#endif
            return $"http://{host}:{port}";
        }

        public static string HttpPost(string url, string postDataStr)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.Timeout = 2000;
                Stream myRequestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.UTF8);
                myStreamWriter.Write(postDataStr);
                myStreamWriter.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                return retString;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string HttpGet(string url, string postDataStr)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (postDataStr == "" ? "" : "?") + postDataStr);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.Timeout = 2000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();

                return retString;
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        internal static RemoteConfigSection GetRemoteConfigSectionParam(string cfgName)
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

        internal static RemoteConfigSectionCollection GetServerVersions(RemoteConfigSectionCollection rcfg)
        {
            var bytes = Helper.SerializeToXml(rcfg);
            var url = Helper.GetRemoteCfgUrl();
            var xmlStr = Helper.HttpPost(url, Encoding.UTF8.GetString(bytes));
            return Helper.DeserializeFromXml<RemoteConfigSectionCollection>(xmlStr);
        }

        internal static bool DownloadRemoteCfg(string sectionName, string url, string targetPath)
        {
            try
            {
                if (!url.StartsWith("http"))
                {
                    url = Helper.GetRemoteCfgShortUrl() + "/" + url;
                }

                var data = Helper.HttpGet(url, "");

                data = FormatXml(data);

                var tempFile = targetPath + "." + Guid.NewGuid();

                File.WriteAllText(tempFile, data);

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                FileInfo file = new FileInfo(tempFile);
                file.MoveTo(targetPath);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal static string FormatXml(string xmlStr)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xmlStr);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 1,
                    IndentChar = '\t'
                };
                xd.WriteTo(xtw);
            }
            finally
            {
                xtw?.Close();
            }
            return sb.ToString();
        }

    }

}
