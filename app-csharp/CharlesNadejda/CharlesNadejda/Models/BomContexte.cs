using System;
using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Contexte de production BOM (ex: "Chocolat noir", "Chocolat lait").
    /// Regroupe les niveaux de fabrication sous une même activité artisanale.
    /// </summary>
    public class BomContexte
    {
        // ── Colonnes DB (table bom_contextes) ─────────────────────

        public int      Id           { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        /// <summary>FK vers activites — l'activité artisanale parente.</summary>
        public int      IdActivite   { get; set; }
        public bool     Actif        { get; set; }
        public DateTime DateCreation { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par BomContexteDAL via JOIN activites.nom.</summary>
        public string ActiviteNom { get; set; }

        /// <summary>Niveaux du contexte — chargés optionnellement par le DAL.</summary>
        public List<BomNiveau> Niveaux { get; set; } = new List<BomNiveau>();

        public override string ToString() => $"{Nom} ({ActiviteNom})";
    }
}
