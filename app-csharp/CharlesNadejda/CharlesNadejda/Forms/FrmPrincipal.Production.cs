using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms
{
    partial class FrmPrincipal
    {
        // ════════════════════════════════════════════════════════════════
        //  Écran Production Workflow — inline dans _pnlDroit
        // ════════════════════════════════════════════════════════════════

        // ── État local de la production ──────────────────────────────────
        private ComboBox      _prodCboContexte, _prodCboNiveau, _prodCboFiche;
        private NumericUpDown _prodNudQuantite, _prodNudDelai;
        private TextBox       _prodTxtNotes;
        private Label         _prodLblInfoBatch, _prodLblResultat, _prodLblCoutEstime;
        private DataGridView  _prodDgvSimulation, _prodDgvHistorique;
        private Button        _prodBtnSimuler, _prodBtnLancer;
        private Panel         _prodJournalPanel;
        private List<BomManque> _prodLignesSimulation;
        private bool            _prodSimulationValide;

        private void ShowProductionScreen(NavigationParams p)
        {
            if (_state.ActiveActivite == null) { ShowOnboarding(); return; }
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            _prodLignesSimulation = null;
            _prodSimulationValide = false;

            // ── Chargement données ──────────────────────────────────────
            List<BomProduction> prods  = new List<BomProduction>();
            List<Ingredient>    ings   = new List<Ingredient>();
            List<BomFiche>      fiches = new List<BomFiche>();
            try
            {
                prods  = BomProductionDAL.GetRecentByActivite(_state.ActiveActivite.Id, 10);
                ings   = IngredientDAL.GetAll(idActivite: _state.ActiveActivite.Id);
                fiches = BomFicheDAL.GetAll(idActivite: _state.ActiveActivite.Id);
            }
            catch (Exception ex)
            {
                Trace.TraceError("ShowProductionScreen — chargement DAL : {0}", ex);
            }

            int alertes = ings.Count(i => i.EstEnAlerte);
            int prods7j = prods.Count(pp => pp.DateProduction >= DateTime.Now.AddDays(-7));
            decimal cout7j = prods.Where(pp => pp.DateProduction >= DateTime.Now.AddDays(-7))
                                  .Sum(pp => pp.CoutIngredients);

            // ═══════════════════════════════════════════════════════════
            //  1. HEADER
            // ═══════════════════════════════════════════════════════════
            var pnlHdr = BuildProdHeader();

            // ═══════════════════════════════════════════════════════════
            //  2. KPI BAR
            // ═══════════════════════════════════════════════════════════
            var pnlKpi = BuildProdKpiBar(prods7j, cout7j, alertes, fiches.Count);

            // ═══════════════════════════════════════════════════════════
            //  SCROLLABLE BODY
            // ═══════════════════════════════════════════════════════════
            var pnlBody = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = CREME_WARM, Padding = new Padding(20, 12, 20, 20)
            };

            int y = 0;

            // ═══════════════════════════════════════════════════════════
            //  3. SECTION PARAMÈTRES
            // ═══════════════════════════════════════════════════════════
            var pnlParams = BuildProdParams();
            pnlParams.Location = new Point(0, y);
            pnlBody.Controls.Add(pnlParams);
            y += pnlParams.Height + 16;

            // ═══════════════════════════════════════════════════════════
            //  5. SECTION SIMULATION
            // ═══════════════════════════════════════════════════════════
            var pnlSim = BuildProdSimulation();
            pnlSim.Location = new Point(0, y);
            pnlBody.Controls.Add(pnlSim);
            y += pnlSim.Height + 16;

            // ═══════════════════════════════════════════════════════════
            //  6. HISTORIQUE PRODUCTIONS
            // ═══════════════════════════════════════════════════════════
            var pnlHist = BuildProdHistorique(prods);
            pnlHist.Location = new Point(0, y);
            pnlBody.Controls.Add(pnlHist);
            y += pnlHist.Height + 16;

            // ═══════════════════════════════════════════════════════════
            //  7. MINI-JOURNAL
            // ═══════════════════════════════════════════════════════════
            _prodJournalPanel = BuildProdJournal(prods);
            _prodJournalPanel.Location = new Point(0, y);
            pnlBody.Controls.Add(_prodJournalPanel);

            // ── Resize handler pour largeur fluide ──────────────────────
            pnlBody.Resize += (s, ev) =>
            {
                int w = pnlBody.ClientSize.Width - 40;
                foreach (Control c in pnlBody.Controls)
                    if (c is Panel) c.Width = w;
            };
            // Déclencher une fois
            int initW = pnlBody.ClientSize.Width - 40;
            foreach (Control c in pnlBody.Controls)
                if (c is Panel) c.Width = initW;

            // ── Assemblage final (DockStyle order: Fill first) ──────────
            _pnlDroit.Controls.Add(pnlBody);
            _pnlDroit.Controls.Add(pnlKpi);
            _pnlDroit.Controls.Add(pnlHdr);

            _pnlDroit.ResumeLayout();

            // ── Charger les combos après affichage ──────────────────────
            ProdChargerContextes();
        }

        // ════════════════════════════════════════════════════════════════
        //  HEADER
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdHeader()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top, Height = 52,
                BackColor = CREME_WARM, Padding = new Padding(20, 0, 20, 0)
            };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawLine(pen, 0, pnl.Height - 1, pnl.Width, pnl.Height - 1);
            };
            pnl.Controls.Add(new Label
            {
                Text = "Production", Location = new Point(20, 6),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, AutoSize = true
            });
            pnl.Controls.Add(new Label
            {
                Text = "Simulation, lancement et suivi des productions BOM",
                Location = new Point(20, 30), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = CHOCO_MED, AutoSize = true
            });
            return pnl;
        }

        // ════════════════════════════════════════════════════════════════
        //  KPI BAR
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdKpiBar(int prods7j, decimal cout7j, int alertes, int fichesCount)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top, Height = 92,
                BackColor = CREME_WARM, Padding = new Padding(16, 12, 16, 8)
            };

            var c1 = MakeStatCard("▶", "Productions 7j", prods7j.ToString(),
                prods7j > 0 ? "dernières 7 jours" : "Aucune", prods7j > 0 ? "success" : "");
            var c2 = MakeStatCard("💰", "Coût 7j", $"{cout7j:F2} €",
                cout7j > 0 ? "total ingrédients" : "", "gold");
            var c3 = MakeStatCard("⚠", "Alertes stock", alertes.ToString(),
                alertes > 0 ? "ingrédients bas" : "Tout OK", alertes > 0 ? "danger" : "");
            var c4 = MakeStatCard("🧪", "Fiches actives", fichesCount.ToString(),
                "recettes configurées", "");

            pnl.Resize += (s, ev) =>
            {
                int w = (pnl.ClientSize.Width - 32 - 12 * 3) / 4;
                for (int i = 0; i < pnl.Controls.Count; i++)
                    pnl.Controls[i].SetBounds(16 + i * (w + 12), 12, w, 68);
            };
            pnl.Controls.Add(c1);
            pnl.Controls.Add(c2);
            pnl.Controls.Add(c3);
            pnl.Controls.Add(c4);

            return pnl;
        }

        // ════════════════════════════════════════════════════════════════
        //  SECTION PARAMÈTRES
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdParams()
        {
            var pnl = new Panel { Height = 220, BackColor = Color.White };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var br = new SolidBrush(CHOCO_BRAND))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, pnl.Height);
            };

            // Titre section
            pnl.Controls.Add(new Label
            {
                Text = "PARAMÈTRES DE PRODUCTION", Location = new Point(14, 10),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true
            });

            int lx = 14, ly = 34;

            // Ligne 1 : Contexte, Niveau, Fiche
            pnl.Controls.Add(MakeProdLabel("Contexte", lx, ly));
            _prodCboContexte = MakeProdCombo(lx, ly + 18, 190);
            _prodCboContexte.SelectedIndexChanged += ProdContexte_Changed;
            pnl.Controls.Add(_prodCboContexte);

            pnl.Controls.Add(MakeProdLabel("Niveau", lx + 200, ly));
            _prodCboNiveau = MakeProdCombo(lx + 200, ly + 18, 160);
            _prodCboNiveau.SelectedIndexChanged += ProdNiveau_Changed;
            pnl.Controls.Add(_prodCboNiveau);

            pnl.Controls.Add(MakeProdLabel("Fiche recette", lx + 370, ly));
            _prodCboFiche = MakeProdCombo(lx + 370, ly + 18, 230);
            _prodCboFiche.SelectedIndexChanged += ProdFiche_Changed;
            pnl.Controls.Add(_prodCboFiche);

            // Ligne 2 : Batches, Délai conservation, Simuler
            int ly2 = ly + 52;
            pnl.Controls.Add(MakeProdLabel("Nb batches", lx, ly2));
            _prodNudQuantite = new NumericUpDown
            {
                DecimalPlaces = 2, Minimum = 0.01m, Maximum = 100000, Value = 1,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(lx, ly2 + 18), Size = new Size(100, 24)
            };
            pnl.Controls.Add(_prodNudQuantite);

            _prodLblInfoBatch = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = CHOCO_MED, Location = new Point(lx + 108, ly2 + 20),
                AutoSize = true
            };
            pnl.Controls.Add(_prodLblInfoBatch);

            pnl.Controls.Add(MakeProdLabel("Délai conserv. (j)", lx + 370, ly2));
            _prodNudDelai = new NumericUpDown
            {
                Minimum = 0, Maximum = 3650, Value = 0,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(lx + 370, ly2 + 18), Size = new Size(80, 24)
            };
            pnl.Controls.Add(_prodNudDelai);

            _prodBtnSimuler = new Button
            {
                Text = "⚡  Simuler", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = AppColors.Info, ForeColor = Color.White,
                Size = new Size(140, 30), Location = new Point(lx + 470, ly2 + 16), Cursor = Cursors.Hand
            };
            _prodBtnSimuler.FlatAppearance.BorderColor = Color.FromArgb(40, 85, 160);
            _prodBtnSimuler.Click += ProdBtnSimuler_Click;
            pnl.Controls.Add(_prodBtnSimuler);

            // Ligne 3 : Notes
            int ly3 = ly2 + 52;
            pnl.Controls.Add(MakeProdLabel("Notes (optionnel)", lx, ly3));
            _prodTxtNotes = new TextBox
            {
                Font = new Font("Segoe UI", 9.5F), Location = new Point(lx, ly3 + 18),
                Size = new Size(580, 24)
            };
            pnl.Controls.Add(_prodTxtNotes);

            // Resize pour étirer les combos et champs
            pnl.Resize += (s, ev) =>
            {
                int w = pnl.Width - 28;
                _prodTxtNotes.Width = Math.Max(200, w - 14);
                int ficheW = Math.Max(150, w - 370 - 14);
                _prodCboFiche.Width = ficheW;
            };

            return pnl;
        }

        private static Label MakeProdLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text, Font = new Font("Segoe UI", 8.5F),
                ForeColor = CHOCO_MED, Location = new Point(x, y), AutoSize = true
            };
        }

        private static ComboBox MakeProdCombo(int x, int y, int w)
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(x, y), Size = new Size(w, 24)
            };
        }

        // ── Cascade Contexte → Niveau → Fiche ──────────────────────────

        private void ProdChargerContextes()
        {
            _prodCboContexte.Items.Clear();
            foreach (var ctx in BomContexteDAL.GetAll(_state.ActiveActivite.Id))
                _prodCboContexte.Items.Add(ctx);

            if (_prodCboContexte.Items.Count == 0)
            {
                _prodLblResultat.ForeColor = CHOCO_MED;
                _prodLblResultat.Text = "Aucun contexte BOM — créez-en depuis la vue stock.";
                _prodBtnSimuler.Enabled = false;
            }
            else
            {
                // Pré-sélectionner le contexte actif si disponible
                bool found = false;
                if (_state.ActiveContexte != null)
                {
                    for (int i = 0; i < _prodCboContexte.Items.Count; i++)
                    {
                        if (((BomContexte)_prodCboContexte.Items[i]).Id == _state.ActiveContexte.Id)
                        {
                            _prodCboContexte.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                    _prodCboContexte.SelectedIndex = 0;
            }
        }

        private void ProdContexte_Changed(object sender, EventArgs e)
        {
            _prodCboNiveau.Items.Clear();
            _prodCboFiche.Items.Clear();
            ProdResetSimulation();

            if (!(_prodCboContexte.SelectedItem is BomContexte ctx)) return;

            foreach (var niv in BomNiveauDAL.GetByContexte(ctx.Id))
                if (niv.Ordre > 1) _prodCboNiveau.Items.Add(niv);

            if (_prodCboNiveau.Items.Count > 0)
            {
                // Pré-sélectionner le niveau actif si disponible
                bool found = false;
                if (_state.ActiveNiveau != null)
                {
                    for (int i = 0; i < _prodCboNiveau.Items.Count; i++)
                    {
                        if (((BomNiveau)_prodCboNiveau.Items[i]).Id == _state.ActiveNiveau.Id)
                        {
                            _prodCboNiveau.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                    _prodCboNiveau.SelectedIndex = 0;
            }
        }

        private void ProdNiveau_Changed(object sender, EventArgs e)
        {
            _prodCboFiche.Items.Clear();
            ProdResetSimulation();

            if (!(_prodCboNiveau.SelectedItem is BomNiveau niv)) return;

            foreach (var f in BomFicheDAL.GetByNiveau(niv.Id))
                _prodCboFiche.Items.Add(f);

            if (_prodCboFiche.Items.Count > 0)
                _prodCboFiche.SelectedIndex = 0;
        }

        private void ProdFiche_Changed(object sender, EventArgs e)
        {
            ProdResetSimulation();
            var fiche = _prodCboFiche.SelectedItem as BomFiche;
            _prodLblInfoBatch.Text = fiche != null
                ? $"1 batch = {fiche.QuantiteOutput} {fiche.UniteOutput}"
                : "";

        }

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
        //  SECTION SIMULATION (DGV avec gauge)
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdSimulation()
        {
            var pnl = new Panel { Height = 340, BackColor = Color.White };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var br = new SolidBrush(AppColors.Info))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, pnl.Height);
            };

            pnl.Controls.Add(new Label
            {
                Text = "RÉSULTAT SIMULATION", Location = new Point(14, 10),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true
            });

            _prodLblResultat = new Label
            {
                Text = "", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(14, 30), Size = new Size(700, 20)
            };
            pnl.Controls.Add(_prodLblResultat);

            _prodLblCoutEstime = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = AppColors.Success, Location = new Point(14, 52), Size = new Size(700, 16)
            };
            pnl.Controls.Add(_prodLblCoutEstime);

            _prodDgvSimulation = BuildProdDgv();
            _prodDgvSimulation.Location = new Point(14, 72);
            _prodDgvSimulation.Size     = new Size(700, 200);
            _prodDgvSimulation.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ProdInitSimColumns();
            _prodDgvSimulation.CellPainting += ProdDgvSimulation_CellPainting;
            _prodDgvSimulation.CellFormatting += ProdDgvSim_CellFormatting;
            pnl.Controls.Add(_prodDgvSimulation);

            // Bouton Lancer
            _prodBtnLancer = new Button
            {
                Text = "▶  Lancer la production",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat, BackColor = AppColors.Success, ForeColor = Color.White,
                Size = new Size(210, 34), Location = new Point(14, 280),
                Enabled = false, Cursor = Cursors.Hand
            };
            _prodBtnLancer.FlatAppearance.BorderColor = Color.FromArgb(20, 100, 50);
            _prodBtnLancer.Click += ProdBtnLancer_Click;
            pnl.Controls.Add(_prodBtnLancer);

            // Label légende
            pnl.Controls.Add(new Label
            {
                Text = "■ Suffisant   ■ Insuffisant   Barre = % disponible",
                Font = new Font("Segoe UI", 7.5F), ForeColor = Color.Gray,
                Location = new Point(240, 288), AutoSize = true
            });

            // Resize DGV
            pnl.Resize += (s, ev) =>
            {
                _prodDgvSimulation.Width = Math.Max(300, pnl.Width - 28);
            };

            return pnl;
        }

        private DataGridView BuildProdDgv()
        {
            var dgv = new DataGridView
            {
                Font = new Font("Segoe UI", 9F), BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, GridColor = BORDER_CLR,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false, MultiSelect = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 28,
                AutoGenerateColumns = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = CREME;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = CHOCO_BRAND;
            dgv.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = CREME;
            dgv.DefaultCellStyle.SelectionBackColor = CHOCO_BRAND;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 247, 244);
            return dgv;
        }

        // ── Simulation ──────────────────────────────────────────────────

        private void ProdBtnSimuler_Click(object sender, EventArgs e)
        {
            if (!ProdSelectionValide()) return;

            var niveau = _prodCboNiveau.SelectedItem as BomNiveau;
            var fiche  = _prodCboFiche.SelectedItem  as BomFiche;

            _prodBtnSimuler.Enabled = false;
            Cursor = Cursors.WaitCursor;
            try
            {
                _prodLignesSimulation = BomProductionDAL.Simuler(
                    niveau.Id, fiche.Id, _prodNudQuantite.Value);

                _prodDgvSimulation.DataSource = null;
                _prodDgvSimulation.DataSource = _prodLignesSimulation;
                ProdColoriserLignes();

                int penuries = _prodLignesSimulation.Count(l => l.Manque > 0);

                if (penuries == 0)
                {
                    _prodLblResultat.ForeColor = AppColors.Success;
                    _prodLblResultat.Text = $"✔ Tous les stocks suffisants — {_prodLignesSimulation.Count} input(s) vérifiés";
                    _prodSimulationValide = true;
                    _prodBtnLancer.Enabled = true;

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
                        Trace.TraceError("ShowProductionScreen — calcul coût : {0}", exCout);
                        _prodLblCoutEstime.ForeColor = RED_CRIT;
                        _prodLblCoutEstime.Text = "Coût non disponible";
                    }
                }
                else
                {
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

        /// <summary>
        /// Crée les colonnes manuelles une seule fois (AutoGenerateColumns = false).
        /// Largeurs fixes verrouillées — le visuel reste identique entre simulations.
        /// </summary>
        private void ProdInitSimColumns()
        {
            var dgv = _prodDgvSimulation;
            dgv.Columns.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NomInput", HeaderText = "Ingrédient / Fiche",
                DataPropertyName = "NomInput",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 120,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "QuantiteNecessaire", HeaderText = "Nécessaire",
                DataPropertyName = "QuantiteNecessaire",
                Width = 110, MinimumWidth = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "QuantiteDisponible", HeaderText = "Disponible",
                DataPropertyName = "QuantiteDisponible",
                Width = 110, MinimumWidth = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Manque", HeaderText = "Manque",
                DataPropertyName = "Manque",
                Width = 100, MinimumWidth = 70,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Gauge", HeaderText = "Stock %",
                Width = 100, MinimumWidth = 80,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable
            });

            // Colonne cachée pour l'unité (utilisée par CellFormatting)
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Unite", DataPropertyName = "Unite", Visible = false
            });
        }

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
                // La gauge est dessinée via CellPainting, on vide le texte
                e.Value = "";
                e.FormattingApplied = true;
            }
        }

        /// <summary>
        /// Custom painting pour la colonne "Gauge" — barre de progression dans la cellule.
        /// </summary>
        private void ProdDgvSimulation_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || _prodLignesSimulation == null || e.RowIndex >= _prodLignesSimulation.Count) return;
            if (e.ColumnIndex < 0) return;

            var dgv = _prodDgvSimulation;
            if (dgv.Columns[e.ColumnIndex].Name != "Gauge") return;

            e.Handled = true;
            e.PaintBackground(e.ClipBounds, true);

            var ligne = _prodLignesSimulation[e.RowIndex];
            if (ligne.QuantiteNecessaire <= 0) return;

            double pct = Math.Min(1.0, (double)(ligne.QuantiteDisponible / ligne.QuantiteNecessaire));
            int pctInt  = (int)(pct * 100);

            // Zone de la barre
            var rect = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 6,
                                     e.CellBounds.Width - 8, e.CellBounds.Height - 12);

            // Fond gris
            using (var br = new SolidBrush(Color.FromArgb(235, 232, 225)))
                e.Graphics.FillRectangle(br, rect);

            // Barre colorée
            Color barColor = pct >= 1.0 ? AppColors.Success
                           : pct >= 0.5 ? ORG_WARN
                           : RED_CRIT;
            int barW = Math.Max(1, (int)(rect.Width * pct));
            using (var br = new SolidBrush(barColor))
                e.Graphics.FillRectangle(br, rect.X, rect.Y, barW, rect.Height);

            // Texte pourcentage
            string pctTxt = $"{pctInt}%";
            using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            using (var br = new SolidBrush(pct >= 0.5 ? Color.White : CHOCO_BRAND))
            {
                var sz = e.Graphics.MeasureString(pctTxt, f);
                float tx = rect.X + (rect.Width - sz.Width) / 2;
                float ty = rect.Y + (rect.Height - sz.Height) / 2;
                e.Graphics.DrawString(pctTxt, f, br, tx, ty);
            }
        }

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

        // ── Lancement production ────────────────────────────────────────

        private async void ProdBtnLancer_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ProdSelectionValide() || !_prodSimulationValide) return;

                var niveau = _prodCboNiveau.SelectedItem as BomNiveau;
                var fiche  = _prodCboFiche.SelectedItem  as BomFiche;
                decimal qteTotale = _prodNudQuantite.Value * fiche.QuantiteOutput;

                var confirm = MessageBox.Show(
                    $"Lancer la production ?\n\n" +
                    $"Fiche    : {fiche.Nom}\n" +
                    $"Niveau   : {niveau.Nom} (N{niveau.Ordre})\n" +
                    $"Batches  : {_prodNudQuantite.Value} × {fiche.QuantiteOutput} {fiche.UniteOutput} = {qteTotale} {fiche.UniteOutput}\n\n" +
                    $"Le stock du niveau N{niveau.Ordre - 1} sera consommé.",
                    "Confirmation de production",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes) return;

                _prodBtnLancer.Enabled  = false;
                _prodBtnSimuler.Enabled = false;
                Cursor = Cursors.WaitCursor;

                try
                {
                    string notes = string.IsNullOrWhiteSpace(_prodTxtNotes.Text) ? null : _prodTxtNotes.Text.Trim();
                    int delaiJ   = (int)_prodNudDelai.Value;

                    int idProd = await System.Threading.Tasks.Task.Run(() =>
                        BomProductionDAL.Executer(niveau.Id, fiche.Id, _prodNudQuantite.Value, notes, delaiJ));

                    MessageBox.Show(
                        $"Production #{idProd} enregistrée.\n" +
                        $"{_prodNudQuantite.Value} batch(es) → {qteTotale} {fiche.UniteOutput} de « {fiche.Nom} ».",
                        "Production réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Ajouter au mini-journal
                    ProdAjouterJournalEntry($"▶ Production #{idProd} — {fiche.Nom} × {_prodNudQuantite.Value} batch(es) = {qteTotale} {fiche.UniteOutput}");

                    _prodTxtNotes.Clear();

                    // Rafraîchir historique et simulation
                    ProdRefreshHistorique();
                    ProdBtnSimuler_Click(sender, e);
                }
                catch (InvalidOperationException ex)
                {
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

        private bool ProdSelectionValide()
        {
            if (_prodCboContexte.SelectedItem == null || _prodCboNiveau.SelectedItem == null || _prodCboFiche.SelectedItem == null)
            {
                MessageBox.Show("Sélectionnez un contexte, un niveau et une fiche.", "Validation",
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
        //  HISTORIQUE PRODUCTIONS
        // ════════════════════════════════════════════════════════════════

        private Panel BuildProdHistorique(List<BomProduction> prods)
        {
            var pnl = new Panel { Height = 240, BackColor = Color.White };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var br = new SolidBrush(OR))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, pnl.Height);
            };

            pnl.Controls.Add(new Label
            {
                Text = "HISTORIQUE — 10 DERNIÈRES PRODUCTIONS", Location = new Point(14, 10),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true
            });

            _prodDgvHistorique = BuildProdDgv();
            _prodDgvHistorique.Location = new Point(14, 32);
            _prodDgvHistorique.Size     = new Size(700, 196);
            _prodDgvHistorique.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnl.Controls.Add(_prodDgvHistorique);

            ProdRemplirHistorique(prods);

            pnl.Resize += (s, ev) =>
            {
                _prodDgvHistorique.Width = Math.Max(300, pnl.Width - 28);
            };

            return pnl;
        }

        private void ProdRemplirHistorique(List<BomProduction> prods)
        {
            if (prods.Count > 0)
            {
                _prodDgvHistorique.DataSource = prods.Select(pp => new
                {
                    Date     = pp.DateProduction.ToString("dd/MM HH:mm"),
                    Fiche    = pp.NomFiche,
                    Niveau   = $"N{pp.OrdreNiveau}",
                    Contexte = pp.NomContexte,
                    Produit  = $"{pp.QuantiteProduite:F0}",
                    Coût     = UnitConvertisseur.FormatPrix(pp.CoutIngredients),
                    Notes    = pp.Notes ?? "—"
                }).ToList();
            }
            else
            {
                _prodDgvHistorique.DataSource = null;
            }
        }

        private void ProdRefreshHistorique()
        {
            try
            {
                var prods = BomProductionDAL.GetRecentByActivite(_state.ActiveActivite.Id, 10);
                ProdRemplirHistorique(prods);
            }
            catch (Exception ex)
            {
                Trace.TraceError("ProdRefreshHistorique : {0}", ex);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  MINI-JOURNAL (chatter)
        // ════════════════════════════════════════════════════════════════

        private List<string> _prodJournalEntries = new List<string>();

        private Panel BuildProdJournal(List<BomProduction> prods)
        {
            var pnl = new Panel { Height = 160, BackColor = Color.White };
            pnl.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR, 1))
                    ev.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var br = new SolidBrush(CHOCO_MED))
                    ev.Graphics.FillRectangle(br, 0, 0, 3, pnl.Height);
            };

            pnl.Controls.Add(new Label
            {
                Text = "JOURNAL D'ACTIVITÉ", Location = new Point(14, 10),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = SIDEBAR_META, AutoSize = true
            });

            // Pré-remplir avec les 5 dernières productions
            _prodJournalEntries.Clear();
            foreach (var pp in prods.Take(5))
            {
                _prodJournalEntries.Add(
                    $"[{pp.DateProduction:dd/MM HH:mm}] Production — {pp.NomFiche} × {pp.QuantiteProduite:F0} ({pp.NomContexte})");
            }
            if (_prodJournalEntries.Count == 0)
                _prodJournalEntries.Add($"[{DateTime.Now:dd/MM HH:mm}] Session ouverte — en attente de production");

            ProdRefreshJournal();

            return pnl;
        }

        private void ProdRefreshJournal()
        {
            if (_prodJournalPanel == null) return;

            // Supprimer les labels existants (sauf le titre)
            var toRemove = _prodJournalPanel.Controls.OfType<Label>()
                .Where(l => l.Location.Y > 26).ToList();
            foreach (var l in toRemove)
            {
                _prodJournalPanel.Controls.Remove(l);
                l.Dispose();
            }

            int y = 32;
            string todayPrefix = "[" + DateTime.Now.ToString("dd/MM");
            foreach (var entry in _prodJournalEntries.Take(5))
            {
                bool isRecent = entry.StartsWith(todayPrefix);
                _prodJournalPanel.Controls.Add(new Label
                {
                    Text = entry,
                    Font = new Font("Segoe UI", 8.5F, isRecent ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isRecent ? CHOCO_BRAND : CHOCO_MED,
                    Location = new Point(14, y), AutoSize = false,
                    Size = new Size(_prodJournalPanel.Width - 28, 20)
                });
                y += 22;
            }
        }

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
