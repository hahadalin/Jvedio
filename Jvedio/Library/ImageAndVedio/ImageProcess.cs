using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    public static class ImageProcess
    {

        public static void SetImage(ref Movie movie)
        {
            //加载图片
            BitmapImage smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");
            BitmapImage bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");
            if (smallimage == null) smallimage = DefaultSmallImage;
            if (bigimage == null) bigimage = DefaultBigImage;
            movie.smallimage = smallimage;
            movie.bigimage = bigimage;
        }

        public static BitmapImage GetActorImage( string name)
        {
            //加载图片
            BitmapImage image = ImageProcess.GetBitmapImage(name, "Actresses");
            if (image == null) image = DefaultActorImage;
            return image;
        }

        public static System.Drawing.Bitmap byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)returnImage;
            return bitmap;
        }

        public static byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public static string ImageToBase64(Bitmap bitmap, string fileFullName = "")
        {
            try
            {
                if (fileFullName != "")
                {
                    Bitmap bmp = new Bitmap(fileFullName);
                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length]; ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length); ms.Close();
                    return Convert.ToBase64String(arr);
                }
                else
                {
                    MemoryStream ms = new MemoryStream();
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length]; ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length); ms.Close();
                    return Convert.ToBase64String(arr);
                }

            }
            catch
            {

                return null;
            }
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            base64 = base64.Replace("data:image/png;base64,", "").Replace("data:image/jgp;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
            byte[] bytes = Convert.FromBase64String(base64);
            MemoryStream memStream = new MemoryStream(bytes);
            Image mImage = Image.FromStream(memStream);
            Bitmap bp = new Bitmap(mImage);
            return bp;
            //bp.Save("C:/Users/Administrator/Desktop/" + DateTime.Now.ToString("yyyyMMddHHss") + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);//注意保存路径
        }

        public static Int32Rect GetActressRect(BitmapSource bitmapSource, Int32Rect int32Rect)
        {
            if (bitmapSource.PixelWidth > 125 && bitmapSource.PixelHeight > 125)
            {
                int width = 250;
                int y = int32Rect.Y + (int32Rect.Height / 2) - width / 2; ;
                int x = int32Rect.X + (int32Rect.Width / 2) - width / 2;
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x + width > bitmapSource.PixelWidth) x = bitmapSource.PixelWidth - width;
                if (y + width > bitmapSource.PixelHeight) y = bitmapSource.PixelHeight - width;
                return new Int32Rect(x, y, width, width);
            }
            else
                return Int32Rect.Empty;

        }

        public static Int32Rect GetRect(BitmapSource bitmapSource, Int32Rect int32Rect)
        {
            // 150*200
            if (bitmapSource.PixelWidth >= bitmapSource.PixelHeight)
            {
                int y = 0;
                int width = (int)(0.75 * bitmapSource.PixelHeight);
                int x = int32Rect.X + (int32Rect.Width / 2) - width / 2;
                int height = bitmapSource.PixelHeight;
                if (x < 0) x = 0;
                if (x + width > bitmapSource.PixelWidth) x = bitmapSource.PixelWidth - width;
                return new Int32Rect(x, y, width, height);
            }
            else
            {
                int x = 0;
                int height = (int)(0.75 * bitmapSource.PixelWidth);
                int y = int32Rect.Y + (int32Rect.Height / 2) - height / 2;
                int width = bitmapSource.PixelWidth;
                if (y < 0) y = 0;
                if (y + height > bitmapSource.PixelHeight) x = bitmapSource.PixelHeight - height;
                return new Int32Rect(x, y, width, height);
            }

        }

        public static BitmapSource CutImage(BitmapSource bitmapSource, Int32Rect cut)
        {
            //计算Stride
            var stride = bitmapSource.Format.BitsPerPixel * cut.Width / 8;
            byte[] data = new byte[cut.Height * stride];
            bitmapSource.CopyPixels(cut, data, stride, 0);
            return BitmapSource.Create(cut.Width, cut.Height, 0, 0, PixelFormats.Bgr32, null, data, stride);
        }

        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;
            Bitmap bmp = new System.Drawing.Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride); bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                return result;
            }
        }


        public static void SaveImage(string ID, byte[] ImageByte, ImageType imageType, string Url)
        {
            string FilePath;
            string ImageDir;
            if (imageType == ImageType.BigImage)
            {
                ImageDir = BasePicPath + $"BigPic\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }
            else if (imageType == ImageType.ExtraImage)
            {
                ImageDir = BasePicPath + $"ExtraPic\\{ID}\\";
                FilePath = ImageDir + Path.GetFileName(new Uri(Url).LocalPath);
            }
            else if (imageType == ImageType.SmallImage)
            {
                ImageDir = BasePicPath + $"SmallPic\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }
            else
            {
                ImageDir = BasePicPath + $"Actresses\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }

            if (!Directory.Exists(ImageDir)) Directory.CreateDirectory(ImageDir);
           FileProcess.  ByteArrayToFile(ImageByte, FilePath);
        }

        /// <summary>
        /// 加载 Gif
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public static BitmapImage GifFromFile(string filepath)
        {
            try
            {
                using (var fs = new FileStream(filepath, System.IO.FileMode.Open))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return null;
        }


        /// <summary>
        /// 防止图片被占用
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static BitmapImage BitmapImageFromFile(string filepath,double DecodePixelWidth=0)
        {
            try
            {
                using (var fs = new FileStream(filepath, System.IO.FileMode.Open))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    if (DecodePixelWidth!=0) bitmap.DecodePixelWidth = (int)DecodePixelWidth;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return null;

        }







        public static BitmapImage GetBitmapImage(string filename, string imagetype, double DecodePixelWidth = 0)
        {
            filename = BasePicPath + $"{imagetype}\\{filename}.jpg";
            if (File.Exists(filename))
                return BitmapImageFromFile(filename, DecodePixelWidth);
            else
                return null;
        }

        public static BitmapImage GetExtraImage(string filepath)
        {
            if (File.Exists(filepath))
                return BitmapImageFromFile(filepath);
            else
                return null;
        }


    }


    public class ScreenShot
    {
        public event EventHandler SingleScreenShotCompleted;
        public void BeginScreenShot(object o)
        {
            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string i = list[1] as string;
            string filePath = list[2] as string;
            string ScreenShotPath = list[3] as string;
            string output = $"{ScreenShotPath}\\ScreenShot-{i.PadLeft(2, '0')}.jpg";

            if (string.IsNullOrEmpty(cutoffTime)) return;
            SemaphoreScreenShot.WaitOne();

            //--使用 ffmpeg.exe 截图
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = Properties.Settings.Default.FFMPEG_Path;
            startInfo.CreateNoWindow = true;
            string str = $"-y -threads 1 -ss {cutoffTime} -i \"{filePath}\" -f image2 -frames:v 1 \"{output}\"";
            startInfo.UseShellExecute = false;
            startInfo.Arguments = str;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();
            StreamReader readerOut = process.StandardOutput;
            StreamReader readerErr = process.StandardError;
            string errors = readerErr.ReadToEnd();
            string output2 = readerOut.ReadToEnd();
            while (!process.HasExited) { continue; }
            //--使用 ffmpeg.exe 截图
            SemaphoreScreenShot.Release();
            SingleScreenShotCompleted?.Invoke(this,new ScreenShotEventArgs(str, output));
            //App.Current.Dispatcher.Invoke((Action)delegate { cmdTextBox.AppendText(str + "\n"); cmdTextBox.ScrollToEnd(); });
            lock (ScreenShotLockObject) { ScreenShotCurrent += 1; }
        }


        public Semaphore SemaphoreScreenShot;
        public int ScreenShotCurrent = 0;
        public object ScreenShotLockObject = 0;

        public async Task<(bool, string)> AsyncScreenShot(Movie movie)
        {
            bool result = true;
            string message = "";
            List<string> outputPath = new List<string>();
            await Task.Run(() => {
                // n 个线程截图
                if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { result = false; message = "未配置 FFmpeg.exe 路径"; return; }

                int num = Properties.Settings.Default.ScreenShot_ThreadNum;
                string ScreenShotPath = "";
                //if (Properties.Settings.Default.ScreenShotToExtraPicPath) ScreenShotPath = BasePicPath + "ExtraPic\\" + movie.id;
                //else 
                    ScreenShotPath = BasePicPath + "ScreenShot\\" + movie.id;

                if (!Directory.Exists(ScreenShotPath)) Directory.CreateDirectory(ScreenShotPath);

                string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
                if (cutoffArray.Length == 0) { result = false; message = "未成功分割影片截图"; return; }
                int SemaphoreNum = cutoffArray.Length > 10 ? 10 : cutoffArray.Length;//最多 10 个线程截图
                SemaphoreScreenShot = new Semaphore(SemaphoreNum, SemaphoreNum);

               

                ScreenShotCurrent = 0;
                int ScreenShotTotal = cutoffArray.Count();
                ScreenShotLockObject = new object();

                for (int i = 0; i < cutoffArray.Count(); i++)
                {
                    outputPath.Add($"{ScreenShotPath}\\ScreenShot-{i.ToString().PadLeft(2, '0')}.jpg");
                    List<object> list = new List<object>() { cutoffArray[i], i.ToString(), movie.filepath, ScreenShotPath };
                    Thread threadObject = new Thread(BeginScreenShot);
                    threadObject.Start(list);
                }

                //等待直到所有线程结束
                while (ScreenShotCurrent != ScreenShotTotal)
                {
                    Task.Delay(100).Wait();
                }
                //cmdTextBox.AppendText($"已启用 {cutoffArray.Count()} 个线程， 3-10S 后即可截图成功\n");
            });
            foreach (var item in outputPath)
            {
                if (!File.Exists(item))
                {
                    result = false;
                    message = $"未成功生成 {item}";
                    break;
                }
            }
            return (result, message);
        }
    }


    public class ScreenShotEventArgs : EventArgs
    {
        public string FFmpegCommand;
        public string FilePath;

        public ScreenShotEventArgs(string _FFmpegCommand,string filepath)
        {
            FFmpegCommand = _FFmpegCommand;
            FilePath = filepath;
        }
    }

}
