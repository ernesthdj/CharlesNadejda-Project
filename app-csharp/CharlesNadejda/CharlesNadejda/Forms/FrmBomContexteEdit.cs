using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
// ActiviteDAL est dans CharlesNadejda.DAL — déjà importé ci-dessus

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Création / modification d'un contexte de production.
    /// En mode création : inclut la définition des niveaux de transformation
    /// (N1 "Ingrédients" automatique + N2, N3... définis par l'artisan).
    /// En mode édition : modification du nom, description et activité uniquement
    /// (les niveaux sont gérés depuis FrmArtisaStock).
    /// </summary>
    public partial class FrmBomContexteEdit : Form
    {
        private readonly BomContexte _contexte;
        private readonly bool        _isEdit;
        private readonly Activite    _activiteForce;

        // Mode création uniquement — liste des noms de niveaux supplémentaires (N2, N3...)
        private readonly List<string> _nomsNiveauxSupp = new List<string>();

        // Contrôles dynamiques (section niveaux, création uniquement)
        private TextBox  _txtNomN1;
        private ListBox  _lstNiveauxSupp;
        private Button   _btnAjouterNiv;
        private Button   _btnSupprimerNiv;

        private static readonly Color CHOCOLAT_FONCE = Color.FromArgb(61, 40, 23);

        public FrmBomContexteEdit(BomContexte contexte, Activite activiteForce = null)
        {
            InitializeComponent();
            _isEdit        = contexte != null;
            _contexte      = contexte ?? new BomContexte();
            _activiteForce = activiteForce;
        }

        private void FrmBomContexteEdit_Load(object sender, EventArgs e)
        {
            this.Text = _isEdit ? "Modifier le contexte" : "Nouveau contexte de production";

            // Chargement dynamique des activités depuis la DB
            var activites = ActiviteDAL.GetAll();
            cboActivite.Items.Clear();
            foreach (var a in activites) cboActivite.Items.Add(a);
            cboActivite.DisplayMember = "Nom";

            if (_activiteForce != null)
            {
                // Trouver l'activité dans la liste par Id
                foreach (var item in cboActivite.Items)
                    if (((Activite)item).Id == _activiteForce.Id) { cboActivite.SelectedItem = item; break; }
                cboActivite.Enabled = false;
            }
            else if (_isEdit && _contexte.IdActivite > 0)
            {
                foreach (var item in cboActivite.Items)
                    if (((Activite)item).Id == _contexte.IdActivite) { cboActivite.SelectedItem = item; break; }
            }
            else if (cboActivite.Items.Count > 0)
            {
                cboActivite.SelectedIndex = 0;
            }

            if (_isEdit)
            {
                txtNom.Text         = _contexte.Nom;
                txtDescription.Text = _contexte.Description;
            }

            if (!_isEdit)
                AjouterSectionNiveaux();
        }

        // ── Section niveaux (mode création uniquement) ───────────────────

        private void AjouterSectionNiveaux()
        {
            // Agrandir le formulaire pour la section niveaux
            this.ClientSize = new Size(390, 540);

            // Déplacer les boutons Enregistrer/Annuler en bas du nouveau formulaire
            btnEnregistrer.Location = new Point(20, 492);
            btnAnnuler.Location     = new Point(200, 492);

            // ── Séparateur + titre section ────────────────────────────────
            var sep = new Panel
            {
                Location  = new Point(0, 295),
                Size      = new Size(390, 1),
                BackColor = Color.FromArgb(220, 210, 200)
            };
            this.Controls.Add(sep);

            var lblSection = new Label
            {
                Text      = "NIVEAUX DE TRANSFORMATION",
                Font      = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 110, 80),
                AutoSize  = false,
                Location  = new Point(20, 306),
                Size      = new Size(340, 18)
            };
            this.Controls.Add(lblSection);

            // ── N1 — toujours présent ─────────────────────────────────────
            var pnlN1 = new Panel
            {
                Location  = new Point(20, 330),
                Size      = new Size(350, 58),
                BackColor = Color.FromArgb(232, 244, 255)
            };

            var lblN1 = new Label
            {
                Text      = "N1 — Niveau de base (automatique)",
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 144, 217),
                AutoSize  = false,
                Location  = new Point(10, 6),
                Size      = new Size(330, 18)
            };

            _txtNomN1 = new TextBox
            {
                Text     = "Ingrédients",
                Font     = new Font("Segoe UI", 10F),
                Location = new Point(10, 28),
                Size     = new Size(330, 24)
            };

            pnlN1.Controls.Add(lblN1);
            pnlN1.Controls.Add(_txtNomN1);
            this.Controls.Add(pnlN1);

            // ── N2, N3... ─────────────────────────────────────────────────
            var lblNivSupp = new Label
            {
                Text      = "Niveaux supérieurs (N2, N3...)",
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 110, 80),
                AutoSize  = false,
                Location  = new Point(20, 400),
                Size      = new Size(240, 18)
            };
            this.Controls.Add(lblNivSupp);

            _lstNiveauxSupp = new ListBox
            {
                Font      = new Font("Segoe UI", 10F),
                Location  = new Point(20, 422),
                Size      = new Size(280, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(_lstNiveauxSupp);

            _btnAjouterNiv = new Button
            {
                Text      = "+",
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location  = new Point(310, 422),
                Size      = new Size(30, 28),
                BackColor = Color.FromArgb(92, 184, 92),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            _btnAjouterNiv.FlatAppearance.BorderSize = 0;
            _btnAjouterNiv.Click += BtnAjouterNiveau_Click;
            this.Controls.Add(_btnAjouterNiv);

            _btnSupprimerNiv = new Button
            {
                Text      = "−",
                Font      = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location  = new Point(310, 456),
                Size      = new Size(30, 28),
                BackColor = Color.FromArgb(200, 80, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            _btnSupprimerNiv.FlatAppearance.BorderSize = 0;
            _btnSupprimerNiv.Click += BtnSupprimerDernierNiveau_Click;
            this.Controls.Add(_btnSupprimerNiv);

            var lblHint = new Label
            {
                Text      = "Ex: Recettes, Assemblages, Finitions...",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize  = false,
                Location  = new Point(20, 485),
                Size      = new Size(280, 16)
            };
            this.Controls.Add(lblHint);

            // S'assurer que les boutons sont par-dessus tout
            btnEnregistrer.BringToFront();
            btnAnnuler.BringToFront();
        }

        private void BtnAjouterNiveau_Click(object sender, EventArgs e)
        {
            int ordreProchain = _nomsNiveauxSupp.Count + 2; // N1=1, premier N supp=2

            using (var dlg = new Form())
            {
                dlg.Text            = $"Nom du niveau N{ordreProchain}";
                dlg.ClientSize      = new Size(320, 110);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;

                var lbl = new Label { Text = $"Nom du N{ordreProchain} :", Location = new Point(14, 14), AutoSize = true, Font = new Font("Segoe UI", 10F) };
                var txt = new TextBox { Location = new Point(14, 36), Size = new Size(290, 26), Font = new Font("Segoe UI", 10F) };
                var btnOk  = new Button { Text = "Ajouter", Location = new Point(14, 70), Size = new Size(120, 28), DialogResult = DialogResult.OK,
                    BackColor = CHOCOLAT_FONCE, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnOk.FlatAppearance.BorderSize = 0;
                var btnNon = new Button { Text = "Annuler", Location = new Point(144, 70), Size = new Size(80, 28), DialogResult = DialogResult.Cancel };

                dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnNon });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnNon;

                if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
                {
                    string nom = txt.Text.Trim();
                    _nomsNiveauxSupp.Add(nom);
                    _lstNiveauxSupp.Items.Add($"N{ordreProchain}  ·  {nom}");
                    _btnSupprimerNiv.Enabled = true;
                }
            }
        }

        private void BtnSupprimerDernierNiveau_Click(object sender, EventArgs e)
        {
            if (_nomsNiveauxSupp.Count == 0) return;
            _nomsNiveauxSupp.RemoveAt(_nomsNiveauxSupp.Count - 1);
            _lstNiveauxSupp.Items.RemoveAt(_lstNiveauxSupp.Items.Count - 1);
            _btnSupprimerNiv.Enabled = _nomsNiveauxSupp.Count > 0;
        }

        // ── Enregistrement ───────────────────────────────────────────────

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                errorProvider.SetError(txtNom, "Le nom est obligatoire.");
                valid = false;
            }
            if (cboActivite.SelectedItem == null)
            {
                errorProvider.SetError(cboActivite, "L'activité est obligatoire.");
                valid = false;
            }
            if (!valid) return;

            var    activite = (Activite)cboActivite.SelectedItem;
            string nom      = txtNom.Text.Trim();

            if (BomContexteDAL.NomExiste(nom, activite.Id, _isEdit ? _contexte.Id : 0))
            {
                errorProvider.SetError(txtNom, "Un contexte avec ce nom existe déjà pour cette activité.");
                return;
            }

            try
            {
                _contexte.Nom         = nom;
                _contexte.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
                _contexte.IdActivite  = activite.Id;
                _contexte.ActiviteNom = activite.Nom;

                if (_isEdit)
                {
                    BomContexteDAL.Update(_contexte);
                }
                else
                {
                    // Construire la liste complète des noms : N1 en premier, puis les supplémentaires
                    string nomN1 = (_txtNomN1 != null && !string.IsNullOrWhiteSpace(_txtNomN1.Text))
                        ? _txtNomN1.Text.Trim()
                        : "Ingrédients";

                    var tousNoms = new List<string> { nomN1 };
                    tousNoms.AddRange(_nomsNiveauxSupp);

                    BomContexteDAL.InsertAvecNiveaux(_contexte, tousNoms);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
