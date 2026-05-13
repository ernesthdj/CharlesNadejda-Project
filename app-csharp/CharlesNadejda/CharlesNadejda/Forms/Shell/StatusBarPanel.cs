using System;
using System.Drawing;
using System.Windows.Forms;
using CharlesNadejda.Navigation;

namespace CharlesNadejda.Forms.Shell
{
    /// <summary>
    /// Barre de statut inférieure (26px).
    /// Affiche : état connexion · activité · contexte · niveau · version · hints clavier.
    /// </summary>
    internal sealed class StatusBarPanel : Panel
    {
        private readonly Label _lblConnected;
        private readonly Label _lblActivite;
        private readonly Label _lblContexte;
        private readonly Label _lblNiveau;
        private readonly Label _lblVersion;
        private readonly Label _lblHints;

        private const int BAR_HEIGHT = 26;

        public StatusBarPanel()
        {
            Height    = BAR_HEIGHT;
            Dock      = DockStyle.Bottom;
            BackColor = AppColors.Surface;
            Padding   = new Padding(14, 0, 14, 0);

            var font     = new Font("Segoe UI", 8.5F);
            var fontBold = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            var fg       = AppColors.ChocoMed;

            // ── Indicateur connexion ─────────────────────────────────
            _lblConnected = MakeLabel("● connecté · charlesnadejda@mysql", font, AppColors.Success);
            _lblConnected.Dock = DockStyle.Left;
            _lblConnected.Padding = new Padding(0, 0, 14, 0);

            // ── Séparateur ──────────────────────────────────────────
            var sep1 = MakeSep();

            // ── Activité ────────────────────────────────────────────
            _lblActivite = MakeLabel("Activité : —", font, fg);
            _lblActivite.Dock = DockStyle.Left;
            _lblActivite.Padding = new Padding(14, 0, 14, 0);

            var sep2 = MakeSep();

            // ── Contexte ────────────────────────────────────────────
            _lblContexte = MakeLabel("Contexte : —", font, fg);
            _lblContexte.Dock = DockStyle.Left;
            _lblContexte.Padding = new Padding(14, 0, 14, 0);

            var sep3 = MakeSep();

            // ── Niveau ──────────────────────────────────────────────
            _lblNiveau = MakeLabel("", font, fg);
            _lblNiveau.Dock = DockStyle.Left;
            _lblNiveau.Padding = new Padding(14, 0, 14, 0);

            // ── Version ─────────────────────────────────────────────
            _lblVersion = MakeLabel("v1.0", font, Color.FromArgb(150, fg));
            _lblVersion.Dock = DockStyle.Left;
            _lblVersion.Padding = new Padding(14, 0, 0, 0);

            // ── Hints clavier (droite) ──────────────────────────────
            _lblHints = MakeLabel("Ctrl+N Nouveau", new Font("Segoe UI", 8F, FontStyle.Italic),
                Color.FromArgb(140, fg));
            _lblHints.Dock = DockStyle.Right;

            // ── Bordure supérieure ──────────────────────────────────
            Paint += (s, e) =>
            {
                using (var pen = new Pen(AppColors.Line2))
                    e.Graphics.DrawLine(pen, 0, 0, Width, 0);
            };

            // Ajout dans l'ordre inverse du Dock (Right d'abord, puis Left de droite à gauche)
            Controls.Add(_lblHints);
            Controls.Add(_lblVersion);
            Controls.Add(sep3);
            Controls.Add(_lblNiveau);
            Controls.Add(sep2);
            Controls.Add(_lblContexte);
            Controls.Add(sep1);
            Controls.Add(_lblActivite);
            Controls.Add(_lblConnected);
        }

        public void UpdateState(AppState state)
        {
            if (state == null) return;

            _lblActivite.Text = state.ActiveActivite != null
                ? $"Activité : {state.ActiveActivite.Nom}"
                : "Activité : —";

            _lblContexte.Text = state.ActiveContexte != null
                ? $"Contexte : {state.ActiveContexte.Nom}"
                : "Contexte : —";

            _lblNiveau.Text = state.ActiveNiveau != null
                ? $"Niveau : N{state.ActiveNiveau.Ordre} {state.ActiveNiveau.Nom}"
                : "";

            _lblNiveau.Visible = state.ActiveNiveau != null;
        }

        private static Label MakeLabel(string text, Font font, Color fg)
        {
            return new Label
            {
                Text      = text,
                Font      = font,
                ForeColor = fg,
                AutoSize  = true,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static Panel MakeSep()
        {
            var sep = new Panel
            {
                Width     = 1,
                Dock      = DockStyle.Left,
                BackColor = AppColors.Line2,
                Margin    = new Padding(0, 5, 0, 5)
            };
            return sep;
        }
    }
}
