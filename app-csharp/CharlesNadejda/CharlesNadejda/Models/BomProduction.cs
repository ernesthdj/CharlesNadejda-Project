using System;
using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    // BomProduction — représente un enregistrement de production dans le système BOM
    // (Bill of Materials — nomenclature de fabrication).
    // Chaque instance = "j'ai produit X unités de la fiche Y dans le niveau Z à telle date".
    // C'est le résultat de l'appel à BomProductionDAL.Executer().
    public class BomProduction
    {
        // ── Champs de la table bom_productions ──────────────────────

        // Clé primaire AUTO_INCREMENT dans MySQL
        public int      Id               { get; set; }

        // FK vers bom_niveaux — dans quel niveau de la hiérarchie BOM cette production a eu lieu
        // (ex: Niveau 1 = matières premières, Niveau 2 = semi-finis, Niveau 3 = produits finis)
        public int      IdNiveau         { get; set; }

        // FK vers bom_fiches — quelle recette/fiche a été utilisée pour cette production
        public int      IdFiche          { get; set; }

        // Quantité effectivement produite (batches × QuantiteOutput de la fiche)
        // Ex: 2 batches d'une fiche qui produit 10 tablettes → 20 ici
        public decimal  QuantiteProduite { get; set; }

        // Coût total de tous les ingrédients consommés pour cette production
        // Calculé en FIFO dans ConsumeStock() — somme de (qté prise × prix unitaire du lot)
        public decimal  CoutIngredients  { get; set; }

        // Coût par unité produite = CoutIngredients / QuantiteProduite
        // Utilisé pour valoriser le stock du produit fini
        public decimal  CoutUnitaire     { get; set; }

        // Date et heure de la production (DEFAULT CURRENT_TIMESTAMP côté MySQL)
        public DateTime DateProduction   { get; set; }

        // Notes libres optionnelles saisies par l'utilisateur au moment de la production
        public string   Notes            { get; set; }

        // ── Champs issus des jointures (pas dans la table directement) ──

        // Nom de la fiche de production (JOIN bom_fiches.nom)
        public string  NomFiche           { get; set; }

        // Nom du niveau BOM (JOIN bom_niveaux.nom, ex: "Ganaches", "Tablettes")
        public string  NomNiveau          { get; set; }

        // Ordre du niveau dans la hiérarchie (JOIN bom_niveaux.ordre)
        // Plus l'ordre est élevé, plus on est haut dans la chaîne de production
        public int     OrdreNiveau        { get; set; }

        // Nom du contexte BOM (JOIN bom_contextes.nom, ex: "Chocolat noir")
        public string  NomContexte        { get; set; }

        // Unité de sortie de la fiche (ex: "kg", "pièce", "tablette")
        public string  UniteOutput        { get; set; }

        // Quantité produite par batch dans la fiche (bom_fiches.quantite_output)
        // Sert à calculer le nombre de batches : QuantiteProduite / QuantiteOutputBatch
        public decimal QuantiteOutputBatch { get; set; }

        // ── Lignes de traçabilité ───────────────────────────────────

        // Liste des lignes de production (bom_productions_lignes) — chargées optionnellement
        // Chaque ligne dit "j'ai pris X du lot/stock Y" — traçabilité complète FIFO
        // Pas toujours chargées par le DAL (seulement quand on affiche le détail)
        public List<BomProductionLigne> Lignes { get; set; } = new List<BomProductionLigne>();

        // ToString() — représentation lisible pour le debug et les ComboBox
        // Format : "10/06/2026 — Tablette Noir 70% × 20 [Ganaches]"
        public override string ToString() =>
            $"{DateProduction:dd/MM/yyyy} — {NomFiche} × {QuantiteProduite} [{NomNiveau}]";
    }
}
