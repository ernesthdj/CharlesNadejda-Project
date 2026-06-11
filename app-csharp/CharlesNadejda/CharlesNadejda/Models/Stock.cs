using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Emplacement de stockage physique (ex: "Frigo principal", "Étagère sec").
    /// Lié aux activités via la table de jonction M:N activites_stocks.
    /// Les lots d'ingrédients sont rattachés à un stock via lots_ingredients.id_stock.
    /// </summary>
    public class Stock
    {
        // ── Colonnes DB (table stocks) ────────────────────────────

        public int      Id           { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public bool     Actif        { get; set; }
        public DateTime DateCreation { get; set; }

        public override string ToString() => Nom;
    }
}
