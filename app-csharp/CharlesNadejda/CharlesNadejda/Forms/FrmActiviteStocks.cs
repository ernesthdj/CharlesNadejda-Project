using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Gestion des stocks liés à une activité (jonction M:N activites_stocks).
    /// Affiche une CheckedListBox de tous les stocks disponibles.
    /// Les stocks déjà liés apparaissent cochés.
    /// </summary>
    public class FrmActiviteStocks : Form
    {
        private readonly Activite       _activite;
        private CheckedListBox          _clb;
        private HashSet<int>            _idsLies;

        private static readonly Color CHOCOLAT_FONCE = AppColors.ChocoBrand;
        private static readonly Color OR             = AppColors.Or;

        public FrmActiviteStocks(Activite activite)
        {
            _activite = activite;
            BuildUI();
            Load += FrmActiviteStocks_Load;
        }

        private void BuildUI()
        {
            this.Text            = $"Stocks liés — {_activite.Nom}";
            this.ClientSize      = new Size(400, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = Color.White;

            // ── En-tête ──────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = CHOCOLAT_FONCE, Padding = new Padding(16, 0, 16, 0) };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "STOCKS LIÉS",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = OR,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 140,
                TextAlign = ContentAlignment.MiddleLeft
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "Cochez les stocks accessibles depuis cette activité",
                Font      = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(200, 175, 140),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            // ── Note ─────────────────────────────────────────────────────
            var lblNote = new Label
            {
                Text      = "Les ingrédients de cette activité seront cherchés dans les stocks cochés.",
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(100, 80, 60),
                Location  = new Point(16, 60),
                Size      = new Size(368, 32),
                AutoSize  = false
            };

            // ── CheckedListBox ────────────────────────────────────────────
            _clb = new CheckedListBox
            {
                Location       = new Point(16, 96),
                Size           = new Size(368, 220),
                CheckOnClick   = true,
                Font           = new Font("Segoe UI", 10F),
                BorderStyle    = BorderStyle.FixedSingle,
                BackColor      = Color.FromArgb(252, 250, 248)
            };

            // ── Boutons ───────────────────────────────────────────────────
            var btnEnregistrer = new Button
            {
                Text      = "Enregistrer",
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = CHOCOLAT_FONCE,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location  = new Point(16, 330),
                Size      = new Size(160, 36),
                Cursor    = Cursors.Hand
            };
            btnEnregistrer.FlatAppearance.BorderSize = 0;
            btnEnregistrer.Click += BtnEnregistrer_Click;

            var btnAnnuler = new Button
            {
                Text     = "Annuler",
                Font     = new Font("Segoe UI", 10F),
                Location = new Point(224, 330),
                Size     = new Size(160, 36),
                Cursor   = Cursors.Hand
            };
            btnAnnuler.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(pnlHeader);
            this.Controls.Add(lblNote);
            this.Controls.Add(_clb);
            this.Controls.Add(btnEnregistrer);
            this.Controls.Add(btnAnnuler);
        }

        private void FrmActiviteStocks_Load(object sender, EventArgs e)
        {
            try
            {
                // Stocks déjà liés
                var liesListe = StockDAL.GetByActivite(_activite.Id);
                _idsLies = new HashSet<int>();
                foreach (var s in liesListe) _idsLies.Add(s.Id);

                // Tous les stocks
                foreach (var s in StockDAL.GetAll())
                {
                    int idx = _clb.Items.Add(s);
                    _clb.SetItemChecked(idx, _idsLies.Contains(s.Id));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            try
            {
                // Synchronise : ajoute les cochés non encore liés, retire les décochés
                for (int i = 0; i < _clb.Items.Count; i++)
                {
                    var stock   = (Stock)_clb.Items[i];
                    bool coché  = _clb.GetItemChecked(i);
                    bool étaitLié = _idsLies.Contains(stock.Id);

                    if (coché && !étaitLié)
                        StockDAL.LierActivite(_activite.Id, stock.Id);
                    else if (!coché && étaitLié)
                        StockDAL.DelierActivite(_activite.Id, stock.Id);
                }

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
