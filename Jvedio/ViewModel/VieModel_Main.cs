using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using static Jvedio.GlobalMethod;
using static Jvedio.GlobalVariable;
using System.Windows.Input;
using System.Drawing;
using DynamicData;
using DynamicData.Binding;
using System.Xml;
using HandyControl.Tools.Extension;
using DynamicData.Annotations;

namespace Jvedio.ViewModel
{
    public class VieModel_Main : ViewModelBase
    {
        public event EventHandler CurrentMovieListHideOrChanged;
        public event EventHandler CurrentActorListHideOrChanged;
        public event EventHandler CurrentMovieListChangedCompleted;

        public bool IsFlipOvering = false;
        public bool StopFlipOver = false;

        public bool StopLoadMovie = false;

        public VedioType CurrentVedioType = VedioType.所有;

        private DispatcherTimer SearchTimer = new DispatcherTimer();


        public int SideIdx = 0;



        public FixedList<List<string>> Record = new FixedList<List<string>>(10);//固定长度
        public int RecordIndex= 0;

        #region "RelayCommand"
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand GenreCommand { get; set; }
        public RelayCommand ActorCommand { get; set; }
        public RelayCommand LabelCommand { get; set; }

        public RelayCommand FavoritesCommand { get; set; }
        public RelayCommand RecentCommand { get; set; }

        public RelayCommand<bool> RecentWatchCommand { get; set; }
        public RelayCommand<VedioType> CensoredCommand { get; set; }
        public RelayCommand<VedioType> UncensoredCommand { get; set; }
        public RelayCommand<VedioType> EuropeCommand { get; set; }

        public RelayCommand AddNewMovie { get; set; }
        #endregion


        public VieModel_Main()
        {
            ResetCommand = new RelayCommand(Reset);
            GenreCommand = new RelayCommand(GetGenreList);
            ActorCommand = new RelayCommand(GetActorList);
            LabelCommand = new RelayCommand(GetLabelList);


            FavoritesCommand = new RelayCommand(GetFavoritesMovie);
            RecentWatchCommand = new RelayCommand<bool>(t => GetRecentWatch());
            RecentCommand = new RelayCommand(GetRecentMovie);
            UncensoredCommand = new RelayCommand<VedioType>(t => GetMoviebyVedioType(VedioType.步兵));
            CensoredCommand = new RelayCommand<VedioType>(t => GetMoviebyVedioType(VedioType.骑兵));
            EuropeCommand = new RelayCommand<VedioType>(t => GetMoviebyVedioType(VedioType.欧美));


            AddNewMovie = new RelayCommand(AddSingleMovie);



            SearchTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.3) };
            SearchTimer.Tick += new EventHandler(SearchTimer_Tick);




            //获得所有数据库
            LoadDataBaseList();

