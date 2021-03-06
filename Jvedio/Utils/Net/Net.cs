using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;


namespace Jvedio
{

    public class ResponseHeaders
    {
        public string Date = "";
        public string ContentType = "";
        public string Connection = "";
        public string CacheControl = "";
        public string SetCookie = "";
        public string Location = "";
        public double ContentLength = 0;
    }

    public class HttpResult
    {
        public bool Success = false;
        public string Error = "";
        public string SourceCode = "";
        public byte[] FileByte = null;
        public string MovieCode = "";
        public HttpStatusCode StatusCode = HttpStatusCode.Forbidden;
        public ResponseHeaders Headers = null;
    }


    public static class Net
    {
        public static int TCPTIMEOUT = 30;   // TCP 超时
        public static int HTTPTIMEOUT = 30; // HTTP 超时
        public static int ATTEMPTNUM = 2; // 最大尝试次数
        public static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";

        public static int REQUESTTIMEOUT = 30000;//网站 HTML 获取超时
        public static int FILE_REQUESTTIMEOUT = 30000;//图片下载超时
        public static int READWRITETIMEOUT = 30000;

        public const string UpgradeSource = "https://hitchao.github.io";
        public const string UpdateUrl = "https://hitchao.github.io/jvedioupdate/Version";
        public const string UpdateExeVersionUrl = "https://hitchao.github.io/jvedioupdate/update";
        public const string UpdateExeUrl = "https://hitchao.github.io/jvedioupdate/JvedioUpdate.exe";




        public static void Init()
        {
            TCPTIMEOUT = Properties.Settings.Default.Timeout_tcp;
            HTTPTIMEOUT = Properties.Settings.Default.Timeout_forcestop;
            REQUESTTIMEOUT = Properties.Settings.Default.Timeout_http * 1000;
            FILE_REQUESTTIMEOUT = Properties.Settings.Default.Timeout_download * 1000;
            READWRITETIMEOUT = Properties.Settings.Default.Timeout_stream * 1000;
        }



        public enum HttpMode
        {
            Normal = 0,
            RedirectGet = 1,
            Stream=2
        }


