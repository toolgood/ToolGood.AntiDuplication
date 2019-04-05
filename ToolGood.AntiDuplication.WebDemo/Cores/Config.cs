using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToolGood.ReadyGo3;

namespace ToolGood.AntiDuplication.WebDemo.Cores
{
    public class Config
    {
        [ThreadStatic]
        private static SqlHelper _sqlHelper;

        public static SqlHelper SqlHelper()
        {
            if (_sqlHelper == null) {
                _sqlHelper = SqlHelperFactory.OpenSqliteFile("db.db3");
            }
            return _sqlHelper;
        }
        public static SqlHelper SqlServerHelper {
            get {
                return SqlHelperFactory.OpenDatabase(@"Server=(LocalDB)\MSSQLLocalDB; Integrated Security=true ;AttachDbFileName=F:\git\ToolGood.ReadyGo\ToolGood.ReadyGo3.CoreTest\bin\Debug\test.mdf", "", SqlType.SqlServer);
            }
        }

        public static SqlHelper MySqlHelper {
            get {
                return SqlHelperFactory.OpenDatabase("Server=localhost;Database=test; User=somain;Password=somain456123;Charset=utf8;SslMode=none;", "MySql.Data", SqlType.MySql);

                //return SqlHelperFactory.OpenMysql("localhost","test","root","123456");
            }
        }

    }
}
