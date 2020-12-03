using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jvedio
{
    /// <summary>
    /// Interaction logic for WindowTest.xaml
    /// </summary>
    public partial class WindowTest : Window
    {
        public WindowTest()
        {
            InitializeComponent();
        }

        Stopwatch Stopwatch = new Stopwatch();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch.Start();

            //测试 MediaParse
            string path1= @"F:\No\FC2\FC2PPV-1458145.mp4";
            string path2 = @"F:\No\步兵系列\Tokyo\n1078_juri_motomiya_hh_n_fhd.wmv";
            string path3 = @"G:\No\FC2\fc2-ppv-684994.mp4";
            //Console.WriteLine(MediaParse.GetVedioDuration(path1));
            //Stopwatch.Stop();
            //Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);
            //Stopwatch.Restart();
            //Console.WriteLine(MediaParse.GetVedioDuration(path2));
            //Stopwatch.Stop();
            //Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);
            //Stopwatch.Restart();

            //Console.WriteLine(MediaParse.GetVedioDuration(path3));
            //Stopwatch.Stop();
            //Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);

            //foreach (var item in MediaParse.GetCutOffArray(path1)) { Console.WriteLine(item); }
            //Stopwatch.Stop();
            //Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);
            //Stopwatch.Restart();

            //foreach (var item in MediaParse.GetCutOffArray(path2)) { Console.WriteLine(item); }
            //Stopwatch.Stop();
            //Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);
            //Stopwatch.Restart();

            //foreach (var item in MediaParse.GetCutOffArray(path3)) { Console.WriteLine(item); }
            //Stopwatch.Stop();
            //Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);


            Console.WriteLine(MediaParse.GetMediaInfo(path1).Format);
            Stopwatch.Stop();
            Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);
            Stopwatch.Restart();
            Console.WriteLine(MediaParse.GetMediaInfo(path2).Format);
            Stopwatch.Stop();
            Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);
            Stopwatch.Restart();

            Console.WriteLine(MediaParse.GetMediaInfo(path3).Format);
            Stopwatch.Stop();
            Console.WriteLine("运行时间：" + Stopwatch.ElapsedMilliseconds);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowConfig windowConfig = new WindowConfig("Main");
            windowConfig.Save(new WindowProperty() { Location=new Point(123,456),Size=new Size(789,777),WinState=GlobalVariable.JvedioWindowState.FullScreen});

            WindowProperty windowProperty = windowConfig.Read();
            Console.WriteLine(123);
        }
    }
}
