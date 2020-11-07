using DynamicData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class MediaParse
    {


        /// <summary>
        /// 获得影片长度（wmv  10ms，其他  100ms）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetVedioDuration(string path)
        {
            MediaInfo mediaInfo = new MediaInfo();
            mediaInfo.Open(path);
            string result = "00:00:00";
            try
            {
                string Duration = mediaInfo.Get(0, 0, "Duration/String3");
                result = Duration.Substring(0, Duration.LastIndexOf("."));
            }
            catch { }

            return result;
        }


        /// <summary>
        /// 生成截图的时间节点（wmv  10ms，其他  100ms）
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string[] GetCutOffArray(string path)
        {
            if (Properties.Settings.Default.ScreenShotNum <= 0 || Properties.Settings.Default.ScreenShotNum > 30) Properties.Settings.Default.ScreenShotNum = 10;
            string[] result = new string[Properties.Settings.Default.ScreenShotNum+2];
            string Duration = GetVedioDuration(path);
            double Second = DurationToSecond(Duration);
            

            if(Second <20) { return null; }
            else
            {
                if (Second > 350) Second = Second - 300; //去掉开头结尾
                    
                // 按照秒 n 等分
                uint splitLength =(uint)( Second / Properties.Settings.Default.ScreenShotNum);
                if (splitLength == 0) splitLength = 1;
                for (int i = 0; i < result.Count(); i++)
                    result[i] = SecondToDuration(300 + splitLength * i);

                if(Second-30> DurationToSecond(result[Properties.Settings.Default.ScreenShotNum - 1]))
                {
                    result[Properties.Settings.Default.ScreenShotNum] = SecondToDuration(Second - 60);
                    result[Properties.Settings.Default.ScreenShotNum + 1] = SecondToDuration(Second - 30);
                }

                return result;
            }

            
        }

        public static double DurationToSecond(string Duration)
        {
            if (string.IsNullOrEmpty(Duration) || Duration.Split(':').Count() < 3) return 0;
            double Hour = double.Parse( Duration.Split(':')[0]);
            double Minutes = double.Parse(Duration.Split(':')[1]);
            double Seconds = double.Parse(Duration.Split(':')[2]);
            return Hour * 3600 + Minutes * 60 + Seconds;
        }

        public static string SecondToDuration(double Second)
        {
            // 36000 10h
            if (Second ==0 ) return "00:00:00";
            TimeSpan timeSpan = TimeSpan.FromSeconds(Second);
            return $"{timeSpan.Hours.ToString().PadLeft(2,'0')}:{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}";
        }



        /// <summary>
        /// 获取视频信息 （wmv  10ms，其他  100ms）
        /// </summary>
        /// <param name="VideoName"></param>
        /// <returns></returns>
        public static VedioInfo GetMediaInfo(string vediopath)
        {
            VedioInfo vedioInfo = new VedioInfo() { Format = "", BitRate = "", Duration = "", FileSize = "", Width = "", Height = "", Resolution = "", DisplayAspectRatio = "", FrameRate = "", BitDepth = "", PixelAspectRatio = "", Encoded_Library = "", FrameCount = "", AudioFormat = "", AudioBitRate = "", AudioSamplingRate = "", Channel = "" };
            if (File.Exists(vediopath))
            {
                MediaInfo MI = new MediaInfo();
                MI.Open(vediopath);
                //全局
                string format = MI.Get(StreamKind.General, 0, "Format");
                string bitrate = MI.Get(StreamKind.General, 0, "BitRate/String");
                string duration = MI.Get(StreamKind.General, 0, "Duration/String1");
                string fileSize = MI.Get(StreamKind.General, 0, "FileSize/String");
                //视频
                string vid = MI.Get(StreamKind.Video, 0, "ID");
                string video = MI.Get(StreamKind.Video, 0, "Format");
                string vBitRate = MI.Get(StreamKind.Video, 0, "BitRate/String");
                string vSize = MI.Get(StreamKind.Video, 0, "StreamSize/String");
                string width = MI.Get(StreamKind.Video, 0, "Width");
                string height = MI.Get(StreamKind.Video, 0, "Height");
                string risplayAspectRatio = MI.Get(StreamKind.Video, 0, "DisplayAspectRatio/String");
                string risplayAspectRatio2 = MI.Get(StreamKind.Video, 0, "DisplayAspectRatio");
                string frameRate = MI.Get(StreamKind.Video, 0, "FrameRate/String");
                string bitDepth = MI.Get(StreamKind.Video, 0, "BitDepth/String");
                string pixelAspectRatio = MI.Get(StreamKind.Video, 0, "PixelAspectRatio");
                string encodedLibrary = MI.Get(StreamKind.Video, 0, "Encoded_Library");
                string encodeTime = MI.Get(StreamKind.Video, 0, "Encoded_Date");
                string codecProfile = MI.Get(StreamKind.Video, 0, "Codec_Profile");
                string frameCount = MI.Get(StreamKind.Video, 0, "FrameCount");

                //音频
                string aid = MI.Get(StreamKind.Audio, 0, "ID");
                string audio = MI.Get(StreamKind.Audio, 0, "Format");
                string aBitRate = MI.Get(StreamKind.Audio, 0, "BitRate/String");
                string samplingRate = MI.Get(StreamKind.Audio, 0, "SamplingRate/String");
                string channel = MI.Get(StreamKind.Audio, 0, "Channel(s)");
                string aSize = MI.Get(StreamKind.Audio, 0, "StreamSize/String");

                string audioInfo = MI.Get(StreamKind.Audio, 0, "Inform") + MI.Get(StreamKind.Audio, 1, "Inform") + MI.Get(StreamKind.Audio, 2, "Inform") + MI.Get(StreamKind.Audio, 3, "Inform");
                string videoInfo = MI.Get(StreamKind.Video, 0, "Inform");
                MI.Close();

                vedioInfo = new VedioInfo()
                {
                    Format = format,
                    BitRate = vBitRate,
                    Duration = duration.Replace("h", "小时").Replace("mn", "分钟").Replace("ms", "毫秒").Replace("s", "秒"),
                    FileSize = fileSize,
                    Width = width,
                    Height = height,
                    Resolution = width + "x" + height,
                    DisplayAspectRatio = risplayAspectRatio,
                    FrameRate = frameRate,
                    BitDepth = bitDepth,
                    PixelAspectRatio = pixelAspectRatio,
                    Encoded_Library = encodedLibrary,
                    FrameCount = frameCount,
                    AudioFormat = audio,
                    AudioBitRate = aBitRate,
                    AudioSamplingRate = samplingRate,
                    Channel = channel
                };
            }
            return vedioInfo;
        }



        //public static string[] ExtractInfo(string path)
        //{
        //    MediaInfo MI = new MediaInfo();
        //    MI.Open(path);
        //    string[] returnInfo = new string[3];

        //    //File name 0
        //    returnInfo[0] = MI.Get(0, 0, "FileName");

        //    //Date created 2
        //    returnInfo[1] = MI.Get(0, 0, "File_Created_Date").Substring(
        //        MI.Get(0, 0, "File_Created_Date").IndexOf(" ") + 1, MI.Get(0, 0, "File_Created_Date").LastIndexOf(".") - 4);

        //    //Length 4
        //    returnInfo[2] = MI.Get(0, 0, "Duration/String3").Substring(0, MI.Get(0, 0, "Duration/String3").LastIndexOf("."));

        //    return returnInfo;
        //}

    }
}
