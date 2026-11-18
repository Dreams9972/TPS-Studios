using System;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace TPSStudios;

/// <summary>
/// Couvertures adaptées à n'importe quel cadre : si le format de l'image est proche de celui du cadre, elle le remplit
/// (recadrage centré) ; sinon elle est affichée entière, sans rien couper (titres, logos), sur un fond fait
/// de la même image floutée et assombrie.
/// </summary>
internal static class Couvertures
{
	/// <summary>Part de l'image qu'on accepte de couper (en largeur ou en hauteur) avant de passer en « image entière sur fond flouté ».</summary>
	public const double PerteMax = 0.34;

	/// <summary>Vrai si l'image peut remplir le cadre sans perdre plus de PerteMax d'un côté.</summary>
	public static bool Remplit(Image im, int w, int h)
	{
		if (im == null || w < 1 || h < 1 || im.Width < 1 || im.Height < 1)
		{
			return true;
		}
		double ri = (double)im.Width / im.Height;
		double rc = (double)w / h;
		double perte = 1.0 - Math.Min(ri, rc) / Math.Max(ri, rc);
		return perte <= PerteMax;
	}

	/// <summary>Énergie (détails : contours, texte) de chaque colonne et de chaque ligne, calculée une fois par image.</summary>
	private class Profil
	{
		public double[] Col;

		public double[] Lig;
	}

	private static readonly ConditionalWeakTable<Image, Profil> profils = new ConditionalWeakTable<Image, Profil>();

	private static readonly object verrou = new object();

	private static Profil ProfilDe(Image im)
	{
		lock (verrou)
		{
			if (profils.TryGetValue(im, out var p))
			{
				return p;
			}
		}
		int pw = 96;
		int ph = Math.Max(8, (int)Math.Round(96.0 * im.Height / Math.Max(1, im.Width)));
		if (ph > 160)
		{
			ph = 160;
			pw = Math.Max(8, (int)Math.Round(160.0 * im.Width / Math.Max(1, im.Height)));
		}
		Profil pr = new Profil
		{
			Col = new double[pw],
			Lig = new double[ph]
		};
		try
		{
			using Bitmap b = new Bitmap(pw, ph, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(b))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBilinear;
				lock (im)
				{
					g.DrawImage(im, 0, 0, pw, ph);
				}
			}
			BitmapData d = b.LockBits(new Rectangle(0, 0, pw, ph), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int[] px = new int[pw * ph];
			System.Runtime.InteropServices.Marshal.Copy(d.Scan0, px, 0, px.Length);
			b.UnlockBits(d);
			double[] lum = new double[px.Length];
			for (int i = 0; i < px.Length; i++)
			{
				int c = px[i];
				lum[i] = 0.299 * ((c >> 16) & 0xFF) + 0.587 * ((c >> 8) & 0xFF) + 0.114 * (c & 0xFF);
			}
			for (int y = 1; y < ph - 1; y++)
			{
				for (int x = 1; x < pw - 1; x++)
				{
					int i = y * pw + x;
					double e = Math.Abs(lum[i + 1] - lum[i - 1]) + Math.Abs(lum[i + pw] - lum[i - pw]);
					pr.Col[x] += e;
					pr.Lig[y] += e;
				}
			}
		}
		catch
		{
		}
		lock (verrou)
		{
			try
			{
				profils.Add(im, pr);
			}
			catch
			{
			}
		}
		return pr;
	}

	/// <summary>
	/// Position (0 à 1) de la fenêtre de recadrage qui garde le plus de détails (titre, logo, véhicule),
	/// avec une légère préférence pour le centre.
	/// </summary>
	private static double Meilleure(double[] e, double part)
	{
		int n = e.Length;
		int f = Math.Max(1, (int)Math.Round(n * part));
		if (f >= n)
		{
			return 0.5;
		}
		double total = 0.0;
		foreach (double v in e)
		{
			total += v;
		}
		if (total <= 0.0)
		{
			return 0.5;
		}
		double s = 0.0;
		for (int i = 0; i < f; i++)
		{
			s += e[i];
		}
		double meilleur = double.MinValue;
		int pos = (n - f) / 2;
		for (int o = 0; o <= n - f; o++)
		{
			if (o > 0)
			{
				s += e[o + f - 1] - e[o - 1];
			}
			double centre = Math.Abs(o - (n - f) / 2.0) / Math.Max(1.0, (n - f) / 2.0);
			double score = s / total - 0.08 * centre;
			if (score > meilleur)
			{
				meilleur = score;
				pos = o;
			}
		}
		return (double)pos / (n - f);
	}

