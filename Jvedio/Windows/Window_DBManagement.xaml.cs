using Jvedio.Plot.Bar;
using Jvedio.ViewModel;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static Jvedio.FileProcess;
using static Jvedio.GlobalMethod;
namespace Jvedio
{
    /// <summary>
    /// Window_DBManagement.xaml 的交互逻辑
    /// </summary>
    public partial class Window_DBManagement : Jvedio_BaseWindow
    {


        public CancellationTokenSource cts;
        public CancellationToken ct;

        public VieModel_DBManagement vieModel_DBManagement;
        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance = null;

        private string src = "";
        private string dst = "";
        public Window_DBManagement()
        {
            InitializeComponent();

            vieModel_DBManagement = new VieModel_DBManagement();
            vieModel_DBManagement.ListDatabase();

            this.DataContext = vieModel_DBManagement;
            vieModel_DBManagement.CurrentDataBase = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);

            this.SizedChangedCompleted += OnSizedChangedCompleted;
        }


        private void OnSizedChangedCompleted(object o, EventArgs e)
        {
            ShowStatistic();
        }




        public void LoadDataBase(object sender, MouseButtonEventArgs e)
        {

            string name = "";
            Border border = sender as Border;
            Grid grid = border.Parent as Grid;
            Grid grid1 = grid.Parent as Grid;
            TextBlock textBlock = grid1.Children[1] as TextBlock;
            name = textBlock.Text.ToLower();

            Main main = GetWindowByName("Main") as Main;
            main.vieModel.DatabaseSelectedIndex = main.vieModel.DataBases.IndexOf(name);

        }

