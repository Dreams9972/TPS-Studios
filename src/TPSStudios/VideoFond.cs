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

	private string url = "";

	private readonly Timer minuteur;

	private int essais;

	private bool pret;

	private bool joue;

	private bool muet = true;

	private bool abandon;

	private bool montre = true;

	private DateTime debut;

	private readonly Bouton bSon;

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
		bSon = new Bouton("");
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
				LecteurWmp.Fixer(LecteurWmp.Lire(wmp.Ocx, "settings"), "mute", muet);
			}
			catch
			{
			}
		};
		base.Controls.Add(bSon);
		bSon.BringToFront();
		minuteur = new Timer();
		minuteur.Interval = 400;
		minuteur.Tick += delegate
		{
			Tic();
		};
		minuteur.Start();
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		if (bSon != null)
		{
			bSon.Location = new Point(base.Width - bSon.Width - Zx.P(14), base.Height - bSon.Height - Zx.P(14));
		}
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
		base.Visible = montre && url.Length > 0 && wmp != null;
	}

	/// <summary>Montre ou cache la vidéo sans l'oublier (onglet changé, page masquée…).</summary>
	public void Afficher(bool m)
	{
		montre = m;
		bool flag = m && url.Length > 0 && wmp != null && !abandon;
		if (base.Visible != flag)
		{
			base.Visible = flag;
		}
	}

	/// <summary>Le lecteur Windows Media lit les fichiers vidéo directs (mp4…), pas YouTube.</summary>
	public static bool Lisible(string u)
	{
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
		if (wmp == null || url.Length == 0 || abandon || base.IsDisposed)
		{
			return;
		}
		bool flag = base.Visible && base.Parent != null && base.Parent.Visible && (Actif == null || Actif());
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
				LecteurWmp.Fixer(obj2, "volume", 70);
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
