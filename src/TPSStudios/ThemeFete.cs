using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace TPSStudios;

internal class ThemeFete : ThemeBase
{
	private struct PointF3
	{
		public float X;

		public float Y;

		public float Z;

		public PointF3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}

	private const double PH = 7.0;

	private readonly string id;

	private Bitmap decor;

	private int dw;

	private int dh;

	private Bitmap lumiere;

	private int lumW;

	private Bitmap scene;

	private int sw0;

	private int sh0;

	private Bitmap banTexte;

	private string banCle;

	private static readonly Font fBan = new Font("Segoe UI Black", Zx.P(13f), GraphicsUnit.Pixel);

	private Bitmap[] neons = new Bitmap[2];

	private int Wl = 1280;

	private int Hl = 760;

	private Bitmap[] tickets;

	private Dictionary<int, Bitmap> ticketsTournes = new Dictionary<int, Bitmap>();

	private Bitmap tables;

	private int tabW;

	private static PointF3[] sphere;

	private Bitmap foule;

	private int fouleW;

	private Color[] Palette => id switch
	{
		"nouvelan" => new Color[4]
		{
			Color.FromArgb(255, 210, 90),
			Color.FromArgb(255, 240, 200),
			Color.FromArgb(190, 150, 255),
			Color.FromArgb(120, 220, 255)
		}, 
		"valentin" => new Color[4]
		{
			Color.FromArgb(255, 80, 130),
			Color.FromArgb(255, 150, 180),
			Color.FromArgb(220, 40, 90),
			Color.FromArgb(255, 210, 225)
		}, 
		"carnaval" => new Color[5]
		{
			Color.FromArgb(255, 70, 90),
			Color.FromArgb(255, 200, 40),
			Color.FromArgb(60, 200, 120),
			Color.FromArgb(70, 150, 255),
			Color.FromArgb(200, 90, 255)
		}, 
		"paques" => new Color[4]
		{
			Color.FromArgb(255, 190, 215),
			Color.FromArgb(255, 240, 150),
			Color.FromArgb(170, 230, 190),
			Color.FromArgb(190, 200, 255)
		}, 
		"ete" => new Color[3]
		{
			Color.FromArgb(0, 85, 164),
			Color.FromArgb(245, 245, 250),
			Color.FromArgb(239, 65, 53)
		}, 
		"halloween" => new Color[4]
		{
			Color.FromArgb(240, 120, 30),
			Color.FromArgb(200, 70, 20),
			Color.FromArgb(230, 170, 40),
			Color.FromArgb(140, 60, 30)
		}, 
		_ => new Color[3]
		{
			Color.FromArgb(255, 200, 70),
			Color.FromArgb(255, 235, 160),
			Color.FromArgb(40, 40, 46)
		}, 
	};

	public override bool ATeinte => true;

	public override bool Secoue
	{
		get
		{
			if (Page == 0)
			{
				return id == "carnaval";
			}
			return false;
		}
	}

