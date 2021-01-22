using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System.IO;
using Jvedio.Plot.Bar;

namespace Jvedio.ViewModel
{
    public class VieModel_DBManagement : ViewModelBase
    {
        protected List<Movie> Movies;

        public VieModel_DBManagement()
        {
            ListDatabaseCommand = new RelayCommand(ListDatabase);
            StatisticCommand = new RelayCommand(Statistic);


        }

        #region "RelayCommand"
        public RelayCommand ListDatabaseCommand { get; set; }
        public RelayCommand StatisticCommand { get; set; }

        #endregion





        public void ListDatabase()
        {
            DataBases = new ObservableCollection<string>();
            try
            {
                var files = Directory.GetFiles("DataBase", "*.sqlite", SearchOption.TopDirectoryOnly).ToList();

                foreach (var item in files)
                {
                    string name = Path.GetFileNameWithoutExtension(item);
                    if (!string.IsNullOrEmpty(name))
                        DataBases.Add(name);
                }
            }
            catch { }
            if (!DataBases.Contains("info")) DataBases.Add("info");

        }






        public  void Statistic()
        {
            Movies = new List<Movie>();
            string name = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower();
            if (name != "info") name = "DataBase\\" + name;
            MySqlite db = new MySqlite(name);
            Movies =  db.SelectMoviesBySql("SELECT * FROM movie");
            db.CloseDB();

            AllCount = Movies.Count;
            UncensoredCount = Movies.Where(arg => arg.vediotype == 1).Count();
            CensoredCount = Movies.Where(arg => arg.vediotype == 2).Count();
            EuropeCount = Movies.Where(arg => arg.vediotype == 3).Count();

            CensoredCountPercent = (int)(100 * CensoredCount / (AllCount == 0 ? 1 : AllCount));
            UncensoredCountPercent = (int)(100 * UncensoredCount / (AllCount == 0 ? 1 : AllCount));
            EuropeCountPercent = (int)(100 * EuropeCount / (AllCount == 0 ? 1 : AllCount));
        }

        public List<BarData> LoadActor()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                arg.actor.Split(new char[] { ' ', '/' }).ToList().ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!dic.ContainsKey(item))
                            dic.Add(item, 1);
                        else
                            dic[item] += 1;
                    }

                });
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();
        }

        public List<BarData> LoadTag()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                arg.tag.Split(' ').ToList().ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!dic.ContainsKey(item))
                            dic.Add(item, 1);
                        else
                            dic[item] += 1;
                    }
                });
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();
        }

        public List<BarData> LoadGenre()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                arg.genre.Split(' ').ToList().ForEach(item =>
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (!dic.ContainsKey(item))
                            dic.Add(item, 1);
                        else
                            dic[item] += 1;
                    }
                });
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();

        }

        public List<BarData> LoadID()
        {
            Dictionary<string, double> dic = new Dictionary<string, double>();
            Movies.ForEach(arg =>
            {
                string id = "";
                if (arg.vediotype == 3)
                    id = Identify.GetEuFanhao(arg.id).Split('.')[0];
                else
                    id = Identify.GetFanhao(arg.id).Split('-')[0];
                if (!dic.ContainsKey(id))
                    dic.Add(id, 1);
                else
                    dic[id] += 1;
            });

            var dicSort = dic.OrderByDescending(arg => arg.Value).ToDictionary(x => x.Key, y => y.Value);
            return dicSort.ToBarDatas();
        }






        private string[] _ActorLabels;

        public string[] ActorLabels
        {

            get { return _ActorLabels; }
            set
            {
                _ActorLabels = value;
                RaisePropertyChanged();
            }
        }






        private string[] _TagLabels;

        public string[] TagLabels
        {

            get { return _TagLabels; }
            set
            {
                _TagLabels = value;
                RaisePropertyChanged();
            }
        }


        private string[] _GenreLabels;

        public string[] GenreLabels
        {

            get { return _GenreLabels; }
            set
            {
                _GenreLabels = value;
                RaisePropertyChanged();
            }
        }



        private string[] _Labels;

        public string[] Labels
        {

            get { return _Labels; }
            set
            {
                _Labels = value;
                RaisePropertyChanged();
            }
        }





        private double _CensoredCountPercent;

        public double CensoredCountPercent
        {
            get { return _CensoredCountPercent; }
            set
            {
                _CensoredCountPercent = value;
                RaisePropertyChanged();
            }


        }

        private double _UncensoredCountPercent;

        public double UncensoredCountPercent
        {
            get { return _UncensoredCountPercent; }
            set
            {
                _UncensoredCountPercent = value;
                RaisePropertyChanged();
            }


        }

        private double _EuropeCountPercent;

        public double EuropeCountPercent
        {
            get { return _EuropeCountPercent; }
            set
            {
                _EuropeCountPercent = value;
                RaisePropertyChanged();
            }


        }


        private double _AllCount;

        public double AllCount
        {
            get { return _AllCount; }
            set
            {
                _AllCount = value;
                RaisePropertyChanged();
            }


        }

        private double _censoredCount;

        public double CensoredCount
        {
            get { return _censoredCount; }
            set
            {
                _censoredCount = value;
                RaisePropertyChanged();
            }


        }

        private double _UncensoredCount;

        public double UncensoredCount
        {
            get { return _UncensoredCount; }
            set
            {
                _UncensoredCount = value;
                RaisePropertyChanged();
            }


        }

        private double _EuropeCount;

        public double EuropeCount
        {
            get { return _EuropeCount; }
            set
            {
                _EuropeCount = value;
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

        private string _CurrentDataBase;

        public string CurrentDataBase
        {
            get { return _CurrentDataBase; }
            set
            {
                _CurrentDataBase = value;
                RaisePropertyChanged();
            }


        }





    }
}
