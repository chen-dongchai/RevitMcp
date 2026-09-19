using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revits.Infrastructure
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logPath;

        static Logger()
        {
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "RevitMcp",
                    "logs");

                Directory.CreateDirectory(dir);

                _logPath = Path.Combine(dir, "revit-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
            }
            catch
            {
                // 静态构造失败，降级到临时目录
                try
                {
                    string fallback = Path.Combine(Path.GetTempPath(), "RevitMcp");
                    Directory.CreateDirectory(fallback);
                    _logPath = Path.Combine(fallback, "revit-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                }
                catch
                {
                    _logPath = null;
                }
            }
        }

        public static void Info(string msg) { Write("INFO", msg); }
        public static void Warn(string msg) { Write("WARN", msg); }
        public static void Error(string msg, Exception ex = null)
        {
            Write("ERROR", ex == null ? msg : msg + "\n" + ex);
        }

        private static void Write(string level, string msg)
        {
            if (_logPath == null) return;

            try
            {
                string line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] [" + level + "] " + msg + "\n";
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line);
                }
            }
            catch { }
        }
    }
}
