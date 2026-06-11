using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Gestion des activités artisanales (CRUD + Désactiver/Réactiver + Stocks liés).
    /// Hérite de FrmListeBase&lt;Activite&gt; pour le layout et le workflow CRUD standard.
    ///
    /// Particularités par rapport à un formulaire liste classique :
    ///   - Bouton "Désactiver / Réactiver" (toggle selon l'état de l'activité sélectionnée)
    ///   - Bouton "Stocks liés" (ouvre FrmActiviteStocks)
    ///   - Les activités inactives sont affichées en gris italique (AppliquerStylesLignes)
    ///   - ChargerDonnees() inclut les inactifs (includeInactifs: true)
    /// </summary>
    public class FrmActivites : FrmListeBase<Activite>
    {
        // ── Boutons supplémentaires propres à ce formulaire ─────────
        private readonly Button _btnDesactiver;
        private readonly Button _btnStocks;

        public FrmActivites()
        {
            // Bouton Désactiver/Réactiver — positionné sous le bouton Supprimer de la base
            _btnDesactiver = new Button
            {
                Text      = "✕  Désactiver",
                Location  = new Point(BtnX, BtnYExtra),
                Size      = new Size(130, 36),
                Font      = new Font("Segoe UI", 9.5F),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                BackColor = Color.FromArgb(160, 120, 60),
                ForeColor = Color.White
            };
            _btnDesactiver.FlatAppearance.BorderSize          = 1;
            _btnDesactiver.FlatAppearance.BorderColor         = Color.FromArgb(140, 105, 50);
            _btnDesactiver.FlatAppearance.MouseOverBackColor  = Color.FromArgb(140, 105, 50);
            _btnDesactiver.Click += (s, e) => Desactiver();

            // Bouton Stocks liés — positionné sous Désactiver
            _btnStocks = new Button
            {
                Text      = "📦 Stocks liés",
                Location  = new Point(BtnX, BtnYExtra + 44),
                Size      = new Size(130, 36),
                Font      = new Font("Segoe UI", 9.5F),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                BackColor = Color.FromArgb(60, 110, 160),
                ForeColor = Color.White
            };
            _btnStocks.FlatAppearance.BorderSize          = 1;
            _btnStocks.FlatAppearance.BorderColor         = Color.FromArgb(45, 90, 140);
            _btnStocks.FlatAppearance.MouseOverBackColor  = Color.FromArgb(45, 90, 140);
            _btnStocks.Click += (s, e) => GererStocks();

            Controls.Add(_btnDesactiver);
            Controls.Add(_btnStocks);

            // Mise à jour du bouton Désactiver quand la sélection change
            dgv.SelectionChanged += (s, e) => MajBoutonDesactiver();
        }

        // ── Membres abstraits — logique métier spécifique ───────────

        protected override string Titre => "Activités";

        protected override List<Activite> ChargerDonnees()
            => ActiviteDAL.GetAll(includeInactifs: true);

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id");

            ConfigCol("Nom",          "Nom",         200, 120);
            ConfigCol("Description",  "Description", 220, 140);
            ConfigCol("Actif",        "Actif",        60,  50);
            ConfigCol("DateCreation", "Créée le",    100,  80);
        }

        protected override Form OuvrirFormulaire(Activite element)
            => new FrmActiviteEdit(element);

        protected override void Supprimer(Activite element)
            => ActiviteDAL.Delete(element.Id);

        protected override string NomElement(Activite element)
            => element?.Nom ?? "?";

        // ── Styles visuels — activités inactives grisées ────────────

        /// <summary>
        /// Colore les lignes des activités inactives en gris italique.
        /// Gestalt "similarité" : les éléments inactifs se distinguent visuellement.
        /// </summary>
        protected override void AppliquerStylesLignes()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.DataBoundItem is Activite act && !act.Actif)
                {
                    row.DefaultCellStyle.ForeColor          = Color.Gray;
                    row.DefaultCellStyle.Font               = new Font("Segoe UI", 9.5F, FontStyle.Italic);
                    row.DefaultCellStyle.SelectionForeColor = Color.LightGray;
                }
            }

            MajBoutonDesactiver();
        }

        // ── Logique Désactiver / Réactiver ──────────────────────────

        private void MajBoutonDesactiver()
        {
            var item = Selectionne();
            if (item == null) return;

            if (item.Actif)
            {
                _btnDesactiver.Text      = "✕  Désactiver";
                _btnDesactiver.BackColor = Color.FromArgb(160, 120, 60);
                _btnDesactiver.FlatAppearance.BorderColor        = Color.FromArgb(140, 105, 50);
                _btnDesactiver.FlatAppearance.MouseOverBackColor = Color.FromArgb(140, 105, 50);
            }
            else
            {
                _btnDesactiver.Text      = "✓  Réactiver";
                _btnDesactiver.BackColor = Color.FromArgb(60, 130, 80);
                _btnDesactiver.FlatAppearance.BorderColor        = Color.FromArgb(46, 110, 60);
                _btnDesactiver.FlatAppearance.MouseOverBackColor = Color.FromArgb(46, 110, 60);
            }
        }

        private void Desactiver()
        {
            var activite = Selectionne();
            if (activite == null)
            {
                MessageBox.Show("Sélectionnez une activité.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (activite.Actif)
            {
                // Désactiver
                if (MessageBox.Show(
                        $"Désactiver l'activité « {activite.Nom} » ?\n\n" +
                        "Elle restera visible (grisée) mais ne sera plus sélectionnable dans les formulaires.",
                        "Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

                try
                {
                    ActiviteDAL.Desactiver(activite.Id);
                    Charger();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Impossible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Réactiver
                if (MessageBox.Show(
                        $"Réactiver l'activité « {activite.Nom} » ?",
                        "Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes) return;

                try
                {
                    ActiviteDAL.Reactiver(activite.Id);
                    Charger();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Logique Stocks liés ─────────────────────────────────────

        private void GererStocks()
        {
            var activite = Selectionne();
            if (activite == null)
            {
                MessageBox.Show("Sélectionnez une activité.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var frm = new FrmActiviteStocks(activite))
                frm.ShowDialog(this);
        }
    }
}
