using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TPSStudios;

internal partial class Fenetre : Form
{
	private enum Vue
	{
		Accueil,
		Jeux,
		Biblio,
		Projet
	}

	private class Diapo
	{
		public string Tag = "";

		public string Titre = "";

		public string Sous = "";

		public string Detail = "";

		public string Cle = "";

		public Color Couleur = C.Jade;

		public Image Img;

		public Action Ouvrir;

		public string Libelle = "";

		public Bitmap Cache;

		public Size CacheTaille;
	}

	private class Annonce
	{
		public string Titre = "";

		public string Texte = "";

		public string Glyphe = "\ue789";

		public Color Couleur = C.Jade;

		public Projet Mod;
	}

	private enum E
	{
		Rien,
		AJour,
		MAJ,
		SansJeu,
		Lance
	}

	public const int TailleW = 1280;

	public const int TailleH = 760;

	private const double DureeDiapo = 6.5;

	private const string DiscordTPS = "https://discord.gg/dA77S5nXVV";

	private Catalogue cat;

	private string erreurCat;

	private Projet sel;

	private Panel cote;

	private Panel liste;

	private Panel page;

	private Panel notesP;

	private Panel descP;

	private Image logo;

	private Image banniere;

	private ToolTip bulle;

	private Label lDos;

	private Label lTitre;

	private Label lSous;

	private Label lEtat;

	private Label lProg;

	private Label lNotes;

	private Label lDesc;

	private Label lBas;

	private Label lAlerte;

	private Bouton bAction;

	private Bouton bDesinst;

	private Bouton bDossier;

	private Bouton bVerif;

	private Bouton bParam;

	private Bouton bOngDesc;

	private Bouton bOngNotes;

	private PuceCompte puce;

	private Vue vue;

	private Panel zone;

	private PanelFond pAccueil;

	private PanelFond pJeux;

	private PanelFond pBiblio;

	private NavItem nAccueil;

	private NavItem nJeux;

	private NavItem nBiblio;

	private Bouton bRetour;

	private Bouton bChangerJeu;

	private Bouton bChemins;

	private Bouton fTous;

	private Bouton fInst;

	private Bouton fMaj;

	private Champ tRecherche;

	private List<Tuile> tuilesA = new List<Tuile>();

	private List<Tuile> tuilesJ = new List<Tuile>();

	private List<Tuile> tuilesB = new List<Tuile>();

	private bool premiereCharge = true;

	private Bouton bRetJeux;

	private string jeuId = "omsi2";

	private string categorie = "";

	private int filtre;

	private int yVide = 200;

	private List<Bouton> chips = new List<Bouton>();

	private bool plein;

	private FormWindowState avantPlein;

	public static bool ModeCapture;

	private Barre barre;

	private List<Entree> entrees = new List<Entree>();

	private Dictionary<string, Image> images = new Dictionary<string, Image>();

	private bool occupe;

	private System.Windows.Forms.Timer minuteur;

	public static string ActionAuto;

	public static int Rouvrir = -1;

	public static bool PleinAuDemarrage;

	public static Fenetre Principale;

	public static bool ForceAnim;

	private List<PanelFond> couches = new List<PanelFond>();

	private bool enTransition;

	private Avis avis = new Avis();

	private System.Windows.Forms.Timer tAvis;

	private Bouton bAime;

	private Panel comP;

	private ZoneDessin comListe;

	private Champ comSaisie;

	private Bouton comPub;

	private Label comInfo;

	private EtatSocial soc = new EtatSocial();

	private FluxSocial flux;

	private string socMod = "";

	private bool socEnLigne;

	private bool garderScroll;

	private int comH = 90;

	private Rectangle rSocial;

	private List<Rectangle> rSuppr = new List<Rectangle>();

	private List<int> idSuppr = new List<int>();

	private System.Windows.Forms.Timer tTel;

	private System.Windows.Forms.Timer tHeure;

	private static readonly Font fComNom = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fComTxt = new Font("Segoe UI", Zx.P(10f));

	private int vIdx;

	private Rectangle rVue;

	private Rectangle rVPrev;

	private Rectangle rVNext;

	private Rectangle rAchat;

	private Rectangle rInfo;

	private List<Rectangle> rMini = new List<Rectangle>();

	private Bouton bCode;

	private bool modeAchat;

	private Bitmap vueCache;

	private Image vueSrc;

	private Size vueTaille;

	private bool relayoutPage;

	private static readonly Font fPrix = new Font("Segoe UI Semibold", Zx.P(24f));

	private static readonly Font fFil = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fCarteT = new Font("Segoe UI Semibold", Zx.P(9f));

	private static readonly Font fLigne = new Font("Segoe UI", Zx.P(10f));

	private List<KeyValuePair<Rectangle, string>> rLiens = new List<KeyValuePair<Rectangle, string>>();

	private List<KeyValuePair<Rectangle, string>> rTags = new List<KeyValuePair<Rectangle, string>>();

	private Rectangle rBVideo;

	private static readonly Font fBVideo = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private Bouton bLies;

	private Bouton bAccueilP;

	private Bouton bAccueilB;

	private Bouton bSupport;

	public static bool VersAccueil;

	private Bitmap heroCache;

	private Image heroSrc;

	private int heroW;

	private int heroH;

	private static int pasImage = -1;

	public string StatJeu = "";

	public long StatSnap;

	private Bitmap logoMarque;

	private bool passeIntro;

	private bool sortie;

	private string raisonPasse = "";

	private DateTime introDebut = DateTime.Now;

	public static string IntroDossier;

	private static readonly Font fMarque = new Font("Segoe UI Black", Zx.P(24f));

	private int neigeTrans;

	private static Dictionary<string, Dictionary<string, string>> JeuxWebParId = new Dictionary<string, Dictionary<string, string>>();

	private static Dictionary<string, Dictionary<string, string>> FichesJeux = new Dictionary<string, Dictionary<string, string>>();

	private static readonly Font fTitre = new Font("Segoe UI Semibold", Zx.P(26f));

	private static readonly Font fTitreG = new Font("Segoe UI Semibold", Zx.P(30f));

	private static readonly Font fSousT = new Font("Segoe UI", Zx.P(11f));

	private static readonly Font fSousG = new Font("Segoe UI", Zx.P(12f));

	private static readonly Font fAlerte = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private FondAnime fond = new FondAnime();

	private System.Windows.Forms.Timer animation;

	private Stopwatch chrono = Stopwatch.StartNew();

	private long dernierTick;

	private string txtBonjour = "";

	private string txtMaj = "";

	private string txtVide = "";

	private Color colMaj = C.Jade;

	private Rectangle rMaj;

	private bool periodeHaute;

	private int[] etatsTuiles = new int[0];

	private static readonly Font fVersion = new Font("Segoe UI", Zx.P(9f));

	private Bitmap enteteCache;

	private string enteteCle = "";

	private Rectangle rJeuB;

	private int xVide = 42;

	private static readonly Font fJeuNom = new Font("Segoe UI Semibold", Zx.P(16f));

	private static readonly Font fJeuTxt = new Font("Segoe UI", Zx.P(9.5f));

	private List<Diapo> diapos = new List<Diapo>();

	private int dIdx;

	private int dPrec = -1;

	private int dDir = 1;

	private double dTemps;

	private double dTrans = 1.0;

	private bool dSurvol;

	private bool popDemandee;

	private Rectangle rProfil;

	private Rectangle rCarte;

	private Rectangle rPrev;

	private Rectangle rNext;

	private List<Rectangle> rPuces = new List<Rectangle>();

	private Dictionary<string, int> telecharg = new Dictionary<string, int>();

	private double dLeve;

	private Point dSouris;

	private Bitmap ombreCarte;

	private int cleOmbre;

	private Bitmap logoCache;

	private int logoCacheTaille;

	private Rectangle rHorloge;

	private static readonly Font fHeure = new Font("Segoe UI Semibold", Zx.P(22f));

	private static readonly Font fDate = new Font("Segoe UI", Zx.P(17f));

	private static readonly Font fTitreA = new Font("Segoe UI Semibold", Zx.P(58f));

	private static readonly Font fSousA = new Font("Segoe UI Semibold", Zx.P(16f));

	private Bitmap cacheProfil;

	private string cleProfil = "";

	private static readonly Font fPseudo = new Font("Segoe UI Semibold", Zx.P(15.5f));

	private static readonly Font fNiv = new Font("Segoe UI Semibold", Zx.P(10f));

	private static readonly Font fPetit = new Font("Segoe UI", Zx.P(8.5f));

	private static readonly Font fStat = new Font("Segoe UI Semibold", Zx.P(9.5f));

	private Rectangle rAppui = Rectangle.Empty;

	private bool appuiBas;

	private bool appuiDiscord;

	private Ressort appuiR = new Ressort
	{
		V = 1f
	};

	private Rectangle rAmisA;

	private Rectangle rAmisPlus;

	private bool amPlusSurvol;

	private bool amisCharges;

	private static string modsEnvoyes = "";

	private DateTime dernierAmis = DateTime.MinValue;

	private static readonly Font fAmN = new Font("Segoe UI Semibold", Zx.P(9.5f));

	private static readonly Font fAmS = new Font("Segoe UI", Zx.P(8f));

	private static readonly Font fAmI = new Font("Segoe UI Semibold", Zx.P(10f));

	private static readonly Font fAmP = new Font("Segoe UI", Zx.P(15f));

	private bool ficheSurvol;

	private double ficheAp;

	private Bitmap ficheBmp;

	private Rectangle rFiche;

	private static readonly Font fFNom = new Font("Segoe UI Semibold", Zx.P(17f));

	private static readonly Font fFStat = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fFTxt = new Font("Segoe UI", Zx.P(10f));

	private static readonly Font fFNiv = new Font("Segoe UI Semibold", Zx.P(11f));

	private static readonly Font fFRub = new Font("Segoe UI Semibold", Zx.P(8.5f));

	private static readonly Font fFPetit = new Font("Segoe UI", Zx.P(8.5f));

	private long ficheTick = -1000L;

	private Bitmap vivant;

	private Bitmap banDemi;

	private long vivantTick = -1L;

	public static Dictionary<string, double> ProfilFps = null;

	public static string LienVip = "";

	public static string InviteVip = "";

	public static List<Projet> ProjetsConnus = new List<Projet>();

	private NotifyIcon icone;

	private FenTray miniTray;

	private Rectangle rCloche;

	private bool clSurvol;

	private int tickNotif;

	private bool vipLance;

	private Bouton bSuivre;

	private static readonly Font fCloche = new Font("Segoe MDL2 Assets", Zx.P(15f));

	private static readonly Font fCompte = new Font("Segoe UI Semibold", Zx.P(8f));

	private Rectangle rPilule;

	private Rectangle rVolBar;

	private bool piSurvol;

	private bool volOuvert;

	private bool volGlisse;

	private double piAp;

	private List<KeyValuePair<Rectangle, Action>> rPiBt = new List<KeyValuePair<Rectangle, Action>>();

	private static readonly Font fPiT = new Font("Segoe UI Semibold", Zx.P(9.5f));

	private static readonly Font fPiS = new Font("Segoe UI", Zx.P(8.5f));

	private static readonly Font fPiG = new Font("Segoe MDL2 Assets", Zx.P(11f));

	private static readonly Font fPiG2 = new Font("Segoe MDL2 Assets", Zx.P(13f));

	private Rectangle rMusique;

	private bool muSurvol;

	private static readonly Font fMus = new Font("Segoe MDL2 Assets", Zx.P(15f));

	private Rectangle rInfos;

	private List<string[]> infos = new List<string[]>();

	private int enLigne = -1;

	private int enJeu;

	private string statutEnvoye;

	private DateTime dernierStatut = DateTime.MinValue;

	private List<Annonce> annonces = new List<Annonce>();

	private List<KeyValuePair<Rectangle, Annonce>> rAnnonces = new List<KeyValuePair<Rectangle, Annonce>>();

	private static readonly string[] NouveautesLanceur = new string[4] { "TPS Studios devient TPS Groupe", "Alerte de mise à jour sur l'accueil", "Bandes-annonces YouTube dans les pages", "Lanceur plus fluide et page du jeu sans défilement" };

	private static readonly Font fAnT2 = new Font("Segoe UI Semibold", Zx.P(11f));

	private static readonly Font fAnG2 = new Font("Segoe MDL2 Assets", Zx.P(15f));

	private static readonly Font fAnT = new Font("Segoe UI Semibold", Zx.P(9.5f));

	private static readonly Font fAnX = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fAnG = new Font("Segoe MDL2 Assets", Zx.P(11f));

	private Rectangle rBandeau;

	private string cleInfosBmp = "";

	private List<KeyValuePair<Rectangle, Annonce>> rAnnoncesCache = new List<KeyValuePair<Rectangle, Annonce>>();

	private Bitmap infosBmp;

	private Bitmap ombreInfos;

	private int cleOmbreI;

	private double iLeve;

	private bool iSurvol;

	private Rectangle rDiscord;

	private bool dcSurvol;

	private int dcMoitie;

	private ThemeBase noel;

	private ThemeBase themeJeux;

	private string idTheme;

	private static readonly Font fDisc1 = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fDisc2 = new Font("Segoe UI", Zx.P(8.5f));

	private bool quitterVraiment;

	public static int MsgMontrer;

	private Dictionary<string, Image> couvJeux = new Dictionary<string, Image>();

	private string jeuWebProfil;

	private Bouton bPhotosJeu;

	private Bouton bVideoJeu;

	private Bouton bJouerOmsi;

	private Projet jeuEnCours;

	private DateTime debutJeu;

	private System.Windows.Forms.Timer minPresence;

	private int tickStats;

	private Panel galP;

	private Bouton bOngGal;

	private List<Vignette> vignettes = new List<Vignette>();

	private Dictionary<string, List<Image>> galImages = new Dictionary<string, List<Image>>();

	private HashSet<string> galEnCours = new HashSet<string>();

	private E etat;

	private WebClient dlEnCours;

	private System.Windows.Forms.Timer dlMinuteur;

	private bool dlAnnule;

	private bool dlSecoursEssaye;

	private bool dlDirectEssaye;

	public static bool Fluide => AnimationActive();

	private bool Maintenance => Opt("maintenance") == "1";

	private bool themeNoel => noel != null;

	private int DiscordCoupe => rDiscord.X + 150;

	public void Toast(string t, Color c, int ms)
	{
		if (base.IsDisposed || !base.IsHandleCreated)
		{
			return;
		}
		avis.Poser(t, c);
		avis.Location = new Point(Math.Max(0, (base.ClientSize.Width - avis.Width) / 2), base.ClientSize.Height - avis.Height - Zx.P(40));
		avis.Visible = true;
		avis.BringToFront();
		if (tAvis == null)
		{
			tAvis = new System.Windows.Forms.Timer();
			tAvis.Tick += delegate
			{
				tAvis.Stop();
				avis.Visible = false;
			};
		}
		tAvis.Stop();
		tAvis.Interval = ms;
		tAvis.Start();
	}

	private static EventHandler Protege(string ctx, EventHandler h)
	{
		return delegate(object o, EventArgs e)
		{
			try
			{
				h(o, e);
			}
			catch (Exception e2)
			{
				Journal.Une(ctx, e2);
			}
		};
	}

	private void RetenirTaille()
	{
		try
		{
			if (!ModeCapture && base.WindowState != FormWindowState.Minimized && !plein)
			{
				Size size = ((base.WindowState == FormWindowState.Normal) ? base.ClientSize : (base.RestoreBounds.Size - (base.Size - base.ClientSize)));
				Config.Ecrire("fenetre", size.Width + "," + size.Height + "," + ((base.WindowState == FormWindowState.Maximized) ? "1" : "0"));
			}
		}
		catch
		{
		}
	}

	public Fenetre()
	{
		Principale = this;
		EventHandler value = delegate
		{
			if (Principale == this)
			{
				Principale = null;
			}
		};
		base.Disposed += value;
		Progression.Notification += delegate(string nt, string nd, Color nc)
		{
			Sur(delegate
			{
				Toast(nt + "  —  " + nd, nc, 6500);
				if (cacheProfil != null)
				{
					cacheProfil.Dispose();
					cacheProfil = null;
				}
				if (pAccueil != null)
				{
					pAccueil.Invalidate();
				}
			});
		};
		if (!ModeCapture)
		{
			base.Opacity = 0.0;
		}
		Text = "TPS Groupe";
		Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
		int val = 1280;
		int val2 = 760;
		bool flag = false;
		try
		{
			string[] array = (Config.Lire("fenetre") ?? "").Split(',');
			if (array.Length >= 2 && int.TryParse(array[0], out var result) && int.TryParse(array[1], out var result2))
			{
				val = Math.Max(Zx.P(1000), result);
				val2 = Math.Max(Zx.P(640), result2);
				flag = array.Length > 2 && array[2] == "1";
			}
		}
		catch
		{
		}
		base.ClientSize = new Size(Math.Min(val, workingArea.Width - Zx.P(40)), Math.Min(val2, workingArea.Height - Zx.P(60)));
		base.FormBorderStyle = FormBorderStyle.Sizable;
		base.MaximizeBox = true;
		MinimumSize = new Size(Zx.P(1000), Zx.P(640));
		Theme.Fenetre(this);
		if (flag && !ModeCapture)
		{
			base.Shown += delegate
			{
				if (base.WindowState == FormWindowState.Normal)
				{
					base.WindowState = FormWindowState.Maximized;
				}
			};
		}
		base.FormClosing += delegate
		{
			RetenirTaille();
		};
		base.ResizeEnd += delegate
		{
			RetenirTaille();
		};
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.StartPosition = FormStartPosition.CenterScreen;
		DoubleBuffered = true;
		try
		{
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		catch
		{
		}
		logo = Res("logo_tps.jpg");
		banniere = Res("banniere_parisview.jpg");
		if (!ModeCapture)
		{
			icone = new NotifyIcon();
			icone.Icon = base.Icon ?? SystemIcons.Application;
			icone.Text = "TPS Groupe";
			icone.Visible = true;
			EventHandler montrer = delegate
			{
				Reapparaitre();
			};
			icone.DoubleClick += delegate(object o, EventArgs e)
			{
				if (miniTray != null && !miniTray.IsDisposed)
				{
					miniTray.Close();
				}
				montrer(o, e);
			};
			icone.BalloonTipClicked += delegate
			{
				montrer(null, EventArgs.Empty);
				OuvrirNotifs();
			};
			icone.MouseClick += delegate(object o, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left)
				{
					if (miniTray != null && !miniTray.IsDisposed)
					{
						miniTray.Close();
					}
					else
					{
						miniTray = new FenTray(delegate
						{
							montrer(null, EventArgs.Empty);
						}, delegate
						{
							QuitterVraiment();
						});
						miniTray.Show();
						miniTray.Activate();
					}
				}
			};
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip
			{
				ShowImageMargin = false,
				Items = 
				{
					{
						"Ouvrir TPS Groupe",
						(Image)null,
						montrer
					},
					(ToolStripItem)new ToolStripSeparator(),
					{
						"⏯   Lecture / pause",
						(Image)null,
						(EventHandler)delegate
						{
							Musique.Touche(179);
						}
					},
					{
						"⏭   Titre suivant",
						(Image)null,
						(EventHandler)delegate
						{
							Musique.Touche(176);
						}
					},
					{
						"⏮   Titre précédent",
						(Image)null,
						(EventHandler)delegate
						{
							Musique.Touche(177);
						}
					},
					(ToolStripItem)new ToolStripSeparator(),
					{
						"Quitter TPS Groupe",
						(Image)null,
						(EventHandler)delegate
						{
							QuitterVraiment();
						}
					}
				}
			};
			icone.ContextMenuStrip = contextMenuStrip;
			base.FormClosed += delegate
			{
				try
				{
					icone.Visible = false;
					icone.Dispose();
				}
				catch
				{
				}
			};
		}
		Notifs.Nouvelle += delegate(Notif n)
		{
			Sur(delegate
			{
				if (pAccueil != null)
				{
					pAccueil.Invalidate();
				}
				if (icone != null && Config.Oui("notif_windows", defaut: true) && (base.WindowState == FormWindowState.Minimized || Form.ActiveForm != this))
				{
					icone.ShowBalloonTip(7000, Lang.T(n.Titre), Lang.T(n.Texte), ToolTipIcon.Info);
				}
				else
				{
					Toast(n.Titre, (n.Type == "vip") ? C.Or : C.Jade, 6000);
				}
			});
		};
		cote = new Panel();
		cote.Dock = DockStyle.Left;
		cote.Width = Zx.P(260);
		cote.BackColor = C.Cote;
		cote.Paint += delegate(object o, PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			if (logo != null)
			{
				using GraphicsPath clip = Bouton.Rond(new Rectangle(Zx.P(20), Zx.P(20), Zx.P(52), Zx.P(52)), Zx.P(10));
				graphics.SetClip(clip);
				graphics.DrawImage(logo, Zx.P(20), Zx.P(20), Zx.P(52), Zx.P(52));
				graphics.ResetClip();
			}
			Lang.DrawText(graphics, "TPS Groupe", F.Logo, new Point(Zx.P(84), Zx.P(22)), C.Texte);
			Lang.DrawText(graphics, "Lanceur", F.SousLogo, new Point(Zx.P(86), Zx.P(50)), C.Gris);
			Lang.DrawText(graphics, "MES PROJETS", F.Rubrique, new Point(Zx.P(20), Zx.P(250)), C.Gris);
			using Pen pen = new Pen(C.Ligne);
			graphics.DrawLine(pen, Zx.P(20), Zx.P(84), cote.Width - Zx.P(20), Zx.P(84));
			graphics.DrawLine(pen, Zx.P(20), Zx.P(236), cote.Width - Zx.P(20), Zx.P(236));
			graphics.DrawLine(pen, Zx.P(20), cote.Height - Zx.P(172), cote.Width - Zx.P(20), cote.Height - Zx.P(172));
		};
		liste = new Panel();
		liste.Location = new Point(0, Zx.P(274));
		liste.Size = new Size(Zx.P(260), Zx.P(300));
		liste.BackColor = C.Cote;
		liste.AutoScroll = true;
		liste.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
		cote.Controls.Add(liste);
		bParam = new Bouton("⚙  Paramètres");
		bParam.Plein = false;
		bParam.Size = new Size(Zx.P(220), Zx.P(36));
		bParam.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		bParam.Click += delegate
		{
			Parametres();
		};
		cote.Controls.Add(bParam);
		lBas = new Label();
		lBas.AutoSize = false;
		lBas.Size = new Size(Zx.P(220), Zx.P(20));
		lBas.ForeColor = C.Gris;
		lBas.Font = new Font("Segoe UI", Zx.P(8.5f));
		lBas.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		lBas.Text = "Lanceur v" + Config.VERSION;
		cote.Controls.Add(lBas);
		cote.Resize += delegate
		{
			lBas.Location = new Point(Zx.P(22), cote.Height - Zx.P(30));
			lDos.Location = new Point(Zx.P(22), cote.Height - Zx.P(56));
			bParam.Location = new Point(Zx.P(20), cote.Height - Zx.P(100));
			puce.Location = new Point(Zx.P(20), cote.Height - Zx.P(158));
			liste.Height = Math.Max(60, cote.Height - 274 - 172);
		};
		PanelFond panelFond = (PanelFond)(page = new PanelFond());
		page.Dock = DockStyle.Fill;
		page.BackColor = C.Fond;
		panelFond.AutoScroll = true;
		panelFond.Peindre = PeintPage;
		Theme.Sombre(panelFond);
		page.Resize += delegate
		{
			Disposer();
			page.Invalidate();
		};
		InitPage();
		lTitre = L(new Font("Segoe UI Semibold", Zx.P(30f)), C.Texte, auto: true);
		lSous = L(new Font("Segoe UI", Zx.P(12f)), Color.FromArgb(200, 225, 235), auto: true);
		lTitre.BackColor = Color.Transparent;
		lSous.BackColor = Color.Transparent;
		bAction = new Bouton("…");
		bAction.Font = new Font("Segoe UI Semibold", Zx.P(13f));
		bAction.Size = new Size(Zx.P(230), Zx.P(54));
		bAction.Click += delegate
		{
			Agir();
		};
		lEtat = L(new Font("Segoe UI", Zx.P(10f)), C.Gris, auto: false);
		lEtat.Size = new Size(Zx.P(520), Zx.P(54));
		bDesinst = Petit("Désinstaller", delegate
		{
			if (occupe)
			{
				if (dlEnCours != null)
				{
					dlAnnule = true;
					try
					{
						dlEnCours.CancelAsync();
					}
					catch
					{
					}
				}
			}
			else
			{
				Desinstaller();
			}
		});
		bDossier = Petit("Ouvrir le dossier", delegate
		{
			OuvrirDossier();
		});
		bSupport = Petit("?   Support", delegate
		{
			OuvrirSupport();
		});
		bSupport.Font = new Font("Segoe UI Semibold", Zx.P(10f));
		bLies = Petit("Mods liés", delegate
		{
			OuvrirLies();
		});
		bLies.Font = new Font("Segoe UI Semibold", Zx.P(10.5f));
		bCode = Petit("J'ai un code d'accès", delegate
		{
			SaisirCode();
		});
		bCode.Visible = false;
		InitSocial();
		bVerif = Petit("⟳", delegate
		{
			Recharger(manuel: true);
		});
		puce = new PuceCompte();
		puce.Size = new Size(Zx.P(220), Zx.P(48));
		puce.Remplissage = C.Carte;
		puce.Click += delegate
		{
			Profil(Comptes.Actif == null);
		};
		puce.Maj(Comptes.Actif);
		cote.Controls.Add(puce);
		nAccueil = new NavItem("\ue80f", "Accueil");
		nJeux = new NavItem("\ue7fc", "Jeux");
		nBiblio = new NavItem("\ue8f1", "Bibliothèque");
		nAccueil.Location = new Point(0, Zx.P(96));
		nJeux.Location = new Point(0, Zx.P(142));
		nBiblio.Location = new Point(0, Zx.P(188));
		nAccueil.Click += delegate
		{
			ShowVue(Vue.Accueil);
		};
		nJeux.Click += delegate
		{
			ShowVue(Vue.Jeux);
		};
		nBiblio.Click += delegate
		{
			ShowVue(Vue.Biblio);
		};
		cote.Controls.Add(nAccueil);
		cote.Controls.Add(nJeux);
		cote.Controls.Add(nBiblio);
		bRetour = Petit("\ue72b", delegate
		{
			ShowVue(Vue.Biblio);
		});
		bRetour.Font = Integre.fNav;
		bRetour.Size = new Size(Zx.P(48), Zx.P(48));
		bAccueilP = Integre.Accueil(0, 0);
		bOngDesc = Onglet("À propos", delegate
		{
			ChoisirOnglet(0);
		});
		bOngNotes = Onglet("Notes de version", delegate
		{
			ChoisirOnglet(1);
		});
		bOngGal = Onglet("Galerie", delegate
		{
			ChoisirOnglet(2);
		});
		bOngGal.Visible = false;
		galP = new Panel();
		galP.BackColor = C.Carte;
		galP.AutoScroll = true;
		galP.Visible = false;
		Theme.Sombre(galP);
		galP.Resize += delegate
		{
			DisposerGalerie();
		};
		barre = new Barre();
		barre.Visible = false;
		lProg = L(new Font("Segoe UI", Zx.P(9f)), C.Gris, auto: false);
		lProg.Visible = false;
		lAlerte = L(new Font("Segoe UI Semibold", Zx.P(9.5f)), C.Orange, auto: false);
		lAlerte.Visible = false;
		lDos = L(new Font("Segoe UI", Zx.P(9f)), C.Gris, auto: false);
		lDos.Cursor = Cursors.Hand;
		lDos.Click += delegate
		{
			ChoisirDossiers();
		};
		lDos.BackColor = C.Cote;
		lDos.Size = new Size(Zx.P(220), Zx.P(20));
		lDos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		bulle = new ToolTip();
		bulle.SetToolTip(bRetour, "Bibliothèque (Échap)");
		bulle.SetToolTip(bAccueilP, "Accueil");
		notesP = new Panel();
		notesP.BackColor = C.Carte;
		notesP.AutoScroll = true;
		lNotes = L(new Font("Segoe UI", Zx.P(10f)), Color.FromArgb(205, 214, 228), auto: false);
		lNotes.Location = new Point(Zx.P(20), Zx.P(18));
		lNotes.BackColor = C.Carte;
		notesP.Controls.Add(lNotes);
		descP = new Panel();
		descP.BackColor = C.Carte;
		descP.AutoScroll = true;
		lDesc = L(new Font("Segoe UI", Zx.P(10f)), Color.FromArgb(205, 214, 228), auto: false);
		lDesc.Location = new Point(Zx.P(20), Zx.P(18));
		lDesc.BackColor = C.Carte;
		descP.Controls.Add(lDesc);
		cote.Controls.Add(lDos);
		Control[] array2 = new Control[23]
		{
			bRetour, bAccueilP, bLies, bSupport, lTitre, lSous, bAction, bCode, bAime, bSuivre,
			comP, lEtat, bDesinst, bDossier, bOngDesc, bOngNotes, bOngGal, barre, lProg, lAlerte,
			notesP, descP, galP
		};
		foreach (Control value2 in array2)
		{
			page.Controls.Add(value2);
		}
		ChoisirOnglet(notes: false);
		zone = new Panel();
		zone.Dock = DockStyle.Fill;
		zone.BackColor = C.Fond;
		pAccueil = VuePanel(defil: true);
		pJeux = VuePanel(defil: true);
		pBiblio = VuePanel(defil: true);
		page.Visible = false;
		pAccueil.Peindre = PeintAccueil;
		pJeux.Peindre = PeintJeux;
		pBiblio.Peindre = PeintBiblio;
		pAccueil.Resize += delegate
		{
			DisposerAccueil();
		};
		InitAccueil();
		pJeux.Resize += delegate
		{
			DisposerJeux();
		};
		pBiblio.Resize += delegate
		{
			DisposerBiblio();
		};
		zone.Controls.Add(page);
		zone.Controls.Add(pAccueil);
		zone.Controls.Add(pJeux);
		zone.Controls.Add(pBiblio);
		base.Controls.Add(zone);
		base.Controls.Add(avis);
		string value3 = Config.Lire("jeu");
		if (!string.IsNullOrEmpty(value3))
		{
			jeuId = value3;
		}
		base.KeyPreview = true;
		base.FormClosing += delegate(object oc, FormClosingEventArgs ec)
		{
			if (!quitterVraiment && !sortie && ec.CloseReason == CloseReason.UserClosing && !ModeCapture && ActionAuto == null && icone != null && Config.Oui("fermer_reduit", defaut: true))
			{
				ec.Cancel = true;
				CacherDansZone();
			}
			else if (!sortie && ec.CloseReason == CloseReason.UserClosing && !ModeCapture && ActionAuto == null && couches.Count == 0 && !enTransition && Animer())
			{
				ec.Cancel = true;
				sortie = true;
				BeginInvoke((MethodInvoker)delegate
				{
					try
					{
						Outro(null);
					}
					catch (Exception e4)
					{
						Journal.Ecrire("outro", e4);
					}
					Close();
				});
			}
			else
			{
				PanelFond[] array3 = couches.ToArray();
				foreach (PanelFond panelFond2 in array3)
				{
					foreach (Control control in panelFond2.Controls)
					{
						if (control is Form form)
						{
							try
							{
								form.Dispose();
							}
							catch
							{
							}
						}
					}
				}
				DiscordPresence.Arreter();
				if (periodeHaute)
				{
					periodeHaute = false;
					try
					{
						Winmm.timeEndPeriod(1u);
					}
					catch
					{
					}
				}
			}
		};
		base.KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (enTransition)
			{
				if ((DateTime.Now - introDebut).TotalMilliseconds > 350.0)
				{
					passeIntro = true;
					raisonPasse = "clavier " + e.KeyCode;
				}
			}
			else if (couches.Count <= 0)
			{
				bool flag2 = true;
				if (e.Control && e.KeyCode == Keys.D1)
				{
					ShowVue(Vue.Accueil);
				}
				else if (e.Control && e.KeyCode == Keys.D2)
				{
					ShowVue(Vue.Jeux);
				}
				else if (e.Control && e.KeyCode == Keys.D3)
				{
					ShowVue(Vue.Biblio);
				}
				else if (e.Control && e.KeyCode == Keys.Q)
				{
					Quitter();
				}
				else if (e.Control && e.KeyCode == Keys.F)
				{
					ShowVue(Vue.Biblio);
					if (tRecherche != null)
					{
						tRecherche.Focus();
					}
				}
				else if (e.KeyCode == Keys.F5)
				{
					Recharger(manuel: true);
				}
				else if (e.KeyCode == Keys.Escape && vue == Vue.Projet)
				{
					ShowVue(Vue.Biblio);
				}
				else if (e.KeyCode == Keys.Escape && vue == Vue.Biblio)
				{
					ShowVue(Vue.Jeux);
				}
				else if (e.KeyCode == Keys.Escape && vue == Vue.Jeux)
				{
					ShowVue(Vue.Accueil);
				}
				else if (vue == Vue.Accueil && e.KeyCode == Keys.Left)
				{
					AllerDiapo(-1);
				}
				else if (vue == Vue.Accueil && e.KeyCode == Keys.Right)
				{
					AllerDiapo(1);
				}
				else
				{
					flag2 = false;
				}
				if (flag2)
				{
					e.Handled = true;
					e.SuppressKeyPress = true;
				}
			}
		};
		bulle.SetToolTip(nAccueil, "Accueil (Ctrl+1)");
		bulle.SetToolTip(nJeux, "Choisir un jeu (Ctrl+2)");
		bulle.SetToolTip(nBiblio, "Bibliothèque (Ctrl+3) · recherche : Ctrl+F");
		animation = new System.Windows.Forms.Timer();
		animation.Interval = 16;
		animation.Tick += delegate
		{
			try
			{
				Anime();
			}
			catch (Exception e3)
			{
				Journal.Une("animation", e3);
			}
		};
		animation.Start();
		fond.QuandPret = delegate
		{
			Sur(delegate
			{
				PanelVisible()?.Invalidate();
			});
		};
		fond.Preparer(ModeCapture);
		minPresence = new System.Windows.Forms.Timer();
		minPresence.Interval = 3000;
		minPresence.Tick += Protege("presence", delegate
		{
			if (jeuEnCours != null && Omsi.JeuLance())
			{
				try
				{
					Progression.Jeu(Comptes.Actif, 3, JeuDe(jeuEnCours));
					if (++tickStats % 20 == 0)
					{
						Progression.LireStatsJeu(Comptes.Actif, Omsi.DetecteOmsi());
					}
				}
				catch
				{
				}
			}
			if (jeuEnCours != null && (DateTime.Now - debutJeu).TotalSeconds > (double)Zx.P(30) && !((jeuWebProfil != null) ? FenetreJeuWeb() : (JeuxWebParId.ContainsKey(jeuEnCours.Id ?? "") ? ((DateTime.Now - debutJeu).TotalMinutes < (double)Zx.P(120)) : Omsi.JeuLance())))
			{
				jeuEnCours = null;
				jeuWebProfil = null;
				MajPresence();
			}
			Compte actif2 = Comptes.Actif;
			string utilisateur = DiscordPresence.Utilisateur;
			if (actif2 != null && utilisateur.Length > 0 && actif2.DiscordNom != utilisateur)
			{
				actif2.DiscordNom = utilisateur;
				Comptes.Sauvegarder();
			}
		});
		minPresence.Start();
		base.Activated += delegate
		{
			if (!occupe && sel != null)
			{
				MajEtat();
			}
		};
		ShowVue(Vue.Accueil);
		Lang.Traduire(this);
		base.FormClosed += delegate
		{
			if (!ModeCapture && Social.Actif && Social.Connecte)
			{
				Thread thread = new Thread((ThreadStart)delegate
				{
					try
					{
						Social.Presence("");
					}
					catch
					{
					}
				});
				thread.IsBackground = true;
				thread.Start();
				thread.Join(1500);
			}
		};
		minuteur = new System.Windows.Forms.Timer();
		minuteur.Interval = 3000;
		minuteur.Tick += Protege("minuteur", delegate
		{
			if (!occupe && sel != null && base.WindowState != FormWindowState.Minimized && Form.ActiveForm == this)
			{
				MajEtat();
			}
			SyncStatut(force: false);
			if (++tickNotif % 20 == 0 && cat != null && !ModeCapture)
			{
				try
				{
					Notifs.Verifier(cat, Omsi.DetecteOmsi(), null);
				}
				catch
				{
				}
				MajInfos();
			}
		});
		minuteur.Start();
		base.Shown += delegate
		{
			if (ModeCapture)
			{
				MajDossiers();
				Recharger(manuel: false);
			}
			else
			{
				bool avecIntro = ActionAuto == null && Rouvrir < 0 && Animer();
				System.Windows.Forms.Timer tc = new System.Windows.Forms.Timer();
				tc.Interval = (avecIntro ? 700 : 20);
				tc.Tick += delegate
				{
					tc.Stop();
					tc.Dispose();
					MajDossiers();
					Recharger(manuel: false);
				};
				tc.Start();
				EventHandler lance = null;
				lance = delegate
				{
					Application.Idle -= lance;
					if (avecIntro)
					{
						Intro(null);
					}
					if (base.Opacity < 1.0)
					{
						base.Opacity = 1.0;
					}
					try
					{
						Compte actif = Comptes.Actif;
						if (actif != null)
						{
							Progression.Connexion(actif);
							Progression.LireStatsJeu(actif, Omsi.DetecteOmsi());
						}
					}
					catch (Exception e2)
					{
						Journal.Ecrire("progression", e2);
					}
					if (Rouvrir >= 0)
					{
						int sr = Rouvrir;
						Rouvrir = -1;
						if (PleinAuDemarrage && !plein)
						{
							BasculerPleinEcran();
						}
						Sur(delegate
						{
							FenParametres.SectionInitiale = sr;
							Parametres();
						});
					}
					else
					{
						Sur(delegate
						{
							if (ActionAuto == null && (Omsi.DetecteOmsi() == null || Omsi.DetecteOpen(Omsi.DetecteOmsi()) == null))
							{
								Dossiers("Bienvenue ! Indiquez où sont installés OMSI 2 et openOMSI sur ce PC (une seule fois).");
							}
							if (ActionAuto == null && Comptes.Actif == null && Config.Lire("profil_propose") != "1")
							{
								Config.Ecrire("profil_propose", "1");
								Profil(nouveau: true);
							}
						});
					}
				};
				Application.Idle += lance;
			}
		};
	}

	private void Sur(Action a)
	{
		try
		{
			if (base.IsDisposed || !base.IsHandleCreated)
			{
				return;
			}
			BeginInvoke((MethodInvoker)delegate
			{
				if (base.IsDisposed)
				{
					return;
				}
				try
				{
					a();
				}
				catch (Exception e)
				{
					Journal.Ecrire("interface", e);
				}
			});
		}
		catch (InvalidOperationException)
		{
		}
	}

	private static Image Res(string n)
	{
		Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(n);
		if (manifestResourceStream == null)
		{
			return null;
		}
		return Image.FromStream(manifestResourceStream);
	}

	private Label L(Font f, Color c, bool auto)
	{
		Label label = new Label();
		label.Font = f;
		label.ForeColor = c;
		label.AutoSize = auto;
		label.BackColor = C.Fond;
		return label;
	}

	private Bouton Petit(string t, EventHandler h)
	{
		Bouton bouton = new Bouton(t);
		bouton.Plein = false;
		bouton.Font = new Font("Segoe UI", Zx.P(9.5f));
		bouton.Size = new Size(Zx.P(150), Zx.P(34));
		bouton.Click += h;
		return bouton;
	}

	private Bouton Onglet(string t, EventHandler h)
	{
		Bouton bouton = Petit(t, h);
		bouton.Size = new Size(Zx.P(160), Zx.P(34));
		bouton.Font = new Font("Segoe UI Semibold", Zx.P(9.5f));
		return bouton;
	}

	private void ChoisirOnglet(bool notes)
	{
		ChoisirOnglet(notes ? 1 : 0);
	}

	private void ChoisirOnglet(int i)
	{
		descP.Visible = i == 0;
		notesP.Visible = i == 1;
		comP.Visible = i == 2;
		galP.Visible = false;
		Bouton[] array = new Bouton[3] { bOngDesc, bOngNotes, bOngGal };
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Plein = j == i;
			array[j].Couleur = C.JadeFonce;
			array[j].Invalidate();
		}
		if (sel != null && page != null)
		{
			Disposer();
		}
	}

	private void InitSocial()
	{
		bAime = Petit("♥  J'aime", delegate
		{
			BasculerLike();
		});
		bAime.Visible = false;
		bAime.Font = new Font("Segoe UI Semibold", Zx.P(10f));
		bSuivre = Petit("\uea8f", delegate
		{
			if (sel != null)
			{
				bool flag = !Notifs.Suivi(sel, Omsi.DetecteOmsi());
				Notifs.Suivre(sel.Id, flag);
				MajSuivi();
				Toast(flag ? ("\ud83d\udd14  Vous serez prévenu pour « " + sel.Nom + " »") : ("Notifications coupées pour « " + sel.Nom + " »"), C.Jade, 3500);
			}
		});
		bSuivre.Font = new Font("Segoe MDL2 Assets", Zx.P(12f));
		comP = new Panel();
		comP.BackColor = C.Fond;
		comP.Visible = false;
		comSaisie = new Champ(multi: true, Zx.P(76));
		comSaisie.Indice = "Écrire un commentaire…";
		comSaisie.MaxLength = 500;
		comSaisie.Location = new Point(0, 0);
		comP.Controls.Add(comSaisie);
		comInfo = new Label();
		comInfo.Font = new Font("Segoe UI", Zx.P(9f));
		comInfo.ForeColor = C.Gris;
		comInfo.BackColor = C.Fond;
		comInfo.AutoSize = false;
		comP.Controls.Add(comInfo);
		comPub = new Bouton("Publier");
		comPub.Size = new Size(Zx.P(140), Zx.P(36));
		comPub.Click += delegate
		{
			PublierCommentaire();
		};
		comP.Controls.Add(comPub);
		comListe = new ZoneDessin();
		comListe.Dessiner = DessinerComs;
		comListe.MouseClick += ClicComs;
		comP.Controls.Add(comListe);
		comP.Resize += delegate
		{
			PlacerCom();
		};
		tTel = new System.Windows.Forms.Timer();
		tTel.Interval = 120000;
		tTel.Tick += delegate
		{
			if (vue == Vue.Projet || vue == Vue.Accueil)
			{
				ChargerPopularite(force: true);
			}
		};
		tTel.Start();
		tHeure = new System.Windows.Forms.Timer();
		tHeure.Interval = 30000;
		tHeure.Tick += delegate
		{
			if (comP.Visible && comListe != null)
			{
				comListe.Invalidate();
			}
		};
		tHeure.Start();
	}

	private void PlacerCom()
	{
		int width = comP.Width;
		comSaisie.Width = width;
		comInfo.SetBounds(0, Zx.P(90), Math.Max(Zx.P(100), width - Zx.P(160)), Zx.P(22));
		comPub.Location = new Point(width - Zx.P(140), Zx.P(86));
		comListe.SetBounds(0, Zx.P(134), width, comH);
	}

	private int Telechargements(Projet p)
	{
		int num = 0;
		string value = "/download/" + p.Id + "-v";
		foreach (KeyValuePair<string, int> item in telecharg)
		{
			if (item.Key.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				num += item.Value;
			}
		}
		return num;
	}

	private static string Depuis(long unix)
	{
		if (unix <= 0)
		{
			return "";
		}
		TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unix);
		if (timeSpan.TotalSeconds < 60.0)
		{
			return "à l'instant";
		}
		if (timeSpan.TotalMinutes < 60.0)
		{
			return "il y a " + (int)timeSpan.TotalMinutes + " min";
		}
		if (timeSpan.TotalHours < 24.0)
		{
			return "il y a " + (int)timeSpan.TotalHours + " h";
		}
		if (timeSpan.TotalDays < 30.0)
		{
			return "il y a " + (int)timeSpan.TotalDays + " j";
		}
		return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unix).ToLocalTime().ToString("dd/MM/yyyy");
	}

	private static int HauteurCom(ComSocial c, int w)
	{
		return Math.Max(78, 44 + Lang.MeasureText(c.Texte, fComTxt, new Size(Math.Max(Zx.P(100), w - Zx.P(96)), Zx.P(4000)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + 16);
	}

	private void RecalcCom(int largeur)
	{
		int num = Zx.P(8);
		foreach (ComSocial com in soc.Coms)
		{
			num += HauteurCom(com, largeur) + Zx.P(10);
		}
		comH = ((soc.Coms.Count == 0) ? 90 : (num + 8));
	}

	private void DessinerComs(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		g.Clear(C.Fond);
		rSuppr.Clear();
		idSuppr.Clear();
		if (soc.Coms.Count == 0)
		{
			Lang.DrawText(g, Social.Actif ? "Aucun commentaire pour l'instant. Soyez le premier !" : "", fLigne, new Rectangle(0, Zx.P(24), w, Zx.P(30)), C.Gris, TextFormatFlags.HorizontalCenter);
			return;
		}
		int num = Zx.P(4);
		for (int num2 = soc.Coms.Count - 1; num2 >= 0; num2--)
		{
			ComSocial comSocial = soc.Coms[num2];
			int num3 = HauteurCom(comSocial, w);
			Rectangle r = new Rectangle(0, num, w - 1, num3);
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
			{
				using (SolidBrush brush = new SolidBrush(C.Carte))
				{
					g.FillPath(brush, path);
				}
				using Pen pen = new Pen(comSocial.Moi ? Color.FromArgb(110, C.Jade) : C.Ligne);
				g.DrawPath(pen, path);
			}
			Rectangle rectangle = new Rectangle(Zx.P(16), num + Zx.P(14), Zx.P(42), Zx.P(42));
			Color color = Comptes.Palette[Math.Abs(comSocial.Auteur.GetHashCode()) % Comptes.Palette.Length];
			using (SolidBrush brush2 = new SolidBrush(color))
			{
				g.FillEllipse(brush2, rectangle);
			}
			Lang.DrawText(g, (comSocial.Auteur.Length > 0) ? comSocial.Auteur.Substring(0, 1).ToUpperInvariant() : "?", new Font("Segoe UI Semibold", Zx.P(15f)), rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Badge.Dessiner(g, new Rectangle(rectangle.Right - Zx.P(24), rectangle.Bottom - Zx.P(22), Zx.P(30), Zx.P(30)), comSocial.Niveau, 0.0);
			int num4 = 72;
			Size size = Lang.MeasureText(comSocial.Auteur, fComNom, new Size(Zx.P(400), Zx.P(24)), TextFormatFlags.NoPadding);
			Lang.DrawText(g, comSocial.Auteur, fComNom, new Point(num4, num + Zx.P(12)), C.Texte);
			int num5 = num4 + size.Width + 8;
			if (comSocial.Admin)
			{
				Rectangle r2 = new Rectangle(num5, num + Zx.P(13), Zx.P(52), Zx.P(18));
				using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(9)))
				{
					using SolidBrush brush3 = new SolidBrush(C.Orange);
					g.FillPath(brush3, path2);
				}
				Lang.DrawText(g, "ÉQUIPE", fPetit, r2, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				num5 += 60;
			}
			if (comSocial.Vip)
			{
				Badge.PilleVip(g, new Rectangle(num5, num + Zx.P(13), Zx.P(38), Zx.P(18)));
				num5 += 46;
			}
			if (comSocial.Statut.Length > 0)
			{
				Color color2 = (comSocial.Statut.StartsWith("En jeu") ? C.Orange : C.Vert);
				string t = "●  " + comSocial.Statut;
				Size size2 = Lang.MeasureText(t, fPetit);
				Lang.DrawText(g, t, fPetit, new Point(num5, num + Zx.P(15)), color2);
				num5 += size2.Width + 10;
				using (SolidBrush brush4 = new SolidBrush(C.Carte))
				{
					g.FillEllipse(brush4, rectangle.Right - Zx.P(14), rectangle.Y - 1, Zx.P(15), Zx.P(15));
				}
				using SolidBrush brush5 = new SolidBrush(color2);
				g.FillEllipse(brush5, rectangle.Right - Zx.P(12), rectangle.Y + 1, Zx.P(11), Zx.P(11));
			}
			Lang.DrawText(g, Depuis(comSocial.Cree), fPetit, new Point(num5, num + Zx.P(15)), C.Gris);
			Lang.DrawText(g, comSocial.Texte, fComTxt, new Rectangle(num4, num + Zx.P(38), w - Zx.P(96), num3 - Zx.P(44)), Color.FromArgb(205, 214, 228), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			if (comSocial.Moi || Social.EstAdmin)
			{
				Size size3 = Lang.MeasureText("Supprimer", fPetit);
				Rectangle rectangle2 = new Rectangle(w - Zx.P(16) - size3.Width, num + Zx.P(12), size3.Width, Zx.P(18));
				Lang.DrawText(g, "Supprimer", fPetit, rectangle2, Color.FromArgb(220, 110, 100), TextFormatFlags.NoPadding);
				rSuppr.Add(Rectangle.Inflate(rectangle2, Zx.P(8), Zx.P(6)));
				idSuppr.Add(comSocial.Id);
			}
			num += num3 + Zx.P(10);
		}
	}

	private void ClicComs(object o, MouseEventArgs e)
	{
		for (int i = 0; i < rSuppr.Count; i++)
		{
			if (rSuppr[i].Contains(e.Location))
			{
				SupprimerCom(idSuppr[i]);
				break;
			}
		}
	}

	private void OuvrirSocial()
	{
		if (sel == null)
		{
			return;
		}
		MajSocialUI();
		if (!Social.Actif || (socMod == sel.Id && flux != null))
		{
			return;
		}
		FermerSocial();
		socMod = sel.Id;
		string mod = sel.Id;
		soc = new EtatSocial();
		socEnLigne = false;
		MajSocialUI();
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				EtatSocial e = Social.Etat(mod);
				Dictionary<string, object> moi = null;
				if (Social.Connecte)
				{
					try
					{
						moi = Social.Moi();
						Social.MajNiveau(Progression.NiveauDe(Comptes.Actif));
					}
					catch
					{
					}
				}
				Sur(delegate
				{
					if (!(socMod != mod))
					{
						soc = e;
						MajSocialUI();
						Compte actif = Comptes.Actif;
						if (moi != null && actif != null)
						{
							try
							{
								int num = Convert.ToInt32(moi["votes"]);
								int num2 = Convert.ToInt32(moi["commentaires"]);
								if (num > Progression.S(actif, "votes"))
								{
									actif.Stats["votes"] = num;
								}
								if (num2 > Progression.S(actif, "commentaires"))
								{
									actif.Stats["commentaires"] = num2;
								}
								Progression.Verifier(actif);
								Comptes.Sauvegarder();
							}
							catch
							{
							}
						}
					}
				});
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				Sur(delegate
				{
					comInfo.Text = "Serveur injoignable : " + ex.Message;
					comInfo.ForeColor = C.Rouge;
				});
			}
		});
		thread.IsBackground = true;
		thread.Start();
		flux = new FluxSocial(mod, delegate(Dictionary<string, object> d)
		{
			Sur(delegate
			{
				SurFlux(mod, d);
			});
		}, delegate(bool en)
		{
			Sur(delegate
			{
				socEnLigne = en;
				MajSocialUI();
			});
		});
	}

	private void FermerSocial()
	{
		if (flux != null)
		{
			flux.Fermer();
			flux = null;
		}
		socMod = "";
		socEnLigne = false;
	}

	private void SurFlux(string mod, Dictionary<string, object> d)
	{
		if (socMod != mod)
		{
			return;
		}
		switch (Convert.ToString(d.ContainsKey("t") ? d["t"] : ""))
		{
		case "likes":
			soc.Likes = Convert.ToInt32(d["likes"]);
			break;
		case "commentaire":
		{
			ComSocial c = Social.Com(d.ContainsKey("c") ? (d["c"] as Dictionary<string, object>) : null);
			if (c != null && soc.Coms.Find((ComSocial x) => x.Id == c.Id) == null)
			{
				soc.Coms.Add(c);
			}
			break;
		}
		case "supprime":
		{
			int id = Convert.ToInt32(d["id"]);
			soc.Coms.RemoveAll((ComSocial x) => x.Id == id);
			break;
		}
		}
		MajSocialUI();
	}

	private void MajSocialUI()
	{
		if (page != null && sel != null && bAime != null)
		{
			bool actif = Social.Actif;
			bAime.Visible = actif;
			bAime.Text = "♥  " + (soc.Aime ? "Aimé" : "J'aime") + "  ·  " + soc.Likes;
			bAime.Plein = soc.Aime;
			bAime.Couleur = C.Orange;
			bAime.Invalidate();
			bOngGal.Text = "Commentaires" + (actif ? ("  ·  " + soc.Coms.Count) : "");
			bOngGal.Invalidate();
			Champ champ = comSaisie;
			bool enabled = (comPub.Enabled = actif);
			champ.Enabled = enabled;
			comInfo.Text = ((!actif) ? "Les commentaires seront disponibles dès que le serveur TPS Groupe sera configuré." : (socEnLigne ? "●  En direct : les nouveaux messages apparaissent tout de suite" : "Connexion au serveur…"));
			comInfo.ForeColor = (socEnLigne ? C.Vert : C.Gris);
			garderScroll = true;
			Disposer();
		}
	}

	private void BasculerLike()
	{
		if (sel == null || !Social.Actif)
		{
			return;
		}
		Compte cpt = Comptes.Actif;
		if (cpt == null)
		{
			Profil(nouveau: true);
			return;
		}
		string mod = sel.Id;
		bAime.Enabled = false;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				string text = Social.Assurer(cpt);
				if (text != null)
				{
					throw new Exception(text);
				}
				bool aime;
				int likes = Social.Aimer(mod, out aime);
				Sur(delegate
				{
					bAime.Enabled = true;
					if (socMod == mod)
					{
						soc.Likes = likes;
						soc.Aime = aime;
					}
					MajSocialUI();
					string key = "xp_like_" + mod;
					if (aime && !cpt.Stats.ContainsKey(key))
					{
						cpt.Stats[key] = 1;
						Progression.Compter(cpt, "votes", 1);
						Progression.Gagner(cpt, 20, "vote pour un mod");
					}
				});
			}
			catch (Exception ex)
			{
				Exception e = ex;
				Sur(delegate
				{
					bAime.Enabled = true;
					Toast("J'aime impossible : " + e.Message, C.Rouge, 5000);
				});
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void PublierCommentaire()
	{
		if (sel == null || !Social.Actif)
		{
			return;
		}
		Compte cpt = Comptes.Actif;
		if (cpt == null)
		{
			Profil(nouveau: true);
			return;
		}
		string texte = comSaisie.Text.Trim();
		if (texte.Length < 2)
		{
			Toast("Écrivez au moins 2 caractères.", C.Orange, 3000);
			return;
		}
		string mod = sel.Id;
		comPub.Enabled = false;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				string text = Social.Assurer(cpt);
				if (text != null)
				{
					throw new Exception(text);
				}
				ComSocial c = Social.Commenter(mod, texte);
				Sur(delegate
				{
					comPub.Enabled = true;
					comSaisie.Text = "";
					if (socMod == mod)
					{
						ComSocial comSocial = soc.Coms.Find((ComSocial x) => x.Id == c.Id);
						if (comSocial != null)
						{
							comSocial.Moi = true;
						}
						else
						{
							soc.Coms.Add(c);
						}
					}
					MajSocialUI();
					Progression.Compter(cpt, "commentaires", 1);
					string text2 = "xp_comm_" + DateTime.Now.ToString("yyyyMMdd");
					int num = Progression.S(cpt, text2);
					if (num < 3)
					{
						Progression.Compter(cpt, "comm_comptes", 1);
					}
					if (num < 5)
					{
						cpt.Stats[text2] = num + 1;
						Progression.Gagner(cpt, 25, "commentaire");
					}
					else
					{
						Progression.Verifier(cpt);
						Comptes.Sauvegarder();
					}
				});
			}
			catch (Exception ex)
			{
				Exception e = ex;
				Sur(delegate
				{
					comPub.Enabled = true;
					Toast("Commentaire impossible : " + e.Message, C.Rouge, 6000);
				});
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void SupprimerCom(int id)
	{
		if (MessageBox.Show(this, "Supprimer ce commentaire ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
		{
			return;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				Social.Supprimer(id);
				Sur(delegate
				{
					soc.Coms.RemoveAll((ComSocial x) => x.Id == id);
					MajSocialUI();
				});
			}
			catch (Exception ex)
			{
				Exception e = ex;
				Sur(delegate
				{
					Toast("Suppression impossible : " + e.Message, C.Rouge, 5000);
				});
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	public void CaptureSocial(string dossier)
	{
		StringBuilder sb = new StringBuilder();
		Func<string, bool, bool> func = delegate(string nom, bool ok)
		{
			sb.AppendLine((ok ? "OK    " : "ECHEC ") + nom);
			return ok;
		};
		Func<Func<bool>, int, bool> func2 = delegate(Func<bool> cond, int ms)
		{
			DateTime dateTime = DateTime.Now.AddMilliseconds(ms);
			while (DateTime.Now < dateTime)
			{
				Application.DoEvents();
				Thread.Sleep(40);
				if (cond())
				{
					return true;
				}
			}
			return cond();
		};
		try
		{
			func("serveur configuré", Social.Actif);
			if (cat == null || cat.Projets.Count == 0 || !Social.Actif)
			{
				File.WriteAllText(Path.Combine(dossier, "social.txt"), sb.ToString(), Encoding.UTF8);
				return;
			}
			Projet p = cat.Projets[0];
			Choisir(p);
			ShowVue(Vue.Projet);
			ChoisirOnglet(2);
			Application.DoEvents();
			func("temps réel connecté", func2(() => socEnLigne, 8000));
			func("état initial chargé", func2(() => soc.Likes >= 0 && socMod == p.Id, 4000));
			int avantLikes = soc.Likes;
			BasculerLike();
			func("J'aime pris en compte", func2(() => soc.Aime && soc.Likes == avantLikes + 1, 8000));
			func("bouton J'aime mis à jour", bAime.Text.Contains("Aimé") && bAime.Text.Contains((avantLikes + 1).ToString()));
			comSaisie.Text = "Commentaire du lanceur " + DateTime.Now.Ticks % 100000;
			PublierCommentaire();
			func("commentaire publié et affiché", func2(() => soc.Coms.Exists((ComSocial x) => x.Moi), 8000));
			_ = soc.Coms.Count;
			func("commentaire d'un autre joueur reçu en temps réel (sans rechargement)", func2(() => soc.Coms.Exists((ComSocial x) => !x.Moi && x.Texte.StartsWith("Depuis un autre PC")), 25000));
			func("onglet Commentaires affiche le total", bOngGal.Text.Contains(soc.Coms.Count.ToString()));
			Refresh();
			Application.DoEvents();
			Thread.Sleep(300);
			Application.DoEvents();
			using (Bitmap bitmap = new Bitmap(base.Width, base.Height))
			{
				DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
				bitmap.Save(Path.Combine(dossier, "social_commentaires.png"));
			}
			ComSocial comSocial = soc.Coms.Find((ComSocial x) => x.Moi);
			if (comSocial != null)
			{
				int id = comSocial.Id;
				Thread thread = new Thread((ThreadStart)delegate
				{
					try
					{
						Social.Supprimer(id);
					}
					catch
					{
					}
				});
				thread.Start();
				func("suppression reçue en temps réel", func2(() => !soc.Coms.Exists((ComSocial x) => x.Id == id), 8000));
			}
			Progression.Verifier(Comptes.Actif);
			func("XP et succès de vote/commentaire attribués", Comptes.Actif != null && Progression.S(Comptes.Actif, "votes") >= 1 && Progression.S(Comptes.Actif, "commentaires") >= 1);
		}
		catch (Exception ex)
		{
			sb.AppendLine("EXCEPTION " + ex);
		}
		File.WriteAllText(Path.Combine(dossier, "social.txt"), sb.ToString(), Encoding.UTF8);
	}

	private List<Image> DiapoPage()
	{
		List<Image> list = new List<Image>();
		if (sel == null)
		{
			return list;
		}
		if (images.TryGetValue(sel.Id, out var value) && value != null)
		{
			list.Add(value);
		}
		else if (banniere != null)
		{
			list.Add(banniere);
		}
		if (galImages.TryGetValue(sel.Id, out var value2))
		{
			foreach (Image item in value2)
			{
				if (item != null)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private static string Restant(TimeSpan r)
	{
		if (r.TotalDays >= 1.0)
		{
			return "Dans " + (int)r.TotalDays + " j " + r.Hours + " h";
		}
		if (r.TotalHours >= 1.0)
		{
			return "Dans " + (int)r.TotalHours + " h " + r.Minutes + " min";
		}
		return "Dans " + Math.Max(1, (int)r.TotalMinutes) + " min";
	}

	private static void Indispo(Projet p, out string bouton, out string message)
	{
		string text = (p.Statut ?? "").ToLowerInvariant();
		DateTime? dateSortie = p.DateSortie;
		if (p.StatutEffectif == "masque")
		{
			bouton = "INDISPONIBLE";
			message = "Page masquée : visible uniquement par les administrateurs.";
		}
		else if (!p.AccesVip && p.StatutEffectif == "publie")
		{
			DateTime? datePublic = p.DatePublic;
			bouton = ((p.Vip == "test") ? "RÉSERVÉ AUX TESTEURS VIP" : "ACCÈS ANTICIPÉ VIP");
			message = ((p.Vip == "test") ? "Version de test réservée aux testeurs VIP (Profil › VIP)." : ("Accès anticipé réservé aux VIP" + (datePublic.HasValue ? ("\nDisponible pour tous le " + datePublic.Value.ToString("dd/MM/yyyy à HH:mm")) : "") + "."));
		}
		else if (text == "programme" && dateSortie.HasValue)
		{
			TimeSpan r = dateSortie.Value - DateTime.Now;
			bouton = "DISPONIBLE LE " + dateSortie.Value.ToString("dd/MM");
			message = "Mise en ligne le " + dateSortie.Value.ToString("dd/MM/yyyy à HH:mm") + ((r.TotalSeconds > 0.0) ? ("\n" + Restant(r)) : "\nPublication imminente…");
		}
		else if (text == "bientot" || text == "programme")
		{
			bouton = "PROCHAINEMENT";
			message = "Prochainement disponible" + (dateSortie.HasValue ? ("\nSortie prévue le " + dateSortie.Value.ToString("dd/MM/yyyy")) : "");
		}
		else
		{
			bouton = "INDISPONIBLE";
			message = "Le paquet de ce contenu n'est pas encore publié.";
		}
	}

	private static string StatutTexte(Projet p, out Color c)
	{
		string text = (p.Statut ?? "").ToLowerInvariant();
		DateTime? dateSortie = p.DateSortie;
		if (p.StatutEffectif == "masque")
		{
			c = C.Gris;
			return "Masqué";
		}
		if (p.Vip == "test")
		{
			c = C.Or;
			return "Test VIP";
		}
		if (p.RestreintVip && p.StatutEffectif == "publie")
		{
			c = C.Or;
			return "Accès anticipé VIP";
		}
		if (p.StatutEffectif == "bientot")
		{
			c = Color.FromArgb(150, 120, 255);
			if (!(text == "programme") || !dateSortie.HasValue)
			{
				return "Prochainement";
			}
			return "Le " + dateSortie.Value.ToString("dd/MM/yyyy");
		}
		c = C.Vert;
		return "Disponible";
	}

	private static bool LienOk(string l)
	{
		if (Uri.TryCreate(l ?? "", UriKind.Absolute, out var result))
		{
			return result.Scheme == Uri.UriSchemeHttps;
		}
		return false;
	}

	private void OuvrirAchat(Projet p)
	{
		if (p != null && LienOk(p.LienAchat))
		{
			try
			{
				Process.Start(new Uri(p.LienAchat).AbsoluteUri);
			}
			catch
			{
			}
		}
	}

	private void SaisirCode()
	{
		if (sel == null)
		{
			return;
		}
		using FenCode f = new FenCode(sel);
		if (f.Afficher(this) == DialogResult.OK)
		{
			MajEtat();
			page.Invalidate();
		}
	}

	private int Mettre(int W)
	{
		int num = 40;
		_ = page.ClientSize.Height;
		bRetour.Location = new Point(num, Zx.P(26));
		bAccueilP.Location = new Point(num + Zx.P(56), Zx.P(26));
		lTitre.Location = new Point(num + Zx.P(118), Zx.P(40));
		lSous.Location = new Point(num + Zx.P(122), Zx.P(96));
		bSuivre.SetBounds(W - num - Zx.P(48), Zx.P(30), Zx.P(48), Zx.P(44));
		bAime.SetBounds(W - num - Zx.P(48) - Zx.P(10) - Zx.P(160), Zx.P(30), Zx.P(160), Zx.P(44));
		bSupport.SetBounds(W - num - Zx.P(48) - Zx.P(10) - Zx.P(160) - Zx.P(10) - Zx.P(140), Zx.P(30), Zx.P(140), Zx.P(44));
		int num2 = 186;
		int num3 = Zx.P(28);
		int num4 = Math.Max(420, (int)((double)(W - 2 * num - num3) * 0.64));
		int num5 = W - 2 * num - num3 - num4;
		if (num5 < 310)
		{
			num5 = 310;
			num4 = Math.Max(360, W - 2 * num - num3 - num5);
		}
		int num6 = Math.Min(540, num4 * 9 / 16);
		rVue = new Rectangle(num, num2, num4, num6);
		List<Image> list = DiapoPage();
		int num7 = Math.Max(1, list.Count);
		rMini.Clear();
		int num9 = num2 + num6 + Zx.P(12);
		int num11 = num9 + 8;
		bOngDesc.Location = new Point(num, num11);
		bOngNotes.Location = new Point(num + Zx.P(170), num11);
		bOngGal.Visible = sel == null || sel.Commentaires;
		bOngGal.Location = new Point(num + Zx.P(340), num11);
		galP.Visible = false;
		if (!bOngGal.Visible && comP.Visible)
		{
			comP.Visible = false;
			descP.Visible = true;
			bOngDesc.Plein = true;
			bOngGal.Plein = false;
			bOngDesc.Invalidate();
		}
		bDossier.SetBounds(num + (bOngGal.Visible ? Zx.P(510) : Zx.P(340)), num11, Zx.P(170), Zx.P(34));
		int width = Math.Max(Zx.P(240), num4 - Zx.P(44));
		lDesc.MaximumSize = new Size(width, 0);
		lDesc.AutoSize = true;
		lNotes.MaximumSize = new Size(width, 0);
		lNotes.AutoSize = true;
		descP.AutoScroll = false;
		notesP.AutoScroll = false;
		int num12 = num11 + 46;
		descP.SetBounds(num, num12, num4, Math.Max(Zx.P(140), lDesc.Height + Zx.P(44)));
		notesP.SetBounds(num, num12, num4, Math.Max(Zx.P(140), lNotes.Height + Zx.P(44)));
		RecalcCom(num4);
		comP.SetBounds(num, num12, num4, Zx.P(140) + comH);
		PlacerCom();
		int val = num12 + (comP.Visible ? comP.Height : (notesP.Visible ? notesP.Height : descP.Height));
		int num13 = num + num4 + num3;
		int num14 = Zx.P(20);
		int num15 = num5 - 2 * num14;
		Zx.P(16);
		lEtat.BackColor = C.Carte;
		lProg.BackColor = C.Carte;
		lAlerte.BackColor = C.Carte;
		lEtat.Location = new Point(num13 + num14, num2 + Zx.P(66));
		lEtat.Size = new Size(num15, Zx.P(46));
		bAction.Location = new Point(num13 + num14, num2 + Zx.P(120));
		bAction.Size = new Size(num15, Zx.P(56));
		int num16 = num2 + Zx.P(120) + Zx.P(56) + Zx.P(10);
		bCode.Visible = modeAchat;
		bCode.SetBounds(num13 + num14, num16, num15, Zx.P(36));
		if (modeAchat)
		{
			num16 += Zx.P(46);
		}
		barre.Location = new Point(num13 + num14, num16 + Zx.P(6));
		barre.Width = num15;
		lProg.Location = new Point(num13 + num14, num16 + Zx.P(18));
		lProg.Size = new Size(num15, Zx.P(20));
		lAlerte.Location = new Point(num13 + num14, num16 + Zx.P(2));
		lAlerte.Size = new Size(num15, Zx.P(40));
		num16 += Zx.P(48);
		int num17 = (num15 - Zx.P(10)) / 2;
		bLies.SetBounds(num13 + num14, num16, num17, Zx.P(80));
		rSocial = new Rectangle(num13 + num14 + num17 + Zx.P(14), num16, num17 - Zx.P(14), Zx.P(34));
		MajSuivi();
		MajLies();
		num16 += Zx.P(46);
		bDesinst.SetBounds(num13 + num14 + num17 + Zx.P(10), num16, num17, Zx.P(34));
		rAchat = new Rectangle(num13, num2, num5, num16 + Zx.P(34) + Zx.P(20) - num2);
		rPhotosP = Rectangle.Empty;
		if (list.Count > 1)
		{
			int num30 = 3;
			int num31 = Zx.P(8);
			int num32 = (num5 - 2 * num14 - (num30 - 1) * num31) / num30;
			int num33 = num32 * 9 / 16;
			int num34 = (list.Count + num30 - 1) / num30;
			rPhotosP = new Rectangle(num13, rAchat.Bottom + Zx.P(16), num5, Zx.P(50) + num34 * (num33 + num31) + Zx.P(12));
			for (int i = 0; i < list.Count; i++)
			{
				rMini.Add(new Rectangle(num13 + num14 + i % num30 * (num32 + num31), rPhotosP.Y + Zx.P(50) + i / num30 * (num33 + num31), num32, num33));
			}
		}
		PlacerVideoPage();
		int num18 = ((sel != null) ? LignesInfo(sel).Count : 8);
		int num19 = ((sel != null) ? sel.Liens.Count : 0);
		int num20 = ((sel != null) ? sel.Dlc.Count : 0);
		int num21 = 0;
		if (sel != null)
		{
			foreach (string[] item in sel.Dlc)
			{
				if (item[2] != "1")
				{
					num21++;
				}
			}
		}
		int num22 = num20 - num21;
		rInfo = new Rectangle(num13, (rPhotosP.IsEmpty ? rAchat.Bottom : rPhotosP.Bottom) + Zx.P(16), num5, Zx.P(54) + num18 * Zx.P(30) + Zx.P(10) + ((num19 > 0) ? (Zx.P(34) + num19 * Zx.P(36)) : 0) + ((num21 > 0) ? (Zx.P(34) + num21 * Zx.P(32)) : 0) + ((num22 > 0) ? (Zx.P(34) + num22 * Zx.P(32)) : 0));
		return Math.Max(val, rInfo.Bottom) + 40;
	}

	private void Disposer()
	{
		Point autoScrollPosition = (garderScroll ? new Point(0, -page.AutoScrollPosition.Y) : Point.Empty);
		garderScroll = false;
		page.AutoScrollPosition = new Point(0, 0);
		int width = page.ClientSize.Width;
		int height = page.ClientSize.Height;
		int num = Mettre(width);
		if (num > height)
		{
			int num2 = Math.Max(600, width - ((!page.VerticalScroll.Visible) ? SystemInformation.VerticalScrollBarWidth : 0));
			if (num2 != width)
			{
				num = Mettre(num2);
			}
		}
		((PanelFond)page).AutoScrollMinSize = new Size(0, num);
		if (autoScrollPosition.Y > 0)
		{
			page.AutoScrollPosition = autoScrollPosition;
		}
		page.Invalidate();
	}

	private Bitmap CacheVue(Image im, Size s)
	{
		if (vueCache != null && vueSrc == im && vueTaille == s)
		{
			return vueCache;
		}
		if (vueCache != null)
		{
			vueCache.Dispose();
		}
		Bitmap bitmap = new Bitmap(s.Width, s.Height, PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, s.Width - 1, s.Height - 1), Zx.P(14));
			GraphicsState gstate = graphics.Save();
			graphics.SetClip(path, CombineMode.Intersect);
			if (im != null)
			{
				double num = Math.Max((double)s.Width / (double)im.Width, (double)s.Height / (double)im.Height);
				int num2 = (int)((double)im.Width * num);
				int num3 = (int)((double)im.Height * num);
				graphics.DrawImage(im, (s.Width - num2) / 2, (s.Height - num3) / 2, num2, num3);
			}
			else
			{
				using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, s.Width, s.Height + 1), C.Carte, C.Cote, 45f);
				graphics.FillRectangle(brush, 0, 0, s.Width, s.Height);
			}
			graphics.Restore(gstate);
			using Pen pen = new Pen(C.Ligne);
			graphics.DrawPath(pen, path);
		}
		vueCache = bitmap;
		vueSrc = im;
		vueTaille = s;
		return bitmap;
	}

	private void Carte(Graphics g, Rectangle r, int rayon)
	{
		using GraphicsPath path = Bouton.Rond(new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1), rayon);
		using (SolidBrush brush = new SolidBrush(C.Carte))
		{
			g.FillPath(brush, path);
		}
		using Pen pen = new Pen(C.Ligne);
		g.DrawPath(pen, path);
	}

	private void Pastille(Graphics g, string t, Color col, ref int x, int y, bool plein)
	{
		Size size = Lang.MeasureText(t, F.Etat);
		Rectangle r = new Rectangle(x, y, size.Width + Zx.P(24), Zx.P(26));
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(13)))
		{
			using (SolidBrush brush = new SolidBrush(plein ? Color.FromArgb(235, col) : Color.FromArgb(40, col)))
			{
				g.FillPath(brush, path);
			}
			if (!plein)
			{
				using Pen pen = new Pen(Color.FromArgb(120, col));
				g.DrawPath(pen, path);
			}
		}
		Txt.Dessiner(g, t, F.Etat, r, plein ? Color.White : ((col == C.Gris) ? C.Texte : col), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		x += r.Width + Zx.P(8);
	}

	private void PeintPage(Graphics g, int dy)
	{
		g.Clear(C.Fond);
		if (sel == null)
		{
			return;
		}
		GraphicsState gstate = g.Save();
		g.TranslateTransform(0f, dy);
		using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, page.ClientSize.Width, Zx.P(330)), Color.FromArgb(22, 26, 58), C.Fond, 90f))
		{
			g.FillRectangle(brush, 0, 0, page.ClientSize.Width, Zx.P(330));
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int num = 40;
		Txt.Dessiner(g, "Bibliothèque  ›  " + NomJeu(JeuDe(sel)) + "  ›  " + Categories.Nom(sel.Categorie), fFil, new Point(num + Zx.P(122), Zx.P(20)), C.Gris);
		int x = num + Zx.P(122);
		int y = Zx.P(140);
		Color c;
		string t = StatutTexte(sel, out c);
		Pastille(g, (!sel.Payant) ? "Gratuit" : (sel.Debloque ? "Acheté" : sel.PrixTexte), (sel.Payant && !sel.Debloque) ? C.Orange : C.Vert, ref x, y, plein: true);
		Pastille(g, t, c, ref x, y, plein: false);
		if (sel.Version.Length > 0)
		{
			Pastille(g, sel.Version, C.Jade, ref x, y, plein: false);
		}
		if (sel.Taille > 0)
		{
			Pastille(g, sel.Taille / 1048576 + " Mo", C.Gris, ref x, y, plein: false);
		}
		rTags.Clear();
		x += Zx.P(10);
		foreach (string tag in sel.Tags)
		{
			string t2 = "#" + tag;
			int num2 = Lang.MeasureText(t2, F.Etat).Width + 24;
			if (x + num2 <= page.ClientSize.Width - 40)
			{
				rTags.Add(new KeyValuePair<Rectangle, string>(new Rectangle(x, y, num2, Zx.P(26)), tag));
				Pastille(g, t2, Color.FromArgb(120, 200, 255), ref x, y, plein: false);
				continue;
			}
			break;
		}
		List<Image> list = DiapoPage();
		if (vIdx >= list.Count)
		{
			vIdx = 0;
		}
		if (rMini.Count != ((list.Count > 1) ? list.Count : 0) && !relayoutPage)
		{
			relayoutPage = true;
			BeginInvoke((MethodInvoker)delegate
			{
				relayoutPage = false;
				Disposer();
			});
		}
		g.DrawImageUnscaled(CacheVue((list.Count > 0) ? list[vIdx] : null, rVue.Size), rVue.X, rVue.Y);
		if (list.Count == 0 && sel.Video.Length == 0)
		{
			Txt.Dessiner(g, "Aucune image pour l'instant", fLigne, rVue, C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}
		rBVideo = Rectangle.Empty;
		if (sel.Video.Length > 0)
		{
			rBVideo = new Rectangle(rVue.X + Zx.P(16), rVue.Bottom - Zx.P(16) - Zx.P(42), Zx.P(200), Zx.P(42));
			using (GraphicsPath path = Bouton.Rond(rBVideo, Zx.P(21)))
			{
				using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(215, 10, 12, 30)))
				{
					g.FillPath(brush2, path);
				}
				using Pen pen = new Pen(Color.FromArgb(200, C.Orange), 1.5f);
				g.DrawPath(pen, path);
			}
			Rectangle rect = new Rectangle(rBVideo.X + Zx.P(7), rBVideo.Y + Zx.P(7), Zx.P(28), Zx.P(28));
			using (SolidBrush brush3 = new SolidBrush(C.Orange))
			{
				g.FillEllipse(brush3, rect);
			}
			using (SolidBrush brush4 = new SolidBrush(Color.White))
			{
				g.FillPolygon(brush4, new PointF[3]
				{
					new PointF(rect.X + Zx.P(11), rect.Y + Zx.P(8)),
					new PointF(rect.X + Zx.P(21), rect.Y + Zx.P(14)),
					new PointF(rect.X + Zx.P(11), rect.Y + Zx.P(20))
				});
			}
			Txt.Dessiner(g, "Bande-annonce", fBVideo, new Rectangle(rect.Right + Zx.P(10), rBVideo.Y, rBVideo.Right - rect.Right - Zx.P(16), rBVideo.Height), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		if (list.Count > 1)
		{
			rVPrev = new Rectangle(rVue.X + Zx.P(14), rVue.Y + rVue.Height / 2 - Zx.P(22), Zx.P(44), Zx.P(44));
			rVNext = new Rectangle(rVue.Right - Zx.P(14) - Zx.P(44), rVPrev.Y, Zx.P(44), Zx.P(44));
			int[] array = new int[2] { 0, 1 };
			foreach (int num3 in array)
			{
				Rectangle rectangle = ((num3 == 0) ? rVPrev : rVNext);
				using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(150, 8, 10, 24)))
				{
					g.FillEllipse(brush5, rectangle);
				}
				Txt.Dessiner(g, (num3 == 0) ? "‹" : "›", new Font("Segoe UI", Zx.P(18f)), rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			string t3 = vIdx + 1 + " / " + list.Count;
			Size size = Lang.MeasureText(t3, F.Etat);
			Rectangle r = new Rectangle(rVue.Right - Zx.P(14) - size.Width - Zx.P(20), rVue.Bottom - Zx.P(14) - Zx.P(24), size.Width + Zx.P(20), Zx.P(24));
			using (GraphicsPath path2 = Bouton.Rond(r, Zx.P(12)))
			{
				using SolidBrush brush6 = new SolidBrush(Color.FromArgb(170, 8, 10, 24));
				g.FillPath(brush6, path2);
			}
			Txt.Dessiner(g, t3, F.Etat, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			if (!rPhotosP.IsEmpty)
			{
				Carte(g, rPhotosP, Zx.P(14));
				Txt.Dessiner(g, "PHOTOS", fCarteT, new Point(rPhotosP.X + Zx.P(20), rPhotosP.Y + Zx.P(18)), C.Gris);
			}
			for (int j = 0; j < list.Count && j < rMini.Count; j++)
			{
				Rectangle rectangle2 = rMini[j];
				g.InterpolationMode = InterpolationMode.HighQualityBilinear;
				using GraphicsPath path3 = Bouton.Rond(rectangle2, Zx.P(7));
				GraphicsState gstate2 = g.Save();
				g.SetClip(path3, CombineMode.Intersect);
				Image image = list[j];
				double num4 = Math.Max((double)rectangle2.Width / (double)image.Width, (double)rectangle2.Height / (double)image.Height);
				int num5 = (int)((double)image.Width * num4);
				int num6 = (int)((double)image.Height * num4);
				g.DrawImage(image, rectangle2.X + (rectangle2.Width - num5) / 2, rectangle2.Y + (rectangle2.Height - num6) / 2, num5, num6);
				if (j != vIdx)
				{
					using SolidBrush brush7 = new SolidBrush(Color.FromArgb(120, 8, 10, 24));
					g.FillRectangle(brush7, rectangle2);
				}
				g.Restore(gstate2);
				using Pen pen2 = new Pen((j == vIdx) ? C.Orange : C.Ligne, (j == vIdx) ? Zx.P(2.5f) : 1f);
				g.DrawPath(pen2, path3);
			}
		}
		Carte(g, rAchat, Zx.P(14));
		string t4 = ((!sel.Payant) ? "Gratuit" : (sel.Debloque ? "Acheté ✔" : sel.PrixTexte));
		Txt.Dessiner(g, t4, fPrix, new Rectangle(rAchat.X + Zx.P(20), rAchat.Y + Zx.P(14), rAchat.Width - Zx.P(40), Zx.P(40)), (sel.Payant && !sel.Debloque) ? C.Orange : C.Vert, TextFormatFlags.NoPadding);
		if (sel.Payant && !sel.Debloque && sel.AncienPrix > sel.Prix)
		{
			using Font f = new Font("Segoe UI", Zx.P(11f), FontStyle.Strikeout);
			int x2 = rAchat.X + 20 + Lang.MeasureText(t4, fPrix).Width + 10;
			Txt.Dessiner(g, sel.AncienPrix.ToString("0.00", CultureInfo.GetCultureInfo("fr-FR")) + " €", f, new Point(x2, rAchat.Y + Zx.P(26)), C.Gris);
		}
		Size size2 = Lang.MeasureText(t, fCarteT);
		Rectangle r2 = new Rectangle(rAchat.Right - Zx.P(20) - size2.Width - Zx.P(22), rAchat.Y + Zx.P(22), size2.Width + Zx.P(22), Zx.P(24));
		using (GraphicsPath path4 = Bouton.Rond(r2, Zx.P(12)))
		{
			using (SolidBrush brush8 = new SolidBrush(Color.FromArgb(40, c)))
			{
				g.FillPath(brush8, path4);
			}
			using Pen pen3 = new Pen(Color.FromArgb(130, c));
			g.DrawPath(pen3, path4);
		}
		Txt.Dessiner(g, t, fCarteT, r2, (c == C.Gris) ? C.Texte : c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		int num7 = Telechargements(sel);
		Txt.Dessiner(g, "↓  " + num7.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + ((num7 > 1) ? " téléchargements" : " téléchargement"), fLigne, rSocial, C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Carte(g, rInfo, Zx.P(14));
		Txt.Dessiner(g, "INFORMATIONS", fCarteT, new Point(rInfo.X + Zx.P(20), rInfo.Y + Zx.P(18)), C.Gris);
		List<string[]> list2 = LignesInfo(sel);
		for (int k = 0; k < list2.Count; k++)
		{
			int num8 = rInfo.Y + Zx.P(50) + k * Zx.P(30);
			using (Pen pen4 = new Pen(Color.FromArgb(60, C.Ligne)))
			{
				g.DrawLine(pen4, rInfo.X + Zx.P(20), num8 - Zx.P(3), rInfo.Right - Zx.P(20), num8 - Zx.P(3));
			}
			Txt.Dessiner(g, list2[k][0], fLigne, new Rectangle(rInfo.X + Zx.P(20), num8, Zx.P(110), Zx.P(24)), C.Gris, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Txt.Dessiner(g, list2[k][1], fLigne, new Rectangle(rInfo.X + Zx.P(132), num8, rInfo.Width - Zx.P(152), Zx.P(24)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		rLiens.Clear();
		if (sel.Liens.Count > 0)
		{
			int num9 = rInfo.Y + Zx.P(50) + list2.Count * Zx.P(30) + Zx.P(10);
			Txt.Dessiner(g, "LIENS UTILES", fCarteT, new Point(rInfo.X + Zx.P(20), num9), C.Gris);
			num9 += Zx.P(26);
			foreach (string[] lien in sel.Liens)
			{
				Rectangle rectangle3 = new Rectangle(rInfo.X + Zx.P(20), num9, rInfo.Width - Zx.P(40), Zx.P(30));
				using (GraphicsPath path5 = Bouton.Rond(rectangle3, Zx.P(15)))
				{
					using (SolidBrush brush9 = new SolidBrush(Color.FromArgb(40, C.Jade)))
					{
						g.FillPath(brush9, path5);
					}
					using Pen pen5 = new Pen(Color.FromArgb(120, C.Jade));
					g.DrawPath(pen5, path5);
				}
				Txt.Dessiner(g, "↗   " + lien[0], fLigne, new Rectangle(rectangle3.X + Zx.P(14), rectangle3.Y, rectangle3.Width - Zx.P(24), rectangle3.Height), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				rLiens.Add(new KeyValuePair<Rectangle, string>(rectangle3, lien[1]));
				num9 += Zx.P(36);
			}
		}
		if (sel.Dlc.Count > 0)
		{
			int num10 = rInfo.Y + Zx.P(50) + list2.Count * Zx.P(30) + Zx.P(10) + ((sel.Liens.Count > 0) ? (Zx.P(34) + sel.Liens.Count * Zx.P(36)) : 0);
			for (int l = 0; l < 2; l++)
			{
				bool flag = l == 1;
				int num11 = 0;
				foreach (string[] item in sel.Dlc)
				{
					if (item[2] == "1" == flag)
					{
						num11++;
					}
				}
				if (num11 == 0)
				{
					continue;
				}
				Txt.Dessiner(g, flag ? "DLC STEAM FACULTATIFS  ·  trafic, personnages" : "DLC STEAM OBLIGATOIRES", fCarteT, new Point(rInfo.X + Zx.P(20), num10), flag ? C.Gris : C.Orange);
				num10 += Zx.P(26);
				foreach (string[] item2 in sel.Dlc)
				{
					if (item2[2] == "1" != flag)
					{
						continue;
					}
					Rectangle rectangle4 = new Rectangle(rInfo.X + Zx.P(20), num10, rInfo.Width - Zx.P(40), Zx.P(28));
					using (GraphicsPath path6 = Bouton.Rond(rectangle4, Zx.P(14)))
					{
						using (SolidBrush brush10 = new SolidBrush(Color.FromArgb(34, 102, 192, 244)))
						{
							g.FillPath(brush10, path6);
						}
						using Pen pen6 = new Pen(Color.FromArgb(110, 102, 192, 244));
						g.DrawPath(pen6, path6);
					}
					Txt.Dessiner(g, "↗   " + item2[1], fLigne, new Rectangle(rectangle4.X + Zx.P(14), rectangle4.Y, rectangle4.Width - Zx.P(24), rectangle4.Height), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
					rLiens.Add(new KeyValuePair<Rectangle, string>(rectangle4, "dlc:" + item2[0]));
					num10 += Zx.P(32);
				}
			}
		}
		g.Restore(gstate);
	}

	private List<string[]> LignesInfo(Projet p)
	{
		List<string[]> list = new List<string[]>();
		CultureInfo cultureInfo = CultureInfo.GetCultureInfo("fr-FR");
		list.Add(new string[2]
		{
			"Jeu",
			NomJeu(JeuDe(p))
		});
		list.Add(new string[2]
		{
			"Catégorie",
			Categories.Nom(p.Categorie)
		});
		list.Add(new string[2]
		{
			"Version",
			(p.Version.Length > 0) ? p.Version : "—"
		});
		list.Add(new string[2]
		{
			"Taille",
			(p.Taille > 0) ? (p.Taille / 1048576 + " Mo") : "—"
		});
		list.Add(new string[2]
		{
			"Mis en ligne",
			(p.Date.Length > 0) ? p.Date.Split(' ')[0] : "—"
		});
		list.Add(new string[2]
		{
			"Éditeur",
			(p.Editeur.Length > 0) ? p.Editeur : "TPS Groupe"
		});
		if (p.Prerequis.Length > 0)
		{
			list.Add(new string[2] { "Prérequis", p.Prerequis });
		}
		list.Add(new string[2]
		{
			"Tarif",
			p.Payant ? p.PrixTexte : "Gratuit"
		});
		list.Add(new string[2]
		{
			"Téléchargements",
			Telechargements(p).ToString("N0", cultureInfo)
		});
		return list;
	}

	private void OuvrirSupport()
	{
		if (sel == null)
		{
			return;
		}
		string text = Omsi.DetecteOmsi();
		string[] array = ((text != null) ? Omsi.Installee(sel, text) : null);
		string text2 = "Lanceur v" + Config.VERSION + "  ·  mod " + ((array != null) ? ("installé " + ((array[0].Trim().Length > 0) ? array[0].Trim() : "?") + (Omsi.Desactive(sel, text) ? " (désactivé)" : "")) : "non installé") + "  ·  catalogue " + ((sel.Version.Length > 0) ? sel.Version : "?") + "  ·  " + Environment.OSVersion.VersionString + "  ·  OMSI 2 " + ((text != null) ? "trouvé" : "introuvable");
		using FenSupport f = new FenSupport(sel, text2);
		f.Afficher(this);
	}

	public void AllerAccueil()
	{
		if (couches.Count == 0)
		{
			ShowVue(Vue.Accueil);
			return;
		}
		VersAccueil = true;
		PanelFond[] array = couches.ToArray();
		foreach (PanelFond panelFond in array)
		{
			foreach (Control control in panelFond.Controls)
			{
				if (control is Form { IsDisposed: false } form)
				{
					form.DialogResult = DialogResult.Cancel;
				}
			}
		}
	}

	private List<Projet> ModsLies(Projet p, out bool auto)
	{
		List<Projet> list = new List<Projet>();
		auto = false;
		if (cat == null || p == null)
		{
			return list;
		}
		foreach (string ly in p.Lies)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (projet.Id == ly && projet != p && projet.StatutEffectif != "masque" && !list.Contains(projet))
				{
					list.Add(projet);
				}
			}
		}
		foreach (Projet projet2 in cat.Projets)
		{
			if (projet2 != p && projet2.Lies.Contains(p.Id) && projet2.StatutEffectif != "masque" && !list.Contains(projet2))
			{
				list.Add(projet2);
			}
		}
		return list;
	}

	private void MajLies()
	{
		if (bLies != null && sel != null)
		{
			bool auto;
			int count = ModsLies(sel, out auto).Count;
			bLies.Text = ((count > 0) ? ("Mods liés  ·  " + count) : "Aucun mod lié");
			bLies.Enabled = count > 0;
			bLies.Invalidate();
		}
	}

	private void OuvrirLies()
	{
		if (sel == null)
		{
			return;
		}
		bool auto;
		List<Projet> list = ModsLies(sel, out auto);
		string omsi = Omsi.DetecteOmsi();
		Func<Projet, int> func = delegate(Projet p)
		{
			LibelleEtat(p, omsi, out var _, out var code);
			switch (code)
			{
			case 1:
			case 2:
				return code;
			case 0:
				if (p.Installable && (!p.Payant || p.Debloque))
				{
					break;
				}
				goto default;
			default:
				return 3;
			}
			return 0;
		};
		Image value;
		string text;
		using (FenLies fenLies = new FenLies(sel.Nom, list, (Projet p) => (!images.TryGetValue(p.Id, out value)) ? null : value, func))
		{
			fenLies.Afficher(this);
			text = fenLies.Demande;
		}
		bool flag = text.StartsWith("installer:");
		if (flag)
		{
			text = text.Substring(10);
		}
		if (text.Length <= 0 || cat == null)
		{
			return;
		}
		foreach (Projet p2 in cat.Projets)
		{
			if (!(p2.Id == text))
			{
				continue;
			}
			Choisir(p2);
			ShowVue(Vue.Projet);
			if (!flag || occupe)
			{
				break;
			}
			BeginInvoke((MethodInvoker)delegate
			{
				if (sel == p2)
				{
					Installer(p2);
				}
			});
			break;
		}
	}

	private void OuvrirVideo()
	{
		if (sel == null || sel.Video.Length == 0)
		{
			return;
		}
		using FenVideo f = new FenVideo(sel.Nom, sel.Video);
		f.Afficher(this);
	}

	private void InitPage()
	{
		page.MouseMove += delegate(object o, MouseEventArgs e)
		{
			Point pt2 = new Point(e.X, e.Y - page.AutoScrollPosition.Y);
			bool flag = rVue.Contains(pt2);
			bool flag2 = false;
			foreach (Rectangle item in rMini)
			{
				if (item.Contains(pt2))
				{
					flag2 = true;
				}
			}
			foreach (KeyValuePair<Rectangle, string> rLien in rLiens)
			{
				if (rLien.Key.Contains(pt2))
				{
					flag2 = true;
				}
			}
			foreach (KeyValuePair<Rectangle, string> rTag in rTags)
			{
				if (rTag.Key.Contains(pt2))
				{
					flag2 = true;
				}
			}
			page.Cursor = ((flag || flag2) ? Cursors.Hand : Cursors.Default);
		};
		page.MouseClick += delegate(object o, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && sel != null && couches.Count <= 0)
			{
				Point pt = new Point(e.X, e.Y - page.AutoScrollPosition.Y);
				List<Image> list = DiapoPage();
				if (!rBVideo.IsEmpty && rBVideo.Contains(pt))
				{
					OuvrirVideo();
				}
				else
				{
					foreach (KeyValuePair<Rectangle, string> rTag2 in rTags)
					{
						if (rTag2.Key.Contains(pt))
						{
							string text = JeuDe(sel);
							if (text.Length > 0)
							{
								jeuId = text;
							}
							categorie = "";
							filtre = 0;
							ShowVue(Vue.Biblio);
							if (tRecherche != null)
							{
								tRecherche.Text = rTag2.Value;
							}
							return;
						}
					}
					foreach (KeyValuePair<Rectangle, string> rLien2 in rLiens)
					{
						if (rLien2.Key.Contains(pt))
						{
							if (rLien2.Value.StartsWith("dlc:"))
							{
								string text2 = rLien2.Value.Substring(4);
								try
								{
									Process.Start("steam://store/" + text2);
									return;
								}
								catch
								{
									try
									{
										Process.Start("https://store.steampowered.com/app/" + text2);
										return;
									}
									catch
									{
										return;
									}
								}
							}
							try
							{
								Process.Start(rLien2.Value);
								return;
							}
							catch
							{
								return;
							}
						}
					}
					if (list.Count > 1 && rVPrev.Contains(pt))
					{
						vIdx = (vIdx + list.Count - 1) % list.Count;
						page.Invalidate();
					}
					else if (list.Count > 1 && rVNext.Contains(pt))
					{
						vIdx = (vIdx + 1) % list.Count;
						page.Invalidate();
					}
					else
					{
						for (int i = 0; i < rMini.Count; i++)
						{
							if (rMini[i].Contains(pt))
							{
								if (videoP != null && videoP.Visible)
								{
									using (FenVisionneuse f2 = new FenVisionneuse(list, Math.Min(i, list.Count - 1)))
									{
										f2.Afficher(this);
									}
									return;
								}
								vIdx = i;
								page.Invalidate();
								return;
							}
						}
						if (rVue.Contains(pt) && list.Count > 0)
						{
							using (FenVisionneuse f = new FenVisionneuse(list, Math.Min(vIdx, list.Count - 1)))
							{
								f.Afficher(this);
							}
						}
					}
				}
			}
		};
	}

	private void Hero(Graphics g, Image im, int w, int HB)
	{
		if (w < 1 || HB < 1)
		{
			return;
		}
		if (heroCache == null || heroSrc != im || heroW != w || heroH != HB)
		{
			if (heroCache != null)
			{
				heroCache.Dispose();
			}
			Bitmap image = new Bitmap(w, HB, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(image))
			{
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				if (im != null)
				{
					double num = Math.Max((double)w / (double)im.Width, (double)HB / (double)im.Height);
					int num2 = (int)((double)im.Width * num);
					int num3 = (int)((double)im.Height * num);
					graphics.DrawImage(im, (w - num2) / 2, (HB - num3) / 2, num2, num3);
				}
				else
				{
					using SolidBrush brush = new SolidBrush(C.Carte);
					graphics.FillRectangle(brush, 0, 0, w, HB);
				}
				using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, w, HB + 1), Color.FromArgb(40, C.Fond), C.Fond, 90f);
				ColorBlend colorBlend = new ColorBlend(3);
				colorBlend.Colors = new Color[3]
				{
					Color.FromArgb(30, C.Fond),
					Color.FromArgb(120, C.Fond),
					C.Fond
				};
				colorBlend.Positions = new float[3] { 0f, 0.55f, 1f };
				linearGradientBrush.InterpolationColors = colorBlend;
				graphics.FillRectangle(linearGradientBrush, 0, 0, w, HB);
			}
			heroCache = image;
			heroSrc = im;
			heroW = w;
			heroH = HB;
		}
		g.DrawImageUnscaled(heroCache, 0, 0);
	}

	private static Bitmap Snap(Control c)
	{
		Bitmap bitmap = new Bitmap(Math.Max(1, c.Width), Math.Max(1, c.Height), PixelFormat.Format32bppPArgb);
		c.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
		return bitmap;
	}

	private static void AlphaDraw(Graphics g, Bitmap b, Rectangle dest, float a)
	{
		if (a >= 0.995f)
		{
			g.DrawImage(b, dest, 0, 0, b.Width, b.Height, GraphicsUnit.Pixel);
		}
		else if (!(a <= 0.004f))
		{
			using (ImageAttributes imageAttributes = new ImageAttributes())
			{
				ColorMatrix colorMatrix = new ColorMatrix();
				colorMatrix.Matrix33 = a;
				imageAttributes.SetColorMatrix(colorMatrix);
				g.DrawImage(b, dest, 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, imageAttributes);
			}
		}
	}

	private static double Ease(double t)
	{
		double num = 1.0 - t;
		return 1.0 - num * num * num * num;
	}

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr h);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr h, IntPtr dc);

	[DllImport("gdi32.dll")]
	private static extern int GetDeviceCaps(IntPtr dc, int i);

	private static int PasImage()
	{
		if (pasImage < 0)
		{
			int num = 60;
			try
			{
				IntPtr dC = GetDC(IntPtr.Zero);
				num = GetDeviceCaps(dC, 116);
				ReleaseDC(IntPtr.Zero, dC);
			}
			catch
			{
			}
			if (num < 50 || num > 400)
			{
				num = 60;
			}
			pasImage = Math.Max(6, (int)Math.Round(1000.0 / (double)Math.Min(num, 144)));
		}
		return pasImage;
	}

	private static double EaseIO(double t)
	{
		if (!(t < 0.5))
		{
			return 1.0 - Math.Pow(-2.0 * t + 2.0, 3.0) / 2.0;
		}
		return 4.0 * t * t * t;
	}

	private static double Cl(double x)
	{
		if (!(x < 0.0))
		{
			if (!(x > 1.0))
			{
				return x;
			}
			return 1.0;
		}
		return 0.0;
	}

	private bool Animer()
	{
		if ((ForceAnim || !ModeCapture) && AnimationActive() && base.WindowState != FormWindowState.Minimized && base.IsHandleCreated)
		{
			if (!base.Visible)
			{
				return ForceAnim;
			}
			return true;
		}
		return false;
	}

	private void Jouer(int ms, Action<double> etape)
	{
		Jouer(ms, etape, entreeSortie: false);
	}

	private void Jouer(int ms, Action<double> etape, bool entreeSortie)
	{
		try
		{
			Winmm.timeBeginPeriod(1u);
		}
		catch
		{
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		long num = 0L;
		int num2 = 0;
		while (!base.IsDisposed)
		{
			double num3 = Math.Min(1.0, (double)stopwatch.ElapsedMilliseconds / (double)ms);
			etape(entreeSortie ? EaseIO(num3) : Ease(num3));
			num2++;
			if (num3 >= 1.0)
			{
				break;
			}
			num += PasImage();
			if (stopwatch.ElapsedMilliseconds > num + 80)
			{
				num = stopwatch.ElapsedMilliseconds;
			}
			while (stopwatch.ElapsedMilliseconds < num && !base.IsDisposed)
			{
				Application.DoEvents();
				Thread.Sleep(1);
			}
		}
		try
		{
			Winmm.timeEndPeriod(1u);
		}
		catch
		{
		}
		StatJeu = num2 + " images en " + stopwatch.ElapsedMilliseconds + " ms";
	}

	private static GraphicsPath Zone(int w, int h, double p, bool gauche)
	{
		float num = (float)h / 4f;
		float num2 = (float)((double)(0f - num) + p * (double)((float)w + num));
		GraphicsPath graphicsPath = new GraphicsPath();
		if (gauche)
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(-1f, -1f),
				new PointF(num2 + num, -1f),
				new PointF(num2, h + 1),
				new PointF(-1f, h + 1)
			});
		}
		else
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(num2 + num, -1f),
				new PointF((float)w + num + 1f, -1f),
				new PointF((float)w + num + 1f, h + 1),
				new PointF(num2, h + 1)
			});
		}
		return graphicsPath;
	}

	private static PointF[] Bande(float x, float pente, int h, float a, float b)
	{
		return new PointF[4]
		{
			new PointF(x + pente + a, -1f),
			new PointF(x + pente + b, -1f),
			new PointF(x + b, h + 1),
			new PointF(x + a, h + 1)
		};
	}

	private static void Bord(Graphics g, int w, int h, double p)
	{
		if (p <= 0.002 || p >= 0.998)
		{
			return;
		}
		float num = (float)h / 4f;
		float x = (float)((double)(0f - num) + p * (double)((float)w + num));
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		for (int num2 = 8; num2 >= 1; num2--)
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb(9, 0, 0, 0));
			g.FillPolygon(brush, Bande(x, num, h, 4f, 4 + num2 * 7));
		}
		for (int num3 = 8; num3 >= 1; num3--)
		{
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(12, C.Orange));
			g.FillPolygon(brush2, Bande(x, num, h, -num3 * 6, 0f));
		}
		using (SolidBrush brush3 = new SolidBrush(C.Orange))
		{
			g.FillPolygon(brush3, Bande(x, num, h, 0f, 4.5f));
		}
		using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(210, 255, 222, 190)))
		{
			g.FillPolygon(brush4, Bande(x, num, h, 3.2f, 4.5f));
		}
		g.SmoothingMode = smoothingMode;
	}

	public DialogResult Integrer(Form f)
	{
		if (VersAccueil)
		{
			return DialogResult.Cancel;
		}
		bool page = f is IPage;
		bool anim = Animer();
		Size clientSize = f.ClientSize;
		Bitmap bg = null;
		Bitmap snap = null;
		double vp = 0.0;
		Point sp = Point.Empty;
		Rectangle carte = Rectangle.Empty;
		bool ferme = false;
		if (anim)
		{
			try
			{
				bg = Snap((couches.Count > 0) ? couches[couches.Count - 1] : zone);
			}
			catch
			{
				bg = null;
			}
		}
		PanelFond c = new PanelFond();
		c.BackColor = C.Fond;
		c.Bounds = new Rectangle(0, 0, base.ClientSize.Width, base.ClientSize.Height);
		c.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		c.Peindre = delegate(Graphics g, int d)
		{
			if (bg != null && bg.Size == c.ClientSize)
			{
				g.DrawImageUnscaled(bg, 0, 0);
			}
			else
			{
				FondPeint(g, c);
			}
			if (!page)
			{
				using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(170.0 * vp), 3, 4, 12)))
				{
					g.FillRectangle(brush, c.ClientRectangle);
				}
				if (!carte.IsEmpty)
				{
					for (int i = 1; i <= 9; i++)
					{
						using SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)((double)(20 - i * 2) * vp), 0, 0, 0));
						g.FillRectangle(brush2, Rectangle.Inflate(carte, i * 2, i * 2));
					}
					using Pen pen = new Pen(Color.FromArgb((int)(255.0 * vp), C.Ligne));
					g.DrawRectangle(pen, carte.X - 1, carte.Y - 1, carte.Width + 1, carte.Height + 1);
				}
			}
			if (snap != null)
			{
				int width = c.ClientSize.Width;
				int height = c.ClientSize.Height;
				if (page)
				{
					double p2 = (ferme ? (1.0 - vp) : vp);
					if (!ferme)
					{
						using SolidBrush brush3 = new SolidBrush(Color.FromArgb((int)(110.0 * vp), 2, 3, 10));
						g.FillRectangle(brush3, 0, 0, width, height);
					}
					int num2 = ((!ferme) ? ((int)Math.Round(36.0 * (1.0 - vp))) : 0);
					using (GraphicsPath path = Zone(width, height, p2, !ferme))
					{
						GraphicsState gstate = g.Save();
						g.SetClip(path, CombineMode.Replace);
						g.DrawImageUnscaled(snap, sp.X + num2, sp.Y);
						if (num2 > 0)
						{
							using SolidBrush brush4 = new SolidBrush(C.Fond);
							g.FillRectangle(brush4, 0, 0, num2, height);
						}
						g.Restore(gstate);
					}
					Bord(g, width, height, p2);
				}
				else if (vp > 0.003)
				{
					float num3 = (float)(0.94 + 0.06 * vp);
					int num4 = (int)((float)snap.Width * num3);
					int num5 = (int)((float)snap.Height * num3);
					Rectangle dest = new Rectangle(sp.X + (snap.Width - num4) / 2, sp.Y + (snap.Height - num5) / 2 + (int)((double)Zx.P(10) * (1.0 - vp)), num4, num5);
					InterpolationMode interpolationMode = g.InterpolationMode;
					g.InterpolationMode = ((vp > 0.995) ? InterpolationMode.NearestNeighbor : InterpolationMode.Bilinear);
					AlphaDraw(g, snap, dest, (float)Math.Min(1.0, vp * 1.4));
					g.InterpolationMode = interpolationMode;
				}
			}
		};
		f.TopLevel = false;
		f.FormBorderStyle = FormBorderStyle.None;
		f.MinimumSize = Size.Empty;
		f.MaximumSize = Size.Empty;
		f.StartPosition = FormStartPosition.Manual;
		f.ShowInTaskbar = false;
		f.KeyPreview = true;
		Size clientSize2 = c.ClientSize;
		if (page)
		{
			f.ClientSize = clientSize2;
		}
		else
		{
			f.ClientSize = new Size(Math.Min(clientSize.Width, clientSize2.Width - Zx.P(24)), Math.Min(clientSize.Height, clientSize2.Height - Zx.P(24)));
		}
		Point point = (page ? Point.Empty : new Point((clientSize2.Width - f.Width) / 2, (clientSize2.Height - f.Height) / 2));
		DialogResult res = DialogResult.Cancel;
		bool fini = false;
		Bitmap snapFin = null;
		Point posFin = point;
		f.KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				f.DialogResult = DialogResult.Cancel;
				e.Handled = true;
			}
		};
		f.FormClosing += delegate(object o, FormClosingEventArgs e)
		{
			if (!e.Cancel && anim && snapFin == null)
			{
				try
				{
					snapFin = Snap(f);
					posFin = f.Location;
					carte = (page ? Rectangle.Empty : f.Bounds);
				}
				catch
				{
					snapFin = null;
				}
			}
		};
		f.FormClosed += delegate
		{
			res = ((f.DialogResult == DialogResult.None) ? DialogResult.Cancel : f.DialogResult);
		};
		f.Disposed += delegate
		{
			fini = true;
		};
		c.Resize += delegate
		{
			if (bg != null)
			{
				bg.Dispose();
				bg = null;
			}
		};
		if (!page)
		{
			c.Resize += delegate
			{
				if (!f.IsDisposed)
				{
					f.Location = new Point((c.ClientSize.Width - f.Width) / 2, (c.ClientSize.Height - f.Height) / 2);
				}
			};
		}
		int largeurPage = Integre.Hote.Width;
		if (page)
		{
			c.Resize += delegate
			{
				if (!f.IsDisposed)
				{
					int num = (c.ClientSize.Width - largeurPage) / 2;
					largeurPage = c.ClientSize.Width;
					if (f.ClientSize != c.ClientSize)
					{
						f.ClientSize = c.ClientSize;
					}
					f.Location = Point.Empty;
					if (num != 0)
					{
						foreach (Control control3 in f.Controls)
						{
							if (control3.Dock == DockStyle.None)
							{
								if ((control3.Anchor & AnchorStyles.Right) == 0 || (control3.Anchor & AnchorStyles.Left) == 0)
								{
									control3.Left += num;
								}
							}
							else
							{
								foreach (Control control4 in control3.Controls)
								{
									if (control4.Dock == DockStyle.None && (control4.Anchor & AnchorStyles.Right) == 0)
									{
										control4.Left += num;
									}
								}
							}
						}
						f.Invalidate(invalidateChildren: true);
					}
				}
			};
		}
		base.Controls.Add(c);
		couches.Add(c);
		c.BringToFront();
		c.Controls.Add(f);
		if (anim && bg != null)
		{
			f.Location = new Point(-Zx.P(30000), 0);
			f.Show();
			f.PerformLayout();
			Application.DoEvents();
			try
			{
				snap = Snap(f);
			}
			catch
			{
				snap = null;
			}
			f.Visible = false;
			f.Location = point;
			sp = point;
			if (!page)
			{
				ref Rectangle reference = ref carte;
				reference = new Rectangle(point, f.Size);
			}
			if (snap != null)
			{
				Jouer(page ? 300 : 190, delegate(double p)
				{
					vp = p;
					c.Invalidate(invalidateChildren: false);
					c.Update();
				}, page);
			}
			if (snap != null)
			{
				snap.Dispose();
				snap = null;
			}
			vp = 1.0;
			f.Visible = true;
		}
		else
		{
			vp = 1.0;
			f.Location = point;
			if (!page)
			{
				ref Rectangle reference2 = ref carte;
				reference2 = new Rectangle(point, f.Size);
			}
			f.Show();
		}
		if (page)
		{
			f.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		}
		c.Invalidate();
		f.Select();
		while (!fini && !base.IsDisposed)
		{
			if (f.DialogResult != 0 && !f.IsDisposed)
			{
				f.Close();
				if (!f.IsDisposed)
				{
					f.DialogResult = DialogResult.None;
				}
			}
			Application.DoEvents();
			if (!fini)
			{
				Thread.Sleep(3);
			}
		}
		if (snapFin != null && !base.IsDisposed)
		{
			snap = snapFin;
			sp = posFin;
			ferme = true;
			Jouer(page ? 260 : 140, delegate(double p)
			{
				vp = 1.0 - p;
				c.Invalidate(invalidateChildren: false);
				c.Update();
			}, page);
		}
		couches.Remove(c);
		if (!base.IsDisposed)
		{
			base.Controls.Remove(c);
		}
		c.Dispose();
		if (VersAccueil && couches.Count == 0)
		{
			VersAccueil = false;
			if (!base.IsDisposed)
			{
				BeginInvoke((MethodInvoker)delegate
				{
					ShowVue(Vue.Accueil);
				});
			}
		}
		if (bg != null)
		{
			bg.Dispose();
		}
		if (snap != null)
		{
			snap.Dispose();
		}
		if (!base.IsDisposed)
		{
			Activate();
			Focus();
		}
		return res;
	}

	private void Transition(Bitmap avant)
	{
		Transition(avant, StyleTrans.Diagonale);
	}

	private void Transition(Bitmap avant, StyleTrans style)
	{
		Bitmap apres = null;
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			zone.PerformLayout();
			zone.Update();
			apres = Snap(zone);
		}
		catch
		{
			apres = null;
		}
		StatSnap = stopwatch.ElapsedMilliseconds;
		if (apres == null || avant.Size != apres.Size)
		{
			avant.Dispose();
			if (apres != null)
			{
				apres.Dispose();
			}
			return;
		}
		enTransition = true;
		double vp = 0.0;
		Voile vo = new Voile();
		vo.Dock = DockStyle.Fill;
		vo.Peindre = delegate(Graphics g)
		{
			int width = apres.Width;
			int height = apres.Height;
			PeindreTrans(g, style, avant, apres, vp);
			if (neigeTrans > 0)
			{
				noel.PeindreNeige(g, width, height, (neigeTrans == 2) ? Math.Min(1.0, vp * 1.5) : (1.0 - vp));
			}
		};
		zone.Controls.Add(vo);
		vo.BringToFront();
		vo.Update();
		long derT = 0L;
		Stopwatch swN = Stopwatch.StartNew();
		Jouer((neigeTrans > 0) ? 520 : ((style == StyleTrans.Diagonale) ? 320 : 360), delegate(double p)
		{
			if (neigeTrans > 0)
			{
				long elapsedMilliseconds = swN.ElapsedMilliseconds;
				noel.Avancer((double)(elapsedMilliseconds - derT) / 1000.0, apres.Width, apres.Height);
				derT = elapsedMilliseconds;
				ThemeNoel.AlphaBlocs = ((neigeTrans == 1) ? ((float)Math.Max(0.0, 1.0 - p * p)) : 1f);
			}
			vp = p;
			vo.Invalidate();
			vo.Update();
		}, style == StyleTrans.Diagonale || neigeTrans > 0);
		ThemeNoel.AlphaBlocs = 1f;
		if (IntroDossier != null)
		{
			Journal.Ecrire("transition", new Exception("snapshot " + StatSnap + " ms ; " + StatJeu));
		}
		zone.Controls.Remove(vo);
		vo.Dispose();
		avant.Dispose();
		apres.Dispose();
		enTransition = false;
		PanelVisible()?.Invalidate();
	}

	private void Marque(Graphics g, Size s, double lg, double tt, double ln)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBilinear;
		g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		int num = s.Width / 2;
		int num2 = s.Height / 2 - Zx.P(34);
		using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, s.Width, s.Height + 1), Color.FromArgb(6, 8, 20), Color.FromArgb(16, 20, 46), 90f))
		{
			g.FillRectangle(brush, 0, 0, s.Width, s.Height);
		}
		if (logo != null)
		{
			if (logoMarque == null)
			{
				logoMarque = new Bitmap(Zx.P(176), Zx.P(176), PixelFormat.Format32bppPArgb);
				using Graphics graphics = Graphics.FromImage(logoMarque);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(logo, 0, 0, Zx.P(176), Zx.P(176));
			}
			int num3 = (int)((double)Zx.P(176) * (0.92 + 0.08 * lg));
			Rectangle rectangle = new Rectangle(num - num3 / 2, num2 - num3 / 2, num3, num3);
			using GraphicsPath path = Bouton.Rond(rectangle, num3 / 5);
			GraphicsState gstate = g.Save();
			g.SetClip(path, CombineMode.Intersect);
			AlphaDraw(g, logoMarque, rectangle, (float)lg);
			g.Restore(gstate);
		}
		string text = "TPS GROUPE";
		float num4 = 4f;
		float num5 = 0f;
		float[] array = new float[text.Length];
		StringFormat genericTypographic = StringFormat.GenericTypographic;
		for (int i = 0; i < text.Length; i++)
		{
			array[i] = ((text[i] == ' ') ? 12f : g.MeasureString(text[i].ToString(), fMarque, 100, genericTypographic).Width);
			num5 += array[i] + ((i < text.Length - 1) ? num4 : 0f);
		}
		float num6 = (float)num - num5 / 2f;
		float num7 = (float)(num2 + Zx.P(112)) + (float)((1.0 - tt) * (double)Zx.P(10));
		using (SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)(255.0 * tt), C.Texte)))
		{
			for (int j = 0; j < text.Length; j++)
			{
				g.DrawString(text[j].ToString(), fMarque, brush2, num6, num7, genericTypographic);
				num6 += array[j] + num4;
			}
		}
		int num8 = (int)((double)num5 * ln);
		if (num8 > 2)
		{
			using (SolidBrush brush3 = new SolidBrush(C.Orange))
			{
				g.FillRectangle(brush3, num - num8 / 2, (int)num7 + Zx.P(50), num8, Zx.P(3));
			}
		}
	}

	private void JouerMarque(Voile vo, Func<double, bool> etape, double skipDe, bool permettreSaut)
	{
		try
		{
			Winmm.timeBeginPeriod(1u);
		}
		catch
		{
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		long num = 0L;
		double num2 = 0.0;
		while (!base.IsDisposed)
		{
			double num3 = stopwatch.Elapsed.TotalSeconds + num2;
			if (permettreSaut && passeIntro && num3 < skipDe)
			{
				num2 += skipDe - num3;
				num3 = skipDe;
			}
			if (!etape(num3))
			{
				break;
			}
			vo.Invalidate();
			vo.Update();
			num += PasImage();
			if (stopwatch.ElapsedMilliseconds > num + 80)
			{
				num = stopwatch.ElapsedMilliseconds;
			}
			while (stopwatch.ElapsedMilliseconds < num && !base.IsDisposed)
			{
				Application.DoEvents();
				Thread.Sleep(1);
			}
		}
		try
		{
			Winmm.timeEndPeriod(1u);
		}
		catch
		{
		}
	}

	private void Intro(string prefixe)
	{
		if (!Animer())
		{
			return;
		}
		Bitmap home = null;
		double ts = 0.0;
		int cap = 0;
		double[] reperes = new double[4] { 0.2, 0.5, 0.9, 1.1 };
		passeIntro = false;
		enTransition = true;
		introDebut = DateTime.Now;
		Voile vo = new Voile();
		vo.Dock = DockStyle.Fill;
		vo.MouseDown += delegate(object om, MouseEventArgs em)
		{
			if ((DateTime.Now - introDebut).TotalMilliseconds > 350.0)
			{
				passeIntro = true;
				raisonPasse = "souris " + em.Button;
			}
		};
		vo.Peindre = delegate(Graphics g)
		{
			Size clientSize = vo.ClientSize;
			double num = Cl((ts - 0.95) / 0.32);
			if (home != null)
			{
				g.DrawImageUnscaled(home, 0, 0);
			}
			else
			{
				g.Clear(Color.FromArgb(6, 8, 20));
			}
			if (num < 1.0)
			{
				using (GraphicsPath path = Zone(clientSize.Width, clientSize.Height, EaseIO(num), gauche: false))
				{
					GraphicsState gstate = g.Save();
					g.SetClip(path, CombineMode.Replace);
					Marque(g, clientSize, Ease(Cl(ts / 0.3)), Ease(Cl((ts - 0.18) / 0.3)), Ease(Cl((ts - 0.35) / 0.4)));
					g.Restore(gstate);
				}
				Bord(g, clientSize.Width, clientSize.Height, EaseIO(num));
			}
		};
		base.Controls.Add(vo);
		vo.BringToFront();
		vo.Update();
		if (base.Opacity < 1.0)
		{
			base.Opacity = 1.0;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		JouerMarque(vo, delegate(double t)
		{
			ts = t;
			if (ts >= 0.6 && home == null)
			{
				try
				{
					home = Snap(zone);
				}
				catch
				{
				}
			}
			if (ts >= 1.3)
			{
				return false;
			}
			if (prefixe != null && IntroDossier != null && cap < reperes.Length && ts >= reperes[cap])
			{
				vo.Invalidate();
				vo.Update();
				try
				{
					using Bitmap bitmap = Snap(vo);
					bitmap.Save(Path.Combine(IntroDossier, prefixe + cap + ".png"));
				}
				catch
				{
				}
				cap++;
			}
			return true;
		}, 0.95, permettreSaut: true);
		if (Environment.GetEnvironmentVariable("TPS_TRACE") != null)
		{
			Journal.Ecrire("intro", new Exception("duree reelle " + stopwatch.ElapsedMilliseconds + " ms ; abregee=" + passeIntro + " " + raisonPasse));
		}
		base.Controls.Remove(vo);
		vo.Dispose();
		if (home != null)
		{
			home.Dispose();
		}
		enTransition = false;
		passeIntro = false;
		PanelVisible()?.Invalidate();
	}

	private void Outro(string prefixe)
	{
		if (!Animer())
		{
			return;
		}
		Bitmap base0 = null;
		try
		{
			base0 = Snap((couches.Count > 0) ? couches[couches.Count - 1] : zone);
		}
		catch
		{
			base0 = null;
		}
		double ts = 0.0;
		int cap = 0;
		double[] reperes = new double[3] { 0.15, 0.3, 0.5 };
		passeIntro = false;
		enTransition = true;
		Voile vo = new Voile();
		vo.Dock = DockStyle.Fill;
		vo.Peindre = delegate(Graphics g)
		{
			Size clientSize = vo.ClientSize;
			double t2 = Cl(ts / 0.3);
			if (base0 != null)
			{
				g.DrawImageUnscaled(base0, 0, 0);
			}
			else
			{
				g.Clear(C.Fond);
			}
			using (GraphicsPath path = Zone(clientSize.Width, clientSize.Height, EaseIO(t2), gauche: true))
			{
				GraphicsState gstate = g.Save();
				g.SetClip(path, CombineMode.Replace);
				Marque(g, clientSize, Ease(Cl((ts - 0.12) / 0.25)), Ease(Cl((ts - 0.22) / 0.25)), Ease(Cl((ts - 0.3) / 0.25)));
				g.Restore(gstate);
			}
			Bord(g, clientSize.Width, clientSize.Height, EaseIO(t2));
		};
		base.Controls.Add(vo);
		vo.BringToFront();
		vo.Update();
		JouerMarque(vo, delegate(double t)
		{
			ts = t;
			if (ts >= 0.62)
			{
				return false;
			}
			if (ts > 0.5 && prefixe == null)
			{
				base.Opacity = 1.0 - Cl((ts - 0.5) / 0.12);
			}
			if (prefixe != null && IntroDossier != null && cap < reperes.Length && ts >= reperes[cap])
			{
				vo.Invalidate();
				vo.Update();
				try
				{
					using Bitmap bitmap = Snap(vo);
					bitmap.Save(Path.Combine(IntroDossier, prefixe + cap + ".png"));
				}
				catch
				{
				}
				cap++;
			}
			return true;
		}, 0.0, permettreSaut: false);
		base.Controls.Remove(vo);
		vo.Dispose();
		if (base0 != null)
		{
			base0.Dispose();
		}
		enTransition = false;
	}

	public void CaptureBannieres(string dossier)
	{
		double[] array = new double[2] { 1.5, 4.0 };
		for (int i = 0; i < array.Length; i++)
		{
			using Bitmap bitmap = new Bitmap(2200, (BanAnim.Ids.Length + 4) / 5 * 150, PixelFormat.Format32bppPArgb);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(C.Fond);
				for (int j = 0; j < BanAnim.Ids.Length; j++)
				{
					Rectangle r = new Rectangle(j % 5 * Zx.P(440) + Zx.P(10), j / 5 * Zx.P(150) + Zx.P(6), Zx.P(420), Zx.P(120));
					using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
					{
						GraphicsState gstate = graphics.Save();
						graphics.SetClip(path, CombineMode.Intersect);
						BanAnim.Dessiner(BanAnim.Ids[j], graphics, r, array[i]);
						graphics.Restore(gstate);
					}
					Lang.DrawText(graphics, BanAnim.Noms[j], F.Etat, new Rectangle(r.X, r.Bottom + Zx.P(4), r.Width, Zx.P(18)), C.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
				}
			}
			bitmap.Save(Path.Combine(dossier, "bannieres_" + i + ".png"));
		}
		for (int k = 0; k < array.Length; k++)
		{
			using Bitmap bitmap2 = new Bitmap(1500, 200, PixelFormat.Format32bppPArgb);
			using (Graphics graphics2 = Graphics.FromImage(bitmap2))
			{
				graphics2.Clear(C.Fond);
				for (int l = 0; l < 10; l++)
				{
					int num = l * 10 + 5;
					Rectangle rc = new Rectangle(l * Zx.P(150) + Zx.P(20), Zx.P(10), Zx.P(110), Zx.P(110));
					Badge.Dessiner(graphics2, rc, num, array[k]);
					Rectangle rectangle = new Rectangle(l * Zx.P(150) + Zx.P(45), Zx.P(130), Zx.P(60), Zx.P(60));
					using (SolidBrush brush = new SolidBrush(C.Jade))
					{
						graphics2.FillEllipse(brush, rectangle);
					}
					Badge.Anneau(graphics2, rectangle, num, array[k]);
					Lang.DrawText(graphics2, "niv. " + num, F.Etat, new Rectangle(l * Zx.P(150), Zx.P(118), Zx.P(150), Zx.P(14)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
				}
			}
			bitmap2.Save(Path.Combine(dossier, "badges_" + k + ".png"));
		}
		using Bitmap image = new Bitmap(420, 120, PixelFormat.Format32bppPArgb);
		using Graphics g = Graphics.FromImage(image);
		Stopwatch stopwatch = Stopwatch.StartNew();
		StringBuilder stringBuilder = new StringBuilder();
		for (int m = 0; m < BanAnim.Ids.Length; m++)
		{
			stopwatch.Restart();
			for (int n = 0; n < 60; n++)
			{
				BanAnim.Dessiner(BanAnim.Ids[m], g, new Rectangle(0, 0, Zx.P(420), Zx.P(120)), (double)n * 0.016);
			}
			stringBuilder.AppendLine(BanAnim.Noms[m] + " : " + ((double)stopwatch.ElapsedMilliseconds / 60.0).ToString("0.00") + " ms par image (420x120)");
		}
		File.WriteAllText(Path.Combine(dossier, "bannieres.txt"), stringBuilder.ToString(), Encoding.UTF8);
	}

	public void CaptureNoel(string dossier)
	{
		ShowVue(Vue.Accueil);
		if (noel == null)
		{
			idTheme = "noel";
			noel = Themes.Creer("noel", 0);
			themeJeux = Themes.Creer("noel", 1);
		}
		foreach (Tuile item in tuilesA)
		{
			item.Neige = (noel.NeigeTuiles ? 1f : 0f);
		}
		Application.DoEvents();
		int width = pAccueil.ClientSize.Width;
		int height = pAccueil.ClientSize.Height;
		double[] array = new double[3] { 1.2, 3.0, 4.6 };
		double num = 0.0;
		bool flag = false;
		string environmentVariable = Environment.GetEnvironmentVariable("TPS_INSTANTS");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			List<double> list = new List<double>();
			string[] array2 = environmentVariable.Split(',');
			foreach (string s in array2)
			{
				list.Add(double.Parse(s, CultureInfo.InvariantCulture));
			}
			array = list.ToArray();
			flag = true;
		}
		for (int j = 0; j < array.Length; j++)
		{
			while (num < array[j])
			{
				num = (ThemeNoel.TempsForce = num + 1.0 / 60.0);
				noel.Avancer(1.0 / 60.0, width, height);
				if (!flag && num >= 4.25 && tuilesA.Count > 0)
				{
					flag = true;
					tuilesA[0].Neige = 0f;
					noel.Deneiger(tuilesA[0].Bounds);
				}
			}
			SuivreTuiles();
			foreach (Tuile item2 in tuilesA)
			{
				item2.Invalidate();
			}
			Refresh();
			Application.DoEvents();
			using Bitmap bitmap = new Bitmap(base.Width, base.Height);
			DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
			bitmap.Save(Path.Combine(dossier, "noel_" + j + ".png"));
		}
		ShowVue(Vue.Jeux);
		for (int k = 0; k < 30; k++)
		{
			num = (ThemeNoel.TempsForce = num + 1.0 / 60.0);
			if (themeJeux != null)
			{
				themeJeux.Avancer(1.0 / 60.0, pJeux.ClientSize.Width, pJeux.ClientSize.Height);
			}
		}
		Refresh();
		Application.DoEvents();
		using (Bitmap bitmap2 = new Bitmap(base.Width, base.Height))
		{
			DrawToBitmap(bitmap2, new Rectangle(0, 0, base.Width, base.Height));
			bitmap2.Save(Path.Combine(dossier, "theme_jeux.png"));
		}
		ShowVue(Vue.Accueil);
		dLeve = 1.0;
		dSouris = new Point(rCarte.X + rCarte.Width / 3, rCarte.Y + rCarte.Height / 3);
		pAccueil.Refresh();
		Application.DoEvents();
		using (Bitmap bitmap3 = new Bitmap(base.Width, base.Height))
		{
			DrawToBitmap(bitmap3, new Rectangle(0, 0, base.Width, base.Height));
			bitmap3.Save(Path.Combine(dossier, "carte_3d.png"));
		}
		ThemeNoel.TempsForce = -1.0;
	}

	public void CaptureTransitions()
	{
		ForceAnim = true;
		IntroDossier = Path.GetTempPath();
		Vue[] array = new Vue[6]
		{
			Vue.Jeux,
			Vue.Biblio,
			Vue.Accueil,
			Vue.Biblio,
			Vue.Jeux,
			Vue.Accueil
		};
		Vue[] array2 = array;
		foreach (Vue v in array2)
		{
			ShowVue(v);
			Application.DoEvents();
			Thread.Sleep(200);
		}
	}

	public void CaptureIntro(string dossier)
	{
		ForceAnim = true;
		IntroDossier = dossier;
		Intro("intro_");
		Outro("outro_");
	}

	private PanelFond VuePanel(bool defil)
	{
		PanelFond panelFond = new PanelFond();
		panelFond.Dock = DockStyle.Fill;
		panelFond.BackColor = C.Fond;
		panelFond.AutoScroll = defil;
		if (defil)
		{
			panelFond.AutoScrollMargin = new Size(0, Zx.P(30));
		}
		Theme.Sombre(panelFond);
		panelFond.Visible = false;
		return panelFond;
	}

	private void ShowVue(Vue v)
	{
		if (v == Vue.Projet && sel == null)
		{
			v = Vue.Biblio;
		}
		Bitmap bitmap = null;
		neigeTrans = ((themeNoel && v != vue) ? ((vue == Vue.Accueil) ? 1 : ((v == Vue.Accueil) ? 2 : 0)) : 0);
		if (neigeTrans > 0)
		{
			noel.Fige = true;
		}
		if (v != vue && couches.Count == 0 && !enTransition && Animer())
		{
			try
			{
				SuspendreVideos(s: true);
				bitmap = Snap(zone);
			}
			catch
			{
				bitmap = null;
			}
		}
		Vue ancienne = vue;
		vue = v;
		MajPresence();
		pAccueil.Visible = v == Vue.Accueil;
		pJeux.Visible = v == Vue.Jeux;
		pBiblio.Visible = v == Vue.Biblio;
		page.Visible = v == Vue.Projet;
		nAccueil.Actif = v == Vue.Accueil;
		nJeux.Actif = v == Vue.Jeux;
		nBiblio.Actif = v == Vue.Biblio || v == Vue.Projet;
		nAccueil.Invalidate();
		nJeux.Invalidate();
		nBiblio.Invalidate();
		switch (v)
		{
		case Vue.Accueil:
			ConstruireAccueil();
			break;
		case Vue.Jeux:
			ConstruireJeux();
			break;
		case Vue.Biblio:
			ConstruireBiblio();
			break;
		default:
			Disposer();
			MajEtat();
			page.Invalidate();
			OuvrirSocial();
			break;
		}
		if (v != Vue.Projet)
		{
			FermerSocial();
		}
		if (bitmap != null)
		{
			Transition(bitmap, ChoisirStyle(ancienne, v));
		}
		SuspendreVideos(s: false);
		if (noel != null)
		{
			noel.Fige = false;
		}
		neigeTrans = 0;
	}

	private void RafraichirVues()
	{
		if (vue == Vue.Accueil)
		{
			ConstruireAccueil();
		}
		else if (vue == Vue.Jeux)
		{
			ConstruireJeux();
		}
		else if (vue == Vue.Biblio)
		{
			RemplirBiblio();
		}
	}

	private static string JeuDe(Projet p)
	{
		if (p.Jeu.Length <= 0)
		{
			return "omsi2";
		}
		return p.Jeu;
	}

	private static string ValJeu(string id, string k)
	{
		if (!FichesJeux.TryGetValue(id ?? "", out var value) || !value.TryGetValue(k, out var value2) || value2 == null)
		{
			return "";
		}
		return value2.Trim();
	}

	private static string NomJeu(string id)
	{
		string text = ValJeu(id, "nom");
		if (text.Length <= 0)
		{
			if (!(id == "omsi2"))
			{
				return id;
			}
			return "OMSI 2";
		}
		return text;
	}

	private Dictionary<string, string> JeuWebCourant()
	{
		if (!JeuxWebParId.TryGetValue(jeuId ?? "", out var value))
		{
			return null;
		}
		return value;
	}

	private bool JeuWebOuvert(Dictionary<string, string> j)
	{
		if (!j.TryGetValue("statut", out var value))
		{
			value = "";
		}
		if (value.Length != 0 && !(value == "publie"))
		{
			return Comptes.EstAdmin;
		}
		return true;
	}

	private static string SousJeu(string id)
	{
		string text = ValJeu(id, "sous_titre");
		if (text.Length <= 0)
		{
			if (!(id == "omsi2"))
			{
				return "Jeu TPS Groupe";
			}
			return "Simulateur de bus et de métro · cartes, rames et plugins de cabine";
		}
		return text;
	}

	private static string LibelleEtat(Projet p, string omsi, out Color couleur, out int code)
	{
		string[] array = Omsi.Installee(p, omsi);
		if (array == null && p.StatutEffectif == "bientot")
		{
			couleur = Color.FromArgb(150, 120, 255);
			code = -1;
			DateTime? dateSortie = p.DateSortie;
			string text = (p.Statut ?? "").ToLowerInvariant();
			if (!(text == "programme") || !dateSortie.HasValue)
			{
				if (!dateSortie.HasValue)
				{
					return "Prochainement";
				}
				return "Prochainement · " + dateSortie.Value.ToString("dd/MM");
			}
			return "Disponible le " + dateSortie.Value.ToString("dd/MM/yyyy");
		}
		if (array == null && p.StatutEffectif == "masque")
		{
			couleur = C.Gris;
			code = -2;
			return "Masqué (administrateur)";
		}
		if (array == null && !p.AccesVip && p.StatutEffectif == "publie")
		{
			couleur = C.Or;
			code = -1;
			DateTime? datePublic = p.DatePublic;
			if (!(p.Vip == "test"))
			{
				return "VIP · pour tous le " + (datePublic.HasValue ? datePublic.Value.ToString("dd/MM") : "?");
			}
			return "Réservé aux testeurs VIP";
		}
		if (array == null)
		{
			couleur = C.Gris;
			code = 0;
			return "Non installé";
		}
		if (Omsi.Perimee(p, array))
		{
			couleur = C.Bleu;
			code = 2;
			return "Mise à jour disponible";
		}
		couleur = C.Vert;
		code = 1;
		return "Prêt à jouer";
	}

	private Projet ProjetRecent()
	{
		if (cat == null)
		{
			return null;
		}
		Compte actif = Comptes.Actif;
		Projet result = null;
		DateTime dateTime = DateTime.MinValue;
		if (actif != null)
		{
			foreach (KeyValuePair<string, string> item in actif.Dernier)
			{
				if (!DateTime.TryParse(item.Value, out var result2))
				{
					continue;
				}
				foreach (Projet projet in cat.Projets)
				{
					if (projet.Id == item.Key && result2 > dateTime)
					{
						result = projet;
						dateTime = result2;
					}
				}
			}
		}
		return result;
	}

	private static int Grille(Panel p, List<Tuile> items, int minW, int h, int top, int pad, double ratio)
	{
		p.AutoScrollPosition = new Point(0, 0);
		int num = Zx.P(20);
		int width = p.ClientSize.Width;
		if (ratio > 0.0)
		{
			minW += Math.Max(0, (width - Zx.P(1000)) / 6);
		}
		int num2 = Math.Max(1, (width - 2 * pad + num) / (minW + num));
		int num3 = Math.Max(Zx.P(120), (width - 2 * pad - (num2 - 1) * num) / num2);
		int num4 = 0;
		int result = top;
		if (ratio > 0.0)
		{
			h = (int)((double)num3 * ratio);
		}
		foreach (Tuile item in items)
		{
			item.SetBounds(pad + num4 % num2 * (num3 + num), top + num4 / num2 * (h + num), num3, h);
			result = item.Bottom;
			num4++;
		}
		return result;
	}

	private new Tuile Menu(string glyphe, string titre, string sous, Action a)
	{
		Tuile tuile = new Tuile();
		tuile.Compacte = true;
		tuile.Glyphe = glyphe;
		tuile.Titre = titre;
		tuile.Sous = sous;
		tuile.Click += delegate
		{
			a();
		};
		pAccueil.Controls.Add(tuile);
		tuilesA.Add(tuile);
		return tuile;
	}

	private static bool AnimationActive()
	{
		return SystemInformation.UIEffectsEnabled;
	}

	private PanelFond PanelVisible()
	{
		if (!pAccueil.Visible)
		{
			if (!pJeux.Visible)
			{
				if (!pBiblio.Visible)
				{
					return null;
				}
				return pBiblio;
			}
			return pJeux;
		}
		return pAccueil;
	}

	private void Anime()
	{
		long elapsedMilliseconds = chrono.ElapsedMilliseconds;
		double num = Math.Min(0.1, (double)(elapsedMilliseconds - dernierTick) / 1000.0);
		dernierTick = elapsedMilliseconds;
		PanelFond panelFond = PanelVisible();
		bool flag = (panelFond == pAccueil || (panelFond == pJeux && themeJeux != null)) && couches.Count == 0 && !enTransition && fond.Pret && !ModeCapture && base.Visible && base.WindowState != FormWindowState.Minimized && (Form.ActiveForm == this || themeNoel) && (AnimationActive() || themeNoel);
		if (flag != periodeHaute)
		{
			periodeHaute = flag;
			try
			{
				if (flag)
				{
					Winmm.timeBeginPeriod(1u);
				}
				else
				{
					Winmm.timeEndPeriod(1u);
				}
			}
			catch
			{
			}
		}
		if (!flag)
		{
			return;
		}
		AvancerDiapos(num);
		if (panelFond == pAccueil && !rAppui.IsEmpty && !appuiR.Pas(appuiBas ? 0.9f : 1f, appuiBas ? 0.32f : 0.2f, appuiBas ? 0.55f : 0.68f) && !appuiBas)
		{
			rAppui = Rectangle.Empty;
			appuiDiscord = false;
		}
		if (panelFond == pAccueil)
		{
			double num2 = (piSurvol ? 1 : 0);
			if (piAp != num2)
			{
				piAp += (num2 - piAp) * Math.Min(1.0, num * 14.0);
				if (Math.Abs(num2 - piAp) < 0.02)
				{
					piAp = num2;
				}
			}
		}
		if (panelFond == pAccueil)
		{
			double num3 = (ficheSurvol ? 1 : 0);
			if (ficheAp != num3)
			{
				ficheAp += (num3 - ficheAp) * Math.Min(1.0, num * 13.0);
				if (Math.Abs(num3 - ficheAp) < 0.02)
				{
					ficheAp = num3;
				}
			}
		}
		if (themeNoel && panelFond == pAccueil)
		{
			noel.Avancer(num, panelFond.ClientSize.Width, panelFond.ClientSize.Height);
			if (noel.NeigeTuiles)
			{
				foreach (Tuile item in tuilesA)
				{
					if (item.Neige < 1f)
					{
						float neige = item.Neige;
						item.Neige = Math.Min(1f, item.Neige + (float)(num / 22.0));
						if ((int)(neige * 24f) != (int)(item.Neige * 24f))
						{
							item.Invalidate();
						}
					}
				}
			}
			SuivreTuiles();
		}
		else if (panelFond == pJeux && themeJeux != null)
		{
			themeJeux.Avancer(num, panelFond.ClientSize.Width, panelFond.ClientSize.Height);
		}
		panelFond.Invalidate(invalidateChildren: false);
		panelFond.Update();
	}

	private void SuivreTuiles()
	{
		ThemeBase themeBase = (Tuile.Decor = (themeNoel ? noel : null));
		Rectangle[] tuiles = ThemeBase.Tuiles;
		for (int i = 0; i < tuilesA.Count && i < tuiles.Length; i++)
		{
			Tuile tuile = tuilesA[i];
			int num = themeBase?.EtatTuile(i) ?? 0;
			if (i < etatsTuiles.Length && num != etatsTuiles[i])
			{
				etatsTuiles[i] = num;
				tuile.Invalidate();
			}
			PointF pointF = ((themeBase != null && themeBase.Secoue) ? themeBase.Decalage(i) : PointF.Empty);
			Point point = new Point(tuiles[i].X + (int)Math.Round(pointF.X), tuiles[i].Y + (int)Math.Round(pointF.Y));
			if (tuile.Location != point)
			{
				tuile.Location = point;
			}
		}
	}

	private void FondPeint(Graphics g, PanelFond p)
	{
		ThemeBase themeBase = ((p == pAccueil) ? noel : ((p == pJeux) ? themeJeux : null));
		if (themeBase != null && themeBase.ATeinte)
		{
			fond.PeindreTeinte(g, p.ClientRectangle, themeBase.Id, themeBase.Teinter);
		}
		else
		{
			fond.Peindre(g, p.ClientRectangle);
		}
		Lang.DrawText(g, "Lanceur v" + Config.VERSION, fVersion, new Rectangle(p.ClientSize.Width - Zx.P(240), p.ClientSize.Height - Zx.P(30), Zx.P(220), Zx.P(20)), Color.FromArgb(150, 160, 178), TextFormatFlags.Right | TextFormatFlags.NoPadding);
	}

	private void EnTete(Graphics g, string titre, string sous, int dy, int dx = 0)
	{
		string text = titre + "|" + sous + "|" + dx;
		if (enteteCache == null || enteteCle != text)
		{
			if (enteteCache != null)
			{
				enteteCache.Dispose();
			}
			Bitmap image = new Bitmap(Zx.P(1500), Zx.P(120), PixelFormat.Format32bppPArgb);
			int num = Zx.P(dx);
			using (Graphics graphics = Graphics.FromImage(image))
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				using (SolidBrush brush = new SolidBrush(C.Texte))
				{
					graphics.DrawString(titre, fTitre, brush, Zx.P(100) + num, Zx.P(26));
				}
				using SolidBrush brush2 = new SolidBrush(C.Gris);
				graphics.DrawString(sous, fSousT, brush2, Zx.P(104) + num, Zx.P(80));
			}
			enteteCache = image;
			enteteCle = text;
		}
		g.DrawImageUnscaled(enteteCache, 0, dy);
	}

	private void PeintJeux(Graphics g, int dy)
	{
		FondPeint(g, pJeux);
		int width = pJeux.ClientSize.Width;
		int height = pJeux.ClientSize.Height;
		if (themeJeux != null)
		{
			themeJeux.PeindreFond(g, width, height);
		}
		EnTete(g, "Choisir un jeu", "Sélectionnez un jeu pour ouvrir sa bibliothèque de cartes.", dy);
		if (themeJeux != null)
		{
			themeJeux.PeindreDevant(g, width, height);
		}
	}

	private void PeintBiblio(Graphics g, int dy)
	{
		FondPeint(g, pBiblio);
		if (ongletB == 1)
		{
			Size size0 = Lang.MeasureText("Mods", fTitre);
			Lang.DrawText(g, "Mods", fTitre, new Point((pBiblio.ClientSize.Width - size0.Width) / 2, Zx.P(30) + dy), C.Texte, TextFormatFlags.NoPadding);
		}
		else
		{
			EnTete(g, "Bibliothèque", NomJeu(jeuId) + "  ·  ma bibliothèque", dy, 56);
		}
		if (ongletB == 0)
		{
			PeindrePageJeu(g, dy);
		}
		else if (txtVide.Length > 0)
		{
			Lang.DrawText(g, txtVide, fSousT, new Rectangle(xVide, yVide + dy, Math.Max(Zx.P(200), pBiblio.ClientSize.Width - Zx.P(40) - xVide), Zx.P(60)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		}
		if (!rJeuB.IsEmpty)
		{
			DessinerFicheJeu(g, new Rectangle(rJeuB.X, rJeuB.Y + dy, rJeuB.Width, rJeuB.Height));
		}
	}

	private void DessinerFicheJeu(Graphics g, Rectangle r)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
		{
			using (SolidBrush brush = new SolidBrush(C.Carte))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(C.Ligne);
			g.DrawPath(pen, path);
		}
		int height = Math.Min(Zx.P(190), r.Width * 9 / 20);
		Rectangle rect = new Rectangle(r.X + 1, r.Y + 1, r.Width - Zx.P(2), height);
		Dictionary<string, string> value;
		Image image = ((!(jeuId == "omsi2")) ? (FichesJeux.TryGetValue(jeuId ?? "", out value) ? CouvertureWeb(value) : null) : ((FichesJeux.TryGetValue("omsi2", out value) && ValJeu("omsi2", "image").Length > 0) ? (CouvertureWeb(value) ?? CouvertureJeu("omsi2")) : CouvertureJeu("omsi2")));
		using (GraphicsPath clip = Bouton.Rond(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + Zx.P(14)), Zx.P(13)))
		{
			GraphicsState gstate = g.Save();
			g.SetClip(clip);
			g.SetClip(rect, CombineMode.Intersect);
			if (image != null)
			{
				double num = Math.Max((double)rect.Width / (double)image.Width, (double)rect.Height / (double)image.Height);
				int num2 = (int)((double)image.Width * num);
				int num3 = (int)((double)image.Height * num);
				g.InterpolationMode = InterpolationMode.HighQualityBilinear;
				g.DrawImage(image, rect.X + (rect.Width - num2) / 2, rect.Y + (rect.Height - num3) / 2, num2, num3);
			}
			else
			{
				using LinearGradientBrush brush2 = new LinearGradientBrush(rect, C.Ligne, C.Carte, 60f);
				g.FillRectangle(brush2, rect);
			}
			using (LinearGradientBrush brush3 = new LinearGradientBrush(new Rectangle(rect.X, rect.Bottom - Zx.P(50), rect.Width, Zx.P(51)), Color.FromArgb(0, C.Carte), C.Carte, 90f))
			{
				g.FillRectangle(brush3, rect.X, rect.Bottom - Zx.P(50), rect.Width, Zx.P(50));
			}
			g.Restore(gstate);
		}
		int x = r.X + Zx.P(20);
		int num4 = rect.Bottom + Zx.P(6);
		int width = r.Width - Zx.P(40);
		Lang.DrawText(g, NomJeu(jeuId), fJeuNom, new Rectangle(x, num4, width, Zx.P(32)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		num4 += Zx.P(32);
		Lang.DrawText(g, SousJeu(jeuId), fJeuTxt, new Rectangle(x, num4, width, Zx.P(20)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		num4 += Zx.P(26);
		int num5 = 0;
		int num6 = 0;
		string omsi = Omsi.DetecteOmsi();
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (JeuDe(projet) == jeuId)
				{
					num5++;
					LibelleEtat(projet, omsi, out var _, out var code);
					if (code >= 1)
					{
						num6++;
					}
				}
			}
		}
		string text = ValJeu(jeuId, "description");
		if (text.Length == 0)
		{
			text = ((jeuId == "omsi2") ? "Installez cartes, bus, livrées et packs IA en un clic, puis lancez le jeu avec openOMSI." : "Jeu en ligne gratuit : il s'ouvre dans sa propre fenêtre, avec ou sans mod.");
		}
		int num7 = ((bChemins != null) ? bChemins.Top : (r.Bottom - 48));
		Bouton[] array = new Bouton[3] { bPhotosJeu, bVideoJeu, bJouerOmsi };
		foreach (Bouton bouton in array)
		{
			if (bouton != null && bouton.Visible)
			{
				num7 = Math.Min(num7, bouton.Top);
			}
		}
		int num8 = num7 - rJeuB.Y + r.Y - Zx.P(10);
		Lang.DrawText(g, text, fJeuTxt, new Rectangle(x, num4, width, Math.Max(Zx.P(20), num8 - num4 - Zx.P(30))), Color.FromArgb(190, 200, 225), TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		string t = num5 + ((num5 > 1) ? " mods" : " mod") + "  ·  " + num6 + ((num6 > 1) ? " installés" : " installé");
		Lang.DrawText(g, t, fJeuTxt, new Rectangle(x, num8 - Zx.P(24), width, Zx.P(20)), (num6 > 0) ? C.Vert : C.Gris, TextFormatFlags.NoPadding);
	}

	private static int Marge(int w)
	{
		return Math.Max(28, Math.Min(56, (int)((double)w * 0.028)));
	}

	private void LogoCentre(Graphics g, int w, int h, int dy)
	{
		if (rProfil.Height >= 20)
		{
			int num = rProfil.Y - Zx.P(14) + dy;
			Size size = Lang.MeasureText("TPS Groupe", fTitreA);
			Lang.DrawText(g, "TPS Groupe", fTitreA, new Point((w - size.Width) / 2, num), C.Texte, TextFormatFlags.NoPadding);
			Size size2 = Lang.MeasureText("LAUNCHER", fSousA);
			int num2 = num + size.Height + Zx.P(2);
			using (Pen pen = new Pen(Color.FromArgb(120, C.Orange), Zx.P(2f)))
			{
				g.DrawLine(pen, w / 2 - size2.Width / 2 - Zx.P(34), num2 + size2.Height / 2, w / 2 - size2.Width / 2 - Zx.P(12), num2 + size2.Height / 2);
				g.DrawLine(pen, w / 2 + size2.Width / 2 + Zx.P(12), num2 + size2.Height / 2, w / 2 + size2.Width / 2 + Zx.P(34), num2 + size2.Height / 2);
			}
			Lang.DrawText(g, "LAUNCHER", fSousA, new Point((w - size2.Width) / 2, num2), Color.FromArgb(160, 172, 210), TextFormatFlags.NoPadding);
			DateTime now = DateTime.Now;
			string t = now.ToString("HH:mm:ss");
			string t2 = now.ToString("dd/MM/yy");
			Size size3 = Lang.MeasureText("00:00:00", fHeure);
			Size size4 = Lang.MeasureText(t2, fDate);
			int num3 = num2 + size2.Height + Zx.P(6);
			int num4 = size3.Width + Zx.P(14) + size4.Width;
			int num5 = (w - num4) / 2;
			Lang.DrawText(g, t, fHeure, new Point(num5, num3), C.Texte, TextFormatFlags.NoPadding);
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, C.Orange)))
			{
				g.FillEllipse(brush, num5 + size3.Width + Zx.P(5), num3 + size3.Height / 2 - Zx.P(2), Zx.P(4), Zx.P(4));
			}
			Lang.DrawText(g, t2, fDate, new Point(num5 + size3.Width + Zx.P(14), num3 + (size3.Height - size4.Height) / 2), C.Gris, TextFormatFlags.NoPadding);
			rHorloge = new Rectangle(num5 - Zx.P(4), num3 - Zx.P(2), num4 + Zx.P(8), size3.Height + Zx.P(4));
		}
	}

	private static void Niveau(Compte c, out int niv, out double frac, out int xp, out int suivant, out string titre)
	{
		Progression.Init(c);
		Progression.Niveau(c.Xp, out niv, out var dans, out var besoin);
		xp = c.Xp;
		frac = Math.Min(1.0, (double)dans / (double)Math.Max(1, besoin));
		suivant = xp - dans + besoin;
		titre = Progression.Titre(niv);
	}

	private string StatutLigne(out Color col)
	{
		if (jeuEnCours != null)
		{
			col = C.Orange;
			return "●  En jeu · " + jeuEnCours.Nom;
		}
		if (cat != null && string.IsNullOrEmpty(erreurCat))
		{
			col = C.Vert;
			return "●  En ligne";
		}
		col = C.Gris;
		return "●  Hors ligne";
	}

	private Bitmap RendreProfil(Compte c, int niv, double frac, int xp, int suivant, string titre, string st, Color cs)
	{
		int width = rProfil.Width;
		int height = rProfil.Height;
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Rectangle r = new Rectangle(0, 0, width - 1, height - 1);
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(16)))
			{
				GraphicsState gstate = graphics.Save();
				graphics.SetClip(path, CombineMode.Intersect);
				if (c == null || !(c.BanType == "anim"))
				{
					if (c != null)
					{
						Banniere.De(c).Dessiner(graphics, new Rectangle(0, 0, width, height));
					}
					else
					{
						using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, width, height + 1), C.JadeFonce, C.Fond, 20f);
						graphics.FillRectangle(brush, 0, 0, width, height);
					}
				}
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, width, height), Color.FromArgb(110, 6, 8, 20), Color.FromArgb(0, 6, 8, 20), LinearGradientMode.Horizontal))
				{
					ColorBlend colorBlend = new ColorBlend(3);
					colorBlend.Colors = new Color[3]
					{
						Color.FromArgb(70, 6, 8, 20),
						Color.FromArgb(10, 6, 8, 20),
						Color.FromArgb(40, 6, 8, 20)
					};
					colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };
					linearGradientBrush.InterpolationColors = colorBlend;
					graphics.FillRectangle(linearGradientBrush, 0, 0, width, height);
				}
				graphics.Restore(gstate);
				using Pen pen = new Pen(C.Ligne);
				graphics.DrawPath(pen, path);
			}
			int num = Math.Min(60, height - 36);
			Rectangle rectangle = new Rectangle(Zx.P(18), (height - num) / 2, num, num);
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(235, C.Fond)))
			{
				graphics.FillEllipse(brush2, Rectangle.Inflate(rectangle, Zx.P(4), Zx.P(4)));
			}
			int num2 = Zx.P(16) + num + Zx.P(16);
			int width2 = width - num2 - Zx.P(16);
			if (c != null)
			{
				Comptes.Avatar(graphics, c, rectangle, (float)num * 0.34f);
				Ombre(graphics, st, fStat, new Rectangle(num2, (height < Zx.P(90)) ? Math.Max(Zx.P(42), height - Zx.P(24)) : (height - Zx.P(34)), width2, Zx.P(20)), cs, TextFormatFlags.Right | TextFormatFlags.NoPadding);
			}
			else
			{
				using (SolidBrush brush3 = new SolidBrush(C.Orange))
				{
					graphics.FillEllipse(brush3, rectangle);
				}
				Lang.DrawText(graphics, "+", new Font("Segoe UI", Zx.P(28f)), rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				Lang.DrawText(graphics, "Créer mon profil", fPseudo, new Rectangle(num2, Zx.P(22), width2, Zx.P(26)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				Lang.DrawText(graphics, "Pseudo, niveau, bannière et statut", fPetit, new Rectangle(num2, Zx.P(54), width2, Zx.P(16)), Color.FromArgb(200, 210, 230), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			}
		}
		return bitmap;
	}

	private GraphicsState Pousse(Graphics g, Rectangle r, bool force = false)
	{
		if ((!force && (rAppui.IsEmpty || r != rAppui)) || Math.Abs(appuiR.V - 1f) < 0.002f)
		{
			return null;
		}
		GraphicsState result = g.Save();
		float num = (float)r.X + (float)r.Width / 2f;
		float num2 = (float)r.Y + (float)r.Height / 2f;
		g.TranslateTransform(num, num2);
		g.ScaleTransform(appuiR.V, appuiR.V);
		g.TranslateTransform(0f - num, 0f - num2);
		return result;
	}

	private void Appuyer(Point p)
	{
		Rectangle rect = Rectangle.Empty;
		appuiDiscord = false;
		if (rDiscord.Contains(p))
		{
			rect = rDiscord;
			appuiDiscord = true;
		}
		else if (rCloche.Contains(p))
		{
			rect = rCloche;
		}
		else if (!rAmisPlus.IsEmpty && rAmisPlus.Contains(p))
		{
			rect = rAmisPlus;
		}
		else
		{
			foreach (KeyValuePair<Rectangle, Action> item in rPiBt)
			{
				if (item.Key.Contains(p))
				{
					rect = item.Key;
				}
			}
		}
		if (!rect.IsEmpty)
		{
			rAppui = rect;
			appuiBas = true;
			pAccueil.Invalidate(Rectangle.Inflate(rect, Zx.P(8), Zx.P(8)));
		}
	}

	private void OuvrirAmis()
	{
		string demande;
		using (FenAmis fenAmis = new FenAmis())
		{
			fenAmis.Afficher(this);
			demande = fenAmis.Demande;
		}
		dernierAmis = DateTime.MinValue;
		if (pAccueil != null)
		{
			pAccueil.Invalidate();
		}
		if (!demande.StartsWith("mod:") || cat == null)
		{
			return;
		}
		string text = demande.Substring(4);
		foreach (Projet projet in cat.Projets)
		{
			if (projet.Id == text)
			{
				Choisir(projet);
				ShowVue(Vue.Projet);
				break;
			}
		}
	}

	private void MajAmis()
	{
		if (!Social.Actif || !Social.Connecte || ModeCapture)
		{
			return;
		}
		dernierAmis = DateTime.Now;
		HashSet<string> avant = new HashSet<string>();
		foreach (Ami recue in Social.Recues)
		{
			avant.Add(recue.Id);
		}
		bool deja = amisCharges;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				try
				{
					string text = Omsi.DetecteOmsi();
					List<string[]> list = new List<string[]>();
					StringBuilder stringBuilder = new StringBuilder();
					if (text != null)
					{
						foreach (Projet item in new List<Projet>(ProjetsConnus))
						{
							if (item.Liste.Length != 0)
							{
								string[] array = Omsi.Installee(item, text);
								if (array != null)
								{
									string[] array2 = new string[3]
									{
										item.Id,
										(array[0].Trim().Length > 0) ? array[0].Trim() : item.Version,
										Omsi.Desactive(item, text) ? "0" : "1"
									};
									list.Add(array2);
									stringBuilder.Append(string.Join("|", array2)).Append(';');
								}
							}
						}
					}
					if (stringBuilder.ToString() != modsEnvoyes && Social.EnvoyerMods(list))
					{
						modsEnvoyes = stringBuilder.ToString();
					}
				}
				catch
				{
				}
				if (Social.ChargerAmis())
				{
					List<string> nv = new List<string>();
					List<string> cad = Social.CadeauxVip;
					if (cad.Count > 0)
					{
						try
						{
							Social.CadeauxVus();
							Social.Moi();
							Social.EtatPoints();
						}
						catch
						{
						}
					}
					if (deja)
					{
						foreach (Ami recue2 in Social.Recues)
						{
							if (!avant.Contains(recue2.Id))
							{
								nv.Add(recue2.Pseudo);
							}
						}
					}
					Sur(delegate
					{
						amisCharges = true;
						if (cad.Count > 0)
						{
							Toast("\ud83c\udf81  " + string.Join(", ", cad.ToArray()) + " t'a offert le VIP TPS Groupe !  Profite de tes avantages.", C.Or, 10000);
							if (cacheProfil != null)
							{
								cacheProfil.Dispose();
								cacheProfil = null;
							}
						}
						if (nv.Count > 0)
						{
							Toast("Demande d'ami de " + string.Join(", ", nv.ToArray()) + "  ·  clique sur + dans « Amis en ligne »", C.Jade, 7000);
						}
						if (pAccueil != null)
						{
							pAccueil.Invalidate();
						}
					});
				}
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void PeindreAmisA(Graphics g)
	{
		rAmisPlus = Rectangle.Empty;
		if (rAmisA.Height < 110 || rAmisA.Width < 160 || !Social.Actif)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Rectangle r = rAmisA;
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(78, 18, 22, 48)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(70, 140, 160, 220));
			g.DrawPath(pen, path);
		}
		List<Ami> list = Social.Amis.FindAll((Ami a) => a.EnLigne);
		list.Sort(delegate(Ami a, Ami b)
		{
			bool flag = a.Statut.StartsWith("En jeu");
			bool flag2 = b.Statut.StartsWith("En jeu");
			if (flag == flag2)
			{
				return string.Compare(a.Pseudo, b.Pseudo, StringComparison.OrdinalIgnoreCase);
			}
			return (!flag) ? 1 : (-1);
		});
		Lang.DrawText(g, "AMIS EN LIGNE", fStat, new Point(r.X + Zx.P(14), r.Y + Zx.P(13)), Color.FromArgb(200, 208, 228));
		int width = Lang.MeasureText("AMIS EN LIGNE", fStat).Width;
		Rectangle r2 = new Rectangle(r.X + Zx.P(14) + width + Zx.P(8), r.Y + Zx.P(12), Lang.MeasureText(list.Count.ToString(), fAmS).Width + Zx.P(14), Zx.P(18));
		using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(9)))
		{
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(150, C.Vert));
			g.FillPath(brush2, path2);
		}
		Lang.DrawText(g, list.Count.ToString(), fAmS, r2, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		rAmisPlus = new Rectangle(r.Right - Zx.P(12) - Zx.P(28), r.Y + Zx.P(8), Zx.P(28), Zx.P(28));
		GraphicsState graphicsState = Pousse(g, rAmisPlus);
		using (SolidBrush brush3 = new SolidBrush(amPlusSurvol ? C.Jade : Color.FromArgb(120, C.JadeFonce)))
		{
			g.FillEllipse(brush3, rAmisPlus);
		}
		using (Pen pen2 = new Pen(Color.FromArgb(180, C.Jade), 1.2f))
		{
			g.DrawEllipse(pen2, rAmisPlus);
		}
		using (Pen pen3 = new Pen(Color.White, Zx.P(2f)))
		{
			int num = rAmisPlus.X + Zx.P(14);
			int num2 = rAmisPlus.Y + Zx.P(14);
			g.DrawLine(pen3, num - Zx.P(6), num2, num + Zx.P(6), num2);
			g.DrawLine(pen3, num, num2 - Zx.P(6), num, num2 + Zx.P(6));
		}
		if (graphicsState != null)
		{
			g.Restore(graphicsState);
		}
		int count = Social.Recues.Count;
		if (count > 0)
		{
			Rectangle r3 = new Rectangle(rAmisPlus.Right - Zx.P(10), rAmisPlus.Y - Zx.P(6), Zx.P(18), Zx.P(16));
			using (GraphicsPath path3 = Bouton.Rond(r3, Zx.P(8)))
			{
				using SolidBrush brush4 = new SolidBrush(C.Rouge);
				g.FillPath(brush4, path3);
			}
			Lang.DrawText(g, (count > 9) ? "9+" : count.ToString(), fAmS, r3, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		using (Pen pen4 = new Pen(Color.FromArgb(50, 255, 255, 255)))
		{
			g.DrawLine(pen4, r.X + Zx.P(12), r.Y + Zx.P(44), r.Right - Zx.P(12), r.Y + Zx.P(44));
		}
		int num3 = r.Y + Zx.P(52);
		int num4 = (r.Bottom - 8 - num3) / 46;
		if (list.Count == 0)
		{
			string t = ((!Social.Connecte) ? "Ajoute tes amis TPS Groupe" : ((Social.Amis.Count == 0) ? "Pas encore d'amis" : "Aucun ami en ligne"));
			string t2 = ((!Social.Connecte || Social.Amis.Count == 0) ? "Clique sur + pour en ajouter" : (Social.Amis.Count + ((Social.Amis.Count > 1) ? " amis hors ligne" : " ami hors ligne")));
			Lang.DrawText(g, t, fAmN, new Rectangle(r.X + Zx.P(10), num3 + Zx.P(14), r.Width - Zx.P(20), Zx.P(20)), Color.FromArgb(200, 208, 228), TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
			Lang.DrawText(g, t2, fAmS, new Rectangle(r.X + Zx.P(10), num3 + Zx.P(36), r.Width - Zx.P(20), Zx.P(18)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		}
		for (int i = 0; i < list.Count && i < num4; i++)
		{
			Ami ami = list[i];
			if (i == num4 - 1 && list.Count > num4)
			{
				Lang.DrawText(g, "+ " + (list.Count - i) + " autres en ligne", fAmS, new Rectangle(r.X + Zx.P(14), num3 + Zx.P(12), r.Width - Zx.P(28), Zx.P(18)), C.Gris, TextFormatFlags.NoPadding);
				break;
			}
			Rectangle rectangle = new Rectangle(r.X + Zx.P(14), num3 + Zx.P(4), Zx.P(34), Zx.P(34));
			int num5 = 0;
			string id = ami.Id;
			foreach (char c in id)
			{
				num5 = num5 * 31 + c;
			}
			using (SolidBrush brush5 = new SolidBrush(Comptes.Palette[Math.Abs(num5) % Comptes.Palette.Length]))
			{
				g.FillEllipse(brush5, rectangle);
			}
			Lang.DrawText(g, (ami.Pseudo.Length > 0) ? ami.Pseudo.Substring(0, 1).ToUpperInvariant() : "?", fAmI, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Color color = (ami.Statut.StartsWith("En jeu") ? C.Orange : C.Vert);
			using (SolidBrush brush6 = new SolidBrush(Color.FromArgb(220, 14, 16, 34)))
			{
				g.FillEllipse(brush6, rectangle.Right - Zx.P(12), rectangle.Bottom - Zx.P(12), Zx.P(15), Zx.P(15));
			}
			using (SolidBrush brush7 = new SolidBrush(color))
			{
				g.FillEllipse(brush7, rectangle.Right - Zx.P(9), rectangle.Bottom - Zx.P(9), Zx.P(9), Zx.P(9));
			}
			Lang.DrawText(g, ami.Pseudo, fAmN, new Rectangle(rectangle.Right + Zx.P(10), num3 + Zx.P(4), r.Right - rectangle.Right - Zx.P(22), Zx.P(18)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			Lang.DrawText(g, ami.Statut, fAmS, new Rectangle(rectangle.Right + Zx.P(10), num3 + Zx.P(22), r.Right - rectangle.Right - Zx.P(22), Zx.P(16)), color, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			num3 += Zx.P(46);
		}
		g.SmoothingMode = smoothingMode;
	}

	private void PeindreFiche(Graphics g)
	{
		if (ModeCapture && Environment.GetEnvironmentVariable("TPS_FICHE") == "1")
		{
			ficheAp = 1.0;
		}
		if (ModeCapture && Environment.GetEnvironmentVariable("TPS_PILULE") == "1")
		{
			piAp = 1.0;
			piSurvol = true;
		}
		Compte actif = Comptes.Actif;
		if (ficheAp <= 0.01 || actif == null || rProfil.Width < 200)
		{
			return;
		}
		int width = Math.Max(rProfil.Width, Zx.P(360));
		int num = Math.Min(Zx.P(452), pAccueil.ClientSize.Height - rProfil.Y - Zx.P(84));
		if (!rAmisA.IsEmpty && rAmisA.Height > 0)
		{
			num = Math.Min(num, rAmisA.Y - rProfil.Y - Zx.P(14));
		}
		if (num < Zx.P(260))
		{
			return;
		}
		bool flag = ficheBmp == null || ficheBmp.Width != width || ficheBmp.Height != num;
		if (flag)
		{
			if (ficheBmp != null)
			{
				ficheBmp.Dispose();
			}
			ficheBmp = new Bitmap(width, num, PixelFormat.Format32bppPArgb);
		}
		long elapsedMilliseconds = chrono.ElapsedMilliseconds;
		if (flag || elapsedMilliseconds - ficheTick >= 50)
		{
			ficheTick = elapsedMilliseconds;
			using Graphics graphics = Graphics.FromImage(ficheBmp);
			graphics.Clear(Color.Transparent);
			DessinerFiche(graphics, actif, width, num);
		}
		double num2 = ficheAp;
		double num3 = 1.0 - Math.Pow(1.0 - num2, 3.0);
		int num4 = (int)Math.Round((double)rProfil.Height + (double)(num - rProfil.Height) * num3);
		int num5 = (int)Math.Round((double)width * (0.96 + 0.04 * num3));
		int num6 = rProfil.X + (width - num5) / 2;
		rFiche = new Rectangle(rProfil.X, rProfil.Y, width, num);
		for (int i = 1; i <= 7; i++)
		{
			using GraphicsPath path = Bouton.Rond(new Rectangle(num6 - i * 2 + Zx.P(2), rProfil.Y - i * 2 + Zx.P(8), num5 + i * 4 - Zx.P(4), num4 + i * 4 - Zx.P(4)), Zx.P(16) + i * 2);
			using SolidBrush brush = new SolidBrush(Color.FromArgb((int)((double)((8 - i) * 10) * num3), 0, 0, 6));
			g.FillPath(brush, path);
		}
		if (num3 >= 0.999)
		{
			GraphicsState gstate = g.Save();
			g.SetClip(new Rectangle(num6, rProfil.Y, num5, num4));
			g.DrawImageUnscaled(ficheBmp, num6, rProfil.Y);
			g.Restore(gstate);
		}
		else
		{
			AlphaDraw(g, ficheBmp, new Rectangle(num6, rProfil.Y, num5, num4), (float)Math.Min(1.0, num3 * 1.25));
		}
	}

	private void DessinerFiche(Graphics g, Compte c, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		double temps = BanAnim.Temps;
		Progression.Init(c);
		Progression.Niveau(c.Xp, out var niv, out var dans, out var besoin);
		Rectangle r = new Rectangle(0, 0, w - 1, h - 1);
		int num = Zx.P(112);
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(16)))
		{
			using (SolidBrush brush = new SolidBrush(C.Carte))
			{
				g.FillPath(brush, path);
			}
			GraphicsState gstate = g.Save();
			g.SetClip(path, CombineMode.Intersect);
			Rectangle r2 = new Rectangle(0, 0, w, num);
			if (c.BanType == "anim")
			{
				BanAnim.Dessiner(c.BanMotif, g, r2, temps);
			}
			else
			{
				Banniere.De(c).Dessiner(g, r2);
			}
			g.Restore(gstate);
			using Pen pen = new Pen(Color.FromArgb(150, C.Ligne));
			g.DrawPath(pen, path);
		}
		Rectangle rectangle = new Rectangle(Zx.P(20), num - Zx.P(52), Zx.P(92), Zx.P(92));
		using (SolidBrush brush2 = new SolidBrush(C.Carte))
		{
			g.FillEllipse(brush2, Rectangle.Inflate(rectangle, Zx.P(6), Zx.P(6)));
		}
		Comptes.Avatar(g, c, rectangle, 30f);
		Badge.Anneau(g, rectangle, niv, temps);
		Deco.Dessiner(g, rectangle, c.Deco, temps);
		StatutLigne(out var col);
		using (SolidBrush brush3 = new SolidBrush(C.Carte))
		{
			g.FillEllipse(brush3, rectangle.Right - Zx.P(26), rectangle.Bottom - Zx.P(26), Zx.P(28), Zx.P(28));
		}
		using (SolidBrush brush4 = new SolidBrush(col))
		{
			g.FillEllipse(brush4, rectangle.Right - Zx.P(21), rectangle.Bottom - Zx.P(21), Zx.P(18), Zx.P(18));
		}
		Badge.Dessiner(g, new Rectangle(w - Zx.P(18) - Zx.P(46), num + Zx.P(8), Zx.P(46), Zx.P(46)), niv, temps);
		string text = DiscordPresence.Utilisateur ?? "";
		string text2 = DiscordPresence.Statut ?? "";
		bool flag = DiscordPresence.EnMarche && text.Length > 0;
		string text3 = (flag ? text : (DiscordPresence.EnMarche ? (text2.StartsWith("Discord non détecté") ? "ouvrez Discord" : "connexion…") : (text2.StartsWith("Identifiant") ? "identifiant refusé" : "désactivé")));
		Color color = (flag ? Color.FromArgb(88, 101, 242) : C.Gris);
		int num2 = rectangle.Right + Zx.P(12);
		int width = Math.Max(60, w - 18 - 46 - 10 - num2);
		Rectangle r3 = new Rectangle(num2, num + Zx.P(16), width, Zx.P(28));
		using (GraphicsPath path2 = Bouton.Rond(r3, Zx.P(14)))
		{
			using (SolidBrush brush5 = new SolidBrush(Color.FromArgb(flag ? 60 : 34, color)))
			{
				g.FillPath(brush5, path2);
			}
			using Pen pen2 = new Pen(Color.FromArgb(flag ? 150 : 90, color));
			g.DrawPath(pen2, path2);
		}
		using (SolidBrush brush6 = new SolidBrush(color))
		{
			g.FillEllipse(brush6, r3.X + Zx.P(10), r3.Y + Zx.P(9), Zx.P(10), Zx.P(10));
		}
		Lang.DrawText(g, "Discord · " + text3, fFStat, new Rectangle(r3.X + Zx.P(26), r3.Y, r3.Width - Zx.P(32), r3.Height), flag ? Color.White : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Rectangle r4 = new Rectangle(Zx.P(12), num + Zx.P(62), w - Zx.P(25), h - num - Zx.P(74));
		using (GraphicsPath path3 = Bouton.Rond(r4, Zx.P(12)))
		{
			using SolidBrush brush7 = new SolidBrush(Color.FromArgb(14, 17, 38));
			g.FillPath(brush7, path3);
		}
		int num3 = r4.X + Zx.P(16);
		int num4 = r4.Width - Zx.P(32);
		int num5 = r4.Y + Zx.P(10);
		int num6 = r4.Bottom - Zx.P(8);
		Lang.DrawText(g, c.Pseudo, fFNom, new Rectangle(num3, num5, num4 - (Social.Vip ? Zx.P(48) : 0), Zx.P(32)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		if (Social.Vip)
		{
			Badge.PilleVip(g, new Rectangle(Math.Min(num3 + Lang.MeasureText(c.Pseudo, fFNom).Width + Zx.P(6), num3 + num4 - Zx.P(42)), num5 + Zx.P(8), Zx.P(40), Zx.P(20)));
		}
		num5 += Zx.P(32);
		string t = ((c.Statut.Trim().Length > 0) ? c.Statut.Trim() : StatutSimple());
		Lang.DrawText(g, t, fFStat, new Rectangle(num3, num5, num4, Zx.P(20)), Color.FromArgb(170, 205, 225), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		num5 += Zx.P(24);
		using (Pen pen3 = new Pen(C.Ligne))
		{
			g.DrawLine(pen3, num3, num5, num3 + num4, num5);
		}
		num5 += Zx.P(10);
		if (num5 + 70 < num6)
		{
			Lang.DrawText(g, "À PROPOS DE MOI", fFRub, new Point(num3, num5), C.Gris);
			num5 += Zx.P(18);
			string t2 = ((c.Bio.Trim().Length > 0) ? c.Bio.Trim() : "Aucune bio pour l'instant.");
			Lang.DrawText(g, t2, fFTxt, new Rectangle(num3, num5, num4, Zx.P(40)), (c.Bio.Trim().Length > 0) ? Color.FromArgb(214, 220, 236) : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			num5 += Zx.P(46);
		}
		if (num5 + 76 < num6)
		{
			Lang.DrawText(g, "NIVEAU", fFRub, new Point(num3, num5), C.Gris);
			num5 += Zx.P(18);
			Lang.DrawText(g, "Niveau " + niv + "  ·  " + Progression.Titre(niv), fFNiv, new Point(num3, num5), C.Texte);
			num5 += Zx.P(24);
			Rectangle r5 = new Rectangle(num3, num5, num4, Zx.P(8));
			double num7 = ((niv >= 100) ? 1.0 : ((double)dans / (double)Math.Max(1, besoin)));
			using (GraphicsPath path4 = Bouton.Rond(r5, Zx.P(4)))
			{
				using SolidBrush brush8 = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
				g.FillPath(brush8, path4);
			}
			int num8 = (int)((double)r5.Width * num7);
			if (num8 > 6)
			{
				using GraphicsPath path5 = Bouton.Rond(new Rectangle(r5.X, r5.Y, num8, r5.Height), Zx.P(4));
				using LinearGradientBrush brush9 = new LinearGradientBrush(new Rectangle(r5.X, r5.Y, Math.Max(Zx.P(2), num8), r5.Height), C.Jade, C.Orange, 0f);
				g.FillPath(brush9, path5);
			}
			num5 += Zx.P(12);
			Lang.DrawText(g, (niv >= 100) ? (c.Xp + " XP") : (dans + " / " + besoin + " XP"), fFPetit, new Point(num3, num5), C.Gris);
			num5 += Zx.P(22);
		}
		if (num5 + 40 >= num6)
		{
			return;
		}
		Lang.DrawText(g, "MEMBRE DEPUIS", fFRub, new Point(num3, num5), C.Gris);
		num5 += Zx.P(18);
		Lang.DrawText(g, c.Cree, fFTxt, new Point(num3, num5), Color.FromArgb(214, 220, 236));
		int num9 = 0;
		foreach (int value in c.Lancements.Values)
		{
			num9 += value;
		}
		string t3 = num9 + ((num9 > 1) ? " lancements" : " lancement") + "  ·  " + c.Succes.Count + " succès";
		Lang.DrawText(g, t3, fFPetit, new Rectangle(num3, num5 + Zx.P(2), num4, Zx.P(18)), C.Gris, TextFormatFlags.Right | TextFormatFlags.NoPadding);
	}

	private static void Ombre(Graphics g, string t, Font f, Rectangle r, Color c, TextFormatFlags fl)
	{
		Lang.DrawText(g, t, f, new Rectangle(r.X + 1, r.Y + 1, r.Width, r.Height), Color.FromArgb(10, 10, 16), fl);
		Lang.DrawText(g, t, f, r, c, fl);
	}

	private void PeindreProfil(Graphics g)
	{
		if (rProfil.Width < 80)
		{
			return;
		}
		Compte actif = Comptes.Actif;
		Color col;
		string text = StatutLigne(out col);
		int niv = 0;
		int xp = 0;
		int suivant = 0;
		double frac = 0.0;
		string titre = "";
		if (jeuEnCours == null && DiscordPresence.EnMarche && (DiscordPresence.Utilisateur ?? "").Length > 0)
		{
			text = "●  Discord · " + DiscordPresence.Utilisateur;
			col = Color.FromArgb(88, 101, 242);
		}
		if (actif != null)
		{
			Niveau(actif, out niv, out frac, out xp, out suivant, out titre);
		}
		string text2 = string.Concat(rProfil.Size, "|", (actif == null) ? "-" : (actif.Id + actif.Pseudo + niv + xp + actif.BanType + actif.BanC1 + actif.BanC2 + actif.BanAngle + actif.BanMotif + actif.CouleurPerso + actif.Couleur), "|", text);
		if (cacheProfil == null || cleProfil != text2)
		{
			if (cacheProfil != null)
			{
				cacheProfil.Dispose();
			}
			cacheProfil = RendreProfil(actif, niv, frac, xp, suivant, titre, text, col);
			cleProfil = text2;
			vivantTick = -1L;
		}
		double temps = BanAnim.Temps;
		long num = (long)(temps * (double)((Zx.Z > 1.05f) ? 16 : 24));
		if (vivant == null || vivant.Width != rProfil.Width || vivant.Height != rProfil.Height)
		{
			if (vivant != null)
			{
				vivant.Dispose();
			}
			vivant = new Bitmap(rProfil.Width, rProfil.Height, PixelFormat.Format32bppPArgb);
			vivantTick = -1L;
		}
		if (num != vivantTick)
		{
			vivantTick = num;
			Rectangle rectangle = new Rectangle(0, 0, rProfil.Width, rProfil.Height);
			using Graphics graphics = Graphics.FromImage(vivant);
			graphics.Clear(Color.Transparent);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (actif != null && actif.BanType == "anim")
			{
				using GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, rectangle.Width - 1, rectangle.Height - 1), Zx.P(16));
				GraphicsState gstate = graphics.Save();
				graphics.SetClip(path, CombineMode.Intersect);
				if (Zx.Z > 1.05f)
				{
					int num2 = Math.Max(1, (int)((float)rectangle.Width / Zx.Z));
					int num3 = Math.Max(1, (int)((float)rectangle.Height / Zx.Z));
					if (banDemi == null || banDemi.Width != num2 || banDemi.Height != num3)
					{
						if (banDemi != null)
						{
							banDemi.Dispose();
						}
						banDemi = new Bitmap(num2, num3, PixelFormat.Format32bppPArgb);
					}
					using (Graphics graphics2 = Graphics.FromImage(banDemi))
					{
						graphics2.Clear(Color.Transparent);
						graphics2.SmoothingMode = SmoothingMode.AntiAlias;
						BanAnim.Dessiner(actif.BanMotif, graphics2, new Rectangle(0, 0, num2, num3), temps);
					}
					graphics.InterpolationMode = InterpolationMode.Bilinear;
					graphics.DrawImage(banDemi, rectangle);
				}
				else
				{
					BanAnim.Dessiner(actif.BanMotif, graphics, rectangle, temps);
				}
				graphics.Restore(gstate);
			}
			graphics.DrawImageUnscaled(cacheProfil, 0, 0);
			if (actif != null)
			{
				int num4 = Math.Min(60, rectangle.Height - 36);
				int y = (rectangle.Height - num4) / 2;
				Badge.Anneau(graphics, new Rectangle(Zx.P(18), y, num4, num4), niv, temps);
				Deco.Dessiner(graphics, new Rectangle(Zx.P(18), y, num4, num4), actif.Deco, temps);
				Badge.Dessiner(graphics, (rectangle.Height < Zx.P(90)) ? new Rectangle(rectangle.Width - Zx.P(10) - Zx.P(30), Zx.P(7), Zx.P(30), Zx.P(30)) : new Rectangle(rectangle.Width - Zx.P(12) - Zx.P(34), Zx.P(10), Zx.P(34), Zx.P(34)), niv, temps);
				if (Social.Vip)
				{
					Badge.PilleVip(graphics, new Rectangle(rectangle.Width - Zx.P(12) - Zx.P(34) - Zx.P(8) - Zx.P(38), Zx.P(18), Zx.P(38), Zx.P(18)));
				}
			}
		}
		g.DrawImageUnscaled(vivant, rProfil.X, rProfil.Y);
		double num5 = (double)chrono.ElapsedMilliseconds / 1000.0;
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (GraphicsPath path2 = Bouton.Rond(new Rectangle(rProfil.X, rProfil.Y, rProfil.Width - 1, rProfil.Height - 1), Zx.P(16)))
		{
			GraphicsState gstate2 = g.Save();
			g.SetClip(path2, CombineMode.Intersect);
			int num6 = rProfil.X + (int)((num5 * 0.16 % 2.2 - 0.5) * (double)rProfil.Width);
			int height = rProfil.Height;
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
			{
				g.FillPolygon(brush, new Point[4]
				{
					new Point(num6 + Zx.P(40), rProfil.Y),
					new Point(num6 + Zx.P(100), rProfil.Y),
					new Point(num6 + Zx.P(60), rProfil.Y + height),
					new Point(num6, rProfil.Y + height)
				});
			}
			g.Restore(gstate2);
		}
		if (actif != null)
		{
			int num7 = Math.Min(60, rProfil.Height - 36);
			int num8 = rProfil.Y + (rProfil.Height - num7) / 2;
			Point point = new Point(rProfil.X + Zx.P(18) + num7 - Zx.P(6), num8 + num7 - Zx.P(6));
			double num9 = num5 % 1.6 / 1.6;
			int num10 = (int)(9.0 + num9 * 12.0);
			using (Pen pen = new Pen(Color.FromArgb((int)(150.0 * (1.0 - num9)), col), Zx.P(2f)))
			{
				g.DrawEllipse(pen, point.X - num10, point.Y - num10, num10 * 2, num10 * 2);
			}
			using (SolidBrush brush2 = new SolidBrush(C.Fond))
			{
				g.FillEllipse(brush2, point.X - Zx.P(8), point.Y - Zx.P(8), Zx.P(16), Zx.P(16));
			}
			using SolidBrush brush3 = new SolidBrush(col);
			g.FillEllipse(brush3, point.X - Zx.P(5), point.Y - Zx.P(5), Zx.P(10), Zx.P(10));
		}
		g.SmoothingMode = smoothingMode;
	}

	private Bitmap RendreDiapo(Diapo d, Size s)
	{
		Bitmap bitmap = new Bitmap(Math.Max(2, s.Width), Math.Max(2, s.Height), PixelFormat.Format32bppPArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
		int width = s.Width;
		int height = s.Height;
		Rectangle r = new Rectangle(0, 0, width - 1, height - 1);
		if (width < Zx.P(520))
		{
			PetiteDiapo(graphics, d, width, height);
			return bitmap;
		}
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(18)))
		{
			GraphicsState gstate = graphics.Save();
			graphics.SetClip(path, CombineMode.Intersect);
			if (d.Img != null)
			{
				double num = Math.Max((double)width / (double)d.Img.Width, (double)height / (double)d.Img.Height);
				int num2 = (int)((double)d.Img.Width * num);
				int num3 = (int)((double)d.Img.Height * num);
				graphics.DrawImage(d.Img, (width - num2) / 2, (height - num3) / 2, num2, num3);
			}
			else
			{
				using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, width, height + 1), C.JadeFonce, C.Cote, 35f);
				graphics.FillRectangle(brush, 0, 0, width, height);
			}
			using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, width, height), Color.FromArgb(235, 6, 8, 20), Color.FromArgb(0, 6, 8, 20), LinearGradientMode.Horizontal))
			{
				ColorBlend colorBlend = new ColorBlend(3);
				colorBlend.Colors = new Color[3]
				{
					Color.FromArgb(238, 6, 8, 20),
					Color.FromArgb(150, 6, 8, 20),
					Color.FromArgb(0, 6, 8, 20)
				};
				colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };
				linearGradientBrush.InterpolationColors = colorBlend;
				graphics.FillRectangle(linearGradientBrush, 0, 0, width, height);
			}
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, height / 2, width, height - height / 2 + 1), Color.FromArgb(0, 6, 8, 20), Color.FromArgb(215, 6, 8, 20), 90f))
			{
				graphics.FillRectangle(brush2, 0, height / 2, width, height - height / 2);
			}
			graphics.Restore(gstate);
			using Pen pen = new Pen(C.Ligne);
			graphics.DrawPath(pen, path);
		}
		bool flag = width < Zx.P(520);
		int num4 = (flag ? Zx.P(18) : Math.Max(Zx.P(28), Math.Min(Zx.P(64), (int)((double)width * 0.05))));
		int width2 = (flag ? (width - 2 * num4) : Math.Max(Zx.P(240), (int)((double)width * 0.62)));
		int num5 = height - (flag ? Zx.P(46) : Zx.P(58));
		if (d.Ouvrir != null)
		{
			string t = ((d.Libelle.Length > 0) ? d.Libelle : "Découvrir") + "  →";
			int width3 = Lang.MeasureText(t, fNiv).Width + Zx.P(40);
			Rectangle rectangle = new Rectangle(num4, num5 - (flag ? Zx.P(32) : Zx.P(40)), width3, flag ? Zx.P(32) : Zx.P(40));
			using (GraphicsPath path2 = Bouton.Rond(rectangle, Zx.P(20)))
			{
				using LinearGradientBrush brush3 = new LinearGradientBrush(rectangle, ControlPaint.Light(C.Orange, 0.1f), ControlPaint.Dark(C.Orange, 0.1f), 90f);
				graphics.FillPath(brush3, path2);
			}
			Lang.DrawText(graphics, t, fNiv, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			num5 -= (flag ? Zx.P(44) : Zx.P(58));
		}
		Font font = new Font("Segoe UI Semibold", flag ? Zx.P(16f) : Math.Max(Zx.P(20f), Math.Min(Zx.P(40f), (float)height * 0.055f)));
		Font font2 = new Font("Segoe UI", flag ? Zx.P(10f) : Math.Max(Zx.P(11f), Math.Min(Zx.P(15f), (float)height * 0.021f)));
		int num6 = 0;
		if (d.Sous.Length > 0)
		{
			num6 = Lang.MeasureText(d.Sous, font2, new Size(width2, Zx.P(400)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
			Lang.DrawText(graphics, d.Sous, font2, new Rectangle(num4, num5 - num6, width2, num6), Color.FromArgb(215, 222, 238), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			num5 -= num6 + Zx.P(8);
		}
		Size size = Lang.MeasureText(d.Titre, font, new Size(width2, Zx.P(400)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		Lang.DrawText(graphics, d.Titre, font, new Rectangle(num4, num5 - size.Height, width2, size.Height), C.Texte, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		num5 -= size.Height + Zx.P(12);
		if (d.Detail.Length > 0)
		{
			Size size2 = Lang.MeasureText(d.Detail, fPetit, new Size(width2, Zx.P(200)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			Lang.DrawText(graphics, d.Detail, fPetit, new Rectangle(num4, num5 - size2.Height, width2, size2.Height), Color.FromArgb(170, 180, 205), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			num5 -= size2.Height + Zx.P(10);
		}
		string t2 = d.Tag.ToUpperInvariant();
		int width4 = Lang.MeasureText(t2, fStat).Width + Zx.P(26);
		Rectangle r2 = new Rectangle(num4, num5 - Zx.P(28), width4, Zx.P(28));
		using (GraphicsPath path3 = Bouton.Rond(r2, Zx.P(14)))
		{
			using SolidBrush brush4 = new SolidBrush(d.Couleur);
			graphics.FillPath(brush4, path3);
		}
		Lang.DrawText(graphics, t2, fStat, r2, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		font.Dispose();
		font2.Dispose();
		return bitmap;
	}

	private void PetiteDiapo(Graphics g, Diapo d, int W, int H)
	{
		Rectangle r = new Rectangle(0, 0, W - 1, H - 1);
		int num = Zx.P(18);
		int num2 = HautImagePetite(W, H);
		int num3 = W - 2 * num;
		using GraphicsPath path = Bouton.Rond(r, Zx.P(16));
		GraphicsState gstate = g.Save();
		g.SetClip(path, CombineMode.Intersect);
		using (SolidBrush brush = new SolidBrush(C.Carte))
		{
			g.FillRectangle(brush, 0, 0, W, H);
		}
		Rectangle rect = new Rectangle(0, 0, W, num2);
		if (d.Img != null)
		{
			double num4 = Math.Max((double)rect.Width / (double)d.Img.Width, (double)rect.Height / (double)d.Img.Height);
			int num5 = (int)((double)d.Img.Width * num4);
			int num6 = (int)((double)d.Img.Height * num4);
			GraphicsState gstate2 = g.Save();
			g.SetClip(rect, CombineMode.Intersect);
			g.DrawImage(d.Img, (W - num5) / 2, (num2 - num6) / 2, num5, num6);
			g.Restore(gstate2);
		}
		else
		{
			using LinearGradientBrush brush2 = new LinearGradientBrush(rect, C.JadeFonce, C.Cote, 35f);
			g.FillRectangle(brush2, rect);
		}
		using (Pen pen = new Pen(C.Ligne))
		{
			g.DrawLine(pen, 0, num2, W, num2);
		}
		g.Restore(gstate);
		string t = d.Tag.ToUpperInvariant();
		int width = Lang.MeasureText(t, fStat).Width + Zx.P(26);
		Rectangle r2 = new Rectangle(Zx.P(14), Zx.P(14), width, Zx.P(28));
		using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(14)))
		{
			using SolidBrush brush3 = new SolidBrush(d.Couleur);
			g.FillPath(brush3, path2);
		}
		Lang.DrawText(g, t, fStat, r2, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		int num7 = num2 + Zx.P(14);
		int num8 = H - ((d.Ouvrir != null) ? Zx.P(62) : Zx.P(14));
		using (Font f = new Font("Segoe UI Semibold", Zx.P(14f)))
		{
			using Font f2 = new Font("Segoe UI", Zx.P(9.5f));
			int num9 = Math.Min(Zx.P(30), Math.Max(0, num8 - num7));
			Lang.DrawText(g, d.Titre, f, new Rectangle(num, num7, num3, num9), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			num7 += num9 + Zx.P(4);
			int num10 = Math.Max(0, num8 - num7);
			if (d.Sous.Length > 0 && num10 >= Zx.P(16))
			{
				int num11 = Math.Max(1, Math.Min(3, num10 / Lang.MeasureText("Ag", f2).Height));
				Lang.DrawText(g, d.Sous, f2, new Rectangle(num, num7, num3, num11 * Lang.MeasureText("Ag", f2).Height), Color.FromArgb(200, 208, 228), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			}
		}
		if (d.Ouvrir != null)
		{
			string t2 = ((d.Libelle.Length > 0) ? d.Libelle : "Découvrir") + "  →";
			int width2 = Math.Min(num3, Lang.MeasureText(t2, fNiv).Width + Zx.P(36));
			Rectangle rectangle = new Rectangle(num, H - Zx.P(50), width2, Zx.P(36));
			using (GraphicsPath path3 = Bouton.Rond(rectangle, Zx.P(18)))
			{
				using LinearGradientBrush brush4 = new LinearGradientBrush(rectangle, ControlPaint.Light(C.Orange, 0.1f), ControlPaint.Dark(C.Orange, 0.1f), 90f);
				g.FillPath(brush4, path3);
			}
			Lang.DrawText(g, t2, fNiv, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		using Pen pen2 = new Pen(C.Ligne);
		g.DrawPath(pen2, path);
	}

	private static int HautImagePetite(int W, int H)
	{
		return Math.Max((int)((double)W * 0.36), Math.Min((int)((double)W * 0.62), H - Zx.P(132)));
	}

	private Bitmap CacheDiapo(Diapo d)
	{
		if (d.Cache == null || d.CacheTaille != rCarte.Size)
		{
			if (d.Cache != null)
			{
				d.Cache.Dispose();
			}
			d.Cache = RendreDiapo(d, rCarte.Size);
			d.CacheTaille = rCarte.Size;
		}
		return d.Cache;
	}

	private void PeindreDiapos(Graphics g)
	{
		if (rCarte.Width < 120 || rCarte.Height < 120)
		{
			return;
		}
		Lang.DrawText(g, "NOUVEAUTÉS", fStat, new Point(rCarte.X + Zx.P(2), rCarte.Y - Zx.P(26)), C.Gris);
		using (SolidBrush brush = new SolidBrush(C.Orange))
		{
			g.FillEllipse(brush, rCarte.X + Zx.P(88), rCarte.Y - Zx.P(20), Zx.P(7), Zx.P(7));
		}
		if (diapos.Count == 0)
		{
			return;
		}
		if (dIdx >= diapos.Count)
		{
			dIdx = 0;
		}
		float num = (float)dLeve;
		int num2 = (int)Math.Round(Zx.P(2f) + Zx.P(7f) * num);
		Rectangle rect = new Rectangle(rCarte.X, rCarte.Y - num2, rCarte.Width, rCarte.Height);
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		float num3 = (float)Math.Round(num * 8f) / 8f;
		int num4 = rCarte.Width * 100000 + rCarte.Height * 10 + (int)(num3 * 8f);
		if (ombreCarte == null || cleOmbre != num4)
		{
			if (ombreCarte != null)
			{
				ombreCarte.Dispose();
			}
			ombreCarte = new Bitmap(rCarte.Width + Zx.P(64), rCarte.Height + Zx.P(64) + Zx.P(20), PixelFormat.Format32bppPArgb);
			cleOmbre = num4;
			using Graphics graphics = Graphics.FromImage(ombreCarte);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			for (int num5 = 12; num5 >= 1; num5--)
			{
				using GraphicsPath path = Bouton.Rond(new Rectangle(Zx.P(32) - Zx.P(2) * num5 + Zx.P(4), Zx.P(32) + Zx.P(10) + (int)((float)Zx.P(8) * num3) - Zx.P(2) * num5, rCarte.Width + Zx.P(4) * num5 - Zx.P(8), rCarte.Height + Zx.P(4) * num5 - Zx.P(8)), Zx.P(16) + Zx.P(2) * num5);
				using SolidBrush brush2 = new SolidBrush(Color.FromArgb((int)((double)(13 - num5) * (1.3 + 1.1 * (double)num3)), 0, 0, 6));
				graphics.FillPath(brush2, path);
			}
		}
		g.DrawImageUnscaled(ombreCarte, rCarte.X - Zx.P(32), rCarte.Y - Zx.P(32));
		using (GraphicsPath path2 = Bouton.Rond(new Rectangle(rect.X, rect.Y + Zx.P(7), rect.Width - 1, rect.Height - 1), Zx.P(14)))
		{
			using LinearGradientBrush brush3 = new LinearGradientBrush(new Rectangle(rect.X, rect.Y + Zx.P(7), rect.Width, rect.Height + 1), Color.FromArgb(46, 54, 104), Color.FromArgb(12, 14, 32), 90f);
			g.FillPath(brush3, path2);
		}
		if (num > 0.01f)
		{
			using GraphicsPath path3 = Bouton.Rond(Rectangle.Inflate(rect, Zx.P(3), Zx.P(3)), Zx.P(17));
			using Pen pen = new Pen(Color.FromArgb((int)(80f * num), C.Orange), Zx.P(3f));
			g.DrawPath(pen, path3);
		}
		g.SmoothingMode = smoothingMode;
		Diapo d = diapos[dIdx];
		GraphicsState gstate = g.Save();
		using (GraphicsPath clip = Bouton.Rond(new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1), Zx.P(14)))
		{
			g.SetClip(clip);
		}
		if (dTrans >= 1.0 || dPrec < 0 || dPrec >= diapos.Count)
		{
			g.DrawImageUnscaled(CacheDiapo(d), rect.X, rect.Y);
		}
		else
		{
			double num6 = EaseIO(dTrans);
			int width = rCarte.Width;
			g.DrawImageUnscaled(CacheDiapo(diapos[dPrec]), rect.X - (int)(num6 * (double)width * (double)dDir), rect.Y);
			g.DrawImageUnscaled(CacheDiapo(d), rect.X + (int)((1.0 - num6) * (double)width * (double)dDir), rect.Y);
		}
		if (num > 0.01f)
		{
			float num7 = Math.Max(rect.X, Math.Min(rect.Right, dSouris.X));
			float num8 = Math.Max(rect.Y, Math.Min(rect.Bottom, dSouris.Y));
			float num9 = (float)rect.Width * 0.7f;
			using GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(num7 - num9, num8 - num9, num9 * 2f, num9 * 2f);
			using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.CenterColor = Color.FromArgb((int)(46f * num), 255, 255, 255);
			pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, 255, 255, 255) };
			g.FillPath(pathGradientBrush, graphicsPath);
		}
		using (LinearGradientBrush brush4 = new LinearGradientBrush(new Rectangle(rect.X, rect.Y, rect.Width, Zx.P(40)), Color.FromArgb((int)(40f + 50f * num), 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
		{
			g.FillRectangle(brush4, rect.X, rect.Y, rect.Width, Zx.P(2));
		}
		g.Restore(gstate);
		using (GraphicsPath path4 = Bouton.Rond(new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1), Zx.P(14)))
		{
			using Pen pen2 = new Pen(Color.FromArgb((int)(70f + 90f * num), 120, 140, 255), 1.2f);
			SmoothingMode smoothingMode2 = g.SmoothingMode;
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.DrawPath(pen2, path4);
			g.SmoothingMode = smoothingMode2;
		}
		SmoothingMode smoothingMode3 = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		rPuces.Clear();
		bool flag = rect.Width < Zx.P(520);
		int num10 = (flag ? Zx.P(18) : Math.Max(Zx.P(28), Math.Min(Zx.P(64), (int)((double)rect.Width * 0.05))));
		int num11 = rect.X + num10;
		int y = (flag ? (rect.Y + HautImagePetite(rect.Width, rect.Height) - Zx.P(16)) : (rect.Bottom - Zx.P(34)));
		for (int i = 0; i < diapos.Count; i++)
		{
			int num12 = ((i != dIdx) ? (flag ? Zx.P(10) : Zx.P(20)) : (flag ? Zx.P(34) : Zx.P(58)));
			Rectangle rectangle = new Rectangle(num11, y, num12, Zx.P(6));
			rPuces.Add(Rectangle.Inflate(rectangle, Zx.P(4), Zx.P(10)));
			if (flag)
			{
				using GraphicsPath path5 = Bouton.Rond(Rectangle.Inflate(rectangle, Zx.P(2), Zx.P(2)), Zx.P(5));
				using SolidBrush brush5 = new SolidBrush(Color.FromArgb(120, 0, 0, 10));
				g.FillPath(brush5, path5);
			}
			using (GraphicsPath path6 = Bouton.Rond(rectangle, Zx.P(3)))
			{
				using SolidBrush brush6 = new SolidBrush(Color.FromArgb((i == dIdx) ? 110 : 80, 255, 255, 255));
				g.FillPath(brush6, path6);
			}
			if (i == dIdx)
			{
				int num13 = (int)((double)num12 * Math.Min(1.0, dTemps / 6.5));
				if (num13 > 3)
				{
					using GraphicsPath path7 = Bouton.Rond(new Rectangle(num11, y, num13, Zx.P(6)), Zx.P(3));
					using SolidBrush brush7 = new SolidBrush(C.Orange);
					g.FillPath(brush7, path7);
				}
			}
			num11 += num12 + (flag ? Zx.P(5) : Zx.P(6));
		}
		int num14 = (flag ? Zx.P(32) : Zx.P(40));
		rNext = (flag ? new Rectangle(rect.Right - Zx.P(14) - num14, rect.Y + Zx.P(12), num14, num14) : new Rectangle(rect.Right - Zx.P(18) - num14, rect.Bottom - Zx.P(18) - num14, num14, num14));
		rPrev = new Rectangle(rNext.X - num14 - Zx.P(8), rNext.Y, num14, num14);
		int[] array = new int[2] { 0, 1 };
		foreach (int num15 in array)
		{
			Rectangle rect2 = ((num15 == 0) ? rPrev : rNext);
			using (SolidBrush brush8 = new SolidBrush(Color.FromArgb(170, 8, 10, 24)))
			{
				g.FillEllipse(brush8, rect2);
			}
			using (Pen pen3 = new Pen(Color.FromArgb(120, 255, 255, 255)))
			{
				g.DrawEllipse(pen3, rect2.X, rect2.Y, rect2.Width - 1, rect2.Height - 1);
			}
			float num16 = (float)rect2.X + (float)rect2.Width / 2f + (float)(((num15 != 0) ? 1 : (-1)) * rect2.Width) * 0.04f;
			float num17 = (float)rect2.Y + (float)rect2.Height / 2f;
			float num18 = (float)rect2.Width * 0.12f;
			float num19 = (float)rect2.Height * 0.2f;
			float num20 = ((num15 == 0) ? 1 : (-1));
			using Pen pen4 = new Pen(Color.White, Math.Max(1.6f, Zx.P(2f)));
			pen4.StartCap = LineCap.Round;
			pen4.EndCap = LineCap.Round;
			pen4.LineJoin = LineJoin.Round;
			g.DrawLines(pen4, new PointF[3]
			{
				new PointF(num16 + num20 * num18, num17 - num19),
				new PointF(num16 - num20 * num18, num17),
				new PointF(num16 + num20 * num18, num17 + num19)
			});
		}
		g.SmoothingMode = smoothingMode3;
	}

	private void AvancerDiapos(double dt)
	{
		if (diapos.Count == 0)
		{
			return;
		}
		dLeve += ((double)(dSurvol ? 1 : 0) - dLeve) * Math.Min(1.0, dt * 9.0);
		iLeve += ((double)(iSurvol ? 1 : 0) - iLeve) * Math.Min(1.0, dt * 9.0);
		if (Math.Abs(iLeve - (double)(iSurvol ? 1 : 0)) < 0.01)
		{
			iLeve = (iSurvol ? 1 : 0);
		}
		if (dTrans < 1.0)
		{
			dTrans = Math.Min(1.0, dTrans + dt / 0.5);
		}
		if (diapos.Count > 1 && !dSurvol && dTrans >= 1.0)
		{
			dTemps += dt;
			if (dTemps >= 6.5)
			{
				AllerDiapo(1);
			}
		}
	}

	private void AllerDiapo(int sens)
	{
		if (diapos.Count > 1)
		{
			AllerA((dIdx + sens + diapos.Count) % diapos.Count, (sens >= 0) ? 1 : (-1));
		}
	}

	private void AllerA(int i, int sens)
	{
		if (i != dIdx && i >= 0 && i < diapos.Count)
		{
			dPrec = dIdx;
			dIdx = i;
			dDir = sens;
			dTemps = 0.0;
			dTrans = ((!AnimationActive()) ? 1 : 0);
			pAccueil.Invalidate();
		}
	}

	private void InitAccueil()
	{
		Tuile.Deneigee += delegate(Tuile tu)
		{
			if (themeNoel && tu.Parent == pAccueil)
			{
				noel.Deneiger(tu.Bounds);
			}
		};
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		timer.Interval = 250;
		int sec = -1;
		timer.Tick += delegate
		{
			int second = DateTime.Now.Second;
			if (second != sec)
			{
				sec = second;
				if (!periodeHaute && base.Visible && pAccueil != null && pAccueil.Visible && !rHorloge.IsEmpty)
				{
					pAccueil.Invalidate(rHorloge);
				}
			}
		};
		timer.Start();
		Tuile.NeigeEnChute = () => noel != null && noel.EnChute;
		pAccueil.MouseMove += delegate(object o, MouseEventArgs e)
		{
			bool flag = (dSurvol = rCarte.Contains(e.Location));
			iSurvol = !rInfos.IsEmpty && rInfos.Contains(e.Location);
			if (!periodeHaute)
			{
				iLeve = (iSurvol ? 1 : 0);
			}
			dSouris = e.Location;
			bool flag2 = rDiscord.Contains(e.Location);
			int num = (flag2 ? ((e.X < DiscordCoupe) ? 1 : 2) : 0);
			if (flag2 != dcSurvol || num != dcMoitie)
			{
				dcSurvol = flag2;
				dcMoitie = num;
				pAccueil.Invalidate(Rectangle.Inflate(rDiscord, Zx.P(6), Zx.P(6)));
			}
			bool flag3 = rCloche.Contains(e.Location);
			if (flag3 != clSurvol)
			{
				clSurvol = flag3;
				pAccueil.Invalidate(Rectangle.Inflate(rCloche, Zx.P(8), Zx.P(8)));
			}
			flag2 = flag2 || flag3;
			Rectangle rectangle = PiluleZone();
			bool flag4 = !rectangle.IsEmpty && rectangle.Contains(e.Location);
			if (volGlisse)
			{
				flag4 = true;
				RegleVolume(e.X);
			}
			if (flag4 != piSurvol)
			{
				piSurvol = flag4;
				if (!flag4)
				{
					volOuvert = false;
				}
				if (!periodeHaute)
				{
					piAp = (flag4 ? 1 : 0);
				}
				pAccueil.Invalidate(Rectangle.Inflate(PiluleZone(grande: true), Zx.P(6), Zx.P(6)));
			}
			if (flag4)
			{
				flag2 = true;
			}
			bool flag5 = rMusique.Contains(e.Location);
			if (flag5 != muSurvol)
			{
				muSurvol = flag5;
				pAccueil.Invalidate(Rectangle.Inflate(rMusique, Zx.P(8), Zx.P(8)));
			}
			flag2 = flag2 || flag5;
			foreach (KeyValuePair<Rectangle, Annonce> rAnnonce in rAnnonces)
			{
				if (rAnnonce.Key.Contains(e.Location))
				{
					flag2 = true;
				}
			}
			bool flagMaj = !rMajA.IsEmpty && MajAAfficher && rMajA.Contains(e.Location);
			if (flagMaj != majSurvol)
			{
				majSurvol = flagMaj;
				pAccueil.Invalidate(Rectangle.Inflate(rMajA, Zx.P(4), Zx.P(4)));
			}
			flag2 = flag2 || flagMaj;
			pAccueil.Cursor = ((flag || flag2 || rProfil.Contains(e.Location)) ? Cursors.Hand : Cursors.Default);
			if (!rAmisA.IsEmpty && Social.Actif && rAmisA.Contains(e.Location))
			{
				pAccueil.Cursor = Cursors.Hand;
			}
			bool flag6 = rAmisPlus.Contains(e.Location);
			if (flag6 != amPlusSurvol)
			{
				amPlusSurvol = flag6;
				pAccueil.Invalidate(Rectangle.Inflate(rAmisPlus, Zx.P(8), Zx.P(8)));
			}
			bool flag7 = Comptes.Actif != null && (rProfil.Contains(e.Location) || (ficheAp > 0.4 && !rFiche.IsEmpty && rFiche.Contains(e.Location)));
			if (flag7 != ficheSurvol)
			{
				ficheSurvol = flag7;
				if (!periodeHaute)
				{
					ficheAp = (flag7 ? 1 : 0);
				}
				pAccueil.Invalidate();
			}
		};
		pAccueil.MouseDown += delegate(object o, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && !rVolBar.IsEmpty && rVolBar.Contains(e.Location))
			{
				volGlisse = true;
				RegleVolume(e.X);
			}
			else if (e.Button == MouseButtons.Left)
			{
				Appuyer(e.Location);
			}
		};
		pAccueil.MouseUp += delegate
		{
			volGlisse = false;
			appuiBas = false;
		};
		pAccueil.MouseLeave += delegate
		{
			if (ficheSurvol)
			{
				ficheSurvol = false;
				if (!periodeHaute)
				{
					ficheAp = 0.0;
				}
				pAccueil.Invalidate();
			}
		};
		pAccueil.MouseLeave += delegate
		{
			dSurvol = false;
			iSurvol = false;
			if (dcSurvol)
			{
				dcSurvol = false;
				pAccueil.Invalidate(rDiscord);
			}
		};
		pAccueil.MouseClick += delegate(object o, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && couches.Count <= 0)
			{
				if (!rMajA.IsEmpty && MajAAfficher && rMajA.Contains(e.Location) && Opt("alerte_type") != "important")
				{
					Sons.Clic();
					ClicMaj();
					return;
				}
				if (rProfil.Contains(e.Location) || rDiscord.Contains(e.Location) || rCarte.Contains(e.Location))
				{
					Sons.Clic();
				}
				if (!rAmisA.IsEmpty && Social.Actif && rAmisA.Contains(e.Location) && (!(ficheAp > 0.4) || !rFiche.Contains(e.Location)))
				{
					Sons.Clic();
					OuvrirAmis();
				}
				else if (rProfil.Contains(e.Location) || (ficheAp > 0.4 && !rFiche.IsEmpty && rFiche.Contains(e.Location)))
				{
					Profil(Comptes.Actif == null);
				}
				else if (rDiscord.Contains(e.Location))
				{
					if (e.X < DiscordCoupe)
					{
						OuvrirDiscord();
					}
					else
					{
						OuvrirAppDiscord();
					}
				}
				else if (rVolBar.IsEmpty || !rVolBar.Contains(e.Location))
				{
					foreach (KeyValuePair<Rectangle, Action> item in rPiBt)
					{
						if (item.Key.Contains(e.Location))
						{
							Sons.Clic();
							item.Value();
							pAccueil.Invalidate(Rectangle.Inflate(PiluleZone(grande: true), Zx.P(6), Zx.P(6)));
							return;
						}
					}
					if (!PiluleZone().IsEmpty && PiluleZone().Contains(e.Location) && Musique.Lecteur.Length == 0)
					{
						Sons.Clic();
						Musique.AfficherLecteur();
					}
					else if (rCloche.Contains(e.Location))
					{
						Sons.Clic();
						OuvrirNotifs();
					}
					else if (rMusique.Contains(e.Location))
					{
						Sons.Clic();
						using (FenMusique f = new FenMusique())
						{
							f.Afficher(this);
						}
						pAccueil.Invalidate();
					}
					else
					{
						foreach (KeyValuePair<Rectangle, Annonce> rAnnonce2 in rAnnonces)
						{
							if (rAnnonce2.Key.Contains(e.Location))
							{
								Sons.Clic();
								Choisir(rAnnonce2.Value.Mod);
								ShowVue(Vue.Projet);
								return;
							}
						}
						if (rPrev.Contains(e.Location))
						{
							AllerDiapo(-1);
						}
						else if (rNext.Contains(e.Location))
						{
							AllerDiapo(1);
						}
						else
						{
							for (int i = 0; i < rPuces.Count; i++)
							{
								if (rPuces[i].Contains(e.Location))
								{
									AllerA(i, (i > dIdx) ? 1 : (-1));
									return;
								}
							}
							if (rCarte.Contains(e.Location) && dIdx < diapos.Count && diapos[dIdx].Ouvrir != null)
							{
								diapos[dIdx].Ouvrir();
							}
						}
					}
				}
			}
		};
	}

	private void ChargerPopularite()
	{
		ChargerPopularite(force: false);
	}

	private void ChargerPopularite(bool force)
	{
		if ((popDemandee && !force) || cat == null || File.Exists(Config.Catalogue))
		{
			return;
		}
		popDemandee = true;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				string input;
				using (WebClient webClient = Web.Client())
				{
					input = webClient.DownloadString("https://api.github.com/repos/Dreams9972/TPS-Studios/releases?per_page=100");
				}
				IEnumerable enumerable = new JavaScriptSerializer().DeserializeObject(input) as object[];
				Dictionary<string, int> i = new Dictionary<string, int>();
				if (enumerable != null)
				{
					foreach (object item in enumerable)
					{
						IEnumerable enumerable2 = ((item is Dictionary<string, object> dictionary && dictionary.ContainsKey("assets")) ? (dictionary["assets"] as IEnumerable) : null);
						if (enumerable2 != null)
						{
							foreach (object item2 in enumerable2)
							{
								if (item2 is Dictionary<string, object> dictionary2 && dictionary2.ContainsKey("browser_download_url") && dictionary2.ContainsKey("download_count"))
								{
									int.TryParse(Convert.ToString(dictionary2["download_count"]), out var result);
									i[Convert.ToString(dictionary2["browser_download_url"])] = result;
								}
							}
						}
					}
				}
				Sur(delegate
				{
					telecharg = i;
					ConstruireDiapos();
					pAccueil.Invalidate();
					page.Invalidate();
				});
			}
			catch (Exception e)
			{
				Journal.Ecrire("popularite", e);
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private static DateTime? DateProjet(Projet p)
	{
		if (!DateTime.TryParseExact(p.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			return null;
		}
		return result;
	}

	private Diapo NouvelleDiapo(string tag, Color col, string titre, string sous, string detail, Image img, string action, Action ouvrir)
	{
		Diapo diapo = new Diapo();
		diapo.Tag = tag;
		diapo.Couleur = col;
		diapo.Titre = titre;
		diapo.Sous = sous;
		diapo.Detail = detail;
		diapo.Img = img;
		diapo.Libelle = action;
		diapo.Ouvrir = ouvrir;
		diapo.Cle = tag + "|" + titre + "|" + sous + "|" + detail + "|" + (img != null) + "|" + action;
		return diapo;
	}

	private void ConstruireDiapos()
	{
		List<Diapo> list = new List<Diapo>();
		string omsi = Omsi.DetecteOmsi();
		if (cat != null)
		{
			ChargerPopularite();
			List<Projet> list2 = new List<Projet>();
			List<Projet> list3 = new List<Projet>();
			foreach (Projet projet2 in cat.Projets)
			{
				if (JeuDe(projet2).Length != 0)
				{
					if (projet2.StatutEffectif == "publie" && projet2.Installable)
					{
						list2.Add(projet2);
					}
					else if (projet2.StatutEffectif == "bientot")
					{
						list3.Add(projet2);
					}
				}
			}
			list2.Sort(delegate(Projet a, Projet b)
			{
				DateTime? dateTime2 = DateProjet(a);
				return (DateProjet(b) ?? DateTime.MinValue).CompareTo(dateTime2 ?? DateTime.MinValue);
			});
			List<Projet> collection = list2.FindAll((Projet x) => x.Nouveautes);
			list2.RemoveAll((Projet x) => x.Nouveautes);
			list2.InsertRange(0, collection);
			int num = 0;
			foreach (Projet item in list2)
			{
				if (num++ < 3)
				{
					Projet pp4 = item;
					DateTime? dateTime = DateProjet(item);
					bool flag = dateTime.HasValue && (DateTime.Now - dateTime.Value).TotalDays <= 60.0;
					images.TryGetValue(item.Id, out var value);
					list.Add(NouvelleDiapo(item.Nouveautes ? "À la une" : (flag ? "Nouveau" : "À découvrir"), C.Orange, item.Nom, item.SousTitre, item.PrixTexte + "  ·  " + item.Version + (dateTime.HasValue ? ("  ·  mis en ligne le " + dateTime.Value.ToString("dd/MM/yyyy")) : "") + ((item.Taille > 0) ? ("  ·  " + item.Taille / 1048576 + " Mo") : ""), value, "Voir la page", delegate
					{
						Choisir(pp4);
						ShowVue(Vue.Projet);
					}));
					continue;
				}
				break;
			}
			Projet projet = null;
			int num2 = 0;
			foreach (Projet item2 in list2)
			{
				if (item2.Zip.Length > 0 && telecharg.TryGetValue(item2.Zip, out var value2) && value2 > num2)
				{
					num2 = value2;
					projet = item2;
				}
			}
			if (projet != null)
			{
				Projet pp3 = projet;
				images.TryGetValue(projet.Id, out var value3);
				list.Add(NouvelleDiapo("Populaire", Color.FromArgb(232, 160, 24), projet.Nom, "Le plus téléchargé : " + num2 + ((num2 > 1) ? " téléchargements" : " téléchargement"), projet.SousTitre, value3, "Voir la page", delegate
				{
					Choisir(pp3);
					ShowVue(Vue.Projet);
				}));
			}
			foreach (Projet item3 in list2)
			{
				string[] array = Omsi.Installee(item3, omsi);
				if (array == null || !Omsi.Perimee(item3, array))
				{
					continue;
				}
				Projet pp2 = item3;
				images.TryGetValue(item3.Id, out var value4);
				string text = "";
				if (item3.Notes.Length > 0)
				{
					text = item3.Notes.Replace("\r", "").Split('\n')[0];
					if (text.Length > 90)
					{
						text = text.Substring(0, 90) + "…";
					}
				}
				list.Add(NouvelleDiapo("Mise à jour", C.Bleu, item3.Nom + "  ·  " + item3.Version, "Une nouvelle version est disponible pour votre installation.", text, value4, "Mettre à jour", delegate
				{
					Choisir(pp2);
					ShowVue(Vue.Projet);
				}));
			}
			List<string> list4 = new List<string>();
			list4.Add("omsi2");
			foreach (Projet projet3 in cat.Projets)
			{
				if (!list4.Contains(JeuDe(projet3)))
				{
					list4.Add(JeuDe(projet3));
				}
			}
			foreach (string item4 in list4)
			{
				int num3 = 0;
				int num4 = 0;
				foreach (Projet projet4 in cat.Projets)
				{
					if (JeuDe(projet4) == item4 && projet4.StatutEffectif == "publie")
					{
						num3++;
						LibelleEtat(projet4, omsi, out var _, out var code);
						if (code >= 1)
						{
							num4++;
						}
					}
				}
				string jid = item4;
				list.Add(NouvelleDiapo("Jeu", Color.FromArgb(40, 150, 210), NomJeu(item4), SousJeu(item4), num3 + ((num3 > 1) ? " cartes disponibles" : " carte disponible") + "  ·  " + num4 + ((num4 > 1) ? " installées" : " installée"), CouvertureJeu(item4), "Ouvrir la bibliothèque", delegate
				{
					jeuId = jid;
					Config.Ecrire("jeu", jid);
					filtre = 0;
					ShowVue(Vue.Biblio);
				}));
			}
			foreach (Projet item5 in list3)
			{
				Projet pp = item5;
				images.TryGetValue(item5.Id, out var value5);
				DateTime? dateSortie = item5.DateSortie;
				list.Add(NouvelleDiapo("Bientôt", Color.FromArgb(130, 96, 235), item5.Nom, (item5.SousTitre.Length > 0) ? item5.SousTitre : "Annoncé par TPS Groupe", dateSortie.HasValue ? ("Sortie prévue le " + dateSortie.Value.ToString("dd/MM/yyyy")) : "Date à venir", value5, "En savoir plus", delegate
				{
					Choisir(pp);
					ShowVue(Vue.Projet);
				}));
			}
			string inv = InviteDiscord();
			if (inv.Length > 0)
			{
				list.Add(NouvelleDiapo("Communauté", Color.FromArgb(88, 101, 242), "Rejoignez TPS Groupe sur Discord", "Annonces, aide et discussions avec les joueurs.", "", null, "Rejoindre", delegate
				{
					try
					{
						Process.Start(inv);
					}
					catch
					{
					}
				}));
			}
		}
		if (list.Count == 0)
		{
			list.Add(NouvelleDiapo("Bienvenue", C.Jade, "Bienvenue sur TPS Groupe", "Tous nos projets, mods et jeux réunis dans un seul lanceur.", "", null, "Jouer", delegate
			{
				ShowVue(Vue.Jeux);
			}));
		}
		for (int i = 0; i < list.Count; i++)
		{
			foreach (Diapo diapo in diapos)
			{
				if (diapo.Cle == list[i].Cle && diapo.Cache != null)
				{
					list[i].Cache = diapo.Cache;
					list[i].CacheTaille = diapo.CacheTaille;
					diapo.Cache = null;
					break;
				}
			}
		}
		foreach (Diapo diapo2 in diapos)
		{
			if (diapo2.Cache != null)
			{
				diapo2.Cache.Dispose();
			}
		}
		string text2 = ((dIdx < diapos.Count) ? diapos[dIdx].Cle : "");
		diapos = list;
		dIdx = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Cle == text2)
			{
				dIdx = j;
			}
		}
		dPrec = -1;
		dTrans = 1.0;
	}

	private void T(string nom, Action a)
	{
		if (ProfilFps == null)
		{
			a();
			return;
		}
		long timestamp = Stopwatch.GetTimestamp();
		a();
		double num = (double)(Stopwatch.GetTimestamp() - timestamp) * 1000.0 / (double)Stopwatch.Frequency;
		ProfilFps.TryGetValue(nom, out var value);
		ProfilFps[nom] = value + num;
	}

	private void PeintAccueil(Graphics g, int dy)
	{
		T("fond", delegate
		{
			FondPeint(g, pAccueil);
		});
		if (themeNoel)
		{
			T("theme-fond", delegate
			{
				int width = pAccueil.ClientSize.Width;
				int height = pAccueil.ClientSize.Height;
				noel.PeindreFond(g, width, height);
				Lang.DrawText(g, "Lanceur v" + Config.VERSION, fVersion, new Rectangle(width - Zx.P(240), height - Zx.P(30), Zx.P(220), Zx.P(20)), Color.FromArgb(90, 100, 130), TextFormatFlags.Right | TextFormatFlags.NoPadding);
			});
		}
		T("calottes", delegate
		{
			if (noel != null && noel.NeigeTuiles)
			{
				foreach (Tuile item in tuilesA)
				{
					if (item.Visible && item.Neige > 0.03f)
					{
						Calotte(g, item.Bounds, item.Neige);
					}
				}
			}
		});
		T("logo", delegate
		{
			LogoCentre(g, pAccueil.ClientSize.Width, pAccueil.ClientSize.Height, dy);
		});
		T("profil", delegate
		{
			PeindreProfil(g);
		});
		T("bandeau", delegate
		{
			if (MajAAfficher && Opt("alerte_type") != "important")
			{
				PeindreMaj(g);
			}
			else
			{
				PeindreBandeau(g);
			}
		});
		T("nouveautes", delegate
		{
			PeindreDiapos(g);
		});
		T("infos", delegate
		{
			PeindreInfos(g);
		});
		T("discord", delegate
		{
			PeindreDiscord(g);
		});
		T("cloche", delegate
		{
			PeindreCloche(g);
			PeindreMusique(g);
		});
		T("pilule", delegate
		{
			PeindrePilule(g);
		});
		T("amis", delegate
		{
			PeindreAmisA(g);
		});
		T("fiche", delegate
		{
			PeindreFiche(g);
		});
		if (themeNoel)
		{
			T("theme-boutons", delegate
			{
				noel.SurTuiles(g, pAccueil.ClientSize.Width, pAccueil.ClientSize.Height);
			});
		}
		if (themeNoel)
		{
			T("theme-devant", delegate
			{
				noel.PeindreDevant(g, pAccueil.ClientSize.Width, pAccueil.ClientSize.Height);
			});
		}
	}

	private void MajVip()
	{
		if (vipLance || !Social.Actif || !Social.Connecte)
		{
			return;
		}
		vipLance = true;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				Social.Moi();
				int gagne = 0;
				if (Social.Vip)
				{
					gagne = Social.Pointage();
					Social.EtatPoints();
				}
				Sur(delegate
				{
					if (gagne > 0)
					{
						Toast("★  VIP : +" + gagne + " points (bonus du jour)", C.Or, 6000);
					}
					if (cacheProfil != null)
					{
						cacheProfil.Dispose();
						cacheProfil = null;
					}
					if (pAccueil != null)
					{
						pAccueil.Invalidate();
					}
					MajEtat();
				});
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void Boutique()
	{
		List<Projet> list = new List<Projet>();
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (projet.Payant && projet.StatutEffectif != "masque")
				{
					list.Add(projet);
				}
			}
		}
		Image value;
		string demande;
		using (FenBoutique fenBoutique = new FenBoutique(list, (Projet p) => (!images.TryGetValue(p.Id, out value)) ? null : value, LienVip))
		{
			fenBoutique.Afficher(this);
			demande = fenBoutique.Demande;
		}
		if (cacheProfil != null)
		{
			cacheProfil.Dispose();
			cacheProfil = null;
		}
		RafraichirVues();
		if (demande == "vip")
		{
			Profil(nouveau: false, 3);
		}
		else
		{
			if (cat == null || (!demande.StartsWith("code:") && !demande.StartsWith("page:")))
			{
				return;
			}
			string text = demande.Substring(5);
			foreach (Projet projet2 in cat.Projets)
			{
				if (projet2.Id == text)
				{
					Choisir(projet2);
					ShowVue(Vue.Projet);
					if (demande.StartsWith("code:"))
					{
						SaisirCode();
					}
					break;
				}
			}
		}
	}

	private void OuvrirNotifs()
	{
		string demande;
		using (FenNotifs fenNotifs = new FenNotifs())
		{
			fenNotifs.Afficher(this);
			demande = fenNotifs.Demande;
		}
		if (pAccueil != null)
		{
			pAccueil.Invalidate();
		}
		if (demande == "reglages")
		{
			FenParametres.SectionInitiale = 4;
			Parametres();
		}
		else
		{
			if (cat == null || !demande.StartsWith("mod:"))
			{
				return;
			}
			string text = demande.Substring(4);
			foreach (Projet projet in cat.Projets)
			{
				if (projet.Id == text)
				{
					Choisir(projet);
					ShowVue(Vue.Projet);
					break;
				}
			}
		}
	}

	private Rectangle PiluleZone(bool grande = false)
	{
		if (rPilule.IsEmpty || Musique.Mode == "aucun")
		{
			return Rectangle.Empty;
		}
		double num = (grande ? 1.0 : (piAp * piAp * (3.0 - 2.0 * piAp)));
		return new Rectangle(rPilule.X, rPilule.Y, rPilule.Width, (int)((double)rPilule.Height + (double)(Zx.P(98) + (volOuvert ? Zx.P(34) : 0) - rPilule.Height) * num));
	}

	private void MenuPlaylists()
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		contextMenuStrip.ShowImageMargin = false;
		contextMenuStrip.Font = new Font("Segoe UI", Zx.P(10f));
		List<string[]> list = Musique.Playlists();
		if (list.Count == 0)
		{
			ToolStripItem toolStripItem = contextMenuStrip.Items.Add("Aucune playlist enregistrée");
			toolStripItem.Enabled = false;
		}
		foreach (string[] item in list)
		{
			string lien = item[1];
			contextMenuStrip.Items.Add("♪   " + item[0], null, delegate
			{
				Musique.OuvrirPlaylist(lien);
			});
		}
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		contextMenuStrip.Items.Add("Gérer mes playlists…", null, delegate
		{
			using FenPlaylists f = new FenPlaylists();
			f.Afficher(this);
		});
		contextMenuStrip.Show(Cursor.Position);
	}

	private void RegleVolume(int x)
	{
		if (!rVolBar.IsEmpty)
		{
			Rectangle rectangle = Rectangle.Inflate(rVolBar, -Zx.P(6), -Zx.P(10));
			VolumeWin.Ecrire((float)(x - rectangle.X) / (float)Math.Max(1, rectangle.Width));
			pAccueil.Invalidate(Rectangle.Inflate(PiluleZone(grande: true), Zx.P(6), Zx.P(6)));
		}
	}

	private void PeindrePilule(Graphics g)
	{
		if (ModeCapture && Environment.GetEnvironmentVariable("TPS_PILULE") == "1")
		{
			piAp = 1.0;
			piSurvol = true;
			volOuvert = true;
		}
		rPiBt.Clear();
		Rectangle r = PiluleZone();
		if (r.IsEmpty || r.Width < 140)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		double e = piAp * piAp * (3.0 - 2.0 * piAp);
		string text = Musique.EnCours();
		string lecteur = Musique.Lecteur;
		string mode = Musique.Mode;
		bool flag = text.Length > 0;
		Color color = ((lecteur == "Spotify" || (lecteur.Length == 0 && mode != "apple")) ? Color.FromArgb(30, 185, 84) : Color.FromArgb(250, 45, 85));
		using (GraphicsPath path = Bouton.Rond(r, Math.Min(Zx.P(19), r.Height / 2)))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(205, 18, 22, 46)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb((flag || piSurvol) ? 190 : 90, color), 1.2f);
			g.DrawPath(pen, path);
		}
		Rectangle rectangle = new Rectangle(r.X + Zx.P(7), r.Y + Zx.P(7), Zx.P(24), Zx.P(24));
		using (SolidBrush brush2 = new SolidBrush(color))
		{
			g.FillEllipse(brush2, rectangle);
		}
		Lang.DrawText(g, "\uec4f", fPiG, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		string text2 = text;
		string text3 = "";
		int num = text.IndexOf(" - ");
		if (num > 0)
		{
			text3 = text.Substring(0, num);
			text2 = text.Substring(num + 3);
		}
		if (!flag && lecteur.Length > 0 && MediaWin.Titre.Length > 0)
		{
			text2 = MediaWin.Titre;
			text3 = "en pause";
		}
		else if (!flag)
		{
			text2 = ((lecteur.Length > 0) ? (lecteur + "  ·  en pause") : ("Musique  ·  ouvrir " + ((mode == "apple") ? "Apple Music" : ((mode == "spotify") ? "Spotify" : "ton lecteur"))));
		}
		if (lecteur == "Lecteur web")
		{
			color = ((mode == "apple") ? Color.FromArgb(250, 45, 85) : Color.FromArgb(30, 185, 84));
		}
		int width = r.Width - Zx.P(44) - (flag ? Zx.P(30) : Zx.P(12));
		Lang.DrawText(g, text2 + ((text3.Length > 0) ? ("  ·  " + text3) : ""), fPiT, new Rectangle(r.X + Zx.P(40), r.Y + Zx.P(10), width, Zx.P(18)), flag ? C.Texte : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		if (flag)
		{
			double temps = BanAnim.Temps;
			for (int i = 0; i < 4; i++)
			{
				float num2 = (float)Zx.P(3) + (float)Zx.P(9) * (float)Math.Abs(Math.Sin(temps * 4.5 + (double)i * 1.1));
				using SolidBrush brush3 = new SolidBrush(color);
				g.FillRectangle(brush3, r.Right - Zx.P(26) + i * 4, (float)(r.Y + Zx.P(26)) - num2, Zx.P(2.5f), num2);
			}
		}
		if (e > 0.4)
		{
			int num3 = r.Y + Zx.P(46);
			int num4 = r.X + r.Width / 2;
			float al = (float)Math.Min(1.0, (e - 0.4) / 0.5);
			Action<Rectangle, string, Font, Color, bool, Action> action = delegate(Rectangle rb, string gl, Font f, Color c, bool plein, Action a)
			{
				bool flag2 = piSurvol && rb.Contains(pAccueil.PointToClient(Cursor.Position));
				GraphicsState graphicsState = Pousse(g, rb);
				using (SolidBrush brush7 = new SolidBrush(Color.FromArgb((int)(al * (float)((!plein) ? (flag2 ? 90 : 45) : (flag2 ? 255 : 230))), c)))
				{
					g.FillEllipse(brush7, rb);
				}
				using (Pen pen2 = new Pen(Color.FromArgb((int)(al * 150f), c)))
				{
					g.DrawEllipse(pen2, rb);
				}
				if ((double)al > 0.6)
				{
					Lang.DrawText(g, gl, f, rb, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				if (graphicsState != null)
				{
					g.Restore(graphicsState);
				}
				if (e > 0.85)
				{
					rPiBt.Add(new KeyValuePair<Rectangle, Action>(rb, a));
				}
			};
			action(new Rectangle(num4 - Zx.P(21), num3, Zx.P(42), Zx.P(42)), flag ? "\ue769" : "\ue768", fPiG2, C.Orange, arg5: true, delegate
			{
				Musique.Basculer();
			});
			action(new Rectangle(num4 - Zx.P(21) - Zx.P(10) - Zx.P(34), num3 + Zx.P(4), Zx.P(34), Zx.P(34)), "\ue892", fPiG, color, arg5: false, delegate
			{
				Musique.Touche(177);
			});
			action(new Rectangle(num4 + Zx.P(21) + Zx.P(10), num3 + Zx.P(4), Zx.P(34), Zx.P(34)), "\ue893", fPiG, color, arg5: false, delegate
			{
				Musique.Touche(176);
			});
			action(new Rectangle(num4 - Zx.P(21) - Zx.P(10) - Zx.P(34) - Zx.P(12) - Zx.P(30), num3 + Zx.P(6), Zx.P(30), Zx.P(30)), "\ue8a7", fPiG, C.Gris, arg5: false, delegate
			{
				Musique.AfficherLecteur();
			});
			action(new Rectangle(num4 + Zx.P(21) + Zx.P(10) + Zx.P(34) + Zx.P(12), num3 + Zx.P(6), Zx.P(30), Zx.P(30)), "\ue767", fPiG, volOuvert ? C.Jade : C.Gris, volOuvert, delegate
			{
				volOuvert = !volOuvert;
				pAccueil.Invalidate();
			});
			rVolBar = Rectangle.Empty;
			if (volOuvert && e > 0.85)
			{
				float num5 = VolumeWin.Lire();
				Rectangle rectangle2 = new Rectangle(r.X + Zx.P(46), r.Y + Zx.P(106), r.Width - Zx.P(46) - Zx.P(56), Zx.P(8));
				rVolBar = Rectangle.Inflate(rectangle2, Zx.P(6), Zx.P(10));
				Lang.DrawText(g, (num5 <= 0.001f) ? "\ue74f" : "\ue767", fPiG, new Rectangle(r.X + Zx.P(14), r.Y + Zx.P(98), Zx.P(24), Zx.P(24)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				using (GraphicsPath path2 = Bouton.Rond(rectangle2, Zx.P(4)))
				{
					using SolidBrush brush4 = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
					g.FillPath(brush4, path2);
				}
				int num6 = (int)((float)rectangle2.Width * Math.Max(0f, num5));
				if (num6 > 4)
				{
					using GraphicsPath path3 = Bouton.Rond(new Rectangle(rectangle2.X, rectangle2.Y, num6, rectangle2.Height), Zx.P(4));
					using LinearGradientBrush brush5 = new LinearGradientBrush(new Rectangle(rectangle2.X, rectangle2.Y, Math.Max(Zx.P(2), num6), rectangle2.Height), color, C.Orange, 0f);
					g.FillPath(brush5, path3);
				}
				if (num5 >= 0f)
				{
					using (SolidBrush brush6 = new SolidBrush(Color.White))
					{
						g.FillEllipse(brush6, rectangle2.X + num6 - Zx.P(7), rectangle2.Y - Zx.P(3), Zx.P(14), Zx.P(14));
					}
					Lang.DrawText(g, (int)Math.Round(num5 * 100f) + " %", fPiS, new Rectangle(rectangle2.Right + Zx.P(8), r.Y + Zx.P(100), Zx.P(44), Zx.P(20)), C.Texte, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
				else
				{
					Lang.DrawText(g, "indisponible", fPiS, new Rectangle(rectangle2.X, r.Y + Zx.P(98), rectangle2.Width, Zx.P(20)), C.Gris, TextFormatFlags.NoPadding);
				}
			}
		}
		g.SmoothingMode = smoothingMode;
	}

	private void PeindreMusique(Graphics g)
	{
		if (rMusique.Width <= 0)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		bool flag = Musique.EnCours().Length > 0;
		using (GraphicsPath path = Bouton.Rond(rMusique, rMusique.Height / 2))
		{
			using (SolidBrush brush = new SolidBrush(muSurvol ? C.CarteSurvol : Color.FromArgb(200, C.Carte)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen((muSurvol || flag) ? C.Vert : C.Ligne);
			g.DrawPath(pen, path);
		}
		Lang.DrawText(g, "\uec4f", fMus, rMusique, flag ? C.Vert : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		if (flag)
		{
			double temps = BanAnim.Temps;
			for (int i = 0; i < 3; i++)
			{
				float num = (float)Zx.P(3) + 5f * (float)Math.Abs(Math.Sin(temps * 5.0 + (double)i * 1.3));
				using SolidBrush brush2 = new SolidBrush(C.Vert);
				g.FillRectangle(brush2, rMusique.Right - Zx.P(14) + i * 4, (float)(rMusique.Bottom - Zx.P(6)) - num, Zx.P(2.5f), num);
			}
		}
		g.SmoothingMode = smoothingMode;
	}

	private void PeindreCloche(Graphics g)
	{
		if (rCloche.Width <= 0)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		int nonLues = Notifs.NonLues;
		GraphicsState graphicsState = Pousse(g, rCloche);
		using (GraphicsPath path = Bouton.Rond(rCloche, rCloche.Height / 2))
		{
			using (SolidBrush brush = new SolidBrush(clSurvol ? C.CarteSurvol : Color.FromArgb(200, C.Carte)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(clSurvol ? C.Jade : C.Ligne);
			g.DrawPath(pen, path);
		}
		Lang.DrawText(g, "\uea8f", fCloche, rCloche, (nonLues > 0) ? Color.White : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		if (nonLues > 0)
		{
			string t = ((nonLues > 9) ? "9+" : nonLues.ToString());
			Rectangle r = new Rectangle(rCloche.Right - Zx.P(16), rCloche.Y - Zx.P(4), Zx.P(22), Zx.P(18));
			using (GraphicsPath path2 = Bouton.Rond(r, Zx.P(9)))
			{
				using SolidBrush brush2 = new SolidBrush(C.Rouge);
				g.FillPath(brush2, path2);
			}
			Lang.DrawText(g, t, fCompte, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		if (graphicsState != null)
		{
			g.Restore(graphicsState);
		}
		g.SmoothingMode = smoothingMode;
	}

	private void MajSuivi()
	{
		if (bSuivre != null && sel != null)
		{
			bool flag = Notifs.Suivi(sel, Omsi.DetecteOmsi());
			bSuivre.Plein = flag;
			bSuivre.Couleur = C.Jade;
			bSuivre.Invalidate();
			bulle.SetToolTip(bSuivre, flag ? "Suivi : vous serez prévenu (mises à jour, sortie). Cliquer pour ne plus suivre." : "Être prévenu des mises à jour et de la sortie de ce contenu");
		}
	}

	private static Annonce An(string titre, string texte, string glyphe, Color c, Projet p)
	{
		Annonce annonce = new Annonce();
		annonce.Titre = titre;
		annonce.Texte = texte;
		annonce.Glyphe = glyphe;
		annonce.Couleur = c;
		annonce.Mod = p;
		return annonce;
	}

	private void MajInfos()
	{
		List<Annonce> list = new List<Annonce>();
		if (Maintenance)
		{
			list.Add(An("Maintenance en cours", (Opt("maintenance_msg").Length > 0) ? Opt("maintenance_msg") : "Installations et mises à jour suspendues quelques instants.", "\ue90f", C.Orange, null));
		}
		if (cat != null && cat.Communaute.TryGetValue("annonces", out var value))
		{
			string[] array = value.Replace("\r", "").Split('\n');
			foreach (string text in array)
			{
				string text2 = text.Trim();
				if (text2.Length != 0)
				{
					int num = text2.IndexOf('|');
					list.Add(An((num > 0) ? text2.Substring(0, num).Trim() : text2, (num > 0) ? text2.Substring(num + 1).Trim() : "", "\ue789", C.Orange, null));
				}
			}
		}
		if (cat != null && cat.LanceurUrl != null && PlusRecent(cat.LanceurVersion, Config.VERSION))
		{
			list.Add(An("Lanceur v" + cat.LanceurVersion + " disponible", "Il s'installe au prochain démarrage.", "\ue895", C.Bleu, null));
		}
		list.Add(An("Nouveau dans le lanceur v" + Config.VERSION, string.Join("  ·  ", NouveautesLanceur), "\ue7c1", C.Jade, null));
		if (cat != null)
		{
			List<Projet> list2 = new List<Projet>(cat.Projets);
			list2.RemoveAll(delegate(Projet p)
			{
				DateTime? dateTime2 = DateProjet(p);
				return p.StatutEffectif == "masque" || !dateTime2.HasValue || (DateTime.Now - dateTime2.Value).TotalDays > 30.0 || p.Zip.Length == 0;
			});
			list2.Sort((Projet a, Projet b) => DateProjet(b).Value.CompareTo(DateProjet(a).Value));
			foreach (Projet item in list2)
			{
				string text3 = (item.Notes ?? "").Replace("\r", "").Split('\n')[0].TrimStart('•', ' ');
				list.Add(An(item.Nom + "  ·  " + item.Version, "Mis à jour le " + DateProjet(item).Value.ToString("dd/MM") + ((text3.Length > 0) ? ("  ·  " + text3) : ""), "\ue896", C.Vert, item));
			}
			foreach (Projet projet in cat.Projets)
			{
				DateTime? dateSortie = projet.DateSortie;
				if (projet.StatutEffectif == "bientot" && dateSortie.HasValue && dateSortie.Value > DateTime.Now)
				{
					list.Add(An(projet.Nom + " · bientôt disponible", "Sortie le " + dateSortie.Value.ToString("dd/MM à HH:mm"), "\ue787", Color.FromArgb(150, 120, 255), projet));
				}
			}
		}
		string text4 = (Themes.CoupeAdmin ? null : Themes.Calendrier(DateTime.Now));
		if (text4 != null)
		{
			DateTime dateTime = DateTime.Now.Date;
			while (Themes.Calendrier(dateTime.AddDays(1.0)) == text4 && dateTime < DateTime.Now.AddDays(60.0))
			{
				dateTime = dateTime.AddDays(1.0);
			}
			list.Add(An("Événement : " + Themes.Noms[Array.IndexOf(Themes.Ids, text4)], "Décor animé du lanceur jusqu'au " + dateTime.ToString("dd/MM"), "\ue734", C.Or, null));
		}
		for (int j = 1; j <= 90; j++)
		{
			if (Themes.CoupeAdmin)
			{
				break;
			}
			string text5 = Themes.Calendrier(DateTime.Now.AddDays(j));
			if (text5 != null && text5 != text4)
			{
				list.Add(An("Bientôt : " + Themes.Noms[Array.IndexOf(Themes.Ids, text5)], "Nouveau décor animé du lanceur à partir du " + DateTime.Now.AddDays(j).ToString("dd/MM"), "\ue787", C.Or, null));
				break;
			}
		}
		annonces = list;
		if (pAccueil != null)
		{
			pAccueil.Invalidate();
		}
	}

	private string Opt(string k)
	{
		if (cat == null || !cat.Communaute.TryGetValue(k, out var value) || value == null)
		{
			return "";
		}
		return value;
	}

	private void PeindreBandeau(Graphics g)
	{
		string text = Opt("alerte");
		string text2 = Opt("alerte_type");
		if (text.Length == 0 && Maintenance)
		{
			text = "Maintenance : " + ((Opt("maintenance_msg").Length > 0) ? Opt("maintenance_msg") : "les installations sont suspendues quelques instants");
			text2 = "avertissement";
		}
		if (text.Length == 0 || rBandeau.Width < 100)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Color baseColor = ((text2 == "important") ? C.Rouge : ((text2 == "avertissement") ? C.Orange : C.Bleu));
		string t = ((text2 == "important") ? "\ue7ba" : ((text2 == "avertissement") ? "\ue90f" : "\ue946"));
		Size size = Lang.MeasureText(text, fAnT);
		int num = Math.Min(rBandeau.Width, size.Width + Zx.P(64));
		Rectangle r = new Rectangle(rBandeau.X + (rBandeau.Width - num) / 2, rBandeau.Y, num, rBandeau.Height);
		using (GraphicsPath path = Bouton.Rond(r, r.Height / 2))
		{
			using (SolidBrush brush = new SolidBrush(Color.FromArgb(70, baseColor)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(170, baseColor));
			g.DrawPath(pen, path);
		}
		Lang.DrawText(g, t, fAnG, new Rectangle(r.X + Zx.P(12), r.Y, Zx.P(24), r.Height), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, text, fAnT, new Rectangle(r.X + Zx.P(42), r.Y, r.Width - Zx.P(54), r.Height), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		g.SmoothingMode = smoothingMode;
	}

	private void PeindreInfos(Graphics g)
	{
		rAnnonces.Clear();
		if (rInfos.IsEmpty || annonces.Count == 0)
		{
			return;
		}
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Lang.DrawText(g, "INFORMATIONS", fStat, new Point(rInfos.X + Zx.P(2), rInfos.Y - Zx.P(26)), C.Gris);
		Rectangle rectangle = rInfos;
		float num = (float)iLeve;
		float num2 = (float)Math.Round(num * 8f) / 8f;
		int num3 = (int)Math.Round(Zx.P(2f) + Zx.P(6f) * num);
		int num4 = rectangle.Width * 100000 + rectangle.Height * 10 + (int)(num2 * 8f);
		if (ombreInfos == null || cleOmbreI != num4)
		{
			if (ombreInfos != null)
			{
				ombreInfos.Dispose();
			}
			ombreInfos = new Bitmap(rectangle.Width + Zx.P(64), rectangle.Height + Zx.P(64) + Zx.P(20), PixelFormat.Format32bppPArgb);
			cleOmbreI = num4;
			using Graphics graphics = Graphics.FromImage(ombreInfos);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			for (int num5 = 12; num5 >= 1; num5--)
			{
				using GraphicsPath path = Bouton.Rond(new Rectangle(Zx.P(32) - Zx.P(2) * num5 + Zx.P(4), Zx.P(32) + Zx.P(9) + (int)((float)Zx.P(7) * num2) - Zx.P(2) * num5, rectangle.Width + Zx.P(4) * num5 - Zx.P(8), rectangle.Height + Zx.P(4) * num5 - Zx.P(8)), Zx.P(14) + Zx.P(2) * num5);
				using SolidBrush brush = new SolidBrush(Color.FromArgb((int)((double)(13 - num5) * (1.3 + 1.1 * (double)num2)), 0, 0, 6));
				graphics.FillPath(brush, path);
			}
		}
		g.DrawImageUnscaled(ombreInfos, rectangle.X - Zx.P(32), rectangle.Y - Zx.P(32));
		using (GraphicsPath path2 = Bouton.Rond(new Rectangle(rectangle.X, rectangle.Y - num3 + Zx.P(6), rectangle.Width - 1, rectangle.Height - 1), Zx.P(14)))
		{
			using LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(rectangle.X, rectangle.Y - num3 + Zx.P(6), rectangle.Width, rectangle.Height + 1), Color.FromArgb(46, 54, 104), Color.FromArgb(12, 14, 32), 90f);
			g.FillPath(brush2, path2);
		}
		if (num > 0.01f)
		{
			using GraphicsPath path3 = Bouton.Rond(Rectangle.Inflate(new Rectangle(rectangle.X, rectangle.Y - num3, rectangle.Width, rectangle.Height), Zx.P(3), Zx.P(3)), Zx.P(17));
			using Pen pen = new Pen(Color.FromArgb((int)(80f * num), C.Orange), Zx.P(3f));
			g.DrawPath(pen, path3);
		}
		rInfos = new Rectangle(rectangle.X, rectangle.Y - num3, rectangle.Width, rectangle.Height);
		try
		{
			using (GraphicsPath path4 = Bouton.Rond(rInfos, Zx.P(14)))
			{
				using SolidBrush brush3 = new SolidBrush(C.Carte);
				g.FillPath(brush3, path4);
			}
			int count = annonces.Count;
			bool flag = count > 1;
			double temps = BanAnim.Temps;
			double num6 = 6.0;
			double num7 = temps % num6;
			int num8 = (flag ? ((int)(temps / num6) % count) : 0);
			double num9 = ((flag && num7 > num6 - 0.6) ? ((num7 - (num6 - 0.6)) / 0.6) : 0.0);
			num9 = num9 * num9 * (3.0 - 2.0 * num9);
			Rectangle rectangle2 = new Rectangle(rInfos.X + Zx.P(2), rInfos.Y + Zx.P(2), rInfos.Width - Zx.P(4), rInfos.Height - Zx.P(4));
			int num10 = (int)Math.Round(num9 * (double)rectangle2.Width);
			if (infosBmp == null || infosBmp.Size != rectangle2.Size)
			{
				if (infosBmp != null)
				{
					infosBmp.Dispose();
				}
				infosBmp = new Bitmap(rectangle2.Width, rectangle2.Height, PixelFormat.Format32bppPArgb);
			}
			string text = string.Concat(num8, "|", num10, "|", count, "|", rectangle2.Size, "|", annonces.GetHashCode());
			if (text == cleInfosBmp)
			{
				rAnnonces.AddRange(rAnnoncesCache);
				for (int i = 0; i < rAnnonces.Count; i++)
				{
					rAnnonces[i] = new KeyValuePair<Rectangle, Annonce>(rInfos, rAnnonces[i].Value);
				}
			}
			else
			{
				cleInfosBmp = text;
				Graphics graphics2 = g;
				g = Graphics.FromImage(infosBmp);
				g.Clear(C.Carte);
				g.SmoothingMode = SmoothingMode.AntiAlias;
				for (int j = 0; j < ((!flag) ? 1 : 2); j++)
				{
					Annonce annonce = annonces[(num8 + j) % count];
					int num11 = j * rectangle2.Width - num10;
					if (num11 < rectangle2.Width && num11 + rectangle2.Width > 0)
					{
						int x = num11 + Zx.P(70);
						int width = rectangle2.Width - Zx.P(70) - Zx.P(16);
						int height = Lang.MeasureText("Ag", fAnT2).Height;
						int height2 = Lang.MeasureText("Ag", fAnX).Height;
						int num12 = Math.Max(1, Math.Min(4, (rectangle2.Height - Zx.P(24) - height - Zx.P(6)) / Math.Max(1, height2)));
						int num13 = Math.Min(num12 * height2, Lang.MeasureText(annonce.Texte, fAnX, new Size(width, Zx.P(400)), TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height);
						int num14 = height + ((annonce.Texte.Length > 0) ? (Zx.P(6) + num13) : 0);
						int num15 = Math.Max(Zx.P(10), (rectangle2.Height - num14) / 2);
						Rectangle rectangle3 = new Rectangle(num11 + Zx.P(16), Math.Max(Zx.P(8), (rectangle2.Height - Zx.P(42)) / 2), Zx.P(42), Zx.P(42));
						using (SolidBrush brush4 = new SolidBrush(Color.FromArgb(55, annonce.Couleur)))
						{
							g.FillEllipse(brush4, rectangle3);
						}
						Lang.DrawText(g, annonce.Glyphe, fAnG2, rectangle3, annonce.Couleur, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
						Lang.DrawText(g, annonce.Titre, fAnT2, new Rectangle(x, num15, width, height), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
						if (annonce.Texte.Length > 0)
						{
							Lang.DrawText(g, annonce.Texte, fAnX, new Rectangle(x, num15 + height + Zx.P(6), width, num13), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
						}
						if (annonce.Mod != null && num10 == 0 && j == 0)
						{
							rAnnonces.Add(new KeyValuePair<Rectangle, Annonce>(rInfos, annonce));
						}
					}
				}
				g.Dispose();
				g = graphics2;
				rAnnoncesCache = new List<KeyValuePair<Rectangle, Annonce>>(rAnnonces);
			}
			using (GraphicsPath path5 = Bouton.Rond(Rectangle.Inflate(rInfos, -1, -1), Zx.P(13)))
			{
				GraphicsState gstate = g.Save();
				g.SetClip(path5, CombineMode.Intersect);
				g.DrawImageUnscaled(infosBmp, rectangle2.X, rectangle2.Y);
				using (LinearGradientBrush brush5 = new LinearGradientBrush(new Rectangle(rInfos.X, rInfos.Y, rInfos.Width, Zx.P(40)), Color.FromArgb((int)(40f + 50f * num), 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
				{
					g.FillRectangle(brush5, rInfos.X, rInfos.Y, rInfos.Width, Zx.P(2));
				}
				g.Restore(gstate);
			}
			using (GraphicsPath path6 = Bouton.Rond(new Rectangle(rInfos.X, rInfos.Y, rInfos.Width - 1, rInfos.Height - 1), Zx.P(14)))
			{
				using Pen pen2 = new Pen(Color.FromArgb((int)(70f + 90f * num), 120, 140, 255), 1.2f);
				g.DrawPath(pen2, path6);
			}
			if (flag && count <= 12)
			{
				int num16 = rInfos.Right - Zx.P(12) - count * Zx.P(9);
				int y = rInfos.Y - Zx.P(18);
				for (int k = 0; k < count; k++)
				{
					using SolidBrush brush6 = new SolidBrush((k == num8) ? C.Orange : Color.FromArgb(70, 255, 255, 255));
					g.FillEllipse(brush6, num16 + k * Zx.P(9), y, Zx.P(6), Zx.P(6));
				}
			}
		}
		finally
		{
			rInfos = rectangle;
		}
		g.SmoothingMode = smoothingMode;
	}

	private string StatutSimple()
	{
		if (jeuEnCours == null)
		{
			return "En ligne";
		}
		return "En jeu · " + jeuEnCours.Nom;
	}

	private void SyncStatut(bool force)
	{
		if (ModeCapture || !Social.Actif)
		{
			return;
		}
		string st = StatutSimple();
		if ((DateTime.Now - dernierAmis).TotalSeconds >= 20.0)
		{
			MajAmis();
		}
		if (!force && st == statutEnvoye && (DateTime.Now - dernierStatut).TotalSeconds < 60.0)
		{
			return;
		}
		statutEnvoye = st;
		dernierStatut = DateTime.Now;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				if (Social.Connecte)
				{
					Social.Presence(st);
				}
				if (Social.StatsPresence(out var a, out var b))
				{
					Sur(delegate
					{
						enLigne = a;
						enJeu = b;
					});
				}
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private static void Calotte(Graphics g, Rectangle b, float n)
	{
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		n = Math.Min(1f, n);
		float num = 3f + 12f * n;
		float num2 = b.Left + Zx.P(7);
		float num3 = b.Right - Zx.P(7);
		float num4 = b.Top + Zx.P(3);
		List<PointF> list = new List<PointF>();
		for (float num5 = num2; num5 <= num3 + 0.1f; num5 += (float)Zx.P(4))
		{
			float num6 = (num5 - num2) / Math.Max(1f, num3 - num2);
			float num7 = (float)Math.Sqrt(Math.Min(1f, Math.Min(num6, 1f - num6) * 9f));
			list.Add(new PointF(num5, num4 - num * num7 * (0.78f + 0.22f * (float)Math.Sin((double)num5 * 0.045 + (double)b.Top * 0.1) + 0.07f * (float)Math.Sin((double)num5 * 0.2))));
		}
		List<PointF> list2 = new List<PointF>(list);
		list2.Add(new PointF(num3, num4));
		list2.Add(new PointF(num2, num4));
		using (GraphicsPath graphicsPath = new GraphicsPath())
		{
			graphicsPath.AddCurve(list.ToArray(), 0.4f);
			graphicsPath.AddLine(num3, num4, num2, num4);
			graphicsPath.CloseFigure();
			using LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(num2, num4 - num - 1f, num3 - num2, num + (float)Zx.P(2)), Color.FromArgb(255, 255, 255), Color.FromArgb(206, 222, 248), 90f);
			g.FillPath(brush, graphicsPath);
		}
		using (Pen pen = new Pen(Color.FromArgb(200, 255, 255, 255), 1.2f))
		{
			g.DrawCurve(pen, list.ToArray(), 0.4f);
		}
		using (Pen pen2 = new Pen(Color.FromArgb(70, 150, 175, 230), 1f))
		{
			g.DrawLine(pen2, num2 + (float)Zx.P(4), num4 - 1f, num3 - (float)Zx.P(4), num4 - 1f);
		}
		g.SmoothingMode = smoothingMode;
	}

	private void MajTheme()
	{
		string text = Themes.Courant();
		if (!(text != idTheme) && (text == null || noel != null))
		{
			return;
		}
		idTheme = text;
		noel = Themes.Creer(text, 0);
		themeJeux = Themes.Creer(text, 1);
		try
		{
			SuivreTuiles();
			foreach (Tuile item in tuilesA)
			{
				item.Invalidate();
			}
		}
		catch
		{
		}
	}

	private void OuvrirDiscord()
	{
		string text = InviteDiscord();
		if (!Regex.IsMatch(text ?? "", "^https://(discord\\.gg|discord\\.com/invite)/[A-Za-z0-9-]{2,32}$"))
		{
			text = "https://discord.gg/dA77S5nXVV";
		}
		try
		{
			Process.Start(text);
			Compte actif = Comptes.Actif;
			if (actif != null && Progression.S(actif, "discord") == 0)
			{
				actif.Stats["discord"] = 1;
				Progression.Verifier(actif);
				Comptes.Sauvegarder();
			}
		}
		catch (Exception ex)
		{
			Toast("Impossible d'ouvrir Discord : " + ex.Message, C.Rouge, 5000);
		}
	}

	private void PeindreDiscord(Graphics g)
	{
		if (rDiscord.Width <= 0)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		Color baseColor = Color.FromArgb(88, 101, 242);
		Rectangle r = rDiscord;
		int discordCoupe = DiscordCoupe;
		if (dcSurvol)
		{
			using GraphicsPath path = Bouton.Rond(new Rectangle(r.X - Zx.P(4), r.Y - Zx.P(4), r.Width + Zx.P(8), r.Height + Zx.P(8)), (r.Height + Zx.P(8)) / 2);
			using SolidBrush brush = new SolidBrush(Color.FromArgb(50, baseColor));
			g.FillPath(brush, path);
		}
		GraphicsState graphicsState = Pousse(g, r, appuiDiscord);
		using (GraphicsPath path2 = Bouton.Rond(r, r.Height / 2))
		{
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height + 1), ControlPaint.Light(baseColor, 0.08f), ControlPaint.Dark(baseColor, 0.1f), 90f))
			{
				g.FillPath(brush2, path2);
			}
			if (dcMoitie > 0)
			{
				GraphicsState gstate = g.Save();
				g.SetClip(path2, CombineMode.Intersect);
				Rectangle rect = ((dcMoitie == 1) ? new Rectangle(r.X, r.Y, discordCoupe - r.X, r.Height) : new Rectangle(discordCoupe, r.Y, r.Right - discordCoupe, r.Height));
				using (SolidBrush brush3 = new SolidBrush(Color.FromArgb(45, 255, 255, 255)))
				{
					g.FillRectangle(brush3, rect);
				}
				g.Restore(gstate);
			}
			using Pen pen = new Pen(Color.FromArgb(dcSurvol ? 140 : 80, 255, 255, 255));
			g.DrawPath(pen, path2);
		}
		using (Pen pen2 = new Pen(Color.FromArgb(90, 255, 255, 255)))
		{
			g.DrawLine(pen2, discordCoupe, r.Y + Zx.P(9), discordCoupe, r.Bottom - Zx.P(9));
		}
		LogoDiscord(g, new Rectangle(r.X + Zx.P(14), r.Y + (r.Height - Zx.P(18)) / 2, Zx.P(24), Zx.P(18)), Color.White, baseColor);
		Lang.DrawText(g, "Rejoindre", fDisc1, new Rectangle(r.X + Zx.P(46), r.Y + Zx.P(6), discordCoupe - r.X - Zx.P(50), Zx.P(20)), Color.White, TextFormatFlags.NoPadding);
		Lang.DrawText(g, "TPS Groupe", fDisc2, new Rectangle(r.X + Zx.P(46), r.Y + Zx.P(26), discordCoupe - r.X - Zx.P(50), Zx.P(16)), Color.FromArgb(222, 226, 255), TextFormatFlags.NoPadding);
		Lang.DrawText(g, "Ouvrir", fDisc1, new Rectangle(discordCoupe + Zx.P(6), r.Y + Zx.P(6), r.Right - discordCoupe - Zx.P(12), Zx.P(20)), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, "Discord ↗", fDisc2, new Rectangle(discordCoupe + Zx.P(6), r.Y + Zx.P(26), r.Right - discordCoupe - Zx.P(12), Zx.P(16)), Color.FromArgb(222, 226, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		if (graphicsState != null)
		{
			g.Restore(graphicsState);
		}
	}

	private void OuvrirAppDiscord()
	{
		string inv = InviteDiscord();
		if (!Regex.IsMatch(inv ?? "", "^https://(discord\\.gg|discord\\.com/invite)/[A-Za-z0-9-]{2,32}$"))
		{
			inv = "https://discord.gg/dA77S5nXVV";
		}
		string code = inv.Substring(inv.LastIndexOf('/') + 1);
		string text = Config.Lire("discord_serveur_" + code) ?? "";
		Action ouvrir = delegate
		{
			string text2 = Config.Lire("discord_serveur_" + code) ?? "";
			try
			{
				Process.Start((text2.Length > 0) ? ("discord://-/channels/" + text2) : ("discord://-/invite/" + code));
			}
			catch
			{
				try
				{
					Process.Start((text2.Length > 0) ? ("https://discord.com/channels/" + text2) : inv);
				}
				catch (Exception ex)
				{
					Toast("Impossible d'ouvrir Discord : " + ex.Message, C.Rouge, 5000);
				}
			}
		};
		if (text.Length > 0)
		{
			ouvrir();
			return;
		}
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				using WebClient webClient = Web.Client();
				string input = webClient.DownloadString("https://discord.com/api/v10/invites/" + code);
				Match match = Regex.Match(input, "\"guild\"\\s*:\\s*\\{[^}]*?\"id\"\\s*:\\s*\"(\\d{15,22})\"");
				if (match.Success)
				{
					Config.Ecrire("discord_serveur_" + code, match.Groups[1].Value);
				}
			}
			catch
			{
			}
			Sur(ouvrir);
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private static void LogoDiscord(Graphics g, Rectangle r, Color c, Color fond)
	{
		float num = r.X;
		float num2 = r.Y;
		float num3 = r.Width;
		float num4 = r.Height;
		using (SolidBrush brush = new SolidBrush(c))
		{
			using (GraphicsPath path = Bouton.Rond(new Rectangle(r.X, r.Y + (int)(num4 * 0.1f), r.Width, (int)(num4 * 0.72f)), (int)(num4 * 0.34f)))
			{
				g.FillPath(brush, path);
			}
			g.FillEllipse(brush, num + num3 * 0.06f, num2, num3 * 0.34f, num4 * 0.42f);
			g.FillEllipse(brush, num + num3 * 0.6f, num2, num3 * 0.34f, num4 * 0.42f);
			g.FillPolygon(brush, new PointF[4]
			{
				new PointF(num + num3 * 0.02f, num2 + num4 * 0.55f),
				new PointF(num + num3 * 0.3f, num2 + num4 * 0.78f),
				new PointF(num + num3 * 0.22f, num2 + num4),
				new PointF(num, num2 + num4 * 0.82f)
			});
			g.FillPolygon(brush, new PointF[4]
			{
				new PointF(num + num3 * 0.98f, num2 + num4 * 0.55f),
				new PointF(num + num3 * 0.7f, num2 + num4 * 0.78f),
				new PointF(num + num3 * 0.78f, num2 + num4),
				new PointF(num + num3, num2 + num4 * 0.82f)
			});
		}
		using SolidBrush brush2 = new SolidBrush(fond);
		g.FillEllipse(brush2, num + num3 * 0.38f, num2 - num4 * 0.08f, num3 * 0.24f, num4 * 0.2f);
		g.FillEllipse(brush2, num + num3 * 0.26f, num2 + num4 * 0.4f, num3 * 0.16f, num4 * 0.26f);
		g.FillEllipse(brush2, num + num3 * 0.58f, num2 + num4 * 0.4f, num3 * 0.16f, num4 * 0.26f);
	}

	private void ConstruireAccueil()
	{
		pAccueil.Controls.Clear();
		tuilesA.Clear();
		if (cacheProfil != null)
		{
			cacheProfil.Dispose();
			cacheProfil = null;
		}
		Tuile tuile = Menu("\ue768", "Jouer", "Choisir un jeu, puis une carte", delegate
		{
			ShowVue(Vue.Jeux);
		});
		tuile.Choisie = true;
		if (Opt("boutique") != "0")
		{
			Menu("\ue719", "Boutique", Social.Vip ? ("DLC et cosmétiques  ·  " + Social.Points + " points VIP") : "DLC et cosmétiques TPS Groupe", delegate
			{
				Boutique();
			});
		}
		Menu("\ue713", "Réglages", "Affichage, mises à jour, dossiers, Discord", delegate
		{
			Parametres();
		});
		if (Comptes.EstAdmin)
		{
			Menu("\ue7ef", "Administration", "Pages du catalogue", delegate
			{
				Admin();
			});
		}
		Menu("\ue7e8", "Quitter", "Fermer le lanceur", delegate
		{
			Quitter();
		});
		if (cat != null)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (Projet projet in cat.Projets)
			{
				if (projet.Installable)
				{
					string key = JeuDe(projet);
					dictionary.TryGetValue(key, out var value);
					dictionary[key] = value + 1;
				}
			}
			if (!dictionary.ContainsKey("omsi2"))
			{
				dictionary["omsi2"] = 0;
			}
			foreach (KeyValuePair<string, int> item in dictionary)
			{
				Progression.EnregistrerJeu(item.Key, NomJeu(item.Key), item.Value);
			}
		}
		MajTheme();
		foreach (Tuile item2 in tuilesA)
		{
			item2.Neige = ((noel != null && noel.NeigeTuiles) ? 1f : 0f);
		}
		ConstruireDiapos();
		DisposerAccueil();
		MajInfos();
	}

	private void Quitter()
	{
		if (!occupe || MessageBox.Show(this, "Une installation est en cours : la fermer maintenant l'interrompt.\n\nQuitter quand même ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
		{
			quitterVraiment = true;
			Close();
		}
	}

	private void QuitterVraiment()
	{
		if (miniTray != null && !miniTray.IsDisposed)
		{
			miniTray.Close();
		}
		if (occupe)
		{
			Reapparaitre();
		}
		Quitter();
	}

	private void CacherDansZone()
	{
		if (miniTray != null && !miniTray.IsDisposed)
		{
			miniTray.Close();
		}
		Hide();
		if (!Config.Oui("zone_info_vue", defaut: false))
		{
			Config.Ecrire("zone_info_vue", "1");
			try
			{
				icone.ShowBalloonTip(6000, Lang.T("TPS Groupe reste ouvert"), Lang.T("Il continue dans la zone de notification, près de l'horloge : clique sur l'icône pour la musique et « Ouvrir le lanceur », ou « Quitter » pour le fermer."), ToolTipIcon.Info);
			}
			catch
			{
			}
		}
	}

	public void Reapparaitre()
	{
		if (!base.Visible)
		{
			Show();
		}
		if (base.WindowState == FormWindowState.Minimized)
		{
			base.WindowState = FormWindowState.Normal;
		}
		Activate();
		PanelVisible()?.Invalidate();
	}

	protected override void WndProc(ref Message m)
	{
		if (MsgMontrer != 0 && m.Msg == MsgMontrer)
		{
			Reapparaitre();
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	private void DisposerAccueil()
	{
		pAccueil.AutoScrollPosition = new Point(0, 0);
		int width = pAccueil.ClientSize.Width;
		int height = pAccueil.ClientSize.Height;
		int num = Marge(width);
		int width2 = Math.Max(Zx.P(230), Math.Min(Zx.P(270), (int)((double)width * 0.14)));
		int num2 = Math.Max(Zx.P(64), Math.Min(Zx.P(78), (int)((double)height * 0.07)));
		rProfil = new Rectangle(num, num, width2, num2);
		CalculerMajAccueil();
		bool flag0 = MajAAfficher || Opt("alerte").Length > 0 || Maintenance;
		int num3 = num + (flag0 ? Zx.P(262) : Zx.P(200));
		int num4 = height - num - Zx.P(14);
		int num5 = Math.Max(Zx.P(220), num4 - num3);
		int count = tuilesA.Count;
		int num6 = Zx.P(14);
		int num7 = Math.Max(Zx.P(340), Math.Min(Zx.P(560), (int)((double)width * 0.34)));
		int num8 = Math.Max(Zx.P(72), Math.Min(Zx.P(140), (num5 - num6 * Math.Max(0, count - 1)) / Math.Max(1, count)));
		if (count > 1)
		{
			num6 = Math.Min(Zx.P(70), Math.Max(num6, (num5 - count * num8) / (count - 1)));
		}
		int num9 = (width - num7) / 2;
		int num10 = num3;
		List<Rectangle> list = new List<Rectangle>();
		foreach (Tuile item in tuilesA)
		{
			item.SetBounds(num9, num10, num7, num8);
			item.IndiceDecor = list.Count;
			list.Add(item.Bounds);
			num10 += num8 + num6;
		}
		ThemeBase.Tuiles = list.ToArray();
		etatsTuiles = new int[list.Count];
		int num11 = Math.Max(Zx.P(260), Math.Min(Zx.P(400), (int)((double)width * 0.2)));
		int num12 = width - num - num11;
		if (num12 < num9 + num7 + Zx.P(24))
		{
			num12 = num9 + num7 + Zx.P(24);
		}
		num11 = Math.Max(Zx.P(180), Math.Min(num11, width - num - num12));
		if (list.Count >= 4 && num6 >= Zx.P(40))
		{
			rCarte = new Rectangle(num12, list[0].Y, num11, list[1].Bottom - list[0].Y);
			rInfos = new Rectangle(num12, list[2].Y, num11, list[2].Height);
			rPilule = new Rectangle(num12, list[3].Y, num11, Zx.P(44));
		}
		else
		{
			int num13 = Math.Min(num5, (int)((double)num11 * 0.42) + Zx.P(120));
			rCarte = new Rectangle(num12, num3, num11, num13);
			int num14 = num3 + num13 + Zx.P(46);
			int num15 = Math.Min(num4 - num14, Zx.P(110));
			rInfos = ((num15 >= Zx.P(84)) ? new Rectangle(num12, num14, num11, num15) : Rectangle.Empty);
			rPilule = (rInfos.IsEmpty ? Rectangle.Empty : new Rectangle(rInfos.X, rInfos.Bottom + Zx.P(16), rInfos.Width, Zx.P(44)));
		}
		rBandeau = new Rectangle(num9, num + Zx.P(202), num7, Zx.P(36));
		rMajA = (MajAAfficher ? new Rectangle(num9, num + Zx.P(198), num7, Zx.P(48)) : Rectangle.Empty);
		ThemeBase.Cartes = (rInfos.IsEmpty ? new Rectangle[2] { rProfil, rCarte } : new Rectangle[3] { rProfil, rCarte, rInfos });
		rDiscord = new Rectangle(Zx.P(24), height - Zx.P(24) - Zx.P(48), Zx.P(250), Zx.P(48));
		int num16 = Math.Max((int)((double)height * 0.37), rProfil.Y + Zx.P(452) + Zx.P(20));
		if (rDiscord.Y - Zx.P(44) - num16 < Zx.P(220))
		{
			num16 = Math.Max(rProfil.Bottom + Zx.P(70), rDiscord.Y - Zx.P(44) - Zx.P(220));
		}
		rAmisA = new Rectangle(num, num16, Zx.P(300), Math.Max(0, Math.Min(Zx.P(560), rDiscord.Y - Zx.P(44) - num16)));
		rCloche = new Rectangle(width - num - Zx.P(44), num + Zx.P(6), Zx.P(44), Zx.P(44));
		rMusique = Rectangle.Empty;
		ThemeBase.ZoneLibre = new Rectangle(num, rProfil.Bottom + Zx.P(16), Math.Max(Zx.P(200), rAmisA.Width), Math.Max(0, rAmisA.Y - rProfil.Bottom - Zx.P(32)));
		pAccueil.Invalidate();
	}

	private Image CouvertureJeu(string id)
	{
		if (couvJeux.TryGetValue(id, out var value))
		{
			return value;
		}
		value = null;
		try
		{
			string text = ((id == "omsi2") ? "252530" : null);
			if (text != null)
			{
				List<string> list = new List<string>();
				try
				{
					string text2 = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null) as string;
					if (!string.IsNullOrEmpty(text2))
					{
						list.Add(text2.Replace('/', '\\'));
					}
				}
				catch
				{
				}
				string text3 = Omsi.DetecteOmsi();
				if (text3 != null)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(text3);
					if (directoryInfo.Parent != null && directoryInfo.Parent.Parent != null && directoryInfo.Parent.Parent.Parent != null)
					{
						list.Add(directoryInfo.Parent.Parent.Parent.FullName);
					}
				}
				list.Add("C:\\Program Files (x86)\\Steam");
				foreach (string item in list)
				{
					string[] array = new string[2] { "header.jpg", "library_hero.jpg" };
					foreach (string text4 in array)
					{
						string path = Path.Combine(item, "appcache", "librarycache", text, text4);
						if (!File.Exists(path))
						{
							continue;
						}
						using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(path)))
						{
							using Image original = Image.FromStream(stream);
							value = new Bitmap(original);
						}
						break;
					}
					if (value != null)
					{
						break;
					}
				}
			}
		}
		catch
		{
			value = null;
		}
		couvJeux[id] = value;
		return value;
	}

	private void ConstruireJeux()
	{
		pJeux.Controls.Clear();
		tuilesJ.Clear();
		MajTheme();
		FichesJeux.Clear();
		if (cat != null)
		{
			if (cat.JeuOmsi != null)
			{
				FichesJeux["omsi2"] = cat.JeuOmsi;
			}
			foreach (Dictionary<string, string> item in cat.JeuxWeb)
			{
				FichesJeux[item["id"]] = item;
			}
		}
		bRetJeux = Petit("\ue72b", delegate
		{
			ShowVue(Vue.Accueil);
		});
		bRetJeux.Font = Integre.fNav;
		bRetJeux.Size = new Size(Zx.P(48), Zx.P(48));
		bulle.SetToolTip(bRetJeux, "Accueil (Échap)");
		pJeux.Controls.Add(bRetJeux);
		List<string> list = new List<string>();
		list.Add("omsi2");
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (!list.Contains(JeuDe(projet)))
				{
					list.Add(JeuDe(projet));
				}
			}
		}
		string omsi = Omsi.DetecteOmsi();
		foreach (string item2 in list)
		{
			int num = 0;
			int num2 = 0;
			if (cat != null)
			{
				foreach (Projet projet2 in cat.Projets)
				{
					if (JeuDe(projet2) == item2)
					{
						LibelleEtat(projet2, omsi, out var _, out var code);
						num++;
						if (code >= 1)
						{
							num2++;
						}
					}
				}
			}
			Tuile tuile = new Tuile();
			tuile.Titre = NomJeu(item2);
			tuile.Sous = SousJeu(item2);
			tuile.Img = ((item2 == "omsi2" && cat != null && cat.JeuOmsi != null && ValJeu("omsi2", "image").Length > 0) ? (CouvertureWeb(cat.JeuOmsi) ?? CouvertureJeu(item2)) : CouvertureJeu(item2));
			tuile.Glyphe = "\ue7fc";
			tuile.Badge = num + ((num > 1) ? " cartes" : " carte") + "  ·  " + num2 + ((num2 > 1) ? " installées" : " installée");
			tuile.BadgeCouleur = ((num2 > 0) ? C.Vert : C.Gris);
			tuile.Choisie = item2 == jeuId;
			string jid2 = item2;
			tuile.Click += delegate
			{
				jeuId = jid2;
				Config.Ecrire("jeu", jid2);
				filtre = 0;
				ShowVue(Vue.Biblio);
			};
			if (item2 == "omsi2")
			{
				tuile.Coin = "Paramètres";
				tuile.ClicCoin += delegate
				{
					ChoisirDossiers();
					RafraichirVues();
				};
			}
			pJeux.Controls.Add(tuile);
			tuilesJ.Add(tuile);
		}
		if (cat != null)
		{
			foreach (Dictionary<string, string> item3 in cat.JeuxWeb)
			{
				Dictionary<string, string> i = item3;
				string value;
				string text = (i.TryGetValue("statut", out value) ? value : "publie");
				bool estAdmin = Comptes.EstAdmin;
				if (text == "masque" && !estAdmin)
				{
					continue;
				}
				Tuile tuile2 = new Tuile();
				tuile2.Titre = (i.TryGetValue("nom", out value) ? value : i["id"]);
				tuile2.Sous = (i.TryGetValue("sous_titre", out value) ? value : "Jeu en ligne");
				tuile2.Glyphe = "\ue7fc";
				tuile2.Img = CouvertureWeb(i);
				if (text == "bientot")
				{
					tuile2.Badge = (estAdmin ? "Prochainement  ·  aperçu admin" : "Prochainement");
					tuile2.BadgeCouleur = C.Orange;
					tuile2.Prix = "BIENTÔT";
					tuile2.PrixCouleur = C.Orange;
				}
				else if (text == "masque")
				{
					tuile2.Badge = "Masqué  ·  visible par les admins";
					tuile2.BadgeCouleur = C.Gris;
				}
				else
				{
					tuile2.Badge = "▶  Jouer · jeu en ligne gratuit";
					tuile2.BadgeCouleur = C.Vert;
				}
				string nomJ = tuile2.Titre;
				string jid = i["id"];
				JeuxWebParId[jid] = i;
				int num3 = 0;
				foreach (Projet projet3 in cat.Projets)
				{
					if (JeuDe(projet3) == jid)
					{
						num3++;
					}
				}
				if (text.Length == 0 || text == "publie")
				{
					tuile2.Badge = ((num3 > 0) ? (num3 + ((num3 > 1) ? " mods" : " mod") + "  ·  ") : "") + "jeu en ligne gratuit";
					tuile2.BadgeCouleur = C.Vert;
				}
				tuile2.Choisie = jid == jeuId;
				tuile2.Click += delegate
				{
					if (!JeuWebOuvert(i))
					{
						Toast(nomJ + " arrive bientôt sur le lanceur !", C.Orange, 5000);
					}
					else
					{
						jeuId = jid;
						Config.Ecrire("jeu", jid);
						filtre = 0;
						ShowVue(Vue.Biblio);
					}
				};
				pJeux.Controls.Add(tuile2);
				tuilesJ.Add(tuile2);
			}
		}
		Tuile tuile3 = new Tuile();
		tuile3.Titre = "D'autres jeux bientôt";
		tuile3.Sous = "Le lanceur accueillera les prochains jeux TPS Groupe.";
		tuile3.Glyphe = "\ue710";
		tuile3.Inactive = true;
		tuile3.Cursor = Cursors.Default;
		pJeux.Controls.Add(tuile3);
		tuilesJ.Add(tuile3);
		DisposerJeux();
	}

	private Image CouvertureWeb(Dictionary<string, string> j)
	{
		string id = j["id"];
		if (couvJeux.TryGetValue("web:" + id, out var value))
		{
			return value;
		}
		string f = Path.Combine(Config.Cache, "jeu_" + id + ".img");
		if (File.Exists(f))
		{
			try
			{
				using MemoryStream stream = new MemoryStream(File.ReadAllBytes(f));
				using Image image = Image.FromStream(stream);
				value = (((float)image.Width < (float)image.Height * 1.6f) ? BanniereLogo(image) : new Bitmap(image));
			}
			catch
			{
				value = null;
			}
			couvJeux["web:" + id] = value;
			return value;
		}
		couvJeux["web:" + id] = null;
		if (j.TryGetValue("image", out var value2) && Uri.TryCreate(value2, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp))
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				try
				{
					using WebClient webClient = Web.Client();
					byte[] array = webClient.DownloadData(u);
					if (array.Length > 0 && array.Length < 8388608)
					{
						File.WriteAllBytes(f, array);
						Sur(delegate
						{
							couvJeux.Remove("web:" + id);
							if (pJeux != null && pJeux.Visible)
							{
								ConstruireJeux();
							}
						});
					}
				}
				catch
				{
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}
		return null;
	}

	private bool FenetreJeuWeb()
	{
		try
		{
			(jeuWebProfil ?? "").Replace("'", "''");
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE Name = 'msedge.exe' OR Name = 'chrome.exe'");
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				if (item["CommandLine"] is string text && text.IndexOf(jeuWebProfil, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
		}
		catch
		{
			return true;
		}
		return false;
	}

	private static Bitmap BanniereLogo(Image logo)
	{
		Bitmap bitmap = new Bitmap(logo);
		Color baseColor = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 6);
		if (baseColor.A < 128)
		{
			baseColor = C.Jade;
		}
		Bitmap bitmap2 = new Bitmap(Zx.P(800), Zx.P(340), PixelFormat.Format32bppPArgb);
		using (Graphics graphics = Graphics.FromImage(bitmap2))
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, Zx.P(800), Zx.P(341)), Color.FromArgb(18, 22, 40), Color.FromArgb(baseColor.R / 3, baseColor.G / 3, baseColor.B / 3), 25f))
			{
				graphics.FillRectangle(brush, 0, 0, Zx.P(800), Zx.P(340));
			}
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(140, -30, 520, 400);
				using PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
				pathGradientBrush.CenterColor = Color.FromArgb(110, baseColor);
				pathGradientBrush.SurroundColors = new Color[1] { Color.FromArgb(0, baseColor) };
				graphics.FillPath(pathGradientBrush, graphicsPath);
			}
			int num = 220;
			graphics.DrawImage(bitmap, Zx.P(400) - num / 2, Zx.P(170) - num / 2 - Zx.P(6), num, num);
		}
		bitmap.Dispose();
		return bitmap2;
	}

	private void LancerJeuWeb(Dictionary<string, string> j)
	{
		string uriString = j["url"];
		if (!j.TryGetValue("nom", out var value))
		{
			value = j["id"];
		}
		if (!Uri.TryCreate(uriString, UriKind.Absolute, out var result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp))
		{
			return;
		}
		string text = null;
		string[] array = new string[2] { "msedge.exe", "chrome.exe" };
		foreach (string text2 in array)
		{
			RegistryKey[] array2 = new RegistryKey[2]
			{
				Registry.CurrentUser,
				Registry.LocalMachine
			};
			foreach (RegistryKey registryKey in array2)
			{
				try
				{
					using RegistryKey registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + text2);
					string text3 = ((registryKey2 != null) ? (registryKey2.GetValue("") as string) : null);
					if (!string.IsNullOrEmpty(text3) && File.Exists(text3.Trim('"')))
					{
						text = text3.Trim('"');
						break;
					}
				}
				catch
				{
				}
			}
			if (text == null && text2 == "msedge.exe")
			{
				string[] array3 = new string[2]
				{
					Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
					Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
				};
				foreach (string path in array3)
				{
					string text4 = Path.Combine(path, "Microsoft\\Edge\\Application\\msedge.exe");
					if (File.Exists(text4))
					{
						text = text4;
						break;
					}
				}
			}
			if (text != null)
			{
				break;
			}
		}
		try
		{
			string value2;
			bool flag = !j.TryGetValue("plein_ecran", out value2) || value2 != "0";
			string text5 = Path.Combine(Config.Dossier, "jeux", j["id"]);
			try
			{
				Directory.CreateDirectory(text5);
			}
			catch
			{
			}
			if (text != null)
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo(text, "--user-data-dir=\"" + text5 + "\" --no-first-run --no-default-browser-check --app=\"" + result.AbsoluteUri + "\"" + (flag ? " --start-fullscreen" : " --window-size=1600,900"));
				processStartInfo.UseShellExecute = false;
				Process.Start(processStartInfo);
			}
			else
			{
				Process.Start(result.AbsoluteUri);
			}
			Toast("▶  " + value + " s'ouvre" + ((text != null && flag) ? " en plein écran (F11 pour en sortir)" : " dans sa fenêtre"), C.Vert, 5000);
			try
			{
				Comptes.Noter(j["id"]);
			}
			catch
			{
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["id"] = j["id"];
			dictionary["nom"] = value;
			dictionary["jeu"] = j["id"];
			jeuEnCours = Projet.De(dictionary);
			debutJeu = DateTime.Now;
			jeuWebProfil = ((text != null) ? text5 : null);
			MajPresence();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Impossible d'ouvrir " + value + " :\n" + ex.Message, Text);
		}
	}

	private void DisposerJeux()
	{
		Grille(pJeux, tuilesJ, Zx.P(340), Zx.P(290), Zx.P(140), Zx.P(40), 0.74);
		if (bRetJeux != null)
		{
			bRetJeux.Location = new Point(Zx.P(40), Zx.P(30));
		}
		pJeux.Invalidate();
	}

	private Bouton Chip(string t, string id)
	{
		Bouton bouton = Petit(t, delegate
		{
			categorie = id;
			RemplirBiblio();
		});
		bouton.Tag = id;
		bouton.Height = Zx.P(32);
		return bouton;
	}

	private Bouton Filtre(string t, int f)
	{
		Bouton bouton = Petit(t, delegate
		{
			filtre = f;
			RemplirBiblio();
		});
		bouton.Size = new Size(Zx.P(130), Zx.P(32));
		return bouton;
	}

	private void ConstruireBiblio()
	{
		pBiblio.Controls.Clear();
		tuilesB.Clear();
		bAccueilB = Integre.Accueil(Zx.P(96), Zx.P(30));
		bulle.SetToolTip(bAccueilB, "Accueil");
		bChangerJeu = Petit("\ue72b", delegate
		{
			ShowVue(Vue.Jeux);
		});
		bChangerJeu.Font = Integre.fNav;
		bChangerJeu.Size = new Size(Zx.P(48), Zx.P(48));
		bulle.SetToolTip(bChangerJeu, "Choix du jeu (Échap)");
		bChemins = Petit("⚙  Chemins du jeu", delegate
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
				ChoisirDossiers();
				RafraichirVues();
			}
		});
		bChemins.Size = new Size(Zx.P(170), Zx.P(34));
		tRecherche = new Champ(multi: false, Zx.P(36));
		tRecherche.Indice = "Rechercher (nom ou tag)…";
		tRecherche.TextChanged += delegate
		{
			RemplirBiblio();
		};
		fTous = Filtre("Toutes", 0);
		fInst = Filtre("Installées", 1);
		fMaj = Filtre("Mises à jour", 2);
		bPhotosJeu = Petit("▣  Photos", delegate
		{
			PhotosJeu();
		});
		bPhotosJeu.Size = new Size(Zx.P(120), Zx.P(34));
		bVideoJeu = Petit("▶  Bande-annonce", delegate
		{
			string text = ValJeu(jeuId, "video");
			if (text.Length > 0)
			{
				using (FenVideo f = new FenVideo(NomJeu(jeuId), text))
				{
					f.Afficher(this);
				}
			}
		});
		bVideoJeu.Size = new Size(Zx.P(150), Zx.P(34));
		bJouerOmsi = Petit("▶  Jouer à OMSI 2", delegate
		{
			JouerOmsi();
		});
		bJouerOmsi.Plein = true;
		bJouerOmsi.Couleur = C.Orange;
		Control[] array = new Control[10] { bChangerJeu, bAccueilB, bChemins, bJouerOmsi, bPhotosJeu, bVideoJeu, tRecherche, fTous, fInst, fMaj };
		foreach (Control value in array)
		{
			pBiblio.Controls.Add(value);
		}
		chips.Clear();
		chips.Add(Chip("Tout", ""));
		string[] idsCat = Categories.IdsDe(jeuId);
		string[] nomsCat = Categories.NomsDe(jeuId);
		if (categorie.Length > 0 && Array.IndexOf(idsCat, categorie) < 0)
		{
			categorie = "";
		}
		for (int j = 0; j < idsCat.Length; j++)
		{
			chips.Add(Chip(nomsCat[j], idsCat[j]));
		}
		foreach (Bouton chip in chips)
		{
			pBiblio.Controls.Add(chip);
		}
		ConstruireOngletsBiblio();
		RemplirBiblio();
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr h, int msg, IntPtr w, string l);

	private void JouerOmsi()
	{
		string text = Omsi.DetecteOmsi();
		if (text == null)
		{
			Dossiers("Indiquez d'abord le dossier d'OMSI 2.");
			return;
		}
		string text2 = Omsi.DetecteOpen(text);
		using FenChoixOmsi fenChoixOmsi = new FenChoixOmsi(text2 != null);
		if (fenChoixOmsi.Afficher(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			if (fenChoixOmsi.Choix == "open")
			{
				if (text2 == null)
				{
					Dossiers("openOMSI est introuvable : indiquez son dossier (celui qui contient openomsi.exe).");
					return;
				}
				string text3 = Path.Combine(text2, "openomsi-launcher.exe");
				if (!File.Exists(text3))
				{
					text3 = Path.Combine(text2, "openomsi.exe");
				}
				ProcessStartInfo processStartInfo = new ProcessStartInfo(text3);
				processStartInfo.WorkingDirectory = text2;
				processStartInfo.UseShellExecute = true;
				Process.Start(processStartInfo);
			}
			else
			{
				bool flag = false;
				try
				{
					flag = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamExe", null) != null;
				}
				catch
				{
				}
				if (flag)
				{
					Process.Start("steam://rungameid/252530");
				}
				else
				{
					ProcessStartInfo processStartInfo2 = new ProcessStartInfo(Path.Combine(text, "Omsi.exe"));
					processStartInfo2.WorkingDirectory = text;
					processStartInfo2.UseShellExecute = true;
					Process.Start(processStartInfo2);
				}
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["id"] = "omsi2";
			dictionary["nom"] = "OMSI 2";
			dictionary["jeu"] = "omsi2";
			jeuEnCours = Projet.De(dictionary);
			debutJeu = DateTime.Now;
			MajPresence();
			Toast("▶  " + ((fenChoixOmsi.Choix == "open") ? "openOMSI" : "OMSI 2") + " se lance…", C.Vert, 5000);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Impossible de lancer le jeu : " + ex.Message, Text);
		}
	}

	private void PhotosJeu()
	{
		List<string> urls = new List<string>();
		string[] array = ValJeu(jeuId, "galerie").Replace("\r", "").Split('\n');
		foreach (string text in array)
		{
			if (Uri.TryCreate(text.Trim(), UriKind.Absolute, out var result) && (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp) && urls.Count < 10)
			{
				urls.Add(result.AbsoluteUri);
			}
		}
		if (urls.Count == 0)
		{
			return;
		}
		Toast("Chargement des photos…", C.Bleu, 2500);
		Thread thread = new Thread((ThreadStart)delegate
		{
			List<Image> ims = new List<Image>();
			foreach (string item in urls)
			{
				try
				{
					using WebClient webClient = Web.Client();
					using MemoryStream stream = new MemoryStream(webClient.DownloadData(item));
					using Image original = Image.FromStream(stream);
					ims.Add(new Bitmap(original));
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
					return;
				}
				using FenVisionneuse f = new FenVisionneuse(ims, 0);
				f.Afficher(this);
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void RemplirBiblio()
	{
		if (tRecherche == null)
		{
			return;
		}
		foreach (Tuile item in tuilesB)
		{
			pBiblio.Controls.Remove(item);
			item.Dispose();
		}
		tuilesB.Clear();
		Bouton[] array = new Bouton[3] { fTous, fInst, fMaj };
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Plein = i == filtre;
			array[i].Couleur = C.JadeFonce;
			array[i].Invalidate();
		}
		string omsi = Omsi.DetecteOmsi();
		string text = tRecherche.Text.Trim().ToLowerInvariant();
		int num = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num2 = 0;
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				if (JeuDe(projet) == jeuId)
				{
					dictionary.TryGetValue(projet.Categorie, out var value);
					dictionary[projet.Categorie] = value + 1;
					num2++;
				}
			}
		}
		foreach (Bouton chip in chips)
		{
			string text2 = (string)chip.Tag;
			int value2;
			if (text2.Length == 0)
			{
				value2 = num2;
			}
			else
			{
				dictionary.TryGetValue(text2, out value2);
			}
			chip.Text = ((text2.Length == 0) ? "Tout" : Categories.Nom(text2)) + "  " + value2;
			chip.Plein = text2 == categorie;
			chip.Couleur = C.JadeFonce;
			chip.Invalidate();
		}
		if (cat != null)
		{
			foreach (Projet projet2 in ProjetsTries())
			{
				if (JeuDe(projet2) != jeuId)
				{
					continue;
				}
				num++;
				Color couleur;
				int code;
				string badge = LibelleEtat(projet2, omsi, out couleur, out code);
				if ((categorie.Length <= 0 || !(projet2.Categorie != categorie)) && (filtre != 1 || code >= 1) && (filtre != 2 || code == 2) && (text.Length <= 0 || projet2.TexteRecherche.IndexOf(text) >= 0))
				{
					Tuile tuile = new Tuile();
					tuile.Titre = projet2.Nom + "  ·  " + projet2.Version;
					tuile.Sous = projet2.SousTitre;
					tuile.Glyphe = "\ue8f1";
					tuile.Etiquette = ((projet2.Ruban.Length > 0) ? Projet.NomRuban(projet2.Ruban) : Categories.Nom(projet2.Categorie));
					tuile.Img = (images.ContainsKey(projet2.Id) ? images[projet2.Id] : null);
					tuile.Badge = badge;
					tuile.BadgeCouleur = couleur;
					tuile.Prix = projet2.PrixTexte;
					tuile.PrixCouleur = ((projet2.Payant && !projet2.Debloque) ? C.Orange : C.Vert);
					Projet pp = projet2;
					tuile.Click += delegate
					{
						Choisir(pp);
						ShowVue(Vue.Projet);
					};
					pBiblio.Controls.Add(tuile);
					tuilesB.Add(tuile);
				}
			}
		}
		Dictionary<string, string> dictionary2 = JeuWebCourant();
		if (dictionary2 != null)
		{
			bChemins.Text = "▶  Jouer à " + NomJeu(jeuId);
			bChemins.Plein = true;
			bChemins.Couleur = C.Orange;
			bChemins.Size = new Size(Math.Max(Zx.P(170), Lang.MeasureText(bChemins.Text, bChemins.Font).Width + Zx.P(44)), Zx.P(40));
			txtVide = ((tuilesB.Count > 0) ? "" : ((num == 0) ? ("Aucun mod pour " + NomJeu(jeuId) + " pour l'instant : cliquez sur « Jouer » pour lancer le jeu.") : "Aucun mod ne correspond à ces critères."));
		}
		else
		{
			bChemins.Text = "⚙  Chemins du jeu";
			bChemins.Plein = false;
			bChemins.Couleur = C.Orange;
			bChemins.Size = new Size(Zx.P(170), Zx.P(34));
			txtVide = ((tuilesB.Count > 0) ? "" : ((num == 0) ? "Aucune carte pour ce jeu (catalogue introuvable ou vide)." : "Aucune carte ne correspond à ces critères."));
		}
		DisposerBiblio();
	}

	private void DisposerBiblio()
	{
		if (tRecherche == null)
		{
			return;
		}
		int width = pBiblio.ClientSize.Width;
		pBiblio.AutoScrollPosition = new Point(0, 0);
		int num = Math.Max(320, Math.Min(440, (int)((double)width * 0.34)));
		int num2 = Zx.P(132);
		rJeuB = new Rectangle(Zx.P(40), num2, num, Zx.P(440));
		int margeB = ((ongletB == 1) ? Zx.P(60) : 40);
		int num3 = ((ongletB == 1) ? margeB : (rJeuB.Right + Zx.P(28)));
		int num4 = width - margeB;
		bChangerJeu.Location = new Point(Zx.P(40), Zx.P(30));
		bAccueilB.Location = new Point(Zx.P(96), Zx.P(30));
		bool flag = JeuWebCourant() != null;
		bChemins.Size = new Size(num - Zx.P(40), flag ? Zx.P(40) : Zx.P(34));
		bJouerOmsi.Visible = !flag && jeuId == "omsi2";
		if (bJouerOmsi.Visible)
		{
			bJouerOmsi.SetBounds(rJeuB.X + Zx.P(20), rJeuB.Bottom - Zx.P(20) - Zx.P(40), num - Zx.P(40), Zx.P(40));
			bChemins.Location = new Point(rJeuB.X + Zx.P(20), bJouerOmsi.Top - Zx.P(10) - bChemins.Height);
		}
		else
		{
			bChemins.Location = new Point(rJeuB.X + Zx.P(20), rJeuB.Bottom - Zx.P(20) - Zx.P(40));
		}
		int num5 = 0;
		if (bPhotosJeu != null)
		{
			bPhotosJeu.Visible = ValJeu(jeuId, "galerie").Trim().Length > 0;
			if (bPhotosJeu.Visible)
			{
				num5++;
			}
		}
		if (bVideoJeu != null)
		{
			bVideoJeu.Visible = ValJeu(jeuId, "video").Length > 0;
			if (bVideoJeu.Visible)
			{
				num5++;
			}
		}
		if (num5 > 0)
		{
			int num6 = (num - Zx.P(40) - (num5 - 1) * Zx.P(10)) / num5;
			int num7 = rJeuB.X + Zx.P(20);
			int y = bChemins.Top - Zx.P(10) - Zx.P(34);
			Bouton[] array = new Bouton[2] { bPhotosJeu, bVideoJeu };
			foreach (Bouton bouton in array)
			{
				if (bouton != null && bouton.Visible)
				{
					bouton.SetBounds(num7, y, num6, Zx.P(34));
					num7 += num6 + Zx.P(10);
				}
			}
		}
		int num8 = Math.Max(Zx.P(150), Math.Min(Zx.P(280), num4 - num3 - Zx.P(410) - Zx.P(20)));
		int num9 = num3 + num8 + Zx.P(20);
		tRecherche.Width = num8;
		tRecherche.Location = new Point(num3, num2);
		fTous.Location = new Point(num9, num2 + Zx.P(2));
		fInst.Location = new Point(num9 + Zx.P(140), num2 + Zx.P(2));
		fMaj.Location = new Point(num9 + Zx.P(280), num2 + Zx.P(2));
		if (fMaj.Right > num4)
		{
			tRecherche.Width = num4 - num3;
			fTous.Location = new Point(num3, num2 + Zx.P(48));
			fInst.Location = new Point(num3 + Zx.P(140), num2 + Zx.P(48));
			fMaj.Location = new Point(num3 + Zx.P(280), num2 + Zx.P(48));
		}
		int num10 = num3;
		int num11 = fTous.Bottom + Zx.P(16);
		foreach (Bouton chip in chips)
		{
			int num12 = Math.Max(90, Lang.MeasureText(chip.Text, chip.Font).Width + 32);
			if (num10 + num12 > num4 && num10 > num3)
			{
				num10 = num3;
				num11 += Zx.P(40);
			}
			chip.SetBounds(num10, num11, num12, Zx.P(32));
			num10 += num12 + Zx.P(8);
		}
		int num13 = Zx.P(20);
		int num14 = Zx.P(280);
		int num15 = Math.Max(1, (width - 2 * margeB + num13) / (num14 + num13));
		int num16 = Math.Max(Zx.P(200), (width - 2 * margeB - (num15 - 1) * num13) / num15);
		int num17 = (int)((double)num16 * 0.9);
		int num18 = num11 + Zx.P(52);
		int num19 = 0;
		rJeuB = Rectangle.Empty;
		int num20 = Math.Max(1, (num4 - num3 + num13) / (num16 + num13));
		while (num19 < tuilesB.Count)
		{
			bool flag2 = num18 < rJeuB.Bottom;
			int num21 = (flag2 ? num3 : margeB);
			int num22 = (flag2 ? num20 : num15);
			int num23 = 0;
			while (num23 < num22 && num19 < tuilesB.Count)
			{
				tuilesB[num19].SetBounds(num21 + num23 * (num16 + num13), num18, num16, num17);
				num23++;
				num19++;
			}
			num18 += num17 + num13;
		}
		xVide = num3;
		yVide = num11 + 56;
		DisposerOngletsBiblio(fTous.Top - Zx.P(2), num4);
		pBiblio.Invalidate();
	}

	private string AppDiscord()
	{
		string text = Config.Lire("discord_app");
		if (string.IsNullOrEmpty(text) && cat != null && cat.Communaute.TryGetValue("discord_app", out var value))
		{
			text = value;
		}
		return text ?? "";
	}

	private string InviteDiscord()
	{
		if (cat == null || !cat.Communaute.TryGetValue("discord_invite", out var value))
		{
			return "";
		}
		return value;
	}

	private bool AppDiscordFournie()
	{
		if (cat != null && cat.Communaute.TryGetValue("discord_app", out var value))
		{
			return value.Length > 0;
		}
		return false;
	}

	private void MajPresence()
	{
		if (cacheProfil != null)
		{
			cacheProfil.Dispose();
			cacheProfil = null;
		}
		if (pAccueil != null)
		{
			pAccueil.Invalidate();
		}
		SyncStatut(force: true);
		try
		{
			if (ModeCapture)
			{
				return;
			}
			string text = AppDiscord();
			if (!Config.Oui("discord_presence", defaut: false) || !Regex.IsMatch(text, "^\\d{15,22}$"))
			{
				DiscordPresence.Arreter();
				return;
			}
			DiscordPresence.Demarrer(text, InviteDiscord());
			Compte actif = Comptes.Actif;
			string text2 = ((actif != null && actif.Statut.Length > 0) ? actif.Statut : "TPS Groupe");
			if (jeuEnCours != null)
			{
				DiscordPresence.Definir("Joue à " + jeuEnCours.Nom, text2, debutJeu);
			}
			else
			{
				DiscordPresence.Definir("Dans le lanceur TPS Groupe", (vue == Vue.Projet && sel != null) ? ("Regarde " + sel.Nom) : text2, null);
			}
		}
		catch (Exception e)
		{
			Journal.Ecrire("presence", e);
		}
	}

	private static string Hash8(string t)
	{
		using SHA256 sHA = SHA256.Create();
		return BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(t)), 0, 4).Replace("-", "").ToLowerInvariant();
	}

	private static string FichierGalerie(string pid, int i, string url)
	{
		return Path.Combine(Config.Cache, "image_" + pid + "_g" + i + "_" + Hash8(url) + ".jpg");
	}

	private static Image ChargerPhoto(string pid, int i, string url)
	{
		if (!Web.UrlSure(url))
		{
			throw new Exception("adresse de photo refusée");
		}
		string path = FichierGalerie(pid, i, url);
		if (!File.Exists(path))
		{
			byte[] array;
			if (File.Exists(url))
			{
				array = File.ReadAllBytes(url);
			}
			else
			{
				using WebClient webClient = Web.Client();
				array = webClient.DownloadData(url);
			}
			if (array.Length > 26214400)
			{
				throw new Exception("photo trop lourde");
			}
			File.WriteAllBytes(path, array);
		}
		using MemoryStream stream = new MemoryStream(File.ReadAllBytes(path));
		using Image brut = Image.FromStream(stream);
		return Comptes.ReduireBanniere(brut);
	}

	private void ChargerGalerie(Projet p)
	{
		if (p == null || p.Galerie.Count == 0 || galImages.ContainsKey(p.Id) || galEnCours.Contains(p.Id))
		{
			return;
		}
		galEnCours.Add(p.Id);
		List<string> urls = new List<string>(p.Galerie);
		string pid = p.Id;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				List<string> list = new List<string>();
				for (int i = 0; i < urls.Count; i++)
				{
					list.Add(Path.GetFileName(FichierGalerie(pid, i, urls[i])).ToLowerInvariant());
				}
				string[] files = Directory.GetFiles(Config.Cache, "image_" + pid + "_g*.jpg");
				foreach (string path in files)
				{
					if (Regex.IsMatch(Path.GetFileName(path), "^image_" + Regex.Escape(pid) + "_g\\d+_[0-9a-f]{8}\\.jpg$") && !list.Contains(Path.GetFileName(path).ToLowerInvariant()))
					{
						try
						{
							File.Delete(path);
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
			List<Image> list2 = new List<Image>();
			for (int k = 0; k < urls.Count; k++)
			{
				Image item = null;
				try
				{
					item = ChargerPhoto(pid, k, urls[k]);
				}
				catch (Exception e)
				{
					Journal.Ecrire("galerie", e);
				}
				list2.Add(item);
				List<Image> copie = new List<Image>(list2);
				Sur(delegate
				{
					galImages[pid] = copie;
					if (sel != null && sel.Id == pid)
					{
						RemplirGalerie();
						Disposer();
					}
				});
			}
			Sur(delegate
			{
				galEnCours.Remove(pid);
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void RemplirGalerie()
	{
		foreach (Vignette vignette2 in vignettes)
		{
			galP.Controls.Remove(vignette2);
			vignette2.Dispose();
		}
		vignettes.Clear();
		if (sel == null)
		{
			return;
		}
		galImages.TryGetValue(sel.Id, out var value);
		for (int j = 0; j < sel.Galerie.Count; j++)
		{
			Vignette vignette = new Vignette();
			vignette.Img = ((value != null && j < value.Count) ? value[j] : null);
			int i = j;
			vignette.Click += delegate
			{
				OuvrirVisionneuse(i);
			};
			galP.Controls.Add(vignette);
			vignettes.Add(vignette);
		}
		DisposerGalerie();
	}

	private void DisposerGalerie()
	{
		if (galP != null && vignettes.Count != 0)
		{
			galP.AutoScrollPosition = new Point(0, 0);
			int num = Math.Max(Zx.P(300), galP.ClientSize.Width - Zx.P(40));
			int num2 = Zx.P(14);
			int num3 = Math.Max(1, (num + num2) / (230 + num2));
			int num4 = (num - (num3 - 1) * num2) / num3;
			int num5 = num4 * 9 / 16;
			for (int i = 0; i < vignettes.Count; i++)
			{
				vignettes[i].SetBounds(Zx.P(20) + i % num3 * (num4 + num2), Zx.P(20) + i / num3 * (num5 + num2), num4, num5);
			}
		}
	}

	private void OuvrirVisionneuse(int k)
	{
		if (sel == null || !galImages.TryGetValue(sel.Id, out var value))
		{
			return;
		}
		List<Image> list = new List<Image>();
		int depart = 0;
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i] != null)
			{
				if (i == k)
				{
					depart = list.Count;
				}
				list.Add(value[i]);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		using FenVisionneuse f = new FenVisionneuse(list, depart);
		f.Afficher(this);
	}

	private void BasculerPleinEcran()
	{
		if (!plein)
		{
			avantPlein = base.WindowState;
			base.WindowState = FormWindowState.Normal;
			base.FormBorderStyle = FormBorderStyle.None;
			base.WindowState = FormWindowState.Maximized;
			plein = true;
		}
		else
		{
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.WindowState = ((avantPlein != FormWindowState.Minimized) ? avantPlein : FormWindowState.Normal);
			plein = false;
		}
	}

	private void Recharger(bool manuel)
	{
		if (occupe)
		{
			return;
		}
		lAlerte.Visible = false;
		bVerif.Enabled = false;
		bVerif.Text = "…";
		Thread thread = new Thread((ThreadStart)delegate
		{
			string err;
			Catalogue c = Catalogue.Charger(out err);
			Dictionary<string, Image> ims = new Dictionary<string, Image>();
			foreach (Projet projet2 in c.Projets)
			{
				Image image = ImageProjet(projet2);
				if (image != null)
				{
					ims[projet2.Id] = image;
				}
			}
			Sur(delegate
			{
				cat = c;
				erreurCat = err;
				Themes.CoupeAdmin = Opt("themes") == "off";
				Social.Configurer(c.Communaute.TryGetValue("serveur", out var value) ? value : "");
				List<Image> list = new List<Image>();
				foreach (List<Image> value4 in galImages.Values)
				{
					foreach (Image item in value4)
					{
						if (item != null)
						{
							list.Add(item);
						}
					}
				}
				galImages.Clear();
				foreach (KeyValuePair<string, Image> item2 in ims)
				{
					if (images.TryGetValue(item2.Key, out var value2) && value2 != banniere)
					{
						list.Add(value2);
					}
					images[item2.Key] = item2.Value;
				}
				bVerif.Enabled = true;
				bVerif.Text = "⟳";
				LienVip = (c.Communaute.TryGetValue("vip_lien", out var value3) ? value3 : "");
				InviteVip = (c.Communaute.TryGetValue("discord_invite", out value3) ? value3 : "");
				ProjetsConnus = c.Projets;
				RemplirListe();
				RafraichirVues();
				MajPresence();
				MajVip();
				try
				{
					Notifs.Verifier(cat, Omsi.DetecteOmsi(), (cat.LanceurUrl != null && PlusRecent(cat.LanceurVersion, Config.VERSION)) ? cat.LanceurVersion : null);
				}
				catch
				{
				}
				foreach (Image item3 in list)
				{
					try
					{
						item3.Dispose();
					}
					catch
					{
					}
				}
				if (premiereCharge && ActionAuto == null && !ModeCapture)
				{
					premiereCharge = false;
					string text = Config.Lire("page_demarrage");
					if (text == "biblio")
					{
						ShowVue(Vue.Biblio);
					}
					else if (text == "recent")
					{
						Projet projet = ProjetRecent();
						if (projet != null)
						{
							Choisir(projet);
							ShowVue(Vue.Projet);
						}
					}
				}
				if (err != null)
				{
					lAlerte.Text = "Hors ligne : impossible de joindre le catalogue TPS Groupe (" + err + ").";
					lAlerte.Visible = true;
				}
				bool flag = VerifierLanceur(manuel || (Opt("version_min").Length > 0 && PlusRecent(Opt("version_min"), Config.VERSION) && !ModeCapture));
				if (manuel && !flag)
				{
					Toast((err != null) ? "Impossible de joindre le serveur de mises à jour." : "Le lanceur est à jour (v" + Config.VERSION + ").", (err != null) ? C.Rouge : C.Vert, 5000);
				}
				if (ActionAuto != null)
				{
					string actionAuto = ActionAuto;
					ActionAuto = null;
					{
						foreach (Projet projet3 in cat.Projets)
						{
							if (projet3.Id == actionAuto)
							{
								Choisir(projet3);
								ShowVue(Vue.Projet);
								Agir();
							}
						}
						return;
					}
				}
				if (!flag && !occupe && Config.Oui("maj_auto_projets", defaut: false))
				{
					MajAutoProjets();
				}
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private Image ImageProjet(Projet p)
	{
		try
		{
			string text;
			using (SHA256 sHA = SHA256.Create())
			{
				byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(p.Image));
				text = BitConverter.ToString(array, 0, 4).Replace("-", "").ToLowerInvariant();
			}
			string text2 = Path.Combine(Config.Cache, "image_" + p.Id + "_" + text + ".jpg");
			if (p.Image.Length > 0 && (!File.Exists(text2) || (DateTime.Now - File.GetLastWriteTime(text2)).TotalDays > 3.0))
			{
				string[] files = Directory.GetFiles(Config.Cache, "image_" + p.Id + "*.jpg");
				foreach (string text3 in files)
				{
					if (!string.Equals(text3, text2, StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(Path.GetFileName(text3), "^image_" + Regex.Escape(p.Id) + "(_[0-9a-f]{8})?\\.jpg$"))
					{
						try
						{
							File.Delete(text3);
						}
						catch
						{
						}
					}
				}
				if (!Web.UrlSure(p.Image))
				{
					throw new Exception("adresse d'image refusée");
				}
				if (File.Exists(p.Image))
				{
					File.Copy(p.Image, text2, overwrite: true);
				}
				else
				{
					using WebClient webClient = Web.Client();
					webClient.DownloadFile(p.Image, text2);
				}
			}
			if (File.Exists(text2))
			{
				using (FileStream s = File.OpenRead(text2))
				{
					return Image.FromStream(new MemoryStream(Lire(s)));
				}
			}
		}
		catch
		{
		}
		if (!(p.Id == "parisview"))
		{
			return null;
		}
		return banniere;
	}

	private static byte[] Lire(Stream s)
	{
		MemoryStream memoryStream = new MemoryStream();
		s.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private void RemplirListe()
	{
		liste.Controls.Clear();
		entrees.Clear();
		int num = 0;
		foreach (Projet projet2 in cat.Projets)
		{
			if (cote.Parent != null)
			{
				Entree entree = new Entree();
				entree.P = projet2;
				entree.Width = Zx.P(260);
				entree.Location = new Point(0, num);
				num += Zx.P(66);
				entree.Vignette = (images.ContainsKey(projet2.Id) ? images[projet2.Id] : null);
				Projet pp = projet2;
				entree.Click += delegate
				{
					Choisir(pp);
					ShowVue(Vue.Projet);
				};
				liste.Controls.Add(entree);
				entrees.Add(entree);
				continue;
			}
			break;
		}
		if (cat.Projets.Count == 0)
		{
			Label label = L(new Font("Segoe UI", Zx.P(9.5f)), C.Gris, auto: false);
			label.BackColor = C.Cote;
			label.Text = "Aucun projet (catalogue introuvable).";
			label.Location = new Point(Zx.P(20), Zx.P(4));
			label.Size = new Size(Zx.P(220), Zx.P(60));
			liste.Controls.Add(label);
		}
		Projet projet = null;
		if (sel != null)
		{
			foreach (Projet projet3 in cat.Projets)
			{
				if (projet3.Id == sel.Id)
				{
					projet = projet3;
				}
			}
		}
		Choisir(projet ?? ((cat.Projets.Count > 0) ? cat.Projets[0] : null));
	}

	private void Choisir(Projet p)
	{
		sel = p;
		foreach (Entree entree in entrees)
		{
			entree.Choisi = entree.P == p;
			entree.Invalidate();
		}
		if (p == null)
		{
			lTitre.Text = "TPS Groupe";
			lSous.Text = "";
			bAction.Visible = false;
			return;
		}
		if (jeuId != JeuDe(p))
		{
			jeuId = JeuDe(p);
			Config.Ecrire("jeu", jeuId);
		}
		if (!occupe)
		{
			lAlerte.Visible = false;
		}
		lTitre.Text = p.Nom;
		lSous.Text = p.SousTitre;
		lDesc.Text = p.Description.Replace("\\n", "\n");
		bOngNotes.Text = "Notes · " + p.Version;
		bOngNotes.Invalidate();
		vIdx = 0;
		vueSrc = null;
		bOngGal.Visible = false;
		ChoisirOnglet(0);
		ChargerGalerie(p);
		RemplirGalerie();
		lNotes.Text = p.Notes.Replace("\\n", "\n");
		bAction.Visible = true;
		Disposer();
		MajEtat();
		page.Invalidate();
	}

	private void MajEtat()
	{
		if (sel == null)
		{
			return;
		}
		string text = Omsi.DetecteOmsi();
		string[] array = Omsi.Installee(sel, text);
		bool flag = Omsi.JeuLance();
		if (text == null)
		{
			etat = E.SansJeu;
			lEtat.Text = "OMSI 2 est introuvable sur ce PC.\nCliquez sur « Dossiers du jeu » (en bas à gauche) pour l'indiquer.";
		}
		else if (array == null)
		{
			etat = E.Rien;
			lEtat.Text = "Non installé · " + sel.Version + ((sel.Taille > 0) ? (" · " + sel.Taille / 1048576 + " Mo à télécharger") : "");
		}
		else if (Omsi.Perimee(sel, array))
		{
			etat = E.MAJ;
			lEtat.Text = ((array[0].Length > 0) ? ("Mise à jour disponible : " + sel.Version + "\nInstallée : " + array[0]) : ("Déjà présent dans OMSI 2 (installé sans le lanceur)\nCliquez pour installer la " + sel.Version + " du lanceur"));
		}
		else
		{
			etat = E.AJour;
			lEtat.Text = "✔  " + ((array[0].Length > 0) ? array[0] : sel.Version) + " installée et à jour" + ((array[1].Length > 0) ? ("\ndu " + array[1]) : "");
		}
		if (flag && etat != 0 && etat != E.SansJeu)
		{
			etat = E.Lance;
		}
		bAction.Enabled = !occupe && etat != E.Lance && etat != E.SansJeu && (etat != 0 || sel.APaquet);
		bAction.Text = ((etat == E.Rien) ? "INSTALLER" : ((etat == E.MAJ) ? "METTRE À JOUR" : ((etat == E.Lance) ? "EN COURS" : ((etat == E.SansJeu) ? "INSTALLER" : "JOUER"))));
		bAction.Couleur = ((etat == E.MAJ) ? C.Bleu : C.Orange);
		bAction.Invalidate();
		bool flag2 = modeAchat;
		modeAchat = false;
		if (array == null && !sel.Installable)
		{
			Indispo(sel, out var bouton, out var message);
			bAction.Enabled = false;
			bAction.Text = bouton;
			bAction.Invalidate();
			lEtat.Text = message;
		}
		else if (array == null && sel.Payant && !sel.Debloque)
		{
			modeAchat = true;
			bAction.Text = "ACHETER · " + sel.PrixTexte;
			bAction.Couleur = C.Orange;
			bAction.Enabled = !occupe && LienOk(sel.LienAchat);
			bAction.Invalidate();
			lEtat.Text = (LienOk(sel.LienAchat) ? "Contenu payant. Achetez-le, puis entrez votre code d'accès pour l'installer." : "Contenu payant : la page d'achat n'est pas encore disponible.");
		}
		bCode.Visible = modeAchat;
		if (modeAchat != flag2)
		{
			Disposer();
		}
		else
		{
			page.Invalidate();
		}
		bool flag3 = (etat == E.AJour || etat == E.MAJ || etat == E.Lance) && File.Exists(Path.Combine(text ?? "", sel.Liste));
		if (occupe)
		{
			bDesinst.Text = "Annuler";
			bDesinst.Visible = dlEnCours != null;
			bDesinst.Enabled = dlEnCours != null && !dlAnnule;
		}
		else
		{
			bDesinst.Text = "Désinstaller";
			bDesinst.Visible = flag3;
			bDesinst.Enabled = flag3 && etat != E.Lance;
		}
		bDesinst.Invalidate();
		bDossier.Enabled = text != null;
		foreach (Entree entree in entrees)
		{
			entree.Etat = LibelleEtat(entree.P, text, out var couleur, out var _);
			entree.Pastille = couleur;
			entree.Invalidate();
		}
	}

	private void Agir()
	{
		if (sel != null && !occupe)
		{
			MajEtat();
			if (modeAchat)
			{
				OuvrirAchat(sel);
			}
			else if (etat == E.AJour)
			{
				Jouer();
			}
			else if (etat == E.Rien || etat == E.MAJ)
			{
				Installer(sel);
			}
		}
	}

	private void Jouer()
	{
		string omsi = Omsi.DetecteOmsi();
		string text = Omsi.DetecteOpen(omsi);
		if (Omsi.Desactive(sel, omsi))
		{
			if (MessageBox.Show(this, "« " + sel.Nom + " » est désactivé (Mon profil › Mes mods).\n\nLe réactiver maintenant ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			if (Omsi.JeuLance())
			{
				MessageBox.Show(this, "Fermez OMSI 2 / openOMSI avant de réactiver un mod.", Text);
				return;
			}
			try
			{
				Omsi.Basculer(sel, omsi, activer: true);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Réactivation impossible : " + ex.Message, Text);
				return;
			}
		}
		if (text == null)
		{
			Dossiers("openOMSI est introuvable : indiquez son dossier (celui qui contient openomsi.exe).");
			return;
		}
		string text2 = Path.Combine(text, "openomsi-launcher.exe");
		if (!File.Exists(text2))
		{
			text2 = Path.Combine(text, "openomsi.exe");
		}
		string text3 = "";
		try
		{
			text3 = Preparer(sel);
		}
		catch (Exception ex2)
		{
			text3 = " (réglages non appliqués : " + ex2.Message + ")";
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo(text2);
		processStartInfo.WorkingDirectory = text;
		processStartInfo.UseShellExecute = true;
		try
		{
			Process.Start(processStartInfo);
			bAction.Text = "LANCEMENT…";
			bAction.Enabled = false;
			try
			{
				Comptes.Noter(sel.Id);
				Progression.Lancement(Comptes.Actif, sel.Id, JeuDe(sel));
			}
			catch
			{
			}
			jeuEnCours = sel;
			debutJeu = DateTime.Now;
			MajPresence();
			if (Config.Oui("reduire", defaut: false))
			{
				base.WindowState = FormWindowState.Minimized;
			}
			lAlerte.ForeColor = C.Vert;
			lAlerte.Visible = true;
			lAlerte.Text = "▶  openOMSI s'ouvre sur " + sel.Nom + text3 + ". " + ((sel.Consigne.Length > 0) ? sel.Consigne : "Choisissez le véhicule et la ligne, puis lancez.");
		}
		catch (Exception ex3)
		{
			MessageBox.Show(this, "Impossible de lancer openOMSI :\n" + ex3.Message, Text);
		}
	}

	private static string NomMeteo(string rel)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(rel);
		try
		{
			string text = Omsi.DetecteOmsi();
			if (text == null)
			{
				return fileNameWithoutExtension;
			}
			string path = Path.Combine(text, rel.Replace('/', '\\'));
			if (!File.Exists(path))
			{
				return fileNameWithoutExtension;
			}
			string[] array = File.ReadAllLines(path, Encoding.Default);
			for (int i = 0; i + 1 < array.Length; i++)
			{
				if (array[i].Trim().Equals("[name]", StringComparison.OrdinalIgnoreCase) && array[i + 1].Trim().Length > 0)
				{
					return array[i + 1].Trim();
				}
			}
		}
		catch
		{
		}
		return fileNameWithoutExtension;
	}

	private string Preparer(Projet p)
	{
		if (p.Carte.Length == 0)
		{
			return "";
		}
		string text = Environment.GetEnvironmentVariable("TPS_DUTY");
		if (string.IsNullOrEmpty(text))
		{
			text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openomsi", "launcher-duty.json");
		}
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		Dictionary<string, object> dictionary = (File.Exists(text) ? javaScriptSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(text, Encoding.UTF8)) : null);
		if (dictionary == null)
		{
			dictionary = new Dictionary<string, object>();
		}
		if (File.Exists(text))
		{
			File.Copy(text, Path.Combine(Config.Cache, "launcher-duty.avant.json"), overwrite: true);
		}
		object value;
		string a = ((dictionary.TryGetValue("map", out value) && value != null) ? value.ToString() : "");
		dictionary["map"] = p.Carte;
		if (!string.Equals(a, p.Carte, StringComparison.OrdinalIgnoreCase))
		{
			if (p.BusDefaut.Length > 0)
			{
				dictionary["bus"] = p.BusDefaut;
			}
			if (p.HofDefaut.Length > 0)
			{
				dictionary["hof"] = p.HofDefaut;
				dictionary["hof_manual"] = false;
			}
			dictionary["line"] = "";
			dictionary["tour"] = null;
			dictionary["start_trip"] = null;
			dictionary["entry"] = 0;
			dictionary["paint"] = "";
			dictionary["number"] = "";
			dictionary["plate"] = "";
		}
		string result = "";
		if (p.Meteo.Length > 0)
		{
			dictionary["weather"] = p.Meteo;
			result = (p.Meteo.StartsWith("metar:") ? ", météo réelle" : ((p.Meteo.IndexOf("CAVOK", StringComparison.OrdinalIgnoreCase) >= 0) ? ", ciel dégagé" : (", météo « " + NomMeteo(p.Meteo) + " »")));
		}
		if (!dictionary.ContainsKey("season"))
		{
			dictionary["season"] = "auto";
		}
		Directory.CreateDirectory(Path.GetDirectoryName(text));
		Disque.EcrireTexte(text, javaScriptSerializer.Serialize(dictionary));
		return result;
	}

	private void Installer(Projet p)
	{
		if (Maintenance && !Comptes.EstAdmin)
		{
			MessageBox.Show(this, "Maintenance en cours : les installations et mises à jour sont suspendues quelques instants." + ((Opt("maintenance_msg").Length > 0) ? ("\n\n" + Opt("maintenance_msg")) : ""), Text);
			return;
		}
		string omsi = Omsi.DetecteOmsi();
		if (p.Payant && !p.Debloque && Omsi.Installee(p, omsi) == null)
		{
			MessageBox.Show(this, "Ce contenu est payant : achetez-le, puis entrez votre code d'accès.", Text);
			return;
		}
		if (omsi == null)
		{
			Dossiers("Pour installer " + p.Nom + ", indiquez d'abord le dossier d'OMSI 2.");
			omsi = Omsi.DetecteOmsi();
			if (omsi == null)
			{
				return;
			}
		}
		string open = Omsi.DetecteOpen(omsi);
		if (open == null)
		{
			Dossiers("Indiquez aussi le dossier d'openOMSI : les plugins de cabine (écran, prise de service) y sont installés.");
			open = Omsi.DetecteOpen(omsi);
		}
		if (open == null && MessageBox.Show(this, "openOMSI n'a pas été trouvé : les plugins (écran de cabine, prise de service) ne seront pas installés.\n\nContinuer quand même ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
		{
			return;
		}
		if (Omsi.JeuLance())
		{
			MessageBox.Show(this, "Fermez OMSI 2 / openOMSI avant d'installer.", Text);
			return;
		}
		bool flag = p.Parties.Count > 0;
		foreach (string[] party in p.Parties)
		{
			if (!Web.UrlSure(party[0]))
			{
				flag = false;
			}
		}
		bool num;
		if (p.Parties.Count <= 0)
		{
			if (!Web.UrlSure(p.Zip))
			{
				goto IL_0220;
			}
			if (p.Sha256.Length != 0)
			{
				goto IL_0263;
			}
			num = Config.Catalogue == "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/catalogue.json";
		}
		else
		{
			num = !flag;
		}
		if (num)
		{
			goto IL_0220;
		}
		goto IL_0263;
		IL_0220:
		lAlerte.Text = "Installation refusée : l'adresse ou l'empreinte de " + p.Nom + " n'est pas valide dans le catalogue.";
		lAlerte.ForeColor = C.Rouge;
		lAlerte.Visible = true;
		return;
		IL_0263:
		try
		{
			if (p.Taille > 0)
			{
				long availableFreeSpace = new DriveInfo(Path.GetPathRoot(Config.Cache)).AvailableFreeSpace;
				long availableFreeSpace2 = new DriveInfo(Path.GetPathRoot(omsi)).AvailableFreeSpace;
				if (availableFreeSpace < p.Taille + 209715200 || availableFreeSpace2 < p.Taille * 2)
				{
					lAlerte.Text = "Pas assez de place disque pour " + p.Nom + " : il faut environ " + (p.Taille * 2 / 1073741824 + 1) + " Go libres sur le disque d'OMSI 2.";
					lAlerte.ForeColor = C.Rouge;
					lAlerte.Visible = true;
					return;
				}
			}
		}
		catch
		{
		}
		occupe = true;
		dlAnnule = false;
		MajEtat();
		bAction.Enabled = false;
		bAction.Text = "TÉLÉCHARGEMENT…";
		barre.Visible = true;
		lProg.Visible = true;
		lAlerte.Visible = false;
		barre.Valeur = 0.0;
		barre.Invalidate();
		lProg.Text = "Connexion au serveur de téléchargement…";
		List<string[]> morceaux = new List<string[]>();
		if (p.Parties.Count > 0)
		{
			for (int i = 0; i < p.Parties.Count; i++)
			{
				morceaux.Add(new string[4]
				{
					p.Parties[i][0],
					p.Parties[i][1],
					Path.Combine(Config.Cache, p.Id + "_partie" + (i + 1) + ".zip"),
					Web.UrlSure(p.Parties[i][3]) ? p.Parties[i][3] : ""
				});
			}
		}
		else
		{
			morceaux.Add(new string[4]
			{
				p.Zip,
				p.Sha256,
				Path.Combine(Config.Cache, p.Id + ".zip"),
				""
			});
		}
		int morceau = 0;
		string zip = morceaux[0][2];
		Action<string> fin = delegate(string err)
		{
			occupe = false;
			barre.Visible = false;
			lProg.Visible = false;
			dlEnCours = null;
			if (dlMinuteur != null)
			{
				dlMinuteur.Stop();
				dlMinuteur.Dispose();
				dlMinuteur = null;
			}
			if (err != null && dlAnnule)
			{
				lAlerte.Text = "Téléchargement de " + p.Nom + " annulé.";
				lAlerte.ForeColor = C.Gris;
				lAlerte.Visible = true;
				dlAnnule = false;
			}
			else if (err != null)
			{
				lAlerte.Text = err;
				lAlerte.ForeColor = C.Rouge;
				lAlerte.Visible = true;
			}
			else
			{
				try
				{
					Progression.Installation(Comptes.Actif, p.Id, JeuDe(p), p.Categorie);
				}
				catch
				{
				}
				lAlerte.Text = "✔  " + p.Nom + " " + p.Version + " est installé." + ((Omsi.Nettoyes > 0) ? (" " + Omsi.Nettoyes + " ancien(s) fichier(s) retiré(s).") : "");
				lAlerte.ForeColor = C.Vert;
				lAlerte.Visible = true;
			}
			MajEtat();
		};
		Action extraire = delegate
		{
			dlEnCours = null;
			if (dlMinuteur != null)
			{
				dlMinuteur.Stop();
				dlMinuteur.Dispose();
				dlMinuteur = null;
			}
			MajEtat();
			bAction.Text = "INSTALLATION…";
			lProg.Text = "Vérification du fichier…";
			Thread thread3 = new Thread((ThreadStart)delegate
			{
				string err2 = null;
				try
				{
					foreach (string[] item in morceaux)
					{
						if (item[1].Length > 0 && !string.Equals(Web.Sha256(item[2]), item[1], StringComparison.OrdinalIgnoreCase))
						{
							foreach (string[] item2 in morceaux)
							{
								try
								{
									File.Delete(item2[2]);
								}
								catch
								{
								}
							}
							throw new Exception("un fichier téléchargé est abîmé, réessayez.");
						}
					}
					DateTime der = DateTime.MinValue;
					List<string> list = new List<string>();
					foreach (string[] item3 in morceaux)
					{
						list.Add(item3[2]);
					}
					Omsi.Installer(p, list.ToArray(), omsi, open, delegate(double v, string f)
					{
						if (!((DateTime.Now - der).TotalMilliseconds < 80.0) || !(v < 1.0))
						{
							der = DateTime.Now;
							Sur(delegate
							{
								barre.Valeur = v;
								barre.Invalidate();
								lProg.Text = "Installation " + (int)(v * 100.0) + " %  ·  " + Path.GetFileName(f);
							});
						}
					});
					foreach (string[] item4 in morceaux)
					{
						try
						{
							File.Delete(item4[2]);
						}
						catch
						{
						}
					}
				}
				catch (UnauthorizedAccessException)
				{
					err2 = "ADMIN";
				}
				catch (Exception ex2)
				{
					err2 = "L'installation a échoué : " + ex2.Message;
				}
				Sur(delegate
				{
					if (err2 == "ADMIN")
					{
						bAction.Text = "AUTORISATION…";
						barre.Valeur = 1.0;
						barre.Invalidate();
						lProg.Text = "OMSI 2 est dans un dossier protégé : validez la fenêtre Windows « Voulez-vous autoriser… » pour terminer l'installation.";
						Thread thread4 = new Thread((ThreadStart)delegate
						{
							string text = Path.Combine(Config.Cache, p.Id + "_admin.txt");
							try
							{
								File.Delete(text);
							}
							catch
							{
							}
							List<string> list2 = new List<string>();
							List<string> list3 = new List<string>();
							foreach (string[] item5 in morceaux)
							{
								list2.Add(item5[2]);
								list3.Add(item5[1]);
							}
							int num5 = Admin("/installer-admin " + Q(string.Join("|", list2.ToArray())) + " " + Q(omsi.TrimEnd('\\')) + " " + Q((open == null) ? "-" : open.TrimEnd('\\')) + " " + Q(p.Liste) + " " + Q(text) + " " + Q(string.Join("|", list3.ToArray())));
							string msg = null;
							switch (num5)
							{
							case -1:
								msg = "Installation annulée : l'autorisation administrateur a été refusée. Elle est nécessaire car OMSI 2 est dans un dossier protégé de Windows.";
								break;
							default:
								try
								{
									msg = File.ReadAllText(text, Encoding.UTF8);
								}
								catch
								{
								}
								if (string.IsNullOrEmpty(msg))
								{
									msg = "L'installation a échoué.";
								}
								break;
							case 0:
								foreach (string[] item6 in morceaux)
								{
									try
									{
										File.Delete(item6[2]);
									}
									catch
									{
									}
								}
								Omsi.Nettoyes = 0;
								break;
							}
							Sur(delegate
							{
								fin(msg);
							});
						});
						thread4.IsBackground = true;
						thread4.Start();
					}
					else
					{
						fin(err2);
					}
				});
			});
			thread3.IsBackground = true;
			thread3.Start();
		};
		if (p.Parties.Count == 0 && File.Exists(p.Zip))
		{
			File.Copy(p.Zip, zip, overwrite: true);
			extraire();
			return;
		}
		if (p.Parties.Count > 0 && File.Exists(p.Parties[0][0]))
		{
			foreach (string[] item7 in morceaux)
			{
				File.Copy(item7[0], item7[2], overwrite: true);
			}
			extraire();
			return;
		}
		Action<string, bool> lancer = null;
		Action suivant = null;
		lancer = delegate(string adresse, bool secours)
		{
			zip = morceaux[morceau][2];
			if (dlAnnule)
			{
				fin("annulé");
			}
			else
			{
				WebClient w = (secours ? Web.ClientSecours() : Web.Client());
				dlEnCours = w;
				MajEtat();
				DateTime t0 = DateTime.Now;
				DateTime derT = DateTime.MinValue;
				bool recu = false;
				if (dlMinuteur != null)
				{
					dlMinuteur.Dispose();
				}
				dlMinuteur = new System.Windows.Forms.Timer();
				dlMinuteur.Interval = 500;
				dlMinuteur.Tick += delegate
				{
					if (!recu && occupe)
					{
						int num4 = (int)(DateTime.Now - t0).TotalSeconds;
						lProg.Text = "Connexion au serveur de téléchargement… " + num4 + " s";
						barre.Valeur = 0.02 + 0.03 * Math.Abs(Math.Sin((double)num4 * 0.8));
						barre.Invalidate();
					}
				};
				dlMinuteur.Start();
				w.DownloadProgressChanged += delegate(object o, DownloadProgressChangedEventArgs e)
				{
					if (!((DateTime.Now - derT).TotalMilliseconds < 100.0))
					{
						derT = DateTime.Now;
						recu = true;
						long num2 = ((e.TotalBytesToReceive > 0) ? e.TotalBytesToReceive : p.Taille);
						barre.Valeur = ((num2 > 0) ? ((double)e.BytesReceived / (double)num2) : 0.0);
						barre.Invalidate();
						double num3 = Math.Max(0.5, (DateTime.Now - t0).TotalSeconds);
						lProg.Text = ((morceaux.Count > 1) ? ("Partie " + (morceau + 1) + "/" + morceaux.Count + "  ·  ") : "") + "Téléchargement " + e.BytesReceived / 1048576 + " / " + num2 / 1048576 + " Mo  ·  " + ((double)e.BytesReceived / 1048576.0 / num3).ToString("0.0") + " Mo/s";
					}
				};
				w.DownloadFileCompleted += delegate(object o, AsyncCompletedEventArgs e)
				{
					w.Dispose();
					if (e.Error != null && !e.Cancelled && !dlAnnule)
					{
						if (secours)
						{
							lProg.Text = "Nouvel essai par le lien direct…";
							lancer(morceaux[morceau][0], arg2: false);
							dlDirectEssaye = true;
							return;
						}
						if (!dlSecoursEssaye)
						{
							lProg.Text = "Serveur de téléchargement lent : nouvel essai par un autre chemin…";
							string adr2 = morceaux[morceau][0];
							Thread thread2 = new Thread((ThreadStart)delegate
							{
								string api2 = null;
								try
								{
									api2 = Web.Secours(adr2);
								}
								catch
								{
								}
								Sur(delegate
								{
									if (api2 != null)
									{
										dlSecoursEssaye = true;
										lancer(api2, arg2: true);
									}
									else
									{
										try
										{
											File.Delete(zip);
										}
										catch
										{
										}
										fin("Le téléchargement a échoué : " + e.Error.Message + " Vérifiez votre connexion puis réessayez.");
									}
								});
							});
							thread2.IsBackground = true;
							thread2.Start();
							return;
						}
					}
					if (e.Error != null || e.Cancelled || dlAnnule)
					{
						foreach (string[] item8 in morceaux)
						{
							try
							{
								File.Delete(item8[2]);
							}
							catch
							{
							}
						}
						fin((dlAnnule || e.Cancelled) ? "annulé" : ("Le téléchargement a échoué : " + e.Error.Message + " Vérifiez votre connexion puis réessayez."));
					}
					else if (morceau + 1 < morceaux.Count)
					{
						morceau++;
						suivant();
					}
					else
					{
						extraire();
					}
				};
				w.DownloadFileAsync(new Uri(adresse), zip);
			}
		};
		suivant = delegate
		{
			dlSecoursEssaye = false;
			dlDirectEssaye = false;
			string adr = morceaux[morceau][0];
			if (morceaux[morceau][3].Length > 0)
			{
				lancer(morceaux[morceau][3], arg2: false);
			}
			else
			{
				Thread thread = new Thread((ThreadStart)delegate
				{
					string api = null;
					try
					{
						api = Web.Secours(adr);
					}
					catch
					{
					}
					Sur(delegate
					{
						if (dlAnnule)
						{
							fin("annulé");
						}
						else if (api != null)
						{
							dlSecoursEssaye = true;
							lancer(api, arg2: true);
						}
						else
						{
							lancer(adr, arg2: false);
						}
					});
				});
				thread.IsBackground = true;
				thread.Start();
			}
		};
		suivant();
	}

	private static string Q(string s)
	{
		return "\"" + s + "\"";
	}

	private static int Admin(string args)
	{
		try
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo(Application.ExecutablePath, args);
			processStartInfo.Verb = "runas";
			processStartInfo.UseShellExecute = true;
			using Process process = Process.Start(processStartInfo);
			process.WaitForExit();
			return process.ExitCode;
		}
		catch (Win32Exception)
		{
			return -1;
		}
	}

	private void Desinstaller()
	{
		string text = Omsi.DetecteOmsi();
		if (sel == null || text == null)
		{
			return;
		}
		if (Omsi.JeuLance())
		{
			MessageBox.Show(this, "Fermez OMSI 2 / openOMSI avant de désinstaller.", Text);
		}
		else if (MessageBox.Show(this, "Mettre " + sel.Nom + " à la corbeille (carte, rames, objets, plugins) ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
		{
			try
			{
				int num = Omsi.Desinstaller(sel, text);
				lAlerte.Text = sel.Nom + " désinstallé : " + num + " fichiers mis à la corbeille.";
				lAlerte.ForeColor = C.Gris;
				lAlerte.Visible = true;
			}
			catch (UnauthorizedAccessException)
			{
				int num2 = Admin("/desinstaller-admin " + Q(sel.Id) + " " + Q(sel.Liste) + " " + Q(text.TrimEnd('\\')));
				lAlerte.ForeColor = ((num2 == 0) ? C.Gris : C.Rouge);
				lAlerte.Visible = true;
				lAlerte.Text = num2 switch
				{
					-1 => "Désinstallation annulée : l'autorisation administrateur a été refusée.", 
					0 => sel.Nom + " désinstallé.", 
					_ => "La désinstallation a échoué.", 
				};
			}
			catch (Exception ex2)
			{
				MessageBox.Show(this, "La désinstallation a échoué :\n" + ex2.Message, Text);
			}
			MajEtat();
		}
	}

	private void OuvrirDossier()
	{
		string text = Omsi.DetecteOmsi();
		if (text == null)
		{
			return;
		}
		string text2 = ((sel != null && sel.Marque.Length > 0 && Directory.Exists(Path.Combine(text, sel.Marque))) ? Path.Combine(text, sel.Marque) : text);
		try
		{
			Process.Start("explorer.exe", "\"" + text2 + "\"");
		}
		catch
		{
		}
	}

	private void ChoisirDossiers()
	{
		Dossiers(null);
	}

	private void Dossiers(string pourquoi)
	{
		using (FenDossiers f = new FenDossiers(pourquoi))
		{
			if (f.Afficher(this) == DialogResult.OK)
			{
				Omsi.Oublier();
				MajEtat();
			}
		}
		MajDossiers();
	}

	private void MajDossiers()
	{
		string text = Omsi.DetecteOmsi();
		string text2 = Omsi.DetecteOpen(text);
		lDos.Text = ((text != null && text2 != null) ? "●  OMSI 2 et openOMSI trouvés" : "●  Dossiers du jeu à indiquer");
		lDos.ForeColor = ((text != null && text2 != null) ? C.Vert : C.Orange);
		bulle.SetToolTip(lDos, "OMSI 2 : " + (text ?? "introuvable") + "\nopenOMSI : " + (text2 ?? "introuvable") + "\n(cliquez pour changer)");
	}

	private static int[] Num(string v)
	{
		List<int> list = new List<int>();
		string[] array = (v ?? "").Split('.');
		foreach (string s in array)
		{
			int.TryParse(s, out var result);
			list.Add(result);
		}
		while (list.Count < 3)
		{
			list.Add(0);
		}
		return list.ToArray();
	}

	public static bool PlusRecent(string a, string b)
	{
		int[] array = Num(a);
		int[] array2 = Num(b);
		for (int i = 0; i < 3; i++)
		{
			if (array[i] != array2[i])
			{
				return array[i] > array2[i];
			}
		}
		return false;
	}

	private void MajAutoProjets()
	{
		string text = Omsi.DetecteOmsi();
		if (text == null || Omsi.DetecteOpen(text) == null || Omsi.JeuLance())
		{
			return;
		}
		foreach (Projet projet in cat.Projets)
		{
			string[] array = Omsi.Installee(projet, text);
			if (array != null && Omsi.Perimee(projet, array) && projet.APaquet)
			{
				Choisir(projet);
				ShowVue(Vue.Projet);
				Installer(projet);
				break;
			}
		}
	}

	private void RedemarrerSur(string exe, string plus = "")
	{
		RetenirTaille();
		Programme.Liberer();
		try
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo(exe, "/portable /apres " + Process.GetCurrentProcess().Id + plus);
			processStartInfo.UseShellExecute = false;
			processStartInfo.WorkingDirectory = Path.GetDirectoryName(exe);
			Process.Start(processStartInfo);
		}
		catch (Exception ex)
		{
			Toast("Redémarrage impossible : " + ex.Message, C.Rouge, 8000);
			return;
		}
		sortie = true;
		Close();
	}

	private bool VerifierLanceur(bool manuel)
	{
		if (cat == null || cat.LanceurUrl == null || !PlusRecent(cat.LanceurVersion, Config.VERSION))
		{
			return false;
		}
		if (!manuel && !Config.Oui("maj_lanceur", defaut: false))
		{
			bool flag0 = !majLanceurDispo;
			majLanceurDispo = true;
			if (vue == Vue.Accueil)
			{
				DisposerAccueil();
			}
			if (flag0)
			{
				Toast("Lanceur v" + cat.LanceurVersion + " disponible : cliquez sur « Mettre à jour » sur l'accueil.", C.Bleu, 7000);
			}
			return false;
		}
		lAlerte.Text = "Mise à jour du lanceur vers la v" + cat.LanceurVersion + "…";
		lAlerte.ForeColor = C.Bleu;
		lAlerte.Visible = true;
		Toast("Téléchargement de la mise à jour (v" + cat.LanceurVersion + ")…", C.Bleu, 20000);
		string neuf = Application.ExecutablePath + ".neuf";
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				if (!Web.UrlSure(cat.LanceurUrl) || string.IsNullOrEmpty(cat.LanceurSha))
				{
					throw new Exception("adresse ou empreinte de mise à jour invalide");
				}
				if (File.Exists(cat.LanceurUrl))
				{
					File.Copy(cat.LanceurUrl, neuf, overwrite: true);
				}
				else
				{
					Web.Telecharger(cat.LanceurUrl, neuf);
				}
				bool flag = false;
				try
				{
					using FileStream fileStream = File.OpenRead(neuf);
					flag = fileStream.Length > 102400 && fileStream.ReadByte() == 77 && fileStream.ReadByte() == 90;
				}
				catch
				{
				}
				if (!flag || !string.Equals(Web.Sha256(neuf), cat.LanceurSha, StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						File.Delete(neuf);
					}
					catch
					{
					}
					throw new Exception("fichier abîmé ou non conforme");
				}
				string exe = Application.ExecutablePath;
				string vers = exe + ".old";
				Disque.Deplacer(exe, vers);
				Disque.Deplacer(neuf, exe);
				Sur(delegate
				{
					Toast("Mise à jour installée : redémarrage…", C.Vert, 4000);
					RedemarrerSur(exe);
				});
			}
			catch (Exception ex)
			{
				Exception e = ex;
				Sur(delegate
				{
					lAlerte.Text = "Mise à jour du lanceur impossible : " + e.Message;
					Toast("Mise à jour impossible : " + e.Message, C.Rouge, 8000);
				});
			}
		});
		thread.IsBackground = true;
		thread.Start();
		return true;
	}

	private void Parametres()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool pleinEcran = plein;
		bool flag5 = false;
		int num = 0;
		string versionDispo = ((cat != null && cat.LanceurUrl != null && PlusRecent(cat.LanceurVersion, Config.VERSION)) ? cat.LanceurVersion : null);
		using (FenParametres fenParametres = new FenParametres(plein, versionDispo, AppDiscordFournie()))
		{
			if (fenParametres.Afficher(this) == DialogResult.OK)
			{
				flag = fenParametres.DemandeDossiers;
				flag2 = fenParametres.CacheVide;
				flag3 = fenParametres.DemandeMaj;
				flag4 = fenParametres.Reinitialise;
				pleinEcran = fenParametres.PleinEcran;
				flag5 = fenParametres.ChangeLangue || fenParametres.Relancer;
				num = fenParametres.SectionCourante;
			}
		}
		if (flag5)
		{
			RedemarrerSur(Application.ExecutablePath, " /rouvrir " + num + (plein ? " /plein" : ""));
			return;
		}
		if (pleinEcran != plein)
		{
			BasculerPleinEcran();
		}
		if (flag4)
		{
			jeuId = "omsi2";
		}
		if (flag2)
		{
			images.Clear();
			Recharger(manuel: true);
		}
		else if (flag3)
		{
			Recharger(manuel: true);
		}
		else
		{
			RafraichirVues();
		}
		MajPresence();
		if (flag)
		{
			Dossiers(null);
		}
	}

	private void Profil(bool nouveau, int onglet = 0)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (cat != null)
		{
			foreach (Projet projet in cat.Projets)
			{
				dictionary[projet.Id] = projet.Nom;
			}
		}
		bool estAdmin = Comptes.EstAdmin;
		string text = "";
		using (FenCompte fenCompte = new FenCompte(nouveau, dictionary, onglet))
		{
			fenCompte.Afficher(this);
			text = fenCompte.Demande;
		}
		puce.Maj(Comptes.Actif);
		try
		{
			Progression.Connexion(Comptes.Actif);
		}
		catch
		{
		}
		if (cacheProfil != null)
		{
			cacheProfil.Dispose();
			cacheProfil = null;
		}
		if (Comptes.EstAdmin != estAdmin)
		{
			Recharger(manuel: true);
		}
		else
		{
			RafraichirVues();
		}
		if (text == "boutique")
		{
			Boutique();
		}
	}

	private void Succes()
	{
		Compte actif = Comptes.Actif;
		if (actif == null)
		{
			Profil(nouveau: true);
		}
		else
		{
			Profil(nouveau: false, 2);
		}
	}

	private void Admin()
	{
		if (Comptes.EstAdmin)
		{
			using (FenAdmin f = new FenAdmin())
			{
				f.Afficher(this);
			}
			Recharger(manuel: true);
		}
	}

	public void CaptureIntegre(string dossier, bool animation)
	{
		ForceAnim = animation;
		string[] array = new string[31]
		{
			"params", "profil", "profil_nouveau", "admin", "banniere", "jeton", "dossiers", "message", "communaute", "visionneuse",
			"succes", "profil_apparence", "profil_comptes", "profil_vip", "profil_mods", "lies", "amis", "ami_profil", "offrir_vip", "musique",
			"connexion_spotify", "playlists", "support", "boutique", "notifications", "jeux_admin", "admin_jeux", "admin_accueil", "admin_boutique", "admin_communaute",
			"admin_maintenance"
		};
		foreach (string text in array)
		{
			Form form = null;
			switch (text)
			{
			case "params":
				form = new FenParametres(plein, null);
				break;
			case "profil":
				form = new FenCompte(nouveau: false, new Dictionary<string, string>());
				break;
			case "profil_nouveau":
				form = new FenCompte(nouveau: true, new Dictionary<string, string>());
				break;
			case "admin":
				form = new FenAdmin();
				break;
			default:
				if (text.StartsWith("admin_"))
				{
					int og = Array.IndexOf(new string[6] { "admin_jeux", "admin_mods", "admin_accueil", "admin_boutique", "admin_communaute", "admin_maintenance" }, text);
					FenAdmin fa = new FenAdmin();
					fa.Shown += delegate
					{
						fa.Onglet(Math.Max(0, og));
					};
					form = fa;
					break;
				}
				switch (text)
				{
				case "banniere":
					form = new FenBanniere(Comptes.Actif);
					break;
				case "jeton":
					form = new FenJeton();
					break;
				case "dossiers":
					form = new FenDossiers("Exemple de message");
					break;
				case "message":
					form = new FenMessage("Supprimer le profil « Evens » ?\n\nSon historique de jeu, sa photo et sa bannière sont perdus. Vos projets installés ne sont pas touchés.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					break;
				case "communaute":
					form = new FenCommunaute(new Dictionary<string, string>());
					break;
				case "jeux_admin":
				{
					List<Dictionary<string, string>> list2 = new List<Dictionary<string, string>>();
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary["id"] = "masterplan";
					dictionary["nom"] = "MasterPlan";
					dictionary["sous_titre"] = "City Transport Master · bâtis ton empire du transport";
					dictionary["url"] = "http://citytransportmaster.fr/";
					dictionary["image"] = "http://citytransportmaster.fr/logo.png";
					dictionary["statut"] = "bientot";
					list2.Add(dictionary);
					form = new FenJeux(list2);
					break;
				}
				case "succes":
					form = new FenCompte(nouveau: false, new Dictionary<string, string>(), 2);
					break;
				case "profil_apparence":
					form = new FenCompte(nouveau: false, new Dictionary<string, string>(), 1);
					break;
				case "profil_comptes":
					form = new FenCompte(nouveau: false, new Dictionary<string, string>(), 4);
					break;
				case "profil_vip":
					form = new FenCompte(nouveau: false, new Dictionary<string, string>(), 3);
					break;
				case "profil_mods":
					form = new FenCompte(nouveau: false, new Dictionary<string, string>(), 5);
					break;
				case "lies":
				{
					form = new FenLies("Paris 25", ProjetsConnus, (Projet pz) => (!images.TryGetValue(pz.Id, out var value)) ? null : value, delegate(Projet pz)
					{
						LibelleEtat(pz, Omsi.DetecteOmsi(), out var _, out var code);
						switch (code)
						{
						default:
							return 3;
						case 0:
							return 0;
						case 1:
						case 2:
							return code;
						}
					});
					break;
				}
				case "amis":
					form = new FenAmis();
					break;
				case "musique":
					form = new FenMusique();
					break;
				case "connexion_spotify":
					form = new FenConnexionMusique("spotify");
					break;
				case "playlists":
					form = new FenPlaylists();
					break;
				case "support":
				{
					Projet projet = ((ProjetsConnus.Count > 0) ? ProjetsConnus[0] : new Projet());
					if (projet.Nom == null)
					{
						projet.Nom = "Paris 25";
						projet.Id = "paris25";
						projet.Version = "";
					}
					form = new FenSupport(projet, "Lanceur v" + Config.VERSION + "  ·  test");
					break;
				}
				case "ami_profil":
				{
					Ami ami = ((Social.Amis.Count > 0) ? Social.Amis[0] : new Ami());
					form = new FenAmiProfil(ami);
					break;
				}
				case "offrir_vip":
					form = new FenOffrirVip((Social.Amis.Count > 0) ? Social.Amis[0] : new Ami());
					break;
				case "boutique":
					form = new FenBoutique(new List<Projet>(), (Projet pz) => (Image)null, "");
					break;
				case "notifications":
					form = new FenNotifs();
					break;
				default:
				{
					Bitmap bitmap = new Bitmap(1600, 900);
					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, Zx.P(1600), Zx.P(900)), C.Jade, C.Bleu, 35f);
						graphics.FillRectangle(brush, 0, 0, Zx.P(1600), Zx.P(900));
					}
					List<Image> list = new List<Image>();
					list.Add(bitmap);
					form = new FenVisionneuse(list, 0);
					break;
				}
				}
				break;
			}
			System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
			t.Interval = 1800;
			string nomc = text;
			Stopwatch stopwatch = Stopwatch.StartNew();
			t.Tick += delegate
			{
				t.Stop();
				using (Bitmap bitmap2 = Snap((couches.Count > 0) ? ((ScrollableControl)couches[couches.Count - 1]) : ((ScrollableControl)this)))
				{
					bitmap2.Save(Path.Combine(dossier, "int_" + nomc + ".png"));
				}
				if (couches.Count > 0)
				{
					foreach (Control control in couches[couches.Count - 1].Controls)
					{
						if (control is Form form2)
						{
							form2.DialogResult = DialogResult.Cancel;
						}
					}
				}
			};
			t.Start();
			long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			form.Afficher(this);
			Journal.Ecrire("integre", new Exception(text + " ouvert puis ferme apres " + (stopwatch.ElapsedMilliseconds - elapsedMilliseconds) + " ms"));
			t.Dispose();
		}
	}

	public void CaptureVues(string dossier)
	{
		string[] array = new string[4] { "accueil", "jeux", "biblio", "projet" };
		Vue[] array2 = new Vue[4]
		{
			Vue.Accueil,
			Vue.Jeux,
			Vue.Biblio,
			Vue.Projet
		};
		for (int i = 0; i < array2.Length; i++)
		{
			ShowVue(array2[i]);
			Application.DoEvents();
			Thread.Sleep(150);
			Application.DoEvents();
			Refresh();
			Application.DoEvents();
			using Bitmap bitmap = new Bitmap(base.Width, base.Height);
			DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
			bitmap.Save(Path.Combine(dossier, "vue_" + array[i] + ".png"));
		}
		if (cat == null)
		{
			return;
		}
		foreach (Projet projet in cat.Projets)
		{
			Choisir(projet);
			ShowVue(Vue.Projet);
			for (int j = 0; j < 10; j++)
			{
				Thread.Sleep(150);
				Application.DoEvents();
			}
			Refresh();
			Application.DoEvents();
			using (Bitmap bitmap2 = new Bitmap(base.Width, base.Height))
			{
				DrawToBitmap(bitmap2, new Rectangle(0, 0, base.Width, base.Height));
				bitmap2.Save(Path.Combine(dossier, "page_" + projet.Id + ".png"));
			}
			if (projet == cat.Projets[cat.Projets.Count - 1])
			{
				Vue[] array3 = new Vue[2]
				{
					Vue.Biblio,
					Vue.Jeux
				};
				foreach (Vue vue in array3)
				{
					ShowVue(vue);
					for (int l = 0; l < 6; l++)
					{
						Thread.Sleep(150);
						Application.DoEvents();
					}
					Refresh();
					Application.DoEvents();
					using Bitmap bitmap3 = new Bitmap(base.Width, base.Height);
					DrawToBitmap(bitmap3, new Rectangle(0, 0, base.Width, base.Height));
					bitmap3.Save(Path.Combine(dossier, "retour_" + ((vue == Vue.Biblio) ? "biblio" : "jeux") + ".png"));
				}
			}
			page.AutoScrollPosition = new Point(0, Zx.P(320));
			for (int m = 0; m < 3; m++)
			{
				Thread.Sleep(60);
				Application.DoEvents();
			}
			Refresh();
			Application.DoEvents();
			using Bitmap bitmap4 = new Bitmap(base.Width, base.Height);
			DrawToBitmap(bitmap4, new Rectangle(0, 0, base.Width, base.Height));
			bitmap4.Save(Path.Combine(dossier, "page_" + projet.Id + "_defile.png"));
		}
	}

	public void Benchmark(string dossier)
	{
		fond.Preparer(synchrone: true);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Fenêtre " + base.ClientSize.Width + "x" + base.ClientSize.Height + "  (budget d'une image à 60 images/s : 16,7 ms)");
		string[] array = new string[3] { "Accueil", "Jeux", "Bibliothèque" };
		Vue[] array2 = new Vue[3]
		{
			Vue.Accueil,
			Vue.Jeux,
			Vue.Biblio
		};
		for (int i = 0; i < array2.Length; i++)
		{
			ShowVue(array2[i]);
			Application.DoEvents();
			PanelFond p = ((array2[i] == Vue.Accueil) ? pAccueil : ((array2[i] == Vue.Jeux) ? pJeux : pBiblio));
			int width = p.ClientSize.Width;
			int height = p.ClientSize.Height;
			double num = Moyenne(delegate
			{
				fond.Avancer(1.0 / 60.0);
			});
			double num2;
			using (Bitmap image = new Bitmap(width, height, PixelFormat.Format32bppPArgb))
			{
				Graphics g2 = Graphics.FromImage(image);
				try
				{
					num2 = Moyenne(delegate
					{
						p.Peindre(g2, 0);
					});
				}
				finally
				{
					if (g2 != null)
					{
						((IDisposable)g2).Dispose();
					}
				}
			}
			stringBuilder.AppendLine($"{array[i],-14} fond {num,5:0.00} ms + peinture {num2,5:0.00} ms = {num + num2,5:0.00} ms par image  ->  {1000.0 / (num + num2),6:0} images/s possibles");
		}
		ShowVue(Vue.Biblio);
		Application.DoEvents();
		if (tuilesB.Count > 0)
		{
			Tuile t2 = tuilesB[0];
			Bitmap b3 = new Bitmap(t2.Width, t2.Height);
			try
			{
				stringBuilder.AppendLine(Ligne("Repeinture d'une carte (survol)", Moyenne(delegate
				{
					t2.DrawToBitmap(b3, new Rectangle(0, 0, t2.Width, t2.Height));
				})));
			}
			finally
			{
				if (b3 != null)
				{
					((IDisposable)b3).Dispose();
				}
			}
		}
		ShowVue(Vue.Accueil);
		Application.DoEvents();
		if (tuilesA.Count > 0)
		{
			Tuile t = tuilesA[0];
			Bitmap b2 = new Bitmap(t.Width, t.Height);
			try
			{
				stringBuilder.AppendLine(Ligne("Repeinture d'une tuile de l'accueil", Moyenne(delegate
				{
					t.DrawToBitmap(b2, new Rectangle(0, 0, t.Width, t.Height));
				})));
			}
			finally
			{
				if (b2 != null)
				{
					((IDisposable)b2).Dispose();
				}
			}
		}
		ShowVue(Vue.Projet);
		Application.DoEvents();
		using (Bitmap image2 = new Bitmap(page.ClientSize.Width, page.ClientSize.Height, PixelFormat.Format32bppPArgb))
		{
			Graphics g = Graphics.FromImage(image2);
			try
			{
				stringBuilder.AppendLine(Ligne("Fond de la page carte (bannière)", Moyenne(delegate
				{
					PeintPage(g, 0);
				})));
			}
			finally
			{
				if (g != null)
				{
					((IDisposable)g).Dispose();
				}
			}
		}
		Bitmap b = new Bitmap(bAction.Width, bAction.Height);
		try
		{
			stringBuilder.AppendLine(Ligne("Repeinture du bouton JOUER (survol)", Moyenne(delegate
			{
				bAction.DrawToBitmap(b, new Rectangle(0, 0, b.Width, b.Height));
			})));
		}
		finally
		{
			if (b != null)
			{
				((IDisposable)b).Dispose();
			}
		}
		File.WriteAllText(Path.Combine(dossier, "bench.txt"), stringBuilder.ToString(), Encoding.UTF8);
	}

	public void MesureReelle(string dossier)
	{
		fond.Preparer(synchrone: true);
		base.TopMost = true;
		base.Opacity = 0.02;
		base.Location = new Point(0, 0);
		Application.DoEvents();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MESURE RÉELLE · fenêtre " + base.ClientSize.Width + "x" + base.ClientSize.Height + " · peinture à l'écran, au maximum de ce que permet le processeur");
		string[] array = new string[3] { "Accueil", "Jeux", "Bibliothèque" };
		Vue[] array2 = new Vue[3]
		{
			Vue.Accueil,
			Vue.Jeux,
			Vue.Biblio
		};
		for (int i = 0; i < array2.Length; i++)
		{
			ShowVue(array2[i]);
			Application.DoEvents();
			PanelFond panelFond = PanelVisible();
			List<double> list = new List<double>();
			if (i == 0)
			{
				ProfilFps = new Dictionary<string, double>();
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			long num = 3000L;
			while (stopwatch.ElapsedMilliseconds < num)
			{
				long elapsedTicks = stopwatch.ElapsedTicks;
				fond.Avancer(1.0 / 60.0);
				panelFond.Invalidate(invalidateChildren: false);
				panelFond.Update();
				list.Add((double)(stopwatch.ElapsedTicks - elapsedTicks) * 1000.0 / (double)Stopwatch.Frequency);
			}
			if (i == 0 && ProfilFps != null)
			{
				List<KeyValuePair<string, double>> list2 = new List<KeyValuePair<string, double>>(ProfilFps);
				list2.Sort((KeyValuePair<string, double> a, KeyValuePair<string, double> b) => b.Value.CompareTo(a.Value));
				foreach (KeyValuePair<string, double> item in list2)
				{
					stringBuilder.AppendLine($"   accueil · {item.Key,-14} {item.Value / (double)Math.Max(1, list.Count),6:0.00} ms par image");
				}
				ProfilFps = null;
			}
			list.RemoveRange(0, Math.Min(5, list.Count / 4));
			list.Sort();
			double num2 = 0.0;
			foreach (double item2 in list)
			{
				double num3 = item2;
				num2 += num3;
			}
			num2 /= (double)Math.Max(1, list.Count);
			stringBuilder.AppendLine(string.Format("{0,-14} {1,7:0.0} images/s au maximum · {2,5:0.00} ms en moyenne · 99e centile {3,5:0.00} ms · pire {4,5:0.0} ms  ({5})", array[i], 1000.0 / Math.Max(0.01, num2), num2, list[(int)((double)list.Count * 0.99)], list[list.Count - 1], (1000.0 / Math.Max(0.01, num2) >= 60.0) ? "≥ 60 images/s" : "< 60 images/s"));
		}
		base.Opacity = 1.0;
		base.TopMost = false;
		File.WriteAllText(Path.Combine(dossier, "fps.txt"), stringBuilder.ToString(), Encoding.UTF8);
	}

	private static string Ligne(string nom, double ms)
	{
		return $"{nom,-40} {ms,6:0.00} ms  ->  {1000.0 / Math.Max(0.001, ms),6:0} par seconde possible";
	}

	private static double Moyenne(Action a)
	{
		for (int i = 0; i < 15; i++)
		{
			a();
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		while (stopwatch.ElapsedMilliseconds < 1200)
		{
			a();
			num++;
		}
		return 1200.0 / (double)num;
	}

	/// <summary>Outil de développement : ouvre directement une page (« /apercu biblio », « /apercu projet parisview »).</summary>
	public void Apercu(string vue, string arg)
	{
		Show();
		Application.DoEvents();
		for (int i = 0; i < 80 && cat == null; i++)
		{
			Thread.Sleep(100);
			Application.DoEvents();
		}
		switch (vue)
		{
		case "jeux":
			ShowVue(Vue.Jeux);
			break;
		case "projet":
		{
			Projet projet = (cat != null) ? cat.Projets.Find((Projet x) => x.Id == arg) : null;
			if (projet != null)
			{
				Choisir(projet);
			}
			ShowVue(Vue.Projet);
			break;
		}
		case "biblio":
			if (!string.IsNullOrEmpty(arg))
			{
				jeuId = arg;
			}
			ShowVue(Vue.Biblio);
			break;
		case "profil":
			BeginInvoke((MethodInvoker)delegate
			{
				int.TryParse(arg ?? "0", out var result);
				Profil(nouveau: false, result);
			});
			break;
		}
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		timer.Interval = 1500;
		timer.Tick += delegate
		{
			Action<Control> tout = null;
			tout = delegate(Control c)
			{
				c.Invalidate();
				foreach (Control control in c.Controls)
				{
					tout(control);
				}
			};
			tout(this);
		};
		timer.Start();
	}

	public new void Capture(string dossier)
	{
		Show();
		Application.DoEvents();
		for (int i = 0; i < 60; i++)
		{
			if (cat != null)
			{
				break;
			}
			Thread.Sleep(100);
			Application.DoEvents();
		}
		Thread.Sleep(300);
		Application.DoEvents();
		Refresh();
		Application.DoEvents();
		using Bitmap bitmap = new Bitmap(base.Width, base.Height);
		DrawToBitmap(bitmap, new Rectangle(0, 0, base.Width, base.Height));
		bitmap.Save(Path.Combine(dossier, "lanceur.png"));
	}
}
