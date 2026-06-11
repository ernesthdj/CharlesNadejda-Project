using System;
using System.Collections.Generic;
using System.Linq;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Fiche de production BOM (Bill of Materials — nomenclature de fabrication).
    /// Définit une recette : quels inputs consommer pour produire QuantiteOutput en UniteOutput.
    /// Appartient à un niveau, lui-même rattaché à un contexte et une activité.
    /// </summary>
    public class BomFiche
    {
        // ── Constantes métier ─────────────────────────────────────

        /// <summary>Type d'input : matière première.</summary>
        public const string TypeInputIngredient = "ingredient";
        /// <summary>Type d'input : produit intermédiaire (fiche d'un niveau inférieur).</summary>
        public const string TypeInputFiche = "fiche";

        // ── Colonnes DB (table bom_fiches) ────────────────────────

        public int      Id               { get; set; }
        /// <summary>FK vers bom_niveaux — niveau auquel appartient cette fiche.</summary>
        public int      IdNiveau         { get; set; }
        public string   Nom              { get; set; }
        public string   Description      { get; set; }
        /// <summary>Unité de sortie de la production (kg, g, l, ml, cl, piece).</summary>
        public string   UniteOutput      { get; set; }
        /// <summary>Quantité produite par exécution d'un batch.</summary>
        public decimal  QuantiteOutput   { get; set; }
        /// <summary>Temps de préparation estimé en minutes.</summary>
        public int?     TempsPreparation { get; set; }
        /// <summary>Stock cible pour jauge visuelle dans la vue stock.</summary>
        public decimal? StockCible       { get; set; }
        public bool     Actif            { get; set; }
        public DateTime DateCreation     { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par BomFicheDAL via JOIN bom_niveaux.nom.</summary>
        public string NomNiveau   { get; set; }
        /// <summary>Chargé par BomFicheDAL via JOIN bom_niveaux.ordre.</summary>
        public int    OrdreNiveau { get; set; }
        /// <summary>Chargé par BomFicheDAL via JOIN bom_contextes.id.</summary>
        public int    IdContexte  { get; set; }
        /// <summary>Chargé par BomFicheDAL via JOIN bom_contextes.nom.</summary>
        public string NomContexte { get; set; }
        /// <summary>Chargé par BomFicheDAL via JOIN activites.id.</summary>
        public int    IdActivite  { get; set; }
        /// <summary>Chargé par BomFicheDAL via JOIN activites.nom.</summary>
        public string ActiviteNom { get; set; }

        /// <summary>Lignes de la recette — chargées optionnellement par BomFicheDAL.GetById(avecLignes: true).</summary>
        public List<BomFicheLigne> Lignes { get; set; } = new List<BomFicheLigne>();

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Coût total estimé des inputs pour un batch = somme des sous-totaux des lignes (prix référence).</summary>
        public decimal CoutBatch    => Lignes.Sum(l => l.SousTotal);
        /// <summary>Coût unitaire estimé = CoutBatch / QuantiteOutput. Protégé contre division par zéro.</summary>
        public decimal CoutUnitaire => QuantiteOutput > 0 ? CoutBatch / QuantiteOutput : 0;

        public override string ToString() => Nom;
    }
}
