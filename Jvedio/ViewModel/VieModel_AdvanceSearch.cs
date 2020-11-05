
using DynamicData;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.ViewModel
{
    public class VieModel_AdvanceSearch : ViewModelBase
    {

            public VieModel_AdvanceSearch()
        {
            ResetCommand = new RelayCommand(Reset);
        }


        public  void Reset()
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

            Label.AddRange(models[3]);
            Runtime.AddRange(models[4]);
            FileSize.AddRange(models[5]);
            Rating.AddRange(models[6]);

                Genre.AddRange(models[1]);
                Actor.AddRange(models[2]);


            


        }



        public RelayCommand ResetCommand { get; set; }


        private string _SqlText;

        public string SqlText
        {
            get { return _SqlText; }
            set
            {
                _SqlText = value;
                RaisePropertyChanged();
            }
        }




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



    }
}
