
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.Net;
using static Jvedio.GlobalVariable;
using System.IO;
using System.Net;

namespace Jvedio
{
    public class CrawlerHeader
    {
        public string Method="GET";
        public string Host;
        public string Connection= "keep-alive";
        public string CacheControl= "max-age=0";
        public string UpgradeInsecureRequests= "1";
        public string UserAgent= "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
        public string Accept = "*/*";
        public string SecFetchSite;
        public string SecFetchMode;
        public string SecFetchUser;
        public string SecFetchDest;
        public string AcceptEncoding;
        public string AcceptLanguage;
        public string Cookies;
    }

    public class ResponseHeaders
    {
        public string Date;
        public string ContentType;
        public string Connection;
        public string CacheControl;
        public string SetCookie;
        public string Location;
        public double ContentLength;
    }

    public class HttpResult
    {
        public bool Success=false;
        public string Error="";
        public string SourceCode = "";
        public byte[] FileByte;
        public string MovieCode;
        public HttpStatusCode StatusCode = HttpStatusCode.Forbidden;
        public ResponseHeaders Headers;
    }


    public abstract class Crawler
    {
        public static int TASKDELAY_SHORT = 300;//短时间暂停
        public static int TASKDELAY_MEDIUM = 1000;
        public static int TASKDELAY_LONG = 2000;//长时间暂停

        protected string Url = "";//网址
        protected CrawlerHeader headers;
        protected HttpResult httpResult;
        


        public string ID { get; set; }//必须给出视频 ID

        public Crawler(string Id)
        {
            ID = Id;
        }


        public abstract void InitHeaders();

        public abstract Task<HttpResult> Crawl();
        

        public abstract Dictionary<string, string> GetInfo();
    }


    public class BusCrawler : Crawler
    {
        public VedioType VedioType { get; set; }
        public BusCrawler(string Id, VedioType vedioType) : base(Id)
        {
            VedioType = vedioType;
            if (vedioType == VedioType.欧美) { 
                Url = RootUrl.BusEu + ID.Replace(".", "-"); 
            }
            else { 
                Url = RootUrl.Bus + ID.ToUpper(); 
            }
        }

        public override async Task<HttpResult> Crawl()
        {
            InitHeaders();
            httpResult = await Net.Http(Url,headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                FileProcess.SaveInfo(GetInfo(), ID);
                httpResult.Success = true;
            }
            return httpResult;
        }


        public override void InitHeaders()
        {
            headers = new CrawlerHeader();
        }


        public override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new BusParse(ID, httpResult.SourceCode, VedioType).Parse();
            if (Info.Count >0) 
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
        }


        public override void InitHeaders()
        {
            headers = new CrawlerHeader();
        }

