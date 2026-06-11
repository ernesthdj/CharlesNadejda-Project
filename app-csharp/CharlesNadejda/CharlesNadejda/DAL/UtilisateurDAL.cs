using MySql.Data.MySqlClient;
using CharlesNadejda.Models;

namespace CharlesNadejda.DAL
{
    /// <summary>
    /// Authentification des utilisateurs admin de l'ERP (table utilisateurs).
    /// Utilise BCrypt pour la vérification du mot de passe — compatible avec le hash Laravel.
    /// </summary>
    public static class UtilisateurDAL
    {
        /// <summary>
        /// Authentifie un administrateur par email + mot de passe BCrypt.
        /// Retourne l'utilisateur si les credentials sont valides, null sinon.
        /// Seuls les utilisateurs actifs avec le rôle "admin" peuvent se connecter.
        /// </summary>
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
