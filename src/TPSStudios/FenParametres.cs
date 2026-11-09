using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenParametres : Form, IPage
{
	private Segments sgTaille;

	private Option cMajL;

	private Option cMajP;

	private Option cRed;

	private Option cDem;

	private Option cFerme;

	private Option cAnim;

	private Option cDisc;

	private Option cSons;

	private Option cNotifWin;

	private List<Option> cNotif = new List<Option>();

	private Dictionary<string, Option> cSuivis = new Dictionary<string, Option>();

	public static int SectionInitiale;

	private Champ tAppDisc;

	private Segments sgPage;

	private Segments sgTheme;

	private Segments sgTheme2;

	private Segments sgLangue;

	private Segments sgMus;

	public bool ChangeLangue;

	private Bouton bCache;

	private Label lCache;

	private Panel contenu;

	private int cw;

	private int y;

	private int CW;

	private int X0;

	private List<Cadre> sections = new List<Cadre>();

	private NavVert nav;

	public bool DemandeDossiers;

	public bool CacheVide;

	public bool PleinEcran;

	public bool DemandeMaj;

	public bool Reinitialise;

	public bool Relancer;

	public int SectionCourante;

	private Label LastDetail;

	public FenParametres(bool pleinActuel, string versionDispo, bool appFournie = false)
	{
		PleinEcran = pleinActuel;
		Text = "Paramètres";
		Size hote = Integre.Hote;
		CW = Math.Max(Math.Min(hote.Width, Zx.P(900)), Math.Min(hote.Width - Zx.P(48), Zx.P(2200)));
		X0 = (hote.Width - CW) / 2;
		base.ClientSize = hote;
		base.FormBorderStyle = FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterParent;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		try
		{
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		catch
		{
		}
		Theme.Fenetre(this);
		Label value = new Label
		{
			Text = "Paramètres",
			Font = new Font("Segoe UI Semibold", Zx.P(18f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(X0 + Zx.P(84), Zx.P(16)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Personnalisez le lanceur TPS Groupe",
			Font = new Font("Segoe UI", Zx.P(10f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(X0 + Zx.P(86), Zx.P(52)),
			AutoSize = true
		};
		base.Controls.Add(value2);
		base.Controls.Add(Integre.Retour(delegate
		{
			base.DialogResult = DialogResult.Cancel;
		}, X0 + Zx.P(24), Zx.P(18)));
		int num = Zx.P(240);
		int x = X0 + Zx.P(24) + num + Zx.P(24);
		int num2 = CW - Zx.P(48) - num - Zx.P(24);
		nav = new NavVert();
		nav.Location = new Point(X0 + Zx.P(24), Zx.P(96));
		nav.Size = new Size(num, 7 * Zx.P(48));
		base.Controls.Add(nav);
		contenu = new Panel();
		contenu.Location = new Point(x, Zx.P(96));
		contenu.Size = new Size(num2, base.ClientSize.Height - Zx.P(96) - Zx.P(80));
		contenu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
		contenu.AutoScroll = true;
		contenu.BackColor = C.Fond;
		base.Controls.Add(contenu);
		Theme.Sombre(contenu);
		cw = num2 - SystemInformation.VerticalScrollBarWidth - Zx.P(2);
		y = 0;
		Cadre cadre = Section("Affichage", Zx.P(472));
		cSons = Opt(cadre, 0, "Sons de l'interface", "Un léger son à chaque clic sur un bouton ou un onglet.", Sons.Actifs);
		Etiquette(cadre, "Page affichée au démarrage", Zx.P(24), Zx.P(116));
		sgPage = new Segments();
		sgPage.Items = new string[3] { "Accueil", "Bibliothèque", "Dernier projet joué" };
		sgPage.Location = new Point(Zx.P(24), Zx.P(142));
		sgPage.Width = cw - Zx.P(48);
		sgPage.Etire = true;
		string text = Config.Lire("page_demarrage");
		sgPage.Index = ((text == "biblio") ? 1 : ((text == "recent") ? 2 : 0));
		cadre.Controls.Add(sgPage);
		Etiquette(cadre, "Thème des fêtes  ·  Automatique : selon le calendrier (Noël, Halloween, Pâques…)", Zx.P(24), Zx.P(196));
		sgTheme = new Segments();
		sgTheme.Items = new string[5]
		{
			Themes.Noms[0],
			Themes.Noms[1],
			Themes.Noms[2],
			Themes.Noms[3],
			Themes.Noms[4]
		};
		sgTheme.Location = new Point(Zx.P(24), Zx.P(222));
		sgTheme.Width = cw - Zx.P(48);
		sgTheme.Etire = true;
		sgTheme2 = new Segments();
		sgTheme2.Items = new string[5]
		{
			Themes.Noms[5],
			Themes.Noms[6],
			Themes.Noms[7],
			Themes.Noms[8],
			Themes.Noms[9]
		};
		sgTheme2.Location = new Point(Zx.P(24), Zx.P(262));
		sgTheme2.Width = cw - Zx.P(48);
		sgTheme2.Etire = true;
		string value3 = Config.Lire("theme") ?? "auto";
		int num3 = Math.Max(0, Array.IndexOf(Themes.Ids, value3));
		sgTheme.Index = ((num3 < 5) ? num3 : (-1));
		sgTheme2.Index = ((num3 >= 5) ? (num3 - 5) : (-1));
		bool bascule = false;
		sgTheme.Changed += delegate
		{
			if (!bascule && sgTheme.Index >= 0)
			{
				bascule = true;
				sgTheme2.Index = -1;
				bascule = false;
			}
		};
		sgTheme2.Changed += delegate
		{
			if (!bascule && sgTheme2.Index >= 0)
			{
				bascule = true;
				sgTheme.Index = -1;
				bascule = false;
			}
		};
		cadre.Controls.Add(sgTheme);
		cadre.Controls.Add(sgTheme2);
		Etiquette(cadre, "Langue du lanceur", Zx.P(24), Zx.P(314));
		sgLangue = new Segments();
		sgLangue.Items = Lang.Noms;
		sgLangue.MinW = Zx.P(40);
		sgLangue.Location = new Point(Zx.P(24), Zx.P(340));
		sgLangue.Width = cw - Zx.P(48);
		sgLangue.Etire = true;
		sgLangue.Index = Math.Max(0, Array.IndexOf(Lang.Codes, Lang.Code));
		cadre.Controls.Add(sgLangue);
		Etiquette(cadre, "Taille de l'interface  ·  Automatique : selon ton écran (" + (int)Math.Round(Zx.Auto() * 100f) + " % ici)  ·  s'applique tout de suite", Zx.P(24), Zx.P(394));
		sgTaille = new Segments();
		sgTaille.Items = Zx.Noms;
		sgTaille.Location = new Point(Zx.P(24), Zx.P(420));
		sgTaille.Width = cw - Zx.P(48);
		sgTaille.Etire = true;
		sgTaille.MinW = Zx.P(40);
		sgTaille.Index = Math.Max(0, Array.IndexOf(Zx.Ids, Config.Lire("taille_interface") ?? "auto"));
		cadre.Controls.Add(sgTaille);
		Cadre cadre2 = Section("Mises à jour", Zx.P(40) + Zx.P(62) + 2 * Zx.P(62) + Zx.P(4));
		Bouton bouton = new Bouton("⟳  Rechercher les mises à jour");
		bouton.Plein = versionDispo != null;
		bouton.Couleur = C.Bleu;
		bouton.Size = new Size(Zx.P(270), Zx.P(38));
		bouton.Location = new Point(Zx.P(24), Zx.P(46));
		bouton.Click += delegate
		{
			Enregistrer();
			DemandeMaj = true;
			base.DialogResult = DialogResult.OK;
		};
		cadre2.Controls.Add(bouton);
		Etiquette(cadre2, (versionDispo != null) ? ("Nouvelle version du lanceur : v" + versionDispo + "   (installée : v" + Config.VERSION + ")") : ("Lanceur v" + Config.VERSION + "  ·  à jour"), Zx.P(312), Zx.P(55)).ForeColor = ((versionDispo != null) ? C.Bleu : C.Gris);
		cMajL = Opt(cadre2, 1, "Mettre à jour le lanceur automatiquement", "Installe les nouvelles versions au démarrage. Désactivé : un bandeau sur l'accueil vous prévient.", Config.Oui("maj_lanceur", defaut: false), Zx.P(40) + Zx.P(62) - Zx.P(4));
		cMajP = Opt(cadre2, 2, "Mettre à jour mes projets installés automatiquement", "Au démarrage, un projet à la fois, jamais jeu ouvert.", Config.Oui("maj_auto_projets", defaut: false), Zx.P(40) + Zx.P(62) - Zx.P(4));
		Cadre c = Section("Démarrage et jeu", Zx.P(40) + 3 * Zx.P(62) + Zx.P(6));
		cRed = Opt(c, 0, "Réduire le lanceur quand le jeu démarre", "Le lanceur se met dans la barre des tâches dès que vous cliquez sur JOUER.", Config.Oui("reduire", defaut: false));
		cFerme = Opt(c, 2, "Garder le lanceur ouvert quand on clique sur la croix", "Il reste dans la zone de notification (près de l'horloge) ; « Quitter » le ferme pour de bon.", Config.Oui("fermer_reduit", defaut: true));
		cDem = Opt(c, 1, "Démarrer le lanceur avec Windows", "Ouvre le lanceur à chaque ouverture de session. Désactivable ici à tout moment.", Demarrage.Actif);
		Cadre cadre3 = Section("Activité Discord", Zx.P(40) + Zx.P(62) + Zx.P(116));
		cDisc = Opt(cadre3, 0, "Afficher mon activité sur Discord", "Vos amis Discord voient que vous utilisez TPS Groupe et la carte à laquelle vous jouez.", Config.Oui("discord_presence", defaut: false));
		Etiquette(cadre3, "Statut : " + (Config.Oui("discord_presence", defaut: false) ? DiscordPresence.Statut : "désactivée") + "   ·   désactivé par défaut, aucune connexion à votre compte Discord", Zx.P(24), Zx.P(40) + Zx.P(62) + Zx.P(6));
		if (appFournie)
		{
			Etiquette(cadre3, "Identifiant d'application Discord : fourni par TPS Groupe ✔", Zx.P(24), Zx.P(40) + Zx.P(62) + Zx.P(40));
		}
		else
		{
			Etiquette(cadre3, "Identifiant d'application Discord (Application ID, discord.com/developers/applications)", Zx.P(24), Zx.P(40) + Zx.P(62) + Zx.P(36));
			tAppDisc = new Champ(multi: false, Zx.P(36));
			tAppDisc.Location = new Point(Zx.P(24), Zx.P(40) + Zx.P(62) + Zx.P(60));
			tAppDisc.Width = cw - Zx.P(48);
			tAppDisc.Text = Config.Lire("discord_app") ?? "";
			tAppDisc.Indice = "ex. 123456789012345678";
			cadre3.Controls.Add(tAppDisc);
		}
		List<Projet> list = Fenetre.ProjetsConnus ?? new List<Projet>();
		int num4 = Notifs.Types.Length;
		int num5 = 40 + (num4 + 1) * 62 + 14;
		Cadre cadre4 = Section("Notifications", num5 + Zx.P(30) + Math.Max(1, list.Count) * Zx.P(62) + Zx.P(12));
		for (int i = 0; i < num4; i++)
		{
			cNotif.Add(Opt(cadre4, i, Notifs.TypesNoms[i], Notifs.TypesAide[i], Notifs.TypeActif(Notifs.Types[i])));
		}
		cNotifWin = Opt(cadre4, num4, "Notifications Windows", "Une bulle Windows prévient même quand le lanceur est réduit.", Config.Oui("notif_windows", defaut: true));
		Etiquette(cadre4, "MODS SUIVIS  ·  par défaut : les mods installés", Zx.P(24), num5);
		string omsi = Omsi.DetecteOmsi();
		if (list.Count == 0)
		{
			Etiquette(cadre4, "Le catalogue n'est pas encore chargé.", Zx.P(24), num5 + Zx.P(34));
		}
		for (int j = 0; j < list.Count; j++)
		{
			Option value4 = new Option(list[j].Nom, "Mises à jour et sortie de ce contenu", Notifs.Suivi(list[j], omsi))
			{
				Location = new Point(Zx.P(8), num5 + Zx.P(26) + j * Zx.P(62)),
				Width = cw - Zx.P(16)
			};
			cadre4.Controls.Add(value4);
			cSuivis[list[j].Id] = value4;
		}
		Cadre cadre5 = Section("Musique", Zx.P(382));
		Etiquette(cadre5, "Lecteur affiché sur l'accueil (petite pilule sous « Informations »)", Zx.P(24), Zx.P(46));
		sgMus = new Segments();
		sgMus.Items = new string[4] { "Automatique", "Spotify", "Apple Music", "Aucun" };
		sgMus.Location = new Point(Zx.P(24), Zx.P(70));
		sgMus.Width = cw - Zx.P(48);
		sgMus.Etire = true;
		string mode = Musique.Mode;
		sgMus.Index = mode switch
		{
			"aucun" => 3, 
			"apple" => 2, 
			"spotify" => 1, 
			_ => 0, 
		};
		cadre5.Controls.Add(sgMus);
		Label lMus = Etiquette(cadre5, "", Zx.P(24), Zx.P(120));
		lMus.AutoSize = false;
		lMus.Size = new Size(cw - Zx.P(48), Zx.P(40));
		Action majMus = delegate
		{
			bool flag = Musique.Installe("spotify");
			bool flag2 = Musique.Installe("apple");
			lMus.Text = "Spotify : " + (flag ? "installé ✔" : "non trouvé sur ce PC") + "     ·     Apple Music : " + (flag2 ? "installé ✔" : "non trouvé sur ce PC");
			lMus.ForeColor = ((flag || flag2) ? C.Vert : C.Gris);
		};
		majMus();
		Etiquette(cadre5, "Connecte ton compte dans l'application (une seule fois) : le lanceur pilote ensuite la lecture en cours, sans jamais voir ton mot de passe.", Zx.P(24), Zx.P(166)).MaximumSize = new Size(cw - Zx.P(48), 0);
		Bouton bouton2 = new Bouton("Connecter Spotify");
		bouton2.Couleur = Color.FromArgb(30, 185, 84);
		bouton2.Size = new Size(Zx.P(210), Zx.P(40));
		bouton2.Location = new Point(Zx.P(24), Zx.P(214));
		bouton2.Click += delegate
		{
			using (FenConnexionMusique f2 = new FenConnexionMusique("spotify"))
			{
				if (f2.Afficher(this) == DialogResult.OK)
				{
					sgMus.Index = 1;
				}
			}
			majMus();
		};
		cadre5.Controls.Add(bouton2);
		Bouton bouton3 = new Bouton("Connecter Apple Music");
		bouton3.Couleur = Color.FromArgb(250, 45, 85);
		bouton3.Size = new Size(Zx.P(230), Zx.P(40));
		bouton3.Location = new Point(Zx.P(246), Zx.P(214));
		bouton3.Click += delegate
		{
			using (FenConnexionMusique f = new FenConnexionMusique("apple"))
			{
				if (f.Afficher(this) == DialogResult.OK)
				{
					sgMus.Index = 2;
				}
			}
			majMus();
		};
		cadre5.Controls.Add(bouton3);
		Etiquette(cadre5, "Chaque bouton ouvre une page qui t'accompagne : connexion au compte, application ou lecteur web, premier titre.", Zx.P(24), Zx.P(270));
		Etiquette(cadre5, "Dans la pilule de l'accueil, le bouton à gauche ramène ton lecteur (application ou onglet déjà ouvert) pour changer de playlist.", Zx.P(24), Zx.P(306)).MaximumSize = new Size(cw - Zx.P(48), 0);
		Cadre c2 = Section("Données et dépannage", Zx.P(40) + 4 * Zx.P(52) + Zx.P(10));
		bCache = Rangee(c2, 0, "Vider le cache", danger: false, delegate
		{
			ViderCache();
		}, "");
		lCache = LastDetail;
		MajCache();
		Rangee(c2, 1, "Dossier de données", danger: false, delegate
		{
			try
			{
				Process.Start("explorer.exe", "\"" + Config.Dossier + "\"");
			}
			catch
			{
			}
		}, "Réglages, profils et cache du lanceur");
		Rangee(c2, 2, "Journal d'erreurs", danger: false, delegate
		{
			string text2 = Path.Combine(Config.Dossier, "erreurs.log");
			if (File.Exists(text2))
			{
				try
				{
					Process.Start("notepad.exe", "\"" + text2 + "\"");
					return;
				}
				catch
				{
					return;
				}
			}
			MessageBox.Show(this, "Aucune erreur enregistrée : tout va bien.", Text);
		}, "Utile pour signaler un problème");
		Rangee(c2, 3, "Réinitialiser les réglages", danger: true, delegate
		{
			if (MessageBox.Show(this, "Remettre tous les réglages du lanceur à zéro ?\n\nLes dossiers du jeu, les profils et les projets installés sont conservés.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				Config.Reinitialiser("omsi", "openomsi");
				Demarrage.Definir(oui: false);
				Reinitialise = true;
				base.DialogResult = DialogResult.OK;
			}
		}, "Retour aux valeurs d'origine");
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Label value5 = new Label
		{
			Text = "TPS Groupe Lanceur v" + Config.VERSION,
			Font = new Font("Segoe UI", Zx.P(9f)),
			ForeColor = C.Gris,
			BackColor = C.Cote,
			Location = new Point(X0 + Zx.P(28) + Zx.P(120) + Zx.P(18), Zx.P(25)),
			AutoSize = true
		};
		panel.Controls.Add(value5);
		Bouton bouton4 = new Bouton("Annuler");
		bouton4.Plein = false;
		bouton4.Size = new Size(Zx.P(120), Zx.P(38));
		bouton4.Location = new Point(X0 + Zx.P(28), Zx.P(16));
		bouton4.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		Bouton bouton5 = new Bouton("Enregistrer");
		bouton5.Size = new Size(Zx.P(140), Zx.P(38));
		bouton5.Location = new Point(X0 + CW - Zx.P(28) - Zx.P(140), Zx.P(16));
		bouton5.Click += delegate
		{
			Enregistrer();
			base.DialogResult = DialogResult.OK;
		};
		panel.Controls.Add(bouton4);
		panel.Controls.Add(bouton5);
		string[] glyphes = new string[7] { "\ue7f4", "\ue895", "\ue768", "\ue8f2", "\uea8f", "\uec4f", "\ue8b7" };
		string[] array = new string[sections.Count];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = sections[k].Titre;
		}
		nav.Items = array;
		nav.Glyphes = glyphes;
		nav.Changed += delegate
		{
			Montrer(nav.Index);
		};
		Montrer(0);
		if (SectionInitiale > 0 && SectionInitiale < sections.Count)
		{
			nav.Index = SectionInitiale;
		}
		SectionInitiale = 0;
		sgTaille.Changed += delegate
		{
			if (Zx.Ids[Math.Max(0, sgTaille.Index)] != (Config.Lire("taille_interface") ?? "auto"))
			{
				Enregistrer();
				Relancer = true;
				SectionCourante = nav.Index;
				base.DialogResult = DialogResult.OK;
			}
		};
		sgLangue.Changed += delegate
		{
			if (Lang.Codes[Math.Max(0, sgLangue.Index)] != Lang.Code)
			{
				Enregistrer();
				Relancer = true;
				SectionCourante = nav.Index;
				base.DialogResult = DialogResult.OK;
			}
		};
	}

	private void Montrer(int k)
	{
		for (int i = 0; i < sections.Count; i++)
		{
			sections[i].Visible = i == k;
		}
		contenu.AutoScrollPosition = new Point(0, 0);
	}

	private Cadre Section(string titre, int h)
	{
		Cadre cadre = new Cadre();
		cadre.Titre = titre;
		cadre.Location = new Point(0, 0);
		cadre.Size = new Size(cw, h);
		cadre.Visible = false;
		contenu.Controls.Add(cadre);
		sections.Add(cadre);
		return cadre;
	}

	private Label Etiquette(Control parent, string texte, int x, int yy)
	{
		Label label = new Label();
		label.Text = texte;
		label.Font = new Font("Segoe UI", Zx.P(9.5f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(x, yy);
		label.AutoSize = true;
		parent.Controls.Add(label);
		return label;
	}

	private Option Opt(Cadre c, int i, string titre, string aide, bool val)
	{
		return Opt(c, i, titre, aide, val, Zx.P(40));
	}

	private Option Opt(Cadre c, int i, string titre, string aide, bool val, int top)
	{
		Option option = new Option(titre, aide, val);
		option.Location = new Point(Zx.P(8), top + i * Zx.P(62) - ((top > Zx.P(40)) ? Zx.P(62) : 0));
		option.Width = cw - Zx.P(16);
		c.Controls.Add(option);
		return option;
	}

	private Bouton Rangee(Cadre c, int i, string texte, bool danger, EventHandler h, string detail)
	{
		Bouton bouton = new Bouton(texte);
		bouton.Plein = false;
		bouton.Danger = danger;
		bouton.Size = new Size(Zx.P(220), Zx.P(38));
		bouton.Location = new Point(Zx.P(24), Zx.P(46) + i * Zx.P(52));
		bouton.Click += h;
		c.Controls.Add(bouton);
		Label label = new Label();
		label.Text = detail;
		label.Font = new Font("Segoe UI", Zx.P(8.5f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(Zx.P(262), Zx.P(46) + i * Zx.P(52) + ((!detail.Contains("\n")) ? Zx.P(9) : 0));
		label.Size = new Size(cw - Zx.P(262) - Zx.P(20), Zx.P(40));
		c.Controls.Add(label);
		LastDetail = label;
		return bouton;
	}

	private void Enregistrer()
	{
		Config.Ecrire("maj_lanceur", cMajL.Checked ? "1" : "0");
		if (sgMus != null)
		{
			Config.Ecrire("musique", (new string[4] { "", "spotify", "apple", "aucun" })[Math.Max(0, sgMus.Index)]);
		}
		Config.Ecrire("maj_auto_projets", cMajP.Checked ? "1" : "0");
		Config.Ecrire("reduire", cRed.Checked ? "1" : "0");
		Config.Ecrire("fermer_reduit", cFerme.Checked ? "1" : "0");
		Config.Ecrire("sons", cSons.Checked ? "1" : "0");
		for (int i = 0; i < cNotif.Count; i++)
		{
			Config.Ecrire("notif_" + Notifs.Types[i], cNotif[i].Checked ? "1" : "0");
		}
		Config.Ecrire("notif_windows", cNotifWin.Checked ? "1" : "0");
		foreach (KeyValuePair<string, Option> cSuivi in cSuivis)
		{
			Notifs.Suivre(cSuivi.Key, cSuivi.Value.Checked);
		}
		Config.Ecrire("discord_presence", cDisc.Checked ? "1" : "0");
		if (tAppDisc != null)
		{
			string text = tAppDisc.Text.Trim();
			if (text.Length == 0 || Regex.IsMatch(text, "^\\d{15,22}$"))
			{
				Config.Ecrire("discord_app", text);
			}
		}
		if (Lang.Codes[Math.Max(0, sgLangue.Index)] != Lang.Code)
		{
			Lang.Definir(Lang.Codes[Math.Max(0, sgLangue.Index)]);
			ChangeLangue = true;
		}
		string text2 = Zx.Ids[Math.Max(0, sgTaille.Index)];
		if (text2 != (Config.Lire("taille_interface") ?? "auto"))
		{
			Config.Ecrire("taille_interface", text2);
			Relancer = true;
		}
		Config.Ecrire("theme", Themes.Ids[(sgTheme.Index >= 0) ? sgTheme.Index : ((sgTheme2.Index >= 0) ? (5 + sgTheme2.Index) : 0)]);
		Config.Ecrire("page_demarrage", (sgPage.Index == 1) ? "biblio" : ((sgPage.Index == 2) ? "recent" : "accueil"));
		if (cDem.Checked != Demarrage.Actif)
		{
			Demarrage.Definir(cDem.Checked);
		}
	}

	private static bool Effacable(string f)
	{
		string text = Path.GetFileName(f).ToLowerInvariant();
		if (!(text == "catalogue.json") && (!text.StartsWith("image_") || !text.EndsWith(".jpg")))
		{
			return text.EndsWith(".zip");
		}
		return true;
	}

	private void MajCache()
	{
		long num = 0L;
		try
		{
			string[] files = Directory.GetFiles(Config.Cache);
			foreach (string text in files)
			{
				if (Effacable(text))
				{
					num += new FileInfo(text).Length;
				}
			}
		}
		catch
		{
		}
		lCache.Text = "Catalogue et images : " + ((double)num / 1024.0 / 1024.0).ToString("0.0") + " Mo";
	}

	private void ViderCache()
	{
		try
		{
			string[] files = Directory.GetFiles(Config.Cache);
			foreach (string text in files)
			{
				if (Effacable(text))
				{
					File.Delete(text);
				}
			}
			CacheVide = true;
			bCache.Text = "Cache vidé ✔";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Impossible de vider le cache :\n" + ex.Message, Text);
		}
		MajCache();
	}
}
