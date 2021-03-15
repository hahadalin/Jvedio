﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// IO、文件处理
    /// </summary>
    public static class FileProcess
    {

        public static Window GetWindowByName(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }
            return null;
        }


        public static void SaveInfo(Dictionary<string, string> Info,string id)
        {
            //保存信息
            if (!Info.ContainsKey("id")) Info.Add("id", id);
            DataBase.UpdateInfoFromNet(Info);
            DetailMovie detailMovie = DataBase.SelectDetailMovieById(id);
            SaveNfo(detailMovie);
        }

        public static void SavePartialInfo(Dictionary<string, string> Info,string key, string id)
        {
            //保存信息
            if (!Info.ContainsKey("id")) Info.Add("id", id);
            if (!Info.ContainsKey(key)) return;
            DataBase.UpdateMovieByID(id, key, Info[key], "String");
            DetailMovie detailMovie = DataBase.SelectDetailMovieById(id);
            SaveNfo(detailMovie);
        }

        public static string Unicode2String(string unicode)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         unicode, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }


        public static void SaveNfo(DetailMovie detailMovie)
        {
            if (!Properties.Settings.Default.SaveInfoToNFO) return;
            if (Directory.Exists(Properties.Settings.Default.NFOSavePath))
            {
                //固定位置
                string savepath = Path.Combine(Properties.Settings.Default.NFOSavePath, $"{detailMovie.id}.nfo");
                if (!File.Exists(savepath))
                    NFOHelper.SaveToNFO(detailMovie, savepath);
                else if (Properties.Settings.Default.OverriteNFO)
                    NFOHelper.SaveToNFO(detailMovie, savepath);
            }
            else
            {
                //与视频同路径，视频存在才行
                string path = detailMovie.filepath;
                if (File.Exists(path))
                {
                    string savepath = Path.Combine(new FileInfo(path).DirectoryName, $"{detailMovie.id}.nfo");
                    if(!File.Exists(savepath))
                        NFOHelper.SaveToNFO(detailMovie, savepath);
                    else if (Properties.Settings.Default.OverriteNFO)
                        NFOHelper.SaveToNFO(detailMovie, savepath);
                }
            }

        }


        /// <summary>
        /// 清除最近的观看记录
        /// </summary>
        /// <param name="dateTime"></param>
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

        /// <summary>
        /// 按照指定的条件筛选影片
        /// </summary>
        /// <param name="movies"></param>
        /// <returns></returns>
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

            result = FilterImage(result);
            return result;
        }

        /// <summary>
        /// 筛选有图、无图
        /// </summary>
        /// <param name="originMovies"></param>
        /// <returns></returns>

        private static List<Movie> FilterImage(List<Movie> originMovies)
        {
            List<Movie> result = new List<Movie>();
            result.AddRange(originMovies);

            
            int.TryParse(Properties.Settings.Default.ShowViewMode, out int idx);
            ViewType ShowViewMode = (ViewType)idx;
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


        public static Movie GetInfoFromNfo(string path)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(path);
            }
            catch(Exception ex) {
                Logger.LogE(ex);
                Console.WriteLine(ex.Message);
                return null; 
            }
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
            if (string.IsNullOrEmpty( movie.id )) return null; 

            //视频类型
            movie.vediotype = (int)Identify.GetVideoType(movie.id);

            //扫描视频获得文件大小
            if (File.Exists(path))
            {
                string fatherpath = new FileInfo(path).DirectoryName;
                string[] files = null;
                try { files = Directory.GetFiles(fatherpath, "*.*", SearchOption.TopDirectoryOnly); }
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
            List<string> tags = new List<string>();
            if (tagNodes != null)
            {
                foreach (XmlNode item in tagNodes)
                {
                    if (item.InnerText != "") { tags .Add( item.InnerText.Replace(" ","")); }
                }
                    if (movie.id.IndexOf("FC2") >= 0)
                        movie.genre = string.Join(" ", tags);
                    else
                        movie.tag = string.Join(" ", tags);
            }

            //genre
            XmlNodeList genreNodes = doc.SelectNodes("/movie/genre");
            List<string> genres = new List<string>();
            if (genreNodes != null)
            {
                foreach (XmlNode item in genreNodes)
                {
                    if (item.InnerText != "") { genres.Add( item.InnerText ); }

                }
                movie.genre = string.Join(" ", genres);
            }

            //actor
            XmlNodeList actorNodes = doc.SelectNodes("/movie/actor/name");
            List<string> actors = new List<string>();
            if (actorNodes != null)
            {
                foreach (XmlNode item in actorNodes)
                {
                    if (item.InnerText != "") { actors.Add( item.InnerText ); }
                }
                 movie.actor = string.Join(" ", actors);
                }

            //fanart
            XmlNodeList fanartNodes = doc.SelectNodes("/movie/fanart/thumb");
            List<string> extraimageurls = new List<string>();
            if (fanartNodes != null)
            {
                foreach (XmlNode item in fanartNodes)
                {
                    if (item.InnerText != "") { extraimageurls.Add( item.InnerText ); }
                }
                 movie.extraimageurl = string.Join(" ", extraimageurls);
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


        public static StringCollection ReadScanPathFromConfig(string name)
        {
            return new ScanPathConfig(name).Read();
        }


        public static void SaveScanPathToConfig(string name, List<string> paths)
        {
            ScanPathConfig scanPathConfig = new ScanPathConfig(name);
            scanPathConfig.Save(paths);
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