	public ThemeFete(string id, int page)
		: base(page)
	{
		this.id = id;
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

	private static Color A(int a, Color c)
	{
		return Color.FromArgb(Math.Max(0, Math.Min(255, a)), c);
	}

	public override void Avancer(double dt, int w, int h)
	{
	}

	public override void Teinter(Graphics g, int w, int h)
	{
		Ambiance(g, w, h);
	}

	private void Lumiere(Graphics g, int w, int h)
	{
		if (id != "ete" && id != "halloween")
		{
			return;
		}
		int num = ((id == "ete") ? (w - 120) : 150);
		int num2 = ((id == "ete") ? 40 : 320);
		int num3 = ((id == "ete") ? 260 : 150);
		int num4 = ((id == "ete") ? 200 : 150);
		if (lumiere == null || lumW != w)
		{
			if (lumiere != null)
			{
				lumiere.Dispose();
			}
			lumiere = new Bitmap(num3 * 2, num4 * 2, PixelFormat.Format32bppPArgb);
			lumW = w;
			using Graphics graphics = Graphics.FromImage(lumiere);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (id == "ete")
			{
				BanAnim.Brume(graphics, num3, num4, num3, num4, Color.FromArgb(255, 220, 120), 70);
			}
			else
			{
				BanAnim.Brume(graphics, num3, num4, num3, num4, Color.FromArgb(255, 170, 70), 90);
				using SolidBrush brush = new SolidBrush(Color.FromArgb(255, 238, 190));
				graphics.FillEllipse(brush, num3 - Zx.P(45), num4 - Zx.P(45), Zx.P(90), Zx.P(90));
			}
		}
		g.DrawImageUnscaled(lumiere, num - num3, num2 - num4);
	}

	private void Ambiance(Graphics g, int w, int h)
	{
		Color color;
		Color color2;
		switch (id)
		{
		case "nouvelan":
			color = Color.FromArgb(150, 34, 12, 70);
			color2 = Color.FromArgb(120, 6, 6, 26);
			break;
		case "valentin":
			color = Color.FromArgb(120, 130, 24, 80);
			color2 = Color.FromArgb(110, 46, 8, 44);
			break;
		case "carnaval":
			color = Color.FromArgb(120, 80, 20, 120);
			color2 = Color.FromArgb(110, 10, 80, 100);
			break;
		case "paques":
			color = Color.FromArgb(120, 120, 190, 255);
			color2 = Color.FromArgb(90, 255, 220, 190);
			break;
		case "ete":
			color = Color.FromArgb(110, 255, 150, 70);
			color2 = Color.FromArgb(110, 40, 90, 170);
			break;
		case "halloween":
			color = Color.FromArgb(140, 40, 8, 54);
			color2 = Color.FromArgb(120, 120, 44, 10);
			break;
		default:
			color = Color.FromArgb(170, 0, 0, 0);
			color2 = Color.FromArgb(140, 50, 36, 6);
			break;
		}
		using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, h + 1), color, color2, 90f);
		g.FillRectangle(brush, 0, 0, w, h);
	}

	private void SceneJeux(Graphics g, int w, int h)
	{
		int num = Math.Max(Zx.P(130), Math.Min(Zx.P(200), h / 4));
		if (scene == null || sw0 != w || sh0 != num)
		{
			if (scene != null)
			{
				scene.Dispose();
			}
			sw0 = w;
			sh0 = num;
			scene = new Bitmap(w, num, PixelFormat.Format32bppPArgb);
			Random random = new Random(23);
			using Graphics graphics = Graphics.FromImage(scene);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			float num2 = num - 10;
			switch (id)
			{
			case "nouvelan":
			{
				for (int m = -Zx.P(10); m < w; m += Zx.P(50) + random.Next(40))
				{
					int num7 = 30 + random.Next(50);
					using SolidBrush brush6 = new SolidBrush(Color.FromArgb(18, 16, 40));
					graphics.FillRectangle(brush6, m, num2 - (float)num7, Zx.P(48), num7 + Zx.P(10));
				}
				float num8 = w - Zx.P(170);
				float num9 = Zx.P(70);
				float num10 = num - Zx.P(20);
				using (SolidBrush brush7 = new SolidBrush(Color.FromArgb(26, 22, 52)))
				{
					graphics.FillRectangle(brush7, num8, num2 - num10 + (float)Zx.P(30), num9, num10);
					graphics.FillPolygon(brush7, new PointF[3]
					{
						new PointF(num8 - (float)Zx.P(6), num2 - num10 + (float)Zx.P(30)),
						new PointF(num8 + num9 / 2f, num2 - num10 - (float)Zx.P(10)),
						new PointF(num8 + num9 + (float)Zx.P(6), num2 - num10 + (float)Zx.P(30))
					});
				}
				float num11 = num8 + num9 / 2f;
				float num12 = num2 - num10 + 62f;
				float num13 = 24f;
				using (SolidBrush brush8 = new SolidBrush(Color.FromArgb(255, 236, 190)))
				{
					graphics.FillEllipse(brush8, num11 - num13, num12 - num13, num13 * 2f, num13 * 2f);
				}
				using (Pen pen4 = new Pen(Color.FromArgb(40, 30, 20), Zx.P(3f)))
				{
					graphics.DrawLine(pen4, num11, num12, num11, num12 - num13 * 0.8f);
					graphics.DrawLine(pen4, num11, num12, num11 + 1f, num12 - num13 * 0.6f);
				}
				for (int n = 0; n < 12; n++)
				{
					double num14 = (double)n * Math.PI / 6.0;
					using SolidBrush brush9 = new SolidBrush(Color.FromArgb(60, 40, 20));
					graphics.FillEllipse(brush9, num11 + (float)Math.Cos(num14) * num13 * 0.85f - 1.5f, num12 + (float)Math.Sin(num14) * num13 * 0.85f - 1.5f, Zx.P(3), Zx.P(3));
				}
				break;
			}
			case "valentin":
			{
				using (LinearGradientBrush brush14 = new LinearGradientBrush(new Rectangle(0, (int)((float)num * 0.55f), w, (int)((float)num * 0.45f) + 1), Color.FromArgb(120, 60, 110), Color.FromArgb(60, 20, 60), 90f))
				{
					graphics.FillRectangle(brush14, 0f, (float)num * 0.55f, w, (float)num * 0.45f);
				}
				using (Pen pen6 = new Pen(Color.FromArgb(70, 40, 70), Zx.P(6f)))
				{
					graphics.DrawArc(pen6, (float)w * 0.1f, (float)num * 0.35f, (float)w * 0.8f, (float)num * 0.9f, Zx.P(200), Zx.P(140));
				}
				using (Pen pen7 = new Pen(Color.FromArgb(150, 110, 140), Zx.P(2f)))
				{
					for (int num19 = (int)((float)w * 0.15f); (float)num19 < (float)w * 0.85f; num19 += Zx.P(8))
					{
						graphics.DrawLine(pen7, num19, (float)num * 0.42f, num19, (float)num * 0.58f);
					}
				}
				for (int num20 = 0; num20 < 40; num20++)
				{
					float cx = (float)w * (0.16f + 0.68f * (float)random.NextDouble());
					float cy = (float)num * (0.44f + 0.12f * (float)random.NextDouble());
					using SolidBrush b = new SolidBrush(Color.FromArgb(240, 180 + random.Next(70), 60 + random.Next(80), 100 + random.Next(80)));
					BanAnim.Coeur(graphics, cx, cy, 3.2f, b);
				}
				break;
			}
			case "carnaval":
			{
				for (int k = 0; k < Math.Max(3, w / 260); k++)
				{
					float num3 = 80 + k * (w - 160) / Math.Max(1, Math.Max(3, w / 260) - 1);
					float num4 = (float)num * 0.5f;
					float num5 = 90f;
					float num6 = 50f;
					Color color = Palette[k % Palette.Length];
					using (GraphicsPath graphicsPath = new GraphicsPath())
					{
						graphicsPath.AddBezier(num3 - num5 / 2f, num4, num3 - num5 / 2f, num4 - num6 * 0.8f, num3 + num5 / 2f, num4 - num6 * 0.8f, num3 + num5 / 2f, num4);
						graphicsPath.AddBezier(num3 + num5 / 2f, num4, num3 + num5 * 0.3f, num4 + num6 * 0.6f, num3 + 6f, num4 + num6 * 0.3f, num3, num4 + num6 * 0.1f);
						graphicsPath.AddBezier(num3, num4 + num6 * 0.1f, num3 - 6f, num4 + num6 * 0.3f, num3 - num5 * 0.3f, num4 + num6 * 0.6f, num3 - num5 / 2f, num4);
						using (LinearGradientBrush brush4 = new LinearGradientBrush(new RectangleF(num3 - num5 / 2f, num4 - num6, num5, num6 * 1.6f), ControlPaint.Light(color, 0.3f), color, 45f))
						{
							graphics.FillPath(brush4, graphicsPath);
						}
						using Pen pen2 = new Pen(Color.FromArgb(240, 210, 90), Zx.P(2f));
						graphics.DrawPath(pen2, graphicsPath);
					}
					using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(20, 10, 30)))
					{
						graphics.FillEllipse(brush5, num3 - (float)Zx.P(30), num4 - (float)Zx.P(18), Zx.P(22), Zx.P(12));
						graphics.FillEllipse(brush5, num3 + (float)Zx.P(8), num4 - (float)Zx.P(18), Zx.P(22), Zx.P(12));
					}
					for (int l = 0; l < 3; l++)
					{
						using Pen pen3 = new Pen(Palette[(k + l + 1) % Palette.Length], Zx.P(3f));
						graphics.DrawBezier(pen3, num3 + num5 / 2f - (float)Zx.P(6), num4 - num6 * 0.5f, num3 + num5 / 2f + (float)Zx.P(20), num4 - num6 - (float)Zx.P(10) - (float)(l * Zx.P(8)), num3 + num5 / 2f + (float)Zx.P(40), num4 - num6 - (float)(l * 6), num3 + num5 / 2f + (float)Zx.P(50) + (float)(l * 6), num4 - num6 * 1.3f - (float)(l * Zx.P(10)));
					}
				}
				break;
			}
			case "paques":
			{
				using (SolidBrush brush17 = new SolidBrush(Color.FromArgb(110, 190, 100)))
				{
					graphics.FillRectangle(brush17, 0f, num2 - (float)Zx.P(10), w, Zx.P(20));
				}
				for (int num25 = Zx.P(10); num25 < w; num25 += Zx.P(16) + random.Next(14))
				{
					float num26 = Zx.P(30) + random.Next(40);
					Color color2 = Palette[random.Next(Palette.Length)];
					using (Pen pen10 = new Pen(Color.FromArgb(70, 150, 70), Zx.P(2.4f)))
					{
						graphics.DrawLine(pen10, num25, num2, num25, num2 - num26);
					}
					using SolidBrush brush18 = new SolidBrush(color2);
					graphics.FillEllipse(brush18, num25 - Zx.P(6), num2 - num26 - (float)Zx.P(12), Zx.P(12), Zx.P(16));
					graphics.FillPolygon(brush18, new PointF[5]
					{
						new PointF(num25 - Zx.P(6), num2 - num26 - (float)Zx.P(6)),
						new PointF(num25 - Zx.P(4), num2 - num26 - (float)Zx.P(16)),
						new PointF(num25, num2 - num26 - (float)Zx.P(8)),
						new PointF(num25 + Zx.P(4), num2 - num26 - (float)Zx.P(16)),
						new PointF(num25 + Zx.P(6), num2 - num26 - (float)Zx.P(6))
					});
				}
				float num27 = w - Zx.P(200);
				float num28 = num2 - (float)Zx.P(40);
				using (SolidBrush brush19 = new SolidBrush(Color.FromArgb(170, 110, 50)))
				{
					graphics.FillPie(brush19, num27, num28 - (float)Zx.P(20), Zx.P(110), Zx.P(80), 0f, Zx.P(180));
				}
				using (Pen pen11 = new Pen(Color.FromArgb(140, 90, 40), Zx.P(4f)))
				{
					graphics.DrawArc(pen11, num27 + (float)Zx.P(10), num28 - (float)Zx.P(50), Zx.P(90), Zx.P(80), Zx.P(180), Zx.P(180));
				}
				for (int num29 = 0; num29 < 5; num29++)
				{
					using SolidBrush brush20 = new SolidBrush(Palette[num29 % Palette.Length]);
					graphics.FillEllipse(brush20, num27 + (float)Zx.P(10) + (float)(num29 * Zx.P(18)), num28 + (float)Zx.P(4) - (float)(num29 % 2 * 6), Zx.P(16), Zx.P(22));
				}
				break;
			}
			case "ete":
			{
				using (LinearGradientBrush brush15 = new LinearGradientBrush(new Rectangle(0, (int)(num2 - (float)Zx.P(20)), w, Zx.P(31)), Color.FromArgb(240, 214, 160), Color.FromArgb(214, 180, 120), 90f))
				{
					graphics.FillRectangle(brush15, 0f, num2 - (float)Zx.P(20), w, Zx.P(30));
				}
				float[] array2 = new float[3]
				{
					80f,
					(float)w * 0.45f,
					w - 110
				};
				foreach (float num21 in array2)
				{
					using (Pen pen8 = new Pen(Color.FromArgb(120, 80, 40), Zx.P(7f)))
					{
						graphics.DrawBezier(pen8, num21, num2 - (float)Zx.P(18), num21 + (float)Zx.P(6), num2 - (float)Zx.P(60), num21 - (float)Zx.P(10), num2 - (float)Zx.P(110), num21 + (float)Zx.P(8), (float)num * 0.12f);
					}
					for (int num22 = 0; num22 < 6; num22++)
					{
						double num23 = Math.PI + (double)num22 * Math.PI / 5.0;
						using Pen pen9 = new Pen(Color.FromArgb(40, 140, 70), Zx.P(5f));
						pen9.EndCap = LineCap.Triangle;
						graphics.DrawBezier(pen9, num21 + (float)Zx.P(8), (float)num * 0.12f, num21 + (float)Zx.P(8) + (float)Math.Cos(num23) * (float)Zx.P(30), (float)num * 0.12f - (float)Zx.P(18), num21 + (float)Zx.P(8) + (float)Math.Cos(num23) * (float)Zx.P(60), (float)num * 0.12f, num21 + (float)Zx.P(8) + (float)Math.Cos(num23) * (float)Zx.P(70), (float)num * 0.12f + (float)Zx.P(26) * (float)Math.Abs(Math.Sin(num23)) + (float)Zx.P(10));
					}
				}
				float x = (float)w * 0.7f;
				float y = num2 - (float)Zx.P(34);
				for (int num24 = 0; num24 < 6; num24++)
				{
					using SolidBrush brush16 = new SolidBrush((num24 % 3 == 0) ? Color.FromArgb(0, 85, 164) : ((num24 % 3 == 1) ? Color.White : Color.FromArgb(239, 65, 53)));
					graphics.FillPie(brush16, x, y, Zx.P(30), Zx.P(30), num24 * Zx.P(60), Zx.P(60));
				}
				break;
			}
			case "halloween":
			{
				using (SolidBrush brush10 = new SolidBrush(Color.FromArgb(20, 8, 22)))
				{
					graphics.FillRectangle(brush10, 0f, num2 - (float)Zx.P(8), w, Zx.P(20));
				}
				float num15 = (float)w * 0.5f - (float)Zx.P(90);
				float num16 = num2 - (float)Zx.P(120);
				using (SolidBrush brush11 = new SolidBrush(Color.FromArgb(28, 12, 30)))
				{
					graphics.FillRectangle(brush11, num15, num16 + (float)Zx.P(30), Zx.P(180), Zx.P(90));
					graphics.FillPolygon(brush11, new PointF[3]
					{
						new PointF(num15 - (float)Zx.P(14), num16 + (float)Zx.P(32)),
						new PointF(num15 + (float)Zx.P(90), num16 - (float)Zx.P(30)),
						new PointF(num15 + (float)Zx.P(194), num16 + (float)Zx.P(32))
					});
					graphics.FillRectangle(brush11, num15 + (float)Zx.P(130), num16 - (float)Zx.P(20), Zx.P(26), Zx.P(40));
					graphics.FillPolygon(brush11, new PointF[3]
					{
						new PointF(num15 + (float)Zx.P(124), num16 - (float)Zx.P(18)),
						new PointF(num15 + (float)Zx.P(143), num16 - (float)Zx.P(52)),
						new PointF(num15 + (float)Zx.P(162), num16 - (float)Zx.P(18))
					});
				}
				PointF[] array = new PointF[4]
				{
					new PointF(num15 + (float)Zx.P(24), num16 + (float)Zx.P(50)),
					new PointF(num15 + (float)Zx.P(130), num16 + (float)Zx.P(50)),
					new PointF(num15 + (float)Zx.P(78), num16 + (float)Zx.P(4)),
					new PointF(num15 + (float)Zx.P(136), num16 - (float)Zx.P(8))
				};
				for (int num17 = 0; num17 < array.Length; num17++)
				{
					PointF pointF = array[num17];
					BanAnim.Brume(graphics, pointF.X + 10f, pointF.Y + 10f, 26f, 22f, Color.FromArgb(255, 160, 40), 120);
					using (SolidBrush brush12 = new SolidBrush(Color.FromArgb(255, 200, 70)))
					{
						graphics.FillRectangle(brush12, pointF.X, pointF.Y, Zx.P(20), Zx.P(20));
					}
					using Pen pen5 = new Pen(Color.FromArgb(28, 12, 30), Zx.P(2f));
					graphics.DrawLine(pen5, pointF.X + (float)Zx.P(10), pointF.Y, pointF.X + (float)Zx.P(10), pointF.Y + (float)Zx.P(20));
					graphics.DrawLine(pen5, pointF.X, pointF.Y + (float)Zx.P(10), pointF.X + (float)Zx.P(20), pointF.Y + (float)Zx.P(10));
				}
				using (SolidBrush brush13 = new SolidBrush(Color.FromArgb(60, 30, 20)))
				{
					graphics.FillRectangle(brush13, num15 + (float)Zx.P(80), num16 + (float)Zx.P(80), Zx.P(22), Zx.P(40));
				}
				for (int num18 = Zx.P(30); num18 < w; num18 += Zx.P(200) + random.Next(100))
				{
					Arbre(graphics, num18, num2 - (float)Zx.P(40), 60f, random);
				}
				break;
			}
			default:
			{
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(22, 22, 26)))
				{
					graphics.FillRectangle(brush, 0, Zx.P(10), w, num - Zx.P(10));
				}
				for (int i = Zx.P(20); i < w; i += Zx.P(170))
				{
					using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(i, Zx.P(30), Zx.P(150), num - Zx.P(40)), Color.FromArgb(70, 60, 30), Color.FromArgb(30, 26, 14), 90f))
					{
						graphics.FillRectangle(brush2, i, Zx.P(30), Zx.P(150), num - Zx.P(40));
					}
					using (Pen pen = new Pen(Color.FromArgb(220, 190, 70), Zx.P(2f)))
					{
						graphics.DrawRectangle(pen, i, Zx.P(30), Zx.P(150), num - Zx.P(40));
					}
					using (Font font = new Font("Segoe UI Black", Zx.P(15f), GraphicsUnit.Pixel))
					{
						TextRenderer.DrawText(graphics, "-" + (30 + 10 * random.Next(5)) + "%", font, new Rectangle(i, Zx.P(40), Zx.P(150), Zx.P(30)), Color.FromArgb(255, 210, 80), TextFormatFlags.HorizontalCenter);
					}
					for (int j = 0; j < 3; j++)
					{
						using SolidBrush brush3 = new SolidBrush(Color.FromArgb(50, 50, 58));
						graphics.FillRectangle(brush3, i + Zx.P(18) + j * Zx.P(44), num - Zx.P(60), Zx.P(28), Zx.P(40));
					}
				}
				break;
			}
			}
		}
		g.DrawImageUnscaled(scene, 0, h - sh0);
	}

	private void HautJeux(Graphics g, int w, int h, double t)
	{
		switch (id)
		{
		case "carnaval":
			Suspendu(g, w, t, -Zx.P(2), 16f);
			break;
		case "ete":
		{
			BanAnim.Brume(g, w - 90, 30f, 160f, 160f, Color.FromArgb(255, 220, 120), 120);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(255, 236, 150));
			g.FillEllipse(brush, w - Zx.P(130), -Zx.P(10), Zx.P(80), Zx.P(80));
			break;
		}
		case "halloween":
		{
			using (Pen pen4 = new Pen(Color.FromArgb(120, 220, 220, 230), 1f))
			{
				for (int j = 0; j < 6; j++)
				{
					double num3 = (double)j * Math.PI / 10.0;
					g.DrawLine(pen4, w, 0f, (float)w - (float)Math.Cos(num3) * (float)Zx.P(160), (float)Math.Sin(num3) * (float)Zx.P(160));
				}
				for (int k = 30; k <= 150; k += 30)
				{
					g.DrawArc(pen4, w - k, -k, k * 2, k * 2, Zx.P(90), Zx.P(90));
				}
			}
			float num4 = (float)Zx.P(60) + (float)Zx.P(40) * (float)Math.Sin(t * 0.8);
			using (Pen pen5 = new Pen(Color.FromArgb(150, 220, 220, 230), 1f))
			{
				g.DrawLine(pen5, w - Zx.P(90), 0f, w - Zx.P(90), num4);
			}
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(20, 10, 20)))
			{
				g.FillEllipse(brush2, w - Zx.P(97), num4, Zx.P(14), Zx.P(16));
			}
			using Pen pen6 = new Pen(Color.FromArgb(20, 10, 20), 1.6f);
			for (int l = 0; l < 4; l++)
			{
				g.DrawLine(pen6, w - Zx.P(90), num4 + (float)Zx.P(4) + (float)(l * 3), w - Zx.P(106), num4 + (float)(l * 5));
				g.DrawLine(pen6, w - Zx.P(90), num4 + (float)Zx.P(4) + (float)(l * 3), w - Zx.P(74), num4 + (float)(l * 5));
			}
			break;
		}
		case "blackfriday":
		{
			bool flag = t % 2.4 < 2.1;
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddString("SALE", new FontFamily("Segoe UI Black"), 0, 36f, new PointF(w - Zx.P(190), Zx.P(14)), StringFormat.GenericTypographic);
			using (Pen pen2 = new Pen(Color.FromArgb(flag ? 60 : 15, 255, 60, 90), Zx.P(9f)))
			{
				g.DrawPath(pen2, graphicsPath);
			}
			using Pen pen3 = new Pen(Color.FromArgb(flag ? 255 : 80, 255, 170, 190), 1.6f);
			g.DrawPath(pen3, graphicsPath);
			break;
		}
		case "valentin":
		{
			for (int i = 0; i < 4; i++)
			{
				float num = (float)w * (0.2f + 0.2f * (float)i);
				float num2 = (float)Zx.P(18) + 6f * (float)Math.Sin(t * 1.5 + (double)i);
				using (Pen pen = new Pen(Color.FromArgb(140, 255, 255, 255), 1f))
				{
					g.DrawLine(pen, num, 0f, num, num2);
				}
				using SolidBrush b = new SolidBrush(Color.FromArgb(230, 255, 90, 140));
				BanAnim.Coeur(g, num, num2 + 10f, 9f, b);
			}
			break;
		}
		case "nouvelan":
			Feux(g, new Rectangle(0, 0, w, (int)((float)h * 0.5f)), t, 2, 470);
			break;
		case "paques":
			break;
		}
	}

	private void Trajet(int w, int h, double t, double periode, int graine, out float x, out float y, out float inc, out int sens)
	{
		int num = (int)Math.Floor(t / periode);
		int num2 = Page * 17 + graine;
		double num3 = (t - (double)num * periode) / periode;
		sens = ((H(num, 601 + num2) > 0.5f) ? 1 : (-1));
		float num4 = (float)h * (0.06f + 0.25f * H(num, 602 + num2));
		float num5 = (float)h * (0.06f + 0.25f * H(num, 603 + num2));
		float num6 = 10f + 28f * H(num, 604 + num2);
		float num7 = 0.6f + 1.4f * H(num, 605 + num2);
		double num8 = num3 * num3 * (3.0 - 2.0 * num3);
		double num9 = num3 * Math.PI * 2.0 * (double)num7 + (double)num;
		x = (float)((sens > 0) ? ((double)(-Zx.P(220)) + num3 * (double)(w + Zx.P(440))) : ((double)(w + Zx.P(220)) - num3 * (double)(w + Zx.P(440))));
		y = (float)((double)num4 + (double)(num5 - num4) * num8 + (double)num6 * Math.Sin(num9));
		double num10 = (double)((num5 - num4) * 6f) * num3 * (1.0 - num3) + (double)num6 * Math.Cos(num9) * Math.PI * 2.0 * (double)num7;
		inc = (float)Math.Max(-18.0, Math.Min(18.0, Math.Atan(num10 / (double)(w + 440)) * 180.0 / Math.PI * (double)sens));
	}

	private void Acteur(Graphics g, int w, int h, double t)
	{
		float x;
		float y;
		float inc;
		int sens;
		switch (id)
		{
		case "nouvelan":
			Trajet(w, h, t, 12.0, 1, out x, out y, out inc, out sens);
			Avion(g, (sens > 0) ? x : ((float)w - x), y, inc, t);
			break;
		case "valentin":
			Trajet(w, h, t, 11.0, 2, out x, out y, out inc, out sens);
			Cupidon(g, w, h, x, y, inc, sens, t);
			break;
		case "halloween":
			Trajet(w, h, t, 10.0, 3, out x, out y, out inc, out sens);
			Sorciere(g, x, y, inc, sens, t);
			break;
		case "blackfriday":
			Trajet(w, h, t, 13.0, 4, out x, out y, out inc, out sens);
			Drone(g, x, y, inc, t);
			break;
		case "ete":
		{
			for (int i = 0; i < 3; i++)
			{
				Trajet(w, h, t + (double)i * 0.45, 14.0, 5, out x, out y, out inc, out sens);
				Mouette(g, x - (float)(sens * i * Zx.P(34)), y + (float)(i * Zx.P(14)), sens, t + (double)i);
			}
			break;
		}
		case "carnaval":
			Canons(g, w, h, t);
			break;
		}
	}

	private static void Trans(Graphics g, float x, float y, float inc, int sens)
	{
		g.TranslateTransform(x, y);
		g.RotateTransform(inc);
		g.ScaleTransform(sens, 1f);
	}

	private void Avion(Graphics g, float x, float y, float inc, double t)
	{
		GraphicsState gstate = g.Save();
		Trans(g, x, y, inc * 0.5f, 1);
		string text = Lang.T("BONNE ANNÉE !");
		if (banTexte == null || banCle != text)
		{
			if (banTexte != null)
			{
				banTexte.Dispose();
			}
			banCle = text;
			SizeF sizeF;
			using (Bitmap image = new Bitmap(1, 1))
			{
				using Graphics graphics = Graphics.FromImage(image);
				sizeF = graphics.MeasureString(text, fBan);
			}
			banTexte = new Bitmap((int)sizeF.Width + 2, (int)sizeF.Height + 2, PixelFormat.Format32bppPArgb);
			using Graphics graphics2 = Graphics.FromImage(banTexte);
			graphics2.TextRenderingHint = TextRenderingHint.AntiAlias;
			using SolidBrush brush = new SolidBrush(Color.FromArgb(200, 140, 20));
			graphics2.DrawString(text, fBan, brush, 0f, 0f);
		}
		SizeF sizeF2 = new SizeF(banTexte.Width - Zx.P(2), banTexte.Height - Zx.P(2));
		float num = sizeF2.Width + (float)Zx.P(24);
		float num2 = (float)(-Zx.P(70)) - num;
		using (Pen pen = new Pen(Color.FromArgb(180, 230, 230, 230), 1f))
		{
			g.DrawLine(pen, -Zx.P(26), 0f, num2 + num, 0f);
		}
		List<PointF> list = new List<PointF>();
		List<PointF> list2 = new List<PointF>();
		for (int i = 0; i <= 12; i++)
		{
			float num3 = (float)i / 12f;
			float x2 = num2 + num * num3;
			float num4 = (float)Math.Sin(t * 6.0 - (double)(num3 * 5f)) * 4f * (1f - num3);
			list.Add(new PointF(x2, num4 - (float)Zx.P(11)));
			list2.Insert(0, new PointF(x2, num4 + (float)Zx.P(11)));
		}
		list.AddRange(list2);
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(240, 255, 245, 220)))
		{
			g.FillPolygon(brush2, list.ToArray());
		}
		g.DrawImage(banTexte, num2 + (float)Zx.P(12), (0f - sizeF2.Height) / 2f + (float)Math.Sin(t * 6.0 - 2.5) * 2f, banTexte.Width, banTexte.Height);
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(220, 60, 70)))
		{
			g.FillEllipse(brush3, -Zx.P(28), -Zx.P(7), Zx.P(46), Zx.P(14));
			g.FillPolygon(brush3, new PointF[3]
			{
				new PointF(-Zx.P(26), -Zx.P(4)),
				new PointF(-Zx.P(36), -Zx.P(16)),
				new PointF(-Zx.P(30), -Zx.P(2))
			});
		}
		using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(240, 240, 245)))
		{
			g.FillEllipse(brush4, -Zx.P(10), -Zx.P(13), Zx.P(18), Zx.P(26));
			g.FillEllipse(brush4, Zx.P(2), -Zx.P(5), Zx.P(9), Zx.P(6));
		}
		using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(120, 200, 200, 210)))
		{
			g.FillEllipse(brush5, Zx.P(16), (float)(-Zx.P(11)) * (float)Math.Abs(Math.Sin(t * 40.0)), Zx.P(4), (float)Zx.P(22) * (float)Math.Abs(Math.Sin(t * 40.0)) + 1f);
		}
		g.Restore(gstate);
	}

	private void Cupidon(Graphics g, int w, int h, float x, float y, float inc, int sens, double t)
	{
		double num = 2.2;
		int num2 = (int)Math.Floor(t / num);
		double num3 = t - (double)num2 * num;
		Trajet(w, h, (double)num2 * num, 11.0, 2, out var x2, out var y2, out var _, out var sens2);
		if (num3 < 0.8)
		{
			float num4 = x2 + (float)sens2 * ((float)Zx.P(30) + (float)num3 * (float)Zx.P(420));
			float num5 = y2 + (float)(num3 * num3 * (double)Zx.P(60));
			using (Pen pen = new Pen(Color.FromArgb(230, 180, 120, 60), Zx.P(2f)))
			{
				g.DrawLine(pen, num4 - (float)(sens2 * Zx.P(16)), num5, num4, num5);
			}
			using SolidBrush b = new SolidBrush(Color.FromArgb(240, 255, 80, 120));
			BanAnim.Coeur(g, num4 + (float)(sens2 * 3), num5, 4f, b);
		}
		else if (num3 < 1.5)
		{
			float num6 = x2 + (float)sens2 * ((float)Zx.P(30) + 0.8f * (float)Zx.P(420));
			float num7 = y2 + 0.64f * (float)Zx.P(60);
			float num8 = (float)((num3 - 0.8) / 0.7);
			for (int i = 0; i < 8; i++)
			{
				double num9 = (double)i * Math.PI / 4.0;
				using SolidBrush b2 = new SolidBrush(Color.FromArgb((int)(230f * (1f - num8)), 255, 100, 150));
				BanAnim.Coeur(g, num6 + (float)Math.Cos(num9) * 30f * num8, num7 + (float)Math.Sin(num9) * 30f * num8, 4f * (1f - num8 * 0.5f), b2);
			}
		}
		GraphicsState gstate = g.Save();
		Trans(g, x, y, inc, sens);
		float num10 = (float)Math.Sin(t * 16.0) * 8f;
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
		{
			g.FillEllipse(brush, -Zx.P(16), (float)(-Zx.P(18)) - num10, Zx.P(16), Zx.P(12));
			g.FillEllipse(brush, -Zx.P(12), (float)(-Zx.P(14)) + num10 * 0.5f, Zx.P(14), Zx.P(10));
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(255, 220, 190)))
		{
			g.FillEllipse(brush2, -Zx.P(8), -Zx.P(8), Zx.P(16), Zx.P(16));
			g.FillEllipse(brush2, Zx.P(2), -Zx.P(18), Zx.P(12), Zx.P(12));
		}
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(240, 200, 90)))
		{
			g.FillEllipse(brush3, Zx.P(2), -Zx.P(21), Zx.P(12), Zx.P(6));
		}
		using (Pen pen2 = new Pen(Color.FromArgb(200, 140, 60), Zx.P(2f)))
		{
			g.DrawArc(pen2, Zx.P(6), -Zx.P(12), Zx.P(14), Zx.P(24), -Zx.P(80), Zx.P(160));
		}
		g.Restore(gstate);
	}

	private void Sorciere(Graphics g, float x, float y, float inc, int sens, double t)
	{
		for (int i = 0; i < 12; i++)
		{
			float cx = x - (float)(sens * (Zx.P(40) + i * Zx.P(10)));
			float cy = y + (float)Zx.P(6) + (float)Math.Sin(t * 5.0 + (double)i) * 3f;
			int num = 200 - i * 16;
			if (num <= 0)
			{
				break;
			}
			BanAnim.Brume(g, cx, cy, 5f, 5f, Color.FromArgb(200, 120, 255), num / 2);
		}
		GraphicsState gstate = g.Save();
		Trans(g, x, y, inc, sens);
		float num2 = (float)Math.Sin(t * 8.0) * 4f;
		using (Pen pen = new Pen(Color.FromArgb(120, 80, 40), Zx.P(3f)))
		{
			g.DrawLine(pen, -Zx.P(34), Zx.P(6), Zx.P(26), Zx.P(2));
		}
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, 150, 70)))
		{
			g.FillPolygon(brush, new PointF[4]
			{
				new PointF(-Zx.P(34), Zx.P(6)),
				new PointF(-Zx.P(50), -Zx.P(2)),
				new PointF(-Zx.P(52), Zx.P(8)),
				new PointF(-Zx.P(48), Zx.P(16))
			});
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(240, 20, 10, 30)))
		{
			g.FillPolygon(brush2, new PointF[5]
			{
				new PointF(-Zx.P(4), -Zx.P(14)),
				new PointF(Zx.P(8), -Zx.P(14)),
				new PointF(Zx.P(6), Zx.P(4)),
				new PointF(-Zx.P(22), (float)Zx.P(8) + num2),
				new PointF(-Zx.P(12), -Zx.P(6))
			});
			g.FillEllipse(brush2, -Zx.P(2), -Zx.P(24), Zx.P(12), Zx.P(12));
			g.FillPolygon(brush2, new PointF[3]
			{
				new PointF(-Zx.P(6), -Zx.P(20)),
				new PointF(Zx.P(14), -Zx.P(20)),
				new PointF(0f, -Zx.P(42))
			});
			g.FillEllipse(brush2, -Zx.P(8), -Zx.P(22), Zx.P(24), Zx.P(5));
		}
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(150, 210, 110)))
		{
			g.FillEllipse(brush3, Zx.P(6), -Zx.P(20), Zx.P(5), Zx.P(5));
		}
		g.Restore(gstate);
	}

	private void Drone(Graphics g, float x, float y, float inc, double t)
	{
		GraphicsState gstate = g.Save();
		Trans(g, x, y, inc * 0.4f, 1);
		float num = (float)Math.Sin(t * 2.4) * 6f;
		using (Pen pen = new Pen(Color.FromArgb(200, 200, 200, 210), 1f))
		{
			g.DrawLine(pen, -Zx.P(4), Zx.P(6), (float)(-Zx.P(4)) + num, Zx.P(30));
			g.DrawLine(pen, Zx.P(4), Zx.P(6), (float)Zx.P(4) + num, Zx.P(30));
		}
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, 40, 46)))
		{
			g.FillRectangle(brush, (float)(-Zx.P(11)) + num, Zx.P(30), Zx.P(22), Zx.P(18));
		}
		using (Pen pen2 = new Pen(Color.FromArgb(240, 200, 70), Zx.P(2f)))
		{
			g.DrawLine(pen2, num, Zx.P(30), num, Zx.P(48));
			g.DrawLine(pen2, (float)(-Zx.P(11)) + num, Zx.P(39), (float)Zx.P(11) + num, Zx.P(39));
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(60, 64, 76)))
		{
			g.FillRectangle(brush2, -Zx.P(14), -Zx.P(3), Zx.P(28), Zx.P(9));
			g.FillRectangle(brush2, -Zx.P(30), -1, Zx.P(60), Zx.P(3));
		}
		int[] array = new int[2] { -1, 1 };
		foreach (int num2 in array)
		{
			using SolidBrush brush3 = new SolidBrush(Color.FromArgb(110, 210, 220, 240));
			g.FillEllipse(brush3, num2 * Zx.P(30) - Zx.P(12), -Zx.P(5), Zx.P(24), (float)Zx.P(4) + 2f * (float)Math.Abs(Math.Sin(t * 50.0 + (double)num2)));
		}
		using (SolidBrush brush4 = new SolidBrush((t % 1.0 < 0.5) ? Color.FromArgb(255, 60, 60) : Color.FromArgb(60, 255, 120)))
		{
			g.FillEllipse(brush4, -Zx.P(3), Zx.P(4), Zx.P(6), Zx.P(4));
		}
		g.Restore(gstate);
	}

	private static void Mouette(Graphics g, float x, float y, int sens, double t)
	{
		float num = (float)Math.Sin(t * 7.0) * 6f;
		using Pen pen = new Pen(Color.FromArgb(230, 245, 245, 250), Zx.P(2.2f));
		pen.StartCap = LineCap.Round;
		pen.EndCap = LineCap.Round;
		pen.LineJoin = LineJoin.Round;
		g.DrawLines(pen, new PointF[5]
		{
			new PointF(x - (float)Zx.P(14), y - num),
			new PointF(x - (float)Zx.P(6), y - (float)Zx.P(3)),
			new PointF(x, y + 1f),
			new PointF(x + (float)Zx.P(6), y - (float)Zx.P(3)),
			new PointF(x + (float)Zx.P(14), y - num)
		});
	}

	private void Canons(Graphics g, int w, int h, double t)
	{
		double num = 2.6;
		int num2 = (int)Math.Floor(t / num);
		double num3 = t - (double)num2 * num;
		if (num3 > 2.2)
		{
			return;
		}
		bool flag = num2 % 2 == 0;
		float num4 = (flag ? 40 : (w - 40));
		float num5 = h - 30;
		Color[] palette = Palette;
		for (int i = 0; i < 46; i++)
		{
			double num6 = (flag ? (-1.4207963267948966) : (-1.7207963267948965)) + (double)(flag ? 1 : (-1)) * (0.1 + 0.6 * (double)H(i, 700 + num2 % 7));
			double num7 = 380f + 300f * H(i, 701 + num2 % 7);
			float dx = num4 + (float)(Math.Cos(num6) * num7 * num3);
			float dy = num5 + (float)(Math.Sin(num6) * num7 * num3 + (double)Zx.P(420) * num3 * num3);
			float num8 = (float)Math.Max(0.0, 1.0 - num3 / 2.2);
			GraphicsState gstate = g.Save();
			g.TranslateTransform(dx, dy);
			g.RotateTransform((float)(num3 * 500.0 + (double)(i * 40)));
			using (SolidBrush brush = new SolidBrush(A((int)(240f * num8), palette[i % palette.Length])))
			{
				g.FillRectangle(brush, -Zx.P(3), -1.5f, 6f * (float)Math.Abs(Math.Cos(num3 * 9.0 + (double)i)) + 1f, Zx.P(3));
			}
			g.Restore(gstate);
		}
		if (num3 < 0.25)
		{
			BanAnim.Brume(g, num4, num5, 60f, 60f, Color.FromArgb(255, 230, 150), (int)(200.0 * (1.0 - num3 / 0.25)));
		}
	}

	private void Lapin(Graphics g, int w, float sol, double t)
	{
		double num = 16.0;
		int num2 = (int)Math.Floor(t / num);
		double num3 = (t - (double)num2 * num) / num;
		int num4 = ((H(num2, 720 + Page) > 0.5f) ? 1 : (-1));
		float x = (float)((num4 > 0) ? ((double)(-Zx.P(60)) + num3 * (double)(w + Zx.P(120))) : ((double)(w + Zx.P(60)) - num3 * (double)(w + Zx.P(120))));
		float num5 = (float)Math.Abs(Math.Sin(t * 4.2));
		float y = sol - num5 * (float)Zx.P(34);
		GraphicsState gstate = g.Save();
		Trans(g, x, y, (float)Math.Cos(t * 4.2) * 10f * (float)num4, num4);
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 236, 236, 240)))
		{
			g.FillEllipse(brush, -Zx.P(14), -Zx.P(12), Zx.P(24), Zx.P(18));
			g.FillEllipse(brush, Zx.P(4), -Zx.P(22), Zx.P(14), Zx.P(14));
			g.FillEllipse(brush, Zx.P(8), -Zx.P(40), Zx.P(5), Zx.P(20));
			g.FillEllipse(brush, Zx.P(13), -Zx.P(38), Zx.P(5), Zx.P(18));
			g.FillEllipse(brush, -Zx.P(18), -Zx.P(8), Zx.P(8), Zx.P(8));
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(255, 180, 200)))
		{
			g.FillEllipse(brush2, Zx.P(9), -Zx.P(36), Zx.P(2.5f), Zx.P(14));
			g.FillEllipse(brush2, Zx.P(16), -Zx.P(14), Zx.P(3), Zx.P(3));
		}
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(40, 30, 40)))
		{
			g.FillEllipse(brush3, Zx.P(12), -Zx.P(18), Zx.P(3), Zx.P(3));
		}
		g.Restore(gstate);
	}

	private void Voilier(Graphics g, int w, float mer, double t)
	{
		float num = Mod(t * (double)Zx.P(18), w + 160) - (float)Zx.P(80);
		float num2 = mer + (float)Math.Sin(t * 1.6) * 2f;
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 250, 250, 255)))
		{
			g.FillPolygon(brush, new PointF[4]
			{
				new PointF(num - (float)Zx.P(22), num2),
				new PointF(num + (float)Zx.P(22), num2),
				new PointF(num + (float)Zx.P(16), num2 + (float)Zx.P(8)),
				new PointF(num - (float)Zx.P(16), num2 + (float)Zx.P(8))
			});
		}
		using (Pen pen = new Pen(Color.FromArgb(120, 80, 50), 1.6f))
		{
			g.DrawLine(pen, num, num2, num, num2 - (float)Zx.P(40));
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(0, 85, 164)))
		{
			g.FillPolygon(brush2, new PointF[3]
			{
				new PointF(num + 1f, num2 - (float)Zx.P(40)),
				new PointF(num + 1f, num2 - (float)Zx.P(6)),
				new PointF(num + (float)Zx.P(12), num2 - (float)Zx.P(6))
			});
		}
		using SolidBrush brush3 = new SolidBrush(Color.FromArgb(239, 65, 53));
		g.FillPolygon(brush3, new PointF[3]
		{
			new PointF(num - 1f, num2 - (float)Zx.P(36)),
			new PointF(num - 1f, num2 - (float)Zx.P(6)),
			new PointF(num - (float)Zx.P(16), num2 - (float)Zx.P(6))
		});
	}

	private void Construire(int w, int h)
	{
		int num = Math.Max(Zx.P(110), Math.Min(Zx.P(170), h / 5));
		if (decor != null && dw == w && dh == num)
		{
			return;
		}
		if (decor != null)
		{
			decor.Dispose();
		}
		dw = w;
		dh = num;
		decor = new Bitmap(w, num, PixelFormat.Format32bppPArgb);
		Random random = new Random(11);
		using Graphics graphics = Graphics.FromImage(decor);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		switch (id)
		{
		case "nouvelan":
		{
			int num9;
			for (int n = -Zx.P(10); n < w; n += num9 + Zx.P(2))
			{
				num9 = Zx.P(34) + random.Next(60);
				int num10 = (int)((double)num * (0.35 + 0.6 * random.NextDouble()));
				using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(16 + random.Next(10), 18 + random.Next(10), 40 + random.Next(20))))
				{
					graphics.FillRectangle(brush6, n, num - num10, num9, num10);
				}
				for (int num11 = num - num10 + 8; num11 < num - 6; num11 += 11)
				{
					for (int num12 = n + 5; num12 < n + num9 - 6; num12 += 9)
					{
						if (random.Next(3) > 0)
						{
							using SolidBrush brush7 = new SolidBrush(Color.FromArgb(150 + random.Next(100), 255, 214, 130));
							graphics.FillRectangle(brush7, num12, num11, Zx.P(4), Zx.P(5));
						}
					}
				}
				if (random.Next(3) == 0)
				{
					using Pen pen2 = new Pen(Color.FromArgb(40, 44, 70), Zx.P(2f));
					graphics.DrawLine(pen2, n + num9 / 2, num - num10, n + num9 / 2, num - num10 - Zx.P(14));
				}
			}
			return;
		}
		case "valentin":
		{
			Colline(graphics, w, num, 0.45f, Color.FromArgb(120, 40, 80), Color.FromArgb(70, 20, 50));
			for (int num18 = Zx.P(20); num18 < w; num18 += Zx.P(70) + random.Next(80))
			{
				float num19 = Zx.P(30) + random.Next(26);
				float num20 = (float)num * 0.62f;
				using (Pen pen4 = new Pen(Color.FromArgb(90, 50, 50), Zx.P(3f)))
				{
					graphics.DrawLine(pen4, num18, num20, num18, num20 - num19 * 0.6f);
				}
				using SolidBrush b = new SolidBrush(Color.FromArgb(230, 60 + random.Next(60), 110));
				BanAnim.Coeur(graphics, num18, num20 - num19 * 0.75f, num19 * 0.42f, b);
			}
			Colline(graphics, w, num, 0.72f, Color.FromArgb(160, 50, 100), Color.FromArgb(90, 24, 60));
			return;
		}
		case "carnaval":
		{
			Colline(graphics, w, num, 0.8f, Color.FromArgb(40, 30, 70), Color.FromArgb(24, 18, 44));
			for (int i = Zx.P(10); i < w; i += Zx.P(110) + random.Next(70))
			{
				int num2 = Zx.P(70) + random.Next(40);
				int num3 = Zx.P(50) + random.Next(30);
				float num4 = (float)num * 0.84f;
				Color color = Palette[random.Next(Palette.Length)];
				for (int j = 0; j < 6; j++)
				{
					using SolidBrush brush = new SolidBrush((j % 2 == 0) ? color : Color.FromArgb(245, 240, 230));
					graphics.FillPolygon(brush, new PointF[3]
					{
						new PointF((float)i + (float)num2 / 2f, num4 - (float)num3),
						new PointF((float)i + (float)(num2 * j) / 6f, num4),
						new PointF((float)i + (float)(num2 * (j + 1)) / 6f, num4)
					});
				}
				using (Pen pen = new Pen(Color.FromArgb(90, 60, 30), Zx.P(2f)))
				{
					graphics.DrawLine(pen, (float)i + (float)num2 / 2f, num4 - (float)num3, (float)i + (float)num2 / 2f, num4 - (float)num3 - (float)Zx.P(14));
				}
				using SolidBrush brush2 = new SolidBrush(color);
				graphics.FillPolygon(brush2, new PointF[3]
				{
					new PointF((float)i + (float)num2 / 2f, num4 - (float)num3 - (float)Zx.P(14)),
					new PointF((float)i + (float)num2 / 2f + (float)Zx.P(12), num4 - (float)num3 - (float)Zx.P(10)),
					new PointF((float)i + (float)num2 / 2f, num4 - (float)num3 - (float)Zx.P(6))
				});
			}
			return;
		}
		case "paques":
		{
			Colline(graphics, w, num, 0.5f, Color.FromArgb(120, 200, 110), Color.FromArgb(80, 160, 80));
			for (int num21 = 0; num21 < w / 18; num21++)
			{
				float num22 = random.Next(w);
				float num23 = (float)num * (0.62f + 0.3f * (float)random.NextDouble());
				Color color3 = Palette[random.Next(Palette.Length)];
				for (int num24 = 0; num24 < 5; num24++)
				{
					double num25 = (double)num24 * Math.PI * 2.0 / 5.0;
					using SolidBrush brush11 = new SolidBrush(color3);
					graphics.FillEllipse(brush11, num22 + (float)Math.Cos(num25) * 3f - Zx.P(2.5f), num23 + (float)Math.Sin(num25) * 3f - Zx.P(2.5f), Zx.P(5), Zx.P(5));
				}
				using SolidBrush brush12 = new SolidBrush(Color.FromArgb(255, 220, 80));
				graphics.FillEllipse(brush12, num22 - (float)Zx.P(2), num23 - (float)Zx.P(2), Zx.P(4), Zx.P(4));
			}
			for (int num26 = 0; num26 < 7; num26++)
			{
				float num27 = (float)w * (0.06f + 0.14f * (float)num26) + (float)random.Next(30);
				float num28 = 22 + random.Next(10);
				float num29 = num28 * 0.75f;
				float num30 = (float)num * 0.86f - num28;
				Color color4 = Palette[num26 % Palette.Length];
				using (LinearGradientBrush brush13 = new LinearGradientBrush(new RectangleF(num27, num30, num29, num28 + 1f), ControlPaint.Light(color4, 0.3f), color4, 0f))
				{
					graphics.FillEllipse(brush13, num27, num30, num29, num28);
				}
				using Pen pen5 = new Pen(Color.FromArgb(200, 255, 255, 255), Zx.P(2f));
				graphics.DrawLine(pen5, num27 + (float)Zx.P(2), num30 + num28 * 0.45f, num27 + num29 - (float)Zx.P(2), num30 + num28 * 0.45f);
			}
			return;
		}
		case "ete":
		{
			using (LinearGradientBrush brush8 = new LinearGradientBrush(new Rectangle(0, (int)((float)num * 0.35f), w, (int)((float)num * 0.45f) + 1), Color.FromArgb(40, 120, 200), Color.FromArgb(20, 70, 150), 90f))
			{
				graphics.FillRectangle(brush8, 0f, (float)num * 0.35f, w, (float)num * 0.45f);
			}
			List<PointF> list = new List<PointF>();
			for (int num13 = -Zx.P(10); num13 <= w + 10; num13 += Zx.P(10))
			{
				list.Add(new PointF(num13, (float)((double)num * 0.78 + 5.0 * Math.Sin((double)num13 / 60.0))));
			}
			list.Add(new PointF(w + Zx.P(10), num + 1));
			list.Add(new PointF(-Zx.P(10), num + 1));
			using (LinearGradientBrush brush9 = new LinearGradientBrush(new Rectangle(0, (int)((float)num * 0.7f), w, (int)((float)num * 0.3f) + Zx.P(2)), Color.FromArgb(240, 214, 160), Color.FromArgb(214, 180, 120), 90f))
			{
				graphics.FillPolygon(brush9, list.ToArray());
			}
			for (int num14 = 0; num14 < 4; num14++)
			{
				float num15 = (float)w * (0.12f + 0.24f * (float)num14) + (float)random.Next(40);
				float num16 = (float)num * 0.8f;
				Color color2 = Palette[num14 % 3];
				using (Pen pen3 = new Pen(Color.FromArgb(120, 90, 60), Zx.P(2f)))
				{
					graphics.DrawLine(pen3, num15, num16, num15 + (float)Zx.P(4), num16 - (float)Zx.P(46));
				}
				for (int num17 = 0; num17 < 6; num17++)
				{
					using SolidBrush brush10 = new SolidBrush((num17 % 2 == 0) ? color2 : Color.White);
					graphics.FillPie(brush10, num15 + (float)Zx.P(4) - (float)Zx.P(30), num16 - (float)Zx.P(62), Zx.P(60), Zx.P(34), Zx.P(180) + num17 * Zx.P(30), Zx.P(30));
				}
			}
			return;
		}
		case "halloween":
		{
			Colline(graphics, w, num, 0.5f, Color.FromArgb(30, 16, 36), Color.FromArgb(18, 8, 22));
			for (int k = Zx.P(30); k < w; k += Zx.P(160) + random.Next(120))
			{
				Arbre(graphics, k, (float)num * 0.6f, 50 + random.Next(30), random);
			}
			for (int l = Zx.P(15); l < w; l += Zx.P(60) + random.Next(60))
			{
				float x = l;
				float num5 = (float)num * 0.7f + (float)random.Next(10);
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb(52, 48, 66));
				graphics.FillRectangle(brush3, x, num5, Zx.P(16), Zx.P(22));
				graphics.FillEllipse(brush3, x, num5 - (float)Zx.P(8), Zx.P(16), Zx.P(16));
			}
			for (int m = Zx.P(40); m < w; m += Zx.P(120) + random.Next(110))
			{
				float num6 = m;
				float num7 = (float)num * 0.8f;
				float num8 = 22 + random.Next(10);
				using (LinearGradientBrush brush4 = new LinearGradientBrush(new RectangleF(num6, num7, num8 * 1.3f, num8 + 1f), Color.FromArgb(255, 150, 40), Color.FromArgb(190, 70, 10), 90f))
				{
					graphics.FillEllipse(brush4, num6, num7, num8 * 1.3f, num8);
				}
				using SolidBrush brush5 = new SolidBrush(Color.FromArgb(255, 230, 90));
				graphics.FillPolygon(brush5, new PointF[3]
				{
					new PointF(num6 + num8 * 0.3f, num7 + num8 * 0.45f),
					new PointF(num6 + num8 * 0.5f, num7 + num8 * 0.45f),
					new PointF(num6 + num8 * 0.4f, num7 + num8 * 0.25f)
				});
				graphics.FillPolygon(brush5, new PointF[3]
				{
					new PointF(num6 + num8 * 0.8f, num7 + num8 * 0.45f),
					new PointF(num6 + num8 * 1f, num7 + num8 * 0.45f),
					new PointF(num6 + num8 * 0.9f, num7 + num8 * 0.25f)
				});
				graphics.FillRectangle(brush5, num6 + num8 * 0.35f, num7 + num8 * 0.65f, num8 * 0.6f, num8 * 0.1f);
			}
			Colline(graphics, w, num, 0.9f, Color.FromArgb(14, 6, 16), Color.FromArgb(10, 4, 12));
			return;
		}
		}
		for (int num31 = Zx.P(10); num31 < w; num31 += Zx.P(70) + random.Next(50))
		{
			int num32 = Zx.P(40) + random.Next(26);
			int num33 = 46 + random.Next(30);
			float num34 = num - Zx.P(8);
			using (LinearGradientBrush brush14 = new LinearGradientBrush(new RectangleF(num31, num34 - (float)num33, num32, num33 + 1), Color.FromArgb(40, 40, 48), Color.FromArgb(14, 14, 18), 90f))
			{
				graphics.FillRectangle(brush14, num31, num34 - (float)num33, num32, num33);
			}
			using (Pen pen6 = new Pen(Color.FromArgb(220, 190, 70), Zx.P(2f)))
			{
				graphics.DrawRectangle(pen6, num31, num34 - (float)num33, num32, num33);
				graphics.DrawArc(pen6, (float)num31 + (float)num32 * 0.25f, num34 - (float)num33 - (float)num32 * 0.3f, (float)num32 * 0.5f, (float)num32 * 0.6f, Zx.P(180), Zx.P(180));
			}
			using Font font = new Font("Segoe UI Black", Zx.P(9f));
			TextRenderer.DrawText(graphics, "%", font, new Rectangle(num31, (int)(num34 - (float)num33), num32, num33), Color.FromArgb(240, 200, 80), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
	}

	private static void Colline(Graphics g, int w, int hv, float base_, Color c1, Color c2)
	{
		List<PointF> list = new List<PointF>();
		for (int i = -Zx.P(10); i <= w + 10; i += Zx.P(10))
		{
			list.Add(new PointF(i, (float)((double)((float)hv * base_) + (double)Zx.P(9) * Math.Sin((double)i / 130.0 + (double)(base_ * 7f)) + 5.0 * Math.Sin((double)i / 41.0))));
		}
		list.Add(new PointF(w + Zx.P(10), hv + 1));
		list.Add(new PointF(-Zx.P(10), hv + 1));
		using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, (int)((float)hv * base_) - Zx.P(12), w, hv), c1, c2, 90f);
		g.FillPolygon(brush, list.ToArray());
	}

	private static void Arbre(Graphics g, float x, float y, float t, Random r)
	{
		using Pen pen = new Pen(Color.FromArgb(20, 10, 24), Zx.P(4f));
		pen.StartCap = LineCap.Round;
		pen.EndCap = LineCap.Round;
		g.DrawLine(pen, x, y + (float)Zx.P(20), x, y - t * 0.5f);
		for (int i = 0; i < 5; i++)
		{
			float num = y - t * (0.1f + 0.08f * (float)i);
			float num2 = ((i % 2 != 0) ? 1 : (-1));
			float num3 = t * (0.5f - 0.06f * (float)i);
			pen.Width = (float)Zx.P(3) - (float)i * 0.4f;
			g.DrawLine(pen, x, num, x + num2 * num3, num - num3 * 0.6f);
			g.DrawLine(pen, x + num2 * num3 * 0.6f, num - num3 * 0.36f, x + num2 * num3 * 0.8f, num - num3 * 0.9f);
		}
	}

	private void Suspendu(Graphics g, int w, double t, float y0, float creux)
	{
		Color[] palette = Palette;
		int num = Math.Max(8, w / 46);
		float num2 = (float)Math.Sin(t * 1.1) * 3f;
		PointF[] array = new PointF[num + 1];
		for (int i = 0; i <= num; i++)
		{
			float num3 = (float)i / (float)num;
			float num4 = num3 * 2f % 1f;
			ref PointF reference = ref array[i];
			reference = new PointF(num3 * (float)w, y0 + creux * 4f * num4 * (1f - num4) + num2 * num4);
		}
		using (Pen pen = new Pen(Color.FromArgb(150, 220, 220, 230), 1.4f))
		{
			g.DrawCurve(pen, array);
		}
		for (int j = 0; j < num; j++)
		{
			PointF pointF = array[j];
			Color c = palette[j % palette.Length];
			float num5 = (float)Math.Sin(t * 2.0 + (double)j) * 2f;
			using SolidBrush solidBrush = new SolidBrush(A(235, c));
			if (id == "valentin")
			{
				BanAnim.Coeur(g, pointF.X + 6f, pointF.Y + 12f + num5, 7f, solidBrush);
			}
			else if (id == "halloween")
			{
				g.FillEllipse(solidBrush, pointF.X, pointF.Y + (float)Zx.P(6) + num5, Zx.P(16), Zx.P(13));
				using SolidBrush brush = new SolidBrush(Color.FromArgb(255, 230, 90));
				g.FillRectangle(brush, pointF.X + (float)Zx.P(4), pointF.Y + (float)Zx.P(10) + num5, Zx.P(2.5f), Zx.P(2.5f));
				g.FillRectangle(brush, pointF.X + Zx.P(9.5f), pointF.Y + (float)Zx.P(10) + num5, Zx.P(2.5f), Zx.P(2.5f));
			}
			else if (id == "paques")
			{
				for (int k = 0; k < 5; k++)
				{
					double num6 = (double)k * Math.PI * 2.0 / 5.0;
					g.FillEllipse(solidBrush, pointF.X + (float)Zx.P(6) + (float)Math.Cos(num6) * 5f - Zx.P(3.5f), pointF.Y + (float)Zx.P(12) + num5 + (float)Math.Sin(num6) * 5f - Zx.P(3.5f), Zx.P(7), Zx.P(7));
				}
			}
			else if (id == "nouvelan" || id == "blackfriday")
			{
				BanAnim.Etincelle(g, pointF.X + 6f, pointF.Y + 12f + num5, 6f, c, 255);
			}
			else
			{
				g.FillPolygon(solidBrush, new PointF[3]
				{
					new PointF(pointF.X + (float)Zx.P(2), pointF.Y + (float)Zx.P(2)),
					new PointF(pointF.X + (float)Zx.P(20), pointF.Y + (float)Zx.P(3)),
					new PointF(pointF.X + (float)Zx.P(11), pointF.Y + (float)Zx.P(24) + num5)
				});
			}
		}
	}

	private void Ciel(Graphics g, int w, int h, double t)
	{
		Rectangle r = new Rectangle(0, 0, w, (int)((float)h * 0.55f));
		switch (id)
		{
		case "nouvelan":
			Feux(g, r, t, 4, 400);
			break;
		case "ete":
			Lumiere(g, w, h);
			Feux(g, r, t, 3, 410);
			break;
		case "valentin":
		{
			for (int k = 0; k < 6; k++)
			{
				float num4 = Mod(t * 0.04 + (double)H(k, 420), 1.0);
				float num5 = (float)w * (0.08f + 0.16f * (float)k) + (float)Zx.P(30) * (float)Math.Sin(t * 0.6 + (double)k);
				float num6 = (float)(h + Zx.P(40)) - num4 * (float)(h + Zx.P(120));
				float num7 = 14f + 8f * H(k, 421);
				using (Pen pen2 = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
				{
					g.DrawBezier(pen2, num5, num6 + num7, num5 - (float)Zx.P(6), num6 + num7 + (float)Zx.P(20), num5 + (float)Zx.P(6), num6 + num7 + (float)Zx.P(40), num5, num6 + num7 + (float)Zx.P(60));
				}
				using (LinearGradientBrush b = new LinearGradientBrush(new RectangleF(num5 - num7, num6 - num7, num7 * 2f, num7 * 2f + 1f), Color.FromArgb(230, 255, 130, 170), Color.FromArgb(230, 210, 30, 80), 90f))
				{
					BanAnim.Coeur(g, num5, num6, num7, b);
				}
				using SolidBrush brush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
				g.FillEllipse(brush, num5 - num7 * 0.55f, num6 - num7 * 0.4f, num7 * 0.35f, num7 * 0.25f);
			}
			break;
		}
		case "paques":
			Papillons(g, w, h, t, 7);
			break;
		case "carnaval":
		{
			for (int i = 0; i < 8; i++)
			{
				float num = Mod(t * 0.07 + (double)H(i, 430), 1.0);
				float num2 = (float)w * H(i, 431);
				float num3 = (float)(-Zx.P(40)) + num * (float)(h + Zx.P(80));
				Color c = Palette[i % Palette.Length];
				List<PointF> list = new List<PointF>();
				for (int j = 0; j < 16; j++)
				{
					list.Add(new PointF(num2 + (float)Zx.P(10) * (float)Math.Sin(t * 3.0 + (double)j * 0.7 + (double)i), num3 - (float)(j * 5)));
				}
				using Pen pen = new Pen(A(200, c), Zx.P(2.4f));
				pen.LineJoin = LineJoin.Round;
				g.DrawCurve(pen, list.ToArray());
			}
			break;
		}
		case "halloween":
		{
			Lumiere(g, w, h);
			for (int l = 0; l < 7; l++)
			{
				float num8 = Mod((double)(H(l, 440) * (float)w) + t * (double)((float)Zx.P(26) + (float)Zx.P(20) * H(l, 441)), w + 60) - (float)Zx.P(30);
				float num9 = (float)h * (0.2f + 0.4f * H(l, 442)) + (float)Zx.P(8) * (float)Math.Sin(t * 2.0 + (double)l);
				float num10 = 8f + 6f * H(l, 443);
				float num11 = (float)Math.Sin(t * 14.0 + (double)(l * 2)) * num10 * 0.7f;
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb(230, 12, 6, 18));
				g.FillEllipse(brush2, num8 - num10 * 0.25f, num9 - num10 * 0.3f, num10 * 0.5f, num10 * 0.7f);
				g.FillPolygon(brush2, new PointF[3]
				{
					new PointF(num8 - num10 * 0.2f, num9),
					new PointF(num8 - num10 * 1.3f, num9 - num11),
					new PointF(num8 - num10 * 0.6f, num9 + num10 * 0.2f)
				});
				g.FillPolygon(brush2, new PointF[3]
				{
					new PointF(num8 + num10 * 0.2f, num9),
					new PointF(num8 + num10 * 1.3f, num9 - num11),
					new PointF(num8 + num10 * 0.6f, num9 + num10 * 0.2f)
				});
			}
			break;
		}
		default:
		{
			bool on = t % 3.0 < 2.7 || Math.Sin(t * 50.0) > 0.0;
			Rectangle zoneLibre = ThemeBase.ZoneLibre;
			if (zoneLibre.Height >= Zx.P(76))
			{
				g.DrawImageUnscaled(Neon(on), zoneLibre.X + Math.Max(0, (zoneLibre.Width - Zx.P(200)) / 2), zoneLibre.Y + (zoneLibre.Height - Zx.P(80)) / 2);
			}
			break;
		}
		}
	}

	private Bitmap Neon(bool on)
	{
		int num = (on ? 1 : 0);
		if (neons[num] != null)
		{
			return neons[num];
		}
		Bitmap bitmap = new Bitmap(Zx.P(300), Zx.P(120), PixelFormat.Format32bppPArgb);
		Bitmap bitmap2 = bitmap;
		bitmap = new Bitmap(Zx.P(200), Zx.P(80), PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(bitmap2))
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Rectangle rectangle = new Rectangle(0, 0, Zx.P(300), Zx.P(120));
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddString("BLACK", new FontFamily("Segoe UI Black"), 0, 40f, new PointF(rectangle.X + Zx.P(20), rectangle.Y + Zx.P(6)), StringFormat.GenericTypographic);
			graphicsPath.AddString("FRIDAY", new FontFamily("Segoe UI Black"), 0, 40f, new PointF(rectangle.X + Zx.P(20), rectangle.Y + Zx.P(52)), StringFormat.GenericTypographic);
			Color c = Color.FromArgb(255, 200, 60);
			float[][] array = new float[2][]
			{
				new float[2] { 12f, 22f },
				new float[2] { 6f, 60f }
			};
			foreach (float[] array2 in array)
			{
				using Pen pen = new Pen(A(on ? ((int)array2[1]) : ((int)array2[1] / 4), c), array2[0]);
				pen.LineJoin = LineJoin.Round;
				graphics.DrawPath(pen, graphicsPath);
			}
			using Pen pen2 = new Pen(A(on ? 255 : 80, Color.FromArgb(255, 245, 210)), 1.6f);
			graphics.DrawPath(pen2, graphicsPath);
		}
		using (Graphics graphics2 = Graphics.FromImage(bitmap))
		{
			graphics2.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics2.DrawImage(bitmap2, 0, 0, Zx.P(200), Zx.P(80));
		}
		bitmap2.Dispose();
		neons[num] = bitmap;
		return bitmap;
	}

	private void Feux(Graphics g, Rectangle r, double t, int n, int seed)
	{
		Color[] palette = Palette;
		for (int i = 0; i < n; i++)
		{
			double num = 2.8 + 1.4 * (double)H(i, seed);
			double num2 = (double)Mod(t + (double)H(i, seed + 1) * num * 3.0, num) / num;
			int num3 = (int)Math.Floor((t + (double)H(i, seed + 1) * num * 3.0) / num);
			float num4 = (float)r.X + (float)r.Width * (0.1f + 0.8f * H(i * 7 + num3, seed + 2));
			float num5 = (float)r.Y + (float)r.Height * (0.2f + 0.3f * H(i * 5 + num3, seed + 3));
			float num6 = (float)r.Height * (0.26f + 0.18f * H(i + num3, seed + 4));
			Color c = palette[(i + num3) % palette.Length];
			if (num2 < 0.2)
			{
				float num7 = (float)((double)r.Bottom - (double)((float)r.Bottom - num5) * (num2 / 0.2));
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(220, 255, 220, 160)))
				{
					g.FillEllipse(brush, num4 - 1.5f, num7 - 1.5f, Zx.P(3), Zx.P(3));
				}
				continue;
			}
			double num8 = (num2 - 0.2) / 0.8;
			double num9 = 1.0 - Math.Pow(1.0 - num8, 3.0);
			int a = (int)(255.0 * (1.0 - num8) * (1.0 - num8) + 20.0 * (1.0 - num8));
			if (num8 < 0.2)
			{
				BanAnim.Brume(g, num4, num5, num6 * 0.6f, num6 * 0.6f, c, (int)(170.0 * (1.0 - num8 / 0.2)));
			}
			using SolidBrush brush2 = new SolidBrush(A(a, c));
			for (int j = 0; j < 18; j++)
			{
				double num10 = (double)j * Math.PI * 2.0 / 18.0;
				float num11 = num4 + (float)(Math.Cos(num10) * (double)num6 * num9);
				float num12 = num5 + (float)(Math.Sin(num10) * (double)num6 * num9 + num8 * num8 * (double)num6 * 0.5);
				g.FillEllipse(brush2, num11 - (float)Zx.P(2), num12 - (float)Zx.P(2), Zx.P(4), Zx.P(4));
			}
		}
	}

	private void Papillons(Graphics g, int w, int h, double t, int n)
	{
		for (int i = 0; i < n; i++)
		{
			float num = Mod((double)(H(i, 450) * (float)w) + t * (double)((float)Zx.P(20) + (float)Zx.P(16) * H(i, 451)) + (double)((float)Zx.P(40) * (float)Math.Sin(t * 0.7 + (double)i)), w + 40) - (float)Zx.P(20);
			float num2 = (float)h * (0.15f + 0.45f * H(i, 452)) + (float)Zx.P(20) * (float)Math.Sin(t * 1.3 + (double)(i * 2));
			float num3 = 7f + 4f * H(i, 453);
			float num4 = Math.Abs((float)Math.Sin(t * 10.0 + (double)i));
			Color c = Palette[i % Palette.Length];
			using (SolidBrush brush = new SolidBrush(A(230, c)))
			{
				g.FillEllipse(brush, num - num3 * num4, num2 - num3 * 0.8f, num3 * num4, num3);
				g.FillEllipse(brush, num, num2 - num3 * 0.8f, num3 * num4, num3);
				g.FillEllipse(brush, num - num3 * 0.7f * num4, num2, num3 * 0.7f * num4, num3 * 0.7f);
				g.FillEllipse(brush, num, num2, num3 * 0.7f * num4, num3 * 0.7f);
			}
			using Pen pen = new Pen(Color.FromArgb(200, 60, 40, 40), 1.4f);
			g.DrawLine(pen, num, num2 - num3 * 0.6f, num, num2 + num3 * 0.6f);
		}
	}

	public override void PeindreFond(Graphics g, int w, int h)
	{
		if (w < 10 || h < 10)
		{
			return;
		}
		double temps = ThemeNoel.Temps;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (Page == 0)
		{
			Wl = w;
			Hl = h;
			Ciel(g, w, h, temps);
			Construire(w, h);
			g.DrawImageUnscaled(decor, 0, h - dh);
			SceneAccueil(g, w, h, temps);
			Acteur(g, w, h, temps);
			if (id == "paques")
			{
				Lapin(g, w, (float)h - (float)dh * 0.22f, temps);
			}
			else if (id == "ete")
			{
				Voilier(g, w, (float)(h - dh) + (float)dh * 0.55f, temps);
			}
		}
		else if (Page == 1)
		{
			HautJeux(g, w, h, temps);
			SceneJeux(g, w, h);
			if (id == "paques")
			{
				Papillons(g, w, h, temps, 4);
				Lapin(g, w, h - 24, temps);
			}
			Acteur(g, w, h, temps);
		}
		else
		{
			Suspendu(g, w, temps, -Zx.P(4), 10f);
		}
	}

	public override void SurTuiles(Graphics g, int w, int h)
	{
		if (Page == 0)
		{
			Wl = w;
			Hl = h;
			double temps = ThemeNoel.Temps;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			switch (id)
			{
			case "halloween":
				SortsHalloween(g, temps);
				break;
			case "blackfriday":
				TicketsBF(g, w, h, temps);
				break;
			case "ete":
				MouettesPosees(g, temps);
				break;
			case "paques":
				OeufsPaques(g, w, h, temps);
				break;
			case "valentin":
				Coulures(g, temps);
				break;
			case "nouvelan":
				FuseesBoutons(g, temps);
				break;
			}
		}
	}

	private void SceneAccueil(Graphics g, int w, int h, double t)
	{
		switch (id)
		{
		case "ete":
			JetSkis(g, w, h, t);
			break;
		case "valentin":
			Tables(g, w, h, t);
			break;
		case "carnaval":
			BusCarnaval(g, w, h, t);
			break;
		case "nouvelan":
			Foule(g, w, h, t);
			break;
		}
	}

	public override int EtatTuile(int i)
	{
		if (Page != 0)
		{
			return 0;
		}
		double temps = ThemeNoel.Temps;
		if (id == "halloween")
		{
			int num = 17;
			int num2 = (int)Math.Floor(temps / 7.0);
			for (int j = num2 - 2; j <= num2; j++)
			{
				num = num * 31 + EtatFissure(j, i, temps);
			}
			return num;
		}
		return 0;
	}

	public override PointF Decalage(int i)
	{
		if (!Secoue || i >= ThemeBase.Tuiles.Length)
		{
			return PointF.Empty;
		}
		double temps = ThemeNoel.Temps;
		float num = BusX(temps);
		float num2 = (float)ThemeBase.Tuiles[i].X + (float)ThemeBase.Tuiles[i].Width / 2f;
		float num3 = Math.Max(0f, 1f - Math.Abs(num - num2) / 520f);
		if (num3 <= 0f)
		{
			return PointF.Empty;
		}
		double num4 = temps * Math.PI * 2.0 * 2.2;
		return new PointF((float)(2.2 * (double)num3 * Math.Sin(num4 + (double)i)), (float)(-1.6 * (double)num3 * Math.Abs(Math.Cos(num4))));
	}

	public override void DecorTuile(Graphics g, int i, int w, int h)
	{
		if (Page != 0 || i >= ThemeBase.Tuiles.Length)
		{
			return;
		}
		double temps = ThemeNoel.Temps;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (id == "halloween")
		{
			int num = (int)Math.Floor(temps / 7.0);
			for (int j = num - 2; j <= num; j++)
			{
				Fissure(g, j, i, temps);
			}
			return;
		}
		if (id == "paques" && i > 0)
		{
			float num2 = (float)(w - 70) - 40f * H(i, 956);
			float num3 = h - 10;
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(170, 150, 110, 60)))
			{
				g.FillEllipse(brush, num2 - (float)Zx.P(26), num3 - (float)Zx.P(8), Zx.P(52), Zx.P(12));
			}
			Oeuf(g, num2 - (float)Zx.P(10), num3 - 1f, 24f, i * 3, 1f);
			Oeuf(g, num2 + (float)Zx.P(11), num3, 20f, i * 3 + 1, 1f);
			Oeuf(g, num2, num3 + 1f, 17f, i * 3 + 2, 1f);
			using Pen pen = new Pen(Color.FromArgb(190, 190, 150, 80), 1.6f);
			for (int k = 0; k < 5; k++)
			{
				g.DrawArc(pen, num2 - (float)Zx.P(26) + (float)(k * 3), num3 - (float)Zx.P(6) + (float)(k % 2), Zx.P(52) - k * 6, Zx.P(10), 0f, Zx.P(180));
			}
			return;
		}
		if (!(id == "valentin"))
		{
			return;
		}
		using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, 0, w, h + 1), Color.FromArgb(34, 255, 110, 160), Color.FromArgb(70, 220, 40, 90), 90f))
		{
			using GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, w - 1, h - 1), Zx.P(10));
			g.FillPath(brush2, path);
		}
		List<PointF> list = new List<PointF>();
		for (int l = Zx.P(6); l <= w - 6; l += Zx.P(6))
		{
			list.Add(new PointF(l, (float)(h - Zx.P(3)) - 3f * (float)Math.Abs(Math.Sin((double)l * 0.07 + (double)i)) - (float)((l % 42 == 0) ? Zx.P(4) : 0)));
		}
		list.Add(new PointF(w - Zx.P(6), h));
		list.Add(new PointF(Zx.P(6), h));
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(150, 225, 60, 110)))
		{
			g.FillPolygon(brush3, list.ToArray());
		}
		using Pen pen2 = new Pen(Color.FromArgb(90, 255, 200, 220), 1f);
		g.DrawLines(pen2, list.GetRange(0, list.Count - 2).ToArray());
	}

	private bool Sort(int k, out int i, out PointF cible, out PointF depart)
	{
		i = -1;
		cible = (depart = PointF.Empty);
		int num = ThemeBase.Tuiles.Length;
		if (num == 0 || k < 0)
		{
			return false;
		}
		Trajet(Wl, Hl, (double)k * 7.0 + 0.6, 10.0, 3, out var x, out var y, out var _, out var sens);
		if (x < 40f || x > (float)(Wl - 40))
		{
			return false;
		}
		i = (int)(H(k, 901) * (float)num) % num;
		Rectangle rectangle = ThemeBase.Tuiles[i];
		cible = new PointF((float)rectangle.X + (float)rectangle.Width * (0.14f + 0.72f * H(k, 902)), rectangle.Y + Zx.P(2));
		depart = new PointF(x + (float)(sens * Zx.P(26)), y + (float)Zx.P(8));
		return true;
	}

	private int EtatFissure(int k, int i, double t)
	{
		if (!Sort(k, out var i2, out var _, out var _) || i2 != i)
		{
			return 0;
		}
		double num = t - (double)k * 7.0;
		if (num < 1.4 || num > 15.4)
		{
			return 0;
		}
		return 1 + k * 8 + (int)Math.Round(Math.Min(1.0, (15.4 - num) / 1.5) * 4.0);
	}

	private void Fissure(Graphics g, int k, int i, double t)
	{
		if (!Sort(k, out var i2, out var cible, out var _) || i2 != i)
		{
			return;
		}
		double num = t - (double)k * 7.0;
		if (num < 1.4 || num > 15.4)
		{
			return;
		}
		float num2 = (float)Math.Min(1.0, (15.4 - num) / 1.5);
		num2 = (float)Math.Round(num2 * 4f) / 4f;
		if (num2 <= 0f)
		{
			return;
		}
		float num3 = cible.X - (float)ThemeBase.Tuiles[i].X;
		Random random = new Random(k * 7919 + i);
		PointF[] points = new PointF[6]
		{
			new PointF(num3 - (float)Zx.P(18), 0f),
			new PointF(num3 - (float)Zx.P(9), Zx.P(8) + random.Next(4)),
			new PointF(num3 - (float)Zx.P(2), Zx.P(5)),
			new PointF(num3 + (float)Zx.P(6), Zx.P(13) + random.Next(4)),
			new PointF(num3 + (float)Zx.P(14), Zx.P(6)),
			new PointF(num3 + (float)Zx.P(20), 0f)
		};
		using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(255f * num2), 14, 10, 26)))
		{
			g.FillPolygon(brush, points);
		}
		using (Pen pen = new Pen(Color.FromArgb((int)(160f * num2), 190, 140, 255), 1.2f))
		{
			g.DrawLines(pen, points);
		}
		for (int j = 0; j < 3; j++)
		{
			List<PointF> list = new List<PointF>();
			float num4 = num3 + (float)((j - 1) * 6);
			float num5 = Zx.P(9);
			list.Add(new PointF(num4, num5));
			float num6 = (float)(j - 1) * 0.7f;
			for (int l = 0; l < 5; l++)
			{
				num4 += num6 * (float)Zx.P(8) + (float)(random.NextDouble() - 0.5) * (float)Zx.P(12);
				num5 += (float)Zx.P(6) + (float)random.NextDouble() * (float)Zx.P(9);
				list.Add(new PointF(num4, num5));
			}
			using (Pen pen2 = new Pen(Color.FromArgb((int)(220f * num2), 18, 10, 28), 1.8f))
			{
				g.DrawLines(pen2, list.ToArray());
			}
			using Pen pen3 = new Pen(Color.FromArgb((int)(70f * num2), 200, 160, 255), 1f);
			PointF[] array = list.ToArray();
			for (int m = 0; m < array.Length; m++)
			{
				ref PointF reference = ref array[m];
				reference = new PointF(array[m].X + 1f, array[m].Y);
			}
			g.DrawLines(pen3, array);
		}
	}

	private void SortsHalloween(Graphics g, double t)
	{
		int num = (int)Math.Floor(t / 7.0);
		double num2 = t - (double)num * 7.0;
		if (!Sort(num, out var _, out var cible, out var depart))
		{
			return;
		}
		if (num2 >= 0.6 && num2 < 1.4)
		{
			double num3 = (num2 - 0.6) / 0.8;
			double num4 = num3 * num3 * (3.0 - 2.0 * num3);
			for (int num5 = 6; num5 >= 0; num5--)
			{
				double num6 = Math.Max(0.0, num4 - (double)num5 * 0.035);
				float num7 = (float)((double)depart.X + (double)(cible.X - depart.X) * num6);
				float num8 = (float)((double)depart.Y + (double)(cible.Y - depart.Y) * num6 - Math.Sin(num6 * Math.PI) * (double)Zx.P(60) + Math.Sin(num6 * 30.0) * 4.0);
				if (num5 == 0)
				{
					BanAnim.Brume(g, num7, num8, 26f, 26f, Color.FromArgb(140, 255, 120), 200);
					using SolidBrush brush = new SolidBrush(Color.FromArgb(240, 230, 255, 230));
					g.FillEllipse(brush, num7 - (float)Zx.P(4), num8 - (float)Zx.P(4), Zx.P(8), Zx.P(8));
				}
				else
				{
					using SolidBrush brush2 = new SolidBrush(Color.FromArgb(180 - num5 * 24, (num5 % 2 == 0) ? 160 : 190, 120, 255));
					g.FillEllipse(brush2, num7 - (float)Zx.P(3) + (float)num5 * 0.2f, num8 - (float)Zx.P(3), (float)Zx.P(6) - (float)num5 * 0.6f, (float)Zx.P(6) - (float)num5 * 0.6f);
				}
			}
		}
		else
		{
			if (!(num2 >= 1.4) || !(num2 < 2.6))
			{
				return;
			}
			float num9 = (float)((num2 - 1.4) / 1.2);
			BanAnim.Brume(g, cible.X, cible.Y, 40f * (1f - num9) + 6f, 40f * (1f - num9) + 6f, Color.FromArgb(170, 255, 140), (int)(230f * (1f - num9)));
			Random random = new Random(num * 31);
			for (int j = 0; j < 9; j++)
			{
				float num10 = (float)(random.NextDouble() - 0.5) * 180f;
				float num11 = -60f - (float)random.NextDouble() * 140f;
				float num12 = num9 * 1.2f;
				float num13 = cible.X + num10 * num12;
				float num14 = cible.Y + num11 * num12 + (float)Zx.P(420) * num12 * num12;
				float num15 = 3f + (float)random.NextDouble() * 4f;
				using (SolidBrush brush3 = new SolidBrush(Color.FromArgb((int)(255f * (1f - num9)), 38, 42, 80)))
				{
					g.FillRectangle(brush3, num13 - num15 / 2f, num14 - num15 / 2f, num15, num15);
				}
				using Pen pen = new Pen(Color.FromArgb((int)(200f * (1f - num9)), 120, 130, 220), 1f);
				g.DrawRectangle(pen, num13 - num15 / 2f, num14 - num15 / 2f, num15, num15);
			}
		}
	}

	private Bitmap Ticket(int v)
	{
		if (tickets == null)
		{
			string[] array = new string[6] { "-50%", "-30%", "-70%", "PROMO", "-20%", "SOLDES" };
			Color[] array2 = new Color[3]
			{
				Color.FromArgb(255, 214, 60),
				Color.FromArgb(230, 40, 60),
				Color.FromArgb(24, 24, 30)
			};
			Color[] array3 = new Color[3]
			{
				Color.FromArgb(20, 20, 24),
				Color.White,
				Color.FromArgb(255, 200, 70)
			};
			tickets = new Bitmap[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Bitmap bitmap = new Bitmap(Zx.P(76), Zx.P(34), PixelFormat.Format32bppPArgb);
				int num = i % 3;
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
					using (GraphicsPath graphicsPath = new GraphicsPath())
					{
						graphicsPath.AddPolygon(new PointF[5]
						{
							new PointF(1f, Zx.P(17)),
							new PointF(Zx.P(14), Zx.P(2)),
							new PointF(Zx.P(74), Zx.P(2)),
							new PointF(Zx.P(74), Zx.P(32)),
							new PointF(Zx.P(14), Zx.P(32))
						});
						using (SolidBrush brush = new SolidBrush(array2[num]))
						{
							graphics.FillPath(brush, graphicsPath);
						}
						using Pen pen = new Pen(Color.FromArgb(160, 255, 255, 255), 1.2f);
						graphics.DrawPath(pen, graphicsPath);
					}
					graphics.FillEllipse(Brushes.Transparent, Zx.P(9), Zx.P(14), Zx.P(6), Zx.P(6));
					using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(200, 10, 10, 14)))
					{
						graphics.FillEllipse(brush2, Zx.P(9), Zx.P(14), Zx.P(6), Zx.P(6));
					}
					using Font font = new Font("Segoe UI Black", (array[i].Length > Zx.P(4)) ? Zx.P(9.5f) : Zx.P(12f));
					using SolidBrush brush3 = new SolidBrush(array3[num]);
					StringFormat stringFormat = new StringFormat();
					stringFormat.Alignment = StringAlignment.Center;
					stringFormat.LineAlignment = StringAlignment.Center;
					graphics.DrawString(array[i], font, brush3, new RectangleF(Zx.P(16), Zx.P(2), Zx.P(58), Zx.P(30)), stringFormat);
				}
				tickets[i] = bitmap;
			}
		}
		return tickets[v % tickets.Length];
	}

	private Bitmap TicketTourne(int v, int niv, int pas)
	{
		int key = v % 6 * 10000 + niv * 100 + pas + 50;
		if (ticketsTournes.TryGetValue(key, out var value))
		{
			return value;
		}
		float num = 0.6f + 0.12f * (float)niv;
		value = new Bitmap(Zx.P(80), Zx.P(80), PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(value))
		{
			graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
			graphics.TranslateTransform(40f, Zx.P(40));
			graphics.RotateTransform((float)pas * 5f);
			graphics.ScaleTransform(num, num);
			graphics.DrawImage(Ticket(v), -Zx.P(38), -Zx.P(17), Zx.P(76), Zx.P(34));
		}
		ticketsTournes[key] = value;
		return value;
	}

	private void TicketsBF(Graphics g, int w, int h, double t)
	{
		InterpolationMode interpolationMode = g.InterpolationMode;
		g.InterpolationMode = InterpolationMode.Bilinear;
		for (int i = 0; i < 12; i++)
		{
			float num = 16f + 18f * H(i, 905);
			float num2 = Mod((double)(H(i, 906) * (float)w) + (double)Zx.P(30) * Math.Sin(t * 0.7 + (double)i), w + 80) - (float)Zx.P(40);
			float num3 = Mod((double)(H(i, 907) * (float)(h + Zx.P(80))) + t * (double)num, h + 80) - (float)Zx.P(40);
			int pas = (int)Math.Round(Math.Sin(t * 0.9 + (double)i * 1.7) * 7.0);
			int niv = (int)(H(i, 908) * 3.99f);
			g.DrawImageUnscaled(TicketTourne(i, niv, pas), (int)num2 - Zx.P(40), (int)num3 - Zx.P(40));
		}
		int num4 = ThemeBase.Tuiles.Length;
		if (num4 > 0)
		{
			int num5 = (int)Math.Floor(t / 10.0);
			double num6 = t - (double)(num5 * 10);
			for (int j = 0; j < 3; j++)
			{
				int num7 = (int)(H(num5 * 3 + j, 911) * (float)num4) % num4;
				bool flag = H(num5 * 3 + j, 912) > 0.5f;
				Rectangle rectangle = ThemeBase.Tuiles[num7];
				float num8 = (flag ? (rectangle.Right - 4) : (rectangle.X + 4));
				float dy = rectangle.Y + Zx.P(10) + Zx.P(12) * j % 30;
				float num9 = (float)(Math.Sin(t * 2.3 + (double)(j * 2)) * 14.0);
				float num10 = (float)Math.Min(1.0, Math.Min(num6 / 0.4, (10.0 - num6) / 0.4));
				if (!(num10 <= 0f))
				{
					GraphicsState gstate = g.Save();
					g.TranslateTransform(num8 + (float)(flag ? 6 : (-6)), dy);
					g.RotateTransform(num9 + (float)(flag ? (-8) : 8));
					using (Pen pen = new Pen(Color.FromArgb((int)(220f * num10), 230, 230, 240), 1.2f))
					{
						g.DrawLine(pen, 0, 0, 0, Zx.P(20));
					}
					g.TranslateTransform(flag ? 4 : (-4), Zx.P(20));
					g.RotateTransform(90f);
					g.ScaleTransform(0.85f * num10, 0.85f * num10);
					g.DrawImage(Ticket(num5 + j), -Zx.P(2), -Zx.P(17), Zx.P(76), Zx.P(34));
					g.Restore(gstate);
				}
			}
		}
		g.InterpolationMode = interpolationMode;
	}

	private float BusX(double t)
	{
		double num = 38.0;
		double num2 = (double)Mod(t, num) / num;
		return (float)(-320.0 + num2 * (double)(Wl + 640));
	}

	private void BusCarnaval(Graphics g, int w, int h, double t)
	{
		float num = BusX(t);
		float num2 = h - 14;
		double num3 = t * Math.PI * 2.0 * 2.2;
		Color[] palette = Palette;
		for (int i = 0; i < 16; i++)
		{
			float num4 = num - (float)Zx.P(300) + (float)(i * Zx.P(40)) + (float)((i >= Zx.P(8)) ? Zx.P(340) : 0) - (float)((i >= Zx.P(8)) ? Zx.P(320) : 0) + (float)(i % 2 * 6);
			if (i >= 8)
			{
				num4 = num + (float)Zx.P(140) + (float)((i - Zx.P(8)) * Zx.P(38));
			}
			if (!(num4 < -30f) && !(num4 > (float)(w + 30)))
			{
				float num5 = (float)Math.Abs(Math.Sin(num3 / 2.0 + (double)i * 0.9)) * 7f;
				float num6 = num2 - num5;
				Color color = palette[i % palette.Length];
				bool flag = Math.Sin(num3 / 2.0 + (double)i) > 0.0;
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(230, 30, 22, 50)))
				{
					g.FillEllipse(brush, num4 - (float)Zx.P(6), num6 - (float)Zx.P(40), Zx.P(12), Zx.P(12));
				}
				using (SolidBrush brush2 = new SolidBrush(color))
				{
					g.FillRectangle(brush2, num4 - (float)Zx.P(6), num6 - (float)Zx.P(28), Zx.P(12), Zx.P(18));
				}
				using Pen pen = new Pen(Color.FromArgb(230, 30, 22, 50), Zx.P(3f));
				LineCap startCap = (pen.EndCap = LineCap.Round);
				pen.StartCap = startCap;
				g.DrawLine(pen, num4 - (float)Zx.P(6), num6 - (float)Zx.P(26), num4 - (float)Zx.P(12), flag ? (num6 - (float)Zx.P(42)) : (num6 - (float)Zx.P(16)));
				g.DrawLine(pen, num4 + (float)Zx.P(6), num6 - (float)Zx.P(26), num4 + (float)Zx.P(12), flag ? (num6 - (float)Zx.P(16)) : (num6 - (float)Zx.P(42)));
				g.DrawLine(pen, num4 - (float)Zx.P(3), num6 - (float)Zx.P(10), num4 - (float)Zx.P(5), num6);
				g.DrawLine(pen, num4 + (float)Zx.P(3), num6 - (float)Zx.P(10), num4 + (float)Zx.P(5), num6);
			}
		}
		if (num < -260f || num > (float)(w + 260))
		{
			return;
		}
		float num7 = num - (float)Zx.P(110);
		float num8 = num2 - (float)Zx.P(74);
		using (GraphicsPath path = Bouton.Rond(new Rectangle((int)num7, (int)num8, Zx.P(220), Zx.P(62)), Zx.P(14)))
		{
			using (LinearGradientBrush brush3 = new LinearGradientBrush(new RectangleF(num7, num8, Zx.P(220), Zx.P(63)), Color.FromArgb(255, 210, 60), Color.FromArgb(240, 110, 40), 90f))
			{
				g.FillPath(brush3, path);
			}
			using Pen pen2 = new Pen(Color.FromArgb(150, 90, 40, 20), 1.5f);
			g.DrawPath(pen2, path);
		}
		for (int j = 0; j < 5; j++)
		{
			using SolidBrush brush4 = new SolidBrush(palette[j % palette.Length]);
			g.FillRectangle(brush4, num7 + (float)Zx.P(8) + (float)(j * Zx.P(42)), num8 + (float)Zx.P(46), Zx.P(34), Zx.P(6));
		}
		for (int k = 0; k < 5; k++)
		{
			RectangleF rect = new RectangleF(num7 + (float)Zx.P(10) + (float)(k * Zx.P(41)), num8 + (float)Zx.P(8), Zx.P(32), Zx.P(24));
			using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(200, 120, 200, 255)))
			{
				g.FillRectangle(brush5, rect);
			}
			float num9 = (float)Math.Abs(Math.Sin(num3 / 2.0 + (double)k)) * 4f;
			using SolidBrush brush6 = new SolidBrush(Color.FromArgb(220, 40, 30, 60));
			g.FillEllipse(brush6, rect.X + (float)Zx.P(10), rect.Y + (float)Zx.P(6) - num9, Zx.P(12), Zx.P(12));
			g.FillRectangle(brush6, rect.X + (float)Zx.P(8), rect.Y + (float)Zx.P(17) - num9, Zx.P(16), Zx.P(10));
		}
		float[] array = new float[2]
		{
			num7 + 40f,
			num7 + 178f
		};
		foreach (float num10 in array)
		{
			using (SolidBrush brush7 = new SolidBrush(Color.FromArgb(30, 30, 36)))
			{
				g.FillEllipse(brush7, num10 - (float)Zx.P(13), num2 - (float)Zx.P(26), Zx.P(26), Zx.P(26));
			}
			float num11 = (float)(t * 4.0);
			using Pen pen3 = new Pen(Color.FromArgb(160, 160, 170), Zx.P(2f));
			g.DrawLine(pen3, num10, num2 - (float)Zx.P(13), num10 + (float)Zx.P(8) * (float)Math.Cos(num11), num2 - (float)Zx.P(13) + (float)Zx.P(8) * (float)Math.Sin(num11));
			g.DrawLine(pen3, num10, num2 - (float)Zx.P(13), num10 - (float)Zx.P(8) * (float)Math.Cos(num11), num2 - (float)Zx.P(13) - (float)Zx.P(8) * (float)Math.Sin(num11));
		}
		using (SolidBrush brush8 = new SolidBrush(Color.FromArgb(60, 60, 70)))
		{
			g.FillRectangle(brush8, num7 + (float)Zx.P(96), num8 - (float)Zx.P(12), Zx.P(28), Zx.P(12));
		}
		for (int m = 0; m < 6; m++)
		{
			float num12 = Mod(t * 0.55 + (double)m / 6.0, 1.0);
			float num13 = num7 + 110f + (float)Math.Sin(num12 * 7f + (float)m) * 30f + (float)((m - 3) * 10);
			float num14 = num8 - 14f - num12 * 110f;
			int a = (int)(255f * (1f - num12));
			using SolidBrush brush9 = new SolidBrush(A(a, palette[m % palette.Length]));
			g.FillEllipse(brush9, num13, num14, Zx.P(9), Zx.P(7));
			g.FillRectangle(brush9, num13 + (float)Zx.P(7), num14 - (float)Zx.P(16), Zx.P(2), Zx.P(19));
			g.FillRectangle(brush9, num13 + (float)Zx.P(7), num14 - (float)Zx.P(16), Zx.P(7), Zx.P(3));
		}
	}

	private void JetSkis(Graphics g, int w, int h, double t)
	{
		float num = (float)(h - dh) + (float)dh * 0.35f;
		float num2 = (float)(h - dh) + (float)dh * 0.76f;
		float num3 = (num + num2) / 2f;
		for (int i = 0; i < 2; i++)
		{
			float num4 = (float)w * ((i == 0) ? 0.3f : 0.7f);
			float num5 = ((i == 0) ? 80 : 62);
			float num6 = (num2 - num) * 0.32f;
			double num7 = t * (1.3 + 0.2 * (double)i) + (double)(i * 2);
			for (int num8 = 14; num8 >= 1; num8--)
			{
				double num9 = num7 - (double)num8 * 0.12;
				float num10 = num4 + (float)Math.Cos(num9) * num5;
				float num11 = num3 + (float)Math.Sin(num9) * num6;
				using SolidBrush brush = new SolidBrush(Color.FromArgb(200 - num8 * 13, 255, 255, 255));
				g.FillEllipse(brush, num10 - (float)Zx.P(4) + (float)num8 * 0.2f, num11 - 1.5f, (float)Zx.P(8) - (float)num8 * 0.35f, Zx.P(3));
			}
			float dx = num4 + (float)Math.Cos(num7) * num5;
			float dy = num3 + (float)Math.Sin(num7) * num6;
			float num12 = 0.82f + 0.18f * (float)((Math.Sin(num7) + 1.0) / 2.0);
			int num13 = ((0.0 - Math.Sin(num7) > 0.0) ? 1 : (-1));
			Color color = ((i == 0) ? Color.FromArgb(239, 65, 53) : Color.FromArgb(255, 200, 40));
			GraphicsState gstate = g.Save();
			g.TranslateTransform(dx, dy);
			g.ScaleTransform((float)num13 * num12, num12);
			using (SolidBrush brush2 = new SolidBrush(color))
			{
				g.FillPolygon(brush2, new PointF[5]
				{
					new PointF(-Zx.P(14), -Zx.P(3)),
					new PointF(Zx.P(12), -Zx.P(4)),
					new PointF(Zx.P(16), 0f),
					new PointF(Zx.P(12), Zx.P(3)),
					new PointF(-Zx.P(14), Zx.P(3))
				});
			}
			using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(40, 40, 50)))
			{
				g.FillRectangle(brush3, -Zx.P(6), -Zx.P(7), Zx.P(8), Zx.P(4));
			}
			using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(250, 220, 190)))
			{
				g.FillEllipse(brush4, -Zx.P(4), -Zx.P(16), Zx.P(6), Zx.P(6));
			}
			using (Pen pen = new Pen(Color.FromArgb(30, 90, 170), Zx.P(3f)))
			{
				LineCap startCap = (pen.EndCap = LineCap.Round);
				pen.StartCap = startCap;
				g.DrawLine(pen, -1, -Zx.P(10), Zx.P(2), -Zx.P(5));
				g.DrawLine(pen, 0, -Zx.P(8), Zx.P(6), -Zx.P(6));
			}
			using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
			{
				g.FillEllipse(brush5, -Zx.P(18), 1, Zx.P(10), Zx.P(4));
			}
			g.Restore(gstate);
		}
	}

	private void MouettePosee(Graphics g, float x, float y, int sens, double t)
	{
		float num = (float)Math.Max(0.0, Math.Sin(t * 3.0)) * 2f;
		GraphicsState gstate = g.Save();
		g.TranslateTransform(x, y);
		g.ScaleTransform((float)sens * 1.35f, 1.35f);
		using (Pen pen = new Pen(Color.FromArgb(240, 160, 60), 1.6f))
		{
			g.DrawLine(pen, -Zx.P(3), -Zx.P(5), -Zx.P(3), 0);
			g.DrawLine(pen, Zx.P(3), -Zx.P(5), Zx.P(3), 0);
		}
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(250, 250, 252)))
		{
			g.FillEllipse(brush, -Zx.P(11), -Zx.P(16), Zx.P(22), Zx.P(12));
			g.FillEllipse(brush, Zx.P(5), (float)(-Zx.P(24)) + num, Zx.P(10), Zx.P(10));
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(150, 160, 175)))
		{
			g.FillEllipse(brush2, -Zx.P(10), -Zx.P(16), Zx.P(15), Zx.P(7));
		}
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(250, 200, 50)))
		{
			g.FillPolygon(brush3, new PointF[3]
			{
				new PointF(Zx.P(14), (float)(-Zx.P(20)) + num),
				new PointF(Zx.P(20), (float)(-Zx.P(18)) + num),
				new PointF(Zx.P(14), (float)(-Zx.P(17)) + num)
			});
		}
		using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(20, 20, 30)))
		{
			g.FillEllipse(brush4, Zx.P(10), (float)(-Zx.P(21)) + num, Zx.P(2), Zx.P(2));
		}
		g.Restore(gstate);
	}

	private void MouettesPosees(Graphics g, double t)
	{
		int num = ThemeBase.Tuiles.Length;
		if (num == 0)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			double num2 = 11.0;
			double num3 = t + (double)i * 5.5;
			int num4 = (int)Math.Floor(num3 / num2);
			double num5 = num3 - (double)num4 * num2;
			int num6 = (int)(H(num4 * 2 + i, 941) * (float)num) % num;
			Rectangle rectangle = ThemeBase.Tuiles[num6];
			float num7 = (float)rectangle.X + (float)rectangle.Width * (0.15f + 0.7f * H(num4 * 2 + i, 942));
			float num8 = rectangle.Y + 1;
			int num9 = ((H(num4 * 2 + i, 943) > 0.5f) ? 1 : (-1));
			if (num5 < 2.0)
			{
				double num10 = num5 / 2.0;
				double num11 = 1.0 - (1.0 - num10) * (1.0 - num10);
				float x = (float)((double)num7 - (double)(num9 * Zx.P(360)) * (1.0 - num11));
				float y = (float)((double)num8 - (double)Zx.P(160) * (1.0 - num11) - (double)Zx.P(30) * Math.Sin(num10 * Math.PI));
				Mouette(g, x, y, num9, t + (double)i);
			}
			else if (num5 < 8.0)
			{
				MouettePosee(g, num7, num8, num9, t + (double)i);
			}
			else if (num5 < 10.0)
			{
				double num12 = (num5 - 8.0) / 2.0;
				float x2 = (float)((double)num7 + (double)(num9 * Zx.P(380)) * num12 * num12);
				float y2 = (float)((double)num8 - (double)Zx.P(200) * num12);
				Mouette(g, x2, y2, num9, t + (double)i);
			}
		}
	}

	private void Oeuf(Graphics g, float x, float bas, float hh, int v, float sc)
	{
		Color[] palette = Palette;
		Color color = palette[v % palette.Length];
		Color color2 = palette[(v + 2) % palette.Length];
		float num = hh * 0.76f * sc;
		float num2 = hh * sc;
		RectangleF rect = new RectangleF(x - num / 2f, bas - num2, num, num2);
		using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(rect.X, rect.Y, rect.Width + 1f, rect.Height), ControlPaint.Light(color, 0.35f), color, 0f))
		{
			g.FillEllipse(brush, rect);
		}
		using (Pen pen = new Pen(color2, 2.2f * sc))
		{
			g.DrawArc(pen, rect.X + (float)Zx.P(2), rect.Y + num2 * 0.42f, rect.Width - (float)Zx.P(4), num2 * 0.18f, 0f, Zx.P(180));
		}
		using SolidBrush brush2 = new SolidBrush(Color.FromArgb(150, 255, 255, 255));
		g.FillEllipse(brush2, rect.X + num * 0.22f, rect.Y + num2 * 0.14f, num * 0.22f, num2 * 0.2f);
	}

	private void LapinA(Graphics g, float x, float y, int sens, float saut, bool oeuf, int v)
	{
		GraphicsState gstate = g.Save();
		g.TranslateTransform(x, y);
		g.ScaleTransform(sens, 1f);
		Color color = Color.FromArgb(235, 228, 220);
		Color color2 = Color.FromArgb(190, 180, 170);
		using (SolidBrush brush = new SolidBrush(color2))
		{
			g.FillEllipse(brush, -Zx.P(12), (float)(-Zx.P(14)) - saut * 0.2f, Zx.P(22), Zx.P(14));
		}
		using (SolidBrush brush2 = new SolidBrush(color))
		{
			g.FillEllipse(brush2, -Zx.P(14), -Zx.P(18), Zx.P(24), Zx.P(16));
			g.FillEllipse(brush2, Zx.P(4), -Zx.P(28), Zx.P(14), Zx.P(13));
			g.FillEllipse(brush2, Zx.P(5), -Zx.P(44), Zx.P(5), Zx.P(17));
			g.FillEllipse(brush2, Zx.P(11), -Zx.P(42), Zx.P(5), Zx.P(16));
			g.FillEllipse(brush2, -Zx.P(18), -Zx.P(16), Zx.P(7), Zx.P(7));
		}
		using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(255, 170, 190)))
		{
			g.FillEllipse(brush3, Zx.P(6.5f), -Zx.P(41), Zx.P(2), Zx.P(11));
		}
		using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(30, 20, 20)))
		{
			g.FillEllipse(brush4, Zx.P(12), -Zx.P(24), Zx.P(2.4f), Zx.P(2.4f));
		}
		g.Restore(gstate);
		if (oeuf)
		{
			Oeuf(g, x + (float)(sens * Zx.P(14)), y - (float)Zx.P(8), 14f, v, 1f);
		}
	}

	private List<PointF> NidsPaques()
	{
		List<PointF> list = new List<PointF>();
		if (ThemeBase.Tuiles.Length > 0)
		{
			Rectangle rectangle = ThemeBase.Tuiles[0];
			list.Add(new PointF((float)rectangle.X + (float)rectangle.Width * 0.78f, rectangle.Y + Zx.P(2)));
			list.Add(new PointF((float)rectangle.X + (float)rectangle.Width * 0.3f, rectangle.Y + Zx.P(2)));
		}
		for (int i = 0; i < ThemeBase.Cartes.Length; i++)
		{
			Rectangle rectangle2 = ThemeBase.Cartes[i];
			if (rectangle2.Width >= 40)
			{
				for (int j = 0; j < 2; j++)
				{
					list.Add(new PointF((float)rectangle2.X + (float)rectangle2.Width * (0.18f + 0.6f * H(i * 2 + j, 951)), rectangle2.Y + Zx.P(2)));
				}
			}
		}
		return list;
	}

	private void OeufsPaques(Graphics g, int w, int h, double t)
	{
		List<PointF> list = NidsPaques();
		if (list.Count == 0)
		{
			return;
		}
		double num = 9.0;
		int num2 = (int)Math.Floor(t / num);
		double num3 = t - (double)num2 * num;
		int num4 = (int)(H(num2, 953) * (float)list.Count) % list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			float sc = 1f;
			if (i == num4)
			{
				if (num3 > 3.0 && num3 < 7.6)
				{
					continue;
				}
				if (num3 >= 7.6)
				{
					sc = (float)Math.Min(1.0, (num3 - 7.6) / 0.35);
				}
			}
			Oeuf(g, list[i].X, list[i].Y, 24f + 4f * H(i, 952), i + num2 * ((i == num4) ? 1 : 0), sc);
		}
		float x = list[num4].X;
		float y = list[num4].Y;
		int num5 = ((!(x > (float)(w / 2))) ? 1 : (-1));
		float num6 = x - (float)(num5 * 420);
		float num7 = h - 18;
		if (num3 < 6.5)
		{
			bool flag = num3 < 3.0;
			double num8 = (flag ? (num3 / 3.0) : ((num3 - 3.6) / 2.9));
			if (!flag && num3 < 3.6)
			{
				num8 = 0.0;
			}
			float num9 = (flag ? num6 : x);
			float num10 = (flag ? num7 : y);
			float num11 = (flag ? x : (x - (float)(num5 * 420)));
			float num12 = (flag ? y : num7);
			int num13 = 3;
			double num14 = num8 * (double)num13;
			double num15 = num14 - Math.Floor(num14);
			int num16 = (int)Math.Min(num13 - 1, Math.Floor(num14));
			float num17 = num9 + (num11 - num9) * (float)num16 / (float)num13;
			float num18 = num9 + (num11 - num9) * (float)(num16 + 1) / (float)num13;
			float num19 = num10 + (num12 - num10) * (float)num16 / (float)num13;
			float num20 = num10 + (num12 - num10) * (float)(num16 + 1) / (float)num13;
			float x2 = (float)((double)num17 + (double)(num18 - num17) * num15);
			float y2 = (float)((double)num19 + (double)(num20 - num19) * num15 - Math.Sin(num15 * Math.PI) * (double)Zx.P(46));
			if (!flag && num3 < 3.6)
			{
				x2 = x;
				y2 = y;
			}
			LapinA(g, x2, y2, flag ? num5 : (-num5), (float)Math.Sin(num15 * Math.PI) * 10f, !flag, num4 + num2);
		}
	}

	private void Tables(Graphics g, int w, int h, double t)
	{
		float[] array = new float[3] { 0.16f, 0.52f, 0.84f };
		float num = h - 16;
		if (tables == null || tabW != w)
		{
			if (tables != null)
			{
				tables.Dispose();
			}
			tables = new Bitmap(w, 110, PixelFormat.Format32bppPArgb);
			tabW = w;
			using Graphics graphics = Graphics.FromImage(tables);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			float num2 = 94f;
			for (int i = 0; i < 3; i++)
			{
				float num3 = (float)w * array[i];
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(70, 30, 50)))
				{
					graphics.FillRectangle(brush, num3 - (float)Zx.P(3), num2 - (float)Zx.P(34), Zx.P(6), Zx.P(34));
					graphics.FillRectangle(brush, num3 - (float)Zx.P(14), num2 - (float)Zx.P(3), Zx.P(28), Zx.P(3));
				}
				using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(250, 230, 236)))
				{
					graphics.FillEllipse(brush2, num3 - (float)Zx.P(38), num2 - (float)Zx.P(44), Zx.P(76), Zx.P(16));
				}
				using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(230, 90, 130)))
				{
					graphics.FillRectangle(brush3, num3 - (float)Zx.P(36), num2 - (float)Zx.P(36), Zx.P(72), Zx.P(10));
				}
				for (int j = 0; j < 2; j++)
				{
					float num4 = num3 + (float)((j == 0) ? (-Zx.P(54)) : Zx.P(54));
					Color color = ((j == 0) ? Color.FromArgb(60, 40, 90) : Color.FromArgb(150, 30, 70));
					using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(90, 50, 40)))
					{
						graphics.FillRectangle(brush4, num4 - (float)Zx.P(12), num2 - (float)Zx.P(30), Zx.P(24), Zx.P(4));
						graphics.FillRectangle(brush4, num4 + (float)((j == 0) ? (-Zx.P(12)) : Zx.P(10)), num2 - (float)Zx.P(60), Zx.P(3), Zx.P(30));
						graphics.FillRectangle(brush4, num4 - (float)Zx.P(10), num2 - (float)Zx.P(26), Zx.P(3), Zx.P(26));
						graphics.FillRectangle(brush4, num4 + (float)Zx.P(7), num2 - (float)Zx.P(26), Zx.P(3), Zx.P(26));
					}
					using (SolidBrush brush5 = new SolidBrush(color))
					{
						graphics.FillRectangle(brush5, num4 - (float)Zx.P(9), num2 - (float)Zx.P(56), Zx.P(18), Zx.P(28));
					}
					using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(240, 200, 170)))
					{
						graphics.FillEllipse(brush6, num4 - (float)Zx.P(8), num2 - (float)Zx.P(74), Zx.P(16), Zx.P(16));
					}
					using SolidBrush brush7 = new SolidBrush((j == 0) ? Color.FromArgb(50, 30, 20) : Color.FromArgb(120, 50, 30));
					if (j == 0)
					{
						graphics.FillPie(brush7, num4 - (float)Zx.P(9), num2 - (float)Zx.P(76), Zx.P(18), Zx.P(14), Zx.P(180), Zx.P(180));
						continue;
					}
					graphics.FillEllipse(brush7, num4 - (float)Zx.P(10), num2 - (float)Zx.P(77), Zx.P(20), Zx.P(12));
					graphics.FillRectangle(brush7, num4 - (float)Zx.P(10), num2 - (float)Zx.P(70), Zx.P(5), Zx.P(16));
					graphics.FillRectangle(brush7, num4 + (float)Zx.P(5), num2 - (float)Zx.P(70), Zx.P(5), Zx.P(16));
				}
				using (SolidBrush brush8 = new SolidBrush(Color.White))
				{
					graphics.FillEllipse(brush8, num3 - (float)Zx.P(26), num2 - (float)Zx.P(44), Zx.P(16), Zx.P(6));
					graphics.FillEllipse(brush8, num3 + (float)Zx.P(10), num2 - (float)Zx.P(44), Zx.P(16), Zx.P(6));
				}
				using SolidBrush brush9 = new SolidBrush(Color.FromArgb(250, 245, 230));
				graphics.FillRectangle(brush9, num3 - (float)Zx.P(2), num2 - (float)Zx.P(58), Zx.P(4), Zx.P(14));
			}
		}
		g.DrawImageUnscaled(tables, 0, (int)(num + (float)Zx.P(16) - (float)Zx.P(110)));
		float num5 = num;
		for (int k = 0; k < 3; k++)
		{
			float num6 = (float)w * array[k];
			float num7 = (float)Math.Sin(t * 14.0 + (double)k) * 1.2f;
			BanAnim.Brume(g, num6, num5 - 60f, 16f, 16f, Color.FromArgb(255, 200, 100), 110);
			using (SolidBrush brush10 = new SolidBrush(Color.FromArgb(255, 210, 90)))
			{
				g.FillEllipse(brush10, num6 - Zx.P(2.5f) + num7 * 0.3f, num5 - (float)Zx.P(66), Zx.P(5), Zx.P(8));
			}
			for (int l = 0; l < 2; l++)
			{
				float num8 = num6 + (float)((l == 0) ? (-Zx.P(54)) : Zx.P(54));
				float num9 = ((l == 0) ? 1 : (-1));
				float num10 = (float)(0.5 + 0.5 * Math.Sin(t * 1.6 + (double)(k * 2) + (double)l * 2.5));
				float num11 = num8 + num9 * ((float)Zx.P(10) + (float)Zx.P(22) * (1f - num10));
				float num12 = num5 - (float)Zx.P(48) - (float)Zx.P(14) * num10;
				using (Pen pen = new Pen((l == 0) ? Color.FromArgb(60, 40, 90) : Color.FromArgb(150, 30, 70), Zx.P(4f)))
				{
					LineCap startCap = (pen.EndCap = LineCap.Round);
					pen.StartCap = startCap;
					g.DrawLine(pen, num8 + num9 * 6f, num5 - (float)Zx.P(50), num11, num12);
				}
				using Pen pen2 = new Pen(Color.FromArgb(220, 220, 230), 1.4f);
				g.DrawLine(pen2, num11, num12, num11 + num9 * 6f, num12 - (float)Zx.P(3));
			}
			float num13 = Mod(t * 0.3 + (double)k * 0.33, 1.0);
			using SolidBrush b = new SolidBrush(Color.FromArgb((int)(230f * (1f - num13)), 255, 80, 130));
			BanAnim.Coeur(g, num6 + (float)Math.Sin(num13 * 8f) * 8f, num5 - 86f - num13 * 70f, 6f + 4f * num13, b);
		}
	}

	private void Coulures(Graphics g, double t)
	{
		for (int i = 0; i < ThemeBase.Tuiles.Length; i++)
		{
			Rectangle rectangle = ThemeBase.Tuiles[i];
			for (int j = 0; j < 4; j++)
			{
				float num = (float)rectangle.X + (float)rectangle.Width * (0.12f + 0.24f * (float)j + 0.08f * H(i * 4 + j, 961));
				float num2 = 6f + 3f * H(i * 4 + j, 962);
				double num3 = Mod(t + (double)(H(i * 4 + j, 963) * num2), num2);
				float num4 = (float)Math.Min(1.0, num3 / ((double)num2 * 0.7)) * (8f + 8f * H(i * 4 + j, 964));
				float num5 = rectangle.Bottom - Zx.P(2);
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(210, 225, 60, 110)))
				{
					g.FillRectangle(brush, num - Zx.P(2.5f), num5, Zx.P(5), num4);
					g.FillEllipse(brush, num - (float)Zx.P(4), num5 + num4 - (float)Zx.P(4), Zx.P(8), Zx.P(9));
				}
				using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(150, 255, 200, 220)))
				{
					g.FillEllipse(brush2, num - (float)Zx.P(2), num5 + num4 - (float)Zx.P(2), Zx.P(2.5f), Zx.P(3));
				}
				if (num3 > (double)num2 * 0.7)
				{
					float num6 = (float)((num3 - (double)num2 * 0.7) / ((double)num2 * 0.3));
					using SolidBrush brush3 = new SolidBrush(Color.FromArgb((int)(210f * (1f - num6)), 225, 60, 110));
					g.FillEllipse(brush3, num - (float)Zx.P(3), num5 + num4 + num6 * (float)Zx.P(60), Zx.P(6), Zx.P(7));
				}
			}
		}
	}

	private static PointF3[] Sphere()
	{
		if (sphere != null)
		{
			return sphere;
		}
		int num = 44;
		sphere = new PointF3[num];
		double num2 = Math.PI * (3.0 - Math.Sqrt(5.0));
		for (int i = 0; i < num; i++)
		{
			double num3 = 1.0 - ((double)i + 0.5) * 2.0 / (double)num;
			double num4 = Math.Sqrt(1.0 - num3 * num3);
			double num5 = (double)i * num2;
			ref PointF3 reference = ref sphere[i];
			reference = new PointF3((float)(Math.Cos(num5) * num4), (float)num3, (float)(Math.Sin(num5) * num4));
		}
		return sphere;
	}

	private void FuseesBoutons(Graphics g, double t)
	{
		int num = ThemeBase.Tuiles.Length;
		if (num == 0)
		{
			return;
		}
		Color[] palette = Palette;
		PointF3[] array = Sphere();
		for (int i = 0; i < 3; i++)
		{
			double num2 = 2.9;
			double num3 = t + (double)i * 0.97;
			int num4 = (int)Math.Floor(num3 / num2);
			double num5 = num3 - (double)num4 * num2;
			int num6 = (int)(H(num4 * 3 + i, 971) * (float)num) % num;
			Rectangle rectangle = ThemeBase.Tuiles[num6];
			float num7 = (float)rectangle.X + (float)rectangle.Width * (0.12f + 0.76f * H(num4 * 3 + i, 972));
			float num8 = rectangle.Y;
			float num9 = num7 + (H(num4 * 3 + i, 973) - 0.5f) * 160f;
			float num10 = (float)Hl * (0.08f + 0.18f * H(num4 * 3 + i, 974));
			Color color = palette[(num4 + i) % palette.Length];
			if (num5 < 0.9)
			{
				double num11 = num5 / 0.9;
				double num12 = 1.0 - (1.0 - num11) * (1.0 - num11);
				float num13 = (float)((double)num7 + (double)(num9 - num7) * num12);
				float num14 = (float)((double)num8 + (double)(num10 - num8) * num12);
				using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 255, 230, 180)))
				{
					g.FillEllipse(brush, num13 - Zx.P(2.5f), num14 - Zx.P(2.5f), Zx.P(5), Zx.P(5));
				}
				for (int j = 1; j < 8; j++)
				{
					double num15 = Math.Max(0.0, num11 - (double)j * 0.035);
					double num16 = 1.0 - (1.0 - num15) * (1.0 - num15);
					float num17 = (float)((double)num7 + (double)(num9 - num7) * num16) + (float)Math.Sin((double)(j * 3) + t * 20.0) * 1.5f;
					float num18 = (float)((double)num8 + (double)(num10 - num8) * num16);
					using SolidBrush brush2 = new SolidBrush(Color.FromArgb(200 - j * 24, 255, 180, 90));
					g.FillEllipse(brush2, num17 - 1.5f, num18 - 1.5f, Zx.P(3), Zx.P(3));
				}
				if (num11 < 0.15)
				{
					BanAnim.Brume(g, num7, num8, 18f, 10f, Color.FromArgb(255, 200, 120), (int)(200.0 * (1.0 - num11 / 0.15)));
				}
			}
			else
			{
				if (!(num5 < 2.7))
				{
					continue;
				}
				double num19 = (num5 - 0.9) / 1.8;
				double num20 = 1.0 - Math.Pow(1.0 - num19, 3.0);
				float num21 = 70f + 40f * H(num4 * 3 + i, 975);
				float num22 = (float)(t * 0.8 + (double)i);
				float num23 = (float)Math.Cos(num22);
				float num24 = (float)Math.Sin(num22);
				if (num19 < 0.12)
				{
					BanAnim.Brume(g, num9, num10, num21 * 0.7f, num21 * 0.7f, color, (int)(160.0 * (1.0 - num19 / 0.12)));
				}
				Color color2 = ControlPaint.Light(color, 0.4f);
				using SolidBrush solidBrush = new SolidBrush(color);
				for (int k = 0; k < array.Length; k++)
				{
					float num25 = array[k].X * num23 - array[k].Z * num24;
					float num26 = array[k].X * num24 + array[k].Z * num23;
					float y = array[k].Y;
					float num27 = 1f / (1f - num26 * 0.38f);
					float num28 = num9 + num25 * num21 * (float)num20 * num27;
					float num29 = num10 + y * num21 * (float)num20 * num27 + (float)(num19 * num19 * (double)num21 * 0.7);
					float num30 = 2.4f * num27 + 0.6f;
					int num31 = (int)((255.0 * (1.0 - num19) * (1.0 - num19) + 20.0) * (0.45 + 0.55 * (double)(num26 + 1f) / 2.0));
					if (num31 >= 8)
					{
						solidBrush.Color = A(num31, (num26 > 0.3f) ? color2 : color);
						if (num30 < 2.6f)
						{
							g.FillRectangle(solidBrush, num28 - num30, num29 - num30, num30 * 2f, num30 * 2f);
						}
						else
						{
							g.FillEllipse(solidBrush, num28 - num30, num29 - num30, num30 * 2f, num30 * 2f);
						}
					}
				}
			}
		}
	}

	private void Foule(Graphics g, int w, int h, double t)
	{
		if (foule == null || fouleW != w)
		{
			if (foule != null)
			{
				foule.Dispose();
			}
			foule = new Bitmap(w, 60, PixelFormat.Format32bppPArgb);
			fouleW = w;
			Random random = new Random(5);
			using Graphics graphics = Graphics.FromImage(foule);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			for (int i = -Zx.P(10); i < w + 10; i += Zx.P(15) + random.Next(10))
			{
				float num = 34 + random.Next(16);
				float num2 = 13 + random.Next(4);
				Color color = Color.FromArgb(18 + random.Next(14), 16 + random.Next(10), 34 + random.Next(20));
				using SolidBrush brush = new SolidBrush(color);
				graphics.FillEllipse(brush, i - Zx.P(11), (float)Zx.P(60) - num + num2 * 0.6f, Zx.P(22), num);
				graphics.FillEllipse(brush, (float)i - num2 / 2f, (float)Zx.P(60) - num - num2 * 0.3f, num2, num2);
			}
		}
		g.DrawImageUnscaled(foule, 0, h - Zx.P(60));
		for (int j = 0; j < 9; j++)
		{
			float num3 = (float)w * (0.06f + 0.11f * (float)j) + 6f * H(j, 981);
			float num4 = h - Zx.P(46);
			float num5 = (float)Math.Max(0.0, Math.Sin(t * 1.3 + (double)j * 1.7));
			using (Pen pen = new Pen(Color.FromArgb(26, 22, 44), Zx.P(4f)))
			{
				LineCap startCap = (pen.EndCap = LineCap.Round);
				pen.StartCap = startCap;
				g.DrawLine(pen, num3, num4 + (float)Zx.P(10), num3 + (float)Zx.P(4), num4 - (float)Zx.P(14) * num5);
			}
			if (j % 3 == 0 && num5 > 0.3f)
			{
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)(220f * num5), 190, 220, 255));
				g.FillRectangle(brush2, num3 + 1f, num4 - (float)Zx.P(14) * num5 - (float)Zx.P(7), Zx.P(7), Zx.P(10));
			}
		}
	}

	public override void PeindreNeige(Graphics g, int w, int h, double densite)
	{
		if (w < 10 || h < 10)
		{
			return;
		}
		double temps = ThemeNoel.Temps;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Color[] palette = Palette;
		int num = (int)((double)((Page == 2) ? 30 : 70) * Math.Max(0.0, Math.Min(1.0, densite)));
		for (int i = 0; i < num; i++)
		{
			float num2 = 0.4f + 0.6f * H(i, 460);
			float num3 = 20f + 30f * num2;
			float num4 = Mod((double)(H(i, 461) * (float)w) + (double)Zx.P(18) * Math.Sin(temps * (0.6 + (double)H(i, 462)) + (double)i), w);
			float num5 = Mod((double)(H(i, 463) * (float)(h + Zx.P(20))) + temps * (double)num3, h + 20) - (float)Zx.P(10);
			float num6 = (float)(temps * 90.0 * ((double)H(i, 464) - 0.5) * 2.0 + (double)(i * 37));
			Color c = palette[i % palette.Length];
			int a = (int)(150f + 100f * num2);
			if (id == "valentin")
			{
				float cy = (float)h - num5;
				using (SolidBrush b = new SolidBrush(A(a, c)))
				{
					BanAnim.Coeur(g, num4, cy, 3f + 4f * num2, b);
				}
				continue;
			}
			float ca = (float)Math.Cos((double)num6 * Math.PI / 180.0);
			float sa = (float)Math.Sin((double)num6 * Math.PI / 180.0);
			float px0 = num4;
			float py0 = num5;
			Func<float, float, PointF> func = (float u, float v) => new PointF(px0 + u * ca - v * sa, py0 + u * sa + v * ca);
			using SolidBrush brush = new SolidBrush(A(a, c));
			if (id == "paques" || id == "halloween")
			{
				float num7 = (float)((id == "paques") ? Zx.P(4) : Zx.P(6)) * num2;
				float num8 = ((id == "paques") ? Zx.P(2.5f) : ((float)Zx.P(3))) * num2;
				g.FillPolygon(brush, new PointF[6]
				{
					func(0f - num7, 0f),
					func((0f - num7) * 0.5f, 0f - num8),
					func(num7 * 0.5f, 0f - num8),
					func(num7, 0f),
					func(num7 * 0.5f, num8),
					func((0f - num7) * 0.5f, num8)
				});
			}
			else if (id == "blackfriday" && i % 4 == 0)
			{
				g.FillPolygon(brush, new PointF[5]
				{
					func(-8f, -5f),
					func(6f, -5f),
					func(10f, 0f),
					func(6f, 5f),
					func(-8f, 5f)
				});
			}
			else
			{
				float num9 = 3f * num2 * (float)Math.Abs(Math.Cos(temps * 4.0 + (double)i)) + 0.5f;
				float num10 = 1.6f * num2;
				g.FillPolygon(brush, new PointF[4]
				{
					func(0f - num9, 0f - num10),
					func(num9, 0f - num10),
					func(num9, num10),
					func(0f - num9, num10)
				});
			}
		}
	}
}
