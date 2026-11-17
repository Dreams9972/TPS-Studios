using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FondAnime
{
	private const int Marge = 100;

	private Bitmap grand;

	private volatile bool pret;

	private volatile bool enCours;

	private double t = 40.0;

	public Action QuandPret;

	private float[] px;

	private float[] py;

	private float[] pv;

	private float[] pr;

	private float[] pph;

	private SolidBrush[] noyaux = new SolidBrush[8];

	private SolidBrush[] halos = new SolidBrush[8];

	private Bitmap teinte;

	private string cleTeinte;

	public bool Pret => pret;

	public FondAnime()
	{
		Random random = new Random(11);
		int num = 26;
		px = new float[num];
		py = new float[num];
		pv = new float[num];
		pr = new float[num];
		pph = new float[num];
		for (int i = 0; i < num; i++)
		{
			px[i] = (float)random.NextDouble();
			py[i] = (float)random.NextDouble();
			pv[i] = 0.004f + (float)random.NextDouble() * 0.01f;
			pr[i] = 1.2f + (float)random.NextDouble() * 2f;
			pph[i] = (float)random.NextDouble() * 6.28f;
		}
		for (int j = 0; j < 8; j++)
		{
			noyaux[j] = new SolidBrush(Color.FromArgb(40 + j * 13, 205, 212, 255));
			halos[j] = new SolidBrush(Color.FromArgb(6 + j * 4, 150, 160, 255));
		}
	}

	public void Preparer(bool synchrone)
	{
		if (pret || enCours)
		{
			return;
		}
		enCours = true;
		int w = Zx.P(1280);
		int h = Zx.P(720);
		try
		{
			Screen[] allScreens = Screen.AllScreens;
			foreach (Screen screen in allScreens)
			{
				w = Math.Max(w, screen.Bounds.Width);
				h = Math.Max(h, screen.Bounds.Height);
			}
		}
		catch
		{
		}
		ThreadStart threadStart = delegate
		{
			try
			{
				Construire(w + 200 + Zx.P(40), h + 200 + Zx.P(40));
			}
			catch (Exception e)
			{
				Journal.Ecrire("fond", e);
			}
			enCours = false;
			if (pret && QuandPret != null)
			{
				try
				{
					QuandPret();
				}
				catch
				{
				}
			}
		};
		if (synchrone)
		{
			threadStart();
			return;
		}
		Thread thread = new Thread(threadStart);
		thread.IsBackground = true;
		thread.Priority = ThreadPriority.BelowNormal;
		thread.Start();
	}

	private static void Base(Graphics g, Rectangle r)
	{
		using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height + 1), Color.FromArgb(17, 20, 44), Color.FromArgb(7, 9, 21), 90f);
		g.FillRectangle(brush, r);
	}

	private void Construire(int w, int h)
	{
		Bitmap image = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(image))
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Base(graphics, new Rectangle(0, 0, w, h));
			Lueur(graphics, w, h, 0.2, 0.26, 0.42, C.Jade, 40);
			Lueur(graphics, w, h, 0.82, 0.28, 0.46, Color.FromArgb(120, 70, 230), 34);
			Lueur(graphics, w, h, 0.5, 0.9, 0.4, C.JadeFonce, 28);
			Lueur(graphics, w, h, 0.08, 0.84, 0.28, C.Jade, 18);
			Lueur(graphics, w, h, 0.94, 0.84, 0.28, C.Orange, 20);
		}
		grand = image;
		pret = true;
	}

	private static void Lueur(Graphics g, int w, int h, double cx, double cy, double rayon, Color c, int alpha)
	{
		float num = (float)(rayon * (double)w);
		float num2 = (float)(cx * (double)w);
		float num3 = (float)(cy * (double)h);
		RectangleF rect = new RectangleF(num2 - num, num3 - num * 0.8f, num * 2f, num * 1.6f);
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(rect);
		using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
		pathGradientBrush.CenterColor = Color.FromArgb(alpha, c);
		pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, c) };
		g.FillEllipse(pathGradientBrush, rect);
	}

	public void Avancer(double dt)
	{
		t += dt;
	}

	public void PeindreTeinte(Graphics g, Rectangle dest, string cle, Action<Graphics, int, int> teinter)
	{
		if (!pret || teinter == null)
		{
			Peindre(g, dest);
			return;
		}
		if (teinte == null || cleTeinte != cle || teinte.Size != grand.Size)
		{
			if (teinte != null)
			{
				teinte.Dispose();
			}
			teinte = new Bitmap(grand.Width, grand.Height, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(teinte))
			{
				graphics.DrawImageUnscaled(grand, 0, 0);
				teinter(graphics, teinte.Width, teinte.Height);
			}
			cleTeinte = cle;
		}
		Bitmap bitmap = grand;
		grand = teinte;
		try
		{
			Peindre(g, dest);
		}
		finally
		{
			grand = bitmap;
		}
	}

	/// <summary>Fond « mesh » : grandes taches de couleur floues qui se déplacent lentement (activé par défaut).</summary>
	public static bool Mesh = true;

	/// <summary>Temps de l'animation mesh (avancé seulement quand une page entière est redessinée).</summary>
	public double TempsMesh = 12.0;

	private Bitmap meshPetit;

	private Bitmap meshGrand;

	private double meshT = -1.0;

	private Size meshTaille = Size.Empty;

	private static readonly Color[] meshCouleurs = new Color[6]
	{
		// uniquement des nuances du bleu du logo TPS (#283375)
		Color.FromArgb(210, 56, 72, 168),
		Color.FromArgb(185, 40, 51, 135),
		Color.FromArgb(150, 66, 86, 190),
		Color.FromArgb(130, 40, 51, 117),
		Color.FromArgb(140, 30, 40, 100),
		Color.FromArgb(120, 50, 64, 150)
	};

	private void Tache(Graphics g, int w, int h, int i, double t)
	{
		double cx;
		double cy;
		double r;
		switch (i)
		{
		case 0:
			cx = 0.14 + 0.12 * Math.Sin(t * 0.13);
			cy = 0.22 + 0.14 * Math.Cos(t * 0.11);
			r = 0.62;
			break;
		case 1:
			cx = 0.80 + 0.12 * Math.Cos(t * 0.09 + 0.7);
			cy = 0.20 + 0.12 * Math.Sin(t * 0.12);
			r = 0.55;
			break;
		case 2:
			cx = 0.55 + 0.18 * Math.Sin(t * 0.07 + 1.0);
			cy = 0.88 + 0.08 * Math.Cos(t * 0.10);
			r = 0.60;
			break;
		case 3:
			cx = 0.93 + 0.05 * Math.Sin(t * 0.15);
			cy = 0.92 + 0.06 * Math.Cos(t * 0.13 + 0.4);
			r = 0.36;
			break;
		case 4:
			cx = 0.08 + 0.08 * Math.Cos(t * 0.12 + 2.0);
			cy = 0.86 + 0.08 * Math.Sin(t * 0.09);
			r = 0.40;
			break;
		default:
			cx = 0.42 + 0.20 * Math.Cos(t * 0.05 + 3.0);
			cy = 0.45 + 0.16 * Math.Sin(t * 0.06 + 1.5);
			r = 0.48;
			break;
		}
		float rr = (float)(r * (double)Math.Max(w, h));
		float x = (float)(cx * (double)w);
		float y = (float)(cy * (double)h);
		using GraphicsPath p = new GraphicsPath();
		p.AddEllipse(x - rr, y - rr * 0.8f, rr * 2f, rr * 1.6f);
		using PathGradientBrush b = new PathGradientBrush(p);
		b.CenterPoint = new PointF(x, y);
		b.CenterColor = meshCouleurs[i];
		b.SurroundColors = new Color[1] { Color.FromArgb(0, meshCouleurs[i]) };
		Blend bl = new Blend(3);
		bl.Factors = new float[3] { 0f, 0.55f, 1f };
		bl.Positions = new float[3] { 0f, 0.5f, 1f };
		b.Blend = bl;
		g.FillPath(b, p);
	}

	/// <summary>Fond mesh : rendu en très basse résolution (taches très floues), agrandi et gardé en cache.</summary>
	private void PeindreMesh(Graphics g, Rectangle dest)
	{
		Size taille = dest.Size;
		if (meshGrand == null || meshTaille != taille || Math.Abs(meshT - TempsMesh) > 1E-06)
		{
			int sw = Math.Max(48, taille.Width / 14);
			int sh = Math.Max(30, taille.Height / 14);
			if (meshPetit == null || meshPetit.Width != sw || meshPetit.Height != sh)
			{
				meshPetit?.Dispose();
				meshPetit = new Bitmap(sw, sh, PixelFormat.Format32bppPArgb);
			}
			using (Graphics gp = Graphics.FromImage(meshPetit))
			{
				gp.Clear(Color.FromArgb(8, 11, 30));
				gp.SmoothingMode = SmoothingMode.AntiAlias;
				for (int i = 0; i < meshCouleurs.Length; i++)
				{
					Tache(gp, sw, sh, i, TempsMesh);
				}
				// voile sombre léger : le texte reste bien lisible
				using SolidBrush v = new SolidBrush(Color.FromArgb(70, 6, 8, 20));
				gp.FillRectangle(v, 0, 0, sw, sh);
			}
			if (meshGrand == null || meshGrand.Size != taille)
			{
				meshGrand?.Dispose();
				meshGrand = new Bitmap(Math.Max(1, taille.Width), Math.Max(1, taille.Height), PixelFormat.Format32bppPArgb);
			}
			using (Graphics gg = Graphics.FromImage(meshGrand))
			{
				gg.CompositingMode = CompositingMode.SourceCopy;
				gg.InterpolationMode = InterpolationMode.HighQualityBilinear;
				gg.PixelOffsetMode = PixelOffsetMode.Half;
				gg.DrawImage(meshPetit, new Rectangle(0, 0, taille.Width, taille.Height), 0.5f, 0.5f, (float)sw - 1f, (float)sh - 1f, GraphicsUnit.Pixel);
			}
			meshTaille = taille;
			meshT = TempsMesh;
		}
		CompositingMode cm = g.CompositingMode;
		g.CompositingMode = CompositingMode.SourceCopy;
		g.DrawImageUnscaled(meshGrand, dest.X, dest.Y);
		g.CompositingMode = cm;
	}

	public void Peindre(Graphics g, Rectangle dest)
	{
		if (dest.Width <= 0 || dest.Height <= 0)
		{
			return;
		}
		if (Mesh)
		{
			PeindreMesh(g, dest);
			return;
		}
		if (!pret)
		{
			Base(g, dest);
			return;
		}
		int num = (int)(100.0 * (0.5 + 0.45 * Math.Sin(t * 0.11)));
		int num2 = (int)(100.0 * (0.5 + 0.45 * Math.Cos(t * 0.083)));
		int num3 = Math.Min(dest.Width, grand.Width - num);
		int num4 = Math.Min(dest.Height, grand.Height - num2);
		CompositingMode compositingMode = g.CompositingMode;
		g.CompositingMode = CompositingMode.SourceCopy;
		g.DrawImage(grand, new Rectangle(dest.X, dest.Y, num3, num4), num, num2, num3, num4, GraphicsUnit.Pixel);
		g.CompositingMode = compositingMode;
		if (num3 < dest.Width || num4 < dest.Height)
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb(8, 10, 24));
			if (num3 < dest.Width)
			{
				g.FillRectangle(brush, dest.X + num3, dest.Y, dest.Width - num3, dest.Height);
			}
			if (num4 < dest.Height)
			{
				g.FillRectangle(brush, dest.X, dest.Y + num4, dest.Width, dest.Height - num4);
			}
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float num5 = Math.Max(1f, (float)dest.Width / 1180f);
		for (int i = 0; i < 0; i++)
		{
			float num6 = (py[i] - (float)t * pv[i]) % 1f;
			if (num6 < 0f)
			{
				num6 += 1f;
			}
			float num7 = px[i] + 0.015f * (float)Math.Sin(t * 0.4 + (double)pph[i]);
			int num8 = Math.Max(0, Math.Min(7, (int)(3.5 + 3.5 * Math.Sin(t * 0.8 + (double)pph[i]))));
			float num9 = pr[i] * 1.6f * num5;
			float num10 = (float)dest.X + num7 * (float)dest.Width;
			float num11 = (float)dest.Y + num6 * (float)dest.Height;
			g.FillEllipse(halos[num8], num10 - num9 * 2.6f, num11 - num9 * 2.6f, num9 * 5.2f, num9 * 5.2f);
			g.FillEllipse(noyaux[num8], num10 - num9, num11 - num9, num9 * 2f, num9 * 2f);
		}
		g.SmoothingMode = smoothingMode;
	}
}
