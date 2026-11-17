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
	/// Page qui se tourne : la page actuelle se replie le long d'une ligne de pli légèrement inclinée
	/// (le coin du bas part en premier), on voit le dos de la page replié et son ombre, la nouvelle page apparaît dessous.
	/// </summary>
	private static void TournerPage(Graphics g, Bitmap avant, Bitmap apres, double vp, bool depuisGauche)
	{
		int W = apres.Width;
		int H = apres.Height;
		double e = Math.Max(0.0, Math.Min(1.0, vp));
		g.DrawImageUnscaled(apres, 0, 0);
		if (e >= 0.999)
		{
			return;
		}
		double incl = (double)H * 0.28 * Math.Sin(Math.PI * e);
		double c = (depuisGauche ? ((double)W * e) : ((double)W * (1.0 - e)));
		// pli : en bas il est plus avancé qu'en haut (le coin du bas se soulève d'abord)
		float xb = (float)(depuisGauche ? (c + incl / 2.0) : (c - incl / 2.0));
		float xt = (float)(depuisGauche ? (c - incl / 2.0) : (c + incl / 2.0));
		PointF[] reste = (depuisGauche ? new PointF[4]
		{
			new PointF(xt, 0f),
			new PointF(W, 0f),
			new PointF(W, H),
			new PointF(xb, H)
		} : new PointF[4]
		{
			new PointF(0f, 0f),
			new PointF(xt, 0f),
			new PointF(xb, H),
			new PointF(0f, H)
		});
		PointF[] tournee = (depuisGauche ? new PointF[4]
		{
			new PointF(0f, 0f),
			new PointF(xt, 0f),
			new PointF(xb, H),
			new PointF(0f, H)
		} : new PointF[4]
		{
			new PointF(xt, 0f),
			new PointF(W, 0f),
			new PointF(W, H),
			new PointF(xb, H)
		});
		// ombre portée sur la nouvelle page, du côté où la page est déjà partie
		int lo = (int)((float)Zx.P(46) * (float)Math.Sin(Math.PI * Math.Min(1.0, e * 1.2)) + (float)Zx.P(6));
		using (GraphicsPath po = new GraphicsPath())
		{
			float s = (depuisGauche ? (-1f) : 1f);
			po.AddPolygon(new PointF[4]
			{
				new PointF(xt, 0f),
				new PointF(xt + s * (float)lo, 0f),
				new PointF(xb + s * (float)lo, H),
				new PointF(xb, H)
			});
			float x0 = Math.Min(Math.Min(xt, xb), Math.Min(xt, xb) + s * (float)lo) - 1f;
			float x1 = Math.Max(Math.Max(xt, xb), Math.Max(xt, xb) + s * (float)lo) + 1f;
			using LinearGradientBrush lg = new LinearGradientBrush(new RectangleF(x0, 0f, Math.Max(2f, x1 - x0), H), Color.FromArgb(depuisGauche ? 0 : 150, 0, 0, 8), Color.FromArgb(depuisGauche ? 150 : 0, 0, 0, 8), 0f);
			g.FillPath(lg, po);
		}
		// la partie encore à plat de l'ancienne page
		GraphicsState st = g.Save();
		using (GraphicsPath pr = new GraphicsPath())
		{
			pr.AddPolygon(reste);
			g.SetClip(pr);
			g.DrawImageUnscaled(avant, 0, 0);
		}
		g.Restore(st);
		// réflexion autour de la ligne de pli : le dos de la partie tournée
		double dx = xt - xb;
		double dy = -H;
		double n = Math.Sqrt(dx * dx + dy * dy);
		dx /= n;
		dy /= n;
		float a = (float)(dx * dx - dy * dy);
		float b = (float)(2.0 * dx * dy);
		float px = xb;
		float py = H;
		Func<PointF, PointF> refl = (PointF q) => new PointF(a * (q.X - px) + b * (q.Y - py) + px, b * (q.X - px) - a * (q.Y - py) + py);
		PointF[] rabat = Array.ConvertAll(tournee, (PointF q) => refl(q));
		GraphicsState st2 = g.Save();
		using (GraphicsPath pf = new GraphicsPath())
		{
			pf.AddPolygon(rabat);
			using Region rg = new Region(pf);
			rg.Intersect(new Rectangle(0, 0, W, H));
			// ombre douce du rabat sur la page restante
			using (Matrix mo = new Matrix())
			{
				mo.Translate(depuisGauche ? Zx.P(6) : (-Zx.P(6)), Zx.P(4));
				using GraphicsPath pf2 = (GraphicsPath)pf.Clone();
				pf2.Transform(mo);
				using SolidBrush so = new SolidBrush(Color.FromArgb(70, 0, 0, 8));
				GraphicsState st3 = g.Save();
				using (GraphicsPath pr2 = new GraphicsPath())
				{
					pr2.AddPolygon(reste);
					g.SetClip(pr2);
				}
				g.FillPath(so, pf2);
				g.Restore(st3);
			}
			g.SetClip(rg, CombineMode.Replace);
			using (Matrix m = new Matrix(a, b, b, 0f - a, px - (a * px + b * py), py - (b * px - a * py)))
			{
				GraphicsState st4 = g.Save();
				g.MultiplyTransform(m);
				g.DrawImageUnscaled(avant, 0, 0);
				g.Restore(st4);
			}
			// dos de la page : contenu estompé, plus sombre près du pli
			using (SolidBrush dos = new SolidBrush(Color.FromArgb(170, C.Carte)))
			{
				g.FillRectangle(dos, 0, 0, W, H);
			}
			float fx0 = Math.Min(xt, xb);
			float fx1 = Math.Max(xt, xb);
			float largeur = Math.Max(Zx.P(60), (float)((double)W * 0.35));
			RectangleF rb = (depuisGauche ? new RectangleF(fx0, 0f, fx1 - fx0 + largeur, H) : new RectangleF(fx0 - largeur, 0f, fx1 - fx0 + largeur, H));
			using (LinearGradientBrush ls = new LinearGradientBrush(rb, Color.FromArgb(depuisGauche ? 110 : 0, 0, 0, 12), Color.FromArgb(depuisGauche ? 0 : 110, 0, 0, 12), 0f))
			{
				g.FillRectangle(ls, rb);
			}
			using (Pen bord = new Pen(Color.FromArgb(90, 200, 210, 255), 1f))
			{
				g.DrawPolygon(bord, rabat);
			}
		}
		g.Restore(st2);
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
