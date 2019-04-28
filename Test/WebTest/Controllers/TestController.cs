using SecondPartyManage.BaseCodes;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Web.Mvc;
using ToolGood.AntiDuplication;
using ToolGood.ReadyGo3;

namespace WebTest.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Test1(string id)
        {
            using (var helper = SqlHelperFactory.OpenFormConnStr("Writer")) {
                var count = helper.First<int>("select Count(*) from Test_Insert where FNum=@0", id);
                if (count == 0) {
                    helper.Execute("INSERT INTO Test_Insert (FNum) VALUES (@0);", id);
                }
            }
            return Content(id);
        }

        public ActionResult Test2(string id)
        {
            var rediesConnStr = ConfigurationManager.ConnectionStrings["redis"].ConnectionString;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(rediesConnStr);
            try {
                IDatabase db = redis.GetDatabase(2);
                if (db.LockTake("fnum_" + id, "1", TimeSpan.FromSeconds(1))) {
                    using (var helper = SqlHelperFactory.OpenFormConnStr("Writer")) {
                        var count = helper.First<int>("select Count(*) from Test_Insert where FNum=@0", id);
                        if (count == 0) {
                            helper.Execute("INSERT INTO Test_Insert (FNum) VALUES (@0);", id);
                        }
                    }
                    db.LockRelease("fnum_" + id, "1");
                    return Content(id);
                }
            } catch (Exception ex) {
                LoggerHelper.Error(ex.Message);
            }finally {
                redis.Close();
            }
            return Content("Error");
        }

        private static AntiDupCache<string, string> cache = new AntiDupCache<string, string>(20,1);
        public ActionResult Test3(string id)
        {
            var str = cache.Execute(id, () => {
                using (var helper = SqlHelperFactory.OpenFormConnStr("Writer")) {
                    var count = helper.First<int>("select Count(*) from Test_Insert where FNum=@0", id);
                    if (count == 0) {
                        helper.Execute("INSERT INTO Test_Insert (FNum) VALUES (@0);", id);
                    }
                }
                return id;
            });
            return Content(str);
        }


        private static AntiDupQueue<string, string> queue = new AntiDupQueue<string, string>(20);
        public ActionResult Test4(string id)
        {
            var str = queue.Execute(id, () => {
                using (var helper = SqlHelperFactory.OpenFormConnStr("Writer")) {
                    var count = helper.First<int>("select Count(*) from Test_Insert where FNum=@0", id);
                    if (count == 0) {
                        helper.Execute("INSERT INTO Test_Insert (FNum) VALUES (@0);", id);
                    }
                }
                return id;
            });
            return Content(str);
        }


    }

}