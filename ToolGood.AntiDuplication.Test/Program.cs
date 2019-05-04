using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolGood.AntiDuplication.QueryApi;

namespace ToolGood.AntiDuplication.Test
{
    class Program
    {
        private readonly static AntiDupCache<int, int> antiDupCache = new AntiDupCache<int, int>(50, 1);
        private readonly static AntiDupQueue<int, int> antiDupQueue = new AntiDupQueue<int, int>(50);
        private readonly static DictCache<int, int> dictCache = new DictCache<int, int>();
        private readonly static Cache<int, int> cache = new Cache<int, int>();


        static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            Test2(10000, processorCount);

            Test3(1000, processorCount);

            for (int count = 1; count <= processorCount; count++) {
                Test(count, processorCount);
            }

            //antiDupQueue.Execute(1,   () => {
            //    var task = Task.Run(() => { return 1; });
            //    var val = await task;
            //    return val;
            //});


            Console.WriteLine("----------------------- 结束 -----------------------");
            Console.ReadLine();
        }




        private static void Test(int count, int lism)
        {
            var list = Build(count);
            antiDupCache.Clear();
            antiDupQueue.Clear();
            dictCache.Clear();
            cache.Clear();
            Console.WriteLine($"----------------------- 开始  从1到100   重复次数：{count} 单位： ms -----------------------");
            Console.Write("      并发数量： ");
            for (int i = 1; i <= lism; i++) {
                Console.Write(i.ToString().PadRight(5));
            }
            Console.Write("\r\n");

            var stopwatch = Stopwatch.StartNew();
            Console.Write("      普通并发：");
            for (int i = 1; i <= lism; i++) {
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    Thread.Sleep(1);
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");


            Console.Write("  AntiDupCache：");
            for (int i = 1; i <= lism; i++) {
                antiDupCache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    antiDupCache.GetOrAdd(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("  AntiDupQueue：");
            for (int i = 1; i <= lism; i++) {
                antiDupQueue.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    antiDupQueue.GetOrAdd(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("     DictCache：");
            for (int i = 1; i <= lism; i++) {
                dictCache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    dictCache.GetOrAdd(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("         Cache：");
            for (int i = 1; i <= lism; i++) {
                cache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    cache.Execute(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");


            stopwatch = Stopwatch.StartNew();
            Console.Write("第二次普通并发：");
            for (int i = 1; i <= lism; i++) {
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    Thread.Sleep(1);
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");
            Console.Write("\r\n");

        }

        private static void Test2(int count, int lism)
        {
            var list = Build(count);
            Console.WriteLine($"----------------------- 测试缓存性能 从1到100 重复次数：{count} 单位： ms -----------------------");
            Console.Write("    并发数量： ");
            for (int i = 1; i <= lism; i++) {
                Console.Write(i.ToString().PadRight(5));
            }
            Console.Write("\r\n");

            var stopwatch = Stopwatch.StartNew();
            Console.Write("AntiDupCache：");
            for (int i = 1; i <= lism; i++) {
                antiDupCache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    antiDupCache.GetOrAdd(j, () => {
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("AntiDupQueue：");
            for (int i = 1; i <= lism; i++) {
                antiDupQueue.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    antiDupQueue.GetOrAdd(j, () => {
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("   DictCache：");
            for (int i = 1; i <= lism; i++) {
                dictCache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    dictCache.GetOrAdd(j, () => {
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("       Cache：");
            for (int i = 1; i <= lism; i++) {
                cache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    cache.Execute(j, () => {
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");
            Console.Write("\r\n");

        }

        private static void Test3(int count, int lism)
        {
            var list = Build2(count);
            antiDupCache.Clear();
            antiDupQueue.Clear();
            dictCache.Clear();
            cache.Clear();
            Console.WriteLine($"----------------------- 仿线上环境  从1到{count}  单位： ms -----------------------");
            Console.Write("      并发数量： ");
            for (int i = 1; i <= lism; i++) {
                Console.Write(i.ToString().PadRight(5));
            }
            Console.Write("\r\n");

            var stopwatch = Stopwatch.StartNew();
            Console.Write("      普通并发：");
            for (int i = 1; i <= lism; i++) {
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    Thread.Sleep(1);
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");


            Console.Write("  AntiDupCache：");
            for (int i = 1; i <= lism; i++) {
                antiDupCache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    antiDupCache.GetOrAdd(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("  AntiDupQueue：");
            for (int i = 1; i <= lism; i++) {
                antiDupQueue.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    antiDupQueue.GetOrAdd(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("     DictCache：");
            for (int i = 1; i <= lism; i++) {
                dictCache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    dictCache.GetOrAdd(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");

            Console.Write("         Cache：");
            for (int i = 1; i <= lism; i++) {
                cache.Clear();
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    cache.Execute(j, () => {
                        Thread.Sleep(1);
                        return j;
                    });
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");


            stopwatch = Stopwatch.StartNew();
            Console.Write("第二次普通并发：");
            for (int i = 1; i <= lism; i++) {
                stopwatch = Stopwatch.StartNew();
                Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = i }, (j) => {
                    Thread.Sleep(1);
                });
                stopwatch.Stop();
                Console.Write(" " + stopwatch.ElapsedMilliseconds.ToString().PadRight(4));
            }
            Console.Write("\r\n");
            Console.Write("\r\n");

        }


        private static List<int> Build(int count)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < 100; i++) {
                for (int j = 0; j < count; j++) {
                    list.Add(i);
                }
            }
            return list;
        }

        private static List<int> Build2(int count)
        {
            Random random = new Random();

            List<int> list = new List<int>();
            while (true) {
                for (int i = 0; i < count; i++) {
                    list.Add(i);
                    if (random.NextDouble() > 0.99) {
                        list.Add(i);
                    }
                }
                if (list.Count- count > 10) {
                    return list;
                }
                list.Clear();
            }
        }

    }

}
