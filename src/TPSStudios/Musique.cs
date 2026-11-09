using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TPSStudios;

internal static class Musique
{
	private delegate bool ProcFenetre(IntPtr h, IntPtr l);

	public const byte Precedent = 177;

	public const byte LectPause = 179;

	public const byte Suivant = 176;

	public const byte VolMoins = 174;

	public const byte VolPlus = 175;

	private static string cache = "";

	private static DateTime lu = DateTime.MinValue;

	public static string Lecteur = "";

	private static readonly string[] Navigateurs = new string[6] { "chrome", "msedge", "firefox", "opera", "brave", "vivaldi" };

	public static volatile string FenTitre = "";

	public static volatile string FenLecteur = "";

	public static string Mode
	{
		get
		{
			string text = Config.Lire("musique") ?? "";
			switch (text)
			{
			default:
				return "";
			case "spotify":
			case "apple":
			case "aucun":
				return text;
			}
		}
	}

	public static string NomMode
	{
		get
		{
			if (!(Mode == "apple"))
			{
				if (!(Mode == "spotify"))
				{
					return "Spotify ou Apple Music";
				}
				return "Spotify";
			}
			return "Apple Music";
		}
	}

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

	public static void Touche(byte vk)
	{
		try
		{
			keybd_event(vk, 0, 0u, UIntPtr.Zero);
			keybd_event(vk, 0, 2u, UIntPtr.Zero);
		}
		catch
		{
		}
	}

	public static bool Installe(string quoi)
	{
		try
		{
			if (quoi == "spotify")
			{
				return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spotify", "Spotify.exe")) || Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "SpotifyAB.SpotifyMusic_zpdnekdrzrea0")) || Process.GetProcessesByName("Spotify").Length > 0;
			}
			return Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages", "AppleInc.AppleMusicWin_nzyj5cx40ttqa")) || Process.GetProcessesByName("AppleMusic").Length > 0 || Process.GetProcessesByName("iTunes").Length > 0;
		}
		catch
		{
			return false;
		}
	}

