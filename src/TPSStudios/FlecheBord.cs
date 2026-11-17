using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Bande cliquable collée au bord (Bibliothèque ⇄ tous les mods) : toute la hauteur de la bande sert de bouton.
/// Au survol elle s'éclaire en dégradé ; un coin de page corné rappelle qu'on « tourne la page ».
/// </summary>
internal class FlecheBord : Control
{
	public bool Droite = true;

	/// <summary>Texte affiché à côté de la flèche (vide : flèche seule).</summary>
	public string Libelle = "";

	/// <summary>Largeur de la pastille de la flèche.</summary>
	public int LargeurFleche = Zx.P(30);

	public static readonly Font PoliceLibelle = new Font("Segoe UI Semibold", Zx.P(11f));

	private bool survol;

	private Ressort lueur;

	private Timer tAnim;

	public FlecheBord()
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.StandardDoubleClick, value: false);
		BackColor = Color.Transparent;
		Cursor = Cursors.Hand;
	}

	private void Animer()
	{
		if (!Fenetre.Fluide)
		{
			lueur.V = (survol ? 1f : 0f);
			Invalidate();
			return;
		}
		if (tAnim == null)
		{
			tAnim = new Timer();
			tAnim.Interval = 15;
			tAnim.Tick += delegate
			{
				bool encore = lueur.Pas(survol ? 1f : 0f, 0.22f, 0.72f);
				Invalidate();
				if (!encore)
				{
					tAnim.Stop();
				}
			};
		}
		tAnim.Start();
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		survol = true;
		Animer();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		survol = false;
		Animer();
		base.OnMouseLeave(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && tAnim != null)
		{
			tAnim.Dispose();
			tAnim = null;
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float s = Math.Max(0f, Math.Min(1f, lueur.V));
		int w = base.Width;
		int h = base.Height;
		// dégradé vers le bord : la bande entière « répond » au survol
		if (w > 2 && h > 2)
		{
			int a1 = (int)(10f + 34f * s);
			Rectangle rg = new Rectangle(0, 0, w, h);
			using (LinearGradientBrush lg = new LinearGradientBrush(rg, Color.FromArgb(0, 255, 255, 255), Color.FromArgb(a1, 150, 170, 255), Droite ? 0f : 180f))
			{
				g.FillRectangle(lg, rg);
			}
			// liseré lumineux le long du bord de la fenêtre
			int xl = (Droite ? (w - Zx.P(2)) : 0);
			using (LinearGradientBrush lb = new LinearGradientBrush(new Rectangle(xl, 0, Zx.P(2), h), Color.FromArgb(0, 170, 185, 255), Color.FromArgb((int)(40f + 120f * s), 170, 185, 255), 90f))
			{
				ColorBlend cb = new ColorBlend(3);
				cb.Colors = new Color[3]
				{
					Color.FromArgb(0, 170, 185, 255),
					Color.FromArgb((int)(40f + 120f * s), 170, 185, 255),
					Color.FromArgb(0, 170, 185, 255)
				};
				cb.Positions = new float[3] { 0f, 0.5f, 1f };
				lb.InterpolationColors = cb;
				g.FillRectangle(lb, xl, 0, Zx.P(2), h);
			}
			Coin(g, w, h, s);
		}
		int lf = Math.Min(LargeurFleche, w);
		int decal = (int)((float)Zx.P(4) * s) * (Droite ? 1 : -1);
		int ph = Math.Min(h, Zx.P(110));
		int marge = Zx.P(8);
		Rectangle r = new Rectangle((Droite ? (w - lf - marge) : marge) + decal, (h - ph) / 2, lf - 1, ph - 1);
		if (!string.IsNullOrEmpty(Libelle))
		{
			Size st = Lang.MeasureText(Libelle, PoliceLibelle);
			Rectangle rt = (Droite ? new Rectangle(Math.Max(0, r.X - Zx.P(10) - st.Width), 0, st.Width + 2, h) : new Rectangle(r.Right + Zx.P(10), 0, st.Width + 2, h));
			if (Droite ? (rt.X > 0) : (rt.Right < w))
			{
				Lang.DrawText(g, Libelle, PoliceLibelle, rt, Color.FromArgb((int)(205f + 50f * s), 255, 255, 255), TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
			}
		}
		using (GraphicsPath path = Bouton.Rond(r, Math.Min(lf / 2, Zx.P(14))))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(38f + 52f * s), 255, 255, 255)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb((int)(60f + 60f * s), 255, 255, 255));
			g.DrawPath(pen, path);
		}
		int cx = r.X + r.Width / 2;
		int cy = r.Y + r.Height / 2;
		int k3 = Zx.P(5);
		int k4 = Zx.P(9);
		Point[] points = (Droite ? new Point[3]
		{
			new Point(cx - k3 / 2 - 1, cy - k4),
			new Point(cx + k3, cy),
			new Point(cx - k3 / 2 - 1, cy + k4)
		} : new Point[3]
		{
			new Point(cx + k3 / 2 + 1, cy - k4),
			new Point(cx - k3, cy),
			new Point(cx + k3 / 2 + 1, cy + k4)
		});
		using Pen pen2 = new Pen(Color.FromArgb((int)(200f + 55f * s), 255, 255, 255), Zx.P(2.4f));
		pen2.StartCap = LineCap.Round;
		pen2.EndCap = LineCap.Round;
		pen2.LineJoin = LineJoin.Round;
		g.DrawLines(pen2, points);
	}

	/// <summary>Coin de page corné en bas de la bande : il se soulève au survol.</summary>
	private void Coin(Graphics g, int w, int h, float s)
	{
		int c = (int)((float)Zx.P(18) + (float)Zx.P(16) * s);
		if (c > w - Zx.P(4) || c > h / 3)
		{
			return;
		}
		int bx = (Droite ? w : 0);
		int sx = (Droite ? (-1) : 1);
		// le dessous de la page (ombre) puis le rabat clair
		Point[] trou = new Point[3]
		{
			new Point(bx, h - c),
			new Point(bx, h),
			new Point(bx + sx * c, h)
		};
		using (SolidBrush b0 = new SolidBrush(Color.FromArgb((int)(70f + 50f * s), 0, 0, 10)))
		{
			g.FillPolygon(b0, trou);
		}
		Point[] rabat = new Point[3]
		{
			new Point(bx, h - c),
			new Point(bx + sx * c, h - c),
			new Point(bx + sx * c, h)
		};
		using (LinearGradientBrush b1 = new LinearGradientBrush(new Rectangle(Math.Min(bx, bx + sx * c), h - c, c, c), Color.FromArgb((int)(90f + 100f * s), 200, 210, 255), Color.FromArgb((int)(40f + 60f * s), 90, 100, 170), 45f))
		{
			g.FillPolygon(b1, rabat);
		}
		using Pen p = new Pen(Color.FromArgb((int)(60f + 100f * s), 220, 228, 255), 1f);
		g.DrawLine(p, bx, h - c, bx + sx * c, h);
	}
}
