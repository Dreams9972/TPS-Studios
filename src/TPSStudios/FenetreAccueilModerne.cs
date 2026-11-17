using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Accueil moderne : barre de navigation à gauche, en-tête de bienvenue, grande bannière des derniers mods
/// (qui défile toute seule), rangée « Continuer à jouer », et à droite profil, actualités, amis, Discord.
/// </summary>
internal partial class Fenetre
{
	private Rectangle rRail;

	private Rectangle rEnTeteA;

	private Rectangle rHero;

	private Rectangle rToutVoir;

	private readonly List<Projet> heroProjets = new List<Projet>();

	private int heroIdx;

	private int heroPrec = -1;

	private double heroFondu = 1.0;

	private Timer tHero;

	private Timer tHeroFondu;

	private DateTime heroDepuis = DateTime.Now;

	private readonly List<KeyValuePair<Rectangle, Projet>> rContinuer = new List<KeyValuePair<Rectangle, Projet>>();

	private bool continuerHistorique;

	private int survolAccueil = -2;

	private static readonly Font fBonjour = new Font("Segoe UI Semibold", Zx.P(24f));

	private static readonly Font fBonjourSous = new Font("Segoe UI", Zx.P(11f));

	private static readonly Font fAccHeroTitre = new Font("Segoe UI Semibold", Zx.P(28f));

	private static readonly Font fAccHeroSous = new Font("Segoe UI", Zx.P(11.5f));

	private static readonly Font fAccHeroPetit = new Font("Segoe UI Semibold", Zx.P(8.5f));

	private static readonly Font fSection = new Font("Segoe UI Semibold", Zx.P(14f));

	private static readonly Font fCarteNom = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fCarteSous = new Font("Segoe UI", Zx.P(8.5f));

