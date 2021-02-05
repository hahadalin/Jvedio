using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using static Jvedio.GlobalVariable;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using DynamicData;
using System.Windows.Media.Imaging;

namespace Jvedio.ViewModel
{
    public class VieModel_Details : ViewModelBase
    {

        public event EventHandler QueryCompleted;
        public VieModel_Details()
        {
            QueryCommand = new RelayCommand<string>(Query);
        }

        private int _SelectImageIndex = 0;

        public int SelectImageIndex
        {
            get { return _SelectImageIndex; }
            set
            {
                _SelectImageIndex = value;
                RaisePropertyChanged();
            }
        }



        private string _SwitchInfo = "影片信息";
        public string SwitchInfo
        {
            get { return _SwitchInfo; }
            set
            {
                _SwitchInfo = value;
                RaisePropertyChanged();
            }
        }

        private VedioInfo _VedioInfo;

        public VedioInfo VedioInfo
        {
            get { return _VedioInfo; }
            set
            {
                _VedioInfo = value;
                RaisePropertyChanged();
            }
        }



        private DetailMovie detailmovie;

        public DetailMovie DetailMovie
        {
            get { return detailmovie; }
            set
            {
                detailmovie = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> labellist;
        public ObservableCollection<string> LabelList
        {
            get { return labellist; }
            set
            {
                labellist = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<MyListItem> _MyList;


        public ObservableCollection<MyListItem> MyList
        {
            get { return _MyList; }
            set
            {
                _MyList = value;
                RaisePropertyChanged();
            }
        }

        public void CleanUp()
        {
            MessengerInstance.Unregister(this);
        }

        public void GetLabelList()
        {
            //TextType = "标签";
            List<string> labels = DataBase.SelectLabelByVedioType(VedioType.所有);

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LabelList = new ObservableCollection<string>();
                LabelList.AddRange(labels);
            });
        }


        public RelayCommand<string> QueryCommand { get; set; }


        public void SaveLove()
        {

                DataBase.UpdateMovieByID(DetailMovie.id, "favorites", DetailMovie.favorites, "string");
        }

        public void SaveLabel()
        {
            List<string> labels = DetailMovie.labellist;
            labels.Remove("+");

            DataBase.UpdateMovieByID(DetailMovie.id, "label", string.Join(" ", labels), "string");

        }




        public void Query(string movieid)
        {
            ((WindowDetails)GlobalMethod.GetWindowByName("WindowDetails")).SetStatus(false);
            DetailMovie detailMovie = null;
                detailMovie = DataBase.SelectDetailMovieById(movieid);
                //访问次数+1
                if (detailMovie != null)
                {
                    detailMovie.visits += 1;
                    DataBase.UpdateMovieByID(movieid, "visits", detailMovie.visits);
                }
            
            //释放图片内存
            if (DetailMovie != null)
            {
                DetailMovie.smallimage = null;
                DetailMovie.bigimage = null;
                for (int i = 0; i < DetailMovie.extraimagelist.Count; i++)
                {
                    DetailMovie.extraimagelist[i] = null;
                }

                for (int i = 0; i < DetailMovie.actorlist.Count; i++)
                {
                    DetailMovie.actorlist[i].bigimage = null;
                    DetailMovie.actorlist[i].smallimage = null;
                }
            }
            GC.Collect();


            DetailMovie = new DetailMovie();
            if (detailMovie != null)
            {
                BitmapImage bigimage= ImageProcess.GetBitmapImage(detailMovie.id, "BigPic");
                if (bigimage == null) bigimage = DefaultBigImage;
                detailMovie.bigimage = bigimage;
                MySqlite db = new MySqlite("Translate");
                //加载翻译结果
                if (Properties.Settings.Default.TitleShowTranslate)
                {
                    string translate_title = db.GetInfoBySql($"select translate_title from youdao where id='{detailMovie.id}'");
                    if (translate_title != "") detailMovie.title = translate_title;
                }

                if (Properties.Settings.Default.PlotShowTranslate)
                {
                    string translate_plot = db.GetInfoBySql($"select translate_plot from youdao where id='{detailMovie.id}'");
                    if (translate_plot != "") detailMovie.plot = translate_plot;
                }
                db.CloseDB();

                //显示新增按钮
                List<string> labels = detailMovie.labellist;
                detailMovie.labellist = new List<string>();
                detailMovie.labellist.Add("+");
                detailMovie.labellist.AddRange(labels);
                DetailMovie = detailMovie;
                detailMovie.tagstamps = "";
                FileProcess.addTag(ref detailMovie);
                if (string.IsNullOrEmpty(DetailMovie.title)) DetailMovie.title = Path.GetFileNameWithoutExtension(DetailMovie.filepath);
                QueryCompleted?.Invoke(this, new EventArgs());
            }
        }
    }


}
