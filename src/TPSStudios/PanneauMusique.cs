using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Mini-lecteur qui s'ouvre au survol du bouton « Musique » du menu : choix du lecteur
/// (Spotify / Apple Music), titre en cours, précédent / lecture / suivant, volume.
/// Il se referme dès que la souris quitte le bouton et le panneau.
/// </summary>
internal class PanneauMusique : Form
{
	private static PanneauMusique courant;

	private readonly Timer tm = new Timer();

	private readonly List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private Point souris = new Point(-1, -1);

	private Rectangle ancre;

	private int dehors;

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(11f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fP = new Font("Segoe UI Semibold", Zx.P(8.5f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(13f));

	private static readonly Font fGp = new Font("Segoe MDL2 Assets", Zx.P(16f));

	private static readonly Color CSpotify = Color.FromArgb(30, 185, 84);

	private static readonly Color CApple = Color.FromArgb(250, 45, 85);

	protected override bool ShowWithoutActivation => true;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams cp = base.CreateParams;
			cp.ExStyle |= 0x80 | 0x08000000;   // pas dans Alt+Tab, ne prend pas le focus
			cp.ClassStyle |= 0x20000;          // ombre
			return cp;
		}
	}

	/// <summary>Affiche le panneau à droite du bouton (coordonnées écran) ; null : le referme.</summary>
	public static void Montrer(Form proprio, Rectangle bouton)
	{
		if (courant == null || courant.IsDisposed)
		{
			courant = new PanneauMusique();
		}
		courant.Placer(proprio, bouton);
	}

	public static void Fermer()
	{
		if (courant != null && !courant.IsDisposed)
		{
			courant.Close();
		}
		courant = null;
	}