	/// <summary>Mise en page de l'accueil moderne.</summary>
	private void DisposerAccueilModerne()
	{
		// rien ne défile sur l'accueil : pas de barre de défilement
		if (pAccueil.AutoScroll)
		{
			pAccueil.AutoScroll = false;
			pAccueil.AutoScrollMinSize = Size.Empty;
		}
		int width = pAccueil.ClientSize.Width;
		int height = pAccueil.ClientSize.Height;
		int R = Zx.P(84);
		rRail = new Rectangle(0, 0, R, height);
		// barre de navigation : les boutons en colonne, « Quitter » en bas
		int hi = Zx.P(62);
		int yi = Zx.P(92);
		List<Rectangle> list = new List<Rectangle>();
		for (int i = 0; i < tuilesA.Count; i++)
		{
			Tuile t = tuilesA[i];
			bool dernier = i == tuilesA.Count - 1;
			int y = (dernier ? (height - Zx.P(20) - hi) : yi);
			t.SetBounds(0, y, R, hi);
			t.IndiceDecor = i;
			list.Add(t.Bounds);
			if (!dernier)
			{
				yi += hi + Zx.P(4);
			}
		}
		ThemeBase.Tuiles = list.ToArray();
		etatsTuiles = new int[list.Count];
		int marge = Zx.P(32);
		int xMain = R + marge;
		int rc = Math.Max(Zx.P(290), Math.Min(Zx.P(370), (int)((double)width * 0.24)));
		int xRC = width - marge - rc;
		int mainW = xRC - Zx.P(28) - xMain;
		if (mainW < Zx.P(480))
		{
			rc = Math.Max(Zx.P(250), width - xMain - marge - Zx.P(28) - Zx.P(480));
			xRC = width - marge - rc;
			mainW = Math.Max(Zx.P(300), xRC - Zx.P(28) - xMain);
		}
		CalculerMajAccueil();
		bool flag0 = MajAAfficher || Opt("alerte").Length > 0 || Maintenance;
		rEnTeteA = new Rectangle(xMain, Zx.P(28), mainW, Zx.P(64));
		rHorloge = rEnTeteA;
		rCloche = new Rectangle(xMain + mainW - Zx.P(44), Zx.P(36), Zx.P(44), Zx.P(44));
		int y0 = Zx.P(112);
		rBandeau = new Rectangle(xMain, y0, mainW, Zx.P(36));
		rMajA = (MajAAfficher ? new Rectangle(xMain, y0 - Zx.P(4), mainW, Zx.P(48)) : Rectangle.Empty);
		if (flag0)
		{
			y0 += Zx.P(60);
		}
		// bannière : grande, mais en laissant la place à la rangée « Continuer à jouer »
		int hHero = Math.Max(Zx.P(200), Math.Min((int)((double)mainW * 0.42), height - y0 - Zx.P(250)));
		rHero = new Rectangle(xMain, y0, mainW, hHero);
		ChoisirProjetsAccueil();
		int yT = rHero.Bottom + Zx.P(22);
		Size st = Lang.MeasureText("Tout voir  ›", fCarteSous);
		rToutVoir = new Rectangle(xMain + mainW - st.Width - Zx.P(4), yT + Zx.P(6), st.Width + Zx.P(8), st.Height + Zx.P(6));
		int yC = yT + Zx.P(40);
		int nC = ((mainW >= Zx.P(760)) ? 4 : 3);
		int gap = Zx.P(16);
		int cw = (mainW - (nC - 1) * gap) / nC;
		int ch = Math.Max(Zx.P(90), Math.Min(cw * 56 / 100 + Zx.P(54), height - yC - Zx.P(24)));
		rContinuer.Clear();
		List<Projet> cont = ProjetsContinuer(nC);
		for (int j = 0; j < cont.Count; j++)
		{
			rContinuer.Add(new KeyValuePair<Rectangle, Projet>(new Rectangle(xMain + j * (cw + gap), yC, cw, ch), cont[j]));
		}
		// colonne de droite
		int num2 = Math.Max(Zx.P(64), Math.Min(Zx.P(74), (int)((double)height * 0.07)));
		rProfil = new Rectangle(xRC, Zx.P(28), rc, num2);
		int hCarte = Math.Max(Zx.P(200), Math.Min(Zx.P(330), (int)((double)rc * 0.92)));
		rCarte = new Rectangle(xRC, rProfil.Bottom + Zx.P(48), rc, hCarte);
		rInfos = Rectangle.Empty;
		rPilule = new Rectangle(xRC, rCarte.Bottom + Zx.P(14), rc, Zx.P(44));
		rDiscord = new Rectangle(xRC, height - Zx.P(40) - Zx.P(48), rc, Zx.P(48));
		int yAmis = rPilule.Bottom + Zx.P(44);
		rAmisA = new Rectangle(xRC, yAmis, rc, Math.Max(0, Math.Min(Zx.P(420), rDiscord.Y - Zx.P(20) - yAmis)));
		if (rAmisA.Height < Zx.P(110))
		{
			rAmisA = Rectangle.Empty;
		}
		rMusique = Rectangle.Empty;
		ThemeBase.Cartes = new Rectangle[2] { rProfil, rCarte };
		ThemeBase.ZoneLibre = new Rectangle(xMain + mainW / 2, Zx.P(16), Math.Max(Zx.P(100), mainW / 2 - Zx.P(70)), Zx.P(90));
		DemarrerDefilementHero();
		DemarrerMesh();
		pAccueil.Invalidate();
	}

	private Timer tMesh;

	private System.Diagnostics.Stopwatch swMesh;

