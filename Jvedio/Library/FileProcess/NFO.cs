
using System;
using System.IO;
using System.Xml;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// NFO
    /// </summary>
    public class NFO
    {
        //NFO 标准：https://kodi.wiki/view/NFO_files/Movies

        private XmlDocument XmlDoc = new XmlDocument();
        private string FilePath = "";

        public NFO(string FP, string RootNodeName)
        {
            FilePath = FP;
            try
            {
                XmlNode header = XmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                XmlDoc.AppendChild(header);
                var xm = XmlDoc.CreateElement(RootNodeName);
                XmlDoc.AppendChild(xm);
                CreateNewNode("source");
                CreateNewNode("plot");
                CreateNewNode("title");
                CreateNewNode("director");
                CreateNewNode("rating");
                CreateNewNode("criticrating");
                CreateNewNode("year");
                CreateNewNode("mpaa");
                CreateNewNode("customrating");
                CreateNewNode("countrycode");
                CreateNewNode("premiered");
                CreateNewNode("release");
                CreateNewNode("runtime");
                CreateNewNode("country");
                CreateNewNode("studio");
                CreateNewNode("id");
                CreateNewNode("num");
                XmlDoc.Save(FP);
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }

        private void CreateNewNode(string NodeName, string NodeText = "", string NodeID = "", string NodeIDValue = "")
        {
            var Root = XmlDoc.DocumentElement;
            XmlElement XE = XmlDoc.CreateElement(NodeName);
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

    public static class nfo
    {
        /// <summary>
        /// 保存信息到 NFO 文件
        /// </summary>
        /// <param name="vedio"></param>
        /// <param name="NfoPath"></param>
        public static void SaveToNFO(DetailMovie vedio, string NfoPath)
        {
            var nfo = new NFO(NfoPath, "movie");
            nfo.SetNodeText("source", vedio.sourceurl);
            nfo.SetNodeText("title", vedio.title);
            nfo.SetNodeText("director", vedio.director);
            nfo.SetNodeText("rating", vedio.rating.ToString());
            nfo.SetNodeText("year", vedio.year.ToString());
            nfo.SetNodeText("countrycode", vedio.countrycode.ToString());
            nfo.SetNodeText("release", vedio.releasedate);
            nfo.SetNodeText("runtime", vedio.runtime.ToString());
            nfo.SetNodeText("country", vedio.country);
            nfo.SetNodeText("studio", vedio.studio);
            nfo.SetNodeText("id", vedio.id);
            nfo.SetNodeText("num", vedio.id);

            // 类别
            foreach (var item in vedio.genre?.Split(' '))
            {
                if (!string.IsNullOrEmpty(item)) nfo.AppendNewNode("genre", item);
            }
            // 系列
            foreach (var item in vedio.tag?.Split(' '))
            {
                if (!string.IsNullOrEmpty(item)) nfo.AppendNewNode("tag", item);
            }

            // Fanart
            nfo.AppendNewNode("fanart");
            foreach (var item in vedio.extraimageurl?.Split(';'))
            {
                if (!string.IsNullOrEmpty(item)) nfo.AppendNodeToNode("fanart", "thumb", item, "preview", item);
            }

            // 演员
            if (vedio.vediotype == (int)VedioType.欧美)
            {
                foreach (var item in vedio.actor?.Split('/'))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        nfo.AppendNewNode("actor");
                        nfo.AppendNodeToNode("actor", "name", item);
                        nfo.AppendNodeToNode("actor", "type", "Actor");
                    }
                }
            }
            else
            {
                foreach (var item in vedio.actor?.Split(actorSplitDict[vedio.vediotype]))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        nfo.AppendNewNode("actor");
                        nfo.AppendNodeToNode("actor", "name", item);
                        nfo.AppendNodeToNode("actor", "type", "Actor");
                    }
                }
            }

        }
    }






}