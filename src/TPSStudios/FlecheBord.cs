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

	public static readonly Font PoliceLibelle = new Font("Segoe UI Semibold", Zx.P(12f));

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
		// tranche du livre : fines lignes parallèles au bord, comme les pages empilées
		for (int n = 1; n <= 5; n++)
		{
			float k = (float)(n * Zx.P(5)) * (Droite ? 1f : (-1f));
			using Pen pl = new Pen(Color.FromArgb(Math.Max(0, (int)((50f + 60f * s) * (1f - (float)n / 6f))), 190, 200, 255), 1f);
			g.DrawLine(pl, d0.X + k * 0.35f, d0.Y, d1.X + k, d1.Y);
		}
		// le texte seul, écrit le long du bord (comme sur la tranche d'un livre), à la hauteur où la forme est assez large
		if (!string.IsNullOrEmpty(Libelle))
		{
			string t = Lang.T(Libelle);
			System.Drawing.Text.TextRenderingHint th = g.TextRenderingHint;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			SizeF st = g.MeasureString(t, PoliceLibelle);
			int cy = h / 2;
			float besoin = st.Height + (float)Zx.P(22);
			while (cy < h * 86 / 100 && LargeurA(cy) < besoin)
			{
				cy += Zx.P(8);
			}
			float bandeX = (Droite ? ((float)w - (float)Zx.P(16) - st.Height / 2f) : ((float)Zx.P(16) + st.Height / 2f));
			float decal = (float)Zx.P(4) * s * (Droite ? 1f : (-1f));
			GraphicsState gs = g.Save();
			g.TranslateTransform(bandeX + decal, cy);
			g.RotateTransform(-90f);
			using (SolidBrush ombre = new SolidBrush(Color.FromArgb(120, 0, 0, 10)))
			{
				g.DrawString(t, PoliceLibelle, ombre, 0f - st.Width / 2f + 1f, 0f - st.Height / 2f + 1f);
			}
			using (SolidBrush bt = new SolidBrush(Color.FromArgb((int)(205f + 50f * s), 255, 255, 255)))
			{
				g.DrawString(t, PoliceLibelle, bt, 0f - st.Width / 2f, 0f - st.Height / 2f);
			}
			g.Restore(gs);
			g.TextRenderingHint = th;
		}
	}
}
