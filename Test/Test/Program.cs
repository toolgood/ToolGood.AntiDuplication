using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ToolGood.ReadyGo3;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var domain = "http://localhost:9988";
            //var domain = "http://localhost:60350";

            List<string> list = new List<string>();
            for (int i = 0; i < 1000; i++) {
                for (int j = 0; j < 4; j++) {
                    list.Add(i.ToString());
                }
            }
            var helper = SqlHelperFactory.OpenFormConnStr("Writer");

            Console.Write("通过select防重复:");
            CallWeb(list, domain + "/test/test1/", helper);

            Console.Write("通过redis锁防重复:");
            CallWeb(list, domain + "/test/test2/", helper);

            Console.Write("通过AntiDupCache防重复:");
            CallWeb(list, domain + "/test/test3/", helper);

            Console.Write("通过AntiDupQueue防重复:");
            CallWeb(list, domain + "/test/test4/", helper);

            Console.ReadKey();
        }

        static void CallWeb(List<string> list, string url, SqlHelper helper)
        {
            helper.Execute("TRUNCATE TABLE Test_Insert");
            var stopwatch = Stopwatch.StartNew();
            int errorCount = 0;
            Parallel.ForEach(list, (str) => {
                WebClient webClient = new WebClient();
                var html = webClient.DownloadString(url + str);
                webClient.Dispose();
                if (html != str) {
                    errorCount++;
                }
            });
            stopwatch.Stop();
            Console.Write(stopwatch.ElapsedMilliseconds + "ms\r\n");
            Console.WriteLine("插入个数：" + helper.First<int>("select count(*) from Test_Insert").ToString());
            if (errorCount > 0) {
                Console.WriteLine("错误次数：" + errorCount.ToString());
            }
        }

    }
}
