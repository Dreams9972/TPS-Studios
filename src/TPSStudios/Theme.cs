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

	private static int Ref(Color c)
	{
		return c.R | (c.G << 8) | (c.B << 16);
	}

	public static void Fenetre(Form f)
	{
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
				SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
			}
			catch
			{
			}
		};
		if (c.IsHandleCreated)
		{
			eventHandler(c, EventArgs.Empty);
		}
		else
		{
			c.HandleCreated += eventHandler;
		}
	}
}