        public void RefreshMain()
        {
            //刷新主界面
            string name = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);
            Main main = GetWindowByName("Main") as Main;
            main.vieModel.LoadDataBaseList();
            main.vieModel.DatabaseSelectedIndex = main.vieModel.DataBases.IndexOf(name);
        }

        public void EditDataBase(object sender, MouseButtonEventArgs e)
        {
            string name = "";
            Border border = sender as Border;
            Grid grid = border.Parent as Grid;
            Grid grid1 = grid.Parent as Grid;
            TextBlock textBlock = grid1.Children[1] as TextBlock;
            name = textBlock.Text.ToLower();
            vieModel_DBManagement.CurrentDataBase = name;

            var brush = new SolidColorBrush(Colors.Red);
            NameBorder.Background = brush;
            Color TargColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundMain"].ToString())).Color;
            var ca = new ColorAnimation(TargColor, TimeSpan.FromSeconds(0.75));
            brush.BeginAnimation(SolidColorBrush.ColorProperty, ca);

        }


        public void DelDataBase(object sender, MouseButtonEventArgs e)
        {
            //删除数据库

            string name = "";
            Border border = sender as Border;
            Grid grid = border.Parent as Grid;
            Grid grid1 = grid.Parent as Grid;
            TextBlock textBlock = grid1.Children[1] as TextBlock;
            name = textBlock.Text.ToLower();


            if (new Msgbox(this, $"{Jvedio.Language.Resources.IsToDelete} {name}?").ShowDialog() == true)
            {
                string dirpath = DateTime.Now.ToString("yyyyMMddHHss");
                Directory.CreateDirectory($"BackUp\\{dirpath}");
                if (File.Exists($"DataBase\\{name}.sqlite"))
                {
                    //备份
                    File.Copy($"DataBase\\{name}.sqlite", $"BackUp\\{dirpath}\\{name}.sqlite", true);
                    //删除

                    try
                    {
                        File.Delete($"DataBase\\{name}.sqlite");

                        vieModel_DBManagement.DataBases.Remove(name);
                        RefreshMain();
                    }
                    catch
                    {
                        new Msgbox(this, $"【{name}】{Jvedio.Language.Resources.IsUsing}").ShowDialog();
                    }


                }



            }

        }




        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (LiveCharts.Wpf.PieChart)chartpoint.ChartView;

            //clear selected slice.
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }

        private void CartesianChart_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int num = Properties.Settings.Default.Statictistic_ID_Number;
            num += (int)(e.Delta / 120);

            if (num < 5) num = 5;
            else if (num > 50) num = 50;

            Properties.Settings.Default.Statictistic_ID_Number = num;
            Properties.Settings.Default.Save();
        }

        private void CartesianChart_PreviewMouseWheel1(object sender, MouseWheelEventArgs e)
        {
            int num = Properties.Settings.Default.Statictistic_Genre_Number;
            num += (int)(e.Delta / 120);

            if (num < 5) num = 5;
            else if (num > 50) num = 50;

            Properties.Settings.Default.Statictistic_Genre_Number = num;
            Properties.Settings.Default.Save();
        }

        private void CartesianChart_PreviewMouseWheel2(object sender, MouseWheelEventArgs e)
        {
            int num = Properties.Settings.Default.Statictistic_Tag_Number;
            num += (int)(e.Delta / 120);

            if (num < 5) num = 5;
            else if (num > 50) num = 50;

            Properties.Settings.Default.Statictistic_Tag_Number = num;
            Properties.Settings.Default.Save();
        }


        private void CartesianChart_PreviewMouseWheel3(object sender, MouseWheelEventArgs e)
        {
            int num = Properties.Settings.Default.Statictistic_Actor_Number;
            num += (int)(e.Delta / 120);

            if (num < 5) num = 5;
            else if (num > 50) num = 50;

            Properties.Settings.Default.Statictistic_Actor_Number = num;
            Properties.Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogInput dialogInput = new DialogInput(this, Jvedio.Language.Resources.PleaseEnter);
            if (dialogInput.ShowDialog() == true)
            {
                string name = dialogInput.Text.ToLower();


                if (vieModel_DBManagement.DataBases.Contains(name))
                {
                    new Msgbox(this, Jvedio.Language.Resources.Message_AlreadyExist).ShowDialog();
                    return;
                }




                MySqlite db = new MySqlite("DataBase\\" + name);
                db.CreateTable(DataBase.SQLITETABLE_MOVIE);
                db.CreateTable(DataBase.SQLITETABLE_ACTRESS);
                db.CreateTable(DataBase.SQLITETABLE_LIBRARY);
                db.CreateTable(DataBase.SQLITETABLE_JAVDB);


                vieModel_DBManagement.DataBases.Add(name);
                //刷新主界面
                RefreshMain();


            }


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = Jvedio.Language.Resources.ChooseDataBase;
            OpenFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.Filter = $"Sqlite { Jvedio.Language.Resources.File}|*.sqlite";
            OpenFileDialog1.Multiselect = true;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] names = OpenFileDialog1.FileNames;

                foreach (var item in names)
                {
                    string name = Path.GetFileNameWithoutExtension(item).ToLower();

                    if (!DataBase.IsProperSqlite(item)) continue;

                    if (File.Exists($"DataBase\\{name}.sqlite"))
                    {
                        if (new Msgbox(this, $"{ Jvedio.Language.Resources.Message_AlreadyExist} {name}，{ Jvedio.Language.Resources.IsToOverWrite}").ShowDialog() == true)
                        {
                            File.Copy(item, $"DataBase\\{name}.sqlite", true);

                            if (!vieModel_DBManagement.DataBases.Contains(name)) vieModel_DBManagement.DataBases.Add(name);

                        }
                    }
                    else
                    {
                        File.Copy(item, $"DataBase\\{name}.sqlite", true);
                        if (!vieModel_DBManagement.DataBases.Contains(name)) vieModel_DBManagement.DataBases.Add(name);

                    }

                }



            }
            RefreshMain();
        }

        private void Jvedio_BaseWindow_ContentRendered(object sender, EventArgs e)
        {


            //设置当前数据库
            for (int i = 0; i < vieModel_DBManagement.DataBases.Count; i++)
            {
                if (vieModel_DBManagement.DataBases[i].ToLower() == Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel_DBManagement.DataBases.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;
            //TabControl.SelectedIndex=2;

        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            if (e.AddedItems[0].ToString().ToLower() != Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
            {
                Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{e.AddedItems[0].ToString()}.sqlite";
                //切换数据库
                ShowStatistic();
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            button.IsEnabled = false;
            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Cancel, "DBManageGrowl"); });
            ct = cts.Token;

            //数据库管理
            var cb = CheckBoxWrapPanel.Children.OfType<CheckBox>().ToList();
            string path = $"DataBase\\{vieModel_DBManagement.CurrentDataBase}";
            MySqlite db = new MySqlite(path);

            if ((bool)cb[0].IsChecked)
            {
                //重置信息
                db.DeleteTable("movie");
                db.CreateTable(DataBase.SQLITETABLE_MOVIE);
                //清空最近播放和最近创建
                ClearDateBefore(DateTime.Now);
                db.Vacuum();
                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "DBManageGrowl");
            }

            if ((bool)cb[1].IsChecked)
            {
                //删除不存在影片
                long num = 0;
                await Task.Run(() =>
                {

                    var movies = db.SelectMoviesBySql("select * from movie");
                    try
                    {
                        vieModel_DBManagement.ProgressBarValue = 0;

                        for (int i = 0; i < movies.Count; i++)
                        {
                            ct.ThrowIfCancellationRequested();
                            if (!File.Exists(movies[i].filepath))
                            {
                                db.DeleteByField("movie", "id", movies[i].id);
                                num++;
                            }
                            if (movies.Count > 0) vieModel_DBManagement.ProgressBarValue = (int)((double)(i + 1) / (double)movies.Count * 100);
                        }
                        db.Vacuum();
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    }
                }, ct);

                HandyControl.Controls.Growl.Success($"{ Jvedio.Language.Resources.SuccessDelete} {num}", "DBManageGrowl");
            }

            if ((bool)cb[2].IsChecked)
            {
                var movies = db.SelectMoviesBySql("select * from movie");
                StringCollection ScanPath = ReadScanPathFromConfig(vieModel_DBManagement.CurrentDataBase);

                long num = 0;
                await Task.Run(() =>
                {
                    try
                    {
                        vieModel_DBManagement.ProgressBarValue = 0;

                        for (int i = 0; i < movies.Count; i++)
                        {
                            ct.ThrowIfCancellationRequested();
                            if (!IsPathIn(movies[i].filepath, ScanPath))
                            {
                                db.DeleteByField("movie", "id", movies[i].id);
                                num++;
                            }
                            if (movies.Count > 0) vieModel_DBManagement.ProgressBarValue = (int)((double)(i+1) / (double)movies.Count * 100);
                        }

                        db.Vacuum();
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    }
                }, ct);


                HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.SuccessDelete} {num}", "DBManageGrowl");
            }

            if ((bool)cb[3].IsChecked)
            {
                if (Properties.Settings.Default.SaveInfoToNFO)
                {
                    var detailMovies = db.SelectDetailMoviesBySql("select * from movie");
                    await Task.Run(() =>
                    {
                        try
                        {
                            vieModel_DBManagement.ProgressBarValue = 0;

                            for (int i = 0; i < detailMovies.Count; i++)
                            {
                                ct.ThrowIfCancellationRequested();
                                FileProcess.SaveNfo(detailMovies[i]);
                                if (detailMovies.Count > 0) vieModel_DBManagement.ProgressBarValue = (int)((double)(i + 1) / (double)detailMovies.Count * 100);
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                        }
                    }, ct);

                    HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.Message_Success}", "DBManageGrowl");
                }
                else
                {
                    HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.setnfo}", "DBManageGrowl");
                }

            }
            db.CloseDB();
            cts.Dispose();
            await Task.Run(() => { Task.Delay(500).Wait(); });
            Main main = (Main)GetWindowByName("Main");
            main?.vieModel.Reset();
            button.IsEnabled = true;
        }

        public bool IsPathIn(string path, StringCollection paths)
        {
            foreach (var item in paths)
            {
                if (path.IndexOf(item) >= 0) return true;
            }
            return false;
        }



        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
            WaitingPanel.Visibility = Visibility.Hidden;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowStatistic();
        }


        private async void ShowStatistic()
        {
            if (TabControl.SelectedIndex == 1)
            {
                await Task.Run(() =>
                {
                    vieModel_DBManagement.Statistic();
                    IDBarView.Datas = vieModel_DBManagement.LoadID();
                    IDBarView.Title = Jvedio.Language.Resources.ID;
                    IDBarView.Refresh();
                    Task.Delay(300).Wait();
                    ActorBarView.Datas = vieModel_DBManagement.LoadActor();
                    ActorBarView.Title = Jvedio.Language.Resources.Actor;
                    ActorBarView.Refresh();
                    Task.Delay(300).Wait();
                    GenreBarView.Datas = vieModel_DBManagement.LoadGenre();
                    GenreBarView.Title = Jvedio.Language.Resources.Genre;
                    GenreBarView.Refresh();
                    Task.Delay(300).Wait();
                    TagBarView.Datas = vieModel_DBManagement.LoadTag();
                    TagBarView.Title = Jvedio.Language.Resources.Tag;
                    TagBarView.Refresh();
                });
            }
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal, this);
                taskbarInstance.SetProgressValue((int)e.NewValue, 100, this);
                if (e.NewValue == 100) taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }
        }

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (vieModel_DBManagement.ProgressBarValue == 0 && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress, this);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
            vieModel_DBManagement.ProgressBarValue = 0;
            RunButton.IsEnabled = true;
        }

        private void Jvedio_BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                cts?.Cancel();
                cts_copy?.Cancel();
            }
            catch(ObjectDisposedException ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            StackPanel sp = comboBox.Parent as StackPanel;
            TextBlock tb = sp.Children.OfType<TextBlock>().First();
            StackPanel topsp = sp.Parent as StackPanel;
            TextBlock textBlock = topsp.Children.OfType<TextBlock>().First();
            string path =Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase" ,e.AddedItems[0].ToString() + ".sqlite");
            double count = 0;
            long length = 0;
            if (File.Exists(path))
            {
                using (MySqlite sqlite = new MySqlite(path,true))
                {
                    count= sqlite.SelectCountByTable("movie");
                }
                length = new FileInfo(path).Length;
            }
            textBlock.Text = $"{Jvedio.Language.Resources.Number}：{count}\n{Jvedio.Language.Resources.FileSize}：{length.ToProperFileSize()}";

            if (tb.Text == Jvedio.Language.Resources.Source)
                src = path;
            else
                dst = path;        }



        CancellationToken ct_copy;
        CancellationTokenSource cts_copy;
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (src == "" || dst == "") return;
            if (!File.Exists(src) || !File.Exists(dst)) return;
            if (src == dst)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.SamePathError, "DBManageGrowl");
                return;
            }
            cts_copy = new CancellationTokenSource();
            ct_copy = cts_copy.Token;
            CopyButton.IsEnabled = false;
            bool skipnulltitle = (bool)SkipNullTitle.IsChecked;
            Task.Run(() => {
                try
                {
                    DataBase.CopyDatabaseInfo(src, dst, ct_copy, (value) => {
                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)delegate {
                            CopyProgressBar.Value = value;
                            if (value == 100)
                            {
                                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "DBManageGrowl");
                                cts_copy.Dispose();
                                CopyButton.IsEnabled = true;
                            }
                        });
                    }, skipnulltitle);
                }catch(OperationCanceledException ex)
                {
                    Console.WriteLine(ex.Message);
                }

            });
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                cts_copy?.Cancel();
                CopyButton.IsEnabled = true;
            }
            catch (ObjectDisposedException ex) { Console.WriteLine(ex.Message); }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Button button  = (Button)sender;
            StackPanel sp = button.Parent as StackPanel;
            ComboBox comboBox = sp.Children.OfType<ComboBox>().First();
            string path =Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase" , comboBox.Text + ".sqlite");

            if (File.Exists(path)) { Process.Start("explorer.exe", "/select, \"" + path + "\""); }
            else
            {
                HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {path}", "DBManageGrowl");
            }
            
        }
    }

}
