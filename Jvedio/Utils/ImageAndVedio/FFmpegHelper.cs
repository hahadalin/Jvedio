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
        public string Value { get; set; }
        private int timeout ;
        public FFmpegHelper(string value,int timeoutsecond=0)
        {
           if(!value.EndsWith("&exit")) Value = value + "&exit";
            if (timeoutsecond <= 0)
            {
                timeout = GlobalVariable.MaxProcessWaitingSecond * 1000;
            }
            else
            {
                timeout = timeoutsecond * 1000;
            }
            
        }

        public async Task<string> Run()
        {
            return await Task.Run(() => { 
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
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.StandardInput.WriteLine(Value);
                    process.StandardInput.AutoFlush = true;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout))
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
                return error.ToString();
            }
            });
        }

    }
}
