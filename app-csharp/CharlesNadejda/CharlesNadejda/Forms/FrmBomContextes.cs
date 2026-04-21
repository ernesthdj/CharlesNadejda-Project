using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des contextes de production — hérite de FrmListeBase&lt;BomContexte&gt;.
    ///
    /// TICKET-14 : migration depuis partial class Form vers FrmListeBase&lt;T&gt;.
    /// Bouton supplémentaire « → Niveaux » ajouté en OnLoad à la position BtnYExtra.
    /// </summary>
    public class FrmBomContextes : FrmListeBase<BomContexte>
    {
        private readonly Activite _activite;
        private Button            _btnNiveaux;

        public FrmBomContextes(Activite activite = null)
        {
            _activite = activite;
        }

        // ── Membres abstraits FrmListeBase<BomContexte> ───────────────────

        protected override string Titre => _activite != null
            ? $"Contextes de production — {_activite.Nom}"
            : "Contextes de production";

        protected override List<BomContexte> ChargerDonnees()
            => BomContexteDAL.GetAll(_activite?.Id ?? 0);

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Actif", "DateCreation", "Niveaux");
            if (dgv.Columns["Nom"]         != null) dgv.Columns["Nom"].HeaderText         = "Nom du contexte";
            if (dgv.Columns["ActiviteNom"] != null) dgv.Columns["ActiviteNom"].HeaderText = "Activité";
            if (dgv.Columns["Description"] != null) dgv.Columns["Description"].HeaderText = "Description";
        }

        protected override Form OuvrirFormulaire(BomContexte element)
            => element == null
                ? new FrmBomContexteEdit(null, _activite)
                : new FrmBomContexteEdit(element, _activite);

        protected override void Supprimer(BomContexte element)
            => BomContexteDAL.Delete(element.Id);

        protected override string NomElement(BomContexte element) => element?.Nom ?? "?";

        // ── Cycle de vie ──────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            // Bouton supplémentaire : navigation vers les niveaux du contexte
            _btnNiveaux = new Button
            {
                Text      = "→ Niveaux",
                Location  = new Point(BtnX, BtnYExtra),
                Size      = new Size(130, 36),
                Font      = new Font("Segoe UI", 9.5F),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                BackColor = Color.FromArgb(63, 81, 181),
                ForeColor = Color.White
            };
            _btnNiveaux.FlatAppearance.BorderSize = 0;
            _btnNiveaux.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 63, 159);
            _btnNiveaux.Click += (s, ev) => OuvrirNiveaux();
            Controls.Add(_btnNiveaux);

            base.OnLoad(e);
        }

        private void OuvrirNiveaux()
        {
            var ctx = Selectionne();
            if (ctx == null)
            {
                MessageBox.Show("Sélectionnez un contexte.", "Aucune sélection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var frm = new FrmBomNiveaux(ctx))
                frm.ShowDialog(this);
            Charger();   // rafraîchir après modification des niveaux
        }
    }
}
