using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Page du jeu (onglet Bibliothèque) : bande-annonce, photos sous JOUER et bloc
/// « À propos » façon Steam. Infos et médias : fiche Steam du jeu si elle existe,
/// sinon ce qui est saisi dans le catalogue (jeux_liste : description, galerie, video).
/// </summary>
internal partial class Fenetre
{
	private VideoFond videoB;

	private readonly Dictionary<string, Image> imagesWeb = new Dictionary<string, Image>();

	private readonly HashSet<string> imagesWebEnCours = new HashSet<string>();

	private static readonly Font fInfoL = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fInfoV = new Font("Segoe UI", Zx.P(10f));

	private static readonly Font fLien = new Font("Segoe UI Semibold", Zx.P(9.5f));

	private string AppSteam(string jeu)
	{
		string text = ValJeu(jeu, "steam").Trim();
		if (text.Length > 0)
		{
			return text;
		}
		if (!(jeu == "omsi2"))
		{
			return null;
		}
		return "252530";
	}

	private FicheSteam FicheJeu()
	{
		return Steam.Fiche(AppSteam(jeuId), delegate
		{
			Sur(delegate
			{
				if (vue == Vue.Biblio)
				{
					DisposerBiblio();
				}
			});
		});
	}

	/// <summary>Photos du jeu : { miniature, pleine taille }.</summary>
	private List<string[]> PhotosDuJeu()
	{
		List<string[]> list = new List<string[]>();
		FicheSteam ficheSteam = FicheJeu();
		if (ficheSteam != null)
		{
			list.AddRange(ficheSteam.Captures);
		}
		if (list.Count == 0)
		{
			string[] array = ValJeu(jeuId, "galerie").Replace("\r", "").Split('\n');
			foreach (string text in array)
			{
				if (Uri.TryCreate(text.Trim(), UriKind.Absolute, out var result) && (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp) && list.Count < 12)
				{
					list.Add(new string[2] { result.AbsoluteUri, result.AbsoluteUri });
				}
			}
		}
		return list;
	}

	private string VideoDuJeu()
	{
		string text = ValJeu(jeuId, "video");
		if (VideoFond.Lisible(text))
		{
			return text;
		}
		FicheSteam ficheSteam = FicheJeu();
		if (ficheSteam == null || !VideoFond.Lisible(ficheSteam.Video))
		{
			return "";
		}
		return ficheSteam.Video;
	}

	private void PlacerVideoJeu(bool page)
	{
		if (videoB == null)
		{
			videoB = new VideoFond();
			videoB.Actif = () => vue == Vue.Biblio && ongletB == 0 && base.WindowState != FormWindowState.Minimized && !enTransition;
		}
		if (videoB.Parent != pBiblio)
		{
			pBiblio.Controls.Add(videoB);
		}
		if (!page)
		{
			videoB.Afficher(m: false);
			return;
		}
		videoB.Charger(VideoDuJeu());
		videoB.Bounds = new Rectangle(rPageG.X + 1, rPageG.Y + 1, rPageG.Width - 2, HauteurHero() - 2);
		videoB.Afficher(m: true);
		videoB.BringToFront();
	}