	/// <summary>
	/// Anime le fond mesh (~30 images/s) quand l'accueil ou la page Jeux est affiché sans thème de fête,
	/// fenêtre active, sans transition ni page par-dessus. Ailleurs le fond reste figé (aucun coût).
	/// </summary>
	private void DemarrerMesh()
	{
		if (tMesh != null)
		{
			return;
		}
		swMesh = System.Diagnostics.Stopwatch.StartNew();
		long dernier = 0L;
		tMesh = new Timer();
		tMesh.Interval = 33;
		tMesh.Tick += delegate
		{
			long ms = swMesh.ElapsedMilliseconds;
			double dt = Math.Min(0.1, (double)(ms - dernier) / 1000.0);
			dernier = ms;
			if (!FondAnime.Mesh || !Fluide || ModeCapture || !base.Visible || base.WindowState == FormWindowState.Minimized || couches.Count > 0 || enTransition || Form.ActiveForm != this || (page != null && page.Visible))
			{
				return;
			}
			PanelFond p = PanelVisible();
			if ((p == pAccueil && !themeNoel) || (p == pJeux && themeJeux == null))
			{
				fond.TempsMesh += dt;
				p.Invalidate();
				barreTitre?.Invalidate();
			}
		};
		tMesh.Start();
	}

	/// <summary>Les derniers mods publiés, pour la bannière.</summary>
	private void ChoisirProjetsAccueil()
	{
		Projet ancien = ((heroIdx >= 0 && heroIdx < heroProjets.Count) ? heroProjets[heroIdx] : null);
		heroProjets.Clear();
		if (cat == null)
		{
			return;
		}
		List<Projet> l = cat.Projets.FindAll((Projet p) => p.StatutEffectif != "masque" && p.Installable);
		l.Sort((Projet a, Projet b) => DateMod(b).CompareTo(DateMod(a)));
		for (int i = 0; i < l.Count && heroProjets.Count < 5; i++)
		{
			heroProjets.Add(l[i]);
		}
		heroIdx = ((ancien != null) ? Math.Max(0, heroProjets.FindIndex((Projet p) => p.Id == ancien.Id)) : 0);
	}

	/// <summary>« Continuer à jouer » : les derniers mods joués, sinon ceux installés, sinon les nouveautés.</summary>
	private List<Projet> ProjetsContinuer(int n)
	{
		List<Projet> res = new List<Projet>();
		continuerHistorique = false;
		if (cat == null)
		{
			return res;
		}
		Compte actif = Comptes.Actif;
		if (actif != null && actif.Dernier.Count > 0)
		{
			List<KeyValuePair<string, DateTime>> joues = new List<KeyValuePair<string, DateTime>>();
			foreach (KeyValuePair<string, string> item in actif.Dernier)
			{
				if (DateTime.TryParse(item.Value, out var d))
				{
					joues.Add(new KeyValuePair<string, DateTime>(item.Key, d));
				}
			}
			joues.Sort((KeyValuePair<string, DateTime> a, KeyValuePair<string, DateTime> b) => b.Value.CompareTo(a.Value));
			foreach (KeyValuePair<string, DateTime> j in joues)
			{
				Projet p = cat.Projets.Find((Projet x) => x.Id == j.Key);
				if (p != null && res.Count < n && !res.Contains(p))
				{
					res.Add(p);
				}
			}
			continuerHistorique = res.Count > 0;
		}
		string omsi = null;
		try
		{
			omsi = Omsi.DetecteOmsi();
		}
		catch
		{
		}
		foreach (Projet p2 in cat.Projets)
		{
			if (res.Count >= n)
			{
				break;
			}
			if (!res.Contains(p2) && p2.Installable && p2.StatutEffectif != "masque" && omsi != null && Omsi.Installee(p2, omsi) != null)
			{
				res.Add(p2);
			}
		}
		foreach (Projet p3 in heroProjets)
		{
			if (res.Count >= n)
			{
				break;
			}
			if (!res.Contains(p3))
			{
				res.Add(p3);
			}
		}
		return res;
	}

	private void DemarrerDefilementHero()
	{
		if (tHero == null)
		{
			tHero = new Timer();
			tHero.Interval = 1000;
			tHero.Tick += delegate
			{
				if (heroProjets.Count < 2 || !pAccueil.Visible || couches.Count > 0 || survolAccueil == -1 || (DateTime.Now - heroDepuis).TotalSeconds < 8.0)
				{
					return;
				}
				AllerHero((heroIdx + 1) % heroProjets.Count);
			};
			tHero.Start();
		}
	}

