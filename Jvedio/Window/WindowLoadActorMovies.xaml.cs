using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using static Jvedio.FileProcess;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class LoadActorMovies : Jvedio_BaseWindow
    {

        public List<Server> Servers;
        public LoadActorMovies()
        {
            InitializeComponent();



        }

        private void OpenUrl(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void Jvedio_BaseWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Reset();
            ComboBox.Items.Clear();
            foreach (Server server in Servers)
            {
                if(server!=null && !string.IsNullOrEmpty(server.Url) && !string.IsNullOrEmpty(server.ServerTitle))
                {
                    ComboBoxItem comboBoxItem = new ComboBoxItem();
                    comboBoxItem.Content = server.ServerTitle;
                    ComboBox.Items.Add(comboBoxItem);
                }
            }
            ComboBox.SelectedIndex = 0;
        }


        public void Reset()
        {
            Servers = new List<Server>();
            if (Properties.Settings.Default.Bus != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.Bus);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableBus, Url = Properties.Settings.Default.Bus, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.BusEurope != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.BusEu);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableBusEu, Url = Properties.Settings.Default.BusEurope, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.DB != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.DB);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableDB, Url = Properties.Settings.Default.DB, Cookie = Properties.Settings.Default.DBCookie, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.FC2 != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.FC2);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableFC2, Url = Properties.Settings.Default.FC2, Cookie = "", Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.Library != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.Library);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableLibrary, Url = Properties.Settings.Default.Library, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }

            if (Properties.Settings.Default.DMM != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.DMM);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.EnableDMM, Url = Properties.Settings.Default.DMM, Cookie = Properties.Settings.Default.DMMCookie, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }
            if (Properties.Settings.Default.Jav321 != "")
            {
                List<string> infos = ReadServerInfoFromConfig(WebSite.Jav321);
                Servers.Add(new Server() { IsEnable = Properties.Settings.Default.Enable321, Url = Properties.Settings.Default.Jav321, Available = 0, ServerTitle = infos[1], LastRefreshDate = infos[2] });
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string url = "";
            foreach (Server server in Servers)
            {
                if (server != null && !string.IsNullOrEmpty(server.Url) && !string.IsNullOrEmpty(server.ServerTitle))
                {
                    if (server.ServerTitle == ComboBox.Text)
                    {
                        url = server.Url;
                        break;
                    }
                }
            }

            string acotr = ActorTextBlock.Text.Replace("演员：", "");
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(acotr)) return;
            //检索影片





         }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}