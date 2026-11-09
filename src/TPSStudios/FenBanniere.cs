using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenBanniere : Form
{
	private Banniere m;

	private Compte compte;

	private string importe;

	private Panel zoneBas;

	private RangeeCouleurs rcPh;

	private ZoneDessin apercu;

	private Segments sgType;

	private Segments sgCible;

	private Segments sgAngle;

	private Segments sgMotif;

	private SelecteurCouleur sel;

	private SelecteurCouleur selUni;

	private Cadre cdDeg;

	private Cadre cdUni;

	private Cadre cdImg;

	private Cadre cdAnim;

	private ZoneDessin zoneAnim;

	private Timer tAnimB;

	private Curseur cZoom;

	private Curseur cX;

	private Curseur cY;

	private bool charge;

	private Label lImg;

	private static readonly string[] types = new string[4] { "degrade", "uni", "image", "anim" };

	private static readonly string[] fleches = new string[8] { "→", "↘", "↓", "↙", "←", "↖", "↑", "↗" };

	public FenBanniere(Compte c)
	{
		compte = c;
		m = Banniere.De(c);
		if (m.Type == "image" && m.Img == null)
		{
			m.Type = "degrade";
		}
		Text = "Bannière du profil";
		base.ClientSize = new Size(Zx.P(940), Zx.P(700));
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.StartPosition = FormStartPosition.CenterParent;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		try
		{
			base.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}
		catch
		{
		}
		Theme.Fenetre(this);
		Label value = new Label
		{
			Text = "Bannière du profil",
			Font = new Font("Segoe UI Semibold", Zx.P(18f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(88), Zx.P(16)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Créez un dégradé ou importez votre image : l'aperçu se met à jour en direct.",
			Font = new Font("Segoe UI", Zx.P(10f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(90), Zx.P(52)),
			AutoSize = true
		};
		base.Controls.Add(value2);
		base.Controls.Add(Integre.Retour(delegate
		{
			base.DialogResult = DialogResult.Cancel;
		}, Zx.P(26), Zx.P(18)));
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton = new Bouton("Annuler");
		bouton.Plein = false;
		bouton.Size = new Size(Zx.P(120), Zx.P(38));
		bouton.Location = new Point(Zx.P(28), Zx.P(16));
		bouton.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		Bouton bouton2 = new Bouton("Appliquer");
		bouton2.Size = new Size(Zx.P(150), Zx.P(38));
		bouton2.Location = new Point(base.ClientSize.Width - Zx.P(28) - Zx.P(150), Zx.P(16));
		bouton2.Click += delegate
		{
			Appliquer();
		};
		Bouton bouton3 = new Bouton("Bannière par défaut");
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(200), Zx.P(38));
		bouton3.Plein = false;
		bouton3.Size = new Size(Zx.P(200), Zx.P(38));
		bouton3.Location = new Point(Zx.P(160), Zx.P(16));
		bouton3.Click += delegate
		{
			compte.BanType = "";
			Comptes.RetirerBanniereImage(compte);
			Comptes.Sauvegarder();
			base.DialogResult = DialogResult.OK;
		};
		panel.Controls.Add(bouton3);
		panel.Controls.Add(bouton);
		panel.Controls.Add(bouton2);
		apercu = new ZoneDessin();
		apercu.Location = new Point(Zx.P(28), Zx.P(92));
		apercu.Size = new Size(Zx.P(884), Zx.P(221));
		apercu.Dessiner = DessinerApercu;
		base.Controls.Add(apercu);
		sgType = new Segments();
		sgType.Items = new string[4] { "Dégradé", "Couleur unie", "Image", "Animée" };
		sgType.Location = new Point(Zx.P(28), Zx.P(328));
		sgType.Width = Zx.P(560);
		sgType.Etire = true;
		base.Controls.Add(sgType);
		sgType.Index = Math.Max(0, Array.IndexOf(types, m.Type));
		sgType.Changed += delegate
		{
			if (!charge)
			{
				m.Type = types[sgType.Index];
				MajPanneaux();
				apercu.Invalidate();
			}
		};
		zoneBas = new Panel();
		zoneBas.Location = new Point(0, Zx.P(380));
		zoneBas.Size = new Size(base.ClientSize.Width, base.ClientSize.Height - Zx.P(380) - Zx.P(70));
		zoneBas.AutoScroll = true;
		zoneBas.BackColor = C.Fond;
		base.Controls.Add(zoneBas);
		Theme.Sombre(zoneBas);
		cdDeg = new Cadre();
		cdDeg.Titre = "Dégradé";
		cdDeg.Location = new Point(Zx.P(28), 0);
		cdDeg.Size = new Size(Zx.P(884), Zx.P(408));
		zoneBas.Controls.Add(cdDeg);
		sgCible = new Segments();
		sgCible.Items = new string[2] { "Couleur A", "Couleur B" };
		sgCible.Location = new Point(Zx.P(24), Zx.P(46));
		sgCible.Width = Zx.P(300);
		sgCible.Etire = true;
		cdDeg.Controls.Add(sgCible);
		sel = new SelecteurCouleur();
		sel.Location = new Point(Zx.P(24), Zx.P(92));
		sel.Width = Zx.P(420);
		cdDeg.Controls.Add(sel);
		sgCible.Changed += delegate
		{
			charge = true;
			sel.Couleur = ((sgCible.Index == 0) ? m.C1 : m.C2);
			charge = false;
		};
		sel.Changed += delegate
		{
			if (!charge)
			{
				if (sgCible.Index == 0)
				{
					m.C1 = sel.Couleur;
				}
				else
				{
					m.C2 = sel.Couleur;
				}
				apercu.Invalidate();
			}
		};
		Etiq(cdDeg, "Direction", Zx.P(480), Zx.P(44));
		sgAngle = new Segments();
		sgAngle.MinW = Zx.P(36);
		sgAngle.Items = fleches;
		sgAngle.Location = new Point(Zx.P(480), Zx.P(66));
		sgAngle.Width = Zx.P(380);
		sgAngle.Etire = true;
		cdDeg.Controls.Add(sgAngle);
		sgAngle.Index = Math.Max(0, Math.Min(7, m.Angle / 45));
		sgAngle.Changed += delegate
		{
			if (!charge)
			{
				m.Angle = sgAngle.Index * 45;
				apercu.Invalidate();
			}
		};
		Etiq(cdDeg, "Motif", Zx.P(480), Zx.P(118));
		sgMotif = new Segments();
		sgMotif.Items = Banniere.MotifsNoms;
		sgMotif.Location = new Point(Zx.P(480), Zx.P(140));
		sgMotif.Width = Zx.P(380);
		sgMotif.Etire = true;
		cdDeg.Controls.Add(sgMotif);
		sgMotif.Index = Math.Max(0, Array.IndexOf(Banniere.Motifs, m.Motif));
		sgMotif.Changed += delegate
		{
			if (!charge)
			{
				m.Motif = Banniere.Motifs[sgMotif.Index];
				apercu.Invalidate();
			}
		};
		Etiq(cdDeg, "Modèles", Zx.P(480), Zx.P(192));
		for (int i = 0; i < Banniere.Modeles.Length; i++)
		{
			object[] mo = Banniere.Modeles[i];
			Bouton bouton4 = new Bouton((string)mo[0]);
			bouton4.Plein = false;
			bouton4.Size = new Size(Zx.P(122), Zx.P(34));
			bouton4.Location = new Point(Zx.P(480) + i % 3 * Zx.P(129), Zx.P(214) + i / 3 * Zx.P(40));
			bouton4.Click += delegate
			{
				m.C1 = (Color)mo[1];
				m.C2 = (Color)mo[2];
				m.Angle = (int)mo[3];
				m.Motif = (string)mo[4];
				m.Type = "degrade";
				Synchroniser();
			};
			cdDeg.Controls.Add(bouton4);
		}
		Bouton bouton5 = new Bouton("✦  Au hasard");
		bouton5.Plein = false;
		bouton5.Size = new Size(Zx.P(380), Zx.P(36));
		bouton5.Location = new Point(Zx.P(480), Zx.P(298));
		bouton5.Click += delegate
		{
			Banniere banniere = Banniere.Hasard();
			m.C1 = banniere.C1;
			m.C2 = banniere.C2;
			m.Angle = banniere.Angle;
			m.Motif = banniere.Motif;
			m.Type = "degrade";
			Synchroniser();
		};
		cdDeg.Controls.Add(bouton5);
		Etiq(cdDeg, "Couleurs de votre photo (vers la couleur A ou B)", Zx.P(24), Zx.P(338));
		rcPh = new RangeeCouleurs();
		rcPh.Location = new Point(Zx.P(24), Zx.P(360));
		rcPh.Size = new Size(Zx.P(420), Zx.P(40));
		cdDeg.Controls.Add(rcPh);
		rcPh.Couleurs = Comptes.CouleursPhoto(Comptes.ImageAvatar(c)).ToArray();
		rcPh.Choisie += delegate(Color col)
		{
			if (sgCible.Index == 0)
			{
				m.C1 = col;
			}
			else
			{
				m.C2 = col;
			}
			m.Type = "degrade";
			charge = true;
			sel.Couleur = col;
			charge = false;
			apercu.Invalidate();
		};
		Bouton bouton6 = new Bouton("✦  Dégradé automatique d'après ma photo")
		{
			Plein = false,
			Size = new Size(Zx.P(380), Zx.P(36)),
			Location = new Point(Zx.P(480), Zx.P(346))
		};
		cdDeg.Controls.Add(bouton6);
		bouton6.Click += delegate
		{
			List<Color> list = Comptes.CouleursPhoto(Comptes.ImageAvatar(compte));
			if (list.Count == 0)
			{
				MessageBox.Show(this, "Importez d'abord une photo de profil : ses couleurs servent à composer le dégradé.", Text);
			}
			else
			{
				Comptes.DegradeDepuis(list, out var c2, out var c3);
				m.C1 = c2;
				m.C2 = c3;
				m.Angle = 45;
				m.Type = "degrade";
				Synchroniser();
			}
		};
		cdUni = new Cadre();
		cdUni.Titre = "Couleur unie";
		cdUni.Location = new Point(Zx.P(28), 0);
		cdUni.Size = new Size(Zx.P(884), Zx.P(340));
		zoneBas.Controls.Add(cdUni);
		selUni = new SelecteurCouleur();
		selUni.Location = new Point(Zx.P(24), Zx.P(52));
		selUni.Width = Zx.P(420);
		cdUni.Controls.Add(selUni);
		selUni.Changed += delegate
		{
			if (!charge)
			{
				m.C1 = selUni.Couleur;
				apercu.Invalidate();
			}
		};
		cdImg = new Cadre();
		cdImg.Titre = "Image";
		cdImg.Location = new Point(Zx.P(28), 0);
		cdImg.Size = new Size(Zx.P(884), Zx.P(340));
		zoneBas.Controls.Add(cdImg);
		Bouton bouton7 = new Bouton("Importer une image…");
		bouton7.Size = new Size(Zx.P(240), Zx.P(40));
		bouton7.Location = new Point(Zx.P(24), Zx.P(52));
		bouton7.Click += delegate
		{
			Importer();
		};
		cdImg.Controls.Add(bouton7);
		Bouton bouton8 = new Bouton("Retirer l'image");
		bouton8.Plein = false;
		bouton8.Size = new Size(Zx.P(170), Zx.P(40));
		bouton8.Location = new Point(Zx.P(276), Zx.P(52));
		bouton8.Click += delegate
		{
			m.Img = null;
			importe = null;
			m.Type = "degrade";
			Synchroniser();
		};
		cdImg.Controls.Add(bouton8);
		lImg = new Label();
		lImg.Font = new Font("Segoe UI", Zx.P(9f));
		lImg.ForeColor = C.Gris;
		lImg.BackColor = C.Carte;
		lImg.Location = new Point(Zx.P(460), Zx.P(62));
		lImg.Size = new Size(Zx.P(400), Zx.P(22));
		cdImg.Controls.Add(lImg);
		Etiq(cdImg, "Zoom", Zx.P(24), Zx.P(120));
		cZoom = new Curseur();
		cZoom.Min = 100;
		cZoom.Max = 300;
		cZoom.Location = new Point(Zx.P(14), Zx.P(142));
		cZoom.Width = Zx.P(420);
		cdImg.Controls.Add(cZoom);
		Etiq(cdImg, "Position horizontale", Zx.P(24), Zx.P(184));
		cX = new Curseur();
		cX.Location = new Point(Zx.P(14), Zx.P(206));
		cX.Width = Zx.P(420);
		cdImg.Controls.Add(cX);
		Etiq(cdImg, "Position verticale", Zx.P(24), Zx.P(248));
		cY = new Curseur();
		cY.Location = new Point(Zx.P(14), Zx.P(270));
		cY.Width = Zx.P(420);
		cdImg.Controls.Add(cY);
		cZoom.Changed += delegate
		{
			if (!charge)
			{
				m.Zoom = cZoom.Valeur;
				apercu.Invalidate();
			}
		};
		cX.Changed += delegate
		{
			if (!charge)
			{
				m.X = cX.Valeur;
				apercu.Invalidate();
			}
		};
		cY.Changed += delegate
		{
			if (!charge)
			{
				m.Y = cY.Valeur;
				apercu.Invalidate();
			}
		};
		Label value3 = new Label
		{
			Text = "Format conseillé : image large (ex. 1600×400). Elle est recadrée au format de la bannière ; zoom et position servent à choisir la partie visible. L'image reste sur ce PC.",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(Zx.P(480), Zx.P(120)),
			Size = new Size(Zx.P(380), Zx.P(120))
		};
		cdImg.Controls.Add(value3);
		int num = 4;
		cdAnim = new Cadre();
		cdAnim.Titre = "Bannières animées";
		cdAnim.Location = new Point(Zx.P(28), 0);
		cdAnim.Size = new Size(Zx.P(884), Zx.P(62) + num * Zx.P(134));
		zoneBas.Controls.Add(cdAnim);
		zoneAnim = new ZoneDessin();
		zoneAnim.Location = new Point(Zx.P(24), Zx.P(46));
		zoneAnim.Size = new Size(Zx.P(836), num * Zx.P(134));
		cdAnim.Controls.Add(zoneAnim);
		zoneAnim.Cursor = Cursors.Hand;
		zoneAnim.Dessiner = delegate(Graphics g, int w, int h)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			double temps = BanAnim.Temps;
			int num3 = (w - 4 * Zx.P(12)) / 5;
			for (int k = 0; k < 16; k++)
			{
				Rectangle r = new Rectangle(k % 5 * (num3 + Zx.P(12)), k / 5 * Zx.P(134), num3, Zx.P(98));
				bool flag = m.Type == "anim" && m.Motif == BanAnim.Ids[k];
				using (GraphicsPath path = Bouton.Rond(new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1), Zx.P(10)))
				{
					GraphicsState gstate = g.Save();
					g.SetClip(path, CombineMode.Intersect);
					BanAnim.Dessiner(BanAnim.Ids[k], g, r, temps);
					g.Restore(gstate);
					using Pen pen = new Pen(flag ? C.Orange : C.Ligne, flag ? Zx.P(3f) : 1f);
					g.DrawPath(pen, path);
				}
				Lang.DrawText(g, BanAnim.Noms[k], F.Etat, new Rectangle(r.X, r.Bottom + Zx.P(4), r.Width, Zx.P(18)), flag ? C.Orange : C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
			}
		};
		zoneAnim.MouseDown += delegate(object o, MouseEventArgs e)
		{
			int num2 = (zoneAnim.Width - 4 * Zx.P(12)) / 5;
			for (int j = 0; j < 16; j++)
			{
				if (new Rectangle(j % 5 * (num2 + Zx.P(12)), j / 5 * Zx.P(134), num2, Zx.P(98)).Contains(e.Location))
				{
					m.Type = "anim";
					m.Motif = BanAnim.Ids[j];
					if (sgType.Index != 3)
					{
						charge = true;
						sgType.Index = 3;
						charge = false;
					}
					MajPanneaux();
					apercu.Invalidate();
					zoneAnim.Invalidate();
					break;
				}
			}
		};
		tAnimB = new Timer();
		tAnimB.Interval = 40;
		tAnimB.Tick += delegate
		{
			if (m.Type == "anim")
			{
				apercu.Invalidate();
				if (cdAnim.Visible)
				{
					zoneAnim.Invalidate();
				}
			}
		};
		tAnimB.Start();
		base.Disposed += delegate
		{
			tAnimB.Stop();
			tAnimB.Dispose();
		};
		Synchroniser();
	}

	private void Etiq(Control parent, string t, int x, int y)
	{
		Label label = new Label();
		label.Text = t;
		label.Font = new Font("Segoe UI", Zx.P(9f));
		label.ForeColor = C.Gris;
		label.BackColor = C.Carte;
		label.Location = new Point(x, y);
		label.AutoSize = true;
		parent.Controls.Add(label);
	}

	private void Synchroniser()
	{
		charge = true;
		sgType.Index = Math.Max(0, Array.IndexOf(types, m.Type));
		sgCible.Index = 0;
		sel.Couleur = m.C1;
		selUni.Couleur = m.C1;
		sgAngle.Index = Math.Max(0, Math.Min(7, m.Angle / 45));
		sgMotif.Index = Math.Max(0, Array.IndexOf(Banniere.Motifs, m.Motif));
		cZoom.Valeur = m.Zoom;
		cX.Valeur = m.X;
		cY.Valeur = m.Y;
		lImg.Text = ((m.Img != null) ? ("Image prête (" + m.Img.Width + "×" + m.Img.Height + ")") : "Aucune image importée");
		charge = false;
		MajPanneaux();
		apercu.Invalidate();
	}

	private void MajPanneaux()
	{
		cdDeg.Visible = m.Type == "degrade";
		cdUni.Visible = m.Type == "uni";
		cdImg.Visible = m.Type == "image";
		cdAnim.Visible = m.Type == "anim";
	}

	private void Importer()
	{
		using OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.Filter = "Images (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
		openFileDialog.Title = "Image de bannière";
		if (openFileDialog.ShowDialog(this) != DialogResult.OK)
		{
			return;
		}
		try
		{
			if (new FileInfo(openFileDialog.FileName).Length > 15728640)
			{
				throw new Exception("image trop lourde (15 Mo maximum)");
			}
			using (Image image = Image.FromFile(openFileDialog.FileName))
			{
				if (image.Width < 400 || image.Height < 100)
				{
					throw new Exception("image trop petite (400×100 minimum)");
				}
				m.Img = Comptes.ReduireBanniere(image);
			}
			importe = openFileDialog.FileName;
			m.Type = "image";
			m.Zoom = 100;
			m.X = 50;
			m.Y = 50;
			Synchroniser();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Image refusée : " + ex.Message, Text);
		}
	}

	private void Appliquer()
	{
		try
		{
			if (m.Type == "image" && importe != null)
			{
				Comptes.DefinirBanniereImage(compte, importe);
			}
			if (m.Type != "image")
			{
				Comptes.RetirerBanniereImage(compte);
			}
			m.Enregistrer(compte);
			Comptes.Sauvegarder();
			base.DialogResult = DialogResult.OK;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Impossible d'enregistrer la bannière : " + ex.Message, Text);
		}
	}

	private void DessinerApercu(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.InterpolationMode = InterpolationMode.HighQualityBicubic;
		Rectangle r = new Rectangle(0, 0, w - 1, h - 1);
		using (GraphicsPath path = Bouton.Rond(r, Zx.P(14)))
		{
			GraphicsState gstate = g.Save();
			g.SetClip(path, CombineMode.Intersect);
			m.Dessiner(g, r);
			using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, h - Zx.P(80), w, Zx.P(81)), Color.FromArgb(0, C.Carte), C.Carte, 90f))
			{
				g.FillRectangle(brush, 0, h - Zx.P(80), w, Zx.P(81));
			}
			g.Restore(gstate);
			using Pen pen = new Pen(C.Ligne);
			g.DrawPath(pen, path);
		}
		Rectangle rectangle = new Rectangle(Zx.P(28), h - Zx.P(78), Zx.P(72), Zx.P(72));
		using (SolidBrush brush2 = new SolidBrush(C.Carte))
		{
			g.FillEllipse(brush2, Rectangle.Inflate(rectangle, Zx.P(5), Zx.P(5)));
		}
		Comptes.Avatar(g, compte, rectangle, 24f);
		Lang.DrawText(g, compte.Pseudo, new Font("Segoe UI Semibold", Zx.P(16f)), new Point(Zx.P(112), h - Zx.P(62)), C.Texte);
	}
}
