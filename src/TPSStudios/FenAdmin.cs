using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenAdmin : Form, IPage
{
	private Dictionary<string, object> cat;

	private string sha;

	private List<Dictionary<string, object>> pages = new List<Dictionary<string, object>>();

	private List<Dictionary<string, object>> visibles = new List<Dictionary<string, object>>();

	private HashSet<Dictionary<string, object>> modifiees = new HashSet<Dictionary<string, object>>();

	private HashSet<Dictionary<string, object>> nouvelles = new HashSet<Dictionary<string, object>>();

	private List<string> supprimees = new List<string>();

	private Dictionary<string, string> imagesEnAttente = new Dictionary<string, string>();

	private ListBox liste;

	private Panel edition;

	private Panel entete;

	private Panel pied;

	private Label lEtat;

	private Label lInfo;

	private Label lAideStatut;

	private Dictionary<string, object> cur;

	private bool charge;

	private bool enLigne;

	private Champ tFiltre;

	private Champ tId;

	private Champ tNom;

	private Champ tSous;

	private Champ tDesc;

	private Champ tNotes;

	private Champ tConsigne;

	private Champ tImage;

	private Champ tOrdre;

	private Champ tDate;

	private Champ tHeure;

	private Champ tPubDate;

	private Champ tPubHeure;

	private Champ tPoints;

	private Champ tEditeur;

	private Champ tPrereq;

	private Champ tVideo;

	private Champ tLiens;

	private Champ tAncien;

	private Champ tLies;

	private Champ tTags;

	private Segments sgRuban;

	private Option cCom;

	private Option cNouv;

	private Cadre cdOpt;

	private Dictionary<string, string> videosEnAttente = new Dictionary<string, string>();

	private Label lPubEtiq;

	private Segments sgCat;

	private Segments sgJeuPage;

	private string[] idsJeux = new string[1] { "omsi2" };

	private string[] nomsJeux = new string[1] { "OMSI 2" };

	private Segments sgStatut;

	private Segments sgVip;

	private Option cUne;

	private Option cDate;

	private PictureBox apercu;

	private Bouton bSauver;

	private Bouton bPaquet;

	private Cadre cdIdentite;

	private Cadre cdClass;

	private Cadre cdPub;

	private Cadre cdVisuel;

	private Cadre cdContenu;

	private Cadre cdTarif;

	private Segments sgPrix;

	private Champ tPrix;

	private Champ tLien;

	private Champ tCode;

	private Label lCodeEtat;

	private Label lDateEtiq;

	private List<Control> tarifPaye = new List<Control>();

	private static readonly string[] statutsId = new string[4] { "publie", "bientot", "programme", "masque" };

	private static readonly string[] statutsNom = new string[4] { "Disponible", "Prochainement", "À date précise", "Masqué" };

	private static readonly string[] statutsAide = new string[4] { "Visible et installable par tous les joueurs dès que le paquet existe.", "Annoncé avec son image et sa description, mais pas encore installable. Une date prévue peut être affichée (facultatif).", "Annoncé avec un compte à rebours, puis disponible automatiquement à la date et l'heure choisies (si le paquet existe).", "Invisible pour les joueurs : seuls les administrateurs voient la page." };

	private Cadre cdDlc;

	private ListBox lDlc;

	private Champ tDlcId;

	private Champ tDlcNom;

	private Segments sgDlc;

	private Label lDlcEtat;

	private List<string[]> dlcEd = new List<string[]>();

	private Dictionary<Dictionary<string, object>, List<ItemGal>> galeries = new Dictionary<Dictionary<string, object>, List<ItemGal>>();

	private HashSet<Dictionary<string, object>> galModif = new HashSet<Dictionary<string, object>>();

	private ZoneDessin zoneGal;

	private Cadre cdGal;

	private int galSel = -1;

	private bool dejaRedemande;

	private Segments ongletsAdm;

	private Panel zoneOnglet;

	private static readonly string[] OngletsNoms = new string[6] { "Jeux", "Mods", "Accueil", "Boutique · VIP", "Communauté", "Maintenance" };

	public FenAdmin()
	{
		Text = "Administration · pages du catalogue";
		base.ClientSize = Integre.Hote;
		base.StartPosition = FormStartPosition.Manual;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.FormBorderStyle = FormBorderStyle.None;
		try
		{
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		catch
		{
		}
		Theme.Fenetre(this);
		base.FormClosing += delegate(object o, FormClosingEventArgs e)
		{
			if (modifiees.Count + supprimees.Count > 0 && MessageBox.Show(this, "Des modifications ne sont pas encore publiées. Fermer sans les publier ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		};
		entete = new Panel();
		entete.Dock = DockStyle.Top;
		entete.Height = Zx.P(132);
		entete.BackColor = C.Cote;
		base.Controls.Add(entete);
		entete.Controls.Add(Integre.Retour(delegate
		{
			Close();
		}, Zx.P(20), Zx.P(17)));
		entete.Paint += delegate(object o, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Lang.DrawText(graphics, "Administration", new Font("Segoe UI Semibold", Zx.P(18f)), new Point(Zx.P(140), Zx.P(10)), C.Texte);
			Lang.DrawText(graphics, "Catalogue TPS Groupe  ·  github.com/Dreams9972/TPS-Studios", new Font("Segoe UI", Zx.P(9.5f)), new Point(Zx.P(142), Zx.P(49)), C.Gris);
			int num5 = entete.Width - Zx.P(24);
			string[] array = new string[2]
			{
				enLigne ? "●  Connecté à GitHub" : "●  Hors ligne",
				(modifiees.Count + supprimees.Count > 0) ? ("✎  " + (modifiees.Count + supprimees.Count) + " modification(s) non publiée(s)") : "✔  Tout est publié"
			};
			Color[] array2 = new Color[2]
			{
				enLigne ? C.Vert : C.Rouge,
				(modifiees.Count + supprimees.Count > 0) ? C.Orange : C.Gris
			};
			for (int i = 0; i < array.Length; i++)
			{
				Size size = Lang.MeasureText(array[i], F.Etat);
				Rectangle r = new Rectangle(num5 - size.Width - Zx.P(26), Zx.P(28), size.Width + Zx.P(26), Zx.P(28));
				using (GraphicsPath path = Bouton.Rond(r, Zx.P(13)))
				{
					using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, array2[i])))
					{
						graphics.FillPath(brush, path);
					}
					using Pen pen = new Pen(Color.FromArgb(120, array2[i]));
					graphics.DrawPath(pen, path);
				}
				Lang.DrawText(graphics, array[i], F.Etat, r, array2[i], TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				num5 = r.X - Zx.P(10);
			}
		};
		tFiltre = new Champ(multi: false, Zx.P(38));
		tFiltre.Location = new Point(Zx.P(24), Zx.P(150));
		tFiltre.Size = new Size(Zx.P(320), Zx.P(38));
		tFiltre.Indice = "Filtrer les pages…";
		tFiltre.TextChanged += delegate
		{
			RemplirListe(cur);
		};
		base.Controls.Add(tFiltre);
		liste = new ListBox();
		liste.Location = new Point(Zx.P(24), Zx.P(198));
		liste.Size = new Size(Zx.P(320), base.ClientSize.Height - Zx.P(198) - Zx.P(172));
		liste.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
		liste.DrawMode = DrawMode.OwnerDrawFixed;
		liste.ItemHeight = Zx.P(56);
		liste.BorderStyle = BorderStyle.None;
		liste.BackColor = C.Fond;
		liste.ForeColor = C.Texte;
		liste.IntegralHeight = false;
		liste.DrawItem += DessinerItem;
		liste.SelectedIndexChanged += delegate
		{
			if (!charge)
			{
				Choisir(liste.SelectedIndex);
			}
		};
		base.Controls.Add(liste);
		Theme.Sombre(liste);
		Bt("＋  Nouvelle page", Zx.P(24), base.ClientSize.Height - Zx.P(162), Zx.P(320), AnchorStyles.Bottom | AnchorStyles.Left, plein: true, delegate
		{
			Nouvelle();
		});
		Bt("⟳  Recharger depuis GitHub", Zx.P(24), base.ClientSize.Height - Zx.P(116), Zx.P(320), AnchorStyles.Bottom | AnchorStyles.Left, plein: false, delegate
		{
			Charger();
		});
		pied = new Panel();
		pied.Dock = DockStyle.Bottom;
		pied.Height = Zx.P(70);
		pied.BackColor = C.Cote;
		base.Controls.Add(pied);
		bSauver = new Bouton("Enregistrer et publier");
		bSauver.Size = new Size(Zx.P(210), Zx.P(40));
		bSauver.Location = new Point(base.ClientSize.Width - Zx.P(24) - Zx.P(210), Zx.P(15));
		bSauver.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		bSauver.Click += delegate
		{
			Publier();
		};
		pied.Controls.Add(bSauver);
		Bouton bouton = new Bouton("Supprimer la page");
		bouton.Plein = false;
		bouton.Danger = true;
		bouton.Size = new Size(Zx.P(160), Zx.P(40));
		bouton.Location = new Point(bSauver.Left - Zx.P(10) - Zx.P(160), Zx.P(15));
		bouton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		bouton.Click += delegate
		{
			Supprimer();
		};
		pied.Controls.Add(bouton);
		Bouton bouton2 = new Bouton("Dépôt GitHub");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(140), Zx.P(40));
		bouton2.Location = new Point(bouton.Left - Zx.P(10) - Zx.P(180) - Zx.P(10) - Zx.P(140), Zx.P(15));
		bouton2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		bouton2.Click += delegate
		{
			try
			{
				Process.Start("https://github.com/Dreams9972/TPS-Studios");
			}
			catch
			{
			}
		};
		pied.Controls.Add(bouton2);
		Bouton bouton3 = new Bouton("Activité Discord…");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(180), Zx.P(40));
		bouton3.Location = new Point(bouton.Left - Zx.P(10) - Zx.P(180), Zx.P(15));
		bouton3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		bouton3.Click += delegate
		{
			Communaute();
		};
		pied.Controls.Add(bouton3);
		bouton3.Visible = false;
		bPaquet = new Bouton("Publier le paquet…");
		bPaquet.Plein = false;
		bPaquet.Size = new Size(Zx.P(180), Zx.P(40));
		bPaquet.Location = new Point(bouton.Left - Zx.P(10) - Zx.P(180), Zx.P(15));
		bPaquet.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		bPaquet.Enabled = false;
		bPaquet.Click += delegate
		{
			PublierPaquet();
		};
		pied.Controls.Add(bPaquet);
		ongletsAdm = new Segments();
		ongletsAdm.Items = OngletsNoms;
		ongletsAdm.Etire = true;
		ongletsAdm.Location = new Point(Zx.P(24), Zx.P(82));
		ongletsAdm.Size = new Size(Math.Min(Zx.P(900), base.ClientSize.Width - Zx.P(48)), Zx.P(38));
		ongletsAdm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		ongletsAdm.Changed += delegate
		{
			ChangerOnglet(ongletsAdm.Index);
		};
		entete.Controls.Add(ongletsAdm);
		base.Shown += delegate
		{
			ChangerOnglet(0);
		};
		zoneOnglet = new Panel();
		zoneOnglet.BackColor = C.Fond;
		zoneOnglet.Visible = false;
		zoneOnglet.AutoScroll = true;
		zoneOnglet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		zoneOnglet.Bounds = new Rectangle(0, Zx.P(132), base.ClientSize.Width, base.ClientSize.Height - Zx.P(132));
		base.Controls.Add(zoneOnglet);
		zoneOnglet.BringToFront();
		zoneOnglet.Resize += delegate
		{
			foreach (Control control in zoneOnglet.Controls)
			{
				if (control is Form)
				{
					control.Location = new Point(Math.Max(0, (zoneOnglet.ClientSize.Width - control.Width) / 2) + zoneOnglet.AutoScrollPosition.X, Zx.P(16) + zoneOnglet.AutoScrollPosition.Y);
				}
			}
		};
		Bouton bouton4 = new Bouton("Fermer");
		bouton4.Plein = false;
		bouton4.Size = new Size(Zx.P(120), Zx.P(40));
		bouton4.Anchor = AnchorStyles.Top | AnchorStyles.Left;
		bouton4.Location = new Point(Zx.P(24), Zx.P(15));
		bouton4.Click += delegate
		{
			Close();
		};
		pied.Controls.Add(bouton4);
		lEtat = new Label();
		lEtat.Font = new Font("Segoe UI", Zx.P(9f));
		lEtat.ForeColor = C.Gris;
		lEtat.BackColor = C.Cote;
		lEtat.Location = new Point(Zx.P(156), Zx.P(15));
		lEtat.Size = new Size(Math.Max(Zx.P(120), base.ClientSize.Width - Zx.P(24) - Zx.P(210) - Zx.P(10) - Zx.P(160) - Zx.P(10) - Zx.P(180) - Zx.P(10) - Zx.P(140) - Zx.P(12) - Zx.P(156)), Zx.P(40));
		lEtat.AutoEllipsis = true;
		lEtat.TextAlign = ContentAlignment.MiddleLeft;
		lEtat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		pied.Controls.Add(lEtat);
		edition = new Panel();
		edition.Location = new Point(Zx.P(368), Zx.P(146));
		edition.Size = new Size(base.ClientSize.Width - Zx.P(368) - Zx.P(16), base.ClientSize.Height - Zx.P(146) - Zx.P(70));
		edition.AutoScroll = true;
		edition.BackColor = C.Fond;
		edition.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		base.Controls.Add(edition);
		Theme.Sombre(edition);
		int largeurEd = Math.Min(edition.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Zx.P(8), Zx.P(2400));
		colonnesEd = ((largeurEd >= Zx.P(1000)) ? 2 : 1);
		int num = ((colonnesEd == 2) ? ((largeurEd - Zx.P(16)) / 2) : largeurEd);
		largeurColEd = num;
		int num2 = 0;
		sgSousOnglet = new Segments();
		sgSousOnglet.Items = new string[3] { "Général", "Options et tarif", "Médias et contenu" };
		sgSousOnglet.Etire = true;
		sgSousOnglet.Location = new Point(Zx.P(4), 0);
		sgSousOnglet.Width = Math.Min(largeurEd, Zx.P(620));
		sgSousOnglet.Changed += delegate
		{
			sousOnglet = Math.Max(0, sgSousOnglet.Index);
			OrganiserBlocs();
		};
		edition.Controls.Add(sgSousOnglet);
		lInfo = new Label();
		lInfo.Font = new Font("Segoe UI", Zx.P(9f));
		lInfo.ForeColor = C.Gris;
		lInfo.BackColor = C.Fond;
		lInfo.Location = new Point(sgSousOnglet.Right + Zx.P(20), Zx.P(9));
		lInfo.Size = new Size(Math.Max(Zx.P(200), largeurEd - sgSousOnglet.Width - Zx.P(24)), Zx.P(20));
		lInfo.AutoEllipsis = true;
		edition.Controls.Add(lInfo);
		num2 += Zx.P(28);
		cdIdentite = Bloc("Identité", ref num2, num, Zx.P(44) + 3 * Zx.P(70) + Zx.P(6));
		tId = ChampDans(cdIdentite, "Identifiant (adresse interne, modifiable seulement à la création)", 0, Zx.P(300), multi: false, Zx.P(36));
		tNom = ChampDans(cdIdentite, "Nom affiché", 1, num - Zx.P(40), multi: false, Zx.P(36));
		tSous = ChampDans(cdIdentite, "Sous-titre (une ligne)", 2, num - Zx.P(40), multi: false, Zx.P(36));
		cdClass = Bloc("Classement", ref num2, num, Zx.P(44) + Zx.P(70) + Zx.P(66) + Zx.P(70) + Zx.P(6));
		Etiq(cdClass, "Jeu", Zx.P(24), Zx.P(44));
		sgJeuPage = new Segments();
		sgJeuPage.Items = nomsJeux;
		sgJeuPage.Etire = true;
		sgJeuPage.Location = new Point(Zx.P(24), Zx.P(66));
		sgJeuPage.Width = num - Zx.P(48);
		sgJeuPage.Changed += delegate
		{
			if (!charge && cur != null)
			{
				MajCatsJeu(idsJeux[Math.Max(0, Math.Min(idsJeux.Length - 1, sgJeuPage.Index))], "");
				Appliquer();
			}
		};
		cdClass.Controls.Add(sgJeuPage);
		Etiq(cdClass, "Catégorie (propre au jeu : se règle dans la fenêtre Jeux)", Zx.P(24), Zx.P(114));
		sgCat = new Segments();
		sgCat.Items = Categories.Noms;
		sgCat.Etire = true;
		sgCat.Location = new Point(Zx.P(24), Zx.P(136));
		sgCat.Width = num - Zx.P(48);
		sgCat.Changed += delegate
		{
			Appliquer();
		};
		cdClass.Controls.Add(sgCat);
		cUne = new Option("À la une", "Affichée en premier dans la bibliothèque.", val: false);
		cUne.Location = new Point(Zx.P(14), Zx.P(188));
		cUne.Width = (num - Zx.P(48)) / 2 + 40;
		cUne.Changed += delegate
		{
			Appliquer();
		};
		cdClass.Controls.Add(cUne);
		Etiq(cdClass, "Ordre (0 = automatique)", num - Zx.P(24) - Zx.P(150), Zx.P(194));
		tOrdre = new Champ(multi: false, Zx.P(36));
		tOrdre.Location = new Point(num - Zx.P(24) - Zx.P(150), Zx.P(216));
		tOrdre.Width = Zx.P(150);
		tOrdre.TextChanged += delegate
		{
			Appliquer();
		};
		cdClass.Controls.Add(tOrdre);
		cdPub = Bloc("Disponibilité", ref num2, num, Zx.P(360));
		Etiq(cdPub, "Statut de la page", Zx.P(24), Zx.P(44));
		sgStatut = new Segments();
		sgStatut.Items = statutsNom;
		sgStatut.Etire = true;
		sgStatut.Location = new Point(Zx.P(24), Zx.P(66));
		sgStatut.Width = num - Zx.P(48);
		sgStatut.Changed += delegate
		{
			MajStatut();
			Appliquer();
		};
		cdPub.Controls.Add(sgStatut);
		lAideStatut = new Label();
		lAideStatut.BackColor = C.Carte;
		lAideStatut.ForeColor = C.Gris;
		lAideStatut.Font = new Font("Segoe UI", Zx.P(9f));
		lAideStatut.Location = new Point(Zx.P(26), Zx.P(108));
		lAideStatut.Size = new Size(num - Zx.P(52), Zx.P(34));
		cdPub.Controls.Add(lAideStatut);
		cDate = new Option("", "", val: false);
		lDateEtiq = new Label();
		lDateEtiq.Font = new Font("Segoe UI", Zx.P(9f));
		lDateEtiq.ForeColor = C.Gris;
		lDateEtiq.BackColor = C.Carte;
		lDateEtiq.Location = new Point(Zx.P(24), Zx.P(150));
		lDateEtiq.AutoSize = true;
		cdPub.Controls.Add(lDateEtiq);
		tDate = new Champ(multi: false, Zx.P(36));
		tDate.Location = new Point(Zx.P(24), Zx.P(174));
		tDate.Width = Zx.P(160);
		tDate.Indice = "jj/mm/aaaa";
		tDate.TextChanged += delegate
		{
			Appliquer();
		};
		cdPub.Controls.Add(tDate);
		tHeure = new Champ(multi: false, Zx.P(36));
		tHeure.Location = new Point(Zx.P(196), Zx.P(174));
		tHeure.Width = Zx.P(110);
		tHeure.Indice = "hh:mm";
		tHeure.TextChanged += delegate
		{
			Appliquer();
		};
		cdPub.Controls.Add(tHeure);
		Etiq(cdPub, "Accès VIP", Zx.P(24), Zx.P(228));
		sgVip = new Segments();
		sgVip.Items = new string[3] { "Pour tous", "Accès anticipé VIP", "Test VIP (testeurs)" };
		sgVip.Etire = true;
		sgVip.Location = new Point(Zx.P(24), Zx.P(250));
		sgVip.Width = num - Zx.P(48);
		sgVip.Changed += delegate
		{
			MajVipAdm();
			Appliquer();
		};
		cdPub.Controls.Add(sgVip);
		lPubEtiq = new Label();
		lPubEtiq.Text = "Ouvert à tous le (les VIP y ont accès avant)";
		lPubEtiq.Font = new Font("Segoe UI", Zx.P(9f));
		lPubEtiq.ForeColor = C.Gris;
		lPubEtiq.BackColor = C.Carte;
		lPubEtiq.Location = new Point(Zx.P(24), Zx.P(296));
		lPubEtiq.AutoSize = true;
		cdPub.Controls.Add(lPubEtiq);
		tPubDate = new Champ(multi: false, Zx.P(36));
		tPubDate.Location = new Point(Zx.P(24), Zx.P(316));
		tPubDate.Width = Zx.P(160);
		tPubDate.Indice = "jj/mm/aaaa";
		tPubDate.TextChanged += delegate
		{
			Appliquer();
		};
		cdPub.Controls.Add(tPubDate);
		tPubHeure = new Champ(multi: false, Zx.P(36));
		tPubHeure.Location = new Point(Zx.P(196), Zx.P(316));
		tPubHeure.Width = Zx.P(110);
		tPubHeure.Indice = "hh:mm";
		tPubHeure.TextChanged += delegate
		{
			Appliquer();
		};
		cdPub.Controls.Add(tPubHeure);
		cdOpt = Bloc("Options de la page", ref num2, num, Zx.P(600));
		int num3 = (num - Zx.P(72)) / 2;
		int x = Zx.P(24) + num3 + Zx.P(24);
		Etiq(cdOpt, "Éditeur / auteurs (vide = TPS Groupe)", Zx.P(24), Zx.P(44));
		tEditeur = new Champ(multi: false, Zx.P(36));
		tEditeur.Location = new Point(Zx.P(24), Zx.P(66));
		tEditeur.Width = num3;
		tEditeur.MaxLength = 60;
		tEditeur.TextChanged += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(tEditeur);
		Etiq(cdOpt, "Prérequis (ex. OMSI 2 + DLC Hambourg)", x, Zx.P(44));
		tPrereq = new Champ(multi: false, Zx.P(36));
		tPrereq.Location = new Point(x, Zx.P(66));
		tPrereq.Width = num3;
		tPrereq.MaxLength = 90;
		tPrereq.TextChanged += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(tPrereq);
		Etiq(cdOpt, "Ruban sur la tuile de la bibliothèque", Zx.P(24), Zx.P(116));
		sgRuban = new Segments();
		sgRuban.Items = Projet.RubansNom;
		sgRuban.Etire = true;
		sgRuban.Location = new Point(Zx.P(24), Zx.P(138));
		sgRuban.Width = num - Zx.P(48);
		sgRuban.Changed += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(sgRuban);
		Etiq(cdOpt, "Bande-annonce vidéo : MP4, 3 min maximum, de 1080p à 4K (envoyée sur GitHub à la publication)", Zx.P(24), Zx.P(188));
		tVideo = new Champ(multi: false, Zx.P(36));
		tVideo.Location = new Point(Zx.P(24), Zx.P(210));
		tVideo.Width = num - Zx.P(48) - Zx.P(190) - Zx.P(10) - Zx.P(110) - Zx.P(10);
		tVideo.ReadOnly = true;
		cdOpt.Controls.Add(tVideo);
		Bouton bouton5 = new Bouton("Choisir une vidéo…");
		bouton5.Plein = false;
		bouton5.Size = new Size(Zx.P(190), Zx.P(38));
		bouton5.Location = new Point(num - Zx.P(24) - Zx.P(110) - Zx.P(10) - Zx.P(190), Zx.P(209));
		bouton5.Click += delegate
		{
			ChoisirVideo();
		};
		cdOpt.Controls.Add(bouton5);
		Bouton bouton6 = new Bouton("Retirer");
		bouton6.Plein = false;
		bouton6.Size = new Size(Zx.P(110), Zx.P(38));
		bouton6.Location = new Point(num - Zx.P(24) - Zx.P(110), Zx.P(209));
		bouton6.Click += delegate
		{
			RetirerVideo();
		};
		cdOpt.Controls.Add(bouton6);
		Etiq(cdOpt, "Liens utiles affichés sur la page : une ligne par lien « Libellé | https://… » (3 au maximum)", Zx.P(24), Zx.P(262));
		tLiens = new Champ(multi: true, Zx.P(84));
		tLiens.Location = new Point(Zx.P(24), Zx.P(284));
		tLiens.Width = num - Zx.P(48);
		tLiens.MaxLength = 600;
		tLiens.TextChanged += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(tLiens);
		Etiq(cdOpt, "Ancien prix barré (ex. 9,99)", Zx.P(24), Zx.P(384));
		tAncien = new Champ(multi: false, Zx.P(36));
		tAncien.Location = new Point(Zx.P(24), Zx.P(406));
		tAncien.Width = Zx.P(180);
		tAncien.Indice = "vide = aucun";
		tAncien.TextChanged += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(tAncien);
		Etiq(cdOpt, "Mods liés (compléments : ex. paris25 sur la page de la girouette Paris 25)", Zx.P(228), Zx.P(384));
		tLies = new Champ(multi: false, Zx.P(36));
		tLies.Location = new Point(Zx.P(228), Zx.P(406));
		tLies.Width = num - Zx.P(252);
		tLies.Indice = "ex. paris25, parisview";
		tLies.MaxLength = 300;
		tLies.TextChanged += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(tLies);
		Etiq(cdOpt, "Tags : mots-clés séparés par des virgules (8 au maximum) — affichés sur la page et utilisés par la recherche", Zx.P(24), Zx.P(454));
		tTags = new Champ(multi: false, Zx.P(36));
		tTags.Location = new Point(Zx.P(24), Zx.P(476));
		tTags.Width = num - Zx.P(48);
		tTags.Indice = "ex. Paris, RATP, Citaro, girouette";
		tTags.MaxLength = 200;
		tTags.TextChanged += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(tTags);
		cCom = new Option("Commentaires", "Les joueurs peuvent commenter cette page.", val: true);
		cCom.Location = new Point(Zx.P(14), Zx.P(526));
		cCom.Width = (num - Zx.P(28)) / 2 - 6;
		cCom.Changed += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(cCom);
		cNouv = new Option("Mettre en avant", "Affichée en premier dans « Nouveautés » de l'accueil.", val: false);
		cNouv.Location = new Point(Zx.P(14) + (num - Zx.P(28)) / 2 + Zx.P(6), Zx.P(526));
		cNouv.Width = (num - Zx.P(28)) / 2 - 6;
		cNouv.Changed += delegate
		{
			Appliquer();
		};
		cdOpt.Controls.Add(cNouv);
		cdDlc = Bloc("DLC Steam nécessaires", ref num2, num, Zx.P(44) + Zx.P(22) + Zx.P(120) + Zx.P(14) + Zx.P(38) + Zx.P(12) + Zx.P(38) + Zx.P(30));
		Etiq(cdDlc, "Affichés sur la page du mod ; un clic ouvre le DLC dans Steam.  ★ = obligatoire pour jouer", Zx.P(24), Zx.P(44));
		lDlc = new ListBox();
		lDlc.Location = new Point(Zx.P(24), Zx.P(66));
		lDlc.Size = new Size(num - Zx.P(48), Zx.P(120));
		lDlc.BorderStyle = BorderStyle.None;
		lDlc.BackColor = C.Fond;
		lDlc.ForeColor = C.Texte;
		lDlc.Font = new Font("Segoe UI", Zx.P(10f));
		lDlc.IntegralHeight = false;
		cdDlc.Controls.Add(lDlc);
		Theme.Sombre(lDlc);
		tDlcId = new Champ(multi: false, Zx.P(36));
		tDlcId.Location = new Point(Zx.P(24), Zx.P(200));
		tDlcId.Width = Zx.P(170);
		tDlcId.Indice = "AppID Steam";
		tDlcId.MaxLength = 10;
		cdDlc.Controls.Add(tDlcId);
		tDlcNom = new Champ(multi: false, Zx.P(36));
		tDlcNom.Location = new Point(Zx.P(206), Zx.P(200));
		tDlcNom.Width = num - Zx.P(48) - Zx.P(182);
		tDlcNom.Indice = "nom (vide = repris de Steam)";
		tDlcNom.MaxLength = 90;
		cdDlc.Controls.Add(tDlcNom);
		sgDlc = new Segments();
		sgDlc.Items = new string[2] { "★ Obligatoire", "Facultatif" };
		sgDlc.Etire = true;
		sgDlc.Location = new Point(Zx.P(24), Zx.P(250));
		sgDlc.Width = Zx.P(300);
		cdDlc.Controls.Add(sgDlc);
		Bouton bouton7 = new Bouton("Ajouter / modifier")
		{
			Plein = false,
			Size = new Size(Zx.P(170), Zx.P(38)),
			Location = new Point(num - Zx.P(24) - Zx.P(130) - Zx.P(10) - Zx.P(170), Zx.P(249))
		};
		cdDlc.Controls.Add(bouton7);
		Bouton bouton8 = new Bouton("Retirer")
		{
			Plein = false,
			Danger = true,
			Size = new Size(Zx.P(130), Zx.P(38)),
			Location = new Point(num - Zx.P(24) - Zx.P(130), Zx.P(249))
		};
		cdDlc.Controls.Add(bouton8);
		lDlcEtat = new Label();
		lDlcEtat.Font = new Font("Segoe UI", Zx.P(9f));
		lDlcEtat.ForeColor = C.Gris;
		lDlcEtat.BackColor = C.Carte;
		lDlcEtat.Location = new Point(Zx.P(24), Zx.P(294));
		lDlcEtat.Size = new Size(num - Zx.P(48), Zx.P(20));
		cdDlc.Controls.Add(lDlcEtat);
		lDlc.SelectedIndexChanged += delegate
		{
			int selectedIndex3 = lDlc.SelectedIndex;
			if (selectedIndex3 >= 0 && selectedIndex3 < dlcEd.Count)
			{
				tDlcId.Text = dlcEd[selectedIndex3][0];
				tDlcNom.Text = dlcEd[selectedIndex3][1];
				sgDlc.Index = ((dlcEd[selectedIndex3][2] == "1") ? 1 : 0);
			}
		};
		sgDlc.Changed += delegate
		{
			int selectedIndex2 = lDlc.SelectedIndex;
			if (selectedIndex2 >= 0 && selectedIndex2 < dlcEd.Count && dlcEd[selectedIndex2][0] == tDlcId.Text.Trim())
			{
				dlcEd[selectedIndex2][2] = ((sgDlc.Index == 1) ? "1" : "");
				SauverDlc(selectedIndex2);
			}
		};
		bouton8.Click += delegate
		{
			int selectedIndex = lDlc.SelectedIndex;
			if (selectedIndex < 0 || selectedIndex >= dlcEd.Count)
			{
				lDlcEtat.Text = "Choisissez un DLC dans la liste.";
			}
			else
			{
				dlcEd.RemoveAt(selectedIndex);
				SauverDlc(Math.Min(selectedIndex, dlcEd.Count - 1));
				lDlcEtat.Text = "DLC retiré (publiez pour l'appliquer).";
			}
		};
		bouton7.Click += delegate
		{
			string id = tDlcId.Text.Trim();
			string text = tDlcNom.Text.Trim();
			string fac = ((sgDlc.Index == 1) ? "1" : "");
			if (!Regex.IsMatch(id, "^\\d{3,10}$"))
			{
				lDlcEtat.ForeColor = C.Rouge;
				lDlcEtat.Text = "AppID invalide : le nombre de la page Steam du DLC (store.steampowered.com/app/…).";
			}
			else
			{
				Action<string> poser = delegate(string nom2)
				{
					int num4 = dlcEd.FindIndex((string[] e0) => e0[0] == id);
					if (num4 >= 0)
					{
						dlcEd[num4][1] = nom2;
						dlcEd[num4][2] = fac;
					}
					else
					{
						dlcEd.Add(new string[3] { id, nom2, fac });
						num4 = dlcEd.Count - 1;
					}
					SauverDlc(num4);
					lDlcEtat.ForeColor = C.Vert;
					lDlcEtat.Text = "✔  " + nom2 + ((fac == "1") ? " (facultatif)" : " (obligatoire)") + " : publiez pour l'appliquer.";
				};
				if (text.Length > 0)
				{
					poser(text);
				}
				else
				{
					lDlcEtat.ForeColor = C.Gris;
					lDlcEtat.Text = "Recherche du nom sur Steam…";
					Thread thread = new Thread((ThreadStart)delegate
					{
						string n2 = "DLC " + id;
						try
						{
							using WebClient webClient = Web.Client();
							webClient.Encoding = Encoding.UTF8;
							string input = webClient.DownloadString("https://store.steampowered.com/api/appdetails?appids=" + id + "&l=french&filters=basic");
							Match match = Regex.Match(input, "\"name\":\"((?:[^\"\\\\]|\\\\.)*)\"");
							if (match.Success)
							{
								n2 = Regex.Unescape(match.Groups[1].Value);
							}
						}
						catch
						{
						}
						try
						{
							BeginInvoke((MethodInvoker)delegate
							{
								poser(n2);
							});
						}
						catch
						{
						}
					});
					thread.IsBackground = true;
					thread.Start();
				}
			}
		};
		cdTarif = Bloc("Tarif", ref num2, num, Zx.P(420));
		Func<string, int, int, Label> func = delegate(string t0, int ex, int ey)
		{
			Label label = new Label
			{
				Text = t0,
				Font = new Font("Segoe UI", Zx.P(9f)),
				ForeColor = C.Gris,
				BackColor = C.Carte,
				Location = new Point(ex, ey),
				AutoSize = true
			};
			cdTarif.Controls.Add(label);
			return label;
		};
		func("Prix de ce mod", 24, 44);
		sgPrix = new Segments();
		sgPrix.Items = new string[2] { "Gratuit", "Payant" };
		sgPrix.Location = new Point(Zx.P(24), Zx.P(66));
		sgPrix.Width = Zx.P(320);
		sgPrix.Etire = true;
		sgPrix.Changed += delegate
		{
			MajTarif();
			Appliquer();
		};
		cdTarif.Controls.Add(sgPrix);
		tarifPaye.Add(func("Prix en euros (de 0,50 à 999 — ex. 4,99)", 24, 122));
		tPrix = new Champ(multi: false, Zx.P(36));
		tPrix.Location = new Point(Zx.P(24), Zx.P(144));
		tPrix.Width = Zx.P(180);
		tPrix.Indice = "4,99";
		tPrix.TextChanged += delegate
		{
			Appliquer();
		};
		cdTarif.Controls.Add(tPrix);
		tarifPaye.Add(tPrix);
		tarifPaye.Add(func("Page d'achat (adresse https:// de la boutique ou du lien de paiement)", 24, 194));
		tLien = new Champ(multi: false, Zx.P(36));
		tLien.Location = new Point(Zx.P(24), Zx.P(216));
		tLien.Width = num - Zx.P(48);
		tLien.Indice = "https://…";
		tLien.TextChanged += delegate
		{
			Appliquer();
		};
		cdTarif.Controls.Add(tLien);
		tarifPaye.Add(tLien);
		tarifPaye.Add(func("Code d'accès remis aux acheteurs (laisser vide pour conserver le code actuel)", 24, 266));
		tCode = new Champ(multi: false, Zx.P(36));
		tCode.Location = new Point(Zx.P(24), Zx.P(288));
		tCode.Width = Zx.P(320);
		tCode.Indice = "ex. TPS-2026-MERCI";
		tCode.TextChanged += delegate
		{
			Appliquer();
		};
		cdTarif.Controls.Add(tCode);
		tarifPaye.Add(tCode);
		lCodeEtat = func("", 358, 298);
		tarifPaye.Add(lCodeEtat);
		tarifPaye.Add(func("Prix en points VIP (facultatif : les VIP peuvent le payer avec leurs points, ex. 1500)", 24, 338));
		tPoints = new Champ(multi: false, Zx.P(36));
		tPoints.Location = new Point(Zx.P(24), Zx.P(360));
		tPoints.Width = Zx.P(180);
		tPoints.Indice = "ex. 1500";
		tPoints.TextChanged += delegate
		{
			Appliquer();
		};
		cdTarif.Controls.Add(tPoints);
		tarifPaye.Add(tPoints);
		cdVisuel = Bloc("Photo de couverture", ref num2, num, Zx.P(44) + Zx.P(140) + Zx.P(12));
		apercu = new PictureBox();
		apercu.Location = new Point(Zx.P(24), Zx.P(50));
		apercu.Size = new Size(Zx.P(260), Zx.P(120));
		apercu.SizeMode = PictureBoxSizeMode.Zoom;
		apercu.BackColor = C.Champ;
		cdVisuel.Controls.Add(apercu);
		tImage = new Champ(multi: false, Zx.P(36));
		tImage.Location = new Point(Zx.P(304), Zx.P(50));
		tImage.Width = num - Zx.P(304) - Zx.P(24);
		tImage.ReadOnly = true;
		cdVisuel.Controls.Add(tImage);
		Bouton bouton9 = new Bouton("Choisir une image…");
		bouton9.Plein = false;
		bouton9.Size = new Size(Zx.P(210), Zx.P(38));
		bouton9.Location = new Point(Zx.P(304), Zx.P(98));
		bouton9.Click += delegate
		{
			ChoisirImage();
		};
		cdVisuel.Controls.Add(bouton9);
		Label value = new Label
		{
			Text = "jpg ou png, 5 Mo maximum  ·  envoyée sur GitHub à l'enregistrement",
			Font = new Font("Segoe UI", Zx.P(8.5f)),
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(Zx.P(306), Zx.P(144)),
			AutoSize = true
		};
		cdVisuel.Controls.Add(value);
		cdGal = Bloc("Galerie de la page (10 photos maximum)", ref num2, num, Zx.P(44) + Zx.P(188) + Zx.P(14) + Zx.P(42) + Zx.P(16));
		zoneGal = new ZoneDessin();
		zoneGal.Location = new Point(Zx.P(24), Zx.P(46));
		zoneGal.Size = new Size(num - Zx.P(48), Zx.P(188));
		zoneGal.Dessiner = DessinerGalerie;
		cdGal.Controls.Add(zoneGal);
		zoneGal.MouseDown += delegate(object o, MouseEventArgs e)
		{
			galSel = TuileGalerie(e.Location);
			zoneGal.Invalidate();
		};
		Bouton bouton10 = new Bouton("＋  Ajouter des photos…");
		bouton10.Size = new Size(Zx.P(230), Zx.P(38));
		bouton10.Location = new Point(Zx.P(24), Zx.P(46) + Zx.P(188) + Zx.P(14));
		bouton10.Click += delegate
		{
			AjouterPhotos();
		};
		cdGal.Controls.Add(bouton10);
		Bouton bouton11 = new Bouton("Retirer");
		bouton11.Plein = false;
		bouton11.Size = new Size(Zx.P(110), Zx.P(38));
		bouton11.Location = new Point(Zx.P(264), Zx.P(46) + Zx.P(188) + Zx.P(14));
		bouton11.Click += delegate
		{
			RetirerPhoto();
		};
		cdGal.Controls.Add(bouton11);
		Bouton bouton12 = new Bouton("◀");
		bouton12.Plein = false;
		bouton12.Size = new Size(Zx.P(50), Zx.P(38));
		bouton12.Location = new Point(Zx.P(384), Zx.P(46) + Zx.P(188) + Zx.P(14));
		bouton12.Click += delegate
		{
			DeplacerPhoto(-1);
		};
		cdGal.Controls.Add(bouton12);
		Bouton bouton13 = new Bouton("▶");
		bouton13.Plein = false;
		bouton13.Size = new Size(Zx.P(50), Zx.P(38));
		bouton13.Location = new Point(Zx.P(440), Zx.P(46) + Zx.P(188) + Zx.P(14));
		bouton13.Click += delegate
		{
			DeplacerPhoto(1);
		};
		cdGal.Controls.Add(bouton13);
		Label value2 = new Label
		{
			Text = "jpg ou png · envoyées à la publication",
			Font = new Font("Segoe UI", Zx.P(8.5f)),
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(Zx.P(506), Zx.P(46) + Zx.P(188) + Zx.P(24)),
			AutoSize = true
		};
		cdGal.Controls.Add(value2);
		cdContenu = Bloc("Contenu de la page", ref num2, num, Zx.P(44) + Zx.P(22) + Zx.P(176) + Zx.P(14) + Zx.P(22) + Zx.P(120) + Zx.P(14) + Zx.P(22) + Zx.P(36) + Zx.P(22));
		Etiq(cdContenu, "Description (onglet « À propos »)", Zx.P(24), Zx.P(44));
		tDesc = new Champ(multi: true, Zx.P(176));
		tDesc.Location = new Point(Zx.P(24), Zx.P(66));
		tDesc.Width = num - Zx.P(48);
		tDesc.TextChanged += delegate
		{
			Appliquer();
		};
		cdContenu.Controls.Add(tDesc);
		Etiq(cdContenu, "Notes de version", Zx.P(24), Zx.P(66) + Zx.P(176) + Zx.P(14));
		tNotes = new Champ(multi: true, Zx.P(120));
		tNotes.Location = new Point(Zx.P(24), Zx.P(66) + Zx.P(176) + Zx.P(36));
		tNotes.Width = num - Zx.P(48);
		tNotes.TextChanged += delegate
		{
			Appliquer();
		};
		cdContenu.Controls.Add(tNotes);
		Etiq(cdContenu, "Consigne affichée après JOUER (facultatif)", Zx.P(24), Zx.P(66) + Zx.P(176) + Zx.P(36) + Zx.P(120) + Zx.P(14));
		tConsigne = new Champ(multi: false, Zx.P(36));
		tConsigne.Location = new Point(Zx.P(24), Zx.P(66) + Zx.P(176) + Zx.P(36) + Zx.P(120) + Zx.P(36));
		tConsigne.Width = num - Zx.P(48);
		tConsigne.TextChanged += delegate
		{
			Appliquer();
		};
		cdContenu.Controls.Add(tConsigne);
		OrganiserBlocs();
		bool demarre = false;
		base.VisibleChanged += delegate
		{
			if (base.Visible && !demarre)
			{
				demarre = true;
				BeginInvoke((MethodInvoker)delegate
				{
					Charger();
				});
			}
		};
	}

	private Bouton Bt(string t, int x, int y, int w, AnchorStyles ancre, bool plein, EventHandler h)
	{
		Bouton bouton = new Bouton(t);
		bouton.Plein = plein;
		bouton.Size = new Size(w, Zx.P(38));
		bouton.Location = new Point(x, y);
		bouton.Anchor = ancre;
		bouton.Click += h;
		base.Controls.Add(bouton);
		return bouton;
	}

	private Segments sgSousOnglet;

	private int sousOnglet;

	private int colonnesEd = 2;

	private int largeurColEd;

	/// <summary>
	/// Range les sections de l'éditeur de mod : trois onglets (Général, Options et tarif,
	/// Médias et contenu), chacun sur deux colonnes, pour tout voir sans défiler.
	/// </summary>
	private void OrganiserBlocs()
	{
		if (cdContenu == null)
		{
			return;
		}
		Cadre[][][] groupes = new Cadre[3][][]
		{
			new Cadre[2][]
			{
				new Cadre[2] { cdIdentite, cdClass },
				new Cadre[1] { cdPub }
			},
			new Cadre[2][]
			{
				new Cadre[1] { cdOpt },
				new Cadre[2] { cdTarif, cdDlc }
			},
			new Cadre[2][]
			{
				new Cadre[2] { cdVisuel, cdGal },
				new Cadre[1] { cdContenu }
			}
		};
		edition.SuspendLayout();
		Point defil = edition.AutoScrollPosition;
		int y0 = Zx.P(52) + defil.Y;
		for (int g = 0; g < groupes.Length; g++)
		{
			bool vis = g == sousOnglet;
			int yUnique = y0;
			for (int c = 0; c < groupes[g].Length; c++)
			{
				int x = Zx.P(4) + defil.X + ((colonnesEd == 2) ? (c * (largeurColEd + Zx.P(16))) : 0);
				int y = ((colonnesEd == 2) ? y0 : yUnique);
				foreach (Cadre cadre in groupes[g][c])
				{
					cadre.Location = new Point(x, y);
					cadre.Visible = vis;
					y += cadre.Height + Zx.P(16);
				}
				yUnique = y;
			}
		}
		edition.ResumeLayout();
		edition.Invalidate(invalidateChildren: true);
	}

	private Cadre Bloc(string titre, ref int y, int cw, int h)
	{
		Cadre cadre = new Cadre();
		cadre.Titre = titre;
		cadre.Location = new Point(Zx.P(4), y);
		cadre.Size = new Size(cw, h);
		edition.Controls.Add(cadre);
		y += h + Zx.P(16);
		return cadre;
	}

	private void Etiq(Control parent, string t, int x, int y)
	{
		Label label = new Label();
		label.Text = t;
		label.Font = new Font("Segoe UI", Zx.P(9f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(x, y);
		label.AutoSize = true;
		parent.Controls.Add(label);
	}

	private Champ ChampDans(Cadre c, string etiquette, int rang, int w, bool multi, int h)
	{
		Etiq(c, etiquette, Zx.P(24), Zx.P(44) + rang * Zx.P(70));
		Champ champ = new Champ(multi, h);
		champ.Location = new Point(Zx.P(24), Zx.P(44) + rang * Zx.P(70) + Zx.P(20));
		champ.Width = w;
		champ.TextChanged += delegate
		{
			Appliquer();
		};
		c.Controls.Add(champ);
		return champ;
	}

	private static string Str(Dictionary<string, object> d, string k)
	{
		if (d == null || !d.TryGetValue(k, out var value) || value == null)
		{
			return "";
		}
		return value.ToString();
	}

	private void DessinerItem(object o, DrawItemEventArgs e)
	{
		if (e.Index < 0 || e.Index >= visibles.Count)
		{
			return;
		}
		Dictionary<string, object> dictionary = visibles[e.Index];
		bool flag = (e.State & DrawItemState.Selected) != 0;
		Graphics graphics = e.Graphics;
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.FillRectangle(Brushes.Transparent, e.Bounds);
		using (SolidBrush brush = new SolidBrush(C.Fond))
		{
			graphics.FillRectangle(brush, e.Bounds);
		}
		Rectangle r = new Rectangle(e.Bounds.X + Zx.P(2), e.Bounds.Y + Zx.P(3), e.Bounds.Width - Zx.P(6), e.Bounds.Height - Zx.P(6));
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(9)))
		{
			using (SolidBrush brush2 = new SolidBrush(flag ? C.CarteSurvol : C.Carte))
			{
				graphics.FillPath(brush2, path);
			}
			if (flag)
			{
				using Pen pen = new Pen(Color.FromArgb(150, C.Jade));
				graphics.DrawPath(pen, path);
			}
		}
		string text = Str(dictionary, "statut");
		object obj2;
		object obj;
		switch (text)
		{
		default:
			obj2 = "Disponible";
			goto IL_01a7;
		case "masque":
			obj2 = "Masqué";
			goto IL_01a7;
		case "programme":
			obj2 = "À date précise";
			goto IL_01a7;
		case "bientot":
			{
				obj = "Prochainement";
				break;
			}
			IL_01a7:
			obj = (string)obj2 + ((Str(dictionary, "prix").Length > 0) ? (" · " + Str(dictionary, "prix").Replace(".", ",") + " €") : " · Gratuit");
			break;
		}
		string text2 = (string)obj;
		Color color;
		switch (text)
		{
		default:
			color = C.Vert;
			break;
		case "masque":
			color = C.Gris;
			break;
		case "bientot":
		case "programme":
			color = C.Orange;
			break;
		}
		Color color2 = color;
		using (SolidBrush brush3 = new SolidBrush(color2))
		{
			graphics.FillEllipse(brush3, r.X + Zx.P(12), r.Y + Zx.P(20), Zx.P(10), Zx.P(10));
		}
		Lang.DrawText(graphics, (modifiees.Contains(dictionary) ? "✎ " : "") + Str(dictionary, "nom"), new Font("Segoe UI Semibold", Zx.P(10.5f)), new Rectangle(r.X + Zx.P(32), r.Y + Zx.P(6), r.Width - Zx.P(40), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		Lang.DrawText(graphics, Categories.Nom(Categories.Valide(Str(dictionary, "categorie"), Str(dictionary, "jeu"))) + "  ·  " + text2, F.Etat, new Rectangle(r.X + Zx.P(32), r.Y + Zx.P(27), r.Width - Zx.P(40), Zx.P(18)), color2, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
	}

	private void RemplirListe(Dictionary<string, object> garder)
	{
		charge = true;
		liste.Items.Clear();
		visibles.Clear();
		string text = ((tFiltre != null) ? tFiltre.Text.Trim().ToLowerInvariant() : "");
		foreach (Dictionary<string, object> page in pages)
		{
			if (text.Length == 0 || (Str(page, "nom") + " " + Str(page, "id") + " " + Str(page, "sous_titre")).ToLowerInvariant().IndexOf(text) >= 0)
			{
				visibles.Add(page);
				liste.Items.Add(Str(page, "nom"));
			}
		}
		int num = ((garder != null) ? visibles.IndexOf(garder) : ((visibles.Count <= 0) ? (-1) : 0));
		charge = false;
		if (num >= 0)
		{
			liste.SelectedIndex = num;
		}
		else if (garder == null || !pages.Contains(garder))
		{
			Choisir(-1);
		}
		entete.Invalidate();
	}

	private void Charger()
	{
		if (modifiees.Count + supprimees.Count > 0 && MessageBox.Show(this, "Recharger efface les modifications non publiées. Continuer ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
		{
			return;
		}
		lEtat.ForeColor = C.Gris;
		lEtat.Text = "Chargement depuis GitHub…";
		Application.DoEvents();
		base.UseWaitCursor = true;
		try
		{
			cat = GitHub.LireCatalogue(out sha);
			LireJeux();
			pages.Clear();
			modifiees.Clear();
			nouvelles.Clear();
			supprimees.Clear();
			imagesEnAttente.Clear();
			galeries.Clear();
			galModif.Clear();
			enLigne = true;
			if (cat.TryGetValue("projets", out var value) && value is ArrayList)
			{
				foreach (object item in (ArrayList)value)
				{
					if (item is Dictionary<string, object>)
					{
						pages.Add((Dictionary<string, object>)item);
					}
				}
			}
			lEtat.Text = pages.Count + " page(s) · catalogue à jour";
			RemplirListe(null);
		}
		catch (Exception ex)
		{
			enLigne = false;
			lEtat.ForeColor = C.Rouge;
			lEtat.Text = "Chargement impossible : " + ex.Message;
			Journal.Ecrire("admin-charger", ex);
			if (ex.Message.IndexOf("jeton administrateur", StringComparison.OrdinalIgnoreCase) < 0 || dejaRedemande)
			{
				return;
			}
			dejaRedemande = true;
			BeginInvoke((MethodInvoker)delegate
			{
				if (RedemanderJeton())
				{
					Charger();
				}
			});
		}
		finally
		{
			base.UseWaitCursor = false;
			entete.Invalidate();
		}
	}

	private void MajTarif()
	{
		bool flag = sgPrix.Index == 1;
		foreach (Control item in tarifPaye)
		{
			item.Visible = flag;
		}
		int num = (flag ? Zx.P(420) : Zx.P(112));
		if (num != cdTarif.Height)
		{
			cdTarif.Height = num;
			OrganiserBlocs();
		}
	}

	private void MajVipAdm()
	{
		bool flag = sgVip.Index == 1;
		Label label = lPubEtiq;
		Champ champ = tPubDate;
		bool flag3 = (tPubHeure.Visible = flag);
		bool visible = (champ.Visible = flag3);
		label.Visible = visible;
	}

	private void MajStatut()
	{
		int num = Math.Max(0, sgStatut.Index);
		lAideStatut.Text = statutsAide[num];
		bool flag = num == 1 || num == 2;
		Label label = lDateEtiq;
		Champ champ = tDate;
		bool flag3 = (tHeure.Visible = flag);
		bool visible = (champ.Visible = flag3);
		label.Visible = visible;
		lDateEtiq.Text = ((num == 2) ? "Date et heure de mise en ligne automatique (obligatoire)" : "Date prévue (facultative)");
		cDate.Checked = num switch
		{
			1 => tDate.Text.Trim().Length > 0, 
			2 => true, 
			_ => false, 
		};
	}

	private void Choisir(int i)
	{
		charge = true;
		cur = ((i >= 0 && i < visibles.Count) ? visibles[i] : null);
		edition.Visible = cur != null;
		bSauver.Enabled = cur != null;
		bPaquet.Enabled = cur != null;
		if (cur != null)
		{
			long.TryParse(Str(cur, "taille"), out var result);
			lInfo.Text = ((Str(cur, "version") != "") ? ("Version " + Str(cur, "version") + "  ·  paquet du " + Str(cur, "date") + ((result > 0) ? ("  ·  " + result / 1048576 + " Mo") : "")) : "Page sans paquet à installer (annonce)");
			tId.Text = Str(cur, "id");
			tId.ReadOnly = !nouvelles.Contains(cur);
			tNom.Text = Str(cur, "nom");
			tSous.Text = Str(cur, "sous_titre");
			tDesc.Text = Str(cur, "description").Replace("\\n", "\r\n").Replace("\n", "\r\n");
			tNotes.Text = Str(cur, "notes").Replace("\\n", "\r\n").Replace("\n", "\r\n");
			tConsigne.Text = Str(cur, "consigne");
			string text0 = ((Str(cur, "jeu").Length > 0) ? Str(cur, "jeu") : "omsi2");
			sgJeuPage.Index = Math.Max(0, Array.IndexOf(idsJeux, text0));
			MajCatsJeu(text0, Str(cur, "categorie"));
			int num = Array.IndexOf(statutsId, Str(cur, "statut"));
			sgStatut.Index = ((num >= 0) ? num : 0);
			DateTime result2;
			bool flag = DateTime.TryParseExact(Str(cur, "sortie"), new string[2] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out result2);
			cDate.Checked = flag;
			tDate.Text = (flag ? result2.ToString("dd/MM/yyyy") : "");
			tHeure.Text = (flag ? result2.ToString("HH:mm") : "");
			cUne.Checked = Str(cur, "a_la_une").ToLowerInvariant() == "true";
			tOrdre.Text = Str(cur, "ordre");
			tImage.Text = (imagesEnAttente.ContainsKey(Str(cur, "id")) ? imagesEnAttente[Str(cur, "id")] : Str(cur, "image"));
			ChargerApercu();
			ChargerGalerieAdmin();
			decimal d;
			bool flag2 = Projet.ParsePrix(Str(cur, "prix"), out d);
			sgPrix.Index = (flag2 ? 1 : 0);
			tPrix.Text = (flag2 ? d.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR")) : "");
			tLien.Text = Str(cur, "lien_achat");
			tCode.Text = "";
			lCodeEtat.Text = ((Str(cur, "acces_sha256").Length > 0) ? "✔  code défini" : "aucun code défini");
			lCodeEtat.ForeColor = ((Str(cur, "acces_sha256").Length > 0) ? C.Vert : C.Orange);
			MajTarif();
			string text = Str(cur, "vip");
			sgVip.Index = ((text == "avance") ? 1 : ((text == "test") ? 2 : 0));
			tPoints.Text = Str(cur, "prix_points");
			DateTime result3;
			bool flag3 = DateTime.TryParseExact(Str(cur, "public_le"), new string[2] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out result3);
			tPubDate.Text = (flag3 ? result3.ToString("dd/MM/yyyy") : "");
			tPubHeure.Text = (flag3 ? result3.ToString("HH:mm") : "");
			MajVipAdm();
			ChargerDlc();
			tEditeur.Text = Str(cur, "editeur");
			tPrereq.Text = Str(cur, "prerequis");
			sgRuban.Index = Math.Max(0, Array.IndexOf(Projet.RubansId, Str(cur, "ruban")));
			tVideo.Text = (videosEnAttente.ContainsKey(Str(cur, "id")) ? videosEnAttente[Str(cur, "id")] : Str(cur, "video"));
			if (tEditeur.Text.Length == 0)
			{
				tEditeur.Text = "TPS Groupe";
			}
			if (tVideo.Text.Length == 0)
			{
				Dictionary<string, object> page0 = cur;
				string yt = YouTube.TrailerPour(new Projet
				{
					Nom = Str(cur, "nom")
				}, null, delegate
				{
					try
					{
						BeginInvoke((MethodInvoker)delegate
						{
							if (!base.IsDisposed && cur == page0 && tVideo.Text.Length == 0)
							{
								bool c0 = charge;
								charge = true;
								tVideo.Text = YouTube.TrailerPour(new Projet
								{
									Nom = Str(page0, "nom")
								}, null, null) ?? "";
								charge = c0;
							}
						});
					}
					catch
					{
					}
				});
				if (yt != null)
				{
					tVideo.Text = yt;
				}
			}
			tLiens.Text = Str(cur, "liens").Replace("\r\n", "\n").Replace("\n", "\r\n");
			tAncien.Text = (Projet.ParsePrix(Str(cur, "ancien_prix"), out var d2) ? d2.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR")) : "");
			tLies.Text = Str(cur, "lies");
			tTags.Text = Str(cur, "tags");
			cCom.Checked = Str(cur, "commentaires").ToLowerInvariant() != "off";
			cNouv.Checked = Str(cur, "nouveautes").ToLowerInvariant() == "true";
			MajStatut();
		}
		charge = false;
		liste.Invalidate();
	}

	private void ChargerApercu()
	{
		Image image = apercu.Image;
		apercu.Image = null;
		image?.Dispose();
		try
		{
			string text = tImage.Text;
			if (text.Length == 0)
			{
				return;
			}
			if (File.Exists(text))
			{
				using (FileStream stream = File.OpenRead(text))
				{
					apercu.Image = new Bitmap(stream);
					return;
				}
			}
			if (!Web.UrlSure(text))
			{
				return;
			}
			using WebClient webClient = Web.Client();
			byte[] buffer = webClient.DownloadData(text);
			using MemoryStream stream2 = new MemoryStream(buffer);
			apercu.Image = new Bitmap(stream2);
		}
		catch
		{
		}
	}

	/// <summary>Jeux du catalogue (pour le sélecteur « Jeu ») et leurs catégories.</summary>
	private void LireJeux()
	{
		List<string> list = new List<string> { "omsi2" };
		List<string> list2 = new List<string> { "OMSI 2" };
		if (cat != null && (cat.TryGetValue("jeux_liste", out var value) || cat.TryGetValue("jeux", out value)) && value is ArrayList)
		{
			foreach (object item in (ArrayList)value)
			{
				if (!(item is Dictionary<string, object> dictionary))
				{
					continue;
				}
				string text = Str(dictionary, "id");
				if (text.Length != 0)
				{
					Categories.Definir(text, Str(dictionary, "categories"));
					if (text == "omsi2")
					{
						list2[0] = ((Str(dictionary, "nom").Length > 0) ? Str(dictionary, "nom") : "OMSI 2");
					}
					else if (!list.Contains(text))
					{
						list.Add(text);
						list2.Add((Str(dictionary, "nom").Length > 0) ? Str(dictionary, "nom") : text);
					}
				}
			}
		}
		idsJeux = list.ToArray();
		nomsJeux = list2.ToArray();
		if (sgJeuPage != null)
		{
			bool flag = charge;
			charge = true;
			sgJeuPage.Items = nomsJeux;
			sgJeuPage.Invalidate();
			charge = flag;
		}
	}

	/// <summary>Le sélecteur de catégorie montre les catégories du jeu de la page.</summary>
	private void MajCatsJeu(string jeu, string categorie)
	{
		bool flag = charge;
		charge = true;
		sgCat.Items = Categories.NomsDe(jeu);
		sgCat.Index = Math.Max(0, Array.IndexOf(Categories.IdsDe(jeu), Categories.Valide(categorie, jeu)));
		sgCat.Invalidate();
		charge = flag;
	}

	private void Appliquer()
	{
		if (charge || cur == null)
		{
			return;
		}
		Poser("nom", tNom.Text.Trim(), garder: true);
		Poser("sous_titre", tSous.Text.Trim(), garder: false);
		Poser("description", tDesc.Text.Replace("\r\n", "\n"), garder: false);
		Poser("notes", tNotes.Text.Replace("\r\n", "\n"), garder: false);
		Poser("consigne", tConsigne.Text.Trim(), garder: false);
		string text0 = idsJeux[Math.Max(0, Math.Min(idsJeux.Length - 1, sgJeuPage.Index))];
		cur["jeu"] = text0;
		string[] array0 = Categories.IdsDe(text0);
		cur["categorie"] = array0[Math.Max(0, Math.Min(array0.Length - 1, sgCat.Index))];
		string text = statutsId[Math.Max(0, sgStatut.Index)];
		if (text == "publie")
		{
			cur.Remove("statut");
		}
		else
		{
			cur["statut"] = text;
		}
		bool flag = text == "programme" || (text == "bientot" && tDate.Text.Trim().Length > 0);
		if (flag && DateTime.TryParseExact(tDate.Text.Trim() + " " + ((tHeure.Text.Trim().Length > 0) ? tHeure.Text.Trim() : "00:00"), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			cur["sortie"] = result.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
		}
		else if (!flag)
		{
			cur.Remove("sortie");
		}
		if (sgPrix.Index == 1)
		{
			if (Projet.ParsePrix(tPrix.Text, out var d))
			{
				cur["prix"] = d.ToString("0.00", CultureInfo.InvariantCulture);
			}
			Poser("lien_achat", tLien.Text.Trim(), garder: false);
			if (tCode.Text.Trim().Length > 0)
			{
				cur["acces_sha256"] = Projet.Sha256Hex(tCode.Text.Trim().ToLowerInvariant());
				lCodeEtat.Text = "✔  nouveau code défini";
				lCodeEtat.ForeColor = C.Vert;
			}
		}
		else
		{
			cur.Remove("prix");
			cur.Remove("lien_achat");
			cur.Remove("acces_sha256");
		}
		if (cUne.Checked)
		{
			cur["a_la_une"] = true;
		}
		else
		{
			cur.Remove("a_la_une");
		}
		if (sgVip.Index == 1)
		{
			cur["vip"] = "avance";
			if (DateTime.TryParseExact(tPubDate.Text.Trim() + " " + ((tPubHeure.Text.Trim().Length > 0) ? tPubHeure.Text.Trim() : "00:00"), "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result2))
			{
				cur["public_le"] = result2.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
			}
		}
		else
		{
			if (sgVip.Index == 2)
			{
				cur["vip"] = "test";
			}
			else
			{
				cur.Remove("vip");
			}
			cur.Remove("public_le");
		}
		if (sgPrix.Index == 1 && int.TryParse(tPoints.Text.Trim(), out var result3) && result3 > 0 && result3 <= 1000000)
		{
			cur["prix_points"] = result3;
		}
		else
		{
			cur.Remove("prix_points");
		}
		if (int.TryParse(tOrdre.Text.Trim(), out var result4) && result4 > 0)
		{
			cur["ordre"] = Math.Min(result4, 999);
		}
		else
		{
			cur.Remove("ordre");
		}
		Poser("editeur", tEditeur.Text.Trim(), garder: false);
		Poser("prerequis", tPrereq.Text.Trim(), garder: false);
		Poser("ruban", Projet.RubansId[Math.Max(0, sgRuban.Index)], garder: false);
		Poser("liens", tLiens.Text.Replace("\r\n", "\n").Trim(), garder: false);
		List<string> list = new List<string>();
		string[] array = tTags.Text.Split(new char[2] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text2 in array)
		{
			string text3 = text2.Trim().TrimStart('#');
			if (text3.Length > 0 && text3.Length <= 24 && list.Count < 8 && !list.Contains(text3))
			{
				list.Add(text3);
			}
		}
		Poser("tags", string.Join(", ", list.ToArray()), garder: false);
		Poser("lies", string.Join(", ", tLies.Text.ToLowerInvariant().Split(new char[3] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)), garder: false);
		if (Projet.ParsePrix(tAncien.Text, out var d2))
		{
			cur["ancien_prix"] = d2.ToString("0.00", CultureInfo.InvariantCulture);
		}
		else if (tAncien.Text.Trim().Length == 0)
		{
			cur.Remove("ancien_prix");
		}
		if (cCom.Checked)
		{
			cur.Remove("commentaires");
		}
		else
		{
			cur["commentaires"] = "off";
		}
		if (cNouv.Checked)
		{
			cur["nouveautes"] = true;
		}
		else
		{
			cur.Remove("nouveautes");
		}
		string text4 = tId.Text.Trim().ToLowerInvariant();
		if (!tId.ReadOnly && text4.Length > 0)
		{
			cur["id"] = text4;
		}
		modifiees.Add(cur);
		liste.Invalidate();
		entete.Invalidate();
	}

	private void ChoisirVideo()
	{
		if (cur == null)
		{
			return;
		}
		string text = Str(cur, "id");
		if (text.Length == 0)
		{
			MessageBox.Show(this, "Renseignez d'abord l'identifiant de la page.", Text);
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Vidéo MP4 (*.mp4)|*.mp4";
		openFileDialog.Title = "Bande-annonce de la page (3 min maximum, de 1080p à 4K)";
		if (openFileDialog.ShowDialog(this) == DialogResult.OK)
		{
			base.UseWaitCursor = true;
			string text2;
			double secondes;
			int largeur;
			int hauteur;
			try
			{
				text2 = Video.Verifier(openFileDialog.FileName, out secondes, out largeur, out hauteur);
			}
			finally
			{
				base.UseWaitCursor = false;
			}
			if (text2 != null)
			{
				MessageBox.Show(this, text2, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}
			videosEnAttente[text] = openFileDialog.FileName;
			tVideo.Text = openFileDialog.FileName;
			modifiees.Add(cur);
			liste.Invalidate();
			entete.Invalidate();
			lEtat.ForeColor = C.Vert;
			lEtat.Text = "✔  Vidéo acceptée : " + Video.Resume(secondes, largeur, hauteur) + "  ·  envoyée à la publication";
		}
	}

	private void RetirerVideo()
	{
		if (cur != null)
		{
			videosEnAttente.Remove(Str(cur, "id"));
			cur.Remove("video");
			tVideo.Text = "";
			modifiees.Add(cur);
			liste.Invalidate();
			entete.Invalidate();
		}
	}

	private void Poser(string cle, string v, bool garder)
	{
		if (v.Length == 0 && !garder)
		{
			cur.Remove(cle);
		}
		else
		{
			cur[cle] = v;
		}
	}

	private void ChargerDlc()
	{
		dlcEd.Clear();
		string[] array = Str(cur, "dlc").Replace("\r", "").Split('\n');
		foreach (string text in array)
		{
			string[] array2 = text.Split('|');
			string text2 = array2[0].Trim();
			if (text2.Length != 0)
			{
				dlcEd.Add(new string[3]
				{
					text2,
					(array2.Length > 1) ? array2[1].Trim() : ("DLC " + text2),
					(array2.Length > 2 && array2[2].Trim().ToLowerInvariant().StartsWith("fac")) ? "1" : ""
				});
			}
		}
		RemplirDlc(-1);
		tDlcId.Text = "";
		tDlcNom.Text = "";
		sgDlc.Index = 0;
		lDlcEtat.Text = "";
	}

	private void RemplirDlc(int sel)
	{
		lDlc.Items.Clear();
		dlcEd.Sort(delegate(string[] a, string[] b)
		{
			int num = a[2].CompareTo(b[2]);
			return (num == 0) ? string.Compare(a[1], b[1], StringComparison.OrdinalIgnoreCase) : num;
		});
		foreach (string[] item in dlcEd)
		{
			lDlc.Items.Add(((item[2] == "1") ? "     Facultatif  ·  " : "★  Obligatoire  ·  ") + item[1] + "   (" + item[0] + ")");
		}
		if (sel >= 0 && sel < lDlc.Items.Count)
		{
			lDlc.SelectedIndex = sel;
		}
	}

	private void SauverDlc(int sel)
	{
		if (cur == null)
		{
			return;
		}
		string text = ((sel >= 0 && sel < dlcEd.Count) ? dlcEd[sel][0] : "");
		List<string> list = new List<string>();
		foreach (string[] item in dlcEd)
		{
			list.Add(item[0] + " | " + item[1].Replace("|", "/") + ((item[2] == "1") ? " | facultatif" : ""));
		}
		Poser("dlc", string.Join("\n", list.ToArray()), garder: false);
		int count = dlcEd.FindAll((string[] d) => d[2] != "1").Count;
		int num = dlcEd.Count - count;
		if (dlcEd.Count > 0)
		{
			tPrereq.Text = "OMSI 2 + " + count + ((count > 1) ? " DLC obligatoires" : " DLC obligatoire") + ((num > 0) ? (" (" + num + ((num > 1) ? " facultatifs" : " facultatif") + ")") : "");
		}
		modifiees.Add(cur);
		liste.Invalidate();
		entete.Invalidate();
		RemplirDlc(-1);
		for (int i = 0; i < dlcEd.Count; i++)
		{
			if (dlcEd[i][0] == text)
			{
				lDlc.SelectedIndex = i;
				break;
			}
		}
	}

	private void ChoisirImage()
	{
		if (cur == null)
		{
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
		openFileDialog.Title = "Image de la page";
		if (openFileDialog.ShowDialog(this) == DialogResult.OK)
		{
			string text = Str(cur, "id");
			if (text.Length == 0)
			{
				MessageBox.Show(this, "Renseignez d'abord l'identifiant de la page.", Text);
				return;
			}
			imagesEnAttente[text] = openFileDialog.FileName;
			tImage.Text = openFileDialog.FileName;
			ChargerApercu();
			modifiees.Add(cur);
			liste.Invalidate();
			entete.Invalidate();
		}
	}

	private void Nouvelle()
	{
		int i;
		for (i = 1; pages.Exists((Dictionary<string, object> p) => Str(p, "id") == "nouvelle-page-" + i); i++)
		{
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["id"] = "nouvelle-page-" + i;
		dictionary["nom"] = "Nouvelle page";
		dictionary["sous_titre"] = "";
		dictionary["jeu"] = "omsi2";
		dictionary["categorie"] = "cartes";
		dictionary["statut"] = "bientot";
		dictionary["description"] = "";
		dictionary["notes"] = "";
		pages.Add(dictionary);
		modifiees.Add(dictionary);
		nouvelles.Add(dictionary);
		tFiltre.Text = "";
		RemplirListe(dictionary);
	}

	private void Supprimer()
	{
		if (cur != null && MessageBox.Show(this, "Retirer la page « " + Str(cur, "nom") + " » du catalogue ?\n\nElle disparaît du lanceur de tous les joueurs à la publication. Les fichiers déjà installés et la release GitHub ne sont pas supprimés.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			if (!nouvelles.Contains(cur))
			{
				supprimees.Add(Str(cur, "id"));
			}
			modifiees.Remove(cur);
			nouvelles.Remove(cur);
			pages.Remove(cur);
			cur = null;
			RemplirListe(null);
		}
	}

	private List<ItemGal> GalCourante()
	{
		if (cur == null)
		{
			return new List<ItemGal>();
		}
		if (galeries.TryGetValue(cur, out var value))
		{
			return value;
		}
		value = new List<ItemGal>();
		if (cur.TryGetValue("galerie", out var value2) && value2 is ArrayList)
		{
			foreach (object item in (ArrayList)value2)
			{
				if (item != null && value.Count < 10)
				{
					ItemGal itemGal = new ItemGal();
					itemGal.Url = item.ToString();
					value.Add(itemGal);
					ChargerMini(itemGal);
				}
			}
		}
		galeries[cur] = value;
		return value;
	}

	private void ChargerGalerieAdmin()
	{
		galSel = -1;
		GalCourante();
		zoneGal.Invalidate();
	}

	private void ChargerMini(ItemGal it)
	{
		string src = ((it.Fichier.Length > 0) ? it.Fichier : it.Url);
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				byte[] buffer;
				if (File.Exists(src))
				{
					buffer = File.ReadAllBytes(src);
				}
				else
				{
					if (!Web.UrlSure(src))
					{
						return;
					}
					using WebClient webClient = Web.Client();
					buffer = webClient.DownloadData(src);
				}
				using (MemoryStream stream = new MemoryStream(buffer))
				{
					using Image original = Image.FromStream(stream);
					it.Mini = new Bitmap(original, new Size(Zx.P(296), Zx.P(166)));
				}
				if (base.IsHandleCreated && !base.IsDisposed)
				{
					BeginInvoke((MethodInvoker)delegate
					{
						if (zoneGal != null)
						{
							zoneGal.Invalidate();
						}
					});
				}
			}
			catch (Exception e)
			{
				Journal.Ecrire("admin-galerie", e);
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private Rectangle RectGalerie(int i, int w)
	{
		int num = 5;
		int num2 = Zx.P(12);
		int num3 = (w - (num - 1) * num2) / num;
		int num4 = num3 * 9 / 16;
		return new Rectangle(i % num * (num3 + num2), i / num * (num4 + num2), num3, num4);
	}

	private int TuileGalerie(Point p)
	{
		List<ItemGal> list = GalCourante();
		for (int i = 0; i < list.Count; i++)
		{
			if (RectGalerie(i, zoneGal.Width).Contains(p))
			{
				return i;
			}
		}
		return -1;
	}

	private void DessinerGalerie(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		List<ItemGal> list = GalCourante();
		for (int i = 0; i < 10; i++)
		{
			Rectangle rectangle = RectGalerie(i, w);
			using GraphicsPath path = Bouton.Rond(rectangle, Zx.P(8));
			if (i < list.Count)
			{
				using (SolidBrush brush = new SolidBrush(C.Champ))
				{
					g.FillPath(brush, path);
				}
				if (list[i].Mini != null)
				{
					GraphicsState gstate = g.Save();
					g.SetClip(path, CombineMode.Intersect);
					g.DrawImage(list[i].Mini, rectangle);
					g.Restore(gstate);
				}
				else
				{
					Lang.DrawText(g, "…", new Font("Segoe UI", Zx.P(12f)), rectangle, C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				}
				if (list[i].Fichier.Length > 0)
				{
					Rectangle r = new Rectangle(rectangle.X + Zx.P(6), rectangle.Y + Zx.P(6), Zx.P(46), Zx.P(18));
					using (GraphicsPath path2 = Bouton.Rond(r, Zx.P(8)))
					{
						using SolidBrush brush2 = new SolidBrush(C.Orange);
						g.FillPath(brush2, path2);
					}
					Lang.DrawText(g, "nouv.", new Font("Segoe UI", Zx.P(7.5f), FontStyle.Bold), r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				using (Pen pen = new Pen((i == galSel) ? C.Jade : C.Ligne, (i == galSel) ? Zx.P(3f) : 1f))
				{
					g.DrawPath(pen, path);
				}
				using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(180, C.Fond)))
				{
					g.FillEllipse(brush3, rectangle.Right - Zx.P(24), rectangle.Y + Zx.P(6), Zx.P(18), Zx.P(18));
				}
				Lang.DrawText(g, (i + 1).ToString(), new Font("Segoe UI", Zx.P(8f), FontStyle.Bold), new Rectangle(rectangle.Right - Zx.P(24), rectangle.Y + Zx.P(6), Zx.P(18), Zx.P(18)), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			else
			{
				Pen pen2 = new Pen(Color.FromArgb(70, 150, 170, 200));
				pen2.DashStyle = DashStyle.Dash;
				using (Pen pen3 = pen2)
				{
					g.DrawPath(pen3, path);
				}
				Lang.DrawText(g, "libre", new Font("Segoe UI", Zx.P(8.5f)), rectangle, Color.FromArgb(110, 150, 170, 200), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}
	}

	private void AjouterPhotos()
	{
		if (cur == null)
		{
			return;
		}
		List<ItemGal> list = GalCourante();
		if (list.Count >= 10)
		{
			MessageBox.Show(this, "La galerie contient déjà 10 photos : retirez-en une avant d'en ajouter.", Text);
			return;
		}
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
		openFileDialog.Title = "Photos de la galerie";
		openFileDialog.Multiselect = true;
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		string text = "";
		string[] fileNames = openFileDialog.FileNames;
		foreach (string text2 in fileNames)
		{
			if (list.Count >= 10)
			{
				num2++;
				continue;
			}
			try
			{
				string text3 = Path.GetExtension(text2).ToLowerInvariant();
				if (text3 != ".jpg" && text3 != ".jpeg" && text3 != ".png")
				{
					throw new Exception("format jpg ou png seulement");
				}
				if (new FileInfo(text2).Length > 20971520)
				{
					throw new Exception("photo trop lourde (20 Mo maximum)");
				}
				using (Image image = Image.FromFile(text2))
				{
					if (image.Width < 300 || image.Height < 150)
					{
						throw new Exception("photo trop petite (300×150 minimum)");
					}
				}
				ItemGal itemGal = new ItemGal();
				itemGal.Fichier = text2;
				list.Add(itemGal);
				ChargerMini(itemGal);
				num++;
			}
			catch (Exception ex)
			{
				string text4 = text;
				text = text4 + "\n" + Path.GetFileName(text2) + " : " + ex.Message;
			}
		}
		if (num > 0)
		{
			galModif.Add(cur);
			modifiees.Add(cur);
			liste.Invalidate();
			entete.Invalidate();
			zoneGal.Invalidate();
		}
		if (num2 > 0 || text.Length > 0)
		{
			MessageBox.Show(this, ((num2 > 0) ? (num2 + " photo(s) ignorée(s) : 10 photos au maximum.\n") : "") + ((text.Length > 0) ? ("Refusées :" + text) : ""), Text);
		}
	}

	private void RetirerPhoto()
	{
		List<ItemGal> list = GalCourante();
		if (galSel >= 0 && galSel < list.Count)
		{
			list.RemoveAt(galSel);
			galSel = Math.Min(galSel, list.Count - 1);
			galModif.Add(cur);
			modifiees.Add(cur);
			liste.Invalidate();
			entete.Invalidate();
			zoneGal.Invalidate();
		}
	}

	private void DeplacerPhoto(int d)
	{
		List<ItemGal> list = GalCourante();
		int num = galSel + d;
		if (galSel >= 0 && galSel < list.Count && num >= 0 && num < list.Count)
		{
			ItemGal value = list[galSel];
			list[galSel] = list[num];
			list[num] = value;
			galSel = num;
			galModif.Add(cur);
			modifiees.Add(cur);
			liste.Invalidate();
			entete.Invalidate();
			zoneGal.Invalidate();
		}
	}

	private bool RedemanderJeton()
	{
		if (Fenetre.ModeCapture)
		{
			return false;
		}
		MessageBox.Show(this, "Le jeton GitHub de l'administration n'est plus lisible sur ce PC.\n\nCollez-le à nouveau dans la fenêtre suivante (il reste chiffré par Windows).", Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		using FenJeton f = new FenJeton();
		return f.Afficher(this) == DialogResult.OK;
	}

	public void Onglet(int i)
	{
		if (ongletsAdm.Index == i)
		{
			ChangerOnglet(i);
		}
		else
		{
			ongletsAdm.Index = i;
		}
	}

	private void ChangerOnglet(int i)
	{
		foreach (Control control3 in zoneOnglet.Controls)
		{
			try
			{
				control3.Dispose();
			}
			catch
			{
			}
		}
		zoneOnglet.Controls.Clear();
		foreach (Control control4 in base.Controls)
		{
			if (control4 != entete && control4 != zoneOnglet)
			{
				control4.Visible = i == 1;
			}
		}
		if (i == 1)
		{
			zoneOnglet.Visible = false;
			return;
		}
		try
		{
			Dictionary<string, object> dictionary;
			if (File.Exists(Config.Catalogue))
			{
				string text = "";
				dictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(File.ReadAllText(Config.Catalogue, Encoding.UTF8));
			}
			else
			{
				dictionary = GitHub.LireCatalogue(out var _);
			}
			int onglet = i;
			Action recharger = delegate
			{
				ChangerOnglet(onglet);
			};
			object value;
			if (i == 0)
			{
				List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
				if ((dictionary.TryGetValue("jeux_liste", out value) || dictionary.TryGetValue("jeux", out value)) && value is ArrayList)
				{
					foreach (object item in (ArrayList)value)
					{
						if (!(item is Dictionary<string, object> dictionary2))
						{
							continue;
						}
						Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
						foreach (KeyValuePair<string, object> item2 in dictionary2)
						{
							dictionary3[item2.Key] = string.Concat(item2.Value);
						}
						list.Add(dictionary3);
					}
				}
				FenJeux f = new FenJeux(list);
				f.Annule = recharger;
				f.Publie = delegate
				{
					try
					{
						ArrayList arrayList = new ArrayList();
						foreach (Dictionary<string, string> item3 in f.Jeux)
						{
							Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
							foreach (KeyValuePair<string, string> item4 in item3)
							{
								if (item4.Value.Length > 0)
								{
									dictionary7[item4.Key] = item4.Value;
								}
							}
							arrayList.Add(dictionary7);
						}
						string text3;
						Dictionary<string, object> dictionary8 = GitHub.LireCatalogue(out text3);
						dictionary8.Remove("jeux");
						if (arrayList.Count > 0)
						{
							dictionary8["jeux_liste"] = arrayList;
						}
						else
						{
							dictionary8.Remove("jeux_liste");
						}
						GitHub.EcrireCatalogue(dictionary8, text3, "Jeux modifiés depuis le lanceur par " + ((Comptes.Actif != null) ? Comptes.Actif.Pseudo : "admin"));
						lEtat.ForeColor = C.Vert;
						lEtat.Text = "✔  Jeux publiés (" + f.Jeux.Count + ")";
						recharger();
					}
					catch (Exception ex3)
					{
						Journal.Ecrire("admin-jeux", ex3);
						MessageBox.Show(this, "Publication impossible :\n" + ex3.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
				};
				f.TopLevel = false;
				f.FormBorderStyle = FormBorderStyle.None;
				zoneOnglet.Controls.Add(f);
				f.Location = new Point(Math.Max(0, (zoneOnglet.ClientSize.Width - f.Width) / 2), Zx.P(8));
				f.Show();
			}
			else
			{
				Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
				if (dictionary.TryGetValue("communaute", out value) && value is Dictionary<string, object>)
				{
					foreach (KeyValuePair<string, object> item5 in (Dictionary<string, object>)value)
					{
						dictionary4[item5.Key] = string.Concat(item5.Value);
					}
				}
				PageReglages pg = i switch
				{
					4 => PageCommunaute(dictionary4), 
					3 => PageBoutique(dictionary4), 
					2 => PageAccueil(dictionary4), 
					_ => PageMaintenance(dictionary4), 
				};
				string nomOnglet = OngletsNoms[i];
				pg.Annule = recharger;
				pg.Publie = delegate(Dictionary<string, string> valeurs)
				{
					try
					{
						string text2;
						Dictionary<string, object> dictionary5 = GitHub.LireCatalogue(out text2);
						object value2;
						Dictionary<string, object> dictionary6 = ((dictionary5.TryGetValue("communaute", out value2) && value2 is Dictionary<string, object>) ? ((Dictionary<string, object>)value2) : new Dictionary<string, object>());
						foreach (KeyValuePair<string, string> valeur in valeurs)
						{
							if (valeur.Value.Length > 0)
							{
								dictionary6[valeur.Key] = valeur.Value;
							}
							else
							{
								dictionary6.Remove(valeur.Key);
							}
						}
						if (dictionary6.Count > 0)
						{
							dictionary5["communaute"] = dictionary6;
						}
						else
						{
							dictionary5.Remove("communaute");
						}
						GitHub.EcrireCatalogue(dictionary5, text2, "Onglet « " + nomOnglet + " » modifié depuis le lanceur par " + ((Comptes.Actif != null) ? Comptes.Actif.Pseudo : "admin"));
						lEtat.ForeColor = C.Vert;
						lEtat.Text = "✔  « " + nomOnglet + " » publié";
						pg.Message("✔  Publié pour tous les joueurs.", C.Vert);
					}
					catch (Exception ex2)
					{
						Journal.Ecrire("admin-onglet", ex2);
						pg.Message("✖  Publication impossible : " + ex2.Message, C.Rouge);
					}
				};
				pg.Dock = DockStyle.Fill;
				zoneOnglet.Controls.Add(pg);
			}
			zoneOnglet.Visible = true;
			zoneOnglet.BringToFront();
		}
		catch (Exception ex)
		{
			Journal.Ecrire("admin-onglet", ex);
			if (ex.Message.IndexOf("jeton administrateur", StringComparison.OrdinalIgnoreCase) >= 0 && RedemanderJeton())
			{
				ChangerOnglet(i);
				return;
			}
			if (!Fenetre.ModeCapture)
			{
				MessageBox.Show(this, "Lecture du catalogue impossible :\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			if (i != 1)
			{
				ongletsAdm.Index = 1;
			}
		}
	}

	private PageReglages PageAccueil(Dictionary<string, string> init)
	{
		PageReglages pageReglages = new PageReglages("Accueil", "Ce que tous les joueurs voient sur l'écran d'accueil du lanceur.", init);
		pageReglages.Section("Bandeau d'alerte");
		pageReglages.Texte("alerte", "Message en haut de l'accueil (vide = aucun bandeau)", "ex. Maintenance du serveur ce soir de 22 h à 23 h", 140);
		pageReglages.Choix("alerte_type", "Couleur du bandeau", new string[3] { "Information", "Avertissement", "Important" }, new string[3] { "information", "avertissement", "important" });
		pageReglages.Section("Site web");
		pageReglages.Note("La grande case « Actualités » de l'accueil affiche les articles du site ; un clic ouvre le site.");
		pageReglages.Texte("site", "Adresse du site TPS Groupe", "https://…", 300);
		pageReglages.Texte("site_flux", "Flux des actualités du site (RSS, Atom ou JSON)", "ex. https://…/feed.xml ou https://…/actualites.json", 300);
		pageReglages.Section("Annonces de l'équipe");
		pageReglages.Note("Une annonce par ligne : « Titre | texte ». Elles s'affichent dans la case « Actualités », après les articles du site.");
		pageReglages.Texte("annonces", "Annonces", "Tournoi ParisView samedi | Rendez-vous sur le Discord à 20 h", 1500, multi: true, 160);
		pageReglages.Section("Thèmes des fêtes");
		pageReglages.Choix("themes", "Décors de l'accueil (Noël, Halloween, Carnaval…)", new string[2] { "Selon le calendrier", "Désactivés pour tous" }, new string[2] { "", "off" });
		pageReglages.Verif = delegate(Dictionary<string, string> d)
		{
			if (d["alerte"].Length == 0)
			{
				d["alerte_type"] = "";
			}
			return (string)null;
		};
		return pageReglages;
	}

	private PageReglages PageBoutique(Dictionary<string, string> init)
	{
		PageReglages pageReglages = new PageReglages("Boutique · VIP", "La Boutique de l'accueil et l'offre VIP.", init);
		pageReglages.Section("Boutique");
		pageReglages.Bascule("boutique", "Boutique visible", "Affiche la tuile « Boutique » sur l'accueil de tous les joueurs.", "", "0", defaut: true);
		pageReglages.Section("VIP");
		pageReglages.Texte("vip_lien", "Lien d'achat du VIP à 5 € (https://… : PayPal, Ko-fi, Stripe…)", "https://", 300);
		Champ tNbVip = pageReglages.Texte(null, "Nombre de codes VIP à créer (1 à 500)", "5", 3);
		tNbVip.Text = "5";
		pageReglages.Action("Créer les codes VIP", C.Or, out var lc).Click += delegate
		{
			int nbVip;
			if (!int.TryParse(tNbVip.Text.Trim(), out nbVip) || nbVip < 1 || nbVip > 500)
			{
				lc.ForeColor = C.Rouge;
				lc.Text = "Indiquez un nombre de codes entre 1 et 500.";
				return;
			}
			if (!Social.Actif || !Social.CleAdminPresente)
			{
				lc.ForeColor = C.Rouge;
				lc.Text = "Serveur ou clé administrateur absents (onglet Communauté).";
			}
			else
			{
				lc.ForeColor = C.Gris;
				lc.Text = "Création de " + nbVip + " codes VIP…";
				Thread thread2 = new Thread((ThreadStart)delegate
				{
					string err = null;
					List<string> codes = null;
					try
					{
						codes = Social.CodesVip(nbVip, "lanceur", out err);
					}
					catch (Exception ex2)
					{
						err = ex2.Message;
					}
					try
					{
						BeginInvoke((MethodInvoker)delegate
						{
							if (codes == null)
							{
								lc.ForeColor = C.Rouge;
								lc.Text = "Codes impossibles : " + err;
							}
							else
							{
								string text2 = string.Join("\r\n", codes.ToArray());
								try
								{
									Clipboard.SetText(text2);
								}
								catch
								{
								}
								lc.ForeColor = C.Vert;
								lc.Text = "✔  " + codes.Count + ((codes.Count > 1) ? " codes copiés" : " code copié") + " dans le presse-papiers.";
								string fichierVip = null;
								try
								{
									fichierVip = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Codes VIP " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".txt");
									File.WriteAllText(fichierVip, text2 + "\r\n");
								}
								catch
								{
									fichierVip = null;
								}
								string apercuVip = ((codes.Count <= 20) ? text2 : (string.Join("\r\n", codes.GetRange(0, 20).ToArray()) + "\r\n… et " + (codes.Count - 20) + " autres"));
								MessageBox.Show(this, "Codes VIP à usage unique :\n\n" + apercuVip + "\n\nIls sont copiés dans le presse-papiers" + ((fichierVip != null) ? (" et enregistrés sur le Bureau :\n" + Path.GetFileName(fichierVip)) : ".") + "\n\nIls ne seront plus affichés : gardez-les précieusement.", "Codes VIP");
							}
						});
					}
					catch
					{
					}
				});
				thread2.IsBackground = true;
				thread2.Start();
			}
		};
		Champ tv = pageReglages.TexteLibre("Vérifier l'identifiant VIP d'un joueur (rôle Discord)", "identifiant à 16 caractères", 40);
		pageReglages.Action("Vérifier", C.Or, out var lv).Click += delegate
		{
			if (!Social.Actif || !Social.CleAdminPresente)
			{
				lv.ForeColor = C.Rouge;
				lv.Text = "Serveur ou clé administrateur absents (onglet Communauté).";
			}
			else
			{
				string id = tv.Text.Trim();
				lv.ForeColor = C.Gris;
				lv.Text = "Vérification…";
				Thread thread = new Thread((ThreadStart)delegate
				{
					string r;
					try
					{
						r = Social.VerifierVip(id);
					}
					catch (Exception ex)
					{
						r = "✖  " + ex.Message;
					}
					try
					{
						BeginInvoke((MethodInvoker)delegate
						{
							lv.ForeColor = (r.StartsWith("✔") ? C.Vert : C.Rouge);
							lv.Text = r;
						});
					}
					catch
					{
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		};
		pageReglages.Verif = delegate(Dictionary<string, string> d)
		{
			string text = d["vip_lien"];
			Uri result;
			return (text.Length <= 0 || (Uri.TryCreate(text, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeHttps)) ? null : "Lien d'achat VIP invalide : il doit commencer par https://";
		};
		return pageReglages;
	}

	private PageReglages PageCommunaute(Dictionary<string, string> init)
	{
		PageReglages pageReglages = new PageReglages("Communauté", "Discord, serveur social et tickets de support.", init);
		pageReglages.Section("Discord");
		pageReglages.Texte("discord_app", "Identifiant d'application (discord.com/developers/applications → Application ID)", "ex. 123456789012345678", 22);
		pageReglages.Texte("discord_invite", "Lien d'invitation permanent (https://discord.gg/…)", "https://discord.gg/…", 80);
		pageReglages.Section("Serveur social");
		pageReglages.Texte("serveur", "Adresse du serveur (likes, commentaires, amis, VIP)", "https://….workers.dev", 120);
		Champ tc = pageReglages.TexteLibre("Clé administrateur (gardée sur ce PC, jamais publiée)", Social.CleAdminPresente ? "déjà enregistrée sur ce PC" : "collez la clé ou importez cle-admin.txt", 80);
		Label lk;
		Bouton bouton = pageReglages.Action("Enregistrer la clé", C.Jade, out lk);
		bouton.Click += delegate
		{
			if (tc.Text.Trim().Length == 0)
			{
				lk.ForeColor = C.Gris;
				lk.Text = "Rien à enregistrer.";
			}
			else
			{
				Social.DefinirCleAdmin(tc.Text);
				tc.Text = "";
				lk.ForeColor = C.Vert;
				lk.Text = "✔  Clé enregistrée sur ce PC.";
			}
		};
		pageReglages.Action("Importer cle-admin.txt…", C.Jade, out var li).Click += delegate
		{
			ImportCle.Lancer(FindForm(), li);
		};
		pageReglages.Section("Tickets de support");
		pageReglages.Note("Salon Discord des tickets : Paramètres du salon › Intégrations › Webhooks › Copier l'URL. L'adresse reste sur le serveur, jamais dans le catalogue.");
		Champ tw = pageReglages.TexteLibre("URL du webhook", "https://discord.com/api/webhooks/…", 200);
		Label lw;
		Bouton bouton2 = pageReglages.Action("Enregistrer le webhook", C.Jade, out lw);
		Action etat = delegate
		{
			Thread thread2 = new Thread((ThreadStart)delegate
			{
				bool? b = Social.WebhookRegle();
				try
				{
					BeginInvoke((MethodInvoker)delegate
					{
						lw.ForeColor = ((b == true) ? C.Vert : C.Gris);
						lw.Text = ((b == true) ? "✔  Tickets actifs : ils arrivent dans votre salon Discord." : ((b == false) ? "Aucun salon réglé." : "État inconnu (serveur ou clé absents)."));
					});
				}
				catch
				{
				}
			});
			thread2.IsBackground = true;
			thread2.Start();
		};
		bouton2.Click += delegate
		{
			string u = tw.Text.Trim();
			lw.ForeColor = C.Gris;
			lw.Text = "Enregistrement…";
			Thread thread = new Thread((ThreadStart)delegate
			{
				string e = Social.ReglerWebhook(u);
				try
				{
					BeginInvoke((MethodInvoker)delegate
					{
						if (e != null)
						{
							lw.ForeColor = C.Rouge;
							lw.Text = "✖  " + e;
						}
						else
						{
							tw.Text = "";
							etat();
						}
					});
				}
				catch
				{
				}
			});
			thread.IsBackground = true;
			thread.Start();
		};
		pageReglages.HandleCreated += delegate
		{
			etat();
		};
		pageReglages.Verif = delegate(Dictionary<string, string> d)
		{
			if (d["discord_app"].Length > 0 && !Regex.IsMatch(d["discord_app"], "^\\d{15,22}$"))
			{
				return "Identifiant d'application invalide (15 à 22 chiffres).";
			}
			if (d["discord_invite"].Length > 0 && !Regex.IsMatch(d["discord_invite"], "^https://(discord\\.gg|discord\\.com/invite)/[A-Za-z0-9-]{2,40}$"))
			{
				return "Lien d'invitation invalide : https://discord.gg/…";
			}
			d["serveur"] = d["serveur"].TrimEnd('/');
			return (d["serveur"].Length > 0 && !Regex.IsMatch(d["serveur"], "^https://[A-Za-z0-9.-]+(:\\d{2,5})?$")) ? "Adresse du serveur invalide : https://nom-du-serveur.workers.dev" : null;
		};
		return pageReglages;
	}

	private PageReglages PageMaintenance(Dictionary<string, string> init)
	{
		PageReglages pageReglages = new PageReglages("Maintenance", "Suspendre les installations et imposer une version minimale du lanceur.", init);
		pageReglages.Section("Mode maintenance");
		pageReglages.Bascule("maintenance", "Mode maintenance", "Suspend les installations et mises à jour des mods pour tous les joueurs (le jeu reste jouable).", "1", "", defaut: false);
		pageReglages.Texte("maintenance_msg", "Message affiché pendant la maintenance", "ex. Mise à jour du catalogue en cours, revenez dans une heure", 160);
		pageReglages.Section("Mises à jour du lanceur");
		pageReglages.Texte("version_min", "Version minimale : les versions plus anciennes se mettent à jour d'office (ex. 2.6.0, vide = aucune)", "vide = aucune", 12);
		pageReglages.Verif = delegate(Dictionary<string, string> d)
		{
			if (d["version_min"].Length > 0 && !Regex.IsMatch(d["version_min"], "^\\d+\\.\\d+\\.\\d+$"))
			{
				return "Version minimale invalide (format 2.6.0).";
			}
			if (d["maintenance"] != "1")
			{
				d["maintenance_msg"] = "";
			}
			return (string)null;
		};
		return pageReglages;
	}

	private void GererJeux()
	{
		try
		{
			string text;
			Dictionary<string, object> dictionary = GitHub.LireCatalogue(out text);
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			if ((dictionary.TryGetValue("jeux_liste", out var value) || dictionary.TryGetValue("jeux", out value)) && value is ArrayList)
			{
				foreach (object item in (ArrayList)value)
				{
					if (!(item is Dictionary<string, object> dictionary2))
					{
						continue;
					}
					Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
					foreach (KeyValuePair<string, object> item2 in dictionary2)
					{
						dictionary3[item2.Key] = string.Concat(item2.Value);
					}
					list.Add(dictionary3);
				}
			}
			using FenJeux fenJeux = new FenJeux(list);
			if (fenJeux.Afficher(this) != DialogResult.OK)
			{
				return;
			}
			ArrayList arrayList = new ArrayList();
			foreach (Dictionary<string, string> item3 in fenJeux.Jeux)
			{
				Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
				foreach (KeyValuePair<string, string> item4 in item3)
				{
					if (item4.Value.Length > 0)
					{
						dictionary4[item4.Key] = item4.Value;
					}
				}
				arrayList.Add(dictionary4);
			}
			dictionary.Remove("jeux");
			if (arrayList.Count > 0)
			{
				dictionary["jeux_liste"] = arrayList;
			}
			else
			{
				dictionary.Remove("jeux_liste");
			}
			GitHub.EcrireCatalogue(dictionary, text, "Jeux modifiés depuis le lanceur par " + ((Comptes.Actif != null) ? Comptes.Actif.Pseudo : "admin"));
			lEtat.ForeColor = C.Vert;
			lEtat.Text = "✔  Jeux publiés (" + fenJeux.Jeux.Count + ")";
		}
		catch (Exception ex)
		{
			Journal.Ecrire("admin-jeux", ex);
			MessageBox.Show(this, "Publication impossible :\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void Communaute()
	{
		try
		{
			string text;
			Dictionary<string, object> dictionary = GitHub.LireCatalogue(out text);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			if (dictionary.TryGetValue("communaute", out var value) && value is Dictionary<string, object>)
			{
				foreach (KeyValuePair<string, object> item in (Dictionary<string, object>)value)
				{
					dictionary2[item.Key] = string.Concat(item.Value);
				}
			}
			using FenCommunaute fenCommunaute = new FenCommunaute(dictionary2);
			if (fenCommunaute.Afficher(this) != DialogResult.OK)
			{
				return;
			}
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			foreach (KeyValuePair<string, string> valeur in fenCommunaute.Valeurs)
			{
				if (valeur.Value.Length > 0)
				{
					dictionary3[valeur.Key] = valeur.Value;
				}
			}
			if (dictionary3.Count > 0)
			{
				dictionary["communaute"] = dictionary3;
			}
			else
			{
				dictionary.Remove("communaute");
			}
			GitHub.EcrireCatalogue(dictionary, text, "Activité Discord modifiée depuis le lanceur par " + ((Comptes.Actif != null) ? Comptes.Actif.Pseudo : "admin"));
			lEtat.ForeColor = C.Vert;
			lEtat.Text = "✔  Activité Discord publiée";
		}
		catch (Exception ex)
		{
			Journal.Ecrire("admin-communaute", ex);
			MessageBox.Show(this, "Publication impossible :\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private string Verifier()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Dictionary<string, object> page in pages)
		{
			string text = Str(page, "id");
			string text2 = Str(page, "nom");
			if (!Regex.IsMatch(text, "^[a-z0-9][a-z0-9_-]{1,39}$"))
			{
				return "Identifiant invalide pour « " + text2 + " » : lettres minuscules, chiffres, - et _ (2 à 40 caractères).";
			}
			if (!hashSet.Add(text))
			{
				return "Identifiant en double : " + text;
			}
			if (text2.Trim().Length < 2)
			{
				return "Une page n'a pas de nom (identifiant " + text + ").";
			}
			if (text.StartsWith("nouvelle-page-"))
			{
				return "Renommez l'identifiant de la page « " + text2 + " » avant de publier.";
			}
			if (Projet.LireLiens(Str(page, "liens")).Count != Str(page, "liens").Replace("\r", "").Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Length)
			{
				return "Liens utiles de « " + text2 + " » : une ligne par lien « Libellé | https://… », 3 au maximum.";
			}
			if (Str(page, "ancien_prix").Length > 0 && (!Projet.ParsePrix(Str(page, "ancien_prix"), out var d) || !Projet.ParsePrix(Str(page, "prix"), out var d2) || d <= d2))
			{
				return "Ancien prix de « " + text2 + " » : il doit être supérieur au prix actuel (mod payant).";
			}
			if (Str(page, "vip") == "avance" && Str(page, "public_le") == "")
			{
				return "La page « " + text2 + " » est en accès anticipé VIP sans date d'ouverture à tous (jj/mm/aaaa et hh:mm).";
			}
			string text3 = Str(page, "statut");
			if (text3 == "programme" && Str(page, "sortie") == "")
			{
				return "La page « " + text2 + " » est « À date précise » sans date valide (jj/mm/aaaa et hh:mm).";
			}
			if (Str(page, "prix").Length > 0)
			{
				if (!Projet.ParsePrix(Str(page, "prix"), out var _))
				{
					return "Prix invalide pour « " + text2 + " » (de 0,50 à 999 €).";
				}
				if (!Uri.TryCreate(Str(page, "lien_achat"), UriKind.Absolute, out var result) || result.Scheme != Uri.UriSchemeHttps)
				{
					return "« " + text2 + " » est payant : indiquez une page d'achat en https://.";
				}
				if (Str(page, "acces_sha256").Length != 64)
				{
					return "« " + text2 + " » est payant : définissez un code d'accès à remettre aux acheteurs.";
				}
			}
		}
		return null;
	}

	private void PublierPaquet()
	{
		if (cur == null)
		{
			return;
		}
		if (nouvelles.Contains(cur))
		{
			MessageBox.Show(this, "Publiez d'abord cette nouvelle page (« Enregistrer et publier »), puis envoyez son paquet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		if (modifiees.Count + supprimees.Count > 0)
		{
			MessageBox.Show(this, "Des modifications ne sont pas encore publiées : cliquez d'abord sur « Enregistrer et publier ».", Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		string id = Str(cur, "id");
		using (FenPaquet fenPaquet = new FenPaquet(id, Str(cur, "nom"), Str(cur, "version"), Str(cur, "notes")))
		{
			if (fenPaquet.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}
		}
		lEtat.ForeColor = C.Vert;
		lEtat.Text = "✔  Paquet publié sur GitHub";
		Charger();
	}

	private void Publier()
	{
		if (modifiees.Count + supprimees.Count == 0)
		{
			lEtat.ForeColor = C.Gris;
			lEtat.Text = "Rien à publier.";
			return;
		}
		string text = Verifier();
		if (text != null)
		{
			MessageBox.Show(this, text, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		List<string> list = new List<string>();
		foreach (Dictionary<string, object> modifiee in modifiees)
		{
			list.Add(Str(modifiee, "nom"));
		}
		if (MessageBox.Show(this, "Publier sur GitHub (visible par tous les joueurs) ?\n\nModifiées : " + ((list.Count > 0) ? string.Join(", ", list.ToArray()) : "aucune") + "\nRetirées : " + supprimees.Count, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
		{
			return;
		}
		base.UseWaitCursor = true;
		lEtat.ForeColor = C.Gris;
		lEtat.Text = "Publication…";
		Application.DoEvents();
		try
		{
			foreach (Dictionary<string, object> item in new List<Dictionary<string, object>>(galModif))
			{
				if (!galeries.TryGetValue(item, out var value))
				{
					continue;
				}
				ArrayList arrayList = new ArrayList();
				foreach (ItemGal item2 in value)
				{
					if (item2.Fichier.Length > 0)
					{
						item2.Url = GitHub.EnvoyerImageGalerie(item2.Fichier, Str(item, "id"));
						item2.Fichier = "";
					}
					if (item2.Url.Length > 0)
					{
						arrayList.Add(item2.Url);
					}
				}
				if (arrayList.Count > 0)
				{
					item["galerie"] = arrayList;
				}
				else
				{
					item.Remove("galerie");
				}
			}
			foreach (KeyValuePair<string, string> kv2 in new Dictionary<string, string>(videosEnAttente))
			{
				List<Dictionary<string, object>> list2 = pages;
				Predicate<Dictionary<string, object>> match = delegate(Dictionary<string, object> x)
				{
					string text6 = Str(x, "id");
					KeyValuePair<string, string> keyValuePair7 = kv2;
					return text6 == keyValuePair7.Key;
				};
				Dictionary<string, object> dictionary = list2.Find(match);
				if (dictionary != null)
				{
					KeyValuePair<string, string> keyValuePair = kv2;
					string value2 = keyValuePair.Value;
					KeyValuePair<string, string> keyValuePair2 = kv2;
					dictionary["video"] = GitHub.EnvoyerVideo(value2, keyValuePair2.Key, delegate(string t)
					{
						lEtat.Text = t;
						Application.DoEvents();
					});
					Dictionary<string, string> dictionary2 = videosEnAttente;
					KeyValuePair<string, string> keyValuePair3 = kv2;
					dictionary2.Remove(keyValuePair3.Key);
				}
			}
			foreach (KeyValuePair<string, string> kv in new Dictionary<string, string>(imagesEnAttente))
			{
				List<Dictionary<string, object>> list3 = pages;
				Predicate<Dictionary<string, object>> match2 = delegate(Dictionary<string, object> x)
				{
					string text5 = Str(x, "id");
					KeyValuePair<string, string> keyValuePair6 = kv;
					return text5 == keyValuePair6.Key;
				};
				Dictionary<string, object> dictionary3 = list3.Find(match2);
				if (dictionary3 != null)
				{
					KeyValuePair<string, string> keyValuePair4 = kv;
					string value3 = keyValuePair4.Value;
					KeyValuePair<string, string> keyValuePair5 = kv;
					dictionary3["image"] = GitHub.EnvoyerImage(value3, keyValuePair5.Key);
				}
			}
			string text2;
			Dictionary<string, object> dictionary4 = (cat = GitHub.LireCatalogue(out text2));
			ArrayList arrayList2 = (dictionary4.ContainsKey("projets") ? (dictionary4["projets"] as ArrayList) : null);
			if (arrayList2 == null)
			{
				arrayList2 = new ArrayList();
			}
			ArrayList arrayList3 = new ArrayList();
			foreach (Dictionary<string, object> page in pages)
			{
				Dictionary<string, object> dictionary5 = null;
				foreach (object item3 in arrayList2)
				{
					if (item3 is Dictionary<string, object> dictionary6 && Str(dictionary6, "id") == Str(page, "id"))
					{
						dictionary5 = dictionary6;
					}
				}
				if (dictionary5 != null && modifiees.Contains(page))
				{
					string[] array = new string[30]
					{
						"nom", "sous_titre", "description", "notes", "consigne", "categorie", "statut", "sortie", "a_la_une", "ordre",
						"image", "jeu", "galerie", "prix", "lien_achat", "acces_sha256", "vip", "public_le", "prix_points", "editeur",
						"prerequis", "ruban", "video", "liens", "ancien_prix", "commentaires", "nouveautes", "lies", "tags", "dlc"
					};
					foreach (string key in array)
					{
						if (page.ContainsKey(key))
						{
							dictionary5[key] = page[key];
						}
						else
						{
							dictionary5.Remove(key);
						}
					}
					arrayList3.Add(dictionary5);
				}
				else
				{
					arrayList3.Add((dictionary5 != null) ? dictionary5 : page);
				}
			}
			foreach (object item4 in arrayList2)
			{
				if (!(item4 is Dictionary<string, object> dictionary7))
				{
					continue;
				}
				string text3 = Str(dictionary7, "id");
				bool flag = supprimees.Contains(text3);
				foreach (Dictionary<string, object> page2 in pages)
				{
					if (Str(page2, "id") == text3)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					arrayList3.Add(dictionary7);
				}
			}
			dictionary4["projets"] = arrayList3;
			string text4 = ((Comptes.Actif != null) ? Comptes.Actif.Pseudo : "admin");
			GitHub.EcrireCatalogue(dictionary4, text2, "Catalogue modifié depuis le lanceur par " + text4 + " (" + modifiees.Count + " page(s) modifiée(s), " + supprimees.Count + " retirée(s))");
			modifiees.Clear();
			nouvelles.Clear();
			supprimees.Clear();
			imagesEnAttente.Clear();
			lEtat.ForeColor = C.Vert;
			lEtat.Text = "✔  Publié sur GitHub";
			Charger();
		}
		catch (Exception ex)
		{
			lEtat.ForeColor = C.Rouge;
			lEtat.Text = "Échec : " + ex.Message;
			Journal.Ecrire("admin-publier", ex);
			MessageBox.Show(this, "La publication a échoué :\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			base.UseWaitCursor = false;
			entete.Invalidate();
		}
	}
}
