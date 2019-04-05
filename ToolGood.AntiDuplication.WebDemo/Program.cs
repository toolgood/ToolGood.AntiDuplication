using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ToolGood.AntiDuplication.WebDemo.Cores;
using ToolGood.AntiDuplication.WebDemo.Datas;

namespace ToolGood.AntiDuplication.WebDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //if (File.Exists("db.db3")) {
            //    File.Delete("db.db3");
            //}
            //File.Create("db.db3").Close();
            try {
                var helper = Config.MySqlHelper;
                helper.Execute("TRUNCATE TABLE Users");
                //helper._TableHelper.CreateTable(typeof(DbUser));
                helper.Dispose();
            } catch (Exception ex) {
            }



            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
