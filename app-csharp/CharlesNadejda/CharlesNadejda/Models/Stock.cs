using System;

namespace CharlesNadejda.Models
{
    public class Stock
    {
        public int      Id           { get; set; }
        public string   Nom          { get; set; }
        public string   Description  { get; set; }
        public bool     Actif        { get; set; }
        public DateTime DateCreation { get; set; }

        public override string ToString() => Nom;
    }
}
