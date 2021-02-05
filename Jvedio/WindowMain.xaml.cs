using DynamicData;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Jvedio.GlobalMethod;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using static Jvedio.ImageProcess;
using System.Windows.Media.Effects;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        public const string UpdateUrl = "http://hitchao.gitee.io/jvedioupdate/Version";
        public const string UpdateExeVersionUrl = "http://hitchao.gitee.io/jvedioupdate/update";
        public const string UpdateExeUrl = "http://hitchao.gitee.io/jvedioupdate/JvedioUpdate.exe";
        public const string NoticeUrl = "https://hitchao.github.io/JvedioWebPage/notice";

        public DispatcherTimer CheckurlTimer = new DispatcherTimer();
        public int CheckurlInterval = 10;//每5分钟检测一次网址

        public bool Resizing = false;
        public DispatcherTimer ResizingTimer = new DispatcherTimer();

        public Point WindowPoint = new Point(100, 100);
        public Size WindowSize = new Size(1000, 600);
        public JvedioWindowState WinState = JvedioWindowState.Normal;
        public DispatcherTimer ImageSlideTimer;

        public List<Actress> SelectedActress = new List<Actress>();

        public bool IsMouseDown = false;
        public Point MosueDownPoint;

        public bool CanRateChange = false;
        public bool IsToUpdate = false;

        public CancellationTokenSource RefreshScanCTS;
        public CancellationToken RefreshScanCT;


        public Settings WindowSet = null;
        public VieModel_Main vieModel;

        private HwndSource _hwndSource;

        public DetailMovie CurrentLabelMovie;
        public bool IsFlowing = false;


        public DispatcherTimer FlipoverTimer = new DispatcherTimer();

        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager taskbarInstance = null;


        public Main()
        {
            InitializeComponent();
            SettingsContextMenu.Placement = PlacementMode.Mouse;

            this.Cursor = Cursors.Wait;


            ImageSlideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            ImageSlideTimer.Tick += new EventHandler(ImageSlideTimer_Tick);

            ActorInfoGrid.Visibility = Visibility.Collapsed;
            LoadingGrid.Visibility = Visibility.Collapsed;
            ProgressBar.Visibility = Visibility.Collapsed;
            ActorProgressBar.Visibility = Visibility.Hidden;
            FilterGrid.Visibility = Visibility.Collapsed;
            WinState = 0;

            AdjustWindow();

            InitImage();

            Properties.Settings.Default.Selected_Background = "#FF8000";
            Properties.Settings.Default.Selected_BorderBrush = "#FF8000";
            //Properties.Settings.Default.DisplayNumber = 5;

            BindingEvent();

            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported) taskbarInstance = Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance;


            #region "改变窗体大小"
            //https://www.cnblogs.com/yang-fei/p/4737308.html

            if (resizeGrid != null)
            {
                foreach (UIElement element in resizeGrid.Children)
                {
                    if (element is Rectangle resizeRectangle)
                    {
                        resizeRectangle.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                        resizeRectangle.MouseMove += ResizeRectangle_MouseMove;
                    }
                }
            }
            PreviewMouseMove += OnPreviewMouseMove;
            #endregion
        }

        #region "改变窗体大小"
        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) return;
            if (this.Width == SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height) return;

            if (sender is Rectangle rectangle)
            {
                switch (rectangle.Name)
                {
                    case "TopRectangle":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Top);
                        break;
                    case "Bottom":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Bottom);
                        break;
                    case "LeftRectangle":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Left);
                        break;
                    case "Right":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Right);
                        break;
                    case "TopLeft":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.TopLeft);
                        break;
                    case "TopRight":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.TopRight);
                        break;
                    case "BottomLeft":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.BottomLeft);
                        break;
                    case "BottomRight":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.BottomRight);
                        break;
                    default:
                        break;
                }
            }
        }


        protected void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }

        private void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized) return;
            if (this.Width == SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height) return;

            if (sender is Rectangle rectangle)
            {
                switch (rectangle.Name)
                {
                    case "TopRectangle":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "Bottom":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "LeftRectangle":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "Right":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "TopLeft":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case "TopRight":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "BottomLeft":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "BottomRight":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += MainWindow_SourceInitialized;
            base.OnInitialized(e);
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion


        #region "热键"



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);


            //热键
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            //注册热键

            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (Properties.Settings.Default.HotKey_Enable && modifier != 0 && vk != 0)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success) { MessageBox.Show("热键冲突！", "热键冲突"); }
            }

        }




        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == Properties.Settings.Default.HotKey_VK)
                            {
                                HideAllWindow();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void HideAllWindow()
        {

            if (IsHide)
            {
                foreach (Window window in App.Current.Windows)
                {
                    if (OpeningWindows.Contains(window.GetType().ToString()))
                    {
                        window.Visibility = Visibility.Visible;
                    }
                }
                IsHide = false;
            }
            else
            {
                OpeningWindows.Clear();
                foreach (Window window in App.Current.Windows)
                {
                    window.Visibility = Visibility.Hidden;
                    OpeningWindows.Add(window.GetType().ToString());
                }
                IsHide = true;

                //隐藏图标
                //notifyIcon.Visible = false;
                NotifyIcon.Visibility = Visibility.Collapsed;
            }


        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            //取消热键
            Console.WriteLine("UnregisterHotKey");
            base.OnClosed(e);
        }


        #endregion


        private void InitImage()
        {
            //设置背景
            string path = Properties.Settings.Default.BackgroundImage;
            if (!File.Exists(path)) path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.jpg");
            GlobalVariable.BackgroundImage = null;
            GC.Collect();
            GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);

            DefaultBigImage= new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
            DefaultSmallImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_S.png", UriKind.Relative));
            DefaultActorImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));

        }

        //绑定事件
        private void BindingEvent()
        {


            //绑定演员奥村
            foreach (StackPanel item in ActorInfoStackPanel.Children.OfType<StackPanel>().ToList())
            {
                TextBox textBox = item.Children[1] as TextBox;
                textBox.PreviewKeyUp += SaveActress;
            }




            //设置排序类型
            var radioButtons = SortStackPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in radioButtons)
            {
                item.Click += SetSortValue;
            }

            //设置图片显示模式
            var rbs = ImageTypeStackPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in rbs)
            {
                item.Click += SaveShowImageMode;
            }

            //设置分类中的视频格式
            var rbs2 = ClassifyVedioTypeStackPanel.Children.OfType<RadioButton>().ToList();
            foreach (RadioButton item in rbs2)
            {
                item.Click += SetTypeValue;
            }
        }




        private void ImageSlideTimer_Tick(object sender, EventArgs e)
        {
            Loadslide();
            ImageSlideTimer.Stop();
        }
        public void InitMovie()
        {
            vieModel = new VieModel_Main();
            if (Properties.Settings.Default.RandomDisplay)
            {
                vieModel.RandomDisplay();
            }
            else
            {
                vieModel.Reset();
                AllRB.IsChecked = true;
            }
            //AsyncLoadImage();
            this.DataContext = vieModel;

            vieModel.CurrentMovieListHideOrChanged += (s, ev) => { StopDownLoad(); };
            vieModel.MovieFlipOverCompleted += (s, ev) =>
            {
                //等待加载
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        vieModel.CurrentCount = vieModel.CurrentMovieList.Count;
                        vieModel.TotalCount = vieModel.FilterMovieList.Count;
                        if (Properties.Settings.Default.EditMode) SetSelected();
                        if (Properties.Settings.Default.ShowImageMode == "2") ImageSlideTimer.Start();//0.5s后开始展示预览图
                        SetLoadingStatus(false);



                    }, DispatcherPriority.ContextIdle, null);
            };

            vieModel.ActorFlipOverCompleted += (s, ev) =>
            {
                //等待加载
                Dispatcher.BeginInvoke((Action)delegate
                {
                    vieModel.ActorCurrentCount = vieModel.CurrentActorList.Count;
                    vieModel.ActorTotalCount = vieModel.ActorList.Count;
                    if (Properties.Settings.Default.ActorEditMode) ActorSetSelected();
                    SetActorLoadingStatus(false);

                    //Grid grid = (Grid)ActorItemsControl.Parent;
                    //TabItem tabItem = (TabItem)grid.Parent;
                    //var scrollViewer = this.FindVisualChildOrContentByType<ScrollViewer>(tabItem);
                    //scrollViewer.ScrollToTop();

                }, DispatcherPriority.ContextIdle, null);
            };


            CheckurlTimer.Interval = TimeSpan.FromMinutes(CheckurlInterval);
            CheckurlTimer.Tick += new EventHandler(CheckurlTimer_Tick);

            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);




        }

        public void SetLoadingStatus(bool loading)
        {
            if (loading)
            {
                LoadingGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingGrid.Visibility = Visibility.Hidden;
            }
            PageStackPanel.IsEnabled = !loading;
            IsFlowing = loading;
            vieModel.IsFlipOvering = loading;
            SideBorder.IsEnabled = !loading;
            ToolsGrid.IsEnabled = !loading;
        }


        public void SetActorLoadingStatus(bool loading)
        {
            if (loading)
            {
                LoadingActorGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadingActorGrid.Visibility = Visibility.Hidden;
            }
            ActorPageStackPanel.IsEnabled = !loading;
            SideBorder.IsEnabled = !loading;
            ActorToolsGrid.IsEnabled = !loading;
        }



        public async Task<bool> InitActor()
        {
            vieModel.GetActorList();
            await Task.Delay(1);
            return true;
        }




        public void BeginCheckurlThread()
        {
            Thread threadObject = new Thread(CheckUrl);
            threadObject.Start();
        }



        private void CheckurlTimer_Tick(object sender, EventArgs e)
        {
            BeginCheckurlThread();
        }



        private void ResizingTimer_Tick(object sender, EventArgs e)
        {
            Resizing = false;
            ResizingTimer.Stop();
        }




        public void Notify_Close(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
            this.Close();
        }



        public void Notify_Show(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
            this.Show();
            this.Opacity = 1;
            this.WindowState = WindowState.Normal;
        }


        void CheckUpdate()
        {
            Task.Run(async () =>
            {
                string content = ""; int statusCode;
                try
                {
                    (content, statusCode) = await Net.Http(UpdateUrl, Proxy: null);
                }
                catch (TimeoutException ex) { Logger.LogN($"URL={UpdateUrl},Message-{ex.Message}"); }

                if (content != "")
                {
                    //检查更新
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        string remote = content.Split('\n')[0];
                        string updateContent = content.Replace(remote + "\n", "");
                        string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                        using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "OldVersion"))
                        {
                            sw.WriteLine(local + "\n");
                            sw.WriteLine(updateContent);
                        }

                        LocalVersionTextBlock.Text = $"{Jvedio.Language.Resources.CurrentVersion}：{local}";
                        RemoteVersionTextBlock.Text = $"{Jvedio.Language.Resources.LatestVersion}：{remote}";
                        UpdateContentTextBox.Text = updateContent;

                        if (local.CompareTo(remote) < 0) UpdateGrid.Visibility = Visibility.Visible;
                    });
                }
            });
        }


        void ShowNotice()
        {
            Task.Run(async () =>
            {
                string notices = "";
                string path = AppDomain.CurrentDomain.BaseDirectory + "Notice.txt";
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(path);
                    notices = sr.ReadToEnd();
                    sr.Close();
                }
                string content = ""; int statusCode = 404;
                try
                {
                    (content, statusCode) = await Net.Http(NoticeUrl, Proxy: null);
                }
                catch (TimeoutException ex) { Logger.LogN($"URL={NoticeUrl},Message-{ex.Message}"); }
                if (content != "")
                {
                    if (content != notices)
                    {
                        StreamWriter sw = new StreamWriter(path, false);
                        sw.Write(content);
                        sw.Close();
                        this.Dispatcher.Invoke((Action)delegate ()
                        {
                            NoticeTextBlock.Text = content;
                            NoticeGrid.Visibility = Visibility.Visible;
                        });
                    }

                }
            });
        }



        public DownLoader DownLoader;

        public void StartDownload(List<Movie> movieslist, bool force = false)
        {
            List<Movie> movies = new List<Movie>();
            List<Movie> moviesFC2 = new List<Movie>();
            if (movieslist != null)
            {
                foreach (var item in movieslist)
                {
                    if (item.id.IndexOf("FC2") >= 0) { moviesFC2.Add(item); } else { movies.Add(item); }
                }
            }

            //添加到下载列表
            DownLoader?.CancelDownload();
            DownLoader = new DownLoader(movies, moviesFC2, true);
            DownLoader.StartThread();
            double totalcount = moviesFC2.Count + movies.Count;
            Console.WriteLine(totalcount);
            if (totalcount == 0) return;
            //UI更新
            DownLoader.InfoUpdate += (s, e) =>
            {
                InfoUpdateEventArgs eventArgs = e as InfoUpdateEventArgs;
                try
                {
                    try { Refresh(eventArgs, totalcount); }
                    catch (TaskCanceledException ex) { Logger.LogE(ex); }
                }
                catch (Exception ex1)
                {
                    Console.WriteLine(ex1.StackTrace);
                    Console.WriteLine(ex1.Message);
                }
            };

            //信息显示
            DownLoader.MessageCallBack += (s, e) =>
            {
                MessageCallBackEventArgs eventArgs = e as MessageCallBackEventArgs;
                HandyControl.Controls.Growl.Error(eventArgs.Message);
            };


        }

        public async void RefreshCurrentPage(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_StopAndTry, "Main");
                return;
            }

            //刷新文件夹
            this.Cursor = Cursors.Wait;

            if (vieModel.IsScanning)
            {
                vieModel.IsScanning = false;
                RefreshScanCTS?.Cancel();
            }
            else
            {
                if (Properties.Settings.Default.ScanWhenRefresh)
                {
                    WaitingPanel.Visibility = Visibility.Visible;
                    await ScanWhenRefresh();
                    WaitingPanel.Visibility = Visibility.Collapsed;
                }
            }
            CancelSelect();
            if (Properties.Settings.Default.ScanWhenRefresh)
                vieModel.Reset();
            else
                vieModel.Refresh();
            this.Cursor = Cursors.Arrow;
        }

        public async Task<bool> ScanWhenRefresh()
        {
            vieModel.IsScanning = true;
            RefreshScanCTS = new CancellationTokenSource();
            RefreshScanCTS.Token.Register(() => { Console.WriteLine("取消任务"); this.Cursor = Cursors.Arrow; });
            RefreshScanCT = RefreshScanCTS.Token;
            await Task.Run(() =>
            {
                List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(System.IO.Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath)), RefreshScanCT);
                double num = Scan.InsertWithNfo(filepaths, RefreshScanCT);
                vieModel.IsScanning = false;

                if (Properties.Settings.Default.AutoDeleteNotExistMovie)
                {
                    //删除不存在影片
                    var movies = DataBase.SelectMoviesBySql("select * from movie");
                    movies.ForEach(movie =>
                    {
                        if (!File.Exists(movie.filepath))
                        {
                            DataBase.DeleteByField("movie", "id", movie.id);
                        }
                    });

                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //vieModel.Reset();
                    if (num > 0) HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_ScanNum} {num} --- {Jvedio.Language.Resources.Message_ViewLog}", "Main");
                }), System.Windows.Threading.DispatcherPriority.Render);


            }, RefreshScanCTS.Token);

            return true;
        }


        public void CancelSelect()
        {
            Properties.Settings.Default.EditMode = false; vieModel.SelectedMovie.Clear(); SetSelected();
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (Properties.Settings.Default.EditMode) { CancelSelect(); return; }
            Properties.Settings.Default.EditMode = true;
            foreach (var item in vieModel.CurrentMovieList)
            {
                if (!vieModel.SelectedMovie.Contains(item))
                {
                    vieModel.SelectedMovie.Add(item);

                }
            }
            SetSelected();


        }




        public void Refresh(InfoUpdateEventArgs eventArgs, double totalcount)
        {
            Dispatcher.Invoke((Action)delegate ()
          {
              ProgressBar.Value = ProgressBar.Maximum * (eventArgs.progress / totalcount);
              ProgressBar.Visibility = Visibility.Visible;
              if (ProgressBar.Value == ProgressBar.Maximum) { DownLoader.State = DownLoadState.Completed; ProgressBar.Visibility = Visibility.Hidden; }
              if (DownLoader.State == DownLoadState.Completed | DownLoader.State == DownLoadState.Fail) ProgressBar.Visibility = Visibility.Hidden;
              RefreshMovieByID(eventArgs.Movie.id);
          });
        }

        public void RefreshMovieByID(string ID)
        {
            Movie movie = DataBase.SelectMovieByID(ID);
            addTag(ref movie);
            if (Properties.Settings.Default.ShowImageMode == "2")
            {

            }
            else
            {
                SetImage(ref movie);
            }
            (Movie currentMovie1, int idx1) = GetMovieFromCurrentMovie(ID);
            (Movie currentMovie2, int idx2) = GetMovieFromAllMovie(ID);
            (Movie currentMovie3, int idx3) = GetMovieFromFilterMovie(ID);

            if (currentMovie1 != null && idx1 < vieModel.CurrentMovieList.Count)
            {
                vieModel.CurrentMovieList[idx1] = null;
                vieModel.CurrentMovieList[idx1] = movie;
            }
            if (currentMovie2 != null && idx2 < vieModel.MovieList.Count)
            {
                vieModel.MovieList[idx2] = null;
                vieModel.MovieList[idx2] = movie;
            }

            if (currentMovie3 != null && idx3 < vieModel.FilterMovieList.Count)
            {
                vieModel.FilterMovieList[idx2] = null;
                vieModel.FilterMovieList[idx2] = movie;
            }
        }




        public void OpenSubSuctionVedio(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            TextBlock textBlock = stackPanel.Children.OfType<TextBlock>().Last();
            string filepath = textBlock.Text;
            PlayVedioWithPlayer(filepath, "");
        }



        private static void OnCreated(object obj, FileSystemEventArgs e)
        {
            //导入数据库

            if (Scan.IsProperMovie(e.FullPath))
            {
                FileInfo fileinfo = new FileInfo(e.FullPath);

                //获取创建日期
                string createDate = "";
                try { createDate = fileinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                catch { }
                if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Movie movie = new Movie()
                {
                    filepath = e.FullPath,
                    id = Identify.GetFanhao(fileinfo.Name),
                    filesize = fileinfo.Length,
                    vediotype = (int)Identify.GetVedioType(Identify.GetFanhao(fileinfo.Name)),
                    otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    scandate = createDate
                };
                if (!string.IsNullOrEmpty(movie.id) & movie.vediotype > 0) { DataBase.InsertScanMovie(movie); }
                Console.WriteLine($"成功导入{e.FullPath}");
            }




        }

        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            {
                DataBase.DeleteByField("movie", "filepath", e.FullPath);
            }
            Console.WriteLine("成功删除" + e.FullPath);
        }



        public FileSystemWatcher[] fileSystemWatcher;
        public string failwatcherMessage = "";

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void AddListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            fileSystemWatcher = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++)
            {
                try
                {

                    if (drives[i] == @"C:\") { continue; }
                    FileSystemWatcher watcher = new FileSystemWatcher
                    {
                        Path = drives[i],
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                        Filter = "*.*"
                    };
                    watcher.Created += OnCreated;
                    watcher.Deleted += OnDeleted;
                    watcher.EnableRaisingEvents = true;
                    fileSystemWatcher[i] = watcher;

                }
                catch
                {
                    failwatcherMessage += drives[i] + ",";
                    continue;
                }
            }

            if (failwatcherMessage != "")
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_WatchFail} {failwatcherMessage}", "Main");
        }

        public async void CheckUrl()
        {
            Console.WriteLine("开始检测");
            vieModel.CheckingUrl = true;
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            //获取网址集合

            List<string> urlList = new List<string>
            {
                Properties.Settings.Default.Bus,
                Properties.Settings.Default.BusEurope,
                Properties.Settings.Default.Library,
                Properties.Settings.Default.DB,
                Properties.Settings.Default.FC2,
                Properties.Settings.Default.Jav321,
                Properties.Settings.Default.DMM
            };

            List<bool> enableList = new List<bool>
            {
                Properties.Settings.Default.EnableBus,
                Properties.Settings.Default.EnableBusEu,
                Properties.Settings.Default.EnableLibrary,
                Properties.Settings.Default.EnableDB,
                Properties.Settings.Default.EnableFC2,
                Properties.Settings.Default.Enable321,
                Properties.Settings.Default.EnableDMM
            };

            for (int i = 0; i < urlList.Count; i++)
            {
                bool enable = enableList[i];
                string url = urlList[i];
                if (enable)
                {
                    bool CanConnect = false; bool enablecookie = false; string cookie = "";
                    if (url == Properties.Settings.Default.DB)
                    {
                        enablecookie = true;
                        cookie = Properties.Settings.Default.DBCookie;
                    }
                    else if (url == Properties.Settings.Default.DMM)
                    {
                        enablecookie = true;
                        cookie = Properties.Settings.Default.DMMCookie;
                    }
                    try
                    {
                        CanConnect = await Net.TestUrl(url, enablecookie, cookie, "DB");
                    }
                    catch (TimeoutException ex) { Logger.LogN($"地址：{url}，失败原因：{ex.Message}"); }

                    if (CanConnect) { if (!result.ContainsKey(url)) result.Add(url, true); } else { if (!result.ContainsKey(url)) result.Add(url, false); }
                }
                else
                   if (!result.ContainsKey(url)) result.Add(url, false);
            }

            try
            {
                this.Dispatcher.Invoke((Action)delegate ()
                {
                    try
                    {

                        if (result[Properties.Settings.Default.Bus]) { BusStatus.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.DB]) { DBStatus.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.Library]) { LibraryStatus.Fill = Brushes.Green; }
                        //if (result[Properties.Settings.Default.Fc2Club]) { FC2Status.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.BusEurope]) { BusEuropeStatus.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.Jav321]) { Jav321Status.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.DMM]) { DMMStatus.Fill = Brushes.Green; }

                    }
                    catch (KeyNotFoundException ex) { Console.WriteLine(ex.Message); }


                });
            }
            catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); }

            bool IsAllConnect = true;
            bool IsOneConnect = false;
            for (int i = 0; i < enableList.Count; i++)
            {
                if (enableList[i])
                {
                    if (result.ContainsKey(urlList[i]))
                    {
                        if (!result[urlList[i]])
                            IsAllConnect = false;
                        else
                            IsOneConnect = true;
                    }
                }
            }
            try
            {
                this.Dispatcher.Invoke((Action)delegate ()
                {

                    if (IsAllConnect)
                        AllStatus.Background = Brushes.Green;
                    else if (!IsAllConnect & !IsOneConnect)
                        AllStatus.Background = Brushes.Red;
                    else if (IsOneConnect & !IsAllConnect)
                        AllStatus.Background = Brushes.Yellow;

                });
                vieModel.CheckingUrl = false;
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void AdjustWindow()
        {
            SetWindowProperty();
            if (Properties.Settings.Default.FirstRun)
            {
                this.Width = SystemParameters.WorkArea.Width * 0.8;
                this.Height = SystemParameters.WorkArea.Height * 0.8;
            }


            HideMargin();

            SideBorder.Width = Properties.Settings.Default.SideGridWidth<200?200: Properties.Settings.Default.SideGridWidth;


            if (Properties.Settings.Default.ShowImageMode == "4")
            {
                MovieMainGrid.Visibility = Visibility.Hidden;
                //DetailGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MovieMainGrid.Visibility = Visibility.Visible;
                //DetailGrid.Visibility = Visibility.Hidden;
            }



        }

        private void SetWindowProperty()
        {
            //读取窗体设置
            WindowConfig cj = new WindowConfig(this.GetType().Name);
            WindowProperty windowProperty = cj.Read();
            Rect rect = new Rect() { Location = windowProperty.Location, Size = windowProperty.Size };
            WinState = windowProperty.WinState;
            //读到属性值
            if (WinState == JvedioWindowState.FullScreen)
            {
                this.WindowState = WindowState.Maximized;
            }
            else if (WinState == JvedioWindowState.None)
            {
                WinState = 0;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                this.Left = rect.X >= 0 ? rect.X : 0;
                this.Top = rect.Y >= 0 ? rect.Y : 0;
                this.Height = rect.Height > 100 ? rect.Height : 100;
                this.Width = rect.Width > 100 ? rect.Width : 100;
                if (this.Width == SystemParameters.WorkArea.Width | this.Height == SystemParameters.WorkArea.Height) { WinState = JvedioWindowState.Maximized; }
            }
        }






        private void Window_Closed(object sender, EventArgs e)
        {
            if (!IsToUpdate && Properties.Settings.Default.CloseToTaskBar && this.IsVisible == true)
            {
                NotifyIcon.Visibility = Visibility.Visible;
                this.Hide();
                WindowSet?.Hide();
                WindowTools?.Hide();
                WindowBatch?.Hide();
                WindowEdit?.Hide();
                window_DBManagement?.Hide();
            }
            else
            {
                StopDownLoad();
                SaveRecentWatched();
                ProgressBar.Visibility = Visibility.Hidden;
                WindowTools windowTools = null;
                foreach (Window item in App.Current.Windows)
                {
                    if (item.GetType().Name == "WindowTools") windowTools = item as WindowTools;
                }

                if (windowTools?.IsVisible == true)
                {
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }


            }
        }

        public async void FadeOut()
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                double opacity = this.Opacity;
                await Task.Run(() =>
                {
                    while (opacity > 0.1)
                    {
                        this.Dispatcher.Invoke((Action)delegate { this.Opacity -= 0.05; opacity = this.Opacity; });
                        Task.Delay(1).Wait();
                    }
                });
                this.Opacity = 0;
            }
            this.Close();
        }


        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            FadeOut();
        }

        public async void MinWindow(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                double opacity = this.Opacity;
                await Task.Run(() =>
                {
                    while (opacity > 0.2)
                    {
                        this.Dispatcher.Invoke((Action)delegate { this.Opacity -= 0.1; opacity = this.Opacity; });
                        Task.Delay(20).Wait();
                    }
                });
            }

            this.WindowState = WindowState.Minimized;
            this.Opacity = 1;

        }


        public void MaxWindow(object sender, RoutedEventArgs e)
        {
            Resizing = true;
            if (WinState == 0)
            {
                //最大化
                WinState = JvedioWindowState.Maximized;
                WindowPoint = new Point(this.Left, this.Top);
                WindowSize = new Size(this.Width, this.Height);
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = SystemParameters.WorkArea.Top;
                this.Left = SystemParameters.WorkArea.Left;

            }
            else
            {
                WinState = JvedioWindowState.Normal;
                this.Left = WindowPoint.X;
                this.Width = WindowSize.Width;
                this.Top = WindowPoint.Y;
                this.Height = WindowSize.Height;
            }
            this.WindowState = WindowState.Normal;
            this.OnLocationChanged(EventArgs.Empty);
            HideMargin();
        }

        private void HideMargin()
        {
            if (WinState == JvedioWindowState.Normal)
            {
                MainGrid.Margin = new Thickness(10);
                MainBorder.Margin = new Thickness(5);
                Grid.Margin = new Thickness(5);
                SideBorder.CornerRadius = new CornerRadius(0, 0, 0, 5);
                MainBorder.CornerRadius = new CornerRadius(5);
                this.ResizeMode = ResizeMode.CanResize;
            }
            else if (WinState == JvedioWindowState.Maximized || this.WindowState == WindowState.Maximized)
            {
                MainGrid.Margin = new Thickness(0);
                MainBorder.Margin = new Thickness(0);
                Grid.Margin = new Thickness(0);
                SideBorder.CornerRadius = new CornerRadius(0);
                MainBorder.CornerRadius = new CornerRadius(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            ResizingTimer.Start();
        }



        private void MoveWindow(object sender, MouseEventArgs e)
        {
            AllSearchPopup.IsOpen = false;


            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized || (this.Width == SystemParameters.WorkArea.Width && this.Height == SystemParameters.WorkArea.Height))
                {
                    WinState = 0;
                    double fracWidth = e.GetPosition(TopBorder).X / TopBorder.ActualWidth;
                    this.Width = WindowSize.Width;
                    this.Height = WindowSize.Height;
                    this.WindowState = WindowState.Normal;
                    this.Left = e.GetPosition(TopBorder).X - TopBorder.ActualWidth * fracWidth;
                    this.Top = e.GetPosition(TopBorder).Y - TopBorder.ActualHeight / 2;
                    this.OnLocationChanged(EventArgs.Empty);
                    HideMargin();
                }
                this.DragMove();
            }
        }

        WindowTools WindowTools;

        private void OpenTools(object sender, RoutedEventArgs e)
        {
            if (WindowTools != null) WindowTools.Close();
            WindowTools = new WindowTools();
            WindowTools.Show();
        }

        Window_DBManagement window_DBManagement;
        private void OpenDataBase(object sender, RoutedEventArgs e)
        {
            if (window_DBManagement != null) window_DBManagement.Close();
            window_DBManagement = new Window_DBManagement();
            window_DBManagement.Show();
        }


        private void OpenUrl(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void OpenFeedBack(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/hitchao/Jvedio/issues");
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/hitchao/Jvedio/wiki");
        }

        private void OpenThanks(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.kancloud.cn/hitchao/jvedio/1921337");
        }

        private void OpenJvedioWebPage(object sender, RoutedEventArgs e)
        {
            Process.Start("https://hitchao.github.io/JvedioWebPage/");
        }


        private void HideGrid(object sender, MouseButtonEventArgs e)
        {
            Grid grid = ((Border)sender).Parent as Grid;
            grid.Visibility = Visibility.Hidden;

        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            AboutGrid.Visibility = Visibility.Visible;
            VersionTextBlock.Text = Jvedio.Language.Resources.Version + $" : {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        }

        private void ShowThanks(object sender, RoutedEventArgs e)
        {
            ThanksGrid.Visibility = Visibility.Visible;
        }

        private void ShowUpdate(object sender, MouseButtonEventArgs e)
        {
            CheckUpdate();
            UpdateGrid.Visibility = Visibility.Visible;
        }





        private void OpenSet_MouseDown(object sender, RoutedEventArgs e)
        {
            if (WindowSet != null) WindowSet.Close();
            WindowSet = new Settings();
            WindowSet.Show();
        }



        public void SearchContent(object sender, MouseButtonEventArgs e)
        {
            Grid grid = ((Canvas)(sender)).Parent as Grid;
            TextBox SearchTextBox = grid.Children.OfType<TextBox>().First() as TextBox;
            if (grid.Name == "AllSearchGrid") { vieModel.SearchAll = true; } else { vieModel.SearchAll = false; }
            vieModel.Search = SearchTextBox.Text;
        }





        private void SetSearchValue(object sender, MouseButtonEventArgs e)
        {
            SearchBar.Text = ((TextBlock)sender).Text;
            SearchBar.Select(SearchBar.Text.Length, 0);
            AllSearchPopup.IsOpen = false;
            Resizing = true;
            ResizingTimer.Start();
            vieModel.Search = SearchBar.Text;
        }

        private void SearchBar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AllSearchGrid.Width > 300) return;
            //动画
            DoubleAnimation doubleAnimation = new DoubleAnimation(300, 400, new Duration(TimeSpan.FromMilliseconds(200)));
            AllSearchGrid.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation);
            //边框颜色
            Color color1 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString());
            Color color2 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundSearch"].ToString());
            AllSearchBorder.BorderBrush = new SolidColorBrush(color1);
            ColorAnimation colorAnimation = new ColorAnimation(color1, color2, new Duration(TimeSpan.FromMilliseconds(200)));
            AllSearchBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void SearchBar_LostFocus()
        {
            if (AllSearchGrid.Width <400) return;
            AllSearchPopup.IsOpen = false;
            DoubleAnimation doubleAnimation = new DoubleAnimation(400, 300, new Duration(TimeSpan.FromMilliseconds(200)));
            AllSearchGrid.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation);
            //边框颜色
            Color color1 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString());
            Color color2 = (Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundSearch"].ToString());
            AllSearchBorder.BorderBrush = new SolidColorBrush(color2);
            ColorAnimation colorAnimation = new ColorAnimation(color2, color1, new Duration(TimeSpan.FromMilliseconds(200)));
            AllSearchBorder.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

        }


        public bool CanSearch = false;

        private void RefreshCandiadte(object sender, TextChangedEventArgs e)
        {
            AllSearchPopup.IsOpen = true;
            vieModel?.GetSearchCandidate(SearchBar.Text);
        }


        private int SearchSelectIdex = -1;

        private void SearchBar_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vieModel.Search = SearchBar.Text;
                AllSearchPopup.IsOpen = false;
            }
            else if (e.Key == Key.Down)
            {
                int count = vieModel.CurrentSearchCandidate.Count;

                SearchSelectIdex += 1;
                if (SearchSelectIdex >= count) SearchSelectIdex = 0;
                SetSearchSelect();

            }
            else if (e.Key == Key.Up)
            {
                int count = vieModel.CurrentSearchCandidate.Count;
                SearchSelectIdex -= 1;
                if (SearchSelectIdex < 0) SearchSelectIdex = count - 1;
                SetSearchSelect();


            }
            else if (e.Key == Key.Escape)
            {
                AllSearchPopup.IsOpen = false;
            }
            else if (e.Key == Key.Delete)
            {
                SearchBar.Clear();
            }
        }

        private void SetSearchSelect()
        {
            for (int i = 0; i < SearchItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)SearchItemsControl.ItemContainerGenerator.ContainerFromItem(SearchItemsControl.Items[i]);
                StackPanel stackPanel = FindElementByName<StackPanel>(c, "SearchStackPanel");
                if (stackPanel != null)
                {
                    TextBlock textBlock = stackPanel.Children[0] as TextBlock;
                    if (i == SearchSelectIdex)
                        textBlock.Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
                    else
                        textBlock.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

        }


        private void ShowMovieGrid(object sender, RoutedEventArgs e)
        {
            Grid_Classify.Visibility = Visibility.Hidden;
            Grid_Movie.Visibility = Visibility.Visible;
            ActorInfoGrid.Visibility = Visibility.Collapsed;
            ScrollViewer.ScrollToTop();


        }

        private void ShowClassifyGrid(object sender, RoutedEventArgs e)
        {
            Grid_Movie.Visibility = Visibility.Hidden;
            Grid_Classify.Visibility = Visibility.Visible;
            this.vieModel.ClickGridType = 1;
            LoveActressCheckBox.IsChecked = false;
        }

        private void HideListScrollViewer(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string name = button.Content.ToString();

            if (name == Jvedio.Language.Resources.Hide)
            {
                button.Content = Jvedio.Language.Resources.Show;
                ListScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                button.Content = Jvedio.Language.Resources.Hide;
                ListScrollViewer.Visibility = Visibility.Visible;
            }




        }




        private void ShowLabelEditGrid(object sender, RoutedEventArgs e)
        {
            //LabelEditGrid.Visibility = Visibility.Visible;
        }

        public void Tag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string tag = label.Content.ToString();
            vieModel.GetMoviebyTag(tag);
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string type = sender.GetType().ToString();
            string tag = "";
            if (type == "System.Windows.Controls.Label")
            {
                Label Tag = (Label)sender;
                tag = Tag.Content.ToString();
                Match match = Regex.Match(tag, @"\( \d+ \)");
                if (match != null && match.Value != "")
                {
                    tag = tag.Replace(match.Value, "");
                }
            }
            else if (type == "HandyControl.Controls.Tag")
            {
                HandyControl.Controls.Tag Tag = (HandyControl.Controls.Tag)sender;
                tag = Tag.Content.ToString();
            }
            vieModel.GetMoviebyLabel(tag);
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Studio_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string genre = label.Content.ToString();
            vieModel.GetMoviebyStudio(genre);
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Director_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string genre = label.Content.ToString();
            vieModel.GetMoviebyDirector(genre);
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Genre_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string genre = label.Content.ToString().Split('(').First();
            vieModel.GetMoviebyGenre(genre);
            ShowMovieGrid(sender, new RoutedEventArgs());
            vieModel.TextType = genre;
        }

        public void ShowActorMovieFromDetailWindow(Actress actress)
        {
            vieModel.GetMoviebyActress(actress);
            actress = DataBase.SelectInfoFromActress(actress);
            actress.smallimage = GetActorImage(actress.name);
            vieModel.Actress = actress;
            ShowMovieGrid(this, new RoutedEventArgs());
            ActorInfoGrid.Visibility = Visibility.Visible;
        }

        public void ActorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SelectedActress.Clear();
            ActorSetSelected();
        }

        public void ActorSetSelected()
        {
            for (int i = 0; i < ActorItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ActorItemsControl.ItemContainerGenerator.ContainerFromItem(ActorItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "ActorWrapPanel");
                if (wrapPanel != null)
                {
                    Grid grid = wrapPanel.Children[0] as Grid;
                    Border border = grid.Children[0] as Border;
                    if (c.ContentTemplate.FindName("ActorNameTextBox", c) is TextBox textBox)
                    {
                        //DropShadowEffect dropShadowEffect = new DropShadowEffect() { Color = Colors.SkyBlue, BlurRadius = 10, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
                        //border.Effect = dropShadowEffect;
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                        foreach (Actress actress in SelectedActress)
                        {
                            if (actress.name == textBox.Text.Split('(')[0])
                            {
                                border.Background = (SolidColorBrush)Application.Current.Resources["Selected_Background"];
                                break;
                                //dropShadowEffect = new DropShadowEffect() { Color = Colors.OrangeRed, BlurRadius = 10, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
                                //border.Effect = dropShadowEffect;
                            }
                        }
                    }
                }
            }

        }


        public void BorderMouseEnter(object sender, RoutedEventArgs e)
        {

            if (Properties.Settings.Default.EditMode)
            {
                Image image = sender as Image;
                Grid grid = image.Parent as Grid;
                StackPanel stackPanel = grid.Parent as StackPanel;
                Grid grid1 = stackPanel.Parent as Grid;
                Border border = grid1.Children[0] as Border;
                border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
            }
        }

        public void BorderMouseLeave(object sender, RoutedEventArgs e)
        {

            if (Properties.Settings.Default.EditMode)
            {
                Image image = sender as Image;
                Grid grid = image.Parent as Grid;
                StackPanel stackPanel = grid.Parent as StackPanel;
                Grid grid1 = stackPanel.Parent as Grid;
                Border border = grid1.Children[0] as Border;
                border.BorderBrush = Brushes.Transparent;
            }
        }



        public void ActorBorderMouseEnter(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            Border border = ((Grid)stackPanel.Parent).Children[0] as Border;
            if (Properties.Settings.Default.ActorEditMode)
            {

                border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
            }
            else
            {
                border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundTitle"];
            }

        }

        public void ActorBorderMouseLeave(object sender, RoutedEventArgs e)
        {
            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            Border border = ((Grid)stackPanel.Parent).Children[0] as Border;
            if (Properties.Settings.Default.ActorEditMode)
            {
                border.BorderBrush = Brushes.Transparent;
            }
            else
            {
                border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
            }
        }

        public void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            string name = textBox.Text.Split('(')[0];
            if (Properties.Settings.Default.ActorEditMode)
            {
                foreach (Actress actress in vieModel.ActorList)
                {
                    if (actress.name == name)
                    {
                        if (SelectedActress.Contains(actress))
                            SelectedActress.Remove(actress);
                        else
                            SelectedActress.Add(actress);
                        break;
                    }

                }
                ActorSetSelected();
            }
            else
            {
                Actress actress = new Actress();
                foreach (Actress item in vieModel.ActorList)
                {
                    if (item.name == name)
                    {
                        actress.name = name;
                        actress.smallimage = item.smallimage;
                        actress = DataBase.SelectInfoFromActress(actress);
                        break;
                    }
                }
                actress.id = "";//不按照 id 选取演员
                vieModel.GetMoviebyActressAndVetioType(actress);
                vieModel.Actress = actress;
                vieModel.FlipOver();
                ShowMovieGrid(sender, new RoutedEventArgs());
                ActorInfoGrid.Visibility = Visibility.Visible;
                vieModel.TextType = actress.name;
            }
        }


        private (Movie, int) GetMovieFromAllMovie(string id)
        {
            Movie result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
            {
                if (vieModel.CurrentMovieList[i].id == id)
                {
                    result = vieModel.CurrentMovieList[i];
                    idx = i;
                    break;
                }
            }
            return (result, idx);
        }

        private (Movie, int) GetMovieFromCurrentMovie(string id)
        {
            Movie result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
            {
                if (vieModel.CurrentMovieList[i].id == id)
                {
                    result = vieModel.CurrentMovieList[i];
                    idx = i;
                    break;
                }
            }
            return (result, idx);
        }

        private (Movie, int) GetMovieFromFilterMovie(string id)
        {
            Movie result = null;
            int idx = 0;
            for (int i = 0; i < vieModel.FilterMovieList.Count; i++)
            {
                if (vieModel.FilterMovieList[i].id == id)
                {
                    result = vieModel.FilterMovieList[i];
                    idx = i;
                    break;
                }
            }
            return (result, idx);
        }

        public int idx1;
        public int idx2;

        WindowDetails wd;
        //TODO
        private void ShowDetails(object sender, MouseEventArgs e)
        {
            if (Resizing || !canShowDetails) return;
            StackPanel parent = ((sender as FrameworkElement).Parent as Grid).Parent as StackPanel;
            var TB = parent.Children.OfType<TextBox>().First();//识别码
            string id = TB.Text;
            if (Properties.Settings.Default.EditMode)
            {
                (Movie movie, int selectIdx) = GetMovieFromCurrentMovie(id);
                if (vieModel.SelectedMovie.Contains(movie))
                {
                    vieModel.SelectedMovie.Remove(movie);
                }
                else
                {
                    vieModel.SelectedMovie.Add(movie);
                }



                SetSelected();
            }
            else
            {
                StopDownLoad();

                if (wd != null)  wd.Close(); 
                wd = new WindowDetails(TB.Text);
                wd.Show();
            }

            canShowDetails = false;
        }


        private bool canShowDetails = false;
        private void CanShowDetails(object sender, MouseEventArgs e)
        {
            canShowDetails = true;
        }
        public void ShowSideBar(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.ShowSideBar)
            {
                vieModel.ShowSideBar = false;
            }
            else { vieModel.ShowSideBar = true; }

        }



        public void ShowStatus(object sender, RoutedEventArgs e)
        {
            if (StatusPopup.IsOpen == true)
                StatusPopup.IsOpen = false;
            else
                StatusPopup.IsOpen = true;

        }

        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            DownloadPopup.IsOpen = true;
        }

        public void ShowSortPopup(object sender, MouseButtonEventArgs e)
        {
            SortPopup.IsOpen = true;
        }

        public void ShowImagePopup(object sender, MouseButtonEventArgs e)
        {
            ImageSortPopup.IsOpen = true;
        }


        public void ShowMenu(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            Popup popup = grid.Children.OfType<Popup>().First();
            popup.IsOpen = true;
        }




        public void ShowDownloadMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadPopup.IsOpen = true;
        }





        public void ShowSearchMenu(object sender, MouseButtonEventArgs e)
        {
            SearchOptionPopup.IsOpen = true;
        }





        //TODO
        /// <summary>
        /// 演员里的视频类型分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SetTypeValue(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            int idx = ClassifyVedioTypeStackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            vieModel.ClassifyVedioType=(VedioType)idx;
            //刷新侧边栏显示
            TabControl_SelectionChanged(sender, null);
        }


        public void ShowDownloadActorMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadActorPopup.IsOpen = true;
        }



        public void SetSelected()
        {
            for (int i = 0; i < MovieItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)MovieItemsControl.ItemContainerGenerator.ContainerFromItem(MovieItemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "MovieBorder");
                if (border != null)
                {
                    if (c.ContentTemplate.FindName("idTextBox", c) is TextBox textBox)
                    {
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                        foreach (Movie movie in vieModel.SelectedMovie)
                        {
                            if (movie.id == textBox.Text)
                            {
                                border.Background = (SolidColorBrush)Application.Current.Resources["Selected_Background"];
                                break;
                            }
                        }

                    }
                }

            }

        }

        public T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            if (element == null) return childElement;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }

            return childElement;
        }







        public void SetSortValue(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            var rbs = SortStackPanel.Children.OfType<RadioButton>().ToList();
            int sortindex = rbs.IndexOf(radioButton);
            Sort sorttype = (Sort)sortindex;

            if (sorttype == vieModel.SortType) Properties.Settings.Default.SortDescending = !Properties.Settings.Default.SortDescending;

            vieModel.SortDescending = Properties.Settings.Default.SortDescending;
            Properties.Settings.Default.SortType =((int)sorttype).ToString();
            Properties.Settings.Default.Save();
            vieModel.SortType = sorttype;
            vieModel.Sort();
            if (vieModel.SortDescending)
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_down.png", UriKind.Relative));
            else
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_up.png", UriKind.Relative));
            vieModel.FlipOver();
        }

        public void SaveAllSearchType(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            StackPanel stackPanel = radioButton.Parent as StackPanel;
            radioButton.IsChecked = true;
            int idx = stackPanel.Children.OfType<RadioButton>().ToList().IndexOf(radioButton);
            Properties.Settings.Default.AllSearchType = idx.ToString();
            vieModel?.GetSearchCandidate(SearchBar.Text);
            vieModel.SearchHint = Jvedio.Language.Resources.Search + radioButton.Content.ToString();
            Properties.Settings.Default.Save();
            vieModel.AllSearchType = Properties.Settings.Default.AllSearchType.Length == 1 ? (MySearchType)int.Parse(Properties.Settings.Default.AllSearchType) : 0;

        }


        public void SaveShowViewMode(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuItem father = menuItem.Parent as MenuItem;
            int idx = father.Items.IndexOf(menuItem);

            for (int i = 0; i < father.Items.Count; i++)
            {
                MenuItem item =(MenuItem) father.Items[i];
                if (i == idx)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }


            Properties.Settings.Default.ShowViewMode = idx.ToString();
            Properties.Settings.Default.Save();
            vieModel.FlipOver();
        }

        public void SaveVedioType(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            MenuItem father = menuItem.Parent as MenuItem;
            int idx = father.Items.IndexOf(menuItem);
            

            for (int i = 0; i < father.Items.Count; i++)
            {
                MenuItem item = (MenuItem)father.Items[i];
                if (i == idx)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }

            Properties.Settings.Default.VedioType = idx.ToString();
            Properties.Settings.Default.Save();
            vieModel.FlipOver();
        }


        public void SaveShowImageMode(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            var rbs =ImageTypeStackPanel.Children.OfType<RadioButton>().ToList();
            int sortindex = rbs.IndexOf(radioButton);
            MyImageType imageType = (MyImageType)sortindex;


            Properties.Settings.Default.ShowImageMode = sortindex.ToString();
            Properties.Settings.Default.Save();

            if (sortindex ==4)
            {
                MovieMainGrid.Visibility = Visibility.Hidden;
                //DetailGrid.Visibility = Visibility.Visible;
                vieModel.ShowDetailsData();
            }
            else
            {
                MovieMainGrid.Visibility = Visibility.Visible;
                //DetailGrid.Visibility = Visibility.Hidden;
                vieModel.FlowNum = 0;
                vieModel.FlipOver();
            }

            if (sortindex == 0)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.SmallImage_Width;
            else if (sortindex == 1)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.BigImage_Width;
            else if (sortindex == 2)
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.ExtraImage_Width;
        }


        public List<ImageSlide> ImageSlides;
        public void Loadslide()
        {
            ImageSlides?.Clear();
            ImageSlides = new List<ImageSlide>();
            for (int i = 0; i < MovieItemsControl.Items.Count; i++)
            {
                ContentPresenter myContentPresenter = (ContentPresenter)MovieItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (myContentPresenter != null)
                {
                    Movie movie = (Movie)MovieItemsControl.Items[i];
                    DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                    Image myImage = (Image)myDataTemplate.FindName("myImage", myContentPresenter);
                    Image myImage2 = (Image)myDataTemplate.FindName("myImage2", myContentPresenter);

                    ImageSlide imageSlide = new ImageSlide(BasePicPath + $"ExtraPic\\{movie.id}", myImage, myImage2);
                    ImageSlides.Add(imageSlide);
                    imageSlide.PlaySlideShow();
                }

            }
        }






        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //无进度条时
            ScrollViewer sv = sender as ScrollViewer;
            if (sv.VerticalOffset == 0 && sv.VerticalOffset == sv.ScrollableHeight && vieModel.CurrentMovieList.Count < Properties.Settings.Default.DisplayNumber && !IsFlowing)
            {
                IsFlowing = true;
                LoadMovie();
            }
        }

        private void LoadMovie()
        {
            vieModel.FlowNum++;
            vieModel.Flow();
        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //流动模式
            ScrollViewer sv = sender as ScrollViewer;
            if (sv.VerticalOffset >= 500)
                GoToTopCanvas.Visibility = Visibility.Visible;
            else
                GoToTopCanvas.Visibility = Visibility.Hidden;

            if (!IsFlowing && sv.ScrollableHeight - sv.VerticalOffset <= 10 && sv.VerticalOffset != 0)
            {
                Console.WriteLine("1");
                if (!IsFlowing && vieModel.CurrentMovieList.Count < Properties.Settings.Default.DisplayNumber && vieModel.CurrentMovieList.Count < vieModel.FilterMovieList.Count && vieModel.CurrentMovieList.Count + (vieModel.CurrentPage - 1) * Properties.Settings.Default.DisplayNumber < vieModel.FilterMovieList.Count)
                {
                    IsFlowing = true;
                    LoadMovie();
                }

            }

        }

        public bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        public void GotoTop(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer.ScrollToTop();
        }

        public void PlayVedio(object sender, MouseButtonEventArgs e)
        {
            StackPanel parent = ((sender as FrameworkElement).Parent as Grid).Parent as StackPanel;
            var IDTb = parent.Children.OfType<TextBox>().First();
            string filepath = DataBase.SelectInfoByID("filepath", "movie", IDTb.Text);
            PlayVedioWithPlayer(filepath, IDTb.Text);

        }

        public void PlayVedioWithPlayer(string filepath, string ID)
        {
            if (File.Exists(filepath))
            {

                if (!string.IsNullOrEmpty(Properties.Settings.Default.VedioPlayerPath) && File.Exists(Properties.Settings.Default.VedioPlayerPath))
                {
                    try
                    {
                        Process.Start(Properties.Settings.Default.VedioPlayerPath, filepath);
                        vieModel.AddToRecentWatch(ID);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Logger.LogE(ex);
                        Process.Start(filepath);
                    }

                }
                else
                {
                    //使用默认播放器
                    Process.Start(filepath);
                    vieModel.AddToRecentWatch(ID);
                }
            }
            else
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_OpenFail + filepath, "Main");
            }

        }


        public void OpenImagePath(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                MenuItem _mnu = sender as MenuItem;
                StackPanel sp = null;
                if (_mnu.Parent is MenuItem mnu)
                {
                    int index = mnu.Items.IndexOf(_mnu);
                    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                    string failpath = ""; int num = 0;
                    string filepath = "";
                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {

                        filepath = arg.filepath;
                        if (index == 0) { filepath = arg.filepath; }
                        else if (index == 1) { filepath = BasePicPath + $"BigPic\\{arg.id}.jpg"; }
                        else if (index == 2) { filepath = BasePicPath + $"SmallPic\\{arg.id}.jpg"; }
                        else if (index == 3) { filepath = BasePicPath + $"Gif\\{arg.id}.gif"; }
                        else if (index == 4) { filepath = BasePicPath + $"ExtraPic\\{arg.id}\\"; }
                        else if (index == 5) { filepath = BasePicPath + $"ScreenShot\\{arg.id}\\"; }
                        else if (index == 6) { if (arg.actor.Length > 0) filepath = BasePicPath + $"Actresses\\{arg.actor.Split(actorSplitDict[arg.vediotype])[0]}.jpg"; else filepath = ""; }

                        if (index == 4 | index == 5)
                        {
                            if (Directory.Exists(filepath)) { Process.Start("explorer.exe", "\"" + filepath + "\""); }
                            else
                            {
                                if (vieModel.SelectedMovie.Count == 1)
                                {
                                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {filepath}", "Main");
                                }
                                failpath += filepath + "\n";
                                num++;
                            }
                        }
                        else
                        {
                            if (File.Exists(filepath)) { Process.Start("explorer.exe", "/select, \"" + filepath + "\""); }
                            else
                            {
                                if (vieModel.SelectedMovie.Count == 1)
                                {
                                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {filepath}", "Main");
                                }
                                failpath += filepath + "\n";
                                num++;
                            }
                        }




                    });
                    //if (!string.IsNullOrEmpty(failpath) && vieModel.SelectedMovie.Count==1)
                    //    HandyControl.Controls.Growl.Info($"成功打开{vieModel.SelectedMovie.Count - num}个，失败{num}个，因为不存在{filepath}", "Main");
                    //else
                    //    HandyControl.Controls.Growl.Info($"成功打开{vieModel.SelectedMovie.Count - num}个，失败{num}个，因为不存在", "Main");
                }

            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public void OpenFilePath(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                MenuItem _mnu = sender as MenuItem;
                StackPanel sp = null;
                if (_mnu.Parent is MenuItem mnu)
                {
                    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                    string failpath = ""; int num = 0;
                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {
                        if (File.Exists(arg.filepath)) { Process.Start("explorer.exe", "/select, \"" + arg.filepath + "\""); }
                        else
                        {
                            if (vieModel.SelectedMovie.Count == 1)
                            {
                                HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.NotExists}  {arg.filepath}", "Main");
                            }
                            failpath += arg.filepath + "\n";
                            num++;
                        }
                    });
                    //if (failpath != "")
                    //    HandyControl.Controls.Growl.Info($"成功打开{vieModel.SelectedMovie.Count - num}个，失败{num}个，因为文件夹不存在 ：\n{failpath}", "Main");

                }

            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        public async void TranslateMovie(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetYoudao, "Main");
                return;
            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            StackPanel sp = null;
            if (((MenuItem)(sender)).Parent is MenuItem mnu)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                string result = "";
                MySqlite dataBase = new MySqlite("Translate");


                int successNum = 0;
                int failNum = 0;
                int translatedNum = 0;

                foreach (Movie movie in vieModel.SelectedMovie)
                {

                    //检查是否已经翻译过，如有则跳过
                    if (!string.IsNullOrEmpty(dataBase.SelectByField("translate_title", "youdao", movie.id))) { translatedNum++; continue; }
                    if (movie.title != "")
                    {

                        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.title);
                        //保存
                        if (result != "")
                        {

                            dataBase.SaveYoudaoTranslateByID(movie.id, movie.title, result, "title");

                            //显示
                            int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                            int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                            int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
                            movie.title = result;
                            try
                            {
                                vieModel.CurrentMovieList[index1] = null;
                                vieModel.MovieList[index2] = null;
                                vieModel.FilterMovieList[index3] = null;
                                vieModel.CurrentMovieList[index1] = movie;
                                vieModel.MovieList[index2] = movie;
                                vieModel.FilterMovieList[index3] = movie;
                                successNum++;
                            }
                            catch (ArgumentNullException) { }

                        }

                    }
                    else { failNum++; }

                    if (movie.plot != "")
                    {
                        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.plot);
                        //保存
                        if (result != "")
                        {
                            dataBase.SaveYoudaoTranslateByID(movie.id, movie.plot, result, "plot");
                            dataBase.CloseDB();
                        }

                    }

                }
                dataBase.CloseDB();
                HandyControl.Controls.Growl.Success($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum}", "Main");
                HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FailNum} {failNum}", "Main");
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SkipNum} {translatedNum}", "Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }



        public async void GenerateActor(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetBaiduAI, "Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            StackPanel sp = null;
            if (_mnu.Parent is MenuItem mnu)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                this.Cursor = Cursors.Wait;
                int successNum = 0;

                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    if (movie.actor == "") continue;
                    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";

                    string name;
                    if (ActorInfoGrid.Visibility == Visibility.Visible)
                        name = vieModel.Actress.name;
                    else
                        name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];


                    string ActressesPicPath = Properties.Settings.Default.BasePicPath + $"Actresses\\{name}.jpg";
                    if (File.Exists(BigPicPath))
                    {
                        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
                        if (int32Rect != Int32Rect.Empty)
                        {
                            await Task.Delay(500);
                            //切割演员头像
                            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                            ImageSource actressImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetActressRect(bitmapImage, int32Rect));
                            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(actressImage);
                            try { bitmap.Save(ActressesPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++; }
                            catch (Exception ex) { Logger.LogE(ex); }
                        }
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_PosterMustExist, "Main");
                    }
                }
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", "Main");
                //更新到窗口中
                foreach (Movie movie1 in vieModel.SelectedMovie)
                {
                    if (!string.IsNullOrEmpty(movie1.actor) && movie1.actor.IndexOf(vieModel.Actress.name) >= 0)
                    {
                        vieModel.Actress.smallimage = GetActorImage(vieModel.Actress.name);
                        break;
                    }
                }


            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }



        public void GetGif(object sender, RoutedEventArgs e)
        {
            return;
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetFFmpeg, "Main");
                return;
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = ((MenuItem)(sender)).Parent as MenuItem;
            StackPanel sp = null;

            if (mnu != null)
            {
                int successNum = 0;
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                this.Cursor = Cursors.Wait;
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    if (!File.Exists(movie.filepath)) { HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, "Main"); continue; }
                    bool result = false;
                    try { GenerateGif(movie); } catch (Exception ex) { Logger.LogF(ex); }

                    if (result) successNum++;
                }
                //HandyControl.Controls.Growl.Info( $"成功截图 {successNum} / {vieModel.SelectedMovie.Count} 个影片","Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }

        private void ScreenShot(object sender,MouseButtonEventArgs e)
        {
            new HandyControl.Controls.Screenshot().Start();
        }

        public async void GetScreenShot(object sender, RoutedEventArgs e)
        {

            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetFFmpeg, "Main");
                return;
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = ((MenuItem)(sender)).Parent as MenuItem;
            StackPanel sp = null;

            if (mnu != null)
            {
                int successNum = 0;
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                this.Cursor = Cursors.Wait;
                cmdTextBox.Text = "";
                cmdGrid.Visibility = Visibility.Visible;
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    if (!File.Exists(movie.filepath)) { HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, "Main"); continue; }
                    bool success = false;
                    string message = "";
                    ScreenShot screenShot = new ScreenShot();
                    screenShot.SingleScreenShotCompleted += (s, ev) =>
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Console.WriteLine(((ScreenShotEventArgs)ev).FFmpegCommand);
                            cmdTextBox.AppendText($"{Jvedio.Language.Resources.SuccessScreenShotTo} ： {((ScreenShotEventArgs)ev).FilePath}\n");
                            cmdTextBox.ScrollToEnd();
                        });
                    };
                    (success, message) = await screenShot.AsyncScreenShot(movie);
                    if (success) successNum++;
                    else this.Dispatcher.Invoke((Action)delegate { cmdTextBox.AppendText($"{Jvedio.Language.Resources.Message_Fail}，{Jvedio.Language.Resources.Reason}：{message}"); });
                }
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", "Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;

        }



        public void BeginGenGif(object o)
        {
            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string filePath = list[1] as string;
            string ScreenShotPath = list[2] as string;
            string ID = list[3] as string;
            ScreenShotPath += ID + ".gif";

            if (string.IsNullOrEmpty(cutoffTime)) return;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -t 5 -ss {cutoffTime} -i \"{filePath}\" -s 280x170  \"{ScreenShotPath}\"";
            Console.WriteLine(str);


            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                //显示到界面上
                Movie movie = vieModel.MovieList.Where(arg => arg.id == ID).First();
                int idx1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == ID).First());
                int idx2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == ID).First());
                int idx3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == ID).First());
                //movie.gif = null;
                //if (File.Exists(BasePicPath + $"Gif\\{movie.id}.gif"))
                //    movie.gif = new Uri("pack://siteoforigin:,,,/" + BasePicPath.Replace("\\", "/") + $"Gif/{movie.id}.gif");
                //else
                //    movie.gif = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_B.gif");


                try
                {
                    vieModel.CurrentMovieList[idx1] = null;
                    vieModel.MovieList[idx2] = null;
                    vieModel.FilterMovieList[idx3] = null;
                    vieModel.CurrentMovieList[idx1] = movie;
                    vieModel.MovieList[idx2] = movie;
                    vieModel.FilterMovieList[idx3] = movie;
                }
                catch (ArgumentNullException ex) { }

                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");

            });
        }




        public void GenerateGif(Movie movie)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetFFmpeg, "Main");
                return;
            }


            string GifSavePath = BasePicPath + "Gif\\";
            if (!Directory.Exists(GifSavePath)) Directory.CreateDirectory(GifSavePath);



            string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
            if (cutoffArray.Length == 0) return;
            string startTime = cutoffArray[new Random().Next(cutoffArray.Length)];

            List<object> list = new List<object>() { startTime, movie.filepath, GifSavePath, movie.id };
            Thread threadObject = new Thread(BeginGenGif);
            threadObject.Start(list);

        }

        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_SetBaiduAI, "Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                 int successNum = 0;
                this.Cursor = Cursors.Wait;
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
                    string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
                    if (File.Exists(BigPicPath))
                    {
                        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);

                        if (int32Rect != Int32Rect.Empty)
                        {
                            await Task.Delay(500);
                            //切割缩略图
                            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                            ImageSource smallImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetRect(bitmapImage, int32Rect));
                            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
                            try
                            {
                                bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++;
                            }
                            catch (Exception ex) { Logger.LogE(ex); }


                            //读取
                            int index1 = vieModel.CurrentMovieList.IndexOf(movie);
                            int index2 = vieModel.MovieList.IndexOf(movie);
                            int index3 = vieModel.FilterMovieList.IndexOf(movie);
                            movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");

                            vieModel.CurrentMovieList[index1] = null;
                            vieModel.MovieList[index2] = null;
                            vieModel.FilterMovieList[index3] = null;
                            vieModel.CurrentMovieList[index1] = movie;
                            vieModel.MovieList[index2] = movie;
                            vieModel.FilterMovieList[index3] = movie;
                        }

                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_PosterMustExist, "Main");
                    }

                }
                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successNum} / {vieModel.SelectedMovie.Count}", "Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }


        public void RenameFile(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.RenameFormat.IndexOf("{") < 0)
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetRenameRule, "Main");
                return;
            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                StringCollection paths = new StringCollection();
                int num = 0;
                vieModel.SelectedMovie.ToList().ForEach(arg => { if (File.Exists(arg.filepath)) { paths.Add(arg.filepath); } });
                if (paths.Count > 0)
                {
                    //重命名文件
                    foreach (Movie m in vieModel.SelectedMovie)
                    {
                        if (!File.Exists(m.filepath)) continue;
                        DetailMovie movie = DataBase.SelectDetailMovieById(m.id);
                        //try
                        //{
                        string[] newPath = movie.ToFileName();
                        if (movie.hassubsection)
                        {
                            for (int i = 0; i < newPath.Length; i++)
                            {
                                File.Move(movie.subsectionlist[i], newPath[i]);
                            }
                            num++;

                            //显示
                            int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                            int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                            int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
                            movie.filepath = newPath[0];
                            movie.subsection = string.Join(";", newPath);
                            try
                            {
                                vieModel.CurrentMovieList[index1].filepath = movie.filepath;
                                vieModel.MovieList[index2].filepath = movie.filepath;
                                vieModel.CurrentMovieList[index1].subsection = movie.subsection;
                                vieModel.MovieList[index2].subsection = movie.subsection;
                                vieModel.FilterMovieList[index3].filepath = movie.filepath;
                                vieModel.FilterMovieList[index3].subsection = movie.subsection;
                            }
                            catch (ArgumentNullException) { }
                            DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                            DataBase.UpdateMovieByID(movie.id, "subsection", movie.subsection, "string");//保存
                            if (vieModel.SelectedMovie.Count == 1) HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "Main");
                        }
                        else
                        {
                            if (!File.Exists(newPath[0]))
                            {
                                File.Move(movie.filepath, newPath[0]);
                                num++;
                                //显示
                                int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                                int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                                int index3 = vieModel.FilterMovieList.IndexOf(vieModel.FilterMovieList.Where(arg => arg.id == movie.id).First());
                                movie.filepath = newPath[0];
                                try
                                {
                                    vieModel.CurrentMovieList[index1].filepath = movie.filepath;
                                    vieModel.MovieList[index2].filepath = movie.filepath;
                                    vieModel.FilterMovieList[index3].filepath = movie.filepath;
                                }
                                catch (ArgumentNullException) { }
                                DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                                if (vieModel.SelectedMovie.Count == 1) HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_Success, "Main");
                            }
                            else
                            {
                                //存在同名文件
                                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_Fail, "Main");
                            }

                        }


                        //}catch(Exception ex)
                        //{
                        //    HandyControl.Controls.Growl.Error(ex.Message);
                        //    continue;
                        //}
                    }
                    HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {num}/{vieModel.SelectedMovie.Count} ", "Main");
                }
                else
                {
                    //文件不存在！无法重命名！
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, "Main");
                }



            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }



        public void ReMoveZero(object sender, RoutedEventArgs e)
        {


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                int successnum = 0;
                for (int i = 0; i < vieModel.SelectedMovie.Count; i++)
                {
                    Movie movie = vieModel.SelectedMovie[i];
                    string oldID = movie.id.ToUpper();

                    Console.WriteLine(vieModel.CurrentMovieList[0].id);

                    if (oldID.IndexOf("-") > 0)
                    {
                        string num = oldID.Split('-').Last();
                        string eng = oldID.Remove(oldID.Length - num.Length, num.Length);
                        if (num.Length == 5 && eng.Replace("-", "").All(char.IsLetter))
                        {
                            string newID = eng + num.Remove(0, 2);
                            if (DataBase.SelectMovieByID(newID) == null)
                            {
                                Movie newMovie = DataBase.SelectMovieByID(oldID);
                                DataBase.DeleteByField("movie", "id", oldID);
                                newMovie.id = newID;
                                DataBase.InsertFullMovie(newMovie);
                                UpdateInfo(oldID, newID);
                                successnum++;
                            }
                        }


                    }
                }

                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_SuccessNum} {successnum}/{vieModel.SelectedMovie.Count}", "Main");




            }

            vieModel.SelectedMovie.Clear();
            SetSelected();
        }

        private void UpdateInfo(string oldID, string newID)
        {
            Movie movie = DataBase.SelectMovieByID(newID);
            SetImage(ref movie);

            for (int i = 0; i < vieModel.CurrentMovieList.Count; i++)
            {
                try
                {
                    if (vieModel.CurrentMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        vieModel.CurrentMovieList[i] = null;
                        vieModel.CurrentMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }


            for (int i = 0; i < vieModel.MovieList.Count; i++)
            {
                try
                {
                    if (vieModel.MovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        vieModel.MovieList[i] = null;
                        vieModel.MovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }

            for (int i = 0; i < vieModel.FilterMovieList.Count; i++)
            {
                try
                {
                    if (vieModel.FilterMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        vieModel.FilterMovieList[i] = null;
                        vieModel.FilterMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }
        }


        public void CopyFile(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                StringCollection paths = new StringCollection();
                int num = 0;
                vieModel.SelectedMovie.ToList().ForEach(arg => { if (File.Exists(arg.filepath)) { paths.Add(arg.filepath); num++; } });
                if (paths.Count > 0)
                {
                    try
                    {
                        Clipboard.SetFileDropList(paths);
                        HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_Copied} {num}/{vieModel.SelectedMovie.Count}", "Main");
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
                else
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_FileNotExist, "Main");
                }



            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                int num = 0;
                vieModel.SelectedMovie.ToList().ForEach(arg =>
                {

                    if (arg.subsectionlist.Count > 0)
                    {
                        //分段视频
                        arg.subsectionlist.ForEach(path =>
                        {
                            if (File.Exists(path))
                            {
                                try
                                {
                                    FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                    num++;
                                }
                                catch (Exception ex) { Logger.LogF(ex); }
                            }
                        });
                    }
                    else
                    {
                        if (File.Exists(arg.filepath))
                        {
                            try
                            {
                                FileSystem.DeleteFile(arg.filepath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                num++;
                            }
                            catch (Exception ex) { Logger.LogF(ex); }

                        }
                    }




                });

                HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.Message_DeleteToRecycleBin} {num}/{vieModel.SelectedMovie.Count}", "Main");

                if (num > 0 && Properties.Settings.Default.DelInfoAfterDelFile)
                {
                    try
                    {
                        vieModel.SelectedMovie.ToList().ForEach(arg =>
                        {
                            DataBase.DeleteByField("movie", "id", arg.id);
                            vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                            vieModel.MovieList.Remove(arg);
                            vieModel.FilterMovieList.Remove(arg);
                        });

                        //从详情窗口删除
                        if (GetWindowByName("WindowDetails") != null)
                        {
                            WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                            foreach (var item in vieModel.SelectedMovie.ToList())
                            {
                                if (windowDetails.vieModel.DetailMovie.id == item.id)
                                {
                                    windowDetails.Close();
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    vieModel.Statistic();
                }


            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public WindowEdit WindowEdit;


        public void EditInfo(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode) { HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_NotEdit, "Main"); return; }
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, "Main"); return; }
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                string id = TB.Text;
                if (WindowEdit != null) { WindowEdit.Close(); }
                WindowEdit = new WindowEdit(id);
                WindowEdit.ShowDialog();
            }
        }

        public async void DeleteID(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, "Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                if (sp != null)
                {


                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                    if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToDelete).ShowDialog() == false) { return; }

                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {
                        DataBase.DeleteByField("movie", "id", arg.id);
                        vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                        vieModel.MovieList.Remove(arg);
                        vieModel.FilterMovieList.Remove(arg);
                    });

                    //从详情窗口删除
                    if (GetWindowByName("WindowDetails") != null)
                    {
                        WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                        foreach (var item in vieModel.SelectedMovie.ToList())
                        {
                            if (windowDetails.vieModel.DetailMovie.id == item.id)
                            {
                                windowDetails.Close();
                                break;
                            }
                        }
                    }

                    HandyControl.Controls.Growl.Info($"{Jvedio.Language.Resources.SuccessDelete} {vieModel.SelectedMovie.Count} ", "Main");
                    //修复数字显示
                    vieModel.CurrentCount -= vieModel.SelectedMovie.Count;
                    vieModel.TotalCount -= vieModel.SelectedMovie.Count;

                    vieModel.SelectedMovie.Clear();
                    vieModel.Statistic();

                    await Task.Run(() => { Task.Delay(1000).Wait(); });
                }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        public  void DeleteInfo(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_WaitForDownload, "Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                if (sp != null)
                {


                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                        if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToClearInfo).ShowDialog() == false) { return; }

                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {
                        DataBase.ClearInfoByID(arg.id);
                        RefreshMovieByID(arg.id);
                    });
                }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public Movie GetMovieFromVieModel(string id)
        {
            foreach (Movie movie in vieModel.CurrentMovieList)
            {
                if (movie.id == id)
                {
                    return movie;
                }
            }
            return null;
        }

        public Actress GetActressFromVieModel(string name)
        {
            foreach (Actress actress in vieModel.ActorList)
            {
                if (actress.name == name)
                {
                    return actress;
                }
            }
            return null;
        }

        public string GetFormatGenreString(List<Movie> movies, string type = "genre")
        {
            List<string> list = new List<string>();
            if (type == "genre")
            {
                movies.ForEach(arg =>
                {
                    foreach (var item in arg.genre.Split(' '))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }
            else if (type == "label")
            {
                movies.ForEach(arg =>
                {
                    foreach (var item in arg.label.Split(' '))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }
            else if (type == "actor")
            {

                movies.ForEach(arg =>
                {

                    foreach (var item in arg.actor.Split(actorSplitDict[arg.vediotype]))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }

            string result = "";
            list.ForEach(arg => { result += arg + " "; });
            return result;
        }


        //清空标签
        public void ClearLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                foreach (var movie in this.vieModel.MovieList)
                {
                    foreach (var item in vieModel.SelectedMovie)
                    {
                        if (item.id == movie.id)
                        {
                            DataBase.UpdateMovieByID(item.id, "label", "", "String");
                            break;
                        }
                    }
                }
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");

                vieModel.GetLabelList();

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

        }



        //删除单个影片标签
        public void DelSingleLabel(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode)
            {
                //HandyControl.Controls.Growl.Info("不支持批量", "Main");
                return;
            }
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                DetailMovie CurrentMovie = DataBase.SelectDetailMovieById(TB.Text);
                LabelDelGrid.Visibility = Visibility.Visible;
                vieModel.CurrentMovieLabelList = new List<string>();
                vieModel.CurrentMovieLabelList = CurrentMovie.label.Split(' ').ToList();
                CurrentLabelMovie = CurrentMovie;
                LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            }
        }


        //删除多个影片标签
        public void DelLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                //string TotalLabel = GetFormatGenreString(vieModel.SelectedMovie,"label");
                var di = new DialogInput(this, Jvedio.Language.Resources.InputTitle6, "");
                di.ShowDialog();
                if (di.DialogResult == true & di.Text != "")
                {
                    foreach (var movie in this.vieModel.MovieList)
                    {
                        foreach (var item in vieModel.SelectedMovie)
                        {
                            if (item.id == movie.id)
                            {
                                List<string> originlabel = LabelToList(movie.label);
                                List<string> newlabel = LabelToList(di.Text);
                                movie.label = string.Join(" ", originlabel.Except(newlabel).ToList());
                                DataBase.UpdateMovieByID(item.id, "label", movie.label, "String");
                                break;
                            }
                        }

                    }
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");
                    vieModel.GetLabelList();
                }

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        //增加标签
        public void AddLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                var di = new DialogInput(this, Jvedio.Language.Resources.InputTitle1, "");
                di.ShowDialog();
                if (di.DialogResult == true & di.Text != "")
                {
                    foreach (var movie in this.vieModel.MovieList)
                    {
                        foreach (var item in vieModel.SelectedMovie)
                        {
                            if (item.id == movie.id)
                            {
                                List<string> originlabel = LabelToList(movie.label);
                                List<string> newlabel = LabelToList(di.Text);
                                movie.label = string.Join(" ", originlabel.Union(newlabel).ToList());
                                originlabel.ForEach(arg => Console.WriteLine(arg));
                                newlabel.ForEach(arg => Console.WriteLine(arg));
                                originlabel.Union(newlabel).ToList().ForEach(arg => Console.WriteLine(arg));
                                DataBase.UpdateMovieByID(item.id, "label", movie.label, "String");
                                break;
                            }
                        }

                    }
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");

                    vieModel.GetLabelList();

                }

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }







        //设置喜爱
        public void SetFavorites(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            int favorites = int.Parse(mnu.Header.ToString());
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)((MenuItem)mnu.Parent).Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                foreach (var movie in this.vieModel.MovieList)
                {
                    foreach (var item in vieModel.SelectedMovie)
                    {
                        if (item.id == movie.id)
                        {
                            movie.favorites = favorites;
                            DataBase.UpdateMovieByID(item.id, "favorites", favorites);
                            break;
                        }
                    }
                }


            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

        }

        //打开网址
        private void OpenWeb(object sender, RoutedEventArgs e)
        {

            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            if (mnu != null)
            {
                StackPanel sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                vieModel.SelectedMovie.ToList().ForEach(arg =>
                {
                    if (!string.IsNullOrEmpty(arg.sourceurl) && arg.sourceurl.IndexOf("http") >= 0)
                    {
                        try
                        {
                            Process.Start(arg.sourceurl);
                        }
                        catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "Main"); }

                    }
                    else
                    {
                        //为空则使用 bus 打开
                        if (!string.IsNullOrEmpty(Properties.Settings.Default.Bus) && Properties.Settings.Default.Bus.IndexOf("http") >= 0)
                        {
                            try
                            {
                                Process.Start(Properties.Settings.Default.Bus + arg.id);
                            }
                            catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "Main"); }
                        }
                        else if (arg.id.StartsWith("FC2") && Properties.Settings.Default.FC2.IsProperUrl())
                        {
                            try
                            {
                                Process.Start($"{Properties.Settings.Default.FC2}article/{arg.id}/");
                            }
                            catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "Main"); }
                        }
                        else
                        {
                            HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_UrlNotSet, "Main");
                        }

                    }
                });
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }




        private void DownLoadSelectMovie(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, "Main");
            }
            else if (!Net.IsServersProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetUrl, "Main");
            }
            else
            {
                try
                {
                    if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                    MenuItem mnu = sender as MenuItem;
                    StackPanel sp = null;

                    if (mnu != null)
                    {
                        sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                        var TB = sp.Children.OfType<TextBox>().First();
                        Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                        if (CurrentMovie != null)
                        {
                            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                            StartDownload(vieModel.SelectedMovie.ToList());
                        }

                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        private void ForceToDownLoad(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, "Main");
            }
            else if (!Net.IsServersProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetUrl, "Main");
            }
            else
            {
                try
                {
                    if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                    MenuItem _mnu = sender as MenuItem;
                    MenuItem mnu = _mnu.Parent as MenuItem;
                    StackPanel sp = null;

                    if (mnu != null)
                    {
                        sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                        var TB = sp.Children.OfType<TextBox>().First();
                        Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                        if (CurrentMovie != null)
                        {
                            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                            StartDownload(vieModel.SelectedMovie.ToList(), true);
                        }

                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }
        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ImageSlides == null) return;
            Canvas canvas = (Canvas)sender;
            TextBlock textBlock = canvas.Children.OfType<TextBlock>().First();
            int index = int.Parse(textBlock.Text);
            if (index < ImageSlides.Count)
            {
                ImageSlides[index].PlaySlideShow();
                ImageSlides[index].Start();
            }

        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImageSlides == null) return;
            Canvas canvas = (Canvas)sender;
            TextBlock textBlock = canvas.Children.OfType<TextBlock>().First();
            int index = int.Parse(textBlock.Text);
            if (index < ImageSlides.Count)
            {
                ImageSlides[index].Stop();
            }

        }



        private void EditActress(object sender, MouseButtonEventArgs e)
        {
            vieModel.EnableEditActress = !vieModel.EnableEditActress;
            //Console.WriteLine(vieModel.Actress.age); 
        }

        private void SaveActress(object sender, KeyEventArgs e)
        {

            if (vieModel.EnableEditActress && e.Key == Key.Enter)
            {
                SearchBar.Focus();
                vieModel.EnableEditActress = false;
                //Console.WriteLine(vieModel.Actress.age);
                DataBase.InsertActress(vieModel.Actress);
            }


        }

        private void BeginDownLoadActress(object sender, MouseButtonEventArgs e)
        {
            List<Actress> actresses = new List<Actress>();
            actresses.Add(vieModel.Actress);
            DownLoadActress downLoadActress = new DownLoadActress(actresses);
            downLoadActress.BeginDownLoad();
            downLoadActress.InfoUpdate += (s, ev) =>
            {
                ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
                try
                {
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        vieModel.Actress = null;
                        vieModel.Actress = actressUpdateEventArgs.Actress;
                        downLoadActress.State = DownLoadState.Completed;
                    });
                }
                catch (TaskCanceledException ex) { Logger.LogE(ex); }

            };

            downLoadActress.MessageCallBack += (s, ev) =>
            {
                MessageCallBackEventArgs actressUpdateEventArgs = ev as MessageCallBackEventArgs;
                HandyControl.Controls.Growl.Info(actressUpdateEventArgs.Message, "Main");

            };


        }



        private void ProgressBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ProgressBar PB = sender as ProgressBar;
            if (PB.Value + PB.LargeChange <= PB.Maximum)
            {
                PB.Value += PB.LargeChange;
            }
            else
            {
                PB.Value = PB.Minimum;
            }
        }

        private void DelLabel(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            StackPanel stackPanel = border.Parent as StackPanel;

            Console.WriteLine(stackPanel.Parent.GetType().ToString());

        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                if (this.WindowState == WindowState.Normal) WinState = JvedioWindowState.Normal;
                else if (this.WindowState == WindowState.Maximized) WinState = JvedioWindowState.FullScreen;
                else if (this.Width == SystemParameters.WorkArea.Width & this.Height == SystemParameters.WorkArea.Height) WinState = JvedioWindowState.Maximized;

                WindowConfig cj = new WindowConfig(this.GetType().Name);
                cj.Save(new WindowProperty() { Location = new Point(this.Left, this.Top), Size = new Size(this.Width, this.Height), WinState = WinState });
            }
            Properties.Settings.Default.EditMode = false;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();

            if (!IsToUpdate && Properties.Settings.Default.CloseToTaskBar && this.IsVisible == true)
            {
                e.Cancel = true;
                NotifyIcon.Visibility = Visibility.Visible;
                this.Hide();
                WindowSet?.Hide();
            }


        }

        private void ActorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (e.Key == Key.Enter)
            {
                string pagestring = ((TextBox)sender).Text;
                int page = 1;
                if (pagestring == null) { page = 1; }
                else
                {
                    var isnumeric = int.TryParse(pagestring, out page);
                }
                if (page > vieModel.TotalActorPage) { page = vieModel.TotalActorPage; } else if (page <= 0) { page = 1; }
                vieModel.CurrentActorPage = page;
                vieModel.ActorFlipOver();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vieModel.TotalPage <= 1) return;
            if (e.Key == Key.Enter)
            {
                string pagestring = ((TextBox)sender).Text;
                int page = 1;
                if (pagestring == null) { page = 1; }
                else
                {
                    var isnumeric = int.TryParse(pagestring, out page);
                }
                if (page > vieModel.TotalPage) { page = vieModel.TotalPage; } else if (page <= 0) { page = 1; }
                vieModel.CurrentPage = page;
                vieModel.FlipOver();
            }
        }


        public void StopDownLoad()
        {
            if (DownLoader != null && DownLoader.State == DownLoadState.DownLoading) HandyControl.Controls.Growl.Warning(Jvedio.Language.Resources.Message_Stop, "Main");
            DownLoader?.CancelDownload();
            downLoadActress?.CancelDownload();
            this.Dispatcher.BeginInvoke((Action)delegate { ProgressBar.Visibility = Visibility.Hidden; });


        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }


        private void PreviousActorPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (vieModel.CurrentActorPage - 1 <= 0)
                vieModel.CurrentActorPage = vieModel.TotalActorPage;
            else
                vieModel.CurrentActorPage -= 1;
            vieModel.ActorFlipOver();


        }

        private  void NextActorPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (vieModel.CurrentActorPage + 1 > vieModel.TotalActorPage)
                vieModel.CurrentActorPage = 1;
            else
                vieModel.CurrentActorPage += 1;
             vieModel.ActorFlipOver();
        }



        public T FindVisualChildOrContentByType<T>(DependencyObject parent)
       where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.GetType() == typeof(T))
            {
                return parent as T;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child.GetType() == typeof(T))
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildOrContentByType<T>(child);
                    if (result != null)
                        return result;
                }
            }

            if (parent is ContentControl contentControl)
            {
                return this.FindVisualChildOrContentByType<T>(contentControl.Content as DependencyObject);
            }

            return null;

        }



        //TODO


        private async Task<bool> WaitUntilStopLoad()
        {
            return await Task.Run(() => {
                while (vieModel.IsFlipOvering)
                {
                    Task.Delay(100).Wait();
                }
                return false;
            });

        }

        private  void NextPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalPage <= 1) return;
            if (vieModel.CurrentPage + 1 > vieModel.TotalPage) vieModel.CurrentPage = 1;
            else vieModel.CurrentPage += 1;

            vieModel.FlipOver();
            ScrollViewer.ScrollToTop();

        }

        private  void PreviousPage(object sender, MouseButtonEventArgs e)
        {

            if (vieModel.TotalPage <= 1) return;
            if (vieModel.CurrentPage - 1 <= 0) vieModel.CurrentPage = vieModel.TotalPage;
            else vieModel.CurrentPage -= 1;

            vieModel.FlipOver();
            ScrollViewer.ScrollToTop();

        }




        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
        }


        private void ActorGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.ActorEditMode)
            {
                foreach (var item in vieModel.ActorList)
                {
                    if (!SelectedActress.Contains(item))
                    {
                        SelectedActress.Add(item);

                    }
                }
                ActorSetSelected();
            }
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.EditMode)
            {
                foreach (var item in vieModel.CurrentMovieList)
                {
                    if (!vieModel.SelectedMovie.Contains(item))
                    {
                        vieModel.SelectedMovie.Add(item);
                    }
                }
                SetSelected();
            }
        }

        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }

        public void StopDownLoadActress(object sender, RoutedEventArgs e)
        {
            DownloadActorPopup.IsOpen = false;
            downLoadActress?.CancelDownload();
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Stop, "Main");
        }

        public void DownLoadSelectedActor(object sender, RoutedEventArgs e)
        {
            if (downLoadActress?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, "Main"); return;
            }

            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            StackPanel sp = null;
            if (sender is MenuItem mnu)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                string name = TB.Text.Split('(')[0];
                Actress CurrentActress = GetActressFromVieModel(name);
                if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
                StartDownLoadActor(SelectedActress);

            }
            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
        }

        public void LikeSelectedActor(object sender, RoutedEventArgs e)
        {

            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            StackPanel sp = null;
            if (sender is MenuItem mnu)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                string name = TB.Text.Split('(')[0];
                Actress CurrentActress = GetActressFromVieModel(name);
                if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
                DataBase.CreateTable(DataBase.SQLITETABLE_ACTRESS_LOVE);
                foreach (Actress actress in SelectedActress)
                {
                    DataBase.SaveActressLikeByName(actress.name, 1);
                }

            }
            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();

        }

        public void SelectAllActor(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ActorEditMode) { ActorCancelSelect(); return; }
            Properties.Settings.Default.ActorEditMode = true;
            foreach (var item in vieModel.CurrentActorList)
                if (!SelectedActress.Contains(item)) SelectedActress.Add(item);

            ActorSetSelected();
        }

        public void ActorCancelSelect()
        {
            Properties.Settings.Default.ActorEditMode = false; SelectedActress.Clear(); ActorSetSelected();
        }

        public void RefreshCurrentActressPage(object sender, RoutedEventArgs e)
        {
            ActorCancelSelect();
            vieModel.RefreshActor();
        }

        public void StartDownLoadActor(List<Actress> actresses)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "BusActress.sqlite")) return;

            downLoadActress = new DownLoadActress(actresses);
            downLoadActress?.BeginDownLoad();
            try
            {

                //进度提示
                downLoadActress.InfoUpdate += (s, ev) =>
                {
                    ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actressUpdateEventArgs.Actress.name)
                        {
                            try
                            {
                                Dispatcher.Invoke((Action)delegate ()
                                {
                                    vieModel.ActorList[i] = actressUpdateEventArgs.Actress;
                                    ActorProgressBar.Value = actressUpdateEventArgs.progressBarUpdate.value / actressUpdateEventArgs.progressBarUpdate.maximum * 100; ActorProgressBar.Visibility = Visibility.Visible;
                                    if (ActorProgressBar.Value == ActorProgressBar.Maximum) downLoadActress.State = DownLoadState.Completed;
                                    if (ActorProgressBar.Value == ProgressBar.Maximum | actressUpdateEventArgs.state == DownLoadState.Fail | actressUpdateEventArgs.state == DownLoadState.Completed) { ActorProgressBar.Visibility = Visibility.Hidden; }
                                });
                            }
                            catch (TaskCanceledException ex) { Logger.LogE(ex); }
                            break;
                        }
                    }
                };
            }
            catch (Exception e) { Console.WriteLine(e.Message); }



        }


        DownLoadActress downLoadActress;
        public void StartDownLoadActress(object sender, RoutedEventArgs e)
        {
            DownloadActorPopup.IsOpen = false;
            if (!EnableUrl.Bus || !RootUrl.Bus.IsProperUrl())
            {
                HandyControl.Controls.Growl.Info($"BUS {Jvedio.Language.Resources.Message_NotOpenOrNotEnable}", "Main");
                return;
            }

            if (DownLoader?.State == DownLoadState.DownLoading)
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, "Main");
            else
                StartDownLoadActor(vieModel.CurrentActorList.ToList());



        }




        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ProgressBar.Visibility == Visibility.Hidden && Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance != null)
            {
                    taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress,this);
            }


        }

        private void ShowSameFilePath(object sender, RoutedEventArgs e)
        {
            FilePathPopup.IsOpen = true;
            vieModel.LoadFilePathClassfication();
        }

        public void ShowSubsection(object sender, MouseButtonEventArgs e)
        {


            Image image = sender as Image;
            var grid = image.Parent as Grid;
            Popup popup = grid.Children.OfType<Popup>().First();
            popup.IsOpen = true;

        }


        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            WaitingPanel.Visibility = Visibility.Visible;
            await Task.Run(() =>
            {
                //分为文件夹和文件
                string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                List<string> files = new List<string>();
                StringCollection stringCollection = new StringCollection();
                foreach (var item in dragdropFiles)
                {
                    if (IsFile(item))
                        files.Add(item);
                    else
                        stringCollection.Add(item);
                }
                List<string> filepaths = new List<string>();
                //扫描导入
                if (stringCollection.Count > 0)
                    filepaths = Scan.ScanPaths(stringCollection, new CancellationToken());

                if (files.Count > 0) filepaths.AddRange(files);

                Scan.InsertWithNfo(filepaths, new CancellationToken(), (message) => { HandyControl.Controls.Growl.Info(message, "Main"); });
                Task.Delay(300).Wait();
            });
            WaitingPanel.Visibility = Visibility.Hidden;
            vieModel.Reset();
        }



        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void Button_StopDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;
            StopDownLoad();

        }

        private void Button_StartDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;

            if (!Net.IsServersProper())
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_SetUrl, "Main");

            }
            else
            {
                if (DownLoader?.State == DownLoadState.DownLoading)
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_WaitForDownload, "Main");
                else
                    StartDownload(vieModel.CurrentMovieList.ToList());
            }


        }



        private async void OpenUpdate(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, Jvedio.Language.Resources.IsToUpdate).ShowDialog() == true)
            {
                try
                {
                    //检查升级程序是否是最新的
                    string content = ""; int statusCode; bool IsToDownLoadUpdate = false;
                    try { (content, statusCode) = await Net.Http(UpdateExeVersionUrl, Proxy: null); }
                    catch (TimeoutException ex) { Logger.LogN($"URL={UpdateUrl},Message-{ex.Message}"); }
                    if (content != "")
                    {
                        //跟本地的 md5 对比
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe")) { IsToDownLoadUpdate = true; }
                        else
                        {
                            string md5 = GetFileMD5(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe");
                            if (md5 != content) { IsToDownLoadUpdate = true; }
                        }
                    }
                    if (IsToDownLoadUpdate)
                    {
                        (byte[] filebyte, string cookie, int statuscode) = Net.DownLoadFile(UpdateExeUrl);
                        try
                        {
                            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe", FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(filebyte, 0, filebyte.Length);
                            }
                        }
                        catch { }
                    }
                    try
                    {
                        Process.Start(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe");
                    }catch(Exception ex)
                    {
                        HandyControl.Controls.Growl.Error(ex.Message, "Main");
                    }

                    IsToUpdate = true;
                    Application.Current.Shutdown();//直接关闭
                }
                catch { MessageBox.Show($"{Jvedio.Language.Resources.CannotOpen} JvedioUpdate.exe"); }

            }
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            Border border = sender as Border;
            border.Opacity = 1;
        }



        public void ShowSettingsPopup(object sender, MouseButtonEventArgs e)
        {
            if (SettingsContextMenu.IsOpen)
                SettingsContextMenu.IsOpen = false;
            else
            {
                SettingsContextMenu.IsOpen = true;
                SettingsContextMenu.PlacementTarget = SettingsBorder;
                SettingsContextMenu.Placement = PlacementMode.Bottom;
            }

        }


        private void ClearRecentWatched(object sender, RoutedEventArgs e)
        {
            if (new RecentWatchedConfig("").Clear())
            {
                ReadRecentWatchedFromConfig();
                vieModel.AddToRecentWatch("");
            }
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.FirstRun)
            {
                BeginScanGrid.Visibility = Visibility.Visible;
                Properties.Settings.Default.FirstRun = false;
                Properties.Settings.Default.Save();
            }


            if (Properties.Settings.Default.Opacity_Main >= 0.5)
                this.Opacity = Properties.Settings.Default.Opacity_Main;
            else
                this.Opacity = 1;


            SetSkin();

            //监听文件改动
            //if (Properties.Settings.Default.ListenAllDir)
            //{
            //    try { AddListen(); }
            //    catch (Exception ex) { Logger.LogE(ex); }
            //}

            //显示公告
            ShowNotice();


            //检查更新
            //if (Properties.Settings.Default.AutoCheckUpdate) CheckUpdate();
            CheckUpdate();


            //检查网络连接


            this.Cursor = Cursors.Arrow;

            //ScrollViewer.Focus();


            //设置当前数据库
            for (int i = 0; i < vieModel.DataBases.Count; i++)
            {
                if (vieModel.DataBases[i].ToLower() == System.IO.Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel.DataBases.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;
            CheckurlTimer.Start();
            BeginCheckurlThread();
            ReadRecentWatchedFromConfig();//显示最近播放
            vieModel.AddToRecentWatch("");
            vieModel.GetFilterInfo();

            //设置排序类型
            var radioButtons = SortStackPanel.Children.OfType<RadioButton>().ToList();
            int.TryParse(Properties.Settings.Default.SortType, out int idx);
            radioButtons[idx].IsChecked = true;

            //设置图片类型
            var rbs = ImageTypeStackPanel.Children.OfType<RadioButton>().ToList();
            int.TryParse(Properties.Settings.Default.ShowImageMode, out int idx2);
            rbs[idx2].IsChecked = true;


            if (vieModel.SortDescending)
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_down.png", UriKind.Relative));
            else
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_up.png", UriKind.Relative));

            InitList();



        }

        public void InitList()
        {
            MySqlite dB = new MySqlite("mylist");
            List<string> tables = dB.GetAllTable();
            vieModel.MyList = new ObservableCollection<MyListItem>();
            foreach (string table in tables)
            {
                vieModel.MyList.Add(new MyListItem(table, (long)dB.SelectCountByTable(table)));
            }
            dB.Close();


        }


        public void SetSkin()
        {
            if (Properties.Settings.Default.Themes == "黑色")
            {
                Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22252A"));
                Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B1B1F"));
                Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101013"));
                Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));
                Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#18191B"));
                Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AFAFAF"));
                Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                SideBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString()));
                TopBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundTitle"].ToString()));
            }
            else if (Properties.Settings.Default.Themes == "白色")
            {
                Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E3E5"));
                Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9"));
                Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2F3F4"));
                Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF5EE"));
                Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D1D1"));
                Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
                Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));

                SideBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString()));
                TopBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundTitle"].ToString()));
            }
            else if (Properties.Settings.Default.Themes == "蓝色")

            {
                Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B72BD"));
                Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2BA2D2"));
                Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#61AEDA"));
                Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3DBEDE"));
                Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
                Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
                Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95DCED"));


                //设置侧边栏渐变

                LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush
                {
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1)
                };
                myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 0));
                myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 1));
                SideBorder.Background = myLinearGradientBrush;

                LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush
                {
                    MappingMode = BrushMappingMode.RelativeToBoundingBox,
                    StartPoint = new Point(0, 0.5),
                    EndPoint = new Point(1, 0)
                };
                myLinearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 1));
                myLinearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 0));
                TopBorder.Background = myLinearGradientBrush2;

            }


            //设置背景
            BackGroundImage.Source = GlobalVariable.BackgroundImage;

        }

        private void SetSkinProperty(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = ((Button)sender).Content.ToString();
            Properties.Settings.Default.Save();
            SetSkin();
            SetSelected();
            ActorSetSelected();
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedMovie.Clear();
            SetSelected();
        }



        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            BeginScanGrid.Visibility = Visibility.Hidden;
            OpenTools(sender, e);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
            {
                //高级检索
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right)
            {
                //末页
                if (Grid_Classify.Visibility == Visibility.Hidden)
                {
                    vieModel.CurrentPage = vieModel.TotalPage;
                    vieModel.FlipOver();
                    SetSelected();
                }
                else
                {
                    vieModel.CurrentActorPage = vieModel.TotalActorPage;
                    vieModel.ActorFlipOver();
                    ActorSetSelected();
                }

            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left)
            {
                //首页
                if (Grid_Classify.Visibility == Visibility.Hidden)
                {
                    vieModel.CurrentPage = 1;
                    vieModel.FlipOver();
                    SetSelected();
                }

                else
                {
                    vieModel.CurrentActorPage = 1;
                    vieModel.ActorFlipOver();
                    ActorSetSelected();
                }

            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Up)
            {
                //回到顶部
                //ScrollViewer.ScrollToTop();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Down)
            {
                //滑倒底端

            }
            else if (Grid_Classify.Visibility == Visibility.Hidden && e.Key == Key.Right)
                NextPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (Grid_Classify.Visibility == Visibility.Hidden && e.Key == Key.Left)
                PreviousPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (Grid_Classify.Visibility == Visibility.Visible && e.Key == Key.Right)
                NextActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (Grid_Classify.Visibility == Visibility.Visible && e.Key == Key.Left)
                PreviousActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));




        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //AllSearchTextBox.Focus();
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            string name = e.AddedItems[0].ToString().ToLower();
            if (name != System.IO.Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath).ToLower())
            {
                if (name == "info")
                    Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"{name}.sqlite";
                else
                    Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite";
                //切换数据库
                vieModel.IsRefresh = true;
                vieModel.Reset();
                AllRB.IsChecked = true;
                vieModel.GetFilterInfo();

            }
        }

        private void GotoDownloadUrl(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/hitchao/Jvedio/releases");
        }

        private void RandomDisplay(object sender, MouseButtonEventArgs e)
        {
            vieModel.RandomDisplay();
        }

        private async void ShowFilterGrid(object sender, MouseButtonEventArgs e)
        {
            if (FilterGrid.Visibility == Visibility.Visible)
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(600, 0, new Duration(TimeSpan.FromMilliseconds(300)));
                FilterGrid.BeginAnimation(FrameworkElement.MaxHeightProperty, doubleAnimation1);
                await Task.Delay(300);
                FilterGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                FilterGrid.Visibility = Visibility.Visible;
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(0, 600, new Duration(TimeSpan.FromMilliseconds(300)));
                FilterGrid.BeginAnimation(FrameworkElement.MaxHeightProperty, doubleAnimation1);
                await Task.Delay(300);

            }


        }



        private bool IsDragingSideGrid = false;

        private void DragRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if(sender.GetType().Name== "Border") AllSearchPopup.IsOpen = false;
            if (SideBorder.Width >= 200) { if (sender is Rectangle rectangle) rectangle.Cursor = Cursors.SizeWE; }
            if (IsDragingSideGrid)
            {
                this.Cursor = Cursors.SizeWE;
                double width = e.GetPosition(this).X;
                if (width > 500 || width < 200)
                    return;
                else
                {
                    SideBorder.Width = width;
                }

            }
        }

        private void DragRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && SideBorder.Width >= 200)
            {
                IsDragingSideGrid = true;
            }
        }

        private void DragRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsDragingSideGrid = false;
            Properties.Settings.Default.SideGridWidth = SideBorder.Width;
            Properties.Settings.Default.Save();
        }


        private void CheckMode_Click(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedMovie.Clear();
            SetSelected();
        }


        private void RenameChildTree(object sender, MouseButtonEventArgs e)
        {

        }

        private void RenameType(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            RadioButton radioButton = contextMenu.PlacementTarget as RadioButton;
            TextBox textBox = radioButton.Content as TextBox;
            textBox.Focusable = true;
            textBox.IsReadOnly = false;
            textBox.Focus();
            textBox.SelectAll();

        }




        private void TopBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                MaxWindow(sender, new RoutedEventArgs());
            }
        }

        public void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteInfo);
                if (menuItem != null) DeleteID(menuItem, new RoutedEventArgs());
            }
            else if (e.Key == Key.T)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_DeleteFile);
                if (menuItem != null) DeleteFile(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.S)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_SyncInfo);
                if (menuItem != null) DownLoadSelectMovie(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.E)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_EditInfo);
                if (menuItem != null) EditInfo(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.W)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_OpenWebSite);
                if (menuItem != null) OpenWeb(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.C)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, Jvedio.Language.Resources.Menu_CopyFile);
                if (menuItem != null) CopyFile(menuItem, new RoutedEventArgs());

            }
            contextMenu.IsOpen = false;
        }


        private MenuItem GetMenuItem(ContextMenu contextMenu, string header)
        {
            foreach (MenuItem item in contextMenu.Items)
            {
                if (item.Header.ToString() == header)
                {
                    return item;
                }
            }
            return null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && e.Key == Key.S)
            {
                //MessageBox.Show("1");
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.S)
            {
                //MessageBox.Show("2");
            }
        }

        private void cmdTextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cmdGrid.Visibility = Visibility.Collapsed;
        }


        private void ShowSearchPopup(object sender, MouseButtonEventArgs e)
        {
            AllSearchPopup.IsOpen = true;
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Background = new SolidColorBrush(Colors.Transparent);
        }

        private void ShowSamePath(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            vieModel.GetSamePathMovie(textBlock.Text);
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        private void NotifyIcon_Click(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
            this.Show();
            this.Opacity = 1;
            this.WindowState = WindowState.Normal;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Properties.Settings.Default.ShowImageMode == "0")
            {
                Properties.Settings.Default.SmallImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.SmallImage_Height = (int)((double)Properties.Settings.Default.SmallImage_Width * (200 / 147));

            }
            else if (Properties.Settings.Default.ShowImageMode == "1")
            {
                Properties.Settings.Default.BigImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.BigImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
            else if (Properties.Settings.Default.ShowImageMode == "2")
            {
                Properties.Settings.Default.ExtraImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.ExtraImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }

            //Console.WriteLine(Properties.Settings.Default.BigImage_Height);
            //Console.WriteLine(Properties.Settings.Default.BigImage_Width);
            Properties.Settings.Default.Save();
        }


        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (!CanRateChange) return;
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            StackPanel stackPanel = rate.Parent as StackPanel;
            StackPanel sp = stackPanel.Parent as StackPanel;
            TextBox textBox = sp.Children.OfType<TextBox>().First();

            if (vieModel.CurrentMovieList != null && vieModel.CurrentMovieList.Count > 0)
            {
                foreach (var item in vieModel.CurrentMovieList)
                {
                    if (item != null)
                    {
                        if (item.id.ToUpper() == textBox.Text.ToUpper())
                        {
                            string table = GetCurrentList();
                            if (!string.IsNullOrEmpty(table))
                            {
                                using (MySqlite mySqlite = new MySqlite("mylist"))
                                {
                                    mySqlite.ExecuteSql($"update {table} set favorites={ item.favorites} where id='{item.id}'");
                                }
                            }
                            else
                            {
                                DataBase.UpdateMovieByID(item.id, "favorites", item.favorites, "string");
                            }
                            vieModel.Statistic();
                            break;
                        }
                    }


                }

            }
            CanRateChange = false;



        }

        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            vieModel.GetLabelList();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                if (sp != null)
                {

                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);


                }
            }


            LabelGrid.Visibility = Visibility.Visible;
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    toggleButton.IsChecked = false;
                }
            }


        }


        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            HandyControl.Controls.Tag tag = contextMenu.PlacementTarget as HandyControl.Controls.Tag;
            if (tag.IsSelected) tag.IsSelected = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {



            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        Match match = Regex.Match(toggleButton.Content.ToString(), @"\( \d+ \)");
                        if (match != null && match.Value != "")
                        {
                            string label = toggleButton.Content.ToString().Replace(match.Value, "");
                            if (!originLabels.Contains(label)) originLabels.Add(label);
                        }

                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                //HandyControl.Controls.Growl.Warning("请选择标签！", "Main");
                return;
            }


            foreach (Movie movie in vieModel.SelectedMovie)
            {
                List<string> labels = LabelToList(movie.label);
                labels = labels.Union(originLabels).ToList();
                movie.label = string.Join(" ", labels);
                DataBase.UpdateMovieByID(movie.id, "label", movie.label, "String");
            }
            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            LabelGrid.Visibility = Visibility.Hidden;

        }



        private void AddNewLabel(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.CurrentMovieLabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelDelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelDelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        string label = toggleButton.Content.ToString();
                        if (!originLabels.Contains(label)) originLabels.Add(label);
                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                //HandyControl.Controls.Growl.Warning("请选择标签！", "Main");
                return;
            }

            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Except(originLabels).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }

            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            if (vieModel.CurrentMovieList.Count == 0)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");
                LabelDelGrid.Visibility = Visibility.Hidden;
                vieModel.GetLabelList();
            }



        }


        private void ClearSingleLabel(object sender, RoutedEventArgs e)
        {
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", "", "String");
            vieModel.CurrentMovieLabelList = new List<string>();
            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;
        }

        private void AddSingleLabel(object sender, RoutedEventArgs e)
        {
            List<string> newLabel = new List<string>();

            var di = new DialogInput(this, Jvedio.Language.Resources.InputTitle1, "");
            di.ShowDialog();
            if (di.DialogResult == true & di.Text != "")
            {
                foreach (var item in di.Text.Split(' ').ToList())
                {
                    if (!newLabel.Contains(item)) newLabel.Add(item);
                }

            }
            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Union(newLabel).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }
            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;
        }

        private void DeleteSingleLabel(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.CurrentMovieLabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelDelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelDelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        string label = toggleButton.Content.ToString();
                        if (!originLabels.Contains(label)) originLabels.Add(label);
                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                //HandyControl.Controls.Growl.Warning("请选择标签！", "Main");
                return;
            }

            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Except(originLabels).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }

            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            if (vieModel.CurrentMovieList.Count == 0)
            {
                HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");
                LabelDelGrid.Visibility = Visibility.Hidden;
                vieModel.GetLabelList();
            }



        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            Grid grid = (Grid)stackPanel.Parent;
            ((Grid)grid.Parent).Visibility = Visibility.Hidden;
        }


        WindowBatch WindowBatch;

        private void OpenBatching(object sender, RoutedEventArgs e)
        {
            if (WindowBatch != null) WindowBatch.Close(); 
            WindowBatch = new WindowBatch();
            WindowBatch.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            vieModel.ClearCurrentMovieList();
        }


        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(123);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Width == SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height)
            {
                MainGrid.Margin = new Thickness(0);
                MainBorder.Margin = new Thickness(0);
                Grid.Margin = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                MainGrid.Margin = new Thickness(0);
                MainBorder.Margin = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                MainGrid.Margin = new Thickness(10);
                MainBorder.Margin = new Thickness(5);
                Grid.Margin = new Thickness(5);
                this.ResizeMode = ResizeMode.CanResize;
            }




        }



        private async void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;

            if (mnu == null) return;
            sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            var TB = sp.Children.OfType<TextBox>().First();
            string id = TB.Text;

            DialogInput dialogInput = new DialogInput(this, Jvedio.Language.Resources.InputTitle2);
            if (dialogInput.ShowDialog() == true)
            {
                string url = dialogInput.Text;
                if (!url.StartsWith("http"))
                {
                    HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_WrongUrl, "Main");
                }
                else
                {

                    string host = new Uri(url).Host;
                    WebSite webSite = await Net.CheckUrlType(url.Split(':')[0] + "://" + host);
                    if (webSite == WebSite.None)
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_NotRecognize, "Main");
                    }
                    else
                    {
                        if (webSite == WebSite.DMM || webSite == WebSite.Jav321)
                        {
                            HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.NotSupport, "Main");
                        }
                        else
                        {
                            HandyControl.Controls.Growl.Info($"{webSite} {Jvedio.Language.Resources.Message_BeginParse}", "Main");
                            bool result = await Net.ParseSpecifiedInfo(webSite, id, url);
                            if (result)
                            {
                                HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.Message_BeginDownloadImage, "Main");
                                //更新到主界面
                                RefreshMovieByID(id);

                                //下载图片
                                DetailMovie dm = DataBase.SelectDetailMovieById(id);
                                //下载小图
                                await Net.DownLoadSmallPic(dm, true);
                                dm.smallimage = ImageProcess.GetBitmapImage(dm.id, "SmallPic");
                                RefreshMovieByID(id);


                                if (dm.sourceurl?.IndexOf("fc2club") >= 0)
                                {
                                    //复制大图
                                    if (File.Exists(GlobalVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg") & !File.Exists(GlobalVariable.BasePicPath + $"BigPic\\{dm.id}.jpg"))
                                    {
                                        File.Copy(GlobalVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg", GlobalVariable.BasePicPath + $"BigPic\\{dm.id}.jpg");
                                    }
                                }
                                else
                                {
                                    //下载大图
                                    await Net.DownLoadBigPic(dm, true);
                                }
                                dm.bigimage = ImageProcess.GetBitmapImage(dm.id, "BigPic");
                                RefreshMovieByID(id);


                            }
                            else
                            {
                                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_Fail, "Main");
                            }
                        }
                    }
                }


            }
        }

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            if (IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    File.Copy(fileInfo.FullName, BasePicPath + $"Actresses\\{vieModel.Actress.name}.jpg", true);
                    Actress actress = vieModel.Actress;
                    actress.smallimage = null;
                    actress.smallimage = GetActorImage(actress.name);
                    vieModel.Actress = null;
                    vieModel.Actress = actress;

                    if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actress.name)
                        {
                            vieModel.ActorList[i] = actress;
                            break;
                        }
                    }

                }
                else
                {
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_OnlySupportJPG, "DetailsGrowl");
                }
            }
        }

        private void ActorImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            string name = textBox.Text.Split('(')[0];

            Actress currentActress = null;
            for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            {
                if (vieModel.CurrentActorList[i].name == name)
                {
                    currentActress = vieModel.CurrentActorList[i];
                    break;
                }
            }

            if (currentActress == null) return;


            if (IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    File.Copy(fileInfo.FullName, BasePicPath + $"Actresses\\{currentActress.name}.jpg", true);
                    Actress actress = currentActress;
                    actress.smallimage = null;
                    actress.smallimage = GetActorImage(actress.name);

                    if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actress.name)
                        {
                            vieModel.ActorList[i] = null;
                            vieModel.ActorList[i] = actress;
                            break;
                        }
                    }

                    for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
                    {
                        if (vieModel.CurrentActorList[i].name == actress.name)
                        {
                            vieModel.CurrentActorList[i] = null;
                            vieModel.CurrentActorList[i] = actress;
                            break;
                        }
                    }

                }
                else
                {
                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_OnlySupportJPG, "DetailsGrowl");
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var WrapPanels = FilterStackPanel.Children.OfType<WrapPanel>().ToList(); ;

            List<int> vediotype = new List<int>();
            WrapPanel wrapPanel = WrapPanels[0];
            var tbs = wrapPanel.Children.OfType<ToggleButton>().ToList();

            for (int i = 0; i < tbs.Count; i++)
            {
                ToggleButton tb = tbs[i] as ToggleButton;
                if((bool)tb.IsChecked)
                {
                    vediotype.Add(i+1);
                    break;
                }
            }


            //年份
            wrapPanel = WrapPanels[1];
            ItemsControl itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> year = GetFilterFromItemsControl(itemsControl);



            //时长
            wrapPanel = WrapPanels[2];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> runtime = GetFilterFromItemsControl(itemsControl);

            //文件大小
            wrapPanel = WrapPanels[3];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> filesize = GetFilterFromItemsControl(itemsControl);

            //评分
            wrapPanel = WrapPanels[4];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> rating = GetFilterFromItemsControl(itemsControl);


            //类别
            List<string> genre = GetFilterFromItemsControl(GenreItemsControl);

            //演员
            List<string> actor = GetFilterFromItemsControl(ActorFilterItemsControl);

            //标签
            List<string> label = GetFilterFromItemsControl(LabelFilterItemsControl);

            string sql = "select * from movie where ";

            string s = "";
            vediotype.ForEach(arg => { s += $"vediotype={arg} or "; });
            if (vediotype.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s == "" | vediotype.Count == 3) s = "vediotype>0";
            sql += "(" + s + ") and "; s = "";

            year.ForEach(arg => { s += $"releasedate like '%{arg}%' or "; });
            if (year.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //类别
            genre.ForEach(arg => { s += $"genre like '%{arg}%' or "; });
            if (genre.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //演员
            actor.ForEach(arg => { s += $"actor like '%{arg}%' or "; });
            if (actor.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //类别
            label.ForEach(arg => { s += $"label like '%{arg}%' or "; });
            if (label.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";


            if (runtime.Count > 0 & rating.Count < 4)
            {
                runtime.ForEach(arg => { s += $"(runtime >={arg.Split('-')[0]} and runtime<={arg.Split('-')[1]}) or "; });
                if (runtime.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }

            if (filesize.Count > 0 & rating.Count < 4)
            {
                filesize.ForEach(arg => { s += $"(filesize >={double.Parse(arg.Split('-')[0]) * 1024 * 1024 * 1024} and filesize<={double.Parse(arg.Split('-')[1]) * 1024 * 1024 * 1024}) or "; });
                if (filesize.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }

            if (rating.Count > 0 & rating.Count < 5)
            {
                rating.ForEach(arg => { s += $"(rating >={arg.Split('-')[0]} and rating<={arg.Split('-')[1]}) or "; });
                if (rating.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }


            sql = sql.Substring(0, sql.Length - 5);
            Console.WriteLine(sql);
            vieModel.ExecutiveSqlCommand(0, "筛选", sql);
        }

        private List<string> GetFilterFromItemsControl(ItemsControl itemsControl)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {

                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null)
                    if ((bool)tb.IsChecked) result.Add(tb.Content.ToString());
            }
            return result;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var WrapPanels = FilterStackPanel.Children.OfType<WrapPanel>().ToList(); ;

            List<int> vediotype = new List<int>();
            WrapPanel wrapPanel = WrapPanels[0];
            foreach (var item in wrapPanel.Children.OfType<ToggleButton>())
            {
                if (item.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = item as ToggleButton;
                    if (tb != null) tb.IsChecked = false;
                }
            }
            for (int j = 1; j < WrapPanels.Count; j++)
            {
                ItemsControl itemsControl = WrapPanels[j].Children[1] as ItemsControl;
                if (itemsControl == null) continue;
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {

                    ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                    ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                    if (tb != null) tb.IsChecked = false;


                }
            }

            for (int i = 0; i < GenreItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)GenreItemsControl.ItemContainerGenerator.ContainerFromItem(GenreItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }
            for (int i = 0; i < ActorFilterItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ActorFilterItemsControl.ItemContainerGenerator.ContainerFromItem(ActorFilterItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }


        }

        public ObservableCollection<string> tempList;
        private void Genre_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vieModel.IsRefresh = false;
            foreach (var item in vieModel.GetAllGenre())
            {
                if (vieModel.IsRefresh) break;
                if (!vieModel.Genre.Contains(item))
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadGenreItem), item);
            }

        }

        private delegate void LoadItemDelegate(string content);

        private void LoadGenreItem(string content)
        {
            if (!vieModel.IsRefresh) vieModel.Genre.Add(content);
        }

        private void LoadActorItem(string content)
        {
            if (!vieModel.IsRefresh) vieModel.Actor.Add(content);
        }


        private void Actor_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vieModel.IsRefresh = false;
            foreach (var item in vieModel.GetAllActor())
            {
                if (vieModel.IsRefresh) break;
                if (!vieModel.Actor.Contains(item))
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadActorItem), item);
            }
        }










        private void ShowSameList(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            string listName = radioButton.Content.ToString();
            vieModel.ExecutiveSqlCommand(10, listName, $"select * from {listName}", "mylist");




        }


        private void EditListItem(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            RadioButton radioButton = contextMenu.PlacementTarget as RadioButton;

            string oldName = radioButton.Content.ToString();


            var r = new DialogInput(this, Jvedio.Language.Resources.InputTitle3, oldName);
            if (r.ShowDialog() == true)
            {
                string text = r.Text;
                if (text != "" & text != "+" & text.IndexOf(" ") < 0)
                {
                    if (vieModel.MyList.Where(arg => arg.Name == text).Count() > 0)
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_AlreadyExist);
                        return;
                    }
                    //重命名
                    if (Rename(oldName, text))
                    {
                        radioButton.Content = text;
                        for (int i = 0; i < vieModel.MyList.Count; i++)
                        {
                            if (vieModel.MyList[i].Name == oldName)
                            {
                                vieModel.MyList[i].Name = text;
                            }
                        }
                    }

                }

            }
        }

        private bool Rename(string oldName, string newName)
        {
            MySqlite dB = new MySqlite("mylist");
            if (!dB.IsTableExist(oldName))
            {
                dB.CloseDB();
                return false;
            }
            try
            {
                dB.CreateTable($"ALTER TABLE {oldName} RENAME TO {newName}");
            }
            catch
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotSupport);
                return false;
            }
            finally
            {
                dB.CloseDB();
            }
            return true;
        }

        public RadioButton ListRadibutton;

        private void RemoveListItem(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            ListRadibutton = contextMenu.PlacementTarget as RadioButton;

            if (ListRadibutton == null) return;
            if (new Msgbox(this, Jvedio.Language.Resources.IsToRemove).ShowDialog() == true)
            {

                string listName = ListRadibutton.Content.ToString();
                MyListItem myListItem = vieModel.MyList.Where(arg => arg.Name == listName).FirstOrDefault();
                vieModel.MyList.Remove(myListItem);
                ReMoveFromMyList(listName);
                vieModel.CurrentMovieList.Clear();
            }

        }


        private void AddListItem(object sender, MouseButtonEventArgs e)
        {
            var r = new DialogInput(this, Jvedio.Language.Resources.InputTitle4);
            if (r.ShowDialog() == true)
            {
                string text = r.Text;
                if (text != "" & text != "+" & text.IndexOf(" ") < 0)
                {
                    if (vieModel.MyList.Where(arg => arg.Name == text).Count() > 0)
                    {
                        HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.Message_AlreadyExist);
                        return;
                    }
                    if (AddToMyList(text)) vieModel.MyList.Add(new MyListItem(text, 0));

                }

            }
        }

        private bool AddToMyList(string name)
        {
            MySqlite dB = new MySqlite("mylist");
            if (dB.IsTableExist(name))
            {
                dB.CloseDB();
                return false;
            }
            name = DataBase.SQLITETABLE_MOVIE.Replace("movie", name);
            try
            {
                if (!dB.IsTableExist(name)) dB.CreateTable(name);
            }
            catch
            {
                HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotSupport);
                return false;
            }
            finally
            {
                dB.CloseDB();
            }
            return true;

        }

        private void ReMoveFromMyList(string name)
        {
            MySqlite dB = new MySqlite("mylist");
            if (dB.IsTableExist(name)) dB.DeleteTable(name);
            dB.CloseDB();
        }



        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            bool isInList = false;
            for (int i = 0; i < ListItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
                StackPanel sp = FindElementByName<StackPanel>(c, "ListStackPanel");
                if (sp != null)
                {
                    var grids = sp.Children.OfType<Grid>().ToList();
                    foreach (Grid grid in grids)
                    {
                        RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
                        if (radioButton != null && (bool)radioButton.IsChecked)
                        {
                            isInList = true;
                            break;

                        }
                    }

                }
            }

            StackPanel stackPanel = sender as StackPanel;
            ContextMenu contextMenu = stackPanel.ContextMenu;

            if (isInList)
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                    if (item.Header.ToString() != Jvedio.Language.Resources.Menu_RemoveFromList)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                foreach (MenuItem item in contextMenu.Items)
                {
                    if (item.Header.ToString() == Jvedio.Language.Resources.Menu_RemoveFromList)
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }





            if (contextMenu.Visibility != Visibility.Visible) return;
            Task.Run(() =>
            {
                Task.Delay(100).Wait();
                this.Dispatcher.Invoke(() =>
                {
                    MenuItem menuItem = FindElementByName<MenuItem>(contextMenu, "ListMenuItem");
                    if (menuItem != null)
                    {
                        menuItem.Items.Clear();
                        foreach (var item in vieModel.MyList)
                        {
                            MenuItem menuItem1 = new MenuItem();
                            menuItem1.Header = item.Name;
                            //menuItem1.Name = item.Name;
                            menuItem1.Click += MyListItemClick;
                            menuItem.Items.Add(menuItem1);
                        }
                    }
                });
            });


        }


        private void MyListItemClick(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            string table = menuItem.Header.ToString();
            MenuItem mnu = menuItem.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    Movie newMovie = DataBase.SelectMovieByID(movie.id);
                    MySqlite dB = new MySqlite("mylist");
                    dB.InsertFullMovie(newMovie, table);
                    dB.CloseDB();
                    InitList();
                }
            }
        }

        private void Rate_ValueChanged_1(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (vieModel.Actress != null)
            {
                DataBase.CreateTable(DataBase.SQLITETABLE_ACTRESS_LOVE);
                DataBase.SaveActressLikeByName(vieModel.Actress.name, vieModel.Actress.like);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            vieModel.CurrentActorPage = 1;
            List<string> actressNames = DataBase.SelectActressNameByLove(1);

            List<Actress> oldActress = vieModel.ActorList.ToList();
            List<Actress> newActress = new List<Actress>();
            vieModel.ActorList = new ObservableCollection<Actress>();
            foreach (Actress actress in oldActress)
            {
                if (actressNames.Contains(actress.name))
                {
                    newActress.Add(actress);
                }
            }

            vieModel.ActorList = new ObservableCollection<Actress>();
            vieModel.ActorList.AddRange(newActress);

            vieModel.ActorFlipOver();

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            vieModel.GetActorList();
        }

        private void OpenLogPath(object sender, EventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            if (Directory.Exists(path)) { Process.Start("explorer.exe", "\"" + path + "\""); }
        }

        private void OpenImageSavePath(object sender, EventArgs e)
        {
            if (Directory.Exists(Properties.Settings.Default.BasePicPath)) { Process.Start("explorer.exe", "\"" + Properties.Settings.Default.BasePicPath + "\""); }
        }

        private void OpenApplicationPath(object sender, EventArgs e)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory)) { Process.Start("explorer.exe", "\"" + AppDomain.CurrentDomain.BaseDirectory + "\""); }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void HideActressGrid(object sender, MouseButtonEventArgs e)
        {
            ActorInfoGrid.Visibility = Visibility.Collapsed;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(ProgressTextBox!=null) ProgressTextBox.Text =(int)e.NewValue + "%";
            if (Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.IsPlatformSupported && taskbarInstance!=null)
            {
                taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.Normal,this);
                taskbarInstance.SetProgressValue((int)e.NewValue, 100,this);
                if (e.NewValue == 100) taskbarInstance.SetProgressState(Microsoft.WindowsAPICodePack.Taskbar.TaskbarProgressBarState.NoProgress,this);
            }
        }

        private void ActorProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ActorProgressTextBox != null) ActorProgressTextBox.Text = (int)e.NewValue + "%";
            ActorProgressBar.Visibility = Visibility.Visible;
        }


        public string GetCurrentList()
        {
            string table = "";
            for (int i = 0; i < ListItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
                StackPanel stackPanel = FindElementByName<StackPanel>(c, "ListStackPanel");
                if (stackPanel != null)
                {
                    var grids = stackPanel.Children.OfType<Grid>().ToList();
                    foreach (Grid grid in grids)
                    {
                        RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
                        if (radioButton != null && (bool)radioButton.IsChecked)
                        {
                            table = radioButton.Content.ToString();
                            break;
                        }
                    }

                }
            }
            return table;
        }

        private async void RemoveFromList(object sender, RoutedEventArgs e)
        {
            string table = GetCurrentList();
            if (string.IsNullOrEmpty(table)) return;




            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                if (sp != null)
                {
                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                    if (Properties.Settings.Default.EditMode && new Msgbox(this, Jvedio.Language.Resources.IsToRemove).ShowDialog() == false) return; 
                    MySqlite dB = new MySqlite("mylist");
                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                        {

                            dB.DeleteByField(table, "id", arg.id);

                            vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                            vieModel.MovieList.Remove(arg);
                            vieModel.FilterMovieList.Remove(arg);
                        });
                    dB.Close();
                    //从详情窗口删除
                    if (GetWindowByName("WindowDetails") != null)
                    {
                        WindowDetails windowDetails = GetWindowByName("WindowDetails") as WindowDetails;
                        foreach (var item in vieModel.SelectedMovie.ToList())
                        {
                            if (windowDetails.vieModel.DetailMovie.id == item.id)
                            {
                                windowDetails.Close();
                                break;
                            }
                        }
                    }

                    HandyControl.Controls.Growl.Info(Jvedio.Language.Resources.Message_Success, "Main");
                    //修复数字显示
                    vieModel.CurrentCount -= vieModel.SelectedMovie.Count;
                    vieModel.TotalCount -= vieModel.SelectedMovie.Count;

                    vieModel.SelectedMovie.Clear();
                    InitList();
                    await Task.Run(() => { Task.Delay(100).Wait(); });


                    //侧边栏选项选中
                    for (int i = 0; i < ListItemsControl.Items.Count; i++)
                    {
                        ContentPresenter c = (ContentPresenter)ListItemsControl.ItemContainerGenerator.ContainerFromItem(ListItemsControl.Items[i]);
                        StackPanel stackPanel = FindElementByName<StackPanel>(c, "ListStackPanel");
                        if (stackPanel != null)
                        {
                            var grids = stackPanel.Children.OfType<Grid>().ToList();
                            foreach (Grid grid in grids)
                            {
                                RadioButton radioButton = grid.Children.OfType<RadioButton>().First();
                                if (radioButton != null && radioButton.Content.ToString() == table)
                                {
                                    radioButton.IsChecked = true;
                                    break;
                                }
                            }

                        }
                    }

                }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        LoadActorMovies loadActorMovies;
        private void Border_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (loadActorMovies != null) loadActorMovies.Close();
            loadActorMovies = new LoadActorMovies();
            loadActorMovies.Show();
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FocusTextBlock.Focus();
            SearchBar_LostFocus();
        }





        private void ClearActressInfo(object sender, RoutedEventArgs e)
        {
            string name = vieModel.Actress.name;
            DataBase.DeleteByField("actress", "name", name);

            Actress actress = new Actress(vieModel.Actress.name);
            actress.like = vieModel.Actress.like;
            actress.smallimage = vieModel.Actress.smallimage;

            vieModel.Actress = actress;

        }

        private double SideBorderWidth=200;

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (vieModel.HideSide)
            {
                SideBorderWidth = SideBorder.Width;
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(SideBorder.Width, 0, new Duration(TimeSpan.FromMilliseconds(300)),FillBehavior.Stop);
                doubleAnimation1.Completed += (s, _) => SideBorder.Width = 0;
                SideBorder.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation1);
                ShowSideBorderGrid.Visibility = Visibility.Visible;
            }
            else
            {
                ShowSideBorderGrid.Visibility = Visibility.Collapsed;
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(0, SideBorderWidth, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.Stop);
                doubleAnimation1.Completed += (s, _) => SideBorder.Width = SideBorderWidth;
                SideBorder.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation1);
            }
        }

        private void ShowRestMovie(object sender, RoutedEventArgs e)
        {
            int low = ++vieModel.FlowNum;

            for (int i = low; i < 5; i++)
            {
                vieModel.FlowNum = i;
                vieModel.Flow();
            }



        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ClassTabControl != null)
            {
                ActorToolsStackPanel.Visibility = Visibility.Collapsed;
                ActorPageGrid.Visibility = Visibility.Collapsed;
                switch (ClassTabControl.SelectedIndex)
                {
                    case 0:
                        ActorToolsStackPanel.Visibility = Visibility.Visible;
                        ActorPageGrid.Visibility = Visibility.Visible;
                        vieModel.GetActorList();
                        break;

                    case 1:
                        vieModel.GetGenreList();
                        break;

                    case 2:
                        vieModel.GetLabelList();
                        break;

                    default:

                        break;
                }
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            AllSearchPopup.IsOpen = false;
        }

        private void HideBeginScanGrid(object sender, MouseButtonEventArgs e)
        {
            BeginScanGrid.Visibility = Visibility.Hidden;
        }
    }




}
