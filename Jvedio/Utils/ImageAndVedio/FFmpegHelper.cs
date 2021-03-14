using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Utils.ImageAndVedio
{
    public class FFmpegHelper
    {
        private string ProcessParameters = "";
        private int Timeout=0;
        public FFmpegHelper(string processParameters, int timeoutsecond = 0)
        {
            if (!processParameters.EndsWith("&exit")) ProcessParameters = processParameters + "&exit";
            if (timeoutsecond <= 0)
                Timeout = GlobalVariable.MaxProcessWaitingSecond * 1000;
            else
                Timeout = timeoutsecond * 1000;
        }


        //TODO
        public async Task<string> Run()
        {
            return await Task.Run(() =>
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    //process.StartInfo.Arguments = arguments;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    {
                        using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                        {
                            process.OutputDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    try
                                    {
                                        errorWaitHandle.Set();
                                    }
                                    catch (ObjectDisposedException ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                else
                                    output.AppendLine(e.Data);
                            };
                            process.ErrorDataReceived += (sender, e) =>
                            {
                                if (e.Data == null)
                                {
                                    try
                                    {
                                        errorWaitHandle.Set();
                                    }
                                    catch (ObjectDisposedException ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                else
                                    error.AppendLine(e.Data);
                            };

                            process.Start();
                            process.StandardInput.WriteLine(ProcessParameters);
                            process.StandardInput.AutoFlush = true;
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();

                            if (process.WaitForExit(Timeout) && outputWaitHandle.WaitOne(Timeout) && errorWaitHandle.WaitOne(Timeout))
                            {
                                // Process completed. Check process.ExitCode here.
                                process.Close();
                            }
                            else
                            {
                                // Timed out.
                                error.AppendLine(Jvedio.Language.Resources.TimeOut_Process);
                            }
                        }
                    }

                    return error.ToString();
                }
            });
        }

    }
}
