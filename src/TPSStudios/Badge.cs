using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Badge
{
	private static readonly Font fVerrou = new Font("Segoe MDL2 Assets", Zx.P(12f));

	private static readonly Color[] Hi = new Color[10]
	{
		Color.FromArgb(236, 168, 100),
		Color.FromArgb(226, 232, 244),
		Color.FromArgb(255, 224, 110),
		Color.FromArgb(90, 236, 160),
		Color.FromArgb(110, 170, 255),
		Color.FromArgb(255, 110, 120),
		Color.FromArgb(206, 140, 255),
		Color.FromArgb(220, 250, 255),
		Color.FromArgb(255, 255, 255),
		Color.FromArgb(255, 150, 70)
	};

	private static readonly Color[] Lo = new Color[10]
	{
		Color.FromArgb(126, 66, 24),
		Color.FromArgb(110, 124, 150),
		Color.FromArgb(176, 118, 14),
		Color.FromArgb(8, 104, 66),
		Color.FromArgb(18, 44, 156),
		Color.FromArgb(140, 10, 34),
		Color.FromArgb(84, 28, 164),
		Color.FromArgb(70, 170, 214),
		Color.FromArgb(120, 150, 210),
		Color.FromArgb(60, 78, 214)
	};

	/// <summary>Pastille de rôle (Admin, Équipe, Modo) ; renvoie sa largeur (0 si pas de rôle).</summary>
	public static int Role(Graphics g, string role, int x, int y)
	{
		string t;
		Color c;
		switch (role)
		{
		case "admin":
			t = "ADMIN";
			c = Color.FromArgb(230, 70, 70);
			break;
		case "equipe":
			t = "ÉQUIPE";
			c = C.Orange;
			break;
		case "modo":
			t = "MODO";
			c = Color.FromArgb(60, 140, 255);
			break;
		default:
			return 0;
		}
		using Font f = new Font("Segoe UI Semibold", Zx.P(7.5f));
		int w = Lang.MeasureText(t, f).Width + Zx.P(14);
		Rectangle r = new Rectangle(x, y, w, Zx.P(18));
		SmoothingMode sm = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (GraphicsPath p = Bouton.Rond(r, Zx.P(9)))
		{
			using SolidBrush b = new SolidBrush(c);
			g.FillPath(b, p);
		}
		Lang.DrawText(g, t, f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		g.SmoothingMode = sm;
		return w;
	}

	public static void PilleVip(Graphics g, Rectangle r)
	{
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (GraphicsPath path = Bouton.Rond(r, r.Height / 2))
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height + 1), Color.FromArgb(255, 226, 130), Color.FromArgb(214, 140, 24), 90f))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(255, 245, 200));
			g.DrawPath(pen, path);
		}
		using (Font f = new Font("Segoe UI Black", Zx.P(7.5f)))
		{
			Lang.DrawText(g, "VIP", f, r, Color.FromArgb(70, 40, 0), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		g.SmoothingMode = smoothingMode;
	}

	public static void Verrou(Graphics g, Rectangle r)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(24, 28, 58)))
		{
			g.FillEllipse(brush, r);
		}
		using (Pen pen = new Pen(Color.FromArgb(70, 80, 130), 1.4f))
		{
			pen.DashStyle = DashStyle.Dash;
			g.DrawEllipse(pen, r.X + 1, r.Y + 1, r.Width - Zx.P(3), r.Height - Zx.P(3));
		}
		using Font f = new Font("Segoe MDL2 Assets", Math.Max(Zx.P(8f), (float)r.Height * 0.32f), GraphicsUnit.Pixel);
		Lang.DrawText(g, "\ue72e", f, r, Color.FromArgb(120, 130, 175), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
	}

	private static PointF P(RectangleF r, double fx, double fy)
	{
		return new PointF((float)((double)r.X + (double)r.Width * fx), (float)((double)r.Y + (double)r.Height * fy));
	}

	private static PointF[] Poly(RectangleF r, int n, double rot, double rad, double rad2)
	{
		int num = ((rad2 > 0.0) ? (n * 2) : n);
		PointF[] array = new PointF[num];
		for (int i = 0; i < num; i++)
		{
			double num2 = rot + (double)(i * 2) * Math.PI / (double)num;
			double num3 = ((rad2 > 0.0 && i % 2 == 1) ? rad2 : rad);
			ref PointF reference = ref array[i];
			reference = new PointF((float)((double)(r.X + r.Width / 2f) + Math.Cos(num2) * (double)r.Width * num3), (float)((double)(r.Y + r.Height / 2f) + Math.Sin(num2) * (double)r.Height * num3));
		}
		return array;
	}

	private static GraphicsPath Forme(int tier, RectangleF r)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		switch (tier)
		{
		case 0:
			graphicsPath.AddEllipse(r);
			break;
		case 1:
			graphicsPath.AddPolygon(Poly(r, 6, -Math.PI / 2.0, 0.5, 0.0));
			break;
		case 2:
			graphicsPath.AddPolygon(new PointF[8]
			{
				P(r, 0.5, 0.0),
				P(r, 0.92, 0.14),
				P(r, 0.92, 0.55),
				P(r, 0.72, 0.82),
				P(r, 0.5, 1.0),
				P(r, 0.28, 0.82),
				P(r, 0.08, 0.55),
				P(r, 0.08, 0.14)
			});
			break;
		case 3:
			graphicsPath.AddPolygon(new PointF[4]
			{
				P(r, 0.5, 0.0),
				P(r, 1.0, 0.5),
				P(r, 0.5, 1.0),
				P(r, 0.0, 0.5)
			});
			break;
		case 4:
			graphicsPath.AddPolygon(Poly(r, 8, -Math.PI / 2.0, 0.52, 0.36));
			break;
		case 5:
			graphicsPath.AddPolygon(Poly(r, 12, -Math.PI / 2.0, 0.5, 0.43));
			break;
		case 6:
			graphicsPath.AddPolygon(new PointF[6]
			{
				P(r, 0.5, 0.0),
				P(r, 0.92, 0.26),
				P(r, 0.92, 0.74),
				P(r, 0.5, 1.0),
				P(r, 0.08, 0.74),
				P(r, 0.08, 0.26)
			});
			break;
		case 7:
			graphicsPath.AddPolygon(new PointF[6]
			{
				P(r, 0.5, 0.02),
				P(r, 0.86, 0.16),
				P(r, 0.86, 0.56),
				P(r, 0.5, 0.98),
				P(r, 0.14, 0.56),
				P(r, 0.14, 0.16)
			});
			break;
		case 8:
			graphicsPath.AddPolygon(Poly(r, 8, Math.PI / 8.0, 0.5, 0.0));
			break;
		default:
			graphicsPath.AddEllipse(new RectangleF(r.X + r.Width * 0.04f, r.Y + r.Height * 0.12f, r.Width * 0.92f, r.Height * 0.88f));
			break;
		}
		return graphicsPath;
	}

	private static void Etoile(Graphics g, float x, float y, float s, Color c)
	{
		using SolidBrush brush = new SolidBrush(c);
		g.FillPolygon(brush, new PointF[8]
		{
			new PointF(x, y - s),
			new PointF(x + s * 0.28f, y - s * 0.28f),
			new PointF(x + s, y),
			new PointF(x + s * 0.28f, y + s * 0.28f),
			new PointF(x, y + s),
			new PointF(x - s * 0.28f, y + s * 0.28f),
			new PointF(x - s, y),
			new PointF(x - s * 0.28f, y - s * 0.28f)
		});
	}

	private static void Facettes(Graphics g, GraphicsPath p, RectangleF r, int alpha)
	{
		PointF[] pathPoints = p.PathPoints;
		PointF pt = new PointF(r.X + r.Width / 2f, r.Y + r.Height / 2f);
		using Pen pen = new Pen(Color.FromArgb(alpha, 255, 255, 255), Math.Max(0.8f, r.Width / 60f));
		using Pen pen2 = new Pen(Color.FromArgb(alpha, 0, 0, 0), Math.Max(0.8f, r.Width / 60f));
		for (int i = 0; i < pathPoints.Length; i++)
		{
			g.DrawLine((i % 2 == 0) ? pen : pen2, pt, pathPoints[i]);
		}
	}

	public static void Dessiner(Graphics g, Rectangle rc, int niv, double t)
	{
		int num = Progression.Palier(niv);
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float num2 = (float)rc.Width * 0.12f;
		RectangleF rectangleF = new RectangleF((float)rc.X + num2, (float)rc.Y + num2, (float)rc.Width - 2f * num2, (float)rc.Height - 2f * num2);
		Color color = Hi[num];
		Color color2 = Lo[num];
		if (num >= 2)
		{
			float num3 = 0.55f + 0.45f * (float)Math.Sin(t * 2.2 + (double)num);
			RectangleF rect = new RectangleF((float)rc.X - (float)rc.Width * 0.18f, (float)rc.Y - (float)rc.Height * 0.18f, (float)rc.Width * 1.36f, (float)rc.Height * 1.36f);
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(rect);
			using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = Color.FromArgb((int)(110f * num3), color);
			pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, color) };
			g.FillEllipse(pathGradientBrush, rect);
		}
		if (num == 7)
		{
			for (int i = 0; i < 10; i++)
			{
				double num4 = t * 0.8 + (double)i * Math.PI / 5.0;
				using SolidBrush brush = new SolidBrush(Color.FromArgb(64, 200, 245, 255));
				g.FillPolygon(brush, new PointF[3]
				{
					new PointF((float)rc.X + (float)rc.Width / 2f, (float)rc.Y + (float)rc.Height / 2f),
					new PointF((float)((double)(rc.X + rc.Width / 2) + Math.Cos(num4 - 0.09) * (double)rc.Width * 0.8), (float)((double)(rc.Y + rc.Height / 2) + Math.Sin(num4 - 0.09) * (double)rc.Width * 0.8)),
					new PointF((float)((double)(rc.X + rc.Width / 2) + Math.Cos(num4 + 0.09) * (double)rc.Width * 0.8), (float)((double)(rc.Y + rc.Height / 2) + Math.Sin(num4 + 0.09) * (double)rc.Width * 0.8))
				});
			}
		}
		if (num == 7)
		{
			int[] array = new int[2] { -1, 1 };
			foreach (int num5 in array)
			{
				for (int k = 0; k < 4; k++)
				{
					float num6 = ((num5 < 0) ? 0.1f : 0.9f);
					float num7 = (float)num5 * (0.3f + 0.12f * (float)k);
					float num8 = -0.02f + 0.13f * (float)k;
					using LinearGradientBrush brush2 = new LinearGradientBrush(new RectangleF(rectangleF.X - rectangleF.Width * 0.5f, rectangleF.Y, rectangleF.Width * 2f, rectangleF.Height), Color.FromArgb(240, 236, 252, 255), Color.FromArgb(200, 120, 200, 235), 0f);
					g.FillPolygon(brush2, new PointF[4]
					{
						P(rectangleF, num6, 0.3 + (double)num8 * 0.5),
						P(rectangleF, num6 + num7, 0.1 + (double)num8),
						P(rectangleF, (double)num6 + (double)num7 * 0.9, 0.3 + (double)num8),
						P(rectangleF, num6, 0.46 + (double)num8 * 0.4)
					});
				}
			}
		}
		if (num == 9)
		{
			using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(255, 255, 196, 70)))
			{
				g.FillPolygon(brush3, new PointF[7]
				{
					P(rectangleF, 0.12, 0.2),
					P(rectangleF, 0.24, -0.2),
					P(rectangleF, 0.37, 0.08),
					P(rectangleF, 0.5, -0.28),
					P(rectangleF, 0.63, 0.08),
					P(rectangleF, 0.76, -0.2),
					P(rectangleF, 0.88, 0.2)
				});
			}
			using (Pen pen = new Pen(Color.FromArgb(255, 255, 240, 170), 1.2f))
			{
				g.DrawPolygon(pen, new PointF[7]
				{
					P(rectangleF, 0.12, 0.2),
					P(rectangleF, 0.24, -0.2),
					P(rectangleF, 0.37, 0.08),
					P(rectangleF, 0.5, -0.28),
					P(rectangleF, 0.63, 0.08),
					P(rectangleF, 0.76, -0.2),
					P(rectangleF, 0.88, 0.2)
				});
			}
			double[] array2 = new double[3] { 0.24, 0.5, 0.76 };
			foreach (double num9 in array2)
			{
				using SolidBrush brush4 = new SolidBrush(Color.FromArgb(240, 255, 90, 110));
				g.FillEllipse(brush4, (float)((double)rectangleF.X + (double)rectangleF.Width * num9 - (double)rectangleF.Width * 0.035), (float)((double)rectangleF.Y - (double)rectangleF.Height * ((num9 == 0.5) ? 0.26 : 0.18)), rectangleF.Width * 0.07f, rectangleF.Width * 0.07f);
			}
		}
		using (GraphicsPath path = Forme(num, new RectangleF(rectangleF.X, rectangleF.Y + rectangleF.Height * 0.05f, rectangleF.Width, rectangleF.Height)))
		{
			using SolidBrush brush5 = new SolidBrush(Color.FromArgb(110, 0, 0, 0));
			g.FillPath(brush5, path);
		}
		using (GraphicsPath graphicsPath2 = Forme(num, rectangleF))
		{
			using (LinearGradientBrush brush6 = new LinearGradientBrush(rectangleF, color, color2, 60f))
			{
				g.FillPath(brush6, graphicsPath2);
			}
			float num10 = rectangleF.Width * 0.1f;
			RectangleF rectangleF2 = new RectangleF(rectangleF.X + num10, rectangleF.Y + num10, rectangleF.Width - 2f * num10, rectangleF.Height - 2f * num10);
			using (GraphicsPath graphicsPath3 = Forme(num, rectangleF2))
			{
				using (LinearGradientBrush brush7 = new LinearGradientBrush(rectangleF2, ControlPaint.Dark(color2, 0.3f), ControlPaint.Light(color2, 0.2f), 240f))
				{
					g.FillPath(brush7, graphicsPath3);
				}
				using (Pen pen2 = new Pen(Color.FromArgb(120, 0, 0, 0), Math.Max(1f, (float)rc.Width / 38f)))
				{
					g.DrawPath(pen2, graphicsPath3);
				}
				using (Pen pen3 = new Pen(Color.FromArgb(150, ControlPaint.Light(color, 0.6f)), Math.Max(0.8f, (float)rc.Width / 56f)))
				{
					g.DrawPath(pen3, graphicsPath2);
				}
				GraphicsState gstate = g.Save();
				g.SetClip(graphicsPath3, CombineMode.Intersect);
				if (num == 3 || num == 6 || num == 8)
				{
					Facettes(g, graphicsPath3, rectangleF2, 44);
				}
				if (num == 2)
				{
					using SolidBrush brush8 = new SolidBrush(Color.FromArgb(46, 255, 255, 255));
					g.FillRectangle(brush8, rectangleF2.X + rectangleF2.Width * 0.45f, rectangleF2.Y, rectangleF2.Width * 0.1f, rectangleF2.Height);
				}
				g.Restore(gstate);
			}
			using (Pen pen4 = new Pen(Color.FromArgb(230, ControlPaint.Light(color, 0.55f)), Math.Max(1.1f, (float)rc.Width / 26f)))
			{
				g.DrawPath(pen4, graphicsPath2);
			}
			if (num == 1 || num == 2 || num == 5)
			{
				PointF[] pathPoints = graphicsPath2.PathPoints;
				for (int j = 0; j < pathPoints.Length; j++)
				{
					PointF pointF = pathPoints[j];
					using SolidBrush brush9 = new SolidBrush(Color.FromArgb(220, ControlPaint.Light(color, 0.7f)));
					g.FillEllipse(brush9, pointF.X - (float)rc.Width * 0.035f, pointF.Y - (float)rc.Width * 0.035f, (float)rc.Width * 0.07f, (float)rc.Width * 0.07f);
				}
			}
			GraphicsState gstate2 = g.Save();
			g.SetClip(graphicsPath2, CombineMode.Intersect);
			using (GraphicsPath graphicsPath4 = new GraphicsPath())
			{
				graphicsPath4.AddEllipse(new RectangleF(rectangleF.X + rectangleF.Width * 0.1f, rectangleF.Y + rectangleF.Height * 0.02f, rectangleF.Width * 0.8f, rectangleF.Height * 0.46f));
				using LinearGradientBrush brush10 = new LinearGradientBrush(new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height * 0.5f), Color.FromArgb(118, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f);
				g.FillPath(brush10, graphicsPath4);
			}
			if (num == 8)
			{
				ColorBlend colorBlend = new ColorBlend(5);
				colorBlend.Colors = new Color[5]
				{
					Color.FromArgb(78, 255, 120, 160),
					Color.FromArgb(78, 255, 230, 110),
					Color.FromArgb(78, 110, 255, 190),
					Color.FromArgb(78, 110, 170, 255),
					Color.FromArgb(78, 220, 130, 255)
				};
				colorBlend.Positions = new float[5] { 0f, 0.25f, 0.5f, 0.75f, 1f };
				float num11 = (float)(t * 0.5 % 1.0) * rectangleF.Width * 2f;
				using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new RectangleF(rectangleF.X - rectangleF.Width + num11, rectangleF.Y, rectangleF.Width * 2f, rectangleF.Height), Color.White, Color.White, 20f);
				linearGradientBrush.InterpolationColors = colorBlend;
				g.FillRectangle(linearGradientBrush, rectangleF);
			}
			if (num >= 1)
			{
				float num12 = rectangleF.X + (float)((t * 0.5 + (double)num * 0.17) % 2.0 - 0.5) * rectangleF.Width * 1.5f - rectangleF.Width * 0.3f;
				using SolidBrush brush11 = new SolidBrush(Color.FromArgb(92, 255, 255, 255));
				g.FillPolygon(brush11, new PointF[4]
				{
					new PointF(num12, rectangleF.Y),
					new PointF(num12 + rectangleF.Width * 0.2f, rectangleF.Y),
					new PointF(num12 + rectangleF.Width * 0.2f - rectangleF.Height * 0.35f, rectangleF.Bottom),
					new PointF(num12 - rectangleF.Height * 0.35f, rectangleF.Bottom)
				});
			}
			g.Restore(gstate2);
		}
		if (num == 9)
		{
			int[] array = new int[2] { -1, 1 };
			foreach (int num13 in array)
			{
				for (int l = 0; l < 6; l++)
				{
					double num14 = Math.PI * (0.62 + 0.12 * (double)l) * (double)((num13 < 0) ? 1 : (-1)) + ((num13 < 0) ? 0.0 : Math.PI);
					float num15 = (float)((double)(rc.X + rc.Width / 2) + Math.Cos(num14) * (double)rc.Width * 0.47);
					float num16 = (float)((double)rc.Y + (double)rc.Height * 0.56 + Math.Sin(num14) * (double)rc.Height * 0.4);
					using SolidBrush brush12 = new SolidBrush(Color.FromArgb(220, 255, 200, 90));
					g.FillEllipse(brush12, num15 - (float)rc.Width * 0.04f, num16 - (float)rc.Width * 0.025f, (float)rc.Width * 0.08f, (float)rc.Width * 0.05f);
				}
			}
		}
		string text = niv.ToString();
		float emSize = rectangleF.Height * ((text.Length >= 3) ? 0.36f : 0.5f);
		using (Font font = new Font("Segoe UI Black", emSize, FontStyle.Regular, GraphicsUnit.Pixel))
		{
			SizeF sizeF = g.MeasureString(text, font, 200, StringFormat.GenericTypographic);
			float num17 = rectangleF.X + (rectangleF.Width - sizeF.Width) / 2f;
			float num18 = rectangleF.Y + (rectangleF.Height - sizeF.Height) / 2f + ((num == Zx.P(9)) ? (rectangleF.Height * 0.04f) : 0f);
			using (SolidBrush brush13 = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
			{
				float[][] array3 = new float[3][]
				{
					new float[2] { 1.2f, 1.4f },
					new float[2] { -0.8f, 0.8f },
					new float[2] { 0.8f, -0.8f }
				};
				foreach (float[] array4 in array3)
				{
					g.DrawString(text, font, brush13, num17 + array4[0], num18 + array4[1], StringFormat.GenericTypographic);
				}
			}
			using LinearGradientBrush brush14 = new LinearGradientBrush(new RectangleF(num17, num18, Math.Max(Zx.P(2), sizeF.Width), Math.Max(Zx.P(2), sizeF.Height)), Color.White, Color.FromArgb(255, 226, 232, 248), 90f);
			g.DrawString(text, font, brush14, num17, num18, StringFormat.GenericTypographic);
		}
		if (num == 4)
		{
			for (int m = 0; m < 3; m++)
			{
				double num19 = t * 1.7 + (double)m * 2.094;
				Etoile(g, (float)((double)(rc.X + rc.Width / 2) + Math.Cos(num19) * (double)rc.Width * 0.58), (float)((double)(rc.Y + rc.Height / 2) + Math.Sin(num19) * (double)rc.Height * 0.58), (float)rc.Width * 0.07f, Color.FromArgb(235, 190, 220, 255));
			}
		}
		if (num == 5)
		{
			for (int n = 0; n < 5; n++)
			{
				float num20 = (float)rc.Height * (0.14f + 0.1f * (float)Math.Abs(Math.Sin(t * 5.0 + (double)n * 1.7)));
				float num21 = (float)rc.X + (float)rc.Width * (0.22f + 0.14f * (float)n);
				using SolidBrush brush15 = new SolidBrush(Color.FromArgb(200, 255, 150 + n * 12, 60));
				g.FillPolygon(brush15, new PointF[3]
				{
					new PointF(num21 - (float)rc.Width * 0.06f, (float)rc.Y + (float)rc.Height * 0.16f),
					new PointF(num21, (float)rc.Y + (float)rc.Height * 0.16f - num20),
					new PointF(num21 + (float)rc.Width * 0.06f, (float)rc.Y + (float)rc.Height * 0.16f)
				});
			}
		}
		if (num == 6)
		{
			for (int num22 = 0; num22 < 8; num22++)
			{
				double num23 = (t * 0.45 + (double)num22 * 0.13) % 1.0;
				Etoile(g, (float)rc.X + (float)rc.Width * (0.14f + 0.1f * (float)num22), (float)((double)rc.Bottom - num23 * (double)rc.Height), (float)rc.Width * 0.05f * (1f - (float)num23 * 0.5f), Color.FromArgb((int)(240.0 * (1.0 - num23)), 235, 200, 255));
			}
		}
		if (num == 9)
		{
			for (int num24 = 0; num24 < 12; num24++)
			{
				double num25 = (0.0 - t) * 1.3 + (double)num24 * Math.PI / 6.0;
				double num26 = 0.62 + 0.04 * Math.Sin(t * 3.0 + (double)num24);
				Color c = ((num24 % 2 == 0) ? Color.FromArgb(255, 255, 150, 70) : Color.FromArgb(255, 110, 140, 255));
				Brume2(g, (float)((double)(rc.X + rc.Width / 2) + Math.Cos(num25) * (double)rc.Width * num26), (float)((double)(rc.Y + rc.Height / 2) + Math.Sin(num25) * (double)rc.Height * num26), (float)rc.Width * 0.09f, c);
			}
		}
		g.SmoothingMode = smoothingMode;
	}

	private static void Brume2(Graphics g, float cx, float cy, float rad, Color c)
	{
		RectangleF rect = new RectangleF(cx - rad, cy - rad, rad * 2f, rad * 2f);
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(rect);
		using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
		pathGradientBrush.CenterColor = Color.FromArgb(230, c);
		pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, c) };
		g.FillEllipse(pathGradientBrush, rect);
	}

	public static void Anneau(Graphics g, Rectangle av, int niv, double t)
	{
		int num = Progression.Palier(niv);
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle rect = Rectangle.Inflate(av, Zx.P(5), Zx.P(5));
		Color baseColor = Hi[num];
		using (Pen pen = new Pen(Color.FromArgb((num == 0) ? 150 : 220, baseColor), (num >= Zx.P(5)) ? Zx.P(3.5f) : Zx.P(2.5f)))
		{
			g.DrawEllipse(pen, rect);
		}
		if (num >= 1)
		{
			for (int i = 0; i < 14; i++)
			{
				using Pen pen2 = new Pen(Color.FromArgb(25 + i * 16, ControlPaint.Light(baseColor, 0.5f)), Zx.P(4f));
				g.DrawArc(pen2, rect, (float)(t * (double)(Zx.P(70) + Zx.P(20) * num) + (double)(i * 5)), Zx.P(7));
			}
		}
		if (num >= 3)
		{
			Rectangle rect2 = Rectangle.Inflate(rect, Zx.P(4), Zx.P(4));
			float num2 = 0.5f + 0.5f * (float)Math.Sin(t * 2.4);
			using Pen pen3 = new Pen(Color.FromArgb((int)(40f + 80f * num2), baseColor), Zx.P(2f));
			g.DrawEllipse(pen3, rect2);
		}
		if (num >= 5)
		{
			for (int j = 0; j < 6; j++)
			{
				double num3 = t * 0.9 + (double)j * Math.PI / 3.0;
				double num4 = 0.5 + 0.5 * Math.Sin(t * 4.0 + (double)j);
				Etoile(g, (float)((double)rect.X + (double)rect.Width / 2.0 + Math.Cos(num3) * ((double)rect.Width / 2.0 + (double)Zx.P(7))), (float)((double)rect.Y + (double)rect.Height / 2.0 + Math.Sin(num3) * ((double)rect.Height / 2.0 + (double)Zx.P(7))), (float)(2.5 + 2.5 * num4), Color.FromArgb((int)(120.0 + 130.0 * num4), ControlPaint.Light(baseColor, 0.6f)));
			}
		}
		if (num == 9)
		{
			Rectangle rect3 = Rectangle.Inflate(rect, Zx.P(9), Zx.P(9));
			for (int k = 0; k < 16; k++)
			{
				using Pen pen4 = new Pen(Color.FromArgb(25 + k * 14, (k % 2 == 0) ? Color.FromArgb(255, 150, 70) : Color.FromArgb(110, 140, 255)), Zx.P(3f));
				g.DrawArc(pen4, rect3, (float)((0.0 - t) * (double)Zx.P(90) + (double)(k * 6)), Zx.P(6));
			}
		}
		g.SmoothingMode = smoothingMode;
	}
}
