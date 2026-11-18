using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Page Communauté : photos et vidéos publiées par les joueurs. Onglets Récentes / Populaires / Mes publications,
/// et « À valider » pour l'équipe (les publications n'apparaissent qu'une fois validées).
/// </summary>
internal class FenPartage : Form, IPage
{
	/// <summary>Mods du catalogue (identifiant, nom) proposés à l'envoi ; nom d'un mod d'après son identifiant.</summary>
	public static Func<List<KeyValuePair<string, string>>> ModsCatalogue;

	public static Func<string, string> NomMod;

	/// <summary>Mod demandé en quittant la page (clic sur l'étiquette d'un mod) : « mod:identifiant ».</summary>
	public string Demande = "";

	private readonly ZoneDessin z;

	private readonly Segments onglets;

	private readonly Bouton bPublier;

	private readonly Label lMsg;

	private string[] vues = new string[3] { "recentes", "populaires", "miennes" };

	private List<Social.Publication> pubs = new List<Social.Publication>();

	private bool modo;

	private int attente;

	private bool charge;

	private string erreur = "";

	private int defile;

	private int hauteurContenu;

	private Point souris = new Point(-1, -1);

	private readonly List<KeyValuePair<Rectangle, Action>> clics = new List<KeyValuePair<Rectangle, Action>>();

	private int requete;

	/// <summary>Miniatures déjà prêtes (par publication) ; images en cours de téléchargement.</summary>
	private static readonly Dictionary<long, Bitmap> miniatures = new Dictionary<long, Bitmap>();

	private static readonly HashSet<long> enCours = new HashSet<long>();

	/// <summary>Publication ouverte en grand (null : la grille).</summary>
	private Social.Publication ouverte;

	private Image grande;

	private bool grandeEchec;

	private static readonly Font fTitre = new Font("Segoe UI Semibold", Zx.P(18f));

	private static readonly Font fSous = new Font("Segoe UI", Zx.P(9.5f));

	private static readonly Font fCarteT = new Font("Segoe UI Semibold", Zx.P(10.5f));

	private static readonly Font fCarteS = new Font("Segoe UI", Zx.P(8.5f));

	private static readonly Font fG = new Font("Segoe MDL2 Assets", Zx.P(22f));

	private static readonly Font fGp = new Font("Segoe MDL2 Assets", Zx.P(11f));

	private static readonly Font fBt = new Font("Segoe UI Semibold", Zx.P(9f));

	private static string Cache => Path.Combine(Config.Dossier, "communaute");

