using System.Windows.Forms;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Utilitaires UI reutilisables pour tous les formulaires.
    ///
    /// Regroupe les helpers transverses qui evitent la duplication
    /// de code dans les FrmEdit* et autres formulaires :
    ///   - ActiverPointDecimal : conversion "." → "," pour NumericUpDown
    ///   - SelectionnerParId   : selection combo par ID (remplace for/if copie-colle)
    ///   - ActiverSelectionAuFocus : select-all au focus sur NumericUpDown
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
        /// TICKET-27 : Sélectionne dans un ComboBox l'élément dont l'Id correspond à idCible.
        /// Remplace le pattern for/if/SelectedIndex copié-collé dans les formulaires.
        /// Retourne true si l'élément a été trouvé et sélectionné.
        /// </summary>
        internal static bool SelectionnerParId<T>(ComboBox cbo, System.Func<T, int> getId, int idCible)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                if (cbo.Items[i] is T item && getId(item) == idCible)
                {
                    cbo.SelectedIndex = i;
                    return true;
                }
            }
            return false;
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
