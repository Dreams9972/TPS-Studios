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

	public void Peindre(Graphics g, Rectangle dest)
	{
		if (dest.Width <= 0 || dest.Height <= 0)
		{
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