	/// <summary>Image d'une adresse web, chargée en arrière-plan (null tant qu'elle n'est pas là).</summary>
	private Image ImageWeb(string url, Control cible)
	{
		if (string.IsNullOrEmpty(url))
		{
			return null;
		}
		lock (imagesWeb)
		{
			if (imagesWeb.TryGetValue(url, out var value))
			{
				return value;
			}
			if (!imagesWebEnCours.Add(url))
			{
				return null;
			}
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			Image image = null;
			try
			{
				using WebClient webClient = Web.Client();
				using MemoryStream stream = new MemoryStream(webClient.DownloadData(url));
				using Image original = Image.FromStream(stream);
				image = new Bitmap(original);
			}
			catch
			{
			}
			lock (imagesWeb)
			{
				imagesWeb[url] = image;
				imagesWebEnCours.Remove(url);
			}
			if (image != null)
			{
				Sur(delegate
				{
					cible?.Invalidate();
				});
			}
		});
		thread.IsBackground = true;
		thread.Start();
		return null;
	}

	private void OuvrirPhotos(List<string> urls, int depart)
	{
		if (urls.Count == 0)
		{
			return;
		}
		Toast("Chargement des photos…", C.Bleu, 2500);
		Thread thread = new Thread((ThreadStart)delegate
		{
			List<Image> ims = new List<Image>();
			int num = 0;
			for (int i = 0; i < urls.Count; i++)
			{
				try
				{
					using WebClient webClient = Web.Client();
					using MemoryStream stream = new MemoryStream(webClient.DownloadData(urls[i]));
					using Image original = Image.FromStream(stream);
					ims.Add(new Bitmap(original));
					if (i == depart)
					{
						num = ims.Count - 1;
					}
				}
				catch
				{
				}
			}
			Sur(delegate
			{
				if (ims.Count == 0)
				{
					Toast("Photos indisponibles pour l'instant.", C.Rouge, 4000);
				}
				else
				{
					using FenVisionneuse f = new FenVisionneuse(ims, num);
					f.Afficher(this);
				}
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private static void DessinerCouverture(Graphics g, Image im, Rectangle r, int rayon)
	{
		using GraphicsPath graphicsPath = Bouton.Rond(r, rayon);
		GraphicsState gstate = g.Save();
		g.SetClip(graphicsPath);
		if (im != null)
		{
			double num = Math.Max((double)r.Width / (double)im.Width, (double)r.Height / (double)im.Height);
			int num2 = (int)((double)im.Width * num);
			int num3 = (int)((double)im.Height * num);
			g.DrawImage(im, r.X + (r.Width - num2) / 2, r.Y + (r.Height - num3) / 2, num2, num3);
		}
		else
		{
			using LinearGradientBrush brush = new LinearGradientBrush(r, C.Ligne, C.Carte, 45f);
			g.FillRectangle(brush, r);
		}
		g.Restore(gstate);
	}

	/// <summary>Bande de photos sous le bouton JOUER (clic : visionneuse).</summary>
	private int PeindrePhotosJeu(Graphics g, int y)
	{
		List<string[]> list = PhotosDuJeu();
		if (list.Count == 0)
		{
			return y;
		}
		int num = 5;
		int num2 = Zx.P(10);
		int num3 = (rPageG.Width - (num - 1) * num2) / num;
		int num4 = num3 * 9 / 16;
		List<string> pleines = new List<string>();
		foreach (string[] item in list)
		{
			pleines.Add(item[1]);
		}
		for (int i = 0; i < Math.Min(num, list.Count); i++)
		{
			Rectangle rectangle = new Rectangle(rPageG.X + i * (num3 + num2), y, num3, num4);
			int idx = i;
			Zone(rectangle, delegate
			{
				OuvrirPhotos(pleines, idx);
			}, "Voir les photos");
			DessinerCouverture(g, ImageWeb(list[i][0], pBiblio), rectangle, Zx.P(8));
			bool flag = i == num - 1 && list.Count > num;
			if (flag || Survol(rectangle))
			{
				using GraphicsPath path = Bouton.Rond(rectangle, Zx.P(8));
				using SolidBrush brush = new SolidBrush(Color.FromArgb(flag ? 150 : 50, 8, 10, 24));
				g.FillPath(brush, path);
			}
			if (flag)
			{
				Lang.DrawText(g, "+" + (list.Count - num + 1), fStatV, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			using Pen pen = new Pen(Survol(rectangle) ? C.Orange : Color.FromArgb(50, 255, 255, 255));
			using GraphicsPath path2 = Bouton.Rond(rectangle, Zx.P(8));
			g.DrawPath(pen, path2);
		}
		return y + num4 + Zx.P(28);
	}

	/// <summary>Bloc « À propos » : comme sur Steam (description, développeur, éditeur, date de sortie, genres…).</summary>
	private int PeindreAPropos(Graphics g, int y)
	{
		FicheSteam fiche = FicheJeu();
		int x = rPageG.X;
		int width = rPageG.Width;
		Lang.DrawText(g, "À PROPOS DE CE JEU", fSecT, new Point(x, y), C.Gris, TextFormatFlags.NoPadding);
		y += Zx.P(26);
		string text = ValJeu(jeuId, "description");
		if (fiche != null && fiche.Description.Length > 0)
		{
			text = fiche.Description;
		}
		if (text.Length == 0)
		{
			text = ((jeuId == "omsi2") ? "Simulateur de bus et de métro. Installez cartes, bus, livrées et packs IA en un clic depuis l'onglet Mods, puis lancez le jeu avec openOMSI, ou directement sur une carte depuis « Mes mods »." : "Jeu en ligne gratuit : il s'ouvre dans sa propre fenêtre, avec ou sans mod.");
		}
		Size size = Lang.MeasureText(text, fCorps, new Size(width, Zx.P(200)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		int num = Math.Min(Zx.P(200), size.Height + Zx.P(4));
		Lang.DrawText(g, text, fCorps, new Rectangle(x, y, width, num), Color.FromArgb(205, 212, 232), TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		y += num + Zx.P(20);
		List<string[]> list = new List<string[]>();
		if (fiche != null)
		{
			Ajout(list, "Développeur", fiche.Developpeurs);
			Ajout(list, "Éditeur", fiche.Editeurs);
			Ajout(list, "Date de sortie", fiche.Sortie);
			Ajout(list, "Genres", fiche.Genres);
			Ajout(list, "Fonctionnalités", fiche.Fonctionnalites);
			Ajout(list, "Langues", fiche.Langues);
		}
		else
		{
			Ajout(list, "Éditeur", ValJeu(jeuId, "editeur"));
			Ajout(list, "Date de sortie", ValJeu(jeuId, "sortie"));
			Ajout(list, "Genres", ValJeu(jeuId, "genres"));
		}
		if (list.Count > 0)
		{
			int num2 = Zx.P(16);
			int num3 = Zx.P(150);
			List<int> list2 = new List<int>();
			int num4 = num2;
			foreach (string[] item in list)
			{
				int num5 = Math.Min(Zx.P(60), Lang.MeasureText(item[1], fInfoV, new Size(width - num3 - 2 * num2, Zx.P(60)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height) + Zx.P(14);
				list2.Add(num5);
				num4 += num5;
			}
			num4 += num2 - Zx.P(6);
			Rectangle r = new Rectangle(x, y, width, num4);
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
			{
				using SolidBrush brush = new SolidBrush(Color.FromArgb(130, C.Carte));
				g.FillPath(brush, path);
			}
			int num6 = y + num2;
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					using Pen pen = new Pen(Color.FromArgb(70, C.Ligne));
					g.DrawLine(pen, x + num2, num6 - Zx.P(7), r.Right - num2, num6 - Zx.P(7));
				}
				Lang.DrawText(g, list[i][0].ToUpperInvariant(), fStatL, new Rectangle(x + num2, num6 + Zx.P(2), num3 - Zx.P(10), Zx.P(18)), C.Gris, TextFormatFlags.NoPadding);
				Lang.DrawText(g, list[i][1], fInfoV, new Rectangle(x + num2 + num3, num6, width - num3 - 2 * num2, list2[i] - Zx.P(10)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				num6 += list2[i];
			}
			y = r.Bottom + Zx.P(22);
		}
		if (fiche != null && fiche.ConfigMin.Count > 0)
		{
			Lang.DrawText(g, "CONFIGURATION MINIMALE", fSecT, new Point(x, y), C.Gris, TextFormatFlags.NoPadding);
			y += Zx.P(24);
			foreach (string item2 in fiche.ConfigMin)
			{
				int num7 = Math.Min(Zx.P(40), Lang.MeasureText(item2, fInfoL, new Size(width - Zx.P(16), Zx.P(40)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height) + Zx.P(4);
				using (SolidBrush brush2 = new SolidBrush(C.Jade))
				{
					g.FillEllipse(brush2, x + Zx.P(2), y + Zx.P(7), Zx.P(5), Zx.P(5));
				}
				Lang.DrawText(g, item2, fInfoL, new Rectangle(x + Zx.P(16), y, width - Zx.P(16), num7), Color.FromArgb(195, 203, 225), TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				y += num7 + Zx.P(2);
			}
			y += Zx.P(14);
		}
		if (fiche != null && fiche.Page.Length > 0)
		{
			Rectangle rectangle = Pilule(g, "Voir la page Steam  ↗", x + Lang.MeasureText("Voir la page Steam  ↗", fPilule).Width + Zx.P(28), y, C.Bleu, plein: false);
			Zone(rectangle, delegate
			{
				try
				{
					Process.Start(fiche.Page);
				}
				catch
				{
				}
			}, fiche.Page);
			y = rectangle.Bottom + Zx.P(10);
		}
		return y;
	}

	private static void Ajout(List<string[]> l, string k, string v)
	{
		v = (v ?? "").Trim();
		if (v.Length > 0)
		{
			l.Add(new string[2] { k, v });
		}
	}

	// ---------- Page d'un mod : bande-annonce en haut, photos sous JOUER ----------

	private VideoFond videoP;

	private Rectangle rPhotosP;

	private void PlacerVideoPage()
	{
		if (videoP == null)
		{
			videoP = new VideoFond();
			videoP.Actif = () => vue == Vue.Projet && base.WindowState != FormWindowState.Minimized && !enTransition;
		}
		if (videoP.Parent != page)
		{
			page.Controls.Add(videoP);
		}
		videoP.Charger((sel != null) ? sel.Video : "");
		videoP.Bounds = rVue;
		videoP.Afficher(sel != null);
		videoP.BringToFront();
	}
}
