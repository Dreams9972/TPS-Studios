using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Bibliothèque v2.8 : deux onglets.
///  • « Bibliothèque » : page du jeu façon Steam (bannière, JOUER, stats, actualités)
///    et, à droite, « Mes mods » (installés : activer / désactiver, jouer sur une carte).
///  • « Mods » : tout le catalogue du jeu (recherche, filtres, catégories, tri).
/// Tout est dessiné dans pBiblio (comme l'ancienne fiche du jeu) ; les zones cliquables
/// sont recalculées à chaque peinture.
/// </summary>
internal partial class Fenetre
{
	private class ZoneB
	{
		public Rectangle R;

		public Action Clic;

		public string Bulle;
	}

	private int ongletB;

	private int triB;

	private FlecheBord bFlecheMods;

	private Segments sgTri;

	private Bouton bJouerJeu;

	private bool biblioSouris;

	private readonly List<ZoneB> zonesB = new List<ZoneB>();

	private ZoneB survolB;

	private Rectangle rPageG;

	private Rectangle rPageD;

	private Image heroJeu;

	private string heroJeuId;

	private static readonly string[] TrisB = new string[4] { "Par défaut", "Nom", "Récents", "Populaires" };

	private static readonly Font fHeroNom = new Font("Segoe UI Semibold", Zx.P(30f));

	private static readonly Font fHeroSous = new Font("Segoe UI", Zx.P(11f));

	private static readonly Font fStatV = new Font("Segoe UI Semibold", Zx.P(12.5f));

	private static readonly Font fStatL = new Font("Segoe UI Semibold", Zx.P(8f));

	private static readonly Font fSecT = new Font("Segoe UI Semibold", Zx.P(9f));

	private static readonly Font fCorps = new Font("Segoe UI", Zx.P(10f));

	private static readonly Font fModNom = new Font("Segoe UI Semibold", Zx.P(11.5f));

	private static readonly Font fModInfo = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fPilule = new Font("Segoe UI Semibold", Zx.P(9f));

	private static readonly Font fJouerJeu = new Font("Segoe UI Semibold", Zx.P(13f));

	// ---------- Construction ----------

