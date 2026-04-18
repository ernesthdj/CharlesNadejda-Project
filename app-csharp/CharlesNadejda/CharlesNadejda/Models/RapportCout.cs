using System.Collections.Generic;

namespace CharlesNadejda.Models
{
    /// <summary>
    /// Rapport de coût d'une production BOM pour N batches.
    /// Produit par BomCoutDAL.CalculerCout() — lecture seule, jamais persisté.
    ///
    /// Lecture du résultat :
    ///   CoutTotal    = coût total pour produire NbBatches × QuantiteOutput [UniteOutput]
    ///   CoutUnitaire = coût par unité de sortie (ex : €/g, €/pce)
    ///   Lignes       = détail par input consommé, récursif si TypeInput == "fiche"
    /// </summary>
    public class RapportCout
    {
        public string   NomFiche       { get; set; }
        public decimal  NbBatches      { get; set; }
        /// <summary>Quantité totale produite = NbBatches × fiche.QuantiteOutput.</summary>
        public decimal  QuantiteOutput { get; set; }
        public string   UniteOutput    { get; set; }
        /// <summary>Coût total HTVA de tous les inputs, tous niveaux confondus.</summary>
        public decimal  CoutTotal      { get; set; }
        /// <summary>CoutTotal / QuantiteOutput — coût par unité de sortie.</summary>
        public decimal  CoutUnitaire   { get; set; }
        public List<LigneCout> Lignes  { get; set; } = new List<LigneCout>();
    }

    /// <summary>
    /// Ligne de détail d'un RapportCout.
    /// Si TypeInput == "fiche", DetailFiche contient le rapport récursif du niveau N-1.
    /// </summary>
    public class LigneCout
    {
        public string  NomInput    { get; set; }
        /// <summary>"ingredient" | "fiche"</summary>
        public string  TypeInput   { get; set; }
        /// <summary>Quantité consommée dans l'unité de stockage (UniteMesureInput du niveau).</summary>
        public decimal Quantite    { get; set; }
        public string  Unite       { get; set; }
        /// <summary>Coût par unité (€/g, €/kg, €/pce…). Pour une fiche : coût par unité output.</summary>
        public decimal PrixUnit    { get; set; }
        public decimal SousTotal   { get; set; }
        /// <summary>Détail récursif — rempli uniquement si TypeInput == "fiche".</summary>
        public RapportCout DetailFiche { get; set; }
    }
}
