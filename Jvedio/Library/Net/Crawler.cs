
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.Net;
using static Jvedio.GlobalVariable;
using System.IO;
namespace Jvedio
{
    public class Crawler
    {
        public static int TASKDELAY_SHORT = 300;//短时间暂停
        public static int TASKDELAY_MEDIUM = 1000;//长时间暂停
        public static int TASKDELAY_LONG = 2000;//长时间暂停


        protected bool result = false;
        protected string resultMessage = "";
        protected string Url = "";
        protected string Content = "";
        protected int StatusCode = 404;
        protected VedioType VedioType;
        protected string MovieCode;
        protected string Cookies = "";
        protected object LockDb;
        protected WebSite webSite;

        public string ID { get; set; }

        public Crawler(string Id)
        {
            ID = Id;
            LockDb = new object();
        }

        public void SaveInfo(Dictionary<string, string> Info, WebSite webSite)
        {
            if(!Info.ContainsKey("id")) Info.Add("id", ID);
            //保存信息
            lock (LockDb)
            {
                DataBase.UpdateInfoFromNet(Info);
            }
            
            DetailMovie detailMovie = DataBase.SelectDetailMovieById(ID);


            //nfo 信息保存到视频同目录
            if (Properties.Settings.Default.SaveInfoToNFO)
            {
                if (Directory.Exists(Properties.Settings.Default.NFOSavePath))
                {
                    //固定位置
                    nfo.SaveToNFO(detailMovie, Path.Combine(Properties.Settings.Default.NFOSavePath, $"{ID}.nfo"));
                }
                else
                {
                    //与视频同路径
                    string path = detailMovie.filepath;
                    if (System.IO.File.Exists(path))
                    {
                        nfo.SaveToNFO(detailMovie, Path.Combine(new FileInfo(path).DirectoryName, $"{ID}.nfo"));
                    }
                }
            }




        }


        protected virtual async Task<(string, int)> GetMovieCode()
        {
            return await Task.Run(() => { return ("", 404); });
        }

        public virtual async Task<bool> Crawl(Action<int> callback = null, Action<int> ICcallback = null)
        {
            (Content, StatusCode) = await Net.Http(Url, Cookie: Cookies);
            if (StatusCode == 200 & Content != "") { SaveInfo(GetInfo(), webSite); return true; }
            else
            {
                resultMessage = $"地址：{Url}，错误原因：获取网页源码失败";
                Logger.LogN(resultMessage);
                callback?.Invoke(StatusCode);
                return false;
            }
        }

        protected virtual Dictionary<string, string> GetInfo()
        {
            return new Dictionary<string, string>();
        }
    }


    public class BusCrawler : Crawler
    {

        public BusCrawler(string Id, VedioType vedioType) : base(Id)
        {
            VedioType = vedioType;
            if (vedioType == VedioType.欧美) { Url = RootUrl.BusEu + ID.Replace(".", "-"); webSite = WebSite.BusEu; }
            else { Url = RootUrl.Bus + ID.ToUpper(); webSite = WebSite.Bus; }

        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new BusParse(ID, Content, VedioType).Parse();
            if (Info.Count <= 0) { 
                Logger.LogN($"地址：{Url}，失败原因：信息解析失败"); 
            }
            else
            {
                Info.Add("sourceurl", Url);
                Info.Add("source", "javbus");
                Task.Delay(TASKDELAY_SHORT).Wait();
            }
            return Info;
        }

    }

    public class FC2Crawler : Crawler
    {

        public FC2Crawler(string Id) : base(Id)
        {
             Url =$"{RootUrl.FC2}article/{ID.ToUpper().Replace("FC2-","")}/"; 
             webSite = WebSite.FC2; 
        }


