namespace CharlesNadejda.Models
{
    /// <summary>
    /// Ligne d'une fiche BOM — un input de la recette (ingrédient ou fiche de niveau inférieur).
    /// FK polymorphique : TypeInput détermine si l'input est dans fiches_ingredients ou bom_fiches.
    /// </summary>
    public class BomFicheLigne
    {
        // ── Colonnes DB (table bom_fiches_lignes) ─────────────────

        public int     Id                  { get; set; }
        /// <summary>FK vers bom_fiches — la recette parente.</summary>
        public int     IdFiche             { get; set; }
        /// <summary>Discriminant polymorphique : "ingredient" ou "fiche". Voir BomFiche.TypeInputXxx.</summary>
        public string  TypeInput           { get; set; }
        /// <summary>FK vers fiches_ingredients — rempli si TypeInput == "ingredient".</summary>
        public int?    IdInputIngredient   { get; set; }
        /// <summary>FK vers bom_fiches — rempli si TypeInput == "fiche".</summary>
        public int?    IdInputFiche        { get; set; }
        /// <summary>Quantité requise par batch, dans l'unité UniteMesure.</summary>
        public decimal Quantite            { get; set; }
        /// <summary>Unité de la recette (kg, g, l, ml, cl, piece) — peut différer de l'unité de stockage.</summary>
        public string  UniteMesure         { get; set; }

        // ── Propriétés de jointure (chargées par le DAL) ──────────

        /// <summary>Chargé par BomFicheLigneDAL via COALESCE(fiches_ingredients.nom, bom_fiches.nom).</summary>
        public string NomInput         { get; set; }
        /// <summary>Chargé par BomFicheLigneDAL — unité native du stock (g, ml, piece pour ingrédient ; unite_output pour fiche).</summary>
        public string UniteMesureInput { get; set; }
        /// <summary>Chargé par BomFicheLigneDAL — prix de référence par unité de base (€/g). 0 pour les fiches.</summary>
        public decimal PrixUnitaireRef { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Coût estimé de cette ligne = Quantite * PrixUnitaireRef (basé sur prix référence, pas le coût réel FIFO).</summary>
        public decimal SousTotal => Quantite * PrixUnitaireRef;

        public override string ToString() => $"{Quantite} {UniteMesure} de {NomInput}";
    }
}
