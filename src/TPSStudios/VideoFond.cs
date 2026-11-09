using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Bande-annonce intégrée en haut d'une page (lecteur Windows Media) : muette au départ,
/// en boucle, mise en pause quand la page n'est plus visible. Si la vidéo ne démarre pas,
/// le contrôle se cache et l'image peinte dessous réapparaît.
/// </summary>
internal class VideoFond : Panel
{
	private LecteurWmp wmp;

	/// <summary>LecteurWeb (WebView2), typé Control : WebView2 n'est chargé que si on s'en sert.</summary>
	private Control web;

	private bool modeWeb;

	private string url = "";

	private readonly Timer minuteur;

	private int essais;

	private bool pret;

	private bool joue;

	/// <summary>Volume des bandes-annonces : 5 %, son activé au départ (bouton pour couper).</summary>
	public const int Volume = 5;

	private bool muet;

	private bool abandon;

	private bool montre = true;

	private DateTime debut;

	private readonly Bouton bSon;

	/// <summary>Bouton lecture / pause, à gauche du bouton son.</summary>
	private readonly Bouton bPause;

	/// <summary>Mise en pause demandée par le joueur (gardée tant que la vidéo ne change pas).</summary>
	private bool enPause;

	/// <summary>Rayon des coins : la vidéo est découpée à la forme du cadre arrondi de la page.</summary>
	public int Arrondi = Zx.P(14);

	/// <summary>Vrai quand la page qui contient la vidéo est à l'écran.</summary>
	public Func<bool> Actif;

	public bool Lit
	{
		get
		{
			if (base.Visible && pret)
			{
				return !abandon;
			}
			return false;
		}
	}

	public VideoFond()
	{
		BackColor = Color.Black;
		base.Visible = false;
		try
		{
			if (!WmpDispo())
			{
				throw new NotSupportedException("lecteur Windows Media absent");
			}
			wmp = new LecteurWmp();
			((ISupportInitialize)wmp).BeginInit();
			wmp.Dock = DockStyle.Fill;
			base.Controls.Add(wmp);
			((ISupportInitialize)wmp).EndInit();
		}
		catch (Exception e)
		{
			Journal.Une("video-fond", e);
			wmp = null;
		}
		bSon = new Bouton("");
		bSon.Plein = false;
		bSon.Font = new Font("Segoe MDL2 Assets", Zx.P(12f));
		bSon.Size = new Size(Zx.P(42), Zx.P(42));
		bSon.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		bSon.Click += delegate
		{
			muet = !muet;
			bSon.Text = (muet ? "" : "");
			try
			{
				if (modeWeb)
				{
					WebMuet(muet);
				}
				else
				{
					LecteurWmp.Fixer(LecteurWmp.Lire(wmp.Ocx, "settings"), "mute", muet);
				}
			}
			catch
			{
			}
		};
		base.Controls.Add(bSon);
		bSon.BringToFront();
		bPause = new Bouton("\ue769");
		bPause.Plein = false;
		bPause.Font = new Font("Segoe MDL2 Assets", Zx.P(12f));
		bPause.Size = new Size(Zx.P(42), Zx.P(42));
		bPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		bPause.Click += delegate
		{
			enPause = !enPause;
			MajPause();
			Tic();
		};
		base.Controls.Add(bPause);
		bPause.BringToFront();
		minuteur = new Timer();
		minuteur.Interval = 400;
		minuteur.Tick += delegate
		{
			Tic();
		};
		minuteur.Start();
	}

	private static int dispo = -1;

	/// <summary>Le lecteur Windows Media est-il installé (absent des éditions N de Windows et hors Windows) ?</summary>
	public static bool WmpDispo()
	{
		if (dispo < 0)
		{
			dispo = 0;
			if (Type.GetType("Mono.Runtime") != null || Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				return false;
			}
			try
			{
				Type typeFromCLSID = Type.GetTypeFromCLSID(new Guid("6bf52a52-394a-11d3-b153-00c04f79faa6"), throwOnError: false);
				if (typeFromCLSID != null)
				{
					object o = Activator.CreateInstance(typeFromCLSID);
					System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
					dispo = 1;
				}
			}
			catch
			{
				dispo = 0;
			}
		}
		return dispo == 1;
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (base.Width > 4 && base.Height > 4)
		{
			using (System.Drawing.Drawing2D.GraphicsPath path = Bouton.Rond(new Rectangle(0, 0, base.Width, base.Height), Arrondi))
			{
				Region old = base.Region;
				base.Region = new Region(path);
				old?.Dispose();
			}
		}
		if (bSon != null)
		{
			bSon.Location = new Point(base.Width - bSon.Width - Zx.P(14), base.Height - bSon.Height - Zx.P(14));
		}
		if (bPause != null && bSon != null)
		{
			bPause.Location = new Point(bSon.Left - bPause.Width - Zx.P(8), bSon.Top);
		}
	}

	private void MajPause()
	{
		bPause.Text = (enPause ? "\ue768" : "\ue769");
		bPause.Invalidate();
	}

