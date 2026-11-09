using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenMusique : Form
{
	private ZoneDessin z;

	private Timer tm;

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(12f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(16f));

	private static readonly Font fGp = new Font("Segoe MDL2 Assets", Zx.P(22f));

	private List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private Point souris = new Point(-1, -1);

	public FenMusique()
	{
		Text = "Musique";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(520), Zx.P(360));
		Label value = new Label
		{
			Text = "Musique",
			Font = new Font("Segoe UI Semibold", Zx.P(16f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(20)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Pilote Spotify ou Apple Music sans quitter le lanceur (ou le jeu).",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(56)),
			AutoSize = true
		};
		base.Controls.Add(value2);
		z = new ZoneDessin();
		z.Location = new Point(Zx.P(28), Zx.P(88));
		z.Size = new Size(Zx.P(464), Zx.P(186));
		z.Dessiner = Dessiner;
		base.Controls.Add(z);
		ZoneDessin zoneDessin = z;
		MouseEventHandler value3 = delegate(object o, MouseEventArgs e)
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
		zoneDessin.MouseMove += value3;
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
					item.Value();
					z.Invalidate();
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
		Bouton bouton2 = new Bouton("Spotify");
		bouton2.Couleur = Color.FromArgb(30, 185, 84);
		bouton2.Size = new Size(Zx.P(130), Zx.P(38));
		bouton2.Location = new Point(Zx.P(520) - Zx.P(28) - Zx.P(130) - Zx.P(10) - Zx.P(150), Zx.P(16));
		bouton2.Click += delegate
		{
			Musique.Ouvrir("spotify");
		};
		panel.Controls.Add(bouton2);
		Bouton bouton3 = new Bouton("Apple Music");
		bouton3.Couleur = Color.FromArgb(250, 45, 85);
		bouton3.Size = new Size(Zx.P(150), Zx.P(38));
		bouton3.Location = new Point(Zx.P(520) - Zx.P(28) - Zx.P(150), Zx.P(16));
		bouton3.Click += delegate
		{
			Musique.Ouvrir("apple");
		};
		panel.Controls.Add(bouton3);
		tm = new Timer();
		tm.Interval = 400;
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

	private Rectangle Rond(Graphics g, Rectangle r, string glyphe, Font f, Color c, bool plein, Action a)
	{
		bool flag = r.Contains(souris);
		using (SolidBrush brush = new SolidBrush((!plein) ? Color.FromArgb(flag ? 70 : 34, c) : (flag ? ControlPaint.Light(c, 0.15f) : c)))
		{
			g.FillEllipse(brush, r);
		}
		using (Pen pen = new Pen(Color.FromArgb(160, c)))
		{
			g.DrawEllipse(pen, r);
		}
		Lang.DrawText(g, glyphe, f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		clics.Add(new KeyValuePair<Rectangle, Action>(r, a));
		return r;
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		string text = Musique.EnCours();
		string lecteur = Musique.Lecteur;
		Rectangle r = new Rectangle(0, 0, w - 1, Zx.P(78));
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
		{
			using (SolidBrush brush = new SolidBrush(C.Carte))
			{
				g.FillPath(brush, path);
			}
			using Pen pen = new Pen(Color.FromArgb(120, C.Ligne));
			g.DrawPath(pen, path);
		}
		Rectangle rectangle = new Rectangle(Zx.P(12), Zx.P(11), Zx.P(56), Zx.P(56));
		Color color = ((lecteur == "Spotify") ? Color.FromArgb(30, 185, 84) : ((lecteur.Length > 0) ? Color.FromArgb(250, 45, 85) : Color.FromArgb(70, 80, 110)));
		using (GraphicsPath path2 = Bouton.Rond(rectangle, Zx.P(10)))
		{
			using LinearGradientBrush brush2 = new LinearGradientBrush(rectangle, color, ControlPaint.Dark(color, 0.3f), 45f);
			g.FillPath(brush2, path2);
		}
		Lang.DrawText(g, "\uec4f", fGp, rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		string t = text;
		string text2 = "";
		int num = text.IndexOf(" - ");
		if (num > 0)
		{
			text2 = text.Substring(0, num);
			t = text.Substring(num + 3);
		}
		if (text.Length == 0)
		{
			t = ((lecteur.Length > 0) ? (lecteur + " est ouvert") : "Aucune musique en cours");
			text2 = ((lecteur.Length > 0) ? "En pause, ou lance un titre" : "Ouvre Spotify ou Apple Music ci-dessous");
		}
		else
		{
			text2 = ((text2.Length > 0) ? (text2 + "  ·  ") : "") + lecteur;
		}
		Lang.DrawText(g, t, fT, new Rectangle(Zx.P(82), Zx.P(16), w - Zx.P(96), Zx.P(24)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		Lang.DrawText(g, text2, fS, new Rectangle(Zx.P(82), Zx.P(44), w - Zx.P(96), Zx.P(20)), (text.Length > 0) ? color : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int num2 = Zx.P(104);
		int num3 = w / 2;
		Rond(g, new Rectangle(num3 - Zx.P(26), num2, Zx.P(52), Zx.P(52)), (text.Length > 0) ? "\ue769" : "\ue768", fG, C.Orange, plein: true, delegate
		{
			Musique.Basculer();
		});
		Rond(g, new Rectangle(num3 - Zx.P(26) - Zx.P(18) - Zx.P(44), num2 + Zx.P(4), Zx.P(44), Zx.P(44)), "\ue892", fG, C.Jade, plein: false, delegate
		{
			Musique.Touche(177);
		});
		Rond(g, new Rectangle(num3 + Zx.P(26) + Zx.P(18), num2 + Zx.P(4), Zx.P(44), Zx.P(44)), "\ue893", fG, C.Jade, plein: false, delegate
		{
			Musique.Touche(176);
		});
		Rond(g, new Rectangle(num3 - Zx.P(26) - Zx.P(18) - Zx.P(44) - Zx.P(18) - Zx.P(40), num2 + Zx.P(6), Zx.P(40), Zx.P(40)), "\ue993", fG, C.Gris, plein: false, delegate
		{
			Musique.Touche(174);
		});
		Rond(g, new Rectangle(num3 + Zx.P(26) + Zx.P(18) + Zx.P(44) + Zx.P(18), num2 + Zx.P(6), Zx.P(40), Zx.P(40)), "\ue995", fG, C.Gris, plein: false, delegate
		{
			Musique.Touche(175);
		});
		Lang.DrawText(g, "Ces boutons pilotent le lecteur ouvert (touches multimédia).", fS, new Rectangle(Zx.P(16), Zx.P(164), w - Zx.P(32), Zx.P(40)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
	}
}
