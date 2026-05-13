using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CharlesNadejda.Models;

namespace CharlesNadejda.Forms.Shell
{
    /// <summary>
    /// Bandeau supérieur de l'application (38px).
    /// Gradient chocolat avec logo monogramme "C" doré, titre du document courant,
    /// indicateur de connexion MySQL et avatar utilisateur.
    /// </summary>
    internal sealed class TitleBarPanel : Panel
    {
        private readonly Label _lblDocTitle;
        private readonly Label _lblUser;
        private readonly string _initials;

        private const int BAR_HEIGHT = 38;

        public TitleBarPanel(Utilisateur user)
        {
            Height    = BAR_HEIGHT;
            Dock      = DockStyle.Top;
            BackColor = AppColors.ChocoBrand;
            Padding   = new Padding(10, 0, 10, 0);
            DoubleBuffered = true;

            _initials = BuildInitials(user);

            // ── Titre du document (centre) ──────────────────────────
            _lblDocTitle = new Label
            {
                Text      = "Tableau de bord",
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(140, 245, 230, 211),
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblDocTitle);

            // ── Nom utilisateur ─────────────────────────────────────
            _lblUser = new Label
            {
                Text      = user != null ? $"{user.Prenom} {user.Nom}" : "",
                Font      = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(180, 245, 230, 211),
                AutoSize  = true,
                BackColor = Color.Transparent
            };
            Controls.Add(_lblUser);

            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);

            Resize += (s, e) => LayoutChildren();
        }

        public void SetTitle(string title)
        {
            _lblDocTitle.Text = title ?? "Tableau de bord";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // ── Fond gradient ───────────────────────────────────────
            using (var brush = new LinearGradientBrush(
                ClientRectangle,
                AppColors.ChocoBrand,
                AppColors.ChocoDark,
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, ClientRectangle);
            }

            // ── Ligne séparatrice basse ─────────────────────────────
            using (var pen = new Pen(AppColors.ChocoAbyss))
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

            // ── Logo monogramme "C" ─────────────────────────────────
            var logoRect = new Rectangle(10, 8, 22, 22);
            using (var bgBrush = new SolidBrush(AppColors.Or))
            {
                FillRoundedRect(g, bgBrush, logoRect, 3);
            }
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var f = new Font("Georgia", 12F, FontStyle.Bold | FontStyle.Italic))
            {
                g.DrawString("C", f, new SolidBrush(AppColors.ChocoAbyss), logoRect, sf);
            }

            // ── Texte "ArtisaStock" + sous-titre ────────────────────
            int textX = 40;
            using (var fBrand = new Font("Segoe UI", 10F, FontStyle.Bold))
            {
                g.DrawString("ArtisaStock", fBrand,
                    new SolidBrush(AppColors.SidebarTxt), textX, 5);
            }
            using (var fSub = new Font("Segoe UI", 7F))
            {
                g.DrawString("CHARLES & NADEJDA", fSub,
                    new SolidBrush(Color.FromArgb(160, AppColors.Or)), textX, 22);
            }

            // ── Séparateur après brand ──────────────────────────────
            int sepX = textX + 120;
            using (var pen = new Pen(Color.FromArgb(50, AppColors.Or)))
                g.DrawLine(pen, sepX, 8, sepX, 30);

            // ── Indicateur connexion MySQL (côté droit) ─────────────
            int rightX = Width - 14;

            // Avatar cercle
            int avSize = 22;
            int avX = rightX - avSize;
            int avY = 8;
            using (var avBrush = new LinearGradientBrush(
                new Rectangle(avX, avY, avSize, avSize),
                AppColors.Or, AppColors.RedCrit, LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(avBrush, avX, avY, avSize, avSize);
            }
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var fInit = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                g.DrawString(_initials, fInit, Brushes.White,
                    new RectangleF(avX, avY, avSize, avSize), sf);
            }

            // Nom user + indicateur MySQL
            int connX = avX - 10;
            using (var fConn = new Font("Segoe UI", 8.5F))
            {
                var connText = "MySQL · charlesnadejda";
                var connSize = g.MeasureString(connText, fConn);
                int dotR = 5;

                // Green dot
                g.FillEllipse(new SolidBrush(Color.FromArgb(92, 184, 92)),
                    connX - (int)connSize.Width - dotR - 10, 16, dotR, dotR);

                g.DrawString(connText, fConn,
                    new SolidBrush(Color.FromArgb(160, 245, 230, 211)),
                    connX - (int)connSize.Width - 4, 12);
            }
        }

        private void LayoutChildren()
        {
            int docX = 170;
            int docW = Math.Max(100, Width - 460);
            _lblDocTitle.SetBounds(docX, 0, docW, BAR_HEIGHT);

            _lblUser.Location = new Point(Width - 50 - _lblUser.Width, 12);
        }

        private static string BuildInitials(Utilisateur u)
        {
            if (u == null) return "?";
            string p = string.IsNullOrEmpty(u.Prenom) ? "" : u.Prenom.Substring(0, 1);
            string n = string.IsNullOrEmpty(u.Nom)    ? "" : u.Nom.Substring(0, 1);
            return (p + n).ToUpperInvariant();
        }

        private static void FillRoundedRect(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                int d = radius * 2;
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
