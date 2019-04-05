using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ToolGood.AntiDuplication.WebDemo.Cores;
using ToolGood.AntiDuplication.WebDemo.Datas;

namespace ToolGood.AntiDuplication.WebDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static AntiDupCache<string, int> antiDupCache = new AntiDupCache<string, int>(10000,30);

        [HttpGet("Clear")]
        public IActionResult Clear()
        {
            using (var helper = Config.MySqlHelper) {
                helper.Execute("TRUNCATE TABLE Users");
                antiDupCache.Flush();
            }
            return Ok();
        }

        [HttpGet("Insert_1")]
        public IActionResult Insert_1([FromQuery] UserModel model)
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
            return Ok(id.ToString());
        }


        [HttpGet("Insert_2")]
        public IActionResult Insert_2([FromQuery] UserModel model)
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
            return Ok(id.ToString());
        }
    }

    public class UserModel
    {
        public string Phone { get; set; }

        public string Name { get; set; }
    }
}
