using System;
using System.Windows.Forms;
using CharlesNadejda.Forms;

namespace CharlesNadejda
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // SFA Pattern : FrmLogin en dialogue bloquant avant la boucle de messages.
            // FrmPrincipal devient la Form racine — Application.Exit() dans OnFormClosed reste valide.
            var login = new FrmLogin();
            if (login.ShowDialog() != DialogResult.OK)
                return;   // Annulation login → quitter proprement

            Application.Run(new FrmPrincipal(login.Utilisateur));
        }
    }
}
