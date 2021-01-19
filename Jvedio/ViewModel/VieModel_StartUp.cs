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
using LiveCharts;
using LiveCharts.Wpf;

namespace Jvedio.ViewModel
{
    public class VieModel_StartUp : ViewModelBase
    {
        protected List<Movie> Movies;

        public VieModel_StartUp()
        {
            ListDatabaseCommand = new RelayCommand(ListDatabase);


        }

        #region "RelayCommand"
        public RelayCommand ListDatabaseCommand { get; set; }

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
            if (!DataBases.Contains(Jvedio.Language.Resources.NewLibrary)) DataBases.Add(Jvedio.Language.Resources.NewLibrary);


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






    }
}
