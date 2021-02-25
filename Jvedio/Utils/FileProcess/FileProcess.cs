using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    public static class FileProcess
    {


        public static void SaveInfo(Dictionary<string, string> Info,string id)
        {
            //保存信息
            if (!Info.ContainsKey("id")) Info.Add("id", id);
            DataBase.UpdateInfoFromNet(Info);
            DetailMovie detailMovie = DataBase.SelectDetailMovieById(id);
            FileProcess.SaveNfo(detailMovie);
        }

        public static void SaveNfo(DetailMovie detailMovie)
        {
            if (!Properties.Settings.Default.SaveInfoToNFO) return;

            if (Directory.Exists(Properties.Settings.Default.NFOSavePath))
            {
                //固定位置
                nfo.SaveToNFO(detailMovie, Path.Combine(Properties.Settings.Default.NFOSavePath, $"{detailMovie.id}.nfo"));
            }
            else
            {
                //与视频同路径
                string path = detailMovie.filepath;
                if (File.Exists(path))
                {
                    nfo.SaveToNFO(detailMovie, Path.Combine(new FileInfo(path).DirectoryName, $"{detailMovie.id}.nfo"));
                }
            }

        }


        public static void ClearDateBefore(DateTime dateTime)
        {
            if (!File.Exists("RecentWatch")) return;
            RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig();
            for (int i = 1; i < 60; i++)
            {
                DateTime date = dateTime.AddDays(-1 * i);
                recentWatchedConfig.Remove(date);
            }

        }
        public static List<Movie> FilterMovie(List<Movie> movies )
        {
            List<Movie> result = new List<Movie>();
            result.AddRange(movies);
            //可播放|不可播放
            if (Properties.Settings.Default.OnlyShowPlay)
            {
                foreach (var item in movies)
                {
                    if (!File.Exists((item.filepath))) result.Remove(item);
                }
            }

            //分段|不分段
            if (Properties.Settings.Default.OnlyShowSubSection)
            {
                foreach (var item in movies)
                {
                    if (item.subsectionlist.Count <= 1) result.Remove(item);
                }
            }

            //视频类型
            int.TryParse(Properties.Settings.Default.VedioType, out int vt);
            if (vt > 0)
            {
                result = result.Where(arg => arg.vediotype == vt).ToList();
            }

            result = FilterImage(result);//有图|无图
            return result;
        }

        private static List<Movie> FilterImage(List<Movie> originMovies)
        {
            List<Movie> result = new List<Movie>();
            result.AddRange(originMovies);

            ViewType ShowViewMode = ViewType.默认;
            Enum.TryParse(Properties.Settings.Default.ShowViewMode, out ShowViewMode);
            MyImageType ShowImageMode = MyImageType.缩略图;
            if (Properties.Settings.Default.ShowImageMode.Length == 1)
            {
                ShowImageMode = (MyImageType)(int.Parse(Properties.Settings.Default.ShowImageMode));
            }


            if (ShowViewMode == ViewType.有图)
            {
                foreach (var item in originMovies)
                {
                    if (ShowImageMode == MyImageType.缩略图)
                    {
                        if (!File.Exists(BasePicPath + $"SmallPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.海报图)
                    {
                        if (!File.Exists(BasePicPath + $"BigPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.动态图)
                    {
                        if (!File.Exists(BasePicPath + $"Gif\\{item.id}.gif")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.预览图)
                    {
                        if (!Directory.Exists(BasePicPath + $"ExtraPic\\{item.id}\\")) { result.Remove(item); }
                        else
                        {
                            try { if (Directory.GetFiles(BasePicPath + $"ExtraPic\\{item.id}\\", "*.*", SearchOption.TopDirectoryOnly).Count() == 0) result.Remove(item); }
                            catch { }
                        }
                    }
                }


            }
            else if (ShowViewMode == ViewType.无图)
            {
                foreach (var item in originMovies)
                {
                    if (ShowImageMode == MyImageType.缩略图)
                    {
                        if (File.Exists(BasePicPath + $"SmallPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.海报图)
                    {
                        if (File.Exists(BasePicPath + $"BigPic\\{item.id}.jpg")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.动态图)
                    {
                        if (File.Exists(BasePicPath + $"Gif\\{item.id}.gif")) { result.Remove(item); }
                    }

                    else if (ShowImageMode == MyImageType.预览图)
                    {
                        if (Directory.Exists(BasePicPath + $"ExtraPic\\{item.id}\\"))
                        {
                            try { if (Directory.GetFiles(BasePicPath + $"ExtraPic\\{item.id}\\", "*.*", SearchOption.TopDirectoryOnly).Count() > 0) result.Remove(item); }
                            catch { }
                        }
                    }
                }
            }
            return result;
        }





        /// <summary>
        /// 判断拖入的是文件夹还是文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFile(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return false;
                else
                    return true;
            }
            catch
            {
                return true;
            }

        }


        public static bool IsLetter(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) return true;
            else return false;
        }


        public static Movie GetInfoFromNfo(string path)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch { return null; }
            XmlNode rootNode = doc.SelectSingleNode("movie");
            if (rootNode == null) return null;
            Movie movie = new Movie();
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                try
                {
                    switch (node.Name)
                    {
                        case "id": movie.id = node.InnerText.ToUpper(); break;
                        case "num": movie.id = node.InnerText.ToUpper(); break;
                        case "title": movie.title = node.InnerText; break;
                        case "release": movie.releasedate = node.InnerText; break;
                        case "releasedate": movie.releasedate = node.InnerText; break;
                        case "director": movie.director = node.InnerText; break;
                        case "studio": movie.studio = node.InnerText; break;
                        case "rating": movie.rating = node.InnerText == "" ? 0 : float.Parse(node.InnerText); break;
                        case "plot": movie.plot = node.InnerText; break;
                        case "outline": movie.outline = node.InnerText; break;
                        case "year": movie.year = node.InnerText == "" ? 1970 : int.Parse(node.InnerText); break;
                        case "runtime": movie.runtime = node.InnerText == "" ? 0 : int.Parse(node.InnerText); break;
                        case "country": movie.country = node.InnerText; break;
                        case "source": movie.sourceurl = node.InnerText; break;
                        default: break;

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
            if (movie.id == "") { return null; }
            //视频类型

            movie.vediotype = (int)Identify.GetVedioType(movie.id);

            //扫描视频获得文件大小
            if (File.Exists(path))
            {
                string fatherpath = new FileInfo(path).DirectoryName;
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(fatherpath, "*.*", SearchOption.TopDirectoryOnly);
                }
                catch (Exception e)
                {
                    Logger.LogE(e);
                }

                if (files != null)
                {

                    var movielist = Scan.FirstFilter(files.ToList(), movie.id);
                    if (movielist.Count == 1 && !movielist[0].ToLower().EndsWith(".nfo"))
                    {
                        movie.filepath = movielist[0];
                    }
                    else if (movielist.Count > 1)
                    {
                        //分段视频
                        movie.filepath = movielist[0];
                        string subsection = "";
                        movielist.ForEach(arg => { subsection += arg + ";"; });
                        movie.subsection = subsection;
                    }
                }



            }

            //tag
            XmlNodeList tagNodes = doc.SelectNodes("/movie/tag");
            if (tagNodes != null)
            {
                string tags = "";
                foreach (XmlNode item in tagNodes)
                {
                    if (item.InnerText != "") { tags += item.InnerText + " "; }

                }
                if (tags.Length > 0)
                {

                    if (movie.id.IndexOf("FC2") >= 0)
                    {
                        movie.genre = tags.Substring(0, tags.Length - 1);
                    }
                    else
                    {
                        movie.tag = tags.Substring(0, tags.Length - 1);
                    }


                }
            }

            //genre
            XmlNodeList genreNodes = doc.SelectNodes("/movie/genre");
            if (genreNodes != null)
            {
                string genres = "";
                foreach (XmlNode item in genreNodes)
                {
                    if (item.InnerText != "") { genres += item.InnerText + " "; }

                }
                if (genres.Length > 0) { movie.genre = genres.Substring(0, genres.Length - 1); }
            }

            //actor
            XmlNodeList actorNodes = doc.SelectNodes("/movie/actor/name");
            if (actorNodes != null)
            {
                string actors = "";
                foreach (XmlNode item in actorNodes)
                {
                    if (item.InnerText != "") { actors += item.InnerText + " "; }
                }
                if (actors.Length > 0) { movie.actor = actors.Substring(0, actors.Length - 1); }
            }

            //fanart
            XmlNodeList fanartNodes = doc.SelectNodes("/movie/fanart/thumb");
            if (fanartNodes != null)
            {
                string extraimageurl = "";
                foreach (XmlNode item in fanartNodes)
                {
                    if (item.InnerText != "") { extraimageurl += item.InnerText + ";"; }
                }
                if (extraimageurl.Length > 0) { movie.extraimageurl = extraimageurl.Substring(0, extraimageurl.Length - 1); }
            }


            return movie;
        }

        public static List<string> LabelToList(string label)
        {

            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(label)) return result;
            if (label.IndexOf(' ') > 0)
            {
                foreach (var item in label.Split(' '))
                {
                    if (item.Length > 0)
                        if (!result.Contains(item)) result.Add(item);
                }
            }
            else { if (label.Length > 0) result.Add(label.Replace(" ", "")); }
            return result;
        }

        public static void ByteArrayToFile(byte[] byteArray, string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }


        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        public static string GetFileMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        public static void addTag(ref Movie movie)
        {
            //添加标签戳
            if (movie == null) return;
            if (Identify.IsHDV(movie.filepath) || movie.genre?.IndexOfAnyString(TagStrings_HD) >= 0 || movie.tag?.IndexOfAnyString(TagStrings_HD) >= 0 || movie.label?.IndexOfAnyString(TagStrings_HD) >= 0) movie.tagstamps +=Jvedio.Language.Resources.HD;
            if (Identify.IsCHS(movie.filepath) || movie.genre?.IndexOfAnyString(TagStrings_Translated) >= 0 || movie.tag?.IndexOfAnyString(TagStrings_Translated) >= 0 || movie.label?.IndexOfAnyString(TagStrings_Translated) >= 0) movie.tagstamps += Jvedio.Language.Resources.Translated;
            if (Identify.IsFlowOut(movie.filepath) || movie.genre?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || movie.tag?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || movie.label?.IndexOfAnyString(TagStrings_FlowOut) >= 0) movie.tagstamps += Jvedio.Language.Resources.FlowOut;
        }

        public static void addTag(ref DetailMovie movie)
        {
            //添加标签戳
            if (Identify.IsHDV(movie.filepath) || movie.genre?.IndexOfAnyString(TagStrings_HD) >= 0 || movie.tag?.IndexOfAnyString(TagStrings_HD) >= 0 || movie.label?.IndexOfAnyString(TagStrings_HD) >= 0) movie.tagstamps += Jvedio.Language.Resources.HD;
            if (Identify.IsCHS(movie.filepath) || movie.genre?.IndexOfAnyString(TagStrings_Translated) >= 0 || movie.tag?.IndexOfAnyString(TagStrings_Translated) >= 0 || movie.label?.IndexOfAnyString(TagStrings_Translated) >= 0) movie.tagstamps += Jvedio.Language.Resources.Translated;
            if (Identify.IsFlowOut(movie.filepath) || movie.genre?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || movie.tag?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || movie.label?.IndexOfAnyString(TagStrings_FlowOut) >= 0) movie.tagstamps += Jvedio.Language.Resources.FlowOut;
        }


        #region "配置xml"

        /// <summary>
        /// 读取原有的 config.ini到 xml
        /// </summary>
        public static void SaveScanPathToXml()
        {
            if (!File.Exists("DataBase\\Config.ini")) return;
            Dictionary<string, StringCollection> DataBases = new Dictionary<string, StringCollection>();
            using (StreamReader sr = new StreamReader(DataBaseConfigPath))
            {
                try
                {
                    string content = sr.ReadToEnd();
                    List<string> info = content.Split('\n').ToList();
                    info.ForEach(arg => {
                        string name = arg.Split('?')[0];
                        StringCollection stringCollection = new StringCollection();
                        arg.Split('?')[1].Split('*').ToList().ForEach(path => { if (!string.IsNullOrEmpty(path)) stringCollection.Add(path); });
                        if (!DataBases.ContainsKey(name)) DataBases.Add(name, stringCollection);
                    });
                }
                catch { }
            }
            foreach (var item in DataBases)
            {
                SaveScanPathToConfig(item.Key, item.Value.Cast<string>().ToList());
            }
            File.Delete("DataBase\\Config.ini");
        }

        public static StringCollection ReadScanPathFromConfig(string name)
        {
            return new ScanPathConfig(name).Read();
        }


        public static void SaveScanPathToConfig(string name, List<string> paths)
        {
            ScanPathConfig scanPathConfig = new ScanPathConfig(name);
            scanPathConfig.Save(paths);
        }


        /// <summary>
        /// 读取原有的 config.ini到 xml
        /// </summary>
        public static void SaveServersToXml()
        {
            if (!File.Exists("ServersConfig.ini")) return;
            Dictionary<WebSite, Dictionary<string, string>> Servers = new Dictionary<WebSite, Dictionary<string, string>>();
            using (StreamReader sr = new StreamReader("ServersConfig.ini"))
            {
                try
                {
                    string content = sr.ReadToEnd();
                    List<string> rows = content.Split('\n').ToList();
                    rows.ForEach(arg => {
                        string name = arg.Split('?')[0];
                        WebSite webSite = WebSite.None;
                        Enum.TryParse<WebSite>(name, out webSite);
                        string[] infos = arg.Split('?')[1].Split('*');
                        Dictionary<string, string> info = new Dictionary<string, string>
                        {
                            { "Url", infos[0] },
                            { "ServerName", infos[1] },
                            { "LastRefreshDate", infos[2] }
                        };
                        if (!Servers.ContainsKey(webSite)) Servers.Add(webSite, info);
                    });
                }
                catch { }
            }
            foreach (var item in Servers)
            {
                SaveServersInfoToConfig(item.Key, item.Value.Values.ToList<string>());
            }
            File.Delete("ServersConfig.ini");
        }

        public static void SaveServersInfoToConfig(WebSite webSite, List<string> infos)
        {
            Dictionary<string, string> info = new Dictionary<string, string>
            {
                { "Url", infos[0] },
                { "ServerName", infos[1] },
                { "LastRefreshDate", infos[2] }
            };

            ServerConfig serverConfig = new ServerConfig(webSite.ToString());
            serverConfig.Save(info);
        }

        public static void DeleteServerInfoFromConfig(WebSite webSite)
        {

            if (!File.Exists("ServersConfig.ini")) return;
            ServerConfig serverConfig = new ServerConfig(webSite.ToString());
            serverConfig.Delete();
        }

        public static List<string> ReadServerInfoFromConfig(WebSite webSite)
        {

            if (!File.Exists("ServersConfig")) return new List<string>() { webSite.ToString(), "", "" };


            List<string> result = new ServerConfig(webSite.ToString()).Read();
            if (result.Count < 3) result = new List<string>() { webSite.ToString(), "", "" };
            return result;
        }



        //最近观看

        public static void SaveRecentWatchedToXml()
        {
            if (!File.Exists("RecentWatch.ini")) return;
            Dictionary<string, List<string>> RecentWatchedes = new Dictionary<string, List<string>>();
            using (StreamReader sr = new StreamReader("RecentWatch.ini"))
            {
                try
                {
                    string content = sr.ReadToEnd();
                    List<string> rows = content.Split('\n').ToList();
                    rows.ForEach(arg => {
                        string date = arg.Split(':')[0];
                        var IDs = arg.Split(':')[1].Split(',').ToList();
                        if (!RecentWatchedes.ContainsKey(date)) RecentWatchedes.Add(date, IDs);
                    });
                }
                catch { }
            }
            foreach (var item in RecentWatchedes)
            {
                SaveRecentWatchedToConfig(item.Key, item.Value);
            }
            File.Delete("RecentWatch.ini");
        }

        public static void SaveRecentWatchedToConfig(string date, List<string> IDs)
        {
            RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig(date);
            recentWatchedConfig.Save(IDs);
        }

        public static void ReadRecentWatchedFromConfig()
        {
            if (!File.Exists("RecentWatch")) return;
            RecentWatched = new RecentWatchedConfig("").Read();
        }


        public static void SaveRecentWatched()
        {
            foreach (var keyValuePair in RecentWatched)
            {
                if (keyValuePair.Key <= DateTime.Now && keyValuePair.Key >= DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays))
                {
                    if (keyValuePair.Value.Count > 0)
                    {
                        List<string> IDs = keyValuePair.Value.Where(arg => !string.IsNullOrEmpty(arg)).ToList();

                        string date = keyValuePair.Key.Date.ToString("yyyy-MM-dd");
                        RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig(date);
                        recentWatchedConfig.Save(IDs);
                    }
                }
            }






        }


        #endregion

    }
}
