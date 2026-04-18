using System;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Activité artisanale (Chocolaterie, Pâtisserie, Glacier, Cocktails…).
    /// Créée et gérée par l'utilisateur — remplace les valeurs ENUM codées en dur.
    /// Chaque activité possède son propre stock d'ingrédients et ses contextes de production.
    /// </summary>
    public class Activite
    {
        public int      Id           { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public bool     Actif        { get; set; }
        public DateTime DateCreation { get; set; }

        public override string ToString() => Nom;
    }
}
