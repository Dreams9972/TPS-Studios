using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TPSStudios;

internal class Banniere
{
	public string Type = "degrade";

	public string Motif = "";

	public Color C1 = Color.FromArgb(86, 106, 255);

	public Color C2 = Color.FromArgb(38, 120, 230);

	public int Angle;

	public int Zoom = 100;

	public int X = 50;

	public int Y = 50;

	public Image Img;

	public static readonly string[] Motifs = new string[5] { "", "vagues", "points", "grille", "aurore" };

	public static readonly string[] MotifsNoms = new string[5] { "Aucun", "Vagues", "Points", "Grille", "Aurore" };

	public static readonly object[][] Modeles = new object[6][]
	{
		new object[5]
		{
			"Aurore",
			Color.FromArgb(86, 106, 255),
			Color.FromArgb(120, 84, 220),
			45,
			"aurore"
		},
		new object[5]
		{
			"Coucher de soleil",
			Color.FromArgb(255, 130, 70),
			Color.FromArgb(150, 40, 120),
			90,
			""
		},
		new object[5]
		{
			"Océan",
			Color.FromArgb(20, 120, 210),
			Color.FromArgb(8, 36, 84),
			90,
			"vagues"
		},
		new object[5]
		{
			"Forêt",
			Color.FromArgb(70, 175, 95),
			Color.FromArgb(14, 56, 42),
			135,
			"points"
		},
		new object[5]
		{
			"Néon",
			Color.FromArgb(235, 50, 165),
			Color.FromArgb(48, 28, 200),
			0,
			"grille"
		},
		new object[5]
		{
			"Métro",
			Color.FromArgb(86, 106, 255),
			Color.FromArgb(38, 120, 230),
			0,
			""
		}
	};

	public bool Animee => Type == "anim";

	public static Banniere De(Compte c)
	{
		Banniere banniere = new Banniere();
		if (c == null)
		{
			return banniere;
		}
		if (string.IsNullOrEmpty(c.BanType))
		{
			banniere.Type = "degrade";
			banniere.C1 = Comptes.Palette[Math.Abs(c.Banniere) % Comptes.Palette.Length];
			banniere.C2 = Color.FromArgb(16, 30, 54);
			banniere.Angle = 0;
			return banniere;
		}
		banniere.Type = c.BanType;
		banniere.C1 = Color.FromArgb((c.BanC1 == 0) ? Color.FromArgb(86, 106, 255).ToArgb() : c.BanC1);
		banniere.C2 = Color.FromArgb((c.BanC2 == 0) ? Color.FromArgb(38, 120, 230).ToArgb() : c.BanC2);
		banniere.Angle = c.BanAngle;
		banniere.Motif = c.BanMotif ?? "";
		banniere.Zoom = ((c.BanZoom <= 0) ? 100 : c.BanZoom);
		banniere.X = c.BanX;
		banniere.Y = c.BanY;
		if (banniere.Type == "image")
		{
			banniere.Img = Comptes.ImageBanniere(c);
		}
		if (banniere.Type == "anim" && !BanAnim.Disponible(banniere.Motif))
		{
			banniere.Type = "degrade";
		}
		return banniere;
	}

	public void Enregistrer(Compte c)
	{
		c.BanType = Type;
		c.BanC1 = C1.ToArgb();
		c.BanC2 = C2.ToArgb();
		c.BanAngle = Angle;
		c.BanMotif = Motif;
		c.BanZoom = Zoom;
		c.BanX = X;
		c.BanY = Y;
	}

	public static Banniere Hasard()
	{
		Random random = new Random();
		double num = random.NextDouble() * 360.0;
		double h = num + 40.0 + random.NextDouble() * 80.0;
		Banniere banniere = new Banniere();
		banniere.C1 = SelecteurCouleur.FromHsv(num, 0.72, 0.92);
		banniere.C2 = SelecteurCouleur.FromHsv(h, 0.8, 0.42);
		banniere.Angle = random.Next(8) * 45;
		banniere.Motif = Motifs[random.Next(Motifs.Length)];
		return banniere;
	}

