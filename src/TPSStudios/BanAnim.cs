using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TPSStudios;

internal static class BanAnim
{
	public const int NbBase = 10;

	public const int NbFetesFin = 16;

	public static readonly string[] Ids = new string[20]
	{
		"liquide", "aurore", "pulse", "sakura", "braises", "galaxie", "glitch", "cristaux", "onde", "signature",
		"noel", "nouvelan", "valentin", "paques", "quatorze", "halloween", "or", "prestige", "diamant", "neonvip"
	};

	public static readonly string[] Noms = new string[20]
	{
		"Liquide", "Aurore", "Pulse", "Sakura", "Braises", "Galaxie", "Glitch TPS", "Cristaux", "Onde", "Signature TPS",
		"Noël", "Nouvel An", "Saint-Valentin", "Pâques", "14 Juillet", "Halloween", "Or VIP", "Prestige TPS", "Diamant", "Néon VIP"
	};

	private static readonly Stopwatch chrono = Stopwatch.StartNew();

	private static readonly Color Orange = Color.FromArgb(255, 128, 44);

	private static readonly Color Indigo = Color.FromArgb(84, 100, 255);

	private static readonly Color Rose = Color.FromArgb(255, 80, 170);

	private static readonly Color Cyan = Color.FromArgb(60, 220, 255);

	private static readonly Color Violet = Color.FromArgb(150, 90, 255);

	private static readonly Dictionary<long, Bitmap> halos = new Dictionary<long, Bitmap>();

	private static readonly Color OrC = Color.FromArgb(255, 196, 64);

	private static readonly Color OrClair = Color.FromArgb(255, 236, 170);

	private static readonly Color OrFonce = Color.FromArgb(170, 110, 20);

	private static readonly string[] gT = new string[7] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" };

	private static readonly string[] gP = new string[7] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" };

	private static readonly string[] gS = new string[7] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" };

	public static double Temps => (double)chrono.ElapsedMilliseconds / 1000.0;

	public static float H(int i, int k)
	{
		uint num = (uint)((i * Zx.P(73856093)) ^ (k * Zx.P(19349663) + Zx.P(83492791)));
		num ^= num >> 13;
		num *= 1274126177;
		num ^= num >> 16;
		return (float)(num & 0xFFFFu) / 65535f;
	}

	private static Color A(int a, Color c)
	{
		return Color.FromArgb(Math.Max(0, Math.Min(255, a)), c);
	}

	private static float Mod(double x, double m)
	{
		double num = x % m;
		if (num < 0.0)
		{
			num += m;
		}
		return (float)num;
	}

	private static double Clamp(double x)
	{
		if (!(x < 0.0))
		{
			if (!(x > 1.0))
			{
				return x;
			}
			return 1.0;
		}
		return 0.0;
	}

	private static Color Mel(Color a, Color b, double u)
	{
		u = Clamp(u);
		return Color.FromArgb((int)((double)(int)a.R + (double)(b.R - a.R) * u), (int)((double)(int)a.G + (double)(b.G - a.G) * u), (int)((double)(int)a.B + (double)(b.B - a.B) * u));
	}

