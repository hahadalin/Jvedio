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
            RedirectGet = 1
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
                    throw new TimeoutException("超时！");
                }
            }
        }


        public static async Task<(string, int)> Http(string Url, string Method = "GET", HttpMode Mode = HttpMode.Normal, WebProxy Proxy = null, string Cookie = "",bool allowRedirect=true)
        {
            string HtmlText = "";
            int StatusCode = 404;
            int num = 0;

            while (num < ATTEMPTNUM & string.IsNullOrEmpty(HtmlText))
            {
                try
                {
                    HtmlText = await Task.Run(() =>
                    {
                        string result = "";
                        HttpWebRequest Request;
                        HttpWebResponse Response = default;
                        try
                        {
                            Request = (HttpWebRequest)HttpWebRequest.Create(Url);
                            if (Cookie != "") Request.Headers.Add("Cookie", Cookie);
                            Request.Accept = "*/*";
                            Request.Timeout = 50000;
                            Request.Method = Method;
                            Request.KeepAlive = false;
                            Request.AllowAutoRedirect = allowRedirect;
                            if (Mode == HttpMode.RedirectGet) Request.AllowAutoRedirect = false;
                            Request.Referer = Url;
                            Request.UserAgent = UserAgent;
                            Request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                            Request.ReadWriteTimeout = READWRITETIMEOUT;
                            if (Proxy != null) Request.Proxy = Proxy;
                            Response = (HttpWebResponse)Request.GetResponse();


                            StatusCode = (int)Response.StatusCode;
                            if (Response.StatusCode == HttpStatusCode.OK)
                            {
                                var SR = new StreamReader(Response.GetResponseStream());
                                result = SR.ReadToEnd();
                                SR.Close();
                                StatusCode = 200;
                            }
                            else if (Response.StatusCode == HttpStatusCode.Redirect && Mode == HttpMode.RedirectGet)
                            {
                                StatusCode=(int)Response.StatusCode;
                                result = Response.Headers["Location"];// 获得 library 影片 Code
                            }
                            else { num = 2; }
                            Response.Close();
                        }
                        catch (WebException e)
                        {
                            Logger.LogN($"地址：{Url}，失败原因：{e.Message}");
                            if (e.Status == WebExceptionStatus.Timeout)
                                num += 1;
                            else
                                num = 2;
                        }
                        catch (Exception e)
                        {
                            Logger.LogN($"地址：{Url}，失败原因：{e.Message}");
                            num = 2;
                        }
                        finally
                        {
                            if (Response != null) Response.Close();
                        }

                        return result;
                    }).TimeoutAfter(TimeSpan.FromSeconds(HTTPTIMEOUT));

                }
                catch (TimeoutException ex) { Logger.LogN($"地址：{Url}，失败原因：{ex.Message}"); num = 2; }
            }

            return (HtmlText, StatusCode);
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
                string content = ""; int statusCode = 404;
                if (EnableCookie)
                {
                    if (Label == "JavDB")
                    {
                        title = "JavDB";
                        (content, statusCode) = await Http(Url + "v/P2Rz9", Proxy: null, Cookie: Cookie);
                        if (content != "")
                        {
                            if (content.IndexOf("FC2-659341") >= 0) { result = true; title = "JavDB"; }
                            else result = false;
                        }
                    }else if (Label == "FANZA")
                    {
                        (content, statusCode) = await Http($"{Url}mono/dvd/-/search/=/searchstr=APNS-006/ ", Proxy: null, Cookie: Cookie);
                        if (statusCode == 200)
                        {
                            if (content.IndexOf("里美まゆ・川") >= 0) { result = true; title = "FANZA"; }
                            else result = false;
                        }
                    }
                }
                else
                {
                    (content, statusCode) = await Http(Url, Proxy: null);
                    if (content != "")
                    {
                        result = true;
                        //获得标题
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(content);
                        HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");
                        if (titleNode != null)
                        {
                            title = titleNode.InnerText;
                        }

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
                string content = ""; int statusCode = 404;
                if (EnableCookie)
                {
                    if (Label == "DB")
                    {
                        (content, statusCode) = await Http(Url + "v/P2Rz9", Proxy: null, Cookie: Cookie);
                        if (content != "")
                        {
                            if (content.IndexOf("FC2-659341") >= 0) result = true;
                            else result = false;
                        }
                    }
                }
                else
                {
                    (content, statusCode) = await Http(Url, Proxy: null);
                    if (content != "") result = true;
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
                Logger.LogN($"地址：{aDomain}，失败原因：{e.Message}");
            }
            return false;
        }
        public static (byte[] filebytes, string cookies, int statuscode) DownLoadFile(string Url, WebProxy Proxy = null, string SetCookie = "")
        {
            //if (!IsDomainAlive(new Uri(Url).Host, TCPTIMEOUT)) { Logger.LogN($"地址：{Url}，失败原因：Tcp链接超时"); return (null, ""); }
            Util.SetCertificatePolicy();
            byte[] ImageByte = null;
            string Cookies = SetCookie;
            int statuscode = 404;
            int num = 0;
            while (num < ATTEMPTNUM && ImageByte == null)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest Request;
                var Response = default(HttpWebResponse);
                try
                {
                    Request = (HttpWebRequest)HttpWebRequest.Create(Url);
                    Request.Host = new Uri(Url).Host;
                    if (Proxy != null) Request.Proxy = Proxy;
                    if (SetCookie != "") Request.Headers.Add("Cookie", SetCookie);
                    Request.Accept = "*/*";
                    Request.Timeout = FILE_REQUESTTIMEOUT;
                    Request.Method = "GET";
                    Request.KeepAlive = false;
                    Request.Referer = Url;
                    Request.UserAgent = UserAgent;
                    Request.ReadWriteTimeout = READWRITETIMEOUT;
                    Response = (HttpWebResponse)Request.GetResponse();
                    statuscode = (int)Response.StatusCode;
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            Response.GetResponseStream().CopyTo(ms);
                            ImageByte = ms.ToArray();
                        };
                        //获得 app_uid
                        WebHeaderCollection Headers = Response.Headers;
                        if (Headers != null & SetCookie == "")
                        {
                            if (Headers["Set-Cookie"] != null) Cookies = Headers["Set-Cookie"].Split(';')[0];
                            //Console.WriteLine(Cookies);
                        }
                    }
                    else
                    {
                        num = 2;
                    }
                    Response.Close();
                }
                catch (WebException e)
                {
                    HttpWebResponse res = (HttpWebResponse)e.Response;
                    if(res!=null) statuscode = (int)res.StatusCode;
                    Logger.LogN($"地址：{Url}，失败原因：{e.Message}");
                    if (e.Status == WebExceptionStatus.Timeout) { num += 1; } else { num = 2; }
                }
                catch (Exception e)
                {
                    Logger.LogE(e);
                    num = 2;
                }
                finally { Response?.Close(); }
            }
            return (ImageByte, Cookies,statuscode);
        }



        /// <summary>
        /// 异步下载图片
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="imageType"></param>
        /// <param name="ID"></param>
        /// <param name="Cookie"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> DownLoadImage(string Url, ImageType imageType, string ID, string Cookie = "",Action<int> callback=null)
        {
            if (!Url.IsProperUrl()) return (false, ""); 
            bool result = false; 
            string cookies = Cookie;
            int statuscode = 404;
            byte[] ImageBytes = null;
            (ImageBytes, cookies, statuscode) = await Task.Run(() =>
            {
                (ImageBytes, cookies, statuscode) = DownLoadFile(Url.Replace("\"", "'"), SetCookie: cookies);
                return (ImageBytes, cookies, statuscode);
            });


            if (ImageBytes == null) {
                Logger.LogN($"图片下载失败：{Url}");
                callback?.Invoke(statuscode);
                result = false; 
            }
            else
            {
                result = true;
                ImageProcess.SaveImage(ID, ImageBytes, imageType, Url);
            }
            return (result, cookies);
        }



        public static async Task<bool> DownActress(string ID, string Name,Action<string> callback)
        {
            bool result = false;
            string Url = RootUrl.Bus + $"star/{ID}";
            string Content; int StatusCode; string ResultMessage;
            (Content, StatusCode) = await Http(Url);
            if (StatusCode == 200 && Content != "")
            {
                //id搜索
                BusParse busParse = new BusParse(ID, Content, VedioType.骑兵);
                Actress actress = busParse.ParseActress();
                if (actress ==null && string.IsNullOrEmpty(actress.birthday)  && actress.age == 0 && string.IsNullOrEmpty(actress.birthplace))
                { 
                    ResultMessage = $"该网址无演员信息：{Url}";
                    callback.Invoke(ResultMessage);
                    Logger.LogN($"URL={Url},Message-{ResultMessage}"); 
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
            else { 
                Console.WriteLine($"无法访问 404：{Url}"); 
                ResultMessage = "Bus 无法访问";
                callback.Invoke(ResultMessage);
                Logger.LogN($"URL={Url},Message-{ResultMessage}"); }
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
            string content = "";
            int StatusCode = 404;
            if (webSite==WebSite.DB) (content, StatusCode) = await Net.Http(url, Cookie: Properties.Settings.Default.DBCookie);
            if (webSite == WebSite.DMM) (content, StatusCode) = await Net.Http(url, Cookie: Properties.Settings.Default.DMMCookie);
            else ( content,  StatusCode) = await Net.Http(url);

            if(StatusCode!=200 || content == "")
            {
                return false;
            }
            else
            {
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
                    //保存信息
                    Info["id"] = id;
                    DataBase.UpdateInfoFromNet(Info);
                    DetailMovie detailMovie = DataBase.SelectDetailMovieById(id);


                    //nfo 信息保存到视频同目录
                    if (Properties.Settings.Default.SaveInfoToNFO)
                    {
                        if (Directory.Exists(Properties.Settings.Default.NFOSavePath))
                        {
                            //固定位置
                            nfo.SaveToNFO(detailMovie, Path.Combine(Properties.Settings.Default.NFOSavePath, $"{id}.nfo"));
                        }
                        else
                        {
                            //与视频同路径
                            string path = detailMovie.filepath;
                            if (System.IO.File.Exists(path))
                            {
                                nfo.SaveToNFO(detailMovie, Path.Combine(new FileInfo(path).DirectoryName, $"{id}.nfo"));
                            }
                        }
                    }
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
        public static async Task<(bool, string)> DownLoadFromNet(Movie movie)
        {
            bool success = false;
            string message = "网址未配置";
            Movie newMovie;
            if (movie.vediotype == (int)VedioType.欧美)
            {
                if (RootUrl.BusEu.IsProperUrl() && EnableUrl.BusEu) await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl((statuscode)=> { message = statuscode.ToString(); } );
                else if (RootUrl.BusEu.IsProperUrl() && !EnableUrl.BusEu) message = "未开启欧美网址";
            }
            else
            {
                if (movie.id.ToUpper().IndexOf("FC2") >= 0)
                {
                    //优先从 db 下载
                    if (RootUrl.DB.IsProperUrl() && EnableUrl.DB) {  await new DBCrawler(movie.id).Crawl((statuscode) => { message = statuscode.ToString(); }, (statuscode) => { message = statuscode.ToString(); }); }
                    else if (RootUrl.DB.IsProperUrl() && !EnableUrl.DB) message = "未开启 DB 网址";

                    //db 未下载成功则去 fc2官网
                    newMovie = DataBase.SelectMovieByID(movie.id);
                    if (IsToDownLoadInfo(newMovie))
                    {
                        if (!string.IsNullOrEmpty(RootUrl.FC2) && EnableUrl.FC2) { await new FC2Crawler(movie.id).Crawl((statuscode) => { message = statuscode.ToString(); }, (statuscode) => { message = statuscode.ToString(); }); }
                    }
                }
                else
                {
                    //优先从 Bus 下载
                    if (RootUrl.Bus.IsProperUrl() && EnableUrl.Bus) await new BusCrawler(movie.id, (VedioType)movie.vediotype).Crawl((statuscode) => { message = statuscode.ToString(); });

                    //Bus 未下载成功则去 library
                    newMovie = DataBase.SelectMovieByID(movie.id);
                    if (IsToDownLoadInfo(newMovie))
                    {
                        if (RootUrl.Library.IsProperUrl() && EnableUrl.Library) { await new LibraryCrawler(movie.id).Crawl((statuscode) => { message = statuscode.ToString(); },(statuscode) => { message = statuscode.ToString(); }); }
                    }


                    //library 未下载成功则去 DB
                    newMovie = DataBase.SelectMovieByID(movie.id);
                    if (IsToDownLoadInfo(newMovie))
                    {
                        if (RootUrl.DB.IsProperUrl() && EnableUrl.DB)  await new DBCrawler(movie.id).Crawl((statuscode) => { message = statuscode.ToString(); },(statuscode) => { message = statuscode.ToString(); });
                    }

                    //DB 未下载成功则去 FANZA
                    //newMovie = DataBase.SelectMovieByID(movie.id);
                    //if (StaticClass.IsToDownLoadInfo(newMovie))
                    //{
                    //    if (RootUrl.DMM.IsProperUrl() && EnableUrl.DMM) await new DMMCrawler(movie.id).Crawl((statuscode) => { message = statuscode.ToString(); }, (statuscode) => { message = statuscode.ToString(); });
                    //    else if (RootUrl.DMM.IsProperUrl() && !EnableUrl.DMM) message = "未开启 FANZA 网址";
                    //}

                }

            }

            newMovie = DataBase.SelectMovieByID(movie.id);
            if (newMovie != null && newMovie.title != "")
                success = true;
            else
                success = false;

            return (success,message);
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
