using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;


namespace Jvedio
{


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








        public static async Task<HttpResult> Http(string Url, CrawlerHeader headers = null, HttpMode Mode = HttpMode.Normal, WebProxy Proxy = null,bool allowRedirect=true)
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
                        Request.Host = headers.Host == "" ? new Uri(Url).Host : headers.Host;
                        Request.Accept = headers.Accept;
                        Request.Timeout = 50000;
                        Request.Method = headers.Method;
                        Request.KeepAlive = headers.Connection == "keep-alive";
                        Request.AllowAutoRedirect = allowRedirect;
                        Request.Referer = Url;
                        Request.UserAgent = headers.UserAgent;
                        Request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");//限定为中文，解析的时候就是中文
                        Request.ReadWriteTimeout = READWRITETIMEOUT;
                        if (headers.Cookies != "") Request.Headers.Add("Cookie", headers.Cookies);
                        if (Mode == HttpMode.RedirectGet) Request.AllowAutoRedirect = false;
                        if (Proxy != null) Request.Proxy = Proxy;

                        try
                        {
                            Response = (HttpWebResponse)Request.GetResponse();
                           httpResult = GetHttpResult(Response, Mode);

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
                SetCookie = webHeaderCollection.Get("Set-Cookie")
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
                }else if (mode == HttpMode.Stream)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Response.GetResponseStream().CopyTo(ms);
                        httpResult.FileByte = ms.ToArray();
                    }
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
                else
                {
                    webSite = WebSite.None;
                }
            }
            return webSite;
        }


        public static async Task<(bool, string)> TestAndGetTitle(string Url, bool EnableCookie, string Cookie, string Label)
        {
            return await Task.Run(async () =>
            {
                bool result = false;
                string title = "";
                HttpResult httpResult = null;
                if (EnableCookie)
                {
                    if (Label == "JavDB")
                    {
                        httpResult = await Http(Url + "v/P2Rz9", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult!=null  && httpResult.SourceCode.IndexOf("FC2-659341") >= 0)
                        {
                                result = true; 
                                title = "JavDB"; 
                        }
                        else result = false;
                    }
                    else if (Label == "FANZA")
                    {
                        httpResult = await Http($"{Url}mono/dvd/-/search/=/searchstr=APNS-006/ ", new CrawlerHeader() { Cookies = Cookie });
                        if (httpResult != null && httpResult.SourceCode.IndexOf("里美まゆ・川") >= 0)
                        {
                            result = true; 
                            title = "FANZA"; 
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
            });
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
                cookie = httpResult.Headers.SetCookie.Split(';')[0];
                result = true;
                ImageProcess.SaveImage(ID, httpResult.FileByte, imageType, Url);
            }
            return (result, cookie);
        }



        public static async Task<bool> DownActress(string ID, string Name,Action<string> callback)
        {
            bool result = false;
            string Url = RootUrl.Bus + $"star/{ID}";
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
            if (webSite==WebSite.DB) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies= Properties.Settings.Default.DBCookie } );
            if (webSite == WebSite.DMM) httpResult = await Net.Http(url, new CrawlerHeader() { Cookies = Properties.Settings.Default.DMMCookie });
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
        public static async Task<HttpResult> DownLoadFromNet(Movie movie,bool forceToDownload=false)
        {
            HttpResult httpResult = null;
            string message ="";
            
            if (movie.vediotype == (int)VedioType.欧美)
            {
                if (RootUrl.BusEu.IsProperUrl() && EnableUrl.BusEu)
                    httpResult=await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl();
                else if (RootUrl.BusEu.IsProperUrl() && !EnableUrl.BusEu) 
                    message = Jvedio.Language.Resources.UrlEuropeNotset;
            }
            else
            {
                //FC2 影片
                if (movie.id.ToUpper().IndexOf("FC2") >= 0)
                {
                    //优先从 db 下载
                    if (RootUrl.DB.IsProperUrl() && EnableUrl.DB)
                        httpResult = await new DBCrawler(movie.id).Crawl(); 
                    else if (RootUrl.DB.IsProperUrl() && !EnableUrl.DB) 
                        message = Jvedio.Language.Resources.UrlDBNotset;

                    //db 未下载成功则去 fc2官网
                    if (httpResult!=null && !httpResult.Success)
                    {
                        if (RootUrl.FC2.IsProperUrl() && EnableUrl.FC2)
                            httpResult=await new FC2Crawler(movie.id).Crawl();
                        else if (RootUrl.FC2.IsProperUrl() && !EnableUrl.FC2)
                            message = Jvedio.Language.Resources.UrlFC2Notset;
                    }
                }
                else
                {
                    //非FC2 影片
                    //优先从 Bus 下载
                    if (RootUrl.Bus.IsProperUrl() && EnableUrl.Bus)
                        httpResult = await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl();
                    else if (RootUrl.Bus.IsProperUrl() && !EnableUrl.Bus)
                        message = Jvedio.Language.Resources.UrlBusNotset;

                    //Bus 未下载成功则去 library
                    if (httpResult!=null && !httpResult.Success)
                    {
                        if (RootUrl.Library.IsProperUrl() && EnableUrl.Library)
                            httpResult=await new LibraryCrawler(movie.id).Crawl();
                        else if (RootUrl.Library.IsProperUrl() && !EnableUrl.Library)
                            message = Jvedio.Language.Resources.UrlLibraryNotset;
                    }

                    //library 未下载成功则去 DB
                    if (httpResult != null && !httpResult.Success)
                    {
                        if (RootUrl.DB.IsProperUrl() && EnableUrl.DB)  
                            await new DBCrawler(movie.id).Crawl();
                        else if (RootUrl.DB.IsProperUrl() && !EnableUrl.DB)
                            message = Jvedio.Language.Resources.UrlDBNotset;
                    }

                }

            }

            Movie newMovie = DataBase.SelectMovieByID(movie.id);
            if (newMovie != null && newMovie.title != "" && httpResult!=null) httpResult.Success = true;
            return httpResult;
        }

        public static bool IsToDownLoadInfo(Movie movie)
        {
            return movie != null && (movie.title == "" || movie.sourceurl == "" || movie.smallimageurl == "" || movie.bigimageurl == "");
        }


        /// <summary>
        /// 检查是否启用服务器源且地址不为空
        /// </summary>
        /// <returns></returns>
        public static bool IsServersProper()
        {
            bool result = Properties.Settings.Default.Enable321 && !string.IsNullOrEmpty(Properties.Settings.Default.Jav321)
                                || Properties.Settings.Default.EnableBus && !string.IsNullOrEmpty(Properties.Settings.Default.Bus)
                                || Properties.Settings.Default.EnableBusEu && !string.IsNullOrEmpty(Properties.Settings.Default.BusEurope)
                                || Properties.Settings.Default.EnableLibrary && !string.IsNullOrEmpty(Properties.Settings.Default.Library)
                                || Properties.Settings.Default.EnableDB && !string.IsNullOrEmpty(Properties.Settings.Default.DB)
                                || Properties.Settings.Default.EnableFC2 && !string.IsNullOrEmpty(Properties.Settings.Default.FC2)
                                || Properties.Settings.Default.EnableDMM && !string.IsNullOrEmpty(Properties.Settings.Default.DMM);

            return result;
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
