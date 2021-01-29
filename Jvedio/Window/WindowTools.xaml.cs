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
using static Jvedio.GlobalMethod;

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
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_CancelCurrentTask, "ToolsGrowl"); });
            ct = cts.Token;
            Running = false;
            TabControl.SelectedIndex = Properties.Settings.Default.ToolsIndex;
        }

        public void ShowAccessPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = $"{Jvedio.Language.Resources.Choose} Access {Jvedio.Language.Resources.File}";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter =$"Access {Jvedio.Language.Resources.File}(*.mdb)| *.mdb";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.InitialDirectory = Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "mdb") ? AppDomain.CurrentDomain.BaseDirectory + "mdb" : AppDomain.CurrentDomain.BaseDirectory;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AccessPathTextBox.Text = OpenFileDialog1.FileName;
            }
        }


        public void ShowUNCPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = Jvedio.Language.Resources.ChooseDir;
            folderBrowserDialog.ShowNewFolderButton = false;
            //folderBrowserDialog.SelectedPath = @"D:\2020\VS Project\Jvedio\Jvedio(WPF)\Jvedio\Jvedio\bin\番号测试";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK & !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                UNCPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        public void ShowNFOPath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = $"{Jvedio.Language.Resources.Choose} NFO {Jvedio.Language.Resources.File}";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = $"NFO {Jvedio.Language.Resources.File}(*.nfo)| *.nfo";
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
            folderBrowserDialog.Description = Jvedio.Language.Resources.ChooseDir;
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
                if (!PathConflict) { vieModel.ScanPath.Add(folderBrowserDialog.SelectedPath); } else { new Msgbox(this, Jvedio.Language.Resources.Message_PathConflict).ShowDialog(); }

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
            folderBrowserDialog.Description =Jvedio.Language.Resources.ChooseDir;
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
                if (!PathConflict) { vieModel.ScanEuPath.Add(folderBrowserDialog.SelectedPath); } else { new Msgbox(this, Jvedio.Language.Resources.Message_PathConflict).ShowDialog(); }

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






        public void AddNFOPath(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = Jvedio.Language.Resources.ChooseDir;
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
                if (!PathConflict) { vieModel.NFOScanPath.Add(folderBrowserDialog.SelectedPath); } else { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_PathConflict, "ToolsGrowl"); }

            }




        }


        public async void StartRun(object sender, RoutedEventArgs e)
        {

            if (Running)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_StopAndTry, "ToolsGrowl");
                return;
            }

            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_CancelCurrentTask, "ToolsGrowl"); });
            ct = cts.Token;

            int index = TabControl.SelectedIndex;
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
                                insertnum = Scan.InsertWithNfo(filepaths, ct);
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
                            insertnum = Scan.InsertWithNfo(filepaths, ct);
                        }, cts.Token);

                        }

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_ScanNum} {totalnum}  {Jvedio.Language.Resources.ImportNumber} {insertnum}", "ToolsGrowl");
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
                        HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist} {AccessPath}", "ToolsGrowl");

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
                            HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "ToolsGrowl");
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
                    if (NFOTabControl.SelectedIndex==0)
                    {
                        if (vieModel.NFOScanPath.Count == 0) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_CanNotBeNull, "ToolsGrowl"); }
                    }
                    else { if (!File.Exists(NFOPathTextBox.Text)) { HandyControl.Controls.Growl.Warning($"{Jvedio.Language.Resources.Message_FileNotExist} {NFOPathTextBox.Text}", "ToolsGrowl"); } }


                    Running = true;

                    try
                    {
                        List<string> nfoFiles = new List<string>();
                        if (NFOTabControl.SelectedIndex == 1) 
                        { 
                            nfoFiles.Add(NFOPathTextBox.Text); 
                        }
                        else
                        {
                            //扫描所有nfo文件
                            await Task.Run(() =>
                            {
                                this.Dispatcher.Invoke((Action)delegate
                                {
                                    StatusTextBlock.Visibility = Visibility.Visible;
                                    StatusTextBlock.Text = Jvedio.Language.Resources.BeginScan;
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
                        Logger.LogScanInfo($"\n-----【" + DateTime.Now.ToString() + $"】NFO {Jvedio.Language.Resources.Scan}-----");
                        Logger.LogScanInfo($"\n{Jvedio.Language.Resources.Scan}{Jvedio.Language.Resources.Number} => {nfoFiles.Count}  ");


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
                                        Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNumber} => {item}  ");
                                    }

                                }
                            });


                        });
                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            Logger.LogScanInfo(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNumber} {total} ");
                            HandyControl.Controls.Growl.Success(Environment.NewLine + $"{Jvedio.Language.Resources.ImportNumber} {total} ", "ToolsGrowl");
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
                            insertnum = Scan.InsertWithNfo(filepaths, ct, IsEurope:true);
                        });

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested)
                        {
                            HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Scan}{Jvedio.Language.Resources.Number} {totalnum}  {Jvedio.Language.Resources.ImportNumber} {insertnum} ", "ToolsGrowl");
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

                    if (!CanScan) { LoadingStackPanel.Visibility = Visibility.Hidden; HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.InsufficientPermissions, "ToolsGrowl"); break; }


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
                            insertnum = Scan.InsertWithNfo(filepaths, ct, IsEurope:IsEurope);
                        });

                        LoadingStackPanel.Visibility = Visibility.Hidden;
                        if (!cts.IsCancellationRequested) { HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Scan}{Jvedio.Language.Resources.Number} {totalnum}  {Jvedio.Language.Resources.ImportNumber} {insertnum} ", "ToolsGrowl"); }
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
            Window window = GetWindowByName("Main");
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
                int index = TabControl.SelectedIndex;
                string filepath = "";
                switch (index)
                {
                    case 0:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, "ToolsGrowl");
                        break;

                    case 1:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\DataBase\\{DateTime.Now.ToString("yyyy -MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, "ToolsGrowl");
                        break;
                    case 2:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, "ToolsGrowl");
                        break;

                    case 3:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, "ToolsGrowl");
                        break;

                    case 4:
                        HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.NoLog, "ToolsGrowl");
                        break;

                    case 5:
                        filepath = AppDomain.CurrentDomain.BaseDirectory + $"Log\\ScanLog\\{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                        if (File.Exists(filepath)) Process.Start(filepath); else HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, "ToolsGrowl");
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
            if (Running) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.OtherTaskIsRunning, "ToolsGrowl"); return; }
            if (IsDownLoading()) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, "ToolsGrowl"); return; }



        }



        private void InsertOneMovie(object sender, RoutedEventArgs e)
        {
            Window window = GetWindowByName("WindowEdit");
            WindowEdit windowEdit;
            if (window != null) { windowEdit = (WindowEdit)window; windowEdit.Close(); }
            windowEdit = new WindowEdit();
            windowEdit.Show();

        }

        private void CancelRun(object sender, RoutedEventArgs e)
        {
           if(!cts.IsCancellationRequested) cts.Cancel();
            LoadingStackPanel.Visibility = Visibility.Hidden;

            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Cancel, "ToolsGrowl");
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


        private void Jvedio_BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.ToolsIndex = TabControl.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }

}
