using Jvedio.Plot.Bar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static Jvedio.GlobalVariable;
using System.Windows.Media.Imaging;

namespace Jvedio
{
    public static class CustomExtension
    {
        public static T GetQueryOrDefault<T>(this BitmapMetadata metadata, string query, T defaultValue)
        {
            if (metadata.ContainsQuery(query))
                return (T)Convert.ChangeType(metadata.GetQuery(query), typeof(T));
            return defaultValue;
        }

        public static bool IsIntersectWith(this ObservableCollection<string> collections, string str)
        {
            foreach (var item in collections)
            {
                if (item.IndexOf(str) >= 0 || str.IndexOf(item) >= 0) return true;
            }

            return false;
        }

        public static int IndexOfAnyString(this string str, string[] parameter)
        {
            foreach (var item in parameter)
            {
                int idx = str.IndexOf(item,StringComparison.CurrentCultureIgnoreCase);
                if (idx >= 0) return idx;
            }
            return -1;
        }


        public static string ToTagString(this string str)
        {
            if (str.Length != 1) return "";
            if (str == "0" ) return  Jvedio.Language.Resources.HD ;
            if (str == "1" ) return Jvedio.Language.Resources.Translated;
            if (str == "2"  ) return Jvedio.Language.Resources.FlowOut;
            return str;
        }


        public static string ToSqlString(this string str)
        {
            string result="";

            if (str == "标签")
            {
                result = "label";
            }else if (str == "系列")
            {
                result = "tag";
            }
            else if (str == "发行商")
            {
                result = "studio";
            }
            else if (str == "导演")
            {
                result = "director";
            }

            return result;


        }

        public static string ToSqlString(this Sort sort)
        {
            string result;
            if (sort == Sort.识别码)
            {
                result = "id";
            }
            else if (sort == Sort.文件大小)
            {
                result = "filesize";
            }
            else if (sort == Sort.导入时间)
            {
                result = "otherinfo";
            }
            else if (sort == Sort.创建时间)
            {
                result = "scandate";
            }
            else if (sort == Sort.喜爱程度)
            {
                result = "favorites";
            }
            else if (sort == Sort.名称)
            {
                result = "title";
            }
            else if (sort == Sort.访问次数)
            {
                result = "visits";
            }
            else if (sort == Sort.发行日期)
            {
                result = "releasedate";
            }
            else if (sort == Sort.评分)
            {
                result = "rating";
            }
            else if (sort == Sort.时长)
            {
                result = "runtime";
            }
            else if (sort == Sort.演员)
            {
                result = "actor";
            }
            else
            {
                result = "id";
            }

            return result;


        }


        public static List<BarData> ToBarDatas(this Dictionary<string, double> dicSort)
        {
            List<BarData> result = new List<BarData>();
            foreach (var item in dicSort)
            {
                result.Add(new BarData()
                {
                    Value = item.Value,
                    ActualValue = item.Value,
                    Key = item.Key
                });
            }

            return result;
        }




