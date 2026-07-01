using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    // ════════════════════════════════════════════════════════════════
    //  FrmPrincipal.Simulation — Simulation, lancement, mini-journal
    //
    //  Ce fichier partial contient le workflow complet de simulation
    //  et de lancement d'une production BOM :
    //    1. Simulation   : vérification stocks via BomProductionDAL.Simuler()
    //    2. DGV          : colonnes, formatage, custom painting (jauge %)
    //    3. Lancement    : exécution FIFO via BomProductionDAL.Executer()
    //    4. Validation   : garde sélection niveau + fiche + quantité
    //    5. Mini-journal : historique LIFO des 10 dernières actions
    //
    //  Flux : ProdBtnSimuler_Click → ProdColoriserLignes → [OK] → ProdBtnLancer_Click
    // ════════════════════════════════════════════════════════════════
    partial class FrmPrincipal
    {
        // Liste des entrées du journal (max 10, LIFO — la plus récente en premier)
        // Partagée avec Production.cs (BuildProdColHistorique, ProdRefreshHistorique)
        private List<string> _prodJournalEntries = new List<string>();

        // ════════════════════════════════════════════════════════════════
        //  Reset simulation
        // ════════════════════════════════════════════════════════════════

        // Remet la simulation à zéro : vide la DGV, efface les labels, désactive "Lancer"
        private void ProdResetSimulation()
        {
            if (_prodLblResultat != null)   _prodLblResultat.Text = "";
            if (_prodLblCoutEstime != null)  _prodLblCoutEstime.Text = "";
            if (_prodDgvSimulation != null)  _prodDgvSimulation.DataSource = null;
            _prodLignesSimulation = null;
            _prodSimulationValide = false;
            if (_prodBtnLancer != null) _prodBtnLancer.Enabled = false;
        }

        // ════════════════════════════════════════════════════════════════
        //  Simulation
        //  Le cœur du workflow production : je vérifie si les stocks suffisent
        //  pour produire le nombre de batches demandé. Si oui, j'active "Lancer".
        // ════════════════════════════════════════════════════════════════

        // Handler du bouton "Simuler" : lance la vérification stock vs besoins
        // Remplit la DGV avec les lignes de simulation (ingrédients nécessaires, disponibles, manquants)
        private void ProdBtnSimuler_Click(object sender, EventArgs e)
        {
            // Vérifie qu'un niveau N2+ et une fiche sont sélectionnés
            if (!ProdSelectionValide()) return;

            var niveau = _prodSelectedNiveau;
            var fiche  = _prodSelectedFiche;

            _prodBtnSimuler.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                // Appel au DAL pour simuler : retourne la liste des inputs avec dispo/manque
                _prodLignesSimulation = BomProductionDAL.Simuler(
                    niveau.Id, fiche.Id, _prodNudQuantite.Value);

                // Je bind les résultats dans la DGV
                _prodDgvSimulation.DataSource = null;
                _prodDgvSimulation.DataSource = _prodLignesSimulation;
                // Colorise les lignes en vert (suffisant) ou rouge (pénurie)
                ProdColoriserLignes();

                int penuries = _prodLignesSimulation.Count(l => l.Manque > 0);

                if (penuries == 0)
                {
                    // Tout est OK — j'affiche le résultat en vert et j'active "Lancer"
                    _prodLblResultat.ForeColor = AppColors.Success;
                    _prodLblResultat.Text = $"✔ Tous les stocks suffisants — {_prodLignesSimulation.Count} input(s) vérifiés";
                    _prodSimulationValide = true;
                    _prodBtnLancer.Enabled = true;

                    // Je calcule aussi le coût estimé de la production
                    try
                    {
                        var rapport = BomCoutDAL.CalculerCout(fiche.Id, _prodNudQuantite.Value);
                        _prodLblCoutEstime.ForeColor = AppColors.Success;
                        _prodLblCoutEstime.Text = rapport.QuantiteOutput > 0
                            ? $"Coût estimé : {rapport.CoutTotal:F2} €  ({rapport.CoutUnitaire:F4} €/{rapport.UniteOutput})  →  {_prodNudQuantite.Value} batch(es) × {fiche.QuantiteOutput} {fiche.UniteOutput}"
                            : "Coût estimé : données de prix manquantes";
                    }
                    catch (Exception exCout)
                    {
                        Trace.TraceError("ProdBtnSimuler — calcul coût : {0}", exCout);
                        _prodLblCoutEstime.ForeColor = RED_CRIT;
                        _prodLblCoutEstime.Text = "Coût non disponible";
                    }
                }
                else
                {
                    // Pénurie détectée — j'affiche en rouge et je bloque le lancement
                    _prodLblResultat.ForeColor = RED_CRIT;
                    _prodLblResultat.Text = $"✘ {penuries} pénurie(s) sur {_prodLignesSimulation.Count} — production bloquée";
                    _prodLblCoutEstime.Text = "";
                    _prodSimulationValide = false;
                    _prodBtnLancer.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur simulation : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _prodBtnSimuler.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  DGV colonnes + formatage
        // ════════════════════════════════════════════════════════════════

        // Initialise les colonnes de la DGV de simulation
        // Colonnes : Ingrédient/Fiche, Nécessaire, Disponible, Manque, Stock % (jauge), Unité (hidden)
        private void ProdInitSimColumns()
        {
            var dgv = _prodDgvSimulation;
            dgv.Columns.Clear();

            // Colonne nom de l'input (ingrédient ou fiche du niveau inférieur)
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NomInput", HeaderText = "Ingrédient / Fiche",
                DataPropertyName = "NomInput",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 120,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            // Quantité nécessaire pour produire les batches demandés
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "QuantiteNecessaire", HeaderText = "Nécessaire",
                DataPropertyName = "QuantiteNecessaire",
                Width = 110, MinimumWidth = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            // Quantité actuellement disponible en stock
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "QuantiteDisponible", HeaderText = "Disponible",
                DataPropertyName = "QuantiteDisponible",
                Width = 110, MinimumWidth = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            // Quantité manquante (0 si OK)
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Manque", HeaderText = "Manque",
                DataPropertyName = "Manque",
                Width = 100, MinimumWidth = 70,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            // Jauge visuelle % — dessinée en custom via CellPainting, pas de databinding
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Gauge", HeaderText = "Conso %",
                Width = 100, MinimumWidth = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
            });
            // Colonne unité cachée — utilisée par le CellFormatting pour formater les quantités
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Unite", DataPropertyName = "Unite", Visible = false
            });
        }

        // CellFormatting : je formate les quantités avec les bonnes unités (kg, L, pce...)
        // et j'affiche un tiret pour les manques à 0
        private void ProdDgvSim_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || _prodLignesSimulation == null || e.RowIndex >= _prodLignesSimulation.Count) return;
            var col   = _prodDgvSimulation.Columns[e.ColumnIndex];
            var ligne = _prodLignesSimulation[e.RowIndex];
            string u  = ligne.Unite ?? "";

            if (col.Name == "QuantiteNecessaire")
                e.Value = UnitConvertisseur.FormatQte(ligne.QuantiteNecessaire, u);
            else if (col.Name == "QuantiteDisponible")
                e.Value = UnitConvertisseur.FormatQte(ligne.QuantiteDisponible, u);
            else if (col.Name == "Manque")
                e.Value = ligne.Manque > 0 ? UnitConvertisseur.FormatQte(ligne.Manque, u) : "—";
            else if (col.Name == "Gauge")
            {
                // La jauge est dessinée dans CellPainting, je mets juste une valeur vide ici
                e.Value = "";
                e.FormattingApplied = true;
            }
        }

        // CellPainting : barre qui montre quel % du stock total sera consommé par cette production
        // Vert = consomme peu (<50%), Orange = consomme beaucoup (50-80%), Rouge = consomme presque tout (>80%)
        private void ProdDgvSimulation_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || _prodLignesSimulation == null || e.RowIndex >= _prodLignesSimulation.Count) return;
            if (e.ColumnIndex < 0) return;

            var dgv = _prodDgvSimulation;
            if (dgv.Columns[e.ColumnIndex].Name != "Gauge") return;

            e.Handled = true;
            e.PaintBackground(e.ClipBounds, true);

            var ligne = _prodLignesSimulation[e.RowIndex];
            if (ligne.QuantiteDisponible <= 0 && ligne.QuantiteNecessaire <= 0) return;

            // % du stock total qui sera consommé = Nécessaire / Disponible
            // Ex: besoin 500g, dispo 2000g → 25% du stock sera consommé
            // Si dispo = 0, on est à 100% (tout consommé, stock épuisé)
            double pct = ligne.QuantiteDisponible > 0
                ? Math.Min(1.0, (double)(ligne.QuantiteNecessaire / ligne.QuantiteDisponible))
                : 1.0;
            int pctInt = (int)(pct * 100);

            var rect = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 6,
                                     e.CellBounds.Width - 8, e.CellBounds.Height - 12);

            // Fond gris clair
            using (var br = new SolidBrush(Color.FromArgb(235, 232, 225)))
                e.Graphics.FillRectangle(br, rect);

            // Couleur selon l'impact sur le stock :
            // Vert = faible impact, Orange = impact moyen, Rouge = quasi épuisement
            Color barColor = pct <= 0.50 ? AppColors.Success
                           : pct <= 0.80 ? ORG_WARN
                           : RED_CRIT;
            int barW = Math.Max(1, (int)(rect.Width * pct));
            using (var br = new SolidBrush(barColor))
                e.Graphics.FillRectangle(br, rect.X, rect.Y, barW, rect.Height);

            // Texte centré dans la barre
            string pctTxt = $"{pctInt}%";
            using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            using (var br = new SolidBrush(pct >= 0.4 ? Color.White : CHOCO_BRAND))
            {
                var sz = e.Graphics.MeasureString(pctTxt, f);
                float tx = rect.X + (rect.Width - sz.Width) / 2;
                float ty = rect.Y + (rect.Height - sz.Height) / 2;
                e.Graphics.DrawString(pctTxt, f, br, tx, ty);
            }
        }

        // Colorise chaque ligne de la DGV en vert (stock suffisant) ou rouge (pénurie)
        // Permet un repérage visuel immédiat des problèmes
        private void ProdColoriserLignes()
        {
            if (_prodLignesSimulation == null) return;
            for (int i = 0; i < _prodDgvSimulation.Rows.Count && i < _prodLignesSimulation.Count; i++)
            {
                bool ok = _prodLignesSimulation[i].Manque <= 0;
                _prodDgvSimulation.Rows[i].DefaultCellStyle.ForeColor = ok ? AppColors.Success : RED_CRIT;
                _prodDgvSimulation.Rows[i].DefaultCellStyle.BackColor = ok
                    ? Color.FromArgb(240, 255, 240)
                    : Color.FromArgb(255, 240, 240);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Lancement production
        // ════════════════════════════════════════════════════════════════

        // Handler du bouton "Lancer" — exécute réellement la production
        // Consomme le stock du niveau N-1 et crée un lot dans le stock du niveau courant
        // Async pour ne pas bloquer l'UI pendant l'exécution (appel DB potentiellement long)
        private async void ProdBtnLancer_Click(object sender, EventArgs e)
        {
            try
            {
                // Double vérification : sélection valide ET simulation réussie
                if (!ProdSelectionValide() || !_prodSimulationValide) return;

                var niveau = _prodSelectedNiveau;
                var fiche  = _prodSelectedFiche;
                int nbUnites = (int)_prodNudQuantite.Value;  // 1 batch = 1 unité produite

                // Confirmation détaillée avant lancement — on montre tout ce qui va se passer
                var confirm = MessageBox.Show(
                    $"Lancer la production ?\n\n" +
                    $"Fiche    : {fiche.Nom}\n" +
                    $"Niveau   : {niveau.Nom} (N{niveau.Ordre})\n" +
                    $"Quantité : {nbUnites} unité{(nbUnites != 1 ? "s" : "")} ({nbUnites} × {UnitConvertisseur.FormatQte(fiche.QuantiteOutput, fiche.UniteOutput)}/unité)\n\n" +
                    $"Le stock du niveau N{niveau.Ordre - 1} sera consommé.",
                    "Confirmation de production",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes) return;

                // Je désactive les boutons pour éviter les doubles clics
                _prodBtnLancer.Enabled  = false;
                _prodBtnSimuler.Enabled = false;
                Cursor = Cursors.WaitCursor;

                try
                {
                    string notes = string.IsNullOrWhiteSpace(_prodTxtNotes.Text) ? null : _prodTxtNotes.Text.Trim();
                    int delaiJ   = (int)_prodNudDelai.Value;

                    // Exécution en background thread pour ne pas bloquer l'UI
                    int idProd = await System.Threading.Tasks.Task.Run(() =>
                        BomProductionDAL.Executer(niveau.Id, fiche.Id, _prodNudQuantite.Value, notes, delaiJ));

                    // Confirmation de succès
                    MessageBox.Show(
                        $"Production #{idProd} enregistrée.\n" +
                        $"{nbUnites} unité{(nbUnites != 1 ? "s" : "")} de « {fiche.Nom} ».",
                        "Production réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // J'ajoute une entrée dans le mini-journal
                    ProdAjouterJournalEntry($"▶ Production #{idProd} — {fiche.Nom} × {nbUnites} unité{(nbUnites != 1 ? "s" : "")}");

                    _prodTxtNotes.Clear();

                    // Rafraîchir tout : historique, stock, KPIs, et re-simuler automatiquement
                    ProdRefreshHistorique();
                    ProdRefreshStockCompact(niveau);
                    ProdRefreshKpi();
                    // Re-simulation auto pour voir le nouveau état du stock après production
                    ProdBtnSimuler_Click(sender, e);
                }
                catch (InvalidOperationException ex)
                {
                    // Stock insuffisant entre la simulation et le lancement (race condition possible)
                    MessageBox.Show(ex.Message, "Stock insuffisant", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ProdResetSimulation();
                }
                finally
                {
                    _prodBtnLancer.Enabled  = _prodSimulationValide;
                    _prodBtnSimuler.Enabled = true;
                    Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("ProdBtnLancer_Click : {0}", ex);
                MessageBox.Show("Erreur inattendue : " + ex.Message, "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Validation
        // ════════════════════════════════════════════════════════════════

        // Vérifie que la sélection est valide avant de simuler ou lancer :
        // un niveau N2+ et une fiche doivent être sélectionnés, quantité > 0
        private bool ProdSelectionValide()
        {
            if (_prodSelectedNiveau == null || _prodSelectedFiche == null)
            {
                MessageBox.Show("Sélectionnez un niveau (N2+) et une fiche.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (_prodNudQuantite.Value <= 0)
            {
                MessageBox.Show("La quantité doit être supérieure à 0.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ════════════════════════════════════════════════════════════════
        //  Mini-journal
        //  Petit journal en bas de la colonne droite qui trace les 10 dernières
        //  actions (productions, etc.) avec horodatage. Les entrées du jour
        //  sont affichées en gras pour les repérer facilement.
        // ════════════════════════════════════════════════════════════════

        // Rafraîchit l'affichage du mini-journal dans le panel dédié
        // Supprime les anciens labels et en recrée 5 max
        private void ProdRefreshJournal()
        {
            if (_prodJournalPanel == null) return;

            // Je supprime les labels d'entrées existants (ceux en dessous du titre "JOURNAL")
            var toRemove = _prodJournalPanel.Controls.OfType<Label>()
                .Where(l => l.Location.Y > 20).ToList();
            foreach (var l in toRemove)
            {
                _prodJournalPanel.Controls.Remove(l);
                l.Dispose();
            }

            int y = 24;
            // Je ne garde que les 5 dernières pour le panneau (espace limité)
            string todayPrefix = "[" + DateTime.Now.ToString("dd/MM");
            foreach (var entry in _prodJournalEntries.Take(5))
            {
                // Les entrées du jour sont en gras + couleur plus foncée pour se démarquer
                bool isRecent = entry.StartsWith(todayPrefix);
                _prodJournalPanel.Controls.Add(new Label
                {
                    Text = entry,
                    Font = new Font("Segoe UI", 7.5F, isRecent ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isRecent ? CHOCO_BRAND : CHOCO_MED,
                    Location = new Point(10, y), AutoSize = false,
                    Size = new Size(_prodJournalPanel.Width - 20, 18)
                });
                y += 20;
            }
        }

        // Ajoute une nouvelle entrée horodatée dans le journal
        // L'entrée est insérée en tête de liste (LIFO), et je ne garde que 10 max
        private void ProdAjouterJournalEntry(string message)
        {
            string entry = $"[{DateTime.Now:dd/MM HH:mm}] {message}";
            _prodJournalEntries.Insert(0, entry);
            if (_prodJournalEntries.Count > 10)
                _prodJournalEntries.RemoveAt(_prodJournalEntries.Count - 1);
            ProdRefreshJournal();
        }
    }
}
