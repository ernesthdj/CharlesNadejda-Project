using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CharlesNadejda.DAL;

namespace CharlesNadejda.Forms
{
    // ════════════════════════════════════════════════════════════════
    //  FrmPrincipal.Parametres — Ecran de configuration utilisateur
    //
    //  Responsabilite : modification mot de passe, preferences,
    //  informations compte. Accessible via la sidebar "Parametres".
    // ════════════════════════════════════════════════════════════════
    public partial class FrmPrincipal
    {

        private void ShowParametresScreen()
        {
            _pnlDroit.SuspendLayout();
            ClearAndDisposePanel();

            // ── Header ──────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top, Height = 56, BackColor = CREME_WARM,
                Padding = new Padding(24, 0, 0, 0)
            };
            pnlHeader.Paint += (s, ev) =>
            {
                using (var pen = new Pen(BORDER_CLR))
                    ev.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1,
                                              pnlHeader.Width, pnlHeader.Height - 1);
            };
            pnlHeader.Controls.Add(new Label
            {
                Text = "Paramètres", Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            // ── Contenu scrollable ──────────────────────────────────
            var pnlContent = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                BackColor = CREME_BG, Padding = new Padding(32, 24, 32, 24)
            };

            // ── Section : Base de données ───────────────────────────
            var grpDb = MakeParamSection("Base de données", 320);
            int y = 40;

            var lblWarning = new Label
            {
                Text = "Vider la base supprime TOUTES les données applicatives :\n" +
                       "ingrédients, lots, fiches BOM, productions, stocks, commandes web.\n" +
                       "Les comptes utilisateurs sont conservés.\n\n" +
                       "Cette action est irréversible.",
                Font = new Font("Segoe UI", 9.5F), ForeColor = RED_CRIT,
                Location = new Point(16, y), Size = new Size(520, 80)
            };
            grpDb.Controls.Add(lblWarning);
            y += 88;

            // Checkbox confirmation
            var chkConfirm = new CheckBox
            {
                Text = "Je comprends que toutes les données seront supprimées",
                Font = new Font("Segoe UI", 9F), ForeColor = CHOCO_MED,
                Location = new Point(16, y), AutoSize = true, Checked = false
            };
            grpDb.Controls.Add(chkConfirm);
            y += 32;

            // TextBox saisie "VIDER"
            var lblSaisie = new Label
            {
                Text = "Tapez VIDER pour confirmer :",
                Font = new Font("Segoe UI", 9F), ForeColor = CHOCO_MED,
                Location = new Point(16, y), AutoSize = true
            };
            grpDb.Controls.Add(lblSaisie);

            var txtConfirm = new TextBox
            {
                Font = new Font("Segoe UI", 10F), Width = 120,
                Location = new Point(200, y - 2),
                MaxLength = 10
            };
            grpDb.Controls.Add(txtConfirm);
            y += 40;

            // Bouton flush
            var btnFlush = new Button
            {
                Text = "Vider la base de données",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = RED_CRIT, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Location = new Point(16, y), Size = new Size(240, 38),
                Enabled = false
            };
            btnFlush.FlatAppearance.BorderColor = Color.FromArgb(160, 30, 30);

            // Active le bouton seulement si checkbox + "VIDER" saisi
            EventHandler validateFlush = (s, ev) =>
            {
                btnFlush.Enabled = chkConfirm.Checked
                                && txtConfirm.Text.Trim().Equals("VIDER", StringComparison.Ordinal);
            };
            chkConfirm.CheckedChanged += validateFlush;
            txtConfirm.TextChanged    += validateFlush;

            // Label résultat
            var lblResult = new Label
            {
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(270, y + 8), AutoSize = true,
                Text = "", ForeColor = GREEN_OK
            };
            grpDb.Controls.Add(lblResult);

            btnFlush.Click += (s, ev) =>
            {
                // Dernière confirmation modale
                var result = MessageBox.Show(
                    "Dernière chance !\n\n" +
                    "Toutes les données applicatives seront supprimées.\n" +
                    "Les comptes utilisateurs sont conservés.\n\n" +
                    "Continuer ?",
                    "Confirmer la purge",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes) return;

                try
                {
                    btnFlush.Enabled = false;
                    btnFlush.Text = "Purge en cours...";
                    Application.DoEvents();

                    int tables = FlushDatabase();

                    lblResult.ForeColor = GREEN_OK;
                    lblResult.Text = $"{tables} tables vidées avec succès.";

                    // Reset l'état UI
                    chkConfirm.Checked = false;
                    txtConfirm.Text = "";
                    btnFlush.Text = "Vider la base de données";
                }
                catch (Exception ex)
                {
                    lblResult.ForeColor = RED_CRIT;
                    lblResult.Text = "Erreur : " + ex.Message;
                    btnFlush.Text = "Vider la base de données";
                }
            };

            grpDb.Controls.Add(btnFlush);
            grpDb.Height = y + 56;

            pnlContent.Controls.Add(grpDb);

            _pnlDroit.Controls.Add(pnlContent);
            _pnlDroit.Controls.Add(pnlHeader);
            _pnlDroit.ResumeLayout(true);
        }

        // ── Helper : section paramètre avec bordure ─────────────────

        private Panel MakeParamSection(string title, int height)
        {
            var pnl = new Panel
            {
                Size = new Size(580, height),
                Location = new Point(0, 0),
                BackColor = CREME_WARM
            };
            pnl.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Bordure arrondie
                using (var pen = new Pen(BORDER_CLR, 1))
                using (var path = RoundedRect(new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1), 6))
                    g.DrawPath(pen, path);

                // Accent haut
                using (var brush = new SolidBrush(CHOCO_BRAND))
                    g.FillRectangle(brush, 1, 1, pnl.Width - 2, 3);
            };

            pnl.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = CHOCO_BRAND,
                Location = new Point(16, 10), AutoSize = true
            });

            return pnl;
        }

        // ── Flush DB — exécute les TRUNCATE dans l'ordre de dépendances ─

        /// <summary>
        /// Vide toutes les tables applicatives (préserve utilisateurs).
        /// Retourne le nombre de tables purgées.
        /// </summary>
        private int FlushDatabase()
        {
            // Ordre : enfants → parents (respect des FK)
            string[] tables =
            {
                // E-commerce (enfants d'abord)
                "commandes_web_lignes",
                "commandes_web",
                "produits_web",
                "categories_web",
                "clients",

                // Production & stocks BOM
                "bom_reservations",
                "bom_productions_lignes",
                "bom_productions",
                "bom_stocks",

                // Recettes BOM
                "bom_fiches_lignes",
                "bom_fiches",
                "bom_niveaux",
                "bom_contextes",

                // Ingrédients & achats
                "lots_ingredients",
                "fiches_ingredients",

                // Référentiels
                "activites_stocks",
                "stocks",
                "fournisseurs",
                "activites",
            };

            using (var conn = DbHelper.GetConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 0;";
                cmd.ExecuteNonQuery();

                foreach (var table in tables)
                {
                    cmd.CommandText = $"TRUNCATE TABLE `{table}`;";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = "SET FOREIGN_KEY_CHECKS = 1;";
                cmd.ExecuteNonQuery();
            }

            return tables.Length;
        }
    }
}
