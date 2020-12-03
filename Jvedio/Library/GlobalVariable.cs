
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Jvedio
{
    public static class GlobalVariable
    {
        public static Stopwatch stopwatch = new Stopwatch();//计时

        //路径
        public static string InfoDataBasePath = AppDomain.CurrentDomain.BaseDirectory + "Info.sqlite";
        public static string DataBaseConfigPath = "DataBase\\Config.ini";
        public static string ServersConfigPath = "ServersConfig.ini";

        //禁止的文件名
        public static readonly char[] BANFILECHAR = { '\\', '#', '%', '&', '*', '|', ':', '"', '<', '>', '?', '/', '.' }; //https://docs.microsoft.com/zh-cn/previous-versions/s6feh8zw(v=vs.110)?redirectedfrom=MSDN


        //全局变量
        public static string BasePicPath;
        public static rootUrl RootUrl;
        public static enableUrl EnableUrl;
        public static Cookie AllCookies;

        //骑兵、步兵识别码
        public static List<string> Qibing = new List<string>();
        public static List<string> Bubing = new List<string>();

        //多少 GB 视为高清
        public static double MinHDVFileSize = 2;

        // jav321 转换规则
        public static Dictionary<string, string> Jav321IDDict = new Dictionary<string, string>();

        //命令记录：前进后退
        public const int MaxCommand = 10;
        public static string[] SqlCommandList = new string[MaxCommand];
        public static int CommandIndex = 0;
        public static string CurrentCommand = "";

        //按类别中分类
        public static string[] GenreEurope = new string[8];
        public static string[] GenreCensored = new string[7];
        public static string[] GenreUncensored = new string[8];

        //演员分隔符
        public static Dictionary<int, char[]> actorSplitDict = new Dictionary<int, char[]>();


        //最近播放
        public static Dictionary<DateTime, List<string>> RecentWatched =new Dictionary<DateTime, List<string>>();

        #region "热键"
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int HOTKEY_ID = 2415;
        public static uint VK;
        public static IntPtr _windowHandle;
        public static HwndSource _source;
        public static bool IsHide = false;
        public static List<string> OpeningWindows = new List<string>();
        public static List<Key>  funcKeys = new List<Key>(); //功能键 [1,3] 个
        public static Key key = Key.None;//基础键 1 个
        public static List<Key> _funcKeys = new List<Key>();
        public static Key _key = Key.None;

        public  enum Modifiers
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        public static bool IsProperFuncKey(List<Key> keyList)
        {
            bool result = true;
            List<Key> keys = new List<Key>() { Key.LeftCtrl, Key.LeftAlt, Key.LeftShift };

            foreach (Key item in keyList)
            {
                if (!keys.Contains(item))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        #endregion

        public static void InitVariable()
        {
            if (Directory.Exists(Properties.Settings.Default.BasePicPath))
                BasePicPath = Properties.Settings.Default.BasePicPath;
            else
                BasePicPath = AppDomain.CurrentDomain.BaseDirectory + "Pic\\";


            if (string.IsNullOrEmpty(Properties.Settings.Default.Bus)) Properties.Settings.Default.EnableBus = false;
            if (string.IsNullOrEmpty(Properties.Settings.Default.BusEurope)) Properties.Settings.Default.EnableBusEu = false;
            if (string.IsNullOrEmpty(Properties.Settings.Default.DB)) Properties.Settings.Default.EnableDB = false;
            if (string.IsNullOrEmpty(Properties.Settings.Default.Library)) Properties.Settings.Default.EnableLibrary = false;
            if (string.IsNullOrEmpty(Properties.Settings.Default.DMM)) Properties.Settings.Default.EnableDMM = false;
            if (string.IsNullOrEmpty(Properties.Settings.Default.Jav321)) Properties.Settings.Default.Enable321 = false;
            if (string.IsNullOrEmpty(Properties.Settings.Default.FC2)) Properties.Settings.Default.EnableFC2 = false;
            Properties.Settings.Default.Save();

            //添加演员分隔符
            if(!actorSplitDict.ContainsKey(0)) actorSplitDict.Add(0, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(1)) actorSplitDict.Add(1, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(2)) actorSplitDict.Add(2, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(3)) actorSplitDict.Add(3, new char[] {'/' });//欧美

            FormatUrl();//格式化网址


            RootUrl = new rootUrl
            {
                Bus = Properties.Settings.Default.Bus,
                BusEu = Properties.Settings.Default.BusEurope,
                Library = Properties.Settings.Default.Library,
                FC2 = Properties.Settings.Default.FC2,
                Jav321 = Properties.Settings.Default.Jav321,
                DMM = Properties.Settings.Default.DMM,
                DB = Properties.Settings.Default.DB
            };

            EnableUrl = new enableUrl
            {
                Bus = Properties.Settings.Default.EnableBus,
                BusEu = Properties.Settings.Default.EnableBusEu,
                Library = Properties.Settings.Default.EnableLibrary,
                FC2 = Properties.Settings.Default.EnableFC2,
                Jav321 = Properties.Settings.Default.Enable321,
                DMM = Properties.Settings.Default.EnableDMM,
                DB = Properties.Settings.Default.EnableDB
            };

            AllCookies = new Cookie
            {
                Bus = "",
                BusEu = "",
                Library = "",
                FC2 = "",
                Jav321 = "",
                DMM = Properties.Settings.Default.DMMCookie,
                DB = Properties.Settings.Default.DBCookie
            };

            GenreEurope[0] = Resource_String.GenreEurope.Split('|')[0];
            GenreEurope[1] = Resource_String.GenreEurope.Split('|')[1];
            GenreEurope[2] = Resource_String.GenreEurope.Split('|')[2];
            GenreEurope[3] = Resource_String.GenreEurope.Split('|')[3];
            GenreEurope[4] = Resource_String.GenreEurope.Split('|')[4];
            GenreEurope[5] = Resource_String.GenreEurope.Split('|')[5];
            GenreEurope[6] = Resource_String.GenreEurope.Split('|')[6];
            GenreEurope[7] = Resource_String.GenreEurope.Split('|')[7];

            GenreCensored[0] = Resource_String.GenreCensored.Split('|')[0];
            GenreCensored[1] = Resource_String.GenreCensored.Split('|')[1];
            GenreCensored[2] = Resource_String.GenreCensored.Split('|')[2];
            GenreCensored[3] = Resource_String.GenreCensored.Split('|')[3];
            GenreCensored[4] = Resource_String.GenreCensored.Split('|')[4];
            GenreCensored[5] = Resource_String.GenreCensored.Split('|')[5];
            GenreCensored[6] = Resource_String.GenreCensored.Split('|')[6];

            GenreUncensored[0] = Resource_String.GenreUncensored.Split('|')[0];
            GenreUncensored[1] = Resource_String.GenreUncensored.Split('|')[1];
            GenreUncensored[2] = Resource_String.GenreUncensored.Split('|')[2];
            GenreUncensored[3] = Resource_String.GenreUncensored.Split('|')[3];
            GenreUncensored[4] = Resource_String.GenreUncensored.Split('|')[4];
            GenreUncensored[5] = Resource_String.GenreUncensored.Split('|')[5];
            GenreUncensored[6] = Resource_String.GenreUncensored.Split('|')[6];
            GenreUncensored[7] = Resource_String.GenreUncensored.Split('|')[7];


    }


        public static void FormatUrl()
        {
            Properties.Settings.Default.Bus = Properties.Settings.Default.Bus.ToProperUrl();
            Properties.Settings.Default.DB =Properties.Settings.Default.DB.ToProperUrl();
            Properties.Settings.Default.Library = Properties.Settings.Default.Library.ToProperUrl();
            Properties.Settings.Default.Jav321 = Properties.Settings.Default.Jav321.ToProperUrl();
            Properties.Settings.Default.FC2 = Properties.Settings.Default.FC2.ToProperUrl();
            Properties.Settings.Default.DMM = Properties.Settings.Default.DMM.ToProperUrl();
        }




        #region "enum"
        public enum ViewType { 默认, 有图, 无图 }
        public enum MyImageType { 缩略图, 海报图, 预览图, 动态图, 列表模式 }
        public enum MovieStampType { 无, 高清中字, 无码流出 }

        public enum VedioType { 所有, 步兵, 骑兵, 欧美 }

        public enum ImageType { SmallImage, BigImage, ExtraImage, ActorImage }

        public enum JvedioWindowState { Normal, Minimized, Maximized, FullScreen, None }

        public enum WebSite { Bus, BusEu, Library, DB, FC2, Jav321, DMM,None }

        public enum Skin { 黑色,白色, 蓝色}

        public enum Language { 中文, English, 日本語 }

        public enum Sort { 识别码, 文件大小, 创建时间,导入时间, 喜爱程度, 名称, 访问次数 , 发行日期 , 评分, 时长 , 演员 }
        #endregion



        #region "struct"



        public struct Cookie
        {
            public string Bus;
            public string BusEu;
            public string Library;
            public string DB;
            public string Jav321;
            public string DMM;
            public string FC2;
        }

        public struct rootUrl
        {
            public string Bus;
            public string BusEu;
            public string Library;
            public string DB;
            public string Jav321;
            public string DMM;
            public string FC2;
        }

        public struct enableUrl
        {
            public bool Bus;
            public bool BusEu;
            public bool Library;
            public bool DB;
            public bool Jav321;
            public bool DMM;
            public bool FC2;
        }

        #endregion





    }
}
