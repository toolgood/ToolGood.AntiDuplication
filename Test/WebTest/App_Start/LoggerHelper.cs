using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SecondPartyManage.BaseCodes
{
    public static class LoggerHelper
    {
        private readonly static FileLoggerQueue errorLogger = new FileLoggerQueue(10000, "/logs/error/error-{yyyy}{MM}{dd}.log");
        private readonly static FileLoggerQueue infoLogger = new FileLoggerQueue(10000, "/logs/info/info-{yyyy}{MM}{dd}.log");
        private readonly static FileLoggerQueue debugLogger = new FileLoggerQueue(10000, "/logs/debug/debug-{yyyy}{MM}{dd}.log");
        private readonly static FileLoggerQueue fatalLogger = new FileLoggerQueue(10000, "/logs/fatal/fatal-{yyyy}{MM}{dd}.log");
        private readonly static FileLoggerQueue warnLogger = new FileLoggerQueue(10000, "/logs/warn/warn-{yyyy}{MM}{dd}.log");
        private readonly static FileLoggerQueue traceLogger = new FileLoggerQueue(10000, "/logs/trace/trace-{yyyy}{MM}{dd}.log");


        public static void Error(string content)
        {
            var ip = GetWebClientIp();
            var request = HttpContext.Current.Request;
            var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{ip}|{request.HttpMethod}|{request.Url}\r\n{content}\r\n\r\n";
            errorLogger.EnqueueMessage(msg);
        }
        public static void Info(string content)
        {
            var ip = GetWebClientIp();
            var request = HttpContext.Current.Request;
            var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{ip}|{request.HttpMethod}|{request.Url}\r\n{content}\r\n\r\n";
            infoLogger.EnqueueMessage(msg);
        }
        public static void Debug(string content)
        {
            var ip = GetWebClientIp();
            var request = HttpContext.Current.Request;
            var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{ip}|{request.HttpMethod}|{request.Url}\r\n{content}\r\n\r\n";
            debugLogger.EnqueueMessage(msg);
        }
        public static void Fatal(string content)
        {
            var ip = GetWebClientIp();
            var request = HttpContext.Current.Request;
            var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{ip}|{request.HttpMethod}|{request.Url}\r\n{content}\r\n\r\n";
            fatalLogger.EnqueueMessage(msg);
        }
        public static void Warn(string content)
        {
            var ip = GetWebClientIp();
            var request = HttpContext.Current.Request;
            var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{ip}|{request.HttpMethod}|{request.Url}\r\n{content}\r\n\r\n";
            warnLogger.EnqueueMessage(msg);
        }
        public static void Trace(string content)
        {
            var ip = GetWebClientIp();
            var request = HttpContext.Current.Request;
            var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}|{ip}|{request.HttpMethod}|{request.Url}\r\n{content}\r\n\r\n";
            traceLogger.EnqueueMessage(msg);
        }

        class FileLoggerQueue
        {
            private ConcurrentQueue<string> _writeQueue = new ConcurrentQueue<string>();
            private int _isInProcessMessage = 0;
            private readonly string _filePath;
            private readonly int _maxWriteCount;


            public FileLoggerQueue(int maxWriteCount, string filePath)
            {
                _maxWriteCount = maxWriteCount;
                _filePath = filePath;
            }

            public void EnqueueMessage(string info)
            {
                _writeQueue.Enqueue(info);
                ProcessMessage();
                if (_writeQueue.Count >= _maxWriteCount) Thread.Sleep(1);
            }

            private void ProcessMessage()
            {
                bool flag = Interlocked.CompareExchange(ref _isInProcessMessage, 1, 0) == 0;
                if (flag == false) return;

                Task.Factory.StartNew(() => {
                    try {
                        if (_writeQueue.IsEmpty) return;

                        var filePath = GetFullPath(_filePath, DateTime.Now);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        var fs = File.OpenWrite(filePath);

                        fs.Seek(0, SeekOrigin.End);
                        while (_writeQueue.TryDequeue(out string info)) {
                            var bytes = Encoding.UTF8.GetBytes(info);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                        fs.Close();
                    } finally {
                        Interlocked.Exchange(ref _isInProcessMessage, 0);
                        if (!_writeQueue.IsEmpty) ProcessMessage();
                    }
                });
            }

            private string GetFullPath(string filePath, DateTime dateTime)
            {
                if (filePath.Contains("{")) {
                    var invalidPattern = new Regex(@"[\*\?\042\<\>\|]", RegexOptions.Compiled);
                    filePath = invalidPattern.Replace(filePath, "");
                    StringBuilder file = new StringBuilder(filePath);

                    file.Replace("{time}", (dateTime.Ticks / 1000).ToString());
                    file.Replace("{yyyy}", dateTime.Year.ToString());
                    file.Replace("{yy}", (dateTime.Year % 100).ToString("D2"));
                    file.Replace("{MM}", dateTime.Month.ToString("D2"));
                    file.Replace("{dd}", dateTime.Day.ToString("D2"));
                    file.Replace("{HH}", dateTime.Hour.ToString("D2"));
                    file.Replace("{hh}", dateTime.Hour.ToString("D2"));
                    file.Replace("{mm}", dateTime.Minute.ToString("D2"));
                    file.Replace("{ss}", dateTime.Second.ToString("D2"));
                    filePath = file.ToString();
                }
                return System.Web.Hosting.HostingEnvironment.MapPath(filePath);
                //return Path.GetFullPath(filePath);
            }


        }


        /// <summary>
        /// 获取web客户端ip
        /// </summary>
        /// <returns></returns>
        private static string GetWebClientIp()
        {

            string userIP = "未获取用户IP";

            try {
                if (System.Web.HttpContext.Current == null
                 || System.Web.HttpContext.Current.Request == null
                 || System.Web.HttpContext.Current.Request.ServerVariables == null) {
                    return "";
                }

                string CustomerIP = "";

                //CDN加速后取到的IP simone 090805
                CustomerIP = System.Web.HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP)) {
                    return CustomerIP;
                }

                CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!String.IsNullOrEmpty(CustomerIP)) {
                    return CustomerIP;
                }

                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null) {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                    if (CustomerIP == null) {
                        CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                } else {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                if (string.Compare(CustomerIP, "unknown", true) == 0 || String.IsNullOrEmpty(CustomerIP)) {
                    return System.Web.HttpContext.Current.Request.UserHostAddress;
                }
                return CustomerIP;
            } catch { }

            return userIP;

        }
    }

}