using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TPSStudios;

/// <summary>
/// Lecteur de bandes-annonces YouTube (WebView2) : la vidéo remplit la zone, démarre seule,
/// en boucle, sans les commandes YouTube. Le son se coupe par WebView2 (IsMuted).
/// La page passe par un hôte virtuel (https://tps.lanceur/) car YouTube refuse les
/// lecteurs intégrés sans site d'origine (erreur 153).
/// </summary>
internal class LecteurWeb : Panel
{
	private readonly WebView2 wv;

	private bool pret;

	private bool echec;

	private string id;

	private bool muet;

	private bool joue = true;

	private static bool loaderFixe;

	public event Action Echec;

	public bool Muet
	{
		get
		{
			return muet;
		}
		set
		{
			muet = value;
			try
			{
				if (pret)
				{
					wv.CoreWebView2.IsMuted = muet;
				}
			}
			catch
			{
			}
		}
	}

	public LecteurWeb()
	{
		BackColor = Color.Black;
		wv = new WebView2();
		wv.DefaultBackgroundColor = Color.Black;
		wv.Dock = DockStyle.Fill;
		base.Controls.Add(wv);
		Demarrer();
	}

	private async void Demarrer()
	{
		try
		{
			string text = Path.Combine(Config.Dossier, "webview2");
			if (!loaderFixe)
			{
				loaderFixe = true;
				CoreWebView2Environment.SetLoaderDllFolderPath(WebView2Embarque.DossierLoader());
			}
			string text2 = Path.Combine(text, "page");
			Directory.CreateDirectory(text2);
			File.WriteAllText(Path.Combine(text2, "lecteur.html"), Page(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required");
			CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(text, "donnees"), options);
			await wv.EnsureCoreWebView2Async(environment);
			CoreWebView2Settings settings = wv.CoreWebView2.Settings;
			settings.AreDefaultContextMenusEnabled = false;
			settings.AreDevToolsEnabled = false;
			settings.IsStatusBarEnabled = false;
			settings.IsZoomControlEnabled = false;
			settings.AreBrowserAcceleratorKeysEnabled = false;
			wv.CoreWebView2.SetVirtualHostNameToFolderMapping("tps.lanceur", text2, CoreWebView2HostResourceAccessKind.Allow);
			wv.CoreWebView2.IsMuted = muet;
			wv.CoreWebView2.NewWindowRequested += delegate(object o, CoreWebView2NewWindowRequestedEventArgs e)
			{
				e.Handled = true;
			};
			pret = true;
			if (id != null)
			{
				Naviguer();
			}
		}
		catch (Exception e2)
		{
			Journal.Une("webview2", e2);
			echec = true;
			this.Echec?.Invoke();
		}
	}

	public bool EnEchec => echec;

	/// <summary>Lance la vidéo YouTube (identifiant à 11 caractères).</summary>
	public void Charger(string idYouTube)
	{
		if (idYouTube == id)
		{
			return;
		}
		id = idYouTube;
		joue = true;
		if (pret)
		{
			Naviguer();
		}
	}

	private void Naviguer()
	{
		try
		{
			wv.CoreWebView2.Navigate("https://tps.lanceur/lecteur.html?v=" + Uri.EscapeDataString(id));
		}
		catch (Exception e)
		{
			Journal.Une("webview2", e);
		}
	}

	public void Lecture(bool lire)
	{
		if (lire == joue || !pret)
		{
			return;
		}
		joue = lire;
		try
		{
			wv.CoreWebView2.ExecuteScriptAsync(lire ? "cmd('playVideo')" : "cmd('pauseVideo')");
		}
		catch
		{
		}
	}

	private static string Page()
	{
		return "<!doctype html><html><head><meta charset=\"utf-8\"><meta name=\"referrer\" content=\"strict-origin-when-cross-origin\">" + "<style>html,body{margin:0;height:100%;background:#000;overflow:hidden}iframe{position:absolute;border:0;pointer-events:none}</style></head>" + "<body><iframe id=\"v\" allow=\"autoplay; encrypted-media; picture-in-picture\" referrerpolicy=\"strict-origin-when-cross-origin\"></iframe><script>" + "var id=new URLSearchParams(location.search).get('v')||'';var f=document.getElementById('v');" + "f.src='https://www.youtube-nocookie.com/embed/'+encodeURIComponent(id)+'?autoplay=1&mute=0&controls=0&loop=1&playlist='+encodeURIComponent(id)+'&modestbranding=1&rel=0&playsinline=1&iv_load_policy=3&disablekb=1&fs=0&enablejsapi=1&origin='+encodeURIComponent(location.origin);" + "function taille(){var w=innerWidth,h=innerHeight,W=Math.max(w,h*16/9),H=W*9/16;f.style.width=W+'px';f.style.height=H+'px';f.style.left=((w-W)/2)+'px';f.style.top=((h-H)/2)+'px';}" + "addEventListener('resize',taille);taille();" + "function cmd(c,a){try{f.contentWindow.postMessage(JSON.stringify({event:'command',func:c,args:(a===undefined?'':a)}),'*');}catch(e){}}" + "f.addEventListener('load',function(){try{f.contentWindow.postMessage(JSON.stringify({event:'listening',id:1}),'*');}catch(e){}});" + "setInterval(function(){cmd('setVolume',[" + VideoFond.Volume + "]);},700);" + "</script></body></html>";
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			try
			{
				wv.Dispose();
			}
			catch
			{
			}
		}
		base.Dispose(disposing);
	}
}
