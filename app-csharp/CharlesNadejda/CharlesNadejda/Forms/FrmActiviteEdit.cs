using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Création / modification d'une activité artisanale.
    /// Pattern : classe non-partial, UI construite programmatiquement.
    /// </summary>
    public class FrmActiviteEdit : Form
    {
        private readonly Activite _activite;
        private readonly bool     _isEdit;

        private TextBox  _txtNom;
        private TextBox  _txtDescription;
        private Button   _btnEnregistrer;
        private Button   _btnAnnuler;
        private ErrorProvider _errorProvider;

        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61, 40, 23);
        private static readonly Color CREME          = Color.FromArgb(245, 230, 211);

        public FrmActiviteEdit(Activite activite = null)
        {
            _isEdit   = activite != null;
            _activite = activite ?? new Activite { Actif = true };
            BuildUI();
            Load += FrmActiviteEdit_Load;
        }

        private void BuildUI()
        {
            this.Text            = _isEdit ? "Modifier l'activité" : "Nouvelle activité";
            this.ClientSize      = new Size(400, 230);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.White;

            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            // ── En-tête ──────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = CHOCOLAT_FONCE,
                Padding   = new Padding(16, 12, 16, 12)
            };
            var lblTitre = new Label
            {
                Text      = _isEdit ? "Modifier l'activité" : "Nouvelle activité",
                Font      = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = CREME,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitre);
            this.Controls.Add(pnlHeader);

            // ── Contenu ───────────────────────────────────────────────────
            // Grille 8px : padding 16px, labels 110px, champ = largeur restante
            int lx = 16, cx = 130, cw = 240;

            var lblNom = new Label
            {
                Text      = "Nom *",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = CHOCOLAT_FONCE,
                Location  = new Point(lx, 68),
                Size      = new Size(110, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _txtNom = new TextBox
            {
                Font     = new Font("Segoe UI", 10F),
                Location = new Point(cx, 68),
                Size     = new Size(cw, 24),
                MaxLength = 100
            };

            var lblDesc = new Label
            {
                Text      = "Description",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = CHOCOLAT_FONCE,
                Location  = new Point(lx, 104),
                Size      = new Size(110, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _txtDescription = new TextBox
            {
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(cx, 104),
                Size      = new Size(cw, 64),
                Multiline = true,
                ScrollBars= ScrollBars.Vertical
            };

            // ── Boutons (alignés en bas, espacés φ) ──────────────────────
            // Fitts : bouton principal à gauche, annuler à droite
            _btnEnregistrer = new Button
            {
                Text      = "Enregistrer",
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = CHOCOLAT_FONCE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(16, 190),
                Size      = new Size(136, 32),
                Cursor    = Cursors.Hand
            };
            _btnEnregistrer.FlatAppearance.BorderSize = 0;
            _btnEnregistrer.Click += BtnEnregistrer_Click;

            _btnAnnuler = new Button
            {
                Text      = "Annuler",
                Font      = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(235, 228, 220),
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(160, 190),
                Size      = new Size(96, 32),
                Cursor    = Cursors.Hand
            };
            _btnAnnuler.FlatAppearance.BorderSize = 0;
            _btnAnnuler.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.AcceptButton = _btnEnregistrer;
            this.CancelButton = _btnAnnuler;

            this.Controls.AddRange(new Control[]
            {
                lblNom, _txtNom, lblDesc, _txtDescription,
                _btnEnregistrer, _btnAnnuler
            });
        }

        private void FrmActiviteEdit_Load(object sender, EventArgs e)
        {
            if (_isEdit)
            {
                _txtNom.Text         = _activite.Nom;
                _txtDescription.Text = _activite.Description ?? "";
            }
            _txtNom.Focus();
        }

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            _errorProvider.Clear();

            string nom = _txtNom.Text.Trim();
            if (string.IsNullOrEmpty(nom))
            {
                _errorProvider.SetError(_txtNom, "Le nom est obligatoire.");
                return;
            }
            if (ActiviteDAL.NomExiste(nom, _isEdit ? _activite.Id : 0))
            {
                _errorProvider.SetError(_txtNom, "Une activité avec ce nom existe déjà.");
                return;
            }

            try
            {
                _activite.Nom         = nom;
                _activite.Description = string.IsNullOrWhiteSpace(_txtDescription.Text)
                                        ? null : _txtDescription.Text.Trim();

                if (_isEdit) ActiviteDAL.Update(_activite);
                else         ActiviteDAL.Insert(_activite);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