	/// <summary>Change de vidéo (adresse vide ou non lisible : rien n'est affiché).</summary>
	public void Charger(string u)
	{
		u = u ?? "";
		if (!Lisible(u))
		{
			u = "";
		}
		if (u == url)
		{
			return;
		}
		url = u;
		enPause = false;
		MajPause();
		pret = false;
		joue = false;
		abandon = false;
		essais = 0;
		try
		{
			if (wmp != null && wmp.Ocx != null)
			{
				LecteurWmp.Appeler(LecteurWmp.Lire(wmp.Ocx, "controls"), "stop");
			}
		}
		catch
		{
		}
		string text = YouTube.Id(url);
		modeWeb = text != null;
		if (modeWeb)
		{
			try
			{
				if (web == null)
				{
					CreerWeb();
				}
				web.Visible = true;
				WebMuet(muet);
				WebCharger(text);
			}
			catch (Exception e)
			{
				Journal.Une("video-youtube", e);
				abandon = true;
			}
			if (wmp != null)
			{
				wmp.Visible = false;
			}
		}
		else
		{
			if (web != null)
			{
				web.Visible = false;
				try
				{
					WebCharger("");
				}
				catch
				{
				}
			}
			if (wmp != null)
			{
				wmp.Visible = true;
			}
		}
		Afficher(montre);
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private void CreerWeb()
	{
		LecteurWeb lecteurWeb = new LecteurWeb();
		lecteurWeb.Dock = DockStyle.Fill;
		lecteurWeb.Echec += delegate
		{
			Abandonner();
		};
		web = lecteurWeb;
		base.Controls.Add(web);
		bSon.BringToFront();
		bPause.BringToFront();
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private void WebCharger(string id)
	{
		((LecteurWeb)web).Charger(id);
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private void WebMuet(bool m)
	{
		((LecteurWeb)web).Muet = m;
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private void WebLecture(bool lire)
	{
		((LecteurWeb)web).Lecture(lire);
	}

	private bool suspendue;

	/// <summary>Cache la vidéo le temps d'une transition (l'image peinte dessous la remplace).</summary>
	public void Suspendre(bool s)
	{
		suspendue = s;
		Afficher(montre);
	}

	/// <summary>Montre ou cache la vidéo sans l'oublier (onglet changé, page masquée…).</summary>
	public void Afficher(bool m)
	{
		montre = m;
		bool flag = m && !suspendue && url.Length > 0 && (modeWeb || wmp != null) && !abandon;
		if (base.Visible != flag)
		{
			base.Visible = flag;
		}
	}

	/// <summary>Vidéos lisibles : fichiers directs (mp4… via Windows Media) et YouTube (via WebView2).</summary>
	public static bool Lisible(string u)
	{
		if (YouTube.Id(u) != null)
		{
			return WebView2Embarque.Possible;
		}
		if (!Uri.TryCreate(u ?? "", UriKind.Absolute, out var result) || (result.Scheme != Uri.UriSchemeHttps && result.Scheme != Uri.UriSchemeHttp))
		{
			return false;
		}
		string text = result.AbsolutePath.ToLowerInvariant();
		if (!text.EndsWith(".mp4") && !text.EndsWith(".m4v") && !text.EndsWith(".wmv"))
		{
			return text.EndsWith(".webm");
		}
		return true;
	}

	private void Tic()
	{
		if (url.Length == 0 || abandon || base.IsDisposed)
		{
			return;
		}
		bool flag = !enPause && base.Visible && base.Parent != null && base.Parent.Visible && (Actif == null || Actif());
		if (modeWeb)
		{
			if (web != null)
			{
				try
				{
					WebLecture(flag);
				}
				catch
				{
				}
			}
			return;
		}
		if (wmp == null)
		{
			return;
		}
		object obj = null;
		try
		{
			obj = (wmp.IsHandleCreated ? wmp.Ocx : null);
		}
		catch
		{
		}
		if (obj == null)
		{
			if (++essais > 40)
			{
				Abandonner();
			}
			return;
		}
		try
		{
			if (!pret)
			{
				if (!flag)
				{
					return;
				}
				LecteurWmp.Fixer(obj, "uiMode", "none");
				try
				{
					LecteurWmp.Fixer(obj, "stretchToFit", true);
					LecteurWmp.Fixer(obj, "enableContextMenu", false);
				}
				catch
				{
				}
				object obj2 = LecteurWmp.Lire(obj, "settings");
				LecteurWmp.Fixer(obj2, "autoStart", true);
				LecteurWmp.Fixer(obj2, "mute", muet);
				LecteurWmp.Fixer(obj2, "volume", Volume);
				try
				{
					obj2.GetType().InvokeMember("setMode", System.Reflection.BindingFlags.InvokeMethod, null, obj2, new object[2] { "loop", true });
				}
				catch
				{
				}
				LecteurWmp.Fixer(obj, "URL", url);
				pret = true;
				joue = true;
				debut = DateTime.Now;
				return;
			}
			int num = Convert.ToInt32(LecteurWmp.Lire(obj, "playState"));
			if (num != 3 && joue && (DateTime.Now - debut).TotalSeconds > 20.0 && num != 2)
			{
				Journal.Ecrire("video-fond", new Exception("la vidéo ne démarre pas (état " + num + ") : " + url));
				Abandonner();
				return;
			}
			if (num == 3)
			{
				debut = DateTime.Now;
			}
			object o = LecteurWmp.Lire(obj, "controls");
			if (flag && !joue)
			{
				LecteurWmp.Appeler(o, "play");
				joue = true;
				debut = DateTime.Now;
			}
			else if (!flag && joue)
			{
				LecteurWmp.Appeler(o, "pause");
				joue = false;
			}
		}
		catch (Exception e)
		{
			Journal.Une("video-fond", e);
			Abandonner();
		}
	}

	private void Abandonner()
	{
		abandon = true;
		try
		{
			if (wmp != null && wmp.Ocx != null)
			{
				LecteurWmp.Appeler(LecteurWmp.Lire(wmp.Ocx, "controls"), "stop");
			}
		}
		catch
		{
		}
		base.Visible = false;
		base.Parent?.Invalidate();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			minuteur.Stop();
			minuteur.Dispose();
			try
			{
				if (wmp != null && wmp.Ocx != null)
				{
					LecteurWmp.Appeler(LecteurWmp.Lire(wmp.Ocx, "controls"), "stop");
				}
			}
			catch
			{
			}
		}
		base.Dispose(disposing);
	}
}