	public void Dessiner(Graphics g, Rectangle r)
	{
		if (r.Width < 2 || r.Height < 2)
		{
			return;
		}
		GraphicsState gstate = g.Save();
		g.SetClip(r, CombineMode.Intersect);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		if (Type == "image" && Img != null)
		{
			double num = Math.Max((double)r.Width / (double)Img.Width, (double)r.Height / (double)Img.Height) * (double)Math.Max(100, Zoom) / 100.0;
			int num2 = (int)((double)Img.Width * num);
			int num3 = (int)((double)Img.Height * num);
			int x = r.X - (int)((double)((num2 - r.Width) * Math.Max(0, Math.Min(Zx.P(100), X))) / 100.0);
			int y = r.Y - (int)((double)((num3 - r.Height) * Math.Max(0, Math.Min(Zx.P(100), Y))) / 100.0);
			g.DrawImage(Img, x, y, num2, num3);
		}
		else if (Type == "anim")
		{
			BanAnim.Dessiner(Motif, g, r, BanAnim.Temps);
		}
		else if (Type == "uni")
		{
			using SolidBrush brush = new SolidBrush(C1);
			g.FillRectangle(brush, r);
		}
		else
		{
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height + 1), C1, C2, Angle))
			{
				g.FillRectangle(brush2, r);
			}
			Motif_(g, r);
		}
		g.Restore(gstate);
	}

	private void Motif_(Graphics g, Rectangle r)
	{
		if (Motif == "vagues")
		{
			for (int i = 0; i < 4; i++)
			{
				List<PointF> list = new List<PointF>();
				double num = (double)r.Y + (double)r.Height * (0.4 + 0.14 * (double)i);
				for (int j = r.X; j <= r.Right + 8; j += Zx.P(8))
				{
					list.Add(new PointF(j, (float)(num + (double)Zx.P(10) * Math.Sin((double)(j - r.X) / 70.0 + (double)i * 1.4))));
				}
				list.Add(new PointF(r.Right + Zx.P(8), r.Bottom + Zx.P(2)));
				list.Add(new PointF(r.X, r.Bottom + Zx.P(2)));
				using SolidBrush brush = new SolidBrush(Color.FromArgb(22 + i * 4, 255, 255, 255));
				g.FillPolygon(brush, list.ToArray());
			}
			return;
		}
		if (Motif == "points")
		{
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
			{
				for (int k = r.Y + Zx.P(12); k < r.Bottom; k += Zx.P(24))
				{
					for (int l = r.X + Zx.P(12) + k / 24 % 2 * Zx.P(12); l < r.Right; l += Zx.P(24))
					{
						g.FillEllipse(brush2, l - Zx.P(2), k - Zx.P(2), Zx.P(4), Zx.P(4));
					}
				}
				return;
			}
		}
		if (Motif == "grille")
		{
			using (Pen pen = new Pen(Color.FromArgb(34, 255, 255, 255)))
			{
				for (int m = r.X; m < r.Right; m += Zx.P(28))
				{
					g.DrawLine(pen, m, r.Y, m, r.Bottom);
				}
				for (int n = r.Y; n < r.Bottom; n += Zx.P(28))
				{
					g.DrawLine(pen, r.X, n, r.Right, n);
				}
				return;
			}
		}
		if (!(Motif == "aurore"))
		{
			return;
		}
		Color[] array = new Color[3]
		{
			Color.FromArgb(C1.R, C1.G, C1.B),
			Color.FromArgb(C2.R, C2.G, C2.B),
			Color.White
		};
		int[] array2 = new int[3] { 110, 110, 40 };
		for (int num2 = 0; num2 < 3; num2++)
		{
			RectangleF rect = new RectangleF((float)r.X + (float)r.Width * (0.1f + 0.3f * (float)num2), (float)r.Y - (float)r.Height * 0.3f, (float)r.Width * 0.5f, (float)r.Height * 1.6f);
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(rect);
			using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = Color.FromArgb(array2[num2], array[num2]);
			pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, array[num2]) };
			g.FillEllipse(pathGradientBrush, rect);
		}
	}
}
