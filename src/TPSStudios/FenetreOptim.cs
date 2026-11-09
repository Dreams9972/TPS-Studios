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
