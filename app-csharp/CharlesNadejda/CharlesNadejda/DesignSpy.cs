#if DEBUG
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace CharlesNadejda
{
	/// <summary>
	/// Outil de dev : Ctrl + clic droit sur un contrôle en exécution
	/// pour identifier son nom, sa Form et ses event handlers.
	/// Actif uniquement en DEBUG.
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
						var sb = new StringBuilder();

						// ── Identité ────────────────────────────
						sb.AppendLine($"Contrôle : {ctrl.Name} ({ctrl.GetType().Name})");
						sb.AppendLine($"Form     : {form?.Name}");
						sb.AppendLine($"Fichier  : {form?.GetType().Name}.cs");
						sb.AppendLine($"Parent   : {ctrl.Parent?.Name} ({ctrl.Parent?.GetType().Name})");

						// ── Event handlers ──────────────────────
						var handlers = GetEventHandlers(ctrl);
						if (handlers.Count > 0)
						{
							sb.AppendLine();
							sb.AppendLine("── Événements câblés ──");
							foreach (var kvp in handlers)
							{
								sb.AppendLine($"  {kvp.Key}:");
								foreach (var method in kvp.Value)
									sb.AppendLine($"    → {method}");
							}
						}
						else
						{
							sb.AppendLine();
							sb.AppendLine("(aucun événement câblé détecté)");
						}

						var info = sb.ToString().TrimEnd();
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

		/// <summary>
		/// Inspecte un contrôle via réflexion pour extraire tous les event handlers
		/// attachés (Click, TextChanged, SelectedIndexChanged, etc.).
		/// Retourne un dictionnaire { NomÉvénement → Liste de signatures de méthodes }.
		/// </summary>
		private static Dictionary<string, List<string>> GetEventHandlers(Control ctrl)
		{
			var result = new Dictionary<string, List<string>>();

			// WinForms stocke les delegates dans un EventHandlerList interne
			// accessible via le champ privé "events" hérité de Component.
			var eventsField = typeof(Component).GetField("events",
				BindingFlags.NonPublic | BindingFlags.Instance);
			if (eventsField == null) return result;

			var eventHandlerList = eventsField.GetValue(ctrl) as EventHandlerList;
			if (eventHandlerList == null) return result;

			// Chaque événement WinForms utilise un champ static "EventXxx" comme clé
			// dans l'EventHandlerList. On les cherche sur le type du contrôle.
			var ctrlType = ctrl.GetType();
			var visited = new HashSet<string>();

			// Parcourir toute la hiérarchie de types (Button → ButtonBase → Control → Component)
			for (var type = ctrlType; type != null; type = type.BaseType)
			{
				var fields = type.GetFields(
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

				foreach (var field in fields)
				{
					// Convention WinForms : les clés sont des champs nommés "EventXxx"
					if (!field.Name.StartsWith("Event", StringComparison.Ordinal))
						continue;

					var eventName = field.Name.Substring(5); // "EventClick" → "Click"
					if (visited.Contains(eventName)) continue;
					visited.Add(eventName);

					var key = field.GetValue(null);
					if (key == null) continue;

					// Récupérer le delegate via la méthode interne Find de la head list
					var del = FindDelegate(eventHandlerList, key);
					if (del == null) continue;

					var methods = new List<string>();
					foreach (var d in del.GetInvocationList())
					{
						var mi = d.Method;
						var declaring = mi.DeclaringType?.Name ?? "?";
						var methodName = mi.Name;

						// Lambda compiler-generated : extraire un nom lisible
						if (methodName.Contains("<"))
						{
							// Ex: "<BuildTabCategories>b__42_0" → "BuildTabCategories (lambda)"
							var start = methodName.IndexOf('<') + 1;
							var end = methodName.IndexOf('>');
							if (end > start)
								methodName = methodName.Substring(start, end - start) + " (lambda)";
						}

						methods.Add($"{declaring}.{methodName}");
					}

					if (methods.Count > 0)
						result[eventName] = methods;
				}
			}

			return result;
		}

		/// <summary>
		/// Accède au delegate stocké dans l'EventHandlerList pour une clé donnée.
		/// Utilise la réflexion car EventHandlerList n'expose pas ses entrées publiquement.
		/// </summary>
		private static Delegate FindDelegate(EventHandlerList list, object key)
		{
			// Approche 1 : l'indexeur public list[key] retourne le delegate
			// EventHandlerList a un indexeur this[object] qui retourne Delegate.
			try
			{
				var indexer = typeof(EventHandlerList).GetProperty("Item",
					BindingFlags.Public | BindingFlags.Instance,
					null, typeof(Delegate), new[] { typeof(object) }, null);

				if (indexer != null)
					return indexer.GetValue(list, new[] { key }) as Delegate;
			}
			catch
			{
				// Fallback silencieux
			}

			return null;
		}
	}
}
#endif
