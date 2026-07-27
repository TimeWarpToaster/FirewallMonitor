//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FWM_ClientViewer_03
{
    public static class L
    {
        public const string CLASSNAME = "L";


        public static string logPath = @"";
        public static int cntLogHelper = 0;

        public static RichTextBox rtbLog = null;
        public static ListBox lbLog = null;

        public static bool isFileLogging = true;

        public static long clearLogs()
        {
            const string location = CLASSNAME + ".clearLogs";
            long retVal = 0;
            try
            {
                // If for some reason rtb and lb are both active, take ListBox count for return value
                long temp = 0;
                if (rtbLog != null)
                {
                    temp = rtbLog.Text.Length;
                    rtbLog.Text = "";
                }

                if (lbLog != null)
                {
                    temp = lbLog.Items.Count;
                    lbLog.Items.Clear();
                }
                retVal = temp;
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
            return retVal;
        }

        public static bool logInit(string _logPath, bool _isFileLogging)
        {
            bool retValue = false;
            try
            {
                if (_logPath != null /* && _logPath.Length > 0 */)
                {
                    L.logPath = _logPath;
                    retValue = true;
                }
                L.isFileLogging = _isFileLogging;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Form1.logInit(lb) - Exception - Failed to initialize log UI!");
            }
            return retValue;
        }

        public static bool logInit(string _logPath, ListBox inLbLog, bool _isFileLogging)
        {
            bool retValue = false;
            try
            {
                // Nothing says the log path can't be empty. Must set both for success.
                if (L.logInit(_logPath, _isFileLogging) && inLbLog != null)
                {
                    L.lbLog = inLbLog;
                    retValue = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Form1.logInit(lb) - Exception - Failed to initialize log UI!");
            }
            return retValue;
        }

        public static bool logInit(string _logPath, RichTextBox inRtbLog, bool _isFileLogging)
        {
            bool retValue = false;
            try
            {
                if (L.logInit(_logPath, _isFileLogging) && inRtbLog != null)
                {
                    L.rtbLog = inRtbLog;
                    retValue = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Form1.logInit(rtb) - Exception - Failed to initialize log UI!");
            }
            return retValue;
        }

        private static void logger(string location, string msg)
        {
            L.logger(location, msg, TAG.GENERAL);
        }

        private static void logger(string location, string msg, string grade)
        {
            try
            {
                if (msg.Length > 3000)
                {
                    int idx = 0;
                    while (idx <= msg.Length)
                    {
                        string temp = msg.Substring(idx, (idx + 3000 > msg.Length ? msg.Length : idx + 3000));

                        temp = DateTime.Now.ToString(TAG.DTF) + " - " + location + " - " + grade + " - " + (idx > 0 ? "... " : "") + temp;
                        Console.WriteLine(msg);
                        if (grade != TAG.DEBUG)
                        {
                            if (L.isFileLogging) L.logWriter(msg);
                            L.updateUI(msg);
                            cntLogHelper++;
                        }
                        idx += 3000;
                    }
                }
                else
                {
                    msg = DateTime.Now.ToString(TAG.DTF) + " - " + location + " - " + grade + " - " + msg;
                    Console.WriteLine(msg);
                    if (grade != TAG.DEBUG)
                    {
                        if (L.isFileLogging) L.logWriter(msg);
                        L.updateUI(msg);
                        cntLogHelper++;
                    }
                }
            }
            catch (Exception ex)
            {
                /* Do Nothing - Catch exception to prevent being kicked to engine */
            }
        }

        public static void d(string location, string msg) { if (c.debug) L.logger(location, msg, TAG.DEBUG); }
        public static void err(string location, string msg) { L.logger(location, msg, TAG.ERR); }
        public static void ex(string location, string msg) { L.logger(location, msg, TAG.EX); }
        public static void ex(string location, Exception ex) { if (ex != null && ex.Message.Length > 0) L.logger(location, ex.Message, TAG.EX); }
        public static void l(string location, string msg) { L.logger(location, msg, TAG.GENERAL); }


        public static void logWriter(string s)
        {
            try
            {
                if (L.isFileLogging)
                {
                    if (!File.Exists(logPath))
                    {
                        // Create a file to write to.
                        using (StreamWriter fs = File.CreateText(logPath))
                        {
                            fs.WriteLine(s);
                        }
                    }
                    else
                    {
                        using (StreamWriter fs = File.AppendText(logPath))
                        {
                            fs.WriteLine(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Form1.logWriter - Failed to write log file!");
            }
        }

        public static void logWriter(List<string> s)
        {
            try
            {
                if (L.isFileLogging) File.AppendAllLines(logPath, s);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Form1.logWriter - Failed to write log file!");
            }
        }



        public static void updateUI(string s)
        {
            const string location = CLASSNAME + ".updateUI";
            try
            {
                if (s != null && s.Length > 0)
                {
                    if (L.lbLog != null)
                    {
                        if (L.lbLog.InvokeRequired)
                        {
                            L.lbLog.Invoke(new Action(() => updateUI(s)));
                        }
                        else
                        {
                            L.lbLog.Items.Add(s);
                        }
                    }

                    if (L.rtbLog != null)
                    {
                        if (L.rtbLog.InvokeRequired)
                        {
                            L.rtbLog.Invoke(new Action(() => updateUI(s)));
                        }
                        else
                        {
                            L.rtbLog.Text += s + "\n";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                L.ex(location, ex);
            }
        }


    }
}