        public static async Task<HttpResult> Http(string Url, CrawlerHeader headers = null, HttpMode Mode = HttpMode.Normal, WebProxy Proxy = null,bool allowRedirect=true,string poststring="")
        {
            if (!Url.IsProperUrl()) return null;
            if (headers == null) headers = new CrawlerHeader();
            int trynum = 0;
            HttpResult httpResult = null;

            try
            {
                while (trynum < ATTEMPTNUM && httpResult == null)
                {
                    httpResult = await Task.Run(() =>
                    {
                        HttpWebRequest Request;
                        HttpWebResponse Response = default;
                        try
                        {
                            Request = (HttpWebRequest)HttpWebRequest.Create(Url);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogE(ex);
                            return null;
                        }
                        Uri uri = new Uri(Url);
                        Request.Host = headers.Host == "" ? uri.Host : headers.Host;
                        Request.Accept = headers.Accept;
                        Request.Timeout = 50000;
                        Request.Method = headers.Method;
                        Request.KeepAlive = true;
                        Request.AllowAutoRedirect = allowRedirect;
                        Request.Referer = uri.Scheme + "://" + uri.Host + "/";
                        Request.UserAgent = headers.UserAgent;
                        Request.Headers.Add("Accept-Language", headers.AcceptLanguage);
                        Request.Headers.Add("Upgrade-Insecure-Requests", headers.UpgradeInsecureRequests);
                        Request.Headers.Add("Sec-Fetch-Site", headers.SecFetchSite);
                        Request.Headers.Add("Sec-Fetch-Mode", headers.SecFetchMode);
                        Request.Headers.Add("Sec-Fetch-User", headers.SecFetchUser);
                        Request.Headers.Add("Sec-Fetch-Dest", headers.SecFetchDest);
                        Request.ReadWriteTimeout = READWRITETIMEOUT;
                        if (headers.Cookies != "") Request.Headers.Add("Cookie", headers.Cookies);
                        if (Mode == HttpMode.RedirectGet) Request.AllowAutoRedirect = false;
                        if (Proxy != null) Request.Proxy = Proxy;

                        try
                        {
                            if (headers.Method == "POST")
                            {
                                Request.Method = "POST";
                                Request.ContentType =headers.ContentType;
                                Request.ContentLength = headers.ContentLength;
                                Request.Headers.Add("Origin", headers.Origin);
                                byte[] bs = Encoding.UTF8.GetBytes(poststring);
                                using (Stream reqStream = Request.GetRequestStream())
                                {
                                    reqStream.Write(bs, 0, bs.Length);
                                }
                            }
                            Response = (HttpWebResponse)Request.GetResponse();
                            httpResult = GetHttpResult(Response, Mode);
                            Logger.LogN($" {Jvedio.Language.Resources.Url}：{Url} => {httpResult.StatusCode}");
                        }
                        catch (WebException e)
                        {
                            Logger.LogN($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");
                            if (e.Status == WebExceptionStatus.Timeout)
                                trynum++;
                            else
                                trynum = 2;
                        }
                        catch (Exception e)
                        {
                            Logger.LogN($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");
                            trynum = 2;
                        }
                        finally
                        {
                            if (Response != null) Response.Close();
                        }
                        return httpResult;
                    }).TimeoutAfter(TimeSpan.FromSeconds(HTTPTIMEOUT));
                }
            }
            catch(TimeoutException ex)
            {
                //任务超时了
                Console.WriteLine(ex.Message);
                Logger.LogN(ex.Message);
            }
            return httpResult;
        }


        public static HttpResult GetHttpResult(HttpWebResponse Response,HttpMode mode)
        {
            HttpResult httpResult = new HttpResult();
            httpResult.StatusCode = Response.StatusCode;
            WebHeaderCollection webHeaderCollection = Response.Headers;

            //获得响应头
            ResponseHeaders responseHeaders = new ResponseHeaders()
            {
                Location = webHeaderCollection.Get("Location"),
                Date = webHeaderCollection.Get("Date"),
                ContentType = webHeaderCollection.Get("Content-Type"),
                Connection = webHeaderCollection.Get("Connection"),
                CacheControl = webHeaderCollection.Get("Cache-Control"),
                SetCookie = webHeaderCollection.Get("Set-Cookie"),
            };
            double.TryParse(webHeaderCollection.Get("Content-Length"), out responseHeaders.ContentLength);
            
            httpResult.Headers = responseHeaders;
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                if (mode == HttpMode.Normal)
                {
                    using (StreamReader sr = new StreamReader(Response.GetResponseStream()))
                    {
                        httpResult.SourceCode = sr.ReadToEnd();
                    }
                    if (responseHeaders.ContentLength == 0) responseHeaders.ContentLength = httpResult.SourceCode.Length;
                }
                else if (mode == HttpMode.Stream)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Response.GetResponseStream().CopyTo(ms);
                        httpResult.FileByte = ms.ToArray();
                    }
                    if (responseHeaders.ContentLength == 0) responseHeaders.ContentLength = httpResult.FileByte.Length;
                }



            }
            return httpResult;
        }