        public override async Task<HttpResult> Crawl()
        {
            InitHeaders();
            httpResult = await Net.Http(Url, headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                if(httpResult.SourceCode.IndexOf("非常抱歉，此商品未在您的居住国家公开") < 0 && httpResult.SourceCode.IndexOf("非常抱歉，找不到您要的商品") < 0)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                }
                else if (httpResult.SourceCode.IndexOf("非常抱歉，此商品未在您的居住国家公开") > 0 )
                {
                    httpResult.StatusCode = HttpStatusCode.Forbidden;
                    httpResult.Success = false;
                }
                else if (httpResult.SourceCode.IndexOf("非常抱歉，找不到您要的商品") > 0)
                {
                    httpResult.StatusCode = HttpStatusCode.NotFound;
                    httpResult.Success = false;
                }
            }
            return httpResult;
        }


        public override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new FC2Parse(ID, httpResult.SourceCode).Parse();
            if (Info.Count <= 0)
            {
                Logger.LogN($"{Jvedio.Language.Resources.Url}：{Url}");
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

        protected string MovieCode;
        public DBCrawler(string Id) : base(Id)
        {
            Url = RootUrl.DB + $"search?q={ID}&f=all";
        }
        public override void InitHeaders()
        {
            headers = new CrawlerHeader() {
                Cookies = AllCookies.DB
            };
        }

        public  async Task<string> GetMovieCode()
        {
            //先从数据库获取
            string movieCode = DataBase.SelectInfoByID("code", "javdb", ID);
            if (movieCode == "")
            {
                //从网络获取
                HttpResult result = await Net.Http(Url, headers, allowRedirect: false);
                if (result!=null && result.SourceCode!="") 
                    movieCode = GetMovieCodeFromSearchResult(result.SourceCode);

                //存入数据库
                if (movieCode != "")
                    DataBase.SaveMovieCodeByID(ID, "javdb", movieCode);
            }
            return movieCode;
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



        public override async Task<HttpResult> Crawl()
        {
            InitHeaders();
            MovieCode = await GetMovieCode();
            if (MovieCode != "")
            {
                Url = RootUrl.DB + $"v/{MovieCode}";
                httpResult = await Net.Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                }
            }
            return httpResult;

        }


        public override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new JavDBParse(ID, httpResult.SourceCode, MovieCode).Parse();
            if (Info.Count > 0) 
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
        protected string MovieCode;
        public LibraryCrawler(string Id) : base(Id)
        {
            Url = RootUrl.Library + $"vl_searchbyid.php?keyword={ID}";
        }

        protected  async Task<string> GetMovieCode()
        {
            string movieCode=DataBase.SelectInfoByID("code", "library", ID);
            //先从数据库获取
            if (string.IsNullOrEmpty(movieCode) || movieCode.IndexOf("zh-cn") >= 0)
            {
                HttpResult result= await Net.Http(Url, Mode: HttpMode.RedirectGet);
                if (result != null && result.StatusCode == HttpStatusCode.Redirect) movieCode = httpResult.Headers.Location;
                else if(result!=null) movieCode = GetMovieCodeFromSearchResult(result.SourceCode);

                if (movieCode.IndexOf("=") >= 0) movieCode = movieCode.Split('=').Last();
            }

            //存入数据库
            if (movieCode != "" && movieCode.IndexOf("zh-cn") < 0) { 
                DataBase.SaveMovieCodeByID(ID, "library", movieCode); 
            }

            return movieCode;
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

        public override void InitHeaders()
        {
            headers = new CrawlerHeader();
        }


        public override async Task<HttpResult> Crawl()
        {
            InitHeaders();
            MovieCode = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = RootUrl.Library + $"?v={MovieCode}";
                httpResult = await Net.Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                }
            }
            return httpResult;
        }


        public override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new LibraryParse(ID, httpResult.SourceCode).Parse();
            if (Info.Count > 0)
            { 
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "javlibrary");
                Task.Delay(TASKDELAY_MEDIUM).Wait();
            }
            return Info;
        }

    }

    //public class DMMCrawler : Crawler
    //{
    //    public DMMCrawler(string Id) : base(Id)
    //    {
    //        //https://www.dmm.co.jp/search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr=fsdss-026&commit.x=5&commit.y=18 
    //        Url = $"{RootUrl.DMM}search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr={ID}&commit.x=5&commit.y=18";
    //        webSite = WebSite.DMM;
    //    }

    //    protected override async Task<(string, int)> GetMovieCode()
    //    {
    //        int StatusCode = 404;
    //        string result="";

    //        //从网络获取
    //        string Location = "";
    //        string content = "";
    //        (content, StatusCode) = await Net.Http(Url, Mode: HttpMode.RedirectGet);
    //        if (StatusCode == (int)System.Net.HttpStatusCode.Redirect) Location = content;
    //        //https://www.dmm.co.jp/mono/dvd/-/search/=/searchstr=APNS-006/
    //        if (Location.IsProperUrl())
    //        {
    //            content = "";StatusCode = 404;
    //            (content, StatusCode) = await Net.Http(Location,Cookie:Properties.Settings.Default.DMMCookie, Mode: HttpMode.RedirectGet);
    //            if(StatusCode==200 && !string.IsNullOrEmpty(content))
    //            {
    //                result = GetLinkFromSearchResult(content);
    //            }
    //            else
    //            {
    //                Console.WriteLine("无资源");
    //            }
    //        }
    //        else
    //        {
    //            Console.WriteLine("Location 不是正确的网络地址");
    //        }

    //        return (result, StatusCode);
    //    }

    //    //HACK
    //    private string GetLinkFromSearchResult(string html)
    //    {
    //        string result = "";
    //        if (string.IsNullOrEmpty(html)) return result;
    //        HtmlDocument doc = new HtmlDocument();
    //        doc.LoadHtml(html);
    //        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//p[@class='tmd']/a");

    //        if (nodes != null)
    //        {
    //            foreach (HtmlNode node in nodes)
    //            {
    //                if (node == null) continue;
    //                string link = node.Attributes["href"]?.Value;
    //                if (link.IsProperUrl())
    //                {
    //                    if (Identify.GetFanhaoFromDMMUrl(link) == ID)
    //                    {
    //                        result = link;
    //                        break;
    //                    }
    //                }
    //            }
    //        }


    //        return result;

    //    }


    //    //TODO
    //    public override async Task<HttpResult> Crawl()
    //    {
    //        return false;
    //        int statuscode = 404;
    //        (MovieCode, statuscode) = await GetMovieCode();
    //        if (MovieCode != "")
    //        {
    //            //解析
    //            Url = RootUrl.Library + $"?v={MovieCode}";
    //            return await base.Crawl(callback);
    //        }
    //        else
    //        {
    //            if (statuscode == 200) statuscode = 404;
    //            callback?.Invoke(statuscode);
    //            return false;
    //        }
    //    }

    //    //UNDONE
    //    protected override Dictionary<string, string> GetInfo()
    //    {
    //        Dictionary<string, string> Info = new Dictionary<string, string>();
    //        return Info;
    //        Info = new LibraryParse(ID, Content).Parse();
    //        if (Info.Count <= 0) { Console.WriteLine($"{Jvedio.Language.Resources.Url}：{Url}"); resultMessage = $"{Jvedio.Language.Resources.Url}：{Url}，{Jvedio.Language.Resources.Reason}：{Jvedio.Language.Resources.ParseFail}！"; Logger.LogN(resultMessage); }
    //        else
    //        {
    //            Info.Add("id", ID);
    //            Info.Add("sourceurl", Url);
    //            Info.Add("source", "dmm");
    //            Task.Delay(TASKDELAY_MEDIUM).Wait();
    //        }
    //        return Info;
    //    }

    //}

    //public class Jav321Crawler : Crawler
    //{
    //    public Jav321Crawler(string Id) : base(Id)
    //    {
    //        Url = RootUrl.Jav321 + $"video/{ID.ToJav321()}";
    //        webSite = WebSite.Jav321;
    //    }



    //    public override async Task<HttpResult> Crawl()
    //    {
    //        (Content, StatusCode) = await Net.Http(Url, Cookie: Cookies);
    //        if (StatusCode == 200 & Content != "")
    //        {
    //            Dictionary<string, string> Info = GetInfo();
    //        }




    //        if (MovieCode != "")
    //        {
    //            //解析
    //            Url = RootUrl.Library + $"?v={MovieCode}";
    //            return await base.Crawl(callback);
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }


    //    protected override Dictionary<string, string> GetInfo()
    //    {
    //        Dictionary<string, string> Info = new Jav321Parse(ID, Content).Parse();
    //        if (Info.Count <= 0) { Console.WriteLine($"{Jvedio.Language.Resources.Url}：{Url}"); resultMessage = $"{Jvedio.Language.Resources.Url}：{Url}，{Jvedio.Language.Resources.Reason}：{Jvedio.Language.Resources.ParseFail}！"; Logger.LogN(resultMessage); }
    //        else
    //        {
    //            Info.Add("sourceurl", Url);
    //            Info.Add("source", "jav321");
    //            Task.Delay(TASKDELAY_MEDIUM).Wait();
    //        }
    //        return Info;
    //    }

    //}


   
}