	private void AllerHero(int i)
	{
		if (i == heroIdx || i < 0 || i >= heroProjets.Count)
		{
			return;
		}
		heroPrec = heroIdx;
		heroIdx = i;
		heroDepuis = DateTime.Now;
		heroFondu = 0.0;
		if (!Fluide)
		{
			heroFondu = 1.0;
			pAccueil.Invalidate(Rectangle.Inflate(rHero, Zx.P(4), Zx.P(4)));
			return;
		}
		if (tHeroFondu == null)
		{
			tHeroFondu = new Timer();
			tHeroFondu.Interval = 16;
			tHeroFondu.Tick += delegate
			{
				heroFondu = Math.Min(1.0, heroFondu + 0.07);
				pAccueil.Invalidate(Rectangle.Inflate(rHero, Zx.P(4), Zx.P(4)));
				if (heroFondu >= 1.0)
				{
					tHeroFondu.Stop();
				}
			};
		}
		tHeroFondu.Start();
	}

	/// <summary>Image de la bannière : la première capture de la galerie (grande, sans texte incrusté), sinon l'image du mod.</summary>
	private Image ImageHeroAcc(Projet p)
	{
		if (p == null)
		{
			return null;
		}
		if (galImages.TryGetValue(p.Id, out var l) && l != null)
		{
			foreach (Image im in l)
			{
				if (im != null)
				{
					return im;
				}
			}
		}
		else if (p.Galerie.Count > 0)
		{
			ChargerGalerie(p);
		}
		return ImageProjet(p);
	}

	private Projet HeroCourant => (heroIdx >= 0 && heroIdx < heroProjets.Count) ? heroProjets[heroIdx] : null;

	/// <summary>Zone cliquable de l'accueil moderne sous la souris : -1 bannière, 0..n carte, 100+i point de la bannière, 200 « Tout voir », -2 rien.</summary>
	private int ZoneAccueil(Point p)
	{
		for (int i = 0; i < heroProjets.Count && heroProjets.Count > 1; i++)
		{
			if (Rectangle.Inflate(PointHero(i), Zx.P(5), Zx.P(6)).Contains(p))
			{
				return 100 + i;
			}
		}
		if (!rHero.IsEmpty && rHero.Contains(p) && HeroCourant != null)
		{
			return -1;
		}
		for (int j = 0; j < rContinuer.Count; j++)
		{
			if (rContinuer[j].Key.Contains(p))
			{
				return j;
			}
		}
		if (!rToutVoir.IsEmpty && rToutVoir.Contains(p))
		{
			return 200;
		}
		return -2;
	}

	private Rectangle PointHero(int i)
	{
		int n = heroProjets.Count;
		int d = Zx.P(8);
		int g = Zx.P(8);
		int large = Zx.P(22);
		int total = (n - 1) * (d + g) + large;
		int x = rHero.Right - Zx.P(28) - total;
		for (int k = 0; k < i; k++)
		{
			x += ((k == heroIdx) ? large : d) + g;
		}
		return new Rectangle(x, rHero.Bottom - Zx.P(30), (i == heroIdx) ? large : d, d);
	}

	/// <summary>Souris sur l'accueil moderne : survol et curseur. Vrai si la souris est sur une zone cliquable.</summary>
	private bool SourisAccueil(Point p)
	{
		int z = ZoneAccueil(p);
		if (z != survolAccueil)
		{
			int ancien = survolAccueil;
			survolAccueil = z;
			InvaliderZoneAccueil(ancien);
			InvaliderZoneAccueil(z);
		}
		return z != -2;
	}

	private void InvaliderZoneAccueil(int z)
	{
		if (z == -1 || z >= 100)
		{
			pAccueil.Invalidate(Rectangle.Inflate(rHero, Zx.P(6), Zx.P(6)));
		}
		else if (z >= 0 && z < rContinuer.Count)
		{
			pAccueil.Invalidate(Rectangle.Inflate(rContinuer[z].Key, Zx.P(8), Zx.P(8)));
		}
		else if (z == 200)
		{
			pAccueil.Invalidate(Rectangle.Inflate(rToutVoir, Zx.P(4), Zx.P(4)));
		}
	}

