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
                    System.Diagnostics.Process.Start("upgrade.bat");
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

        private async void OpenUpdate(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, Jvedio.Language.Resources.IsToUpdate).ShowDialog() == true)
            {
                try
                {
                    //检查升级程序是否是最新的
                    
                    bool IsToDownLoadUpdate = false;
                    HttpResult httpResult = await Net.Http(Net.UpdateExeVersionUrl); 

                    if (httpResult != null && httpResult.SourceCode!="")
                    {
                        //跟本地的 md5 对比
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe")) { IsToDownLoadUpdate = true; }
                        else
                        {
                            string md5 = Encrypt.GetFileMD5(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe");
                            if (md5 != httpResult.SourceCode) { IsToDownLoadUpdate = true; }
                        }
                    }
                    if (IsToDownLoadUpdate)
                    {
                        HttpResult streamResult = await Net.DownLoadFile(Net.UpdateExeUrl);
                        try
                        {
                            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe", FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(streamResult.FileByte, 0, streamResult.FileByte.Length);
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        Process.Start(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe");
                    }
                    catch (Exception ex)
                    {
                        HandyControl.Controls.Growl.Error(ex.Message, "Main");
                    }

                    //IsToUpdate = true;
                    Application.Current.Shutdown();//直接关闭
                }
                catch { MessageBox.Show($"{Jvedio.Language.Resources.CannotOpen} JvedioUpdate.exe"); }

            }
        }
        private void GotoDownloadUrl(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/hitchao/Jvedio/releases");
        }



        private async void BaseDialog_ContentRendered(object sender, EventArgs e)
        {
            UpgradeSourceTextBlock.Text = $"{Jvedio.Language.Resources.UpgradeSource}：{Net.UpgradeSource}";
            LocalVersionTextBlock.Text = $"{Jvedio.Language.Resources.CurrentVersion}：{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
            if (remote != "")
            {
                RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                UpdateContentTextBox.Text = log;
                UpgradeLoadingCircle.Visibility = Visibility.Hidden;
            }
            else
            {

                (bool success, string remote, string updateContent) = await Net.CheckUpdate();
                string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (success )
                {
                    RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                    UpdateContentTextBox.Text = updateContent;
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
                UpdateContentTextBox.Text = updateContent;
            }
            UpgradeLoadingCircle.Visibility = Visibility.Hidden;

            button.IsEnabled = true;
        }
    }
}