	/// <summary>Partie de l'image à afficher pour remplir un cadre w×h en gardant l'essentiel.</summary>
	public static RectangleF Cadrage(Image im, int w, int h)
	{
		double ri = (double)im.Width / im.Height;
		double rc = (double)w / Math.Max(1, h);
		if (Math.Abs(ri - rc) < 0.01)
		{
			return new RectangleF(0f, 0f, im.Width, im.Height);
		}
		Profil p = ProfilDe(im);
		if (ri > rc)
		{
			float sw = (float)(im.Height * rc);
			double t = Meilleure(p.Col, sw / im.Width);
			return new RectangleF((float)((im.Width - sw) * t), 0f, sw, im.Height);
		}
		float sh = (float)(im.Width / rc);
		double t2 = Meilleure(p.Lig, sh / im.Height);
		return new RectangleF(0f, (float)((im.Height - sh) * t2), im.Width, sh);
	}

	public static Bitmap Composer(Image im, int w, int h)
	{
		Bitmap b = new Bitmap(Math.Max(1, w), Math.Max(1, h), PixelFormat.Format32bppPArgb);
		using Graphics g = Graphics.FromImage(b);
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		if (im == null)
		{
			using LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(40, 48, 96), Color.FromArgb(18, 22, 48), 45f);
			g.FillRectangle(lg, 0, 0, w, h);
			return b;
		}
		if (Remplit(im, w, h))
		{
			Couvrir(g, im, new Rectangle(0, 0, w, h));
			return b;
		}
		// fond : la même image, très floutée (réduite puis agrandie deux fois) et assombrie
		int pw = Math.Max(2, w / 24);
		int ph = Math.Max(2, h / 24);
		using (Bitmap petit = new Bitmap(pw, ph, PixelFormat.Format32bppPArgb))
		{
			using (Graphics gp = Graphics.FromImage(petit))
			{
				gp.InterpolationMode = InterpolationMode.HighQualityBilinear;
				Couvrir(gp, im, new Rectangle(0, 0, pw, ph));
			}
			using Bitmap moyen = new Bitmap(Math.Max(2, w / 6), Math.Max(2, h / 6), PixelFormat.Format32bppPArgb);
			using (Graphics gm = Graphics.FromImage(moyen))
			{
				gm.InterpolationMode = InterpolationMode.HighQualityBilinear;
				gm.PixelOffsetMode = PixelOffsetMode.Half;
				gm.DrawImage(petit, new Rectangle(0, 0, moyen.Width, moyen.Height));
			}
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.PixelOffsetMode = PixelOffsetMode.Half;
			g.DrawImage(moyen, new Rectangle(-Zx.P(8), -Zx.P(8), w + Zx.P(16), h + Zx.P(16)));
		}
		using (SolidBrush voile = new SolidBrush(Color.FromArgb(110, 6, 8, 22)))
		{
			g.FillRectangle(voile, 0, 0, w, h);
		}
		// image entière, centrée, avec une légère ombre
		double k = Math.Min((double)w / im.Width, (double)h / im.Height);
		int iw = Math.Max(1, (int)Math.Round(im.Width * k));
		int ih = Math.Max(1, (int)Math.Round(im.Height * k));
		Rectangle ri = new Rectangle((w - iw) / 2, (h - ih) / 2, iw, ih);
		for (int i = 1; i <= 6; i++)
		{
			using SolidBrush o = new SolidBrush(Color.FromArgb(14 - i * 2, 0, 0, 0));
			g.FillRectangle(o, Rectangle.Inflate(ri, i * 2, i * 2));
		}
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.PixelOffsetMode = PixelOffsetMode.HighQuality;
		g.DrawImage(im, ri);
		return b;
	}

	/// <summary>Recadrage intelligent qui remplit tout le rectangle (garde la zone la plus détaillée).</summary>
	public static void Couvrir(Graphics g, Image im, Rectangle r)
	{
		RectangleF src = Cadrage(im, r.Width, r.Height);
		lock (im)
		{
			g.DrawImage(im, r, src, GraphicsUnit.Pixel);
		}
	}
}