        public override async Task<bool> Crawl(Action<int> callback = null, Action<int> ICcallback = null)
        {
            (Content, StatusCode) = await Net.Http(Url, Cookie: Cookies);
            if (StatusCode == 200 && Content .IndexOf("非常抱歉，此商品未在您的居住国家公开")<0 && Content.IndexOf("非常抱歉，找不到您要的商品") <0) { 
                SaveInfo(GetInfo(), webSite);
                await Task.Delay(TASKDELAY_LONG);
                return true; 
            }
            else if (Content.IndexOf("非常抱歉，此商品未在您的居住国家公开")> 0 || Content.IndexOf("お探しの商品が見つかりません") > 0)
            {
                resultMessage = $"地址：{Url}，错误原因：非常抱歉，此商品未在您的居住国家公开";
                Logger.LogN(resultMessage);
                callback?.Invoke(403);
                return false;
            }else if (Content.IndexOf("非常抱歉，找不到您要的商品")>0)
            {
                resultMessage = $"地址：{Url}，错误原因：非常抱歉，找不到您要的商品";
                Logger.LogN(resultMessage);
                callback?.Invoke(404);
                return false;
            }
            else
            {
                resultMessage = $"地址：{Url}，错误原因：获取网页源码失败";
                Logger.LogN(resultMessage);
                callback?.Invoke(StatusCode);
                return false;
            }
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new FC2Parse(ID, Content).Parse();
            if (Info.Count <= 0)
            {
                Logger.LogN($"地址：{Url}，失败原因：信息解析失败");
            }
            else if (Content.IndexOf("非常抱歉，此商品未在您的居住国家公开")>0)
            {
                Logger.LogN($"非常抱歉，此商品未在您的居住国家公开");
            }
            else
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "fc2adult");
                Task.Delay(TASKDELAY_SHORT).Wait();
            }
            return Info;
        }

    }
    public class DBCrawler : Crawler
    {
        public DBCrawler(string Id) : base(Id)
        {
            Url = RootUrl.DB + $"search?q={ID}&f=all";
            Cookies = AllCookies.DB;
            webSite = WebSite.DB;
        }

        protected override async Task<(string, int)> GetMovieCode()
        {
            string result = DataBase.SelectInfoByID("code", "javdb", ID);
            int statusCode = 404;
            //先从数据库获取
            if (result == "")
            {
                //从网络获取
                string content;
                (content, statusCode) = await Net.Http(Url, Cookie: Cookies,allowRedirect:false);

                if (statusCode == 200 & content != "")
                    result = GetMovieCodeFromSearchResult(content);
            }

            //存入数据库
            if (result != "") { DataBase.SaveMovieCodeByID(ID, "javdb", result); }

            return (result, statusCode);
        }

        protected string GetMovieCodeFromSearchResult(string content)
        {
            string result = "";
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);

            HtmlNodeCollection gridNodes = doc.DocumentNode.SelectNodes("//div[@class='grid columns']/div/a/div[@class='uid']");
            if (gridNodes != null)
            {
                foreach (HtmlNode gridNode in gridNodes)
                {
                    if (gridNode.InnerText.ToUpper() == ID.ToUpper())
                    {
                        result = gridNode.ParentNode.Attributes["href"].Value.Replace("/v/", "");
                        break;
                    }
                }
            }
            return result;
        }



        public override async Task<bool> Crawl(Action<int> callback = null, Action<int> ICcallback = null)
        {
            int statuscode = 404;
            (MovieCode, statuscode) = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = RootUrl.DB + $"v/{MovieCode}";
                return await base.Crawl(callback);
            }
            else
            {
                if (statuscode==200) statuscode = 404;
                ICcallback?.Invoke(statuscode);
                return false;
            }
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new JavDBParse(ID, Content, MovieCode).Parse();
            if (Info.Count <= 0) { Console.WriteLine($"解析失败：{Url}"); resultMessage = $"地址：{Url}，失败原因：无法解析！"; Logger.LogN(resultMessage); }
            else
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "javdb");
                Task.Delay(TASKDELAY_MEDIUM).Wait();
            }
            return Info;

        }

    }

    public class LibraryCrawler : Crawler
    {
        public LibraryCrawler(string Id) : base(Id)
        {
            Url = RootUrl.Library + $"vl_searchbyid.php?keyword={ID}";
            webSite = WebSite.Library;
        }

        protected override async Task<(string, int)> GetMovieCode()
        {
            int StatusCode = 404;
            string result;
            //先从数据库获取
            result = DataBase.SelectInfoByID("code", "library", ID);
            if (string.IsNullOrEmpty(result) || result.IndexOf("zh-cn") >= 0)
            {
                //从网络获取
                string Location="";
                string content = "";
                (content, StatusCode) = await Net.Http(Url, Mode: HttpMode.RedirectGet);
                if (StatusCode == (int)System.Net.HttpStatusCode.Redirect) Location = content;
                else result = GetMovieCodeFromSearchResult(content);

                if (Location.IndexOf("=") >= 0) result = Location.Split('=').Last();
            }

            //存入数据库
            if (result != "" && result.IndexOf("zh-cn") < 0) { DataBase.SaveMovieCodeByID(ID, "library", result); } else { result = ""; }

            return (result, StatusCode);
        }

        private string GetMovieCodeFromSearchResult(string html)
        {
            string result = "";
            if (string.IsNullOrEmpty(html)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='id']");
            if (nodes != null)
            {
                HtmlNode linknode = null;
                string id = "";
                foreach (HtmlNode node in nodes)
                {
                    if (node == null) continue;
                    id = node.InnerText;
                    if(!string.IsNullOrEmpty(id) && id.ToUpper() == ID.ToUpper())
                    {
                        linknode = node.ParentNode;
                        if (linknode != null)
                        {
                            string link = linknode.Attributes["href"]?.Value;
                            if (link.IndexOf("=") > 0) result = link.Split('=').Last();
                        }
                        break;

                    }
                }
                }


            return result;

        }


        public override async Task<bool> Crawl(Action<int> callback = null, Action<int> ICcallback = null)
        {
            int statuscode = 404;
            (MovieCode, statuscode) = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = RootUrl.Library + $"?v={MovieCode}";
                return await base.Crawl(callback);
            }
            else
            {
                if (statuscode == 200) statuscode = 404;
                ICcallback?.Invoke(statuscode);
                return false;
            }
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new LibraryParse(ID, Content).Parse();
            if (Info.Count <= 0) { Console.WriteLine($"解析失败：{Url}"); resultMessage = $"地址：{Url}，失败原因：无法解析！"; Logger.LogN(resultMessage); }
            else
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "javlibrary");
                Task.Delay(TASKDELAY_MEDIUM).Wait();
            }
            return Info;
        }

    }

    public class DMMCrawler : Crawler
    {
        public DMMCrawler(string Id) : base(Id)
        {
            //https://www.dmm.co.jp/search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr=fsdss-026&commit.x=5&commit.y=18 
            Url = $"{RootUrl.DMM}search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr={ID}&commit.x=5&commit.y=18";
            webSite = WebSite.DMM;
        }

        protected override async Task<(string, int)> GetMovieCode()
        {
            int StatusCode = 404;
            string result="";

            //从网络获取
            string Location = "";
            string content = "";
            (content, StatusCode) = await Net.Http(Url, Mode: HttpMode.RedirectGet);
            if (StatusCode == (int)System.Net.HttpStatusCode.Redirect) Location = content;
            //https://www.dmm.co.jp/mono/dvd/-/search/=/searchstr=APNS-006/
            if (Location.IsProperUrl())
            {
                content = "";StatusCode = 404;
                (content, StatusCode) = await Net.Http(Location,Cookie:Properties.Settings.Default.DMMCookie, Mode: HttpMode.RedirectGet);
                if(StatusCode==200 && !string.IsNullOrEmpty(content))
                {
                    result = GetLinkFromSearchResult(content);
                }
                else
                {
                    Console.WriteLine("无资源");
                }
            }
            else
            {
                Console.WriteLine("Location 不是正确的网络地址");
            }

            return (result, StatusCode);
        }

        //HACK
        private string GetLinkFromSearchResult(string html)
        {
            string result = "";
            if (string.IsNullOrEmpty(html)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//p[@class='tmd']/a");

            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    if (node == null) continue;
                    string link = node.Attributes["href"]?.Value;
                    if (link.IsProperUrl())
                    {
                        if (Identify.GetFanhaoFromDMMUrl(link) == ID)
                        {
                            result = link;
                            break;
                        }
                    }
                }
            }


            return result;

        }


        //TODO
        public override async Task<bool> Crawl(Action<int> callback = null, Action<int> ICcallback = null)
        {
            return false;
            int statuscode = 404;
            (MovieCode, statuscode) = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = RootUrl.Library + $"?v={MovieCode}";
                return await base.Crawl(callback);
            }
            else
            {
                if (statuscode == 200) statuscode = 404;
                ICcallback?.Invoke(statuscode);
                return false;
            }
        }

        //UNDONE
        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new Dictionary<string, string>();
            return Info;
            Info = new LibraryParse(ID, Content).Parse();
            if (Info.Count <= 0) { Console.WriteLine($"解析失败：{Url}"); resultMessage = $"地址：{Url}，失败原因：无法解析！"; Logger.LogN(resultMessage); }
            else
            {
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "dmm");
                Task.Delay(TASKDELAY_MEDIUM).Wait();
            }
            return Info;
        }

    }

    public class Jav321Crawler : Crawler
    {
        public Jav321Crawler(string Id) : base(Id)
        {
            Url = RootUrl.Jav321 + $"video/{ID.ToJav321()}";
            webSite = WebSite.Jav321;
        }



        public override async Task<bool> Crawl(Action<int> callback = null, Action<int> ICcallback = null)
        {
            (Content, StatusCode) = await Net.Http(Url, Cookie: Cookies);
            if (StatusCode == 200 & Content != "")
            {
                Dictionary<string, string> Info = GetInfo();
            }




            if (MovieCode != "")
            {
                //解析
                Url = RootUrl.Library + $"?v={MovieCode}";
                return await base.Crawl(callback);
            }
            else
            {
                return false;
            }
        }


        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new Jav321Parse(ID, Content).Parse();
            if (Info.Count <= 0) { Console.WriteLine($"解析失败：{Url}"); resultMessage = $"地址：{Url}，失败原因：无法解析！"; Logger.LogN(resultMessage); }
            else
            {
                Info.Add("sourceurl", Url);
                Info.Add("source", "jav321");
                Task.Delay(TASKDELAY_MEDIUM).Wait();
            }
            return Info;
        }

    }
}
