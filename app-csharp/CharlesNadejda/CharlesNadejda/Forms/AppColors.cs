using System.Drawing;

namespace CharlesNadejda.Forms
{
    /// <summary>
    /// Palette de couleurs centralisée de l'application ArtisaStock.
    ///
    /// TICKET-12 : remplace les constantes Color private dupliquées dans chaque Form.
    /// Source de vérité unique — toute modification de charte graphique ici
    /// se propage automatiquement à l'ensemble de l'interface.
    /// </summary>
    internal static class AppColors
    {
        // ── Chocolat — couleurs de marque ────────────────────────────────
        /// <summary>#3D2817 — brun chocolat foncé, couleur de marque principale.</summary>
        public static readonly Color ChocoBrand  = Color.FromArgb(61,  40,  23);

        /// <summary>#6F4E37 — brun chocolat moyen, textes secondaires.</summary>
        public static readonly Color ChocoMed    = Color.FromArgb(111, 78,  55);

        /// <summary>#1E0F08 — brun très sombre, effet profondeur (sidebar active).</summary>
        public static readonly Color ChocoAbyss  = Color.FromArgb(30,  15,   8);

        /// <summary>#2C1810 — brun sombre, variante intermédiaire.</summary>
        public static readonly Color ChocoDark   = Color.FromArgb(44,  24,  16);

        // ── Crème — fonds ────────────────────────────────────────────────
        /// <summary>#F5E6D3 — crème rosée, fond de panneaux principaux.</summary>
        public static readonly Color Creme       = Color.FromArgb(245, 230, 211);

        /// <summary>#FDFBF6 — crème très clair, fond zones de contenu (hub).</summary>
        public static readonly Color CremeWarm   = Color.FromArgb(253, 251, 246);

        /// <summary>#ECE9D8 — crème neutre, fond header / grilles.</summary>
        public static readonly Color CremeBg     = Color.FromArgb(236, 233, 216);

        // ── Or / Bouton primaire ─────────────────────────────────────────
        /// <summary>#D4AF37 — or, bouton CTA principal.</summary>
        public static readonly Color Or          = Color.FromArgb(212, 175,  55);

        // ── Neutres interface ────────────────────────────────────────────
        /// <summary>#EFEAE1 — gris chaud, fond boutons secondaires.</summary>
        public static readonly Color GreyBtn     = Color.FromArgb(239, 234, 225);

        /// <summary>#C3B9A8 — taupe clair, bordures de panneaux et séparateurs.</summary>
        public static readonly Color Border      = Color.FromArgb(195, 185, 168);

        // ── Sidebar ──────────────────────────────────────────────────────
        /// <summary>#E8D9C0 — beige doré, texte principal sidebar.</summary>
        public static readonly Color SidebarTxt  = Color.FromArgb(232, 217, 192);

        /// <summary>#9E7B5C — brun clair, métadonnées / infos sidebar.</summary>
        public static readonly Color SidebarMeta = Color.FromArgb(158, 123,  92);

        /// <summary>#C8AF8C — beige clair, texte hint/note sur fond sombre.</summary>
        public static readonly Color HintOnDark  = Color.FromArgb(200, 175, 140);

        // ── Statuts ──────────────────────────────────────────────────────
        /// <summary>#3EA23E — vert, stock disponible / état OK.</summary>
        public static readonly Color GreenOk     = Color.FromArgb(62,  162,  62);

        /// <summary>#C72C48 — rouge, rupture de stock / état critique.</summary>
        public static readonly Color RedCrit     = Color.FromArgb(199,  44,  72);

        /// <summary>#D35400 — orange, alerte / avertissement.</summary>
        public static readonly Color OrgWarn     = Color.FromArgb(211,  84,   0);

        // ── Fonds de lignes (vue stock) ──────────────────────────────────
        /// <summary>#E0F3E0 — fond vert pâle, ingrédient disponible.</summary>
        public static readonly Color VertDispo    = Color.FromArgb(224, 243, 224);

        /// <summary>#FFEDCC — fond orange pâle, ingrédient réservé.</summary>
        public static readonly Color OrangeReserv = Color.FromArgb(255, 237, 204);

        /// <summary>#FFDADA — fond rouge pâle, ingrédient en pénurie.</summary>
        public static readonly Color RougePenur   = Color.FromArgb(255, 218, 218);

        // ── Shell ERP — title bar, sidebar, status bar ───────────────
        /// <summary>#FBF9F4 — fond status bar et toolbar.</summary>
        public static readonly Color Surface      = Color.FromArgb(250, 247, 241);

        /// <summary>#43301A — fond nav item actif dans la sidebar.</summary>
        public static readonly Color SidebarActive = Color.FromArgb(67, 45, 26);

        /// <summary>#372616 — fond nav item hover dans la sidebar.</summary>
        public static readonly Color SidebarHover  = Color.FromArgb(55, 38, 22);

        /// <summary>#ECE5D7 — bordure fine entre sections (ligne 1).</summary>
        public static readonly Color Line1         = Color.FromArgb(236, 229, 215);

        /// <summary>#DDD4C0 — bordure moyenne (ligne 2).</summary>
        public static readonly Color Line2         = Color.FromArgb(221, 212, 192);

        /// <summary>#1B7A3E — vert sémantique success (plus sombre que GreenOk).</summary>
        public static readonly Color Success       = Color.FromArgb(27, 122, 62);

        /// <summary>#316AC5 — bleu info / sélection.</summary>
        public static readonly Color Info          = Color.FromArgb(49, 106, 197);

        // ── Grille / DGV ───────────────────────────────────────────────────
        /// <summary>#E6DCD2 — couleur des lignes de grille DGV.</summary>
        public static readonly Color GridLine      = Color.FromArgb(230, 220, 210);

        /// <summary>#DCD0C0 — fond des chips inactifs / boutons secondaires tertiaires.</summary>
        public static readonly Color ChipInactive  = Color.FromArgb(220, 208, 192);
    }
}
