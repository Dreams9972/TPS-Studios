using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenConnexionMusique : Form
{
	private string quoi;

	private ZoneDessin z;

	private Timer tm;

	private List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private Point souris = new Point(-1, -1);

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(18f));

	private static readonly Font fE = new Font("Segoe UI Semibold", Zx.P(11f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fB = new Font("Segoe UI Semibold", Zx.P(10f));

	private static readonly Font fN = new Font("Segoe UI Semibold", Zx.P(12f));

	private Color Couleur
	{
		get
		{
			if (!(quoi == "apple"))
			{
				return Color.FromArgb(30, 185, 84);
			}
			return Color.FromArgb(250, 45, 85);
		}
	}

	private string Nom
	{
		get
		{
			if (!(quoi == "apple"))
			{
				return "Spotify";
			}
			return "Apple Music";
		}
	}

	public FenConnexionMusique(string service)
	{
		quoi = ((service == "apple") ? "apple" : "spotify");
		Text = "Connecter " + Nom;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(620), Zx.P(470));
		z = new ZoneDessin();
		z.Location = new Point(0, 0);
		z.Size = new Size(Zx.P(620), Zx.P(400));
		z.Dessiner = Dessiner;
		base.Controls.Add(z);
		z.MouseMove += delegate(object o, MouseEventArgs e)
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
		z.MouseLeave += delegate
		{
			souris = new Point(-1, -1);
			z.Invalidate();
		};
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
		bouton.Location = new Point(Zx.P(28), Zx.P(16));
		bouton.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		panel.Controls.Add(bouton);
		Bouton bouton2 = new Bouton("Utiliser " + Nom);
		bouton2.Couleur = Couleur;
		bouton2.Size = new Size(Zx.P(210), Zx.P(38));
		bouton2.Location = new Point(Zx.P(620) - Zx.P(28) - Zx.P(210), Zx.P(16));
		bouton2.Click += delegate
		{
			Config.Ecrire("musique", quoi);
			base.DialogResult = DialogResult.OK;
		};
		panel.Controls.Add(bouton2);
		tm = new Timer();
		tm.Interval = 1000;
		tm.Tick += delegate
		{
			z.Invalidate();
		};
		tm.Start();
		base.FormClosed += delegate
		{
			tm.Stop();
		};
	}

	private static void Ouvrir(string u)
	{
		try
		{
			Process.Start(u);
		}
		catch
		{
		}
	}

	private void Etape(Graphics g, int n, int y, int w, string titre, string aide, string bouton, Action a)
	{
		Etape(g, n, y, w, titre, aide, bouton, a, null, null);
	}

	private void Etape(Graphics g, int n, int y, int w, string titre, string aide, string bouton, Action a, string bouton2, Action a2)
	{
		Rectangle rectangle = new Rectangle(Zx.P(28), y, Zx.P(30), Zx.P(30));
		using (SolidBrush brush = new SolidBrush(Couleur))
		{
			g.FillEllipse(brush, rectangle);
		}
		Lang.DrawText(g, n.ToString(), fN, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		int num = ((bouton != null) ? (Lang.MeasureText(bouton, fB).Width + 32) : 0);
		int num2 = ((bouton2 != null) ? (Lang.MeasureText(bouton2, fB).Width + 32) : 0);
		if (bouton2 != null)
		{
			Rectangle rectangle2 = new Rectangle(w - Zx.P(28) - num - Zx.P(10) - num2, y + Zx.P(6), num2, Zx.P(36));
			bool flag = rectangle2.Contains(souris);
			using (GraphicsPath path = Bouton.Rond(rectangle2, Zx.P(18)))
			{
				using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(flag ? 80 : 36, Couleur)))
				{
					g.FillPath(brush2, path);
				}
				using Pen pen = new Pen(Color.FromArgb(170, Couleur));
				g.DrawPath(pen, path);
			}
			Lang.DrawText(g, bouton2, fB, rectangle2, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			clics.Add(new KeyValuePair<Rectangle, Action>(rectangle2, a2));
		}
		int num3 = num + ((bouton2 != null) ? (num2 + 10) : 0);
		Lang.DrawText(g, titre, fE, new Rectangle(Zx.P(72), y + 1, w - Zx.P(72) - num3 - Zx.P(40), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
		Lang.DrawText(g, aide, fS, new Rectangle(Zx.P(72), y + Zx.P(24), w - Zx.P(72) - num3 - Zx.P(40), Zx.P(52)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		if (bouton == null)
		{
			return;
		}
		Rectangle rectangle3 = new Rectangle(w - Zx.P(28) - num, y + Zx.P(6), num, Zx.P(36));
		bool flag2 = rectangle3.Contains(souris);
		using (GraphicsPath path2 = Bouton.Rond(rectangle3, Zx.P(18)))
		{
			using SolidBrush brush3 = new SolidBrush(flag2 ? ControlPaint.Light(Couleur, 0.15f) : Couleur);
			g.FillPath(brush3, path2);
		}
		Lang.DrawText(g, bouton, fB, rectangle3, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		clics.Add(new KeyValuePair<Rectangle, Action>(rectangle3, a));
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, w, Zx.P(110)), Color.FromArgb(90, Couleur), C.Fond, 90f))
		{
			g.FillRectangle(brush, 0, 0, w, Zx.P(110));
		}
		Rectangle rectangle = new Rectangle(Zx.P(28), Zx.P(26), Zx.P(56), Zx.P(56));
		using (SolidBrush brush2 = new SolidBrush(Couleur))
		{
			g.FillEllipse(brush2, rectangle);
		}
		Lang.DrawText(g, "\uec4f", new Font("Segoe MDL2 Assets", Zx.P(22f)), rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		Lang.DrawText(g, "Connecter " + Nom, fT, new Point(Zx.P(100), Zx.P(28)), C.Texte);
		Lang.DrawText(g, "Connexion sécurisée sur la page officielle de " + Nom + " : le lanceur ne voit jamais ton mot de passe.", fS, new Rectangle(Zx.P(100), Zx.P(64), w - Zx.P(128), Zx.P(36)), C.Gris, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
		bool inst = Musique.Installe(quoi);
		Etape(g, 1, Zx.P(130), w, "Connecte-toi à ton compte", "La page de connexion officielle s'ouvre dans ton navigateur.", "Ouvrir la page de connexion", delegate
		{
			Ouvrir((quoi == "apple") ? "https://music.apple.com/login" : "https://accounts.spotify.com/login");
		});
		Etape(g, 2, Zx.P(210), w, "Écoute avec l'application ou sur le site", "Rien à installer : le lecteur web marche aussi (navigateur). Le lanceur pilote l'un ou l'autre.", inst ? "Application" : "Installer", delegate
		{
			if (inst)
			{
				Musique.Ouvrir(quoi);
			}
			else
			{
				Ouvrir((quoi == "apple") ? "ms-windows-store://pdp/?productid=9PFHDD62MXS1" : "https://www.spotify.com/download/windows/");
			}
		}, "Lecteur web", delegate
		{
			Ouvrir(Musique.LienWeb(quoi));
		});
		Etape(g, 3, Zx.P(290), w, "Lance un titre", "Il s'affiche dans la pilule de l'accueil et dans le mini-lecteur de Windows (icône TPS Studios près de l'horloge).", null, null);
		string text = Musique.EnCours();
		string lecteur = Musique.Lecteur;
		bool flag = lecteur == "Lecteur web" || (quoi == "apple" && lecteur.Length > 0 && lecteur != "Spotify") || (quoi == "spotify" && lecteur == "Spotify");
		string t = ((!flag) ? (Nom + " n'est pas ouvert (application ou site)") : ((lecteur == "Lecteur web" && text.Length > 0) ? ("✔  Lecteur web  ·  en cours : " + text) : ((text.Length > 0) ? ("✔  Connecté  ·  en cours : " + text) : ("✔  " + Nom + " est ouvert  ·  lance un titre"))));
		Rectangle r = new Rectangle(Zx.P(28), Zx.P(356), w - Zx.P(56), Zx.P(34));
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(17)))
		{
			using SolidBrush brush3 = new SolidBrush(Color.FromArgb(flag ? 50 : 25, flag ? C.Vert : C.Gris));
			g.FillPath(brush3, path);
		}
		Lang.DrawText(g, t, fS, new Rectangle(r.X + Zx.P(16), r.Y, r.Width - Zx.P(32), r.Height), flag ? C.Vert : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
	}
}
