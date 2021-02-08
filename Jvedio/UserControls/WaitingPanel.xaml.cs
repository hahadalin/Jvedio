using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jvedio.Controls
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingPanel : UserControl
    {
        public event RoutedEventHandler Cancel;

        public static readonly DependencyProperty ShowCancelButtonProperty = DependencyProperty.Register(
            "ShowCancelButton", typeof(Visibility), typeof(WaitingPanel), new PropertyMetadata(Visibility.Visible));

        public Visibility ShowCancelButton
        {
            get { return (Visibility)GetValue(ShowCancelButtonProperty); }
            set { SetValue(ShowCancelButtonProperty, value);
            }
        }

    //    public static new readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(
    //"Visibility", typeof(Visibility), typeof(WaitingPanel), new PropertyMetadata(Visibility.Visible));

    //    public new Visibility Visibility
    //    {
    //        get { return (Visibility)GetValue(VisibilityProperty); }
    //        set
    //        {
    //            SetValue(VisibilityProperty, value);
    //        }
    //    }


        public WaitingPanel()
        {
            InitializeComponent();
        }
        

        void onButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Cancel != null)
            {
                this.Cancel(this, e);
            }
        }
    }
}