        /// <summary>
        /// 根据文件的实际数字大小排序而不是 1,10,100,1000
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<string> CustomSort(this IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();

            return list.Select(s => new
            {
                OrgStr = s,
                SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, char.IsDigit(m.Value[0]) ? ' ' : '\xffff'))
            })
            .OrderBy(x => x.SortStr)
            .Select(x => x.OrgStr);
        }

        
        public static string ToJav321(this string ID)
        {
            ID = ID.ToUpper();
            if (Jav321IDDict.ContainsKey(ID))
                return Jav321IDDict[ID];
            else
                return ID;

        }
        public static string ToProperSql(this string sql)
        {
            return sql.Replace("%", "").Replace("'", "").ToUpper();
        }

        public static string ToProperUrl(this string url)
        {
            url = url.ToLower();
            if (string.IsNullOrEmpty(url)) return "";
            if (url.IndexOf("http") < 0) url = "https://" + url;
            if (!url.EndsWith("/")) url += "/";
            return url;
        }




        public static string ToStatusMessage(this string status)
        {
            switch (status)
            {
                case "403":
                    return Jvedio.Language.Resources.NotShowInCountry;
                case "404":
                    return Jvedio.Language.Resources.NoID;
                case "504":
                    return Jvedio.Language.Resources.TimeOut;
                case "302":
                    return Jvedio.Language.Resources.TooFrequent;
                default:
                    return status;
            }
        }

        public static bool IsProperUrl(this string source) => Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        public static string ToSqlField(this string content)
        {
            if (content == Jvedio.Language.Resources.ID)
            {
                return "id";
            }else if (content == Jvedio.Language.Resources.Title)
            {
                return "title";
            }
            else if (content == Jvedio.Language.Resources.TranslatedTitle)
            {
                return "chinesetitle";
            }
            else if (content == Jvedio.Language.Resources.VedioType)
            {
                return "vediotype";
            }
            else if (content == Jvedio.Language.Resources.ReleaseDate)
            {
                return "releasedate";
            }
            else if (content == Jvedio.Language.Resources.Year)
            {
                return "year";
            }
            else if (content == Jvedio.Language.Resources.Duration)
            {
                return "runtime";
            }
            else if (content == Jvedio.Language.Resources.Country)
            {
                return "country";
            }
            else if (content == Jvedio.Language.Resources.Director)
            {
                return "director";
            }
            else if (content == Jvedio.Language.Resources.Genre)
            {
                return "genre";
            }
            else if (content == Jvedio.Language.Resources.Label)
            {
                return "label";
            }
            else if (content == Jvedio.Language.Resources.Actor)
            {
                return "actor";
            }
            else if (content == Jvedio.Language.Resources.Studio)
            {
                return "studio";
            }
            else if (content == Jvedio.Language.Resources.Rating)
            {
                return "rating";
            }
            else
            {
                return "";
            }

        }


        public static string[] ToFileName(this DetailMovie movie)
        {
            string txt = Properties.Settings.Default.RenameFormat;
            FileInfo fileInfo = new FileInfo(movie.filepath);
            string oldName = movie.filepath;
            string name = Path.GetFileNameWithoutExtension(movie.filepath);
            string dir = fileInfo.Directory.FullName;
            string ext = fileInfo.Extension;
            string newName = "";
            MatchCollection matches = Regex.Matches(txt, "\\{[a-z]+\\}");
            if (matches != null && matches.Count > 0)
            {
                newName = txt;
                foreach (Match match in matches)
                {
                    string property = match.Value.Replace("{", "").Replace("}", "");
                    ReplaceWithValue(property, movie, ref newName);
                }
            }

            //替换掉特殊字符
            foreach (char item in BANFILECHAR) { newName = newName.Replace(item.ToString(), ""); }

            if (Properties.Settings.Default.DelRenameTitleSpace) newName = newName.Replace(" ", "");
            if (movie.hassubsection)
            {
                string[] result = new string[movie.subsectionlist.Count];
                for (int i = 0; i < movie.subsectionlist.Count; i++)
                {
                    if(Properties.Settings.Default.AddLabelTagWhenRename && Identify.IsCHS(oldName))
                    {
                        result[i] = Path.Combine(dir, $"{newName}-{i + 1}_{Jvedio.Language.Resources.Translated}{ext}");
                    }
                    else
                    {
                        result[i] = Path.Combine(dir, $"{newName}-{i + 1}{ext}");
                    }
                    
                }
                return result;
            }
            else
            {
                if (Properties.Settings.Default.AddLabelTagWhenRename && Identify.IsCHS(oldName))
                {
                    return new string[] { Path.Combine(dir, $"{newName}_{Jvedio.Language.Resources.Translated}{ext}") };
                }
                else
                {
                    return new string[] { Path.Combine(dir, $"{newName}{ext}") };
                }
            }


        }



        private static void ReplaceWithValue(string property,Movie movie,ref string result)
        {
            string inSplit = Properties.Settings.Default.InSplit.Replace("无", "");
            PropertyInfo[] PropertyList = movie.GetType().GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                string name = item.Name;
                if (name == property)
                {
                    object o = item.GetValue(movie);
                    if (o != null)
                    {
                        string value = o.ToString();

                        if (property == "actor" || property == "genre" || property == "label")
                            value = value.Replace(" ", inSplit).Replace("/", inSplit);

                        if (property == "vediotype")
                        {
                            int v = 1;
                            int.TryParse(value, out v);
                            if (v == 1)
                                value = Jvedio.Language.Resources.Uncensored;
                            else if (v == 2)
                                value = Jvedio.Language.Resources.Censored;
                            else if (v == 3)
                                value = Jvedio.Language.Resources.Europe;
                        }

                       



                        if (string.IsNullOrEmpty(value))
                        {
                            //如果值为空，则删掉前面的分隔符
                            int idx = result.IndexOf("{" + property + "}");
                            if (idx >= 1)
                            {
                                result = result.Remove(idx - 1,1);
                            }
                            result = result.Replace("{" + property + "}","");
                        }
                        else
                            result = result.Replace("{" + property + "}", value);



                    }
                    else
                    {
                        int idx = result.IndexOf("{" + property + "}");
                        if (idx >= 1)
                        {
                            result = result.Remove(idx - 1);
                        }
                        result = result.Replace("{" + property + "}", "");
                    }
                    break;
                }
            }
            
        }




    }




    /// <summary> 
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
    /// </summary> 
    /// <typeparam name="T"></typeparam> 
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
        /// </summary> 
        public ObservableRangeCollection() : base() { }

        /// <summary> 
        /// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
        /// </summary> 
        /// <param name="collection">collection: The collection from which the elements are copied.</param> 
        /// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }


        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). 
        /// </summary> 
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            foreach (var i in collection) Items.Remove(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified item. 
        /// </summary> 
        public void Replace(T item)
        {
            ReplaceRange(new T[] { item });
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection. 
        /// </summary> 
        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            Items.Clear();
            foreach (var i in collection) Items.Add(i);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

    }



    public class FixedList<T> : List<T>
    {

        private int max;

        public FixedList(int max=10){ this.max = max; }



        public new void Add(T item)
        {
            if (Count >= max)
            {
                Remove(this[0]);
            }
            base.Add(item);
        }
    }




    public class RatingComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return Comparer<double>.Default.Compare(double.Parse(x.Split('-').First()), double.Parse(y.Split('-').First()));

        }
    }


}
