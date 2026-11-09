using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenNotifs : Form
{
	public string Demande = "";

	private ZoneDessin z;

	private Panel defil;

	private List<KeyValuePair<Rectangle, string>> zones = new List<KeyValuePair<Rectangle, string>>();

	private Point souris = new Point(-1, -1);

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fX = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(15f));

	public FenNotifs()
	{
		Text = "Notifications";
		base.ClientSize = new Size(Zx.P(600), Zx.P(620));
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		Label value = new Label
		{
			Text = "Notifications",
			Font = new Font("Segoe UI Semibold", Zx.P(17f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(26), Zx.P(18)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Bouton bouton = new Bouton("Réglages…")
		{
			Plein = false,
			Size = new Size(Zx.P(120), Zx.P(34)),
			Location = new Point(base.ClientSize.Width - Zx.P(26) - Zx.P(120), Zx.P(22))
		};
		EventHandler value2 = delegate
		{
			Demande = "reglages";
			base.DialogResult = DialogResult.OK;
		};
		bouton.Click += value2;
		base.Controls.Add(bouton);
		Bouton bouton2 = new Bouton("Tout effacer");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(130), Zx.P(34));
		bouton2.Location = new Point(base.ClientSize.Width - Zx.P(26) - Zx.P(120) - Zx.P(10) - Zx.P(130), Zx.P(22));
		bouton2.Click += delegate
		{
			Notifs.Vider();
			z.Height = Zx.P(200);
			z.Invalidate();
		};
		base.Controls.Add(bouton2);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(64),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton3 = new Bouton("Fermer");
		bouton3.Size = new Size(Zx.P(130), Zx.P(38));
		bouton3.Location = new Point(base.ClientSize.Width - Zx.P(26) - Zx.P(130), Zx.P(13));
		bouton3.Click += delegate
		{
			base.DialogResult = DialogResult.OK;
		};
		panel.Controls.Add(bouton3);
		Label value3 = new Label
		{
			Text = "Choisissez les mods suivis dans Réglages ou avec la cloche de chaque page.",
			Font = new Font("Segoe UI", Zx.P(8.5f)),
			ForeColor = C.Gris,
			BackColor = C.Cote,
			Location = new Point(Zx.P(26), Zx.P(24)),
			AutoSize = true
		};
		panel.Controls.Add(value3);
		defil = new Panel();
		defil.Location = new Point(0, Zx.P(72));
		defil.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - Zx.P(72) - Zx.P(64));
		defil.AutoScroll = true;
		defil.BackColor = C.Fond;
		base.Controls.Add(defil);
		Theme.Sombre(defil);
		z = new ZoneDessin();
		z.Location = new Point(0, 0);
		z.Width = base.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
		z.Height = Math.Max(200, Notifs.Toutes.Count * 82 + 16);
		z.Dessiner = Dessiner;
		defil.Controls.Add(z);
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			souris = e.Location;
			bool flag = false;
			foreach (KeyValuePair<Rectangle, string> zone in zones)
			{
				if (zone.Key.Contains(e.Location))
				{
					flag = true;
				}
			}
			z.Cursor = (flag ? Cursors.Hand : Cursors.Default);
			z.Invalidate();
		};
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			foreach (KeyValuePair<Rectangle, string> zone2 in zones)
			{
				if (zone2.Key.Contains(e.Location) && zone2.Value.Length > 0)
				{
					Sons.Clic();
					Demande = "mod:" + zone2.Value;
					base.DialogResult = DialogResult.OK;
					break;
				}
			}
		};
		base.FormClosed += delegate
		{
			Notifs.ToutLu();
		};
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.Clear(C.Fond);
		zones.Clear();
		List<Notif> toutes = Notifs.Toutes;
		if (toutes.Count == 0)
		{
			Lang.DrawText(g, "Aucune notification pour l'instant.", fX, new Rectangle(0, Zx.P(40), w, Zx.P(30)), C.Gris, TextFormatFlags.HorizontalCenter);
			return;
		}
		for (int i = 0; i < toutes.Count; i++)
		{
			Notif notif = toutes[i];
			Rectangle rectangle = new Rectangle(Zx.P(20), Zx.P(8) + i * Zx.P(82), w - Zx.P(40), Zx.P(72));
			bool flag = rectangle.Contains(souris) && notif.Mod.Length > 0;
			Color color = ((notif.Type == "maj") ? C.Bleu : ((notif.Type == "sortie") ? C.Vert : ((notif.Type == "vip") ? C.Or : ((notif.Type == "lanceur") ? C.Orange : C.Jade))));
			using (GraphicsPath path = Bouton.Rond(rectangle, Zx.P(12)))
			{
				using (SolidBrush brush = new SolidBrush(flag ? C.CarteSurvol : C.Carte))
				{
					g.FillPath(brush, path);
				}
				using Pen pen = new Pen(notif.Lu ? C.Ligne : Color.FromArgb(160, color));
				g.DrawPath(pen, path);
			}
			Rectangle rectangle2 = new Rectangle(rectangle.X + Zx.P(14), rectangle.Y + Zx.P(14), Zx.P(44), Zx.P(44));
			using (SolidBrush brush2 = new SolidBrush(Color.FromArgb(50, color)))
			{
				g.FillEllipse(brush2, rectangle2);
			}
			string t = ((notif.Type == "maj") ? "\ue895" : ((notif.Type == "sortie") ? "\ue768" : ((notif.Type == "vip") ? "\ue734" : ((notif.Type == "lanceur") ? "\ue777" : "\ue7c3"))));
			Lang.DrawText(g, t, fG, rectangle2, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Lang.DrawText(g, notif.Titre, fT, new Rectangle(rectangle2.Right + Zx.P(14), rectangle.Y + Zx.P(12), rectangle.Width - Zx.P(90) - Zx.P(110), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			Lang.DrawText(g, notif.Date, fX, new Rectangle(rectangle.Right - Zx.P(124), rectangle.Y + Zx.P(14), Zx.P(110), Zx.P(18)), C.Gris, TextFormatFlags.Right | TextFormatFlags.NoPadding);
			Lang.DrawText(g, notif.Texte, fX, new Rectangle(rectangle2.Right + Zx.P(14), rectangle.Y + Zx.P(38), rectangle.Width - Zx.P(90), Zx.P(20)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
			if (!notif.Lu)
			{
				using SolidBrush brush3 = new SolidBrush(C.Orange);
				g.FillEllipse(brush3, rectangle.X + Zx.P(6), rectangle.Y + rectangle.Height / 2 - Zx.P(3), Zx.P(6), Zx.P(6));
			}
			zones.Add(new KeyValuePair<Rectangle, string>(rectangle, notif.Mod));
		}
	}
}