	public FenPartage()
	{
		Text = "Communauté";
		FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		ClientSize = Integre.Hote;
		int width = ClientSize.Width;
		int largeur = Math.Max(Math.Min(width, Zx.P(900)), Math.Min(width - Zx.P(48), Zx.P(2400)));
		int x0 = (width - largeur) / 2;
		Panel pied = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(44),
			BackColor = C.Fond
		};
		Controls.Add(pied);
		lMsg = new Label
		{
			Font = fSous,
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(x0 + Zx.P(28), 0),
			Size = new Size(largeur - Zx.P(56), Zx.P(40)),
			TextAlign = ContentAlignment.MiddleLeft,
			UseMnemonic = false,
			Text = "Les photos et vidéos sont visibles par tous une fois validées par l'équipe TPS Groupe."
		};
		pied.Controls.Add(lMsg);
		Panel tete = new Panel
		{
			Dock = DockStyle.Top,
			Height = Zx.P(132),
			BackColor = C.Fond
		};
		Controls.Add(tete);
		tete.Controls.Add(Integre.Retour(delegate
		{
			DialogResult = DialogResult.OK;
		}, x0 + Zx.P(24), Zx.P(15)));
		tete.Paint += delegate(object o, PaintEventArgs e)
		{
			Lang.DrawText(e.Graphics, "Communauté", fTitre, new Point(x0 + Zx.P(84), Zx.P(14)), C.Texte);
			Lang.DrawText(e.Graphics, "Photos et vidéos des joueurs TPS Groupe : partagez vos plus beaux moments sur les cartes et les mods.", fSous, new Point(x0 + Zx.P(86), Zx.P(52)), C.Gris);
		};
		bPublier = new Bouton("＋  Publier une photo ou une vidéo")
		{
			Size = new Size(Zx.P(280), Zx.P(40)),
			Location = new Point(x0 + largeur - Zx.P(24) - Zx.P(280), Zx.P(20))
		};
		bPublier.Click += delegate
		{
			Publier();
		};
		tete.Controls.Add(bPublier);
		onglets = new Segments();
		onglets.Items = new string[3] { "Récentes", "Populaires", "Mes publications" };
		onglets.Location = new Point(x0 + Zx.P(24), Zx.P(84));
		onglets.Size = new Size(Math.Min(Zx.P(620), largeur - Zx.P(48)), Zx.P(36));
		onglets.Etire = true;
		onglets.Changed += delegate
		{
			ouverte = null;
			defile = 0;
			Charger();
		};
		tete.Controls.Add(onglets);
		z = new ZoneDessin
		{
			Dock = DockStyle.Fill,
			Dessiner = Dessiner
		};
		Controls.Add(z);
		z.BringToFront();
		z.MouseMove += delegate(object o, MouseEventArgs e)
		{
			souris = e.Location;
			z.Cursor = clics.Exists((KeyValuePair<Rectangle, Action> k) => k.Key.Contains(e.Location)) ? Cursors.Hand : Cursors.Default;
			z.Invalidate();
		};
		z.MouseLeave += delegate
		{
			souris = new Point(-1, -1);
			z.Invalidate();
		};
		z.MouseClick += delegate(object o, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
			{
				return;
			}
			foreach (KeyValuePair<Rectangle, Action> k in new List<KeyValuePair<Rectangle, Action>>(clics))
			{
				if (k.Key.Contains(e.Location))
				{
					Sons.Clic();
					k.Value();
					z.Invalidate();
					return;
				}
			}
			if (ouverte != null)
			{
				Fermer();   // clic à côté de l'image : retour à la grille
			}
		};
		z.MouseWheel += delegate(object o, MouseEventArgs e)
		{
			if (ouverte != null)
			{
				return;
			}
			defile = Math.Max(0, Math.Min(Math.Max(0, hauteurContenu - z.Height), defile - e.Delta * Zx.P(90) / 120));
			z.Invalidate();
		};
		KeyPreview = true;
		KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (ouverte == null)
			{
				return;
			}
			if (e.KeyCode == Keys.Escape)
			{
				Fermer();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
			{
				Voisine((e.KeyCode == Keys.Right) ? 1 : (-1));
				e.Handled = true;
			}
		};
		Shown += delegate
		{
			Charger();
		};
	}

	private void Message(string t, Color c)
	{
		lMsg.Text = Lang.T(t);
		lMsg.ForeColor = c;
	}

	private string VueCourante => vues[Math.Max(0, Math.Min(vues.Length - 1, onglets.Index))];

	/// <summary>Travail en arrière-plan, puis suite sur la page (si elle est encore ouverte).</summary>
	private void Fond(Action travail, Action apres)
	{
		Thread t = new Thread((ThreadStart)delegate
		{
			try
			{
				travail();
			}
			catch
			{
			}
			try
			{
				if (!IsDisposed && IsHandleCreated)
				{
					BeginInvoke((MethodInvoker)delegate
					{
						if (!IsDisposed)
						{
							apres?.Invoke();
						}
					});
				}
			}
			catch
			{
			}
		});
		t.IsBackground = true;
		t.Start();
	}

	private void Charger()
	{
		string demo = Environment.GetEnvironmentVariable("TPS_DEMO_PARTAGE");
		if (!string.IsNullOrEmpty(demo) && File.Exists(demo))
		{
			// outil de développement : grille remplie de publications fictives
			charge = true;
			pubs = new List<Social.Publication>();
			string[] noms = new string[6] { "Coucher de soleil ligne 25", "Terminus Vitry", "", "Nuit à Périgueux", "Dépôt RATP", "Trottinette en ville" };
			using (Image im = Image.FromFile(demo))
			{
				for (int i = 0; i < 6; i++)
				{
					pubs.Add(new Social.Publication { Id = 900 + i, Type = (i == 2) ? "video" : "photo", Titre = noms[i], Auteur = "Joueur" + i, Cree = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - i * 7200, Likes = i * 3, Aime = i == 1, Statut = (i == 4) ? "attente" : "ok", Mod = (i == 0) ? "paris25" : "" });
					miniatures[900 + i] = Couvertures.Composer(im, Zx.P(640), Zx.P(400));
				}
			}
			z.Invalidate();
			return;
		}
		if (!Social.Actif)
		{
			charge = true;
			erreur = "Le serveur social n'est pas configuré : la communauté est indisponible.";
			z.Invalidate();
			return;
		}
		int num = ++requete;
		string vue = VueCourante;
		charge = false;
		erreur = "";
		z.Invalidate();
		Social.PageCommunaute res = null;
		string err = null;
		Fond(delegate
		{
			try
			{
				err = Social.Assurer(Comptes.Actif);
				if (err == null)
				{
					res = Social.Communaute(vue, 0L, 60);
				}
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
		}, delegate
		{
			if (num != requete)
			{
				return;
			}
			charge = true;
			if (res == null)
			{
				erreur = "Communauté indisponible : " + (err ?? "serveur injoignable") + ".";
				pubs = new List<Social.Publication>();
			}
			else
			{
				pubs = res.Publications;
				attente = res.Attente;
				if (res.Modo != modo)
				{
					modo = res.Modo;
					int idx = onglets.Index;
					vues = (modo ? new string[4] { "recentes", "populaires", "miennes", "attente" } : new string[3] { "recentes", "populaires", "miennes" });
					onglets.Items = (modo ? new string[4] { "Récentes", "Populaires", "Mes publications", "À valider" } : new string[3] { "Récentes", "Populaires", "Mes publications" });
					onglets.Index = Math.Min(idx, vues.Length - 1);
					onglets.Invalidate();
				}
				if (modo && onglets.Items.Length == 4)
				{
					onglets.Items[3] = (attente > 0) ? (Lang.T("À valider") + "  ·  " + attente) : "À valider";
					onglets.Invalidate();
				}
				foreach (Social.Publication p in pubs)
				{
					Miniature(p);
				}
			}
			z.Invalidate();
		});
	}

	/// <summary>Miniature d'une publication : depuis le cache disque, sinon téléchargée (photos seulement).</summary>
	private void Miniature(Social.Publication p)
	{
		if (p.EstVideo || miniatures.ContainsKey(p.Id) || enCours.Contains(p.Id))
		{
			return;
		}
		enCours.Add(p.Id);
		Bitmap mini = null;
		Fond(delegate
		{
			byte[] b = LireImage(p);
			if (b == null)
			{
				return;
			}
			using MemoryStream ms = new MemoryStream(b);
			using Image im = Image.FromStream(ms);
			int w = Zx.P(640);
			int h = w * 10 / 16;
			mini = Couvertures.Composer(im, w, h);
		}, delegate
		{
			enCours.Remove(p.Id);
			if (mini != null)
			{
				miniatures[p.Id] = mini;
				z.Invalidate();
			}
		});
	}

	/// <summary>Octets d'une photo : cache disque (publications validées), sinon le serveur.</summary>
	internal static byte[] LireImage(Social.Publication p)
	{
		string f = Path.Combine(Cache, p.Id + ".img");
		try
		{
			if (p.Statut == "ok" && File.Exists(f))
			{
				return File.ReadAllBytes(f);
			}
		}
		catch
		{
		}
		byte[] b = Social.Media(p.Id);
		if (p.Statut == "ok")
		{
			try
			{
				Directory.CreateDirectory(Cache);
				File.WriteAllBytes(f, b);
			}
			catch
			{
			}
		}
		return b;
	}

	private void Ajouter(Rectangle r, Action a)
	{
		clics.Add(new KeyValuePair<Rectangle, Action>(r, a));
	}

	private void Pilule(Graphics g, Rectangle r, string t, Color c, bool plein, Action a)
	{
		bool sv = r.Contains(souris);
		using (GraphicsPath p = Bouton.Rond(r, r.Height / 2))
		{
			using SolidBrush b = new SolidBrush(plein ? (sv ? ControlPaint.Light(c, 0.15f) : c) : Color.FromArgb(sv ? 60 : 30, c));
			g.FillPath(b, p);
			if (!plein)
			{
				using Pen pen = new Pen(Color.FromArgb(150, c));
				g.DrawPath(pen, p);
			}
		}
		Lang.DrawText(g, t, fBt, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
		if (a != null)
		{
			Ajouter(r, a);
		}
	}

	private void Dessiner(Graphics g, int w, int h)
	{
		g.SmoothingMode = SmoothingMode.AntiAlias;
		clics.Clear();
		int largeur = Math.Max(Math.Min(w, Zx.P(900)), Math.Min(w - Zx.P(48), Zx.P(2400)));
		int x0 = (w - largeur) / 2 + Zx.P(24);
		int lu = largeur - Zx.P(48);
		if (ouverte != null)
		{
			DessinerVisionneuse(g, w, h);
			return;
		}
		if (!charge)
		{
			Lang.DrawText(g, "Chargement…", fSous, new Rectangle(0, Zx.P(40), w, Zx.P(30)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
			return;
		}
		if (erreur.Length > 0)
		{
			Lang.DrawText(g, erreur, fSous, new Rectangle(x0, Zx.P(40), lu, Zx.P(60)), C.Rouge, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			Pilule(g, new Rectangle(w / 2 - Zx.P(70), Zx.P(100), Zx.P(140), Zx.P(34)), "Réessayer", C.Jade, plein: false, Charger);
			return;
		}
		if (pubs.Count == 0)
		{
			string vide = (VueCourante == "miennes") ? "Vous n'avez encore rien publié. Partagez une capture de vos parties !" : ((VueCourante == "attente") ? "Rien à valider pour le moment." : "Aucune publication pour le moment : soyez le premier à partager une photo !");
			Lang.DrawText(g, vide, fSous, new Rectangle(x0, Zx.P(60), lu, Zx.P(40)), C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
			return;
		}
		int gap = Zx.P(18);
		int nc = Math.Max(1, (lu + gap) / (Zx.P(300) + gap));
		int cw = (lu - (nc - 1) * gap) / nc;
		int ih = cw * 10 / 16;
		int ch = ih + Zx.P(70) + ((VueCourante == "attente") ? Zx.P(46) : 0);
		int y0 = Zx.P(8) - defile;
		for (int i = 0; i < pubs.Count; i++)
		{
			Rectangle r = new Rectangle(x0 + i % nc * (cw + gap), y0 + i / nc * (ch + gap), cw, ch);
			if (r.Bottom < 0 || r.Top > h)
			{
				continue;
			}
			DessinerCarte(g, pubs[i], r, ih);
		}
		hauteurContenu = Zx.P(8) + (pubs.Count + nc - 1) / nc * (ch + gap) + Zx.P(20);
		if (hauteurContenu > h)
		{
			// barre de défilement fine
			int hb = Math.Max(Zx.P(40), h * h / hauteurContenu);
			int yb = (int)((long)(h - hb) * defile / Math.Max(1, hauteurContenu - h));
			using SolidBrush bb = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
			using GraphicsPath pb = Bouton.Rond(new Rectangle(w - Zx.P(8), yb, Zx.P(4), hb), Zx.P(2));
			g.FillPath(bb, pb);
		}
	}

	private void DessinerCarte(Graphics g, Social.Publication p, Rectangle r, int ih)
	{
		bool sv = r.Contains(souris);
		using (GraphicsPath pc = Bouton.Rond(r, Zx.P(14)))
		{
			using SolidBrush b = new SolidBrush(sv ? C.CarteSurvol : C.Carte);
			g.FillPath(b, pc);
			using Pen pen = new Pen(sv ? Color.FromArgb(110, 150, 170, 255) : Color.FromArgb(26, 255, 255, 255));
			g.DrawPath(pen, pc);
		}
		Rectangle ri = new Rectangle(r.X + Zx.P(6), r.Y + Zx.P(6), r.Width - Zx.P(12), ih - Zx.P(6));
		GraphicsState st = g.Save();
		using (GraphicsPath pi = Bouton.Rond(ri, Zx.P(10)))
		{
			g.SetClip(pi, CombineMode.Intersect);
		}
		if (miniatures.TryGetValue(p.Id, out var mini))
		{
			g.InterpolationMode = InterpolationMode.HighQualityBilinear;
			g.DrawImage(mini, ri);
		}
		else
		{
			using LinearGradientBrush lg = new LinearGradientBrush(ri, Color.FromArgb(40, 48, 96), Color.FromArgb(18, 22, 48), 45f);
			g.FillRectangle(lg, ri);
		}
		g.Restore(st);
		if (p.EstVideo)
		{
			Rectangle rp = new Rectangle(ri.X + ri.Width / 2 - Zx.P(28), ri.Y + ri.Height / 2 - Zx.P(28), Zx.P(56), Zx.P(56));
			using (SolidBrush bp = new SolidBrush(Color.FromArgb(sv ? 230 : 190, 10, 12, 30)))
			{
				g.FillEllipse(bp, rp);
			}
			Lang.DrawText(g, "", fG, rp, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Pastille(g, new Point(ri.X + Zx.P(10), ri.Y + Zx.P(10)), "VIDÉO", Color.FromArgb(200, 235, 68, 80));
		}
		if (p.Statut == "attente")
		{
			Pastille(g, new Point(ri.Right - Zx.P(10), ri.Y + Zx.P(10)), "EN ATTENTE DE VALIDATION", Color.FromArgb(220, 255, 124, 40), droite: true);
		}
		int tx = r.X + Zx.P(16);
		int tw = r.Width - Zx.P(32);
		string titre = (p.Titre.Length > 0) ? p.Titre : Lang.T("Sans titre");
		Lang.DrawText(g, titre, fCarteT, new Rectangle(tx, r.Y + ih + Zx.P(12), tw - Zx.P(70), Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		string mod = (p.Mod.Length > 0 && NomMod != null) ? (NomMod(p.Mod) ?? "") : "";
		string sous = Lang.T("par ") + p.Auteur + "  ·  " + Lang.T(Fenetre.Depuis(p.Cree)) + ((mod.Length > 0) ? ("  ·  " + mod) : "");
		Lang.DrawText(g, sous, fCarteS, new Rectangle(tx, r.Y + ih + Zx.P(38), tw, Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		// J'aime (publications validées)
		if (p.Statut == "ok")
		{
			Rectangle rl = new Rectangle(r.Right - Zx.P(16) - Zx.P(64), r.Y + ih + Zx.P(10), Zx.P(64), Zx.P(26));
			bool svl = rl.Contains(souris);
			using (GraphicsPath pl = Bouton.Rond(rl, rl.Height / 2))
			{
				using SolidBrush bl = new SolidBrush(p.Aime ? Color.FromArgb(70, 235, 68, 80) : Color.FromArgb(svl ? 40 : 20, 255, 255, 255));
				g.FillPath(bl, pl);
			}
			Lang.DrawText(g, (p.Aime ? "♥" : "♡") + "  " + p.Likes, fBt, rl, p.Aime ? Color.FromArgb(255, 120, 130) : C.Texte, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			Social.Publication pp = p;
			Ajouter(rl, delegate
			{
				Aimer(pp);
			});
		}
		if (VueCourante == "attente")
		{
			int bw = (r.Width - Zx.P(44)) / 2;
			Social.Publication pv = p;
			Pilule(g, new Rectangle(r.X + Zx.P(16), r.Bottom - Zx.P(46), bw, Zx.P(32)), "✔  Valider", C.Vert, plein: true, delegate
			{
				Moderer(pv, "valider");
			});
			Pilule(g, new Rectangle(r.X + Zx.P(28) + bw, r.Bottom - Zx.P(46), bw, Zx.P(32)), "✖  Refuser", C.Rouge, plein: false, delegate
			{
				Moderer(pv, "refuser");
			});
		}
		Social.Publication po = p;
		Ajouter(new Rectangle(r.X, r.Y, r.Width, ih + Zx.P(4)), delegate
		{
			Ouvrir(po);
		});
	}

	private static void Pastille(Graphics g, Point p, string t, Color c, bool droite = false)
	{
		Size s = Lang.MeasureText(t, fCarteS);
		Rectangle r = new Rectangle(droite ? (p.X - s.Width - Zx.P(16)) : p.X, p.Y, s.Width + Zx.P(16), Zx.P(22));
		using (GraphicsPath pp = Bouton.Rond(r, r.Height / 2))
		{
			using SolidBrush b = new SolidBrush(c);
			g.FillPath(b, pp);
		}
		Lang.DrawText(g, t, fCarteS, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
	}

	// ───────────── visionneuse ─────────────

	private void Ouvrir(Social.Publication p)
	{
		if (p.EstVideo)
		{
			LireVideo(p);
			return;
		}
		ouverte = p;
		grande?.Dispose();
		grande = null;
		grandeEchec = false;
		z.Invalidate();
		Image im = null;
		Fond(delegate
		{
			if (miniatures.TryGetValue(p.Id, out var mini) && p.Id >= 900 && p.Id < 1000 && Environment.GetEnvironmentVariable("TPS_DEMO_PARTAGE") != null)
			{
				im = new Bitmap(mini);
				return;
			}
			byte[] b = LireImage(p);
			if (b != null)
			{
				using MemoryStream ms = new MemoryStream(b);
				using Image tmp = Image.FromStream(ms);
				im = new Bitmap(tmp);
			}
		}, delegate
		{
			if (ouverte == p)
			{
				grande = im;
				grandeEchec = im == null;
				z.Invalidate();
			}
			else
			{
				im?.Dispose();
			}
		});
	}

	private void Fermer()
	{
		ouverte = null;
		grande?.Dispose();
		grande = null;
		z.Invalidate();
	}

	private void Voisine(int sens)
	{
		if (ouverte == null)
		{
			return;
		}
		int i = pubs.IndexOf(ouverte);
		for (int k = 0; k < pubs.Count; k++)
		{
			i = (i + sens + pubs.Count) % pubs.Count;
			if (!pubs[i].EstVideo)
			{
				Ouvrir(pubs[i]);
				return;
			}
		}
	}

	private void DessinerVisionneuse(Graphics g, int w, int h)
	{
		Social.Publication p = ouverte;
		using (SolidBrush fond = new SolidBrush(Color.FromArgb(6, 8, 20)))
		{
			g.FillRectangle(fond, 0, 0, w, h);
		}
		int barre = Zx.P(64);
		Rectangle zone = new Rectangle(Zx.P(70), Zx.P(10), w - Zx.P(140), h - barre - Zx.P(20));
		if (grande != null)
		{
			double k = Math.Min((double)zone.Width / grande.Width, (double)zone.Height / grande.Height);
			int iw = (int)(grande.Width * k);
			int ih = (int)(grande.Height * k);
			Rectangle ri = new Rectangle(zone.X + (zone.Width - iw) / 2, zone.Y + (zone.Height - ih) / 2, iw, ih);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(grande, ri);
			Ajouter(ri, delegate
			{
			});
		}
		else
		{
			Lang.DrawText(g, grandeEchec ? "Photo indisponible pour le moment." : "Chargement de la photo…", fSous, zone, C.Gris, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
		// flèches
		if (pubs.FindAll((Social.Publication x) => !x.EstVideo).Count > 1)
		{
			Rectangle rg = new Rectangle(Zx.P(14), h / 2 - barre / 2 - Zx.P(24), Zx.P(44), Zx.P(48));
			Rectangle rd = new Rectangle(w - Zx.P(58), rg.Y, Zx.P(44), Zx.P(48));
			Pilule(g, rg, "‹", Color.FromArgb(90, 110, 200), plein: false, delegate
			{
				Voisine(-1);
			});
			Pilule(g, rd, "›", Color.FromArgb(90, 110, 200), plein: false, delegate
			{
				Voisine(1);
			});
		}
		// barre du bas : titre, auteur, J'aime, supprimer, fermer
		Rectangle rb = new Rectangle(0, h - barre, w, barre);
		using (SolidBrush bb = new SolidBrush(Color.FromArgb(14, 17, 40)))
		{
			g.FillRectangle(bb, rb);
		}
		int x = Zx.P(24);
		string titre = (p.Titre.Length > 0) ? p.Titre : Lang.T("Sans titre");
		Lang.DrawText(g, titre, fCarteT, new Rectangle(x, rb.Y + Zx.P(12), w / 2, Zx.P(22)), C.Texte, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		string mod = (p.Mod.Length > 0 && NomMod != null) ? (NomMod(p.Mod) ?? "") : "";
		Lang.DrawText(g, Lang.T("par ") + p.Auteur + "  ·  " + Lang.T(Fenetre.Depuis(p.Cree)) + ((mod.Length > 0) ? ("  ·  " + mod) : ""), fCarteS, new Rectangle(x, rb.Y + Zx.P(36), w / 2, Zx.P(18)), C.Gris, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
		int bx = w - Zx.P(24);
		Rectangle rf = new Rectangle(bx - Zx.P(110), rb.Y + Zx.P(14), Zx.P(110), Zx.P(36));
		Pilule(g, rf, "Fermer  ✕", Color.FromArgb(90, 110, 200), plein: false, Fermer);
		bx = rf.X - Zx.P(10);
		if (p.Moi || modo)
		{
			Rectangle rs = new Rectangle(bx - Zx.P(120), rf.Y, Zx.P(120), Zx.P(36));
			Social.Publication ps = p;
			Pilule(g, rs, "Supprimer", C.Rouge, plein: false, delegate
			{
				Supprimer(ps);
			});
			bx = rs.X - Zx.P(10);
		}
		if (p.Statut == "ok")
		{
			Rectangle rl = new Rectangle(bx - Zx.P(110), rf.Y, Zx.P(110), Zx.P(36));
			Social.Publication pl = p;
			Pilule(g, rl, (p.Aime ? "♥  " : "♡  ") + p.Likes, p.Aime ? Color.FromArgb(235, 68, 80) : Color.FromArgb(90, 110, 200), p.Aime, delegate
			{
				Aimer(pl);
			});
			bx = rl.X - Zx.P(10);
		}
		if (p.Mod.Length > 0 && mod.Length > 0)
		{
			Rectangle rm = new Rectangle(bx - Zx.P(170), rf.Y, Zx.P(170), Zx.P(36));
			string id = p.Mod;
			Pilule(g, rm, "Voir le mod  ›", C.Jade, plein: false, delegate
			{
				Demande = "mod:" + id;
				DialogResult = DialogResult.OK;
			});
		}
	}

	// ───────────── actions ─────────────

	private void Aimer(Social.Publication p)
	{
		bool avant = p.Aime;
		p.Aime = !avant;
		p.Likes = Math.Max(0, p.Likes + (p.Aime ? 1 : (-1)));
		z.Invalidate();
		int likes = -1;
		bool aime = p.Aime;
		string err = null;
		Fond(delegate
		{
			try
			{
				string e0 = Social.Assurer(Comptes.Actif);
				if (e0 != null)
				{
					throw new Exception(e0);
				}
				likes = Social.AimerPublication(p.Id, out aime);
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
		}, delegate
		{
			if (err != null)
			{
				p.Aime = avant;
				p.Likes = Math.Max(0, p.Likes + (avant ? 1 : (-1)));
				Message("✖  " + err, C.Rouge);
			}
			else
			{
				p.Aime = aime;
				p.Likes = likes;
			}
			z.Invalidate();
		});
	}

	private void Moderer(Social.Publication p, string action)
	{
		string err = null;
		Fond(delegate
		{
			try
			{
				err = Social.Moderer(p.Id, action);
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
		}, delegate
		{
			if (err != null)
			{
				Message("✖  " + err, C.Rouge);
				return;
			}
			pubs.Remove(p);
			attente = Math.Max(0, attente - 1);
			if (onglets.Items.Length == 4)
			{
				onglets.Items[3] = (attente > 0) ? (Lang.T("À valider") + "  ·  " + attente) : "À valider";
				onglets.Invalidate();
			}
			Message((action == "valider") ? "✔  Publication validée : elle est visible par tous." : "Publication refusée et supprimée.", (action == "valider") ? C.Vert : C.Gris);
			z.Invalidate();
		});
	}

	private void Supprimer(Social.Publication p)
	{
		if (MessageBox.Show(this, Lang.T("Supprimer définitivement cette publication ?"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
		{
			return;
		}
		string err = null;
		Fond(delegate
		{
			try
			{
				err = Social.SupprimerPublication(p.Id);
			}
			catch (Exception ex)
			{
				err = ex.Message;
			}
		}, delegate
		{
			if (err != null)
			{
				Message("✖  " + err, C.Rouge);
				return;
			}
			pubs.Remove(p);
			try
			{
				File.Delete(Path.Combine(Cache, p.Id + ".img"));
			}
			catch
			{
			}
			Fermer();
			Message("Publication supprimée.", C.Gris);
		});
	}

	/// <summary>Vidéo : lue directement depuis le serveur une fois validée ; avant, téléchargée puis lue (auteur, équipe).</summary>
	private void LireVideo(Social.Publication p)
	{
		if (p.Statut == "ok")
		{
			using FenVideo f = new FenVideo((p.Titre.Length > 0) ? p.Titre : Lang.T("Vidéo de ") + p.Auteur, Social.MediaUrl(p.Id));
			f.Afficher(this);
			return;
		}
		string tmp = Path.Combine(Path.GetTempPath(), "tps-communaute-" + p.Id + ((p.Mime == "video/webm") ? ".webm" : ".mp4"));
		Message("Téléchargement de la vidéo…", C.Gris);
		string err = null;
		Fond(delegate
		{
			try
			{
				if (!File.Exists(tmp))
				{
					Social.MediaVersFichier(p.Id, tmp, null);
				}
			}
			catch (Exception ex)
			{
				err = ex.Message;
				try
				{
					File.Delete(tmp);
				}
				catch
				{
				}
			}
		}, delegate
		{
			if (err != null)
			{
				Message("✖  " + err, C.Rouge);
				return;
			}
			Message("", C.Gris);
			using FenVideo f = new FenVideo((p.Titre.Length > 0) ? p.Titre : Lang.T("Vidéo de ") + p.Auteur, tmp);
			f.Afficher(this);
		});
	}

	private void Publier()
	{
		if (!Social.Actif)
		{
			return;
		}
		string chemin;
		using (OpenFileDialog d = new OpenFileDialog())
		{
			d.Title = Lang.T("Choisir une photo ou une vidéo");
			d.Filter = Lang.T("Photos et vidéos") + "|*.jpg;*.jpeg;*.png;*.webp;*.mp4;*.m4v;*.webm|" + Lang.T("Photos") + "|*.jpg;*.jpeg;*.png;*.webp|" + Lang.T("Vidéos") + "|*.mp4;*.m4v;*.webm";
			if (d.ShowDialog(this) != DialogResult.OK)
			{
				return;
			}
			chemin = d.FileName;
		}
		using FenPublier f = new FenPublier(chemin, (ModsCatalogue != null) ? ModsCatalogue() : new List<KeyValuePair<string, string>>());
		if (f.Afficher(this) == DialogResult.OK)
		{
			Message((f.Statut == "ok") ? "✔  Publié : visible par tous." : "✔  Envoyé ! Votre publication sera visible après validation par l'équipe.", C.Vert);
			onglets.Index = (f.Statut == "ok") ? 0 : 2;
			onglets.Invalidate();
			defile = 0;
			Charger();
		}
	}
}
