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

	public FenAmiProfil(Ami ami)
	{
		a = ami;
		Text = ami.Pseudo;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(700), Zx.P(640));
		pz = new Panel();
		pz.Location = new Point(0, 0);
		pz.Size = new Size(Zx.P(700), Zx.P(640) - Zx.P(70));
		pz.AutoScroll = true;
		pz.BackColor = C.Fond;
		base.Controls.Add(pz);
		Theme.Sombre(pz);
		z = new ZoneDessin();
		z.Location = new Point(0, 0);
		z.Size = new Size(Zx.P(700) - SystemInformation.VerticalScrollBarWidth - Zx.P(2), Zx.P(560));
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
		bVip.Location = new Point(Zx.P(700) - Zx.P(24) - Zx.P(190), Zx.P(16));
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
		bAct.Location = new Point(Zx.P(700) - Zx.P(24) - Zx.P(190) - Zx.P(10) - Zx.P(170), Zx.P(16));
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
		int val = Zx.P(210) + Zx.P(34) + Math.Max(1, (pr.Communs.Count + Zx.P(3)) / 4) * Zx.P(40) + Zx.P(40) + (pr.EstAmi ? (Math.Max(1, pr.Mods.Count) * Zx.P(50)) : Zx.P(40)) + Zx.P(30);
		z.Height = Math.Max(pz.ClientSize.Height, val);
		z.Invalidate();
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		Ami ami = ((pr != null) ? pr.A : a);
		using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, Zx.P(120)), Color.FromArgb(60, FenAmis.CouleurDe(ami.Id)), C.Fond, 90f))
		{
			g.FillRectangle(brush, 0, 0, w, Zx.P(120));
		}
		FenAmis.Avatar(g, ami, new Rectangle(Zx.P(28), Zx.P(28), Zx.P(84), Zx.P(84)), pr != null && pr.EstAmi, fI);
		Lang.DrawText(g, ami.Pseudo, fP, new Rectangle(Zx.P(132), Zx.P(34), w - Zx.P(160), Zx.P(34)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int num = Zx.P(132) + Math.Min(w - Zx.P(200), Lang.MeasureText(ami.Pseudo, fP).Width) + Zx.P(12);
		if (ami.Vip)
		{
			Badge.PilleVip(g, new Rectangle(num, Zx.P(44), Zx.P(40), Zx.P(20)));
			num += Zx.P(48);
		}
		Lang.DrawText(g, "Niv. " + ami.Niveau, fM, new Point(num, Zx.P(44)), Color.FromArgb(255, 196, 140));
		string t = ((pr != null) ? ((!pr.EstAmi) ? "Pas encore dans vos amis" : (pr.A.EnLigne ? pr.A.Statut : "Hors ligne")) : ((err != null) ? ("✖  " + err) : "Chargement du profil…"));
		Lang.DrawText(g, t, fT, new Rectangle(Zx.P(132), Zx.P(76), w - Zx.P(160), Zx.P(20)), (pr == null || !pr.EstAmi || !pr.A.EnLigne) ? ((err != null) ? C.Rouge : C.Gris) : (pr.A.Statut.StartsWith("En jeu") ? C.Orange : C.Vert), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		Lang.DrawText(g, "Code ami : " + Social.CodeAmi(ami.Id), FenAmis.fSt, new Point(Zx.P(132), Zx.P(98)), C.Gris);
		if (pr == null)
		{
			return;
		}
		int num2 = Zx.P(140);
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
