namespace CharlesNadejda.Services
{
    public class SimulationResultat
    {
        public string  NomInput           { get; set; }
        public string  Unite              { get; set; }
        public decimal QuantiteNecessaire { get; set; }
        public decimal QuantiteDisponible { get; set; }
        public decimal Manque             { get; set; }

        public bool   EnPenurie => Manque > 0;
        public string Resultat  => EnPenurie ? "PÉNURIE" : "OK";
    }
}
