using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmLogin : Form
    {
        /// <summary>
        /// Utilisateur authentifié — disponible après DialogResult.OK.
        /// </summary>
        public Utilisateur Utilisateur { get; private set; }

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
                    // SFA Pattern : exposer l'utilisateur puis fermer en DialogResult.OK.
                    // Program.Main instanciera FrmPrincipal comme Form racine.
                    Utilisateur    = u;
                    DialogResult   = DialogResult.OK;
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

        // ── Gradient horizontal bandeau titre : #3D2817 → #6F4E37 ──────────
        private void PnlHeader_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            using (var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                Color.FromArgb(61,  40, 23),   // #3D2817 CHOCOLAT_FONCE
                Color.FromArgb(111, 78, 55),   // #6F4E37 CHOCOLAT_MOYEN
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, panel.ClientRectangle);
            }
        }
    }
}
