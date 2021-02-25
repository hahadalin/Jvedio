
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
        public string Host="";
        public string Connection= "keep-alive";
        public string CacheControl= "max-age=0";
        public string UpgradeInsecureRequests= "1";
        public string UserAgent= "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
        public string Accept = "*/*";
        public string SecFetchSite = "same-origin";
        public string SecFetchMode = "navigate";
        public string SecFetchUser = "?1";
        public string SecFetchDest = "document";
        public string AcceptEncoding = "";
        public string AcceptLanguage = "zh-CN,zh;q=0.9";
        public string Cookies = "";
        public string Referer = "";
    }


    public abstract class Crawler
    {
        protected static int TASKDELAY_SHORT = 300;//短时间暂停
        protected static int TASKDELAY_MEDIUM = 1000;
        protected static int TASKDELAY_LONG = 2000;//长时间暂停

        protected string Url = "";//网址
        protected CrawlerHeader headers;
        protected HttpResult httpResult;
        


        public string ID { get; set; }//必须给出视频 ID

        public Crawler(string Id)
        {
            ID = Id;
            if (Url.IsProperUrl()) InitHeaders();
        }


        protected abstract void InitHeaders();
        protected abstract Dictionary<string, string> GetInfo();

        protected abstract void ParseCookies(string SetCookie);

        public abstract Task<HttpResult> Crawl();


        public void SaveCookies()
        {
            List<string> cookies = new List<string>();
            foreach (KeyValuePair<string, string> item in UrlCookies)
            {
                cookies.Add(item.Key + "：" + item.Value);
            }

            using (StreamWriter sw = new StreamWriter("Cookies", false))
            {
                sw.Write(string.Join(Environment.NewLine, cookies));
            }
        }




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
            httpResult = await Net.Http(Url,headers);
            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
            {
                FileProcess.SaveInfo(GetInfo(), ID);
                httpResult.Success = true;
                ParseCookies(httpResult.Headers.SetCookie);
            }
            return httpResult;
        }

        protected override void ParseCookies(string SetCookie)
        {
            if (SetCookie == null) return;
            List<string> Cookies = new List<string>();
            var values = SetCookie.Split(new char[] { ',',';'}).ToList();
            foreach (var item in values)
            {
                if (item.IndexOf('=') < 0) continue;
                string key = item.Split('=')[0];
                string value= item.Split('=')[1];
                if (key == "__cfduid" || key == "PHPSESSID" || key == "existmag") Cookies.Add(key + "=" + value);
            }
            string cookie = string.Join(";", Cookies);
            Uri uri = new Uri(Url);
            if (!UrlCookies.ContainsKey(uri.Host)) UrlCookies.Add(uri.Host, cookie);
            else UrlCookies[uri.Host] = cookie;
            SaveCookies();
        }


        protected override void InitHeaders()
        {
            
            Uri uri = new Uri(Url);
            string cookie = "";
            if (UrlCookies.ContainsKey(uri.Host)) cookie = UrlCookies[uri.Host];
            headers = new CrawlerHeader() { Cookies=cookie};
        }


        protected override Dictionary<string, string> GetInfo()
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


        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() {
                Cookies = AllCookies.FC2
                };
        }

        public override async Task<HttpResult> Crawl()
        {
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


        protected override Dictionary<string, string> GetInfo()
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

        protected override void ParseCookies(string SetCookie)
        {
            throw new NotImplementedException();
        }
    }
    public class DBCrawler : Crawler
    {

        protected string MovieCode;
        public DBCrawler(string Id) : base(Id)
        {
            Url = RootUrl.DB + $"search?q={ID}&f=all";
        }
        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() {
                Cookies = AllCookies.DB
            };
        }

        public  async Task<string> GetMovieCode(Action<string> callback=null)
        {
            //先从数据库获取
            string movieCode = DataBase.SelectInfoByID("code", "javdb", ID);
            if (movieCode == "")
            {
                //从网络获取
                HttpResult result = await Net.Http(Url, headers, allowRedirect: false);
                if (result != null && result.StatusCode == HttpStatusCode.Redirect) callback?.Invoke(Jvedio.Language.Resources.SearchTooFrequent);
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
            MovieCode = await GetMovieCode((error)=> {
                httpResult = new HttpResult() { Error=error,Success=false};
            });
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

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }


        protected override Dictionary<string, string> GetInfo()
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
                HttpResult result= await Net.Http(Url,headers, Mode: HttpMode.RedirectGet);
                if (result != null && result.StatusCode == HttpStatusCode.Redirect) movieCode = result.Headers.Location;
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

        protected override void InitHeaders()
        {
            Uri uri = new Uri(Url);
            string cookie = "";
            if (UrlCookies.ContainsKey(uri.Host)) cookie = UrlCookies[uri.Host];
            headers = new CrawlerHeader() { Cookies = cookie };
        }


        public override async Task<HttpResult> Crawl()
        {
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
                    ParseCookies(httpResult.Headers.SetCookie);
                }
            }
            return httpResult;
        }

        protected override void ParseCookies(string SetCookie)
        {
            if (SetCookie == null) return;
            List<string> Cookies = new List<string>();
            var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
            foreach (var item in values)
            {
                if (item.IndexOf('=') < 0) continue;
                string key = item.Split('=')[0];
                string value = item.Split('=')[1];
                if (key == "__cfduid" || key == "__qca") Cookies.Add(key + "=" + value);
            }
            Cookies.Add("over18=18");
            string cookie = string.Join(";", Cookies);
            Uri uri = new Uri(Url);
            if (!UrlCookies.ContainsKey(uri.Host)) UrlCookies.Add(uri.Host, cookie);
            else UrlCookies[uri.Host] = cookie;
            SaveCookies();
        }

        protected override Dictionary<string, string> GetInfo()
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

    public class FANZACrawler : Crawler
    {

        protected string MovieCode = "";
        public FANZACrawler(string Id) : base(Id)
        {
            //https://www.dmm.co.jp/search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr=fsdss-026&commit.x=5&commit.y=18 
            Url = $"{RootUrl.DMM}search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr={ID}&commit.x=5&commit.y=18";
        }

        protected  async Task<string> GetMovieCode()
        {
            //从网络获取
            string movieCode = "";
            string link = "";
            HttpResult result = await Net.Http(Url, headers, Mode: HttpMode.RedirectGet);
            if (result != null && result.StatusCode == HttpStatusCode.Redirect) 
                link = result.Headers.Location;//https://www.dmm.co.jp/mono/dvd/-/search/=/searchstr=ABP-123/

            if (link != "")
            {
                HttpResult newResult= await Net.Http(link, headers);
                if (newResult != null && newResult.StatusCode == HttpStatusCode.OK)
                {
                    if (newResult.SourceCode.IndexOf("に一致する商品は見つかりませんでした") > 0)
                    {
                        //不存在
                    }
                    else
                    {
                        //存在并解析
                        movieCode = GetLinkFromSearchResult(newResult.SourceCode);
                    }
                }
                    
            }
            return movieCode;
        }

        //HACK
        private string GetLinkFromSearchResult(string html)
        {
            string result = "";
            if (string.IsNullOrEmpty(html)) return result;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//p[@class='tmb']/a");

            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    if (node == null) continue;
                    string link = node.Attributes["href"]?.Value;
                    if (link.IsProperUrl())
                    {
                        string fanhao = Identify.GetFanhaoFromDMMUrl(link);
                        if (Identify.GetEng(fanhao).ToUpper() == Identify.GetEng(ID).ToUpper())
                        {
                            string str1 = Identify.GetNum(fanhao);
                            string str2 = Identify.GetNum(ID);
                            int num1 = 0;
                            int num2 = 1;
                            int.TryParse(str1, out num1);
                            int.TryParse(str2, out num2);
                            if (num1 == num2)
                            {
                                result = link;
                                break;
                            }
                        }

                    }
                }
            }


            return result;

        }




        public override async Task<HttpResult> Crawl()
        {
            MovieCode = await GetMovieCode();
            if (MovieCode != "")
            {
                //解析
                Url = MovieCode;
                httpResult = await Net.Http(Url, headers);
                if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
                {
                    FileProcess.SaveInfo(GetInfo(), ID);
                    httpResult.Success = true;
                }
            }
            return httpResult;
        }

        protected override Dictionary<string, string> GetInfo()
        {
            Dictionary<string, string> Info = new Dictionary<string, string>();
            Info = new FanzaParse(ID, httpResult.SourceCode).Parse();
            if (Info.Count > 0)
            { 
                Info.Add("id", ID);
                Info.Add("sourceurl", Url);
                Info.Add("source", "FANZA");
                Task.Delay(TASKDELAY_MEDIUM).Wait();
            }
            return Info;
        }

        protected override void InitHeaders()
        {
            headers = new CrawlerHeader() { Cookies = AllCookies.DMM };
        }

        protected override void ParseCookies(string SetCookie)
        {
            return;
        }
    }

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
