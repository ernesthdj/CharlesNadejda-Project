using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Niveau dans la hiérarchie BOM d'un contexte (ex: Niveau 1 = Matières premières, Niveau 2 = Semi-finis).
    /// L'ordre croissant indique la progression dans la chaîne de production.
    /// </summary>
    public class BomNiveau
    {
        // ── Colonnes DB (table bom_niveaux) ───────────────────────

        public int      Id           { get; set; }
        /// <summary>FK vers bom_contextes — le contexte parent.</summary>
        public int      IdContexte   { get; set; }
        /// <summary>Position dans la hiérarchie (1 = matières premières, 2+ = niveaux supérieurs).</summary>
        public int      Ordre        { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public DateTime DateCreation { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par BomNiveauDAL via JOIN bom_contextes.nom.</summary>
        public string NomContexte { get; set; }
        /// <summary>Chargé par BomNiveauDAL via JOIN bom_contextes.id_activite.</summary>
        public int    IdActivite  { get; set; }
        /// <summary>Chargé par BomNiveauDAL via JOIN activites.nom.</summary>
        public string ActiviteNom { get; set; }

        public override string ToString() => $"Niveau {Ordre} — {Nom}";
    }
}
