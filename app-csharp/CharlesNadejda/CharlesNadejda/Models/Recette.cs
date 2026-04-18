using System.Collections.Generic;
using System.Linq;

namespace CharlesNadejda.Models
{
    public class Recette
    {
        public int     Id                  { get; set; }
        public string  Nom                 { get; set; }
        public string  Description         { get; set; }
        public string  TypeRendement       { get; set; }   // par_lot | par_unite
        public decimal RendementQuantite   { get; set; }
        public int?    TempsPreparation    { get; set; }
        public bool    Actif               { get; set; }

        public List<RecetteIngredient> Ingredients { get; set; } = new List<RecetteIngredient>();

        public decimal CoutBatch    => Ingredients.Sum(i => i.SousTotal);
        public decimal CoutUnitaire => RendementQuantite > 0 ? CoutBatch / RendementQuantite : 0;

        public override string ToString() => Nom;
    }
}
