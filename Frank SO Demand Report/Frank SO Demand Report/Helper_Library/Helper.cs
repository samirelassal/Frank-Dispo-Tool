using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Helper_Library
{
    public static class Helper
    {
        public static readonly string LogFile = System.AppDomain.CurrentDomain.BaseDirectory + @"\logfile.log";

        public static void LogFileWrite(string text)
        {
            FileStream fs = File.Open(LogFile, FileMode.Append, FileAccess.Write);
            string content = "[ " + System.DateTime.Now.ToString() + " ]: " + text + "\r\n";
            byte[] data = UTF8Encoding.UTF8.GetBytes(content);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
        public static void LogFileWrite(bool UseDate, string text)
        {
            FileStream fs = File.Open(LogFile, FileMode.Append, FileAccess.Write);
            string content = "";
            if (UseDate)
                content = "[ " + System.DateTime.Now.ToString() + " ]: ";
            content += text + "\r\n";
            byte[] data = UTF8Encoding.UTF8.GetBytes(content);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
        public static void LogFileWrite(string text, params string[] values)
        {
            FileStream fs = File.Open(LogFile, FileMode.Append, FileAccess.Write);
            string content = "[ " + System.DateTime.Now.ToString() + " ]: " + string.Format(text, values) + "\r\n";
            byte[] data = UTF8Encoding.UTF8.GetBytes(content);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
        public static void LogFileWrite(ConsoleColor color, bool UseDate, string text, params string[] values)
        {
            FileStream fs = File.Open(LogFile, FileMode.Append, FileAccess.Write);
            string content = "";
            if (UseDate)
                content = "[ " + System.DateTime.Now.ToString() + " ]: ";
            content += string.Format(text, values) + "\r\n";
            byte[] data = UTF8Encoding.UTF8.GetBytes(content);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }

        public static void LogWrite(string text)
        {
            Console.Write("[ " + System.DateTime.Now.ToString() + " ]: ");
            if (text != null)
                Console.WriteLine(text);
        }
        public static void LogWrite(ConsoleColor color, string text)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write("[ " + System.DateTime.Now.ToString() + " ]: ");
            if (text != null)
                Console.WriteLine(text);
            Console.ForegroundColor = c;
        }
        public static void LogWrite(ConsoleColor color, bool UseDate, string text)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (UseDate)
                Console.Write("[ " + System.DateTime.Now.ToString() + " ]: ");
            if (text != null)
                Console.WriteLine(text);
            Console.ForegroundColor = c;
        }
        public static void LogWrite(ConsoleColor color, string text, params string[] values)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write("[ " + System.DateTime.Now.ToString() + " ]: ");
            if (text != null)
                Console.WriteLine(text, values);
            Console.ForegroundColor = c;
        }
        public static void LogWrite(ConsoleColor color, bool UseDate, string text, params string[] values)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (UseDate)
                Console.Write("[ " + System.DateTime.Now.ToString() + " ]: ");
            if (text != null)
                Console.WriteLine(text, values);
            Console.ForegroundColor = c;
        }

        public static void ErrorMessage(Exception ex)
        {
            var r = ConsoleColor.Red;
            LogWrite(r, null);
            LogWrite(r, false, new string('=', 37) + "Error" + new string('=', 37));
            LogWrite(r, false, "Error Type:     {0}", ex.GetType().ToString());
            LogWrite(r, false, "Error Message:  {0}", ex.Message);
            LogWrite(r, false, "\nStack Trace:  {0}", ex.StackTrace);

            LogWrite(r, false, new string('=', 79));
        }
        public static void ErrorMessage(string message)
        {
            var r = ConsoleColor.Red;
            LogWrite(r, null);
            LogWrite(r, false, new string('=', 37) + "Error" + new string('=', 37));
            LogWrite(r, false, "Message:  {0}", message);
            LogWrite(r, false, new string('=', 79));
        }
        public static void ErrorMessage(string message, params string[] values)
        {
            var r = ConsoleColor.Red;
            LogWrite(r, null);
            LogWrite(r, false, new string('=', 37) + "Error" + new string('=', 37));
            LogWrite(r, false, message, values);
            LogWrite(r, false, new string('=', 79));
        }

        /// <summary>
        /// checks, if client version is compatible with server
        /// </summary>
        /// <param name="required_version">required version of client</param>
        /// <param name="actual_version">actual version of client</param>
        /// <returns></returns>
        public static bool CompareVersion(string required_version, string actual_version) 
        {
            bool compatible = true;
            string[] req_segments = required_version.Split('.');
            string[] act_segments = actual_version.Split('.');

            for (int i = 0; i < req_segments.Length && i < act_segments.Length; i++) 
            {
                int actual = int.Parse(act_segments[i]);
                int required = int.Parse(req_segments[i]);
                if (actual > required)
                    break;
                if (actual < required) 
                {
                    compatible = false;
                    break;
                }
            }

            return compatible;
        }
    }
}
