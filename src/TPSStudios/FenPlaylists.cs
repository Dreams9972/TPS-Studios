using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenPlaylists : Form
{
	private Champ tNom;

	private Champ tLien;

	private ZoneDessin z;

	private Label lErr;

	private List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private Point souris = new Point(-1, -1);

	private static readonly Font fN = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fB = new Font("Segoe UI Semibold", Zx.P(9f));

	public FenPlaylists()
	{
		Text = "Mes playlists";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(640), Zx.P(600));
		Label value = new Label
		{
			Text = "Mes playlists",
			Font = new Font("Segoe UI Semibold", Zx.P(16f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(20)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Ajoute une fois tes playlists Spotify ou Apple Music : tu les lances ensuite d'un clic depuis la pilule de l'accueil.\nLien : Spotify › clic droit sur la playlist › Partager › Copier le lien  ·  Apple Music › ··· › Partager › Copier le lien.",
			Font = new Font("Segoe UI", Zx.P(9f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(56)),
			Size = new Size(Zx.P(584), Zx.P(70))
		};
		base.Controls.Add(value2);
		tNom = new Champ(multi: false, Zx.P(38));
		tNom.Location = new Point(Zx.P(28), Zx.P(132));
		tNom.Width = Zx.P(170);
		tNom.Indice = "Nom (ex. Conduite)";
		tNom.MaxLength = 40;
		base.Controls.Add(tNom);
		tLien = new Champ(multi: false, Zx.P(38));
		tLien.Location = new Point(Zx.P(206), Zx.P(132));
		tLien.Width = Zx.P(290);
		tLien.Indice = "https://open.spotify.com/playlist/…";
		tLien.MaxLength = 300;
		base.Controls.Add(tLien);
		Bouton bouton = new Bouton("Ajouter")
		{
			Size = new Size(Zx.P(108), Zx.P(38)),
			Location = new Point(Zx.P(504), Zx.P(132))
		};
		base.Controls.Add(bouton);
		lErr = new Label();
		lErr.Font = new Font("Segoe UI", Zx.P(9f));
		lErr.ForeColor = C.Rouge;
		lErr.BackColor = C.Fond;
		lErr.Location = new Point(Zx.P(28), Zx.P(176));
		lErr.Size = new Size(Zx.P(584), Zx.P(20));
		base.Controls.Add(lErr);
		EventHandler value3 = delegate
		{
			string text = tNom.Text.Trim();
			string text2 = tLien.Text.Trim();
			if (text.Length == 0)
			{
				text = (text2.Contains("apple") ? "Playlist Apple Music" : "Playlist Spotify");
			}
			if (!Musique.LienValide(text2))
			{
				lErr.ForeColor = C.Rouge;
				lErr.Text = "Lien invalide : colle un lien de partage Spotify (open.spotify.com) ou Apple Music (music.apple.com).";
			}
			else
			{
				List<string[]> list = Musique.Playlists();
				if (list.Count >= 20)
				{
					lErr.Text = "20 playlists au maximum.";
				}
				else
				{
					list.Add(new string[2] { text, text2 });
					Musique.SauverPlaylists(list);
					tNom.Text = "";
					tLien.Text = "";
					lErr.ForeColor = C.Vert;
					lErr.Text = "✔  « " + text + " » ajoutée";
					z.Invalidate();
				}
			}
		};
		bouton.Click += value3;
		z = new ZoneDessin();
		z.Location = new Point(Zx.P(28), Zx.P(202));
		z.Size = new Size(Zx.P(584), Zx.P(600) - Zx.P(202) - Zx.P(70) - Zx.P(10));
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
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			foreach (KeyValuePair<Rectangle, Action> item in new List<KeyValuePair<Rectangle, Action>>(clics))
			{
				if (item.Key.Contains(e.Location))
				{
					Sons.Clic();
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
		Bouton bouton2 = new Bouton("Fermer");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(110), Zx.P(38));
		bouton2.Location = new Point(Zx.P(28), Zx.P(16));
		bouton2.Click += delegate
		{
			base.DialogResult = DialogResult.OK;
		};
		panel.Controls.Add(bouton2);
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		List<string[]> list = Musique.Playlists();
		int num = 0;
		if (list.Count == 0)
		{
			Lang.DrawText(g, "Aucune playlist pour l'instant.", fS, new Point(Zx.P(2), Zx.P(4)), C.Gris);
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (num + 50 > h)
			{
				break;
			}
			string[] array = list[i];
			int idx = i;
			bool flag = !array[1].Contains("apple");
			Color color = (flag ? Color.FromArgb(30, 185, 84) : Color.FromArgb(250, 45, 85));
			Rectangle r = new Rectangle(0, num, w - 1, Zx.P(46));
			using (GraphicsPath path = Bouton.Rond(r, Zx.P(10)))
			{
				using (SolidBrush brush = new SolidBrush(C.Carte))
				{
					g.FillPath(brush, path);
				}
				using Pen pen = new Pen(Color.FromArgb(110, C.Ligne));
				g.DrawPath(pen, path);
			}
			Rectangle rectangle = new Rectangle(Zx.P(10), num + Zx.P(9), Zx.P(28), Zx.P(28));
			using (SolidBrush brush2 = new SolidBrush(color))
			{
				g.FillEllipse(brush2, rectangle);
			}
			Lang.DrawText(g, "\uec4f", new Font("Segoe MDL2 Assets", Zx.P(11f)), rectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Lang.DrawText(g, array[0], fN, new Rectangle(Zx.P(48), num + Zx.P(5), w - Zx.P(48) - Zx.P(210), Zx.P(20)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
			Lang.DrawText(g, flag ? "Spotify" : "Apple Music", fS, new Rectangle(Zx.P(48), num + Zx.P(25), w - Zx.P(48) - Zx.P(210), Zx.P(16)), color, TextFormatFlags.NoPadding);
			Rectangle rectangle2 = new Rectangle(w - Zx.P(10) - Zx.P(90), num + Zx.P(8), Zx.P(90), Zx.P(30));
			Rectangle rectangle3 = new Rectangle(rectangle2.X - Zx.P(8) - Zx.P(90), num + Zx.P(8), Zx.P(90), Zx.P(30));
			Rectangle[] array2 = new Rectangle[2] { rectangle3, rectangle2 };
			for (int j = 0; j < array2.Length; j++)
			{
				Rectangle rectangle4 = array2[j];
				bool flag2 = rectangle4 == rectangle3;
				bool flag3 = rectangle4.Contains(souris);
				Color color2 = (flag2 ? color : C.Rouge);
				using (GraphicsPath path2 = Bouton.Rond(rectangle4, Zx.P(15)))
				{
					using (SolidBrush brush3 = new SolidBrush((!flag2) ? Color.FromArgb(flag3 ? 70 : 34, color2) : (flag3 ? ControlPaint.Light(color2, 0.15f) : color2)))
					{
						g.FillPath(brush3, path2);
					}
					using Pen pen2 = new Pen(Color.FromArgb(160, color2));
					g.DrawPath(pen2, path2);
				}
				Lang.DrawText(g, flag2 ? "Ouvrir" : "Retirer", fB, rectangle4, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			string lien = array[1];
			clics.Add(new KeyValuePair<Rectangle, Action>(rectangle3, delegate
			{
				Musique.OuvrirPlaylist(lien);
			}));
			clics.Add(new KeyValuePair<Rectangle, Action>(rectangle2, delegate
			{
				List<string[]> list2 = Musique.Playlists();
				if (idx < list2.Count)
				{
					list2.RemoveAt(idx);
					Musique.SauverPlaylists(list2);
				}
			}));
			num += Zx.P(54);
		}
	}
}