	public static string EnCours()
	{
		if ((DateTime.Now - lu).TotalMilliseconds < 700.0)
		{
			return cache;
		}
		lu = DateTime.Now;
		string text = "";
		string text2 = "";
		string mode = Mode;
		if (mode == "aucun")
		{
			cache = "";
			Lecteur = "";
			return "";
		}
		MediaWin.Demarrer();
		if (MediaWin.Dispo && MediaWin.Source.Length > 0)
		{
			string text3 = MediaWin.Source.ToLowerInvariant();
			text2 = (text3.Contains("spotify") ? "Spotify" : ((text3.Contains("apple") || text3.Contains("itunes")) ? "Apple Music" : ((text3.Contains("chrome") || text3.Contains("msedge") || text3.Contains("firefox") || text3.Contains("opera") || text3.Contains("brave") || text3.Contains("vivaldi")) ? "Lecteur web" : "")));
			if (text2.Length > 0 && MediaWin.EnLecture && MediaWin.Titre.Length > 0)
			{
				text = ((MediaWin.Artiste.Length > 0) ? (MediaWin.Artiste + " - ") : "") + MediaWin.Titre;
			}
		}
		if (text.Length == 0 && text2.Length == 0)
		{
			string fenTitre = FenTitre;
			string fenLecteur = FenLecteur;
			if (fenLecteur.Length > 0 && (!(mode == "apple") || !(fenLecteur == "Spotify")) && (!(mode == "spotify") || !(fenLecteur != "Spotify")))
			{
				text2 = fenLecteur;
				text = fenTitre;
			}
		}
		cache = text;
		Lecteur = text2;
		return text;
	}

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr h);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr h, int n);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr h);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr h);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr h, StringBuilder s, int n);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(ProcFenetre f, IntPtr l);

	private static bool Devant(IntPtr h)
	{
		if (h == IntPtr.Zero)
		{
			return false;
		}
		ShowWindow(h, IsIconic(h) ? 9 : 5);
		SetForegroundWindow(h);
		return true;
	}

	private static IntPtr FenetreDe(string[] programmes, string[] mots)
	{
		HashSet<uint> pids = new HashSet<uint>();
		foreach (string processName in programmes)
		{
			try
			{
				Process[] processesByName = Process.GetProcessesByName(processName);
				foreach (Process process in processesByName)
				{
					pids.Add((uint)process.Id);
					process.Dispose();
				}
			}
			catch
			{
			}
		}
		if (pids.Count == 0)
		{
			return IntPtr.Zero;
		}
		IntPtr trouve = IntPtr.Zero;
		IntPtr premier = IntPtr.Zero;
		ProcFenetre procFenetre = delegate(IntPtr h, IntPtr l)
		{
			if (!IsWindowVisible(h))
			{
				return true;
			}
			GetWindowThreadProcessId(h, out var pid);
			if (!pids.Contains(pid))
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder(400);
			GetWindowText(h, stringBuilder, 400);
			string text = stringBuilder.ToString();
			if (text.Trim().Length == 0)
			{
				return true;
			}
			if (premier == IntPtr.Zero)
			{
				premier = h;
			}
			if (mots == null)
			{
				trouve = h;
				return false;
			}
			string[] array = mots;
			foreach (string value in array)
			{
				if (text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					trouve = h;
					return false;
				}
			}
			return true;
		};
		try
		{
			EnumWindows(procFenetre, IntPtr.Zero);
		}
		catch
		{
		}
		GC.KeepAlive(procFenetre);
		if (!(trouve != IntPtr.Zero))
		{
			if (mots != null)
			{
				return IntPtr.Zero;
			}
			return premier;
		}
		return trouve;
	}

	public static void AfficherLecteur()
	{
		string mode = Mode;
		lu = DateTime.MinValue;
		EnCours();
		string lecteur = Lecteur;
		object obj;
		switch (lecteur)
		{
		default:
			obj = ((mode == "apple") ? new string[2] { "AppleMusic", "iTunes" } : ((mode == "spotify") ? new string[1] { "Spotify" } : new string[3] { "Spotify", "AppleMusic", "iTunes" }));
			break;
		case "Spotify":
			obj = new string[1] { "Spotify" };
			break;
		case "Apple Music":
		case "iTunes":
			obj = new string[2] { "AppleMusic", "iTunes" };
			break;
		}
		string[] programmes = (string[])obj;
		string[] mots = ((mode == "apple") ? new string[1] { "Apple Music" } : ((mode == "spotify") ? new string[1] { "Spotify" } : new string[2] { "Spotify", "Apple Music" }));
		if ((!(lecteur != "Lecteur web") || !Devant(FenetreDe(programmes, null))) && !Devant(FenetreDe(Navigateurs, mots)) && (!(lecteur == "Lecteur web") || !Devant(FenetreDe(Navigateurs, null))) && (lecteur.Length != 0 || !Devant(FenetreDe(programmes, null))))
		{
			Ouvrir((mode == "apple") ? "apple" : "spotify");
		}
	}

	public static void Basculer()
	{
		Touche(179);
		if (cache.Length > 0)
		{
			cache = "";
			MediaWin.EnLecture = false;
		}
		else if (Lecteur.Length > 0)
		{
			MediaWin.EnLecture = true;
		}
		lu = DateTime.Now.AddMilliseconds(-300.0);
	}

	public static void ScanFenetres()
	{
		string text = "";
		string text2 = "";
		try
		{
			Process[] processesByName = Process.GetProcessesByName("Spotify");
			foreach (Process process in processesByName)
			{
				string text3 = process.MainWindowTitle ?? "";
				if (text3.Length > 0)
				{
					text2 = "Spotify";
					if (text3.Contains(" - "))
					{
						text = text3;
					}
				}
				process.Dispose();
			}
			if (text.Length == 0)
			{
				string[] array = new string[2] { "AppleMusic", "iTunes" };
				foreach (string text4 in array)
				{
					Process[] processesByName2 = Process.GetProcessesByName(text4);
					foreach (Process process2 in processesByName2)
					{
						string text5 = process2.MainWindowTitle ?? "";
						if (text5.Length > 0)
						{
							if (text2.Length == 0)
							{
								text2 = ((text4 == "iTunes") ? "iTunes" : "Apple Music");
							}
							if (text5.Contains(" - ") || text5.Contains(" — "))
							{
								text = text5;
								text2 = ((text4 == "iTunes") ? "iTunes" : "Apple Music");
							}
						}
						process2.Dispose();
					}
				}
			}
		}
		catch
		{
		}
		FenTitre = text;
		FenLecteur = text2;
	}

	public static string LienWeb(string quoi)
	{
		if (!(quoi == "apple"))
		{
			return "https://open.spotify.com";
		}
		return "https://music.apple.com";
	}

	public static void Ouvrir(string quoi)
	{
		try
		{
			if (!Installe(quoi))
			{
				Process.Start(LienWeb(quoi));
				return;
			}
			if (quoi == "spotify")
			{
				try
				{
					Process.Start("spotify:");
					return;
				}
				catch
				{
					Process.Start(LienWeb(quoi));
					return;
				}
			}
			try
			{
				Process.Start("explorer.exe", "shell:AppsFolder\\AppleInc.AppleMusicWin_nzyj5cx40ttqa!App");
			}
			catch
			{
				Process.Start(LienWeb(quoi));
			}
		}
		catch
		{
		}
	}

	public static List<string[]> Playlists()
	{
		List<string[]> list = new List<string[]>();
		string[] array = (Config.Lire("playlists") ?? "").Split(new char[1] { '§' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf('|');
			if (num > 0)
			{
				list.Add(new string[2]
				{
					text.Substring(0, num),
					text.Substring(num + 1)
				});
			}
		}
		return list;
	}

	public static void SauverPlaylists(List<string[]> l)
	{
		List<string> list = new List<string>();
		foreach (string[] item in l)
		{
			list.Add(item[0].Replace("§", " ").Replace("|", " ") + "|" + item[1]);
		}
		Config.Ecrire("playlists", string.Join("§", list.ToArray()));
	}

	public static bool LienValide(string u)
	{
		if (u.StartsWith("spotify:"))
		{
			return Regex.IsMatch(u, "^spotify:(playlist|album|artist|track|show):[A-Za-z0-9]{10,40}$");
		}
		if (Uri.TryCreate(u, UriKind.Absolute, out var result) && result.Scheme == Uri.UriSchemeHttps)
		{
			if (!(result.Host == "open.spotify.com") && !(result.Host == "music.apple.com"))
			{
				return result.Host == "embed.music.apple.com";
			}
			return true;
		}
		return false;
	}

	public static void OuvrirPlaylist(string lien)
	{
		try
		{
			Match match = Regex.Match(lien, "^https://open\\.spotify\\.com/(?:intl-[a-z-]+/)?(playlist|album|artist|track|show)/([A-Za-z0-9]+)");
			if (match.Success && Installe("spotify"))
			{
				Process.Start("spotify:" + match.Groups[1].Value + ":" + match.Groups[2].Value);
			}
			else if (LienValide(lien))
			{
				Process.Start(lien);
			}
		}
		catch
		{
		}
	}
}
