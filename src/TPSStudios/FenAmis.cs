using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenAmis : Form, IPage
{
	public string Demande = "";

	private Champ tq;

	private Champ tCode;

	private Champ tAjout;

	private ZoneDessin zr;

	private ZoneDessin zl;

	private Panel pl;

	private Label lMsg;

	private System.Windows.Forms.Timer tRech;

	private System.Windows.Forms.Timer tMaj;

	private List<Ami> res = new List<Ami>();

	private bool rechEnCours;

	private string derniere = "";

	private List<KeyValuePair<Rectangle, Action>> clicsR = new List<KeyValuePair<Rectangle, Action>>();

	private List<KeyValuePair<Rectangle, Action>> clicsL = new List<KeyValuePair<Rectangle, Action>>();

	private Point sourisR = new Point(-1, -1);

	private Point sourisL = new Point(-1, -1);

	internal static readonly Font fRub = new Font("Segoe UI Semibold", Zx.P(9f));

	internal static readonly Font fNom = new Font("Segoe UI Semibold", Zx.P(10.5f));

	internal static readonly Font fSt = new Font("Segoe UI", Zx.P(9f));

	internal static readonly Font fBt = new Font("Segoe UI Semibold", Zx.P(9f));

	internal static readonly Font fIni = new Font("Segoe UI Semibold", Zx.P(12f));

	public FenAmis()
	{
		Text = "Amis";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = Integre.Hote;
		int width = base.ClientSize.Width;
		int height = base.ClientSize.Height;
		int num = Math.Max(Math.Min(width, Zx.P(900)), Math.Min(width - Zx.P(48), Zx.P(2400)));
		int num2 = (width - num) / 2;
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(56),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		lMsg = new Label();
		lMsg.Font = new Font("Segoe UI", Zx.P(9.5f));
		lMsg.ForeColor = C.Gris;
		lMsg.BackColor = C.Cote;
		lMsg.Location = new Point(num2 + Zx.P(28), 0);
		lMsg.Size = new Size(num - Zx.P(56), Zx.P(56));
		lMsg.TextAlign = ContentAlignment.MiddleLeft;
		lMsg.UseMnemonic = false;
		panel.Controls.Add(lMsg);
		Panel panel2 = new Panel
		{
			Dock = DockStyle.Top,
			Height = Zx.P(82),
			BackColor = C.Fond
		};
		base.Controls.Add(panel2);
		Control.ControlCollection controls = panel2.Controls;
		EventHandler h = delegate
		{
			base.DialogResult = DialogResult.OK;
		};
		controls.Add(Integre.Retour(h, num2 + Zx.P(24), Zx.P(15)));
		Label value = new Label
		{
			Text = "Amis",
			Font = new Font("Segoe UI Semibold", Zx.P(18f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(num2 + Zx.P(84), Zx.P(14)),
			AutoSize = true
		};
		panel2.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Ajoutez des joueurs TPS Groupe et voyez en direct quand ils sont en ligne ou en jeu.",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(num2 + Zx.P(86), Zx.P(50)),
			AutoSize = true
		};
		panel2.Controls.Add(value2);
		Panel corps = new Panel();
		corps.Dock = DockStyle.Fill;
		corps.BackColor = C.Fond;
		base.Controls.Add(corps);
		corps.BringToFront();
		int num3 = height - Zx.P(82) - Zx.P(56);
		int num4 = Zx.P(420);
		int lx = num2 + Zx.P(24);
		int num5 = lx + num4 + Zx.P(32);
		int num6 = num2 + num - Zx.P(24) - num5;
		Func<string, int, Label> func = delegate(string t, int y)
		{
			Label label = new Label
			{
				Text = t,
				Font = fRub,
				ForeColor = C.Gris,
				BackColor = C.Fond,
				Location = new Point(lx, y),
				AutoSize = true
			};
			corps.Controls.Add(label);
			return label;
		};
		func("VOTRE CODE AMI  ·  donnez-le à vos amis pour qu'ils vous ajoutent", Zx.P(8));
		tCode = new Champ(multi: false, Zx.P(38));
		tCode.Location = new Point(lx, Zx.P(30));
		tCode.Width = num4 - Zx.P(118);
		tCode.ReadOnly = true;
		tCode.Text = "…";
		corps.Controls.Add(tCode);
		Bouton bouton = new Bouton("Copier")
		{
			Plein = false,
			Size = new Size(Zx.P(108), Zx.P(38)),
			Location = new Point(lx + num4 - Zx.P(108), Zx.P(30))
		};
		corps.Controls.Add(bouton);
		bouton.Click += delegate
		{
			if (Social.MonId.Length == 0)
			{
				return;
			}
			try
			{
				Clipboard.SetText(Social.CodeAmi(Social.MonId));
				Message("✔  Code ami copié : envoyez-le à votre ami (Discord, message…).", C.Vert);
			}
			catch
			{
			}
		};
		func("AJOUTER AVEC UN CODE AMI", Zx.P(88));
		tAjout = new Champ(multi: false, Zx.P(38));
		tAjout.Location = new Point(lx, Zx.P(110));
		tAjout.Width = num4 - Zx.P(118);
		tAjout.Indice = "ex. 1A2B-3C4D-5E6F-7A8B";
		tAjout.MaxLength = 24;
		corps.Controls.Add(tAjout);
		Bouton bouton2 = new Bouton("Ajouter")
		{
			Size = new Size(Zx.P(108), Zx.P(38)),
			Location = new Point(lx + num4 - Zx.P(108), Zx.P(110))
		};
		corps.Controls.Add(bouton2);
		bouton2.Click += delegate
		{
			string id = Social.IdDepuisCode(tAjout.Text);
			if (id == null)
			{
				Message("✖  Code ami invalide : 16 caractères (chiffres et lettres A à F), ex. 1A2B-3C4D-5E6F-7A8B.", C.Rouge);
			}
			else if (id == Social.MonId)
			{
				Message("C'est votre propre code ami.", C.Orange);
			}
			else
			{
				Agir(() => Social.DemanderAmi(id), "✔  Demande d'ami envoyée");
				tAjout.Text = "";
			}
		};
		func("RECHERCHER UN JOUEUR", Zx.P(168));
		tq = new Champ(multi: false, Zx.P(38));
		tq.Location = new Point(lx, Zx.P(190));
		tq.Width = num4;
		tq.Indice = "Pseudo ou code ami (2 caractères au moins)…";
		tq.MaxLength = 24;
		corps.Controls.Add(tq);
		zr = new ZoneDessin();
		zr.Location = new Point(lx, Zx.P(242));
		zr.Size = new Size(num4, num3 - Zx.P(242) - Zx.P(12));
		zr.Dessiner = DessinerRecherche;
		corps.Controls.Add(zr);
		pl = new Panel();
		pl.Location = new Point(num5, 0);
		pl.Size = new Size(num6, num3 - Zx.P(8));
		pl.AutoScroll = true;
		pl.BackColor = C.Fond;
		corps.Controls.Add(pl);
		Theme.Sombre(pl);
		zl = new ZoneDessin();
		zl.Location = new Point(0, 0);
		zl.Size = new Size(num6 - SystemInformation.VerticalScrollBarWidth - Zx.P(4), Zx.P(200));
		zl.Dessiner = DessinerListe;
		pl.Controls.Add(zl);
		ZoneDessin[] array = new ZoneDessin[2] { zr, zl };
		foreach (ZoneDessin zoneDessin in array)
		{
			ZoneDessin zz = zoneDessin;
			zz.MouseMove += delegate(object o, MouseEventArgs e)
			{
				if (zz == zr)
				{
					sourisR = e.Location;
				}
				else
				{
					sourisL = e.Location;
				}
				bool flag = false;
				foreach (KeyValuePair<Rectangle, Action> item in (zz == zr) ? clicsR : clicsL)
				{
					if (item.Key.Contains(e.Location))
					{
						flag = true;
					}
				}
				zz.Cursor = (flag ? Cursors.Hand : Cursors.Default);
				zz.Invalidate();
			};
			zz.MouseLeave += delegate
			{
				if (zz == zr)
				{
					sourisR = new Point(-1, -1);
				}
				else
				{
					sourisL = new Point(-1, -1);
				}
				zz.Invalidate();
			};
			zz.MouseClick += delegate(object o, MouseEventArgs e)
			{
				foreach (KeyValuePair<Rectangle, Action> item2 in new List<KeyValuePair<Rectangle, Action>>((zz == zr) ? clicsR : clicsL))
				{
					if (item2.Key.Contains(e.Location))
					{
						Sons.Clic();
						item2.Value();
						break;
					}
				}
			};
		}
		tRech = new System.Windows.Forms.Timer();
		tRech.Interval = 380;
		tRech.Tick += delegate
		{
			tRech.Stop();
			Chercher();
		};
		tq.TextChanged += delegate
		{
			tRech.Stop();
			tRech.Start();
		};
		tMaj = new System.Windows.Forms.Timer();
		tMaj.Interval = 15000;
		tMaj.Tick += delegate
		{
			Recharger(premier: false);
		};
		tMaj.Start();
		base.FormClosed += delegate
		{
			tMaj.Stop();
			tRech.Stop();
		};
		base.HandleCreated += delegate
		{
			BeginInvoke((MethodInvoker)delegate
			{
				Recharger(premier: true);
			});
		};
	}

	private void Message(string t, Color c)
	{
		lMsg.ForeColor = c;
		lMsg.Text = t;
	}

	private void Fond(Func<string> travail, Action apres)
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			string err;
			try
			{
				err = travail();
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					if (!base.IsDisposed)
					{
						if (err != null)
						{
							Message("✖  " + err, C.Rouge);
						}
						if (apres != null)
						{
							apres();
						}
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

	private void Recharger(bool premier)
	{
		if (!Social.Actif)
		{
			Message("Le serveur TPS Groupe n'est pas disponible pour le moment : les amis reviendront dès qu'il répondra.", C.Orange);
			return;
		}
		if (premier)
		{
			Message("Connexion au serveur TPS Groupe…", C.Gris);
		}
		Fond(delegate
		{
			string text = Social.Assurer(Comptes.Actif);
			if (text != null)
			{
				return text;
			}
			return (!Social.ChargerAmis()) ? "liste d'amis indisponible" : null;
		}, delegate
		{
			if (Social.MonId.Length > 0)
			{
				tCode.Text = Social.CodeAmi(Social.MonId);
			}
			if (premier && lMsg.ForeColor != C.Rouge)
			{
				Message("Votre pseudo TPS : " + Social.Pseudo + "  ·  vos amis voient votre statut et vos mods, personne d'autre.", C.Gris);
			}
			MajTaille();
		});
	}

	private void Chercher()
	{
		string q = tq.Text.Trim();
		if (q.Length < 2)
		{
			res.Clear();
			derniere = "";
			zr.Invalidate();
		}
		else
		{
			if (!Social.Actif)
			{
				return;
			}
			rechEnCours = true;
			zr.Invalidate();
			List<Ami> trouve = null;
			Fond(delegate
			{
				string text = Social.Assurer(Comptes.Actif);
				if (text != null)
				{
					return text;
				}
				trouve = Social.Chercher(q);
				return (string)null;
			}, delegate
			{
				rechEnCours = false;
				derniere = q;
				if (trouve != null)
				{
					res = trouve;
				}
				zr.Invalidate();
			});
		}
	}

	private void Agir(Func<string> action, string ok)
	{
		Fond(delegate
		{
			string text = Social.Assurer(Comptes.Actif);
			if (text != null)
			{
				return text;
			}
			text = action();
			if (text == null)
			{
				Social.ChargerAmis();
			}
			return text;
		}, delegate
		{
			MajTaille();
			Chercher();
		});
		Message(ok, C.Vert);
	}

	private void Profil(Ami a)
	{
		string demande;
		using (FenAmiProfil fenAmiProfil = new FenAmiProfil(a))
		{
			fenAmiProfil.Afficher(this);
			demande = fenAmiProfil.Demande;
		}
		if (demande.StartsWith("mod:"))
		{
			Demande = demande;
			base.DialogResult = DialogResult.OK;
			return;
		}
		switch (demande)
		{
		case "ajouter":
			Agir(() => Social.DemanderAmi(a.Id), "✔  Demande envoyée à " + a.Pseudo);
			break;
		case "retirer":
			Agir(() => Social.RetirerAmi(a.Id), a.Pseudo + " a été retiré de vos amis");
			break;
		case "vip":
			Message("\ud83c\udf81  Le VIP a été offert à " + a.Pseudo + " : il sera prévenu dans son lanceur.", C.Or);
			Recharger(premier: false);
			break;
		}
	}

	private const int Ecart = 16;

	private static int HautCarte => Zx.P(262);

	private int Colonnes(int w)
	{
		return Math.Max(1, (w + Zx.P(Ecart)) / (Zx.P(300) + Zx.P(Ecart)));
	}

	private int HauteurSection(int n, int w)
	{
		if (n <= 0)
		{
			return 0;
		}
		int cols = Colonnes(w);
		int rangs = (n + cols - 1) / cols;
		return Zx.P(40) + rangs * (HautCarte + Zx.P(Ecart));
	}

	private void MajTaille()
	{
		int w = zl.Width;
		int num = Zx.P(8);
		num += HauteurSection(Social.Recues.Count, w);
		num += Math.Max(HauteurSection(Math.Max(1, Social.Amis.Count), w), Zx.P(120));
		num += HauteurSection(Social.Envoyees.Count, w);
		zl.Height = Math.Max(pl.ClientSize.Height - 4, num);
		zl.Invalidate();
	}

	internal static Color CouleurDe(string id)
	{
		int num = 0;
		string text = id ?? "";
		foreach (char c in text)
		{
			num = num * Zx.P(31) + c;
		}
		return Comptes.Palette[Math.Abs(num) % Comptes.Palette.Length];
	}

	/// <summary>Couleur d'avatar choisie par le joueur (palette du profil), sinon une couleur tirée de son identifiant.</summary>
	internal static Color CouleurAvatar(Ami a)
	{
		if (a.App.ContainsKey("couleur"))
		{
			int i = a.Nombre("couleur");
			if (i >= 0 && i < Comptes.Palette.Length)
			{
				return Comptes.Palette[i];
			}
		}
		return CouleurDe(a.Id);
	}

	/// <summary>Bannière du joueur (comme sur son profil) ; un modèle tiré de son identifiant s'il n'en a pas publié.</summary>
	internal static void DessinerBanniere(Graphics g, Ami a, Rectangle r)
	{
		Compte c = a.EnCompte();
		if (a.App.Count == 0)
		{
			c.Banniere = Math.Abs((a.Id ?? "").GetHashCode()) % Math.Max(1, Banniere.Modeles.Length);
		}
		try
		{
			Banniere.De(c).Dessiner(g, r);
		}
		catch
		{
			using LinearGradientBrush brush = new LinearGradientBrush(r, CouleurAvatar(a), C.Carte, 20f);
			g.FillRectangle(brush, r);
		}
	}

	internal static string Duree(int secondes)
	{
		if (secondes < 3600)
		{
			return Math.Max(0, secondes / 60) + " min";
		}
		return (secondes / 3600).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("fr-FR")) + " h";
	}

	internal static void Avatar(Graphics g, Ami a, Rectangle r, bool point, Font f)
	{
		using (SolidBrush brush = new SolidBrush(CouleurAvatar(a)))
		{
			g.FillEllipse(brush, r);
		}
		Lang.DrawText(g, (a.Pseudo.Length > 0) ? a.Pseudo.Substring(0, 1).ToUpperInvariant() : "?", f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		if (point)
		{
			Color color = ((!a.EnLigne) ? Color.FromArgb(110, 120, 140) : (a.Statut.StartsWith("En jeu") ? C.Orange : C.Vert));
			int num = Math.Max(10, r.Width / 4);
			using (SolidBrush brush2 = new SolidBrush(C.Fond))
			{
				g.FillEllipse(brush2, r.Right - num - Zx.P(3), r.Bottom - num - Zx.P(3), num + Zx.P(6), num + Zx.P(6));
			}
			using SolidBrush brush3 = new SolidBrush(color);
			g.FillEllipse(brush3, r.Right - num, r.Bottom - num, num, num);
		}
	}

	internal static Rectangle Bt(Graphics g, List<KeyValuePair<Rectangle, Action>> clics, Point souris, int droite, int y, string t, Color c, bool plein, Action a)
	{
		int num = Lang.MeasureText(t, fBt).Width + Zx.P(28);
		Rectangle rectangle = new Rectangle(droite - num, y, num, Zx.P(32));
		bool flag = a != null && rectangle.Contains(souris);
		using (GraphicsPath path = Bouton.Rond(rectangle, Zx.P(16)))
		{
			using (SolidBrush brush = new SolidBrush((!plein) ? Color.FromArgb(flag ? 70 : 34, c) : (flag ? ControlPaint.Light(c, 0.15f) : c)))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(170, c));
			g.DrawPath(pen, path);
		}
		Lang.DrawText(g, t, fBt, rectangle, plein ? Color.White : ((a == null) ? C.Gris : C.Texte), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		if (a != null)
		{
			clics.Add(new KeyValuePair<Rectangle, Action>(rectangle, a));
		}
		return rectangle;
	}

	private void Ligne(Graphics g, Ami a, int y, int w, string sous, Color cs, bool point, Point souris)
	{
		Rectangle r = new Rectangle(0, y, w - 1, Zx.P(54));
		bool flag = r.Contains(souris);
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
		{
			using (SolidBrush brush = new SolidBrush(flag ? C.CarteSurvol : C.Carte))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(flag ? 200 : 120, flag ? C.Jade : C.Ligne));
			g.DrawPath(pen, path);
		}
		Avatar(g, a, new Rectangle(Zx.P(12), y + Zx.P(9), Zx.P(36), Zx.P(36)), point, fIni);
		int num = w - Zx.P(60) - Zx.P(230);
		string pseudo = a.Pseudo;
		Lang.DrawText(g, pseudo, fNom, new Rectangle(Zx.P(60), y + Zx.P(8), num, Zx.P(20)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int num2 = Zx.P(60) + Math.Min(num, Lang.MeasureText(pseudo, fNom).Width) + Zx.P(8);
		if (a.Vip)
		{
			Badge.PilleVip(g, new Rectangle(num2, y + Zx.P(10), Zx.P(36), Zx.P(16)));
			num2 += Zx.P(42);
		}
		Lang.DrawText(g, "Niv. " + a.Niveau, fSt, new Rectangle(num2, y + Zx.P(10), Zx.P(80), Zx.P(18)), Color.FromArgb(255, 196, 140), TextFormatFlags.NoPadding);
		Lang.DrawText(g, sous + ((a.Communs > 0) ? ("  ·  " + a.Communs + ((a.Communs > 1) ? " amis en commun" : " ami en commun")) : ""), fSt, new Rectangle(Zx.P(60), y + Zx.P(30), num, Zx.P(18)), cs, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
	}

	private void DessinerRecherche(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clicsR.Clear();
		if (tq.Text.Trim().Length < 2)
		{
			Lang.DrawText(g, "Tapez au moins 2 lettres du pseudo d'un joueur, ou son code ami.\nLe joueur doit avoir déjà utilisé les fonctions en ligne du lanceur.", fSt, new Rectangle(Zx.P(4), Zx.P(6), w - Zx.P(8), Zx.P(60)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			return;
		}
		if (rechEnCours && res.Count == 0)
		{
			Lang.DrawText(g, "Recherche…", fSt, new Point(Zx.P(4), Zx.P(6)), C.Gris);
			return;
		}
		if (res.Count == 0)
		{
			Lang.DrawText(g, (derniere.Length > 0) ? ("Aucun joueur trouvé pour « " + derniere + " ».") : "", fSt, new Rectangle(Zx.P(4), Zx.P(6), w - Zx.P(8), Zx.P(40)), C.Gris, TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			return;
		}
		int num = 0;
		foreach (Ami re in res)
		{
			if (num + Zx.P(54) > h)
			{
				break;
			}
			Ami aa = re;
			Ligne(g, re, num, w, (re.Lien == "ami") ? "Déjà votre ami" : ((re.Lien == "envoyee") ? "Demande envoyée" : ((re.Lien == "recue") ? "Il vous a demandé en ami" : "Joueur TPS Groupe")), C.Gris, point: false, sourisR);
			if (re.Lien == "")
			{
				Bt(g, clicsR, sourisR, w - Zx.P(12), num + Zx.P(11), "＋  Ajouter", C.Jade, plein: true, delegate
				{
					Agir(() => Social.DemanderAmi(aa.Id), "✔  Demande envoyée à " + aa.Pseudo);
				});
			}
			else if (re.Lien == "recue")
			{
				Bt(g, clicsR, sourisR, w - Zx.P(12), num + Zx.P(11), "Accepter", C.Vert, plein: true, delegate
				{
					Agir(() => Social.AccepterAmi(aa.Id), "✔  " + aa.Pseudo + " est maintenant votre ami");
				});
			}
			else
			{
				Bt(g, clicsR, sourisR, w - Zx.P(12), num + Zx.P(11), (re.Lien == "ami") ? "Ami ✔" : "Envoyée", C.Gris, plein: false, null);
			}
			clicsR.Add(new KeyValuePair<Rectangle, Action>(new Rectangle(0, num, w, Zx.P(54)), delegate
			{
				Profil(aa);
			}));
			num += Zx.P(62);
		}
	}

	private static readonly Font fCarteNom = new Font("Segoe UI Semibold", Zx.P(12f));

	private static readonly Font fCarteIni = new Font("Segoe UI Semibold", Zx.P(20f));

	private static readonly Font fPuce = new Font("Segoe UI", Zx.P(8.5f));

	/// <summary>
	/// Carte d'un joueur : bannière, avatar, décor, niveau et statut. Au survol, elle montre la
	/// bio, le jeu favori, le lieu, le temps de jeu, les succès et les boutons ; un clic ouvre tout le profil.
	/// </summary>
	private void Carte(Graphics g, Ami a, Rectangle r, string mode)
	{
		bool survol = r.Contains(sourisL);
		Ami aa = a;
		using (GraphicsPath cadre = Bouton.Rond(r, Zx.P(14)))
		{
			using (SolidBrush fond = new SolidBrush(survol ? C.CarteSurvol : C.Carte))
			{
				g.FillPath(fond, cadre);
			}
			int hb = Zx.P(84);
			Rectangle rb = new Rectangle(r.X, r.Y, r.Width, hb);
			GraphicsState gs = g.Save();
			g.SetClip(cadre, CombineMode.Intersect);
			g.SetClip(rb, CombineMode.Intersect);
			DessinerBanniere(g, a, rb);
			using (LinearGradientBrush ombre = new LinearGradientBrush(new Rectangle(rb.X, rb.Bottom - Zx.P(40), rb.Width, Zx.P(41)), Color.FromArgb(0, 8, 10, 24), Color.FromArgb(150, 8, 10, 24), 90f))
			{
				g.FillRectangle(ombre, rb.X, rb.Bottom - Zx.P(40), rb.Width, Zx.P(40));
			}
			g.Restore(gs);
			using Pen bord = new Pen(survol ? Color.FromArgb(220, C.Jade) : Color.FromArgb(150, C.Ligne), survol ? 1.6f : 1f);
			g.DrawPath(bord, cadre);
		}
		Badge.Dessiner(g, new Rectangle(r.Right - Zx.P(46), r.Y + Zx.P(10), Zx.P(36), Zx.P(36)), a.Niveau, 0.0);
		int d = Zx.P(66);
		Rectangle av = new Rectangle(r.X + Zx.P(16), r.Y + Zx.P(84) - d / 2 - Zx.P(4), d, d);
		using (SolidBrush anneau = new SolidBrush(survol ? C.CarteSurvol : C.Carte))
		{
			g.FillEllipse(anneau, Rectangle.Inflate(av, Zx.P(4), Zx.P(4)));
		}
		Avatar(g, a, av, mode == "ami", fCarteIni);
		string deco = a.Texte("deco");
		if (deco.Length > 0)
		{
			try
			{
				Deco.Forcer(g, av, deco, 0.0);
			}
			catch
			{
			}
		}
		int x = av.Right + Zx.P(12);
		int wt = r.Right - Zx.P(14) - x;
		int y = r.Y + Zx.P(90);
		Lang.DrawText(g, a.Pseudo, fCarteNom, new Rectangle(x, y, wt, Zx.P(26)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int xv = x + Math.Min(wt - Zx.P(40), Lang.MeasureText(a.Pseudo, fCarteNom).Width + Zx.P(8));
		if (a.Role.Length > 0)
		{
			xv += Badge.Role(g, a.Role, xv, y + Zx.P(4)) + Zx.P(6);
		}
		if (a.Vip && xv + Zx.P(36) < r.Right - Zx.P(10))
		{
			Badge.PilleVip(g, new Rectangle(xv, y + Zx.P(4), Zx.P(36), Zx.P(16)));
		}
		string etat;
		Color ce;
		switch (mode)
		{
		case "recue":
			etat = "Veut vous ajouter en ami";
			ce = C.Orange;
			break;
		case "envoyee":
			etat = "Demande envoyée";
			ce = C.Gris;
			break;
		default:
			etat = (a.EnLigne ? ((a.Statut.Length > 0) ? a.Statut : "En ligne") : "Hors ligne");
			ce = ((!a.EnLigne) ? C.Gris : (a.Statut.StartsWith("En jeu") ? C.Orange : C.Vert));
			break;
		}
		Lang.DrawText(g, etat, fSt, new Rectangle(x, y + Zx.P(28), wt, Zx.P(18)), ce, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int yc = r.Y + Zx.P(150);
		int xc = r.X + Zx.P(16);
		int wc = r.Width - Zx.P(32);
		List<string> puces = new List<string>();
		if (a.Texte("jeu_favori").Length > 0)
		{
			puces.Add("🎮  " + a.Texte("jeu_favori"));
		}
		if (a.Texte("lieu").Length > 0)
		{
			puces.Add("📍  " + a.Texte("lieu"));
		}
		if (a.Nombre("secondes") > 0)
		{
			puces.Add("⏱  " + Duree(a.Nombre("secondes")) + " de jeu");
		}
		if (a.Nombre("succes") > 0)
		{
			puces.Add("🏆  " + a.Nombre("succes") + " succès");
		}
		if (a.Communs > 0)
		{
			puces.Add(a.Communs + ((a.Communs > 1) ? " amis en commun" : " ami en commun"));
		}
		if (!survol)
		{
			string ligne = ((a.Texte("statut").Length > 0) ? ("« " + a.Texte("statut") + " »") : ((a.Texte("bio").Length > 0) ? a.Texte("bio") : "Joueur TPS Groupe"));
			Lang.DrawText(g, ligne, fSt, new Rectangle(xc, yc, wc, Zx.P(36)), Color.FromArgb(190, 200, 220), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			DessinerPuces(g, puces, xc, r.Bottom - Zx.P(34), wc);
		}
		else
		{
			string bio = ((a.Texte("bio").Length > 0) ? a.Texte("bio") : ((a.Texte("statut").Length > 0) ? ("« " + a.Texte("statut") + " »") : "Pas encore de bio."));
			Lang.DrawText(g, bio, fSt, new Rectangle(xc, yc - Zx.P(4), wc, Zx.P(34)), Color.FromArgb(205, 214, 228), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			DessinerPuces(g, puces, xc, yc + Zx.P(34), wc);
			int yb = r.Bottom - Zx.P(44);
			int droite = r.Right - Zx.P(12);
			if (mode == "ami")
			{
				Rectangle rr = Bt(g, clicsL, sourisL, droite, yb, "Retirer", C.Rouge, plein: false, delegate
				{
					if (MessageBox.Show(this, "Retirer « " + aa.Pseudo + " » de vos amis ?", "Amis", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						Agir(() => Social.RetirerAmi(aa.Id), aa.Pseudo + " a été retiré de vos amis");
					}
				});
				Bt(g, clicsL, sourisL, rr.X - Zx.P(8), yb, "Profil et mods", C.Jade, plein: true, delegate
				{
					Profil(aa);
				});
			}
			else if (mode == "recue")
			{
				Rectangle rr2 = Bt(g, clicsL, sourisL, droite, yb, "Refuser", C.Gris, plein: false, delegate
				{
					Agir(() => Social.RetirerAmi(aa.Id), "Demande de " + aa.Pseudo + " refusée");
				});
				Bt(g, clicsL, sourisL, rr2.X - Zx.P(8), yb, "Accepter", C.Vert, plein: true, delegate
				{
					Agir(() => Social.AccepterAmi(aa.Id), "✔  " + aa.Pseudo + " est maintenant votre ami");
				});
			}
			else
			{
				Bt(g, clicsL, sourisL, droite, yb, "Annuler la demande", C.Gris, plein: false, delegate
				{
					Agir(() => Social.RetirerAmi(aa.Id), "Demande à " + aa.Pseudo + " annulée");
				});
			}
		}
		clicsL.Add(new KeyValuePair<Rectangle, Action>(r, delegate
		{
			Profil(aa);
		}));
	}

	private static void DessinerPuces(Graphics g, List<string> puces, int x, int y, int w)
	{
		int xp = x;
		foreach (string p in puces)
		{
			int lp = Lang.MeasureText(p, fPuce).Width + Zx.P(16);
			if (xp + lp > x + w)
			{
				break;
			}
			Rectangle rp = new Rectangle(xp, y, lp, Zx.P(24));
			using (GraphicsPath path = Bouton.Rond(rp, Zx.P(12)))
			{
				using SolidBrush b = new SolidBrush(Color.FromArgb(40, 255, 255, 255));
				g.FillPath(b, path);
			}
			Lang.DrawText(g, p, fPuce, rp, Color.FromArgb(215, 222, 236), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			xp += lp + Zx.P(6);
		}
	}

	private int Grille(Graphics g, List<Ami> l, int y, int w, string mode)
	{
		int cols = Colonnes(w);
		int gap = Zx.P(Ecart);
		int cw = (w - (cols - 1) * gap) / cols;
		for (int i = 0; i < l.Count; i++)
		{
			Rectangle r = new Rectangle(i % cols * (cw + gap), y + i / cols * (HautCarte + gap), cw - 1, HautCarte);
			Carte(g, l[i], r, mode);
		}
		return y + (l.Count + cols - 1) / cols * (HautCarte + gap);
	}

	private void DessinerListe(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBilinear;
		clicsL.Clear();
		int num = Zx.P(8);
		List<Ami> list = new List<Ami>(Social.Amis);
		list.Sort((Ami a, Ami b) => (a.EnLigne != b.EnLigne) ? ((!a.EnLigne) ? 1 : (-1)) : string.Compare(a.Pseudo, b.Pseudo, StringComparison.OrdinalIgnoreCase));
		if (Social.Recues.Count > 0)
		{
			Lang.DrawText(g, "DEMANDES REÇUES  ·  " + Social.Recues.Count, fRub, new Point(Zx.P(2), num), C.Orange);
			num = Grille(g, Social.Recues, num + Zx.P(26), w, "recue") + Zx.P(14);
		}
		int num2 = 0;
		foreach (Ami item in list)
		{
			if (item.EnLigne)
			{
				num2++;
			}
		}
		Lang.DrawText(g, "MES AMIS  ·  " + list.Count + "   (" + num2 + " en ligne)", fRub, new Point(Zx.P(2), num), C.Gris);
		num += Zx.P(26);
		if (list.Count == 0)
		{
			Lang.DrawText(g, "Pas encore d'amis : ajoutez-les avec leur code ami ou cherchez-les à gauche.", fSt, new Rectangle(Zx.P(4), num + Zx.P(6), w - Zx.P(8), Zx.P(48)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			num += Zx.P(70);
		}
		else
		{
			num = Grille(g, list, num, w, "ami") + Zx.P(14);
		}
		if (Social.Envoyees.Count > 0)
		{
			Lang.DrawText(g, "DEMANDES ENVOYÉES  ·  " + Social.Envoyees.Count, fRub, new Point(Zx.P(2), num), C.Gris);
			Grille(g, Social.Envoyees, num + Zx.P(26), w, "envoyee");
		}
	}
}
