using System.Windows.Forms;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Utilitaires UI réutilisables pour tous les formulaires.
    /// </summary>
    internal static class FormHelper
    {
        /// <summary>
        /// Convertit la touche "." en séparateur décimal de la locale courante (",")
        /// sur chaque NumericUpDown fourni. Permet une saisie clavier naturelle sur
        /// les pavés numériques qui n'émettent que ".".
        /// </summary>
        internal static void ActiverPointDecimal(params NumericUpDown[] nuds)
        {
            foreach (var nud in nuds)
            {
                nud.KeyPress += (s, e) =>
                {
                    if (e.KeyChar == '.')
                        e.KeyChar = System.Globalization.NumberFormatInfo.CurrentInfo
                                        .NumberDecimalSeparator[0];
                };
            }
        }

        /// <summary>
        /// Sélectionne tout le texte à la prise de focus — le premier caractère saisi
        /// remplace immédiatement la valeur existante, sans avoir à effacer manuellement.
        /// </summary>
        internal static void ActiverSelectionAuFocus(params NumericUpDown[] nuds)
        {
            foreach (var nud in nuds)
                nud.Enter += (s, e) => ((NumericUpDown)s).Select(0, ((NumericUpDown)s).Text.Length);
        }
    }
}
