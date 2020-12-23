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
using static Jvedio.FileProcess;


namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class WindowTools : Jvedio_BaseWindow
    {

        public CancellationTokenSource cts;
        public CancellationToken ct;
        public bool Running;
        VieModel_Tools vieModel;

        public WindowTools()
        {
            InitializeComponent();
            //WinState = 0;//每次重新打开窗体默认为Normal
            vieModel = new VieModel_Tools();
            this.DataContext = vieModel;
            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info("取消当前下载任务！", "ToolsGrowl"); });
            ct = cts.Token;
            Running = false;


            var Grids = MainGrid.Children.OfType<Grid>().ToList();
            foreach (var item in Grids) item.Visibility = Visibility.Hidden;
            Grids[Properties.Settings.Default.ToolsIndex].Visibility = Visibility.Visible;

            var RadioButtons = RadioButtonStackPanel.Children.OfType<RadioButton>().ToList();
            RadioButtons[Properties.Settings.Default.ToolsIndex].IsChecked = true;

        }

        public void ShowGrid(object sender, RoutedEventArgs e)
        {

            RadioButton radioButton = (RadioButton)sender;
            StackPanel SP = radioButton.Parent as StackPanel;
            var radioButtons = SP.Children.OfType<RadioButton>().ToList();
            var Grids = MainGrid.Children.OfType<Grid>().ToList();
            foreach (var item in Grids) item.Visibility = Visibility.Hidden;
            Grids[radioButtons.IndexOf(radioButton)].Visibility = Visibility.Visible;

            Properties.Settings.Default.ToolsIndex = radioButtons.IndexOf(radioButton);
            Properties.Settings.Default.Save();

        }

        public void ShowAccessPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = "选择一个Access文件";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = "Access文件(*.mdb)| *.mdb";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.InitialDirectory = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "mdb") ? AppDomain.CurrentDomain.BaseDirectory + "mdb" : AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AccessPathTextBox.Text = OpenFileDialog1.FileName;
            }
        }

        public void ShowNFOPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = "选择一个NFO文件";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = "NFO文件(*.nfo)| *.nfo";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.InitialDirectory = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Download\\NFO") ? AppDomain.CurrentDomain.BaseDirectory + "Download\\NFO" : AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NFOPathTextBox.Text = OpenFileDialog1.FileName;
            }
        }



        public void AddPath(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "选择文件夹";
            folderBrowserDialog.ShowNewFolderButton = false;
            //folderBrowserDialog.SelectedPath = @"D:\2020\VS Project\Jvedio\Jvedio(WPF)\Jvedio\Jvedio\bin\番号测试";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK & !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                bool PathConflict = false;
                foreach (var item in vieModel.ScanPath)
                {
                    if (folderBrowserDialog.SelectedPath.IndexOf(item) >= 0 | item.IndexOf(folderBrowserDialog.SelectedPath) >= 0)
                    {
                        PathConflict = true;
                        break;
                    }
                }
                if (!PathConflict) { vieModel.ScanPath.Add(folderBrowserDialog.SelectedPath); } else { new Msgbox(this, "路径冲突！").ShowDialog(); }

            }




        }

        public void DelPath(object sender, MouseButtonEventArgs e)
        {
            if (PathListBox.SelectedIndex != -1)
            {
                for (int i = PathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.ScanPath.Remove(PathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearPath(object sender, MouseButtonEventArgs e)
        {
            vieModel.ScanPath.Clear();
        }



        public void AddEuPath(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "选择文件夹";
            folderBrowserDialog.SelectedPath = @"D:\2020\VS Project\Jvedio\Jvedio(WPF)\Jvedio\Jvedio\资料\欧美测试";
            folderBrowserDialog.ShowNewFolderButton = true;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK & !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                bool PathConflict = false;
                foreach (var item in vieModel.ScanEuPath)
                {
                    if (folderBrowserDialog.SelectedPath.IndexOf(item) >= 0 | item.IndexOf(folderBrowserDialog.SelectedPath) >= 0)
                    {
                        PathConflict = true;
                        break;
                    }
                }
                if (!PathConflict) { vieModel.ScanEuPath.Add(folderBrowserDialog.SelectedPath); } else { new Msgbox(this, "路径冲突！").ShowDialog(); }

            }




        }

        public void DelEuPath(object sender, MouseButtonEventArgs e)
        {
            if (EuropePathListBox.SelectedIndex != -1)
            {
                for (int i = EuropePathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.ScanEuPath.Remove(EuropePathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearEuPath(object sender, MouseButtonEventArgs e)
        {
            vieModel.ScanEuPath.Clear();
        }





        public void AddSingleNFOPath(object sender, RoutedEventArgs e)
        {
            string path = NFODirPathTextBox.Text;
            if (Directory.Exists(path))
            {
                bool PathConflict = false;
                foreach (var item in vieModel.NFOScanPath)
                {
                    if (path.IndexOf(item) >= 0 | item.IndexOf(path) >= 0)
                    {
                        PathConflict = true;
                        break;
                    }
                }
                if (!PathConflict) { vieModel.NFOScanPath.Add(path); NFODirPathTextBox.Text = ""; }
                else
                {
                    HandyControl.Controls.Growl.Warning("路径冲突！", "ToolsGrowl");
                }

            }




        }



        public void AddNFOPath(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "选择文件夹";
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.SelectedPath = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "DownLoad\\NFO") ? AppDomain.CurrentDomain.BaseDirectory + "DownLoad\\NFO" : AppDomain.CurrentDomain.BaseDirectory;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK & !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                bool PathConflict = false;
                foreach (var item in vieModel.NFOScanPath)
                {
                    if (folderBrowserDialog.SelectedPath.IndexOf(item) >= 0 | item.IndexOf(folderBrowserDialog.SelectedPath) >= 0)
                    {
                        PathConflict = true;
                        break;
                    }
                }
                if (!PathConflict) { vieModel.NFOScanPath.Add(folderBrowserDialog.SelectedPath); } else { HandyControl.Controls.Growl.Warning("路径冲突！", "ToolsGrowl"); }

            }




        }


        public async void StartRun(object sender, RoutedEventArgs e)
        {

            if (Running)
            {
                HandyControl.Controls.Growl.Error("其他任务正在进行！", "ToolsGrowl");
                return;
            }

            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info("取消当前下载任务！", "ToolsGrowl"); });
            ct = cts.Token;

            var grids = MainGrid.Children.OfType<Grid>().ToList();
            int index = 0;
            for (int i = 0; i < grids.Count; i++) { if (grids[i].Visibility == Visibility.Visible) { index = i; break; } }
            Running = true;
            switch (index)
            {
                case 0:
                    //扫描
                    double totalnum = 0;//扫描出的视频总数
                    double insertnum = 0;//导入的视频总数
                    try
                    {
                        //全盘扫描
                        if ((bool)ScanAll.IsChecked)
                        {
                            LoadingStackPanel.Visibility = Visibility.Visible;
                            await Task.Run(() =>
                            {
                                ct.ThrowIfCancellationRequested();

                                List<string> filepaths = Scan.ScanAllDrives();
                                totalnum = filepaths.Count;
                                insertnum = Scan.DistinctMovieAndInsert(filepaths, ct);
                            });
                        }
                        else
                        {
                            if (vieModel.ScanPath.Count == 0) { break; }
                            LoadingStackPanel.Visibility = Visibility.Visible;



                            await Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();

                            StringCollection stringCollection = new StringCollection();
                            foreach (var item in vieModel.ScanPath)
                            {
                                if (Directory.Exists(item)) { stringCollection.Add(item); }
                            }
                            List<string> filepaths = Scan.ScanPaths(stringCollection, ct);
                            totalnum = filepaths.Count;
                            insertnum = Scan.DistinctMovieAndInsert(filepaths, ct);
                        }, cts.Token);

                        }

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Info($"扫描出 {totalnum} 个，导入 {insertnum} 个", "ToolsGrowl");

                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {ex.Message}");
                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }


                    break;
                case 1:
                    //Access
                    LoadingStackPanel.Visibility = Visibility.Visible;
                    string AccessPath = AccessPathTextBox.Text;
                    if (!File.Exists(AccessPath))
                    {
                        HandyControl.Controls.Growl.Error($"不存在 ：{AccessPath}", "ToolsGrowl");

                        break;
                    }
                    try
                    {
                        await Task.Run(() =>
                    {
                        DataBase.InsertFromAccess(AccessPath);
                    });
                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Success("成功！", "ToolsGrowl");
                        }

                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }
                    break;
                case 2:
                    //NFO
                    if ((bool)NfoRB1.IsChecked)
                    {
                        if (vieModel.NFOScanPath.Count == 0) { HandyControl.Controls.Growl.Warning("路径为空！", "ToolsGrowl"); }
                    }
                    else { if (!File.Exists(NFOPathTextBox.Text)) { HandyControl.Controls.Growl.Warning($"文件不存在{NFOPathTextBox.Text}", "ToolsGrowl"); } }


                    Running = true;

                    try
                    {
                        List<string> nfoFiles = new List<string>();
                        if (!(bool)NfoRB1.IsChecked) { nfoFiles.Add(NFOPathTextBox.Text); }
                        else
                        {
                            //扫描所有nfo文件
                            await Task.Run(() =>
                            {
                                this.Dispatcher.Invoke((Action)delegate
                                {
                                    StatusTextBlock.Visibility = Visibility.Visible;
                                    StatusTextBlock.Text = "开始扫描";
                                });

                                StringCollection stringCollection = new StringCollection();
                                foreach (var item in vieModel.NFOScanPath)
                                {
                                    if (Directory.Exists(item)) { stringCollection.Add(item); }
                                }
                                nfoFiles = Scan.ScanNFO(stringCollection, ct, (filepath) =>
                                {
                                    this.Dispatcher.Invoke((Action)delegate { StatusTextBlock.Text = filepath; });
                                });
                            }, cts.Token);
                        }


                        //记录日志
                        Logger.LogScanInfo("\n-----【" + DateTime.Now.ToString() + "】NFO扫描-----");
                        Logger.LogScanInfo($"\n扫描出 => {nfoFiles.Count}  个 ");


                        //导入所有 nfo 文件信息
                        double total = 0;
                        bool importpic = (bool)NFOCopyPicture.IsChecked;
                        await Task.Run(() =>
                        {

                            nfoFiles.ForEach(item =>
                            {
                                if (File.Exists(item))
                                {
                                    Movie movie = GetInfoFromNfo(item);
                                    if (movie != null && !string.IsNullOrEmpty(movie.id))
                                    {

                                        DataBase.InsertFullMovie(movie);
                                        //复制并覆盖所有图片
                                        if (importpic) CopyPicToPath(movie.id, item);
                                        total += 1;
                                        Logger.LogScanInfo($"\n成功导入数据库 => {item}  ");
                                    }

                                }
                            });


                        });
                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            Logger.LogScanInfo($"\n成功导入 {total} 个");
                            HandyControl.Controls.Growl.Success($"\n成功导入 {total} 个", "ToolsGrowl");
                        }

                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }


                    break;

                case 3:
                    //欧美扫描
                    if (vieModel.ScanEuPath.Count == 0) { break; }
                    LoadingStackPanel.Visibility = Visibility.Visible;
                    totalnum = 0;
                    insertnum = 0;

                    try
                    {
                        await Task.Run(() =>
                        {
                            StringCollection stringCollection = new StringCollection();
                            foreach (var item in vieModel.ScanEuPath) if (Directory.Exists(item)) { stringCollection.Add(item); }
                            List<string> filepaths = Scan.ScanPaths(stringCollection, ct);
                            totalnum = filepaths.Count;
                            insertnum = Scan.DistinctMovieAndInsert(filepaths, ct, true);
                        });

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Info($"扫描出 {totalnum} 个，导入 {insertnum} 个");
                        }
                    }
                    finally

                    {
                        cts.Dispose();
                        Running = false;
                    }

                    break;

                case 4:

                    break;

                case 5:
                    //网络驱动器
                    LoadingStackPanel.Visibility = Visibility.Visible;

                    string path = UNCPathTextBox.Text;
                    if (path == "") { break; }

                    bool CanScan = true;
                    //检查权限
                    await Task.Run(() =>
                    {
                        try { var tl = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly); }
                        catch { CanScan = false; }
                    });

                    if (!CanScan) { LoadingStackPanel.Visibility = Visibility.Hidden; HandyControl.Controls.Growl.Error($"权限不够！"); break; }


                    bool IsEurope = !(bool)ScanTypeRadioButton.IsChecked;

                    totalnum = 0;
                    insertnum = 0;
                    try
                    {
                        await Task.Run(() =>
                        {
                            StringCollection stringCollection = new StringCollection();
                            stringCollection.Add(path);
                            List<string> filepaths = Scan.ScanPaths(stringCollection, ct);
                            totalnum = filepaths.Count;
                            insertnum = Scan.DistinctMovieAndInsert(filepaths, ct, IsEurope);
                        });

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested) { HandyControl.Controls.Growl.Info($"扫描出 {totalnum} 个，导入 {insertnum} 个"); }
                    }
                    finally
                    {
                        cts.Dispose();
                        Running = false;
                    }
                    break;

                default:

                    break;

            }
            Running = false;

        }



        public bool IsDownLoading()
        {
            bool result = false;
            Main main = null;
            Window window = Jvedio.GetWindow.Get("Main");
            if (window != null) main = (Main)window;


            if (main?.DownLoader != null)
            {
                if (main.DownLoader.State == DownLoadState.DownLoading | main.DownLoader.State == DownLoadState.Pause)
                {
                    Console.WriteLine("main.DownLoader.State   " + main.DownLoader.State);
                    result = true;
                }


            }


            return result;
        }




        public void ShowRunInfo(object sender, RoutedEventArgs e)
        {
            try
            {

                var grids = MainGrid.Children.OfType<Grid>().ToList();
                int index = 0;
                for (int i = 0; i < grids.Count; i++) { if (grids[i].Visibility == Visibility.Visible) { index = i; break; } }
                string filepath = "";
                switch (index)
                {
                    case 0:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error("不存在");
                        break;

                    case 1:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\DataBase\\{DateTime.Now.ToString("yyyy -MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error("不存在");
                        break;
                    case 2:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error("不存在");
                        break;

                    case 3:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error("不存在");
                        break;

                    case 4:
                        HandyControl.Controls.Growl.Info("无报告");
                        break;

                    case 5:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error("不存在");
                        break;

                    default:

                        break;
                }



            }
            catch { }
        }

        public void DelNFOPath(object sender, MouseButtonEventArgs e)
        {
            if (NFOPathListBox.SelectedIndex != -1)
            {
                for (int i = NFOPathListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    vieModel.NFOScanPath.Remove(NFOPathListBox.SelectedItems[i].ToString());
                }
            }
        }

        public void ClearNFOPath(object sender, MouseButtonEventArgs e)
        {
            vieModel.NFOScanPath.Clear();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Hide();
        }


        public void StartScan(object sender, RoutedEventArgs e)
        {

        }

        public void CopyPicToPath(string id, string path)
        {
            string fatherpath = new FileInfo(path).DirectoryName;
            string[] files = null;
            try
            {
                files = Directory.GetFiles(fatherpath, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Logger.LogE(e);
            }

            string ImageExt = "bmp;gif;ico;jpe;jpeg;jpg;png";
            List<string> ImageExtList = new List<string>(); foreach (var item in ImageExt.Split(';')) { ImageExtList.Add('.' + item); }

            //识别图片
            if (files != null)
            {
                var piclist = files.Where(s => ImageExtList.Contains(Path.GetExtension(s))).ToList();
                if (piclist.Count <= 0) return;
                foreach (var item in piclist)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        if (item.ToLower().IndexOf("poster") >= 0 || item.ToLower().IndexOf($"{id.ToLower()}_s") >= 0)
                        {
                            try { File.Copy(item, GlobalVariable.BasePicPath + $"SmallPic\\{id}.jpg", true); }
                            catch { }

                        }
                        else if (item.ToLower().IndexOf("fanart") >= 0 || item.ToLower().IndexOf($"{id.ToLower()}_b") >= 0)
                        {
                            try { File.Copy(item, GlobalVariable.BasePicPath + $"BigPic\\{id}.jpg", true); }
                            catch { }
                        }
                    }

                }




            }
        }



        private void DownloadMany(object sender, RoutedEventArgs e)
        {
            if (Running) { HandyControl.Controls.Growl.Warning("其他任务正在进行！"); return; }
            if (IsDownLoading()) { HandyControl.Controls.Growl.Warning("请等待下载结束！"); return; }



        }



        private void InsertOneMovie(object sender, RoutedEventArgs e)
        {
            Window window = Jvedio.GetWindow.Get("WindowEdit");
            WindowEdit windowEdit;
            if (window != null) { windowEdit = (WindowEdit)window; windowEdit.Close(); }
            windowEdit = new WindowEdit();
            windowEdit.Show();

        }

        private void CancelRun(object sender, RoutedEventArgs e)
        {
           if(!cts.IsCancellationRequested) cts.Cancel();
            LoadingStackPanel.Visibility = Visibility.Hidden;

            HandyControl.Controls.Growl.Info("已取消！");
            Running = false;
        }

        private void PathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void PathListBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    bool PathConflict = false;
                    foreach (var path in vieModel.ScanPath)
                    {
                        if (dragdropFile.IndexOf(path) >= 0 | path.IndexOf(dragdropFile) >= 0)
                        {
                            PathConflict = true;
                            break;
                        }
                    }
                    if (!PathConflict) { vieModel.ScanPath.Add(dragdropFile); }
                }
            }
        }

        private void AccessPathTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void AccessPathTextBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (IsFile(dragdropFile))
                {
                    if (new FileInfo(dragdropFile).Extension == ".mdb")
                    {
                        AccessPathTextBox.Text = dragdropFile;
                        break;
                    }
                }
            }
        }

        private void NFOPathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void NFOPathListBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    bool PathConflict = false;
                    foreach (var path in vieModel.NFOScanPath)
                    {
                        if (dragdropFile.IndexOf(path) >= 0 | path.IndexOf(dragdropFile) >= 0)
                        {
                            PathConflict = true;
                            break;
                        }
                    }
                    if (!PathConflict) { vieModel.NFOScanPath.Add(dragdropFile); }
                }
            }
        }

        private void SingleNFOBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void SingleNFOBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (IsFile(dragdropFile))
                {
                    if (new FileInfo(dragdropFile).Extension == ".nfo")
                    {
                        NFOPathTextBox.Text = dragdropFile;
                        break;
                    }
                }
            }
        }

        private void EuropePathListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void EuropePathListBox_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    bool PathConflict = false;
                    foreach (var path in vieModel.ScanEuPath)
                    {
                        if (dragdropFile.IndexOf(path) >= 0 | path.IndexOf(dragdropFile) >= 0)
                        {
                            PathConflict = true;
                            break;
                        }
                    }
                    if (!PathConflict) { vieModel.ScanEuPath.Add(dragdropFile); }
                }
            }
        }

        private void UNCPathBorder_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void UNCPathBorder_DragOver(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (!IsFile(dragdropFile))
                {
                    UNCPathTextBox.Text = dragdropFile;
                    break;
                }
            }
        }
    }

    public class IntToVisibility : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int v = int.Parse(value.ToString());
            if (v <= 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }
}
