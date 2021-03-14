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
using static Jvedio.FileProcess;
using System.Windows.Documents;
using Jvedio.Library.Encrypt;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Upgrade : BaseDialog
    {


        private string remote = "";
        private string log = "";
        public Dialog_Upgrade(Window owner,bool showbutton,string remote,string log) : base(owner, showbutton)
        {
            InitializeComponent();
            this.remote = remote;
            this.log = log;
        }


        Upgrade upgrade;
        private void BeginUpgrade(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string text = button.Content.ToString();
            if (text == Jvedio.Language.Resources.BeginUpgrade)
            {
                button.Content = Jvedio.Language.Resources.StopUpgrade;
                upgrade = new Upgrade();
                upgrade.UpgradeCompleted += (s, _) =>
                {
                    UpgradeLoadingCircle.Visibility = Visibility.Hidden;
                    //执行命令
                    string arg = "xcopy /y/e Temp %cd%&TIMEOUT /T 1&start \"\" \"jvedio.exe\" &exit";
                    using (StreamWriter sw = new StreamWriter("upgrade.bat"))
                    {
                        sw.Write(arg);
                    }
                    FileHelper.TryOpenFile("upgrade.bat");
                    Application.Current.Shutdown();
                };

                upgrade.onProgressChanged += (s, _) =>
                {
                    ProgressBUpdateEventArgs ev = _ as ProgressBUpdateEventArgs;
                    UpgradeProgressStackPanel.Visibility = Visibility;
                    if (ev.maximum != 0)
                    {
                        UpgradeProgressBar.Value = (int)(ev.value / ev.maximum * 100);
                    }

                };
                button.Style = (Style)App.Current.Resources["ButtonDanger"];
                UpgradeProgressBar.Value = 0;
                UpgradeLoadingCircle.Visibility = Visibility.Visible;
                upgrade.Start();
                UpgradeProgressStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                button.Content = Jvedio.Language.Resources.BeginUpgrade;
                button.Style = (Style)App.Current.Resources["ButtonStyleFill"];
                upgrade?.Stop();
                UpgradeProgressStackPanel.Visibility = Visibility.Collapsed;
                UpgradeLoadingCircle.Visibility = Visibility.Collapsed;
            }





        }

        private void GotoDownloadUrl(object sender, RoutedEventArgs e)
        {
            FileHelper.TryOpenUrl(ReleaseUrl);
        }



        private async void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            UpgradeProgressStackPanel.Visibility = Visibility.Collapsed;
            UpgradeSourceTextBlock.Text = $"{Jvedio.Language.Resources.UpgradeSource}：{Net.UpgradeSource}";
            LocalVersionTextBlock.Text = $"{Jvedio.Language.Resources.CurrentVersion}：{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            if (remote != "")
            {
                RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                UpdateContentTextBox.Text = GetContentByLanguage(log);
                UpgradeLoadingCircle.Visibility = Visibility.Hidden;
            }
            else
            {

                (bool success, string remote, string updateContent) = await Net.CheckUpdate();
                string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (success )
                {
                    RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                    UpdateContentTextBox.Text = GetContentByLanguage(updateContent);
                }
                UpgradeLoadingCircle.Visibility = Visibility.Hidden;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            button.IsEnabled = false;

            UpgradeLoadingCircle.Visibility = Visibility.Visible;
            (bool success, string remote, string updateContent) = await Net.CheckUpdate();
            string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (success)
            {
                RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                UpdateContentTextBox.Text = GetContentByLanguage(updateContent);
            }
            UpgradeLoadingCircle.Visibility = Visibility.Hidden;

            button.IsEnabled = true;
        }

        private string GetContentByLanguage(string content)
        {
            int start = -1;
            int end = -1;
            switch (Properties.Settings.Default.Language)
            {

                case "中文":
                    end = content.IndexOf("--English--");
                    if (end == -1) return content;
                    else return content.Substring(0, end).Replace("--中文--", "");

                case "English":
                    start = content.IndexOf("--English--");
                    end = content.IndexOf("--日本語--");
                    if (end == -1 || start == -1) return content;
                    else return content.Substring(start, end - start).Replace("--English--", "");

                case "日本語":
                    start = content.IndexOf("--日本語--");
                    if (start == -1) return content;
                    else return content.Substring(start).Replace("--日本語--", "");

                default:
                    return content;
            }
        }
    }
}