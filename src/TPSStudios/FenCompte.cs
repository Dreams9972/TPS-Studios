using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenCompte : Form, IPage
{
	private const int GCols = 4;

	private bool creation;

	private bool fermerApres;

	private bool change;

	private Dictionary<string, string> noms;

	private string retour = "";

	private Control hote;

	private int CW;

	private int X0;

	private int onglet;

	private Panel[] pages;

	private ZoneDessin zSucces;

	private ZoneDessin zVip;

	public string Demande = "";

	private Champ tPseudo;

	private Champ tStatut;

	private Champ tBio;

	private Champ tJeu;

	private Champ tLieu;

	private PaletteCouleurs pal;

	private SelecteurCouleur selAvatar;

	private RangeeCouleurs rcPhoto;

	private Option oAuto;

	private Label lErr;

	private ListBox lb;

	private Panel contenu;

	private ZoneDessin apercu;

	private ZoneDessin miniature;

	private static readonly Font fModN = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fModS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fNomA = new Font("Segoe UI Semibold", Zx.P(18f));

	private static readonly Font fStatA = new Font("Segoe UI", Zx.P(10f), FontStyle.Italic);

	private static readonly Font fTxtA = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fNivA = new Font("Segoe UI Semibold", Zx.P(12f));

	private static readonly Font fNomS = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fDescS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fGlyS = new Font("Segoe MDL2 Assets", Zx.P(16f));

	private static readonly Font fPetitS = new Font("Segoe UI", Zx.P(8.5f));

	private static readonly Font fCadenas = new Font("Segoe MDL2 Assets", Zx.P(15f));

	private static int GGap => Zx.P(12);

	private static int GTitre => Zx.P(34);

	public FenCompte(bool nouveau, Dictionary<string, string> noms, int ongletInitial = 0)
	{
		this.noms = noms;
		onglet = ongletInitial;
		creation = nouveau || Comptes.Actif == null;
		fermerApres = creation;
		Text = "Mon profil";
		base.FormBorderStyle = FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		Theme.Fenetre(this);
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
		Construire();
		if (Social.Actif && Social.Connecte && Social.Vip)
		{
			Thread tv = new Thread((ThreadStart)delegate
			{
				try
				{
					Social.EtatPoints();
					BeginInvoke((MethodInvoker)delegate
					{
						if (!base.IsDisposed && !creation && onglet == 3)
						{
							Construire();
						}
						else if (!base.IsDisposed && apercu != null)
						{
							apercu.Invalidate();
						}
					});
				}
				catch
				{
				}
			});
			tv.IsBackground = true;
			base.HandleCreated += delegate
			{
				tv.Start();
			};
		}
		System.Windows.Forms.Timer ta = new System.Windows.Forms.Timer();
		ta.Interval = 40;
		ta.Tick += delegate
		{
			Compte actif = Comptes.Actif;
			if (actif != null && apercu != null && !apercu.IsDisposed)
			{
				apercu.Invalidate();
				if (miniature != null && !miniature.IsDisposed)
				{
					miniature.Invalidate();
				}
				if (onglet == 2 && zSucces != null && !zSucces.IsDisposed)
				{
					zSucces.Invalidate(new Rectangle(0, 0, zSucces.Width, Zx.P(200)));
				}
				if (onglet == 3 && zVip != null && !zVip.IsDisposed)
				{
					zVip.Invalidate();
				}
			}
		};
		ta.Start();
		base.Disposed += delegate
		{
			ta.Stop();
			ta.Dispose();
		};
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
		hote.Controls.Add(label);
		return label;
	}

	private Bouton Bt(string t, int x, int y, int w, bool plein, EventHandler h)
	{
		Bouton bouton = new Bouton(t);
		bouton.Plein = plein;
		bouton.Size = new Size(w, Zx.P(38));
		bouton.Location = new Point(x, y);
		bouton.Click += h;
		hote.Controls.Add(bouton);
		return bouton;
	}

	private void Saisie(int y, string pseudo, int couleur)
	{
		tPseudo = new Champ(multi: false, Zx.P(38));
		tPseudo.Location = new Point(Zx.P(24), y);
		tPseudo.Width = Zx.P(320);
		tPseudo.MaxLength = 20;
		tPseudo.Text = pseudo;
		hote.Controls.Add(tPseudo);
		pal = new PaletteCouleurs();
		pal.Index = couleur;
		pal.Location = new Point(Zx.P(18), y + Zx.P(48));
		hote.Controls.Add(pal);
		lErr = Lb("", Zx.P(24), y + Zx.P(92), Zx.P(512), Zx.P(20), new Font("Segoe UI", Zx.P(9f)), C.Rouge);
	}

	private Label Etiq(Control parent, string t, int x, int y)
	{
		Label label = new Label();
		label.Text = t;
		label.Font = new Font("Segoe UI", Zx.P(9f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(x, y);
		label.AutoSize = true;
		parent.Controls.Add(label);
		return label;
	}

	private Champ ChampBloc(Cadre c, string etiquette, int x, int y, int w, int h, bool multi, int max, string val)
	{
		Etiq(c, etiquette, x, y);
		Champ champ = new Champ(multi, h);
		champ.Location = new Point(x, y + Zx.P(20));
		champ.Width = w;
		champ.MaxLength = max;
		champ.Text = val ?? "";
		champ.TextChanged += delegate
		{
			if (apercu != null)
			{
				apercu.Invalidate();
			}
		};
		c.Controls.Add(champ);
		return champ;
	}

	private void Construire()
	{
		base.Controls.Clear();
		hote = this;
		Compte a = Comptes.Actif;
		if (creation)
		{
			base.ClientSize = Integre.Hote;
			BackColor = C.Cote;
			Panel carte = new Panel();
			carte.BackColor = C.Fond;
			carte.Size = new Size(Zx.P(560), Zx.P(340));
			carte.Paint += delegate(object oc, PaintEventArgs ec)
			{
				using (Pen pen = new Pen(C.Ligne))
				{
					ec.Graphics.DrawRectangle(pen, 0, 0, carte.Width - 1, carte.Height - 1);
				}
				using SolidBrush brush2 = new SolidBrush(C.Jade);
				ec.Graphics.FillRectangle(brush2, 0, 0, carte.Width, Zx.P(3));
			};
			carte.Location = new Point((base.ClientSize.Width - Zx.P(560)) / 2, Math.Max(Zx.P(10), (base.ClientSize.Height - Zx.P(340)) / 2));
			base.Controls.Add(carte);
			hote = carte;
			Lb("Créer un profil", Zx.P(24), Zx.P(20), Zx.P(512), Zx.P(30), new Font("Segoe UI Semibold", Zx.P(16f)), C.Texte);
			Lb("Un profil garde votre pseudo et votre historique de jeu sur ce PC. Il n'y a pas de mot de passe et rien n'est envoyé sur Internet.", Zx.P(24), Zx.P(58), Zx.P(512), Zx.P(44), Font, C.Gris);
			Lb("Pseudo", Zx.P(24), Zx.P(116), Zx.P(300), Zx.P(20), new Font("Segoe UI Semibold", Zx.P(10f)), C.Texte);
			Saisie(Zx.P(140), "", Comptes.Tous.Count % Comptes.Palette.Length);
			Bt("Annuler", Zx.P(24), Zx.P(286), Zx.P(120), plein: false, delegate
			{
				if (Comptes.Actif == null || fermerApres)
				{
					base.DialogResult = (change ? DialogResult.OK : DialogResult.Cancel);
				}
				else
				{
					creation = false;
					Construire();
				}
			});
			Bt("Créer le profil", Zx.P(426), Zx.P(286), Zx.P(110), plein: true, delegate
			{
				string text2 = ((Comptes.Tous.Count >= Zx.P(6)) ? "6 profils au maximum sur ce PC." : Comptes.Valider(tPseudo.Text, null));
				if (text2 != null)
				{
					lErr.Text = text2;
				}
				else
				{
					Comptes.Creer(tPseudo.Text, pal.Index);
					change = true;
					if (fermerApres)
					{
						base.DialogResult = DialogResult.OK;
					}
					else
					{
						creation = false;
						Construire();
					}
				}
			});
			tPseudo.Focus();
			return;
		}
		base.ClientSize = Integre.Hote;
		BackColor = C.Fond;
		CW = Math.Max(Zx.P(900), Math.Min(base.ClientSize.Width - Zx.P(48), Zx.P(2200)));
		X0 = (base.ClientSize.Width - CW) / 2;
		Panel panel = new Panel();
		panel.Dock = DockStyle.Bottom;
		panel.Height = Zx.P(70);
		panel.BackColor = C.Cote;
		base.Controls.Add(panel);
		lErr = new Label();
		lErr.Font = new Font("Segoe UI", Zx.P(9.5f));
		lErr.ForeColor = C.Gris;
		lErr.BackColor = C.Cote;
		lErr.AutoSize = false;
		lErr.TextAlign = ContentAlignment.MiddleLeft;
		lErr.Location = new Point(X0 + Zx.P(28), Zx.P(16));
		lErr.Size = new Size(Math.Max(Zx.P(200), CW - Zx.P(56) - Zx.P(200)), Zx.P(38));
		panel.Controls.Add(lErr);
		lErr.Text = "La photo est enregistrée tout de suite ; le reste avec « Enregistrer ». Rien n'est envoyé sur Internet.";
		if (retour.Length > 0)
		{
			lErr.ForeColor = C.Vert;
			lErr.Text = retour;
			retour = "";
		}
		Bouton bouton = new Bouton("Enregistrer");
		bouton.Size = new Size(Zx.P(170), Zx.P(40));
		bouton.Location = new Point(X0 + CW - Zx.P(28) - Zx.P(170), Zx.P(15));
		panel.Controls.Add(bouton);
		Panel panel2 = new Panel();
		panel2.Dock = DockStyle.Top;
		panel2.Height = Zx.P(78);
		panel2.BackColor = C.Fond;
		base.Controls.Add(panel2);
		panel2.Controls.Add(Integre.Retour(delegate
		{
			base.DialogResult = (change ? DialogResult.OK : DialogResult.Cancel);
		}, X0 + Zx.P(24), Zx.P(15)));
		Label label = new Label();
		label.Text = "Mon profil";
		label.Font = new Font("Segoe UI Semibold", Zx.P(18f));
		label.ForeColor = C.Texte;
		label.BackColor = C.Fond;
		label.Location = new Point(X0 + Zx.P(84), Zx.P(22));
		label.AutoSize = true;
		panel2.Controls.Add(label);
		contenu = new Panel();
		contenu.Dock = DockStyle.Fill;
		contenu.BackColor = C.Fond;
		base.Controls.Add(contenu);
		contenu.BringToFront();
		int num = base.ClientSize.Height - 78 - 70;
		int num2 = 360;
		int num3 = X0 + 24 + num2 + 24;
		int num4 = CW - 48 - num2 - 24;
		apercu = new ZoneDessin();
		apercu.Location = new Point(X0 + Zx.P(24), Zx.P(4));
		apercu.Size = new Size(num2, Math.Min(num - Zx.P(18), Zx.P(590)));
		apercu.Dessiner = DessinerApercu;
		contenu.Controls.Add(apercu);
		Segments tabs = new Segments();
		tabs.Items = new string[5] { "Profil et apparence", "Succès", "VIP", "Mes mods", "Profils de ce PC" };
		tabs.Etire = true;
		tabs.Location = new Point(num3, Zx.P(4));
		tabs.Width = num4;
		tabs.BackColor = C.Fond;
		contenu.Controls.Add(tabs);
		pages = new Panel[6];
		int num5 = (num4 - 2 * Zx.P(16)) / 3;
		for (int i = 0; i < 6; i++)
		{
			Panel panel3 = new Panel();
			panel3.Location = new Point(num3, Zx.P(52));
			panel3.Size = new Size(num4, num - Zx.P(66));
			panel3.BackColor = C.Fond;
			panel3.AutoScroll = i == 1 || i == 2;
			panel3.Visible = false;
			contenu.Controls.Add(panel3);
			Theme.Sombre(panel3);
			pages[i] = panel3;
		}
		pages[0].Width = 2 * num5 + Zx.P(16);
		pages[0].AutoScroll = true;
		pages[1].Location = new Point(num3 + 2 * num5 + Zx.P(32), Zx.P(52));
		pages[1].Width = num4 - 2 * num5 - Zx.P(32);
		Cadre cadre = new Cadre();
		cadre.Titre = "Informations";
		cadre.Location = new Point(0, 0);
		cadre.Size = new Size(num5, Zx.P(396));
		pages[0].Controls.Add(cadre);
		int num6 = (num5 - 48 - 16) / 2;
		tPseudo = ChampBloc(cadre, "Pseudo", Zx.P(24), Zx.P(44), num6, Zx.P(36), multi: false, 20, a.Pseudo);
		tStatut = ChampBloc(cadre, "Statut (60 caractères maximum)", Zx.P(24) + num6 + Zx.P(16), Zx.P(44), num6, Zx.P(36), multi: false, 60, a.Statut);
		tBio = ChampBloc(cadre, "Bio (240 caractères maximum)", Zx.P(24), Zx.P(112), num5 - Zx.P(48), Zx.P(90), multi: true, 240, a.Bio);
		tJeu = ChampBloc(cadre, "Carte ou jeu favori", Zx.P(24), Zx.P(238), num6, Zx.P(36), multi: false, 40, a.JeuFavori);
		tLieu = ChampBloc(cadre, "Lieu (facultatif)", Zx.P(24) + num6 + Zx.P(16), Zx.P(238), num6, Zx.P(36), multi: false, 40, a.Lieu);
		Etiq(cadre, "Photo de profil", Zx.P(24), Zx.P(306));
		Bouton bouton2 = new Bouton("Choisir une image…");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(176), Zx.P(40));
		bouton2.Location = new Point(Zx.P(24), Zx.P(328));
		cadre.Controls.Add(bouton2);
		bouton2.Click += delegate
		{
			using OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
			openFileDialog.Title = "Photo de profil";
			if (openFileDialog.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}
			try
			{
				Comptes.DefinirAvatar(Comptes.Actif, openFileDialog.FileName);
				change = true;
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  Photo de profil enregistrée";
				rcPhoto.Couleurs = Comptes.CouleursPhoto(Comptes.ImageAvatar(Comptes.Actif)).ToArray();
				rcPhoto.Invalidate();
				if (Comptes.Actif.AutoCouleurs)
				{
					AppliquerAuto(Comptes.Actif);
				}
				apercu.Invalidate();
				miniature.Invalidate();
				lb.Invalidate();
			}
			catch (Exception ex)
			{
				lErr.ForeColor = C.Rouge;
				lErr.Text = "Photo refusée : " + ex.Message;
			}
		};
		Bouton bouton3 = new Bouton("Retirer la photo");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(150), Zx.P(40));
		bouton3.Location = new Point(Zx.P(208), Zx.P(328));
		cadre.Controls.Add(bouton3);
		bouton3.Click += delegate
		{
			Comptes.RetirerAvatar(Comptes.Actif);
			change = true;
			rcPhoto.Couleurs = new Color[0];
			rcPhoto.Invalidate();
			apercu.Invalidate();
			lb.Invalidate();
		};
		bouton.Click += delegate
		{
			Compte actif5 = Comptes.Actif;
			string text = Comptes.Valider(tPseudo.Text, actif5.Id);
			if (text != null)
			{
				lErr.ForeColor = C.Rouge;
				lErr.Text = text;
				onglet = 0;
				tabs.Index = 0;
			}
			else
			{
				Comptes.Modifier(actif5, tPseudo.Text, actif5.Couleur);
				actif5.Statut = tStatut.Text.Trim();
				actif5.Bio = tBio.Text.Trim();
				actif5.JeuFavori = tJeu.Text.Trim();
				actif5.Lieu = tLieu.Text.Trim();
				actif5.CouleurPerso = selAvatar.Couleur.ToArgb();
				Comptes.Sauvegarder();
				change = true;
				retour = "✔  Profil enregistré";
				Construire();
			}
		};
		pages[1].AutoScroll = true;
		int num7 = pages[1].Width - SystemInformation.VerticalScrollBarWidth - 4;
		Cadre cadre2 = new Cadre();
		cadre2.Titre = "Bannière";
		cadre2.Location = new Point(0, 0);
		cadre2.Width = num7;
		pages[1].Controls.Add(cadre2);
		ZoneDessin gal = new ZoneDessin();
		gal.Location = new Point(Zx.P(16), Zx.P(44));
		gal.Width = num7 - Zx.P(32);
		gal.Height = HauteurGalerie(gal.Width);
		gal.Cursor = Cursors.Hand;
		cadre2.Controls.Add(gal);
		miniature = gal;
		gal.Dessiner = delegate(Graphics g, int w, int h)
		{
			DessinerGalerie(g, w);
		};
		gal.MouseDown += delegate(object o, MouseEventArgs e)
		{
			int num8 = CaseGalerie(gal.Width, e.Location);
			if (num8 >= 0)
			{
				Sons.Clic();
				if (!BanAnim.Disponible(BanAnim.Ids[num8]))
				{
					lErr.ForeColor = C.Or;
					lErr.Text = (Social.Vip ? ("« " + BanAnim.Noms[num8] + " » s'achète dans la Boutique (" + BanAnim.PrixPoints(BanAnim.Ids[num8]) + " points).") : ("« " + BanAnim.Noms[num8] + " » est réservée aux membres VIP : voir l'onglet VIP."));
				}
				else
				{
					Compte actif4 = Comptes.Actif;
					actif4.BanType = "anim";
					actif4.BanMotif = BanAnim.Ids[num8];
					Comptes.Sauvegarder();
					change = true;
					try
					{
						Progression.Verifier(actif4);
					}
					catch
					{
					}
					lErr.ForeColor = C.Vert;
					lErr.Text = "✔  Bannière « " + BanAnim.Noms[num8] + " » appliquée";
					apercu.Invalidate();
					gal.Invalidate();
				}
			}
		};
		Bouton bouton4 = new Bouton("Dégradé, couleur unie ou image…");
		bouton4.Plein = false;
		bouton4.Size = new Size(Math.Min(Zx.P(280), num7 - Zx.P(32)), Zx.P(38));
		bouton4.Location = new Point(Zx.P(16), gal.Bottom + Zx.P(12));
		cadre2.Controls.Add(bouton4);
		bouton4.Click += delegate
		{
			using FenBanniere f2 = new FenBanniere(Comptes.Actif);
			if (f2.Afficher(this) == DialogResult.OK)
			{
				change = true;
				apercu.Invalidate();
				gal.Invalidate();
			}
		};
		Bouton bouton5 = new Bouton("Bannière par défaut");
		bouton5.Plein = false;
		bouton5.Size = new Size(Math.Min(Zx.P(280), num7 - Zx.P(32)), Zx.P(38));
		bouton5.Location = new Point(Zx.P(16), bouton4.Bottom + Zx.P(8));
		cadre2.Controls.Add(bouton5);
		bouton5.Click += delegate
		{
			Compte actif3 = Comptes.Actif;
			actif3.BanType = "";
			Comptes.RetirerBanniereImage(actif3);
			Comptes.Sauvegarder();
			change = true;
			apercu.Invalidate();
			gal.Invalidate();
		};
		cadre2.Height = bouton5.Bottom + Zx.P(18);
		Cadre cadre3 = new Cadre();
		cadre3.Titre = "Couleurs";
		cadre3.Location = new Point(num5 + Zx.P(16), 0);
		cadre3.Size = new Size(num5, Zx.P(390));
		pages[0].Controls.Add(cadre3);
		int width = num5 - 48;
		int x = Zx.P(24);
		oAuto = new Option("Couleurs d'après ma photo", "Avatar et bannière suivent la photo.", a.AutoCouleurs);
		oAuto.Location = new Point(Zx.P(8), Zx.P(38));
		oAuto.Width = num5 - Zx.P(16);
		cadre3.Controls.Add(oAuto);
		oAuto.Changed += delegate
		{
			Compte actif2 = Comptes.Actif;
			actif2.AutoCouleurs = oAuto.Checked;
			Comptes.Sauvegarder();
			change = true;
			if (oAuto.Checked && !AppliquerAuto(actif2))
			{
				lErr.ForeColor = C.Orange;
				lErr.Text = "Importez d'abord une photo de profil : ses couleurs seront appliquées automatiquement.";
			}
		};
		Etiq(cadre3, "Couleurs de votre photo (cliquez pour choisir)", Zx.P(24), Zx.P(112));
		rcPhoto = new RangeeCouleurs();
		rcPhoto.Location = new Point(Zx.P(24), Zx.P(134));
		rcPhoto.Size = new Size(width, Zx.P(40));
		cadre3.Controls.Add(rcPhoto);
		rcPhoto.Couleurs = Comptes.CouleursPhoto(Comptes.ImageAvatar(a)).ToArray();
		Etiq(cadre3, "Couleur d'avatar (sans photo)", x, Zx.P(192));
		selAvatar = new SelecteurCouleur();
		selAvatar.Location = new Point(x, Zx.P(214));
		selAvatar.Width = width;
		cadre3.Controls.Add(selAvatar);
		selAvatar.Couleur = Comptes.CouleurAvatar(a);
		selAvatar.Changed += delegate
		{
			apercu.Invalidate();
			lb.Invalidate();
		};
		rcPhoto.Choisie += delegate(Color cl)
		{
			selAvatar.Couleur = cl;
			apercu.Invalidate();
			lb.Invalidate();
		};
		cadre3.Height = selAvatar.Bottom + Zx.P(20);
		int num40 = Math.Max(cadre.Height, cadre3.Height);
		cadre.Height = num40;
		cadre3.Height = num40;
		apercu.Height = Math.Min(num - Zx.P(18), Zx.P(590));
		Progression.Init(a);
		Progression.Verifier(a);
		zSucces = new ZoneDessin();
		zSucces.Location = new Point(0, 0);
		zSucces.Width = num4 - SystemInformation.VerticalScrollBarWidth - Zx.P(2);
		zSucces.Height = Zx.P(236) + (Progression.Vitrine(a).Count + 1) / 2 * 106 + 8;
		zSucces.Dessiner = DessinerSucces;
		pages[2].Controls.Add(zSucces);
		ConstruireVip(pages[3], num4, a);
		Cadre cadre4 = new Cadre();
		cadre4.Titre = "Mes profils sur ce PC  ·  " + Comptes.Tous.Count + " / 6  ·  chacun a son niveau, ses succès et son style";
		cadre4.Location = new Point(0, 0);
		cadre4.Size = new Size(num4, Zx.P(262));
		pages[4].Controls.Add(cadre4);
		lb = new ListBox();
		lb.Location = new Point(Zx.P(16), Zx.P(46));
		lb.Size = new Size(num4 - Zx.P(32) - Zx.P(290), Zx.P(200));
		lb.DrawMode = DrawMode.OwnerDrawFixed;
		lb.ItemHeight = Zx.P(46);
		lb.BorderStyle = BorderStyle.None;
		lb.BackColor = C.Carte;
		lb.ForeColor = C.Texte;
		lb.IntegralHeight = false;
		Theme.Sombre(lb);
		foreach (Compte tou in Comptes.Tous)
		{
			lb.Items.Add(tou.Pseudo);
			if (tou.Id == a.Id)
			{
				lb.SelectedIndex = lb.Items.Count - 1;
			}
		}
		lb.DrawItem += delegate(object o, DrawItemEventArgs e)
		{
			if (e.Index >= 0 && e.Index < Comptes.Tous.Count)
			{
				Compte compte2 = Comptes.Tous[e.Index];
				bool flag = (e.State & DrawItemState.Selected) != 0;
				Graphics graphics = e.Graphics;
				using (SolidBrush brush = new SolidBrush(flag ? C.CarteSurvol : C.Carte))
				{
					graphics.FillRectangle(brush, e.Bounds);
				}
				Color couleur = ((compte2.Id == a.Id && selAvatar != null) ? selAvatar.Couleur : Comptes.CouleurAvatar(compte2));
				Comptes.AvatarBrut(graphics, new Rectangle(e.Bounds.X + Zx.P(8), e.Bounds.Y + Zx.P(7), Zx.P(32), Zx.P(32)), Comptes.ImageAvatar(compte2), compte2.Pseudo, couleur, 10f);
				Lang.DrawText(graphics, compte2.Pseudo + ((compte2.Id == Comptes.ActifId) ? "   (actif)" : ""), new Font("Segoe UI Semibold", Zx.P(10f)), new Point(e.Bounds.X + Zx.P(50), e.Bounds.Y + Zx.P(6)), C.Texte);
				Lang.DrawText(graphics, "Niveau " + Progression.NiveauDe(compte2) + ((compte2.Statut.Length > 0) ? ("  ·  " + compte2.Statut) : ""), F.Etat, new Rectangle(e.Bounds.X + Zx.P(50), e.Bounds.Y + Zx.P(25), e.Bounds.Width - Zx.P(58), Zx.P(16)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			}
		};
		cadre4.Controls.Add(lb);
		int x2 = num4 - Zx.P(16) - Zx.P(270);
		Bouton bouton6 = new Bouton("Utiliser ce profil");
		bouton6.Size = new Size(Zx.P(270), Zx.P(38));
		bouton6.Location = new Point(x2, Zx.P(46));
		cadre4.Controls.Add(bouton6);
		bouton6.Click += delegate
		{
			if (lb.SelectedIndex >= 0)
			{
				Comptes.Choisir(Comptes.Tous[lb.SelectedIndex].Id);
				change = true;
				Construire();
			}
		};
		Bouton bouton7 = new Bouton("Nouveau profil");
		bouton7.Plein = false;
		bouton7.Size = new Size(Zx.P(270), Zx.P(38));
		bouton7.Location = new Point(x2, Zx.P(92));
		cadre4.Controls.Add(bouton7);
		bouton7.Click += delegate
		{
			if (Comptes.Tous.Count >= 6)
			{
				lErr.ForeColor = C.Orange;
				lErr.Text = "6 profils au maximum sur ce PC : supprimez-en un pour en créer un nouveau.";
			}
			else
			{
				creation = true;
				fermerApres = false;
				Construire();
			}
		};
		Bouton bouton8 = new Bouton("Supprimer ce profil");
		bouton8.Plein = false;
		bouton8.Danger = true;
		bouton8.Size = new Size(Zx.P(270), Zx.P(38));
		bouton8.Location = new Point(x2, Zx.P(138));
		cadre4.Controls.Add(bouton8);
		bouton8.Click += delegate
		{
			if (lb.SelectedIndex >= 0)
			{
				Compte compte = Comptes.Tous[lb.SelectedIndex];
				if (MessageBox.Show(this, "Supprimer le profil « " + compte.Pseudo + " » ?\n\nSon historique de jeu, sa photo et sa bannière sont perdus. Vos mods installés ne sont pas touchés.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Comptes.RetirerAvatar(compte);
					Comptes.RetirerBanniereImage(compte);
					Comptes.Supprimer(compte.Id);
					change = true;
					creation = Comptes.Actif == null;
					fermerApres = creation;
					Construire();
				}
			}
		};
		Bouton bouton9 = new Bouton((a.Admin && GitHub.JetonPresent) ? "Administrateur ✔  (désactiver)" : "Mode administrateur…");
		bouton9.Plein = false;
		bouton9.Size = new Size(Zx.P(270), Zx.P(38));
		bouton9.Location = new Point(x2, Zx.P(184));
		cadre4.Controls.Add(bouton9);
		bouton9.Click += delegate
		{
			Compte actif = Comptes.Actif;
			if (actif.Admin && GitHub.JetonPresent)
			{
				if (MessageBox.Show(this, "Désactiver le mode administrateur sur ce PC ?\n\nLe jeton GitHub enregistré est supprimé.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					GitHub.OublierJeton();
					Comptes.DefinirAdmin(actif, oui: false);
					change = true;
					Construire();
				}
				return;
			}
			using FenJeton f = new FenJeton();
			if (f.Afficher(this) == DialogResult.OK)
			{
				Comptes.DefinirAdmin(actif, oui: true);
				change = true;
				Construire();
			}
		};
		ConstruireMods(pages[5], num4);
		int[] ids = new int[5] { 0, 2, 3, 5, 4 };
		if (onglet < 0 || onglet > 5 || onglet == 1)
		{
			onglet = 0;
		}
		tabs.Index = Math.Max(0, Array.IndexOf(ids, onglet));
		Action montrer = delegate
		{
			for (int j = 0; j < pages.Length; j++)
			{
				pages[j].Visible = j == onglet || (onglet == 0 && j == 1);
			}
		};
		montrer();
		tabs.Changed += delegate
		{
			onglet = ids[Math.Max(0, tabs.Index)];
			montrer();
		};
	}

	private void ConstruireMods(Panel pg, int RW)
	{
		pg.Controls.Clear();
		pg.AutoScroll = false;
		string omsi = Omsi.DetecteOmsi();
		List<Projet> i = new List<Projet>();
		if (omsi != null)
		{
			foreach (Projet projetsConnu in Fenetre.ProjetsConnus)
			{
				if (projetsConnu.Liste.Length > 0 && Omsi.Installee(projetsConnu, omsi) != null)
				{
					i.Add(projetsConnu);
				}
			}
		}
		HashSet<string> choisis = new HashSet<string>();
		Label lInfo = new Label();
		lInfo.Font = new Font("Segoe UI", Zx.P(9.5f));
		lInfo.ForeColor = C.Gris;
		lInfo.BackColor = C.Fond;
		lInfo.Location = new Point(Zx.P(2), Zx.P(52));
		lInfo.Size = new Size(RW - Zx.P(4), Zx.P(22));
		pg.Controls.Add(lInfo);
		Panel panel = new Panel();
		panel.Location = new Point(0, Zx.P(84));
		panel.Size = new Size(RW, pg.Height - Zx.P(84));
		panel.AutoScroll = true;
		panel.BackColor = C.Fond;
		pg.Controls.Add(panel);
		Theme.Sombre(panel);
		ZoneDessin z = new ZoneDessin();
		z.Location = new Point(0, 0);
		z.Size = new Size(RW - SystemInformation.VerticalScrollBarWidth - Zx.P(4), Math.Max(Zx.P(80), i.Count * Zx.P(66)));
		panel.Controls.Add(z);
		z.Cursor = Cursors.Hand;
		Action maj = delegate
		{
			int num4 = 0;
			foreach (Projet item in i)
			{
				if (!Omsi.Desactive(item, omsi))
				{
					num4++;
				}
			}
			lInfo.Text = ((omsi == null) ? "Dossier d'OMSI 2 introuvable : indiquez-le dans Réglages." : (i.Count + ((i.Count > 1) ? " mods installés" : " mod installé") + "  ·  " + num4 + ((num4 > 1) ? " activés" : " activé") + "  ·  " + choisis.Count + " sélectionné(s)  ·  désactiver range les fichiers à part, sans rien supprimer"));
			z.Invalidate();
		};
		z.Dessiner = delegate(Graphics g, int w, int h)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			if (i.Count == 0)
			{
				Lang.DrawText(g, (omsi == null) ? "" : "Aucun mod TPS Groupe installé pour l'instant.", fModN, new Rectangle(0, 0, w, Zx.P(60)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
			else
			{
				for (int j = 0; j < i.Count; j++)
				{
					Projet projet2 = i[j];
					bool flag2 = Omsi.Desactive(projet2, omsi);
					bool flag3 = choisis.Contains(projet2.Id);
					Rectangle r = new Rectangle(0, j * Zx.P(66), w - 1, Zx.P(58));
					using (GraphicsPath path = Bouton.Rond(r, Zx.P(10)))
					{
						using (SolidBrush brush = new SolidBrush(flag3 ? C.CarteSurvol : C.Carte))
						{
							g.FillPath(brush, path);
						}
						using Pen pen = new Pen(flag3 ? Color.FromArgb(160, C.Jade) : C.Ligne);
						g.DrawPath(pen, path);
					}
					Rectangle r2 = new Rectangle(Zx.P(16), r.Y + Zx.P(19), Zx.P(20), Zx.P(20));
					using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(5)))
					{
						using (SolidBrush brush2 = new SolidBrush(flag3 ? C.Jade : C.Champ))
						{
							g.FillPath(brush2, path2);
						}
						using Pen pen2 = new Pen(flag3 ? C.Jade : C.Gris);
						g.DrawPath(pen2, path2);
					}
					if (flag3)
					{
						using Pen pen3 = new Pen(Color.White, Zx.P(2.2f));
						g.DrawLines(pen3, new Point[3]
						{
							new Point(r2.X + Zx.P(5), r2.Y + Zx.P(10)),
							new Point(r2.X + Zx.P(9), r2.Y + Zx.P(14)),
							new Point(r2.X + Zx.P(15), r2.Y + Zx.P(6))
						});
					}
					Lang.DrawText(g, projet2.Nom, fModN, new Rectangle(Zx.P(52), r.Y + Zx.P(9), w - Zx.P(52) - Zx.P(200), Zx.P(22)), flag2 ? C.Gris : C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
					Lang.DrawText(g, ((projet2.Version.Length > 0) ? projet2.Version : "—") + "  ·  " + Categories.Nom(projet2.Categorie) + ((projet2.Taille > 0) ? ("  ·  " + projet2.Taille / 1048576 + " Mo") : ""), fModS, new Rectangle(Zx.P(52), r.Y + Zx.P(32), w - Zx.P(52) - Zx.P(200), Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
					Lang.DrawText(g, flag2 ? "Désactivé" : "Activé", fModS, new Rectangle(w - Zx.P(200), r.Y, Zx.P(116), r.Height), flag2 ? C.Orange : C.Vert, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
					Rectangle r3 = new Rectangle(w - Zx.P(16) - Zx.P(50), r.Y + Zx.P(17), Zx.P(50), Zx.P(24));
					using (GraphicsPath path3 = Bouton.Rond(r3, Zx.P(12)))
					{
						using SolidBrush brush3 = new SolidBrush(flag2 ? Color.FromArgb(52, 68, 96) : C.Jade);
						g.FillPath(brush3, path3);
					}
					using SolidBrush brush4 = new SolidBrush(Color.White);
					g.FillEllipse(brush4, flag2 ? (r3.X + Zx.P(3)) : (r3.Right - Zx.P(21)), r3.Y + Zx.P(3), Zx.P(18), Zx.P(18));
				}
			}
		};
		Func<Projet, bool, bool> basculer = delegate(Projet p, bool activer)
		{
			if (Omsi.Desactive(p, omsi) != activer)
			{
				return true;
			}
			if (Omsi.JeuLance())
			{
				lErr.ForeColor = C.Orange;
				lErr.Text = "Fermez OMSI 2 / openOMSI avant d'activer ou de désactiver un mod.";
				return false;
			}
			try
			{
				base.UseWaitCursor = true;
				Omsi.Basculer(p, omsi, activer);
				return true;
			}
			catch (Exception ex)
			{
				lErr.ForeColor = C.Rouge;
				lErr.Text = "« " + p.Nom + " » : " + ex.Message;
				Journal.Une("mods-basculer", ex);
				return false;
			}
			finally
			{
				base.UseWaitCursor = false;
			}
		};
		z.MouseDown += delegate(object o, MouseEventArgs e)
		{
			int num3 = e.Y / 66;
			if (num3 >= 0 && num3 < i.Count && e.Y % 66 <= 58)
			{
				Projet projet = i[num3];
				Sons.Clic();
				if (e.X > z.Width - 90)
				{
					bool flag = Omsi.Desactive(projet, omsi);
					if (basculer(projet, flag))
					{
						lErr.ForeColor = C.Vert;
						lErr.Text = "✔  « " + projet.Nom + " » " + (flag ? "activé" : "désactivé");
					}
				}
				else if (!choisis.Remove(projet.Id))
				{
					choisis.Add(projet.Id);
				}
				maj();
			}
		};
		Func<bool, string> liste = delegate(bool seulSel)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Mes mods TPS Groupe (" + DateTime.Now.ToString("dd/MM/yyyy") + ")");
			foreach (Projet item2 in i)
			{
				if (!seulSel || choisis.Contains(item2.Id))
				{
					stringBuilder.AppendLine("• " + item2.Nom + "  —  " + ((item2.Version.Length > 0) ? item2.Version : "?") + "  —  " + (Omsi.Desactive(item2, omsi) ? "désactivé" : "activé"));
				}
			}
			return stringBuilder.ToString().TrimEnd();
		};
		int bx = 0;
		Func<string, int, bool, EventHandler, Bouton> func = delegate(string t, int w0, bool plein, EventHandler h)
		{
			Bouton bouton = new Bouton(t)
			{
				Plein = plein
			};
			w0 = Math.Max(Zx.P(w0), Lang.MeasureText(t, new Font("Segoe UI Semibold", Zx.P(10.5f))).Width + Zx.P(30));
			bouton.Size = new Size(w0, Zx.P(38));
			bouton.Location = new Point(bx, 0);
			bouton.Click += h;
			pg.Controls.Add(bouton);
			bx += w0 + Zx.P(8);
			return bouton;
		};
		func("Tout activer", 132, arg3: true, delegate
		{
			int num2 = 0;
			foreach (Projet item3 in i)
			{
				if (basculer(item3, arg2: true))
				{
					num2++;
				}
			}
			if (num2 == i.Count)
			{
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  Tous les mods sont activés";
			}
			maj();
		});
		func("Tout désactiver", 146, arg3: false, delegate
		{
			if (i.Count != 0 && MessageBox.Show(this, "Désactiver les " + i.Count + " mods installés ?\n\nLeurs fichiers sont rangés à part (rien n'est supprimé) et reviennent à leur place quand vous les réactivez.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				int num = 0;
				foreach (Projet item4 in i)
				{
					if (basculer(item4, arg2: false))
					{
						num++;
					}
				}
				if (num == i.Count)
				{
					lErr.ForeColor = C.Vert;
					lErr.Text = "✔  Tous les mods sont désactivés";
				}
				maj();
			}
		});
		func("Tout sélectionner", 150, arg3: false, delegate
		{
			if (choisis.Count == i.Count)
			{
				choisis.Clear();
			}
			else
			{
				foreach (Projet item5 in i)
				{
					choisis.Add(item5.Id);
				}
			}
			maj();
		});
		func("Copier la sélection", 160, arg3: false, delegate
		{
			if (choisis.Count == 0)
			{
				lErr.ForeColor = C.Orange;
				lErr.Text = "Cochez d'abord un ou plusieurs mods.";
				return;
			}
			try
			{
				Clipboard.SetText(liste(arg: true));
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  " + choisis.Count + " mod(s) copié(s) dans le presse-papiers";
			}
			catch
			{
			}
		});
		func("Copier toute la liste", 172, arg3: false, delegate
		{
			if (i.Count == 0)
			{
				return;
			}
			try
			{
				Clipboard.SetText(liste(arg: false));
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  Liste des " + i.Count + " mods copiée dans le presse-papiers";
			}
			catch
			{
			}
		});
		maj();
	}

	private void ConstruireVip(Panel pg, int RW, Compte a)
	{
		pg.AutoScroll = true;
		int num = RW - SystemInformation.VerticalScrollBarWidth - 4;
		ZoneDessin zoneDessin = new ZoneDessin();
		zoneDessin.Location = new Point(0, 0);
		zoneDessin.Size = new Size(num, Zx.P(140));
		pg.Controls.Add(zoneDessin);
		zVip = zoneDessin;
		zoneDessin.Dessiner = delegate(Graphics g, int w, int h)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			Rectangle rectangle = new Rectangle(0, 0, w - 1, h - 1);
			double temps = BanAnim.Temps;
			using (GraphicsPath path = Bouton.Rond(rectangle, Zx.P(16)))
			{
				GraphicsState gstate = g.Save();
				g.SetClip(path, CombineMode.Intersect);
				BanAnim.Dessiner("or", g, rectangle, temps);
				using (LinearGradientBrush brush = new LinearGradientBrush(rectangle, Color.FromArgb(230, 12, 9, 4), Color.FromArgb(40, 12, 9, 4), 0f))
				{
					g.FillRectangle(brush, rectangle);
				}
				g.Restore(gstate);
				using Pen pen = new Pen(C.Or, 1.4f);
				g.DrawPath(pen, path);
			}
			Deco.Forcer(g, new Rectangle(Zx.P(34), Zx.P(40), Zx.P(60), Zx.P(60)), "couronne", temps);
			Lang.DrawText(g, Social.Vip ? "Membre VIP  ✔" : "TPS VIP  ·  5 € à vie", new Font("Segoe UI Semibold", Zx.P(16f)), new Point(Zx.P(118), Zx.P(26)), Color.FromArgb(255, 232, 170));
			string t = ((!Social.Actif) ? "Le serveur TPS n'est pas encore configuré." : (Social.Vip ? (Social.Points.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + " points VIP  ·  " + (Social.BonusPris ? "bonus du jour récupéré ✔" : "bonus du jour au prochain démarrage")) : "Soutenez TPS Groupe et profitez d'avantages exclusifs, sur tous les profils de ce PC."));
			Lang.DrawText(g, t, new Font("Segoe UI", Zx.P(10f)), new Rectangle(Zx.P(118), Zx.P(66), w - Zx.P(140), Zx.P(44)), Color.FromArgb(240, 230, 210), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		};
		int y = Zx.P(156);
		if (!Social.Actif)
		{
			return;
		}
		if (!Social.Vip)
		{
			string[] array = new string[7] { "Badge VIP doré sur votre profil et à côté de vos commentaires", "2 bannières animées exclusives : « Or VIP » et « Prestige TPS »", "Accès anticipé aux nouveaux mods et versions de test réservées aux testeurs VIP", "Points VIP : +50 par jour, +10 par mod aimé, +5 par commentaire (3 par jour)", "Boutique : dépensez vos points en cosmétiques (couronne, flammes, bannières) et en DLC", "Rôle VIP sur le Discord TPS Groupe", "+200 points de bienvenue" };
			Cadre cadre = new Cadre();
			cadre.Titre = "Avantages VIP";
			cadre.Location = new Point(0, y);
			cadre.Size = new Size(num, Zx.P(44) + array.Length * Zx.P(28) + Zx.P(12));
			pg.Controls.Add(cadre);
			for (int i = 0; i < array.Length; i++)
			{
				Label label = new Label();
				label.Text = "✦   " + array[i];
				label.Font = new Font("Segoe UI", Zx.P(9.5f));
				label.ForeColor = Color.FromArgb(242, 228, 196);
				label.BackColor = C.Carte;
				label.Location = new Point(Zx.P(24), Zx.P(44) + i * Zx.P(28));
				label.AutoSize = true;
				cadre.Controls.Add(label);
			}
			y = cadre.Bottom + Zx.P(16);
			Cadre cadre2 = new Cadre();
			cadre2.Titre = "Devenir VIP";
			cadre2.Location = new Point(0, y);
			cadre2.Size = new Size(num, Zx.P(176));
			pg.Controls.Add(cadre2);
			Bouton bouton = new Bouton("Devenir VIP · 5 €");
			bouton.Couleur = C.Or;
			bouton.Size = new Size(Zx.P(220), Zx.P(40));
			bouton.Location = new Point(Zx.P(24), Zx.P(44));
			cadre2.Controls.Add(bouton);
			bouton.Click += delegate
			{
				if (Uri.TryCreate(Fenetre.LienVip ?? "", UriKind.Absolute, out var result) && result.Scheme == Uri.UriSchemeHttps)
				{
					try
					{
						Process.Start(result.AbsoluteUri);
						return;
					}
					catch
					{
						return;
					}
				}
				lErr.ForeColor = C.Orange;
				lErr.Text = "Le lien d'achat VIP n'est pas encore configuré par TPS Groupe.";
			};
			Label label2 = new Label();
			label2.Text = "Après le paiement, TPS Groupe vous envoie un code VIP personnel, utilisable une seule fois.";
			label2.Font = new Font("Segoe UI", Zx.P(9f));
			label2.ForeColor = C.Gris;
			label2.BackColor = C.Carte;
			label2.Location = new Point(Zx.P(262), Zx.P(56));
			label2.AutoSize = true;
			cadre2.Controls.Add(label2);
			Etiq(cadre2, "Code VIP", Zx.P(24), Zx.P(100));
			Champ tCv = new Champ(multi: false, Zx.P(36));
			tCv.Location = new Point(Zx.P(24), Zx.P(122));
			tCv.Width = Zx.P(320);
			tCv.MaxLength = 40;
			tCv.Indice = "TPS-VIP-XXXX-XXXX-XXXX";
			cadre2.Controls.Add(tCv);
			Bouton bAct = new Bouton("Activer");
			bAct.Size = new Size(Zx.P(140), Zx.P(38));
			bAct.Location = new Point(Zx.P(356), Zx.P(121));
			cadre2.Controls.Add(bAct);
			bAct.Click += delegate
			{
				string code = tCv.Text.Trim();
				if (code.Length < 10)
				{
					lErr.ForeColor = C.Rouge;
					lErr.Text = "Saisissez le code VIP reçu après l'achat.";
				}
				else
				{
					bAct.Enabled = false;
					lErr.ForeColor = C.Gris;
					lErr.Text = "Vérification du code…";
					Compte cp = Comptes.Actif;
					Thread thread = new Thread((ThreadStart)delegate
					{
						string er = null;
						try
						{
							er = Social.Assurer(cp);
							if (er == null)
							{
								er = Social.ActiverVip(code);
							}
							if (er == null)
							{
								try
								{
									Social.EtatPoints();
								}
								catch
								{
								}
							}
						}
						catch (Exception ex)
						{
							er = ex.Message;
						}
						try
						{
							BeginInvoke((MethodInvoker)delegate
							{
								bAct.Enabled = true;
								if (er != null)
								{
									lErr.ForeColor = C.Rouge;
									lErr.Text = "✖  " + er;
								}
								else
								{
									retour = "✔  Bienvenue parmi les VIP ! +200 points de bienvenue";
									change = true;
									onglet = 3;
									Construire();
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
			};
			return;
		}
		Cadre cadre3 = new Cadre();
		cadre3.Titre = "Mes points VIP";
		cadre3.Location = new Point(0, y);
		cadre3.Size = new Size(num, Zx.P(150));
		pg.Controls.Add(cadre3);
		Label label3 = new Label();
		label3.Text = "★  " + Social.Points.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + " points";
		label3.Font = new Font("Segoe UI Semibold", Zx.P(18f));
		label3.ForeColor = C.Or;
		label3.BackColor = C.Carte;
		label3.Location = new Point(Zx.P(24), Zx.P(44));
		label3.AutoSize = true;
		cadre3.Controls.Add(label3);
		Label label4 = new Label();
		label4.Text = "+50 par jour  ·  +10 par mod aimé  ·  +5 par commentaire (3 par jour)  ·  comptés par le serveur TPS";
		label4.Font = new Font("Segoe UI", Zx.P(9f));
		label4.ForeColor = C.Gris;
		label4.BackColor = C.Carte;
		label4.Location = new Point(Zx.P(26), Zx.P(90));
		label4.AutoSize = true;
		cadre3.Controls.Add(label4);
		Bouton bouton2 = new Bouton("Ouvrir la Boutique");
		bouton2.Couleur = C.Or;
		bouton2.Size = new Size(Zx.P(200), Zx.P(40));
		bouton2.Location = new Point(num - Zx.P(24) - Zx.P(200), Zx.P(46));
		bouton2.Click += delegate
		{
			Demande = "boutique";
			base.DialogResult = DialogResult.OK;
		};
		cadre3.Controls.Add(bouton2);
		y = cadre3.Bottom + Zx.P(16);
		List<string> ids = new List<string>();
		List<string> list = new List<string>();
		ids.Add("");
		list.Add("Aucune");
		for (int j = 0; j < Deco.Ids.Length; j++)
		{
			if (Deco.Disponible(Deco.Ids[j]))
			{
				ids.Add(Deco.Ids[j]);
				list.Add(Deco.Noms[j]);
			}
		}
		Cadre cadre4 = new Cadre();
		cadre4.Titre = "Rôle VIP sur Discord";
		cadre4.Location = new Point(0, y);
		cadre4.Size = new Size(num, Zx.P(150));
		pg.Controls.Add(cadre4);
		Label label5 = new Label();
		label5.Text = "Rejoignez le Discord TPS Groupe et envoyez votre identifiant VIP dans le salon #vip : l'équipe le vérifie et vous donne le rôle VIP.";
		label5.Font = new Font("Segoe UI", Zx.P(9.5f));
		label5.ForeColor = Color.FromArgb(242, 228, 196);
		label5.BackColor = C.Carte;
		label5.Location = new Point(Zx.P(24), Zx.P(44));
		label5.Size = new Size(num - Zx.P(48), Zx.P(40));
		cadre4.Controls.Add(label5);
		Label label6 = new Label();
		label6.Text = "Identifiant VIP :  " + Social.MonId;
		label6.Font = new Font("Consolas", Zx.P(11f));
		label6.ForeColor = C.Or;
		label6.BackColor = C.Carte;
		label6.Location = new Point(Zx.P(24), Zx.P(98));
		label6.AutoSize = true;
		cadre4.Controls.Add(label6);
		Bouton bouton3 = new Bouton("Copier");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(110), Zx.P(36));
		bouton3.Location = new Point(num - Zx.P(24) - Zx.P(110) - Zx.P(10) - Zx.P(190), Zx.P(92));
		cadre4.Controls.Add(bouton3);
		bouton3.Click += delegate
		{
			try
			{
				Clipboard.SetText(Social.MonId);
				lErr.ForeColor = C.Vert;
				lErr.Text = "✔  Identifiant VIP copié : collez-le dans le salon #vip du Discord.";
			}
			catch
			{
			}
		};
		Bouton bouton4 = new Bouton("Ouvrir le Discord");
		bouton4.Couleur = Color.FromArgb(88, 101, 242);
		bouton4.Size = new Size(Zx.P(190), Zx.P(36));
		bouton4.Location = new Point(num - Zx.P(24) - Zx.P(190), Zx.P(92));
		cadre4.Controls.Add(bouton4);
		bouton4.Click += delegate
		{
			try
			{
				Process.Start(string.IsNullOrEmpty(Fenetre.InviteVip) ? "https://discord.gg/dA77S5nXVV" : Fenetre.InviteVip);
			}
			catch
			{
			}
		};
		y = cadre4.Bottom + Zx.P(16);
		Cadre cadre5 = new Cadre();
		cadre5.Titre = "Décoration d'avatar";
		cadre5.Location = new Point(0, y);
		cadre5.Size = new Size(num, Zx.P(130));
		pg.Controls.Add(cadre5);
		if (ids.Count == 1)
		{
			Label label7 = new Label();
			label7.Text = "Aucune décoration achetée : la couronne et l'anneau de flammes sont dans la Boutique.";
			label7.Font = new Font("Segoe UI", Zx.P(9.5f));
			label7.ForeColor = C.Gris;
			label7.BackColor = C.Carte;
			label7.Location = new Point(Zx.P(24), Zx.P(52));
			label7.AutoSize = true;
			cadre5.Controls.Add(label7);
			return;
		}
		Segments sd = new Segments();
		sd.Items = list.ToArray();
		sd.Etire = true;
		sd.Location = new Point(Zx.P(24), Zx.P(52));
		sd.Width = Math.Min(num - 48, 160 * list.Count);
		sd.Index = Math.Max(0, ids.IndexOf(a.Deco ?? ""));
		cadre5.Controls.Add(sd);
		sd.Changed += delegate
		{
			Compte actif = Comptes.Actif;
			actif.Deco = ids[sd.Index];
			Comptes.Sauvegarder();
			change = true;
			apercu.Invalidate();
		};
	}

	private bool AppliquerAuto(Compte c)
	{
		if (!Comptes.AppliquerCouleursPhoto(c))
		{
			return false;
		}
		selAvatar.Couleur = Comptes.CouleurAvatar(c);
		change = true;
		if (apercu != null)
		{
			apercu.Invalidate();
		}
		if (miniature != null)
		{
			miniature.Invalidate();
		}
		if (lb != null)
		{
			lb.Invalidate();
		}
		lErr.ForeColor = C.Vert;
		lErr.Text = "✔  Couleurs adaptées à votre photo";
		return true;
	}

	private void DessinerApercu(Graphics g, int w, int h)
	{
		Compte actif = Comptes.Actif;
		if (actif == null || tPseudo == null || selAvatar == null)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		double temps = BanAnim.Temps;
		Progression.Niveau(actif.Xp, out var niv, out var dans, out var besoin);
		Rectangle r = new Rectangle(0, 0, w - 1, h - 1);
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(16)))
		{
			using (SolidBrush brush = new SolidBrush(C.Carte))
			{
				g.FillPath(brush, path);
			}
			GraphicsState gstate = g.Save();
			g.SetClip(path, CombineMode.Intersect);
			Banniere.De(actif).Dessiner(g, new Rectangle(0, 0, w, Zx.P(124)));
			g.Restore(gstate);
			using Pen pen = new Pen(C.Ligne);
			g.DrawPath(pen, path);
		}
		Rectangle rectangle = new Rectangle(Zx.P(22), Zx.P(74), Zx.P(96), Zx.P(96));
		using (SolidBrush brush2 = new SolidBrush(C.Carte))
		{
			g.FillEllipse(brush2, new Rectangle(rectangle.X - Zx.P(6), rectangle.Y - Zx.P(6), rectangle.Width + Zx.P(12), rectangle.Height + Zx.P(12)));
		}
		Comptes.AvatarBrut(g, rectangle, Comptes.ImageAvatar(actif), tPseudo.Text.Trim(), selAvatar.Couleur, 32f);
		Badge.Anneau(g, rectangle, niv, temps);
		Deco.Dessiner(g, rectangle, actif.Deco, temps);
		using (SolidBrush brush3 = new SolidBrush(C.Carte))
		{
			g.FillEllipse(brush3, rectangle.Right - Zx.P(26), rectangle.Bottom - Zx.P(26), Zx.P(28), Zx.P(28));
		}
		using (SolidBrush brush4 = new SolidBrush(C.Vert))
		{
			g.FillEllipse(brush4, rectangle.Right - Zx.P(21), rectangle.Bottom - Zx.P(21), Zx.P(18), Zx.P(18));
		}
		Badge.Dessiner(g, new Rectangle(w - Zx.P(22) - Zx.P(50), Zx.P(132), Zx.P(50), Zx.P(50)), niv, temps);
		Rectangle r2 = new Rectangle(Zx.P(12), Zx.P(184), w - Zx.P(25), h - Zx.P(196));
		using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(12)))
		{
			using SolidBrush brush5 = new SolidBrush(Color.FromArgb(14, 17, 38));
			g.FillPath(brush5, path2);
		}
		int num = r2.X + Zx.P(16);
		int num2 = r2.Width - Zx.P(32);
		int num3 = r2.Y + Zx.P(12);
		string t = ((tPseudo.Text.Trim().Length > 0) ? tPseudo.Text.Trim() : "Pseudo");
		Lang.DrawText(g, t, fNomA, new Rectangle(num, num3, num2 - (Social.Vip ? Zx.P(48) : 0), Zx.P(34)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		if (Social.Vip)
		{
			Badge.PilleVip(g, new Rectangle(Math.Min(num + Lang.MeasureText(t, fNomA).Width + Zx.P(6), num + num2 - Zx.P(42)), num3 + Zx.P(8), Zx.P(40), Zx.P(20)));
		}
		num3 += Zx.P(34);
		if (tStatut.Text.Trim().Length > 0)
		{
			Lang.DrawText(g, tStatut.Text.Trim(), fStatA, new Rectangle(num, num3, num2, Zx.P(20)), Color.FromArgb(170, 205, 225), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			num3 += Zx.P(22);
		}
		num3 += Zx.P(8);
		using (Pen pen2 = new Pen(C.Ligne))
		{
			g.DrawLine(pen2, num, num3, num + num2, num3);
		}
		num3 += Zx.P(12);
		Lang.DrawText(g, "À PROPOS DE MOI", F.Rubrique, new Point(num, num3), C.Gris);
		num3 += Zx.P(20);
		string t2 = ((tBio.Text.Trim().Length > 0) ? tBio.Text.Trim() : "Aucune bio pour l'instant.");
		Lang.DrawText(g, t2, fTxtA, new Rectangle(num, num3, num2, Zx.P(54)), (tBio.Text.Trim().Length > 0) ? Color.FromArgb(214, 220, 236) : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		num3 += Zx.P(60);
		Lang.DrawText(g, "NIVEAU", F.Rubrique, new Point(num, num3), C.Gris);
		num3 += Zx.P(20);
		Lang.DrawText(g, "Niveau " + niv + "  ·  " + Progression.Titre(niv), fNivA, new Point(num, num3), C.Texte);
		num3 += Zx.P(26);
		Rectangle r3 = new Rectangle(num, num3, num2, Zx.P(8));
		double num4 = ((niv >= 100) ? 1.0 : ((double)dans / (double)Math.Max(1, besoin)));
		using (GraphicsPath path3 = Bouton.Rond(r3, Zx.P(4)))
		{
			using SolidBrush brush6 = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
			g.FillPath(brush6, path3);
		}
		int num5 = (int)((double)r3.Width * num4);
		if (num5 > 6)
		{
			using GraphicsPath path4 = Bouton.Rond(new Rectangle(r3.X, r3.Y, num5, r3.Height), Zx.P(4));
			using LinearGradientBrush brush7 = new LinearGradientBrush(new Rectangle(r3.X, r3.Y, Math.Max(Zx.P(2), num5), r3.Height), C.Jade, C.Orange, 0f);
			g.FillPath(brush7, path4);
		}
		num3 += Zx.P(12);
		Lang.DrawText(g, (niv >= 100) ? (actif.Xp + " XP") : (dans + " / " + besoin + " XP"), fPetitS, new Point(num, num3), C.Gris);
		num3 += Zx.P(26);
		Lang.DrawText(g, "MEMBRE DEPUIS", F.Rubrique, new Point(num, num3), C.Gris);
		num3 += Zx.P(20);
		Lang.DrawText(g, actif.Cree, fTxtA, new Point(num, num3), Color.FromArgb(214, 220, 236));
		num3 += Zx.P(30);
		List<string> list = new List<string>();
		int num6 = 0;
		foreach (int value in actif.Lancements.Values)
		{
			num6 += value;
		}
		list.Add(num6 + ((num6 > 1) ? " lancements" : " lancement"));
		list.Add(actif.Succes.Count + " succès");
		if (tJeu.Text.Trim().Length > 0)
		{
			list.Add("★ " + tJeu.Text.Trim());
		}
		if (tLieu.Text.Trim().Length > 0)
		{
			list.Add(tLieu.Text.Trim());
		}
		if (actif.DiscordNom.Length > 0)
		{
			list.Add("Discord : " + actif.DiscordNom);
		}
		int num7 = num;
		foreach (string item in list)
		{
			int num8 = Math.Min(num2, Lang.MeasureText(item, F.Etat).Width + 22);
			if (num7 + num8 > num + num2)
			{
				num7 = num;
				num3 += Zx.P(32);
			}
			if (num3 + 26 > r2.Bottom - 8)
			{
				break;
			}
			Rectangle r4 = new Rectangle(num7, num3, num8, Zx.P(26));
			using (GraphicsPath path5 = Bouton.Rond(r4, Zx.P(12)))
			{
				using (SolidBrush brush8 = new SolidBrush(Color.FromArgb(60, C.Jade)))
				{
					g.FillPath(brush8, path5);
				}
				using Pen pen3 = new Pen(Color.FromArgb(110, C.Jade));
				g.DrawPath(pen3, path5);
			}
			Lang.DrawText(g, item, F.Etat, r4, C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			num7 += num8 + Zx.P(8);
		}
	}

	private static int GCase(int w)
	{
		return (w - 3 * GGap) / 4;
	}

	private static int GHaut(int w)
	{
		return GCase(w) * 3 / 8;
	}

	private static int Rub(int i)
	{
		if (i >= 10)
		{
			if (i >= 16)
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	private static int RubDebut(int k)
	{
		return k switch
		{
			1 => 10, 
			0 => 0, 
			_ => 16, 
		};
	}

	private static int RubFin(int k)
	{
		return k switch
		{
			1 => 16, 
			0 => 10, 
			_ => BanAnim.Ids.Length, 
		};
	}

	private static int RubHaut(int w, int k)
	{
		int num = RubFin(k) - RubDebut(k);
		return GTitre + (num + 4 - 1) / 4 * (GHaut(w) + Zx.P(30)) + Zx.P(14);
	}

	private static Rectangle RectGalerie(int w, int i)
	{
		int num = Rub(i);
		int num2 = 0;
		for (int j = 0; j < num; j++)
		{
			num2 += RubHaut(w, j);
		}
		int num3 = i - RubDebut(num);
		int num4 = GCase(w);
		int num5 = GHaut(w);
		return new Rectangle(num3 % 4 * (num4 + GGap), num2 + GTitre + num3 / 4 * (num5 + Zx.P(30)), num4, num5);
	}

	private static int HauteurGalerie(int w)
	{
		return RectGalerie(w, BanAnim.Ids.Length - 1).Bottom + Zx.P(30);
	}

	private static int CaseGalerie(int w, Point p)
	{
		for (int i = 0; i < BanAnim.Ids.Length; i++)
		{
			if (RectGalerie(w, i).Contains(p))
			{
				return i;
			}
		}
		return -1;
	}

	private void DessinerGalerie(Graphics g, int w)
	{
		Compte actif = Comptes.Actif;
		if (actif == null)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		double temps = BanAnim.Temps;
		string[] array = new string[3]
		{
			"ANIMÉES",
			"FÊTES DE L'ANNÉE",
			Social.Vip ? "VIP  ·  INCLUSES OU À ACHETER EN POINTS" : "VIP  ·  RÉSERVÉES AUX MEMBRES VIP"
		};
		for (int i = 0; i < 3; i++)
		{
			Lang.DrawText(g, array[i], F.Rubrique, new Point(Zx.P(2), RectGalerie(w, RubDebut(i)).Y - GTitre + Zx.P(2)), (i == 2) ? C.Or : C.Gris);
		}
		for (int j = 0; j < BanAnim.Ids.Length; j++)
		{
			Rectangle rectangle = RectGalerie(w, j);
			string text = BanAnim.Ids[j];
			bool flag = actif.BanType == "anim" && actif.BanMotif == text;
			bool flag2 = BanAnim.Disponible(text);
			using (GraphicsPath path = Bouton.Rond(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1), Zx.P(10)))
			{
				GraphicsState gstate = g.Save();
				g.SetClip(path, CombineMode.Intersect);
				BanAnim.Dessiner(text, g, rectangle, temps);
				if (!flag2)
				{
					using (SolidBrush brush = new SolidBrush(Color.FromArgb(150, 6, 6, 14)))
					{
						g.FillRectangle(brush, rectangle);
					}
					Lang.DrawText(g, "\ue72e", fCadenas, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height / 2 - Zx.P(26), rectangle.Width, Zx.P(26)), C.Or, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
					Lang.DrawText(g, Social.Vip ? (BanAnim.PrixPoints(text) + " points") : "VIP", F.Etat, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height / 2 + Zx.P(2), rectangle.Width, Zx.P(18)), Color.FromArgb(255, 232, 170), TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
				}
				g.Restore(gstate);
				using Pen pen = new Pen(flag ? C.Orange : ((j >= 16) ? Color.FromArgb(150, C.Or) : C.Ligne), flag ? Zx.P(3f) : 1f);
				g.DrawPath(pen, path);
			}
			Lang.DrawText(g, (flag ? "✔ " : "") + BanAnim.Noms[j], F.Etat, new Rectangle(rectangle.X, rectangle.Bottom + Zx.P(5), rectangle.Width, Zx.P(18)), flag ? C.Orange : Color.FromArgb(190, 198, 222), TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		}
	}

	private void DessinerSucces(Graphics g, int w, int h)
	{
		Compte actif = Comptes.Actif;
		if (actif == null)
		{
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		g.Clear(C.Fond);
		Progression.Niveau(actif.Xp, out var niv, out var dans, out var besoin);
		double temps = BanAnim.Temps;
		Rectangle r = new Rectangle(0, 0, w - 1, Zx.P(196));
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, Zx.P(197)), Color.FromArgb(32, 36, 80), C.Carte, 0f))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(C.Ligne);
			g.DrawPath(pen, path);
		}
		Badge.Dessiner(g, new Rectangle(Zx.P(20), Zx.P(16), Zx.P(90), Zx.P(90)), niv, temps);
		int num = Zx.P(130);
		Lang.DrawText(g, "Niveau " + niv + "  ·  " + Progression.Titre(niv), new Font("Segoe UI Semibold", Zx.P(15f)), new Point(num, Zx.P(20)), C.Texte);
		Rectangle r2 = new Rectangle(num, Zx.P(56), w - num - Zx.P(24), Zx.P(10));
		double num2 = ((niv >= 100) ? 1.0 : ((double)dans / (double)Math.Max(1, besoin)));
		using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(5)))
		{
			using SolidBrush brush2 = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
			g.FillPath(brush2, path2);
		}
		int num3 = (int)((double)r2.Width * num2);
		if (num3 > 8)
		{
			using GraphicsPath path3 = Bouton.Rond(new Rectangle(r2.X, r2.Y, num3, r2.Height), Zx.P(5));
			using LinearGradientBrush brush3 = new LinearGradientBrush(new Rectangle(r2.X, r2.Y, Math.Max(Zx.P(2), num3), r2.Height), C.Jade, C.Orange, 0f);
			g.FillPath(brush3, path3);
		}
		Lang.DrawText(g, (niv >= 100) ? ("Niveau maximum atteint · " + actif.Xp + " XP") : (dans + " / " + besoin + " XP pour le niveau " + (niv + 1)), fDescS, new Point(num, Zx.P(74)), C.Gris);
		Lang.DrawText(g, "Série : " + actif.Serie + " jour" + ((actif.Serie > 1) ? "s" : "") + "  ·  record " + actif.SerieMax + "  ·  " + actif.Succes.Count + " / " + Progression.Defs.Count + " succès", fDescS, new Point(num, Zx.P(94)), C.Gris);
		int num4 = 38;
		float num5 = (float)(w - 40 - num4) / 9f;
		for (int i = 0; i < 10; i++)
		{
			Rectangle rectangle = new Rectangle(Zx.P(20) + (int)((float)i * num5), Zx.P(126), num4, num4);
			bool flag = niv >= i * 10 + 1;
			if (flag)
			{
				Badge.Dessiner(g, rectangle, i * 10 + Math.Min(10, niv - i * 10), temps);
			}
			else
			{
				Badge.Verrou(g, rectangle);
			}
			Lang.DrawText(g, flag ? ("Niv. " + (i * 10 + 1)) : "???", fPetitS, new Rectangle(rectangle.X - Zx.P(14), rectangle.Bottom + Zx.P(4), rectangle.Width + Zx.P(28), Zx.P(16)), flag ? C.Texte : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
		}
		Lang.DrawText(g, "SUCCÈS", F.Rubrique, new Point(Zx.P(4), Zx.P(212)), C.Gris);
		int num6 = (w - Zx.P(14)) / 2;
		List<DefSucces> list = Progression.Vitrine(actif);
		for (int j = 0; j < list.Count; j++)
		{
			DefSucces defSucces = list[j];
			bool flag2 = actif.Succes.ContainsKey(defSucces.Id);
			int num7 = Math.Min(defSucces.Objectif, Math.Max(0, defSucces.Valeur(actif)));
			Rectangle r3 = new Rectangle(j % 2 * (num6 + Zx.P(14)), Zx.P(236) + j / 2 * Zx.P(106), num6, Zx.P(94));
			using (GraphicsPath path4 = Bouton.Rond(r3, Zx.P(14)))
			{
				using (SolidBrush brush4 = new SolidBrush(flag2 ? Color.FromArgb(34, 38, 78) : C.Carte))
				{
					g.FillPath(brush4, path4);
				}
				using Pen pen2 = new Pen(flag2 ? Color.FromArgb(200, C.Orange) : C.Ligne, flag2 ? 1.6f : 1f);
				g.DrawPath(pen2, path4);
			}
			Rectangle rectangle2 = new Rectangle(r3.X + Zx.P(14), r3.Y + Zx.P(20), Zx.P(50), Zx.P(50));
			using (LinearGradientBrush brush5 = new LinearGradientBrush(rectangle2, flag2 ? ControlPaint.Light(C.Orange, 0.1f) : Color.FromArgb(46, 52, 90), flag2 ? ControlPaint.Dark(C.Orange, 0.2f) : Color.FromArgb(28, 32, 62), 60f))
			{
				g.FillEllipse(brush5, rectangle2);
			}
			Lang.DrawText(g, flag2 ? "\ue73e" : defSucces.Glyphe, fGlyS, rectangle2, flag2 ? Color.White : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			int num8 = rectangle2.Right + Zx.P(12);
			int num9 = r3.Right - num8 - Zx.P(12);
			string t = "+" + defSucces.Xp + " XP";
			Size size = Lang.MeasureText(t, fPetitS);
			Rectangle r4 = new Rectangle(r3.Right - Zx.P(12) - size.Width - Zx.P(14), r3.Y + Zx.P(12), size.Width + Zx.P(14), Zx.P(20));
			Lang.DrawText(g, defSucces.Nom, fNomS, new Rectangle(num8, r3.Y + Zx.P(12), r4.X - num8 - Zx.P(6), Zx.P(22)), flag2 ? C.Texte : Color.FromArgb(200, 206, 226), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			using (GraphicsPath path5 = Bouton.Rond(r4, Zx.P(10)))
			{
				using SolidBrush brush6 = new SolidBrush(Color.FromArgb(flag2 ? 70 : 40, flag2 ? C.Vert : C.Jade));
				g.FillPath(brush6, path5);
			}
			Lang.DrawText(g, t, fPetitS, r4, flag2 ? C.Vert : Color.FromArgb(170, 184, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			int num10 = ((defSucces.NbRangs > 1) ? (defSucces.NbRangs * 11 + 6) : 0);
			Lang.DrawText(g, defSucces.Desc, fDescS, new Rectangle(num8, r3.Y + Zx.P(36), num9 - num10, Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			if (defSucces.NbRangs > 1)
			{
				for (int k = 0; k < defSucces.NbRangs; k++)
				{
					bool flag3 = k < defSucces.Rang - 1 || (k == defSucces.Rang - 1 && flag2);
					Rectangle rect = new Rectangle(r3.Right - Zx.P(12) - (defSucces.NbRangs - k) * Zx.P(11), r3.Y + Zx.P(41), Zx.P(8), Zx.P(8));
					using SolidBrush brush7 = new SolidBrush(flag3 ? C.Orange : Color.FromArgb(60, 255, 255, 255));
					g.FillEllipse(brush7, rect);
				}
			}
			if (flag2)
			{
				Lang.DrawText(g, "Débloqué le " + actif.Succes[defSucces.Id], fPetitS, new Point(num8, r3.Y + Zx.P(64)), C.Vert);
				continue;
			}
			Rectangle r5 = new Rectangle(num8, r3.Y + Zx.P(68), num9 - Zx.P(66), Zx.P(8));
			using (GraphicsPath path6 = Bouton.Rond(r5, Zx.P(4)))
			{
				using SolidBrush brush8 = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
				g.FillPath(brush8, path6);
			}
			int num11 = (int)((double)r5.Width * ((double)num7 / (double)Math.Max(1, defSucces.Objectif)));
			if (num11 > 4)
			{
				using GraphicsPath path7 = Bouton.Rond(new Rectangle(r5.X, r5.Y, num11, r5.Height), Zx.P(4));
				using SolidBrush brush9 = new SolidBrush(C.Jade);
				g.FillPath(brush9, path7);
			}
			Lang.DrawText(g, num7 + " / " + defSucces.Objectif, fPetitS, new Rectangle(r5.Right + Zx.P(8), r3.Y + Zx.P(62), Zx.P(60), Zx.P(18)), C.Gris, TextFormatFlags.NoPadding);
		}
	}
}
