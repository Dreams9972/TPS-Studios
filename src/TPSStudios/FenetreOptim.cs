using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace TPSStudios;

/// <summary>
/// Optimisations de la page du jeu : état des mods lu une seule fois (pas de lecture
/// disque à chaque peinture) et images redimensionnées une seule fois (cache).
/// </summary>
internal partial class Fenetre
{
	private class EtatMod
	{
		public Projet P;

		public int Code;

		public bool Off;

		public string VInst = "";
	}

	private List<EtatMod> etatsB = new List<EtatMod>();

	private int nbModsJeu;

	private int nbMajJeu;

	private string omsiB;

	private readonly Dictionary<string, Bitmap> echelles = new Dictionary<string, Bitmap>();

	/// <summary>Lit l'état des mods du jeu (installé, à jour, désactivé) : appelé à la mise en page, pas à chaque peinture.</summary>
	private void CalculerEtats()
	{
		omsiB = Omsi.DetecteOmsi();
		List<EtatMod> list = new List<EtatMod>();
		int num = 0;
		int num2 = 0;
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (JeuDe(projet) != jeuId)
				{
					continue;
				}
				num++;
				LibelleEtat(projet, omsiB, out var _, out var code);
				if (code >= 1)
				{
					string[] array = Omsi.Installee(projet, omsiB);
					list.Add(new EtatMod
					{
						P = projet,
						Code = code,
						Off = Omsi.Desactive(projet, omsiB),
						VInst = ((array != null && array[0].Trim().Length > 0) ? array[0].Trim() : projet.Version)
					});
					if (code == 2)
					{
						num2++;
					}
				}
			}
		}
		etatsB = list;
		nbModsJeu = num;
		nbMajJeu = num2;
	}

	/// <summary>Image recadrée (« couvrir ») à la taille voulue, gardée en cache.</summary>
	/// <summary>
	/// Image dans une case sans la zoomer à l'excès : si les proportions sont proches, elle remplit la
	/// case (léger recadrage) ; sinon elle s'affiche en entier sur un fond flou fait de la même image.
	/// </summary>
	internal static void DessinerAjuste(Graphics g, Image im, Rectangle r, double tolerance = 1.22, int voileFond = 110)
	{
		if (im == null || r.Width < 1 || r.Height < 1 || im.Width < 1 || im.Height < 1)
		{
			return;
		}
		double rc = (double)r.Width / (double)r.Height;
		double ri = (double)im.Width / (double)im.Height;
		double ecart = Math.Max(rc, ri) / Math.Min(rc, ri);
		InterpolationMode im0 = g.InterpolationMode;
		if (ecart <= tolerance)
		{
			double k = Math.Max((double)r.Width / (double)im.Width, (double)r.Height / (double)im.Height);
			int w = (int)Math.Ceiling((double)im.Width * k);
			int h = (int)Math.Ceiling((double)im.Height * k);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(im, r.X + (r.Width - w) / 2, r.Y + (r.Height - h) / 2, w, h);
			g.InterpolationMode = im0;
			return;
		}
		int pw = Math.Max(2, r.Width / 14);
		int ph = Math.Max(2, r.Height / 14);
		using (Bitmap petit = new Bitmap(pw, ph, PixelFormat.Format32bppPArgb))
		{
			using (Graphics gp = Graphics.FromImage(petit))
			{
				gp.InterpolationMode = InterpolationMode.HighQualityBilinear;
				double k2 = Math.Max((double)pw / (double)im.Width, (double)ph / (double)im.Height);
				int w2 = (int)Math.Ceiling((double)im.Width * k2);
				int h2 = (int)Math.Ceiling((double)im.Height * k2);
				gp.DrawImage(im, (pw - w2) / 2, (ph - h2) / 2, w2, h2);
			}
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.DrawImage(petit, new Rectangle(r.X - r.Width / 20, r.Y - r.Height / 20, r.Width + r.Width / 10, r.Height + r.Height / 10), 0, 0, pw, ph, GraphicsUnit.Pixel);
		}
		using (SolidBrush voile = new SolidBrush(Color.FromArgb(voileFond, 6, 8, 20)))
		{
			g.FillRectangle(voile, r);
		}
		double k3 = Math.Min((double)r.Width / (double)im.Width, (double)r.Height / (double)im.Height);
		int w3 = (int)Math.Round((double)im.Width * k3);
		int h3 = (int)Math.Round((double)im.Height * k3);
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.DrawImage(im, r.X + (r.Width - w3) / 2, r.Y + (r.Height - h3) / 2, w3, h3);
		g.InterpolationMode = im0;
	}

	private Bitmap Couvrir(Image im, int w, int h)
	{
		if (im == null || w < 1 || h < 1)
		{
			return null;
		}
		string key = RuntimeHelpers.GetHashCode(im) + "|" + w + "x" + h;
		if (echelles.TryGetValue(key, out var value))
		{
			return value;
		}
		if (echelles.Count > 80)
		{
			foreach (Bitmap value2 in echelles.Values)
			{
				value2.Dispose();
			}
			echelles.Clear();
		}
		value = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(value))
		{
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			double num = Math.Max((double)w / (double)im.Width, (double)h / (double)im.Height);
			int num2 = (int)Math.Ceiling((double)im.Width * num);
			int num3 = (int)Math.Ceiling((double)im.Height * num);
			graphics.DrawImage(im, (w - num2) / 2, (h - num3) / 2, num2, num3);
		}
		echelles[key] = value;
		return value;
	}

	/// <summary>Dessine une image recadrée dans un rectangle arrondi (sans redimensionner à chaque fois).</summary>
	private void DessinerArrondi(Graphics g, Image im, Rectangle r, int rayon)
	{
		using GraphicsPath graphicsPath = Bouton.Rond(r, rayon);
		GraphicsState gstate = g.Save();
		g.SetClip(graphicsPath, CombineMode.Intersect);
		Bitmap bitmap = Couvrir(im, r.Width, r.Height);
		if (bitmap != null)
		{
			g.DrawImageUnscaled(bitmap, r.X, r.Y);
		}
		else
		{
			using LinearGradientBrush brush = new LinearGradientBrush(r, C.Ligne, C.Carte, 45f);
			g.FillRectangle(brush, r);
		}
		g.Restore(gstate);
	}

	/// <summary>Zone à repeindre quand le survol change (et pas toute la page).</summary>
	private Rectangle ZoneARepeindre(ZoneB z)
	{
		if (z == null)
		{
			return Rectangle.Empty;
		}
		if (z.R.X >= rPageD.X)
		{
			return new Rectangle(rPageD.X, z.R.Y - Zx.P(48), rPageD.Width + Zx.P(4), z.R.Height + Zx.P(96));
		}
		return Rectangle.Inflate(z.R, Zx.P(6), Zx.P(6));
	}
}
