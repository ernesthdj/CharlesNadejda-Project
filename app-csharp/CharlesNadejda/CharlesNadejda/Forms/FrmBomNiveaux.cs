using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Liste des niveaux de transformation d'un contexte — hérite de FrmListeBase&lt;BomNiveau&gt;.
    ///
    /// TICKET-14 : migration depuis partial class Form vers FrmListeBase&lt;T&gt;.
    /// Logique métier Ajouter préservée : OuvrirFormulaire(null) calcule GetOrdreMax + 1
    /// pour garantir l'ordre croissant des niveaux sans trou.
    /// </summary>
    public class FrmBomNiveaux : FrmListeBase<BomNiveau>
    {
        private readonly BomContexte _contexte;

        public FrmBomNiveaux(BomContexte contexte)
        {
            _contexte = contexte;
        }

        // ── Membres abstraits FrmListeBase<BomNiveau> ─────────────────────

        protected override string Titre
            => $"Niveaux de transformation — {_contexte.Nom}";

        protected override List<BomNiveau> ChargerDonnees()
            => BomNiveauDAL.GetByContexte(_contexte.Id);

        protected override void ConfigurerColonnes()
        {
            CacherColonnes("Id", "IdContexte", "NomContexte", "Activite", "DateCreation");

            if (dgv.Columns["Ordre"] != null)
            {
                dgv.Columns["Ordre"].HeaderText = "Ordre";
                dgv.Columns["Ordre"].Width      = 60;
            }
            if (dgv.Columns["Nom"]         != null) dgv.Columns["Nom"].HeaderText         = "Nom du niveau";
            if (dgv.Columns["Description"] != null) dgv.Columns["Description"].HeaderText = "Description";
        }

        protected override Form OuvrirFormulaire(BomNiveau element)
        {
            if (element == null)
            {
                // Règle métier : ordre = max actuel + 1 pour éviter les trous de séquence
                int ordreMax = BomNiveauDAL.GetOrdreMax(_contexte.Id);
                var nouveau  = new BomNiveau { IdContexte = _contexte.Id, Ordre = ordreMax + 1 };
                return new FrmBomNiveauEdit(nouveau, false);
            }
            return new FrmBomNiveauEdit(element, true);
        }

        protected override void Supprimer(BomNiveau element)
            => BomNiveauDAL.Delete(element.Id);

        protected override string NomElement(BomNiveau element)
            => element != null ? $"{element.Nom} (N{element.Ordre})" : "?";
    }
}
