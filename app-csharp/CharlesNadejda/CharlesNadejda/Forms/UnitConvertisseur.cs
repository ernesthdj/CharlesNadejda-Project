using System;
using System.Collections.Generic;
using System.Linq;

namespace CharlesNadejda
{
    /// <summary>
    /// Convertisseur d'unités de mesure — utilitaire central de l'application.
    ///
    /// Chaînes complètes supportées :
    ///   Masse  : mg → cg → g → kg          (unité de base = g)
    ///              0.001  0.01  1   1000
    ///   Volume : ml → cl → dl → l           (unité de base = ml)
    ///              1    10   100  1000
    ///   Pièce  : piece                       (non convertible, ratio = 1)
    ///
    /// Règle fondamentale :
    ///   Toute conversion passe par l'unité de base du groupe :
    ///     valeur_source × facteur_source  →  valeur_base
    ///     valeur_base   / facteur_cible   →  valeur_cible
    ///
    ///   Exemple : 500 g → kg = (500 × 1) / 1000 = 0.5 kg
    ///             0.5 kg → g = (0.5 × 1000) / 1 = 500 g
    ///
    /// Points d'appel obligatoires :
    ///   - BomProductionDAL.VerifierDisponibilite() : avant comparaison stock
    ///   - BomProductionDAL.Simuler()               : avant comparaison stock
    ///   - BomProductionDAL.ConsumeStock()           : avant décrémentation FIFO
    ///   - FrmBomFicheEdit.SynchroniserUniteInput()  : liste des unités compatibles
    /// </summary>
    public static class UnitConvertisseur
    {
        // ── Définition des groupes (ordre croissant dans chaque groupe) ────

        private static readonly string[] _masse  = { "mg", "cg", "g", "kg" };
        private static readonly string[] _volume = { "ml", "cl", "dl", "l" };
        private static readonly string[] _piece  = { "piece" };

        private static readonly string[][] _groupes = { _masse, _volume, _piece };

        // ── Facteurs de conversion vers l'unité de base du groupe ─────────
        // Masse base = g  │  Volume base = ml  │  Pièce base = piece
        private static readonly Dictionary<string, decimal> _versBase =
            new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            // Masse
            { "mg",    0.001m  },   // 1 mg  = 0.001 g
            { "cg",    0.01m   },   // 1 cg  = 0.01  g
            { "g",     1m      },   // 1 g   = 1     g  (base)
            { "kg",    1000m   },   // 1 kg  = 1000  g
            // Volume
            { "ml",    1m      },   // 1 ml  = 1     ml (base)
            { "cl",    10m     },   // 1 cl  = 10    ml
            { "dl",    100m    },   // 1 dl  = 100   ml
            { "l",     1000m   },   // 1 l   = 1000  ml
            // Pièce
            { "piece", 1m      }
        };

        // ── API publique ───────────────────────────────────────────────────

        /// <summary>
        /// Retourne toutes les unités du même groupe que <paramref name="unite"/>
        /// (y compris elle-même), dans l'ordre croissant.
        /// Si l'unité est inconnue, retourne uniquement elle-même.
        /// </summary>
        public static IEnumerable<string> UnitesCompatibles(string unite)
        {
            foreach (var groupe in _groupes)
                if (groupe.Contains(unite, StringComparer.OrdinalIgnoreCase))
                    return groupe;
            return new[] { unite };
        }

        /// <summary>Indique si deux unités appartiennent au même groupe.</summary>
        public static bool SontCompatibles(string u1, string u2)
            => UnitesCompatibles(u1).Contains(u2, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Retourne l'unité de base du groupe auquel appartient <paramref name="unite"/>.
        /// Masse → "g" | Volume → "ml" | Pièce/inconnu → "piece"
        /// </summary>
        public static string UniteBase(string unite)
        {
            if (_masse.Contains(unite,  StringComparer.OrdinalIgnoreCase)) return "g";
            if (_volume.Contains(unite, StringComparer.OrdinalIgnoreCase)) return "ml";
            return "piece";
        }

        /// <summary>
        /// Convertit <paramref name="valeur"/> de <paramref name="unite"/> vers l'unité
        /// de base du groupe (g, ml ou piece).
        /// </summary>
        public static decimal VersUniteBase(decimal valeur, string unite)
            => Convertir(valeur, unite, UniteBase(unite));

        /// <summary>
        /// Convertit <paramref name="valeur"/> de <paramref name="uniteSource"/>
        /// vers <paramref name="uniteCible"/>.
        /// Lance <see cref="InvalidOperationException"/> si les unités sont incompatibles.
        /// </summary>
        public static decimal Convertir(decimal valeur, string uniteSource, string uniteCible)
        {
            // TICKET-04 : guards contre null et valeur négative
            if (uniteSource == null) throw new ArgumentNullException(nameof(uniteSource));
            if (uniteCible  == null) throw new ArgumentNullException(nameof(uniteCible));
            if (valeur < 0) throw new ArgumentOutOfRangeException(nameof(valeur), "La valeur à convertir ne peut pas être négative.");

            if (string.Equals(uniteSource, uniteCible, StringComparison.OrdinalIgnoreCase))
                return valeur;

            if (!SontCompatibles(uniteSource, uniteCible))
                throw new InvalidOperationException(
                    $"Unités incompatibles : '{uniteSource}' → '{uniteCible}'.");

            decimal enBase = valeur * _versBase[uniteSource];
            return enBase / _versBase[uniteCible];
        }
    }
}
