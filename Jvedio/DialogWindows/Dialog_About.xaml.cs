﻿using Jvedio.ViewModel;
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
using static Jvedio.FileProcess;
using System.Windows.Documents;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_About : BaseDialog
    {

        public Dialog_About(Window owner,bool showbutton) : base(owner, showbutton)
        {
            InitializeComponent();

        }

        private void OpenUrl(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            FileHelper.TryOpenUrl(hyperlink.NavigateUri.ToString());
        }

        private void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            VersionTextBlock.Text = Jvedio.Language.Resources.Version + $" : {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        }
    }
}