using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        public static void SerializeToXml<T>(string filePath, T obj)
        {
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath)) { System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T)); xs.Serialize(writer, obj); }
            }
            catch (Exception ex) { }
        }
        /// <summary>     
        /// 从某一XML文件反序列化到某一类型   
        /// </summary>    
        /// <param name="filePath">待反序列化的XML文件名称</param>  
        /// <param name="type">反序列化出的</param>  
        /// <returns></returns>    
        public static T DeserializeFromXml<T>(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    throw new ArgumentNullException(filePath + " not Exists");
                using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
                    T ret = (T)xs.Deserialize(reader);
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
    }
}
}
