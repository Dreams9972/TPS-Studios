using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Integre
{
	public static readonly Font fNav = new Font("Segoe MDL2 Assets", Zx.P(14f));

	public static Size Hote
	{
		get
		{
			Fenetre principale = Fenetre.Principale;
			if (principale == null || principale.IsDisposed || !principale.IsHandleCreated)
			{
				return new Size(1280, 760);
			}
			return principale.ClientSize;
		}
	}

	public static Bouton Retour(EventHandler h, int x, int y)
	{
		Bouton b = new Bouton("\ue72b");
		b.Plein = false;
		b.Font = fNav;
		b.Size = new Size(Zx.P(48), Zx.P(48));
		b.Location = new Point(x, y);
		b.Click += h;
		b.ParentChanged += delegate
		{
			AjouterAccueil(b);
		};
		return b;
	}

	public static Bouton Accueil(int x, int y)
	{
		Bouton bouton = new Bouton("\ue80f");
		bouton.Plein = false;
		bouton.Font = fNav;
		bouton.Size = new Size(Zx.P(48), Zx.P(48));
		bouton.Location = new Point(x, y);
		bouton.Click += delegate
		{
			Fenetre principale = Fenetre.Principale;
			if (principale != null && !principale.IsDisposed)
			{
				principale.AllerAccueil();
			}
		};
		return bouton;
	}

	private static void AjouterAccueil(Bouton b)
	{
		Control parent = b.Parent;
		if (parent == null || b.Tag is Bouton)
		{
			return;
		}
		Bouton i = Accueil(b.Left + Zx.P(56), b.Top);
		i.Anchor = b.Anchor;
		b.Tag = i;
		Action<Control> dec = delegate(Control c)
		{
			if (c != b && c != i && c.Top < b.Bottom && c.Bottom > b.Top && c.Left >= b.Right && c.Left < b.Right + 64)
			{
				c.Left += 56;
			}
		};
		foreach (Control control in parent.Controls)
		{
			dec(control);
		}
		parent.Controls.Add(i);
		parent.ControlAdded += delegate(object o, ControlEventArgs e)
		{
			dec(e.Control);
		};
	}

	public static DialogResult Afficher(this Form f, IWin32Window proprietaire)
	{
		Fenetre principale = Fenetre.Principale;
		if (principale == null || principale.IsDisposed || !principale.IsHandleCreated)
		{
			return f.ShowDialog(proprietaire);
		}
		return principale.Integrer(f);
	}
}
