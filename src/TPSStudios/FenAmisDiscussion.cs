using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Page Amis : liste compacte à gauche (demandes, amis en ligne, hors ligne, messages non lus) et, à droite,
/// l'ami choisi : bannière, actions (inviter à jouer, profil, retirer) et conversation.
/// </summary>
internal partial class FenAmis
{
	/// <summary>Mods proposés pour une invitation à jouer (identifiant, nom) : fourni par la fenêtre principale.</summary>
	public static Func<List<KeyValuePair<string, string>>> ModsInvitables;

	/// <summary>Nom d'un mod à partir de son identifiant (affichage des invitations).</summary>
	public static Func<string, string> NomMod;

	private Panel pAjout;

	private Panel pConv;

	private ZoneDessin zTete;

	private ZoneDessin zConv;

	private Champ tMsg;

	private Bouton bEnvoyer;

	private Ami selAmi;

	private readonly List<Social.MessageAmi> msgs = new List<Social.MessageAmi>();

	private long dernierMsg;

	private bool convChargee;

	private bool convErreur;

	private int defile;

	private System.Windows.Forms.Timer tConv;

	private readonly List<KeyValuePair<Rectangle, Action>> clicsT = new List<KeyValuePair<Rectangle, Action>>();

	private readonly List<KeyValuePair<Rectangle, Action>> clicsC = new List<KeyValuePair<Rectangle, Action>>();

	private Point sourisT = new Point(-1, -1);

	private Point sourisC = new Point(-1, -1);

	private Dictionary<string, int> nonLus = new Dictionary<string, int>();

	private bool envoiEnCours;

	private static readonly Font fMsg = new Font("Segoe UI", Zx.P(10f));

	private static readonly Font fHeure = new Font("Segoe UI", Zx.P(8f));

	private static readonly Font fTeteNom = new Font("Segoe UI Semibold", Zx.P(17f));

	private static int HautTete => Zx.P(150);

