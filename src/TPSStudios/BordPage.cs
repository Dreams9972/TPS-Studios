using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Bord de page cliquable (Bibliothèque ⇄ Mods), dessiné par la page elle-même à position fixe :
/// il ne bouge pas quand on fait défiler. Une longue diagonale sur toute la hauteur, façon tranche de livre :
/// la page du dessous dépasse, l'épaisseur des pages est visible, le texte est écrit le long du bord.
/// Au survol la page se soulève un peu.
/// </summary>
internal class BordPage
{
	/// <summary>Zone occupée, en coordonnées de la zone visible de la page (pas du contenu qui défile).</summary>
	public Rectangle Zone = Rectangle.Empty;

	public bool Droite = true;

	public string Libelle = "";

	/// <summary>Largeur de la forme en haut (elle s'élargit jusqu'en bas).</summary>
	public int LargeurHaut = Zx.P(30);

	public static readonly Font Police = new Font("Segoe UI Semibold", Zx.P(12.5f));

	private readonly Control hote;

	private bool survol;

	private Ressort lueur;

	private Timer tAnim;

	public BordPage(Control hote)
	{
		this.hote = hote;
	}

	public bool Survol => survol;

	private PointF[] Forme(float leve)
	{
		Rectangle z = Zone;
		// bande bien droite : même largeur en haut et en bas ; au survol la page « sort » un peu
		float lb = Math.Min((float)z.Width, (float)z.Width - (float)Zx.P(10) + leve);
		float lh = lb;
		if (Droite)
		{
			return new PointF[4]
			{
				new PointF((float)z.Right - lh, z.Top),
				new PointF(z.Right, z.Top),
				new PointF(z.Right, z.Bottom),
				new PointF((float)z.Right - lb, z.Bottom)
			};
		}
		return new PointF[4]
		{
			new PointF(z.Left, z.Top),
			new PointF((float)z.Left + lh, z.Top),
			new PointF((float)z.Left + lb, z.Bottom),
			new PointF(z.Left, z.Bottom)
		};
	}

	/// <summary>Vrai si la zone à repeindre ne touche que la bande (survol) : la page n'a pas besoin de redessiner son contenu.</summary>
	public bool SeulementBord(RectangleF clip)
	{
		if (Zone.IsEmpty)
		{
			return false;
		}
		int marge = Zx.P(41);
		if (!Droite)
		{
			return clip.Right <= (float)(Zone.Right + marge + 1);
		}
		return clip.Left >= (float)(Zone.Left - marge - 1);
	}

	public bool Contient(Point p)
	{
		if (Zone.IsEmpty || !Zone.Contains(p))
		{
			return false;
		}
		using GraphicsPath gp = new GraphicsPath();
		gp.AddPolygon(Forme((float)Zx.P(10)));
		return gp.IsVisible(p);
	}

	public void ChangerSurvol(bool s)
	{
		if (s == survol)
		{
			return;
		}
		survol = s;
		if (!Fenetre.Fluide)
		{
			lueur.V = (s ? 1f : 0f);
			Invalider();
			return;
		}
		if (tAnim == null)
		{
			tAnim = new Timer();
			tAnim.Interval = 15;
			tAnim.Tick += delegate
			{
				bool encore = lueur.Pas(survol ? 1f : 0f, 0.2f, 0.7f);
				Invalider();
				if (!encore)
				{
					tAnim.Stop();
				}
			};
		}
		tAnim.Start();
	}

	public void Invalider()
	{
		if (hote != null && !hote.IsDisposed && !Zone.IsEmpty)
		{
			hote.Invalidate(Rectangle.Inflate(Zone, Zx.P(40), 0));
		}
	}

