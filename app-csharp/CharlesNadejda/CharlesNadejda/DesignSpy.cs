#if DEBUG
using System.Diagnostics;
using System.Windows.Forms;

namespace CharlesNadejda
{
	/// <summary>
	/// Outil de dev : Ctrl + clic droit sur un contrôle en exécution
	/// pour identifier son nom et sa Form. Actif uniquement en DEBUG.
	/// </summary>
	internal static class DesignSpy
	{
		public static void Enable() => Application.AddMessageFilter(new ClickSpy());

		private class ClickSpy : IMessageFilter
		{
			private const int WM_RBUTTONDOWN = 0x0204;

			public bool PreFilterMessage(ref Message m)
			{
				if (m.Msg == WM_RBUTTONDOWN
					&& (Control.ModifierKeys & Keys.Control) == Keys.Control)
				{
					var ctrl = Control.FromHandle(m.HWnd);
					if (ctrl != null)
					{
						var form = ctrl.FindForm();
						var info =
							$"Contrôle : {ctrl.Name} ({ctrl.GetType().Name})\n" +
							$"Form     : {form?.Name}\n" +
							$"Fichier  : {form?.GetType().Name}.cs\n" +
							$"Parent   : {ctrl.Parent?.Name}";

						Debug.WriteLine("═══ SPY ═══\n" + info);

						if (!string.IsNullOrEmpty(ctrl.Name))
							Clipboard.SetText(ctrl.Name);

						MessageBox.Show(info, "DesignSpy",
							MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
					return true;
				}
				return false;
			}
		}
	}
}
#endif