	/// <summary>Clic sur l'accueil moderne. Vrai si le clic a été traité.</summary>
	private bool ClicAccueil(Point p)
	{
		int z = ZoneAccueil(p);
		if (z == -2)
		{
			return false;
		}
		Sons.Clic();
		if (z >= 100 && z < 200)
		{
			AllerHero(z - 100);
		}
		else if (z == 200)
		{
			ShowVue(Vue.Biblio);
		}
		else
		{
			Projet pr = ((z == -1) ? HeroCourant : rContinuer[z].Value);
			if (pr != null)
			{
				Choisir(pr);
				ShowVue(Vue.Projet);
			}
		}
		return true;
	}

	private void PeindreRailFond(Graphics g)
	{
		if (rRail.IsEmpty)
		{
			return;
		}
		using (SolidBrush b = new SolidBrush(Color.FromArgb(150, 6, 8, 20)))
		{
			g.FillRectangle(b, rRail);
		}
		using (Pen p = new Pen(Color.FromArgb(22, 255, 255, 255)))
		{
			g.DrawLine(p, rRail.Right - 1, 0, rRail.Right - 1, rRail.Bottom);
		}
		if (logo != null)
		{
			int t = Zx.P(46);
			Rectangle rl = new Rectangle((rRail.Width - t) / 2, Zx.P(24), t, t);
			DessinerArrondi(g, logo, rl, Zx.P(12));
		}
	}

