using System;
using System.Drawing;
using System.Windows.Forms;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Formulaire d'édition générique — base commune à tous les formulaires Create/Update.
    ///
    /// USAGE — créer un formulaire d'édition :
    ///   1. Hériter de FrmEditBase
    ///   2. Dans le constructeur : créer les contrôles métier, puis appeler PositionnerBoutons(y)
    ///   3. Implémenter Valider() et Sauvegarder()
    ///   4. Ne pas déclarer btnEnregistrer, btnAnnuler ni errorProvider — ils viennent de la base
    ///   5. Supprimer ou vider le Designer.cs
    ///
    /// CYCLE :
    ///   Clic Enregistrer → errorProvider.Clear() → Valider() → Sauvegarder() → DialogResult.OK
    /// </summary>
    public abstract class FrmEditBase : Form
    {
        // ── Contrôles disponibles dans les sous-classes ───────────────────
        protected readonly ErrorProvider errorProvider;
        protected readonly Button        btnEnregistrer;
        protected readonly Button        btnAnnuler;

        // ── Palette artisanale partagée ───────────────────────────────
        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61, 40, 23);

        protected FrmEditBase()
        {
            errorProvider = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };

            btnEnregistrer = new Button
            {
                Text      = "Enregistrer",
                Size      = new Size(160, 36),
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = CHOCOLAT_FONCE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btnEnregistrer.FlatAppearance.BorderSize        = 0;
            btnEnregistrer.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 60, 36);

            btnAnnuler = new Button
            {
                Text      = "Annuler",
                Size      = new Size(160, 36),
                Font      = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 215, 210),
                ForeColor = CHOCOLAT_FONCE,
                Cursor    = Cursors.Hand
            };
            btnAnnuler.FlatAppearance.BorderSize = 0;

            btnEnregistrer.Click += (s, e) => Confirmer();
            btnAnnuler.Click     += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(btnEnregistrer);
            Controls.Add(btnAnnuler);

            // Propriétés communes à tous les formulaires d'édition
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            BackColor           = Color.White;
        }

        // ── Positionnement ────────────────────────────────────────────────

        /// <summary>
        /// Positionne les boutons Enregistrer / Annuler à la coordonnée Y donnée
        /// et ajuste la hauteur du formulaire en conséquence.
        ///
        /// À appeler EN DERNIER dans le constructeur de la sous-classe, après
        /// que tous les contrôles métier ont été positionnés.
        /// </summary>
        /// <param name="y">Coordonnée Y de la rangée de boutons.</param>
        protected void PositionnerBoutons(int y)
        {
            btnEnregistrer.Location = new Point(20, y);
            btnAnnuler.Location     = new Point(200, y);
            ClientSize = new Size(ClientSize.Width, y + 56);
        }

        // ── Logique de sauvegarde ─────────────────────────────────────────

        private void Confirmer()
        {
            errorProvider.Clear();
            if (!Valider()) return;
            try
            {
                Sauvegarder();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Membres abstraits ─────────────────────────────────────────────

        /// <summary>
        /// Valide les entrées utilisateur.
        /// Utilisez errorProvider.SetError(ctrl, msg) pour signaler les erreurs.
        /// Retourne true si toutes les validations passent.
        /// </summary>
        protected abstract bool Valider();

        /// <summary>
        /// Persiste les données en base via le DAL.
        /// Appelé uniquement si Valider() retourne true.
        /// Les exceptions sont capturées et affichées par la classe de base.
        /// </summary>
        protected abstract void Sauvegarder();
    }
}
