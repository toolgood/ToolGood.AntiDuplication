using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToolGood.ReadyGo3.Attributes;

namespace ToolGood.AntiDuplication.WebDemo.Datas
{
    [Table("Users")]
    public class DbUser
    {
        public int Id { get; set; }

        public string Phone { get; set; }

        public string Name { get; set; }
    }
}