	private void ConstruireOngletsBiblio()
	{
		bFlecheMods = new FlecheBord();
		bFlecheMods.Size = new Size(Zx.P(30), Zx.P(110));
		bFlecheMods.Click += delegate
		{
			BasculerMods(ongletB == 0);
		};
		sgTri = new Segments();
		sgTri.Items = TrisB;
		sgTri.MinW = Zx.P(90);
		sgTri.Etire = true;
		sgTri.Index = triB;
		sgTri.Size = new Size(Zx.P(420), Zx.P(34));
		sgTri.Changed += delegate
		{
			triB = sgTri.Index;
			RemplirBiblio();
		};
		bJouerJeu = new Bouton("▶  JOUER");
		bJouerJeu.Plein = true;
		bJouerJeu.Couleur = C.Vert;
		bJouerJeu.Font = fJouerJeu;
		bJouerJeu.Size = new Size(Zx.P(200), Zx.P(54));
		bJouerJeu.Click += delegate
		{
			JouerJeuCourant();
		};
		pBiblio.Controls.Add(bFlecheMods);
		pBiblio.Controls.Add(sgTri);
		pBiblio.Controls.Add(bJouerJeu);
		if (!biblioSouris)
		{
			biblioSouris = true;
			pBiblio.MouseMove += delegate(object o, MouseEventArgs e)
			{
				ZoneB zoneB = ZoneSous(e.Location);
				if (zoneB != survolB)
				{
					Rectangle rectangle = ZoneARepeindre(survolB);
					survolB = zoneB;
					pBiblio.Cursor = ((zoneB != null) ? Cursors.Hand : Cursors.Default);
					bulle.SetToolTip(pBiblio, (zoneB != null && zoneB.Bulle != null) ? zoneB.Bulle : null);
					if (!rectangle.IsEmpty)
					{
						pBiblio.Invalidate(rectangle);
					}
					if (zoneB != null)
					{
						pBiblio.Invalidate(ZoneARepeindre(zoneB));
					}
				}
			};
			pBiblio.Scroll += delegate
			{
				PlacerFleche();
			};
			pBiblio.ClientSizeChanged += delegate
			{
				PlacerFleche();
			};
			pBiblio.MouseWheel += delegate
			{
				PlacerFleche();
			};
			pBiblio.MouseLeave += delegate
			{
				if (survolB != null)
				{
					survolB = null;
					pBiblio.Cursor = Cursors.Default;
					pBiblio.Invalidate();
				}
			};
			pBiblio.MouseDown += delegate(object o, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left)
				{
					ZoneB zoneB2 = ZoneSous(e.Location);
					if (zoneB2 != null && zoneB2.Clic != null)
					{
						Sons.Clic();
						zoneB2.Clic();
					}
				}
			};
		}
	}

	private ZoneB ZoneSous(Point p)
	{
		if (ongletB != 0 || vue != Vue.Biblio)
		{
			return null;
		}
		for (int num = zonesB.Count - 1; num >= 0; num--)
		{
			if (zonesB[num].R.Contains(p))
			{
				return zonesB[num];
			}
		}
		return null;
	}

	/// <summary>Bibliothèque ⇄ tous les mods, avec un glissement horizontal.</summary>
	private void BasculerMods(bool versMods)
	{
		if ((ongletB == 1) == versMods)
		{
			return;
		}
		Bitmap bitmap = null;
		if (couches.Count == 0 && !enTransition && Animer())
		{
			try
			{
				SuspendreVideos(s: true);
				bitmap = Figer();
			}
			catch
			{
				bitmap = null;
			}
		}
		ongletB = (versMods ? 1 : 0);
		RemplirBiblio();
		if (bitmap != null)
		{
			Transition(bitmap, versMods ? StyleTrans.Pousse : StyleTrans.Retour);
		}
		RetirerCouvercle();
		SuspendreVideos(s: false);
	}

	private void PlacerFleche()
	{
		if (bFlecheMods != null)
		{
			int num = pBiblio.ClientSize.Width;
			if (pBiblio.AutoScrollMinSize.Height > pBiblio.ClientSize.Height)
			{
				num -= SystemInformation.VerticalScrollBarWidth;
			}
			int lf = bFlecheMods.LargeurFleche;
			int w = lf;
			bFlecheMods.Libelle = ((ongletB == 0) ? "Les Mods" : "");
			if (ongletB == 0)
			{
				int wt = Lang.MeasureText("Les Mods", FlecheBord.PoliceLibelle).Width + Zx.P(12);
				int place = num - lf - Zx.P(6) - (rPageD.Right + Zx.P(12));
				if (rPageD.Width > 0 && place >= wt)
				{
					w = lf + wt;
				}
			}
			if (bFlecheMods.Width != w)
			{
				bFlecheMods.Width = w;
				bFlecheMods.Invalidate();
			}
			int x = ((ongletB == 0) ? (num - bFlecheMods.Width - Zx.P(6)) : Zx.P(14));
			bFlecheMods.Location = new Point(x, Math.Max(Zx.P(132), (pBiblio.ClientSize.Height - bFlecheMods.Height) / 2));
			bFlecheMods.BringToFront();
		}
	}

	private void JouerJeuCourant()
	{
		Dictionary<string, string> dictionary = JeuWebCourant();
		if (dictionary != null)
		{
			if (JeuWebOuvert(dictionary))
			{
				LancerJeuWeb(dictionary);
			}
			else
			{
				Toast(NomJeu(jeuId) + " arrive bientôt sur le lanceur !", C.Orange, 5000);
			}
		}
		else
		{
			JouerOmsi();
		}
	}

	// ---------- Tri de l'onglet Mods ----------

	private List<Projet> ProjetsTries()
	{
		List<Projet> list = new List<Projet>((cat != null) ? cat.Projets : new List<Projet>());
		switch (triB)
		{
		case 1:
			list.Sort((Projet a, Projet b) => string.Compare(a.Nom, b.Nom, StringComparison.CurrentCultureIgnoreCase));
			break;
		case 2:
			list.Sort((Projet a, Projet b) => DateMod(b).CompareTo(DateMod(a)));
			break;
		case 3:
			list.Sort((Projet a, Projet b) => Telechargements(b).CompareTo(Telechargements(a)));
			break;
		}
		return list;
	}

	private static DateTime DateMod(Projet p)
	{
		if (DateTime.TryParseExact(p.Date ?? "", new string[2] { "dd/MM/yyyy HH:mm", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			return result;
		}
		return DateTime.MinValue;
	}

	// ---------- Mise en page ----------

	/// <summary>Appelé à la fin de DisposerBiblio : montre / cache les éléments de chaque onglet.</summary>
	private void DisposerOngletsBiblio(int yFiltres, int xDroite)
	{
		if (bFlecheMods == null)
		{
			return;
		}
		bool page = ongletB == 0;
		bChangerJeu.Visible = page;
		bAccueilB.Visible = page;
		bFlecheMods.Droite = page;
		bFlecheMods.Invalidate();
		bulle.SetToolTip(bFlecheMods, page ? "Tous les mods du jeu" : "Retour à la bibliothèque");
		PlacerFleche();
		Control[] array = new Control[5] { tRecherche, fTous, fInst, fMaj, sgTri };
		foreach (Control control in array)
		{
			control.Visible = !page;
		}
		foreach (Bouton chip in chips)
		{
			chip.Visible = !page;
		}
		foreach (Tuile item in tuilesB)
		{
			item.Visible = !page;
		}
		bJouerOmsi.Visible = false;
		bJouerJeu.Visible = page;
		bool web = JeuWebCourant() != null;
		bChemins.Visible = page && !web;
		if (bPhotosJeu != null)
		{
			bPhotosJeu.Visible = page && ValJeu(jeuId, "galerie").Trim().Length > 0;
		}
		if (bVideoJeu != null)
		{
			bVideoJeu.Visible = page && ValJeu(jeuId, "video").Length > 0;
		}
		if (!page)
		{
			sgTri.Location = new Point(Math.Max(fMaj.Right + Zx.P(20), xDroite - sgTri.Width), yFiltres);
			pBiblio.AutoScrollMinSize = Size.Empty;
			PlacerVideoJeu(page: false);
			pBiblio.Invalidate();
			return;
		}
		int width = pBiblio.ClientSize.Width;
		int num = Zx.P(40);
		int y = Zx.P(132);
		int num2 = Zx.P(28);
		int num3 = Math.Max(Zx.P(560), (int)((double)(width - 2 * num - num2) * 0.58));
		rPageG = new Rectangle(num, y, num3, 0);
		rPageD = new Rectangle(num + num3 + num2, y, Math.Max(Zx.P(360), (int)((double)(width - 2 * num - num3 - num2) * 0.82)), 0);
		int num4 = HauteurHero();
		int num5 = y + num4 + Zx.P(18);
		bJouerJeu.Text = (web ? "▶  JOUER" : "▶  JOUER");
		bJouerJeu.SetBounds(rPageG.X, num5, Zx.P(200), Zx.P(54));
		int num6 = rPageG.Right;
		Bouton[] array2 = new Bouton[3] { bVideoJeu, bPhotosJeu, bChemins };
		foreach (Bouton bouton in array2)
		{
			if (bouton != null && bouton.Visible)
			{
				bouton.Text = ((bouton == bChemins) ? "⚙  Dossiers" : ((bouton == bPhotosJeu) ? "▣  Photos" : "▶  Bande-annonce"));
				int lb = Lang.MeasureText(bouton.Text, bouton.Font).Width + Zx.P(30);
				bouton.SetBounds(num6 - lb, num5 + Zx.P(5), lb, Zx.P(44));
				bulle.SetToolTip(bouton, (bouton == bChemins) ? "Dossiers du jeu (OMSI 2 et openOMSI)" : ((bouton == bPhotosJeu) ? "Voir toutes les photos" : "Voir la bande-annonce en grand"));
				num6 -= lb + Zx.P(8);
			}
		}
		CalculerEtats();
		PlacerVideoJeu(page: true);
		PlacerFleche();
		pBiblio.Invalidate();
	}

	/// <summary>La bannière prend la place qui reste pour que toute la page tienne dans la fenêtre.</summary>
	private int HauteurHero()
	{
		int num = Math.Min(Zx.P(400), (int)((double)rPageG.Width * 0.36));
		int num2 = pBiblio.ClientSize.Height - rPageG.Y - Zx.P(24) - Zx.P(18) - Zx.P(78) - HauteurPhotos() - HauteurAPropos();
		return Math.Max(Zx.P(140), Math.Min(num, num2));
	}

	/// <summary>Bannière du jeu : library_hero de Steam si présente, sinon la couverture habituelle.</summary>
	private Image ImageHero()
	{
		if (heroJeuId == jeuId)
		{
			return heroJeu;
		}
		heroJeuId = jeuId;
		heroJeu = null;
		if (jeuId == "omsi2")
		{
			try
			{
				string text = Omsi.DetecteOmsi();
				List<string> list = new List<string>();
				string text2 = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null) as string;
				if (!string.IsNullOrEmpty(text2))
				{
					list.Add(text2.Replace('/', '\\'));
				}
				if (text != null)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(text);
					if (directoryInfo.Parent != null && directoryInfo.Parent.Parent != null && directoryInfo.Parent.Parent.Parent != null)
					{
						list.Add(directoryInfo.Parent.Parent.Parent.FullName);
					}
				}
				foreach (string item in list)
				{
					string path = Path.Combine(item, "appcache", "librarycache", "252530");
					if (!Directory.Exists(path))
					{
						continue;
					}
					string[] files = Directory.GetFiles(path, "library_hero.jpg", SearchOption.AllDirectories);
					if (files.Length != 0)
					{
						using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(files[0])))
						{
							using Image original = Image.FromStream(stream);
							heroJeu = new Bitmap(original);
						}
						break;
					}
				}
			}
			catch
			{
				heroJeu = null;
			}
		}
		if (heroJeu == null)
		{
			Dictionary<string, string> value;
			heroJeu = ((!(jeuId == "omsi2")) ? (FichesJeux.TryGetValue(jeuId ?? "", out value) ? CouvertureWeb(value) : null) : ((FichesJeux.TryGetValue("omsi2", out value) && ValJeu("omsi2", "image").Length > 0) ? (CouvertureWeb(value) ?? CouvertureJeu("omsi2")) : CouvertureJeu("omsi2")));
			if (heroJeu == null)
			{
				heroJeuId = null;
			}
		}
		return heroJeu;
	}

	// ---------- Peinture ----------

	private void Zone(Rectangle r, Action a, string b = null)
	{
		zonesB.Add(new ZoneB
		{
			R = r,
			Clic = a,
			Bulle = b
		});
	}

	private bool Survol(Rectangle r)
	{
		if (survolB != null)
		{
			return survolB.R == r;
		}
		return false;
	}

	private void PeindrePageJeu(Graphics g, int dy)
	{
		zonesB.Clear();
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBilinear;
		List<Projet> list = new List<Projet>();
		List<Projet> list2 = new List<Projet>();
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (JeuDe(projet) == jeuId)
				{
					list.Add(projet);
				}
			}
		}
		foreach (EtatMod item in etatsB)
		{
			list2.Add(item.P);
		}
		int num = nbMajJeu;
		string omsi = omsiB;
		int bas = PeindreColonneJeu(g, dy, list, list2, num);
		int val = PeindreMesMods(g, dy, list2, omsi);
		int height = Math.Max(bas, val) + Zx.P(50);
		if (pBiblio.AutoScrollMinSize.Height != height)
		{
			pBiblio.AutoScrollMinSize = new Size(0, height);
			PlacerFleche();
		}
	}

	private int PeindreColonneJeu(Graphics g, int dy, List<Projet> tous, List<Projet> installes, int majs)
	{
		Rectangle rectangle = new Rectangle(rPageG.X, rPageG.Y + dy, rPageG.Width, HauteurHero());
		using (GraphicsPath graphicsPath = Bouton.Rond(rectangle, Zx.P(14)))
		{
			GraphicsState gstate = g.Save();
			g.SetClip(graphicsPath, CombineMode.Intersect);
			Bitmap image = Couvrir(ImageHero(), rectangle.Width, rectangle.Height);
			if (image != null)
			{
				g.DrawImageUnscaled(image, rectangle.X, rectangle.Y);
			}
			else
			{
				using LinearGradientBrush brush = new LinearGradientBrush(rectangle, Color.FromArgb(70, 84, 170), Color.FromArgb(20, 24, 56), 35f);
				g.FillRectangle(brush, rectangle);
			}
			int num4 = rectangle.Height * 3 / 5;
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(rectangle.X, rectangle.Bottom - num4, rectangle.Width, num4 + 1), Color.FromArgb(0, 8, 10, 24), Color.FromArgb(235, 8, 10, 24), 90f))
			{
				g.FillRectangle(brush2, rectangle.X, rectangle.Bottom - num4, rectangle.Width, num4);
			}
			g.Restore(gstate);
			using Pen pen = new Pen(Color.FromArgb(60, 255, 255, 255));
			g.DrawPath(pen, graphicsPath);
		}
		int num5 = rectangle.X + Zx.P(28);
		Lang.DrawText(g, NomJeu(jeuId), fHeroNom, new Rectangle(num5, rectangle.Bottom - Zx.P(96), rectangle.Width - Zx.P(56), Zx.P(54)), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		Lang.DrawText(g, SousJeu(jeuId), fHeroSous, new Rectangle(num5, rectangle.Bottom - Zx.P(42), rectangle.Width - Zx.P(56), Zx.P(24)), Color.FromArgb(210, 218, 240), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		int num6 = rectangle.Bottom + Zx.P(18);
		Rectangle rectangle2 = new Rectangle(rPageG.X - Zx.P(8), num6 - Zx.P(8), rPageG.Width + Zx.P(16), Zx.P(70));
		using (GraphicsPath path = Bouton.Rond(rectangle2, Zx.P(12)))
		{
			using SolidBrush brush3 = new SolidBrush(Color.FromArgb(110, C.Carte));
			g.FillPath(brush3, path);
		}
		Compte actif = Comptes.Actif;
		int num7 = ((actif != null) ? Progression.S(actif, "g_" + jeuId + "_sec") : 0);
		string text = "—";
		if (actif != null)
		{
			DateTime dateTime = DateTime.MinValue;
			foreach (KeyValuePair<string, string> item in actif.Dernier)
			{
				Projet projet = tous.Find((Projet x) => x.Id == item.Key);
				if (projet != null && DateTime.TryParse(item.Value, out var result) && result > dateTime)
				{
					dateTime = result;
				}
			}
			if (dateTime > DateTime.MinValue)
			{
				int num8 = (int)(DateTime.Now.Date - dateTime.Date).TotalDays;
				text = ((num8 <= 0) ? "Aujourd'hui" : ((num8 == 1) ? "Hier" : ((num8 < 30) ? ("Il y a " + num8 + " jours") : dateTime.ToString("dd/MM/yyyy"))));
			}
		}
		List<string[]> list = new List<string[]>();
		list.Add(new string[2]
		{
			"MODS INSTALLÉS",
			installes.Count + " / " + tous.Count
		});
		list.Add(new string[2]
		{
			"TEMPS DE JEU",
			(num7 >= 3600) ? (num7 / 3600 + " h " + (num7 % 3600 / 60).ToString("00")) : (num7 / 60 + " min")
		});
		list.Add(new string[2] { "DERNIÈRE SESSION", text });
		if (majs > 0)
		{
			list.Add(new string[2]
			{
				"MISES À JOUR",
				majs + " disponible" + ((majs > 1) ? "s" : "")
			});
		}
		int num9 = rPageG.X + Zx.P(224);
		int num10 = rPageG.Right - Zx.P(16);
		Bouton[] array = new Bouton[3] { bChemins, bPhotosJeu, bVideoJeu };
		foreach (Bouton bouton in array)
		{
			if (bouton != null && bouton.Visible)
			{
				num10 -= bouton.Width + Zx.P(8);
			}
		}
		while (list.Count > 1 && (num10 - num9) / list.Count < Zx.P(96))
		{
			list.RemoveAt(list.Count - 1);
		}
		int num11 = Math.Min(Zx.P(170), (num10 - num9) / Math.Max(1, list.Count));
		if (num11 >= Zx.P(96))
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num12 = num9 + i * num11;
				Lang.DrawText(g, list[i][0], fStatL, new Rectangle(num12, num6 + Zx.P(6), num11 - Zx.P(10), Zx.P(16)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				Lang.DrawText(g, list[i][1], fStatV, new Rectangle(num12, num6 + Zx.P(24), num11 - Zx.P(10), Zx.P(26)), (i == 3) ? C.Bleu : C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				if (i == 3)
				{
					Rectangle rectangle3 = new Rectangle(num12 - Zx.P(6), num6, num11, Zx.P(54));
					Zone(rectangle3, delegate
					{
						filtre = 2;
						BasculerMods(versMods: true);
					}, "Voir les mises à jour");
				}
			}
		}
		int num13 = num6 + Zx.P(78);
		num13 = PeindrePhotosJeu(g, num13);
		num13 = PeindreAPropos(g, num13);
		return num13 - dy;
	}

	private void Vignette(Graphics g, Projet p, Rectangle r)
	{
		images.TryGetValue(p.Id, out var value);
		DessinerArrondi(g, value, r, Zx.P(8));
	}

	private Rectangle Pilule(Graphics g, string t, int droite, int y, Color c, bool plein)
	{
		int num = Lang.MeasureText(t, fPilule).Width + Zx.P(28);
		Rectangle rectangle = new Rectangle(droite - num, y, num, Zx.P(32));
		bool flag = Survol(rectangle);
		using (GraphicsPath path = Bouton.Rond(rectangle, Zx.P(16)))
		{
			using (SolidBrush brush = new SolidBrush(plein ? (flag ? ControlPaint.Light(c, 0.15f) : c) : Color.FromArgb(flag ? 70 : 40, c)))
			{
				g.FillPath(brush, path);
			}
			if (!plein)
			{
				using Pen pen = new Pen(Color.FromArgb(150, c));
				g.DrawPath(pen, path);
			}
		}
		Lang.DrawText(g, t, fPilule, rectangle, plein ? Color.White : c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		return rectangle;
	}

	private int PeindreMesMods(Graphics g, int dy, List<Projet> installes, string omsi)
	{
		int num = rPageD.X;
		int num2 = rPageD.Y + dy;
		int width = rPageD.Width;
		bool flag = JeuWebCourant() != null;
		int num3 = Zx.P(96);
		int num4 = Math.Max(1, installes.Count);
		Rectangle rectangle = new Rectangle(num, num2, width, Math.Max(Zx.P(70) + num4 * num3 + Zx.P(14) + ((installes.Count == 0) ? Zx.P(60) : 0), pBiblio.ClientSize.Height - rPageD.Y - Zx.P(40)));
		using (GraphicsPath graphicsPath = Bouton.Rond(rectangle, Zx.P(14)))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(200, C.Carte)))
			{
				g.FillPath(brush, graphicsPath);
			}
			using Pen pen = new Pen(C.Ligne);
			g.DrawPath(pen, graphicsPath);
		}
		Lang.DrawText(g, "Mes Cartes", fModNom, new Point(num + Zx.P(22), num2 + Zx.P(20)), C.Texte, TextFormatFlags.NoPadding);
		int num5 = num + Zx.P(22) + Lang.MeasureText("Mes Cartes", fModNom).Width + Zx.P(10);
		Lang.DrawText(g, installes.Count + ((installes.Count > 1) ? " installés" : " installé"), fModInfo, new Point(num5, num2 + Zx.P(23)), C.Gris, TextFormatFlags.NoPadding);
		int num6 = num2 + Zx.P(66);
		using (Pen pen2 = new Pen(C.Ligne))
		{
			g.DrawLine(pen2, num + Zx.P(18), num6 - Zx.P(4), rectangle.Right - Zx.P(18), num6 - Zx.P(4));
		}
		if (installes.Count == 0)
		{
			string t = (flag ? ("Les mods de " + NomJeu(jeuId) + " se lancent depuis le jeu. Parcourez-les dans l'onglet Mods.") : ((omsi == null) ? "OMSI 2 n'est pas encore trouvé : indiquez son dossier avec ⚙, puis installez des mods depuis l'onglet Mods." : "Aucun mod installé pour l'instant. Choisissez des cartes, bus ou livrées dans l'onglet Mods."));
			Lang.DrawText(g, t, fCorps, new Rectangle(num + Zx.P(22), num6 + Zx.P(14), width - Zx.P(44), Zx.P(50)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			Rectangle rectangle3 = Pilule(g, "Parcourir les mods", num + Zx.P(22) + Zx.P(180), num6 + Zx.P(74), C.Jade, plein: true);
			Zone(rectangle3, delegate
			{
				BasculerMods(versMods: true);
			});
			return rectangle.Bottom - dy;
		}
		foreach (EtatMod etat in etatsB)
		{
			Projet p = etat.P;
			Rectangle rectangle4 = new Rectangle(num + Zx.P(10), num6, width - Zx.P(20), num3 - Zx.P(6));
			bool flag2 = etat.Off;
			int code = etat.Code;
			string text = etat.VInst;
			Zone(rectangle4, delegate
			{
				Choisir(p);
				ShowVue(Vue.Projet);
			}, "Ouvrir la page de " + p.Nom);
			if (Survol(rectangle4) || (survolB != null && rectangle4.Contains(survolB.R.Location)))
			{
				using GraphicsPath path = Bouton.Rond(rectangle4, Zx.P(10));
				using SolidBrush brush2 = new SolidBrush(C.CarteSurvol);
				g.FillPath(brush2, path);
			}
			int num11 = ((width < Zx.P(600)) ? Zx.P(96) : Zx.P(128));
			Rectangle r = new Rectangle(rectangle4.X + Zx.P(10), rectangle4.Y + (rectangle4.Height - num11 * 9 / 16) / 2, num11, num11 * 9 / 16);
			Vignette(g, p, r);
			if (flag2)
			{
				using SolidBrush brush3 = new SolidBrush(Color.FromArgb(150, 8, 10, 24));
				using GraphicsPath path2 = Bouton.Rond(r, Zx.P(8));
				g.FillPath(brush3, path2);
			}
			int num7 = rectangle4.Right - Zx.P(14);
			Rectangle rectangle5 = new Rectangle(num7 - Zx.P(52), rectangle4.Y + (rectangle4.Height - Zx.P(28)) / 2, Zx.P(52), Zx.P(28));
			using (GraphicsPath path3 = Bouton.Rond(rectangle5, Zx.P(14)))
			{
				using SolidBrush brush4 = new SolidBrush(flag2 ? Color.FromArgb(52, 60, 96) : C.Vert);
				g.FillPath(brush4, path3);
			}
			using (SolidBrush brush5 = new SolidBrush(Color.White))
			{
				g.FillEllipse(brush5, flag2 ? (rectangle5.X + Zx.P(4)) : (rectangle5.Right - Zx.P(24)), rectangle5.Y + Zx.P(4), Zx.P(20), Zx.P(20));
			}
			Zone(new Rectangle(rectangle5.X - Zx.P(6), rectangle5.Y - Zx.P(6), rectangle5.Width + Zx.P(12), rectangle5.Height + Zx.P(12)), delegate
			{
				BasculerMod(p, omsi);
			}, flag2 ? "Activer ce mod" : "Désactiver ce mod (ses fichiers sont mis de côté, OMSI 2 ne le charge plus)");
			int num8 = rectangle5.X - Zx.P(14);
			int y = rectangle4.Y + (rectangle4.Height - Zx.P(32)) / 2;
			int num9 = r.Right + Zx.P(16) + Zx.P(110);
			if (p.Carte.Length > 0 && !flag)
			{
				Rectangle rectangle6 = new Rectangle(num8 - Zx.P(150), y, Zx.P(150), Zx.P(32));
				if (rectangle6.X > num9)
				{
					rectangle6 = Pilule(g, "▶  Jouer", num8, y, C.Orange, plein: true);
					Zone(rectangle6, delegate
					{
						JouerCarte(p);
					}, "Lancer openOMSI directement sur " + p.Nom);
					num8 = rectangle6.X - Zx.P(10);
				}
			}
			if (code == 2)
			{
				Rectangle rectangle7 = new Rectangle(num8 - Zx.P(150), y, Zx.P(150), Zx.P(32));
				if (rectangle7.X > num9)
				{
					rectangle7 = Pilule(g, "Mettre à jour", num8, y, C.Bleu, plein: false);
					Zone(rectangle7, delegate
					{
						Choisir(p);
						ShowVue(Vue.Projet);
						Installer(p);
					}, "Télécharger la nouvelle version " + p.Version);
					num8 = rectangle7.X - Zx.P(10);
				}
			}
			int num10 = r.Right + Zx.P(16);
			int width2 = Math.Max(Zx.P(60), num8 - num10 - Zx.P(6));
			Lang.DrawText(g, p.Nom, fModNom, new Rectangle(num10, rectangle4.Y + Zx.P(14), width2, Zx.P(24)), flag2 ? C.Gris : C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			Lang.DrawText(g, text + "  ·  " + Categories.Nom(p.Categorie), fModInfo, new Rectangle(num10, rectangle4.Y + Zx.P(40), width2, Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			string t2 = (flag2 ? "Désactivé" : ((code == 2) ? ("Mise à jour disponible" + ((p.Version != text) ? (" : " + p.Version) : "")) : "Activé · à jour"));
			Color color = (flag2 ? C.Orange : ((code == 2) ? C.Bleu : C.Vert));
			using (SolidBrush brush6 = new SolidBrush(color))
			{
				g.FillEllipse(brush6, num10, rectangle4.Y + Zx.P(66), Zx.P(8), Zx.P(8));
			}
			Lang.DrawText(g, t2, fModInfo, new Rectangle(num10 + Zx.P(14), rectangle4.Y + Zx.P(61), width2 - Zx.P(14), Zx.P(18)), color, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			num6 += num3;
		}
		return rectangle.Bottom - dy;
	}

	// ---------- Actions de « Mes mods » ----------

	private void BasculerMod(Projet p, string omsi)
	{
		if (omsi == null)
		{
			return;
		}
		bool flag = Omsi.Desactive(p, omsi);
		if (Omsi.JeuLance())
		{
			Toast("Fermez OMSI 2 / openOMSI avant d'activer ou de désactiver un mod.", C.Orange, 5000);
			return;
		}
		try
		{
			base.UseWaitCursor = true;
			Omsi.Basculer(p, omsi, flag);
			Toast("✔  « " + p.Nom + " » " + (flag ? "activé" : "désactivé"), flag ? C.Vert : C.Orange, 3500);
		}
		catch (Exception ex)
		{
			Toast("« " + p.Nom + " » : " + ex.Message, C.Rouge, 6000);
			Journal.Une("mods-basculer", ex);
		}
		finally
		{
			base.UseWaitCursor = false;
		}
		CalculerEtats();
		pBiblio.Invalidate();
	}

	/// <summary>Lance openOMSI directement sur la carte du mod (même chemin que JOUER sur sa page).</summary>
	private void JouerCarte(Projet p)
	{
		Choisir(p);
		lAlerte.Visible = false;
		Jouer();
		if (lAlerte.Visible && lAlerte.Text.Length > 0)
		{
			Toast(lAlerte.Text, (lAlerte.ForeColor == C.Vert) ? C.Vert : C.Orange, 7000);
		}
	}
}