	private static void Fond(Graphics g, Rectangle r, Color haut, Color bas)
	{
		using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height + 1), haut, bas, 90f);
		g.FillRectangle(brush, r);
	}

	public static void Brume(Graphics g, float cx, float cy, float rx, float ry, Color c, int alpha)
	{
		if (alpha > 0 && !(rx < 1f) && !(ry < 1f))
		{
			InterpolationMode interpolationMode = g.InterpolationMode;
			g.InterpolationMode = InterpolationMode.Bilinear;
			g.DrawImage(Halo(c, alpha), new RectangleF(cx - rx, cy - ry, rx * 2f, ry * 2f));
			g.InterpolationMode = interpolationMode;
		}
	}

	private static Bitmap Halo(Color c, int alpha)
	{
		int num = Math.Max(1, Math.Min(32, (Math.Min(255, alpha) * 32 + 127) / 255));
		long key = ((long)(c.ToArgb() & 0xFFFFFF) << 8) | num;
		lock (halos)
		{
			if (halos.TryGetValue(key, out var value))
			{
				return value;
			}
			if (halos.Count > 700)
			{
				foreach (Bitmap value2 in halos.Values)
				{
					value2.Dispose();
				}
				halos.Clear();
			}
			value = new Bitmap(64, 64, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(value))
			{
				using GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddEllipse(0, 0, 64, 64);
				using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
				pathGradientBrush.CenterColor = Color.FromArgb(num * 255 / 32, c);
				pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, c) };
				graphics.FillEllipse(pathGradientBrush, 0, 0, Zx.P(64), Zx.P(64));
			}
			halos[key] = value;
			return value;
		}
	}

	private static void Point(Graphics g, float x, float y, float rad, Color c, int alpha)
	{
		using (SolidBrush brush = new SolidBrush(A(alpha / 5, c)))
		{
			g.FillEllipse(brush, x - rad * 3f, y - rad * 3f, rad * 6f, rad * 6f);
		}
		using (SolidBrush brush2 = new SolidBrush(A(alpha / 3, c)))
		{
			g.FillEllipse(brush2, x - rad * 1.7f, y - rad * 1.7f, rad * 3.4f, rad * 3.4f);
		}
		using SolidBrush brush3 = new SolidBrush(A(alpha, Color.White));
		g.FillEllipse(brush3, x - rad * 0.6f, y - rad * 0.6f, rad * 1.2f, rad * 1.2f);
	}

	private static void Maille(Graphics g, Rectangle r, double t, Color fond, Color[] cs, int[] al, double vit)
	{
		using (SolidBrush brush = new SolidBrush(fond))
		{
			g.FillRectangle(brush, r);
		}
		for (int i = 0; i < cs.Length; i++)
		{
			double num = t * vit * (0.6 + 0.23 * (double)i) + (double)i * 2.4;
			Brume(g, (float)r.X + (float)r.Width * (0.5f + 0.44f * (float)Math.Sin(num * 0.9 + (double)i)), (float)r.Y + (float)r.Height * (0.5f + 0.5f * (float)Math.Cos(num * 0.7 + (double)i * 1.7)), (float)r.Width * (0.34f + 0.1f * (float)Math.Sin(num * 0.5 + (double)i)), (float)r.Height * (0.95f + 0.3f * (float)Math.Cos(num * 0.4 + (double)i)), cs[i], al[i]);
		}
	}

	private static void Reflet(Graphics g, Rectangle r, double t, int force)
	{
		float num = (float)r.Width * 0.28f;
		float num2 = (float)r.X + (float)(t * 0.07 % 1.7 - 0.35) * (float)r.Width * 1.35f;
		float num3 = (float)r.Height * 0.45f;
		using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new RectangleF(num2 - num3, r.Y, num + num3, r.Height), A(0, Color.White), A(0, Color.White), 0f);
		ColorBlend colorBlend = new ColorBlend(3);
		colorBlend.Colors = new Color[3]
		{
			A(0, Color.White),
			A(force, Color.White),
			A(0, Color.White)
		};
		colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };
		linearGradientBrush.InterpolationColors = colorBlend;
		g.FillPolygon(linearGradientBrush, new PointF[4]
		{
			new PointF(num2, r.Y),
			new PointF(num2 + num, r.Y),
			new PointF(num2 + num - num3, r.Bottom),
			new PointF(num2 - num3, r.Bottom)
		});
	}

	private static void Lucioles(Graphics g, Rectangle r, double t, int n, int seed, Color c, float vit)
	{
		for (int i = 0; i < n; i++)
		{
			float num = Mod(t * (double)vit * (double)(0.4f + H(i, seed)) + (double)H(i, seed + 1), 1.0);
			float x = (float)r.X + H(i, seed + 2) * (float)r.Width + 18f * (float)Math.Sin(t * 0.6 + (double)i * 1.3);
			float y = (float)r.Bottom - num * (float)(r.Height + Zx.P(20)) + (float)Zx.P(10);
			float num2 = 0.5f + 0.5f * (float)Math.Sin(t * 2.0 + (double)i);
			Point(g, x, y, 1.2f + 2.2f * H(i, seed + 3), c, (int)((double)(90f + 150f * num2) * Math.Sin((double)num * Math.PI)));
		}
	}

	private static void Vignette(Graphics g, Rectangle r)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(new RectangleF(r.X - 1, r.Y - 1, r.Width + Zx.P(2), r.Height + Zx.P(2)));
		using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
		pathGradientBrush.CenterColor = Color.FromArgb(0, 0, 0, 0);
		pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(105, 0, 0, 14) };
		pathGradientBrush.FocusScales = new PointF(0.74f, 0.4f);
		g.FillRectangle(pathGradientBrush, r);
	}

	public static void Dessiner(string id, Graphics g, Rectangle r, double t)
	{
		if (r.Width >= 4 && r.Height >= 4)
		{
			int num = Array.IndexOf(Ids, id);
			if (num < 0)
			{
				num = 0;
			}
			GraphicsState gstate = g.Save();
			g.SetClip(r, CombineMode.Intersect);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			switch (num)
			{
			case 0:
				Liquide(g, r, t);
				break;
			case 1:
				Aurore(g, r, t);
				break;
			case 2:
				Pulse(g, r, t);
				break;
			case 3:
				Sakura(g, r, t);
				break;
			case 4:
				Braises(g, r, t);
				break;
			case 5:
				Galaxie(g, r, t);
				break;
			case 6:
				Glitch(g, r, t);
				break;
			case 7:
				Cristaux(g, r, t);
				break;
			case 8:
				Onde(g, r, t);
				break;
			case 9:
				Signature(g, r, t);
				break;
			case 10:
				Noel(g, r, t);
				break;
			case 11:
				NouvelAn(g, r, t);
				break;
			case 12:
				Valentin(g, r, t);
				break;
			case 13:
				Paques(g, r, t);
				break;
			case 14:
				Quatorze(g, r, t);
				break;
			case 15:
				Halloween(g, r, t);
				break;
			case 16:
				Or(g, r, t);
				break;
			case 17:
				Prestige(g, r, t);
				break;
			case 18:
				Diamant(g, r, t);
				break;
			default:
				NeonVip(g, r, t);
				break;
			}
			Vignette(g, r);
			g.Restore(gstate);
		}
	}

	private static void Liquide(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(10, 10, 34), new Color[5] { Indigo, Orange, Rose, Cyan, Violet }, new int[5] { 235, 215, 190, 150, 190 }, 0.32);
		for (int i = 0; i < 3; i++)
		{
			double num = t * 0.2 + (double)i * 2.1;
			Brume(g, (float)r.X + (float)r.Width * (0.5f + 0.35f * (float)Math.Sin(num)), (float)r.Y + (float)r.Height * (0.45f + 0.3f * (float)Math.Cos(num * 1.3)), (float)r.Width * 0.12f, (float)r.Height * 0.5f, Color.White, 60);
		}
		Lucioles(g, r, t, 22, 3, Color.FromArgb(255, 235, 215), 0.06f);
		Reflet(g, r, t, 36);
	}

	private static void Aurore(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(3, 6, 24), Color.FromArgb(10, 22, 52));
		for (int i = 0; i < 70; i++)
		{
			float num = 0.5f + 0.5f * (float)Math.Sin(t * (double)(1f + 2f * H(i, 3)) + (double)i);
			using SolidBrush brush = new SolidBrush(Color.FromArgb((int)(60f + 190f * num), 225, 235, 255));
			g.FillEllipse(brush, (float)r.X + H(i, 1) * (float)r.Width, (float)r.Y + H(i, 2) * (float)r.Height * 0.8f, 0.8f + 1.6f * H(i, 4), 0.8f + 1.6f * H(i, 4));
		}
		Color[] array = new Color[5]
		{
			Color.FromArgb(40, 255, 160),
			Color.FromArgb(40, 210, 255),
			Violet,
			Rose,
			Color.FromArgb(120, 255, 200)
		};
		for (int j = 0; j < 5; j++)
		{
			List<PointF> list = new List<PointF>();
			float num2 = (float)r.Y + (float)r.Height * (0.28f + 0.1f * (float)j);
			for (int k = r.X - Zx.P(10); k <= r.Right + 10; k += Zx.P(8))
			{
				double num3 = (double)(k - r.X) / ((double)r.Width * 0.22) + t * (0.5 + 0.1 * (double)j) + (double)j * 1.7;
				list.Add(new PointF(k, (float)((double)num2 + (double)r.Height * 0.15 * Math.Sin(num3) + (double)r.Height * 0.05 * Math.Sin(num3 * 2.7 - t * 0.8 + (double)j))));
			}
			float[] array2 = new float[4] { 0.62f, 0.36f, 0.2f, 0.08f };
			int[] array3 = new int[4] { 10, 16, 26, 70 };
			for (int l = 0; l < 4; l++)
			{
				using Pen pen = new Pen(A(array3[l], array[j]), (float)r.Height * array2[l]);
				pen.LineJoin = LineJoin.Round;
				g.DrawLines(pen, list.ToArray());
			}
		}
		Brume(g, (float)r.X + (float)r.Width * 0.5f, r.Bottom, (float)r.Width * 0.6f, (float)r.Height * 0.5f, Color.FromArgb(60, 255, 200), 40);
	}

	private static void Pulse(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(8, 6, 28), new Color[3] { Violet, Rose, Indigo }, new int[3] { 150, 120, 160 }, 0.2);
		int num = Math.Max(12, r.Width / 9);
		float num2 = (float)r.Y + (float)r.Height * 0.62f;
		float num3 = (float)r.Width / (float)num;
		for (int i = 0; i < num; i++)
		{
			double num4 = (double)i / (double)num;
			double num5 = (Math.Sin((double)i * 0.33 + t * 2.1) * 0.5 + 0.5) * (Math.Sin((double)i * 0.11 - t * 1.3) * 0.5 + 0.5) * 0.72 + 0.1 + 0.05 * Math.Sin(t * 5.0 + (double)i);
			Color color = ((num4 < 0.5) ? Mel(Orange, Rose, num4 * 2.0) : Mel(Rose, Cyan, (num4 - 0.5) * 2.0));
			float num6 = (float)(num5 * (double)r.Height * 0.55);
			float num7 = (float)r.X + (float)i * num3 + num3 * 0.2f;
			float num8 = num3 * 0.6f;
			using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(num7, num2 - num6, num8, num6 + 1f), color, A(30, color), 90f))
			{
				g.FillRectangle(brush, num7, num2 - num6, num8, num6);
			}
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new RectangleF(num7, num2, num8, num6 * 0.45f + 1f), A(70, color), A(0, color), 90f))
			{
				g.FillRectangle(brush2, num7, num2, num8, num6 * 0.45f);
			}
			using (SolidBrush brush3 = new SolidBrush(Color.White))
			{
				g.FillRectangle(brush3, num7, num2 - num6 - 1f, num8, Zx.P(2.2f));
			}
			if (num5 > 0.62)
			{
				Brume(g, num7 + num8 / 2f, num2 - num6, num3 * 2.6f, num3 * 2.6f, color, 120);
			}
		}
		Reflet(g, r, t, 22);
	}

	private static void Sakura(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(34, 12, 54), new Color[4]
		{
			Rose,
			Violet,
			Color.FromArgb(255, 150, 190),
			Indigo
		}, new int[4] { 200, 170, 150, 120 }, 0.18);
		Lucioles(g, r, t, 14, 8, Color.FromArgb(255, 220, 235), 0.04f);
		for (int i = 0; i < 46; i++)
		{
			int num = i % 3;
			float num2 = 0.45f + 0.55f * ((float)num / 2f);
			float num3 = (14f + 22f * num2) * (0.7f + 0.6f * H(i, 2));
			float num4 = (6f + 9f * num2) * (0.7f + 0.6f * H(i, 3));
			float dx = (float)r.X + Mod((double)(H(i, 1) * (float)(r.Width + Zx.P(80))) - t * (double)num3 * 0.5 + (double)Zx.P(34) * Math.Sin(t * 0.7 + (double)i), r.Width + 80) - (float)Zx.P(40);
			float dy = (float)r.Y + Mod((double)(H(i, 4) * (float)(r.Height + Zx.P(40))) + t * (double)num3, r.Height + 40) - (float)Zx.P(20);
			float angle = (float)(t * 50.0 * (double)(H(i, 5) - 0.5f) * 2.0 + (double)(i * 41));
			GraphicsState gstate = g.Save();
			g.TranslateTransform(dx, dy);
			g.RotateTransform(angle);
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(0f - num4, (0f - num4) * 0.55f, num4 * 2f, num4 * 1.1f);
				using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(0f - num4, (0f - num4) * 0.6f, num4 * 2f, num4 * 1.2f), A((int)(190f * num2), Color.FromArgb(255, 200, 225)), A((int)(220f * num2), Color.FromArgb(255, 120, 175)), 0f))
				{
					g.FillPath(brush, graphicsPath);
				}
				using Pen pen = new Pen(A((int)(120f * num2), Color.White), 1f);
				g.DrawPath(pen, graphicsPath);
			}
			g.Restore(gstate);
		}
		Reflet(g, r, t, 22);
	}

	private static void Braises(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(10, 2, 8), Color.FromArgb(70, 12, 6));
		Brume(g, (float)r.X + (float)r.Width * 0.5f, (float)r.Bottom + (float)r.Height * 0.2f, (float)r.Width * 0.8f, (float)r.Height * 1f, Orange, (int)(190.0 + 40.0 * Math.Sin(t * 1.3)));
		Brume(g, (float)r.X + (float)r.Width * (0.3f + 0.1f * (float)Math.Sin(t * 0.4)), r.Bottom, (float)r.Width * 0.4f, (float)r.Height * 0.7f, Color.FromArgb(255, 200, 80), 120);
		for (int i = 0; i < 5; i++)
		{
			float num = Mod(t * 0.05 + (double)i * 0.2, 1.0);
			Brume(g, (float)r.X + (float)r.Width * (0.15f + 0.18f * (float)i) + 24f * (float)Math.Sin(t * 0.3 + (double)i), (float)r.Bottom - num * (float)r.Height * 1.1f, (float)r.Width * 0.16f, (float)r.Height * 0.3f, Color.FromArgb(40, 14, 10), (int)(110f * (1f - num)));
		}
		for (int j = 0; j < 70; j++)
		{
			float num2 = Mod(t * (0.1 + 0.16 * (double)H(j, 2)) + (double)H(j, 3), 1.0);
			float x = (float)r.X + H(j, 0) * (float)r.Width + 22f * (float)Math.Sin(t * (0.7 + (double)H(j, 1)) + (double)j);
			float y = (float)r.Bottom - num2 * (float)r.Height * 1.2f;
			Point(g, x, y, 1f + 1.8f * H(j, 4), Mel(Color.FromArgb(255, 80, 20), Color.FromArgb(255, 210, 90), H(j, 5)), (int)((double)(255f * (1f - num2)) * (0.6 + 0.4 * Math.Sin(t * 6.0 + (double)j))));
		}
		Reflet(g, r, t, 16);
	}

	private static void Galaxie(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(4, 4, 20), new Color[4]
		{
			Violet,
			Indigo,
			Rose,
			Color.FromArgb(80, 60, 200)
		}, new int[4] { 150, 150, 100, 130 }, 0.12);
		GraphicsState gstate = g.Save();
		g.TranslateTransform((float)r.X + (float)r.Width * 0.62f, (float)r.Y + (float)r.Height * 0.5f);
		g.RotateTransform((float)(t * 6.0));
		Brume(g, 0f, 0f, (float)r.Width * 0.34f, (float)r.Height * 0.32f, Color.FromArgb(255, 170, 230), 70);
		Brume(g, 0f, 0f, (float)r.Width * 0.2f, (float)r.Height * 0.14f, Color.FromArgb(255, 240, 220), 110);
		g.Restore(gstate);
		for (int i = 0; i < 90; i++)
		{
			float num = 0.5f + 0.5f * (float)Math.Sin(t * (0.8 + (double)(2f * H(i, 3))) + (double)i * 1.7);
			float x = (float)r.X + Mod((double)(H(i, 1) * (float)r.Width) - t * (double)(1f + 5f * H(i, 7)), r.Width);
			float y = (float)r.Y + H(i, 2) * (float)r.Height;
			Point(g, x, y, 0.5f + 1.3f * H(i, 4), Mel(Color.White, Color.FromArgb(190, 210, 255), H(i, 6)), (int)(60f + 190f * num));
		}
		for (int j = 0; j < 2; j++)
		{
			double num2 = 4.5 + (double)j * 2.1;
			double num3 = (t + (double)j * 2.7) % num2 / num2;
			if (num3 > 0.3)
			{
				continue;
			}
			double num4 = num3 / 0.3;
			float num5 = (float)((double)(r.Right + Zx.P(40)) - num4 * (double)(r.Width + Zx.P(120)));
			float num6 = (float)((double)r.Y + (double)r.Height * (0.1 + 0.4 * (double)j) + num4 * (double)r.Height * 0.55);
			for (int num7 = 26; num7 >= 1; num7--)
			{
				using Pen pen = new Pen(A((int)(210.0 * (1.0 - (double)num7 / 27.0) * (1.0 - num4 * 0.4)), Color.FromArgb(200, 220, 255)), 3.2f * (1f - (float)num7 / 30f));
				g.DrawLine(pen, num5 + (float)(num7 * 7), num6 - (float)num7 * 3.3f, num5 + (float)(num7 * 7) + (float)Zx.P(7), num6 - (float)num7 * 3.3f - Zx.P(3.3f));
			}
			Point(g, num5, num6, 3.2f, Color.FromArgb(220, 235, 255), 255);
		}
	}

	private static void Glitch(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(6, 4, 22), new Color[3] { Indigo, Rose, Cyan }, new int[3] { 190, 150, 90 }, 0.4);
		using (Font font = new Font("Segoe UI Black", Math.Max(Zx.P(9f), Math.Min((float)r.Height * 0.5f, (float)r.Width * 0.1f)), FontStyle.Regular, GraphicsUnit.Pixel))
		{
			string text = "TPS GROUPE";
			SizeF sizeF = g.MeasureString(text, font, 4000, StringFormat.GenericTypographic);
			float num = (float)r.X + ((float)r.Width - sizeF.Width) / 2f;
			float y = (float)r.Y + ((float)r.Height - sizeF.Height) / 2f;
			double num2 = t % 2.4;
			double num3 = ((num2 < 0.28) ? 1 : 0);
			float num4 = (float)(num3 * (double)(6f + 18f * H((int)(t * 20.0), 1)));
			StringFormat genericTypographic = StringFormat.GenericTypographic;
			using (SolidBrush brush = new SolidBrush(A(210, Color.FromArgb(255, 40, 120))))
			{
				g.DrawString(text, font, brush, num - 3f - num4, y, genericTypographic);
			}
			using (SolidBrush brush2 = new SolidBrush(A(210, Color.FromArgb(40, 230, 255))))
			{
				g.DrawString(text, font, brush2, num + 3f + num4, y, genericTypographic);
			}
			using (SolidBrush brush3 = new SolidBrush(Color.White))
			{
				g.DrawString(text, font, brush3, num, y, genericTypographic);
			}
			if (num3 > 0.0)
			{
				for (int i = 0; i < 4; i++)
				{
					float y2 = (float)r.Y + H((int)(t * 20.0) + i, 2) * (float)r.Height;
					float height = 3f + 12f * H((int)(t * 20.0) + i, 3);
					float num5 = (H((int)(t * 20.0) + i, 4) - 0.5f) * 60f;
					GraphicsState gstate = g.Save();
					g.SetClip(new RectangleF(r.X, y2, r.Width, height), CombineMode.Intersect);
					using (SolidBrush brush4 = new SolidBrush(A(230, (i % 2 == 0) ? Color.FromArgb(40, 230, 255) : Color.FromArgb(255, 60, 150))))
					{
						g.DrawString(text, font, brush4, num + num5, y, genericTypographic);
					}
					g.Restore(gstate);
				}
			}
		}
		using Pen pen = new Pen(Color.FromArgb(20, 255, 255, 255), 1f);
		for (int j = r.Y; j < r.Bottom; j += Zx.P(3))
		{
			g.DrawLine(pen, r.X, j, r.Right, j);
		}
	}

	private static void Cristaux(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(6, 8, 30), new Color[4] { Indigo, Cyan, Violet, Rose }, new int[4] { 170, 120, 150, 90 }, 0.2);
		for (int i = 0; i < 16; i++)
		{
			float num = 0.4f + 0.6f * H(i, 1);
			float num2 = ((float)r.Height * 0.16f + (float)r.Height * 0.34f * H(i, 2)) * num;
			float num3 = (float)r.X + Mod((double)(H(i, 3) * (float)(r.Width + Zx.P(120))) - t * (double)((float)Zx.P(4) + (float)Zx.P(14) * num), r.Width + 120) - (float)Zx.P(60);
			float num4 = (float)r.Y + (float)r.Height * (0.1f + 0.8f * H(i, 4)) + 8f * (float)Math.Sin(t * 0.6 + (double)i);
			float angle = (float)(t * 14.0 * (double)(H(i, 5) - 0.5f) + (double)(H(i, 6) * 360f));
			int num5 = 3 + i % 3;
			PointF[] array = new PointF[num5 * 2];
			for (int j = 0; j < array.Length; j++)
			{
				double num6 = (double)j * Math.PI / (double)num5;
				float num7 = num2 * ((j % 2 == 0) ? 1f : 0.62f);
				ref PointF reference = ref array[j];
				reference = new PointF((float)Math.Cos(num6) * num7, (float)Math.Sin(num6) * num7);
			}
			GraphicsState gstate = g.Save();
			g.TranslateTransform(num3, num4);
			g.RotateTransform(angle);
			Color c = Mel(Cyan, Rose, H(i, 7));
			Color c2 = Mel(Indigo, Violet, H(i, 8));
			using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(0f - num2, 0f - num2, num2 * 2f, num2 * 2f), A((int)(150f * num), c), A((int)(60f * num), c2), 45f))
			{
				g.FillPolygon(brush, array);
			}
			using (Pen pen = new Pen(A((int)(200f * num), Color.White), 1.3f))
			{
				pen.LineJoin = LineJoin.Round;
				g.DrawPolygon(pen, array);
			}
			for (int k = 0; k < array.Length; k += 2)
			{
				using Pen pen2 = new Pen(A((int)(60f * num), Color.White), 1f);
				g.DrawLine(pen2, 0f, 0f, array[k].X, array[k].Y);
			}
			g.Restore(gstate);
			Point(g, num3, num4, 1.4f, Color.White, (int)(160.0 * (0.5 + 0.5 * Math.Sin(t * 2.0 + (double)(i * 2)))));
		}
		Reflet(g, r, t, 30);
	}

	private static void Onde(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(4, 8, 28), new Color[3] { Cyan, Indigo, Violet }, new int[3] { 150, 170, 130 }, 0.18);
		PointF[] array = new PointF[2]
		{
			new PointF((float)r.X + (float)r.Width * (0.3f + 0.06f * (float)Math.Sin(t * 0.3)), (float)r.Y + (float)r.Height * 0.55f),
			new PointF((float)r.X + (float)r.Width * (0.72f + 0.06f * (float)Math.Cos(t * 0.25)), (float)r.Y + (float)r.Height * 0.45f)
		};
		Color[] array2 = new Color[2] { Cyan, Rose };
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 7; j++)
			{
				double num = (t * 0.22 + (double)j / 7.0) % 1.0;
				float num2 = (float)(num * (double)r.Width * 0.7);
				using (Pen pen = new Pen(A((int)(34.0 * (1.0 - num)), array2[i]), Zx.P(10f)))
				{
					g.DrawEllipse(pen, array[i].X - num2, array[i].Y - num2, num2 * 2f, num2 * 2f);
				}
				using Pen pen2 = new Pen(A((int)(210.0 * (1.0 - num)), Color.FromArgb(220, 240, 255)), 1.8f);
				g.DrawEllipse(pen2, array[i].X - num2, array[i].Y - num2, num2 * 2f, num2 * 2f);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			Brume(g, array[k].X, array[k].Y, 30f, 30f, array2[k], 200);
		}
		Lucioles(g, r, t, 18, 12, Color.FromArgb(210, 240, 255), 0.05f);
		Reflet(g, r, t, 22);
	}

	public static void Coeur(Graphics g, float cx, float cy, float s, Brush b)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddBezier(cx, cy + s, cx - s * 1.25f, cy + s * 0.15f, cx - s * 0.95f, cy - s * 0.95f, cx, cy - s * 0.35f);
		graphicsPath.AddBezier(cx, cy - s * 0.35f, cx + s * 0.95f, cy - s * 0.95f, cx + s * 1.25f, cy + s * 0.15f, cx, cy + s);
		g.FillPath(b, graphicsPath);
	}

	public static void Artifices(Graphics g, Rectangle r, double t, int n, Color[] cs, int seed)
	{
		for (int i = 0; i < n; i++)
		{
			double num = 2.6 + 1.4 * (double)H(i, seed);
			double num2 = (double)Mod(t + (double)H(i, seed + 1) * num * 3.0, num) / num;
			int num3 = (int)Math.Floor((t + (double)H(i, seed + 1) * num * 3.0) / num);
			float num4 = (float)r.X + (float)r.Width * (0.1f + 0.8f * H(i * 7 + num3, seed + 2));
			float num5 = (float)r.Y + (float)r.Height * (0.2f + 0.3f * H(i * 5 + num3, seed + 3));
			float num6 = (float)r.Height * (0.28f + 0.2f * H(i + num3, seed + 4));
			Color c = cs[(i + num3) % cs.Length];
			if (num2 < 0.22)
			{
				double num7 = num2 / 0.22;
				float num8 = (float)((double)r.Bottom - (double)((float)r.Bottom - num5) * (1.0 - Math.Pow(1.0 - num7, 2.0)));
				Point(g, num4, num8, 1.2f, Color.FromArgb(255, 220, 160), 230);
				using (Pen pen = new Pen(A(90, Color.FromArgb(255, 200, 140)), 1.2f))
				{
					g.DrawLine(pen, num4, num8, num4, num8 + (float)Zx.P(14));
				}
				continue;
			}
			double num9 = (num2 - 0.22) / 0.78;
			double num10 = 1.0 - Math.Pow(1.0 - num9, 3.0);
			int num11 = (int)(255.0 * (1.0 - num9) * (1.0 - num9) + 20.0 * (1.0 - num9));
			if (num9 < 0.25)
			{
				Brume(g, num4, num5, num6 * 1.4f, num6 * 1.4f, c, (int)(170.0 * (1.0 - num9 / 0.25)));
			}
			int num12 = 26;
			for (int j = 0; j < num12; j++)
			{
				double num13 = (double)j * Math.PI * 2.0 / (double)num12 + (double)H(j, i) * 0.2;
				float num14 = 0.75f + 0.25f * H(j, i + 9);
				float num15 = num4 + (float)(Math.Cos(num13) * (double)num6 * (double)num14 * num10);
				float num16 = num5 + (float)(Math.Sin(num13) * (double)num6 * (double)num14 * num10 + num9 * num9 * (double)num6 * 0.55);
				float x = num4 + (float)(Math.Cos(num13) * (double)num6 * (double)num14 * num10 * 0.86);
				float y = num5 + (float)(Math.Sin(num13) * (double)num6 * (double)num14 * num10 * 0.86 + num9 * num9 * (double)num6 * 0.5);
				using (Pen pen2 = new Pen(A(num11 * 2 / 3, c), Zx.P(2f)))
				{
					g.DrawLine(pen2, x, y, num15, num16);
				}
				Point(g, num15, num16, 1.7f, c, Math.Min(255, num11 + 30));
			}
		}
	}

	private static void Etoiles(Graphics g, Rectangle r, double t, int n, int seed)
	{
		for (int i = 0; i < n; i++)
		{
			float num = 0.5f + 0.5f * (float)Math.Sin(t * (double)(1f + 2f * H(i, seed)) + (double)i);
			using SolidBrush brush = new SolidBrush(Color.FromArgb((int)(50f + 170f * num), 230, 236, 255));
			float num2 = 0.8f + 1.4f * H(i, seed + 3);
			g.FillEllipse(brush, (float)r.X + H(i, seed + 1) * (float)r.Width, (float)r.Y + H(i, seed + 2) * (float)r.Height * 0.75f, num2, num2);
		}
	}

	private static void Noel(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(6, 14, 40), Color.FromArgb(14, 46, 60));
		Brume(g, (float)r.X + (float)r.Width * 0.5f, r.Bottom, (float)r.Width * 0.7f, (float)r.Height * 0.6f, Color.FromArgb(120, 200, 255), 50);
		Etoiles(g, r, t, 40, 20);
		for (int i = 0; i < 9; i++)
		{
			float num = (float)r.X + (float)r.Width * ((float)i / 8f) + (H(i, 31) - 0.5f) * (float)Zx.P(30);
			float num2 = (float)r.Height * (0.32f + 0.25f * H(i, 32));
			float num3 = num2 * 0.55f;
			float num4 = r.Bottom + 4;
			Color color = ((i % 2 == 0) ? Color.FromArgb(24, 92, 70) : Color.FromArgb(18, 74, 60));
			Color color2 = Color.FromArgb(8, 40, 34);
			for (int j = 0; j < 3; j++)
			{
				float num5 = num4 - num2 + (float)j * num2 * 0.25f;
				float num6 = num4 - num2 * (0.45f - (float)j * 0.2f) + num2 * 0.2f;
				float num7 = num3 * (0.55f + 0.25f * (float)j);
				PointF[] points = new PointF[3]
				{
					new PointF(num, num5),
					new PointF(num + num7, num6),
					new PointF(num - num7, num6)
				};
				using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(num - num7, num5, num7 * 2f, num6 - num5 + 1f), color, color2, 0f))
				{
					g.FillPolygon(brush, points);
				}
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb(225, 236, 244, 255));
				g.FillEllipse(brush2, num - num7 * 0.95f, num6 - Zx.P(2.5f), num7 * 1.9f, Zx.P(5));
				g.FillEllipse(brush2, num - Zx.P(2.5f), num5 - 1f, Zx.P(5), Zx.P(5));
			}
		}
		using (LinearGradientBrush brush3 = new LinearGradientBrush(new RectangleF(r.X, r.Bottom - Zx.P(12), r.Width, Zx.P(13)), Color.FromArgb(220, 230, 240, 255), Color.FromArgb(255, 200, 220, 240), 90f))
		{
			g.FillRectangle(brush3, r.X, r.Bottom - Zx.P(12), r.Width, Zx.P(12));
		}
		Color[] array = new Color[4]
		{
			Color.FromArgb(255, 70, 70),
			Color.FromArgb(255, 200, 60),
			Color.FromArgb(70, 230, 120),
			Color.FromArgb(80, 170, 255)
		};
		for (int k = 0; k < 2; k++)
		{
			float num8 = (float)r.Y + (float)r.Height * (0.08f + 0.14f * (float)k);
			float num9 = (float)r.Height * 0.16f;
			float num10 = (float)Math.Sin(t * 1.3 + (double)k) * 4f;
			List<PointF> list = new List<PointF>();
			for (int l = 0; l <= 40; l++)
			{
				float num11 = (float)l / 40f;
				float num12 = num11 * 3f % 1f;
				list.Add(new PointF((float)r.X + num11 * (float)r.Width, num8 + num9 * 4f * num12 * (1f - num12) + num10 * num12));
			}
			using (Pen pen = new Pen(Color.FromArgb(160, 30, 40, 50), 1.5f))
			{
				g.DrawCurve(pen, list.ToArray());
			}
			for (int m = 1; m < 40; m += 2)
			{
				PointF pointF = list[m];
				Color c = array[(m / 2 + k) % 4];
				double num13 = Math.Sin(t * 3.0 + (double)m * 0.9 + (double)(k * 2));
				int num14 = ((num13 > -0.3) ? 255 : 90);
				Brume(g, pointF.X, pointF.Y + (float)Zx.P(5), 14f, 14f, c, num14 / 2);
				using SolidBrush brush4 = new SolidBrush(A(num14, c));
				g.FillEllipse(brush4, pointF.X - (float)Zx.P(3), pointF.Y + 1f, Zx.P(6), Zx.P(9));
			}
		}
		for (int n = 0; n < 90; n++)
		{
			float num15 = 0.4f + 0.6f * ((float)(n % 3) / 2f);
			float num16 = 12f + 26f * num15;
			float x = (float)r.X + Mod((double)(H(n, 41) * (float)r.Width) + (double)Zx.P(14) * Math.Sin(t * 0.8 + (double)n), r.Width);
			float y = (float)r.Y + Mod((double)(H(n, 42) * (float)(r.Height + Zx.P(10))) + t * (double)num16, r.Height + 10) - (float)Zx.P(5);
			using SolidBrush brush5 = new SolidBrush(Color.FromArgb((int)(140f + 110f * num15), 255, 255, 255));
			float num17 = 1.2f + 2.6f * num15 * H(n, 43) + 0.6f;
			g.FillEllipse(brush5, x, y, num17, num17);
		}
		Reflet(g, r, t, 18);
	}

	private static void NouvelAn(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(8, 4, 26), new Color[3]
		{
			Color.FromArgb(90, 40, 170),
			Color.FromArgb(30, 30, 110),
			Color.FromArgb(150, 60, 120)
		}, new int[3] { 130, 140, 90 }, 0.12);
		Etoiles(g, r, t, 50, 50);
		Artifices(g, r, t, 4, new Color[5]
		{
			Color.FromArgb(255, 210, 90),
			Color.FromArgb(255, 120, 200),
			Color.FromArgb(120, 220, 255),
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(190, 140, 255)
		}, 60);
		for (int i = 0; i < 40; i++)
		{
			float x = (float)r.X + Mod((double)(H(i, 71) * (float)r.Width) + (double)Zx.P(10) * Math.Sin(t * 1.5 + (double)i), r.Width);
			float y = (float)r.Y + Mod((double)(H(i, 72) * (float)r.Height) + t * (double)((float)Zx.P(16) + (float)Zx.P(18) * H(i, 73)), r.Height);
			float val = 4.5f * (float)Math.Abs(Math.Cos(t * 4.0 + (double)i));
			using SolidBrush brush = new SolidBrush(Color.FromArgb(220, Mel(Color.FromArgb(255, 215, 110), Color.FromArgb(255, 245, 200), H(i, 74))));
			g.FillRectangle(brush, x, y, Math.Max(0.8f, val), Zx.P(2.4f));
		}
		Reflet(g, r, t, 20);
	}

	private static void Valentin(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(40, 4, 22), new Color[4]
		{
			Color.FromArgb(255, 60, 110),
			Color.FromArgb(200, 30, 80),
			Color.FromArgb(255, 140, 170),
			Color.FromArgb(150, 40, 140)
		}, new int[4] { 200, 170, 140, 130 }, 0.2);
		for (int i = 0; i < 30; i++)
		{
			float num = 0.4f + 0.6f * ((float)(i % 3) / 2f);
			float num2 = Mod(t * (0.04 + 0.05 * (double)num) + (double)H(i, 81), 1.0);
			float num3 = (5f + 11f * num) * (0.7f + 0.5f * H(i, 82));
			float num4 = (float)r.X + H(i, 83) * (float)r.Width + (float)Zx.P(18) * (float)Math.Sin(t * 0.9 + (double)i);
			float num5 = (float)(r.Bottom + Zx.P(20)) - num2 * (float)(r.Height + Zx.P(40));
			int num6 = (int)(255.0 * Math.Sin((double)num2 * Math.PI) * (0.45 + 0.55 * (double)num));
			float num7 = 1f + 0.08f * (float)Math.Sin(t * 6.0 + (double)i);
			Brume(g, num4, num5, num3 * 2.4f, num3 * 2.4f, Color.FromArgb(255, 90, 140), num6 / 3);
			using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(num4 - num3, num5 - num3, num3 * 2f, num3 * 2f + 1f), A(num6, Color.FromArgb(255, 150, 180)), A(num6, Color.FromArgb(225, 30, 80)), 90f))
			{
				Coeur(g, num4, num5, num3 * num7, b);
			}
			using SolidBrush brush = new SolidBrush(A(num6 / 2, Color.White));
			g.FillEllipse(brush, num4 - num3 * 0.55f, num5 - num3 * 0.45f, num3 * 0.35f, num3 * 0.25f);
		}
		Lucioles(g, r, t, 14, 85, Color.FromArgb(255, 220, 230), 0.05f);
		Reflet(g, r, t, 26);
	}

	private static void Paques(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(150, 205, 255), Color.FromArgb(255, 222, 200));
		Brume(g, (float)r.X + (float)r.Width * 0.8f, (float)r.Y + (float)r.Height * 0.15f, (float)r.Height * 0.9f, (float)r.Height * 0.9f, Color.FromArgb(255, 250, 210), 170);
		for (int i = 0; i < 4; i++)
		{
			float num = (float)r.X + Mod((double)(H(i, 91) * (float)r.Width) + t * 6.0 * (0.5 + (double)H(i, 92)), r.Width + 160) - (float)Zx.P(80);
			float num2 = (float)r.Y + (float)r.Height * (0.12f + 0.2f * H(i, 93));
			for (int j = 0; j < 3; j++)
			{
				Brume(g, num + (float)(j * Zx.P(22)), num2 + (float)((j == 1) ? (-Zx.P(6)) : 0), 30f, 16f, Color.White, 200);
			}
		}
		for (int k = 0; k < 2; k++)
		{
			List<PointF> list = new List<PointF>();
			float num3 = (float)r.Y + (float)r.Height * (0.66f + 0.12f * (float)k);
			for (int l = r.X - Zx.P(10); l <= r.Right + 10; l += Zx.P(10))
			{
				list.Add(new PointF(l, num3 + (float)((double)r.Height * 0.08 * Math.Sin((double)(l - r.X) / ((double)r.Width * 0.25) + (double)(k * 2)))));
			}
			list.Add(new PointF(r.Right + Zx.P(10), r.Bottom + Zx.P(2)));
			list.Add(new PointF(r.X - Zx.P(10), r.Bottom + Zx.P(2)));
			using SolidBrush brush = new SolidBrush((k == 0) ? Color.FromArgb(140, 210, 120) : Color.FromArgb(105, 190, 95));
			g.FillPolygon(brush, list.ToArray());
		}
		Color[,] array = new Color[5, 2]
		{
			{
				Color.FromArgb(255, 150, 190),
				Color.FromArgb(255, 235, 120)
			},
			{
				Color.FromArgb(140, 190, 255),
				Color.FromArgb(255, 255, 255)
			},
			{
				Color.FromArgb(190, 150, 255),
				Color.FromArgb(150, 235, 200)
			},
			{
				Color.FromArgb(255, 200, 120),
				Color.FromArgb(255, 120, 150)
			},
			{
				Color.FromArgb(150, 230, 180),
				Color.FromArgb(255, 170, 200)
			}
		};
		for (int m = 0; m < 5; m++)
		{
			float num4 = (float)r.Height * 0.42f;
			float num5 = num4 * 0.74f;
			float dx = (float)r.X + (float)r.Width * (0.12f + 0.19f * (float)m);
			float num6 = (float)r.Y + (float)r.Height * (0.86f + 0.04f * (float)(m % 2)) - num4;
			float angle = (float)(7.0 * Math.Sin(t * 2.2 + (double)m * 1.3));
			GraphicsState gstate = g.Save();
			g.TranslateTransform(dx, num6 + num4);
			g.RotateTransform(angle);
			RectangleF rect = new RectangleF((0f - num5) / 2f, 0f - num4, num5, num4);
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(rect);
				Color color = array[m, 0];
				Color color2 = array[m, 1];
				using (LinearGradientBrush brush2 = new LinearGradientBrush(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height + 1f), ControlPaint.Light(color, 0.3f), color, 0f))
				{
					g.FillPath(brush2, graphicsPath);
				}
				GraphicsState gstate2 = g.Save();
				g.SetClip(graphicsPath, CombineMode.Intersect);
				using (SolidBrush brush3 = new SolidBrush(color2))
				{
					g.FillRectangle(brush3, rect.X, rect.Y + num4 * 0.42f, num5, num4 * 0.12f);
					for (int n = 0; n < 5; n++)
					{
						g.FillEllipse(brush3, rect.X + num5 * (0.1f + 0.2f * (float)n), rect.Y + num4 * 0.66f, num5 * 0.1f, num5 * 0.1f);
					}
				}
				using (Pen pen = new Pen(color2, Zx.P(2f)))
				{
					List<PointF> list2 = new List<PointF>();
					for (int num7 = 0; num7 <= 8; num7++)
					{
						list2.Add(new PointF(rect.X + num5 * (float)num7 / 8f, rect.Y + num4 * ((num7 % 2 == 0) ? 0.26f : 0.34f)));
					}
					g.DrawLines(pen, list2.ToArray());
				}
				g.Restore(gstate2);
				using SolidBrush brush4 = new SolidBrush(Color.FromArgb(110, 255, 255, 255));
				g.FillEllipse(brush4, rect.X + num5 * 0.2f, rect.Y + num4 * 0.12f, num5 * 0.18f, num4 * 0.2f);
			}
			g.Restore(gstate);
		}
		for (int num8 = 0; num8 < 18; num8++)
		{
			float x = (float)r.X + Mod((double)(H(num8, 95) * (float)(r.Width + Zx.P(40))) + t * (double)Zx.P(20), r.Width + 40) - (float)Zx.P(20);
			float y = (float)r.Y + (float)r.Height * (0.1f + 0.6f * H(num8, 96)) + (float)Zx.P(10) * (float)Math.Sin(t * 1.5 + (double)num8);
			using SolidBrush brush5 = new SolidBrush(Color.FromArgb(200, Mel(Color.FromArgb(255, 180, 210), Color.White, H(num8, 97))));
			g.FillEllipse(brush5, x, y, Zx.P(5), Zx.P(3.4f));
		}
		Reflet(g, r, t, 28);
	}

	private static void Quatorze(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(6, 8, 30), Color.FromArgb(20, 22, 60));
		Etoiles(g, r, t, 40, 110);
		Artifices(g, r, t, 4, new Color[3]
		{
			Color.FromArgb(80, 130, 255),
			Color.FromArgb(255, 255, 255),
			Color.FromArgb(255, 70, 80)
		}, 120);
		Color[] array = new Color[3]
		{
			Color.FromArgb(0, 85, 164),
			Color.FromArgb(245, 245, 250),
			Color.FromArgb(239, 65, 53)
		};
		for (int i = 0; i < 3; i++)
		{
			List<PointF> list = new List<PointF>();
			List<PointF> list2 = new List<PointF>();
			for (int j = r.X - Zx.P(10); j <= r.Right + 10; j += Zx.P(8))
			{
				double a = (double)(j - r.X) / ((double)r.Width * 0.18) - t * 1.6;
				float num = (float)r.Y + (float)r.Height * 0.78f + (float)((double)r.Height * 0.06 * Math.Sin(a)) + (float)(i * r.Height) * 0.07f;
				list.Add(new PointF(j, num));
				list2.Insert(0, new PointF(j, num + (float)r.Height * 0.07f + 0.5f));
			}
			list.AddRange(list2);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(230, array[i]));
			g.FillPolygon(brush, list.ToArray());
		}
		Reflet(g, r, t, 20);
	}

	private static void Halloween(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(26, 8, 40), Color.FromArgb(90, 30, 20));
		float num = (float)r.X + (float)r.Width * 0.76f;
		float num2 = (float)r.Y + (float)r.Height * 0.36f;
		float num3 = (float)r.Height * 0.26f;
		Brume(g, num, num2, num3 * 3.2f, num3 * 3.2f, Color.FromArgb(255, 170, 70), 110);
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 238, 190)))
		{
			g.FillEllipse(brush, num - num3, num2 - num3, num3 * 2f, num3 * 2f);
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(50, 170, 120, 60)))
		{
			g.FillEllipse(brush2, num - num3 * 0.5f, num2 - num3 * 0.3f, num3 * 0.4f, num3 * 0.35f);
			g.FillEllipse(brush2, num + num3 * 0.15f, num2 + num3 * 0.2f, num3 * 0.3f, num3 * 0.28f);
			g.FillEllipse(brush2, num - num3 * 0.1f, num2 - num3 * 0.7f, num3 * 0.22f, num3 * 0.2f);
		}
		Etoiles(g, r, t, 30, 130);
		for (int i = 0; i < 6; i++)
		{
			Brume(g, (float)r.X + Mod((double)(H(i, 131) * (float)r.Width) + t * (double)Zx.P(10), r.Width + 200) - (float)Zx.P(100), (float)r.Bottom - (float)r.Height * 0.12f, (float)r.Width * 0.22f, (float)r.Height * 0.2f, Color.FromArgb(170, 140, 200), 60);
		}
		for (int j = 0; j < 7; j++)
		{
			float num4 = (float)r.X + Mod((double)(H(j, 141) * (float)r.Width) + t * (double)((float)Zx.P(22) + (float)Zx.P(20) * H(j, 142)), r.Width + 60) - (float)Zx.P(30);
			float num5 = (float)r.Y + (float)r.Height * (0.15f + 0.4f * H(j, 143)) + (float)Zx.P(8) * (float)Math.Sin(t * 2.0 + (double)j);
			float num6 = 6f + 6f * H(j, 144);
			float num7 = (float)Math.Sin(t * 14.0 + (double)(j * 2)) * num6 * 0.7f;
			using SolidBrush brush3 = new SolidBrush(Color.FromArgb(235, 14, 6, 20));
			g.FillEllipse(brush3, num4 - num6 * 0.25f, num5 - num6 * 0.3f, num6 * 0.5f, num6 * 0.7f);
			g.FillPolygon(brush3, new PointF[5]
			{
				new PointF(num4 - num6 * 0.2f, num5),
				new PointF(num4 - num6 * 1.3f, num5 - num7),
				new PointF(num4 - num6 * 0.95f, num5 + num6 * 0.15f - num7 * 0.4f),
				new PointF(num4 - num6 * 0.6f, num5 + num6 * 0.05f),
				new PointF(num4 - num6 * 0.35f, num5 + num6 * 0.3f)
			});
			g.FillPolygon(brush3, new PointF[5]
			{
				new PointF(num4 + num6 * 0.2f, num5),
				new PointF(num4 + num6 * 1.3f, num5 - num7),
				new PointF(num4 + num6 * 0.95f, num5 + num6 * 0.15f - num7 * 0.4f),
				new PointF(num4 + num6 * 0.6f, num5 + num6 * 0.05f),
				new PointF(num4 + num6 * 0.35f, num5 + num6 * 0.3f)
			});
		}
		using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(16, 6, 14)))
		{
			g.FillRectangle(brush4, r.X, (float)r.Bottom - (float)r.Height * 0.12f, r.Width, (float)r.Height * 0.12f + 1f);
		}
		for (int k = 0; k < 4; k++)
		{
			float num8 = (float)r.Height * (0.26f + 0.08f * (float)(k % 2));
			float num9 = num8 * 1.25f;
			float num10 = (float)r.X + (float)r.Width * (0.08f + 0.17f * (float)k);
			float num11 = (float)r.Bottom - (float)r.Height * 0.08f - num8;
			double num12 = 0.75 + 0.25 * Math.Sin(t * 9.0 + (double)(k * 3)) * Math.Sin(t * 4.3 + (double)k);
			Brume(g, num10 + num9 / 2f, num11 + num8 * 0.55f, num9, num8, Color.FromArgb(255, 140, 30), (int)(110.0 * num12));
			using (LinearGradientBrush brush5 = new LinearGradientBrush(new RectangleF(num10, num11, num9, num8 + 1f), Color.FromArgb(255, 150, 40), Color.FromArgb(190, 70, 10), 90f))
			{
				g.FillEllipse(brush5, num10, num11, num9, num8);
				g.FillEllipse(brush5, num10 + num9 * 0.2f, num11, num9 * 0.6f, num8);
			}
			using (Pen pen = new Pen(Color.FromArgb(120, 120, 40, 0), 1.2f))
			{
				g.DrawArc(pen, num10 + num9 * 0.2f, num11, num9 * 0.6f, num8, Zx.P(100), Zx.P(160));
				g.DrawArc(pen, num10 + num9 * 0.2f, num11, num9 * 0.6f, num8, -Zx.P(80), Zx.P(160));
			}
			using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(60, 90, 40)))
			{
				g.FillRectangle(brush6, num10 + num9 * 0.46f, num11 - num8 * 0.14f, num9 * 0.08f, num8 * 0.18f);
			}
			using SolidBrush brush7 = new SolidBrush(A((int)(255.0 * num12), Color.FromArgb(255, 230, 90)));
			g.FillPolygon(brush7, new PointF[3]
			{
				new PointF(num10 + num9 * 0.25f, num11 + num8 * 0.45f),
				new PointF(num10 + num9 * 0.4f, num11 + num8 * 0.45f),
				new PointF(num10 + num9 * 0.33f, num11 + num8 * 0.28f)
			});
			g.FillPolygon(brush7, new PointF[3]
			{
				new PointF(num10 + num9 * 0.6f, num11 + num8 * 0.45f),
				new PointF(num10 + num9 * 0.75f, num11 + num8 * 0.45f),
				new PointF(num10 + num9 * 0.67f, num11 + num8 * 0.28f)
			});
			g.FillPolygon(brush7, new PointF[7]
			{
				new PointF(num10 + num9 * 0.28f, num11 + num8 * 0.6f),
				new PointF(num10 + num9 * 0.72f, num11 + num8 * 0.6f),
				new PointF(num10 + num9 * 0.64f, num11 + num8 * 0.76f),
				new PointF(num10 + num9 * 0.56f, num11 + num8 * 0.68f),
				new PointF(num10 + num9 * 0.5f, num11 + num8 * 0.78f),
				new PointF(num10 + num9 * 0.44f, num11 + num8 * 0.68f),
				new PointF(num10 + num9 * 0.36f, num11 + num8 * 0.76f)
			});
		}
		Reflet(g, r, t, 14);
	}

	public static bool EstVip(string id)
	{
		return Array.IndexOf(Ids, id) >= 16;
	}

	public static int PrixPoints(string id)
	{
		if (!(id == "diamant"))
		{
			if (!(id == "neonvip"))
			{
				return 0;
			}
			return 500;
		}
		return 300;
	}

	public static bool Disponible(string id)
	{
		if (!EstVip(id))
		{
			return true;
		}
		if (!Social.Vip)
		{
			return false;
		}
		if (PrixPoints(id) != 0)
		{
			return Social.Possede("ban:" + id);
		}
		return true;
	}

	public static void Etincelle(Graphics g, float x, float y, float s, Color c, int a)
	{
		if (a <= 0)
		{
			return;
		}
		Brume(g, x, y, s * 2.4f, s * 2.4f, c, a / 2);
		using SolidBrush brush = new SolidBrush(A(a, Color.White));
		g.FillPolygon(brush, new PointF[8]
		{
			new PointF(x, y - s),
			new PointF(x + s * 0.18f, y - s * 0.18f),
			new PointF(x + s, y),
			new PointF(x + s * 0.18f, y + s * 0.18f),
			new PointF(x, y + s),
			new PointF(x - s * 0.18f, y + s * 0.18f),
			new PointF(x - s, y),
			new PointF(x - s * 0.18f, y - s * 0.18f)
		});
	}

	private static void Or(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(22, 14, 4), new Color[5]
		{
			OrC,
			Color.FromArgb(255, 140, 30),
			OrClair,
			OrFonce,
			Color.FromArgb(255, 210, 90)
		}, new int[5] { 200, 150, 120, 190, 140 }, 0.22);
		for (int i = 0; i < 3; i++)
		{
			List<PointF> list = new List<PointF>();
			float num = (float)r.Y + (float)r.Height * (0.35f + 0.18f * (float)i);
			for (int j = r.X - Zx.P(10); j <= r.Right + 10; j += Zx.P(8))
			{
				list.Add(new PointF(j, (float)((double)num + (double)r.Height * 0.09 * Math.Sin((double)(j - r.X) / ((double)r.Width * 0.2) + t * (0.7 + 0.2 * (double)i) + (double)(i * 2)))));
			}
			using (Pen pen = new Pen(A(40, OrClair), (float)r.Height * 0.12f))
			{
				pen.LineJoin = LineJoin.Round;
				g.DrawLines(pen, list.ToArray());
			}
			using Pen pen2 = new Pen(A(160, OrClair), 1.4f);
			g.DrawLines(pen2, list.ToArray());
		}
		for (int k = 0; k < 26; k++)
		{
			float num2 = (float)Math.Max(0.0, Math.Sin(t * (1.2 + (double)H(k, 301)) + (double)k * 2.1));
			Etincelle(g, (float)r.X + H(k, 302) * (float)r.Width, (float)r.Y + H(k, 303) * (float)r.Height, 2f + 4f * H(k, 304), OrClair, (int)(255f * num2 * num2));
		}
		Lucioles(g, r, t, 18, 305, OrClair, 0.05f);
		Reflet(g, r, t, 70);
	}

	private static void Prestige(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(10, 9, 14), Color.FromArgb(24, 18, 10));
		float num = (float)r.X + (float)r.Width / 2f;
		float num2 = (float)r.Bottom + (float)r.Height * 0.15f;
		for (int i = 0; i < 25; i++)
		{
			double num3 = Math.PI * (0.06 + 0.88 * (double)i / 24.0);
			float num4 = (float)r.Height * 1.25f;
			int a = (int)(60.0 + 50.0 * Math.Sin(t * 1.5 - (double)i * 0.4));
			using Pen pen = new Pen(A(a, OrC), 1f);
			g.DrawLine(pen, num, num2, num - (float)Math.Cos(num3) * num4 * 1.6f, num2 - (float)Math.Sin(num3) * num4);
		}
		for (int j = 1; j <= 3; j++)
		{
			using Pen pen2 = new Pen(A(110, OrC), 1.2f);
			g.DrawArc(pen2, num - (float)r.Height * 0.4f * (float)j, num2 - (float)r.Height * 0.4f * (float)j, (float)r.Height * 0.8f * (float)j, (float)r.Height * 0.8f * (float)j, Zx.P(180), Zx.P(180));
		}
		using (Pen pen3 = new Pen(A(170, OrC), 1.2f))
		{
			g.DrawRectangle(pen3, r.X + Zx.P(6), r.Y + Zx.P(6), r.Width - Zx.P(13), r.Height - Zx.P(13));
			g.DrawRectangle(pen3, r.X + Zx.P(10), r.Y + Zx.P(10), r.Width - Zx.P(21), r.Height - Zx.P(21));
		}
		using (Font font = new Font("Segoe UI Semibold", Math.Max(Zx.P(9f), Math.Min((float)r.Height * 0.3f, (float)r.Width * 0.075f)), FontStyle.Regular, GraphicsUnit.Pixel))
		{
			string text = "P R E S T I G E";
			SizeF sizeF = g.MeasureString(text, font, 4000, StringFormat.GenericTypographic);
			float num5 = (float)r.X + ((float)r.Width - sizeF.Width) / 2f;
			float y = (float)r.Y + ((float)r.Height - sizeF.Height) / 2f;
			using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(num5, y, sizeF.Width + 1f, sizeF.Height + 1f), OrClair, OrFonce, 90f))
			{
				g.DrawString(text, font, brush, num5, y, StringFormat.GenericTypographic);
			}
			float num6 = (float)(t * 0.3 % 1.4) * (sizeF.Width + 120f) + num5 - 60f;
			using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new RectangleF(num6 - (float)Zx.P(40), y, Zx.P(80), sizeF.Height + 1f), A(0, Color.White), A(0, Color.White), 0f);
			ColorBlend colorBlend = new ColorBlend(3);
			colorBlend.Colors = new Color[3]
			{
				A(0, Color.White),
				A(220, Color.White),
				A(0, Color.White)
			};
			colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };
			linearGradientBrush.InterpolationColors = colorBlend;
			linearGradientBrush.WrapMode = WrapMode.TileFlipX;
			GraphicsState gstate = g.Save();
			g.SetClip(new RectangleF(num6 - (float)Zx.P(40), y, Zx.P(80), sizeF.Height), CombineMode.Intersect);
			g.DrawString(text, font, linearGradientBrush, num5, y, StringFormat.GenericTypographic);
			g.Restore(gstate);
		}
		for (int k = 0; k < 10; k++)
		{
			float num7 = (float)Math.Max(0.0, Math.Sin(t * 1.4 + (double)k * 1.9));
			Etincelle(g, (float)r.X + H(k, 311) * (float)r.Width, (float)r.Y + H(k, 312) * (float)r.Height, 2f + 3f * H(k, 313), OrClair, (int)(220f * num7 * num7));
		}
	}

	private static void Diamant(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(4, 12, 32), new Color[4]
		{
			Cyan,
			Color.FromArgb(120, 200, 255),
			Color.FromArgb(200, 230, 255),
			Violet
		}, new int[4] { 160, 150, 90, 110 }, 0.18);
		for (int i = 0; i < 12; i++)
		{
			float num = 0.45f + 0.55f * H(i, 321);
			float num2 = ((float)r.Height * 0.18f + (float)r.Height * 0.3f * H(i, 322)) * num;
			float dx = (float)r.X + Mod((double)(H(i, 323) * (float)(r.Width + Zx.P(120))) - t * (double)((float)Zx.P(5) + (float)Zx.P(12) * num), r.Width + 120) - (float)Zx.P(60);
			float dy = (float)r.Y + (float)r.Height * (0.12f + 0.76f * H(i, 324)) + 6f * (float)Math.Sin(t * 0.7 + (double)i);
			GraphicsState gstate = g.Save();
			g.TranslateTransform(dx, dy);
			g.RotateTransform((float)(t * 10.0 * (double)(H(i, 325) - 0.5f)));
			PointF[] points = new PointF[5]
			{
				new PointF((0f - num2) * 0.6f, (0f - num2) * 0.25f),
				new PointF((0f - num2) * 0.3f, (0f - num2) * 0.55f),
				new PointF(num2 * 0.3f, (0f - num2) * 0.55f),
				new PointF(num2 * 0.6f, (0f - num2) * 0.25f),
				new PointF(0f, num2 * 0.7f)
			};
			using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(0f - num2, 0f - num2, num2 * 2f, num2 * 2f), A((int)(170f * num), Color.FromArgb(220, 245, 255)), A((int)(80f * num), Color.FromArgb(60, 140, 255)), 60f))
			{
				g.FillPolygon(brush, points);
			}
			using (Pen pen = new Pen(A((int)(220f * num), Color.White), 1.2f))
			{
				pen.LineJoin = LineJoin.Round;
				g.DrawPolygon(pen, points);
				g.DrawLine(pen, (0f - num2) * 0.6f, (0f - num2) * 0.25f, num2 * 0.6f, (0f - num2) * 0.25f);
				g.DrawLine(pen, (0f - num2) * 0.3f, (0f - num2) * 0.55f, 0f, num2 * 0.7f);
				g.DrawLine(pen, num2 * 0.3f, (0f - num2) * 0.55f, 0f, num2 * 0.7f);
			}
			g.Restore(gstate);
		}
		for (int j = 0; j < 22; j++)
		{
			float num3 = (float)Math.Max(0.0, Math.Sin(t * (1.5 + (double)H(j, 326)) + (double)j * 2.3));
			Color c = SelecteurCouleur.FromHsv((t * 60.0 + (double)(j * 40)) % 360.0, 0.5, 1.0);
			Etincelle(g, (float)r.X + H(j, 327) * (float)r.Width, (float)r.Y + H(j, 328) * (float)r.Height, 2f + 4f * H(j, 329), c, (int)(255f * num3 * num3));
		}
		Reflet(g, r, t, 60);
	}

	private static void NeonVip(Graphics g, Rectangle r, double t)
	{
		Fond(g, r, Color.FromArgb(14, 8, 24), Color.FromArgb(24, 10, 30));
		using (Pen pen = new Pen(Color.FromArgb(22, 255, 255, 255), 1f))
		{
			for (int i = r.Y; i < r.Bottom; i += Zx.P(14))
			{
				g.DrawLine(pen, r.X, i, r.Right, i);
				for (int j = r.X + i / 14 % 2 * Zx.P(20); j < r.Right; j += Zx.P(40))
				{
					g.DrawLine(pen, j, i, j, i + Zx.P(14));
				}
			}
		}
		Brume(g, (float)r.X + (float)r.Width / 2f, (float)r.Y + (float)r.Height / 2f, (float)r.Width * 0.45f, (float)r.Height * 0.9f, Color.FromArgb(255, 40, 160), 70);
		int num = ((t % 4.0 > 3.75 && Math.Sin(t * 60.0) > 0.0) ? 70 : 255);
		using (GraphicsPath graphicsPath = new GraphicsPath())
		{
			float emSize = Math.Max(14f, Math.Min((float)r.Height * 0.62f, (float)r.Width * 0.2f));
			graphicsPath.AddString("VIP", new FontFamily("Segoe UI Black"), 0, emSize, new PointF(0f, 0f), StringFormat.GenericTypographic);
			RectangleF bounds = graphicsPath.GetBounds();
			using (Matrix matrix = new Matrix())
			{
				matrix.Translate((float)r.X + ((float)r.Width - bounds.Width) / 2f - bounds.X, (float)r.Y + ((float)r.Height - bounds.Height) / 2f - bounds.Y + (float)r.Height * 0.06f);
				graphicsPath.Transform(matrix);
			}
			Color c = Color.FromArgb(255, 60, 190);
			float[][] array = new float[3][]
			{
				new float[2] { 16f, 18f },
				new float[2] { 9f, 40f },
				new float[2] { 4f, 110f }
			};
			foreach (float[] array2 in array)
			{
				using Pen pen2 = new Pen(A((int)(array2[1] * (float)num / 255f), c), array2[0]);
				pen2.LineJoin = LineJoin.Round;
				g.DrawPath(pen2, graphicsPath);
			}
			using Pen pen3 = new Pen(A(num, Color.FromArgb(255, 220, 245)), 1.6f);
			pen3.LineJoin = LineJoin.Round;
			g.DrawPath(pen3, graphicsPath);
		}
		float num2 = (float)r.X + (float)r.Width / 2f;
		float num3 = (float)r.Y + (float)r.Height * 0.16f;
		float num4 = Math.Max(6f, (float)r.Height * 0.11f);
		PointF[] points = new PointF[7]
		{
			new PointF(num2 - num4 * 1.6f, num3 + num4 * 0.6f),
			new PointF(num2 - num4 * 1.6f, num3 - num4 * 0.4f),
			new PointF(num2 - num4 * 0.8f, num3 + num4 * 0.1f),
			new PointF(num2, num3 - num4 * 0.8f),
			new PointF(num2 + num4 * 0.8f, num3 + num4 * 0.1f),
			new PointF(num2 + num4 * 1.6f, num3 - num4 * 0.4f),
			new PointF(num2 + num4 * 1.6f, num3 + num4 * 0.6f)
		};
		float[][] array3 = new float[2][]
		{
			new float[2] { 8f, 30f },
			new float[2] { 3.5f, 110f }
		};
		foreach (float[] array4 in array3)
		{
			using Pen pen4 = new Pen(A((int)array4[1], Cyan), array4[0]);
			pen4.LineJoin = LineJoin.Round;
			g.DrawLines(pen4, points);
		}
		using (Pen pen5 = new Pen(Color.FromArgb(230, 220, 255, 255), 1.3f))
		{
			pen5.LineJoin = LineJoin.Round;
			g.DrawLines(pen5, points);
		}
		Reflet(g, r, t, 16);
	}

	private static void Signature(Graphics g, Rectangle r, double t)
	{
		Maille(g, r, t, Color.FromArgb(10, 10, 36), new Color[3] { Indigo, Orange, Violet }, new int[3] { 230, 190, 170 }, 0.28);
		float num = Math.Min((float)r.Height * 0.74f / 7f, (float)r.Width * 0.7f / 17f);
		float num2 = (float)r.X + ((float)r.Width - num * 17f) / 2f;
		float num3 = (float)r.Y + ((float)r.Height - num * 7f) / 2f;
		double num4 = t % 11.0;
		int num5 = 0;
		for (int i = 0; i < 3; i++)
		{
			string[] array = i switch
			{
				1 => gP, 
				0 => gT, 
				_ => gS, 
			};
			for (int j = 0; j < 7; j++)
			{
				for (int k = 0; k < 5; k++)
				{
					if (array[j][k] != '1')
					{
						continue;
					}
					num5++;
					double num6 = (double)H(num5, 0) * 1.5 + (double)k * 0.03;
					double num7 = ((num4 < 3.2) ? Clamp((num4 - num6) / 1.3) : ((num4 < 8.6) ? 1.0 : (1.0 - Clamp((num4 - Zx.P(8.6) - num6 * 0.5) / 1.3))));
					if (num7 <= 0.001)
					{
						continue;
					}
					double num8 = 1.0 - Math.Pow(1.0 - num7, 3.0);
					float num9 = num2 + (float)(i * 6 + k) * num;
					float num10 = num3 + (float)j * num;
					float num11 = num9 + (H(num5, 1) - 0.5f) * (float)r.Width * 1.1f * (1f - (float)num8);
					float num12 = num10 + (H(num5, 2) - 0.5f) * (float)r.Height * 1.5f * (1f - (float)num8);
					float num13 = num * (0.45f + 0.5f * (float)num8);
					float num14 = ((num4 > 3.2 && num4 < 8.6) ? (0.5f + 0.5f * (float)Math.Sin(t * 2.4 - (double)num9 * 0.035)) : 0f);
					Color c = Mel(Color.FromArgb(245, 247, 255), Color.FromArgb(255, 210, 160), (double)num14 * 0.5);
					Brume(g, num11 + num13 / 2f, num12 + num13 / 2f, num13 * 1.5f, num13 * 1.5f, Orange, (int)(70.0 * num7));
					using GraphicsPath path = Bouton.Rond(new Rectangle((int)num11 + 1, (int)num12 + 1, (int)num13 - Zx.P(2), (int)num13 - Zx.P(2)), Math.Max(1, (int)(num * 0.2f)));
					using (SolidBrush brush = new SolidBrush(A((int)(235.0 * num7), c)))
					{
						g.FillPath(brush, path);
					}
					using Pen pen = new Pen(A((int)(150.0 * num7), Color.White), 1f);
					g.DrawPath(pen, path);
				}
			}
		}
		Lucioles(g, r, t, 16, 5, Color.FromArgb(255, 220, 190), 0.05f);
		Reflet(g, r, t, 34);
	}
}
