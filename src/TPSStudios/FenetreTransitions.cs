using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Transitions entre les pages (v2.8.1) : plusieurs effets, choisis selon le sens de
/// navigation et sans répéter deux fois de suite le même.
/// </summary>
internal partial class Fenetre
{
	private enum StyleTrans
	{
		Diagonale,
		Pousse,
		Retour,
		Zoom,
		Cercle,
		Volet,
		Monte,
		Descend,
		/// <summary>Page qui se tourne depuis le bord droit (bibliothèque → mods).</summary>
		PageAvance,
		/// <summary>Page qui se tourne depuis le bord gauche (mods → bibliothèque).</summary>
		PageRetour
	}

	private StyleTrans dernierStyle = StyleTrans.Diagonale;

	private readonly Random hasardTrans = new Random();

	private Point centreTrans = Point.Empty;

	/// <summary>Une seule transition entre les menus : la page monte quand on avance, descend quand on revient.</summary>
	private StyleTrans ChoisirStyle(Vue de, Vue vers)
	{
		dernierStyle = (((int)vers >= (int)de) ? StyleTrans.Monte : StyleTrans.Descend);
		return dernierStyle;
	}

	private void PeindreTrans(Graphics g, StyleTrans style, Bitmap avant, Bitmap apres, double vp)
	{
		int width = apres.Width;
		int height = apres.Height;
		g.InterpolationMode = InterpolationMode.Low;
		g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
		switch (style)
		{
		case StyleTrans.PageAvance:
		case StyleTrans.PageRetour:
			TournerPage(g, avant, apres, vp, style == StyleTrans.PageRetour);
			break;
		case StyleTrans.Monte:
		{
			int num20 = (int)Math.Round((double)height * (1.0 - vp));
			g.DrawImageUnscaled(avant, 0, -(int)Math.Round((double)height * 0.3 * vp));
			Assombrir(g, new Rectangle(0, 0, width, height), 0.55 * vp);
			OmbreV(g, num20, width);
			g.DrawImageUnscaled(apres, 0, num20);
			break;
		}
		case StyleTrans.Descend:
		{
			int num21 = (int)Math.Round((double)height * vp);
			g.DrawImageUnscaled(apres, 0, -(int)Math.Round((double)height * 0.3 * (1.0 - vp)));
			Assombrir(g, new Rectangle(0, 0, width, height), 0.55 * (1.0 - vp));
			OmbreV(g, num21, width);
			g.DrawImageUnscaled(avant, 0, num21);
			break;
		}
		case StyleTrans.Pousse:
		{
			int num11 = (int)Math.Round((double)width * (1.0 - vp));
			g.DrawImageUnscaled(avant, -(int)Math.Round((double)width * 0.3 * vp), 0);
			Assombrir(g, new Rectangle(0, 0, width, height), 0.55 * vp);
			Ombre(g, num11, height, gauche: true);
			g.DrawImageUnscaled(apres, num11, 0);
			break;
		}
		case StyleTrans.Retour:
		{
			int num10 = (int)Math.Round((double)width * vp);
			g.DrawImageUnscaled(apres, -(int)Math.Round((double)width * 0.3 * (1.0 - vp)), 0);
			Assombrir(g, new Rectangle(0, 0, width, height), 0.55 * (1.0 - vp));
			Ombre(g, num10, height, gauche: true);
			g.DrawImageUnscaled(avant, num10, 0);
			break;
		}
		case StyleTrans.Zoom:
		{
			g.DrawImageUnscaled(avant, 0, 0);
			Assombrir(g, new Rectangle(0, 0, width, height), 0.6 * vp);
			double num6 = 0.6 + 0.4 * vp;
			int num7 = (int)((double)width * num6);
			int num8 = (int)((double)height * num6);
			Rectangle rectangle = new Rectangle((width - num7) / 2, (height - num8) / 2 + (int)((double)Zx.P(24) * (1.0 - vp)), num7, num8);
			int num9 = Math.Max(1, (int)((double)Zx.P(28) * (1.0 - vp)));
			using (GraphicsPath path2 = Bouton.Rond(rectangle, num9))
			{
				GraphicsState gstate2 = g.Save();
				g.SetClip(path2);
				g.DrawImageUnscaled(apres, 0, (int)((double)Zx.P(24) * (1.0 - vp)));
				Assombrir(g, rectangle, 0.45 * (1.0 - vp));
				g.Restore(gstate2);
			}
			break;
		}
		case StyleTrans.Cercle:
		{
			g.DrawImageUnscaled(avant, 0, 0);
			Assombrir(g, new Rectangle(0, 0, width, height), 0.45 * vp);
			Point point = ((centreTrans.IsEmpty || centreTrans.X < 0 || centreTrans.Y < 0 || centreTrans.X > width || centreTrans.Y > height) ? new Point(width / 2, height / 2) : centreTrans);
			double val = Math.Max(Math.Max(Dist(point, 0, 0), Dist(point, width, 0)), Math.Max(Dist(point, 0, height), Dist(point, width, height)));
			int num4 = (int)(val * vp) + 1;
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(point.X - num4, point.Y - num4, num4 * 2, num4 * 2);
				GraphicsState gstate = g.Save();
				g.SetClip(graphicsPath);
				g.DrawImageUnscaled(apres, 0, 0);
				g.Restore(gstate);
			}
			if (vp < 0.98)
			{
				SmoothingMode smoothingMode = g.SmoothingMode;
				g.SmoothingMode = SmoothingMode.AntiAlias;
				using (Pen pen = new Pen(Color.FromArgb((int)(220.0 * (1.0 - vp)), C.Orange), Zx.P(3f)))
				{
					g.DrawEllipse(pen, point.X - num4, point.Y - num4, num4 * 2, num4 * 2);
				}
				g.SmoothingMode = smoothingMode;
			}
			break;
		}
		case StyleTrans.Volet:
		{
			int num = (int)Math.Round((double)height * (1.0 - vp));
			g.DrawImageUnscaled(avant, 0, -(int)Math.Round((double)Zx.P(60) * vp));
			Assombrir(g, new Rectangle(0, 0, width, height), 0.5 * vp);
			GraphicsState gstate3 = g.Save();
			g.SetClip(new Rectangle(0, num, width, height - num));
			g.DrawImageUnscaled(apres, 0, (int)Math.Round((double)Zx.P(40) * (1.0 - vp)));
			g.Restore(gstate3);
			if (vp > 0.002 && vp < 0.998)
			{
				int num2 = Zx.P(26);
				using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, num - num2, width, num2 + 1), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(120, 0, 0, 0), 90f))
				{
					g.FillRectangle(brush, 0, num - num2, width, num2);
				}
				using SolidBrush brush2 = new SolidBrush(C.Orange);
				g.FillRectangle(brush2, 0, num - Zx.P(2), width, Zx.P(3));
			}
			break;
		}
		default:
		{
			g.DrawImageUnscaled(avant, 0, 0);
			using (SolidBrush brush3 = new SolidBrush(Color.FromArgb((int)(110.0 * vp), 2, 3, 10)))
			{
				g.FillRectangle(brush3, 0, 0, width, height);
			}
			using (GraphicsPath path = Zone(width, height, vp, gauche: true))
			{
				GraphicsState gstate4 = g.Save();
				g.SetClip(path, CombineMode.Replace);
				g.DrawImageUnscaled(apres, (int)Math.Round(36.0 * (1.0 - vp)), 0);
				if (vp < 1.0)
				{
					using SolidBrush brush4 = new SolidBrush(C.Fond);
					g.FillRectangle(brush4, 0, 0, (int)Math.Round(36.0 * (1.0 - vp)), height);
				}
				g.Restore(gstate4);
			}
			Bord(g, width, height, vp);
			break;
		}
		}
	}

	/// <summary>
	/// Page qui se tourne, pli vertical : la page actuelle se replie sur elle-même (on voit son dos, plus sombre
	/// près du pli), la nouvelle page apparaît dessous avec une ombre. Rien que des copies d'images droites ou
	/// retournées : léger à dessiner, donc fluide même en grand écran.
	/// </summary>
	private static void TournerPage(Graphics g, Bitmap avant, Bitmap apres, double vp, bool depuisGauche)
	{
		int W = apres.Width;
		int H = apres.Height;
		double e = Math.Max(0.0, Math.Min(1.0, vp));
		// démarrage et arrivée en douceur
		e = e * e * (3.0 - 2.0 * e);
		g.DrawImageUnscaled(apres, 0, 0);
		if (e >= 0.999)
		{
			return;
		}
		InterpolationMode im = g.InterpolationMode;
		g.InterpolationMode = InterpolationMode.NearestNeighbor;
		int xf = (int)Math.Round(depuisGauche ? ((double)W * e) : ((double)W * (1.0 - e)));
		// ombre portée sur la nouvelle page, côté où la page est déjà partie
		int lo = Zx.P(16) + (int)((double)Zx.P(40) * Math.Sin(Math.PI * e));
		Rectangle ro = (depuisGauche ? new Rectangle(xf - lo, 0, lo, H) : new Rectangle(xf, 0, lo, H));
		if (ro.Width > 0)
		{
			using LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(ro.X - 1, 0, ro.Width + 2, H), Color.FromArgb(depuisGauche ? 0 : 150, 0, 0, 8), Color.FromArgb(depuisGauche ? 150 : 0, 0, 0, 8), 0f);
			g.FillRectangle(lg, ro);
		}
		// partie encore à plat
		Rectangle reste = (depuisGauche ? new Rectangle(xf, 0, W - xf, H) : new Rectangle(0, 0, xf, H));
		if (reste.Width > 0)
		{
			g.DrawImage(avant, reste, reste, GraphicsUnit.Pixel);
		}
		// rabat : la partie tournée, retournée en miroir autour du pli
		int L = (depuisGauche ? Math.Min(xf, W - xf) : Math.Min(W - xf, xf));
		if (L > 0)
		{
			Rectangle rab = (depuisGauche ? new Rectangle(xf, 0, L, H) : new Rectangle(xf - L, 0, L, H));
			// ombre douce du rabat sur la page restante
			int lo2 = Zx.P(14);
			Rectangle rs = (depuisGauche ? new Rectangle(rab.Right, 0, lo2, H) : new Rectangle(rab.X - lo2, 0, lo2, H));
			using (LinearGradientBrush ls = new LinearGradientBrush(new Rectangle(rs.X - 1, 0, rs.Width + 2, H), Color.FromArgb(depuisGauche ? 110 : 0, 0, 0, 8), Color.FromArgb(depuisGauche ? 0 : 110, 0, 0, 8), 0f))
			{
				g.FillRectangle(ls, rs);
			}
			Rectangle srcM = (depuisGauche ? new Rectangle(xf - L, 0, L, H) : new Rectangle(xf, 0, L, H));
			Point[] dest;
			if (depuisGauche)
			{
				dest = new Point[3]
				{
					new Point(xf + L, 0),
					new Point(xf, 0),
					new Point(xf + L, H)
				};
			}
			else
			{
				dest = new Point[3]
				{
					new Point(xf, 0),
					new Point(xf - L, 0),
					new Point(xf, H)
				};
			}
			g.DrawImage(avant, dest, srcM, GraphicsUnit.Pixel);
			// dos de la page : contenu estompé, plus sombre près du pli
			using (SolidBrush dos = new SolidBrush(Color.FromArgb(165, C.Carte)))
			{
				g.FillRectangle(dos, rab);
			}
			using (LinearGradientBrush lp = new LinearGradientBrush(new Rectangle(rab.X - 1, 0, rab.Width + 2, H), Color.FromArgb(depuisGauche ? 120 : 10, 0, 0, 12), Color.FromArgb(depuisGauche ? 10 : 120, 0, 0, 12), 0f))
			{
				g.FillRectangle(lp, rab);
			}
			using Pen bord = new Pen(Color.FromArgb(110, 200, 210, 255), 1f);
			int xb = (depuisGauche ? (rab.Right - 1) : rab.X);
			g.DrawLine(bord, xb, 0, xb, H);
		}
		g.InterpolationMode = im;
	}

	private static double Dist(Point p, int x, int y)
	{
		double num = p.X - x;
		double num2 = p.Y - y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	private static void Assombrir(Graphics g, Rectangle r, double a)
	{
		if (!(a <= 0.004))
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255.0 * Math.Min(1.0, a)), 2, 3, 10));
			g.FillRectangle(brush, r);
		}
	}

	/// <summary>Ombre au-dessus du bord haut de la page qui glisse.</summary>
	private static void OmbreV(Graphics g, int y, int w)
	{
		int num = Zx.P(30);
		if (y > 0)
		{
			using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, y - num, w, num + 1), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(140, 0, 0, 0), 90f);
			g.FillRectangle(brush, 0, y - num, w, num);
		}
	}

	private static void Ombre(Graphics g, int x, int h, bool gauche)
	{
		int num = Zx.P(30);
		if (x > 0)
		{
			using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(x - num, 0, num + 1, h), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(140, 0, 0, 0), 0f);
			g.FillRectangle(brush, x - num, 0, num, h);
		}
	}

	/// <summary>Masque les bandes-annonces pendant les captures et l'animation d'une transition.</summary>
	private void SuspendreVideos(bool s)
	{
		videoB?.Suspendre(s);
		videoP?.Suspendre(s);
	}
}
