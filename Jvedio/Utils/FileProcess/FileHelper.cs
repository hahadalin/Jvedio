using System;

using System.Diagnostics;
using System.IO;


namespace Jvedio
{
    public static class FileHelper
    {
        public static bool TryOpenUrl(string url, string token = "")
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
            catch (Exception ex)
            {
                if (token != "") HandyControl.Controls.Growl.Error(ex.Message, token);
                Logger.LogE(ex);
                return false;
            }
        }

        public static bool TryOpenPath(string path, string token = "")
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

        public static bool TryOpenSelectPath(string path, string token = "")
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

        public static bool TryOpenFile(string filename, string token = "")
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
                    if (token != "") HandyControl.Controls.Growl.Error($"{Jvedio.Language.Resources.Message_FileNotExist}：{filename}", token);
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

        public static bool TryOpenFile(string processPath, string filename, string token)
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