	private PanneauMusique()
	{
		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		StartPosition = FormStartPosition.Manual;
		BackColor = Color.FromArgb(18, 22, 48);
		ClientSize = new Size(Zx.P(340), Zx.P(214));
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, value: true);
		using (GraphicsPath p = Bouton.Rond(new Rectangle(0, 0, Width, Height), Zx.P(14)))
		{
			Region = new Region(p);
		}
		tm.Interval = 120;
		tm.Tick += delegate
		{
			Point c = Cursor.Position;
			Rectangle moi = Bounds;
			// le pont entre le bouton et le panneau compte comme « dedans »
			Rectangle pont = Rectangle.FromLTRB(ancre.Left, Math.Min(ancre.Top, moi.Top), moi.Left + Zx.P(4), Math.Max(ancre.Bottom, moi.Bottom));
			if (moi.Contains(c) || ancre.Contains(c) || pont.Contains(c))
			{
				dehors = 0;
			}
			else if (++dehors >= 3)
			{
				Fermer();
				return;
			}
			if (++rafraichir % 4 == 0)
			{
				Invalidate();
			}
		};
		FormClosed += delegate
		{
			tm.Stop();
			tm.Dispose();
		};
	}

	private int rafraichir;

	private void Placer(Form proprio, Rectangle bouton)
	{
		ancre = bouton;
		dehors = 0;
		Rectangle ecran = Screen.FromRectangle(bouton).WorkingArea;
		int x = bouton.Right + Zx.P(8);
		int y = bouton.Bottom - Height + Zx.P(10);
		y = Math.Max(ecran.Top + Zx.P(8), Math.Min(y, ecran.Bottom - Height - Zx.P(8)));
		Location = new Point(x, y);
		if (!Visible)
		{
			if (proprio != null && !proprio.IsDisposed)
			{
				Owner = proprio;
			}
			Show();
			tm.Start();
		}
		Invalidate();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		souris = e.Location;
		bool main = clics.Exists((KeyValuePair<Rectangle, Action> k) => k.Key.Contains(e.Location));
		Cursor = main ? Cursors.Hand : Cursors.Default;
		Invalidate();
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		souris = new Point(-1, -1);
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		foreach (KeyValuePair<Rectangle, Action> k in new List<KeyValuePair<Rectangle, Action>>(clics))
		{
			if (k.Key.Contains(e.Location))
			{
				Sons.Clic();
				k.Value();
				Invalidate();
				break;
			}
		}
		base.OnMouseClick(e);
	}

	private void Pastille(Graphics g, Rectangle r, string texte, Color c, bool actif, Action a)
	{
		bool sv = r.Contains(souris);
		using (GraphicsPath p = Bouton.Rond(r, r.Height / 2))
		{
			using SolidBrush b = new SolidBrush(actif ? Color.FromArgb(70, c) : Color.FromArgb(sv ? 34 : 18, 255, 255, 255));
			g.FillPath(b, p);
			using Pen pen = new Pen(actif ? Color.FromArgb(200, c) : Color.FromArgb(40, 255, 255, 255));
			g.DrawPath(pen, p);
		}
		Lang.DrawText(g, texte, fP, r, actif ? Color.White : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		clics.Add(new KeyValuePair<Rectangle, Action>(r, a));
	}

	private void Rond(Graphics g, Rectangle r, string glyphe, Font f, Color c, bool plein, Action a)
	{
		bool sv = r.Contains(souris);
		using (SolidBrush b = new SolidBrush(plein ? (sv ? ControlPaint.Light(c, 0.15f) : c) : Color.FromArgb(sv ? 40 : 18, 255, 255, 255)))
		{
			g.FillEllipse(b, r);
		}
		Lang.DrawText(g, glyphe, f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		clics.Add(new KeyValuePair<Rectangle, Action>(r, a));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		int w = ClientSize.Width;
		int h = ClientSize.Height;
		using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(26, 32, 70), Color.FromArgb(14, 17, 40), 90f))
		{
			g.FillRectangle(lg, 0, 0, w, h);
		}
		using (GraphicsPath bord = Bouton.Rond(new Rectangle(0, 0, w - 1, h - 1), Zx.P(14)))
		{
			using Pen pen = new Pen(Color.FromArgb(46, 255, 255, 255));
			g.DrawPath(pen, bord);
		}
		string mode = Musique.Mode;
		// choix du lecteur
		int m = Zx.P(16);
		Lang.DrawText(g, "MUSIQUE", F.Rubrique, new Point(m, Zx.P(16)), C.Gris);
		int pw = Zx.P(92);
		int ph = Zx.P(26);
		Rectangle rA = new Rectangle(w - m - pw, Zx.P(10), pw, ph);
		Rectangle rS = new Rectangle(rA.X - Zx.P(6) - pw, Zx.P(10), pw, ph);
		Pastille(g, rS, "Spotify", CSpotify, mode == "spotify", delegate
		{
			Config.Ecrire("musique", "spotify");
		});
		Pastille(g, rA, "Apple Music", CApple, mode == "apple", delegate
		{
			Config.Ecrire("musique", "apple");
		});
		// titre en cours
		string enCours = Musique.EnCours();
		string lecteur = Musique.Lecteur;
		Color couleur = ((mode == "apple" || lecteur == "Apple Music" || lecteur == "iTunes") ? CApple : ((mode == "spotify" || lecteur == "Spotify") ? CSpotify : C.Jade));
		Rectangle rIc = new Rectangle(m, Zx.P(50), Zx.P(52), Zx.P(52));
		using (GraphicsPath p = Bouton.Rond(rIc, Zx.P(10)))
		{
			using LinearGradientBrush b = new LinearGradientBrush(rIc, couleur, ControlPaint.Dark(couleur, 0.3f), 45f);
			g.FillPath(b, p);
		}
		Lang.DrawText(g, "", fGp, rIc, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		string titre = enCours;
		string sous = "";
		int tiret = enCours.IndexOf(" - ");
		if (tiret > 0)
		{
			sous = enCours.Substring(0, tiret);
			titre = enCours.Substring(tiret + 3);
		}
		if (enCours.Length == 0)
		{
			titre = (lecteur.Length > 0) ? (lecteur + " est ouvert") : "Aucune musique en cours";
			sous = (lecteur.Length > 0) ? "En pause : lancez un titre" : ("Choisissez votre lecteur : " + Musique.NomMode);
		}
		else if (lecteur.Length > 0)
		{
			sous = ((sous.Length > 0) ? (sous + "  ·  ") : "") + lecteur;
		}
		int tx = rIc.Right + Zx.P(12);
		Lang.DrawText(g, titre, fT, new Rectangle(tx, Zx.P(56), w - tx - m, Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		Lang.DrawText(g, sous, fS, new Rectangle(tx, Zx.P(80), w - tx - m, Zx.P(18)), (enCours.Length > 0) ? couleur : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		// commandes
		int cy = Zx.P(116);
		int cx = w / 2;
		int d = Zx.P(44);
		int d2 = Zx.P(36);
		Rond(g, new Rectangle(cx - d / 2, cy, d, d), (enCours.Length > 0) ? "" : "", fGp, C.Orange, plein: true, delegate
		{
			Musique.Basculer();
		});
		Rond(g, new Rectangle(cx - d / 2 - Zx.P(12) - d2, cy + (d - d2) / 2, d2, d2), "", fG, C.Jade, plein: false, delegate
		{
			Musique.Commande("precedent");
		});
		Rond(g, new Rectangle(cx + d / 2 + Zx.P(12), cy + (d - d2) / 2, d2, d2), "", fG, C.Jade, plein: false, delegate
		{
			Musique.Commande("suivant");
		});
		Rond(g, new Rectangle(m, cy + (d - d2) / 2, d2, d2), "", fG, C.Gris, plein: false, delegate
		{
			Musique.Touche(174);
		});
		Rond(g, new Rectangle(w - m - d2, cy + (d - d2) / 2, d2, d2), "", fG, C.Gris, plein: false, delegate
		{
			Musique.Touche(175);
		});
		// ouvrir le lecteur
		Rectangle rO = new Rectangle(m, h - Zx.P(40), w - 2 * m, Zx.P(28));
		bool sv = rO.Contains(souris);
		using (GraphicsPath p = Bouton.Rond(rO, rO.Height / 2))
		{
			using SolidBrush b = new SolidBrush(Color.FromArgb(sv ? 34 : 16, 255, 255, 255));
			g.FillPath(b, p);
		}
		Lang.DrawText(g, "Ouvrir le lecteur  ›", fP, rO, sv ? Color.White : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		clics.Add(new KeyValuePair<Rectangle, Action>(rO, delegate
		{
			Fermer();
			Musique.AfficherLecteur();
		}));
	}
}