        public static async Task<(bool, string, string)> CheckUpdate()
        {
            return await Task.Run(async () =>
            {
                HttpResult httpResult=null;
                try
                {
                    httpResult = await Net.Http(UpdateUrl);
                }
                catch (TimeoutException ex) { Logger.LogN($"URL={UpdateUrl},Message-{ex.Message}"); }

                if(httpResult==null || string.IsNullOrEmpty(httpResult.SourceCode)) return (false, "", "");

                string remote = httpResult.SourceCode.Split('\n')[0];
                string updateContent = httpResult.SourceCode.Replace(remote + "\n", "");
                string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "OldVersion"))
                {
                    sw.WriteLine(local + "\n");
                    sw.WriteLine(updateContent);
                }
                return (true, remote, updateContent);


            });
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException(Jvedio.Language.Resources.TO);
                }
            }
        }
        public async static Task<WebSite> CheckUrlType(string url)
        {
            WebSite webSite = WebSite.None;
            bool enablecookie = false;
            string label = "";
            (bool result, string title) = await Net.TestAndGetTitle(url, enablecookie, "", label);
            if (result)
            {
                //其他，进行判断
                if (title.ToLower().IndexOf("javbus") >= 0 && title.IndexOf("歐美") < 0)
                {
                    webSite = WebSite.Bus;
                }
                else if (title.ToLower().IndexOf("javbus") >= 0 && title.IndexOf("歐美") >= 0)
                {
                    webSite = WebSite.BusEu;
                }
                else if (title.ToLower().IndexOf("javlibrary") >= 0)
                {
                    webSite = WebSite.Library;
                }
                else if (title.ToLower().IndexOf("fanza") >= 0)
                {
                    webSite = WebSite.DMM;
                }
                else if (title.ToLower().IndexOf("jav321") >= 0)
                {
                    webSite = WebSite.Jav321;
                }
                else if (title.ToLower().IndexOf("javdb") >= 0)
                {
                    webSite = WebSite.DB;

                }
                else if (title.ToLower().IndexOf("avmoo") >= 0)
                {
                    webSite = WebSite.MOO;

                }
                else
                {
                    webSite = WebSite.None;
                }
            }
            return webSite;
        }


        public static async Task<(bool, string)> TestAndGetTitle(string Url, bool EnableCookie, string Cookie, string Label)
        {
                bool result = false;
                string title = "";
                HttpResult httpResult = null;
                if (EnableCookie)
                {
                    if (Label == "DB")
                    {
                        httpResult = await Http(Url + "v/P2Rz9", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult!=null  && httpResult.SourceCode.IndexOf("FC2-659341") >= 0)
                        {
                                result = true; 
                                title = "DB"; 
                        }
                        else result = false;
                    }
                    else if (Label == "DMM")
                    {
                        httpResult = await Http($"{Url}mono/dvd/-/search/=/searchstr=APNS-006/ ", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult != null && httpResult.SourceCode.IndexOf("里美まゆ・川") >= 0)
                        {
                            result = true; 
                            title = "DMM"; 
                        }
                        else result = false;
                    }
                    else if (Label == "MOO")
                    {
                        httpResult = await Http($"{Url}movie/655358482fd14364 ", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult != null && httpResult.SourceCode.IndexOf("SIVR-118") >= 0)
                        {
                            result = true;
                            title = "MOO";
                        }
                        else result = false;
                    }
                }
                else
                {
                    httpResult = await Http(Url);
                    if (httpResult != null )
                    {
                        result = true;
                        //获得标题
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(httpResult.SourceCode);
                        HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");
                        if (titleNode != null) title = titleNode.InnerText;
                    }
                }
                return (result, title);
        }


        public static async Task<bool> TestUrl(string Url, bool EnableCookie, string Cookie, string Label)
        {
            return await Task.Run(async () =>
            {
                bool result = false;
                HttpResult httpResult = null;
                if (EnableCookie)
                {
                    if (Label == "DB")
                    {
                        httpResult = await Http(Url + "v/P2Rz9", new CrawlerHeader() { Cookies=Cookie});
                        if (httpResult!=null && httpResult.SourceCode.IndexOf("FC2-659341") >= 0) result = true;
                    }
                }
                else
                {
                    httpResult = await Http(Url);
                    if (httpResult != null && httpResult.SourceCode!="") result = true;
                }
                return result;
            });
        }




        private static bool IsDomainAlive(string aDomain, int aTimeoutSeconds)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(aDomain, 80, null, null);

                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(aTimeoutSeconds));

                    if (!success)
                    {
                        return false;
                    }

                    // we have connected
                    client.EndConnect(result);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                Logger.LogN($" {Jvedio.Language.Resources.Url}：{aDomain}， {Jvedio.Language.Resources.Reason}：{e.Message}");
            }
            return false;
        }
        public static async Task<HttpResult> DownLoadFile(string Url, WebProxy Proxy = null, string SetCookie = "")
        {
            Util.SetCertificatePolicy();
            HttpResult httpResult = await Net.Http(Url, new CrawlerHeader() { Cookies = SetCookie }, HttpMode.Stream);
            return httpResult;
        }


        /// <summary>
        /// 异步下载图片
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="imageType"></param>
        /// <param name="ID"></param>
        /// <param name="Cookie"></param>
        /// <returns></returns>
        public static async Task<(bool,string)> DownLoadImage(string Url, ImageType imageType, string ID, string Cookie = "",Action<int> callback=null)
        {
            //如果文件存在则不下载
            string filepath = BasePicPath;
            if(imageType == ImageType.SmallImage)
            {
                filepath = Path.Combine(filepath, "SmallPic", ID + ".jpg");
            }
            else if (imageType == ImageType.BigImage)
            {
                filepath = Path.Combine(filepath, "BigPic", ID + ".jpg");
            }
            if (File.Exists(filepath)) return (true, "");

            if (!Url.IsProperUrl()) return (false,"");
            HttpResult httpResult = await DownLoadFile(Url.Replace("\"", "'"), SetCookie: Cookie);

            bool result = false;
            string cookie = "";


            if (httpResult == null) {
                Logger.LogN($" {Jvedio.Language.Resources.DownLoadImageFail}：{Url}");
                callback?.Invoke((int)HttpStatusCode.Forbidden);
                result = false; 
            }
            else
            {
                if (httpResult.Headers.SetCookie != null)
                    cookie = httpResult.Headers.SetCookie.Split(';')[0];
                else
                    cookie = Cookie;
                result = true;
                ImageProcess.SaveImage(ID, httpResult.FileByte, imageType, Url);
            }
            return (result, cookie);
        }



        public static async Task<bool> DownActress(string ID, string Name,Action<string> callback)
        {
            bool result = false;
            string Url = JvedioServers.Bus.Url + $"star/{ID}";
            HttpResult httpResult = null;
            httpResult = await Http(Url);
            string error = "";
            if (httpResult!=null && httpResult.StatusCode==HttpStatusCode.OK && httpResult.SourceCode!="")
            {
                //id搜索
                BusParse busParse = new BusParse(ID, httpResult.SourceCode, VedioType.骑兵);
                Actress actress = busParse.ParseActress();
                if (actress ==null && string.IsNullOrEmpty(actress.birthday)  && actress.age == 0 && string.IsNullOrEmpty(actress.birthplace))
                {
                    error = $"{Jvedio.Language.Resources.NoActorInfo}：{Url}";

                }
                else
                {
                    actress.sourceurl = Url;
                    actress.source = "javbus";
                    actress.id = ID;
                    actress.name = Name;
                    //保存信息
                    DataBase.InsertActress(actress);
                    result = true;
                }
            }
            else if (httpResult != null)
            {
                error = httpResult.StatusCode.ToStatusMessage();
            }
            else {
                error = Jvedio.Language.Resources.HttpFail;
            }
            Console.WriteLine(error);
            callback.Invoke(error);
            Logger.LogN($"URL={Url},Message-{error}");
            return result;
        }


        public async static Task<(bool, string)> DownLoadSmallPic(DetailMovie dm,bool overWrite=false)
        {
            if (overWrite) return await Net.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id);
            //不存在才下载
            if (!File.Exists(GlobalVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg"))
            {
                 return await Net.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id);
            }
            else return (false, "");

        }


        public async static Task<(bool, string)> DownLoadBigPic(DetailMovie dm,bool overWrite=false)
        {

            if(overWrite) return await Net.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);


            if (!File.Exists(GlobalVariable.BasePicPath + $"BigPic\\{dm.id}.jpg"))
            {
                return await Net.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);
            }
            else
            {
                return (false, "");
            }
        }

        public static async  Task<bool> ParseSpecifiedInfo(WebSite webSite,string id,string url)
        {
            HttpResult httpResult = null;

            if (webSite == WebSite.Bus) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.Bus.Cookie });
            else if (webSite == WebSite.BusEu) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.BusEurope.Cookie });
            else if (webSite == WebSite.Library) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.Library.Cookie });
            else if (webSite == WebSite.Jav321) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.Jav321.Cookie });
            else if (webSite == WebSite.FC2) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.FC2.Cookie });
            else if (webSite==WebSite.DB) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies= JvedioServers.DB.Cookie } );
            else if (webSite == WebSite.DMM) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.DMM.Cookie });
            else if (webSite == WebSite.MOO) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = JvedioServers.MOO.Cookie });
            else httpResult = await Net.Http(url);

            if(httpResult!=null && httpResult.StatusCode==HttpStatusCode.OK && httpResult.SourceCode!="")
            {
                string content = httpResult.SourceCode;
                Dictionary<string, string> Info = new Dictionary<string, string>();
                
                if (webSite == WebSite.Bus)
                {
                    Info = new BusParse(id, content,Identify.GetVedioType(id)).Parse();
                    Info.Add("source", "javbus");
                }
                else if (webSite == WebSite.BusEu)
                {
                    Info = new BusParse(id, content, VedioType.欧美).Parse();
                    Info.Add("source", "javbus");
                }
                else if (webSite == WebSite.DB)
                {
                    Info = new JavDBParse(id, content,url.Split('/').Last()).Parse();
                    Info.Add("source", "javdb");
                }
                else if (webSite == WebSite.Library)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "javlibrary");
                }
                else if (webSite == WebSite.Jav321)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "Jav321");
                }
                else if (webSite == WebSite.DMM)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "DMM");
                }
                else if (webSite == WebSite.MOO)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "MOO");
                }
                else if (webSite == WebSite.FC2)
                {
                    Info = new LibraryParse(id, content).Parse();
                    Info.Add("source", "FC2");
                }
                Info.Add("sourceurl", url);
                if (Info.Count > 2)
                {
                    FileProcess.SaveInfo(Info, id);
                    return true;
                }
            }
            return false;
        }



        /// <summary>
        /// 从网络上下载信息
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        public static async Task<HttpResult> DownLoadFromNet(Movie movie)
        {
            HttpResult httpResult = null;
            string message ="";
            
            if (movie.vediotype == (int)VedioType.欧美)
            {
                if (JvedioServers.BusEurope.IsEnable)
                    httpResult=await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl();
                //else if (supportServices.BusEu.IsProperUrl() && !serviceEnables.BusEu) 
                //    message = Jvedio.Language.Resources.UrlEuropeNotset;
            }
            else
            {
                //FC2 影片
                if (movie.id.ToUpper().IndexOf("FC2") >= 0)
                {
                    //优先从 db 下载
                    if (JvedioServers.DB.IsEnable)
                        httpResult = await new DBCrawler(movie.id).Crawl(); 
                    //else if (supportServices.DB.IsProperUrl() && !serviceEnables.DB) 
                    //    message = Jvedio.Language.Resources.UrlDBNotset;

                    //db 未下载成功则去 fc2官网
                    if (httpResult==null )
                    {
                        if (JvedioServers.FC2.IsEnable)
                            httpResult=await new FC2Crawler(movie.id).Crawl();
                        //else if (supportServices.FC2.IsProperUrl() && !serviceEnables.FC2)
                        //    message = Jvedio.Language.Resources.UrlFC2Notset;
                    }
                }
                else
                {
                    //非FC2 影片
                    //优先从 Bus 下载
                    if (JvedioServers.Bus.IsEnable)
                        httpResult = await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl();
                    //else if (supportServices.Bus.IsProperUrl() && !serviceEnables.Bus)
                    //    message = Jvedio.Language.Resources.UrlBusNotset;

                    //Bus 未下载成功则去 library
                    if (httpResult==null )
                    {
                        if (JvedioServers.Library.IsEnable)
                            httpResult=await new LibraryCrawler(movie.id).Crawl();
                        //else if (supportServices.Library.IsProperUrl() && !serviceEnables.Library)
                        //    message = Jvedio.Language.Resources.UrlLibraryNotset;
                    }

                    //library 未下载成功则去 DB
                    if (httpResult == null )
                    {
                        if (JvedioServers.DB.IsEnable)
                            httpResult=await new DBCrawler(movie.id).Crawl();
                        //else if (supportServices.DB.IsProperUrl() && !serviceEnables.DB)
                        //    message = Jvedio.Language.Resources.UrlDBNotset;
                    }

                    //DB未下载成功则去 FANZA
                    if (httpResult == null)
                    {
                        if (JvedioServers.DMM.IsEnable)
                            httpResult = await new FANZACrawler(movie.id).Crawl();
                        //else if (supportServices.DMM.IsProperUrl() && !serviceEnables.DMM)
                        //    message = Jvedio.Language.Resources.UrlDMMNotset;
                    }

                    //FANZA 未下载成功则去 MOO
                    if (httpResult == null)
                    {
                        if (JvedioServers.MOO.IsEnable)
                            httpResult = await new MOOCrawler(movie.id).Crawl();
                        //else if (supportServices.MOO.IsProperUrl() && !serviceEnables.MOO)
                        //    message = Jvedio.Language.Resources.UrlMOONotset;
                    }

                    //MOO 未下载成功则去 JAV321
                    if (httpResult == null)
                    {
                        if (JvedioServers.Jav321.IsEnable)
                            httpResult = await new Jav321Crawler(movie.id).Crawl();
                        //else if (supportServices.Jav321.IsProperUrl() && !serviceEnables.Jav321)
                        //    message = Jvedio.Language.Resources.UrlJAV321Notset;
                    }

                }

            }

            Movie newMovie = DataBase.SelectMovieByID(movie.id);
            if (newMovie != null && newMovie.title != "" && httpResult!=null) httpResult.Success = true;
            if (httpResult == null && message != "") httpResult = new HttpResult() { Error = message, Success = false };
            return httpResult;
        }
    }









    public static class Util
    {
        /// <summary>
        /// Sets the cert policy.
        /// </summary>
        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback
                       += RemoteCertificateValidate;
        }

        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
            X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!
            //System.Console.WriteLine("Warning, trust any certificate");
            return true;
        }
    }



}
