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
			DessinerArrondi(g, ImageWeb(list[i][0], pBiblio), rectangle, Zx.P(8));
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
		return y + num4 + Zx.P(22);
	}

	private string DescriptionJeu(FicheSteam fiche)
	{
		string text = ValJeu(jeuId, "description");
		if (fiche != null && fiche.Description.Length > 0)
		{
			text = fiche.Description;
		}
		if (text.Length == 0)
		{
			text = ((jeuId == "omsi2") ? "Simulateur de bus et de métro. Installez cartes, bus, livrées et packs IA en un clic depuis l'onglet Mods, puis lancez le jeu avec openOMSI, ou directement sur une carte depuis « Mes mods »." : "Jeu en ligne gratuit : il s'ouvre dans sa propre fenêtre, avec ou sans mod.");
		}
		return text;
	}

	private List<string[]> InfosJeu(FicheSteam fiche)
	{
		List<string[]> list = new List<string[]>();
		if (fiche != null)
		{
			Ajout(list, "Développeur", fiche.Developpeurs);
			Ajout(list, "Éditeur", fiche.Editeurs);
			Ajout(list, "Date de sortie", fiche.Sortie);
			Ajout(list, "Genres", fiche.Genres);
			Ajout(list, "Fonctionnalités", fiche.Fonctionnalites);
			Ajout(list, "Langues", fiche.Langues);
			List<string> list2 = new List<string>();
			foreach (string item in fiche.ConfigMin)
			{
				int num = item.IndexOf(':');
				list2.Add(((num > 0 && num < 40) ? item.Substring(num + 1) : item).Trim());
			}
			Ajout(list, "Configuration", string.Join("  ·  ", list2.ToArray()));
		}
		else
		{
			Ajout(list, "Éditeur", ValJeu(jeuId, "editeur"));
			Ajout(list, "Date de sortie", ValJeu(jeuId, "sortie"));
			Ajout(list, "Genres", ValJeu(jeuId, "genres"));
		}
		return list;
	}

	private int HauteurAPropos()
	{
		int num = (InfosJeu(FicheJeu()).Count + 1) / 2;
		return Zx.P(26) + Zx.P(44) + Zx.P(16) + ((num > 0) ? (num * Zx.P(34) + Zx.P(16)) : 0);
	}

	private int HauteurPhotos()
	{
		if (PhotosDuJeu().Count == 0)
		{
			return 0;
		}
		return (rPageG.Width - 4 * Zx.P(10)) / 5 * 9 / 16 + Zx.P(22);
	}

	/// <summary>Bloc « À propos » compact, comme sur Steam : tient dans la fenêtre sans défiler.</summary>
	private int PeindreAPropos(Graphics g, int y)
	{
		FicheSteam fiche = FicheJeu();
		int x = rPageG.X;
		int width = rPageG.Width;
		Lang.DrawText(g, "À PROPOS DE CE JEU", fSecT, new Point(x, y), C.Gris, TextFormatFlags.NoPadding);
		if (fiche != null && fiche.Page.Length > 0)
		{
			string text = "Voir la page Steam  ↗";
			Size size = Lang.MeasureText(text, fLien);
			Rectangle rectangle = new Rectangle(rPageG.Right - size.Width, y - Zx.P(2), size.Width, size.Height + Zx.P(4));
			Lang.DrawText(g, text, fLien, new Point(rectangle.X, y), Survol(rectangle) ? C.Texte : C.Bleu, TextFormatFlags.NoPadding);
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
		}
		y += Zx.P(26);
		Lang.DrawText(g, DescriptionJeu(fiche), fCorps, new Rectangle(x, y, width, Zx.P(42)), Color.FromArgb(205, 212, 232), TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		y += Zx.P(44) + Zx.P(16);
		List<string[]> list = InfosJeu(fiche);
		if (list.Count > 0)
		{
			int num = (list.Count + 1) / 2;
			int num2 = Zx.P(34);
			Rectangle r = new Rectangle(x, y, width, num * num2 + Zx.P(16));
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
			{
				using SolidBrush brush = new SolidBrush(Color.FromArgb(130, C.Carte));
				g.FillPath(brush, path);
			}
			int num3 = (width - Zx.P(16) * 3) / 2;
			int num4 = Math.Min(Zx.P(128), num3 / 3);
			for (int i = 0; i < list.Count; i++)
			{
				int num5 = i / 2;
				int num6 = x + Zx.P(16) + i % 2 * (num3 + Zx.P(16));
				int num7 = y + Zx.P(8) + num5 * num2;
				if (num5 > 0 && i % 2 == 0)
				{
					using Pen pen = new Pen(Color.FromArgb(60, C.Ligne));
					g.DrawLine(pen, x + Zx.P(16), num7, r.Right - Zx.P(16), num7);
				}
				Lang.DrawText(g, list[i][0].ToUpperInvariant(), fStatL, new Rectangle(num6, num7, num4 - Zx.P(6), num2), C.Gris, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				Lang.DrawText(g, list[i][1], fInfoV, new Rectangle(num6 + num4, num7, num3 - num4, num2), C.Texte, TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			y = r.Bottom + Zx.P(10);
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
		string text = ((sel != null) ? sel.Video : "");
		if (sel != null && !VideoFond.Lisible(text))
		{
			Projet p = sel;
			text = YouTube.TrailerPour(sel, ChaineYouTube(), delegate
			{
				Sur(delegate
				{
					if (vue == Vue.Projet && sel == p)
					{
						PlacerVideoPage();
					}
				});
			}) ?? "";
		}
		videoP.Charger(text);
		videoP.Bounds = rVue;
		videoP.Afficher(sel != null);
		videoP.BringToFront();
	}

	/// <summary>Chaîne YouTube où chercher les bandes-annonces des mods (catalogue › communaute › youtube).</summary>
	private string ChaineYouTube()
	{
		if (cat != null && cat.Communaute.TryGetValue("youtube", out var value) && !string.IsNullOrEmpty(value))
		{
			return value;
		}
		return YouTube.ChaineParDefaut;
	}
}
