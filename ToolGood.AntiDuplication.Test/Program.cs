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
        private static AntiDupCache<int, int> antiDupCache = new AntiDupCache<int, int>(50, 1);
        private static AntiDupQueue<int, int> antiDupQueue = new AntiDupQueue<int, int>(50);
        private static DictCache<int, int> dictCache = new DictCache<int, int>();
        private static Cache<int, int> cache = new Cache<int, int>();


        static void Main(string[] args)
        {
            var processorCount = Environment.ProcessorCount;
            for (int slim = 1; slim <= processorCount; slim++) {
                for (int count = 1; count <= processorCount; count++) {
                    Test(count, slim);
                }
            }

            Console.WriteLine("----------------------- 结束 -----------------------");
            Console.ReadLine();
        }




        private static void Test(int count, int lism = 8)
        {
            var list = Build(count);
            antiDupCache.Clear();
            antiDupQueue.Clear();
            dictCache.Clear();
            cache.Clear();
            Console.WriteLine($"----------------------- 开始  从1到100   重复次数：{count}  并发数：{lism} -----------------------");

            var stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = lism }, (j) => {
                Thread.Sleep(1);
            });
            stopwatch.Stop();
            Console.WriteLine("使用普通并发：" + stopwatch.ElapsedMilliseconds + "ms");

            stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = lism }, (j) => {
                antiDupCache.Execute(j, () => {
                    Thread.Sleep(1);
                    return j;
                });
            });
            stopwatch.Stop();
            Console.WriteLine("使用AntiDupCache：" + stopwatch.ElapsedMilliseconds + "ms");

            stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = lism }, (j) => {
                antiDupQueue.Execute(j, () => {
                    Thread.Sleep(1);
                    return j;
                });
            });
            stopwatch.Stop();
            Console.WriteLine("使用AntiDupQueue：" + stopwatch.ElapsedMilliseconds + "ms");

            stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = lism }, (j) => {
                dictCache.Execute(j, () => {
                    Thread.Sleep(1);
                    return j;
                });
            });
            stopwatch.Stop();
            Console.WriteLine("使用DictCache：" + stopwatch.ElapsedMilliseconds + "ms");


            stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = lism }, (j) => {
                cache.Execute(j, () => {
                    Thread.Sleep(1);
                    return j;
                });
            });
            stopwatch.Stop();
            Console.WriteLine("使用Cache：" + stopwatch.ElapsedMilliseconds + "ms");

            stopwatch = Stopwatch.StartNew();
            Parallel.ForEach(list, new ParallelOptions() { MaxDegreeOfParallelism = lism }, (j) => {
                Thread.Sleep(1);
            });
            stopwatch.Stop();
            Console.WriteLine("第二次使用普通并发：" + stopwatch.ElapsedMilliseconds + "ms");
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

    }

}
