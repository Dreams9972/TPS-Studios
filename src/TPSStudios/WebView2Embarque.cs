using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TPSStudios;

/// <summary>
/// WebView2 (moteur d'Edge) est embarqué dans l'exe : les DLL .NET sont chargées depuis
/// la mémoire, et WebView2Loader.dll est déposé dans le dossier du lanceur au premier usage.
/// Le lanceur reste un seul fichier.
/// </summary>
internal static class WebView2Embarque
{
	private static bool init;

	/// <summary>À appeler au tout début du programme.</summary>
	public static void Init()
	{
		if (init)
		{
			return;
		}
		init = true;
		AppDomain.CurrentDomain.AssemblyResolve += delegate(object s, ResolveEventArgs e)
		{
			string name = new AssemblyName(e.Name).Name;
			if (name != "Microsoft.Web.WebView2.Core" && name != "Microsoft.Web.WebView2.WinForms")
			{
				return null;
			}
			try
			{
				using Stream stream = typeof(WebView2Embarque).Assembly.GetManifestResourceStream("wv2/" + name + ".dll");
				if (stream == null)
				{
					return null;
				}
				using MemoryStream memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				return Assembly.Load(memoryStream.ToArray());
			}
			catch
			{
				return null;
			}
		};
	}

	/// <summary>WebView2 n'existe que sous Windows (pas sous Mono / Linux).</summary>
	public static bool Possible
	{
		get
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				return Type.GetType("Mono.Runtime") == null;
			}
			return false;
		}
	}

	/// <summary>Dépose WebView2Loader.dll (bonne architecture) et renvoie son dossier.</summary>
	public static string DossierLoader()
	{
		string text = (Environment.Is64BitProcess ? ((RuntimeInformation.ProcessArchitecture == Architecture.Arm64) ? "arm64" : "x64") : "x86");
		string text2 = Path.Combine(Config.Dossier, "webview2", text);
		string path = Path.Combine(text2, "WebView2Loader.dll");
		using (Stream stream = typeof(WebView2Embarque).Assembly.GetManifestResourceStream("wv2/" + text + "/WebView2Loader.dll"))
		{
			if (stream == null)
			{
				return null;
			}
			if (!File.Exists(path) || new FileInfo(path).Length != stream.Length)
			{
				Directory.CreateDirectory(text2);
				using FileStream destination = File.Create(path);
				stream.CopyTo(destination);
			}
		}
		return text2;
	}
}
