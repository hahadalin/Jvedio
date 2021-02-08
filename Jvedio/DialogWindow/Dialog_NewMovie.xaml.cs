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
namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_NewMovie : BaseDialog
    {
        public NewMovieDialogResult Result { get; private set; }
        public Dialog_NewMovie(Window owner,double width,double height):base(owner,width, height)
        {
            InitializeComponent();
        }

        public Dialog_NewMovie(Window owner) : base(owner)
        {
            InitializeComponent();
        }

        protected override void Confirm(object sender, RoutedEventArgs e)
        {
            var rbs = RadioButtonStackPanel.Children.OfType<RadioButton>().ToList();
            int idx = rbs.FindIndex(arg => arg.IsChecked == true);
            Result = new NewMovieDialogResult(AddMovieTextBox.Text, idx);
            base.Confirm(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowTools windowTools = (WindowTools)GetWindowByName("WindowTools");
            if (windowTools == null) windowTools = new WindowTools();
            windowTools.Show();
            windowTools.Activate();
            windowTools.TabControl.SelectedIndex = 0;
            this.Close();
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            AddMovieTextBox.Focus();
        }
    }


    public class NewMovieDialogResult : JvedioDialogResult
    {

        public VedioType VedioType {get;set;}
        public NewMovieDialogResult(string text, int option) : base(text, option)
        {
            VedioType = (VedioType)(option+1);
        }
    }
}