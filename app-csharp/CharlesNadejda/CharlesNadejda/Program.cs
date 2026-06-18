using System;
using System.Windows.Forms;
using CharlesNadejda.Forms;
using CharlesNadejda.Models;

namespace CharlesNadejda
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if DEBUG
			DesignSpy.Enable();
#endif

			// 📌 SCRIPT DEFENSE — Étape 0.1 : SFA Pattern (Show First Approach)
			// FrmLogin en dialogue bloquant avant la boucle de messages.
			// FrmPrincipal devient la Form racine — Application.Exit() dans OnFormClosed reste valide.
			Utilisateur user;
			using (var login = new FrmLogin())
			{
				if (login.ShowDialog() != DialogResult.OK)
					return;   // Annulation login → quitter proprement
				user = login.Utilisateur;
			}

            Application.Run(new FrmPrincipal(user));
        }
    }
}
