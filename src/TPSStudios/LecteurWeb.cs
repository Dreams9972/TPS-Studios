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

	/// <summary>Le joueur a cliqué sur pause / lecture dans le lecteur (vrai = en pause).</summary>
	public event Action<bool> PauseDemandee;

	/// <summary>Le joueur a cliqué sur le bouton son dans le lecteur (vrai = muet).</summary>
	public event Action<bool> MuetDemande;

	private string mp4;

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
					wv.CoreWebView2.ExecuteScriptAsync("etatSon(" + (muet ? "true" : "false") + ")");
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
			wv.CoreWebView2.WebMessageReceived += delegate(object o, CoreWebView2WebMessageReceivedEventArgs e)
			{
				string m = "";
				try
				{
					m = e.TryGetWebMessageAsString() ?? "";
				}
				catch
				{
				}
				if (m == "pause" || m == "lecture")
				{
					this.PauseDemandee?.Invoke(m == "pause");
				}
				else if (m == "muet" || m == "son")
				{
					muet = m == "muet";
					try
					{
						wv.CoreWebView2.IsMuted = muet;
					}
					catch
					{
					}
					this.MuetDemande?.Invoke(muet);
				}
				else if (m == "erreur")
				{
					echec = true;
					this.Echec?.Invoke();
				}
			};
			pret = true;
			if (id != null || mp4 != null)
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
		if (idYouTube == id && mp4 == null)
		{
			return;
		}
		id = idYouTube;
		mp4 = null;
		joue = true;
		if (pret)
		{
			Naviguer();
		}
	}

	/// <summary>Lance une vidéo MP4 (adresse https://…) dans le même lecteur.</summary>
	public void ChargerMp4(string url)
	{
		if (url == mp4)
		{
			return;
		}
		mp4 = url;
		id = null;
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
			wv.CoreWebView2.Navigate("https://tps.lanceur/lecteur.html?" + ((mp4 != null) ? ("u=" + Uri.EscapeDataString(mp4)) : ("v=" + Uri.EscapeDataString(id ?? ""))) + "&m=" + (muet ? "1" : "0"));
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
			wv.CoreWebView2.ExecuteScriptAsync("hote(" + (lire ? "true" : "false") + ")");
		}
		catch
		{
		}
	}

	private static string Page()
	{
		StringBuilder h = new StringBuilder();
		h.Append("<!doctype html><html><head><meta charset=\"utf-8\"><meta name=\"referrer\" content=\"strict-origin-when-cross-origin\">");
		h.Append("<style>html,body{margin:0;height:100%;background:#000;overflow:hidden;font-family:Segoe UI,sans-serif}");
		h.Append("iframe,video{position:absolute;border:0}iframe{pointer-events:none}video{left:0;top:0;width:100%;height:100%;object-fit:cover}");
		h.Append("#b{position:absolute;right:14px;bottom:14px;display:flex;gap:8px;z-index:5}");
		h.Append("#b button{width:42px;height:42px;border-radius:12px;border:1px solid rgba(255,255,255,.28);background:rgba(14,16,36,.38);");
		h.Append("backdrop-filter:blur(10px);-webkit-backdrop-filter:blur(10px);color:#fff;display:flex;align-items:center;justify-content:center;");
		h.Append("cursor:pointer;padding:0;outline:none;transition:background .18s,border-color .18s,transform .12s;box-shadow:0 4px 14px rgba(0,0,0,.25)}");
		h.Append("#b button:hover{background:rgba(40,46,96,.55);border-color:rgba(170,185,255,.7)}#b button:active{transform:scale(.92)}");
		h.Append("#b svg{width:18px;height:18px;fill:#fff}</style></head><body>");
		h.Append("<div id=\"b\"><button id=\"bp\" title=\"Pause\"></button><button id=\"bs\" title=\"Son\"></button></div><script>");
		h.Append("var q=new URLSearchParams(location.search),id=q.get('v')||'',u=q.get('u')||'',muet=q.get('m')=='1',pauseJ=false,cache=false,f=null,v=null;");
		h.Append("var IP='<svg viewBox=\"0 0 24 24\"><rect x=\"6\" y=\"5\" width=\"4\" height=\"14\" rx=\"1\"/><rect x=\"14\" y=\"5\" width=\"4\" height=\"14\" rx=\"1\"/></svg>';");
		h.Append("var IL='<svg viewBox=\"0 0 24 24\"><path d=\"M8 5.5v13a1 1 0 0 0 1.5.86l10.5-6.5a1 1 0 0 0 0-1.72L9.5 4.64A1 1 0 0 0 8 5.5z\"/></svg>';");
		h.Append("var IS='<svg viewBox=\"0 0 24 24\"><path d=\"M4 9h4l5-4v14l-5-4H4z\"/><path d=\"M16 8.5a5 5 0 0 1 0 7\" stroke=\"#fff\" stroke-width=\"2\" fill=\"none\" stroke-linecap=\"round\"/><path d=\"M18.5 6a8.5 8.5 0 0 1 0 12\" stroke=\"#fff\" stroke-width=\"2\" fill=\"none\" stroke-linecap=\"round\"/></svg>';");
		h.Append("var IM='<svg viewBox=\"0 0 24 24\"><path d=\"M4 9h4l5-4v14l-5-4H4z\"/><path d=\"M16 9l5 6M21 9l-5 6\" stroke=\"#fff\" stroke-width=\"2\" stroke-linecap=\"round\"/></svg>';");
		h.Append("var bp=document.getElementById('bp'),bs=document.getElementById('bs');");
		h.Append("function envoi(m){try{chrome.webview.postMessage(m)}catch(e){}}");
		h.Append("function ui(){bp.innerHTML=pauseJ?IL:IP;bp.title=pauseJ?'Lecture':'Pause';bs.innerHTML=muet?IM:IS;bs.title=muet?'Activer le son':'Couper le son';}");
		h.Append("function cmd(c,a){try{f.contentWindow.postMessage(JSON.stringify({event:'command',func:c,args:(a===undefined?'':a)}),'*');}catch(e){}}");
		h.Append("function lire(on){if(v){if(on){var p=v.play();if(p&&p.catch)p.catch(function(){});}else v.pause();}else if(f){cmd(on?'playVideo':'pauseVideo');}}");
		h.Append("function hote(on){cache=!on;lire(on&&!pauseJ);}");
		h.Append("function etatSon(m){muet=m;ui();}");
		h.Append("bp.onclick=function(){pauseJ=!pauseJ;lire(!pauseJ&&!cache);ui();envoi(pauseJ?'pause':'lecture');};");
		h.Append("bs.onclick=function(){muet=!muet;ui();envoi(muet?'muet':'son');};");
		h.Append("if(u){v=document.createElement('video');v.src=u;v.autoplay=true;v.loop=true;v.playsInline=true;v.volume=" + (VideoFond.Volume / 100.0).ToString(System.Globalization.CultureInfo.InvariantCulture) + ";");
		h.Append("v.onerror=function(){envoi('erreur')};document.body.insertBefore(v,document.body.firstChild);}");
		h.Append("else{f=document.createElement('iframe');f.allow='autoplay; encrypted-media; picture-in-picture';f.referrerPolicy='strict-origin-when-cross-origin';");
		h.Append("f.src='https://www.youtube-nocookie.com/embed/'+encodeURIComponent(id)+'?autoplay=1&mute=0&controls=0&loop=1&playlist='+encodeURIComponent(id)+'&modestbranding=1&rel=0&playsinline=1&iv_load_policy=3&disablekb=1&fs=0&enablejsapi=1&origin='+encodeURIComponent(location.origin);");
		h.Append("document.body.insertBefore(f,document.body.firstChild);");
		h.Append("f.addEventListener('load',function(){try{f.contentWindow.postMessage(JSON.stringify({event:'listening',id:1}),'*');}catch(e){}});");
		h.Append("function taille(){var w=innerWidth,hh=innerHeight,W=Math.max(w,hh*16/9)*1.24,H=W*9/16;f.style.width=W+'px';f.style.height=H+'px';f.style.left=((w-W)/2)+'px';f.style.top=((hh-H)/2)+'px';}");
		h.Append("addEventListener('resize',taille);taille();");
		h.Append("setInterval(function(){cmd('setVolume',[" + VideoFond.Volume + "]);},700);}");
		h.Append("ui();</script></body></html>");
		return h.ToString();
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
