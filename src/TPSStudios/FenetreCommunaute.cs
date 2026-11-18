using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>Accueil et menu : la case « Communauté » (dernières photos des joueurs) et l'ouverture de la page.</summary>
internal partial class Fenetre
{
	private Rectangle rCommu;

	private readonly List<Image> commuImages = new List<Image>();

	private int commuNb;

	/// <summary>Publications en attente de validation (équipe) : pastille sur « Communauté » dans le menu.</summary>
	private int commuAttente;

	private DateTime commuLu = DateTime.MinValue;

	private bool commuEnCours;

	private Bitmap commuMosaique;

	private string commuCle = "";

	private static readonly Font fCommuG = new Font("Segoe MDL2 Assets", Zx.P(26f));

	/// <summary>Met à jour la case Communauté de l'accueil (au plus toutes les 3 minutes, ou tout de suite si forcer).</summary>
	private void ChargerCommuAccueil(bool forcer = false)
	{
		if (!Social.Actif || commuEnCours || ModeCapture || (!forcer && (DateTime.Now - commuLu).TotalMinutes < 3.0))
		{
			return;
		}
		commuEnCours = true;
		commuLu = DateTime.Now;
		List<Image> imgs = new List<Image>();
		int nb = 0;
		int att = commuAttente;
		Thread t = new Thread((ThreadStart)delegate
		{
			try
			{
				if (Social.Assurer(Comptes.Actif) == null)
				{
					Social.PageCommunaute p = Social.Communaute("recentes", 0L, 12);
					nb = p.Publications.Count;
					att = p.Attente;
					foreach (Social.Publication pub in p.Publications)
					{
						if (pub.EstVideo || imgs.Count >= 4)
						{
							continue;
						}
						try
						{
							byte[] b = FenPartage.LireImage(pub);
							using MemoryStream ms = new MemoryStream(b);
							using Image im = Image.FromStream(ms);
							int w = Math.Min(im.Width, Zx.P(640));
							imgs.Add(new Bitmap(im, w, Math.Max(1, im.Height * w / im.Width)));
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					commuEnCours = false;
					foreach (Image old in commuImages)
					{
						old.Dispose();
					}
					commuImages.Clear();
					commuImages.AddRange(imgs);
					commuNb = nb;
					commuAttente = att;
					commuCle = "";
					if (!rCommu.IsEmpty)
					{
						pAccueil.Invalidate(Rectangle.Inflate(rCommu, Zx.P(8), Zx.P(8)));
					}
					rail?.Invalidate();
				});
			}
			catch
			{
				commuEnCours = false;
			}
		});
		t.IsBackground = true;
		t.Start();
	}

	/// <summary>Mosaïque des dernières photos (1, 2 ou 4 cases), préparée une fois pour la taille de la case.</summary>
	private Bitmap MosaiqueCommu(int w, int h)
	{
		string cle = w + "x" + h + "|" + commuImages.Count + "|" + commuImages.GetHashCode();
		if (commuMosaique != null && cle == commuCle)
		{
			return commuMosaique;
		}
		commuMosaique?.Dispose();
		commuMosaique = new Bitmap(Math.Max(1, w), Math.Max(1, h));
		using (Graphics g = Graphics.FromImage(commuMosaique))
		{
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(60, 70, 160), Color.FromArgb(22, 26, 60), 45f))
			{
				g.FillRectangle(lg, 0, 0, w, h);
			}
			int n = commuImages.Count;
			int e = Zx.P(3);
			List<Rectangle> cases = new List<Rectangle>();
			if (n == 1)
			{
				cases.Add(new Rectangle(0, 0, w, h));
			}
			else if (n == 2 || n == 3)
			{
				cases.Add(new Rectangle(0, 0, (w - e) / 2, h));
				cases.Add(new Rectangle((w + e) / 2, 0, w - (w + e) / 2, h));
			}
			else if (n >= 4)
			{
				int cw = (w - e) / 2;
				int ch = (h - e) / 2;
				cases.Add(new Rectangle(0, 0, cw, ch));
				cases.Add(new Rectangle(cw + e, 0, w - cw - e, ch));
				cases.Add(new Rectangle(0, ch + e, cw, h - ch - e));
				cases.Add(new Rectangle(cw + e, ch + e, w - cw - e, h - ch - e));
			}
			for (int i = 0; i < cases.Count; i++)
			{
				GraphicsState st = g.Save();
				g.SetClip(cases[i]);
				Couvertures.Couvrir(g, commuImages[i], cases[i]);
				g.Restore(st);
			}
		}
		commuCle = cle;
		return commuMosaique;
	}

	private void PeindreCommu(Graphics g)
	{
		if (rCommu.IsEmpty)
		{
			return;
		}
		ChargerCommuAccueil();
		Rectangle r = rCommu;
		bool sv = survolAccueil == 300;
		if (sv)
		{
			r = new Rectangle(r.X, r.Y - Zx.P(3), r.Width, r.Height);
		}
		using (GraphicsPath pc = Bouton.Rond(r, Zx.P(14)))
		{
			using SolidBrush b = new SolidBrush(sv ? C.CarteSurvol : C.Carte);
			g.FillPath(b, pc);
			using Pen pn = new Pen(sv ? Color.FromArgb(120, 150, 170, 255) : Color.FromArgb(28, 255, 255, 255));
			g.DrawPath(pn, pc);
		}
		int hi = Math.Max(Zx.P(40), r.Height - Zx.P(58));
		Rectangle ri = new Rectangle(r.X + Zx.P(6), r.Y + Zx.P(6), r.Width - Zx.P(12), hi - Zx.P(6));
		Bitmap mos = MosaiqueCommu(ri.Width, ri.Height);
		GraphicsState st = g.Save();
		using (GraphicsPath pi = Bouton.Rond(ri, Zx.P(10)))
		{
			g.SetClip(pi, CombineMode.Intersect);
		}
		g.DrawImageUnscaled(mos, ri.X, ri.Y);
		if (commuImages.Count == 0)
		{
			Lang.DrawText(g, "", fCommuG, ri, Color.FromArgb(220, 255, 255, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		g.Restore(st);
		Lang.DrawText(g, "Communauté", fCarteNom, new Rectangle(r.X + Zx.P(14), r.Y + hi + Zx.P(8), r.Width - Zx.P(28), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
		string sous = (commuNb > 0) ? ("Photos et vidéos des joueurs  ·  " + commuNb + ((commuNb > 1) ? " récentes" : " récente")) : "Partagez vos photos et vidéos";
		Lang.DrawText(g, sous, fCarteSous, new Rectangle(r.X + Zx.P(14), r.Y + hi + Zx.P(30), r.Width - Zx.P(28), Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		if (sv)
		{
			int t = Zx.P(42);
			Rectangle rj = new Rectangle(ri.Right - t - Zx.P(10), ri.Bottom - t - Zx.P(10), t, t);
			using SolidBrush bj = new SolidBrush(C.Jade);
			g.FillEllipse(bj, rj);
			Txt.Dessiner(g, "›", fCarteNom, rj, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
	}

	/// <summary>Ouvre la page Communauté (photos et vidéos des joueurs).</summary>
	private void OuvrirPartage()
	{
		FenPartage.NomMod = (string id) => (cat != null) ? cat.Projets.Find((Projet p) => p.Id == id)?.Nom : null;
		FenPartage.ModsCatalogue = delegate
		{
			List<KeyValuePair<string, string>> l = new List<KeyValuePair<string, string>>();
			if (cat != null)
			{
				foreach (Projet p in cat.Projets)
				{
					if (p.StatutEffectif != "masque")
					{
						l.Add(new KeyValuePair<string, string>(p.Id, p.Nom));
					}
				}
			}
			return l;
		};
		string demande;
		using (FenPartage f = new FenPartage())
		{
			f.Afficher(this);
			demande = f.Demande;
		}
		ChargerCommuAccueil(forcer: true);
		if (!demande.StartsWith("mod:") || cat == null)
		{
			return;
		}
		Projet pr = cat.Projets.Find((Projet p) => p.Id == demande.Substring(4));
		if (pr != null)
		{
			Choisir(pr);
			ShowVue(Vue.Projet);
		}
	}

	// ───────────── mods découverts (rideau des nouveaux mods) ─────────────

	private HashSet<string> decouverts;

	/// <summary>
	/// Vrai si le joueur a déjà découvert la couverture du mod. Au premier lancement de cette version,
	/// les mods installés et ceux publiés depuis plus de 30 jours comptent comme découverts.
	/// </summary>
	private bool ModDecouvert(string id)
	{
		if (decouverts == null)
		{
			decouverts = new HashSet<string>();
			string l = Config.Lire("mods_decouverts");
			if (l == null)
			{
				string omsi = null;
				try
				{
					omsi = Omsi.DetecteOmsi();
				}
				catch
				{
				}
				if (cat != null)
				{
					foreach (Projet p in cat.Projets)
					{
						bool installe = false;
						try
						{
							installe = omsi != null && Omsi.Installee(p, omsi) != null;
						}
						catch
						{
						}
						if (installe || EstAncien(p))
						{
							decouverts.Add(p.Id);
						}
					}
				}
				Config.Ecrire("mods_decouverts", string.Join(",", decouverts));
			}
			else
			{
				foreach (string x in l.Split(','))
				{
					if (x.Length > 0)
					{
						decouverts.Add(x);
					}
				}
			}
		}
		return decouverts.Contains(id);
	}

	private static bool EstAncien(Projet p)
	{
		try
		{
			string d = p.Date;
			if (!string.IsNullOrEmpty(d) && DateTime.TryParse(d, System.Globalization.CultureInfo.GetCultureInfo("fr-FR"), System.Globalization.DateTimeStyles.None, out var dt))
			{
				return (DateTime.Now - dt).TotalDays > 30.0;
			}
		}
		catch
		{
		}
		return false;
	}

	private void MarquerDecouvert(string id)
	{
		if (ModDecouvert(id))
		{
			return;
		}
		decouverts.Add(id);
		Config.Ecrire("mods_decouverts", string.Join(",", decouverts));
	}
}
