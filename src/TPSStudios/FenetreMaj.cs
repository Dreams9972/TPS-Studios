using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Accueil : bandeau « Mise à jour disponible » (lanceur et/ou mods installés), bien visible
/// au-dessus des tuiles. Un clic lance la mise à jour (ou ouvre la liste s'il y en a plusieurs).
/// </summary>
internal partial class Fenetre
{
	private List<Projet> majsAccueil = new List<Projet>();

	private bool majLanceurDispo;

	private Rectangle rMajA;

	private bool majSurvol;

	private static readonly Font fMajT = new Font("Segoe UI Semibold", Zx.P(11.5f));

	private static readonly Font fMajB = new Font("Segoe UI Semibold", Zx.P(10f));

	private static readonly Font fMajG = new Font("Segoe MDL2 Assets", Zx.P(14f));

	/// <summary>Cherche les mises à jour (lanceur et mods installés) : à la construction de l'accueil, pas à chaque peinture.</summary>
	private void CalculerMajAccueil()
	{
		List<Projet> list = new List<Projet>();
		string omsi = Omsi.DetecteOmsi();
		if (cat != null && omsi != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				try
				{
					LibelleEtat(projet, omsi, out var _, out var code);
					if (code == 2)
					{
						list.Add(projet);
					}
				}
				catch
				{
				}
			}
		}
		majsAccueil = list;
		majLanceurDispo = cat != null && cat.LanceurUrl != null && PlusRecent(cat.LanceurVersion, Config.VERSION);
	}

	private bool MajAAfficher
	{
		get
		{
			if (majsAccueil.Count <= 0)
			{
				return majLanceurDispo;
			}
			return true;
		}
	}

	private string TexteMaj()
	{
		int count = majsAccueil.Count;
		if (majLanceurDispo && count > 0)
		{
			return "Mises à jour disponibles : lanceur v" + cat.LanceurVersion + " et " + count + " mod" + ((count > 1) ? "s" : "");
		}
		if (majLanceurDispo)
		{
			return "Nouvelle version du lanceur disponible : v" + cat.LanceurVersion;
		}
		if (count == 1)
		{
			return "Mise à jour disponible : " + majsAccueil[0].Nom + "  ·  " + majsAccueil[0].Version;
		}
		return count + " mises à jour de mods disponibles";
	}

	private void PeindreMaj(Graphics g)
	{
		if (rMajA.IsEmpty || !MajAAfficher)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle r = rMajA;
		using (GraphicsPath path = Bouton.Rond(r, r.Height / 2))
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(r, majSurvol ? Color.FromArgb(235, 70, 130, 255) : Color.FromArgb(215, 52, 104, 230), majSurvol ? Color.FromArgb(235, 96, 112, 255) : Color.FromArgb(215, 74, 88, 220), 0f))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(majSurvol ? 230 : 150, 170, 200, 255), Zx.P(1.5f));
			g.DrawPath(pen, path);
		}
		Rectangle rect = new Rectangle(r.X + Zx.P(8), r.Y + Zx.P(6), r.Height - Zx.P(12), r.Height - Zx.P(12));
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
		{
			g.FillEllipse(brush2, rect);
		}
		Lang.DrawText(g, "", fMajG, rect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		string text = ((majsAccueil.Count > 1 && !majLanceurDispo) ? "Voir  ›" : "Mettre à jour  ›");
		int num = Lang.MeasureText(text, fMajB).Width + Zx.P(30);
		Rectangle rectangle = new Rectangle(r.Right - Zx.P(7) - num, r.Y + Zx.P(7), num, r.Height - Zx.P(14));
		using (GraphicsPath path2 = Bouton.Rond(rectangle, rectangle.Height / 2))
		{
			using SolidBrush brush3 = new SolidBrush(Color.White);
			g.FillPath(brush3, path2);
		}
		Lang.DrawText(g, text, fMajB, rectangle, Color.FromArgb(40, 70, 190), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, TexteMaj(), fMajT, new Rectangle(rect.Right + Zx.P(12), r.Y, rectangle.X - rect.Right - Zx.P(20), r.Height), Color.White, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		g.SmoothingMode = smoothingMode;
	}

	private void ClicMaj()
	{
		if (majLanceurDispo)
		{
			VerifierLanceur(manuel: true);
		}
		else if (majsAccueil.Count == 1)
		{
			Projet projet = majsAccueil[0];
			Choisir(projet);
			ShowVue(Vue.Projet);
			Installer(projet);
		}
		else if (majsAccueil.Count > 1)
		{
			Projet projet2 = majsAccueil[0];
			jeuId = JeuDe(projet2);
			filtre = 2;
			ongletB = 1;
			ShowVue(Vue.Biblio);
		}
	}
}
