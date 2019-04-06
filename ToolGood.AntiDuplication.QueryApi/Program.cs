using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace ToolGood.AntiDuplication.QueryApi
{
    class Program
    {
        private static AntiDupCache<string, int> antiDupCache = new AntiDupCache<string, int>(50, 1);
        //private static AntiDupQueue<string, int> antiDupCache = new AntiDupQueue<string, int>(50);


        static void Main(string[] args)
        {
            try {
                var helper = Config.MySqlHelper;
                helper.Execute("TRUNCATE TABLE Users");
                //helper._TableHelper.CreateTable(typeof(DbUser));
                helper.Dispose();
            } catch (Exception ex) { }

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 7, (i) => {
                if (i % 2 == 1) {
                    for (int j = 0; j < 100; j++) {
                        UserModel model = new UserModel() {
                            Name = j.ToString(),
                            Phone = j.ToString()
                        };
                        var id = Insert_2(model);
                        var id2 = Insert_2(model);
                        Console.WriteLine(i + "\t" + j + "\t" + id2);
                    }
                } else {
                    for (int j = 100 - 1; j >= 0; j--) {
                        UserModel model = new UserModel() {
                            Name = j.ToString(),
                            Phone = j.ToString()
                        };
                        var id = Insert_2(model);
                        var id2 = Insert_2(model);
                        Console.WriteLine(i + "\t" + j + "\t" + id2);
                    }
                }

            });


            //Parallel.For(0, 1000, /*new ParallelOptions() { MaxDegreeOfParallelism = 8 },*/ (j) => {
            //    UserModel model = new UserModel() {
            //        Name = j.ToString(),
            //        Phone = j.ToString()
            //    };
            //    var id = Insert_2(model);
            //    var id2 = Insert_2(model);
            //    Console.WriteLine(id2);
            //});
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            Console.ReadLine();

        }

        static long Test1()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //Parallel.For(0, 5, (i) => {
            var url = "http://localhost:51002/api/values/Insert_1?Phone={0}&Name={1}";

            for (int j = 0; j < 1000; j++) {
                var u = string.Format(url, j, j);
                var id = webClient.DownloadString(u);
                //Console.WriteLine(i + "\t" + id);
                Console.WriteLine(id);

            }
            //});
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;
        }
        static long Test2()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //Parallel.For(0, 5, (i) => {
            var url = "http://localhost:51002/api/values/Insert_1?Phone={0}&Name={1}";


            for (int j = 0; j < 1000; j++) {
                var u = string.Format(url, j, j);
                var id = webClient.DownloadString(u);

            }
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;
        }
        static long Test3()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //Parallel.For(0, 5, (i) => {
            var url = "http://localhost:51002/api/values/Insert_2?Phone={0}&Name={1}";

            for (int j = 0; j < 1000; j++) {
                var u = string.Format(url, j, j);
                var id = webClient.DownloadString(u);
                //Console.WriteLine(i + "\t" + id);
                Console.WriteLine(id);

            }
            //});
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;
        }
        static long Test4()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 1000, (j) => {
                WebClient webClient2 = new WebClient();

                var url = "http://localhost:51002/api/values/Insert_1?Phone={0}&Name={1}";

                var u = string.Format(url, j, j);
                var id = webClient2.DownloadString(u);
                //Console.WriteLine(i + "\t" + id);
                Console.WriteLine(id);


            });
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;
        }
        static long Test5()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 1000, (j) => {
                WebClient webClient2 = new WebClient();

                var url = "http://localhost:51002/api/values/Insert_2?Phone={0}&Name={1}";

                var u = string.Format(url, j, j);
                var id = webClient2.DownloadString(u);
                //Console.WriteLine(i + "\t" + id);
                Console.WriteLine(id);


            });
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;
        }

        static long Test6()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 6, (j) => {
                List<int> list = new List<int>();
                Random random = new Random(Guid.NewGuid().GetHashCode());
                for (int k = 0; k < 1000; k++) {
                    list.Add(k);
                }

                WebClient webClient2 = new WebClient();
                var url = "http://localhost:51002/api/values/Insert_1?Phone={0}&Name={1}";
                foreach (var k in list) {
                    var u = string.Format(url, k, k);
                    var id = webClient2.DownloadString(u);
                    //Console.WriteLine(i + "\t" + id);
                    Console.WriteLine(id);
                }
            });
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;

        }
        static long Test7()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 6, (j) => {
                List<int> list = new List<int>();
                Random random = new Random(Guid.NewGuid().GetHashCode());
                for (int k = 0; k < 1000; k++) {
                    list.Add(k);
                }

                WebClient webClient2 = new WebClient();
                var url = "http://localhost:51002/api/values/Insert_2?Phone={0}&Name={1}";
                foreach (var k in list) {
                    var u = string.Format(url, k, k);
                    var id = webClient2.DownloadString(u);
                    //Console.WriteLine(i + "\t" + id);
                    Console.WriteLine(id);
                }
            });
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;

        }

        static long Test5_1()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadString("http://localhost:51002/api/values/clear");

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, 1000, (j) => {
                WebClient webClient2 = new WebClient();

                var url = "http://localhost:51002/api/values/Insert_2?Phone={0}&Name={1}";

                var u = string.Format(url, j, j);
                var id = webClient2.DownloadString(u);
                var id2 = webClient2.DownloadString(u);
                //Console.WriteLine(i + "\t" + id);
                Console.WriteLine(id2);


            });
            stopwatch.Stop();
            Console.WriteLine("共花了时间" + stopwatch.ElapsedMilliseconds + "ms");

            return stopwatch.ElapsedMilliseconds;
        }



        public static int Insert_2(UserModel model)
        {
            var id = antiDupCache.Execute(model.Phone, () => {
                using (var helper = Config.MySqlHelper) {
                    var db = helper.FirstOrDefault<DbUser>("where Phone=@0", model.Phone);
                    if (db == null) {
                        DbUser user = new DbUser() {
                            Name = model.Name,
                            Phone = model.Phone
                        };
                        helper.Insert(user);
                        return user.Id;
                    } else {
                        return db.Id;
                    }
                }
            });
            return id;
        }
        public static int Insert_1(UserModel model)
        {
            int id = 0;
            using (var helper = Config.MySqlHelper) {
                var db = helper.FirstOrDefault<DbUser>("where Phone=@0", model.Phone);
                if (db == null) {
                    DbUser user = new DbUser() {
                        Name = model.Name,
                        Phone = model.Phone
                    };
                    helper.Insert(user);
                    id = user.Id;
                } else {
                    id = db.Id;
                }
            }
            return id;
        }


    }
}

