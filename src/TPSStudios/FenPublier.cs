using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>Envoi d'une photo ou d'une vidéo à la communauté : aperçu, titre, mod concerné, progression.</summary>
internal class FenPublier : Form
{
	public string Statut = "";

	private readonly string chemin;

	private readonly bool video;

	private readonly Bitmap apercu;

	private readonly Champ tTitre;

	private readonly ComboBox cMod;

	private readonly List<KeyValuePair<string, string>> mods;

	private readonly Bouton bEnvoyer;

	private readonly Bouton bAnnuler;

	private readonly Label lEtat;

	private double progression = -1.0;

	private readonly ZoneDessin zAp;

	private static readonly Font fT = new Font("Segoe UI Semibold", Zx.P(14f));

	private static readonly Font fS = new Font("Segoe UI", Zx.P(9f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(26f));

	public FenPublier(string chemin, List<KeyValuePair<string, string>> mods)
	{
		this.chemin = chemin;
		this.mods = mods ?? new List<KeyValuePair<string, string>>();
		string mime = Social.MimeDe(chemin) ?? "";
		video = mime.StartsWith("video/");
		Text = "Publier";
		FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Carte;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		ClientSize = new Size(Zx.P(600), Zx.P(560));
		int m = Zx.P(28);
		int w = ClientSize.Width - 2 * m;
		Label titre = new Label
		{
			Text = video ? "Publier une vidéo" : "Publier une photo",
			Font = fT,
			ForeColor = C.Texte,
			BackColor = C.Carte,
			Location = new Point(m, Zx.P(20)),
			AutoSize = true
		};
		Controls.Add(titre);
		if (!video)
		{
			try
			{
				using Image im = Image.FromFile(chemin);
				apercu = Couvertures.Composer(im, w, Zx.P(240));
			}
			catch
			{
				apercu = null;
			}
		}
		zAp = new ZoneDessin
		{
			Location = new Point(m, Zx.P(62)),
			Size = new Size(w, Zx.P(240))
		};
		zAp.Dessiner = DessinerApercu;
		Controls.Add(zAp);
		long taille = 0L;
		try
		{
			taille = new FileInfo(chemin).Length;
		}
		catch
		{
		}
		Label fichier = new Label
		{
			Text = Path.GetFileName(chemin) + "  ·  " + ((taille >= 1048576) ? ((taille / 1048576.0).ToString("0.0") + " Mo") : (Math.Max(1L, taille / 1024) + " Ko")),
			Font = fS,
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(m, Zx.P(308)),
			Size = new Size(w, Zx.P(20)),
			AutoEllipsis = true,
			UseMnemonic = false
		};
		Controls.Add(fichier);
		Controls.Add(new Label
		{
			Text = "Titre (facultatif)",
			Font = fS,
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(m, Zx.P(338)),
			AutoSize = true
		});
		tTitre = new Champ(multi: false, Zx.P(38))
		{
			Location = new Point(m, Zx.P(360)),
			Width = w,
			MaxLength = 80,
			Indice = "ex. Coucher de soleil sur la ligne 25"
		};
		Controls.Add(tTitre);
		Controls.Add(new Label
		{
			Text = "Mod concerné (facultatif)",
			Font = fS,
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(m, Zx.P(408)),
			AutoSize = true
		});
		cMod = new ComboBox
		{
			DropDownStyle = ComboBoxStyle.DropDownList,
			FlatStyle = FlatStyle.Flat,
			BackColor = C.Champ,
			ForeColor = C.Texte,
			Location = new Point(m, Zx.P(430)),
			Width = w
		};
		cMod.Items.Add(Lang.T("Aucun"));
		foreach (KeyValuePair<string, string> kv in this.mods)
		{
			cMod.Items.Add(kv.Value);
		}
		cMod.SelectedIndex = 0;
		Controls.Add(cMod);
		lEtat = new Label
		{
			Font = fS,
			ForeColor = C.Gris,
			BackColor = C.Carte,
			Location = new Point(m, Zx.P(470)),
			Size = new Size(w, Zx.P(20)),
			AutoEllipsis = true,
			UseMnemonic = false,
			Text = Lang.T("Visible par tous après validation par l'équipe TPS Groupe.")
		};
		Controls.Add(lEtat);
		bAnnuler = new Bouton("Annuler")
		{
			Plein = false,
			Size = new Size(Zx.P(120), Zx.P(40)),
			Location = new Point(m, ClientSize.Height - Zx.P(58))
		};
		bAnnuler.Click += delegate
		{
			DialogResult = DialogResult.Cancel;
		};
		Controls.Add(bAnnuler);
		bEnvoyer = new Bouton("Publier")
		{
			Size = new Size(Zx.P(160), Zx.P(40)),
			Location = new Point(ClientSize.Width - m - Zx.P(160), ClientSize.Height - Zx.P(58))
		};
		bEnvoyer.Click += delegate
		{
			Envoyer();
		};
		Controls.Add(bEnvoyer);
		FormClosed += delegate
		{
			apercu?.Dispose();
		};
	}

	private void DessinerApercu(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.Clear(C.Carte);
		Rectangle r = new Rectangle(0, 0, w - 1, h - 1);
		using (GraphicsPath p = Bouton.Rond(r, Zx.P(12)))
		{
			GraphicsState st = g.Save();
			g.SetClip(p);
			if (apercu != null)
			{
				g.DrawImageUnscaled(apercu, 0, 0);
			}
			else
			{
				using LinearGradientBrush lg = new LinearGradientBrush(r, Color.FromArgb(40, 48, 96), Color.FromArgb(14, 17, 40), 45f);
				g.FillRectangle(lg, r);
				Lang.DrawText(g, video ? "" : "", fG, r, Color.FromArgb(200, 255, 255, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			if (progression >= 0.0)
			{
				using SolidBrush voile = new SolidBrush(Color.FromArgb(150, 6, 8, 22));
				g.FillRectangle(voile, r);
				Rectangle rb = new Rectangle(Zx.P(40), h / 2 - Zx.P(4), w - Zx.P(80), Zx.P(8));
				using (GraphicsPath pb = Bouton.Rond(rb, Zx.P(4)))
				{
					using SolidBrush bb = new SolidBrush(Color.FromArgb(50, 255, 255, 255));
					g.FillPath(bb, pb);
				}
				int pw = Math.Max(Zx.P(8), (int)(rb.Width * progression));
				using (GraphicsPath pp = Bouton.Rond(new Rectangle(rb.X, rb.Y, pw, rb.Height), Zx.P(4)))
				{
					using SolidBrush bp = new SolidBrush(C.Orange);
					g.FillPath(bp, pp);
				}
				Lang.DrawText(g, Lang.T("Envoi… ") + (int)(progression * 100.0) + " %", fS, new Rectangle(0, rb.Bottom + Zx.P(10), w, Zx.P(20)), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
			}
			g.Restore(st);
		}
	}

	private void Envoyer()
	{
		string titre = tTitre.Text.Trim();
		string mod = (cMod.SelectedIndex > 0 && cMod.SelectedIndex - 1 < mods.Count) ? mods[cMod.SelectedIndex - 1].Key : "";
		bEnvoyer.Enabled = false;
		tTitre.Enabled = false;
		cMod.Enabled = false;
		progression = 0.0;
		zAp.Invalidate();
		lEtat.ForeColor = C.Gris;
		lEtat.Text = Lang.T("Envoi en cours, ne fermez pas cette fenêtre…");
		string err = null;
		string statut = null;
		DateTime dernier = DateTime.MinValue;
		Thread t = new Thread((ThreadStart)delegate
		{
			try
			{
				err = Social.Assurer(Comptes.Actif);
				if (err == null)
				{
					statut = Social.Publier(chemin, titre, mod, delegate(double p)
					{
						if ((DateTime.Now - dernier).TotalMilliseconds < 80.0 && p < 1.0)
						{
							return;
						}
						dernier = DateTime.Now;
						try
						{
							BeginInvoke((MethodInvoker)delegate
							{
								progression = p;
								zAp.Invalidate();
							});
						}
						catch
						{
						}
					});
				}
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
			try
			{
				BeginInvoke((MethodInvoker)delegate
				{
					if (IsDisposed)
					{
						return;
					}
					if (err != null)
					{
						progression = -1.0;
						zAp.Invalidate();
						bEnvoyer.Enabled = true;
						tTitre.Enabled = true;
						cMod.Enabled = true;
						lEtat.ForeColor = C.Rouge;
						lEtat.Text = "✖  " + Lang.T(err);
						return;
					}
					Statut = statut ?? "attente";
					DialogResult = DialogResult.OK;
				});
			}
			catch
			{
			}
		});
		t.IsBackground = true;
		t.Start();
	}
}
