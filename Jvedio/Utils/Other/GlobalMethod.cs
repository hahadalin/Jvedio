using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static Jvedio.GlobalVariable;

namespace Jvedio
{
    public static class GlobalMethod
    {
        public static Window GetWindowByName(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }
            return null;
        }




    }

    public static class ClipBoard {
        public static bool TrySetDataObject(object o, string token, bool showsuccess = true)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetDataObject(o, false, 5, 200);
                if (showsuccess)
                    HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.HasCopy, token);

                return true;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, token);
                return false;
            }
        }

        public static bool TrySetFileDropList(StringCollection filePaths, string token, bool showsuccess = true)
        {
            try
            {
                System.Windows.Clipboard.Clear();
                System.Windows.Clipboard.SetFileDropList(filePaths);
                if (showsuccess)
                    HandyControl.Controls.Growl.Success(Jvedio.Language.Resources.HasCopy, token);
                return true;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, token);
                return false;
            }
        }

    }



    public static class FileHelper
    {
        public static bool TryOpenUrl(string url,string token="")
        {
            try
            {
                if (url.IsProperUrl())
                {
                    Process.Start(url);
                    return true;
                }
                else
                {
                    if (token != "") HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.ErrorUrl, token);
                    return false;
                }
                
            }
            catch(Exception ex)
            {
                if (token!="") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogE(ex);
                return false;
            } 
        }

        public static bool TryOpenPath(string path, string token="")
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                    return true;
                }
                else
                {
                    if (token != "") HandyControl.Controls.Growl.Error(Jvedio.Language.Resources.NotExists, token);
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenSelectPath(string path, string token="")
        {
            try
            {
                if (File.Exists(path))
                {
                    Process.Start("explorer.exe", "/select, \"" + path + "\"");
                    return true;
                }
                    
                else
                {
                    if (token != "") HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{path}", token);
                    return false;
                }
                    
            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenFile(string filename, string token="")
        {
            try
            {
                if (File.Exists(filename))
                {
                    Process.Start("\"" + filename + "\"");
                    return true;
                }

                else
                {
                    if(token!="") HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{filename}", token);
                    return false;
                }
                    
                
            }
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }

        public static bool TryOpenFile(string processPath,string filename,string token)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Process.Start("\"" + filename + "\"");
                    return true;
                }
                    
                else
                {
                    HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{filename}", token);
                    return false;
                }
  
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogF(ex);
                return false;
            }
        }
    }

}
