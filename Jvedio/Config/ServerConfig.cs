using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using static Jvedio.StaticVariable;

namespace Jvedio
{
    public class ServerConfig
    {

        public WebSite WebSite = WebSite.None;

        public string  filepath = "ServersConfig";

        public ServerConfig(string website)
        {
            Enum.TryParse<WebSite>(website, out WebSite);
        }

        public bool InitXML()
        {
            try
            {
                if (WebSite == WebSite.None) return false;
                XmlDocument XmlDoc = new XmlDocument();
                string Root = "Servers";
                bool CreateRoot = false;
                if (File.Exists(filepath))
                {
                    try { XmlDoc.Load(filepath); }
                    catch { CreateRoot = true; }
                }
                else
                {
                    CreateRoot = true;
                }


                if (CreateRoot)
                {
                    try
                    {
                        XmlNode header = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                        XmlDoc.AppendChild(header);
                    }
                    catch { }

                    //生成根节点
                    var xm = XmlDoc.CreateElement(Root);
                    XmlDoc.AppendChild(xm);
                }
                XmlElement rootElement = XmlDoc.DocumentElement;
                XmlNode node = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']");
                if (node == null)
                {
                    //不存在该节点
                    XmlElement XE = XmlDoc.CreateElement("Server");
                    XE.SetAttribute("Name", WebSite.ToString());
                    XmlElement x1 = XmlDoc.CreateElement("Url");
                    x1.InnerText = "";
                    XmlElement x2 = XmlDoc.CreateElement("ServerName");
                    x2.InnerText = "";
                    XmlElement x3 = XmlDoc.CreateElement("LastRefreshDate");
                    x3.InnerText = "";

                    XE.AppendChild(x1);
                    XE.AppendChild(x2);
                    XE.AppendChild(x3);
                    rootElement.AppendChild(XE);
                }
                else
                {
                    XmlNode x1 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/Url");
                    XmlNode x2 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/ServerName");
                    XmlNode x3 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/LastRefreshDate");

                    if (x1 == null)
                    {
                        XmlElement xe1 = XmlDoc.CreateElement("Url");
                        xe1.InnerText = "";
                        node.AppendChild(xe1);
                    }

                    if (x2 == null)
                    {
                        XmlElement xe2 = XmlDoc.CreateElement("ServerName");
                        xe2.InnerText = "";
                        node.AppendChild(xe2);
                    }

                    if (x3 == null)
                    {
                        XmlElement xe3 = XmlDoc.CreateElement("LastRefreshDate");
                        xe3.InnerText = "";
                        node.AppendChild(xe3);
                    }
                }
                XmlDoc.Save(filepath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Save(Dictionary<string, string> Infos)
        {
            if (WebSite == WebSite.None) return;
            
            InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);
            XmlNode x1 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/Url");
            XmlNode x2 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/ServerName");
            XmlNode x3 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/LastRefreshDate");
            if (x1 != null) x1.InnerText = Infos["Url"];
            if (x2 != null) x2.InnerText = Infos["ServerName"];
            if (x3 != null) x3.InnerText = Infos["LastRefreshDate"];

            XmlDoc.Save(filepath);
        }

       


        public List<string>  Read()
        {
            List<string> result = new List<string>();

            if (!File.Exists(filepath)) InitXML();
            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);

            XmlNode x1 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/Url");
            XmlNode x2 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/ServerName");
            XmlNode x3 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']/LastRefreshDate");

            if (x1 != null) result.Add( x1.InnerText) ;
            if (x2 != null)  result.Add(x2.InnerText);
            if (x3 != null)  result.Add( x3.InnerText);
            return result;
        }


        public bool Delete()
        {

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(filepath);
            XmlNode x1 = XmlDoc.SelectSingleNode($"/Servers/Server[@Name='{WebSite.ToString()}']");
            XmlElement root = XmlDoc.DocumentElement;
            if (x1 != null) root.RemoveChild(x1);
            XmlDoc.Save(filepath);
            return false;
        }

    }

}
