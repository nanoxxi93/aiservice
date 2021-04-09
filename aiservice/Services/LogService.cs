using System;
using System.IO;
using System.Text.RegularExpressions;
using AIService.Entities;

namespace AIService.Logs
{
    public class Log
    {
        public static void Write(AppSettings appSettings, string level, string label, string Class, string method, string msg, string path = "logs")
        {
            try
            {
                LogPaths settings = new LogPaths();
                if (appSettings.LogPaths.Exists(x => x.Label == label))
                {
                    settings = appSettings.LogPaths.Find(x => x.Label == label).Parameters;
                }
                else
                {
                    settings = appSettings.LogPaths.Find(x => x.Label == "Default").Parameters;
                }

                if (settings.Enabled &&
                    (level == LogEnum.DEBUG.ToString() && appSettings.LogSettings.Debug
                    || level == LogEnum.INFO.ToString() && appSettings.LogSettings.Info
                    || level == LogEnum.ERROR.ToString() && appSettings.LogSettings.Error))
                {
                    msg = Regex.Replace(msg, "\n", "");
                    msg = Regex.Replace(msg, "\r", "");
                    msg = Regex.Replace(msg, "    ", " ");
                    msg = Regex.Replace(msg, "   ", " ");
                    msg = Regex.Replace(msg, "  ", " ");

                    if (Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)) == false)
                    {
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), path));
                    }
                    string pathFile = Path.Combine(Directory.GetCurrentDirectory(), path, DateTime.Now.ToString("yyyyMMdd") + "." + label + "." + Class + ".log");

                    if (File.Exists(pathFile))
                    {
                        using (StreamWriter sw = File.AppendText(pathFile))
                        {
                            if (method.Split(new String[] { "<" }, StringSplitOptions.None).Length > 1)
                                sw.WriteLine(DateTime.Now.ToString("yyyyMMdd") + "." + DateTime.Now.ToString("HHmmss") + $" {level} " + method.Split(new String[] { "<" }, StringSplitOptions.None)[1].Split(">")[0].Trim() + " --> " + msg);
                            else
                                sw.WriteLine(DateTime.Now.ToString("yyyyMMdd") + "." + DateTime.Now.ToString("HHmmss") + $" {level} " + method + " --> " + msg);
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = File.CreateText(pathFile))
                        {
                            if (method.Split(new String[] { "<" }, StringSplitOptions.None).Length > 1)
                                sw.WriteLine(DateTime.Now.ToString("yyyyMMdd") + "." + DateTime.Now.ToString("HHmmss") + $" {level} " + method.Split(new String[] { "<" }, StringSplitOptions.None)[1].Split(">")[0].Trim() + " --> " + msg);
                            else
                                sw.WriteLine(DateTime.Now.ToString("yyyyMMdd") + "." + DateTime.Now.ToString("HHmmss") + $" {level} " + method + " --> " + msg);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
