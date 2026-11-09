using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenBoutique : Form, IPage
{
	public string Demande = "";

	private ThemeBase th = Themes.Creer(Themes.Courant(), 2);

	private List<Projet> dlc;

	private Func<Projet, Image> image;

	private string lienVip;

	private int CW;

	private int X0;

	private Panel defil;

	private ZoneDessin z;

	private ZoneDessin haut;

	private Label lMsg;

	private System.Windows.Forms.Timer anim;

	private bool occupe;

	private List<KeyValuePair<Rectangle, Action>> zones = new List<KeyValuePair<Rectangle, Action>>();

	private Rectangle rCosm;

	private Point souris = new Point(-1, -1);

	private static readonly Font fTitre = new Font("Segoe UI Semibold", Zx.P(18f));

	private static readonly Font fSec = new Font("Segoe UI Semibold", Zx.P(9f));

	private static readonly Font fNom = new Font("Segoe UI Semibold", Zx.P(11.5f));

	private static readonly Font fTxt = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fBt = new Font("Segoe UI Semibold", Zx.P(9.5f));

	private static readonly Font fGros = new Font("Segoe UI Semibold", Zx.P(15f));

	private static readonly Font fCour = new Font("Segoe MDL2 Assets", Zx.P(26f));

	private static readonly string[,] cosm = new string[6, 3]
	{
		{ "ban:or", "Bannière « Or VIP »", "0" },
		{ "ban:prestige", "Bannière « Prestige TPS »", "0" },
		{ "ban:diamant", "Bannière « Diamant »", "300" },
		{ "ban:neonvip", "Bannière « Néon VIP »", "500" },
		{ "deco:couronne", "Couronne dorée", "400" },
		{ "deco:flammes", "Anneau de flammes", "600" }
	};

	public FenBoutique(List<Projet> dlc, Func<Projet, Image> image, string lienVip)
	{
		this.dlc = dlc;
		this.image = image;
		this.lienVip = lienVip ?? "";
		Text = "Boutique";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = Integre.Hote;
		CW = Math.Min(base.ClientSize.Width, Zx.P(1180));
		X0 = (base.ClientSize.Width - CW) / 2;
		haut = new ZoneDessin();
		haut.Dock = DockStyle.Top;
		haut.Height = Zx.P(92);
		haut.Dessiner = DessinerHaut;
		base.Controls.Add(haut);
		haut.Controls.Add(Integre.Retour(delegate
		{
			base.DialogResult = DialogResult.OK;
		}, X0 + Zx.P(24), Zx.P(18)));
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(50),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		lMsg = new Label();
		lMsg.Font = new Font("Segoe UI", Zx.P(9.5f));
		lMsg.ForeColor = C.Gris;
		lMsg.BackColor = C.Cote;
		lMsg.Location = new Point(X0 + Zx.P(28), Zx.P(14));
		lMsg.Size = new Size(CW - Zx.P(56), Zx.P(22));
		lMsg.Text = "Les achats en euros se font sur la page de paiement TPS Groupe ; les points VIP sont gérés par le serveur TPS.";
		panel.Controls.Add(lMsg);
		defil = new Panel();
		defil.Dock = DockStyle.Fill;
		defil.AutoScroll = true;
		defil.BackColor = C.Fond;
		base.Controls.Add(defil);
		defil.BringToFront();
		Theme.Sombre(defil);
		z = new ZoneDessin();
		z.Location = new Point(X0, 0);
		z.Width = CW - SystemInformation.VerticalScrollBarWidth;
		z.Height = Zx.P(1200);
		z.Dessiner = Dessiner;
		defil.Controls.Add(z);
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			souris = e.Location;
			bool flag = false;
			foreach (KeyValuePair<Rectangle, Action> zone in zones)
			{
				if (zone.Key.Contains(e.Location))
				{
					flag = true;
				}
			}
			z.Cursor = (flag ? Cursors.Hand : Cursors.Default);
			z.Invalidate();
		};
		z.MouseLeave += delegate
		{
			souris = new Point(-1, -1);
			z.Invalidate();
		};
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && !occupe)
			{
				KeyValuePair<Rectangle, Action>[] array = zones.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<Rectangle, Action> keyValuePair = array[i];
					if (keyValuePair.Key.Contains(e.Location))
					{
						Sons.Clic();
						keyValuePair.Value();
						break;
					}
				}
			}
		};
		anim = new System.Windows.Forms.Timer();
		anim.Interval = 40;
		anim.Tick += delegate
		{
			if (!rCosm.IsEmpty)
			{
				z.Invalidate(rCosm);
			}
			if (th != null)
			{
				th.Avancer(0.04, haut.Width, haut.Height);
			}
			haut.Invalidate();
		};
		anim.Start();
		base.Disposed += delegate
		{
			anim.Stop();
			anim.Dispose();
		};
		if (Social.Actif && Social.Connecte)
		{
			Fond(delegate
			{
				Social.EtatPoints();
			}, null);
		}
	}

	private void Msg(string t, Color c)
	{
		lMsg.ForeColor = c;
		lMsg.Text = t;
	}

	private void Fond(Action travail, Action fini)
	{
		occupe = true;
		Thread thread = new Thread((ThreadStart)delegate
		{
			string err = null;
			try
			{
				travail();
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					occupe = false;
					if (err != null)
					{
						Msg("✖  " + err, C.Rouge);
					}
					else if (fini != null)
					{
						fini();
					}
					z.Invalidate();
					haut.Invalidate();
				});
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void DessinerHaut(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.Clear(C.Fond);
		if (th != null)
		{
			th.PeindreFond(g, w, h);
		}
		Lang.DrawText(g, "Boutique", fTitre, new Point(X0 + Zx.P(140), Zx.P(16)), C.Texte);
		Lang.DrawText(g, "DLC et cosmétiques TPS Groupe", fTxt, new Point(X0 + Zx.P(142), Zx.P(52)), C.Gris);
		string t = (Social.Vip ? ("★  " + Social.Points.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + " points VIP") : "Pas encore VIP");
		Size size = Lang.MeasureText(t, fBt);
		Rectangle rectangle = new Rectangle(X0 + CW - Zx.P(24) - size.Width - Zx.P(36), Zx.P(26), size.Width + Zx.P(36), Zx.P(36));
		using (GraphicsPath path = Bouton.Rond(rectangle, Zx.P(18)))
		{
			using (LinearGradientBrush brush = new LinearGradientBrush(rectangle, Social.Vip ? Color.FromArgb(120, 84, 16) : C.Carte, Social.Vip ? Color.FromArgb(70, 46, 8) : C.Champ, 90f))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Social.Vip ? C.Or : C.Ligne, 1.2f);
			g.DrawPath(pen, path);
		}
		Lang.DrawText(g, t, fBt, rectangle, Social.Vip ? Color.FromArgb(255, 230, 160) : C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		if (th != null)
		{
			th.PeindreDevant(g, w, h);
		}
	}

	private void Bt(Graphics g, Rectangle r, string t, Color c, bool plein, Action a)
	{
		bool flag = a != null && r.Contains(souris);
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(8)))
		{
			if (plein)
			{
				using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height + 1), ControlPaint.Light(c, flag ? 0.35f : 0.15f), ControlPaint.Dark(c, 0.1f), 90f);
				g.FillPath(brush, path);
			}
			else
			{
				using (SolidBrush brush2 = new SolidBrush(flag ? C.CarteSurvol : C.Champ))
				{
					g.FillPath(brush2, path);
				}
				using Pen pen = new Pen(flag ? c : Color.FromArgb(120, c));
				g.DrawPath(pen, path);
			}
		}
		Lang.DrawText(g, t, fBt, r, plein ? Color.White : ((a == null) ? C.Gris : c), TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		if (a != null)
		{
			zones.Add(new KeyValuePair<Rectangle, Action>(r, a));
		}
	}

	private static bool LienOk(string l)
	{
		if (Uri.TryCreate(l ?? "", UriKind.Absolute, out var result))
		{
			return result.Scheme == Uri.UriSchemeHttps;
		}
		return false;
	}

	private void Ouvrir(string l, string sinon)
	{
		if (LienOk(l))
		{
			try
			{
				Process.Start(new Uri(l).AbsoluteUri);
				Msg("Page de paiement ouverte dans votre navigateur.", C.Jade);
				return;
			}
			catch
			{
				return;
			}
		}
		Msg(sinon, C.Orange);
	}

	private void Acheter(string article, string nom)
	{
		if (!Social.Actif)
		{
			Msg("Le serveur TPS n'est pas encore configuré.", C.Orange);
			return;
		}
		if (!Social.Vip)
		{
			Demande = "vip";
			base.DialogResult = DialogResult.OK;
			return;
		}
		Msg("Achat de « " + nom + " »…", C.Gris);
		Fond(delegate
		{
			string text = Social.Acheter(article);
			if (text != null)
			{
				throw new Exception(text);
			}
		}, delegate
		{
			Msg("✔  « " + nom + " » acheté. Il vous reste " + Social.Points + " points.", C.Vert);
			Comptes.Sauvegarder();
		});
	}

	private void Equiper(string article)
	{
		Compte actif = Comptes.Actif;
		if (actif == null)
		{
			return;
		}
		string text = article.Substring(article.IndexOf(':') + 1);
		if (article.StartsWith("ban:"))
		{
			actif.BanType = "anim";
			actif.BanMotif = text;
			Msg("✔  Bannière équipée sur le profil « " + actif.Pseudo + " ».", C.Vert);
		}
		else
		{
			actif.Deco = ((actif.Deco == text) ? "" : text);
			Msg((actif.Deco.Length > 0) ? "✔  Décoration équipée." : "Décoration retirée.", C.Vert);
		}
		Comptes.Sauvegarder();
		try
		{
			Progression.Verifier(actif);
		}
		catch
		{
		}
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
		g.Clear(C.Fond);
		zones.Clear();
		int num = Zx.P(24);
		int num2 = w - 48;
		int y = Zx.P(8);
		double temps = BanAnim.Temps;
		Compte actif = Comptes.Actif;
		Rectangle rectangle = new Rectangle(num, y, num2, Zx.P(150));
		using (GraphicsPath path = Bouton.Rond(rectangle, Zx.P(16)))
		{
			GraphicsState gstate = g.Save();
			g.SetClip(path, CombineMode.Intersect);
			BanAnim.Dessiner("or", g, rectangle, temps);
			using (LinearGradientBrush brush = new LinearGradientBrush(rectangle, Color.FromArgb(225, 14, 10, 4), Color.FromArgb(60, 14, 10, 4), 0f))
			{
				g.FillRectangle(brush, rectangle);
			}
			g.Restore(gstate);
			using Pen pen = new Pen(C.Or, 1.4f);
			g.DrawPath(pen, path);
		}
		Deco.Forcer(g, new Rectangle(rectangle.X + Zx.P(40), rectangle.Y + Zx.P(46), Zx.P(64), Zx.P(64)), "couronne", temps);
		if (!Social.Vip)
		{
			Lang.DrawText(g, "TPS VIP  ·  5 € à vie", fGros, new Point(rectangle.X + Zx.P(128), rectangle.Y + Zx.P(20)), Color.FromArgb(255, 232, 170));
			Lang.DrawText(g, "Badge VIP doré, rôle VIP sur le Discord, bannières exclusives, accès anticipé et versions de test,\npoints VIP chaque jour à dépenser ici (cosmétiques, DLC)… et +200 points de bienvenue.", fTxt, new Rectangle(rectangle.X + Zx.P(128), rectangle.Y + Zx.P(54), num2 - Zx.P(160) - Zx.P(220), Zx.P(44)), Color.FromArgb(240, 230, 210), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			Bt(g, new Rectangle(rectangle.Right - Zx.P(24) - Zx.P(200), rectangle.Y + Zx.P(30), Zx.P(200), Zx.P(40)), "Devenir VIP · 5 €", C.Or, plein: true, delegate
			{
				Ouvrir(lienVip, "Le lien d'achat VIP n'est pas encore configuré par TPS Groupe.");
			});
			Bt(g, new Rectangle(rectangle.Right - Zx.P(24) - Zx.P(200), rectangle.Y + Zx.P(80), Zx.P(200), Zx.P(36)), "J'ai un code VIP", C.Or, plein: false, delegate
			{
				Demande = "vip";
				base.DialogResult = DialogResult.OK;
			});
		}
		else
		{
			Lang.DrawText(g, "Membre VIP  ✔", fGros, new Point(rectangle.X + Zx.P(128), rectangle.Y + Zx.P(20)), Color.FromArgb(255, 232, 170));
			Lang.DrawText(g, Social.Points.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + " points  ·  " + Social.PointsTotal.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + " gagnés au total", fNom, new Point(rectangle.X + Zx.P(128), rectangle.Y + Zx.P(58)), Color.White);
			Lang.DrawText(g, "+50 points par jour (bonus du jour " + (Social.BonusPris ? "déjà récupéré ✔" : "à récupérer au prochain démarrage") + ")  ·  +10 par mod aimé  ·  +5 par commentaire (3 par jour)", fTxt, new Rectangle(rectangle.X + Zx.P(128), rectangle.Y + Zx.P(92), num2 - Zx.P(160), Zx.P(36)), Color.FromArgb(240, 230, 210), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		}
		y = rectangle.Bottom + Zx.P(26);
		Lang.DrawText(g, "DLC", fSec, new Point(num + Zx.P(2), y), C.Gris);
		y += Zx.P(24);
		if (dlc.Count == 0)
		{
			Lang.DrawText(g, "Aucun DLC payant pour l'instant : les prochains apparaîtront ici.", fTxt, new Point(num + Zx.P(2), y + Zx.P(6)), C.Gris);
			y += Zx.P(44);
		}
		else
		{
			int num3 = 3;
			int num4 = Zx.P(16);
			int num5 = (num2 - (num3 - 1) * num4) / num3;
			int num6 = num5 * 9 / 16 + 150;
			for (int i = 0; i < dlc.Count; i++)
			{
				Projet projet = dlc[i];
				Rectangle r = new Rectangle(num + i % num3 * (num5 + num4), y + i / num3 * (num6 + num4), num5, num6);
				Rectangle rectangle2 = new Rectangle(r.X, r.Y, num5, num5 * 9 / 16);
				using (GraphicsPath path2 = Bouton.Rond(r, Zx.P(14)))
				{
					using (SolidBrush brush2 = new SolidBrush(C.Carte))
					{
						g.FillPath(brush2, path2);
					}
					GraphicsState gstate2 = g.Save();
					g.SetClip(path2, CombineMode.Intersect);
					Image image = this.image(projet);
					if (image != null)
					{
						double num7 = Math.Max((double)rectangle2.Width / (double)image.Width, (double)rectangle2.Height / (double)image.Height);
						int num8 = (int)((double)image.Width * num7);
						int num9 = (int)((double)image.Height * num7);
						g.InterpolationMode = InterpolationMode.HighQualityBilinear;
						g.DrawImage(image, rectangle2.X + (rectangle2.Width - num8) / 2, rectangle2.Y + (rectangle2.Height - num9) / 2, num8, num9);
					}
					g.Restore(gstate2);
					using Pen pen2 = new Pen(projet.Debloque ? C.Vert : C.Ligne);
					g.DrawPath(pen2, path2);
				}
				Lang.DrawText(g, projet.Nom, fNom, new Rectangle(r.X + Zx.P(14), rectangle2.Bottom + Zx.P(10), num5 - Zx.P(28), Zx.P(24)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				Lang.DrawText(g, projet.Debloque ? "Possédé ✔" : (projet.PrixTexte + ((projet.PrixPoints > 0) ? ("  ·  ou " + projet.PrixPoints + " points VIP") : "")), fTxt, new Rectangle(r.X + Zx.P(14), rectangle2.Bottom + Zx.P(36), num5 - Zx.P(28), Zx.P(18)), projet.Debloque ? C.Vert : C.Or, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
				int num10 = r.Bottom - Zx.P(50);
				int num11 = (num5 - Zx.P(28) - Zx.P(8)) / 2;
				Projet pp = projet;
				if (projet.Debloque)
				{
					Bt(g, new Rectangle(r.X + Zx.P(14), num10, num5 - Zx.P(28), Zx.P(36)), "Voir la page", C.Jade, plein: false, delegate
					{
						Demande = "page:" + pp.Id;
						base.DialogResult = DialogResult.OK;
					});
					continue;
				}
				Bt(g, new Rectangle(r.X + Zx.P(14), num10 - Zx.P(44), num5 - Zx.P(28), Zx.P(36)), "Acheter · " + projet.PrixTexte, C.Orange, plein: true, delegate
				{
					Ouvrir(pp.LienAchat, "Le lien d'achat de ce DLC n'est pas encore configuré.");
				});
				Bt(g, new Rectangle(r.X + Zx.P(14), num10, num11, Zx.P(36)), "J'ai un code", C.Jade, plein: false, delegate
				{
					Demande = "code:" + pp.Id;
					base.DialogResult = DialogResult.OK;
				});
				if (projet.PrixPoints > 0)
				{
					Bt(g, new Rectangle(r.X + Zx.P(14) + num11 + Zx.P(8), num10, num11, Zx.P(36)), projet.PrixPoints + " points", C.Or, plein: false, delegate
					{
						Acheter("mod:" + pp.Id, pp.Nom);
					});
					continue;
				}
				Bt(g, new Rectangle(r.X + Zx.P(14) + num11 + Zx.P(8), num10, num11, Zx.P(36)), "Voir la page", C.Jade, plein: false, delegate
				{
					Demande = "page:" + pp.Id;
					base.DialogResult = DialogResult.OK;
				});
			}
			y += (dlc.Count + num3 - 1) / num3 * (num6 + num4) + Zx.P(10);
		}
		Lang.DrawText(g, "COSMÉTIQUES  ·  RÉSERVÉS AUX MEMBRES VIP", fSec, new Point(num + Zx.P(2), y), C.Gris);
		y += Zx.P(24);
		int num12 = 3;
		int num13 = Zx.P(16);
		int num14 = (num2 - (num12 - 1) * num13) / num12;
		int num15 = Zx.P(214);
		rCosm = new Rectangle(0, y, w, (cosm.GetLength(0) + num12 - 1) / num12 * (num15 + num13));
		for (int j = 0; j < cosm.GetLength(0); j++)
		{
			string text = cosm[j, 0];
			string text2 = cosm[j, 1];
			int prix = int.Parse(cosm[j, 2]);
			string text3 = text.Substring(text.IndexOf(':') + 1);
			bool flag = text.StartsWith("ban:");
			Rectangle r2 = new Rectangle(num + j % num12 * (num14 + num13), y + j / num12 * (num15 + num13), num14, num15);
			Rectangle rectangle3 = new Rectangle(r2.X + Zx.P(12), r2.Y + Zx.P(12), num14 - Zx.P(24), Zx.P(96));
			bool flag2 = ((prix == 0) ? Social.Vip : Social.Possede(text));
			bool flag3 = actif != null && ((!flag) ? (actif.Deco == text3) : (actif.BanType == "anim" && actif.BanMotif == text3));
			using (GraphicsPath path3 = Bouton.Rond(r2, Zx.P(14)))
			{
				using (SolidBrush brush3 = new SolidBrush(C.Carte))
				{
					g.FillPath(brush3, path3);
				}
				using Pen pen3 = new Pen(flag3 ? C.Or : C.Ligne, flag3 ? Zx.P(2f) : 1f);
				g.DrawPath(pen3, path3);
			}
			using (GraphicsPath path4 = Bouton.Rond(rectangle3, Zx.P(10)))
			{
				GraphicsState gstate3 = g.Save();
				g.SetClip(path4, CombineMode.Intersect);
				if (flag)
				{
					BanAnim.Dessiner(text3, g, rectangle3, temps);
				}
				else
				{
					using (LinearGradientBrush brush4 = new LinearGradientBrush(rectangle3, Color.FromArgb(30, 34, 70), Color.FromArgb(14, 16, 36), 90f))
					{
						g.FillRectangle(brush4, rectangle3);
					}
					Rectangle rectangle4 = new Rectangle(rectangle3.X + rectangle3.Width / 2 - Zx.P(30), rectangle3.Y + Zx.P(22), Zx.P(60), Zx.P(60));
					using (SolidBrush brush5 = new SolidBrush(C.Jade))
					{
						g.FillEllipse(brush5, rectangle4);
					}
					Lang.DrawText(g, (actif != null && actif.Pseudo.Length > 0) ? actif.Pseudo.Substring(0, 1).ToUpperInvariant() : "T", fNom, rectangle4, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
					Deco.Forcer(g, rectangle4, text3, temps);
				}
				g.Restore(gstate3);
			}
			Lang.DrawText(g, text2, fNom, new Rectangle(r2.X + Zx.P(14), rectangle3.Bottom + Zx.P(8), num14 - Zx.P(28), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			Lang.DrawText(g, (prix == 0) ? "Inclus avec le VIP" : (prix + " points"), fTxt, new Rectangle(r2.X + Zx.P(14), rectangle3.Bottom + Zx.P(32), num14 - Zx.P(28), Zx.P(22)), C.Or, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			Rectangle r3 = new Rectangle(r2.X + Zx.P(14), r2.Bottom - Zx.P(46), num14 - Zx.P(28), Zx.P(34));
			string a2 = text;
			string n2 = text2;
			if (!Social.Vip)
			{
				Bt(g, r3, "Réservé aux VIP", C.Or, plein: false, delegate
				{
					Demande = "vip";
					base.DialogResult = DialogResult.OK;
				});
				continue;
			}
			if (flag2)
			{
				Bt(g, r3, flag3 ? "Équipé ✔" : "Équiper", C.Or, !flag3, (flag3 && flag) ? null : ((Action)delegate
				{
					Equiper(a2);
					z.Invalidate();
				}));
				continue;
			}
			Bt(g, r3, "Acheter · " + prix + " points", C.Or, plein: true, (Social.Points >= prix) ? ((Action)delegate
			{
				Acheter(a2, n2);
			}) : ((Action)delegate
			{
				Msg("Il vous faut " + (prix - Social.Points) + " points de plus pour « " + n2 + " ».", C.Orange);
			}));
		}
		y += rCosm.Height + Zx.P(10);
		if (z.Height == y + 20)
		{
			return;
		}
		int ny = y + 20;
		BeginInvoke((MethodInvoker)delegate
		{
			if (!z.IsDisposed)
			{
				z.Height = ny;
			}
		});
	}
}
