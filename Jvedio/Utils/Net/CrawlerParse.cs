
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;

namespace Jvedio
{

    public class InfoParse
    {
        protected string HtmlText { get; set; }
        public string ID { get; set; }
        protected VedioType VedioType { get; set; }

        public InfoParse(string htmlText, string id = "", VedioType vedioType = VedioType.步兵)
        {
            ID = id;
            HtmlText = htmlText;
            VedioType = vedioType;
        }


        public virtual Dictionary<string, string> Parse()
        {
            return new Dictionary<string, string>();
        }

    }


    public class BusParse : InfoParse
    {
        public BusParse(string id, string htmlText, VedioType vedioType) : base(htmlText, id, vedioType) { }


        public override Dictionary<string, string> Parse()
        {
            if (string.IsNullOrEmpty(HtmlText)) return base.Parse();
            Dictionary<string, string> result = new Dictionary<string, string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);
            //基本信息
            HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//span[@class='header']");
            if (headerNodes != null)
            {
                foreach (HtmlNode headerNode in headerNodes)
                {
                    if (headerNode == null) continue;
                    string headerText = headerNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNode linkNode = null;
                    switch (headerText)
                    {
                        case "發行日期:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("releasedate", Regex.Match(content, "[0-9]{4}-[0-9]{2}-[0-9]{2}").Value);
                            result.Add("year", Regex.Match(content, "[0-9]{4}").Value);
                            break;
                        case "長度:":
                            node = headerNode.ParentNode; if (node == null) break;
                            content = node.InnerText;
                            result.Add("runtime", Regex.Match(content, "[0-9]+").Value);
                            break;
                        case "製作商:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("studio", content);
                            break;
                        case "系列:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("tag", content);
                            break;
                        case "導演:":
                            node = headerNode.ParentNode; if (node == null) break;
                            linkNode = node.SelectSingleNode("a"); if (linkNode == null) break;
                            content = linkNode.InnerText;
                            result.Add("director", content);
                            break;
                        default:
                            break;
                    }
                }
            }

            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h3");
            if (titleNodes != null && titleNodes.Count > 0)
            {
                if (VedioType == VedioType.欧美)
                    result.Add("title", titleNodes[0].InnerText.Replace(ID, ""));
                else
                {
                    string title = titleNodes[0].InnerText.ToUpper().Replace(ID.ToUpper(), "");
                    if(title.StartsWith(" "))
                        result.Add("title", title.Substring(1));
                    else
                        result.Add("title", title);
                }
                    
            }


            //类别、演员
            List<string> genres = new List<string>();
            List<string> actors = new List<string>();
            List<string> actorsid = new List<string>();
            HtmlNodeCollection genreNodes = doc.DocumentNode.SelectNodes("//span[@class='genre']/a");
            if (genreNodes != null)
            {
                HtmlNode node = null;

                foreach (HtmlNode genreNode in genreNodes)
                {
                    if (genreNode == null) continue;
                    node = genreNode.ParentNode; if (node == null) continue;

                    if (node.Attributes["onmouseover"] != null)
                    {
                        actors.Add(genreNode.InnerText);//演员
                        string link = genreNode.Attributes["href"]?.Value;
                        if (!string.IsNullOrEmpty(link) && link.IndexOf("/") >= 0)
                            actorsid.Add(link.Split('/').Last());
                    }
                    else
                    {
                        genres.Add(genreNode.InnerText);//类别
                    }
                }
                result.Add("genre", string.Join(" ", genres));
            }
            if (actors.Count > 0 && actorsid.Count > 0)
            {
                result.Add("actor", string.Join("/", actors));
                result.Add("actorid", string.Join("/", actorsid));
                List<string> url_a = new List<string>();//演员头像地址
                foreach (var item in actorsid)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    if (VedioType == VedioType.骑兵)
                        url_a.Add($"https://pics.javcdn.net/actress/" + item + "_a.jpg");
                    else if (VedioType == VedioType.欧美)
                        url_a.Add(RootUrl.BusEu.Replace("www", "images") + "actress/" + item + "_a.jpg");//https://images.javbus.one/actress/41r_a.jpg
                    else if (VedioType == VedioType.步兵)
                        url_a.Add($"https://images.javcdn.net/actress/" + item + ".jpg");//步兵没有 _a
                }
                result.Add("actressimageurl", string.Join(";", url_a));
            }

