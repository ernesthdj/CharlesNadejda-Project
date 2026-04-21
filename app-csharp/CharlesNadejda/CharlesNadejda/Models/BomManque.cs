namespace CharlesNadejda.Models
{
    /// <summary>
    /// Représente un manque de stock détecté avant une production.
    /// Retourné par BomProductionDAL.VerifierDisponibilite().
    /// </summary>
    public class BomManque
    {
        public string  NomInput          { get; set; }   // nom ingrédient ou fiche
        public string  Unite             { get; set; }
        public decimal QuantiteNecessaire { get; set; }
        public decimal QuantiteDisponible { get; set; }
        public decimal Manque            => QuantiteNecessaire > QuantiteDisponible
                                               ? QuantiteNecessaire - QuantiteDisponible
                                               : 0m;

        public override string ToString() =>
            $"{NomInput} : besoin {QuantiteNecessaire} {Unite}, dispo {QuantiteDisponible} {Unite} (manque {Manque} {Unite})";
    }
}
