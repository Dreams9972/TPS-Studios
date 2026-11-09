using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenLies : Form
{
	public string Demande = "";

	private List<Projet> l;

	private List<Projet> tous;

	private Func<Projet, Image> img;

	private Func<Projet, int> etat;

	private int survol = -1;

	private static readonly Font fN = new Font("Segoe UI Semibold", Zx.P(11f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fT = new Font("Segoe UI", Zx.P(8.5f));

	public FenLies(string nom, List<Projet> liste, Func<Projet, Image> image, Func<Projet, int> etat)
	{
		FenLies fenLies = this;
		tous = liste;
		l = new List<Projet>(liste);
		img = image;
		this.etat = etat;
		Text = "Mods liés";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(660), Math.Min(Zx.P(660), Zx.P(236) + Math.Max(1, l.Count) * Zx.P(80)));
		Label value = new Label
		{
			Text = "Mods liés à « " + nom + " »",
			UseMnemonic = false,
			Font = new Font("Segoe UI Semibold", Zx.P(16f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(20)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Compléments de ce mod : installez-les directement ou ouvrez leur page.",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(58)),
			AutoSize = true
		};
		base.Controls.Add(value2);
		Champ tq = new Champ(multi: false, Zx.P(36));
		tq.Location = new Point(Zx.P(28), Zx.P(90));
		tq.Width = Zx.P(604);
		tq.Indice = "Rechercher un mod lié (nom ou tag)…";
		base.Controls.Add(tq);
		Panel panel = new Panel
		{
			Location = new Point(Zx.P(28), Zx.P(140)),
			Size = new Size(Zx.P(604), base.ClientSize.Height - Zx.P(140) - Zx.P(70) - Zx.P(12)),
			AutoScroll = true,
			BackColor = C.Fond
		};
		base.Controls.Add(panel);
		Theme.Sombre(panel);
		ZoneDessin z = new ZoneDessin();
		z.Location = new Point(0, 0);
		z.Size = new Size(panel.Width - ((l.Count * Zx.P(80) > panel.Height) ? (SystemInformation.VerticalScrollBarWidth + Zx.P(2)) : 0), Math.Max(Zx.P(60), l.Count * Zx.P(80)));
		z.Cursor = Cursors.Hand;
		panel.Controls.Add(z);
		z.Dessiner = delegate(Graphics g, int w, int h)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			if (fenLies.l.Count == 0)
			{
				Lang.DrawText(g, (fenLies.tous.Count == 0) ? "Aucun autre contenu pour l'instant." : "Aucun mod lié ne correspond à cette recherche.", fN, new Rectangle(0, 0, w, Zx.P(60)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
			else
			{
				for (int i = 0; i < fenLies.l.Count; i++)
				{
					Projet projet = fenLies.l[i];
					Rectangle r = new Rectangle(0, i * Zx.P(80), w - 1, Zx.P(72));
					using (GraphicsPath path = Bouton.Rond(r, Zx.P(12)))
					{
						using (SolidBrush brush = new SolidBrush((i == fenLies.survol) ? C.CarteSurvol : C.Carte))
						{
							g.FillPath(brush, path);
						}
						using Pen pen = new Pen((i == fenLies.survol) ? C.Jade : C.Ligne);
						g.DrawPath(pen, path);
					}
					Rectangle rectangle = new Rectangle(Zx.P(10), r.Y + Zx.P(8), Zx.P(100), Zx.P(56));
					Image image2 = ((fenLies.img != null) ? fenLies.img(projet) : null);
					using (GraphicsPath path2 = Bouton.Rond(rectangle, Zx.P(8)))
					{
						GraphicsState gstate = g.Save();
						g.SetClip(path2, CombineMode.Intersect);
						if (image2 != null)
						{
							double num4 = Math.Max((double)rectangle.Width / (double)image2.Width, (double)rectangle.Height / (double)image2.Height);
							int num5 = (int)((double)image2.Width * num4);
							int num6 = (int)((double)image2.Height * num4);
							g.DrawImage(image2, rectangle.X + (rectangle.Width - num5) / 2, rectangle.Y + (rectangle.Height - num6) / 2, num5, num6);
						}
						else
						{
							using LinearGradientBrush brush2 = new LinearGradientBrush(rectangle, C.JadeFonce, C.Cote, 35f);
							g.FillRectangle(brush2, rectangle);
						}
						g.Restore(gstate);
					}
					Lang.DrawText(g, projet.Nom, fN, new Rectangle(Zx.P(126), r.Y + Zx.P(9), w - Zx.P(126) - Zx.P(166), Zx.P(24)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
					Lang.DrawText(g, projet.SousTitre, fS, new Rectangle(Zx.P(126), r.Y + Zx.P(34), w - Zx.P(126) - Zx.P(166), Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
					if (projet.Tags.Count > 0)
					{
						Lang.DrawText(g, "#" + string.Join("  #", projet.Tags.ToArray()), fT, new Rectangle(Zx.P(126), r.Y + Zx.P(52), w - Zx.P(126) - Zx.P(166), Zx.P(16)), Color.FromArgb(120, 200, 255), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
					}
					int num7 = ((etat != null) ? etat(projet) : 3);
					Rectangle r2 = new Rectangle(w - Zx.P(16) - Zx.P(130), r.Y + Zx.P(18), Zx.P(130), Zx.P(36));
					string t = num7 switch
					{
						1 => "Installé ✔", 
						2 => "Mettre à jour", 
						0 => "Installer", 
						_ => (projet.Payant && !projet.Debloque) ? projet.PrixTexte : "Voir la page", 
					};
					Color color = num7 switch
					{
						1 => C.Vert, 
						2 => C.Bleu, 
						0 => C.Orange, 
						_ => C.Gris, 
					};
					using (GraphicsPath path3 = Bouton.Rond(r2, Zx.P(18)))
					{
						using (SolidBrush brush3 = new SolidBrush((num7 == 0 || num7 == 2) ? color : Color.FromArgb(40, color)))
						{
							g.FillPath(brush3, path3);
						}
						using Pen pen2 = new Pen(Color.FromArgb(170, color));
						g.DrawPath(pen2, path3);
					}
					Font f = fS;
					Color c;
					switch (num7)
					{
					default:
						c = C.Texte;
						break;
					case 1:
						c = C.Vert;
						break;
					case 0:
					case 2:
						c = Color.White;
						break;
					}
					Lang.DrawText(g, t, f, r2, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				}
			}
		};
		tq.TextChanged += delegate
		{
			string q = tq.Text.Trim().ToLowerInvariant();
			fenLies.l = fenLies.tous.FindAll((Projet p) => q.Length == 0 || p.TexteRecherche.IndexOf(q) >= 0);
			z.Height = Math.Max(60, fenLies.l.Count * 80);
			fenLies.survol = -1;
			z.Invalidate();
		};
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			int num3 = e.Y / 80;
			if (num3 >= fenLies.l.Count || e.Y % 80 > 72)
			{
				num3 = -1;
			}
			if (num3 != fenLies.survol)
			{
				fenLies.survol = num3;
				z.Invalidate();
			}
		};
		z.MouseLeave += delegate
		{
			fenLies.survol = -1;
			z.Invalidate();
		};
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			int num = e.Y / 80;
			if (num >= 0 && num < fenLies.l.Count && e.Y % 80 <= 72)
			{
				Sons.Clic();
				int num2 = ((etat != null) ? etat(fenLies.l[num]) : 3);
				bool flag = e.X > z.Width - 16 - 130 - 4;
				fenLies.Demande = ((flag && (num2 == 0 || num2 == 2)) ? "installer:" : "") + fenLies.l[num].Id;
				fenLies.DialogResult = DialogResult.OK;
			}
		};
		Panel panel2 = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel2);
		Bouton bouton = new Bouton("Fermer");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(130), Zx.P(38));
		bouton.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(130), Zx.P(16));
		bouton.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		panel2.Controls.Add(bouton);
	}
}
