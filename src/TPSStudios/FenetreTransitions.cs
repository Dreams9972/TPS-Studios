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
		Volet
	}

	private StyleTrans dernierStyle = StyleTrans.Diagonale;

	private readonly Random hasardTrans = new Random();

	private Point centreTrans = Point.Empty;

	/// <summary>Plus on « entre » (Accueil → Jeux → Bibliothèque → page), plus le rang est haut.</summary>
	private StyleTrans ChoisirStyle(Vue de, Vue vers)
	{
		StyleTrans[] array = (((int)vers >= (int)de) ? new StyleTrans[4]
		{
			StyleTrans.Pousse,
			StyleTrans.Zoom,
			StyleTrans.Cercle,
			StyleTrans.Diagonale
		} : new StyleTrans[3]
		{
			StyleTrans.Retour,
			StyleTrans.Volet,
			StyleTrans.Zoom
		});
		StyleTrans styleTrans;
		do
		{
			styleTrans = array[hasardTrans.Next(array.Length)];
		}
		while (styleTrans == dernierStyle && array.Length > 1);
		dernierStyle = styleTrans;
		try
		{
			centreTrans = zone.PointToClient(Cursor.Position);
		}
		catch
		{
			centreTrans = Point.Empty;
		}
		return styleTrans;
	}

	private void PeindreTrans(Graphics g, StyleTrans style, Bitmap avant, Bitmap apres, double vp)
	{
		int width = apres.Width;
		int height = apres.Height;
		g.InterpolationMode = InterpolationMode.Low;
		g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
		switch (style)
		{
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
			double num6 = 0.55 + 0.45 * vp;
			int num7 = (int)((double)width * num6);
			int num8 = (int)((double)height * num6);
			Rectangle rectangle = new Rectangle((width - num7) / 2, (height - num8) / 2, num7, num8);
			int num9 = Math.Max(1, (int)((double)Zx.P(28) * (1.0 - vp)));
			using (GraphicsPath path2 = Bouton.Rond(rectangle, num9))
			{
				GraphicsState gstate2 = g.Save();
				g.SetClip(path2);
				g.DrawImage(apres, rectangle);
				Assombrir(g, rectangle, 0.5 * (1.0 - vp));
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

	private static void Ombre(Graphics g, int x, int h, bool gauche)
	{
		int num = Zx.P(30);
		if (x > 0)
		{
			using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(x - num, 0, num + 1, h), Color.FromArgb(0, 0, 0, 0), Color.FromArgb(140, 0, 0, 0), 0f);
			g.FillRectangle(brush, x - num, 0, num, h);
		}
	}
}
