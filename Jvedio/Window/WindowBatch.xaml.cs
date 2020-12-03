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
using static Jvedio.GlobalVariable;
using System.Data;
using System.Windows.Controls.Primitives;
using FontAwesome.WPF;
using System.ComponentModel;
using DynamicData.Annotations;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading;

namespace Jvedio
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class WindowBatch : Jvedio_BaseWindow
    {
        public VieModel_Batch vieModel;

        public CancellationTokenSource cts;
        public CancellationToken ct;
        public bool Running = false;
        public bool Pause = false;


        public WindowBatch()
        {
            InitializeComponent();

            var stackPanels = MainGrid.Children.OfType<StackPanel>().ToList();
            foreach (var item in stackPanels) item.Visibility = Visibility.Collapsed;


            var wrapPanels = SettingsGrid.Children.OfType<StackPanel>().ToList();
            foreach (var item in wrapPanels) item.Visibility = Visibility.Collapsed;
            var RadioButtons = SideStackPanel.Children.OfType<RadioButton>().ToList();


            stackPanels[Properties.Settings.Default.BatchIndex].Visibility = Visibility.Visible;
            wrapPanels[Properties.Settings.Default.BatchIndex].Visibility = Visibility.Visible;
            RadioButtons[Properties.Settings.Default.BatchIndex].IsChecked = true;

            

            if (Properties.Settings.Default.BatchIndex == 2 || Properties.Settings.Default.BatchIndex == 0) FirsrProgressBar.Visibility = Visibility.Visible;
            else FirsrProgressBar.Visibility = Visibility.Collapsed;



            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info("取消当前任务！", "BatchGrowl"); });
            ct = cts.Token;

        }

        private void ShowGrid(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            StackPanel SP = radioButton.Parent as StackPanel;
            var radioButtons = SP.Children.OfType<RadioButton>().ToList();
            

            Properties.Settings.Default.BatchIndex = radioButtons.IndexOf(radioButton);
            Properties.Settings.Default.Save();

            
            
            var stackPanels = MainGrid.Children.OfType<StackPanel>().ToList();
            foreach (var item in stackPanels) item.Visibility = Visibility.Collapsed;


            var wrapPanels = SettingsGrid.Children.OfType<StackPanel>().ToList();
            foreach (var item in wrapPanels) item.Visibility = Visibility.Collapsed;


            stackPanels[Properties.Settings.Default.BatchIndex].Visibility = Visibility.Visible;
            wrapPanels[Properties.Settings.Default.BatchIndex].Visibility = Visibility.Visible;




            if (Properties.Settings.Default.BatchIndex == 2 || Properties.Settings.Default.BatchIndex == 0) FirsrProgressBar.Visibility = Visibility.Visible;
            else FirsrProgressBar.Visibility = Visibility.Collapsed;

            ResetTask();

        }

        private void ShowStatus(string str,bool newline=true)
        {
            if (App.Current != null)
            {
                if (newline)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate {
                        StatusTextBox.AppendText(str + '\n');
                        StatusTextBox.ScrollToEnd();
                    });
                }
                else
                {
                    App.Current.Dispatcher.Invoke((Action)delegate {
                        StatusTextBox.AppendText(str);
                    });
                }

            }

        }


        public Semaphore SemaphoreScreenShot;
        public object lockobject = new object();

        public bool ScreenShot(string id)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) return false;

            Movie movie = DataBase.SelectMovieByID(id);
            int SemaphoreNum = vieModel.ScreenShot_Num;
            string ScreenShotPath = "";
            if (Properties.Settings.Default.ScreenShotToExtraPicPath)
                ScreenShotPath = BasePicPath + "ExtraPic\\" + movie.id;
            else
                ScreenShotPath = BasePicPath + "ScreenShot\\" + movie.id;

            if (!Directory.Exists(ScreenShotPath)) Directory.CreateDirectory(ScreenShotPath);



            string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
            SemaphoreScreenShot = new Semaphore(SemaphoreNum, SemaphoreNum);
            vieModel.ScreenShot_TotalNum_S = cutoffArray.Count();
            vieModel.ScreenShot_CurrentProgress_S = 0;

            
            for (int i = 0; i < cutoffArray.Count(); i++)
            {
                List<object> list = new List<object>() { cutoffArray[i], i.ToString(), movie.filepath, ScreenShotPath };
                Thread threadObject = new Thread(BeginScreenShot);
                threadObject.Start(list);
            }

            //等待直到所有线程完成
            while (vieModel.ScreenShot_CurrentProgress_S < vieModel.ScreenShot_TotalNum_S)
            {
                Task.Delay(500).Wait();
            }
            return true;

        }


        public void BeginScreenShot(object o)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) {
                lock (lockobject)
                {
                    vieModel.ScreenShot_CurrentProgress_S++;
                }
                return;
            };

            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string i = list[1] as string;
            string filePath = list[2] as string;
            string ScreenShotPath = list[3] as string;

            if (string.IsNullOrEmpty(cutoffTime))
            {
                lock (lockobject)
                {
                    vieModel.ScreenShot_CurrentProgress_S++;
                }
                return;
            }

            //try
            //{
            //    ct.ThrowIfCancellationRequested();

            //    if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }

            //    while (Pause) { Task.Delay(500).Wait(); }
            //}
            //catch (OperationCanceledException ex)
            //{
            //    return;
            //}


            SemaphoreScreenShot.WaitOne();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -threads 1 -ss {cutoffTime} -i \"{filePath}\" -f image2 -frames:v 1 \"{ScreenShotPath}\\ScreenShot-{i.PadLeft(2, '0')}.jpg\"";



            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            _ = p.StandardOutput.ReadToEnd();
            //App.Current.Dispatcher.Invoke((Action)delegate { cmdTextBox.AppendText(output + "\n"); });
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            SemaphoreScreenShot.Release();

            lock (lockobject)
            {
                vieModel.ScreenShot_CurrentProgress_S++;
            }
            ShowStatus(">",false);

        }












        private bool GenGif(string id)
        {
            Movie movie = DataBase.SelectMovieByID(id);
            string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
            if (cutoffArray.Count() < 2) return false;
            string cutoffTime = cutoffArray[new Random().Next(1, cutoffArray.Count()-1)];
            string filePath = movie.filepath;

            string GifSavePath = BasePicPath + "Gif\\";
            if (!Directory.Exists(GifSavePath)) Directory.CreateDirectory(GifSavePath);
            GifSavePath += id + ".gif";
            

            if (string.IsNullOrEmpty(cutoffTime)) return false;
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) return false;

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            int width = vieModel.Gif_Width;
            int height = vieModel.Gif_Height;

            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -t {vieModel.Gif_Length} -ss {cutoffTime} -i \"{filePath}\" -s {width}x{height}  \"{GifSavePath}\"";
            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            _ = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();

            return true;
        }

        private void SetEnable(bool enable)
        {
            if (!enable)
            {

                var radioButtons = SideStackPanel.Children.OfType<RadioButton>().ToList();
                for (int i = 0; i < radioButtons.Count; i++)
                {
                    if (i != Properties.Settings.Default.BatchIndex) radioButtons[i].IsEnabled = false;
                }

                var stackpanels = SettingsGrid.Children.OfType<StackPanel>().ToList();
                foreach (var item in stackpanels) { item.IsEnabled = false;  }


            }
            else
            {
                var radioButtons = SideStackPanel.Children.OfType<RadioButton>().ToList();
                for (int i = 0; i < radioButtons.Count; i++)
                {
                     radioButtons[i].IsEnabled = true;
                }
                var stackpanels = SettingsGrid.Children.OfType<StackPanel>().ToList();
                foreach (var item in stackpanels) { item.IsEnabled = true; }
            }
        }


        private async  Task<bool> DownLoad(string id)
        {
            

            Movie movie = DataBase.SelectMovieByID(id);
            bool success; string resultMessage;

            if (vieModel.Info_ForceDownload)
            {
                (success, resultMessage) = await Task.Run(() => { return Net.DownLoadFromNet(movie); });
                if (success)
                {
                    ShowStatus($"同步信息成功：{id}");
                    Task.Delay(3000).Wait();
                }
                else
                    ShowStatus($"同步信息失败：{id}，原因为：{resultMessage}");

                if (id.ToUpper().IndexOf("FC2") >= 0) Task.Delay(5000).Wait();
            }
            else
            {
                //智能判断
                if (movie.title == "" || movie.smallimageurl == "" || movie.bigimageurl == "" || movie.extraimageurl == "")
                {
                    (success, resultMessage) = await Task.Run(() => { return Net.DownLoadFromNet(movie); });
                    if (success)
                    {
                        ShowStatus($"同步信息成功：{id}");
                        Task.Delay(3000).Wait();
                    }
                    else
                        ShowStatus($"同步信息失败：{id}，原因为：{resultMessage}");

                    if (id.ToUpper().IndexOf("FC2") >= 0) Task.Delay(5000).Wait();
                }
                else
                {
                    ShowStatus($"跳过已同步的信息：{id}");
                }
            }

            
            

            vieModel.Info_CurrentProgress_S++;
            movie = DataBase.SelectMovieByID(id);

            List<string> extraImageList = new List<string>();

            if (!string.IsNullOrEmpty(movie.extraimageurl) && movie.extraimageurl.IndexOf(";") > 0) {
                extraImageList = movie.extraimageurl.Split(';').ToList().Where(arg =>  !string.IsNullOrEmpty(arg) && arg.IndexOf("http")>=0 && arg.IndexOf("dmm") >= 0).ToList();
            }
            vieModel.Info_TotalNum_S += extraImageList.Count;

            //同步缩略图
            if (vieModel.Info_DS)
            {
                (success, resultMessage) = await DownLoadSmallPic(movie);
                ShowStatus($"下载缩略图：{resultMessage}");
                if (success) Task.Delay(1000).Wait();
            }

            vieModel.Info_CurrentProgress_S++;

            try
            {
                if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                while (Pause) { Task.Delay(500).Wait(); }
                ct.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException ex) { return false; }

            //同步海报图
            if (vieModel.Info_DB)
            {
                (success, resultMessage) = await DownLoadBigPic(movie);
                ShowStatus($"下载海报图：{resultMessage}");
                if (success) Task.Delay(1000).Wait();
            }
            vieModel.Info_CurrentProgress_S++;

            try
            {
                if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                while (Pause) { Task.Delay(500).Wait(); }
                ct.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException ex) { return false; }

            //同步预览图
            if (vieModel.Info_DE)
            {
                string cookies = "";
                bool extraImageSuccess = false;
                string filepath = "";
            
                for (int i = 0; i < extraImageList.Count; i++)
                {
                    try
                    {
                        if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                        while (Pause) { Task.Delay(500).Wait(); }
                        ct.ThrowIfCancellationRequested();
                    } catch (OperationCanceledException ex) { break; }

                    if (extraImageList[i].Length > 0)
                    {
                        filepath = GlobalVariable.BasePicPath + "ExtraPic\\" + movie.id + "\\" + Path.GetFileName(new Uri(extraImageList[i]).LocalPath);
                        if (!File.Exists(filepath))
                        {
                            (extraImageSuccess, cookies) = await Task.Run(() => { return Net.DownLoadImage(extraImageList[i], ImageType.ExtraImage, movie.id, Cookie: cookies); });
                            if (extraImageSuccess)
                            {
                                Task.Delay(5000).Wait();
                                ShowStatus($"下载预览图成功{i+1}/{extraImageList.Count}");
                            }
                            else
                            {
                                ShowStatus($"下载预览图失败{i + 1}/{extraImageList.Count}");
                            }
                        }
                    }
                    vieModel.Info_CurrentProgress_S++;
                }

            }
            return true;
        }


        private async Task<(bool, string)> DownLoadSmallPic(Movie movie)
        {
            if(movie.smallimageurl.IndexOf("pics.dmm")<=0) return (false, "失败，原因：链接仅支持 dmm");

            if (!File.Exists(GlobalVariable.BasePicPath + $"SmallPic\\{movie.id}.jpg"))  
            {
                if (movie.source == "javdb" ) return (false, "失败，原因：链接仅支持 dmm");
                else return await Net.DownLoadImage(movie.smallimageurl, ImageType.SmallImage, movie.id);
            }
            else return (false, "跳过");

        }

        private async Task<(bool, string)> DownLoadBigPic(Movie movie)
        {
            if (movie.smallimageurl.IndexOf("pics.dmm") <= 0) return (false, "失败，原因：链接仅支持 dmm");

            if ( !File.Exists(GlobalVariable.BasePicPath + $"BigPic\\{movie.id}.jpg"))
            {
                if (movie.source == "javdb") return (false, "失败，原因：链接仅支持 dmm");
                else return await Net.DownLoadImage(movie.bigimageurl, ImageType.BigImage, movie.id);
            }
            else return (false, "跳过");

        }

        private int GetInfoNum_S()
        {
            int num = 1;
            if (vieModel.Info_DS) num++;
            if (vieModel.Info_DB) num++;
            return num;
        }

        private async void StartTask(object sender, RoutedEventArgs e)
        {

            // n 个线程截图
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path))
            {
                HandyControl.Controls.Growl.Error("请在【设置-视频处理】中配置 FFmpeg.exe 路径", "BatchGrowl");
                return;
            }

            if (Running)
            {
                HandyControl.Controls.Growl.Error("其他任务正在进行！", "BatchGrowl");
                return;
            }
            ResetTask();
            SetEnable(false);


            cts = new CancellationTokenSource();
            cts.Token.Register(() => { HandyControl.Controls.Growl.Info("取消当前任务！", "BatchGrowl");  });
            ct = cts.Token;

            Running = true;
            StatusTextBox.Text = "";
            PauseButton.IsEnabled = true;
            PauseButton.Content = "暂停";
            vieModel.CurrentNum = 0;
            vieModel.Progress = 0;

            int idx = Properties.Settings.Default.BatchIndex;
            switch (idx)
            {
                case 0:
                    //信息同步
                    await Task.Run(async () =>
                    {
                        try
                        {
                            ShowStatus("------------开始同步------------");
                            for (int i = 0; i < vieModel.Info_TotalNum; i++)
                            {

                                if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                                while (Pause) { Task.Delay(500).Wait(); }
                                ct.ThrowIfCancellationRequested();

                                vieModel.Info_TotalNum_S = GetInfoNum_S();
                                vieModel.Info_CurrentProgress_S = 0;

                                bool result =  await DownLoad(vieModel.Movies[i]);
                                vieModel.Info_CurrentProgress = i + 1;

                                //每 10 个停一分钟，FC2停 2 分钟
                                if(i>0 && i % 10 == 0)
                                {
                                    if (vieModel.Movies[i].ToUpper().IndexOf("FC2") >= 0)
                                    {
                                        ShowStatus("暂停 2 分钟……");
                                        Task.Delay(120000).Wait();
                                    }
                                    else
                                    {
                                        ShowStatus("暂停 1 分钟……");
                                        Task.Delay(60000).Wait();
                                    }
                                }


                            }

                            ShowStatus("------------完成------------");



                        }
                        catch (OperationCanceledException ex)
                        {
                            ShowStatus("------------任务已取消------------");
                            App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });
                        }
                        finally
                        {
                            cts.Dispose();
                            Running = false;
                        }
                    }, ct);

                    break;


                case 1:
                    //Gif
                    await Task.Run(() =>
                    {
                        try
                        {
                            ShowStatus("------------开始截取Gif------------");
                            for (int i = 0; i < vieModel.Gif_TotalNum; i++)
                            {
                                
                                if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                                while (Pause) { Task.Delay(500).Wait(); }
                                ct.ThrowIfCancellationRequested();
                                bool result = GenGif(vieModel.Movies[i]);
                                if (result)
                                    ShowStatus($"{i+1}/{vieModel.Gif_TotalNum} => 成功");
                                else
                                    ShowStatus($"{i + 1}/{vieModel.Gif_TotalNum} => 失败");

                                vieModel.Gif_CurrentProgress = i + 1;

                                


                            }
                            ShowStatus("------------完成------------");


                        }
                        catch (OperationCanceledException ex)
                        {
                            ShowStatus("------------任务已取消------------");
                            App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });
                        }
                        finally
                        {
                            cts.Dispose();
                            Running = false;
                        }
                    }, ct);
                    break;


                case 2:
                    //ScreenShot
                    await Task.Run(() =>
                    {
                        try
                        {
                            ShowStatus("------------开始截图------------");
                            for (int i = 0; i < vieModel.ScreenShot_TotalNum; i++)
                            {
                                if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                                while (Pause) { Task.Delay(500).Wait(); }
                                ct.ThrowIfCancellationRequested();

                                bool result = ScreenShot(vieModel.Movies[i]);
                                if (result)
                                    ShowStatus($"\n{i + 1}/{vieModel.ScreenShot_TotalNum} => 成功：{vieModel.Movies[i]}");
                                else
                                    ShowStatus($"\n{i + 1}/{vieModel.ScreenShot_TotalNum} => 失败：{vieModel.Movies[i]}");

                                vieModel.ScreenShot_CurrentProgress = i + 1;
                            }

                            ShowStatus("------------完成------------");


                        }
                        catch (OperationCanceledException ex)
                        {
                            ShowStatus("------------任务已取消------------");
                            App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });
                        }
                        finally
                        {
                            cts.Dispose();
                            Running = false;
                        }
                    }, ct);
                    break;

                case 3:

                    break;

                case 4:

                    break;
                case 5:
                    //重命名
                    if (Properties.Settings.Default.RenameFormat.IndexOf("{") < 0)
                    {
                        HandyControl.Controls.Growl.Error("请在设置中配置【重命名】规则");
                        cts.Dispose();
                        Running = false;
                        return;
                    }


                    await Task.Run(() =>
                    {
                        try
                        {
                            double totalcount = vieModel.Movies.Count;
                            ShowStatus("------------开始重命名------------");
                            for (int i = 0; i < totalcount; i++)
                            {
                                if (Pause) { ShowStatus("暂停中……"); App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; }); }
                                while (Pause) { Task.Delay(500).Wait(); }
                                ct.ThrowIfCancellationRequested();
                                (bool result,string message) = Rename(vieModel.Movies[i]);
                                if (result)
                                    ShowStatus($"\n{i + 1}/{totalcount} => 成功：{vieModel.Movies[i]}");
                                else
                                    ShowStatus($"\n{i + 1}/{totalcount} => 失败：{vieModel.Movies[i]}，原因为：{message}");

                                //进度+1
                                vieModel.Rename_CurrentProgress = i + 1;
                            }

                            ShowStatus("------------完成------------");


                        }
                        catch (OperationCanceledException ex)
                        {
                            ShowStatus("------------任务已取消------------");
                            App.Current.Dispatcher.Invoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });
                        }
                        finally
                        {
                            cts.Dispose();
                            Running = false;
                        }
                    }, ct);
                    break;

                default:

                    break;
            }

            SetEnable(true);


        }



        private (bool,string) Rename(string id)
        {
            DetailMovie detailMovie = DataBase.SelectDetailMovieById(id);

            if (!File.Exists(detailMovie.filepath)) return (false,$"文件不存在：{detailMovie.filepath}");
            DetailMovie movie = DataBase.SelectDetailMovieById(id);
            //try
            //{
                string[] newPath = movie.ToFileName();
                if (movie.hassubsection)
                {
                for (int i = 0; i < newPath.Length; i++)
                {
                    if (File.Exists(newPath[i])) return (false, $"新文件已存在：{newPath[i]}，原文件为：{detailMovie.filepath}");
                }

                for (int i = 0; i < newPath.Length; i++)
                    {
                        File.Move(movie.subsectionlist[i], newPath[i]);
                    }
                    movie.filepath = newPath[0];
                    movie.subsection = string.Join(";", newPath);
                    DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                    DataBase.UpdateMovieByID(movie.id, "subsection", movie.subsection, "string");//保存
                    
                }
                else
                {
                    if(File.Exists(newPath[0])) return (false, $"新文件已存在：{newPath[0]}，原文件为：{detailMovie.filepath}");
                    File.Move(movie.filepath, newPath[0]);
                    movie.filepath = newPath[0];
                    DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                }
                return (true, "");
            //}
            //catch (Exception ex)
            //{
            //    return (false, ex.Message);
            //}
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (Pause)
            {
                Pause = false;
                button.Content = "暂停";
            }
            else
            {
                Pause = true;
                button.Content = "继续";
                WaitingPanel.Visibility = Visibility.Visible;
            }

            

            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Running)
            {
                Pause = false;
                try
                {
                    cts?.Cancel();
                    WaitingPanel.Visibility = Visibility.Visible;
                }
                catch (ObjectDisposedException ex) { }
                PauseButton.IsEnabled = false;
                SetEnable(true);

            }
            

        }

        private  void Jvedio_BaseWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Running) { cts.Cancel(); }
        }

        private void ResetTask()
        {
            if (Running)
            {
                HandyControl.Controls.Growl.Error("其他任务正在进行！", "BatchGrowl");
                return;
            }
            WaitingPanel.Visibility = Visibility.Visible;
            Task.Run(() => {
                vieModel.Reset((message) => { Dispatcher.BeginInvoke((Action)delegate { WaitingPanel.Visibility = Visibility.Collapsed; });  });
            });
        }


        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            StatusTextBox.Clear();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ResetTask();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetTask();
        }

        private void Jvedio_BaseWindow_ContentRendered(object sender, EventArgs e)
        {

            vieModel = new VieModel_Batch();
            this.DataContext = vieModel;
            ResetTask();
            //WaitingPanel.Visibility = Visibility.Visible;
        }
    }



    }
