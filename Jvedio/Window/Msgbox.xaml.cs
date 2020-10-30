using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Jvedio
{
    /// <summary>
    /// Msgbox.xaml 的交互逻辑
    /// </summary>
    public partial class Msgbox : Window
    {
        string Text;

        public event EventHandler CancelTask;

        public Msgbox(Window window, string text,bool waiting=false)
        {
            InitializeComponent();
            Text = text;

            TextBlock.Text = text;

            if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
            window.Activate();
            window.Focus();
            this.Left = window.Left;
            this.Top = window.Top;
            this.Height = window.Height;
            this.Width = window.Width;

            if (waiting)
            {
                YesButton.Visibility = Visibility.Collapsed;
                CancelButton.Content = "强制停止";
                CancelButton.Width = 125;
                WaitingImageAwesome.Visibility = Visibility.Visible;
            }



            
        }

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CancelTask?.Invoke(this, e);
            this.DialogResult = false;
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.DialogResult = true;
            else if (e.Key == Key.Escape)
                this.DialogResult = false;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            TextBlock.Focus();

        }
    }

    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double height = 200;
            double.TryParse(value.ToString(), out height);

            if (height > 500) height = 500;
            return height+80;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
