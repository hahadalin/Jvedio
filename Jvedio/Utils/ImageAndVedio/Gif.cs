using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace Jvedio.Utils.ImageAndVedio
{
    public class Gif
    {
        string gifpath { get; set; }
        public Gif(string path)
        {
            this.gifpath = path;
        }

        public  FrameMetadata GetFrameMetadata(BitmapFrame frame)
        {
            var metadata = (BitmapMetadata)frame.Metadata;
            var delay = TimeSpan.FromMilliseconds(100);
            var metadataDelay = metadata.GetQueryOrDefault("/grctlext/Delay", 10);
            if (metadataDelay != 0)
                delay = TimeSpan.FromMilliseconds(metadataDelay * 10);
            var disposalMethod = (FrameDisposalMethod)metadata.GetQueryOrDefault("/grctlext/Disposal", 0);
            var frameMetadata = new FrameMetadata
            {
                Left = metadata.GetQueryOrDefault("/imgdesc/Left", 0),
                Top = metadata.GetQueryOrDefault("/imgdesc/Top", 0),
                Width = metadata.GetQueryOrDefault("/imgdesc/Width", frame.PixelWidth),
                Height = metadata.GetQueryOrDefault("/imgdesc/Height", frame.PixelHeight),
                Delay = delay,
                DisposalMethod = disposalMethod
            };
            return frameMetadata;
        }

        public BitmapSource MakeFrame(int width, int height, BitmapSource rawFrame, FrameMetadata metadata, BitmapSource baseFrame)
        {

            DrawingVisual visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                if (baseFrame != null)
                {
                    var fullRect = new Rect(0, 0, width, height);
                    context.DrawImage(baseFrame, fullRect);
                }

                var rect = new Rect(metadata.Left, metadata.Top, metadata.Width, metadata.Height);
                context.DrawImage(rawFrame, rect);
            }
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);

            var result = new WriteableBitmap(bitmap);

            if (result.CanFreeze && !result.IsFrozen)
                result.Freeze();
            return result;
        }

        public TimeSpan GetTotalDuration()
        {
            GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri(gifpath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            var totalDuration = TimeSpan.Zero;
            if (decoder != null && decoder.Frames.Count > 0)
            {
                for (int i = 0; i < decoder.Frames.Count; i++)
                {
                    var metadata = GetFrameMetadata(decoder.Frames[i]);
                    totalDuration += metadata.Delay;
                }
            }
            return totalDuration;
        }

        public (List<BitmapSource> BitmapSources, List<TimeSpan> TimeSpans) GetAllFrame()
        {
            List<BitmapSource> bitmapSources = new List<BitmapSource>();
            List<TimeSpan> spans = new List<TimeSpan>();
            GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri(gifpath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            if (decoder != null && decoder.Frames.Count > 0)
            {
                int index = 0;
                BitmapSource baseFrame = null;
                foreach (var rawFrame in decoder.Frames)
                {
                    var metadata = GetFrameMetadata(decoder.Frames[index]);
                    int width = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Width", 0);
                    int height = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Height", 0);
                    var frame = MakeFrame(width, height, rawFrame, metadata, baseFrame);
                    baseFrame = frame;
                    bitmapSources.Add(frame);
                    spans.Add(metadata.Delay);
                    index++;
                }
            }
            return (bitmapSources, spans);
        }

        public BitmapSource GetFirstFrame()
        {
            GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri(gifpath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BitmapSource result = null;
            if (decoder != null && decoder.Frames.Count > 0)
            {
                    var frame = decoder.Frames[0];
                    var metadata = GetFrameMetadata(frame);
                    int width = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Width", 0);
                    int height = decoder.Metadata.GetQueryOrDefault("/logscrdesc/Height", 0);
                    result = MakeFrame(width, height, frame, metadata, frame);
            }
            return result;
        }

        public GifBitmapDecoder GetDecoder()
        {
            return new GifBitmapDecoder(new Uri(gifpath), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        }
    }

    public class FrameMetadata
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TimeSpan Delay { get; set; }
        public FrameDisposalMethod DisposalMethod { get; set; }
    }

    public enum FrameDisposalMethod
    {
        None = 0,
        DoNotDispose = 1,
        RestoreBackground = 2,
        RestorePrevious = 3
    }
}
