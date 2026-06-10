using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    partial class FrmPrincipal
    {
        // ════════════════════════════════════════════════════════════════
        //  BOUTIQUE WEB — Mini CMS (3 onglets, lazy init)
        // ════════════════════════════════════════════════════════════════

        private TabControl _tabBoutique;
        private DataGridView _dgvCategories, _dgvProduits, _dgvCommandes;
        private Panel _pnlDetailCommande;
        private bool _tabProduitsBuilt, _tabCommandesBuilt;

        // ── Écran principal ─────────────────────────────────────────

        private void ShowBoutiqueWebScreen()
        {
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            _tabProduitsBuilt = false;
            _tabCommandesBuilt = false;

            // Header
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 52,
                BackColor = CREME_WARM, Padding = new Padding(20, 0, 20, 0)
            };
            pnlHeader.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };
            pnlHeader.Controls.Add(new Label
            {
                Text = "\U0001f6d2  Boutique en ligne",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, AutoSize = true,
                Location = new Point(20, 12)
            });

            // TabControl
            _tabBoutique = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(12, 6)
            };

            var tabCategories = new TabPage("Catégories");
            var tabProduits   = new TabPage("Produits");
            var tabCommandes  = new TabPage("Commandes");

            _tabBoutique.TabPages.AddRange(new[] { tabCategories, tabProduits, tabCommandes });
            _tabBoutique.SelectedIndexChanged += TabBoutique_SelectedIndexChanged;

            _pnlDroit.Controls.Add(_tabBoutique);
            _pnlDroit.Controls.Add(pnlHeader);
            _pnlDroit.ResumeLayout(true);

            // Seul l'onglet 0 est construit immédiatement (il est visible)
            BuildTabCategories(tabCategories);
        }

        // ════════════════════════════════════════════════════════════════
        //  TAB SWITCH — Lazy init des onglets 1 et 2
        // ════════════════════════════════════════════════════════════════

        private void TabBoutique_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tab = _tabBoutique.SelectedTab;
            if (tab == null) return;

            switch (_tabBoutique.SelectedIndex)
            {
                case 1:
                    if (!_tabProduitsBuilt)
                    {
                        BuildTabProduits(tab);
                        _tabProduitsBuilt = true;
                    }
                    break;
                case 2:
                    if (!_tabCommandesBuilt)
                    {
                        BuildTabCommandes(tab);
                        _tabCommandesBuilt = true;
                    }
                    break;
            }

            // Force le re-layout complet de l'onglet actif
            // Workaround WinForms : les DGV Dock=Fill ne se recalculent
            // pas correctement quand on revient sur un onglet déjà visité.
            tab.SuspendLayout();
            foreach (Control ctrl in tab.Controls)
            {
                if (ctrl is DataGridView dgv)
                {
                    dgv.Visible = false;
                    dgv.Visible = true;
                }
            }
            tab.ResumeLayout(true);
            tab.PerformLayout();
        }

        // ════════════════════════════════════════════════════════════════
        //  ONGLET 1 — CATÉGORIES
        // ════════════════════════════════════════════════════════════════

        private void BuildTabCategories(TabPage tab)
        {
            tab.SuspendLayout();
            tab.BackColor = CREME_WARM;

            // Boutons
            var pnlBtnCat = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = CREME_WARM };
            var btnAddCat  = MakeBoutiqueButton("+ Ajouter",    new Point(12, 6));
            var btnEditCat = MakeBoutiqueButton("\u270f Modifier", new Point(132, 6));
            var btnDelCat  = MakeBoutiqueButton("\U0001f5d1 Supprimer", new Point(252, 6));
            btnAddCat.Click  += (s, e) => CatOuvrirForm(null);
            btnEditCat.Click += (s, e) => CatOuvrirForm(CatSelected());
            btnDelCat.Click  += (s, e) => CatSupprimer();
            pnlBtnCat.Controls.AddRange(new Control[] { btnAddCat, btnEditCat, btnDelCat });

            // DGV
            _dgvCategories = MakeBoutiqueDgv();
            _dgvCategories.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn  { Name = "Id",          HeaderText = "ID",          DataPropertyName = "Id",              FillWeight = 8,  MinimumWidth = 40 },
                new DataGridViewTextBoxColumn  { Name = "Nom",         HeaderText = "Nom",         DataPropertyName = "Nom",             FillWeight = 30, MinimumWidth = 100 },
                new DataGridViewTextBoxColumn  { Name = "Description", HeaderText = "Description", DataPropertyName = "Description",     FillWeight = 40, MinimumWidth = 120 },
                new DataGridViewTextBoxColumn  { Name = "Ordre",       HeaderText = "Ordre",       DataPropertyName = "OrdreAffichage",  FillWeight = 8,  MinimumWidth = 50 },
                new DataGridViewTextBoxColumn  { Name = "NbProduits",  HeaderText = "Produits",    DataPropertyName = "NbProduits",       FillWeight = 8,  MinimumWidth = 60 },
                new DataGridViewCheckBoxColumn { Name = "Actif",       HeaderText = "Actif",       DataPropertyName = "Actif",            FillWeight = 6,  MinimumWidth = 50 }
            });
            _dgvCategories.DoubleClick += (s, e) => CatOuvrirForm(CatSelected());

            tab.Controls.Add(_dgvCategories);
            tab.Controls.Add(pnlBtnCat);
            tab.ResumeLayout(true);

            CatRefresh();
        }

        private CategorieWeb CatSelected()
        {
            if (_dgvCategories.CurrentRow == null) return null;
            return _dgvCategories.CurrentRow.DataBoundItem as CategorieWeb;
        }

        private void CatRefresh()
        {
            var data = CategorieWebDAL.GetAll(includeInactifs: true);
            _dgvCategories.DataSource = null;
            _dgvCategories.DataSource = data;
            _dgvCategories.Columns["Id"].Visible = false;
        }

        private void CatOuvrirForm(CategorieWeb cat)
        {
            using (var frm = new FrmCategorieWebEdit(cat))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    CatRefresh();
            }
        }

        private void CatSupprimer()
        {
            var cat = CatSelected();
            if (cat == null) return;
            var res = MessageBox.Show(
                $"Supprimer la catégorie « {cat.Nom} » ?\n\nLes produits associés ne seront pas supprimés mais perdront leur catégorie.",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                CategorieWebDAL.Delete(cat.Id);
                CatRefresh();
                if (_tabProduitsBuilt) ProdRefresh();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ONGLET 2 — PRODUITS
        // ════════════════════════════════════════════════════════════════

        private void BuildTabProduits(TabPage tab)
        {
            tab.SuspendLayout();
            tab.BackColor = CREME_WARM;

            // Boutons
            var pnlBtnProd = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = CREME_WARM };
            var btnAddProd    = MakeBoutiqueButton("+ Publier fiche", new Point(12, 6));
            var btnEditProd   = MakeBoutiqueButton("\u270f Modifier",   new Point(162, 6));
            var btnToggleProd = MakeBoutiqueButton("\u2b06\u2b07 Publier/Dépublier", new Point(282, 6));
            var btnDelProd    = MakeBoutiqueButton("\U0001f5d1 Supprimer", new Point(460, 6));
            btnAddProd.Click    += (s, e) => ProdOuvrirForm(null);
            btnEditProd.Click   += (s, e) => ProdOuvrirForm(ProdSelected());
            btnToggleProd.Click += (s, e) => ProdToggle();
            btnDelProd.Click    += (s, e) => ProdSupprimer();
            pnlBtnProd.Controls.AddRange(new Control[] { btnAddProd, btnEditProd, btnToggleProd, btnDelProd });

            // DGV
            _dgvProduits = MakeBoutiqueDgv();
            _dgvProduits.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn  { Name = "Id",              HeaderText = "ID",        DataPropertyName = "Id",              FillWeight = 6,  MinimumWidth = 40 },
                new DataGridViewTextBoxColumn  { Name = "NomCommercial",   HeaderText = "Produit",   DataPropertyName = "NomCommercial",   FillWeight = 25, MinimumWidth = 120 },
                new DataGridViewTextBoxColumn  { Name = "NomCategorie",    HeaderText = "Catégorie", DataPropertyName = "NomCategorie",    FillWeight = 15, MinimumWidth = 80 },
                new DataGridViewTextBoxColumn  { Name = "PrixVente",       HeaderText = "Prix (€)",  DataPropertyName = "PrixVente",       FillWeight = 10, MinimumWidth = 70,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn  { Name = "StockUnites",     HeaderText = "Stock",     DataPropertyName = "StockUnites",     FillWeight = 10, MinimumWidth = 60,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewCheckBoxColumn { Name = "EnVente",         HeaderText = "Publié",    DataPropertyName = "EnVente",         FillWeight = 8,  MinimumWidth = 55 },
                new DataGridViewTextBoxColumn  { Name = "NomFiche",        HeaderText = "Fiche BOM", DataPropertyName = "NomFiche",        FillWeight = 20, MinimumWidth = 100 }
            });
            _dgvProduits.CellFormatting += ProdDgvFormatting;
            _dgvProduits.DoubleClick    += (s, e) => ProdOuvrirForm(ProdSelected());

            tab.Controls.Add(_dgvProduits);
            tab.Controls.Add(pnlBtnProd);
            tab.ResumeLayout(true);

            ProdRefresh();
        }

        private ProduitWeb ProdSelected()
        {
            if (_dgvProduits == null || _dgvProduits.CurrentRow == null) return null;
            return _dgvProduits.CurrentRow.DataBoundItem as ProduitWeb;
        }

        private void ProdRefresh()
        {
            if (_dgvProduits == null) return;
            _dgvProduits.DataSource = null;
            var data = ProduitWebDAL.GetAll();
            _dgvProduits.DataSource = data;
            _dgvProduits.Columns["Id"].Visible = false;
        }

        private void ProdOuvrirForm(ProduitWeb prod)
        {
            using (var frm = new FrmProduitWebEdit(prod))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                    ProdRefresh();
            }
        }

        private void ProdToggle()
        {
            var prod = ProdSelected();
            if (prod == null) return;
            ProduitWebDAL.ToggleEnVente(prod.Id);
            ProdRefresh();
        }

        private void ProdSupprimer()
        {
            var prod = ProdSelected();
            if (prod == null) return;

            if (!ProduitWebDAL.PeutSupprimer(prod.Id))
            {
                MessageBox.Show("Ce produit ne peut pas être supprimé car il est référencé dans des commandes.\n\nUtilisez la dépublication à la place.",
                    "Suppression impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var res = MessageBox.Show($"Supprimer le produit « {prod.NomCommercial} » de la boutique ?",
                "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                ProduitWebDAL.Delete(prod.Id);
                ProdRefresh();
            }
        }

        private void ProdDgvFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvProduits.Columns[e.ColumnIndex].Name == "StockUnites" && e.Value != null)
            {
                var val = Convert.ToInt32(e.Value);
                e.Value = $"{val} unité{(val != 1 ? "s" : "")}";
                e.CellStyle.ForeColor = val > 0 ? GREEN_OK : RED_CRIT;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ONGLET 3 — COMMANDES
        // ════════════════════════════════════════════════════════════════

        private void BuildTabCommandes(TabPage tab)
        {
            tab.SuspendLayout();
            tab.BackColor = CREME_WARM;

            // Filtre statut
            var pnlFiltre = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = CREME_WARM };
            var lblFiltre = new Label
            {
                Text = "Filtre :", Font = new Font("Segoe UI", 9F),
                ForeColor = CHOCO_MED, AutoSize = true, Location = new Point(12, 10)
            };
            var cboStatut = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(70, 7), Width = 140
            };
            cboStatut.Items.AddRange(new object[] { "Toutes", "payee", "annulee" });
            cboStatut.SelectedIndex = 0;
            cboStatut.SelectedIndexChanged += (s, e) =>
            {
                var filtre = cboStatut.SelectedItem.ToString();
                CmdRefresh(filtre == "Toutes" ? null : filtre);
            };
            pnlFiltre.Controls.AddRange(new Control[] { lblFiltre, cboStatut });

            // Panneau détail (bas)
            _pnlDetailCommande = new Panel
            {
                Dock = DockStyle.Bottom, Height = 160,
                BackColor = Color.White, Padding = new Padding(12),
                Visible = false
            };
            _pnlDetailCommande.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, 0, _pnlDetailCommande.Width, 0);
            };

            // DGV
            _dgvCommandes = MakeBoutiqueDgv();
            _dgvCommandes.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id",          HeaderText = "N°",       DataPropertyName = "Id",              FillWeight = 8,  MinimumWidth = 45 },
                new DataGridViewTextBoxColumn { Name = "Client",      HeaderText = "Client",   DataPropertyName = "NomCompletClient", FillWeight = 30, MinimumWidth = 120 },
                new DataGridViewTextBoxColumn { Name = "DateCmd",     HeaderText = "Date",     DataPropertyName = "DateCommande",    FillWeight = 22, MinimumWidth = 120,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } },
                new DataGridViewTextBoxColumn { Name = "NbArticles",  HeaderText = "Articles", DataPropertyName = "NbArticles",      FillWeight = 10, MinimumWidth = 60,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                new DataGridViewTextBoxColumn { Name = "TotalTtc",    HeaderText = "Total (€)", DataPropertyName = "TotalTtc",       FillWeight = 15, MinimumWidth = 75,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { Name = "Statut",      HeaderText = "Statut",   DataPropertyName = "Statut",          FillWeight = 12, MinimumWidth = 70 }
            });
            _dgvCommandes.SelectionChanged += (s, e) => CmdAfficherDetail();

            tab.Controls.Add(_dgvCommandes);
            tab.Controls.Add(_pnlDetailCommande);
            tab.Controls.Add(pnlFiltre);
            tab.ResumeLayout(true);

            CmdRefresh(null);
        }

        private void CmdRefresh(string filtreStatut)
        {
            if (_dgvCommandes == null) return;
            _dgvCommandes.DataSource = null;
            var data = CommandeWebDAL.GetAll(filtreStatut);
            _dgvCommandes.DataSource = data;
            if (_pnlDetailCommande != null) _pnlDetailCommande.Visible = false;
        }

        private void CmdAfficherDetail()
        {
            if (_dgvCommandes.CurrentRow == null) { _pnlDetailCommande.Visible = false; return; }
            var cmd = _dgvCommandes.CurrentRow.DataBoundItem as CommandeWeb;
            if (cmd == null) { _pnlDetailCommande.Visible = false; return; }

            // Charger les lignes
            var detail = CommandeWebDAL.GetById(cmd.Id);
            if (detail == null) return;

            _pnlDetailCommande.Controls.Clear();
            _pnlDetailCommande.Visible = true;

            int y = 4;
            _pnlDetailCommande.Controls.Add(new Label
            {
                Text = $"Commande #{detail.Id} — {detail.NomCompletClient} · {detail.EmailClient}",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, AutoSize = true, Location = new Point(12, y)
            });
            y += 22;

            if (!string.IsNullOrEmpty(detail.AdresseLivraison))
            {
                _pnlDetailCommande.Controls.Add(new Label
                {
                    Text = $"Adresse : {detail.AdresseLivraison}",
                    Font = new Font("Segoe UI", 8.5F), ForeColor = CHOCO_MED,
                    AutoSize = true, Location = new Point(12, y)
                });
                y += 18;
            }
            y += 6;

            foreach (var ligne in detail.Lignes)
            {
                _pnlDetailCommande.Controls.Add(new Label
                {
                    Text = $"  {ligne.NomProduit}  ×{ligne.Quantite}  —  {ligne.SousTotal:N2} €",
                    Font = new Font("Segoe UI", 8.5F), ForeColor = CHOCO_MED,
                    AutoSize = true, Location = new Point(12, y)
                });
                y += 17;
            }

            y += 4;
            _pnlDetailCommande.Controls.Add(new Label
            {
                Text = $"Total : {detail.TotalTtc:N2} €",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = OR, AutoSize = true, Location = new Point(12, y)
            });

            _pnlDetailCommande.Height = Math.Max(y + 28, 100);
        }

        // ════════════════════════════════════════════════════════════════
        //  HELPERS BOUTIQUE
        // ════════════════════════════════════════════════════════════════

        private DataGridView MakeBoutiqueDgv()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = BORDER_CLR,
                Font = new Font("Segoe UI", 9F),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = CREME_WARM,
                    ForeColor = CHOCO_BRAND,
                    SelectionBackColor = CREME_WARM,
                    SelectionForeColor = CHOCO_BRAND,
                    Padding = new Padding(4)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    SelectionBackColor = Color.FromArgb(40, AppColors.Or),
                    SelectionForeColor = CHOCO_BRAND,
                    Padding = new Padding(4, 2, 4, 2)
                },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 30, MinimumHeight = 28 }
            };
        }

        private Button MakeBoutiqueButton(string text, Point location)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = CHOCO_BRAND,
                ForeColor = Color.White,
                Size = new Size(text.Length * 10 + 30, 32),
                Location = location,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = CHOCO_BRAND;
            return btn;
        }
    }
}
