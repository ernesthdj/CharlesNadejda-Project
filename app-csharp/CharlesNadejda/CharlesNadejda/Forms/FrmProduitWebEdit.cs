using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public class FrmProduitWebEdit : FrmEditBase
    {
        private readonly bool _isEdit;
        private readonly ProduitWeb _prod;

        private readonly ComboBox    cboFiche;
        private readonly ComboBox    cboCategorie;
        private readonly TextBox     txtNom;
        private readonly TextBox     txtDescription;
        private readonly NumericUpDown nudPrix;
        private readonly NumericUpDown nudOrdre;
        private readonly CheckBox    chkEnVente;
        private readonly PictureBox  picImage;
        private readonly Button      btnImage;
        private readonly Label       lblImagePath;

        private string _selectedImagePath;   // chemin source choisi par FileDialog
        private string _laravelStoragePath;  // chemin cible storage Laravel

        public FrmProduitWebEdit(ProduitWeb prod)
        {
            _isEdit = prod != null;
            _prod   = prod ?? new ProduitWeb { EnVente = true, OrdreAffichage = 0 };

            Text = _isEdit ? "Modifier le produit web" : "Publier une fiche en boutique";
            Size = new Size(520, 520);
            MinimumSize = new Size(480, 480);

            // Chemin Laravel depuis App.config (obligatoire pour le partage d'images)
            _laravelStoragePath = System.Configuration.ConfigurationManager.AppSettings["LaravelStoragePath"];
            if (string.IsNullOrEmpty(_laravelStoragePath))
            {
                // Fallback : remonter depuis bin/Debug jusqu'à la racine du projet
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                _laravelStoragePath = Path.GetFullPath(
                    Path.Combine(exeDir, @"..\..\..\..\..\..\site-laravel\storage\app\public\"));
            }

            int y = 16;

            // ── Fiche BOM (non modifiable en édition) ────────
            Controls.Add(new Label
            {
                Text = "Fiche BOM *", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            cboFiche = new ComboBox
            {
                Location = new Point(140, y), Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            if (_isEdit)
            {
                cboFiche.Items.Add(_prod.NomFiche);
                cboFiche.SelectedIndex = 0;
                cboFiche.Enabled = false;
            }
            else
            {
                var fiches = ProduitWebDAL.GetFichesNonPubliees();
                cboFiche.DisplayMember = "Nom";
                cboFiche.ValueMember   = "Id";
                cboFiche.DataSource    = fiches;
            }
            Controls.Add(cboFiche);
            y += 38;

            // ── Catégorie ────────
            Controls.Add(new Label
            {
                Text = "Catégorie", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            cboCategorie = new ComboBox
            {
                Location = new Point(140, y), Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            var categories = CategorieWebDAL.GetAll();
            cboCategorie.Items.Add("(Aucune)");
            foreach (var c in categories) cboCategorie.Items.Add(c);
            cboCategorie.SelectedIndex = 0;
            Controls.Add(cboCategorie);
            y += 38;

            // ── Nom commercial ────────
            txtNom = MakeRow("Nom commercial *", ref y);
            y += 38;

            // ── Description ────────
            Controls.Add(new Label
            {
                Text = "Description", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            txtDescription = new TextBox
            {
                Location = new Point(140, y), Width = 340, Height = 60,
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(txtDescription);
            y += 68;

            // ── Prix ────────
            Controls.Add(new Label
            {
                Text = "Prix de vente (€) *", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            nudPrix = new NumericUpDown
            {
                Location = new Point(140, y), Width = 120,
                Minimum = 0.01m, Maximum = 99999m, DecimalPlaces = 2,
                Value = 1.00m, Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(nudPrix);
            y += 38;

            // ── Image ────────
            Controls.Add(new Label
            {
                Text = "Image", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            btnImage = new Button
            {
                Text = "Choisir une image...",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(140, y), Size = new Size(160, 28),
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnImage.Click += BtnImage_Click;
            lblImagePath = new Label
            {
                Text = "(aucune)", Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.Gray, Location = new Point(310, y + 4), AutoSize = true
            };
            Controls.Add(btnImage);
            Controls.Add(lblImagePath);
            y += 32;

            picImage = new PictureBox
            {
                Location = new Point(140, y), Size = new Size(120, 90),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 240, 230)
            };
            Controls.Add(picImage);
            y += 98;

            // ── Ordre + En vente ────────
            Controls.Add(new Label
            {
                Text = "Ordre d'affichage", Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            nudOrdre = new NumericUpDown
            {
                Location = new Point(140, y), Width = 80,
                Minimum = 0, Maximum = 999, Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(nudOrdre);

            chkEnVente = new CheckBox
            {
                Text = "Publié (visible sur le site)",
                Font = new Font("Segoe UI", 9F),
                Location = new Point(260, y), AutoSize = true, Checked = true
            };
            Controls.Add(chkEnVente);
            y += 38;

            PositionnerBoutons(y);

            // ── Pré-remplir si édition ────────
            if (_isEdit)
            {
                txtNom.Text         = _prod.NomCommercial;
                txtDescription.Text = _prod.Description;
                nudPrix.Value       = _prod.PrixVente > 0 ? _prod.PrixVente : 1.00m;
                nudOrdre.Value      = _prod.OrdreAffichage;
                chkEnVente.Checked  = _prod.EnVente;

                // Sélectionner la catégorie
                for (int i = 1; i < cboCategorie.Items.Count; i++)
                {
                    if (cboCategorie.Items[i] is CategorieWeb c && c.Id == _prod.IdCategorie)
                    {
                        cboCategorie.SelectedIndex = i;
                        break;
                    }
                }

                // Charger l'image existante
                if (!string.IsNullOrEmpty(_prod.ImagePath))
                {
                    lblImagePath.Text = _prod.ImagePath;
                    var fullPath = Path.Combine(_laravelStoragePath, _prod.ImagePath.Replace('/', '\\'));
                    if (File.Exists(fullPath))
                        picImage.Image = Image.FromFile(fullPath);
                }
            }
        }

        private void BtnImage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title  = "Choisir une image produit";
                dlg.Filter = "Images|*.jpg;*.jpeg;*.png;*.webp|Tous les fichiers|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = dlg.FileName;
                    lblImagePath.Text  = Path.GetFileName(dlg.FileName);
                    picImage.Image?.Dispose();
                    picImage.Image = Image.FromFile(dlg.FileName);
                }
            }
        }

        protected override bool Valider()
        {
            if (!_isEdit && cboFiche.SelectedItem == null)
            {
                errorProvider.SetError(cboFiche, "Sélectionnez une fiche BOM.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNom.Text))
            {
                errorProvider.SetError(txtNom, "Le nom commercial est obligatoire.");
                return false;
            }
            if (nudPrix.Value <= 0)
            {
                errorProvider.SetError(nudPrix, "Le prix doit être supérieur à 0.");
                return false;
            }
            return true;
        }

        protected override void Sauvegarder()
        {
            // Assigner les valeurs
            if (!_isEdit)
                _prod.IdBomFiche = ((BomFiche)cboFiche.SelectedItem).Id;

            _prod.IdCategorie    = cboCategorie.SelectedItem is CategorieWeb c ? (int?)c.Id : null;
            _prod.NomCommercial  = txtNom.Text.Trim();
            _prod.Description    = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();
            _prod.PrixVente      = nudPrix.Value;
            _prod.OrdreAffichage = (int)nudOrdre.Value;
            _prod.EnVente        = chkEnVente.Checked;

            // Copie de l'image vers le storage Laravel
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                var ext = Path.GetExtension(_selectedImagePath).ToLower();
                var fileName = $"produits/{(_isEdit ? _prod.Id : 0)}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                var destDir  = Path.Combine(_laravelStoragePath, "produits");
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                // Path.Combine + remplacement pour compatibilité Windows
                var destPath = Path.Combine(_laravelStoragePath, fileName.Replace('/', '\\'));
                File.Copy(_selectedImagePath, destPath, overwrite: true);
                // En DB : toujours des slashes (URL web)
                _prod.ImagePath = fileName;
            }

            if (_isEdit) ProduitWebDAL.Update(_prod);
            else
            {
                int newId = ProduitWebDAL.Insert(_prod);
                // Renommer l'image avec le vrai ID si nécessaire
                if (!string.IsNullOrEmpty(_prod.ImagePath) && _prod.ImagePath.Contains("/0_"))
                {
                    var oldPath = Path.Combine(_laravelStoragePath, _prod.ImagePath.Replace('/', '\\'));
                    var newFileName = _prod.ImagePath.Replace("/0_", $"/{newId}_");
                    var newPath = Path.Combine(_laravelStoragePath, newFileName.Replace('/', '\\'));
                    if (File.Exists(oldPath))
                    {
                        File.Move(oldPath, newPath);
                        _prod.ImagePath = newFileName;
                        _prod.Id = newId;
                        ProduitWebDAL.Update(_prod);
                    }
                }
            }
        }

        private TextBox MakeRow(string label, ref int y)
        {
            Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 9F),
                Location = new Point(16, y + 2), AutoSize = true
            });
            var txt = new TextBox
            {
                Location = new Point(140, y), Width = 340,
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(txt);
            return txt;
        }
    }
}
