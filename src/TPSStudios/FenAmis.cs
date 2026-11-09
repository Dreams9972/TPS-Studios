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
		int num = Math.Min(width, 1240);
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
			Text = "Ajoute des joueurs TPS Studios et vois en direct quand ils sont en ligne ou en jeu.",
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
		int num3 = height - 82 - 56;
		int num4 = Zx.P(430);
		int lx = num2 + Zx.P(24);
		int num5 = lx + num4 + 28;
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
		func("TON CODE AMI  ·  donne-le à tes amis pour qu'ils t'ajoutent", 6);
		tCode = new Champ(multi: false, Zx.P(38));
		tCode.Location = new Point(lx, Zx.P(28));
		tCode.Width = num4 - Zx.P(118);
		tCode.ReadOnly = true;
		tCode.Text = "…";
		corps.Controls.Add(tCode);
		Bouton bouton = new Bouton("Copier")
		{
			Plein = false,
			Size = new Size(Zx.P(108), Zx.P(38)),
			Location = new Point(lx + num4 - Zx.P(108), Zx.P(28))
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
				Message("✔  Code ami copié : colle-le à ton ami (Discord, message…).", C.Vert);
			}
			catch
			{
			}
		};
		func("AJOUTER AVEC UN CODE AMI", 80);
		tAjout = new Champ(multi: false, Zx.P(38));
		tAjout.Location = new Point(lx, Zx.P(102));
		tAjout.Width = num4 - Zx.P(118);
		tAjout.Indice = "ex. 1A2B-3C4D-5E6F-7A8B";
		tAjout.MaxLength = 24;
		corps.Controls.Add(tAjout);
		Bouton bouton2 = new Bouton("Ajouter")
		{
			Size = new Size(Zx.P(108), Zx.P(38)),
			Location = new Point(lx + num4 - Zx.P(108), Zx.P(102))
		};
		corps.Controls.Add(bouton2);
		bouton2.Click += delegate
		{
			string id = Social.IdDepuisCode(tAjout.Text);
			if (id == null)
			{
				Message("✖  Code ami invalide : 16 caractères (chiffres et lettres A à F), par ex. 1A2B-3C4D-5E6F-7A8B.", C.Rouge);
			}
			else if (id == Social.MonId)
			{
				Message("C'est ton propre code ami.", C.Orange);
			}
			else
			{
				Agir(() => Social.DemanderAmi(id), "✔  Demande d'ami envoyée");
				tAjout.Text = "";
			}
		};
		func("RECHERCHER UN JOUEUR", 156);
		tq = new Champ(multi: false, Zx.P(38));
		tq.Location = new Point(lx, Zx.P(178));
		tq.Width = num4;
		tq.Indice = "Pseudo du joueur (2 lettres au moins)…";
		tq.MaxLength = 20;
		corps.Controls.Add(tq);
		zr = new ZoneDessin();
		zr.Location = new Point(lx, Zx.P(228));
		zr.Size = new Size(num4, num3 - Zx.P(228) - Zx.P(12));
		zr.Dessiner = DessinerRecherche;
		corps.Controls.Add(zr);
		pl = new Panel();
		pl.Location = new Point(num5, Zx.P(8));
		pl.Size = new Size(num6, num3 - Zx.P(20));
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
			Message("Le serveur TPS Studios n'est pas disponible pour le moment : les amis reviendront dès qu'il répondra.", C.Orange);
			return;
		}
		if (premier)
		{
			Message("Connexion au serveur TPS Studios…", C.Gris);
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
				Message("Ton pseudo TPS : " + Social.Pseudo + "  ·  tes amis voient ton statut et tes mods, personne d'autre.", C.Gris);
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
			Agir(() => Social.RetirerAmi(a.Id), a.Pseudo + " a été retiré de tes amis");
			break;
		case "vip":
			Message("\ud83c\udf81  Le VIP a été offert à " + a.Pseudo + " : il sera prévenu dans son lanceur.", C.Or);
			Recharger(premier: false);
			break;
		}
	}

	private void MajTaille()
	{
		int num = Zx.P(40);
		if (Social.Recues.Count > 0)
		{
			num += Zx.P(40) + Social.Recues.Count * Zx.P(62);
		}
		num += Zx.P(40) + Math.Max(1, Social.Amis.Count) * Zx.P(62);
		if (Social.Envoyees.Count > 0)
		{
			num += Zx.P(40) + Social.Envoyees.Count * Zx.P(62);
		}
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

	internal static void Avatar(Graphics g, Ami a, Rectangle r, bool point, Font f)
	{
		using (SolidBrush brush = new SolidBrush(CouleurDe(a.Id)))
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
		int num2 = 60 + Math.Min(num, Lang.MeasureText(pseudo, fNom).Width) + 8;
		if (a.Vip)
		{
			Badge.PilleVip(g, new Rectangle(num2, y + Zx.P(10), Zx.P(36), Zx.P(16)));
			num2 += 42;
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
			Lang.DrawText(g, "Tape au moins 2 lettres du pseudo d'un joueur.\nLe joueur doit avoir déjà utilisé les fonctions en ligne du lanceur.", fSt, new Rectangle(Zx.P(4), Zx.P(6), w - Zx.P(8), Zx.P(60)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
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
			if (num + 54 > h)
			{
				break;
			}
			Ami aa = re;
			Ligne(g, re, num, w, (re.Lien == "ami") ? "Déjà ton ami" : ((re.Lien == "envoyee") ? "Demande envoyée" : ((re.Lien == "recue") ? "Il t'a demandé en ami" : "Joueur TPS Studios")), C.Gris, point: false, sourisR);
			if (re.Lien == "")
			{
				Bt(g, clicsR, sourisR, w - Zx.P(12), num + 11, "＋  Ajouter", C.Jade, plein: true, delegate
				{
					Agir(() => Social.DemanderAmi(aa.Id), "✔  Demande envoyée à " + aa.Pseudo);
				});
			}
			else if (re.Lien == "recue")
			{
				Bt(g, clicsR, sourisR, w - Zx.P(12), num + 11, "Accepter", C.Vert, plein: true, delegate
				{
					Agir(() => Social.AccepterAmi(aa.Id), "✔  " + aa.Pseudo + " est maintenant ton ami");
				});
			}
			else
			{
				Bt(g, clicsR, sourisR, w - Zx.P(12), num + 11, (re.Lien == "ami") ? "Ami ✔" : "Envoyée", C.Gris, plein: false, null);
			}
			clicsR.Add(new KeyValuePair<Rectangle, Action>(new Rectangle(0, num, w, Zx.P(54)), delegate
			{
				Profil(aa);
			}));
			num += Zx.P(62);
		}
	}

	private void DessinerListe(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clicsL.Clear();
		int num = 0;
		List<Ami> list = new List<Ami>(Social.Amis);
		list.Sort((Ami a, Ami b) => (a.EnLigne != b.EnLigne) ? ((!a.EnLigne) ? 1 : (-1)) : string.Compare(a.Pseudo, b.Pseudo, StringComparison.OrdinalIgnoreCase));
		if (Social.Recues.Count > 0)
		{
			Lang.DrawText(g, "DEMANDES REÇUES  ·  " + Social.Recues.Count, fRub, new Point(Zx.P(2), num + Zx.P(8)), C.Orange);
			num += Zx.P(32);
			foreach (Ami recue in Social.Recues)
			{
				Ami aa3 = recue;
				Ligne(g, recue, num, w, "Veut t'ajouter en ami", C.Gris, point: false, sourisL);
				Rectangle rectangle = Bt(g, clicsL, sourisL, w - Zx.P(12), num + 11, "Refuser", C.Gris, plein: false, delegate
				{
					Agir(() => Social.RetirerAmi(aa3.Id), "Demande de " + aa3.Pseudo + " refusée");
				});
				Bt(g, clicsL, sourisL, rectangle.X - Zx.P(8), num + 11, "Accepter", C.Vert, plein: true, delegate
				{
					Agir(() => Social.AccepterAmi(aa3.Id), "✔  " + aa3.Pseudo + " est maintenant ton ami");
				});
				clicsL.Add(new KeyValuePair<Rectangle, Action>(new Rectangle(0, num, w, Zx.P(54)), delegate
				{
					Profil(aa3);
				}));
				num += Zx.P(62);
			}
			num += Zx.P(8);
		}
		int num2 = 0;
		foreach (Ami item in list)
		{
			if (item.EnLigne)
			{
				num2++;
			}
		}
		Lang.DrawText(g, "MES AMIS  ·  " + list.Count + "   (" + num2 + " en ligne)", fRub, new Point(Zx.P(2), num + Zx.P(8)), C.Gris);
		num += Zx.P(32);
		if (list.Count == 0)
		{
			Lang.DrawText(g, "Pas encore d'amis : ajoute-les avec leur code ami ou cherche-les à gauche.", fSt, new Rectangle(Zx.P(4), num + Zx.P(6), w - Zx.P(8), Zx.P(48)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			num += Zx.P(62);
		}
		foreach (Ami item2 in list)
		{
			Ami aa2 = item2;
			Ligne(g, item2, num, w, item2.EnLigne ? item2.Statut : "Hors ligne", item2.EnLigne ? (item2.Statut.StartsWith("En jeu") ? C.Orange : C.Vert) : C.Gris, point: true, sourisL);
			Rectangle rectangle2 = Bt(g, clicsL, sourisL, w - Zx.P(12), num + 11, "Retirer", C.Rouge, plein: false, delegate
			{
				if (MessageBox.Show(this, "Retirer « " + aa2.Pseudo + " » de tes amis ?", "Amis", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Agir(() => Social.RetirerAmi(aa2.Id), aa2.Pseudo + " a été retiré de tes amis");
				}
			});
			Bt(g, clicsL, sourisL, rectangle2.X - Zx.P(8), num + 11, "Profil et mods", C.Jade, plein: false, delegate
			{
				Profil(aa2);
			});
			clicsL.Add(new KeyValuePair<Rectangle, Action>(new Rectangle(0, num, w, Zx.P(54)), delegate
			{
				Profil(aa2);
			}));
			num += Zx.P(62);
		}
		if (Social.Envoyees.Count <= 0)
		{
			return;
		}
		num += Zx.P(8);
		Lang.DrawText(g, "DEMANDES ENVOYÉES  ·  " + Social.Envoyees.Count, fRub, new Point(Zx.P(2), num + Zx.P(8)), C.Gris);
		num += Zx.P(32);
		foreach (Ami envoyee in Social.Envoyees)
		{
			Ami aa = envoyee;
			Ligne(g, envoyee, num, w, "En attente de réponse", C.Gris, point: false, sourisL);
			Bt(g, clicsL, sourisL, w - Zx.P(12), num + 11, "Annuler", C.Gris, plein: false, delegate
			{
				Agir(() => Social.RetirerAmi(aa.Id), "Demande à " + aa.Pseudo + " annulée");
			});
			clicsL.Add(new KeyValuePair<Rectangle, Action>(new Rectangle(0, num, w, Zx.P(54)), delegate
			{
				Profil(aa);
			}));
			num += Zx.P(62);
		}
	}
}
