using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using static Jvedio.GlobalVariable;

namespace Jvedio.ViewModel
{
    public class VieModel_Batch : ViewModelBase
    {




        /// <summary>
        /// 如果目录下的图片数目少于网址的数目，则下载
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private bool IsToDownload(Movie movie)
        {
            if (Net.IsToDownLoadInfo(movie) || movie.extraimageurl=="")
                return true;
            else
            {
                //判断预览图个数
                List<string> extraImageList = new List<string>();
                if (!string.IsNullOrEmpty(movie.extraimageurl) && movie.extraimageurl.IndexOf(";") > 0)
                {
                    //预览图地址不为空
                    extraImageList = movie.extraimageurl.Split(';').ToList().Where(arg => !string.IsNullOrEmpty(arg) && arg.IndexOf("http") >= 0 && arg.IndexOf("dmm") >= 0)?.ToList(); 

                    int count = 0;
                    try
                    {
                        var files = Directory.GetFiles(BasePicPath + "ExtraPic\\" + movie.id + "\\", "*.*", SearchOption.TopDirectoryOnly);
                        if (files != null)  count = files.Count(); 
                    } catch { }

                    if (extraImageList.Count > count)
                        return true;
                    else 
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool IsScreenShotExist(string path)
        {
            if (!Directory.Exists(path)) return false;
            try
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                if (files.Count() > 0)
                    return true;
                else
                    return false;
            }
            catch { }
            return false;
        }


        public bool Reset(Action<string> callback)
        {
            Movies = new ObservableCollection<string>();
            var movies = DataBase.SelectMoviesBySql("SELECT * FROM movie");
            int idx = Properties.Settings.Default.BatchIndex;//侧边栏哪个被选中了

            switch (idx)
            {
                case 0:
                    if (Info_ForceDownload)
                    {
                        movies.ForEach(arg => Movies.Add(arg.id));
                    }
                    else
                    {
                        //判断哪些需要下载
                        movies.ForEach(arg => {
                            if (IsToDownload(arg))
                            {
                                Movies.Add(arg.id);
                            }
                        });
                    }
                    Info_TotalNum = Movies.Count;

                    break;

                case 1:
                    if (Gif_Skip)
                    {
                        //跳过已截取的
                        string path = Properties.Settings.Default.BasePicPath + "Gif\\";
                        movies.ForEach(arg => { 
                            if(!File.Exists(path + arg.id + ".gif"))
                            {
                                Movies.Add(arg.id);
                            }
                        });
                    }
                    else
                    {
                        movies.ForEach(arg => { Movies.Add(arg.id); });
                    }
                    Gif_TotalNum = Movies.Count;
                    break;

                case 2:
                    if (ScreenShot_Skip)
                    {
                        string path = Properties.Settings.Default.BasePicPath;
                        if (Properties.Settings.Default.ScreenShotToExtraPicPath)
                            path += "ExtraPic\\";
                        else
                            path += "ScreenShot\\";

                        movies.ForEach(arg => {
                            if (!IsScreenShotExist(path + arg.id))
                            {
                                Movies.Add(arg.id);
                            }
                        });
                    }
                    else
                    {
                        movies.ForEach(arg => { Movies.Add(arg.id); });
                    }
                    ScreenShot_TotalNum = Movies.Count;
                    break;

                case 3:

                    break;

                case 4:

                    break;

                case 5:
                    //重命名
                    movies.ForEach(arg => { if (File.Exists(arg.filepath)) Movies.Add(arg.id); });
                    Rename_TotalNum = Movies.Count;
                    break;

                default:

                    break;
            }
            callback.Invoke("完成");
            return true;
        }




        private int _TotalNum = 0;

        public int TotalNum
        {
            get { return _TotalNum; }
            set
            {
                _TotalNum = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentNum = 0;

        public int CurrentNum
        {
            get { return _CurrentNum; }
            set
            {
                _CurrentNum = value;
                RaisePropertyChanged();
            }
        }


        private int _TotalNum_S = 0;

        public int TotalNum_S
        {
            get { return _TotalNum_S; }
            set
            {
                _TotalNum_S = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentNum_S = 0;

        public int CurrentNum_S
        {
            get { return _CurrentNum_S; }
            set
            {
                _CurrentNum_S = value;
                RaisePropertyChanged();
            }
        }

        private int _Progress = 0;

        public int Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                RaisePropertyChanged();
            }
        }

        private int _Progress_S = 0;

        public int Progress_S
        {
            get { return _Progress_S; }
            set
            {
                _Progress_S = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _Movies = new ObservableCollection<string>();

        public ObservableCollection<string> Movies
        {
            get { return _Movies; }
            set
            {
                _Movies = value;
                RaisePropertyChanged();
            }
        }

        #region "Rename"




        private int _Rename_TotalNum = 0;

        public int Rename_TotalNum
        {
            get { return _Rename_TotalNum; }
            set
            {
                _Rename_TotalNum = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 5) TotalNum = Rename_TotalNum;
            }
        }



        private int _Rename_CurrentProgress = 0;

        public int Rename_CurrentProgress
        {
            get { return _Rename_CurrentProgress; }
            set
            {
                _Rename_CurrentProgress = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 5)
                {
                    CurrentNum = Rename_CurrentProgress;
                    Progress = 100 * Rename_CurrentProgress / (Rename_TotalNum == 0 ? 1 : Rename_TotalNum);
                }
            }
        }


        #endregion

        #region "Gif"




        private bool _Gif_Skip = true;

        public bool Gif_Skip
        {
            get { return _Gif_Skip; }
            set
            {
                _Gif_Skip = value;
                RaisePropertyChanged();
            }
        }




        private int _Gif_Length = 5;

        public int Gif_Length
        {
            get { return _Gif_Length; }
            set
            {
                _Gif_Length = value;
                RaisePropertyChanged();
            }
        }


        private int _Gif_Width = 280;

        public int Gif_Width
        {
            get { return _Gif_Width; }
            set
            {
                _Gif_Width = value;
                RaisePropertyChanged();
            }
        }





        private int _Gif_Height = 170;

        public int Gif_Height
        {
            get { return _Gif_Height; }
            set
            {
                _Gif_Height = value;
                RaisePropertyChanged();
            }
        }



        private int _Gif_TotalNum = 0;

        public int Gif_TotalNum
        {
            get { return _Gif_TotalNum; }
            set
            {
                _Gif_TotalNum = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 1) TotalNum = Gif_TotalNum;
            }
        }

        private int _Gif_CurrentProgress = 0;

        public int Gif_CurrentProgress
        {
            get { return _Gif_CurrentProgress; }
            set
            {
                _Gif_CurrentProgress = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 1) {
                    CurrentNum = Gif_CurrentProgress;
                    Progress = 100 * Gif_CurrentProgress / (Gif_TotalNum == 0 ? 1 : Gif_TotalNum); }
            }
        }





        #endregion



        #region "ScreenShot"




        private bool _ScreenShot_Skip = true;

        public bool ScreenShot_Skip
        {
            get { return _ScreenShot_Skip; }
            set
            {
                _ScreenShot_Skip = value;
                RaisePropertyChanged();
            }
        }

        private bool _ScreenShot_DefaultSave = true;

        public bool ScreenShot_DefaultSave
        {
            get { return _ScreenShot_DefaultSave; }
            set
            {
                _ScreenShot_DefaultSave = value;
                RaisePropertyChanged();
            }
        }


        private int _ScreenShot_Num = 5;

        public int ScreenShot_Num
        {
            get { return _ScreenShot_Num; }
            set
            {
                _ScreenShot_Num = value;
                RaisePropertyChanged();
            }
        }








        private int _ScreenShot_TotalNum = 0;

        public int ScreenShot_TotalNum
        {
            get { return _ScreenShot_TotalNum; }
            set
            {
                _ScreenShot_TotalNum = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 2) TotalNum = ScreenShot_TotalNum;
            }
        }

        private int _ScreenShot_CurrentProgress = 0;

        public int ScreenShot_CurrentProgress
        {
            get { return _ScreenShot_CurrentProgress; }
            set
            {
                _ScreenShot_CurrentProgress = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 2) {
                    CurrentNum = ScreenShot_CurrentProgress;
                    Progress = 100 * ScreenShot_CurrentProgress / (ScreenShot_TotalNum == 0 ? 1 : ScreenShot_TotalNum); 
                }
            }
        }


