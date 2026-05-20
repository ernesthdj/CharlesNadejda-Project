using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Création / modification d'un contexte de production.
    /// En mode création : inclut la définition des niveaux de transformation
    /// (N1 "Ingrédients" automatique + N2, N3... définis par l'artisan).
    /// En mode édition : modification du nom, description et activité uniquement.
    /// Migré vers FrmEditBase — errorProvider et boutons gérés par la classe de base.
    /// </summary>
    public class FrmBomContexteEdit : FrmEditBase
    {
        private readonly BomContexte _contexte;
        private readonly bool        _isEdit;
        private readonly Activite    _activiteForce;

        // Mode création uniquement — liste des noms de niveaux supplémentaires (N2, N3...)
        private readonly List<string> _nomsNiveauxSupp = new List<string>();

        private readonly TextBox  txtNom;
        private readonly TextBox  txtDescription;
        private readonly ComboBox cboActivite;

        // Mode création uniquement
        private TextBox _txtNomN1;
        private ListBox _lstNiveauxSupp;
        private Button  _btnSupprimerNiv;

        private static readonly Color CHOCOLAT_FONCE = AppColors.ChocoBrand;

        public FrmBomContexteEdit(BomContexte contexte, Activite activiteForce = null)
        {
            _isEdit        = contexte != null;
            _contexte      = contexte ?? new BomContexte();
            _activiteForce = activiteForce;

            var font = new Font("Segoe UI", 10F);
            ClientSize = new Size(390, 100);

            // ── Nom ───────────────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 20), Text = "Nom du contexte *" });
            txtNom = new TextBox { Font = font, Location = new Point(20, 42), Size = new Size(340, 26), TabIndex = 0 };
            Controls.Add(txtNom);

            // ── Activité ──────────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 82), Text = "Activité *" });
            cboActivite = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = font, Location = new Point(20, 104),
                Size = new Size(200, 26), TabIndex = 1
            };
            Controls.Add(cboActivite);

            // ── Description ───────────────────────────────────────────────
            Controls.Add(new Label { AutoSize = true, Font = font, Location = new Point(20, 144), Text = "Description" });
            txtDescription = new TextBox
            {
                Font = font, Location = new Point(20, 166),
                Multiline = true, Size = new Size(340, 60), TabIndex = 2
            };
            Controls.Add(txtDescription);

            // Section niveaux (création uniquement) — avant PositionnerBoutons pour que la hauteur soit correcte
            if (!_isEdit)
                AjouterSectionNiveaux();

            PositionnerBoutons(_isEdit ? 244 : 492);

            Text = _isEdit ? "Modifier le contexte" : "Nouveau contexte de production";
            Load += FrmBomContexteEdit_Load;
        }

        private void FrmBomContexteEdit_Load(object sender, EventArgs e)
        {
            // Chargement dynamique des activités depuis la DB
            var activites = ActiviteDAL.GetAll();
            cboActivite.Items.Clear();
            foreach (var a in activites) cboActivite.Items.Add(a);
            cboActivite.DisplayMember = "Nom";

            if (_activiteForce != null)
            {
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
        }

        // ── Section niveaux (mode création uniquement) ───────────────────

        private void AjouterSectionNiveaux()
        {
            // Séparateur + titre section
            Controls.Add(new Panel
            {
                Location = new Point(0, 240), Size = new Size(390, 1),
                BackColor = Color.FromArgb(220, 210, 200)
            });
            Controls.Add(new Label
            {
                Text = "NIVEAUX DE TRANSFORMATION",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 110, 80),
                AutoSize = false, Location = new Point(20, 250), Size = new Size(340, 18)
            });

            // N1 — toujours présent
            var pnlN1 = new Panel
            {
                Location = new Point(20, 274), Size = new Size(350, 58),
                BackColor = Color.FromArgb(232, 244, 255)
            };
            pnlN1.Controls.Add(new Label
            {
                Text = "N1 — Niveau de base (automatique)",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 144, 217),
                AutoSize = false, Location = new Point(10, 6), Size = new Size(330, 18)
            });
            _txtNomN1 = new TextBox
            {
                Text = "Ingrédients", Font = new Font("Segoe UI", 10F),
                Location = new Point(10, 28), Size = new Size(330, 24)
            };
            pnlN1.Controls.Add(_txtNomN1);
            Controls.Add(pnlN1);

            // Niveaux supérieurs
            Controls.Add(new Label
            {
                Text = "Niveaux supérieurs (N2, N3...)",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(140, 110, 80),
                AutoSize = false, Location = new Point(20, 344), Size = new Size(240, 18)
            });
            _lstNiveauxSupp = new ListBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 366), Size = new Size(280, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(_lstNiveauxSupp);

            var btnAjouterNiv = new Button
            {
                Text = "+", Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(310, 366), Size = new Size(30, 28),
                BackColor = Color.FromArgb(92, 184, 92), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnAjouterNiv.FlatAppearance.BorderSize = 0;
            btnAjouterNiv.Click += BtnAjouterNiveau_Click;
            Controls.Add(btnAjouterNiv);

            _btnSupprimerNiv = new Button
            {
                Text = "−", Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(310, 400), Size = new Size(30, 28),
                BackColor = Color.FromArgb(200, 80, 60), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Enabled = false
            };
            _btnSupprimerNiv.FlatAppearance.BorderSize = 0;
            _btnSupprimerNiv.Click += BtnSupprimerDernierNiveau_Click;
            Controls.Add(_btnSupprimerNiv);

            Controls.Add(new Label
            {
                Text = "Ex: Recettes, Assemblages, Finitions...",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = false, Location = new Point(20, 429), Size = new Size(280, 16)
            });
        }

        private void BtnAjouterNiveau_Click(object sender, EventArgs e)
        {
            int ordreProchain = _nomsNiveauxSupp.Count + 2;

            using (var dlg = new Form())
            {
                dlg.Text            = $"Nom du niveau N{ordreProchain}";
                dlg.ClientSize      = new Size(320, 110);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition   = FormStartPosition.CenterParent;
                dlg.MaximizeBox     = false;

                var lbl = new Label { Text = $"Nom du N{ordreProchain} :", Location = new Point(14, 14), AutoSize = true, Font = new Font("Segoe UI", 10F) };
                var txt = new TextBox { Location = new Point(14, 36), Size = new Size(290, 26), Font = new Font("Segoe UI", 10F) };
                var btnOk  = new Button
                {
                    Text = "Ajouter", Location = new Point(14, 70), Size = new Size(120, 28),
                    DialogResult = DialogResult.OK,
                    BackColor = CHOCOLAT_FONCE, ForeColor = Color.White, FlatStyle = FlatStyle.Flat
                };
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

        // ── Validation + Sauvegarde ──────────────────────────────────────

        protected override bool Valider()
        {
            bool ok = true;

            if (string.IsNullOrWhiteSpace(txtNom.Text))
            { errorProvider.SetError(txtNom, "Le nom est obligatoire."); ok = false; }

            if (cboActivite.SelectedItem == null)
            { errorProvider.SetError(cboActivite, "L'activité est obligatoire."); ok = false; }

            if (!ok) return false;

            var    activite = (Activite)cboActivite.SelectedItem;
            string nom      = txtNom.Text.Trim();

            if (BomContexteDAL.NomExiste(nom, activite.Id, _isEdit ? _contexte.Id : 0))
            {
                errorProvider.SetError(txtNom, "Un contexte avec ce nom existe déjà pour cette activité.");
                return false;
            }

            return true;
        }

        protected override void Sauvegarder()
        {
            var activite = (Activite)cboActivite.SelectedItem;

            _contexte.Nom         = txtNom.Text.Trim();
            _contexte.Description = txtDescription.Text.Trim().NullIfEmpty();
            _contexte.IdActivite  = activite.Id;
            _contexte.ActiviteNom = activite.Nom;

            if (_isEdit)
            {
                BomContexteDAL.Update(_contexte);
            }
            else
            {
                string nomN1 = (_txtNomN1 != null && !string.IsNullOrWhiteSpace(_txtNomN1.Text))
                    ? _txtNomN1.Text.Trim()
                    : "Ingrédients";

                var tousNoms = new List<string> { nomN1 };
                tousNoms.AddRange(_nomsNiveauxSupp);

                BomContexteDAL.InsertAvecNiveaux(_contexte, tousNoms);
            }
        }
    }
}