	public void Dessiner(Graphics g)
	{
		if (Zone.Width < 4 || Zone.Height < 4)
		{
			return;
		}
		float s = Math.Max(0f, Math.Min(1f, lueur.V));
		float leve = (float)Zx.P(14) * s;
		SmoothingMode sm = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		PointF[] f = Forme(leve);
		PointF d0 = (Droite ? f[0] : f[1]);
		PointF d1 = (Droite ? f[3] : f[2]);
		float sens = (Droite ? (-1f) : 1f);
		// ombre portée sur la page, juste avant le bord
		int lo = Zx.P(22) + (int)((float)Zx.P(10) * s);
		for (int i = 0; i < 6; i++)
		{
			float k = sens * ((float)lo * (float)(i + 1) / 6f);
			using Pen po = new Pen(Color.FromArgb((int)((26f + 14f * s) * (1f - (float)i / 6f)), 0, 0, 8), (float)lo / 6f + 1f);
			g.DrawLine(po, d0.X + k, d0.Y, d1.X + k, d1.Y);
		}
		using (GraphicsPath pf = new GraphicsPath())
		{
			pf.AddPolygon(f);
			// la page : claire près du bord (lumière), plus sombre vers le bord de la fenêtre
			RectangleF rb = pf.GetBounds();
			rb.Inflate(1f, 1f);
			Color clair = Color.FromArgb(255, Mix(C.Carte, Color.FromArgb(130, 145, 230), 0.3f + 0.15f * s));
			Color sombre = Color.FromArgb(255, Mix(C.Carte, Color.FromArgb(70, 80, 160), 0.12f));
			using (LinearGradientBrush lg = new LinearGradientBrush(rb, Droite ? clair : sombre, Droite ? sombre : clair, 0f))
			{
				g.FillPath(lg, pf);
			}
			// épaisseur du livre : fines tranches de pages le long du bord
			GraphicsState st = g.Save();
			g.SetClip(pf);
			for (int n = 0; n < 7; n++)
			{
				float k2 = -sens * (float)(Zx.P(3) + n * Zx.P(3));
				Color c = ((n % 2 == 0) ? Color.FromArgb((int)(70f - 8f * (float)n), 215, 222, 255) : Color.FromArgb((int)(55f - 6f * (float)n), 10, 12, 30));
				using Pen pt = new Pen(c, 1f);
				g.DrawLine(pt, d0.X + k2, d0.Y, d1.X + k2, d1.Y);
			}
			g.Restore(st);
		}
		// arête de la page, éclairée
		using (Pen arete = new Pen(Color.FromArgb((int)(120f + 120f * s), 190, 202, 255), Zx.P(1.5f)))
		{
			g.DrawLine(arete, d0, d1);
		}
		// texte écrit le long de la tranche
		if (!string.IsNullOrEmpty(Libelle))
		{
			string t = Lang.T(Libelle);
			System.Drawing.Text.TextRenderingHint th = g.TextRenderingHint;
			g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			SizeF st2 = g.MeasureString(t, Police);
			float cy = (float)Zone.Top + (float)Zone.Height * 0.5f;
			float largeur = LargeurA(cy, leve);
			float x = (Droite ? ((float)Zone.Right - largeur / 2f) : ((float)Zone.Left + largeur / 2f));
			GraphicsState gs = g.Save();
			g.TranslateTransform(x, cy);
			g.RotateTransform(-90f);
			using (SolidBrush ombre = new SolidBrush(Color.FromArgb(140, 0, 0, 10)))
			{
				g.DrawString(t, Police, ombre, 0f - st2.Width / 2f + 1f, 0f - st2.Height / 2f + 1.5f);
			}
			using (SolidBrush bt = new SolidBrush(Color.FromArgb((int)(215f + 40f * s), 255, 255, 255)))
			{
				g.DrawString(t, Police, bt, 0f - st2.Width / 2f, 0f - st2.Height / 2f);
			}
			g.Restore(gs);
			g.TextRenderingHint = th;
		}
		g.SmoothingMode = sm;
	}

	private float LargeurA(float y, float leve)
	{
		return (float)Zone.Width - (float)Zx.P(10) + leve;
	}

	private static Color Mix(Color a, Color b, float t)
	{
		return Color.FromArgb((int)((float)a.R + (float)(b.R - a.R) * t), (int)((float)a.G + (float)(b.G - a.G) * t), (int)((float)a.B + (float)(b.B - a.B) * t));
	}
}