            DataBase.MovieListChanged += (o, e) => { CurrentPage = 1; };


        }







        #region "enum"
        private VedioType vedioType =  Properties.Settings.Default.VedioType.Length == 1? (VedioType) int.Parse(Properties.Settings.Default.VedioType):0; 
        public VedioType VedioType
        {
            get { return vedioType; }
            set
            {
                vedioType = value;
                RaisePropertyChanged();
            }
        }



        private MyImageType _ShowImageMode= Properties.Settings.Default.ShowImageMode.Length == 1 ? (MyImageType)int.Parse(Properties.Settings.Default.ShowImageMode) : 0;

        public MyImageType ShowImageMode
        {
            get { return _ShowImageMode; }
            set
            {
                _ShowImageMode = value;
                RaisePropertyChanged();
                Properties.Settings.Default.ShowImageMode = value.ToString();
            }
        }



        private ViewType _ShowViewMode = Properties.Settings.Default.ShowViewMode.Length == 1 ? (ViewType)int.Parse(Properties.Settings.Default.ShowViewMode) : 0;

        public ViewType ShowViewMode
        {
            get { return _ShowViewMode; }
            set
            {
                _ShowViewMode = value;
                RaisePropertyChanged();
            }
        }


        private MySearchType _SearchType = Properties.Settings.Default.SearchType.Length == 1 ? (MySearchType)int.Parse(Properties.Settings.Default.SearchType) : 0;

        public MySearchType SearchType
        {
            get { return _SearchType; }
            set
            {
                _SearchType = value;
                RaisePropertyChanged();
            }
        }


        private MySearchType _AllSearchType = Properties.Settings.Default.AllSearchType.Length == 1 ? (MySearchType)int.Parse(Properties.Settings.Default.AllSearchType) : 0;

        public MySearchType AllSearchType
        {
            get { return _AllSearchType; }
            set
            {
                _AllSearchType = value;
                RaisePropertyChanged();
            }
        }

        #endregion


        #region "ObservableCollection"


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

        private ObservableCollection<string> _DataBases;


        public ObservableCollection<string> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Movie> currentmovielist;


        public ObservableCollection<Movie> CurrentMovieList
        {
            get { return currentmovielist; }
            set
            {
                currentmovielist = value;
                RaisePropertyChanged();
                CurrentMovieListHideOrChanged?.Invoke(this, EventArgs.Empty);
                if (MovieList != null) TotalCount = MovieList.Count;
                IsFlipOvering = false;
            }
        }


        private ObservableCollection<Movie> _DetailsDataList;


        public ObservableCollection<Movie> DetailsDataList
        {
            get { return _DetailsDataList; }
            set
            {
                _DetailsDataList = value;
                RaisePropertyChanged();
            }
        }





        private ObservableCollection<Movie> selectedMovie = new ObservableCollection<Movie>();

        public ObservableCollection<Movie> SelectedMovie
        {
            get { return selectedMovie; }
            set
            {
                selectedMovie = value;
                RaisePropertyChanged();
            }
        }


        public List<Movie> MovieList;

        private ObservableCollection<Genre> genrelist;
        public ObservableCollection<Genre> GenreList
        {
            get { return genrelist; }
            set
            {
                genrelist = value;
                RaisePropertyChanged();

            }
        }


        private ObservableCollection<Actress> actorlist;
        public ObservableCollection<Actress> ActorList
        {
            get { return actorlist; }
            set
            {
                actorlist = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Actress> _CurrentActorList;


        public ObservableCollection<Actress> CurrentActorList
        {
            get { return _CurrentActorList; }
            set
            {
                _CurrentActorList = value;
                RaisePropertyChanged();
                CurrentActorListHideOrChanged?.Invoke(this, EventArgs.Empty);
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




        private ObservableCollection<string> _AllSearchCandidate;


        public ObservableCollection<string> AllSearchCandidate
        {
            get { return _AllSearchCandidate; }
            set
            {
                _AllSearchCandidate = value;
                RaisePropertyChanged();

            }
        }


        private ObservableCollection<string> _FilePathClassification;


        public ObservableCollection<string> FilePathClassification
        {
            get { return _FilePathClassification; }
            set
            {
                _FilePathClassification = value;
                RaisePropertyChanged();

            }
        }


        private ObservableCollection<string> _CurrentSearchCandidate;


        public ObservableCollection<string> CurrentSearchCandidate
        {
            get { return _CurrentSearchCandidate; }
            set
            {
                _CurrentSearchCandidate = value;
                RaisePropertyChanged();

            }
        }



        private ObservableCollection<Movie> _SearchCandidate;


        public ObservableCollection<Movie> SearchCandidate
        {
            get { return _SearchCandidate; }
            set
            {
                _SearchCandidate = value;
                RaisePropertyChanged();

            }
        }

        #endregion






        #region "Variable"

        private bool _HideSide = false;

        public bool HideSide
        {
            get { return _HideSide; }
            set
            {
                _HideSide = value;
                RaisePropertyChanged();
            }
        }


        private List<string> _CurrentMovieLabelList;

        public List<string> CurrentMovieLabelList
        {
            get { return _CurrentMovieLabelList; }
            set
            {
                _CurrentMovieLabelList = value;
                RaisePropertyChanged();

            }
        }


        private int _SqlIndex = 0;

        public int SqlIndex
        {
            get { return _SqlIndex; }
            set
            {
                _SqlIndex = value;
                RaisePropertyChanged();

            }
        }

        private FixedList<Dictionary<string, string>> _SqlCommands = new FixedList<Dictionary<string, string>>();

        public FixedList<Dictionary<string, string>> SqlCommands
        {
            get { return _SqlCommands; }
            set
            {
                _SqlCommands = value;
                RaisePropertyChanged();

            }
        }


        private bool _ShowHDV = true;
        public bool ShowHDV
        {
            get { return _ShowHDV; }
            set
            {
                _ShowHDV = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowCHS = true;
        public bool ShowCHS
        {
            get { return _ShowCHS; }
            set
            {
                _ShowCHS = value;
                RaisePropertyChanged();
            }
        }

        private bool _ShowFlowOut = true;
        public bool ShowFlowOut
        {
            get { return _ShowFlowOut; }
            set
            {
                _ShowFlowOut = value;
                RaisePropertyChanged();
            }
        }




        private Sort _SortType = 0;
        public Sort SortType
        {
            get { return _SortType; }
            set
            {
                _SortType = value;
                RaisePropertyChanged();
            }
        }


        private bool _SortDescending = Properties.Settings.Default.SortDescending;
        public bool SortDescending
        {
            get { return _SortDescending; }
            set
            {
                _SortDescending = value;
                RaisePropertyChanged();
            }
        }



        private double _VedioTypeACount = 0;
        public double VedioTypeACount
        {
            get { return _VedioTypeACount; }
            set
            {
                _VedioTypeACount = value;
                RaisePropertyChanged();
            }
        }



        private double _VedioTypeBCount = 0;
        public double VedioTypeBCount
        {
            get { return _VedioTypeBCount; }
            set
            {
                _VedioTypeBCount = value;
                RaisePropertyChanged();
            }
        }



        private double _VedioTypeCCount = 0;
        public double VedioTypeCCount
        {
            get { return _VedioTypeCCount; }
            set
            {
                _VedioTypeCCount = value;
                RaisePropertyChanged();
            }
        }

        private double _RecentWatchedCount = 0;
        public double RecentWatchedCount
        {
            get { return _RecentWatchedCount; }
            set
            {
                _RecentWatchedCount = value;
                RaisePropertyChanged();
            }
        }


        private double _AllVedioCount = 0;
        public double AllVedioCount
        {
            get { return _AllVedioCount; }
            set
            {
                _AllVedioCount = value;
                RaisePropertyChanged();
            }
        }

        private double _FavoriteVedioCount = 0;
        public double FavoriteVedioCount
        {
            get { return _FavoriteVedioCount; }
            set
            {
                _FavoriteVedioCount = value;
                RaisePropertyChanged();
            }
        }

        private double _RecentVedioCount = 0;
        public double RecentVedioCount
        {
            get { return _RecentVedioCount; }
            set
            {
                _RecentVedioCount = value;
                RaisePropertyChanged();
            }
        }


        public bool _IsScanning = false;
        public bool IsScanning
        {
            get { return _IsScanning; }
            set
            {
                _IsScanning = value;
                RaisePropertyChanged();
            }
        }


        public bool _EnableEditActress = false;

        public bool EnableEditActress
        {
            get { return _EnableEditActress; }
            set
            {
                _EnableEditActress = value;
                RaisePropertyChanged();
            }
        }


        public string movieCount = "总计 0 个";


        public int currentpage = 1;
        public int CurrentPage
        {
            get { return currentpage; }
            set
            {
                currentpage = value;
                FlowNum = 0;
                RaisePropertyChanged();
            }
        }


        public double _CurrentCount = 0;
        public double CurrentCount
        {
            get { return _CurrentCount; }
            set
            {
                _CurrentCount = value;
                RaisePropertyChanged();

            }
        }


        public double _TotalCount = 0;
        public double TotalCount
        {
            get { return _TotalCount; }
            set
            {
                _TotalCount = value;
                RaisePropertyChanged();

            }
        }

        public int totalpage = 1;
        public int TotalPage
        {
            get { return totalpage; }
            set
            {
                totalpage = value;
                RaisePropertyChanged();

            }
        }


        public int currentactorpage = 1;
        public int CurrentActorPage
        {
            get { return currentactorpage; }
            set
            {
                currentactorpage = value;
                FlowNum = 0;
                RaisePropertyChanged();
            }
        }


        public int totalactorpage = 1;
        public int TotalActorPage
        {
            get { return totalactorpage; }
            set
            {
                totalactorpage = value;
                RaisePropertyChanged();
            }
        }






        public int _FlowNum = 0;
        public int FlowNum
        {
            get { return _FlowNum; }
            set
            {
                _FlowNum = value;
                RaisePropertyChanged();
            }
        }








        public string textType = "所有视频";

        public string TextType
        {
            get { return textType; }
            set
            {
                textType = value;
                RaisePropertyChanged();
            }
        }

        public int ClickGridType { get; set; }

        private string search = string.Empty;

        private bool IsSearching = false;

        public string Search
        {
            get { return search; }
            set
            {
                search = value;
                RaisePropertyChanged();
                if (search == "") Reset();
                else
                {

                    SearchTimer.Stop();
                    SearchTimer.Start();
                }

            }
        }

        private string _SearchHint = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.ID;


        public string SearchHint
        {
            get { return _SearchHint; }
            set
            {
                _SearchHint = value;
                RaisePropertyChanged();
            }
        }


        private Actress actress;
        public Actress Actress
        {
            get { return actress; }
            set
            {
                actress = value;
                RaisePropertyChanged();
            }
        }

        private bool showSideBar = false;

        public bool ShowSideBar
        {
            get { return showSideBar; }
            set
            {
                showSideBar = value;
                RaisePropertyChanged();
            }
        }



        private bool Checkingurl = false;

        public bool CheckingUrl
        {
            get { return Checkingurl; }
            set
            {
                Checkingurl = value;
                RaisePropertyChanged();
            }
        }

        private bool searchAll = true;

        public bool SearchAll
        {
            get { return searchAll; }
            set
            {
                searchAll = value;
            }
        }


        private bool searchInCurrent = false;

        public bool SearchInCurrent
        {
            get { return searchInCurrent; }
            set
            {
                searchInCurrent = value;
            }
        }

        #endregion



        #region "筛选"

        public ObservableCollection<string> _Year;

        public ObservableCollection<string> Year
        {
            get { return _Year; }
            set
            {
                _Year = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> _Genre;

        public ObservableCollection<string> Genre
        {
            get { return _Genre; }
            set
            {
                _Genre = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<string> _Actor;

        public ObservableCollection<string> Actor
        {
            get { return _Actor; }
            set
            {
                _Actor = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<string> _Label;

        public ObservableCollection<string> Label
        {
            get { return _Label; }
            set
            {
                _Label = value;
                RaisePropertyChanged();
            }
        }


        public ObservableCollection<string> _Runtime;

        public ObservableCollection<string> Runtime
        {
            get { return _Runtime; }
            set
            {
                _Runtime = value;
                RaisePropertyChanged();
            }
        }



        public ObservableCollection<string> _FileSize;

        public ObservableCollection<string> FileSize
        {
            get { return _FileSize; }
            set
            {
                _FileSize = value;
                RaisePropertyChanged();
            }
        }



        public ObservableCollection<string> _Rating;

        public ObservableCollection<string> Rating
        {
            get { return _Rating; }
            set
            {
                _Rating = value;
                RaisePropertyChanged();
            }
        }

        private bool _IsRefresh = false;

        public bool IsRefresh
        {
            get { return _IsRefresh; }
            set
            {
                _IsRefresh = value;
                RaisePropertyChanged();
            }
        }

        public void GetFilterInfo()
        {
            Year = new ObservableCollection<string>();
            Genre = new ObservableCollection<string>();
            Actor = new ObservableCollection<string>();
            Label = new ObservableCollection<string>();
            Runtime = new ObservableCollection<string>();
            FileSize = new ObservableCollection<string>();
            Rating = new ObservableCollection<string>();
            var models = DataBase.GetAllFilter();
            Year.AddRange(models[0]);
            Runtime.AddRange(models[4]);
            FileSize.AddRange(models[5]);
            Rating.AddRange(models[6]);
            Genre.AddRange(models[1].Take(30));
            Actor.AddRange(models[2].Take(30));
            Label.AddRange(models[3]);




        }


        public ObservableCollection<string> GetAllGenre()
        {
            //Genre = new ObservableCollection<string>();
            //var models = DataBase.GetAllFilter();
            //Genre.AddRange(models[1]);
            ObservableCollection<string> result = new ObservableCollection<string>();
            var models = DataBase.GetAllFilter();
            result.AddRange(models[1]);
            return result;
        }

        public ObservableCollection<string> GetAllActor()
        {
            //Actor = new ObservableCollection<string>();
            //var models = DataBase.GetAllFilter();
            //Actor.AddRange(models[2]);
            ObservableCollection<string> result = new ObservableCollection<string>();
            var models = DataBase.GetAllFilter();
            result.AddRange(models[2]);
            return result;

        }

        #endregion

        public async void BeginSearch()
        {
            IsSearching = true;
            GetSearchCandidate(Search.ToProperSql());
            await Query();
            IsSearching = false;
            FlipOver();
        }

        public void LoadDataBaseList()
        {
            DataBases = new ObservableCollection<string>();
            try
            {
                var fiels = Directory.GetFiles("DataBase", "*.sqlite", SearchOption.TopDirectoryOnly).ToList();
                fiels.ForEach(arg => DataBases.Add(Path.GetFileNameWithoutExtension(arg).ToLower()));
            }
            catch { }

            if (!DataBases.Contains("info")) DataBases.Add("info");
        }





        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            if (!IsSearching) BeginSearch();
            SearchTimer.Stop();
        }


        private void AddSingleMovie()
        {
            Dialog_NewMovie dialog_NewMovie = new Dialog_NewMovie((Main)GetWindowByName("Main"));
            var b= (bool)dialog_NewMovie.ShowDialog();
            NewMovieDialogResult result=dialog_NewMovie.Result;

            if (b && !string.IsNullOrEmpty(result.Text))
            {
                List<string> IDList = GetIDListFromString(result.Text,result.VedioType);
                foreach (var item in IDList)
                {
                    InsertID(item,result.VedioType);
                }
            }


        }

        private void InsertID(string id,VedioType vedioType)
        {
            Movie movie = DataBase.SelectMovieByID(id);
            if (movie != null)
            {
                HandyControl.Controls.Growl.Info($"{id} {Jvedio.Language.Resources.Message_AlreadyExist}", "Main");
            }
            else
            {
                Movie movie1 = new Movie()
                {
                    id = id,
                    vediotype = (int)vedioType,
                    otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                DataBase.InsertScanMovie(movie1);
                MovieList.Insert(0, movie1);
                CurrentMovieList.Insert(0, movie1);
            }
        }


        public List<string> GetIDListFromString(string str,VedioType vedioType)
        {
            List<string> result = new List<string>();
            foreach (var item in str.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None))
            {
                string id = "";
                if (vedioType == VedioType.欧美)
                    id=item.Replace(" ","");
                else
                    id = item.ToUpper().Replace(" ", "");

                if (!string.IsNullOrEmpty(id) && !result.Contains(id)) result.Add(id);
            }
            return result;
        }

        public void GetSearchCandidate(string Search)
        {
            CurrentSearchCandidate = new ObservableCollection<string>();
            if (Search == "") return;

            //提取出英文和数字
            string extraSearch = "";
            string number = Identify.GetNum(Search);
            string eng = Identify.GetEng(Search);
            if (!string.IsNullOrEmpty(number)) extraSearch = eng + "-" + number;
            List<Movie> movies = new List<Movie>();
            if (AllSearchType == MySearchType.名称)
            {
                movies = MovieList.Where(m => m.title.ToUpper().Contains(Search.ToUpper())).ToList();
                foreach (Movie movie in movies)
                {
                    CurrentSearchCandidate.Add(movie.title);
                    if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                }
            }
            else if (AllSearchType == MySearchType.演员)
            {
                movies = MovieList.Where(m => m.actor.ToUpper().Contains(Search.ToUpper())).ToList();
                foreach (Movie movie in movies)
                {
                    string[] actor = movie.actor.Split(actorSplitDict[movie.vediotype]);
                    foreach (var item in actor)
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                        {
                            if (!CurrentSearchCandidate.Contains(item) & item.ToUpper().IndexOf(Search.ToUpper()) >= 0) CurrentSearchCandidate.Add(item);
                            if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                        }
                    }
                    if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                }
            }
            else if (AllSearchType == MySearchType.识别码)
            {
                movies = MovieList.Where(m => m.id.ToUpper().Contains(Search.ToUpper())).ToList();
                if (movies.Count==0) movies = MovieList.Where(m => m.id.ToUpper().Contains(extraSearch.ToUpper())).ToList();
                foreach (Movie movie in movies)
                {
                    CurrentSearchCandidate.Add(movie.id);
                    if (CurrentSearchCandidate.Count >= Properties.Settings.Default.SearchCandidateMaxCount) break;
                }
            }
        }





        public void Flow()
        {
            if (MovieList != null)
            {
                CurrentMovieListHideOrChanged?.Invoke(this, EventArgs.Empty); //停止下载
                int DisPlayNum = Properties.Settings.Default.DisplayNumber;//每页展示数目
                int SetFlowNum = Properties.Settings.Default.FlowNum;//流动数目
                Movies = new List<Movie>();
                for (int i = (CurrentPage - 1) * DisPlayNum + FlowNum * SetFlowNum; i < (CurrentPage - 1) * DisPlayNum + (FlowNum + 1) * SetFlowNum; i++)
                {
                    if (CurrentMovieList.Count + Movies.Count< DisPlayNum)
                    {
                        if (i <= MovieList.Count - 1)
                        {
                            Movie movie = MovieList[i];
                            //添加标签戳
                            FileProcess.addTag(ref movie);

                            if (!string.IsNullOrEmpty(movie.id)) Movies.Add(movie);
                        }
                        else { break; }
                    }
                    else
                    {
                        FlowNum = 0;
                    }

                }

                

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (CurrentMovieList != null) CurrentCount = CurrentMovieList.Count + Movies.Count;
                    if (CurrentCount > MovieList.Count) CurrentCount = MovieList.Count;

                });

                foreach (Movie movie in Movies)
                {
                    movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");
                    movie.bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadMovie), movie);
                    if (StopLoadMovie) break;
                }

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Main main = App.Current.Windows[0] as Main;
                    if (Properties.Settings.Default.ShowImageMode == "2") main.ImageSlideTimer.Start();//0.5s后开始展示预览图
                    if (main != null)
                    {
                        //main.IsFlowing = false;
                        main.SetSelected();
                        CurrentMovieListChangedCompleted?.Invoke(this, EventArgs.Empty);
                    }
                    
                });

            }
        }


        public void Refresh()
        {
            App.Current.Windows[0].Cursor = Cursors.Wait;
            List<string> CurrentID = new List<string>();
            foreach (Movie movie in CurrentMovieList) CurrentID.Add(movie.id);
            CurrentMovieList = new ObservableCollection<Movie>();
            FlipOver();
            App.Current.Windows[0].Cursor = Cursors.Arrow;
        }

        public void RefreshActor()
        {
            GetActorList();
        }


        public void ActorFlipOver()
        {
            stopwatch.Restart();
            if (ActorList != null)
            {
                TotalActorPage = (int)Math.Ceiling((double)ActorList.Count / (double)Properties.Settings.Default.ActorDisplayNum);

                //只在翻页时加载图片、显示翻译结果
                int ActorDisplayNum = Properties.Settings.Default.ActorDisplayNum;
                List<Actress> actresses = new List<Actress>();
                for (int i = (CurrentActorPage - 1) * ActorDisplayNum; i < CurrentActorPage * ActorDisplayNum; i++)
                {
                    if (i < ActorList.Count)
                    {
                        Actress actress = ActorList[i];
                        actress.smallimage = ImageProcess.GetBitmapImage(actress.name, "Actresses");
                        actresses.Add(actress);
                    }
                    else { break; }
                    if (actresses.Count == ActorDisplayNum) { break; }
                }

                App.Current.Dispatcher.BeginInvoke((Action)delegate {
                    CurrentActorList = new ObservableCollection<Actress>();
                });
                foreach (Actress actress1 in actresses)
                {
                    App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadActorDelegate(LoadActor), actress1);
                }

                App.Current.Dispatcher.BeginInvoke((Action)delegate {
                    if (App.Current.Windows[0] is Main main) { main.ActorSetSelected(); }
                });
                

            }
            stopwatch.Stop();

        }



        private delegate void LoadActorDelegate(Actress actress);
        private void LoadActor(Actress actress)
        {
            CurrentActorList.Add(actress);
        }

        public void DisposeMovieList(ObservableCollection<Movie> movies)
        {
            if (movies == null) return;

            for (int i = 0; i < movies.Count; i++)
            {
                movies[i].bigimage = null;
                movies[i].smallimage = null;
            }
            GC.Collect();
        }




        public List<Movie> Movies;

        /// <summary>
        /// 翻页：加载图片以及其他
        /// </summary>
        public bool FlipOver()
        {
            GetLabelList();
            if (Properties.Settings.Default.ShowImageMode == "4")
            {
                ShowDetailsData();
            }
            else
            {
                Sort();
                IsFlipOvering = true;
                Task.Run(() =>
                {
                    if (MovieList != null)
                    {

                        TotalPage = (int)Math.Ceiling((double)MovieList.Count / (double)Properties.Settings.Default.DisplayNumber);
                        int DisPlayNum = Properties.Settings.Default.DisplayNumber;
                        int FlowNum = Properties.Settings.Default.FlowNum;
                        DisposeMovieList(CurrentMovieList);

                        //Console.WriteLine("CurrentPage=" + CurrentPage);
                        Movies = new List<Movie>();
                        for (int i = (CurrentPage - 1) * DisPlayNum; i < (CurrentPage - 1) * DisPlayNum + FlowNum; i++)
                        {
                            if (i <= MovieList.Count - 1)
                            {
                                Movie movie = MovieList[i];
                                Movies.Add(movie);
                            }
                            else { break; }
                            if (Movies.Count == FlowNum) { break; }

                        }
                        for (int i = 0; i < Movies.Count; i++)
                        {

                            //添加标签戳
                            if (Identify.IsHDV(Movies[i].filepath) || Movies[i].genre?.IndexOfAnyString(TagStrings_HD) >= 0 || Movies[i].tag?.IndexOfAnyString(TagStrings_HD) >= 0 || Movies[i].label?.IndexOfAnyString(TagStrings_HD) >= 0) Movies[i].tagstamps += Jvedio.Language.Resources.HD;
                            if (Identify.IsCHS(Movies[i].filepath) || Movies[i].genre?.IndexOfAnyString(TagStrings_Translated) >= 0 || Movies[i].tag?.IndexOfAnyString(TagStrings_Translated) >= 0 || Movies[i].label?.IndexOfAnyString(TagStrings_Translated) >= 0) Movies[i].tagstamps += Jvedio.Language.Resources.Translated;
                            if (Identify.IsFlowOut(Movies[i].filepath) || Movies[i].genre?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || Movies[i].tag?.IndexOfAnyString(TagStrings_FlowOut) >= 0 || Movies[i].label?.IndexOfAnyString(TagStrings_FlowOut) >= 0) Movies[i].tagstamps += Jvedio.Language.Resources.FlowOut;
                        }

                        App.Current.Dispatcher.BeginInvoke((Action)delegate { 
                            CurrentMovieList = new ObservableCollection<Movie>();
                            CurrentCount = Movies.Count;
                        });

                        foreach (Movie movie in Movies)
                        {

                            if (StopFlipOver) break;
                            movie.smallimage = ImageProcess. GetBitmapImage(movie.id, "SmallPic");
                            movie.bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");
                            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadMovie), movie);
                        }

                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            if (GetWindowByName("Main") is Main main)
                            {
                                CurrentMovieListChangedCompleted?.Invoke(this, EventArgs.Empty);
                            }

                        });
                    }
                });
            }

            return true;
        }

        private delegate void LoadItemDelegate(Movie movie);
        private void LoadMovie(Movie movie)
        {
            CurrentMovieList.Add(movie);
        }



        //获得标签
        public void GetLabelList()
        {
            //TextType = "标签";
            List<string> labels = DataBase.SelectLabelByVedioType(VedioType);

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                LabelList = new ObservableCollection<string>();
                LabelList.AddRange(labels);
            });
        }


        //获得演员，信息照片都获取
        public void GetActorList()
        {
            //TextType = "演员";
            Statistic();
            stopwatch.Restart();
            List<Actress> Actresses = DataBase.SelectAllActorName(VedioType);
            stopwatch.Stop();


            if (ActorList != null && Actresses != null && Actresses.Count == ActorList.ToList().Count) { return; }
            ActorList = new ObservableCollection<Actress>();
            ActorList.AddRange(Actresses);

            ActorFlipOver();


        }


        //获得类别
        public void GetGenreList()
        {
            //TextType = "类别";
            Statistic();
            List<Genre> Genres = DataBase.SelectGenreByVedioType(VedioType);
            GenreList = new ObservableCollection<Genre>();
            GenreList.AddRange(Genres);

        }


        public void AddToRecentWatch(string ID)
        {
            DateTime dateTime = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(ID))
            {
                if (RecentWatched.ContainsKey(dateTime))
                {
                    if (!RecentWatched[dateTime].Contains(ID))
                        RecentWatched[dateTime].Add(ID);

                }
                else
                {
                    RecentWatched.Add(dateTime, new List<string>() { ID });
                }
            }



            List<string> total = new List<string>();

            foreach (var keyvalue in RecentWatched)
            {
                total = total.Union(keyvalue.Value).ToList();
            }


            RecentWatchedCount = total.Count;

        }









        /// <summary>
        /// 在数据库中搜索影片
        /// </summary>
        public async  Task<bool> Query()
        {
            if (!DataBase.IsTableExist("movie")) { return false; }

            IsSearching = true;
            if (Search == "") return false;

            string FormatSearch = Search.ToProperSql();

            if (string.IsNullOrEmpty(FormatSearch)) { return false; }

            string fanhao;
            if (CurrentVedioType == VedioType.欧美)
                fanhao = Identify.GetEuFanhao(FormatSearch);
            else
                fanhao = Identify.GetFanhao(FormatSearch);

            string searchContent;
            if (string.IsNullOrEmpty(fanhao)) searchContent = FormatSearch;
            else searchContent = fanhao;

            List<Movie> oldMovieList = MovieList.ToList();

            if (AllSearchType== MySearchType.识别码)
            {
                TextType =Jvedio.Language.Resources.Search + Jvedio.Language.Resources.ID + searchContent;

                if (SearchInCurrent)
                    MovieList = oldMovieList.Where(arg => arg.id.IndexOf(searchContent) >= 0).ToList();
                else
                    MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where id like '%{searchContent}%'");

                
            }

            else if (AllSearchType == MySearchType.名称)
            {
                TextType = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.Title + searchContent;
                if (SearchInCurrent)
                    MovieList = oldMovieList.Where(arg => arg.title.IndexOf(searchContent) >= 0).ToList();
                else
                    MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where title like '%{searchContent}%'");
            }

            else if (AllSearchType == MySearchType.演员)
            {
                TextType = Jvedio.Language.Resources.Search + Jvedio.Language.Resources.Actor + searchContent;
                if (SearchInCurrent)
                    MovieList = oldMovieList.Where(arg => arg.actor.IndexOf(searchContent) >= 0).ToList();
                else
                    MovieList = DataBase.SelectPartialInfo($"SELECT * FROM movie where actor like '%{searchContent}%'");
            }




            return true;
        }


        public void RandomDisplay()
        {
            TextType = "随机展示";
            Statistic();
            MovieList = DataBase.SelectMoviesBySql($"SELECT * FROM movie ORDER BY RANDOM() limit {Properties.Settings.Default.DisplayNumber}");
            FlipOver();
        }



        public void ExecutiveSqlCommand(int sideIndex, string textType, string sql,string dbName="")
        {

            Dictionary<string, string> sqlInfo = new Dictionary<string, string>
            {
                { "SideIndex", sideIndex.ToString() },
                { "TextType", textType },
                { "SqlCommand", sql }
            };
            SqlCommands.Add(sqlInfo);
            SqlIndex = SqlCommands.Count - 1;
            TextType = textType;

            string viewText = "";
            int.TryParse(Properties.Settings.Default.ShowViewMode, out int vm);
            if (vm == 1)
            {
                viewText = Jvedio.Language.Resources.WithImage;
            }else if (vm == 2)
            {
                viewText = Jvedio.Language.Resources.NoImage;
            }

            if (vm!=0) TextType = TextType + "，" + viewText;
            if (Properties.Settings.Default.OnlyShowPlay ) TextType = TextType + "，"  +Jvedio.Language.Resources.Playable;
            if (Properties.Settings.Default.OnlyShowSubSection) TextType = TextType + "，" + Jvedio.Language.Resources.OnlyShowSubsection;

            Task.Run(() =>
            {
                if (sideIndex == 5 || sideIndex == 6 || sideIndex == 7)
                {
                    var movies = DataBase.SelectMoviesBySql(sql);
                    MovieList = new List<Movie>();
                    if (sideIndex == 5)
                        movies?.ForEach(arg => { if (arg.genre.Split(' ').Any(m => m.ToUpper() == textType.ToUpper())) MovieList.Add(arg); });
                    else if (sideIndex == 6)
                        movies?.ForEach(arg => { if (arg.actor.Split(' ').Any(m => m.ToUpper() == textType.ToUpper())) MovieList.Add(arg); });
                    else if (sideIndex == 7)
                        movies?.ForEach(arg => { if (arg.label.Split(' ').Any(m => m.ToUpper() == textType.ToUpper())) MovieList.Add(arg); });

                    CurrentPage = 1;
                }
                else
                {
                    MovieList = DataBase.SelectMoviesBySql(sql,dbName);
                }
                Record.Add(MovieList.Select(arg=>arg.id).ToList());
                RecordIndex = Record.Count-1;
                Statistic();
                FlipOver();
            });


        }

        public void SwitchSqlCommand()
        {
            if (SqlCommands.Count > 0 && SqlIndex >= 0 && SqlIndex < SqlCommands.Count)
            {

                Dictionary<string, string> sqlInfo = SqlCommands[SqlIndex];
                int idx = int.Parse(sqlInfo["SideIndex"]);
                if (idx == 3)
                {
                    GetRecentWatch(false);
                    SetSideButtonChecked(3);
                }
                else if (idx == 5 || idx == 6 || idx == 7)
                {
                    Statistic();
                    var movies = DataBase.SelectMoviesBySql(sqlInfo["SqlCommand"]);
                    MovieList = new List<Movie>();

                    if (idx == 5)
                        movies?.ForEach(arg => { if (arg.genre.Split(' ').Any(m => m.ToUpper() == textType.ToUpper())) MovieList.Add(arg); });
                    else if (idx == 6)
                        movies?.ForEach(arg => { if (arg.actor.Split(' ').Any(m => m.ToUpper() == textType.ToUpper())) MovieList.Add(arg); });
                    else if (idx == 7)
                        movies?.ForEach(arg => { if (arg.label.Split(' ').Any(m => m.ToUpper() == textType.ToUpper())) MovieList.Add(arg); });

                    CurrentPage = 1;

                    Record.Add(MovieList.Select(arg => arg.id).ToList());
                    RecordIndex = Record.Count-1;

                    FlipOver();
                    SetSideButtonChecked(5);
                }
                else
                {
                    int.TryParse(sqlInfo["SideIndex"], out int sideIndex);
                    TextType = sqlInfo["TextType"];
                    Task.Run(() =>
                    {
                        Statistic();
                        MovieList = DataBase.SelectPartialInfo(sqlInfo["SqlCommand"]);

                        Record.Add(MovieList.Select(arg => arg.id).ToList());
                        RecordIndex = Record.Count - 1;

                        FlipOver();
                    });
                    SetSideButtonChecked(sideIndex);
                }

            }

        }


        private void SetSideButtonChecked(int idx)
        {
            Main main = App.Current.Windows[0] as Main;
            if (idx == 0)
            {
                main.AllRB.IsChecked = true;
            }
            else if (idx == 1)
            {
                main.LoveRB.IsChecked = true;
            }
            else if (idx == 2)
            {
                main.CreateRB.IsChecked = true;
            }
            else if (idx == 3)
            {
                main.PlayRB.IsChecked = true;
            }
            else if (idx == 4)
            {
                main.GenreRB.IsChecked = true;
            }
            else if (idx == 5)
            {
                main.ActorRB.IsChecked = true;
            }
            else if (idx == 6)
            {
                main.LabelRB.IsChecked = true;
            }
            else if (idx == 7)
            {
                main.DirRB.IsChecked = true;
            }
            else if (idx == 8)
            {
                main.TypeRB1.IsChecked = true;
            }
            else if (idx == 9)
            {
                main.TypeRB2.IsChecked = true;
            }
            else if (idx == 9)
            {
                main.TypeRB3.IsChecked = true;
            }


        }



        public void Sort()
        {
            if (MovieList != null)
            {
                List<Movie> sortMovieList = new List<Movie>();
                bool SortDescending = Properties.Settings.Default.SortDescending;
                int.TryParse(Properties.Settings.Default.SortType, out int sortindex);
                switch (sortindex)
                {
                    case 0:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.id).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.id).ToList(); }
                        break;
                    case 1:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.filesize).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.filesize).ToList(); }
                        break;
                    case 2:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.scandate).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.scandate).ToList(); }
                        break;
                    case 3:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.otherinfo).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.otherinfo).ToList(); }
                        break;
                    case 4:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.favorites).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.favorites).ToList(); }
                        break;
                    case 5:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.title).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.title).ToList(); }
                        break;
                    case 6:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.visits).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.visits).ToList(); }
                        break;
                    case 7:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.releasedate).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.releasedate).ToList(); }
                        break;
                    case 8:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.rating).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.rating).ToList(); }
                        break;
                    case 9:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.runtime).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.runtime).ToList(); }
                        break;
                    case 10:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.actor.Split(new char[] { ' ', '/' })[0]).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.actor.Split(new char[] { ' ', '/' })[0]).ToList(); }
                        break;
                    default:
                        if (SortDescending) { sortMovieList = MovieList.OrderByDescending(o => o.id).ToList(); } else { sortMovieList = MovieList.OrderBy(o => o.id).ToList(); }
                        break;
                }
                MovieList = new List<Movie>();
                sortMovieList.ForEach(arg => { MovieList.Add(arg); });
            }
            
        }




        public void Reset()
        {
            SideIdx = 0;
            ExecutiveSqlCommand(0, "所有视频", "SELECT * FROM movie");

        }

        public void GetFavoritesMovie()
        {
            SideIdx = 1;
            ExecutiveSqlCommand(1, "我的喜爱", "SELECT * from movie where favorites>0 and favorites<=5");
        }

        public void GetRecentMovie()
        {
            SideIdx = 2;
            string date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays).Date.ToString("yyyy-MM-dd");
            string date2 = DateTime.Now.ToString("yyyy-MM-dd");
            ExecutiveSqlCommand(2, $"{Properties.Settings.Default.RecentDays} 天内的最近创建", $"SELECT * from movie WHERE scandate BETWEEN '{date1}' AND '{date2}'");
        }

        public void GetRecentWatch(bool add = true)
        {
            SideIdx = 3;
            List<Movie> movies = new List<Movie>();
            foreach (var keyValuePair in RecentWatched)
            {
                if (keyValuePair.Key <= DateTime.Now && keyValuePair.Key >= DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays))
                {
                    foreach (var item in keyValuePair.Value)
                    {
                        Movie movie = DataBase.SelectMovieByID(item);
                        if (movie != null) movies.Add(movie);
                        

                    }
                }
            }

            TextType = $"{Properties.Settings.Default.RecentDays} 天内的最近播放";
            Statistic();
            MovieList = new List<Movie>();
            MovieList.AddRange(movies);
            CurrentPage = 1;

            Record.Add(MovieList.Select(arg => arg.id).ToList());
            RecordIndex = Record.Count - 1;

            FlipOver();
            //if (MovieList.Count == 0 && RecentWatchedCount > 0)
            //    HandyControl.Controls.Growl.Info("该库中无最近播放，请切换到其他库！");
            //else if (MovieList.Count < RecentWatchedCount)
            //    HandyControl.Controls.Growl.Info($"该库中仅发现 {MovieList.Count} 个最近播放");
            if (add)
            {
                Dictionary<string, string> sqlInfo = new Dictionary<string, string>
                {
                    { "SideIndex", "3" },
                    { "TextType", TextType },
                    { "SqlCommand", "RecentWatch" }
                };
                SqlCommands.Add(sqlInfo);
                SqlIndex = SqlCommands.Count - 1;
            }

        }



        public void GetSamePathMovie(string path)
        {
            //Bug: 大数据库卡死
            TextType = path;
            List<Movie> movies = DataBase.SelectMoviesBySql($"SELECT * from movie WHERE filepath like '%{path}%'");
            MovieList = new List<Movie>();
            MovieList.AddRange(movies);
            CurrentPage = 1;

            Record.Add(MovieList.Select(arg => arg.id).ToList());
            RecordIndex = Record.Count - 1;

            FlipOver();
        }



        public void GetMoviebyStudio(string moviestudio)
        {
            ExecutiveSqlCommand(0, moviestudio, $"SELECT * from movie where studio = '{moviestudio}'");
        }

        public void GetMoviebyTag(string movietag)
        {
            ExecutiveSqlCommand(0, movietag, $"SELECT * from movie where tag like '%{movietag}%'");
        }

        public void GetMoviebyDirector(string moviedirector)
        {
            ExecutiveSqlCommand(0, moviedirector, $"SELECT * from movie where director ='{moviedirector}'");
        }


        public void GetMoviebyGenre(string moviegenre)
        {
            ExecutiveSqlCommand(5, moviegenre, "SELECT * from movie where genre like '%" + moviegenre + "%'");
        }

        public void GetMoviebyLabel(string movielabel)
        {
            ExecutiveSqlCommand(7, movielabel, "SELECT * from movie where label like '%" + movielabel + "%'");
        }



        public void GetMoviebyActress(Actress actress)
        {
            Statistic();
            int vediotype = (int)VedioType;
            //根据视频类型选择演员

            List<Movie> movies;
            if (actress.id == "")
                movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{actress.name}%'");
            else
                movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actorid like '%{actress.id}%'");


            MovieList = new List<Movie>();
            movies?.ForEach(arg =>
            {
                if (arg.actor.Split(actorSplitDict[arg.vediotype]).Any(m => m.ToUpper() == actress.name.ToUpper())) MovieList.Add(arg);
            });

            Record.Add(MovieList.Select(arg => arg.id).ToList());
            RecordIndex = Record.Count - 1;

            CurrentPage = 1;
            FlipOver();
        }


        //根据视频类型选择演员
        public void GetMoviebyActressAndVetioType(Actress actress)
        {
            Statistic();
            List<Movie> movies;
            if (actress.id == "")
            {
                if (VedioType == 0) { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{actress.name}%'"); }
                else { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actor like '%{actress.name}%' and vediotype={(int)VedioType}"); }
            }
            else
            {
                if (VedioType == 0) { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actorid like '%{actress.id}%'"); }
                else { movies = DataBase.SelectMoviesBySql($"SELECT * from movie where actorid like '%{actress.id}%' and vediotype={(int)VedioType}"); }
            }


            MovieList = new List<Movie>();
            if (movies != null || movies.Count > 0)
            {
                movies.ForEach(arg =>
                {
                    try { if (arg.actor.Split(actorSplitDict[arg.vediotype]).Any(m => m.ToUpper() == actress.name.ToUpper())) MovieList.Add(arg); }
                    catch (Exception e)
                    {
                        Logger.LogE(e);
                    }
                });

            }
            CurrentPage = 1;
            Record.Add(MovieList.Select(arg => arg.id).ToList());
            RecordIndex = Record.Count - 1;

        }





        public void GetMoviebyVedioType(VedioType vt)
        {
            CurrentVedioType = vt;
            TextType = vt.ToString();
            Statistic();
            MovieList = DataBase.SelectPartialInfo("SELECT * from movie where vediotype=" + (int)vt);
            FlipOver();
        }





        public void ShowDetailsData()
        {
            Task.Run(() =>
            {

                TextType = "详情模式";
                Statistic();
                List<Movie> movies = new List<Movie>();

                TotalPage = (int)Math.Ceiling((double)MovieList.Count / (double)Properties.Settings.Default.DisplayNumber);
                if (MovieList != null && MovieList.Count > 0)
                {
                    MovieList.ForEach(arg =>
                    {
                        Movie movie = DataBase.SelectMovieByID(arg.id);
                        if (movie != null) movies.Add(movie);
                    });

                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //App.Current.Windows[0].Cursor = Cursors.Wait;
                        DetailsDataList = new ObservableCollection<Movie>();
                        DetailsDataList.AddRange(movies);
                    });


                }
                CurrentCount = DetailsDataList.Count;
                TotalCount = MovieList.Count;
                IsFlipOvering = false;
                //App.Current.Windows[0].Cursor = Cursors.Arrow;
            });
        }


        /// <summary>
        /// 统计：加载时间 <70ms (15620个信息)
        /// </summary>
        public void Statistic()
        {
            if (!DataBase.IsTableExist("movie")) { return; }

            stopwatch.Restart();
            AllVedioCount = DataBase.SelectCountBySql("");
            FavoriteVedioCount = DataBase.SelectCountBySql("where favorites>0 and favorites<=5");
            VedioTypeACount = DataBase.SelectCountBySql("where vediotype=1");
            VedioTypeBCount = DataBase.SelectCountBySql("where vediotype=2");
            VedioTypeCCount = DataBase.SelectCountBySql("where vediotype=3");

            string date1 = DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays).Date.ToString("yyyy-MM-dd");
            string date2 = DateTime.Now.ToString("yyyy-MM-dd");
            RecentVedioCount = DataBase.SelectCountBySql($"WHERE scandate BETWEEN '{date1}' AND '{date2}'");
            stopwatch.Stop();
            //Console.WriteLine($"\n统计用时：{stopwatch.ElapsedMilliseconds} ms");
        }



        public void LoadFilePathClassfication()
        {
            //加载路经筛选
            FilePathClassification = new ObservableCollection<string>();
            foreach (Movie movie in MovieList)
            {
                string path = GetPathByDepth(movie.filepath, Properties.Settings.Default.FilePathClassificationMaxDepth);
                if (!string.IsNullOrEmpty(path) && !FilePathClassification.Contains(path)) FilePathClassification.Add(path);
                if (FilePathClassification.Count > Properties.Settings.Default.FilePathClassificationMaxCount) break;
            }
        }

        private string GetPathByDepth(string path, int depth)
        {

            if (string.IsNullOrEmpty(path) || path.IndexOf("\\") < 0) return "";
            string[] paths = path.Split('\\');
            string result = "";
            for (int i = 0; i < paths.Length - 1; i++)
            {
                result += paths[i] + "\\";
                if (i >= depth - 1) break;
            }
            return result;



        }



    }
}
