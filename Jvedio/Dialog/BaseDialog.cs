
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static Jvedio.GlobalVariable;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;

namespace Jvedio
{

    public  class BaseDialog : Window
    {

        public BaseDialog(Window owner)
        {
            this.Style = (Style)App.Current.Resources["BaseDialogStyle"];
            this.Loaded += delegate { InitEvent(); };//初始化载入事件
            this.Owner = owner;
            this.Width = SystemParameters.WorkArea.Width * 0.4;
            this.Height = SystemParameters.WorkArea.Height * 0.4;
        }



        public BaseDialog(Window owner,double width,double height):this(owner)
        {
            this.Width = width;
            this.Height = height;
        }



        private void InitEvent()
        {
            ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];

            Button closeBtn = (Button)baseDialogControlTemplate.FindName("BorderClose", this);
            closeBtn.Click += delegate (object sender, RoutedEventArgs e)
            {
                FadeOut();
            };


            Button cancel = (Button)baseDialogControlTemplate.FindName("CancelButton", this);
            cancel.Click += delegate (object sender, RoutedEventArgs e)
            {
                this.DialogResult = false;
            };

            Button confirm = (Button)baseDialogControlTemplate.FindName("ConfirmButton", this);
            confirm.Click += Confirm;

            Border borderTitle = (Border)baseDialogControlTemplate.FindName("BorderTitle", this);
            borderTitle.MouseMove += MoveWindow;
            FadeIn();
        }


        protected virtual void Confirm(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        protected override void OnActivated(EventArgs e)
        {
            IsFlashing = false;
            ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];
            Border border = (Border)baseDialogControlTemplate.FindName("MainBorder", this);
            DropShadowEffect dropShadowEffect = new DropShadowEffect() { Color = Colors.SkyBlue, BlurRadius = 20, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };
            border.Effect = dropShadowEffect;
            base.OnActivated(e);
        }


        bool IsFlashing = false;

        protected async override void OnDeactivated(EventArgs e)
        {
            if (IsFlashing) return;
            IsFlashing = true;
            //边缘闪动
            ControlTemplate baseDialogControlTemplate = (ControlTemplate)App.Current.Resources["BaseDialogControlTemplate"];
            Border border = (Border)baseDialogControlTemplate.FindName("MainBorder", this);
            DropShadowEffect dropShadowEffect1 = new DropShadowEffect() {Color=Colors.Red,BlurRadius= 20, Direction=-90,RenderingBias=RenderingBias.Quality,ShadowDepth=0 };
            DropShadowEffect dropShadowEffect2 = new DropShadowEffect() { Color = Colors.SkyBlue, BlurRadius = 20, Direction = -90, RenderingBias = RenderingBias.Quality, ShadowDepth = 0 };


            for (int i = 0; i <3; i++)
            {
                if (!IsFlashing) break;
                border.Effect = dropShadowEffect1;
                await Task.Delay(100);
                border.Effect = dropShadowEffect2;
                await Task.Delay(100);
            }
            if(IsFlashing) border.Effect = dropShadowEffect1;
            IsFlashing = false;
            base.OnDeactivated(e);
        }


        public async void FadeIn()
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                this.Opacity = 0;
                double opacity = this.Opacity;
                await Task.Run(() => {
                    while (opacity < 0.5)
                    {
                        this.Dispatcher.Invoke((Action)delegate { this.Opacity += 0.05; opacity = this.Opacity; });
                        Task.Delay(1).Wait();
                    }
                });
            }
            this.Opacity = 1;
        }

        public async void FadeOut()
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                double opacity = this.Opacity;
                await Task.Run(() => {
                    while (opacity > 0.1)
                    {
                        this.Dispatcher.Invoke((Action)delegate { this.Opacity -= 0.05; opacity = this.Opacity; });
                        Task.Delay(1).Wait();
                    }
                });
                this.Opacity = 0;
            }

            this.DialogResult = false;
            this.Close();
        }



        private void MoveWindow(object sender, MouseEventArgs e)
        {
            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }


    public class JvedioDialogResult
    {
        public string Text { get; set; }
        public int Option { get; set; }


        public JvedioDialogResult(string text,int option){
            this.Text = text;
            this.Option = option;
        }


    }




}