            //大图
            string movieid = ""; string bigimageurl = "";
            HtmlNodeCollection bigimgeNodes = doc.DocumentNode.SelectNodes("//a[@class='bigImage']");
            if (bigimgeNodes != null && bigimgeNodes.Count > 0)
            {
                bigimageurl = bigimgeNodes[0].Attributes["href"]?.Value;
                if (!string.IsNullOrEmpty(bigimageurl))
                {
                    result.Add("bigimageurl", bigimageurl);
                    movieid = System.IO.Path.GetFileNameWithoutExtension(new Uri(bigimageurl).LocalPath).Replace("_b", "");
                }


            }

            //小图
            if (!string.IsNullOrEmpty(bigimageurl))
            {
                if (bigimageurl.IndexOf("pics.dmm.co.jp") >= 0)
                    result.Add("smallimageurl", bigimageurl.Replace("pl.jpg", "ps.jpg"));
                else if (!string.IsNullOrEmpty(movieid))
                {
                    if (VedioType == VedioType.骑兵)
                        result.Add("smallimageurl", "https://pics.javcdn.net/thumb/" + movieid + ".jpg");
                    else if (VedioType == VedioType.步兵)
                        result.Add("smallimageurl", "https://images.javcdn.net/thumbs/" + movieid + ".jpg");
                    else if (VedioType == VedioType.欧美)
                        result.Add("smallimageurl", "https://images.javbus.one/thumb/" + movieid + ".jpg");//https://images.javbus.one/thumb/10jc.jpg
                }
            }

