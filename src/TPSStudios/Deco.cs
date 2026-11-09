using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TPSStudios;

internal static class Deco
{
	public static readonly string[] Ids = new string[2] { "couronne", "flammes" };

	public static readonly string[] Noms = new string[2] { "Couronne dorée", "Anneau de flammes" };

	public static readonly int[] Prix = new int[2] { 400, 600 };

	public static bool Disponible(string id)
	{
		if (!string.IsNullOrEmpty(id) && Social.Vip)
		{
			return Social.Possede("deco:" + id);
		}
		return false;
	}

	private static void Lueur(Graphics g, float cx, float cy, float r, Color c, int a)
	{
		BanAnim.Brume(g, cx, cy, r, r, c, a);
	}

	public static void Dessiner(Graphics g, Rectangle ra, string id, double t)
	{
		if (Disponible(id))
		{
			Forcer(g, ra, id, t);
		}
	}

	public static void Forcer(Graphics g, Rectangle ra, string id, double t)
	{
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (id == "couronne")
		{
			Couronne(g, ra, t);
		}
		else if (id == "flammes")
		{
			Flammes(g, ra, t);
		}
		g.SmoothingMode = smoothingMode;
	}

	private static void Couronne(Graphics g, Rectangle ra, double t)
	{
		float num = (float)ra.Width * 0.5f;
		float dx = (float)ra.Right - (float)ra.Width * 0.2f;
		float dy = (float)ra.Y + (float)ra.Height * 0.04f + (float)Math.Sin(t * 2.2) * 2f;
		GraphicsState gstate = g.Save();
		g.TranslateTransform(dx, dy);
		g.RotateTransform(22f);
		PointF[] array = new PointF[7]
		{
			new PointF((0f - num) * 0.5f, num * 0.22f),
			new PointF((0f - num) * 0.56f, (0f - num) * 0.3f),
			new PointF((0f - num) * 0.24f, (0f - num) * 0.04f),
			new PointF(0f, (0f - num) * 0.42f),
			new PointF(num * 0.24f, (0f - num) * 0.04f),
			new PointF(num * 0.56f, (0f - num) * 0.3f),
			new PointF(num * 0.5f, num * 0.22f)
		};
		Lueur(g, 0f, 0f, num * 0.9f, Color.FromArgb(255, 200, 80), Zx.P(70));
		using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF((0f - num) * 0.6f, (0f - num) * 0.45f, num * 1.2f, num * 0.7f), Color.FromArgb(255, 240, 160), Color.FromArgb(200, 130, 20), 90f))
		{
			g.FillPolygon(brush, array);
		}
		using (Pen pen = new Pen(Color.FromArgb(150, 90, 10), 1.2f))
		{
			pen.LineJoin = LineJoin.Round;
			g.DrawPolygon(pen, array);
		}
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(255, 230, 140)))
		{
			g.FillRectangle(brush2, (0f - num) * 0.5f, num * 0.12f, num, num * 0.1f);
		}
		Color[] array2 = new Color[3]
		{
			Color.FromArgb(230, 40, 70),
			Color.FromArgb(60, 140, 255),
			Color.FromArgb(230, 40, 70)
		};
		for (int i = 0; i < 3; i++)
		{
			float num2 = (float)(i - 1) * num * 0.3f;
			using SolidBrush brush3 = new SolidBrush(array2[i]);
			g.FillEllipse(brush3, num2 - num * 0.06f, num * 0.02f, num * 0.12f, num * 0.12f);
		}
		PointF[] array3 = new PointF[3]
		{
			array[1],
			array[3],
			array[5]
		};
		for (int j = 0; j < array3.Length; j++)
		{
			PointF pointF = array3[j];
			using SolidBrush brush4 = new SolidBrush(Color.FromArgb(255, 250, 220));
			g.FillEllipse(brush4, pointF.X - num * 0.06f, pointF.Y - num * 0.06f, num * 0.12f, num * 0.12f);
		}
		float num3 = (float)Math.Max(0.0, Math.Sin(t * 3.0));
		if (num3 > 0.2f)
		{
			using SolidBrush brush5 = new SolidBrush(Color.FromArgb((int)(255f * num3), 255, 255, 255));
			float num4 = num * 0.3f;
			float num5 = (0f - num) * 0.2f;
			float num6 = num * 0.16f * num3;
			g.FillPolygon(brush5, new PointF[4]
			{
				new PointF(num4, num5 - num6),
				new PointF(num4 + num6 * 0.2f, num5),
				new PointF(num4, num5 + num6),
				new PointF(num4 - num6 * 0.2f, num5)
			});
			g.FillPolygon(brush5, new PointF[4]
			{
				new PointF(num4 - num6, num5),
				new PointF(num4, num5 - num6 * 0.2f),
				new PointF(num4 + num6, num5),
				new PointF(num4, num5 + num6 * 0.2f)
			});
		}
		g.Restore(gstate);
	}

	private static void Flammes(Graphics g, Rectangle ra, double t)
	{
		float num = (float)ra.X + (float)ra.Width / 2f;
		float num2 = (float)ra.Y + (float)ra.Height / 2f;
		float num3 = (float)ra.Width / 2f + 1f;
		int num4 = 26;
		Lueur(g, num, num2, num3 * 1.5f, Color.FromArgb(255, 120, 20), Zx.P(90));
		for (int i = 0; i < num4; i++)
		{
			double num5 = (double)i * Math.PI * 2.0 / (double)num4 + t * 0.4;
			double num6 = Math.Abs(Math.Sin(t * 7.0 + (double)i * 1.7)) * 0.6 + Math.Abs(Math.Sin(t * 4.3 + (double)i * 0.9)) * 0.4;
			float num7 = num3 * (0.16f + 0.2f * (float)num6);
			float num8 = num3 * 0.16f;
			double num9 = Math.Cos(num5);
			double num10 = Math.Sin(num5);
			double num11 = 0.0 - num10;
			double num12 = num9;
			PointF pt = new PointF(num + (float)(num9 * (double)num3 + num11 * (double)num8), num2 + (float)(num10 * (double)num3 + num12 * (double)num8));
			PointF pt2 = new PointF(num + (float)(num9 * (double)num3 - num11 * (double)num8), num2 + (float)(num10 * (double)num3 - num12 * (double)num8));
			float num13 = (float)Math.Sin(t * 9.0 + (double)i) * num7 * 0.25f;
			PointF pointF = new PointF(num + (float)(num9 * (double)(num3 + num7) + num11 * (double)num13), num2 + (float)(num10 * (double)(num3 + num7) + num12 * (double)num13));
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddBezier(pt, new PointF((pt.X + pointF.X) / 2f + (float)num11 * num8 * 0.6f, (pt.Y + pointF.Y) / 2f + (float)num12 * num8 * 0.6f), new PointF(pointF.X, pointF.Y), pointF);
			graphicsPath.AddBezier(pointF, new PointF(pointF.X, pointF.Y), new PointF((pt2.X + pointF.X) / 2f - (float)num11 * num8 * 0.6f, (pt2.Y + pointF.Y) / 2f - (float)num12 * num8 * 0.6f), pt2);
			graphicsPath.CloseFigure();
			using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(num + (float)(num9 * (double)num3), num2 + (float)(num10 * (double)num3)), pointF, Color.FromArgb(240, 255, 230, 120), Color.FromArgb(0, 255, 60, 0));
			ColorBlend colorBlend = new ColorBlend(3);
			colorBlend.Colors = new Color[3]
			{
				Color.FromArgb(240, 255, 240, 160),
				Color.FromArgb(220, 255, 130, 20),
				Color.FromArgb(0, 230, 40, 0)
			};
			colorBlend.Positions = new float[3] { 0f, 0.45f, 1f };
			linearGradientBrush.InterpolationColors = colorBlend;
			g.FillPath(linearGradientBrush, graphicsPath);
		}
		using Pen pen = new Pen(Color.FromArgb(220, 255, 170, 60), Zx.P(2.2f));
		g.DrawEllipse(pen, num - num3, num2 - num3, num3 * 2f, num3 * 2f);
	}
}
