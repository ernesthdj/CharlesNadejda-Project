using MySql.Data.MySqlClient;
using System.Configuration;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// Point d'entrée unique pour les connexions MySQL.
    /// La connection string est lue depuis App.config (clé "charlesnadejda").
    /// Chaque appel retourne une connexion ouverte — l'appelant est responsable du Dispose via using.
    /// </summary>
    public static class DbHelper
    {
        /// <summary>
        /// Crée et ouvre une nouvelle connexion MySQL.
        /// Usage systématique : using (var conn = DbHelper.GetConnection())
        /// </summary>
        public static MySqlConnection GetConnection()
        {
            string cs = ConfigurationManager.ConnectionStrings["charlesnadejda"].ConnectionString;
            var conn = new MySqlConnection(cs);
            conn.Open();
            return conn;
        }
    }
}
