using System.Collections.Generic;
using System.Threading.Tasks;
using CharlesNadejda.DAL;
using CharlesNadejda.Models;

namespace CharlesNadejda.Services
{
    // Couche applicative entre les DAL et la vue de simulation.
    // Responsabilité unique : appeler BomProductionDAL.Simuler() sur un thread de fond
    // et projeter chaque BomManque vers un SimulationResultat consommable par l'UI.
    //
    // BomProductionDAL.Simuler() retourne TOUTES les lignes (pas seulement les pénuries) ;
    // le champ Resultat est calculé ici à partir de BomManque.Manque.
    public static class SimulationService
    {
        // async sur Task.Run uniquement pour BomProductionDAL.Simuler() — conformément
        // à la décision d'architecture (seuls Simuler() et VueStockGlobalDAL.GetAll() sont async).
        public static async Task<List<SimulationResultat>> SimulerAsync(
            int     idNiveau,
            int     idFiche,
            decimal quantiteCible)
        {
            List<BomManque> manques = await Task.Run(
                () => BomProductionDAL.Simuler(idNiveau, idFiche, quantiteCible));

            return Project(manques);
        }

        // ── Projection ────────────────────────────────────────────────────────

        private static List<SimulationResultat> Project(List<BomManque> manques)
        {
            var résultats = new List<SimulationResultat>(manques.Count);

            foreach (BomManque m in manques)
            {
                résultats.Add(new SimulationResultat
                {
                    NomInput           = m.NomInput,
                    Unite              = m.Unite,
                    QuantiteNecessaire = m.QuantiteNecessaire,
                    QuantiteDisponible = m.QuantiteDisponible,
                    Manque             = m.Manque
                });
            }

            return résultats;
        }
    }
}
