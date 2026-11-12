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

	public static string[] LireVersionInstallee(string f)
	{
		return LireV(f);
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

	/// <summary>
	/// Le mod installé est-il plus ancien que celui du catalogue ? L'empreinte du paquet fait foi quand
	/// elle est connue ; sinon on compare les numéros de version sans tenir compte du libellé
	/// (« Béta V1.1.1 » = « v1.1.1 »), pour ne pas redemander une mise à jour déjà faite.
	/// </summary>
	public static bool Perimee(Projet p, string[] v)
	{
		if (v == null)
		{
			return false;
		}
		if (p.Principal == null && p.Variantes.Count > 0)
		{
			Projet q = VarianteDe(p, v);
			if (q != p)
			{
				return Perimee(q, v);
			}
		}
		string md5Inst = ((v.Length > 2) ? v[2].Trim() : "");
		if (p.Md5.Length > 0 && md5Inst.Length > 0)
		{
			return !string.Equals(md5Inst, p.Md5.Trim(), StringComparison.OrdinalIgnoreCase);
		}
		string vInst = v[0].Trim();
		if (vInst.Length == 0)
		{
			return true;
		}
		if (p.Version.Length == 0)
		{
			return false;
		}
		string a = NumVersion(vInst);
		string b = NumVersion(p.Version);
		if (a.Length > 0 && b.Length > 0)
		{
			return a != b;
		}
		return !string.Equals(vInst, p.Version.Trim(), StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>« Béta V1.1.1 » → « 1.1.1 » ; « V2.10.0 » → « 2.10 ».</summary>
	public static string NumVersion(string v)
	{
		System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(v ?? "", "\\d+(\\.\\d+)*");
		if (!m.Success)
		{
			return "";
		}
		string[] parts = m.Value.Split('.');
		for (int i = 0; i < parts.Length; i++)
		{
			parts[i] = parts[i].TrimStart('0');
			if (parts[i].Length == 0)
			{
				parts[i] = "0";
			}
		}
		int n = parts.Length;
		while (n > 1 && parts[n - 1] == "0")
		{
			n--;
		}
		return string.Join(".", parts, 0, n);
	}

	/// <summary>Quelle version de la page est installée (la normale ou une version spéciale) ?</summary>
	public static Projet VarianteDe(Projet p, string[] v)
	{
		Projet principal = p.Principal ?? p;
		if (v == null || principal.Variantes.Count == 0)
		{
			return principal;
		}
		for (int i = 3; i < v.Length; i++)
		{
			string l = v[i].Trim();
			if (l.StartsWith("variante:"))
			{
				string id = l.Substring(9).Trim();
				Projet q = principal.Variantes.Find((Projet x) => x.VarianteId == id);
				return q ?? principal;
			}
		}
		string md5 = ((v.Length > 2) ? v[2].Trim() : "");
		if (md5.Length > 0)
		{
			Projet q2 = principal.Variantes.Find((Projet x) => string.Equals(x.Md5, md5, StringComparison.OrdinalIgnoreCase));
			if (q2 != null)
			{
				return q2;
			}
		}
		return principal;
	}

	private static string DossierVersion(Projet p, string omsi, string varianteId)
	{
		return Path.Combine(RacineOff(omsi), (p.Principal ?? p).Id + "@" + ((varianteId.Length > 0) ? varianteId : "normale"));
	}

	/// <summary>Cette version a-t-elle été rangée de côté (remise en place instantanée, sans téléchargement) ?</summary>
	public static bool VersionRangee(Projet p, string omsi, string varianteId)
	{
		return omsi != null && File.Exists(Path.Combine(DossierVersion(p, omsi, varianteId), "fichiers.txt"));
	}

	/// <summary>
	/// Range la version installée hors du jeu (même disque, déplacement instantané) pour laisser la place
	/// à une autre version ; elle pourra être remise en place sans rien retélécharger.
	/// </summary>
	public static void RangerVersion(Projet p, string omsi, string varianteId, HashSet<string> seulement = null)
	{
		Projet principal = p.Principal ?? p;
		string dossier = DossierVersion(principal, omsi, varianteId);
		string liste = Path.Combine(omsi, principal.Liste);
		string fVersion = Path.Combine(omsi, principal.FichierVersion);
		if (!File.Exists(liste))
		{
			throw new Exception("liste des fichiers installés introuvable : réinstallez ce mod");
		}
		string open = DetecteOpen(omsi);
		string racine = Path.GetFullPath(omsi).TrimEnd('\\');
		Directory.CreateDirectory(dossier);
		List<string> paires = new List<string>();
		try
		{
			IEnumerable<string> lignes = ((seulement != null) ? ((IEnumerable<string>)seulement) : ((IEnumerable<string>)File.ReadAllLines(liste)));
			foreach (string ligne in lignes)
			{
				string f = ligne.Trim();
				if (f.Length == 0 || !File.Exists(f))
				{
					continue;
				}
				string plein = Path.GetFullPath(f);
				if (string.Equals(plein, Path.GetFullPath(fVersion), StringComparison.OrdinalIgnoreCase) || string.Equals(plein, Path.GetFullPath(liste), StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				string rel = (Sous(plein, omsi) ? ("jeu\\" + plein.Substring(racine.Length + 1)) : ((open != null && Sous(plein, open)) ? ("openOMSI\\" + plein.Substring(Path.GetFullPath(open).TrimEnd('\\').Length + 1)) : null));
				if (rel == null)
				{
					continue;
				}
				string cible = Path.Combine(dossier, rel);
				Directory.CreateDirectory(Path.GetDirectoryName(cible));
				if (File.Exists(cible))
				{
					File.Delete(cible);
				}
				File.Move(plein, cible);
				paires.Add(plein + "|" + cible);
			}
		}
		finally
		{
			File.WriteAllLines(Path.Combine(dossier, "fichiers.txt"), paires.ToArray(), Encoding.UTF8);
			string fTouches = Path.Combine(dossier, "touches.txt");
			if (seulement != null)
			{
				File.WriteAllLines(fTouches, new List<string>(seulement).ToArray(), Encoding.UTF8);
			}
			else if (File.Exists(fTouches))
			{
				File.Delete(fTouches);
			}
			File.Copy(liste, Path.Combine(dossier, "liste.txt"), overwrite: true);
			if (File.Exists(fVersion))
			{
				File.Copy(fVersion, Path.Combine(dossier, "version.txt"), overwrite: true);
				File.Delete(fVersion);
			}
			File.Delete(liste);
		}
	}

	/// <summary>
	/// Fichiers concernés par un complément (version spéciale posée par-dessus la normale), lus dans la
	/// version rangée : passer de l'une à l'autre ne déplace que ces fichiers. null si la version a été rangée entière.
	/// </summary>
	public static HashSet<string> Touches(Projet p, string omsi, string varianteId)
	{
		if (omsi == null)
		{
			return null;
		}
		string f = Path.Combine(DossierVersion(p.Principal ?? p, omsi, varianteId), "touches.txt");
		if (!File.Exists(f) || !File.Exists(Path.Combine(DossierVersion(p.Principal ?? p, omsi, varianteId), "fichiers.txt")))
		{
			return null;
		}
		HashSet<string> t = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string l in File.ReadAllLines(f))
		{
			if (l.Trim().Length > 0)
			{
				t.Add(l.Trim());
			}
		}
		return t;
	}

	/// <summary>Où s'installe une entrée de paquet (jeu/…, plugins/…, fonts/…) ; null si elle est ignorée.</summary>
	public static string Cible(string entree, string omsi, string open)
	{
		string e = entree.Replace('/', '\\');
		string c = null;
		if (e.StartsWith("jeu\\"))
		{
			c = Path.Combine(omsi, e.Substring(4));
			return Sous(c, omsi) ? Path.GetFullPath(c) : null;
		}
		if (open != null && e.StartsWith("plugins\\"))
		{
			c = Path.Combine(open, "plugins", e.Substring(8));
		}
		else if (open != null && e.StartsWith("fonts\\"))
		{
			c = Path.Combine(open, "Fonts", e.Substring(6));
		}
		return (c != null && Sous(c, open)) ? Path.GetFullPath(c) : null;
	}

	/// <summary>
	/// Installe une version spéciale « complément » par-dessus la version normale : les fichiers de la normale
	/// qu'il remplace sont rangés de côté (retour instantané), le reste de la normale ne bouge pas.
	/// </summary>
	public static void InstallerComplement(Projet p, string[] zips, string omsi, string open, Progres pr)
	{
		Projet principal = p.Principal ?? p;
		if (Desactive(principal, omsi))
		{
			Basculer(principal, omsi, activer: true);
		}
		string liste = Path.Combine(omsi, principal.Liste);
		if (!File.Exists(liste))
		{
			throw new Exception("installez d'abord la version normale de " + principal.Nom);
		}
		List<string> avant = new List<string>();
		foreach (string l in File.ReadAllLines(liste))
		{
			if (l.Trim().Length > 0)
			{
				avant.Add(l.Trim());
			}
		}
		Projet installee = VarianteDe(principal, Installee(principal, omsi));
		if (installee.VarianteId == p.VarianteId)
		{
			// mise à jour du complément déjà en place : on pose les nouveaux fichiers sans rien retirer
			File.Delete(liste);
			try
			{
				Installer(p, zips, omsi, open, pr);
			}
			catch
			{
				File.WriteAllLines(liste, avant.ToArray(), Encoding.UTF8);
				throw;
			}
		}
		else
		{
			if (installee.VarianteId.Length > 0)
			{
				throw new Exception("revenez d'abord à la version normale");
			}
			HashSet<string> t = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string z in zips)
			{
				using ZipArchive a = ZipFile.OpenRead(z);
				foreach (ZipArchiveEntry en in a.Entries)
				{
					if (!en.FullName.EndsWith("/"))
					{
						string c = Cible(en.FullName, omsi, open);
						if (c != null)
						{
							t.Add(c);
						}
					}
				}
			}
			RangerVersion(principal, omsi, "", t);
			try
			{
				Installer(p, zips, omsi, open, pr);
			}
			catch
			{
				// échec en cours de route : on retire ce qui a été posé et on remet la version normale telle quelle
				try
				{
					foreach (string c2 in t)
					{
						if (File.Exists(c2))
						{
							File.SetAttributes(c2, FileAttributes.Normal);
							File.Delete(c2);
						}
					}
					string fv = Path.Combine(omsi, principal.FichierVersion);
					if (File.Exists(fv))
					{
						File.Delete(fv);
					}
					RestaurerVersion(principal, omsi, "");
				}
				catch (Exception e2)
				{
					Journal.Une("complement", e2);
				}
				throw;
			}
		}
		HashSet<string> vu = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<string> tout = new List<string>();
		foreach (string l2 in avant)
		{
			if (vu.Add(l2))
			{
				tout.Add(l2);
			}
		}
		foreach (string l3 in File.ReadAllLines(liste))
		{
			if (l3.Trim().Length > 0 && vu.Add(l3.Trim()))
			{
				tout.Add(l3.Trim());
			}
		}
		File.WriteAllLines(liste, tout.ToArray(), Encoding.UTF8);
	}

	/// <summary>Supprime les versions rangées de côté d'une page (désinstallation complète).</summary>
	public static void OublierVersionsRangees(Projet p, string omsi)
	{
		if (omsi == null)
		{
			return;
		}
		foreach (Projet q in (p.Principal ?? p).Versions())
		{
			string d = DossierVersion(p, omsi, q.VarianteId);
			if (Directory.Exists(d))
			{
				try
				{
					FileSystem.DeleteDirectory(d, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
				}
				catch (Exception e)
				{
					Journal.Une("version-rangee", e);
				}
			}
		}
	}

	/// <summary>Remet en place une version rangée par RangerVersion.</summary>
	public static void RestaurerVersion(Projet p, string omsi, string varianteId)
	{
		Projet principal = p.Principal ?? p;
		string dossier = DossierVersion(principal, omsi, varianteId);
		string fp = Path.Combine(dossier, "fichiers.txt");
		if (!File.Exists(fp))
		{
			throw new Exception("cette version n'a pas été rangée sur ce PC");
		}
		foreach (string ligne in File.ReadAllLines(fp))
		{
			int k = ligne.IndexOf('|');
			if (k <= 0)
			{
				continue;
			}
			string orig = ligne.Substring(0, k);
			string cible = ligne.Substring(k + 1);
			if (!File.Exists(cible))
			{
				continue;
			}
			Directory.CreateDirectory(Path.GetDirectoryName(orig));
			if (File.Exists(orig))
			{
				File.SetAttributes(orig, FileAttributes.Normal);
				File.Delete(orig);
			}
			File.Move(cible, orig);
		}
		string l2 = Path.Combine(dossier, "liste.txt");
		if (File.Exists(l2))
		{
			File.Copy(l2, Path.Combine(omsi, principal.Liste), overwrite: true);
		}
		string v2 = Path.Combine(dossier, "version.txt");
		if (File.Exists(v2))
		{
			File.Copy(v2, Path.Combine(omsi, principal.FichierVersion), overwrite: true);
		}
		try
		{
			Directory.Delete(dossier, recursive: true);
		}
		catch (Exception e)
		{
			Journal.Une("version-rangee", e);
		}
	}

	/// <summary>Après une installation : le fichier de version reprend la version et l'empreinte du catalogue.</summary>
	private static void EcrireVersion(Projet p, string omsi, List<string> installes)
	{
		if (string.IsNullOrEmpty(p.FichierVersion))
		{
			return;
		}
		try
		{
			string f = Path.Combine(omsi, p.FichierVersion);
			string[] ancien = LireV(f);
			string version = ((p.Version.Length > 0) ? p.Version : ((ancien != null) ? ancien[0] : ""));
			string date = ((ancien != null && ancien[1].Trim().Length > 0) ? ancien[1].Trim() : DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
			string md5 = ((p.Md5.Length > 0) ? p.Md5 : ((ancien != null) ? ancien[2] : ""));
			Directory.CreateDirectory(Path.GetDirectoryName(f));
			File.WriteAllText(f, version.Trim() + "\r\n" + date + "\r\n" + md5.Trim() + "\r\nvariante:" + p.VarianteId + "\r\n", Encoding.UTF8);
			if (installes != null && !installes.Exists((string x) => string.Equals(x, f, StringComparison.OrdinalIgnoreCase)))
			{
				installes.Add(f);
			}
		}
		catch (Exception e)
		{
			Journal.Une("version", e);
		}
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

	// mods désactivés : rangés HORS du dossier du jeu, sur le même disque (déplacement instantané) : <disque>\TPS Studios\Mods désactivés\<id>
	// ancien emplacement (jusqu'à la 2.9.9) : <OMSI 2>\_TPS Studios - mods désactivés\<id>, encore lu tant qu'un mod y est rangé
	private const string AncienOff = "_TPS Studios - mods désactivés";

	public static string RacineOff(string omsi)
	{
		try
		{
			string text = Path.Combine(Path.GetPathRoot(Path.GetFullPath(omsi)), "TPS Studios", "Mods désactivés");
			Directory.CreateDirectory(text);
			return text;
		}
		catch
		{
			return Path.Combine(Config.Dossier, "Mods désactivés");
		}
	}

	public static string DossierOff(Projet p, string omsi)
	{
		string text = Path.Combine(omsi, AncienOff, p.Id);
		if (File.Exists(Path.Combine(text, "desactive.txt")))
		{
			return text;
		}
		return Path.Combine(RacineOff(omsi), p.Id);
	}

	// l'ancien dossier dans le jeu, une fois vide, part à la corbeille
	public static void RangerAncienOff(string omsi)
	{
		try
		{
			if (string.IsNullOrEmpty(omsi))
			{
				return;
			}
			string text = Path.Combine(omsi, AncienOff);
			if (Directory.Exists(text) && Directory.GetFiles(text, "*", System.IO.SearchOption.AllDirectories).Length == 0)
			{
				FileSystem.DeleteDirectory(text, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
			}
		}
		catch
		{
		}
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
		RangerAncienOff(omsi);
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
		EcrireVersion(p, omsi, list);
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

	private static bool lanceCache;

	private static DateTime lanceQuand = DateTime.MinValue;

	/// <summary>OMSI 2 / openOMSI tourne-t-il ? (résultat gardé 1,5 s : lire la liste des processus coûte cher)</summary>
	public static bool JeuLance()
	{
		if ((DateTime.Now - lanceQuand).TotalMilliseconds < 1500.0)
		{
			return lanceCache;
		}
		lanceCache = JeuLance0();
		lanceQuand = DateTime.Now;
		return lanceCache;
	}

	private static bool JeuLance0()
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
