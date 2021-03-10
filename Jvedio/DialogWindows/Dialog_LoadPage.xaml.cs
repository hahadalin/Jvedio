using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMethod;
using System.Windows.Documents;
using System.Windows.Media;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_LoadPage : BaseDialog
    {

        public List<ActorSearch> ActorSearches;
        public string url = "";
        public int VedioType = 1;
        public int StartPage = 1;
        public int EndPage = 500;

        public Dialog_LoadPage(Window owner,bool showbutton) : base(owner, showbutton)
        {
            InitializeComponent();
        }



        private void SaveVedioType(object sender, RoutedEventArgs e)
        {
            var rbs = VedioTypeStackPanel.Children.OfType<RadioButton>().ToList();
            RadioButton rb = sender as RadioButton;
            VedioType = rbs.IndexOf(rb) +1;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StartPage =(int)e.NewValue;
        }

        private void SliderEnd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EndPage = (int)e.NewValue;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            url = ((TextBox)sender).Text;
        }
    }
}