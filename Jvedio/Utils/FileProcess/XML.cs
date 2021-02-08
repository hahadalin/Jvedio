
using System;
using System.IO;
using System.Xml;

namespace Jvedio
{
    public class XML
    {
        private XmlDocument XmlDoc = new XmlDocument();
        public string FilePath = "";
        public string Root = "";
        public XML(string filepath="WindowConfig.xml", string root="Windows")
        {
            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,filepath);
            Root = root;
            if (!File.Exists(FilePath)) Init();
        }

        public virtual bool Init()
        {
            try
            {
                XmlNode header = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                XmlDoc.AppendChild(header);
                var xm = XmlDoc.CreateElement(Root);
                XmlDoc.AppendChild(xm);
                XmlDoc.Save(Root);
                return true;
            }
            catch { return false; }
        }

        public void CreateNewNode(string NodeName, string NodeText = "", string NodeID = "", string NodeIDValue = "")
        {
            var Root = XmlDoc.DocumentElement;
            XmlElement XE = null;
            XE = XmlDoc.CreateElement(NodeName);
            if (!string.IsNullOrEmpty(NodeID))
                XE.SetAttribute(NodeID, NodeIDValue);
            XE.InnerText = NodeText;
            Root.AppendChild(XE);
        }

        public void AppendNewNode(string NodeName, string NodeText = "", string NodeID = "", string NodeIDValue = "")
        {
            try
            {
                XmlDoc.Load(FilePath);
                var Root = XmlDoc.DocumentElement;
                XmlElement XE = null;
                XE = XmlDoc.CreateElement(NodeName);
                if (!string.IsNullOrEmpty(NodeID))
                    XE.SetAttribute(NodeID, NodeIDValue);
                XE.InnerText = NodeText;
                Root.AppendChild(XE);
                XmlDoc.Save(FilePath);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }

        public void AppendNodeToNode(string FatherNode, string NodeName, string NodeText = "", string NodeID = "", string NodeIDValue = "")
        {
            if (File.Exists(FilePath) && !string.IsNullOrEmpty(NodeName) && !string.IsNullOrEmpty(FatherNode))
            {
                XmlDoc.Load(FilePath);
                var XE = XmlDoc.CreateElement(NodeName);
                if (!string.IsNullOrEmpty(NodeID))
                    XE.SetAttribute(NodeID, NodeIDValue);
                XE.InnerText = NodeText;
                var FatherList = XmlDoc.GetElementsByTagName(FatherNode);
                var Father = FatherList[FatherList.Count - 1];
                Father.AppendChild(XE);
                XmlDoc.Save(FilePath);
            }
        }

        public string ReadNodeFromXML(string NodeName)
        {
            if (File.Exists(FilePath) && !string.IsNullOrEmpty(NodeName))
            {
                XmlDoc.Load(FilePath);
                var XN = XmlDoc.GetElementsByTagName(NodeName)[0];
                if (XN is object)
                {
                    return XN.InnerText;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public void SetNodeText(string NodeName, string NodeText)
        {
            if (File.Exists(FilePath) && !string.IsNullOrEmpty(NodeName))
            {
                XmlDoc.Load(FilePath);
                var XN = XmlDoc.GetElementsByTagName(NodeName)[0];
                if (XN is object)
                {
                    XN.InnerText = NodeText;
                    XmlDoc.Save(FilePath);
                }
            }
        }
    }
}