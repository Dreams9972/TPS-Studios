using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace TPSStudios;

internal class ThemeNoel : ThemeBase
{
	private class Bloc
	{
		public float X;

		public float Y;

		public float Y0;

		public float VX;

		public float VY;

		public float S;

		public float Vie = 1f;

		public float Rot;

		public float VRot;

		public bool Pose;

		public bool Poudre;
	}

	private class Cadeau
	{
		public float X0;

		public float Y0;

		public float X1;

		public float Y1;

		public float T;

		public float Vie = 8f;

		public Color C;

		public bool Sol;
	}

	private const double Cycle = 9.0;

	private const double Vol = 9.0;

	private Bitmap foret;

	private int fw0;

	private int fh0;

	public static double TempsForce = -1.0;

	private Bitmap village;

	private int vw;

	private int vh;

	private List<PointF> chem = new List<PointF>();

	private List<RectangleF> fen = new List<RectangleF>();

	private float sol;

	public static float AlphaBlocs = 1f;

	private double lancer = -10.0;

	private int cible = -1;

	private bool lache;

	private double prochain = 2.0;

	private List<Bloc> blocs = new List<Bloc>();

	private List<Cadeau> cadeaux = new List<Cadeau>();

	private List<float[]> bouffees = new List<float[]>();

	private HashSet<int> lances = new HashSet<int>();

	private static readonly Random rnd = new Random();

	public override bool NeigeTuiles => Page == 0;

	public static double Temps
	{
		get
		{
			if (!(TempsForce >= 0.0))
			{
				return BanAnim.Temps;
			}
			return TempsForce;
		}
	}

	public override bool EnChute
	{
		get
		{
			foreach (Bloc bloc in blocs)
			{
				if (!bloc.Pose && !bloc.Poudre && bloc.Y < bloc.Y0 + 70f)
				{
					return true;
				}
			}
			return false;
		}
	}

	public ThemeNoel(int page)
		: base(page)
	{
	}

