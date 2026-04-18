using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    public static class UtilisateurDAL
    {
        public static Utilisateur Authenticate(string email, string motDePasse)
        {
            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT id, nom, prenom, email, role, mot_de_passe
                    FROM utilisateurs
                    WHERE email = @email AND actif = 1 AND role = 'admin'";

                cmd.Parameters.AddWithValue("@email", email);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    string hash = reader["mot_de_passe"].ToString();

                    if (!BCrypt.Net.BCrypt.Verify(motDePasse, hash))
                        return null;

                    return new Utilisateur
                    {
                        Id     = (int)reader["id"],
                        Nom    = reader["nom"].ToString(),
                        Prenom = reader["prenom"].ToString(),
                        Email  = reader["email"].ToString(),
                        Role   = reader["role"].ToString()
                    };
                }
            }
        }
    }
}
