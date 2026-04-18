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
            Application.Run(new FrmLogin());
        }
    }
}
