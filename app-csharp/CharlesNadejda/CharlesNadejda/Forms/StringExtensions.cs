namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Helpers d'extension sur string — utilisés dans les formulaires d'édition
    /// pour convertir les TextBox vides en NULL côté DB.
    /// </summary>
    internal static class StringExtensions
    {
        public static string NullIfEmpty(this string s) =>
            string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
