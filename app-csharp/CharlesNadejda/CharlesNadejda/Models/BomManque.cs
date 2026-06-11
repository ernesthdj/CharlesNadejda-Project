namespace CharlesNadejda.Models
{
    /// <summary>
    /// Représente un manque de stock détecté avant une production.
    /// Retourné par BomProductionDAL.VerifierDisponibilite() et Simuler().
    /// Utilisé aussi pour les jauges de simulation (même quand le stock est suffisant).
    /// </summary>
    public class BomManque
    {
        // ── Données fournies par le DAL ───────────────────────────

        /// <summary>Nom de l'ingrédient ou de la fiche source.</summary>
        public string  NomInput          { get; set; }
        /// <summary>Unité de stockage (g, ml, piece, kg...).</summary>
        public string  Unite             { get; set; }
        /// <summary>Quantité totale nécessaire pour la production demandée.</summary>
        public decimal QuantiteNecessaire { get; set; }
        /// <summary>Quantité actuellement disponible en stock.</summary>
        public decimal QuantiteDisponible { get; set; }

        // ── Propriétés calculées ──────────────────────────────────

        /// <summary>Quantité manquante = max(0, QuantiteNecessaire - QuantiteDisponible).</summary>
        public decimal Manque            => QuantiteNecessaire > QuantiteDisponible
                                               ? QuantiteNecessaire - QuantiteDisponible
                                               : 0m;

        public override string ToString() =>
            $"{NomInput} : besoin {QuantiteNecessaire} {Unite}, dispo {QuantiteDisponible} {Unite} (manque {Manque} {Unite})";
    }
}