        private int _ScreenShot_TotalNum_S = 0;

        public int ScreenShot_TotalNum_S
        {
            get { return _ScreenShot_TotalNum_S; }
            set
            {
                _ScreenShot_TotalNum_S = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 2) TotalNum_S = ScreenShot_TotalNum_S;
            }
        }

        private int _ScreenShot_CurrentProgress_S = 0;

        public int ScreenShot_CurrentProgress_S
        {
            get { return _ScreenShot_CurrentProgress_S; }
            set
            {
                _ScreenShot_CurrentProgress_S = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 2) {
                    CurrentNum_S = ScreenShot_CurrentProgress_S;
                    Progress_S = 100 * ScreenShot_CurrentProgress_S / (ScreenShot_TotalNum_S == 0 ? 1 : ScreenShot_TotalNum_S); }
            }
        }





        #endregion


        #region "Download"




        private bool _Info_ForceDownload = false;

        public bool Info_ForceDownload
        {
            get { return _Info_ForceDownload; }
            set
            {
                _Info_ForceDownload = value;
                RaisePropertyChanged();
            }
        }

        private bool _Info_DS = false;

        public bool Info_DS
        {
            get { return _Info_DS; }
            set
            {
                _Info_DS = value;
                RaisePropertyChanged();
            }
        }

        private bool _Info_DB = false;

        public bool Info_DB
        {
            get { return _Info_DB; }
            set
            {
                _Info_DB = value;
                RaisePropertyChanged();
            }
        }

        private bool _Info_DE = true;

        public bool Info_DE
        {
            get { return _Info_DE; }
            set
            {
                _Info_DE = value;
                RaisePropertyChanged();
            }
        }





        private int _Info_TotalNum = 0;

        public int Info_TotalNum
        {
            get { return _Info_TotalNum; }
            set
            {
                _Info_TotalNum = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 0) TotalNum = Info_TotalNum;
            }
        }

        private int _Info_CurrentProgress = 0;

        public int Info_CurrentProgress
        {
            get { return _Info_CurrentProgress; }
            set
            {
                _Info_CurrentProgress = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 0)
                {
                    CurrentNum = Info_CurrentProgress;
                    Progress = 100 * Info_CurrentProgress / (Info_TotalNum == 0 ? 1 : Info_TotalNum);
                }
            }
        }


        private int _Info_TotalNum_S = 0;

        public int Info_TotalNum_S
        {
            get { return _Info_TotalNum_S; }
            set
            {
                _Info_TotalNum_S = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 0) TotalNum_S = Info_TotalNum_S;
            }
        }

        private int _Info_CurrentProgress_S = 0;

        public int Info_CurrentProgress_S
        {
            get { return _Info_CurrentProgress_S; }
            set
            {
                _Info_CurrentProgress_S = value;
                RaisePropertyChanged();
                if (Properties.Settings.Default.BatchIndex == 0) {
                    CurrentNum_S = Info_CurrentProgress_S;
                    Progress_S = 100 * Info_CurrentProgress_S / (Info_TotalNum_S == 0 ? 1 : Info_TotalNum_S); 
                
                }
            }
        }




        #endregion

    }
}
