using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Theme
{
	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr h, int attr, ref int v, int taille);

	[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
	private static extern int SetWindowTheme(IntPtr h, string app, string ids);

	/// <summary>uxtheme #135 (Windows 10 1903+) : SetPreferredAppMode, 2 = toujours sombre.</summary>
	[DllImport("uxtheme.dll", EntryPoint = "#135")]
	private static extern int SetPreferredAppMode(int mode);

	/// <summary>uxtheme #133 (Windows 10 1809+) : AllowDarkModeForWindow.</summary>
	[DllImport("uxtheme.dll", EntryPoint = "#133")]
	private static extern bool AllowDarkModeForWindow(IntPtr h, bool allow);

	/// <summary>uxtheme #136 : FlushMenuThemes.</summary>
	[DllImport("uxtheme.dll", EntryPoint = "#136")]
	private static extern void FlushMenuThemes();

	private static int modeSombre = -1;

	private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, object> deja = new System.Runtime.CompilerServices.ConditionalWeakTable<Control, object>();

	/// <summary>
	/// Active le mode sombre de Windows pour tout le lanceur : barres de défilement, champs et
	/// listes deviennent sombres (Windows 10 1903 et plus récent ; sans effet ailleurs).
	/// </summary>
	public static bool ModeSombre()
	{
		if (modeSombre < 0)
		{
			modeSombre = 0;
			try
			{
				if (Environment.OSVersion.Platform == PlatformID.Win32NT && Type.GetType("Mono.Runtime") == null)
				{
					object b = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuildNumber", "0");
					if (int.TryParse(Convert.ToString(b), out var build) && build >= 18362)
					{
						SetPreferredAppMode(2);
						FlushMenuThemes();
						modeSombre = 1;
					}
				}
			}
			catch
			{
				modeSombre = 0;
			}
		}
		return modeSombre == 1;
	}

	/// <summary>Thème sombre sur tout ce qui défile dans ce contrôle, et sur ce qu'on y ajoutera.</summary>
	public static void SombreTout(Control racine)
	{
		if (racine == null)
		{
			return;
		}
		if (Defile(racine))
		{
			Sombre(racine);
		}
		foreach (Control c in racine.Controls)
		{
			SombreTout(c);
		}
		racine.ControlAdded -= Ajoute;
		racine.ControlAdded += Ajoute;
	}

	private static void Ajoute(object o, ControlEventArgs e)
	{
		SombreTout(e.Control);
	}

	private static bool Defile(Control c)
	{
		if (c is TextBoxBase || c is ListBox || c is ListView || c is TreeView || c is ComboBox || c is DataGridView)
		{
			return true;
		}
		return c is ScrollableControl;
	}

	private static int Ref(Color c)
	{
		return c.R | (c.G << 8) | (c.B << 16);
	}

	public static void Fenetre(Form f)
	{
		ModeSombre();
		SombreTout(f);
		EventHandler eventHandler = delegate
		{
			try
			{
				int v = 1;
				int v2 = Ref(C.Cote);
				int v3 = Ref(C.Texte);
				int v4 = Ref(C.Ligne);
				IntPtr handle = f.Handle;
				DwmSetWindowAttribute(handle, 20, ref v, 4);
				DwmSetWindowAttribute(handle, 19, ref v, 4);
				DwmSetWindowAttribute(handle, 35, ref v2, 4);
				DwmSetWindowAttribute(handle, 36, ref v3, 4);
				DwmSetWindowAttribute(handle, 34, ref v4, 4);
			}
			catch
			{
			}
		};
		if (f.IsHandleCreated)
		{
			eventHandler(f, EventArgs.Empty);
		}
		else
		{
			f.HandleCreated += eventHandler;
		}
	}

	public static void Sombre(Control c)
	{
		EventHandler eventHandler = delegate
		{
			try
			{
				if (ModeSombre())
				{
					AllowDarkModeForWindow(c.Handle, allow: true);
				}
				SetWindowTheme(c.Handle, (c is ComboBox) ? "DarkMode_CFD" : "DarkMode_Explorer", null);
			}
			catch
			{
			}
		};
		if (deja.TryGetValue(c, out var _))
		{
			return;
		}
		deja.Add(c, null);
		if (c.IsHandleCreated)
		{
			eventHandler(c, EventArgs.Empty);
		}
		c.HandleCreated += eventHandler;
	}
}