            //预览图
            List<string> url_e = new List<string>();
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//a[@class='sample-box']");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    url_e.Add(extrapicNode.Attributes["href"].Value);
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }
            return result;
        }


        public Actress ParseActress()
        {
            if (string.IsNullOrEmpty(HtmlText)) return null;
            Actress result = new Actress();
           
            string info;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //基本信息
            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//div[@class='photo-info']/p");
            if (infoNodes != null)
            {
                foreach (HtmlNode infoNode in infoNodes)
                {
                    try
                    {
                        info = infoNode.InnerText;
                        if (info.IndexOf("生日") >= 0)
                        {
                            result.birthday = info.Replace("生日: ", "");
                        }
                        else if (info.IndexOf("年齡") >= 0)
                        {
                            int.TryParse(info.Replace("年齡: ", ""), out int age);
                            result.age = age;
                        }
                        else if (info.IndexOf("身高") >= 0)
                        {
                            int h = 0;
                            if (Regex.Match(info, @"[0-9]+") != null)
                                int.TryParse(Regex.Match(info, @"[0-9]+").Value, out h);
                            result.height = h;
                        }
                        else if (info.IndexOf("罩杯") >= 0)
                        {
                            result.cup = info.Replace("罩杯: ", "");
                        }
                        else if (info.IndexOf("胸圍") >= 0)
                        {
                            result.chest = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("腰圍") >= 0)
                        {
                            result.waist = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("臀圍") >= 0)
                        {
                            result.hipline = int.Parse(Regex.Match(info, @"[0-9]+").Value);
                        }
                        else if (info.IndexOf("愛好") >= 0)
                        {
                            result.hobby = info.Replace("愛好: ", "");
                        }
                        else if (info.IndexOf("出生地") >= 0)
                        {
                            result.birthplace = info.Replace("出生地: ", "");
                        }
                    }
                    catch { continue; }
                }
            }
            return result;
        }


    }

    public class LibraryParse : InfoParse
    {
        public LibraryParse(string id, string htmlText, VedioType vedioType = 0) : base(htmlText, id, vedioType) { }


        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") return result;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);
            string id = "";
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h3[@class='post-title text']/a");
            if (titleNode != null)
            {
                id = titleNode.InnerText.Split(' ')[0].ToUpper();
                result.Add("title", titleNode.InnerText.ToUpper().Replace(id, "").Substring(1));
            }

            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//div[@id='video_info']/div/table/tr");
            if (infoNodes != null)
            {
                foreach (HtmlNode infoNode in infoNodes)
                {
                    if (infoNode == null) continue;
                    string header = infoNode.InnerText;
                    string content = "";
                    HtmlNode node = null;
                    HtmlNodeCollection nodes = null;
                    if (header.IndexOf("发行日期") >= 0)
                    {
                        nodes = infoNode.SelectNodes("td"); if (nodes == null || nodes.Count == 0) continue;
                        content = nodes[1].InnerText;
                        result.Add("releasedate", content);
                    }
                    else if (header.IndexOf("长度") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/span"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("runtime", content);
                    }
                    else if (header.IndexOf("导演") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/span/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("director", content);
                    }
                    else if (header.IndexOf("发行商") >= 0)
                    {
                        node = infoNode.SelectSingleNode("td/span/a"); if (node == null) continue;
                        content = node.InnerText;
                        result.Add("studio", content);
                    }

                    else if (header.IndexOf("使用者评价:") >= 0)
                    {
                        node = doc.DocumentNode.SelectSingleNode("//span[@class='score']");
                        if (node == null) continue;
                        content = node.InnerText;
                        Match match = Regex.Match(content, @"([0-9]|\.)+");
                        if (match == null) continue;
                        double.TryParse(match.Value, out double rating);
                        result.Add("rating", Math.Ceiling(rating * 10).ToString());
                    }
                    else if (header.IndexOf("类别") >= 0)
                    {
                        HtmlNodeCollection genreNodes = infoNode.SelectNodes("td/span/a");
                        if (genreNodes != null)
                        {
                            List<string> genres = new List<string>();
                            foreach (HtmlNode genreNode in genreNodes)
                            {
                                genres.Add(genreNode.InnerText);
                            }
                            result.Add("genre", string.Join(" ",genres));
                        }

                    }
                    else if (header.IndexOf("演员") >= 0)
                    {
                        HtmlNodeCollection actressNodes = infoNode.SelectNodes("td/span/span/a");
                        if (actressNodes != null)
                        {
                            List<string> actress = new List<string>();
                            foreach (HtmlNode actressNode in actressNodes)
                            {
                                actress.Add( actressNode.InnerText );
                            }
                            result.Add("actor", string.Join("/",actress));
                        }

                    }
                }
            }

            // library 小图地址与大图地址无规律
            HtmlNode bigimageNode = doc.DocumentNode.SelectSingleNode("//img[@id='video_jacket_img']");
            if (bigimageNode != null) {
                result.Add("bigimageurl", "http:" + bigimageNode.Attributes["src"].Value); 
                result.Add("smallimageurl", result["bigimageurl"].Replace("pl.jpg", "ps.jpg")); //如果地址来自于dmm则替换，否则大小图一致
            }



            //预览图
            //标准预览图是：https://pics.dmm.co.jp/digital/video/1star00319/1star00319jp-17.jpg
            //小的预览图时：https://pics.dmm.co.jp/digital/video/1star00319/1star00319-17.jpg
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//div[@class='previewthumbs']/img");
            if (extrapicNodes != null)
            {
                List<string> extraImage = new List<string>();
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    string link = "https:" + extrapicNode.Attributes["src"].Value;
                    if (link.IsProperUrl())
                    {
                        string name = System.IO.Path.GetFileName(new Uri(link).LocalPath);
                        string path = link.Replace(name, "");
                        string[] names = name.Split('-');
                        if (names.Length>=2  )
                        {
                            if(names.First().EndsWith("jp"))
                                extraImage.Add(link);
                            else
                            {
                                names[0] = names[0] + "jp";
                                extraImage.Add(path + string.Join("-", names));
                            }
                                
                        }
                    }
                    
                }
                result.Add("extraimageurl", string.Join(";",extraImage));
            }

            return result;
        }

    }

    public class JavDBParse : InfoParse
    {
        protected string MovieCode { get; set; }

        public JavDBParse(string id, string htmlText, string movieCode) : base(htmlText)
        {
            ID = id;
            HtmlText = htmlText;
            MovieCode = movieCode;
        }





        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h2[@class='title is-4']/strong");
            if (titleNode != null)
            {
                result.Add("title", titleNode.InnerText.Replace(ID, " ").Substring(1));
            }

            HtmlNodeCollection infoNodes = doc.DocumentNode.SelectNodes("//nav[@class='panel video-panel-info']/div");
            if (infoNodes != null)
            {

                foreach (HtmlNode infoNode in infoNodes)
                {
                    HtmlNode node = null;
                    string content = "";
                    if (infoNode == null) continue;
                    string headerText = infoNode.InnerText;
                    if (headerText.IndexOf("時間") >= 0 || headerText.IndexOf("日期") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("releasedate", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("時長") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A")
                        {
                            Match match = Regex.Match(content, "[0-9]+");
                            if (match != null) result.Add("runtime", match.Value);
                        }
                    }
                    else if (infoNode.InnerText.IndexOf("賣家") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span/a"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("director", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("評分") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A")
                        {
                            Match match = Regex.Match(content, @"([0-9]|\.)+分");
                            if (match != null)
                            {
                                string rating = match.Value.Replace("分", "");
                                double.TryParse(rating, out double rate);
                                result.Add("rating", Math.Ceiling(rate * 20).ToString());
                            }
                        }
                    }
                    else if (infoNode.InnerText.IndexOf("類別") >= 0)
                    {
                        HtmlNodeCollection genreNodes = infoNode.SelectNodes("span/a");
                        if (genreNodes != null && genreNodes.Count > 0)
                        {
                            List<string> genres = new List<string>();
                            foreach (HtmlNode genreNode in genreNodes)
                            {
                                if (genreNode != null)
                                    genres.Add(genreNode.InnerText);
                            }
                            result.Add("genre", string.Join(" ", genres));
                        }

                    }
                    else if (infoNode.InnerText.IndexOf("片商") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span/a"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("studio", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("系列") >= 0)
                    {
                        node = infoNode.SelectSingleNode("span/a"); if (node == null) continue;
                        content = node.InnerText;
                        if (content != "N/A") { result.Add("tag", content); }
                    }
                    else if (infoNode.InnerText.IndexOf("演員") >= 0)
                    {
                        HtmlNodeCollection actressNodes = infoNode.SelectNodes("span/a");
                        if (actressNodes != null)
                        {
                            List<string> actress = new List<string>();
                            foreach (HtmlNode actressNode in actressNodes)
                            {
                                if (actressNode != null)
                                    actress.Add(actressNode.InnerText);
                            }
                            result.Add("actor", string.Join("/", actress));
                        }
                    }
                }

            }
            //大小图
            HtmlNode bigimageNode = doc.DocumentNode.SelectSingleNode("//img[@class='video-cover']");
            if (bigimageNode != null) { result.Add("bigimageurl", bigimageNode.Attributes["src"].Value); }

            string smallimageurl = "https://jdbimgs.com/thumbs/" + MovieCode.ToLower().Substring(0, 2) + "/" + MovieCode + ".jpg";
            result.Add("smallimageurl", smallimageurl);

            //预览图
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//a[@class='tile-item']");
            if (extrapicNodes != null)
            {
                List<string> extraimage = new List<string>();
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    string link = "";
                    link = extrapicNode.Attributes["href"]?.Value;
                    if (!string.IsNullOrEmpty(link) && link.IndexOf("/v/") < 0)
                        extraimage.Add(link);
                }
                result.Add("extraimageurl", string.Join(";", extraimage));
            }

            return result;
        }





    }

    public class Fc2ClubParse : InfoParse
    {
        public Fc2ClubParse(string id, string htmlText, VedioType vedioType = 0) : base(htmlText, id, vedioType) { }


        public override Dictionary<string, string> Parse()
        {

            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            string content; string title; 
            //string id = "";

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            //基本信息
            HtmlNodeCollection headerNodes = doc.DocumentNode.SelectNodes("//h5");
            if (headerNodes != null)
            {
                foreach (HtmlNode headerNode in headerNodes)
                {
                    try
                    {
                        title = headerNode.InnerText;
                        //Console.WriteLine(title);
                        if (title.IndexOf("影片评分") >= 0)
                        {
                            content = title;
                            result.Add("rating", Regex.Match(content, "[0-9]+").Value);
                        }
                        else if (title.IndexOf("资源参数") >= 0)
                        {
                            content = title;
                            if (content.IndexOf("无码") > 0) { result.Add("vediotype", "1"); }
                            else { result.Add("vediotype", "2"); }
                        }
                        else if (title.IndexOf("卖家信息") >= 0)
                        {
                            content = headerNode.SelectSingleNode("a").InnerText;
                            result.Add("director", content.Replace("\n", "").Replace("\r", ""));
                            result.Add("studio", content.Replace("\n", "").Replace("\r", ""));
                        }
                        else if (title.IndexOf("影片标签") >= 0)
                        {
                            HtmlNodeCollection genreNodes = headerNode.SelectNodes("a");
                            if (genreNodes != null)
                            {
                                string genre = "";
                                foreach (HtmlNode genreNode in genreNodes)
                                {
                                    genre = genre + genreNode.InnerText + " ";
                                }
                                if (genre.Length > 0) { result.Add("genre", genre.Substring(0, genre.Length - 1)); }
                            }

                        }
                        else if (title.IndexOf("女优名字") >= 0)
                        {
                            content = title;
                            result.Add("actor", content.Replace("女优名字：", "").Replace("\n", "").Replace("\r", "").Replace("/", " "));
                        }
                    }
                    catch { continue; }
                }
            }

            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//h3");
            if (titleNodes != null)
            {
                foreach (HtmlNode titleNode in titleNodes)
                {
                    try
                    {
                        if (titleNode.InnerText.IndexOf("FC2优质资源推荐") < 0)
                        {
                            //id = titleNode.InnerText.Split(' ')[0];
                            //result.Add("id", id);
                            result.Add("title", titleNode.InnerText.Replace(ID, "").Substring(1).Replace("\n", "").Replace("\r", ""));
                            break;
                        }
                    }
                    catch { continue; }
                }

            }

            //预览图
            string url_e = "";
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//ul[@class='slides']/li/img");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    try
                    {
                        url_e = url_e + "https://fc2club.com" + extrapicNode.Attributes["src"].Value + ";";
                    }
                    catch { continue; }
                }
                result.Add("extraimageurl", url_e);
            }

            //大图小图
            if (url_e.IndexOf(';') > 0)
            {
                result.Add("bigimageurl", url_e.Split(';')[0]);
                result.Add("smallimageurl", url_e.Split(';')[0]);
            }

            //发行日期和发行年份
            if (url_e.IndexOf(";") > 0)
            {
                // / uploadfile / 2018 / 1213 / 20181213104511782.jpg
                string url = url_e.Split(';')[0];
                string datestring = Regex.Match(url, "[0-9]{4}/[0-9]{4}").Value;

                result.Add("releasedate", datestring.Substring(0, 4) + "-" + datestring.Substring(5, 2) + "-" + datestring.Substring(7, 2));
                result.Add("year", datestring.Substring(0, 4));
            }

            return result;
        }

    }


    public class FC2Parse : InfoParse
    {
        public FC2Parse(string id, string htmlText) : base(htmlText, id) { }


        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") return result;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);


            //发行商、评分
            Dictionary<string, string> studio_rating = GetUrlInfo(ref doc);
            foreach (var item in studio_rating)
            {
                if (!result.ContainsKey(item.Key))
                    result.Add(item.Key, item.Value);
            }

            

            //长度
            HtmlNodeCollection runtimeNodes = doc.DocumentNode.SelectNodes("//div[@class='items_article_MainitemThumb']/span/p");
            if (runtimeNodes != null && runtimeNodes.Count > 0)
            {
                string runtime = runtimeNodes[0].InnerText;
                runtime = RuntimeToMinute(runtime);
                result.Add("runtime", runtime);
            }

            //日期
            HtmlNodeCollection dateNodes = doc.DocumentNode.SelectNodes("//div[@class='items_article_Releasedate']/p");
            if (dateNodes != null && dateNodes.Count > 0)
            {
                string date = dateNodes[0].InnerText.Replace("上架时间 : ", "").Replace("販売日 : ","").Replace("/","-");
                result.Add("releasedate", date);
                result.Add("year", Regex.Match(date, "[0-9]{4}").Value);
            }


            //标题
            HtmlNodeCollection titleNodes = doc.DocumentNode.SelectNodes("//section[@class='items_article_headerTitleInArea']/div[@class='items_article_headerInfo']/h3");
            if (titleNodes != null && titleNodes.Count > 0)
            {
                result.Add("title", titleNodes[0].InnerText);
            }


            //类别、演员
            List<string> genres = new List<string>();
            HtmlNodeCollection genreNodes = doc.DocumentNode.SelectNodes("//a[@class='tag tagTag']");
            if (genreNodes != null)
            {
                foreach (HtmlNode genreNode in genreNodes)
                {
                    if (genreNode != null) 
                        genres.Add(genreNode.InnerText);
                    
                }
                result.Add("genre", string.Join(" ", genres));
            }

            //大图
            string bigimageurl = "";
            HtmlNodeCollection bigimgeNodes = doc.DocumentNode.SelectNodes("//section[@class='items_article_headerTitleInArea']/div[@class='items_article_MainitemThumb']/span/img");
            if (bigimgeNodes != null && bigimgeNodes.Count > 0)
            {
                bigimageurl = bigimgeNodes[0].Attributes["src"]?.Value;
                if (!string.IsNullOrEmpty(bigimageurl))
                {
                    result.Add("bigimageurl", "https:"+bigimageurl.Replace("'","\""));//下载的时候双引号替换为单引号
                    result.Add("smallimageurl", "https:" + bigimageurl.Replace("'", "\""));
                }
            }


            //预览图
            List<string> url_e = new List<string>();
            HtmlNodeCollection extrapicNodes = doc.DocumentNode.SelectNodes("//ul[@class='items_article_SampleImagesArea']/li/a");
            if (extrapicNodes != null)
            {
                foreach (HtmlNode extrapicNode in extrapicNodes)
                {
                    if (extrapicNode == null) continue;
                    url_e.Add(extrapicNode.Attributes["href"].Value);
                }
                result.Add("extraimageurl", string.Join(";", url_e));
            }
            return result;
        }

        private Dictionary<string,string> GetUrlInfo(ref HtmlDocument doc)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string studio = "";
            double rating = 0;
            HtmlNode node = null;
            HtmlNode studioNode = null;
            HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div[@class='items_article_headerInfo']/ul/li");
            if(htmlNodes!=null && htmlNodes.Count > 0)
            {
                int num = htmlNodes.Count;
                switch (num)
                {
                    case 1:
                        node = htmlNodes[0].SelectSingleNode("//a");
                        if (node != null)
                            studio = node.InnerText;
                        break;

                    case 2:
                        node = htmlNodes[0].SelectSingleNode("//a[@class='items_article_Stars']");
                        if (node != null)
                        {
                            string spanClass = node.Attributes["class"].Value;
                            if (!string.IsNullOrEmpty(spanClass))
                            {
                                Match match = Regex.Match(spanClass, @"\d");
                                if (match != null)
                                    double.TryParse(match.Value, out rating);
                            }
                        }

                        studioNode  = htmlNodes[1].SelectSingleNode("//a");
                        if (studioNode != null)
                            studio = studioNode.InnerText;

                        break;

                    case 3:
                        node = htmlNodes[1].SelectSingleNode("//a/p/span[1]");
                        if (node != null)
                        {
                            string spanClass = node.Attributes["class"].Value;
                            if(!string.IsNullOrEmpty(spanClass))
                            {
                                Match match = Regex.Match(spanClass, @"\d");
                                if (match != null)
                                    double.TryParse(match.Value, out rating);
                            }
                        }
                         studioNode = htmlNodes[2].SelectSingleNode("a");
                        if (studioNode != null)
                            studio = studioNode.InnerText;

                        break;


                    default:
                        break;
                }
            }
            rating *= 20;
            result.Add("rating", rating.ToString());
            result.Add("studio", studio);
            return result;

        }


        private string RuntimeToMinute(string Runtime)
        {
            int result = 0;
            if (Runtime.IndexOf(":") > 0)
            {
                List<string> runtimes = Runtime.Split(':').ToList();
                if (runtimes.Count == 3)
                    result =int.Parse( runtimes[0]) * 60 + int.Parse(runtimes[1]);
                else if(runtimes.Count==2)
                {
                    if (runtimes[0] == "00")
                        result = 0;
                    else
                        result = int.Parse(runtimes[0]) ;
                }
            }
            return result.ToString();
        }

    }

    public class Jav321Parse : InfoParse
    {
        public Jav321Parse(string id, string htmlText, VedioType vedioType = 0) : base(htmlText, id, vedioType) { }


        public override Dictionary<string, string> Parse()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (HtmlText == "") { return result; }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HtmlText);

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//h3");
            if (titleNode != null)
            {
                result.Add("title", titleNode.InnerText);
            }

            HtmlNode infoNode = doc.DocumentNode.SelectSingleNode("//div[@class='col-md-9']");
            if (infoNode != null)
            {
                //强行正则
                string info = infoNode.InnerHtml;
                List<string> infos = info.Split(new string[] { "<br>" }, StringSplitOptions.None).ToList<string>();
                foreach (var item in infos)
                {
                    Match match = Regex.Match(item, @"<b>.*?</b>");
                    if (match != null)
                    {
                        string key = match.Value.ToUpper();
                        if (key.IndexOf("女优") > 0)
                        {

                        }
                    }
                }


            }


            //缩略图
            HtmlNode smallimageNode = doc.DocumentNode.SelectSingleNode("//div[@id='panel-body']/div/div/img");
            if (smallimageNode != null) { result.Add("smallimageurl", smallimageNode.Attributes["src"].Value); }

            //海报图
            HtmlNodeCollection imageNodes = doc.DocumentNode.SelectNodes("//div[@id='col-md-3']/div/p/a/img");
            if (imageNodes != null)
            {
                result.Add("bigimageurl", imageNodes[0].Attributes["src"].Value);
                string extraimage = "";
                for (int i = 1; i < imageNodes.Count; i++)
                {
                    HtmlNode htmlNode = imageNodes[i];
                    try { extraimage = extraimage + htmlNode.Attributes["src"].Value + ";"; }
                    catch { continue; }
                }
                result.Add("extraimageurl", extraimage);
            }


            return result;
        }

    }
}
