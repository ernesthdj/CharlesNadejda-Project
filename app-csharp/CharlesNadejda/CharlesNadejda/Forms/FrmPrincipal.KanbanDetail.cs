using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    public partial class FrmPrincipal
    {
        // ════════════════════════════════════════════════════════════════
        //  Volet detail Kanban (colonne droite)
        // ════════════════════════════════════════════════════════════════

        private void ShowKanbanDetail()
        {
            if (_pnlKanbanDetailContent == null) return;
            _pnlKanbanDetailContent.SuspendLayout();
            _pnlKanbanDetailContent.Controls.Clear();

            // Detail base sur la selection Stock
            if (_dgvStock != null && _dgvStock.CurrentRow != null)
            {
                if (_dgvStock.CurrentRow.DataBoundItem is BomStock bs)
                    RenderStockDetail(bs);
                else if (_dgvStock.CurrentRow.DataBoundItem is Ingredient ingS)
                    RenderIngredientDetail(ingS);
            }

            _pnlKanbanDetailContent.ResumeLayout();
        }

        private void RenderIngredientDetail(Ingredient ing)
        {
            int y = 0;
            y = KDetailHeader(ing.Nom, "Ingrédient", y);

            y = KDetailSection("IDENTITÉ", y);
            if (!string.IsNullOrEmpty(ing.Marque))
                y = KDetailRow("Marque", ing.Marque, y);
            y = KDetailRow("Type", ing.TypePhysique, y);
            y = KDetailRow("Unité", ing.UniteMesure, y);
            if (ing.Densite.HasValue)
                y = KDetailRow("Densité", $"{ing.Densite.Value:F3} g/ml", y);
            if (!string.IsNullOrEmpty(ing.ConditionnementLabel))
                y = KDetailRow("Cond.", ing.ConditionnementLabel, y);
            y = KDetailRow("Qté/cond.", UnitConvertisseur.FormatQte(ing.QteParConditionnement, ing.UniteMesure), y);

            y = KDetailSection("STOCK & PRIX", y);
            y = KDetailRow("En stock", UnitConvertisseur.FormatQte(ing.StockActuel, ing.UniteMesure), y,
                ing.EstEnAlerte ? RED_CRIT : AppColors.Success);
            if (ing.StockPieces > 0)
                y = KDetailRow("Pièces", $"{ing.StockPieces:0} cond.", y);
            if (ing.StockCible.HasValue)
            {
                int pct = (int)(ing.StockRatio.Value * 100);
                y = KDetailRow("Cible", UnitConvertisseur.FormatQte(ing.StockCible.Value, ing.UniteMesure), y);
                y = KDetailRow("Niveau", $"{pct}%", y,
                    pct < 20 ? RED_CRIT : pct < 50 ? AppColors.OrgWarn : AppColors.Success);
            }
            if (ing.SeuilAlerteStock.HasValue)
            {
                y = KDetailRow("Seuil alerte", UnitConvertisseur.FormatQte(ing.SeuilAlerteStock.Value, ing.UniteMesure), y);
                y = KDetailRow("État", ing.EstEnAlerte ? "⚠ EN ALERTE" : "✓ OK", y,
                    ing.EstEnAlerte ? RED_CRIT : AppColors.Success);
            }
            y = KDetailRow("Prix/cond.", UnitConvertisseur.FormatPrix(ing.PrixAchatReference), y);
            if (!string.IsNullOrEmpty(ing.StockNom))
                y = KDetailRow("Emplacement", ing.StockNom, y);
            if (!string.IsNullOrEmpty(ing.NomFournisseur))
                y = KDetailRow("Fournisseur", ing.NomFournisseur, y);

            // Utilisé dans quelles fiches
            try
            {
                var recettes = BomFicheLigneDAL.GetFichesUtilisant(ing.Id);
                if (recettes.Count > 0)
                {
                    y = KDetailSection("UTILISÉ DANS", y);
                    foreach (var r in recettes)
                        y = KDetailTag(r, y);
                }
            }
            catch { }
        }

        private void RenderFicheDetail(BomFiche fiche)
        {
            int y = 0;
            y = KDetailHeader(fiche.Nom, $"Fiche recette — N{fiche.OrdreNiveau}", y);

            y = KDetailSection("IDENTITÉ", y);
            if (!string.IsNullOrEmpty(fiche.Description))
                y = KDetailRow("Description", fiche.Description, y);
            y = KDetailRow("Output", UnitConvertisseur.FormatQte(fiche.QuantiteOutput, fiche.UniteOutput), y);
            if (fiche.TempsPreparation.HasValue)
                y = KDetailRow("Temps prép.", $"{fiche.TempsPreparation.Value} min", y);
            y = KDetailRow("Créée le", fiche.DateCreation.ToString("dd/MM/yyyy"), y);
            y = KDetailRow("Statut", fiche.Actif ? "✓ Active" : "✕ Inactive", y,
                fiche.Actif ? AppColors.Success : RED_CRIT);

            // Composition (lignes de la fiche)
            try
            {
                var ficheComplete = BomFicheDAL.GetById(fiche.Id, avecLignes: true);
                if (ficheComplete?.Lignes != null && ficheComplete.Lignes.Count > 0)
                {
                    y = KDetailSection("COMPOSITION", y);
                    foreach (var ligne in ficheComplete.Lignes)
                    {
                        string qte = UnitConvertisseur.FormatQte(ligne.Quantite, ligne.UniteMesure);
                        y = KDetailRow(ligne.NomInput, qte, y);
                    }
                }
            }
            catch { }

            // Consommé par quelles fiches de niveau supérieur
            try
            {
                var consommePar = BomFicheLigneDAL.GetFichesConsommant(fiche.Id);
                if (consommePar.Count > 0)
                {
                    y = KDetailSection("CONSOMMÉ PAR", y);
                    foreach (var r in consommePar)
                        y = KDetailTag(r, y);
                }
            }
            catch { }
        }

        private void RenderStockDetail(BomStock stock)
        {
            int y = 0;
            y = KDetailHeader(stock.NomFiche, "Produit en stock", y);

            y = KDetailSection("STOCK", y);
            y = KDetailRow("Quantité", UnitConvertisseur.FormatQte(stock.QuantiteDisponible, stock.UniteOutput), y);
            y = KDetailRow("Coût unit.", UnitConvertisseur.FormatPrix(stock.CoutUnitaire), y);
            y = KDetailRow("Valeur", UnitConvertisseur.FormatPrix(stock.CoutTotal), y, CHOCO_BRAND);

            y = KDetailSection("PRODUCTION", y);
            y = KDetailRow("Produit le", stock.DateProduction.ToString("dd/MM/yyyy HH:mm"), y);
            y = KDetailRow("ID prod.", $"#{stock.IdProduction}", y);
            if (stock.DateDlc.HasValue)
            {
                bool expire = stock.DateDlc.Value < DateTime.Today;
                bool proche = !expire && (stock.DateDlc.Value - DateTime.Today).TotalDays < 7;
                y = KDetailRow("DLC", stock.DateDlc.Value.ToString("dd/MM/yyyy"), y,
                    expire ? RED_CRIT : proche ? AppColors.OrgWarn : CHOCO_MED);
            }

            y = KDetailSection("CONTEXTE", y);
            if (!string.IsNullOrEmpty(stock.NomNiveau))
                y = KDetailRow("Niveau", $"N{stock.OrdreNiveau} {stock.NomNiveau}", y);
            if (!string.IsNullOrEmpty(stock.NomContexte))
                y = KDetailRow("Contexte", stock.NomContexte, y);

            // Composition de la fiche source
            try
            {
                var fiche = BomFicheDAL.GetById(stock.IdFiche, avecLignes: true);
                if (fiche?.Lignes != null && fiche.Lignes.Count > 0)
                {
                    y = KDetailSection("COMPOSITION", y);
                    foreach (var ligne in fiche.Lignes)
                    {
                        string qte = UnitConvertisseur.FormatQte(ligne.Quantite, ligne.UniteMesure);
                        y = KDetailRow(ligne.NomInput, qte, y);
                    }
                }
            }
            catch { }
        }

        // ── Helpers rendu detail Kanban ───────────────────────────────

        private int KDetailHeader(string nom, string type, int y)
        {
            var accent = new Panel
            {
                Location = new Point(0, y), Size = new Size(300, 4),
                BackColor = CHOCO_BRAND
            };
            _pnlKanbanDetailContent.Controls.Add(accent);
            y += 8;
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = nom, Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Location = new Point(0, y),
                AutoSize = true, MaximumSize = new Size(280, 0)
            });
            y += 28;
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = type, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = CHOCO_MED, BackColor = Color.FromArgb(240, 235, 225),
                AutoSize = true, Padding = new Padding(6, 2, 6, 2),
                Location = new Point(0, y)
            });
            return y + 24;
        }

        private int KDetailSection(string title, int y)
        {
            y += 6;
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = title, Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, Location = new Point(0, y), AutoSize = true
            });
            y += 18;
            _pnlKanbanDetailContent.Controls.Add(new Panel
            {
                Location = new Point(0, y - 2), Size = new Size(280, 1),
                BackColor = BORDER_CLR
            });
            return y;
        }

        private int KDetailRow(string label, string value, int y, Color? valColor = null)
        {
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = label, Font = new Font("Segoe UI", 8.5F),
                ForeColor = CHOCO_MED, Location = new Point(0, y), AutoSize = true
            });
            _pnlKanbanDetailContent.Controls.Add(new Label
            {
                Text = value ?? "—", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = valColor ?? CHOCO_BRAND,
                Location = new Point(110, y), AutoSize = true, MaximumSize = new Size(180, 0)
            });
            return y + 20;
        }

        private int KDetailTag(string text, int y)
        {
            var tag = new Label
            {
                Text = text, Font = new Font("Segoe UI", 8F),
                ForeColor = CHOCO_BRAND, BackColor = Color.FromArgb(245, 240, 232),
                AutoSize = true, Padding = new Padding(6, 2, 6, 2),
                Location = new Point(0, y)
            };
            tag.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, tag.Width - 1, tag.Height - 1);
            };
            _pnlKanbanDetailContent.Controls.Add(tag);
            return y + 24;
        }
    }
}
