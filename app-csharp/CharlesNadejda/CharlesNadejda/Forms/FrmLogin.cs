using System;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
#if DEBUG
            txtEmail.Text      = "charles@charlesnadejda.be";
            txtMotDePasse.Text = "password";
#endif
        }

        private void btnConnexion_Click(object sender, EventArgs e)
        {
            lblErreur.Visible = false;

            string email = txtEmail.Text.Trim();
            string mdp   = txtMotDePasse.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(mdp))
            {
                lblErreur.Text    = "Veuillez remplir tous les champs.";
                lblErreur.Visible = true;
                return;
            }

            try
            {
                Utilisateur u = UtilisateurDAL.Authenticate(email, mdp);

                if (u != null)
                {
                    var frmPrincipal = new FrmPrincipal(u);
                    frmPrincipal.Show();
                    this.Hide();
                }
                else
                {
                    lblErreur.Text    = "Email ou mot de passe incorrect.";
                    lblErreur.Visible = true;
                    txtMotDePasse.Clear();
                    txtMotDePasse.Focus();
                }
            }
            catch (Exception ex)
            {
                lblErreur.Text    = "Erreur de connexion à la base de données.";
                lblErreur.Visible = true;
                // Log en dev uniquement — ne jamais exposer ex.Message en prod
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void txtMotDePasse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnConnexion_Click(sender, e);
        }
    }
}
