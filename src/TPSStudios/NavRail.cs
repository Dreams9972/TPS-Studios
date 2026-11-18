using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Barre de navigation commune, à gauche de la fenêtre, visible sur toutes les pages principales
/// (Accueil, Jeux, Bibliothèque, Mods, page d'un mod). L'élément de la page en cours est mis en avant.
/// </summary>
internal class NavRail : Control
{
	public class Element
	{
		public string Glyphe;

		public string Libelle;

		public string Bulle;

		public Action Action;

		public Func<bool> Actif;

		public bool EnBas;

		/// <summary>Pastille rouge (nombre de messages non lus…), 0 : rien.</summary>
		public Func<int> Badge;
	}

	private readonly List<Element> elements = new List<Element>();

	private readonly List<Rectangle> zones = new List<Rectangle>();

	private int survol = -1;

	private int appui = -1;

	public Image Logo;

	public Action ClicLogo;

	public ToolTip Infobulle;

	/// <summary>Survol d'un élément (null : plus rien de survolé), avec sa zone en coordonnées écran.</summary>
	public Action<Element, Rectangle> Survol;

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(16f));

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(8.5f));

	public static int Largeur => Zx.P(84);

	public NavRail()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		BackColor = Color.FromArgb(9, 11, 28);
		base.Width = Largeur;
		base.Dock = DockStyle.Left;
		base.TabStop = false;
	}

	public void Definir(List<Element> l)
	{
		elements.Clear();
		elements.AddRange(l);
		survol = -1;
		Placer();
		Invalidate();
	}

	private void Placer()
	{
		zones.Clear();
		int h = base.Height;
		int hi = Zx.P(62);
		int nHaut = elements.FindAll((Element e) => !e.EnBas).Count;
		int nBas = elements.Count - nHaut;
		int y0 = Zx.P(92);
		int place = h - y0 - nBas * hi - Zx.P(28);
		int pas = ((nHaut > 0) ? Math.Max(Zx.P(48), Math.Min(hi + Zx.P(4), place / Math.Max(1, nHaut))) : hi);
		int y = y0;
		int yb = h - Zx.P(20) - nBas * hi;
		foreach (Element e in elements)
		{
			if (e.EnBas)
			{
				zones.Add(new Rectangle(0, yb, base.Width, hi));
				yb += hi;
			}
			else
			{
				zones.Add(new Rectangle(0, y, base.Width, Math.Min(hi, pas)));
				y += pas;
			}
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		Placer();
	}

	private int Index(Point p)
	{
		for (int i = 0; i < zones.Count; i++)
		{
			if (zones[i].Contains(p))
			{
				return i;
			}
		}
		return -1;
	}

	private Rectangle ZoneLogo => new Rectangle((base.Width - Zx.P(46)) / 2, Zx.P(24), Zx.P(46), Zx.P(46));

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int i = Index(e.Location);
		if (i != survol)
		{
			if (survol >= 0 && survol < zones.Count)
			{
				Invalidate(zones[survol]);
			}
			survol = i;
			if (i >= 0)
			{
				Invalidate(zones[i]);
			}
			Infobulle?.SetToolTip(this, (i >= 0) ? (elements[i].Bulle ?? elements[i].Libelle) : null);
			Survol?.Invoke((i >= 0) ? elements[i] : null, (i >= 0) ? RectangleToScreen(zones[i]) : Rectangle.Empty);
		}
		Cursor = ((i >= 0 || (ClicLogo != null && ZoneLogo.Contains(e.Location))) ? Cursors.Hand : Cursors.Default);
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = -1;
		appui = -1;
		Invalidate();
		Survol?.Invoke(null, Rectangle.Empty);
		base.OnMouseLeave(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			appui = Index(e.Location);
			if (appui >= 0)
			{
				Invalidate(zones[appui]);
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		int i = Index(e.Location);
		bool ok = e.Button == MouseButtons.Left && i >= 0 && i == appui;
		appui = -1;
		Invalidate();
		base.OnMouseUp(e);
		if (ok)
		{
			Sons.Clic();
			elements[i].Action?.Invoke();
			Invalidate();
		}
		else if (e.Button == MouseButtons.Left && ClicLogo != null && ZoneLogo.Contains(e.Location))
		{
			Sons.Clic();
			ClicLogo();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int w = base.Width;
		int h = base.Height;
		using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 0, w, Math.Max(2, h)), Color.FromArgb(12, 15, 38), Color.FromArgb(8, 10, 26), 90f))
		{
			g.FillRectangle(lg, 0, 0, w, h);
		}
		using (Pen p = new Pen(Color.FromArgb(24, 255, 255, 255)))
		{
			g.DrawLine(p, w - 1, 0, w - 1, h);
		}
		if (Logo != null)
		{
			Rectangle rl = ZoneLogo;
			using GraphicsPath pl = Bouton.Rond(rl, Zx.P(12));
			GraphicsState st = g.Save();
			g.SetClip(pl);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(Logo, rl);
			g.Restore(st);
		}
		for (int i = 0; i < elements.Count && i < zones.Count; i++)
		{
			Element el = elements[i];
			Rectangle z = zones[i];
			bool actif = el.Actif != null && el.Actif();
			bool sv = i == survol;
			Rectangle rf = new Rectangle(z.X + Zx.P(8), z.Y + Zx.P(3), z.Width - Zx.P(16), z.Height - Zx.P(6));
			if (sv || actif)
			{
				using GraphicsPath pf = Bouton.Rond(rf, Zx.P(12));
				using SolidBrush b = new SolidBrush(actif ? Color.FromArgb((i == appui) ? 70 : 52, 90, 110, 230) : Color.FromArgb((i == appui) ? 34 : 22, 255, 255, 255));
				g.FillPath(b, pf);
			}
			if (actif)
			{
				using GraphicsPath pa = Bouton.Rond(new Rectangle(0, z.Y + z.Height / 2 - Zx.P(12), Zx.P(4), Zx.P(24)), Zx.P(2));
				using SolidBrush ba = new SolidBrush(C.Orange);
				g.FillPath(ba, pa);
			}
			Color ci = ((actif || sv) ? Color.White : Color.FromArgb(170, 182, 210));
			int hg = Math.Min(Zx.P(26), z.Height - Zx.P(24));
			Txt.Dessiner(g, el.Glyphe, fG, new Rectangle(z.X, z.Y + Zx.P(8), z.Width, hg), ci, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			int nb = ((el.Badge != null) ? el.Badge() : 0);
			if (nb > 0)
			{
				string tb = ((nb > 9) ? "9+" : nb.ToString());
				Size sb = TextRenderer.MeasureText(tb, fT);
				Rectangle rbg = new Rectangle(z.X + z.Width / 2 + Zx.P(4), z.Y + Zx.P(4), Math.Max(Zx.P(18), sb.Width + Zx.P(8)), Zx.P(18));
				using (GraphicsPath pb = Bouton.Rond(rbg, Zx.P(9)))
				{
					using SolidBrush bb = new SolidBrush(C.Rouge);
					g.FillPath(bb, pb);
				}
				Txt.Dessiner(g, tb, fT, rbg, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			Txt.Dessiner(g, el.Libelle, fT, new Rectangle(z.X + Zx.P(2), z.Y + Zx.P(8) + hg + Zx.P(2), z.Width - Zx.P(4), Zx.P(18)), (actif || sv) ? Color.FromArgb(235, 240, 255) : Color.FromArgb(145, 156, 186), TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		}
	}
}
