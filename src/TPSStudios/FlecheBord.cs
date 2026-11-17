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
	public int LargeurFleche = Zx.P(24);

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

	/// <summary>Largeur de la forme en haut (la diagonale s'élargit jusqu'en bas).</summary>
	public int LargeurHaut = Zx.P(34);

	/// <summary>Forme du bouton : un long bord de page en diagonale, étroit en haut et large en bas.</summary>
	private PointF[] Forme()
	{
		int w = base.Width;
		int h = base.Height;
		float lh = Math.Min(LargeurHaut, w);
		if (Droite)
		{
			return new PointF[4]
			{
				new PointF(w - lh, 0f),
				new PointF(w, 0f),
				new PointF(w, h),
				new PointF(0f, h)
			};
		}
		return new PointF[4]
		{
			new PointF(0f, 0f),
			new PointF(lh, 0f),
			new PointF(w, h),
			new PointF(0f, h)
		};
	}

	/// <summary>Largeur de la forme à la hauteur y.</summary>
	private float LargeurA(float y)
	{
		float lh = Math.Min(LargeurHaut, base.Width);
		return lh + ((float)base.Width - lh) * (y / Math.Max(1f, base.Height));
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (base.Width > 0 && base.Height > 0)
		{
			using GraphicsPath p = new GraphicsPath();
			p.AddPolygon(Forme());
			base.Region = new Region(p);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float s = Math.Max(0f, Math.Min(1f, lueur.V));
		int w = base.Width;
		int h = base.Height;
		if (w < 2 || h < 2)
		{
			return;
		}
		PointF[] f = Forme();
		using (GraphicsPath pf = new GraphicsPath())
		{
			pf.AddPolygon(f);
			// dos de page : clair le long de la diagonale, plus sombre vers le bord de la fenêtre
			Rectangle rg = new Rectangle(0, 0, w, h);
			using (LinearGradientBrush lg = new LinearGradientBrush(rg, Color.FromArgb((int)(34f + 40f * s), 150, 168, 255), Color.FromArgb((int)(8f + 22f * s), 30, 36, 90), Droite ? 0f : 180f))
			{
				g.FillPath(lg, pf);
			}
		}
		// liseré lumineux le long de la diagonale (le bord de la page)
		PointF d0 = (Droite ? f[0] : f[1]);
		PointF d1 = (Droite ? f[3] : f[2]);
		using (Pen bord = new Pen(Color.FromArgb((int)(70f + 140f * s), 175, 190, 255), Zx.P(1.6f)))
		{
			g.DrawLine(bord, d0, d1);
		}
		// ombre douce juste à l'intérieur du bord, pour l'effet de page soulevée
		using (Pen ombre = new Pen(Color.FromArgb((int)(40f + 40f * s), 0, 0, 10), Zx.P(6f)))
		{
			float k = (Droite ? Zx.P(4) : (-Zx.P(4)));
			g.DrawLine(ombre, d0.X + k, d0.Y, d1.X + k, d1.Y);
		}
		// flèche et texte : au milieu, ou plus bas si la diagonale y est trop étroite pour le texte
		int lf = LargeurFleche;
		int besoin = lf + Zx.P(10) + Zx.P(22) + (string.IsNullOrEmpty(Libelle) ? 0 : Lang.MeasureText(Libelle, PoliceLibelle).Width);
		int cy = h / 2;
		while (cy < h * 86 / 100 && LargeurA(cy) < (float)besoin)
		{
			cy += Zx.P(8);
		}
		float wm = LargeurA(cy);
		int ph = Zx.P(96);
		int marge = Zx.P(10);
		int decal = (int)((float)Zx.P(5) * s) * (Droite ? 1 : -1);
		Rectangle r = new Rectangle((Droite ? (w - lf - marge) : marge) + decal, cy - ph / 2, lf - 1, ph - 1);
		if (!string.IsNullOrEmpty(Libelle))
		{
			Size st = Lang.MeasureText(Libelle, PoliceLibelle);
			int place = (int)wm - lf - marge - Zx.P(18);
			if (place >= st.Width)
			{
				Rectangle rt = (Droite ? new Rectangle(r.X - Zx.P(10) - st.Width, cy - st.Height / 2, st.Width + 2, st.Height + 2) : new Rectangle(r.Right + Zx.P(10), cy - st.Height / 2, st.Width + 2, st.Height + 2));
				Lang.DrawText(g, Libelle, PoliceLibelle, rt, Color.FromArgb((int)(210f + 45f * s), 255, 255, 255), TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
			}
		}
		// pas de pastille : la diagonale entière est le bouton, seule la flèche est dessinée
		int cx = r.X + r.Width / 2;
		int k3 = Zx.P(7);
		int k4 = Zx.P(13);
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
		using Pen pen2 = new Pen(Color.FromArgb((int)(200f + 55f * s), 255, 255, 255), Zx.P(3f));
		pen2.StartCap = LineCap.Round;
		pen2.EndCap = LineCap.Round;
		pen2.LineJoin = LineJoin.Round;
		g.DrawLines(pen2, points);
	}
}
