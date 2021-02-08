using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using static Jvedio.GlobalVariable;

namespace Jvedio
{


    

    public class OppositeBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class BiggerWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            double width = 0;
            double.TryParse(value.ToString(), out width);
            return width;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter==null) return 0;
            double.TryParse(value.ToString(), out double width);
            double.TryParse(parameter.ToString(), out double w);
            if (width + w > 0)
                return width + w;
            else
                return 0;
            

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }


    public class BoolToVisibilityConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return Visibility.Visible; else return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class IntToCheckedConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) { return false; }
            int intparameter = int.Parse(parameter.ToString());
            if ((int)value == intparameter)
                return true;
            else
                return false;
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) return 0;
            int intparameter = int.Parse(parameter.ToString());
            return intparameter;
        }


    }


    public class StringToCheckedConverter : IValueConverter
    {
        //判断是否相同
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == parameter.ToString()) { return true; } else { return false; }
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.ToString();
        }


    }








    public class ViewTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            ViewType myViewType = ViewType.默认;
            Enum.TryParse<ViewType>(value.ToString(), out myViewType);

            return myViewType.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(ViewType), parameter.ToString(), true) : null;
        }
    }





    public class WidthToMarginConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || double.Parse(value.ToString()) <= 0) return
                      150;
            else
                return double.Parse(value.ToString()) - 40;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToUriStringConverterMain : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "黑色")
                return $"Resources/Skin/black/{parameter}.png";
            else if (value.ToString() == "白色")
                return $"Resources/Skin/white/{parameter}.png";
            else if (value.ToString() == "蓝色")
                return $"Resources/Skin/black/{parameter}.png";
            else
                return $"Resources/Skin/black/{parameter}.png";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToUriStringConverterOther : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "黑色")
                return $"pack://application:,,,/Resources/Skin/black/{parameter}.png";
            else if (value.ToString() == "白色")
                return $"pack://application:,,,/Resources/Skin/white/{parameter}.png";
            else if (value.ToString() == "蓝色")
                return $"pack://application:,,,/Resources/Skin/black/{parameter}.png";
            else
                return $"pack://application:,,,/Resources/Skin/black/{parameter}.png";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ImageTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return (((MyImageType)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MyImageType), parameter.ToString(), true) : null;
        }
    }




    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "2")
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }







    public class MovieStampTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Properties.Settings.Default.DisplayStamp) return Visibility.Hidden;

            if (value == null)
            {
                return Visibility.Hidden;
            }
            else
            {
                MovieStampType movieStampType = (MovieStampType)value;
                if (movieStampType == MovieStampType.无)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }


            }



        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
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




    public class CloseEventArgs : EventArgs
    {
        public bool IsExitApp = true;
    }

    public static class GetBounds
    {
        public static Rect BoundsRelativeTo(this FrameworkElement element, Visual relativeTo)
        {
            return element.TransformToVisual(relativeTo).TransformBounds(new Rect(element.RenderSize));
        }

        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }


    }



    public class PlusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == " + ")
                return Visibility.Collapsed;
            else
                return Visibility.Visible;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class LabelToListConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return " + ";
            List<string> result = value.ToString().Split(' ').ToList();
            result.Insert(0, " + ");
            return result;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            List<string> vs = value as List<string>;
            vs.Remove(" + ");
            return string.Join(" ", vs);
        }
    }

    public class TagStampsConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            if (value.ToString().IndexOf(parameter.ToString().ToTagString()) >= 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null || value.ToString() == "") return "微软雅黑";

            return value.ToString();

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    //setings




    public class SkinStringToCheckedConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return false;


            if (value.ToString() == parameter.ToString())
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null) return null;
            if ((bool)value)
                return parameter.ToString();
            else
                return null;
        }
    }



    public class SkinTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            return (((Skin)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(Skin), parameter.ToString(), true) : null;
        }
    }

    public class SmallerValueConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter==null) return 0;
            int.TryParse(value.ToString(), out int w1);
            int.TryParse(parameter.ToString(), out int w2);
            if (w1 - w2 < 0) return 0;
            else return w1 - w2;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToVedioTypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Jvedio.Language.Resources.Uncensored;

            int.TryParse(value.ToString(), out int vt);
            if (vt == 1)
            {
                return Jvedio.Language.Resources.Uncensored;
            }
            else if (vt == 2)
            {
                return Jvedio.Language.Resources.Censored;
            }
            else if (vt == 3)
            {
                return Jvedio.Language.Resources.Europe;
            }
            return Jvedio.Language.Resources.All;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }



    public class LanguageTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;
            return (((MyLanguage)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MyLanguage), parameter.ToString(), true) : null;
        }
    }


    public class MultiIntToMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int padUD = 5;
            int padLR = 5;
            int.TryParse(values[0].ToString(), out padUD);
            int.TryParse(values[1].ToString(), out padLR);

            return new Thickness(padLR, padUD, padLR, padUD);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
            {
                if (parameter.ToString() == "BorderBrush")
                    return Brushes.Gold;
                else if (parameter.ToString() == "Background")
                    return Brushes.LightGreen;
            }

            if (parameter.ToString() == "BorderBrush")
                return Brushes.Gold;
            else
                return Brushes.LightGreen;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter.ToString() == "BorderBrush")
                    return System.Drawing.Color.Gold;
                else if (parameter.ToString() == "Background")
                    return System.Drawing.Color.LightGreen;
            }

            return "#123455";



        }
    }


    public class FontFamilyToSelectedIndexConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return 0;

            int i = -1;
            foreach (FontFamily item in Fonts.SystemFontFamilies)
            {
                i++;
                if (item.ToString() == value.ToString()) return i;
            }
            return 0;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class BoolToImageStretchConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return Stretch.Uniform;

            if ((bool)value)
                return Stretch.UniformToFill;
            else
                return Stretch.Uniform;


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }



    public class BoolToFontBoldConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return FontWeights.Normal;

            if ((bool)value)
                return FontWeights.Bold;
            else
                return FontWeights.Normal;


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }



    public class BoolToFontItalicConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return FontStyles.Normal;

            if ((bool)value)
                return FontStyles.Italic;
            else
                return FontStyles.Normal;


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }


    public class BoolToUnderLineConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null)
                return "";

            if ((bool)value)
                return "Underline";
            else
                return "";


        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }



    //Edit
    public class BitToGBConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "0 GB";

            //保留2位小数
            double.TryParse(value.ToString(), out double filesize);

            filesize = filesize / 1024 / 1024 / 1024;//GB
            if (filesize >= 0.9)
                return $"{Math.Round(filesize, 2)} GB";//GB
            else
                return $"{Math.Ceiling(filesize * 1024)} MB";//MB
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    //Detail


    public class PlotToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null) return Visibility.Hidden;
            if (value.ToString() == "" | string.IsNullOrEmpty(value.ToString()))
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }








}
