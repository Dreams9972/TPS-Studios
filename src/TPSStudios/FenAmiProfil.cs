using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenAmiProfil : Form
{
	public string Demande = "";

	private Ami a;

	private ProfilAmi pr;

	private string err;

	private ZoneDessin z;

	private Panel pz;

	private Bouton bVip;

	private Bouton bAct;

	private List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private Point souris = new Point(-1, -1);

	private static readonly Font fP = new Font("Segoe UI Semibold", Zx.P(17f));

	private static readonly Font fI = new Font("Segoe UI Semibold", Zx.P(22f));

	private static readonly Font fT = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fM = new Font("Segoe UI Semibold", Zx.P(10f));

	/// <summary>Bas de l'en-tête (bannière, bio, statistiques), calculé au dessin.</summary>
	private int hautEntete = Zx.P(300);

	public FenAmiProfil(Ami ami)
	{
		a = ami;
		Text = ami.Pseudo;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(760), Zx.P(780));
		pz = new Panel();
		pz.Location = new Point(0, 0);
		pz.Size = new Size(Zx.P(760), Zx.P(780) - Zx.P(70));
		pz.AutoScroll = true;
		pz.BackColor = C.Fond;
		base.Controls.Add(pz);
		Theme.Sombre(pz);
		z = new ZoneDessin();
		z.Location = new Point(0, 0);
		z.Size = new Size(Zx.P(760) - SystemInformation.VerticalScrollBarWidth - Zx.P(2), Zx.P(560));
		z.Dessiner = Dessiner;
		pz.Controls.Add(z);
		ZoneDessin zoneDessin = z;
		MouseEventHandler value = delegate(object o, MouseEventArgs e)
		{
			souris = e.Location;
			bool flag = false;
			foreach (KeyValuePair<Rectangle, Action> clic in clics)
			{
				if (clic.Key.Contains(e.Location))
				{
					flag = true;
				}
			}
			z.Cursor = (flag ? Cursors.Hand : Cursors.Default);
			z.Invalidate();
		};
		zoneDessin.MouseMove += value;
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			foreach (KeyValuePair<Rectangle, Action> item in new List<KeyValuePair<Rectangle, Action>>(clics))
			{
				if (item.Key.Contains(e.Location))
				{
					Sons.Clic();
					item.Value();
					break;
				}
			}
		};
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton = new Bouton("Fermer");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(110), Zx.P(38));
		bouton.Location = new Point(Zx.P(24), Zx.P(16));
		bouton.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		panel.Controls.Add(bouton);
		bVip = new Bouton("\ud83c\udf81  Offrir le VIP");
		bVip.Couleur = C.Or;
		bVip.Size = new Size(Zx.P(190), Zx.P(38));
		bVip.Location = new Point(Zx.P(760) - Zx.P(24) - Zx.P(190), Zx.P(16));
		bVip.Visible = false;
		panel.Controls.Add(bVip);
		bVip.Click += delegate
		{
			using FenOffrirVip f = new FenOffrirVip(a);
			if (f.Afficher(this) == DialogResult.OK)
			{
				Demande = "vip";
				base.DialogResult = DialogResult.OK;
			}
		};
		bAct = new Bouton("…");
		bAct.Plein = false;
		bAct.Size = new Size(Zx.P(170), Zx.P(38));
		bAct.Location = new Point(Zx.P(760) - Zx.P(24) - Zx.P(190) - Zx.P(10) - Zx.P(170), Zx.P(16));
		bAct.Visible = false;
		panel.Controls.Add(bAct);
		base.HandleCreated += delegate
		{
			BeginInvoke((MethodInvoker)delegate
			{
				Charger();
			});
		};
	}

	private void Charger()
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			ProfilAmi p = null;
			string e = null;
			try
			{
				p = Social.ProfilJoueur(a.Id);
			}
			catch (Exception ex)
			{
				e = ex.Message;
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					if (!base.IsDisposed)
					{
						pr = p;
						err = e;
						MajBoutons();
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

	private void MajBoutons()
	{
		if (pr == null)
		{
			z.Invalidate();
			return;
		}
		bVip.Visible = pr.EstAmi;
		bVip.Enabled = !pr.A.Vip;
		bVip.Text = (pr.A.Vip ? "★  Déjà VIP" : "\ud83c\udf81  Offrir le VIP");
		bAct.Visible = true;
		bAct.Danger = pr.EstAmi;
		if (pr.EstAmi)
		{
			bAct.Text = "Retirer de mes amis";
			bAct.Click += delegate
			{
				if (MessageBox.Show(this, "Retirer « " + a.Pseudo + " » de vos amis ?", "Amis", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					Demande = "retirer";
					base.DialogResult = DialogResult.OK;
				}
			};
		}
		else if (a.Lien == "" || a.Lien == null)
		{
			bAct.Plein = true;
			bAct.Text = "＋  Ajouter en ami";
			bAct.Click += delegate
			{
				Demande = "ajouter";
				base.DialogResult = DialogResult.OK;
			};
		}
		else
		{
			bAct.Visible = false;
		}
		int val = hautEntete + Zx.P(70) + Zx.P(34) + Math.Max(1, (pr.Communs.Count + Zx.P(3)) / 4) * Zx.P(40) + Zx.P(40) + (pr.EstAmi ? (Math.Max(1, pr.Mods.Count) * Zx.P(50)) : Zx.P(40)) + Zx.P(30);
		z.Height = Math.Max(pz.ClientSize.Height, val);
		z.Invalidate();
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		Ami ami = ((pr != null) ? pr.A : a);
		if (ami.App.Count == 0 && a.App.Count > 0)
		{
			ami.App = a.App;
		}
		Rectangle rb = new Rectangle(0, 0, w, Zx.P(130));
		FenAmis.DessinerBanniere(g, ami, rb);
		using (LinearGradientBrush ombre = new LinearGradientBrush(new Rectangle(0, Zx.P(70), w, Zx.P(61)), Color.FromArgb(0, 8, 10, 24), Color.FromArgb(200, C.Fond), 90f))
		{
			g.FillRectangle(ombre, 0, Zx.P(70), w, Zx.P(60));
		}
		Badge.Dessiner(g, new Rectangle(w - Zx.P(70), Zx.P(14), Zx.P(48), Zx.P(48)), ami.Niveau, 0.0);
		Rectangle av = new Rectangle(Zx.P(28), Zx.P(84), Zx.P(96), Zx.P(96));
		using (SolidBrush anneau = new SolidBrush(C.Fond))
		{
			g.FillEllipse(anneau, Rectangle.Inflate(av, Zx.P(5), Zx.P(5)));
		}
		FenAmis.Avatar(g, ami, av, pr != null && pr.EstAmi, fI);
		if (ami.Texte("deco").Length > 0)
		{
			try
			{
				Deco.Forcer(g, av, ami.Texte("deco"), 0.0);
			}
			catch
			{
			}
		}
		int xt = av.Right + Zx.P(20);
		Lang.DrawText(g, ami.Pseudo, fP, new Rectangle(xt, Zx.P(136), w - xt - Zx.P(24), Zx.P(34)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int num = xt + Math.Min(w - xt - Zx.P(100), Lang.MeasureText(ami.Pseudo, fP).Width) + Zx.P(12);
		if (ami.Vip)
		{
			Badge.PilleVip(g, new Rectangle(num, Zx.P(146), Zx.P(40), Zx.P(20)));
			num += Zx.P(48);
		}
		Lang.DrawText(g, "Niv. " + ami.Niveau, fM, new Point(num, Zx.P(146)), Color.FromArgb(255, 196, 140));
		string t = ((pr != null) ? ((!pr.EstAmi) ? "Pas encore dans vos amis" : (pr.A.EnLigne ? pr.A.Statut : "Hors ligne")) : ((err != null) ? ("✖  " + err) : "Chargement du profil…"));
		Lang.DrawText(g, t, fT, new Rectangle(xt, Zx.P(174), w - xt - Zx.P(24), Zx.P(20)), (pr == null || !pr.EstAmi || !pr.A.EnLigne) ? ((err != null) ? C.Rouge : C.Gris) : (pr.A.Statut.StartsWith("En jeu") ? C.Orange : C.Vert), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		Lang.DrawText(g, "Code ami : " + Social.CodeAmi(ami.Id), FenAmis.fSt, new Point(xt, Zx.P(196)), C.Gris);
		int yi = Zx.P(226);
		string bio = ami.Texte("bio");
		string statutPerso = ami.Texte("statut");
		if (statutPerso.Length > 0)
		{
			Lang.DrawText(g, "« " + statutPerso + " »", fM, new Rectangle(28, yi, w - 56, Zx.P(22)), Color.FromArgb(225, 230, 245), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			yi += Zx.P(26);
		}
		if (bio.Length > 0)
		{
			int hb = Math.Min(Zx.P(60), Lang.MeasureText(bio, fT, new Size(w - 56, Zx.P(200)), TextFormatFlags.WordBreak).Height);
			Lang.DrawText(g, bio, fT, new Rectangle(28, yi, w - 56, hb), Color.FromArgb(200, 210, 228), TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			yi += hb + Zx.P(8);
		}
		List<string> infos = new List<string>();
		if (ami.Texte("jeu_favori").Length > 0)
		{
			infos.Add("🎮  " + ami.Texte("jeu_favori"));
		}
		if (ami.Texte("lieu").Length > 0)
		{
			infos.Add("📍  " + ami.Texte("lieu"));
		}
		if (ami.Nombre("secondes") > 0)
		{
			infos.Add("⏱  " + FenAmis.Duree(ami.Nombre("secondes")) + " de jeu");
		}
		if (ami.Nombre("lancements") > 0)
		{
			infos.Add("▶  " + ami.Nombre("lancements") + " lancements");
		}
		if (ami.Nombre("succes") > 0)
		{
			infos.Add("🏆  " + ami.Nombre("succes") + " succès");
		}
		if (ami.Cree > 0)
		{
			infos.Add("Membre depuis le " + new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(ami.Cree).ToLocalTime().ToString("dd/MM/yyyy"));
		}
		int xi = 28;
		foreach (string inf in infos)
		{
			int li = Lang.MeasureText(inf, FenAmis.fSt).Width + Zx.P(20);
			if (xi + li > w - 28)
			{
				xi = 28;
				yi += Zx.P(34);
			}
			Rectangle ri = new Rectangle(xi, yi, li, Zx.P(28));
			using (GraphicsPath pi = Bouton.Rond(ri, Zx.P(14)))
			{
				using SolidBrush bi = new SolidBrush(C.Carte);
				g.FillPath(bi, pi);
			}
			Lang.DrawText(g, inf, FenAmis.fSt, ri, C.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			xi += li + Zx.P(8);
		}
		if (infos.Count > 0)
		{
			yi += Zx.P(40);
		}
		hautEntete = yi;
		if (pr == null)
		{
			return;
		}
		int num2 = Math.Max(Zx.P(140), yi + Zx.P(6));
		int num3 = 28;
		int num4 = w - 56;
		Lang.DrawText(g, "AMIS EN COMMUN  ·  " + pr.Communs.Count, FenAmis.fRub, new Point(num3, num2), C.Gris);
		num2 += Zx.P(26);
		if (pr.Communs.Count == 0)
		{
			Lang.DrawText(g, "Aucun ami en commun pour l'instant.", fT, new Point(num3, num2), C.Gris);
			num2 += Zx.P(32);
		}
		else
		{
			int num5 = num3;
			foreach (Ami commun in pr.Communs)
			{
				int num6 = Math.Min(num4, Lang.MeasureText(commun.Pseudo, FenAmis.fBt).Width + Zx.P(48));
				if (num5 + num6 > num3 + num4)
				{
					num5 = num3;
					num2 += Zx.P(40);
				}
				Rectangle r = new Rectangle(num5, num2, num6, Zx.P(32));
				using (GraphicsPath path = Bouton.Rond(r, Zx.P(16)))
				{
					using (SolidBrush brush2 = new SolidBrush(C.Carte))
					{
						g.FillPath(brush2, path);
					}
					using Pen pen = new Pen(Color.FromArgb(120, C.Ligne));
					g.DrawPath(pen, path);
				}
				FenAmis.Avatar(g, commun, new Rectangle(num5 + Zx.P(4), num2 + Zx.P(4), Zx.P(24), Zx.P(24)), point: false, FenAmis.fBt);
				Lang.DrawText(g, commun.Pseudo, FenAmis.fBt, new Rectangle(num5 + Zx.P(34), num2, num6 - Zx.P(40), Zx.P(32)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				num5 += num6 + Zx.P(8);
			}
			num2 += Zx.P(48);
		}
		num2 += Zx.P(6);
		Lang.DrawText(g, "SES MODS" + (pr.EstAmi ? ("  ·  " + pr.Mods.Count) : ""), FenAmis.fRub, new Point(num3, num2), C.Gris);
		num2 += Zx.P(26);
		if (!pr.EstAmi)
		{
			Lang.DrawText(g, "Sa liste de mods est visible quand vous êtes amis.", fT, new Point(num3, num2), C.Gris);
			return;
		}
		if (pr.Mods.Count == 0)
		{
			Lang.DrawText(g, "Aucun mod TPS Groupe installé (ou liste pas encore synchronisée).", fT, new Point(num3, num2), C.Gris);
			return;
		}
		string text = Omsi.DetecteOmsi();
		foreach (string[] i in pr.Mods)
		{
			List<Projet> projetsConnus = Fenetre.ProjetsConnus;
			Predicate<Projet> match = (Projet q) => q.Id == i[0];
			Projet projet = projetsConnus.Find(match);
			Rectangle r2 = new Rectangle(num3, num2, num4, Zx.P(42));
			using (GraphicsPath path2 = Bouton.Rond(r2, Zx.P(10)))
			{
				using (SolidBrush brush3 = new SolidBrush(C.Carte))
				{
					g.FillPath(brush3, path2);
				}
				using Pen pen2 = new Pen(Color.FromArgb(110, C.Ligne));
				g.DrawPath(pen2, path2);
			}
			Lang.DrawText(g, (projet != null) ? projet.Nom : i[0], fM, new Rectangle(num3 + Zx.P(14), num2 + Zx.P(4), num4 - Zx.P(240), Zx.P(18)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			Lang.DrawText(g, ((i[1].Length > 0) ? i[1] : "—") + ((i[2] == "0") ? "  ·  désactivé" : ""), FenAmis.fSt, new Rectangle(num3 + Zx.P(14), num2 + Zx.P(22), num4 - Zx.P(240), Zx.P(16)), (i[2] == "0") ? C.Orange : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			bool flag = projet != null && text != null && Omsi.Installee(projet, text) != null;
			string pid = i[0];
			if (flag)
			{
				Lang.DrawText(g, "Vous l'avez aussi ✔", FenAmis.fSt, new Rectangle(num3 + num4 - Zx.P(220), num2, Zx.P(206), Zx.P(42)), C.Vert, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			else if (projet != null)
			{
				FenAmis.Bt(g, clics, souris, num3 + num4 - 10, num2 + 5, "Voir la page", C.Jade, plein: true, delegate
				{
					Demande = "mod:" + pid;
					base.DialogResult = DialogResult.OK;
				});
			}
			num2 += Zx.P(50);
		}
	}
}
