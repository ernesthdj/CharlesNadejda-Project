using MySql.Data.MySqlClient;
using System.Configuration;

namespace CharlesNadejda.DAL
{
    public static class DbHelper
    {
        public static MySqlConnection GetConnection()
        {
            string cs = ConfigurationManager.ConnectionStrings["charlesnadejda"].ConnectionString;
            var conn = new MySqlConnection(cs);
            conn.Open();
            return conn;
        }
    }
}
