using Jvedio.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Jvedio.StaticVariable;
using static Jvedio.StaticClass;
using System.Data;
using System.Windows.Controls.Primitives;
using FontAwesome.WPF;
using System.ComponentModel;
using DynamicData.Annotations;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class WindowBatch : Jvedio_BaseWindow
    {
        public WindowBatch()
        {
            InitializeComponent();

            var childsps = MainGrid.Children.OfType<Grid>().ToList();
            foreach (var item in childsps) item.Visibility = Visibility.Hidden;
            var RadioButtons = RadioButtonStackPanel.Children.OfType<RadioButton>().ToList();

            childsps[Properties.Settings.Default.BatchIndex].Visibility = Visibility.Visible;
            RadioButtons[Properties.Settings.Default.BatchIndex].IsChecked = true;






        }

        private void ShowGrid(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            StackPanel SP = radioButton.Parent as StackPanel;
            var radioButtons = SP.Children.OfType<RadioButton>().ToList();
            var childsps = MainGrid.Children.OfType<Grid>().ToList();
            foreach (var item in childsps) item.Visibility = Visibility.Hidden;
            childsps[radioButtons.IndexOf(radioButton)].Visibility = Visibility.Visible;
            Properties.Settings.Default.BatchIndex = radioButtons.IndexOf(radioButton);
            Properties.Settings.Default.Save();
        }
    }



    }
