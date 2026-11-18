using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenJeux : Form
{
	public Action Publie;

	public Action Annule;

	public List<Dictionary<string, string>> Jeux;

	private Bouton[] boutonsMedias;

	private ListBox liste;

	private Champ tId;

	private Champ tNom;

	private Champ tSous;

	private Champ tUrl;

	private Champ tImage;

	private Champ tPhotos;

	private Champ tVideo;

	private Champ tCats;

	private Champ tSteam;

	private Champ tEditeur;

	private Champ tSortie;

	private Champ tGenres;

	private Champ tDescJ;

	private Segments sgStatut;

	private Option cPlein;

	private Label lErr;

	private bool charge;

	private static readonly string[] statutsId = new string[3] { "publie", "bientot", "masque" };

	private static readonly string[] statutsNom = new string[3] { "Disponible", "Prochainement", "Masqué" };

	private Dictionary<string, string> Cur
	{
		get
		{
			int selectedIndex = liste.SelectedIndex;
			if (selectedIndex < 0 || selectedIndex >= Jeux.Count)
			{
				return null;
			}
			return Jeux[selectedIndex];
		}
	}

	private string IdMedia
	{
		get
		{
			string text = tId.Text.Trim().ToLowerInvariant();
			return "jeu-" + (Regex.IsMatch(text, "^[a-z0-9_-]{2,40}$") ? text : "nouveau");
		}
	}

	public FenJeux(List<Dictionary<string, string>> jeux)
	{
		Jeux = jeux;
		if (!jeux.Exists((Dictionary<string, string> e0) => e0.TryGetValue("id", out var value) && value == "omsi2"))
		{
			jeux.Insert(0, new Dictionary<string, string>
			{
				["id"] = "omsi2",
				["nom"] = "OMSI 2"
			});
		}
		Text = "Jeux du lanceur";
		base.ClientSize = new Size(Zx.P(1240), Zx.P(1036));
		SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, value: true);
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
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
		Lb("Jeux du lanceur", Zx.P(28), Zx.P(18), Zx.P(600), Zx.P(34), new Font("Segoe UI Semibold", Zx.P(17f)), C.Texte);
		Lb("Jeux en ligne affichés dans la page « Jeux ». « Prochainement » et « Masqué » : seuls les administrateurs peuvent les ouvrir.", Zx.P(28), Zx.P(54), Zx.P(900), Zx.P(22), new Font("Segoe UI", Zx.P(9.5f)), C.Gris);
		Groupe("Jeux");
		liste = new ListBox();
		liste.Location = new Point(Zx.P(44), Zx.P(132));
		liste.Size = new Size(Zx.P(216), Zx.P(700));
		liste.BorderStyle = BorderStyle.None;
		liste.BackColor = C.Carte;
		liste.ForeColor = C.Texte;
		liste.Font = new Font("Segoe UI", Zx.P(10.5f));
		liste.IntegralHeight = false;
		liste.DrawMode = DrawMode.OwnerDrawFixed;
		liste.ItemHeight = Zx.P(46);
		liste.DrawItem += DessinerJeu;
		base.Controls.Add(liste);
		Membre(liste);
		Theme.Sombre(liste);
		liste.SelectedIndexChanged += delegate
		{
			Afficher();
		};
		Bouton bouton = new Bouton("＋  Nouveau jeu");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(216), Zx.P(38));
		bouton.Location = new Point(Zx.P(44), Zx.P(844));
		Membre(bouton);
		bouton.Click += delegate
		{
			Nouveau();
		};
		base.Controls.Add(bouton);
		Bouton bouton2 = new Bouton("Retirer ce jeu");
		bouton2.Plein = false;
		bouton2.Danger = true;
		bouton2.Size = new Size(Zx.P(216), Zx.P(38));
		bouton2.Location = new Point(Zx.P(44), Zx.P(890));
		Membre(bouton2);
		bouton2.Click += delegate
		{
			Retirer();
		};
		base.Controls.Add(bouton2);
		int num = Zx.P(312);
		int num2 = Zx.P(532);
		Groupe("Identité");
		tId = ChampL("Identifiant (lettres minuscules, chiffres, - et _)", num, Zx.P(128), Zx.P(260), 40);
		tNom = ChampL("Nom affiché", num, Zx.P(190), num2, 60);
		tSous = ChampL("Sous-titre (une ligne)", num, Zx.P(252), num2, 120);
		tUrl = ChampL("Adresse du jeu (http:// ou https://)", num, Zx.P(314), num2, 300);
		Groupe("Médias");
		int num3 = Zx.P(150);
		int num4 = num2 - num3 - Zx.P(10);
		tImage = ChampL("Image : logo ou bannière (adresse ou fichier jpg / png)", num, Zx.P(448), num4, 300);
		Bouton bouton3 = new Bouton("Importer…");
		bouton3.Plein = false;
		bouton3.Size = new Size(num3, Zx.P(36));
		bouton3.Location = new Point(num + num4 + Zx.P(10), Zx.P(470));
		Membre(bouton3);
		bouton3.Click += delegate
		{
			ImporterImage();
		};
		base.Controls.Add(bouton3);
		Lb("Photos : une adresse par ligne (10 au maximum), ou « Ajouter des photos… »", num, Zx.P(512), num2, Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		tPhotos = new Champ(multi: true, Zx.P(96));
		tPhotos.Location = new Point(num, Zx.P(534));
		Membre(tPhotos);
		tPhotos.Width = num2;
		tPhotos.MaxLength = 3000;
		base.Controls.Add(tPhotos);
		Bouton bouton4 = new Bouton("＋  Ajouter des photos…");
		bouton4.Plein = false;
		bouton4.Size = new Size(Zx.P(220), Zx.P(36));
		bouton4.Location = new Point(num, Zx.P(638));
		Membre(bouton4);
		bouton4.Click += delegate
		{
			ImporterPhotos();
		};
		base.Controls.Add(bouton4);
		tVideo = ChampL("Bande-annonce MP4 (adresse https://… ou fichier)", num, Zx.P(688), num4, 400);
		Bouton bouton5 = new Bouton("Importer…");
		bouton5.Plein = false;
		bouton5.Size = new Size(num3, Zx.P(36));
		bouton5.Location = new Point(num + num4 + Zx.P(10), Zx.P(710));
		Membre(bouton5);
		bouton5.Click += delegate
		{
			ImporterVideo();
		};
		base.Controls.Add(bouton5);
		Groupe("Publication");
		Lb("Statut", num, Zx.P(828), Zx.P(200), Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		sgStatut = new Segments();
		sgStatut.Items = statutsNom;
		sgStatut.Etire = true;
		sgStatut.Location = new Point(num, Zx.P(850));
		Membre(sgStatut);
		sgStatut.Width = num2;
		sgStatut.Changed += delegate
		{
			Enregistrer();
		};
		base.Controls.Add(sgStatut);
		cPlein = new Option("Plein écran", "Le jeu s'ouvre en plein écran (F11 pour en sortir).", val: true);
		cPlein.Location = new Point(num - Zx.P(10), Zx.P(898));
		cPlein.Width = num2 + Zx.P(10);
		Membre(cPlein);
		cPlein.Changed += delegate
		{
			Enregistrer();
		};
		base.Controls.Add(cPlein);
		boutonsMedias = new Bouton[3] { bouton3, bouton4, bouton5 };
		Groupe("Catégories de tri des mods");
		Lb("Une catégorie par ligne (12 au maximum). Elles servent de filtres dans « Mods » et de choix dans les pages de ce jeu.", Zx.P(900), Zx.P(128), Zx.P(292), Zx.P(54), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		tCats = new Champ(multi: true, Zx.P(180));
		tCats.Location = new Point(Zx.P(900), Zx.P(186));
		tCats.Width = Zx.P(292);
		Membre(tCats);
		tCats.MaxLength = 1000;
		base.Controls.Add(tCats);
		int xd = Zx.P(900);
		int wd = Zx.P(292);
		Groupe("Fiche « À propos » du jeu");
		tSteam = ChampL("AppID Steam (remplit automatiquement la fiche)", xd, Zx.P(448), wd, 12);
		tSteam.Indice = "ex. 252530";
		tEditeur = ChampL("Éditeur / développeur", xd, Zx.P(510), wd, 120);
		int wm = (wd - Zx.P(10)) / 2;
		tSortie = ChampL("Date de sortie", xd, Zx.P(572), wm, 40);
		tGenres = ChampL("Genres", xd + wm + Zx.P(10), Zx.P(572), wm, 120);
		Lb("Description (remplace celle de Steam)", xd, Zx.P(634), wd, Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		tDescJ = new Champ(multi: true, Zx.P(240));
		tDescJ.Location = new Point(xd, Zx.P(656));
		Membre(tDescJ);
		groupeCourant = null;
		tDescJ.Width = wd;
		tDescJ.MaxLength = 4000;
		base.Controls.Add(tDescJ);
		Champ[] array = new Champ[13] { tId, tNom, tSous, tUrl, tImage, tPhotos, tVideo, tCats, tSteam, tEditeur, tSortie, tGenres, tDescJ };
		foreach (Champ champ in array)
		{
			champ.TextChanged += delegate
			{
				Enregistrer();
			};
		}
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(56),
			BackColor = C.Fond
		};
		base.Controls.Add(panel);
		lErr = new Label();
		lErr.Font = new Font("Segoe UI", Zx.P(9.5f));
		lErr.ForeColor = C.Rouge;
		lErr.BackColor = C.Fond;
		lErr.Location = new Point(Zx.P(146), Zx.P(10));
		lErr.Size = new Size(Zx.P(530), Zx.P(36));
		lErr.AutoEllipsis = true;
		lErr.TextAlign = ContentAlignment.MiddleLeft;
		panel.Controls.Add(lErr);
		Bouton bouton6 = new Bouton("Annuler");
		bouton6.Plein = false;
		bouton6.Size = new Size(Zx.P(100), Zx.P(38));
		bouton6.Location = new Point(Zx.P(28), Zx.P(9));
		bouton6.Click += delegate
		{
			if (Annule != null)
			{
				Annule();
			}
			else
			{
				base.DialogResult = DialogResult.Cancel;
			}
		};
		panel.Controls.Add(bouton6);
		Bouton bouton7 = new Bouton("Publier");
		bouton7.Size = new Size(Zx.P(140), Zx.P(38));
		bouton7.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(140), Zx.P(9));
		bouton7.Click += delegate
		{
			Valider();
		};
		panel.Controls.Add(bouton7);
		boutonPublier = bouton7;
		Remplir(0);
	}

	private Bouton boutonPublier;

	/// <summary>
	/// Fenêtre étroite (petit écran, zoom Windows) : la colonne de droite (catégories, fiche « À propos »)
	/// passe sous la colonne du milieu au lieu d'être coupée.
	/// </summary>
	public void AdapterLargeur(int largeur)
	{
		int seuil = Zx.P(860);
		if (largeur >= base.ClientSize.Width || largeur < Zx.P(400))
		{
			return;
		}
		List<Control> droite = new List<Control>();
		int basMilieu = 0;
		int hautDroite = int.MaxValue;
		int basDroite = 0;
		foreach (Control c in base.Controls)
		{
			if (c.Dock != DockStyle.None)
			{
				continue;
			}
			if (c.Left >= seuil)
			{
				droite.Add(c);
				hautDroite = Math.Min(hautDroite, c.Top);
				basDroite = Math.Max(basDroite, c.Bottom);
			}
			else if (c.Left >= Zx.P(280))
			{
				basMilieu = Math.Max(basMilieu, c.Bottom);
			}
		}
		if (droite.Count == 0)
		{
			return;
		}
		int dx = Zx.P(296) - (seuil + Zx.P(10));
		int dy = basMilieu + Zx.P(28) - hautDroite;
		foreach (Control c2 in droite)
		{
			c2.Location = new Point(Math.Max(Zx.P(296), c2.Left + dx), c2.Top + dy);
		}
		base.ClientSize = new Size(seuil, basDroite + dy + Zx.P(24) + Zx.P(56));
		if (boutonPublier != null)
		{
			boutonPublier.Left = base.ClientSize.Width - Zx.P(28) - boutonPublier.Width;
		}
	}

	/// <summary>Cartes de la page (titre + contrôles) : dessinées derrière les contrôles, elles suivent leur place.</summary>
	private readonly List<KeyValuePair<string, List<Control>>> groupes = new List<KeyValuePair<string, List<Control>>>();

	private List<Control> groupeCourant;

	private void Groupe(string titre)
	{
		groupeCourant = new List<Control>();
		groupes.Add(new KeyValuePair<string, List<Control>>(titre, groupeCourant));
	}

	private void Membre(Control c)
	{
		if (groupeCourant == null || groupeCourant.Contains(c))
		{
			return;
		}
		groupeCourant.Add(c);
		if (!(c is Bouton))
		{
			c.BackColor = C.Carte;
		}
	}

	private Rectangle Carte(List<Control> l)
	{
		Rectangle r = Rectangle.Empty;
		foreach (Control c in l)
		{
			r = (r.IsEmpty ? c.Bounds : Rectangle.Union(r, c.Bounds));
		}
		if (r.IsEmpty)
		{
			return r;
		}
		return Rectangle.FromLTRB(r.Left - Zx.P(16), r.Top - Zx.P(46), r.Right + Zx.P(16), r.Bottom + Zx.P(14));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		// colonnes de même hauteur sur une même ligne : on aligne le bas des cartes voisines
		List<Rectangle> rs = new List<Rectangle>();
		foreach (KeyValuePair<string, List<Control>> gr in groupes)
		{
			rs.Add(Carte(gr.Value));
		}
		for (int i = 0; i < groupes.Count; i++)
		{
			Rectangle r = rs[i];
			if (r.IsEmpty)
			{
				continue;
			}
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
			{
				using (SolidBrush brush = new SolidBrush(C.Carte))
				{
					g.FillPath(brush, path);
				}
				using Pen pen = new Pen(Color.FromArgb(40, 255, 255, 255));
				g.DrawPath(pen, path);
			}
			using (SolidBrush brush2 = new SolidBrush(C.Jade))
			{
				g.FillRectangle(brush2, r.X + Zx.P(16), r.Y + Zx.P(19), Zx.P(3), Zx.P(14));
			}
			Lang.DrawText(g, Lang.T(groupes[i].Key).ToUpperInvariant(), F.Rubrique, new Point(r.X + Zx.P(26), r.Y + Zx.P(17)), C.Gris);
		}
	}

	private static readonly Font fNomJeu = new Font("Segoe UI Semibold", Zx.P(10f));

	private void DessinerJeu(object o, DrawItemEventArgs e)
	{
		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (SolidBrush fond = new SolidBrush(C.Carte))
		{
			g.FillRectangle(fond, e.Bounds);
		}
		if (e.Index < 0 || e.Index >= Jeux.Count)
		{
			return;
		}
		Dictionary<string, string> j = Jeux[e.Index];
		bool sel = (e.State & DrawItemState.Selected) != 0;
		Rectangle r = new Rectangle(e.Bounds.X + Zx.P(2), e.Bounds.Y + Zx.P(3), e.Bounds.Width - Zx.P(4), e.Bounds.Height - Zx.P(6));
		if (sel)
		{
			using GraphicsPath p = Bouton.Rond(r, Zx.P(10));
			using SolidBrush b = new SolidBrush(Color.FromArgb(60, C.Jade));
			g.FillPath(b, p);
			using SolidBrush b2 = new SolidBrush(C.Jade);
			g.FillRectangle(b2, r.X, r.Y + Zx.P(10), Zx.P(3), r.Height - Zx.P(20));
		}
		string statut = V(j, "statut");
		Color point = ((statut == "bientot") ? C.Orange : ((statut == "masque") ? C.Gris : C.Vert));
		using (SolidBrush bp = new SolidBrush(point))
		{
			g.FillEllipse(bp, r.X + Zx.P(14), r.Y + r.Height / 2 - Zx.P(4), Zx.P(8), Zx.P(8));
		}
		string nom = (V(j, "nom").Length > 0) ? V(j, "nom") : V(j, "id");
		string etat = ((statut == "bientot") ? "Prochainement" : ((statut == "masque") ? "Masqué" : "Disponible"));
		Lang.DrawText(g, nom, fNomJeu, new Rectangle(r.X + Zx.P(30), r.Y + Zx.P(4), r.Width - Zx.P(36), Zx.P(20)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		Lang.DrawText(g, etat, F.Etat, new Rectangle(r.X + Zx.P(30), r.Y + Zx.P(22), r.Width - Zx.P(36), Zx.P(16)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
	}

	private Label Lb(string t, int x, int y, int w, int h, Font f, Color c)
	{
		Label label = new Label();
		label.Text = t;
		label.Location = new Point(x, y);
		label.Size = new Size(w, h);
		label.Font = f;
		label.ForeColor = c;
		label.BackColor = C.Fond;
		base.Controls.Add(label);
		Membre(label);
		return label;
	}

	private Champ ChampL(string etiquette, int x, int y, int w, int max)
	{
		Lb(etiquette, x, y, w, Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Gris);
		Champ champ = new Champ(multi: false, Zx.P(36));
		champ.Location = new Point(x, y + Zx.P(22));
		champ.Width = w;
		champ.MaxLength = max;
		base.Controls.Add(champ);
		Membre(champ);
		return champ;
	}

	private static string V(Dictionary<string, string> j, string k)
	{
		if (!j.TryGetValue(k, out var value))
		{
			return "";
		}
		return value;
	}

	private void Remplir(int sel)
	{
		charge = true;
		liste.Items.Clear();
		foreach (Dictionary<string, string> item in Jeux)
		{
			string text = V(item, "statut");
			liste.Items.Add(((V(item, "nom").Length > 0) ? V(item, "nom") : V(item, "id")) + ((text == "bientot") ? "  · bientôt" : ((text == "masque") ? "  · masqué" : "")));
		}
		charge = false;
		if (Jeux.Count > 0)
		{
			liste.SelectedIndex = Math.Max(0, Math.Min(sel, Jeux.Count - 1));
		}
		else
		{
			Afficher();
		}
	}

	private void Afficher()
	{
		if (!charge)
		{
			Dictionary<string, string> cur = Cur;
			charge = true;
			Control[] array = new Control[7] { tId, tNom, tSous, tUrl, tImage, sgStatut, cPlein };
			foreach (Control control in array)
			{
				control.Enabled = cur != null;
			}
			tId.Text = ((cur != null) ? V(cur, "id") : "");
			tNom.Text = ((cur != null) ? V(cur, "nom") : "");
			tSous.Text = ((cur != null) ? V(cur, "sous_titre") : "");
			tUrl.Text = ((cur != null) ? V(cur, "url") : "");
			tImage.Text = ((cur != null) ? V(cur, "image") : "");
			tPhotos.Text = ((cur != null) ? V(cur, "galerie").Replace("\r\n", "\n").Replace("\n", "\r\n") : "");
			tCats.Enabled = cur != null;
			tCats.Text = ((cur == null) ? "" : ((V(cur, "categories").Trim().Length > 0) ? V(cur, "categories").Replace("\r\n", "\n").Replace("\n", "\r\n") : Categories.Texte(V(cur, "id"))));
			tVideo.Text = ((cur != null) ? V(cur, "video") : "");
			Champ[] fiche = new Champ[5] { tSteam, tEditeur, tSortie, tGenres, tDescJ };
			foreach (Champ c2 in fiche)
			{
				c2.Enabled = cur != null;
			}
			tSteam.Text = ((cur != null) ? V(cur, "steam") : "");
			tEditeur.Text = ((cur != null) ? V(cur, "editeur") : "");
			tSortie.Text = ((cur != null) ? V(cur, "sortie") : "");
			tGenres.Text = ((cur != null) ? V(cur, "genres") : "");
			tDescJ.Text = ((cur != null) ? V(cur, "description").Replace("\r\n", "\n").Replace("\n", "\r\n") : "");
			if (cur != null)
			{
				RemplirAuto(cur);
			}
			bool flag = cur != null && V(cur, "id") == "omsi2";
			tId.Enabled = cur != null && !flag;
			tUrl.Enabled = cur != null && !flag;
			sgStatut.Enabled = cur != null && !flag;
			cPlein.Enabled = cur != null && !flag;
			if (flag)
			{
				tUrl.Text = "";
				tUrl.Indice = "lancé par OMSI 2 / openOMSI";
			}
			else
			{
				tUrl.Indice = "";
			}
			sgStatut.Index = ((cur != null) ? Math.Max(0, Array.IndexOf(statutsId, (V(cur, "statut").Length > 0) ? V(cur, "statut") : "publie")) : 0);
			cPlein.Checked = cur == null || V(cur, "plein_ecran") != "0";
			charge = false;
		}
	}

	/// <summary>
	/// Champs vides : on y met les valeurs réellement affichées dans le lanceur (fiche Steam,
	/// textes par défaut), pour que l'admin voie et puisse corriger ce que voient les joueurs.
	/// Ce qui est saisi ici passe avant Steam.
	/// </summary>
	private void RemplirAuto(Dictionary<string, string> cur)
	{
		string id = V(cur, "id");
		bool omsi = id == "omsi2";
		string appid = V(cur, "steam").Trim();
		if (appid.Length == 0 && omsi)
		{
			appid = "252530";
		}
		if (tSteam.Text.Length == 0 && omsi)
		{
			tSteam.Text = appid;
		}
		if (tSous.Text.Length == 0)
		{
			tSous.Text = (omsi ? "Simulateur de bus et de métro · cartes, rames et plugins de cabine" : "Jeu TPS Groupe");
		}
		if (tImage.Text.Length == 0 && omsi)
		{
			tImage.Indice = "image intégrée au lanceur";
		}
		else
		{
			tImage.Indice = "";
		}
		FicheSteam f = null;
		if (appid.Length > 0)
		{
			f = Steam.Fiche(appid, delegate
			{
				try
				{
					BeginInvoke((MethodInvoker)delegate
					{
						if (!base.IsDisposed && Cur == cur)
						{
							Afficher();
						}
					});
				}
				catch
				{
				}
			});
		}
		if (f != null)
		{
			if (tEditeur.Text.Length == 0)
			{
				tEditeur.Text = ((f.Editeurs.Length > 0) ? f.Editeurs : f.Developpeurs);
			}
			if (tSortie.Text.Length == 0)
			{
				tSortie.Text = f.Sortie;
			}
			if (tGenres.Text.Length == 0)
			{
				tGenres.Text = f.Genres;
			}
			if (tDescJ.Text.Length == 0)
			{
				tDescJ.Text = f.Description.Replace("\r\n", "\n").Replace("\n", "\r\n");
			}
			if (tPhotos.Text.Length == 0 && f.Captures.Count > 0)
			{
				List<string> l = new List<string>();
				foreach (string[] c in f.Captures)
				{
					if (l.Count < 10)
					{
						l.Add(c[1]);
					}
				}
				tPhotos.Text = string.Join("\r\n", l.ToArray());
			}
			if (tVideo.Text.Length == 0 && f.Video.Length > 0)
			{
				tVideo.Text = f.Video;
			}
		}
		if (tDescJ.Text.Length == 0 && omsi)
		{
			tDescJ.Text = "Simulateur de bus et de métro. Installez cartes, bus, livrées et packs IA en un clic depuis l'onglet Mods, puis lancez le jeu avec openOMSI, ou directement sur une carte depuis « Mes mods ».";
		}
	}

	private void Enregistrer()
	{
		if (charge)
		{
			return;
		}
		Dictionary<string, string> cur = Cur;
		if (cur != null)
		{
			cur["id"] = tId.Text.Trim().ToLowerInvariant();
			cur["nom"] = tNom.Text.Trim();
			cur["sous_titre"] = tSous.Text.Trim();
			cur["url"] = tUrl.Text.Trim();
			cur["image"] = tImage.Text.Trim();
			cur["galerie"] = tPhotos.Text.Replace("\r\n", "\n").Trim();
			cur["video"] = tVideo.Text.Trim();
			string st = Regex.Match(tSteam.Text, "\\d{1,10}").Value;
			cur["steam"] = st;
			cur["editeur"] = tEditeur.Text.Trim();
			cur["sortie"] = tSortie.Text.Trim();
			cur["genres"] = tGenres.Text.Trim();
			cur["description"] = tDescJ.Text.Replace("\r\n", "\n").Trim();
			cur["categories"] = tCats.Text.Replace("\r\n", "\n").Trim();
			Categories.Definir(cur["id"], cur["categories"]);
			cur["statut"] = ((sgStatut.Index > 0) ? statutsId[sgStatut.Index] : "");
			cur["plein_ecran"] = (cPlein.Checked ? "" : "0");
			if (cur["id"] == "omsi2")
			{
				cur["url"] = "";
				cur["statut"] = "";
				cur["plein_ecran"] = "";
			}
			int selectedIndex = liste.SelectedIndex;
			charge = true;
			string text = cur["statut"];
			liste.Items[selectedIndex] = ((cur["nom"].Length > 0) ? cur["nom"] : cur["id"]) + ((text == "bientot") ? "  · bientôt" : ((text == "masque") ? "  · masqué" : ""));
			charge = false;
			lErr.Text = "";
		}
	}

	private void Envoyer(string quoi, Func<Action<string>, string> envoi, Action<string> fini)
	{
		if (Cur == null || Fenetre.ModeCapture)
		{
			return;
		}
		Bouton[] array = boutonsMedias;
		foreach (Bouton bouton in array)
		{
			bouton.Enabled = false;
		}
		lErr.ForeColor = C.Gris;
		lErr.Text = "Envoi " + quoi + "…";
		Action<string> etat = delegate(string t)
		{
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					lErr.ForeColor = C.Gris;
					lErr.Text = t;
				});
			}
			catch
			{
			}
		};
		Thread thread = new Thread((ThreadStart)delegate
		{
			string url = null;
			string err = null;
			try
			{
				url = envoi(etat);
			}
			catch (Exception ex)
			{
				err = ex.Message;
				Journal.Ecrire("admin-jeux-media", ex);
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					Bouton[] array2 = boutonsMedias;
					foreach (Bouton bouton2 in array2)
					{
						bouton2.Enabled = true;
					}
					if (err != null)
					{
						lErr.ForeColor = C.Rouge;
						lErr.Text = "✖  Envoi impossible : " + err;
					}
					else
					{
						fini(url);
						lErr.ForeColor = C.Vert;
						lErr.Text = "✔  Fichier envoyé : cliquez sur « Publier » pour l'appliquer.";
					}
				});
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void ImporterImage()
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Image (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
		openFileDialog.Title = "Logo ou bannière du jeu";
		if (openFileDialog.ShowDialog(this) == DialogResult.OK)
		{
			string f = openFileDialog.FileName;
			string id = IdMedia;
			Envoyer("de l'image", (Action<string> e) => GitHub.EnvoyerImage(f, id), delegate(string u)
			{
				tImage.Text = u;
			});
		}
	}

	private void ImporterPhotos()
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
		openFileDialog.Multiselect = true;
		openFileDialog.Title = "Photos du jeu (10 au maximum)";
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		string[] fs = openFileDialog.FileNames;
		string id = IdMedia;
		int num = tPhotos.Text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
		if (num + fs.Length > 10)
		{
			lErr.ForeColor = C.Rouge;
			lErr.Text = "10 photos au maximum (il y en a déjà " + num + ").";
			return;
		}
		Envoyer("des photos", delegate(Action<string> e)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < fs.Length; i++)
			{
				e("Envoi des photos… " + (i + 1) + " / " + fs.Length);
				list.Add(GitHub.EnvoyerImageGalerie(fs[i], id));
			}
			return string.Join("\r\n", list.ToArray());
		}, delegate(string u)
		{
			string text = tPhotos.Text.TrimEnd();
			tPhotos.Text = ((text.Length > 0) ? (text + "\r\n") : "") + u;
		});
	}

	private void ImporterVideo()
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Vidéo MP4 (*.mp4)|*.mp4";
		openFileDialog.Title = "Bande-annonce du jeu";
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		string f = openFileDialog.FileName;
		string id = IdMedia;
		if (new FileInfo(f).Length > 1992294400)
		{
			lErr.ForeColor = C.Rouge;
			lErr.Text = "Vidéo trop lourde (1,9 Go maximum).";
			return;
		}
		Envoyer("de la vidéo", (Action<string> e) => GitHub.EnvoyerVideo(f, id, e), delegate(string u)
		{
			tVideo.Text = u;
		});
	}

	private void Nouveau()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["id"] = "nouveau-jeu";
		dictionary["nom"] = "Nouveau jeu";
		dictionary["statut"] = "masque";
		Jeux.Add(dictionary);
		Remplir(Jeux.Count - 1);
		tNom.Focus();
	}

	private void Retirer()
	{
		Dictionary<string, string> cur = Cur;
		if (cur != null)
		{
			if (V(cur, "id") == "omsi2")
			{
				lErr.Text = "OMSI 2 reste toujours dans le lanceur.";
			}
			else if (MessageBox.Show(this, "Retirer « " + V(cur, "nom") + " » du lanceur ?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				int selectedIndex = liste.SelectedIndex;
				Jeux.Remove(cur);
				Remplir(selectedIndex);
			}
		}
	}

	private void Valider()
	{
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < Jeux.Count; i++)
		{
			Dictionary<string, string> j = Jeux[i];
			string text = ((V(j, "nom").Length > 0) ? V(j, "nom") : ("jeu " + (i + 1)));
			if (!Regex.IsMatch(V(j, "id"), "^[a-z0-9_-]{2,40}$"))
			{
				liste.SelectedIndex = i;
				lErr.Text = text + " : identifiant invalide (2 à 40 lettres minuscules, chiffres, - ou _).";
				return;
			}
			if (!hashSet.Add(V(j, "id")))
			{
				liste.SelectedIndex = i;
				lErr.Text = text + " : cet identifiant est déjà utilisé.";
				return;
			}
			if (V(j, "nom").Length == 0)
			{
				liste.SelectedIndex = i;
				lErr.Text = "Indiquez le nom du jeu.";
				return;
			}
			if (V(j, "id") != "omsi2" && (!Uri.TryCreate(V(j, "url"), UriKind.Absolute, out var result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp)))
			{
				liste.SelectedIndex = i;
				lErr.Text = text + " : adresse du jeu invalide (http:// ou https://).";
				return;
			}
			string[] array = V(j, "galerie").Replace("\r", "").Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array)
			{
				if (!Uri.TryCreate(text2.Trim(), UriKind.Absolute, out result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp))
				{
					liste.SelectedIndex = i;
					lErr.Text = text + " : une adresse de photo est invalide (http:// ou https://).";
					return;
				}
			}
			if (V(j, "video").Length > 0 && (!Uri.TryCreate(V(j, "video"), UriKind.Absolute, out result) || result.Scheme != Uri.UriSchemeHttps))
			{
				liste.SelectedIndex = i;
				lErr.Text = text + " : l'adresse de la vidéo doit commencer par https://";
				return;
			}
			if (V(j, "image").Length > 0 && (!Uri.TryCreate(V(j, "image"), UriKind.Absolute, out result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp)))
			{
				liste.SelectedIndex = i;
				lErr.Text = text + " : adresse de l'image invalide.";
				return;
			}
		}
		if (Publie != null)
		{
			Publie();
		}
		else
		{
			base.DialogResult = DialogResult.OK;
		}
	}
}
