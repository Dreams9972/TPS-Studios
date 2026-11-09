using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

namespace TPSStudios;

internal static class Omsi
{
	private static string omsiCache;

	private static DateTime omsiQuand;

	public static int Nettoyes;

	public static void Oublier()
	{
		omsiCache = null;
		omsiQuand = DateTime.MinValue;
	}

	public static string DetecteOmsi()
	{
		if ((DateTime.Now - omsiQuand).TotalSeconds < 30.0)
		{
			return omsiCache;
		}
		omsiCache = DetecteOmsi0();
		omsiQuand = DateTime.Now;
		return omsiCache;
	}

	private static string DetecteOmsi0()
	{
		string text = Config.Lire("omsi");
		if (!string.IsNullOrEmpty(text) && File.Exists(Path.Combine(text, "Omsi.exe")))
		{
			return text;
		}
		List<string> list = new List<string>();
		try
		{
			object value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);
			if (value != null)
			{
				string path = value.ToString().Replace('/', '\\');
				list.Add(Path.Combine(path, "steamapps\\common\\OMSI 2"));
				string path2 = Path.Combine(path, "steamapps\\libraryfolders.vdf");
				if (File.Exists(path2))
				{
					string[] array = File.ReadAllLines(path2);
					foreach (string text2 in array)
					{
						string text3 = text2.Trim();
						if (text3.StartsWith("\"path\""))
						{
							list.Add(Path.Combine(text3.Substring(6).Trim().Trim('"')
								.Replace("\\\\", "\\"), "steamapps\\common\\OMSI 2"));
						}
					}
				}
			}
		}
		catch
		{
		}
		list.Add("C:\\Program Files (x86)\\Steam\\steamapps\\common\\OMSI 2");
		foreach (string item in list)
		{
			if (File.Exists(Path.Combine(item, "Omsi.exe")))
			{
				return item;
			}
		}
		return null;
	}

	public static string DetecteOpen(string omsi)
	{
		string text = Config.Lire("openomsi");
		if (!string.IsNullOrEmpty(text) && File.Exists(Path.Combine(text, "openomsi.exe")))
		{
			return text;
		}
		try
		{
			if (omsi != null && Directory.Exists(omsi))
			{
				string[] directories = Directory.GetDirectories(omsi, "openOMSI*");
				foreach (string text2 in directories)
				{
					if (File.Exists(Path.Combine(text2, "openomsi.exe")))
					{
						return text2;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	private static string[] LireV(string f)
	{
		if (!File.Exists(f))
		{
			return null;
		}
		string[] array = File.ReadAllText(f, Encoding.UTF8).Replace("\r", "").Split('\n');
		if (array.Length < 3)
		{
			return null;
		}
		return array;
	}

	public static bool Perimee(Projet p, string[] v)
	{
		if (v == null)
		{
			return false;
		}
		if (v[0].Trim().Length == 0)
		{
			return true;
		}
		if (p.Md5.Length > 0 && v[2].Trim() != p.Md5)
		{
			return true;
		}
		if (p.Version.Length > 0)
		{
			return v[0].Trim() != p.Version;
		}
		return false;
	}

	public static string[] Installee(Projet p, string omsi)
	{
		if (omsi == null)
		{
			return null;
		}
		string[] array = LireV(Path.Combine(omsi, p.FichierVersion));
		if (array != null)
		{
			return array;
		}
		if (File.Exists(Path.Combine(omsi, p.Liste)) || (p.Marque.Length > 0 && Directory.Exists(Path.Combine(omsi, p.Marque))))
		{
			return new string[3] { "", "", "" };
		}
		return null;
	}

	public static bool Sous(string chemin, string racine)
	{
		if (string.IsNullOrEmpty(racine) || string.IsNullOrEmpty(chemin))
		{
			return false;
		}
		try
		{
			return Path.GetFullPath(chemin).StartsWith(Path.GetFullPath(racine).TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	private static bool Parasite(string f)
	{
		string text = Path.GetFileName(f).ToLowerInvariant();
		if (!(text == "thumbs.db") && !(text == "desktop.ini"))
		{
			return text == ".ds_store";
		}
		return true;
	}

	public static string DossierOff(Projet p, string omsi)
	{
		return Path.Combine(omsi, "_TPS Studios - mods désactivés", p.Id);
	}

	public static bool Desactive(Projet p, string omsi)
	{
		if (omsi != null && p != null)
		{
			return File.Exists(Path.Combine(DossierOff(p, omsi), "desactive.txt"));
		}
		return false;
	}

	public static int Basculer(Projet p, string omsi, bool activer)
	{
		string text = DossierOff(p, omsi);
		string path = Path.Combine(text, "desactive.txt");
		string text2 = DetecteOpen(omsi);
		int num = 0;
		if (!activer)
		{
			if (File.Exists(path))
			{
				return 0;
			}
			string path2 = Path.Combine(omsi, p.Liste);
			string fullPath = Path.GetFullPath(Path.Combine(omsi, p.FichierVersion));
			string text3 = Path.GetFullPath(omsi).TrimEnd('\\');
			if (!File.Exists(path2))
			{
				throw new Exception("liste des fichiers installés introuvable : réinstallez ce mod");
			}
			List<string> list = new List<string>();
			Directory.CreateDirectory(text);
			try
			{
				string[] array = File.ReadAllLines(path2);
				foreach (string text4 in array)
				{
					string text5 = text4.Trim();
					if (text5.Length == 0 || !File.Exists(text5))
					{
						continue;
					}
					string fullPath2 = Path.GetFullPath(text5);
					if (string.Equals(fullPath2, fullPath, StringComparison.OrdinalIgnoreCase) || string.Equals(fullPath2, Path.GetFullPath(path2), StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					string text6 = (Sous(fullPath2, omsi) ? fullPath2.Substring(text3.Length + 1) : ((text2 != null && Sous(fullPath2, text2)) ? ("openOMSI\\" + fullPath2.Substring(Path.GetFullPath(text2).TrimEnd('\\').Length + 1)) : null));
					if (text6 != null)
					{
						string text7 = Path.Combine(text, text6);
						if (!File.Exists(text7))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(text7));
							File.Move(fullPath2, text7);
							list.Add(fullPath2 + "|" + text7);
							num++;
						}
					}
				}
				return num;
			}
			finally
			{
				File.WriteAllLines(path, list.ToArray(), Encoding.UTF8);
			}
		}
		if (!File.Exists(path))
		{
			return 0;
		}
		List<string> list2 = new List<string>();
		string[] array2 = File.ReadAllLines(path);
		foreach (string text8 in array2)
		{
			int num2 = text8.IndexOf('|');
			if (num2 <= 0)
			{
				continue;
			}
			string text9 = text8.Substring(0, num2);
			string text10 = text8.Substring(num2 + 1);
			if (Sous(text10, text) && (Sous(text9, omsi) || (text2 != null && Sous(text9, text2))) && File.Exists(text10))
			{
				if (File.Exists(text9))
				{
					list2.Add(text8);
					continue;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(text9));
				File.Move(text10, text9);
				num++;
			}
		}
		if (list2.Count > 0)
		{
			File.WriteAllLines(path, list2.ToArray(), Encoding.UTF8);
			throw new Exception(list2.Count + " fichier(s) existent déjà à leur place d'origine : laissés de côté");
		}
		try
		{
			FileSystem.DeleteDirectory(text, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
		}
		catch
		{
		}
		return num;
	}

	public static void Installer(Projet p, string zip, string omsi, string open, Progres pr)
	{
		Installer(p, new string[1] { zip }, omsi, open, pr);
	}

	public static void Installer(Projet p, string[] zips, string omsi, string open, Progres pr)
	{
		if (Desactive(p, omsi))
		{
			Basculer(p, omsi, activer: true);
		}
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string path = Path.Combine(omsi, p.Liste);
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			foreach (string text in array)
			{
				if (text.Trim().Length > 0)
				{
					hashSet.Add(text.Trim());
				}
			}
		}
		long num = 0L;
		long num2 = 0L;
		foreach (string archiveFileName in zips)
		{
			using ZipArchive zipArchive = ZipFile.OpenRead(archiveFileName);
			if (zipArchive.Entries.Count > 300000)
			{
				throw new Exception("archive invalide (trop de fichiers)");
			}
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				num += entry.Length;
			}
		}
		if (num > 64424509440L)
		{
			throw new Exception("archive invalide (taille démesurée)");
		}
		foreach (string archiveFileName2 in zips)
		{
			using ZipArchive zipArchive2 = ZipFile.OpenRead(archiveFileName2);
			foreach (ZipArchiveEntry entry2 in zipArchive2.Entries)
			{
				if (entry2.FullName.EndsWith("/"))
				{
					continue;
				}
				string text2 = entry2.FullName.Replace('/', '\\');
				string text3 = null;
				if (text2.StartsWith("jeu\\"))
				{
					text3 = Path.Combine(omsi, text2.Substring(4));
				}
				else if (open != null && text2.StartsWith("plugins\\"))
				{
					text3 = Path.Combine(open, "plugins", text2.Substring(8));
				}
				else if (open != null && text2.StartsWith("fonts\\"))
				{
					text3 = Path.Combine(open, "Fonts", text2.Substring(6));
				}
				num2 += entry2.Length;
				if (text3 == null || !Sous(text3, text2.StartsWith("jeu\\") ? omsi : open))
				{
					continue;
				}
				switch (Path.GetFileName(text3).ToLowerInvariant())
				{
				case "thumbs.db":
				case "desktop.ini":
				case ".ds_store":
					continue;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(text3));
				if (File.Exists(text3) && (File.GetAttributes(text3) & (FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System)) != 0)
				{
					File.SetAttributes(text3, FileAttributes.Normal);
				}
				entry2.ExtractToFile(text3, overwrite: true);
				list.Add(text3);
				pr?.Invoke((num > 0) ? ((double)num2 / (double)num) : 1.0, text2);
			}
		}
		HashSet<string> hashSet2 = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
		Nettoyes = 0;
		foreach (string item in hashSet)
		{
			if (!hashSet2.Contains(item) && File.Exists(item) && (Sous(item, omsi) || Sous(item, open)))
			{
				Corbeille(item);
				Nettoyes++;
			}
		}
		File.WriteAllLines(path, list.ToArray(), Encoding.UTF8);
	}

	private static void Corbeille(string f)
	{
		FileSystem.DeleteFile(f, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
	}

	public static int Desinstaller(Projet p, string omsi)
	{
		if (Desactive(p, omsi))
		{
			Basculer(p, omsi, activer: true);
		}
		string text = Path.Combine(omsi, p.Liste);
		string racine = DetecteOpen(omsi);
		List<string> list = new List<string>();
		if (File.Exists(text))
		{
			string[] array = File.ReadAllLines(text);
			foreach (string text2 in array)
			{
				if (text2.Trim().Length > 0 && File.Exists(text2.Trim()) && (Sous(text2.Trim(), omsi) || Sous(text2.Trim(), racine)))
				{
					list.Add(text2.Trim());
				}
			}
		}
		HashSet<string> hashSet = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		string value = p.Id.ToLowerInvariant();
		foreach (string item in list)
		{
			string directoryName = Path.GetDirectoryName(item);
			while (directoryName != null && Path.GetDirectoryName(directoryName) != null && Path.GetFileName(directoryName).ToLowerInvariant().IndexOf(value) < 0)
			{
				directoryName = Path.GetDirectoryName(directoryName);
			}
			if (directoryName != null && Path.GetFileName(directoryName).ToLowerInvariant().IndexOf(value) >= 0)
			{
				dictionary[directoryName] = true;
			}
		}
		int num = 0;
		foreach (string key in dictionary.Keys)
		{
			if (!Directory.Exists(key))
			{
				continue;
			}
			string[] files = Directory.GetFiles(key, "*", System.IO.SearchOption.AllDirectories);
			bool flag = true;
			string[] array2 = files;
			foreach (string text3 in array2)
			{
				if (!hashSet.Contains(text3) && !Parasite(text3))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				FileSystem.DeleteDirectory(key, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
				num += files.Length;
			}
		}
		foreach (string item2 in list)
		{
			if (File.Exists(item2))
			{
				Corbeille(item2);
				num++;
			}
		}
		if (File.Exists(text))
		{
			Corbeille(text);
		}
		return num;
	}

	public static bool JeuLance()
	{
		bool result = false;
		string[] array = new string[2] { "openomsi", "omsi" };
		foreach (string processName in array)
		{
			Process[] processesByName;
			try
			{
				processesByName = Process.GetProcessesByName(processName);
			}
			catch
			{
				continue;
			}
			Process[] array2 = processesByName;
			foreach (Process process in array2)
			{
				result = true;
				try
				{
					process.Dispose();
				}
				catch
				{
				}
			}
		}
		return result;
	}
}
