namespace CharlesNadejda.Navigation
{
    public class NavigationParams
    {
        public int?          ScrollToId           { get; set; }
        public object        Entity               { get; set; }
        public bool          IsEdit               { get; set; }
        public RessourceType? RessourceType       { get; set; }
        // US-08 : filtre alertes transmis lors de la navigation vers Ingrédients
        public bool          FiltreAlertesSeulement { get; set; }
    }
}