	private void PeindreEnTeteAccueil(Graphics g)
	{
		if (rEnTeteA.IsEmpty)
		{
			return;
		}
		DateTime n = DateTime.Now;
		string salut = ((n.Hour < 5) ? "Bonne nuit" : ((n.Hour < 12) ? "Bonjour" : ((n.Hour < 18) ? "Bon après-midi" : "Bonsoir")));
		Compte a = Comptes.Actif;
		string t = Lang.T(salut) + ((a != null && a.Pseudo.Length > 0) ? (", " + a.Pseudo) : "");
		Lang.DrawText(g, t, fBonjour, new Rectangle(rEnTeteA.X, rEnTeteA.Y, rEnTeteA.Width - Zx.P(60), Zx.P(40)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
		CultureInfo ci;
		try
		{
			ci = CultureInfo.GetCultureInfo(Lang.Code);
		}
		catch
		{
			ci = CultureInfo.GetCultureInfo("fr-FR");
		}
		string jour = n.ToString("dddd d MMMM", ci);
		if (jour.Length > 0)
		{
			jour = char.ToUpper(jour[0]) + jour.Substring(1);
		}
		Lang.DrawText(g, jour + "   ·   " + n.ToString("HH:mm:ss"), fBonjourSous, new Point(rEnTeteA.X + Zx.P(2), rEnTeteA.Y + Zx.P(42)), C.Gris, TextFormatFlags.NoPadding);
	}

	private void PeindreHero(Graphics g)
	{
		if (rHero.Width < Zx.P(100) || rHero.Height < Zx.P(80))
		{
			return;
		}
		Projet p = HeroCourant;
		int rayon = Zx.P(18);
		using (GraphicsPath path = Bouton.Rond(rHero, rayon))
		{
			using SolidBrush b0 = new SolidBrush(C.Carte);
			g.FillPath(b0, path);
		}
		if (p == null)
		{
			Lang.DrawText(g, "Chargement des nouveautés…", fAccHeroSous, rHero, C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			return;
		}
		Image im = ImageHeroAcc(p);
		if (heroFondu < 1.0 && heroPrec >= 0 && heroPrec < heroProjets.Count)
		{
			Image ip = ImageHeroAcc(heroProjets[heroPrec]);
			if (ip != null)
			{
				DessinerArrondi(g, ip, rHero, rayon);
			}
			if (im != null)
			{
				Bitmap c = Couvrir(im, rHero.Width, rHero.Height);
				if (c != null)
				{
					GraphicsState st = g.Save();
					using (GraphicsPath pc = Bouton.Rond(rHero, rayon))
					{
						g.SetClip(pc, CombineMode.Intersect);
					}
					AlphaDraw(g, c, rHero, (float)heroFondu);
					g.Restore(st);
				}
			}
		}
		else if (im != null)
		{
			DessinerArrondi(g, im, rHero, rayon);
		}
		GraphicsState st2 = g.Save();
		using (GraphicsPath pc2 = Bouton.Rond(rHero, rayon))
		{
			g.SetClip(pc2, CombineMode.Intersect);
		}
		// voiles pour la lisibilité du texte : à gauche et en bas
		using (LinearGradientBrush lg = new LinearGradientBrush(new Rectangle(rHero.X - 1, rHero.Y, rHero.Width / 2 + 2, rHero.Height), Color.FromArgb(215, 6, 8, 22), Color.FromArgb(0, 6, 8, 22), 0f))
		{
			g.FillRectangle(lg, rHero.X, rHero.Y, rHero.Width / 2 + 1, rHero.Height);
		}
		using (LinearGradientBrush lb = new LinearGradientBrush(new Rectangle(rHero.X, rHero.Bottom - rHero.Height / 2 - 1, rHero.Width, rHero.Height / 2 + 2), Color.FromArgb(0, 6, 8, 22), Color.FromArgb(190, 6, 8, 22), 90f))
		{
			g.FillRectangle(lb, rHero.X, rHero.Bottom - rHero.Height / 2, rHero.Width, rHero.Height / 2);
		}
		if (survolAccueil == -1)
		{
			using SolidBrush bs = new SolidBrush(Color.FromArgb(18, 255, 255, 255));
			g.FillRectangle(bs, rHero);
		}
		g.Restore(st2);
		int x = rHero.X + Zx.P(32);
		int largeur = Math.Min(rHero.Width * 55 / 100, Zx.P(560));
		// étiquette
		string etiq = (heroIdx == 0) ? "NOUVEAUTÉ" : "À DÉCOUVRIR";
		Size se = Lang.MeasureText(etiq, fAccHeroPetit);
		Rectangle re = new Rectangle(x, rHero.Y + Zx.P(26), se.Width + Zx.P(20), Zx.P(24));
		using (GraphicsPath pe = Bouton.Rond(re, Zx.P(12)))
		{
			using SolidBrush be = new SolidBrush((heroIdx == 0) ? C.Orange : Color.FromArgb(120, 90, 110, 230));
			g.FillPath(be, pe);
		}
		Txt.Dessiner(g, etiq, fAccHeroPetit, re, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		// titre, sous-titre, informations
		int yb = rHero.Bottom - Zx.P(34);
		string info = Categories.Nom(p.Categorie) + ((p.Version.Length > 0) ? ("  ·  " + p.Version) : "") + "  ·  " + NomJeu(JeuDe(p));
		Size si = Lang.MeasureText(info, fCarteSous);
		yb -= si.Height;
		Lang.DrawText(g, info, fCarteSous, new Point(x, yb), Color.FromArgb(170, 182, 214), TextFormatFlags.NoPadding);
		string sous = p.SousTitre ?? "";
		if (sous.Length > 0)
		{
			Size ss = Lang.MeasureText(sous, fAccHeroSous, new Size(largeur, Zx.P(60)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			int hs = Math.Min(ss.Height, Zx.P(46));
			yb -= hs + Zx.P(8);
			Lang.DrawText(g, sous, fAccHeroSous, new Rectangle(x, yb, largeur, hs), Color.FromArgb(222, 228, 242), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		}
		Size sn = Lang.MeasureText(p.Nom, fAccHeroTitre);
		yb -= sn.Height + Zx.P(4);
		Lang.DrawText(g, p.Nom, fAccHeroTitre, new Rectangle(x, yb, largeur, sn.Height + Zx.P(4)), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
		// bouton « Découvrir »
		string bt = "Découvrir  ›";
		Size sb = Lang.MeasureText(bt, fCarteNom);
		Rectangle rb = new Rectangle(rHero.Right - Zx.P(32) - sb.Width - Zx.P(36), rHero.Y + Zx.P(24), sb.Width + Zx.P(36), Zx.P(38));
		using (GraphicsPath pb = Bouton.Rond(rb, Zx.P(19)))
		{
			using SolidBrush bb = new SolidBrush((survolAccueil == -1) ? Color.White : Color.FromArgb(232, 240, 244, 255));
			g.FillPath(bb, pb);
		}
		Txt.Dessiner(g, bt, fCarteNom, rb, Color.FromArgb(14, 18, 40), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		// points du défilement
		if (heroProjets.Count > 1)
		{
			for (int i = 0; i < heroProjets.Count; i++)
			{
				Rectangle rp = PointHero(i);
				using GraphicsPath pp = Bouton.Rond(rp, rp.Height / 2);
				using SolidBrush bp = new SolidBrush((i == heroIdx) ? Color.White : Color.FromArgb((survolAccueil == 100 + i) ? 200 : 110, 255, 255, 255));
				g.FillPath(bp, pp);
			}
		}
	}

	private void PeindreContinuer(Graphics g)
	{
		if (rContinuer.Count == 0 || rHero.IsEmpty)
		{
			return;
		}
		int yT = rHero.Bottom + Zx.P(22);
		Lang.DrawText(g, continuerHistorique ? "Continuer à jouer" : "Vos mods", fSection, new Point(rHero.X, yT), C.Texte, TextFormatFlags.NoPadding);
		Lang.DrawText(g, "Tout voir  ›", fCarteSous, new Point(rToutVoir.X + Zx.P(4), rToutVoir.Y + Zx.P(3)), (survolAccueil == 200) ? Color.White : C.Bleu, TextFormatFlags.NoPadding);
		Compte a = Comptes.Actif;
		for (int i = 0; i < rContinuer.Count; i++)
		{
			Rectangle r = rContinuer[i].Key;
			Projet p = rContinuer[i].Value;
			bool sv = survolAccueil == i;
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
			Image im = ImageProjet(p);
			if (im != null)
			{
				DessinerArrondi(g, im, ri, Zx.P(10));
			}
			else
			{
				using GraphicsPath pv = Bouton.Rond(ri, Zx.P(10));
				using SolidBrush bv = new SolidBrush(C.Ligne);
				g.FillPath(bv, pv);
			}
			Lang.DrawText(g, p.Nom, fCarteNom, new Rectangle(r.X + Zx.P(14), r.Y + hi + Zx.P(8), r.Width - Zx.P(28), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
			string sous = Categories.Nom(p.Categorie) + ((p.Version.Length > 0) ? ("  ·  " + p.Version) : "");
			if (continuerHistorique && a != null && a.Dernier.TryGetValue(p.Id, out var dj) && DateTime.TryParse(dj, out var d))
			{
				TimeSpan ec = DateTime.Now - d;
				sous = ((ec.TotalDays < 1.0) ? "Joué aujourd'hui" : ((ec.TotalDays < 2.0) ? "Joué hier" : ("Joué il y a " + (int)ec.TotalDays + " j")));
			}
			Lang.DrawText(g, sous, fCarteSous, new Rectangle(r.X + Zx.P(14), r.Y + hi + Zx.P(30), r.Width - Zx.P(28), Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			if (sv)
			{
				// pastille « ▶ » sur l'image au survol
				int t = Zx.P(42);
				Rectangle rj = new Rectangle(ri.Right - t - Zx.P(10), ri.Bottom - t - Zx.P(10), t, t);
				using SolidBrush bj = new SolidBrush(C.Vert);
				g.FillEllipse(bj, rj);
				Txt.Dessiner(g, "▶", fCarteNom, new Rectangle(rj.X + Zx.P(2), rj.Y, rj.Width, rj.Height), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
		}
	}
}