	private void Foret(Graphics g, int w, int h)
	{
		int num = Math.Max(Zx.P(90), Math.Min(Zx.P(140), h / 6));
		if (foret == null || fw0 != w || fh0 != num)
		{
			if (foret != null)
			{
				foret.Dispose();
			}
			fw0 = w;
			fh0 = num;
			foret = new Bitmap(w, num, PixelFormat.Format32bppPArgb);
			Random random = new Random(5);
			using Graphics graphics = Graphics.FromImage(foret);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			for (int i = 0; i < 2; i++)
			{
				for (int j = -Zx.P(10) + i * Zx.P(20); j < w + 20; j += Zx.P(26) + random.Next(26))
				{
					float num2 = ((i == 0) ? Zx.P(50) : Zx.P(70)) + random.Next(30);
					float num3 = num - ((i == 0) ? Zx.P(26) : Zx.P(8));
					Color color = ((i == 0) ? Color.FromArgb(20, 60, 60) : Color.FromArgb(18, 80, 62));
					for (int k = 0; k < 3; k++)
					{
						float num4 = num3 - num2 + (float)k * num2 * 0.28f;
						float num5 = num2 * (0.2f + 0.1f * (float)k);
						using (SolidBrush brush = new SolidBrush(color))
						{
							graphics.FillPolygon(brush, new PointF[3]
							{
								new PointF(j, num4),
								new PointF((float)j + num5, num4 + num2 * 0.36f),
								new PointF((float)j - num5, num4 + num2 * 0.36f)
							});
						}
						using SolidBrush brush2 = new SolidBrush(Color.FromArgb(235, 245, 250, 255));
						graphics.FillEllipse(brush2, (float)j - num5, num4 + num2 * 0.33f, num5 * 2f, Zx.P(4));
					}
				}
			}
			using (LinearGradientBrush brush3 = new LinearGradientBrush(new Rectangle(0, num - Zx.P(14), w, Zx.P(15)), Color.FromArgb(250, 252, 255), Color.FromArgb(200, 214, 238), 90f))
			{
				graphics.FillRectangle(brush3, 0, num - Zx.P(14), w, Zx.P(14));
			}
			float num6 = w - Zx.P(120);
			float num7 = num - Zx.P(14);
			using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(248, 250, 255)))
			{
				graphics.FillEllipse(brush4, num6 - (float)Zx.P(22), num7 - (float)Zx.P(40), Zx.P(44), Zx.P(42));
				graphics.FillEllipse(brush4, num6 - (float)Zx.P(16), num7 - (float)Zx.P(70), Zx.P(32), Zx.P(32));
				graphics.FillEllipse(brush4, num6 - (float)Zx.P(11), num7 - (float)Zx.P(92), Zx.P(22), Zx.P(22));
			}
			using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(30, 30, 40)))
			{
				graphics.FillEllipse(brush5, num6 - (float)Zx.P(5), num7 - (float)Zx.P(86), Zx.P(3), Zx.P(3));
				graphics.FillEllipse(brush5, num6 + (float)Zx.P(2), num7 - (float)Zx.P(86), Zx.P(3), Zx.P(3));
				graphics.FillRectangle(brush5, num6 - (float)Zx.P(12), num7 - (float)Zx.P(104), Zx.P(24), Zx.P(4));
				graphics.FillRectangle(brush5, num6 - (float)Zx.P(8), num7 - (float)Zx.P(118), Zx.P(16), Zx.P(15));
				for (int l = 0; l < 3; l++)
				{
					graphics.FillEllipse(brush5, num6 - (float)Zx.P(2), num7 - (float)Zx.P(64) + (float)(l * Zx.P(9)), Zx.P(4), Zx.P(4));
				}
			}
			using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(240, 120, 30)))
			{
				graphics.FillPolygon(brush6, new PointF[3]
				{
					new PointF(num6, num7 - (float)Zx.P(82)),
					new PointF(num6 + (float)Zx.P(12), num7 - (float)Zx.P(80)),
					new PointF(num6, num7 - (float)Zx.P(78))
				});
			}
			using (Pen pen = new Pen(Color.FromArgb(210, 50, 50), Zx.P(4f)))
			{
				graphics.DrawLine(pen, num6 - (float)Zx.P(12), num7 - (float)Zx.P(72), num6 + (float)Zx.P(12), num7 - (float)Zx.P(72));
			}
			using Pen pen2 = new Pen(Color.FromArgb(100, 70, 40), Zx.P(2f));
			graphics.DrawLine(pen2, num6 - (float)Zx.P(14), num7 - (float)Zx.P(56), num6 - (float)Zx.P(34), num7 - (float)Zx.P(70));
			graphics.DrawLine(pen2, num6 + (float)Zx.P(14), num7 - (float)Zx.P(56), num6 + (float)Zx.P(34), num7 - (float)Zx.P(72));
		}
		g.DrawImageUnscaled(foret, 0, h - num);
	}

	private static void Guirlande(Graphics g, int w, double t, float y0)
	{
		Color[] array = new Color[4]
		{
			Color.FromArgb(255, 70, 70),
			Color.FromArgb(255, 200, 60),
			Color.FromArgb(70, 230, 120),
			Color.FromArgb(80, 170, 255)
		};
		int num = Math.Max(10, w / 40);
		PointF[] array2 = new PointF[num + 1];
		float num2 = (float)Math.Sin(t * 1.3) * 3f;
		for (int i = 0; i <= num; i++)
		{
			float num3 = (float)i / (float)num;
			float num4 = num3 * 3f % 1f;
			ref PointF reference = ref array2[i];
			reference = new PointF(num3 * (float)w, y0 + (float)(Zx.P(14) * 4) * num4 * (1f - num4) + num2 * num4);
		}
		using (Pen pen = new Pen(Color.FromArgb(170, 30, 40, 50), 1.5f))
		{
			g.DrawCurve(pen, array2);
		}
		for (int j = 1; j < num; j++)
		{
			PointF pointF = array2[j];
			Color color = array[j % 4];
			int num5 = ((Math.Sin(t * 3.0 + (double)j * 0.9) > -0.3) ? 255 : 90);
			Lueur(g, pointF.X, pointF.Y + (float)Zx.P(5), Zx.P(12), Zx.P(12), color, num5 / 2);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(num5, color));
			g.FillEllipse(brush, pointF.X - (float)Zx.P(3), pointF.Y + 1f, Zx.P(6), Zx.P(9));
		}
	}

	public static bool Actif()
	{
		string text = Environment.GetEnvironmentVariable("TPS_THEME");
		if (string.IsNullOrEmpty(text))
		{
			text = Config.Lire("theme") ?? "auto";
		}
		if (text == "noel")
		{
			return true;
		}
		if (text == "aucun")
		{
			return false;
		}
		DateTime now = DateTime.Now;
		if (now.Month != 12)
		{
			if (now.Month == 1)
			{
				return now.Day <= 6;
			}
			return false;
		}
		return true;
	}

	private static float H(int i, int k)
	{
		return BanAnim.H(i, k);
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

	private static void Lueur(Graphics g, float cx, float cy, float rx, float ry, Color c, int alpha)
	{
		BanAnim.Brume(g, cx, cy, rx, ry, c, alpha);
	}

	public static int HautVillage(int h)
	{
		return Math.Max(120, Math.Min(180, h / 5));
	}

	private void Construire(int w, int h)
	{
		int num = HautVillage(h);
		if (village != null && vw == w && vh == num)
		{
			return;
		}
		if (village != null)
		{
			village.Dispose();
		}
		vw = w;
		vh = num;
		chem.Clear();
		fen.Clear();
		village = new Bitmap(w, num, PixelFormat.Format32bppPArgb);
		Random random = new Random(7);
		using Graphics graphics = Graphics.FromImage(village);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		List<PointF> list = new List<PointF>();
		for (int i = -Zx.P(10); i <= w + 10; i += Zx.P(10))
		{
			list.Add(new PointF(i, (float)((double)num * 0.34 + (double)Zx.P(12) * Math.Sin((double)i / 150.0) + 7.0 * Math.Sin((double)i / 47.0 + 1.0))));
		}
		list.Add(new PointF(w + Zx.P(10), num));
		list.Add(new PointF(-Zx.P(10), num));
		using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, num + 1), Color.FromArgb(200, 214, 236), Color.FromArgb(120, 140, 190), 90f))
		{
			graphics.FillPolygon(brush, list.ToArray());
		}
		for (int j = Zx.P(6); j < w; j += Zx.P(22) + random.Next(30))
		{
			float num2 = Zx.P(18) + random.Next(16);
			float num3 = j;
			float num4 = (float)((double)num * 0.4 + (double)Zx.P(12) * Math.Sin((double)j / 150.0));
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(40, 70, 90)))
			{
				graphics.FillPolygon(brush2, new PointF[3]
				{
					new PointF(num3, num4 - num2),
					new PointF(num3 + num2 * 0.35f, num4),
					new PointF(num3 - num2 * 0.35f, num4)
				});
			}
			using SolidBrush brush3 = new SolidBrush(Color.FromArgb(220, 235, 245, 255));
			graphics.FillEllipse(brush3, num3 - (float)Zx.P(2), num4 - num2 - 1f, Zx.P(4), Zx.P(3));
		}
		sol = (float)num * 0.8f;
		Color[] array = new Color[5]
		{
			Color.FromArgb(70, 46, 54),
			Color.FromArgb(44, 56, 92),
			Color.FromArgb(86, 54, 46),
			Color.FromArgb(46, 70, 76),
			Color.FromArgb(78, 66, 96)
		};
		int num5 = 4 + random.Next(30);
		while (num5 < w - 40)
		{
			int num6 = 56 + random.Next(44);
			int num7 = 30 + random.Next(26);
			int num8 = (int)((float)num6 * 0.42f);
			int num9 = (int)sol - num7;
			Color baseColor = array[random.Next(array.Length)];
			using (LinearGradientBrush brush4 = new LinearGradientBrush(new Rectangle(num5, num9, num6, num7 + 1), ControlPaint.Light(baseColor, 0.15f), ControlPaint.Dark(baseColor, 0.1f), 0f))
			{
				graphics.FillRectangle(brush4, num5, num9, num6, num7);
			}
			float num10 = (float)num5 + (float)num6 * (0.62f + 0.12f * (float)random.NextDouble());
			float num11 = (float)num8 / ((float)num6 / 2f + 6f);
			float num12 = (float)(num9 - num8) + num11 * Math.Abs(num10 - ((float)num5 + (float)num6 / 2f));
			float num13 = num12 - 16f;
			using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(110, 56, 48)))
			{
				graphics.FillRectangle(brush5, num10 - (float)Zx.P(5), num13, Zx.P(10), num12 - num13 + (float)Zx.P(6));
			}
			using (Pen pen = new Pen(Color.FromArgb(70, 30, 26)))
			{
				graphics.DrawLine(pen, num10 - (float)Zx.P(5), num13 + (float)Zx.P(6), num10 + (float)Zx.P(5), num13 + (float)Zx.P(6));
				graphics.DrawLine(pen, num10 - (float)Zx.P(5), num13 + (float)Zx.P(11), num10 + (float)Zx.P(5), num13 + (float)Zx.P(11));
			}
			using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(245, 248, 255)))
			{
				graphics.FillEllipse(brush6, num10 - (float)Zx.P(7), num13 - (float)Zx.P(3), Zx.P(14), Zx.P(6));
			}
			chem.Add(new PointF(num10, num13 - 1f));
			PointF[] points = new PointF[3]
			{
				new PointF(num5 - Zx.P(7), num9 + Zx.P(2)),
				new PointF((float)num5 + (float)num6 / 2f, num9 - num8),
				new PointF(num5 + num6 + Zx.P(7), num9 + Zx.P(2))
			};
			using (LinearGradientBrush brush7 = new LinearGradientBrush(new RectangleF(num5, num9 - num8, num6, num8 + Zx.P(3)), Color.FromArgb(255, 255, 255), Color.FromArgb(200, 214, 238), 90f))
			{
				graphics.FillPolygon(brush7, points);
			}
			using (Pen pen2 = new Pen(Color.FromArgb(120, 140, 175), 1.4f))
			{
				graphics.DrawLine(pen2, num5 - Zx.P(7), num9 + Zx.P(2), num5 + num6 + Zx.P(7), num9 + Zx.P(2));
			}
			for (int k = 0; k < 4; k++)
			{
				using SolidBrush brush8 = new SolidBrush(Color.FromArgb(200, 225, 240, 255));
				float num14 = (float)num5 + (float)num6 * (0.1f + 0.25f * (float)k);
				graphics.FillPolygon(brush8, new PointF[3]
				{
					new PointF(num14 - (float)Zx.P(2), num9 + Zx.P(2)),
					new PointF(num14 + (float)Zx.P(2), num9 + Zx.P(2)),
					new PointF(num14, num9 + Zx.P(7) + random.Next(5))
				});
			}
			int num15 = ((num6 <= 76) ? 1 : 2);
			for (int l = 0; l < num15; l++)
			{
				RectangleF rectangleF = new RectangleF((float)num5 + ((num15 == 1) ? ((float)num6 * 0.22f) : ((float)num6 * (0.14f + 0.5f * (float)l))), (float)num9 + (float)num7 * 0.22f, Zx.P(12), Zx.P(11));
				fen.Add(rectangleF);
				using (SolidBrush brush9 = new SolidBrush(Color.FromArgb(255, 206, 120)))
				{
					graphics.FillRectangle(brush9, rectangleF);
				}
				using Pen pen3 = new Pen(Color.FromArgb(90, 50, 30), 1.2f);
				graphics.DrawRectangle(pen3, rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
				graphics.DrawLine(pen3, rectangleF.X + rectangleF.Width / 2f, rectangleF.Y, rectangleF.X + rectangleF.Width / 2f, rectangleF.Bottom);
				graphics.DrawLine(pen3, rectangleF.X, rectangleF.Y + rectangleF.Height / 2f, rectangleF.Right, rectangleF.Y + rectangleF.Height / 2f);
			}
			using (SolidBrush brush10 = new SolidBrush(Color.FromArgb(60, 36, 28)))
			{
				graphics.FillRectangle(brush10, (float)num5 + (float)num6 * 0.68f - ((num15 == 1) ? 0f : ((float)num6 * 0.3f)) + ((num15 == 1) ? 0f : ((float)num6 * 0.3f)), sol - (float)num7 * 0.5f, Zx.P(10), (float)num7 * 0.5f);
			}
			using (Pen pen4 = new Pen(Color.FromArgb(40, 140, 70), Zx.P(2f)))
			{
				graphics.DrawEllipse(pen4, (float)num5 + (float)num6 * 0.68f + 1.5f, sol - (float)num7 * 0.5f + (float)Zx.P(3), Zx.P(7), Zx.P(7));
			}
			num5 += num6 + 26 + random.Next(48);
			if (random.Next(3) != 0 || num5 >= w - 30)
			{
				continue;
			}
			float num16 = num5 - Zx.P(14);
			float num17 = Zx.P(26) + random.Next(10);
			for (int m = 0; m < 3; m++)
			{
				float num18 = sol - num17 + (float)m * num17 * 0.28f;
				float num19 = num17 * (0.22f + 0.1f * (float)m);
				using (SolidBrush brush11 = new SolidBrush(Color.FromArgb(28, 90, 64)))
				{
					graphics.FillPolygon(brush11, new PointF[3]
					{
						new PointF(num16, num18),
						new PointF(num16 + num19, num18 + num17 * 0.36f),
						new PointF(num16 - num19, num18 + num17 * 0.36f)
					});
				}
				using SolidBrush brush12 = new SolidBrush(Color.FromArgb(235, 245, 250, 255));
				graphics.FillEllipse(brush12, num16 - num19, num18 + num17 * 0.33f, num19 * 2f, Zx.P(3.5f));
			}
		}
		List<PointF> list2 = new List<PointF>();
		for (int n = -Zx.P(10); n <= w + 10; n += Zx.P(8))
		{
			list2.Add(new PointF(n, (float)((double)sol + 3.0 * Math.Sin((double)n / 37.0) + 2.0 * Math.Sin((double)n / 13.0))));
		}
		list2.Add(new PointF(w + Zx.P(10), num + 1));
		list2.Add(new PointF(-Zx.P(10), num + 1));
		using (LinearGradientBrush brush13 = new LinearGradientBrush(new RectangleF(0f, sol - (float)Zx.P(4), w, (float)num - sol + (float)Zx.P(6)), Color.FromArgb(250, 252, 255), Color.FromArgb(196, 210, 236), 90f))
		{
			graphics.FillPolygon(brush13, list2.ToArray());
		}
		using SolidBrush brush14 = new SolidBrush(Color.FromArgb(70, 120, 140, 200));
		for (int num20 = Zx.P(20); num20 < w; num20 += Zx.P(70))
		{
			graphics.FillEllipse(brush14, (float)num20 + (float)random.NextDouble() * (float)Zx.P(30), sol + (float)Zx.P(8) + (float)random.NextDouble() * (float)Zx.P(10), Zx.P(40), Zx.P(4));
		}
	}

	public override void Deneiger(Rectangle tuile)
	{
		float num = (float)tuile.Left + (float)tuile.Width / 2f;
		for (int i = 0; i < 64; i++)
		{
			Bloc bloc = new Bloc();
			bool flag = i % 2 == 0;
			bloc.X = (flag ? ((float)(tuile.Left + 2) + (float)rnd.NextDouble() * 26f) : ((float)(tuile.Right - 2) - (float)rnd.NextDouble() * 26f));
			bloc.Y = (float)(tuile.Top - 2) - (float)rnd.NextDouble() * 9f;
			bloc.Y0 = bloc.Y;
			_ = (bloc.X - num) / Math.Max(1f, (float)tuile.Width / 2f);
			bloc.VX = (float)((!flag) ? 1 : (-1)) * (60f + (float)rnd.NextDouble() * 230f);
			bloc.VY = -40f - (float)rnd.NextDouble() * 130f;
			bloc.S = 2.5f + (float)rnd.NextDouble() * 6.5f;
			bloc.Rot = (float)rnd.NextDouble() * 360f;
			bloc.VRot = (float)(rnd.NextDouble() - 0.5) * 520f;
			blocs.Add(bloc);
		}
		for (int j = 0; j < 70; j++)
		{
			Bloc bloc2 = new Bloc();
			bloc2.Poudre = true;
			bloc2.X = (float)tuile.Left + (float)rnd.NextDouble() * (float)tuile.Width;
			bloc2.Y = (float)tuile.Top - (float)rnd.NextDouble() * 10f;
			bloc2.Y0 = bloc2.Y;
			bloc2.VX = (float)(rnd.NextDouble() - 0.5) * 90f;
			bloc2.VY = -70f - (float)rnd.NextDouble() * 110f;
			bloc2.S = 1.4f + (float)rnd.NextDouble() * 2.2f;
			bloc2.Vie = 0.9f + (float)rnd.NextDouble() * 0.4f;
			blocs.Add(bloc2);
		}
	}

	public override void Avancer(double dt, int w, int h)
	{
		if (w < 10 || h < 10)
		{
			return;
		}
		double temps = Temps;
		if (Page == 0)
		{
			Construire(w, h);
			float num = (float)(h - vh) + sol - 2f;
			for (int num2 = blocs.Count - 1; num2 >= 0; num2--)
			{
				Bloc bloc = blocs[num2];
				if (bloc.Poudre)
				{
					bloc.VY += (float)(260.0 * dt);
					bloc.VX *= (float)Math.Pow(0.25, dt);
					bloc.X += (float)((double)bloc.VX * dt);
					bloc.Y += (float)((double)bloc.VY * dt);
					bloc.Vie -= (float)(dt * 1.1);
					if (bloc.Vie <= 0f)
					{
						blocs.RemoveAt(num2);
					}
				}
				else if (!bloc.Pose)
				{
					bloc.VY += (float)(1500.0 * dt);
					bloc.VX *= (float)Math.Pow(0.55, dt);
					bloc.X += (float)((double)bloc.VX * dt);
					bloc.Y += (float)((double)bloc.VY * dt);
					bloc.Rot += (float)((double)bloc.VRot * dt);
					if (bloc.Y >= num)
					{
						bloc.Y = num + (float)rnd.NextDouble() * 4f;
						bloc.Pose = true;
					}
				}
				else
				{
					bloc.Vie -= (float)(dt / 6.0);
					if (bloc.Vie <= 0f)
					{
						blocs.RemoveAt(num2);
					}
				}
			}
		}
		if (Page == 2)
		{
			return;
		}
		int num3 = (int)Math.Floor(temps / 9.0);
		double ph = temps - (double)num3 * 9.0;
		Traineau(w, h, ph, temps, num3, out var x, out var y, out var _, out var sens);
		double num4 = temps - lancer;
		if (num4 >= 0.5249999999999999 && !lache && num4 < 0.75)
		{
			lache = true;
			Main(0.7, out var hx, out var hy);
			float num5 = x + (float)sens * hx;
			float y2 = y + hy;
			Cadeau cadeau = new Cadeau();
			cadeau.X0 = num5;
			cadeau.Y0 = y2;
			cadeau.T = 0f;
			Color[] array = new Color[5]
			{
				Color.FromArgb(220, 50, 60),
				Color.FromArgb(60, 160, 90),
				Color.FromArgb(70, 120, 230),
				Color.FromArgb(230, 180, 50),
				Color.FromArgb(170, 80, 220)
			};
			cadeau.C = array[(num3 + cadeaux.Count) % array.Length];
			if (Page == 0 && cible >= 0 && cible < chem.Count)
			{
				cadeau.X1 = chem[cible].X;
				cadeau.Y1 = (float)(h - vh) + chem[cible].Y;
			}
			else
			{
				cadeau.X1 = num5 + (float)sens * (60f + 140f * (float)rnd.NextDouble());
				cadeau.Y1 = (float)(h - 20) - 18f * (float)rnd.NextDouble();
				cadeau.Sol = true;
			}
			cadeaux.Add(cadeau);
		}
		bool flag = num4 > 1.05;
		if (flag && Page == 0 && chem.Count > 0)
		{
			for (int i = 0; i < chem.Count; i++)
			{
				if (H(i, num3) > 0.6f)
				{
					continue;
				}
				int item = num3 * 1000 + i;
				if (!lances.Contains(item))
				{
					float num6 = (float)sens * (chem[i].X - x);
					if (num6 > 70f && num6 < 230f)
					{
						lances.Add(item);
						cible = i;
						lancer = temps;
						lache = false;
						break;
					}
				}
			}
		}
		if (flag && Page == 1 && temps >= prochain && x > 60f && x < (float)(w - 60))
		{
			cible = -1;
			lancer = temps;
			lache = false;
			prochain = temps + 1.6 + 2.2 * rnd.NextDouble();
		}
		if (lances.Count > 400)
		{
			lances.Clear();
		}
		for (int num7 = cadeaux.Count - 1; num7 >= 0; num7--)
		{
			Cadeau cadeau2 = cadeaux[num7];
			if (cadeau2.T < 1f)
			{
				cadeau2.T = Math.Min(1f, cadeau2.T + (float)(dt / 1.0));
				if (cadeau2.T >= 1f && !cadeau2.Sol)
				{
					bouffees.Add(new float[3] { cadeau2.X1, cadeau2.Y1, 0f });
					cadeaux.RemoveAt(num7);
				}
			}
			else
			{
				cadeau2.Vie -= (float)dt;
				if (cadeau2.Vie <= 0f)
				{
					cadeaux.RemoveAt(num7);
				}
			}
		}
		for (int num8 = bouffees.Count - 1; num8 >= 0; num8--)
		{
			bouffees[num8][2] += (float)dt;
			if (bouffees[num8][2] > 0.7f)
			{
				bouffees.RemoveAt(num8);
			}
		}
	}

	private void Traineau(int w, int h, double ph, double t, int cyc, out float x, out float y, out float inc, out int sens)
	{
		int num = Page * 17;
		double num2 = ph / 9.0;
		sens = ((H(cyc, 501 + num) > 0.5f) ? 1 : (-1));
		float num3 = ((Page == 0) ? 0.3f : 0.22f);
		float num4 = (float)h * (0.05f + num3 * H(cyc, 502 + num));
		float num5 = (float)h * (0.05f + num3 * H(cyc, 503 + num));
		float num6 = 10f + 30f * H(cyc, 504 + num);
		float num7 = 0.6f + 1.6f * H(cyc, 505 + num);
		double num8 = num2 * num2 * (3.0 - 2.0 * num2);
		double num9 = num2 * Math.PI * 2.0 * (double)num7 + (double)cyc;
		x = (float)((sens > 0) ? ((double)(-Zx.P(260)) + num2 * (double)(w + Zx.P(520))) : ((double)(w + Zx.P(260)) - num2 * (double)(w + Zx.P(520))));
		y = (float)((double)num4 + (double)(num5 - num4) * num8 + (double)num6 * Math.Sin(num9) + 4.0 * Math.Sin(t * 3.1));
		double num10 = (double)((num5 - num4) * 6f) * num2 * (1.0 - num2) + (double)num6 * Math.Cos(num9) * Math.PI * 2.0 * (double)num7;
		double num11 = w + Zx.P(520);
		inc = (float)Math.Max(-20.0, Math.Min(20.0, Math.Atan(num10 / num11) * 180.0 / Math.PI * (double)sens));
	}

	private static void Main(double p, out float hx, out float hy)
	{
		double num = ((p < 0.0) ? 25.0 : ((p < 0.35) ? (25.0 + 150.0 * Doux(p / 0.35)) : ((p < 0.6) ? (175.0 + 65.0 * Doux((p - 0.35) / 0.25)) : ((p < 0.8) ? (240.0 + 90.0 * Doux((p - 0.6) / 0.2)) : ((!(p < 1.0)) ? 25.0 : (330.0 + 55.0 * Doux((p - 0.8) / 0.2)))))));
		double num2 = num * Math.PI / 180.0;
		hx = (float)((double)(-Zx.P(6)) + (double)Zx.P(14) * Math.Cos(num2));
		hy = (float)((double)(-Zx.P(14)) + (double)Zx.P(14) * Math.Sin(num2));
	}

	private static double Doux(double u)
	{
		u = Math.Max(0.0, Math.Min(1.0, u));
		return u * u * (3.0 - 2.0 * u);
	}

	public override void PeindreFond(Graphics g, int w, int h)
	{
		if (w < 10 || h < 10)
		{
			return;
		}
		double temps = Temps;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (Page == 2)
		{
			Guirlande(g, w, temps, -Zx.P(6));
			return;
		}
		int num = (int)Math.Floor(temps / 9.0);
		double ph = temps - (double)num * 9.0;
		Traineau(w, h, ph, temps, num, out var x, out var y, out var inc, out var sens);
		double num2 = temps - lancer;
		PereNoel(g, x, y, temps, (num2 >= 0.0 && num2 < 0.75) ? (num2 / 0.75) : (-1.0), inc, sens);
		foreach (Cadeau item in cadeaux)
		{
			float t = item.T;
			float dx = item.X0 + (item.X1 - item.X0) * t;
			float dy = item.Y0 + (item.Y1 - item.Y0) * t * t - (float)Zx.P(40) * t * (1f - t);
			float num3 = (item.Sol ? 9f : (9f - 3f * t));
			int alpha = (int)(255f * Math.Max(0f, Math.Min(1f, item.Vie / 2f)));
			GraphicsState gstate = g.Save();
			g.TranslateTransform(dx, dy);
			g.RotateTransform((item.T < 1f) ? (t * 300f * (float)sens) : 8f);
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, item.C)))
			{
				g.FillRectangle(brush, (0f - num3) / 2f, (0f - num3) / 2f, num3, num3);
			}
			using (Pen pen = new Pen(Color.FromArgb(alpha, 255, 235, 150), 1.6f))
			{
				g.DrawLine(pen, 0f, (0f - num3) / 2f, 0f, num3 / 2f);
				g.DrawLine(pen, (0f - num3) / 2f, 0f, num3 / 2f, 0f);
			}
			g.Restore(gstate);
		}
		if (Page == 1)
		{
			Foret(g, w, h);
			Guirlande(g, w, temps, -Zx.P(2));
			return;
		}
		Construire(w, h);
		int num4 = h - vh;
		g.DrawImageUnscaled(village, 0, num4);
		for (int i = 0; i < fen.Count; i++)
		{
			RectangleF rectangleF = fen[i];
			Lueur(g, rectangleF.X + rectangleF.Width / 2f, (float)num4 + rectangleF.Y + rectangleF.Height / 2f, Zx.P(16), Zx.P(14), Color.FromArgb(255, 190, 90), (int)(70.0 + 40.0 * Math.Sin(temps * (1.2 + (double)H(i, 3)) + (double)i)));
		}
		for (int j = 0; j < chem.Count; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				float num5 = Mod(temps * 0.3 + (double)H(j, 5) + (double)k / 4.0, 1.0);
				float num6 = chem[j].X + (float)Zx.P(10) * num5 + 3f * (float)Math.Sin(num5 * 5f + (float)j);
				float num7 = (float)num4 + chem[j].Y - (float)Zx.P(4) - num5 * (float)Zx.P(46);
				float num8 = 3f + num5 * 9f;
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)(80f * (1f - num5)), 200, 205, 220));
				g.FillEllipse(brush2, num6 - num8, num7 - num8, num8 * 2f, num8 * 2f);
			}
		}
		foreach (float[] bouffee in bouffees)
		{
			float num9 = bouffee[2] / 0.7f;
			for (int l = 0; l < 7; l++)
			{
				double num10 = (double)l * 0.9;
				float num11 = 4f + num9 * 16f;
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb((int)(220f * (1f - num9)), 255, 230, 150));
				g.FillEllipse(brush3, bouffee[0] + (float)Math.Cos(num10) * num11 - 1.5f, bouffee[1] + 0f - (float)Math.Abs(Math.Sin(num10)) * num11 - 1.5f, Zx.P(3), Zx.P(3));
			}
		}
	}

	public override void PeindreNeige(Graphics g, int w, int h, double densite)
	{
		if (w < 10 || h < 10)
		{
			return;
		}
		double temps = Temps;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int num = (int)((double)((Page == 2) ? 40 : 130) * Math.Max(0.0, Math.Min(1.0, densite)));
		for (int i = 0; i < num; i++)
		{
			float num2 = 0.35f + 0.65f * ((float)(i % 3) / 2f);
			float num3 = 18f + 34f * num2;
			float x = Mod((double)(H(i, 201) * (float)w) + (double)Zx.P(22) * Math.Sin(temps * (0.5 + (double)H(i, 202)) + (double)i), w);
			float y = Mod((double)(H(i, 203) * (float)(h + Zx.P(10))) + temps * (double)num3, h + 10) - (float)Zx.P(5);
			float num4 = 1.4f + 2.8f * num2 * (0.5f + H(i, 204));
			using SolidBrush brush = new SolidBrush(Color.FromArgb((int)(110f + 140f * num2), 255, 255, 255));
			g.FillEllipse(brush, x, y, num4, num4);
		}
		foreach (Bloc bloc in blocs)
		{
			int num5 = (int)(255f * Math.Max(0f, Math.Min(1f, bloc.Vie)) * AlphaBlocs);
			if (num5 <= 2)
			{
				continue;
			}
			if (bloc.Poudre)
			{
				using (SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)((double)num5 * 0.75), 240, 246, 255)))
				{
					g.FillEllipse(brush2, bloc.X - bloc.S / 2f, bloc.Y - bloc.S / 2f, bloc.S, bloc.S);
				}
				continue;
			}
			if (bloc.Pose)
			{
				using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(num5, 248, 251, 255)))
				{
					g.FillEllipse(brush3, bloc.X - bloc.S, bloc.Y - bloc.S * 0.35f, bloc.S * 2f, bloc.S * 0.7f);
				}
				continue;
			}
			float num6 = (float)((double)bloc.Rot * Math.PI / 180.0);
			float num7 = (float)Math.Cos(num6);
			float num8 = (float)Math.Sin(num6);
			float num9 = bloc.S * 0.62f;
			float num10 = bloc.S * 0.42f;
			PointF[] array = new PointF[6];
			for (int j = 0; j < 6; j++)
			{
				double num11 = (double)j * Math.PI / 3.0;
				float num12 = ((j % 2 == 0) ? num9 : num10);
				float num13 = (float)Math.Cos(num11) * num12;
				float num14 = (float)Math.Sin(num11) * num12 * 0.8f;
				ref PointF reference = ref array[j];
				reference = new PointF(bloc.X + num13 * num7 - num14 * num8, bloc.Y + num13 * num8 + num14 * num7);
			}
			using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(num5 / 3, 120, 145, 210)))
			{
				PointF[] array2 = new PointF[6];
				for (int k = 0; k < 6; k++)
				{
					ref PointF reference2 = ref array2[k];
					reference2 = new PointF(array[k].X + 1.2f, array[k].Y + 1.4f);
				}
				g.FillClosedCurve(brush4, array2, FillMode.Winding, 0.4f);
			}
			using SolidBrush brush5 = new SolidBrush(Color.FromArgb(num5, 250, 252, 255));
			g.FillClosedCurve(brush5, array, FillMode.Winding, 0.4f);
		}
	}

	private static void PereNoel(Graphics g, float x0, float y0, double t, double geste, float inc, int sens)
	{
		GraphicsState gstate = g.Save();
		g.TranslateTransform(x0, y0);
		g.RotateTransform(inc);
		g.ScaleTransform(sens, 1f);
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < 14; i++)
		{
			float num3 = num - (float)Zx.P(60) - (float)(i * Zx.P(12));
			float num4 = num2 + (float)Zx.P(10) + (float)Math.Sin(t * 4.0 + (double)i) * 3f;
			int num5 = 200 - i * 14;
			if (num5 <= 0)
			{
				break;
			}
			Lueur(g, num3, num4, Zx.P(5), Zx.P(5), Color.FromArgb(255, 230, 160), num5 / 2);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(num5, 255, 245, 210));
			g.FillEllipse(brush, num3 - 1.2f, num4 - 1.2f, Zx.P(2.4f), Zx.P(2.4f));
		}
		for (int j = 0; j < 3; j++)
		{
			float num6 = num + 34f + (float)(j * 36);
			float num7 = num2 - 4f + (float)Math.Sin(t * 9.0 + (double)j) * 2.5f;
			float num8 = (float)Math.Sin(t * 14.0 + (double)j * 1.7);
			using (Pen pen = new Pen(Color.FromArgb(230, 196, 150, 60), 1.2f))
			{
				g.DrawLine(pen, (j == 0) ? (num + (float)Zx.P(18)) : (num6 - (float)Zx.P(36) + (float)Zx.P(22)), (j == 0) ? (num2 + (float)Zx.P(2)) : (num7 - (float)Zx.P(2)), num6 + (float)Zx.P(2), num7 + 1f);
			}
			using (Pen pen2 = new Pen(Color.FromArgb(105, 70, 45), Zx.P(2f)))
			{
				pen2.StartCap = LineCap.Round;
				pen2.EndCap = LineCap.Round;
				g.DrawLine(pen2, num6 + (float)Zx.P(4), num7 + (float)Zx.P(4), num6 + 1f - num8 * 5f, num7 + (float)Zx.P(13));
				g.DrawLine(pen2, num6 + (float)Zx.P(9), num7 + (float)Zx.P(4), num6 + (float)Zx.P(12) + num8 * 5f, num7 + (float)Zx.P(12));
				g.DrawLine(pen2, num6 + (float)Zx.P(18), num7 + (float)Zx.P(4), num6 + (float)Zx.P(16) - num8 * 5f, num7 + (float)Zx.P(13));
				g.DrawLine(pen2, num6 + (float)Zx.P(22), num7 + (float)Zx.P(3), num6 + (float)Zx.P(26) + num8 * 5f, num7 + (float)Zx.P(11));
			}
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(132, 88, 54)))
			{
				g.FillEllipse(brush2, num6, num7 - (float)Zx.P(3), Zx.P(26), Zx.P(10));
				g.FillEllipse(brush2, num6 + (float)Zx.P(21), num7 - (float)Zx.P(10), Zx.P(6), Zx.P(10));
				g.FillEllipse(brush2, num6 + (float)Zx.P(23), num7 - (float)Zx.P(13), Zx.P(10), Zx.P(7));
			}
			using (Pen pen3 = new Pen(Color.FromArgb(200, 170, 120), 1.2f))
			{
				g.DrawLine(pen3, num6 + (float)Zx.P(25), num7 - (float)Zx.P(12), num6 + (float)Zx.P(23), num7 - (float)Zx.P(19));
				g.DrawLine(pen3, num6 + (float)Zx.P(24), num7 - (float)Zx.P(16), num6 + (float)Zx.P(20), num7 - (float)Zx.P(18));
				g.DrawLine(pen3, num6 + (float)Zx.P(28), num7 - (float)Zx.P(12), num6 + (float)Zx.P(30), num7 - (float)Zx.P(19));
				g.DrawLine(pen3, num6 + (float)Zx.P(29), num7 - (float)Zx.P(16), num6 + (float)Zx.P(33), num7 - (float)Zx.P(18));
			}
			if (j == 2)
			{
				Lueur(g, num6 + (float)Zx.P(33), num7 - (float)Zx.P(9), Zx.P(7), Zx.P(7), Color.FromArgb(255, 60, 60), 220);
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb(255, 70, 70));
				g.FillEllipse(brush3, num6 + (float)Zx.P(31), num7 - (float)Zx.P(11), Zx.P(4.5f), Zx.P(4.5f));
			}
		}
		using (GraphicsPath graphicsPath = new GraphicsPath())
		{
			graphicsPath.AddBezier(num - 40f, num2 - 4f, num - 38f, num2 + 10f, num - 24f, num2 + 12f, num + 4f, num2 + 12f);
			graphicsPath.AddBezier(num + 4f, num2 + 12f, num + 16f, num2 + 12f, num + 20f, num2 + 6f, num + 18f, num2 - 2f);
			graphicsPath.AddLine(num + 18f, num2 - 2f, num - 6f, num2 - 2f);
			graphicsPath.AddLine(num - 6f, num2 - 2f, num - 10f, num2 - 8f);
			graphicsPath.AddLine(num - 10f, num2 - 8f, num - 40f, num2 - 4f);
			graphicsPath.CloseFigure();
			using (LinearGradientBrush brush4 = new LinearGradientBrush(new RectangleF(num - (float)Zx.P(40), num2 - (float)Zx.P(8), Zx.P(60), Zx.P(21)), Color.FromArgb(225, 50, 55), Color.FromArgb(150, 20, 30), 90f))
			{
				g.FillPath(brush4, graphicsPath);
			}
			using Pen pen4 = new Pen(Color.FromArgb(240, 200, 90), 1.4f);
			g.DrawPath(pen4, graphicsPath);
		}
		using (Pen pen5 = new Pen(Color.FromArgb(240, 205, 100), Zx.P(2f)))
		{
			g.DrawBezier(pen5, num - (float)Zx.P(44), num2 + (float)Zx.P(16), num - (float)Zx.P(20), num2 + (float)Zx.P(19), num + (float)Zx.P(10), num2 + (float)Zx.P(19), num + (float)Zx.P(24), num2 + (float)Zx.P(12));
			g.DrawLine(pen5, num - (float)Zx.P(26), num2 + (float)Zx.P(12), num - (float)Zx.P(26), num2 + (float)Zx.P(17));
			g.DrawLine(pen5, num + (float)Zx.P(2), num2 + (float)Zx.P(12), num + (float)Zx.P(2), num2 + (float)Zx.P(18));
		}
		using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(150, 100, 60)))
		{
			g.FillEllipse(brush5, num - (float)Zx.P(38), num2 - (float)Zx.P(20), Zx.P(20), Zx.P(18));
		}
		using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(220, 60, 70)))
		{
			g.FillRectangle(brush6, num - (float)Zx.P(34), num2 - (float)Zx.P(22), Zx.P(6), Zx.P(5));
		}
		using (SolidBrush brush7 = new SolidBrush(Color.FromArgb(205, 30, 40)))
		{
			g.FillEllipse(brush7, num - (float)Zx.P(18), num2 - (float)Zx.P(18), Zx.P(18), Zx.P(18));
		}
		using (SolidBrush brush8 = new SolidBrush(Color.FromArgb(40, 30, 30)))
		{
			g.FillRectangle(brush8, num - (float)Zx.P(18), num2 - (float)Zx.P(9), Zx.P(18), Zx.P(3));
		}
		using (SolidBrush brush9 = new SolidBrush(Color.FromArgb(250, 210, 180)))
		{
			g.FillEllipse(brush9, num - (float)Zx.P(13), num2 - (float)Zx.P(27), Zx.P(10), Zx.P(10));
		}
		using (SolidBrush brush10 = new SolidBrush(Color.White))
		{
			g.FillEllipse(brush10, num - (float)Zx.P(14), num2 - (float)Zx.P(21), Zx.P(12), Zx.P(9));
			g.FillEllipse(brush10, num - (float)Zx.P(15), num2 - (float)Zx.P(28), Zx.P(13), Zx.P(4));
		}
		using (SolidBrush brush11 = new SolidBrush(Color.FromArgb(205, 30, 40)))
		{
			g.FillPolygon(brush11, new PointF[3]
			{
				new PointF(num - (float)Zx.P(15), num2 - (float)Zx.P(27)),
				new PointF(num - (float)Zx.P(2), num2 - (float)Zx.P(27)),
				new PointF(num - (float)Zx.P(16), num2 - (float)Zx.P(37))
			});
		}
		using (SolidBrush brush12 = new SolidBrush(Color.White))
		{
			g.FillEllipse(brush12, num - (float)Zx.P(19), num2 - (float)Zx.P(39), Zx.P(5), Zx.P(5));
		}
		float hx;
		float hy;
		if (geste < 0.0)
		{
			hx = Zx.P(8);
			hy = (float)(-Zx.P(6)) + (float)Math.Sin(t * 6.0) * 3f;
		}
		else
		{
			Main(geste, out hx, out hy);
		}
		using (Pen pen6 = new Pen(Color.FromArgb(205, 30, 40), Zx.P(3.2f)))
		{
			pen6.StartCap = LineCap.Round;
			pen6.EndCap = LineCap.Round;
			g.DrawLine(pen6, num - (float)Zx.P(6), num2 - (float)Zx.P(14), num + hx, num2 + hy);
		}
		using (SolidBrush brush13 = new SolidBrush(Color.FromArgb(250, 250, 250)))
		{
			g.FillEllipse(brush13, num + hx - Zx.P(2.2f), num2 + hy - Zx.P(2.2f), Zx.P(4.4f), Zx.P(4.4f));
		}
		if (geste >= 0.3 && geste < 0.7)
		{
			using (SolidBrush brush14 = new SolidBrush(Color.FromArgb(70, 120, 230)))
			{
				g.FillRectangle(brush14, num + hx - (float)Zx.P(4), num2 + hy - (float)Zx.P(9), Zx.P(8), Zx.P(8));
			}
			using Pen pen7 = new Pen(Color.FromArgb(255, 235, 150), 1.4f);
			g.DrawLine(pen7, num + hx, num2 + hy - (float)Zx.P(9), num + hx, num2 + hy - 1f);
			g.DrawLine(pen7, num + hx - (float)Zx.P(4), num2 + hy - (float)Zx.P(5), num + hx + (float)Zx.P(4), num2 + hy - (float)Zx.P(5));
		}
		g.Restore(gstate);
	}
}
