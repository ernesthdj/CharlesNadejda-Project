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
    /// Tout est peint via OnPaint — aucun label enfant pour éviter les superpositions.
    /// </summary>
    internal sealed class TitleBarPanel : Panel
    {
        private string _docTitle = "Tableau de bord";
        private readonly string _userName;
        private readonly string _initials;

        private const int BAR_HEIGHT = 38;

        public TitleBarPanel(Utilisateur user)
        {
            Height    = BAR_HEIGHT;
            Dock      = DockStyle.Top;
            BackColor = AppColors.ChocoBrand;
            DoubleBuffered = true;

            _userName = user != null ? $"{user.Prenom} {user.Nom}" : "";
            _initials = BuildInitials(user);

            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void SetTitle(string title)
        {
            _docTitle = title ?? "Tableau de bord";
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode    = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // ── Fond gradient ───────────────────────────────────────
            using (var brush = new LinearGradientBrush(
                ClientRectangle, AppColors.ChocoBrand, AppColors.ChocoDark,
                LinearGradientMode.Vertical))
                g.FillRectangle(brush, ClientRectangle);

            // ── Ligne séparatrice basse ─────────────────────────────
            using (var pen = new Pen(AppColors.ChocoAbyss))
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

            // ── Logo monogramme "C" ─────────────────────────────────
            var logoRect = new Rectangle(12, 8, 22, 22);
            using (var bgBrush = new SolidBrush(AppColors.Or))
                FillRoundedRect(g, bgBrush, logoRect, 3);

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var f = new Font("Georgia", 12F, FontStyle.Bold | FontStyle.Italic))
            using (var br = new SolidBrush(AppColors.ChocoAbyss))
                g.DrawString("C", f, br, logoRect, sf);

            // ── Texte "ArtisaStock" + sous-titre ────────────────────
            int textX = 42;
            using (var fBrand = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (var br = new SolidBrush(AppColors.SidebarTxt))
                g.DrawString("ArtisaStock", fBrand, br, textX, 5);

            using (var fSub = new Font("Segoe UI", 7F))
            using (var br = new SolidBrush(Color.FromArgb(160, AppColors.Or)))
                g.DrawString("CHARLES & NADEJDA", fSub, br, textX, 22);

            // ── Séparateur après brand ──────────────────────────────
            int sepX = textX + 120;
            using (var pen = new Pen(Color.FromArgb(50, AppColors.Or)))
                g.DrawLine(pen, sepX, 8, sepX, 30);

            // ── Titre du document (centre) ──────────────────────────
            int docX = sepX + 12;
            using (var fDoc = new Font("Segoe UI", 9.5F))
            using (var br = new SolidBrush(Color.FromArgb(140, 245, 230, 211)))
                g.DrawString(_docTitle, fDoc, br, docX, 10);

            // ── Zone droite : MySQL · Nom user · Avatar ─────────────
            int rightEdge = Width - 14;

            // Avatar cercle
            int avSize = 24;
            int avX = rightEdge - avSize;
            int avY = (BAR_HEIGHT - avSize) / 2;
            using (var avBrush = new LinearGradientBrush(
                new Rectangle(avX, avY, avSize, avSize),
                AppColors.Or, AppColors.RedCrit, LinearGradientMode.ForwardDiagonal))
                g.FillEllipse(avBrush, avX, avY, avSize, avSize);

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var fInit = new Font("Segoe UI", 8F, FontStyle.Bold))
                g.DrawString(_initials, fInit, Brushes.White,
                    new RectangleF(avX, avY, avSize, avSize), sf);

            // Nom utilisateur
            int nameX = avX - 8;
            using (var fName = new Font("Segoe UI", 8.5F))
            using (var br = new SolidBrush(Color.FromArgb(200, AppColors.SidebarTxt)))
            {
                var nameSz = g.MeasureString(_userName, fName);
                nameX -= (int)nameSz.Width;
                g.DrawString(_userName, fName, br, nameX, 11);
            }

            // Séparateur
            int sep2X = nameX - 10;
            using (var pen = new Pen(Color.FromArgb(50, AppColors.Or)))
                g.DrawLine(pen, sep2X, 10, sep2X, 28);

            // Indicateur MySQL
            int mysqlX = sep2X - 8;
            using (var fConn = new Font("Segoe UI", 8F))
            using (var br = new SolidBrush(Color.FromArgb(150, AppColors.SidebarTxt)))
            {
                string connText = "MySQL";
                var connSz = g.MeasureString(connText, fConn);
                mysqlX -= (int)connSz.Width;

                // Green dot
                int dotSize = 6;
                int dotX = mysqlX - dotSize - 4;
                int dotY = (BAR_HEIGHT - dotSize) / 2;
                using (var dotBr = new SolidBrush(Color.FromArgb(92, 184, 92)))
                    g.FillEllipse(dotBr, dotX, dotY, dotSize, dotSize);

                g.DrawString(connText, fConn, br, mysqlX, 12);
            }
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