	private void ConstruireDiscussion(Panel corps, Rectangle r)
	{
		pConv = new Panel();
		pConv.Bounds = r;
		pConv.BackColor = C.Fond;
		pConv.Visible = false;
		corps.Controls.Add(pConv);
		zTete = new ZoneDessin();
		zTete.Bounds = new Rectangle(0, 0, r.Width, HautTete);
		zTete.Dessiner = DessinerTete;
		pConv.Controls.Add(zTete);
		zConv = new ZoneDessin();
		zConv.Bounds = new Rectangle(0, HautTete + Zx.P(10), r.Width, r.Height - HautTete - Zx.P(10) - Zx.P(58));
		zConv.Dessiner = DessinerConv;
		pConv.Controls.Add(zConv);
		tMsg = new Champ(multi: false, Zx.P(42));
		tMsg.Location = new Point(0, r.Height - Zx.P(46));
		tMsg.Width = r.Width - Zx.P(130);
		tMsg.MaxLength = 500;
		tMsg.Indice = "Écrire un message…";
		pConv.Controls.Add(tMsg);
		bEnvoyer = new Bouton("Envoyer")
		{
			Size = new Size(Zx.P(118), Zx.P(42)),
			Location = new Point(r.Width - Zx.P(118), r.Height - Zx.P(46))
		};
		pConv.Controls.Add(bEnvoyer);
		bEnvoyer.Click += delegate
		{
			Envoyer();
		};
		tMsg.KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				e.SuppressKeyPress = true;
				Envoyer();
			}
		};
		Brancher(zTete, clicsT, (Point p) => sourisT = p);
		Brancher(zConv, clicsC, (Point p) => sourisC = p);
		zConv.MouseWheel += delegate(object o, MouseEventArgs e)
		{
			defile = Math.Max(0, defile + ((e.Delta > 0) ? Zx.P(60) : (-Zx.P(60))));
			zConv.Invalidate();
		};
		zConv.MouseEnter += delegate
		{
			zConv.Focus();
		};
		tConv = new System.Windows.Forms.Timer();
		tConv.Interval = 3000;
		tConv.Tick += delegate
		{
			if (selAmi != null && pConv.Visible)
			{
				ChargerConversation(tout: false);
			}
		};
		tConv.Start();
		base.FormClosed += delegate
		{
			tConv.Stop();
		};
	}

	private void Brancher(ZoneDessin z, List<KeyValuePair<Rectangle, Action>> clics, Action<Point> souris)
	{
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			souris(e.Location);
			bool sur = false;
			foreach (KeyValuePair<Rectangle, Action> c in clics)
			{
				if (c.Key.Contains(e.Location))
				{
					sur = true;
				}
			}
			z.Cursor = (sur ? Cursors.Hand : Cursors.Default);
			z.Invalidate();
		};
		z.MouseLeave += delegate
		{
			souris(new Point(-1, -1));
			z.Invalidate();
		};
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			foreach (KeyValuePair<Rectangle, Action> c in new List<KeyValuePair<Rectangle, Action>>(clics))
			{
				if (c.Key.Contains(e.Location))
				{
					Sons.Clic();
					c.Value();
					break;
				}
			}
		};
	}

	/// <summary>Choisit un ami (ou null : panneau « ajouter un ami »).</summary>
	private void Selectionner(Ami a)
	{
		selAmi = a;
		msgs.Clear();
		dernierMsg = 0L;
		convChargee = false;
		defile = 0;
		pAjout.Visible = a == null;
		pConv.Visible = a != null;
		zl.Invalidate();
		if (a != null)
		{
			zTete.Invalidate();
			zConv.Invalidate();
			nonLus.Remove(a.Id);
			ChargerConversation(tout: true);
			tMsg.Focus();
		}
	}

	private void ChargerConversation(bool tout)
	{
		Ami a = selAmi;
		if (a == null)
		{
			return;
		}
		long apres = (tout ? 0 : dernierMsg);
		List<Social.MessageAmi> recus = null;
		Fond(delegate
		{
			string text = Social.Assurer(Comptes.Actif);
			if (text != null)
			{
				return text;
			}
			recus = Social.Messages(a.Id, apres);
			return (string)null;
		}, delegate
		{
			if (selAmi != a)
			{
				return;
			}
			if (recus == null)
			{
				if (tout)
				{
					convChargee = true;
					convErreur = true;
					zConv.Invalidate();
				}
				return;
			}
			convErreur = false;
			if (tout)
			{
				msgs.Clear();
			}
			foreach (Social.MessageAmi m in recus)
			{
				if (m.Id > dernierMsg)
				{
					msgs.Add(m);
					dernierMsg = m.Id;
				}
			}
			convChargee = true;
			if (recus.Count > 0)
			{
				defile = 0;
			}
			zConv.Invalidate();
		});
	}

	private void Envoyer(string type = "texte", string extra = "", string texteImpose = null)
	{
		Ami a = selAmi;
		string t = texteImpose ?? tMsg.Text.Trim();
		if (a == null || envoiEnCours || (type == "texte" && t.Length == 0))
		{
			return;
		}
		envoiEnCours = true;
		if (texteImpose == null)
		{
			tMsg.Text = "";
		}
		Fond(delegate
		{
			string text = Social.Assurer(Comptes.Actif);
			if (text != null)
			{
				return text;
			}
			return Social.EnvoyerMessage(a.Id, t, type, extra);
		}, delegate
		{
			envoiEnCours = false;
			ChargerConversation(tout: false);
		});
	}

	/// <summary>Inviter l'ami à jouer : menu des mods (installés en premier).</summary>
	private void Inviter(Rectangle r)
	{
		if (selAmi == null)
		{
			return;
		}
		ContextMenuStrip menu = new ContextMenuStrip();
		menu.ShowImageMargin = false;
		menu.Font = new Font("Segoe UI", Zx.P(10f));
		menu.Items.Add(Lang.T("Inviter sur TPS Groupe")).Click += delegate
		{
			Envoyer("invitation", "", "Rejoins-moi sur le lanceur TPS Groupe !");
		};
		List<KeyValuePair<string, string>> mods = ((ModsInvitables != null) ? ModsInvitables() : new List<KeyValuePair<string, string>>());
		if (mods.Count > 0)
		{
			menu.Items.Add(new ToolStripSeparator());
			foreach (KeyValuePair<string, string> m in mods)
			{
				KeyValuePair<string, string> mm = m;
				menu.Items.Add(Lang.T("Jouer à ") + mm.Value).Click += delegate
				{
					Envoyer("invitation", mm.Key, "On joue à " + mm.Value + " ?");
				};
			}
		}
		menu.Show(zTete, new Point(r.X, r.Bottom + Zx.P(4)));
	}

	private void DessinerTete(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clicsT.Clear();
		Ami a = selAmi;
		if (a == null)
		{
			return;
		}
		Rectangle rb = new Rectangle(0, 0, w - 1, h - 1);
		using (GraphicsPath p = Bouton.Rond(rb, Zx.P(16)))
		{
			GraphicsState st = g.Save();
			g.SetClip(p);
			DessinerBanniere(g, a, rb);
			using (LinearGradientBrush lg = new LinearGradientBrush(rb, Color.FromArgb(40, 6, 8, 20), Color.FromArgb(215, 6, 8, 20), 90f))
			{
				g.FillRectangle(lg, rb);
			}
			g.Restore(st);
			using Pen pn = new Pen(Color.FromArgb(40, 255, 255, 255));
			g.DrawPath(pn, p);
		}
		int ta = Zx.P(72);
		Rectangle ra = new Rectangle(Zx.P(22), h - ta - Zx.P(20), ta, ta);
		Avatar(g, a, ra, point: true, new Font("Segoe UI Semibold", Zx.P(24f)));
		int x = ra.Right + Zx.P(16);
		Lang.DrawText(g, a.Pseudo, fTeteNom, new Rectangle(x, ra.Y + Zx.P(4), w / 2, Zx.P(34)), Color.White, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		string st2 = (a.EnLigne ? ((a.Statut.Length > 0) ? a.Statut : "En ligne") : "Hors ligne");
		Color cs = ((!a.EnLigne) ? C.Gris : (a.Statut.StartsWith("En jeu") ? C.Orange : C.Vert));
		Lang.DrawText(g, "●  " + st2 + "   ·   Niv. " + a.Niveau, fSt, new Point(x, ra.Y + Zx.P(42)), cs, TextFormatFlags.NoPrefix);
		// actions
		int droite = w - Zx.P(18);
		int yb = h - Zx.P(52);
		Rectangle r1 = Bt(g, clicsT, sourisT, droite, yb, "Retirer", C.Rouge, plein: false, delegate
		{
			Ami cible = selAmi;
			if (cible != null && MessageBox.Show(this, Lang.T("Retirer ") + cible.Pseudo + Lang.T(" de vos amis ? La conversation sera effacée."), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				Agir(() => Social.RetirerAmi(cible.Id), cible.Pseudo + " a été retiré de vos amis");
				Selectionner(null);
			}
		});
		Rectangle r2 = Bt(g, clicsT, sourisT, r1.X - Zx.P(10), yb, "Voir le profil", C.Bleu, plein: false, delegate
		{
			if (selAmi != null)
			{
				Profil(selAmi);
			}
		});
		Rectangle rInv = Rectangle.Empty;
		rInv = Bt(g, clicsT, sourisT, r2.X - Zx.P(10), yb, "🎮  Inviter à jouer", C.Vert, plein: true, delegate
		{
			Inviter(rInv);
		});
	}

	private void DessinerConv(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clicsC.Clear();
		using (GraphicsPath pf = Bouton.Rond(new Rectangle(0, 0, w - 1, h - 1), Zx.P(14)))
		{
			using SolidBrush bf = new SolidBrush(Color.FromArgb(120, C.Carte));
			g.FillPath(bf, pf);
		}
		if (selAmi == null)
		{
			return;
		}
		if (!convChargee)
		{
			Lang.DrawText(g, "Chargement de la conversation…", fSt, new Rectangle(0, 0, w, h), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			return;
		}
		if (convErreur && msgs.Count == 0)
		{
			Lang.DrawText(g, "Conversation indisponible pour le moment : le serveur TPS Groupe ne répond pas.\nElle reviendra toute seule.", fSt, new Rectangle(0, 0, w, h), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
			return;
		}
		if (msgs.Count == 0)
		{
			Lang.DrawText(g, "Pas encore de message avec " + selAmi.Pseudo + ".\nDites bonjour ou invitez-le à jouer !", fSt, new Rectangle(0, 0, w, h), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
			return;
		}
		GraphicsState st = g.Save();
		g.SetClip(new Rectangle(0, Zx.P(6), w, h - Zx.P(12)));
		int maxW = Math.Max(Zx.P(200), w * 62 / 100);
		int y = h - Zx.P(14) + defile;
		for (int i = msgs.Count - 1; i >= 0 && y > -Zx.P(200); i--)
		{
			Social.MessageAmi m = msgs[i];
			bool inv = m.Type == "invitation";
			string t = m.Texte;
			string nomMod = ((inv && m.Extra.Length > 0 && NomMod != null) ? NomMod(m.Extra) : null);
			if (inv)
			{
				t = "🎮  " + (m.Moi ? "Invitation envoyée" : (selAmi.Pseudo + " vous invite à jouer")) + ((nomMod != null) ? (" · " + nomMod) : "") + ((t.Length > 0) ? ("\n" + t) : "");
			}
			Size sz = Lang.MeasureText(t, fMsg, new Size(maxW - Zx.P(28), 2000), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			int bw = Math.Max(Zx.P(60), sz.Width + Zx.P(28));
			int bh = sz.Height + Zx.P(20) + ((inv && !m.Moi && nomMod != null) ? Zx.P(40) : 0);
			string heure = DateTimeOffset.FromUnixTimeSeconds(m.Cree).ToLocalTime().ToString("dd/MM HH:mm");
			y -= Zx.P(18);
			int xb = (m.Moi ? (w - Zx.P(14) - bw) : Zx.P(14));
			Lang.DrawText(g, heure, fHeure, new Rectangle(xb, y + Zx.P(2), bw, Zx.P(14)), C.Gris, (m.Moi ? TextFormatFlags.Right : TextFormatFlags.Left) | TextFormatFlags.NoPadding);
			y -= bh;
			Rectangle rb = new Rectangle(xb, y, bw, bh);
			using (GraphicsPath pb = Bouton.Rond(rb, Zx.P(14)))
			{
				Color fond = (inv ? Color.FromArgb(m.Moi ? 150 : 120, 40, 140, 90) : (m.Moi ? Color.FromArgb(235, 52, 70, 170) : Color.FromArgb(235, 34, 40, 74)));
				using SolidBrush bb = new SolidBrush(fond);
				g.FillPath(bb, pb);
			}
			Lang.DrawText(g, t, fMsg, new Rectangle(rb.X + Zx.P(14), rb.Y + Zx.P(10), rb.Width - Zx.P(28), sz.Height + Zx.P(2)), Color.White, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			if (inv && !m.Moi && nomMod != null)
			{
				string idMod = m.Extra;
				Bt(g, clicsC, sourisC, rb.Right - Zx.P(12), rb.Bottom - Zx.P(40), "Voir le mod  ›", C.Vert, plein: true, delegate
				{
					Demande = "mod:" + idMod;
					base.DialogResult = DialogResult.OK;
				});
			}
			y -= Zx.P(12);
		}
		g.Restore(st);
		// on ne peut pas remonter au-delà du premier message
		if (y > Zx.P(6) && defile > 0)
		{
			defile = Math.Max(0, defile - (y - Zx.P(6)));
		}
	}

	private int HauteurListe()
	{
		int n = Social.Recues.Count + Social.Amis.Count + Social.Envoyees.Count;
		return Zx.P(20) + n * Zx.P(66) + Zx.P(34) * 4 + Zx.P(80);
	}

	/// <summary>Une ligne de la liste : avatar, pseudo, statut, messages non lus ; clic : choisir l'ami.</summary>
	private int LigneAmi(Graphics g, Ami a, int y, int w, string mode)
	{
		Rectangle r = new Rectangle(0, y, w - 1, Zx.P(60));
		bool sv = r.Contains(sourisL);
		bool choisi = selAmi != null && selAmi.Id == a.Id;
		using (GraphicsPath p = Bouton.Rond(r, Zx.P(12)))
		{
			using SolidBrush b = new SolidBrush(choisi ? Color.FromArgb(70, 90, 110, 230) : (sv ? C.CarteSurvol : Color.FromArgb(150, C.Carte)));
			g.FillPath(b, p);
			if (choisi)
			{
				using Pen pn = new Pen(Color.FromArgb(160, 120, 140, 255));
				g.DrawPath(pn, p);
			}
		}
		Avatar(g, a, new Rectangle(Zx.P(10), y + Zx.P(10), Zx.P(40), Zx.P(40)), mode == "ami", fIni);
		int xt = Zx.P(60);
		int droite = w - Zx.P(10);
		if (mode == "recue")
		{
			Rectangle rr = Bt(g, clicsL, sourisL, droite, y + Zx.P(14), "✕", C.Rouge, plein: false, delegate
			{
				Agir(() => Social.RetirerAmi(a.Id), "Demande de " + a.Pseudo + " refusée");
			});
			Rectangle ra = Bt(g, clicsL, sourisL, rr.X - Zx.P(6), y + Zx.P(14), "Accepter", C.Vert, plein: true, delegate
			{
				Agir(() => Social.AccepterAmi(a.Id), "✔  " + a.Pseudo + " est maintenant votre ami");
			});
			droite = ra.X - Zx.P(6);
		}
		else if (mode == "envoyee")
		{
			Rectangle rc = Bt(g, clicsL, sourisL, droite, y + Zx.P(14), "Annuler", C.Gris, plein: false, delegate
			{
				Agir(() => Social.RetirerAmi(a.Id), "Demande à " + a.Pseudo + " annulée");
			});
			droite = rc.X - Zx.P(6);
		}
		else if (nonLus.TryGetValue(a.Id, out var nl) && nl > 0)
		{
			string tn = ((nl > 9) ? "9+" : nl.ToString());
			Rectangle rn = new Rectangle(droite - Zx.P(24), y + Zx.P(19), Zx.P(24), Zx.P(22));
			using (GraphicsPath pn2 = Bouton.Rond(rn, Zx.P(11)))
			{
				using SolidBrush bn = new SolidBrush(C.Rouge);
				g.FillPath(bn, pn2);
			}
			Lang.DrawText(g, tn, fBt, rn, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			droite = rn.X - Zx.P(6);
		}
		Lang.DrawText(g, a.Pseudo, fNom, new Rectangle(xt, y + Zx.P(11), Math.Max(Zx.P(40), droite - xt), Zx.P(20)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		string sous = ((mode == "recue") ? "Veut vous ajouter" : ((mode == "envoyee") ? "Demande envoyée" : (a.EnLigne ? ((a.Statut.Length > 0) ? a.Statut : "En ligne") : "Hors ligne")));
		Color cs = ((mode != "ami") ? C.Gris : ((!a.EnLigne) ? C.Gris : (a.Statut.StartsWith("En jeu") ? C.Orange : C.Vert)));
		Lang.DrawText(g, sous, fSt, new Rectangle(xt, y + Zx.P(33), Math.Max(Zx.P(40), droite - xt), Zx.P(18)), cs, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		if (mode == "ami")
		{
			Ami aa = a;
			clicsL.Add(new KeyValuePair<Rectangle, Action>(r, delegate
			{
				Selectionner(aa);
			}));
		}
		return y + Zx.P(66);
	}

	private void DessinerListe(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clicsL.Clear();
		int y = Zx.P(4);
		if (Social.Recues.Count > 0)
		{
			Lang.DrawText(g, "DEMANDES REÇUES  ·  " + Social.Recues.Count, fRub, new Point(Zx.P(2), y), C.Orange);
			y += Zx.P(24);
			foreach (Ami a in Social.Recues)
			{
				y = LigneAmi(g, a, y, w, "recue");
			}
			y += Zx.P(10);
		}
		List<Ami> enLigne = Social.Amis.FindAll((Ami a) => a.EnLigne);
		List<Ami> horsLigne = Social.Amis.FindAll((Ami a) => !a.EnLigne);
		Comparison<Ami> tri = (Ami a, Ami b) => string.Compare(a.Pseudo, b.Pseudo, StringComparison.OrdinalIgnoreCase);
		enLigne.Sort(tri);
		horsLigne.Sort(tri);
		if (Social.Amis.Count == 0)
		{
			Lang.DrawText(g, "Pas encore d'amis.\nCliquez sur « + Ajouter un ami » : par leur code ami ou leur pseudo.", fSt, new Rectangle(Zx.P(4), y + Zx.P(6), w - Zx.P(8), Zx.P(60)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			y += Zx.P(70);
		}
		if (enLigne.Count > 0)
		{
			Lang.DrawText(g, "EN LIGNE  ·  " + enLigne.Count, fRub, new Point(Zx.P(2), y), C.Vert);
			y += Zx.P(24);
			foreach (Ami a2 in enLigne)
			{
				y = LigneAmi(g, a2, y, w, "ami");
			}
			y += Zx.P(10);
		}
		if (horsLigne.Count > 0)
		{
			Lang.DrawText(g, "HORS LIGNE  ·  " + horsLigne.Count, fRub, new Point(Zx.P(2), y), C.Gris);
			y += Zx.P(24);
			foreach (Ami a3 in horsLigne)
			{
				y = LigneAmi(g, a3, y, w, "ami");
			}
			y += Zx.P(10);
		}
		if (Social.Envoyees.Count > 0)
		{
			Lang.DrawText(g, "DEMANDES ENVOYÉES  ·  " + Social.Envoyees.Count, fRub, new Point(Zx.P(2), y), C.Gris);
			y += Zx.P(24);
			foreach (Ami a4 in Social.Envoyees)
			{
				y = LigneAmi(g, a4, y, w, "envoyee");
			}
		}
	}

	/// <summary>Messages non lus (après chaque rechargement de la liste).</summary>
	private void MajNonLus()
	{
		Dictionary<string, int> d = null;
		Fond(delegate
		{
			Dictionary<string, int> r = new Dictionary<string, int>();
			foreach (Social.NonLu n in Social.NonLus())
			{
				r[n.Id] = n.N;
			}
			d = r;
			return (string)null;
		}, delegate
		{
			if (d != null)
			{
				if (selAmi != null)
				{
					d.Remove(selAmi.Id);
				}
				nonLus = d;
				zl.Invalidate();
			}
		});
	}
}
