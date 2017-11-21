using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xcfg
{
    public static class Helper
    {
        public const string Product = "prod";
        public const string Dev = "dev";
        public const string Testing = "testing";
        public const string Labs = "labs";

        public static string GetCfgFolder()
        {
            return "c:/xcfg.configs";
        }

        public static string GetAppName()
        {
            string appName = ConfigurationManager.AppSettings["appname"]??"";
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
            string environment = ConfigurationManager.AppSettings["environment"]??"";
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
            return GetRemoteCfgShortUrl() +"/ConfigVersionHandler.ashx";
        }

        public static T DeserializeFromXml<T>(string xmlStr)
        {
            var bytes = Encoding.UTF8.GetBytes(xmlStr);
            try
            {
                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                T ret = (T)xs.Deserialize(new MemoryStream(bytes));
                return ret;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static string GetRemoteCfgShortUrl()
        {
            var host = ConfigurationManager.AppSettings["remote_cfg_host"] ?? "";
            var port = ConfigurationManager.AppSettings["remote_cfg_port"] ?? "";
            return $"http://{host}:{port}";
        }

        public static string HttpPost(string Url, string postDataStr)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "text/xml";
                //request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
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

            }
            return null;
        }

        public static string HttpGet(string url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
    }

}
