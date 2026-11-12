using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace TPSStudios;

/// <summary>
/// Prépare le paquet d'un mod à partir d'un dossier : zips de ~250 Mo au format
/// attendu par Omsi.Installer (jeu/, plugins/, fonts/). Chaque partie est remise
/// à l'appelant dès qu'elle est prête (pour l'envoyer puis l'effacer), si bien
/// qu'il n'y a jamais plus d'une partie sur le disque.
/// </summary>
internal static class Paquet
{
	public const long TAILLE_PARTIE = 250L * 1024 * 1024;

	public const int MAX_PARTIES = 80;

	private const long MAX_FICHIER = 1800L * 1024 * 1024;

	public class Fichier
	{
		public string Chemin;

		public string Entree;

		public long Taille;
	}

	/// <summary>
	/// Liste les fichiers du dossier. Si le dossier contient un sous-dossier « jeu »,
	/// on garde la structure jeu/ plugins/ fonts/ telle quelle ; sinon tout le dossier
	/// est considéré comme le contenu du dossier d'OMSI 2 (préfixe jeu/).
	/// </summary>
	public static List<Fichier> Lister(string dossier, string[] ignorer, out string structure, out List<string> ignores)
	{
		dossier = Path.GetFullPath(dossier).TrimEnd('\\', '/');
		List<Fichier> list = new List<Fichier>();
		ignores = new List<string>();
		HashSet<string> exclus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string x in ignorer)
		{
			if (!string.IsNullOrEmpty(x))
			{
				exclus.Add(x.Replace('\\', '/'));
			}
		}
		bool avecJeu = Directory.Exists(Path.Combine(dossier, "jeu"));
		structure = (avecJeu ? "jeu/ + plugins/ + fonts/" : "contenu du dossier d'OMSI 2");
		foreach (string f in Directory.GetFiles(dossier, "*", SearchOption.AllDirectories))
		{
			string rel = f.Substring(dossier.Length + 1).Replace('\\', '/');
			string nom = Path.GetFileName(f).ToLowerInvariant();
			if (nom == "thumbs.db" || nom == "desktop.ini" || nom == ".ds_store")
			{
				continue;
			}
			string entree;
			if (avecJeu)
			{
				string tete = rel.Split('/')[0].ToLowerInvariant();
				if (rel.IndexOf('/') < 0 || (tete != "jeu" && tete != "plugins" && tete != "fonts"))
				{
					ignores.Add(rel);
					continue;
				}
				entree = tete + rel.Substring(tete.Length);
			}
			else
			{
				entree = "jeu/" + rel;
			}
			if (entree.StartsWith("jeu/") && exclus.Contains(entree.Substring(4)))
			{
				continue;
			}
			long taille = new FileInfo(f).Length;
			if (taille > MAX_FICHIER)
			{
				if (rel.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || rel.EndsWith(".rar", StringComparison.OrdinalIgnoreCase) || rel.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception("« " + rel + " » est une archive : décompressez-la, puis choisissez le dossier décompressé (celui qui contient maps\\, Vehicles\\…), sans l'archive dedans");
				}
				throw new Exception("le fichier « " + rel + " » est trop gros (" + taille / 1048576 + " Mo, 1800 Mo maximum par fichier)");
			}
			list.Add(new Fichier
			{
				Chemin = f,
				Entree = entree,
				Taille = taille
			});
		}
		list.Sort((a, b) => string.CompareOrdinal(a.Entree, b.Entree));
		return list;
	}

	/// <summary>
	/// Construit les parties dans « tmp ». Pour chaque partie terminée, appelle
	/// partiePrete(chemin, numéro) puis efface le fichier.
	/// </summary>
	public static void Construire(List<Fichier> fichiers, string entreeVersion, string contenuVersion, string tmp, string baseNom, bool unSeul, Action<string, int> partiePrete, Action<long> progres, Func<bool> annule)
	{
		Directory.CreateDirectory(tmp);
		int numero = 0;
		FileStream fs = null;
		ZipArchive zip = null;
		string chemin = null;
		long fait = 0L;
		Action ouvrir = delegate
		{
			numero++;
			if (numero > MAX_PARTIES)
			{
				throw new Exception("mod trop gros : plus de " + MAX_PARTIES + " parties de 250 Mo");
			}
			chemin = Path.Combine(tmp, unSeul ? (baseNom + ".zip") : (baseNom + "_partie" + numero + ".zip"));
			fs = new FileStream(chemin, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: true);
		};
		Action fermer = delegate
		{
			zip.Dispose();
			fs.Dispose();
			zip = null;
			fs = null;
			try
			{
				partiePrete(chemin, numero);
			}
			finally
			{
				try
				{
					File.Delete(chemin);
				}
				catch
				{
				}
			}
		};
		try
		{
			ouvrir();
			ZipArchiveEntry ev = zip.CreateEntry(entreeVersion, CompressionLevel.Optimal);
			using (Stream s = ev.Open())
			{
				byte[] b = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(contenuVersion);
				s.Write(b, 0, b.Length);
			}
			byte[] buf = new byte[1048576];
			foreach (Fichier f in fichiers)
			{
				if (annule())
				{
					throw new OperationCanceledException();
				}
				if (!unSeul && fs.Length >= TAILLE_PARTIE)
				{
					fermer();
					ouvrir();
				}
				ZipArchiveEntry e = zip.CreateEntry(f.Entree, CompressionLevel.Optimal);
				e.LastWriteTime = File.GetLastWriteTime(f.Chemin);
				using (Stream dst = e.Open())
				{
					using FileStream src = File.OpenRead(f.Chemin);
					int n;
					while ((n = src.Read(buf, 0, buf.Length)) > 0)
					{
						dst.Write(buf, 0, n);
						fait += n;
						progres(fait);
					}
				}
				fs.Flush();
			}
			fermer();
		}
		finally
		{
			if (zip != null)
			{
				try
				{
					zip.Dispose();
				}
				catch
				{
				}
			}
			if (fs != null)
			{
				try
				{
					fs.Dispose();
				}
				catch
				{
				}
				try
				{
					File.Delete(chemin);
				}
				catch
				{
				}
			}
		}
	}

	/// <summary>Nom de tag GitHub sûr : « id-version » (ex. parisview-v1.2, parisview-beta-v1.2).</summary>
	public static string Tag(string id, string version)
	{
		string v = version.Trim().Normalize(NormalizationForm.FormD);
		StringBuilder sb = new StringBuilder();
		foreach (char c in v.ToLowerInvariant())
		{
			if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark)
			{
				continue;
			}
			bool ok = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '.';
			if (ok)
			{
				sb.Append(c);
			}
			else if (sb.Length > 0 && sb[sb.Length - 1] != '-')
			{
				sb.Append('-');
			}
		}
		string slug = sb.ToString().Trim('-', '.');
		if (slug.Length == 0)
		{
			slug = "sans-numero";
		}
		if (char.IsDigit(slug[0]))
		{
			slug = "v" + slug;
		}
		return NomFichier(id).ToLowerInvariant() + "-" + slug;
	}

	public static string NomFichier(string id)
	{
		StringBuilder sb = new StringBuilder();
		foreach (char c in id)
		{
			sb.Append((char.IsLetterOrDigit(c) && c < '\u0080') || c == '-' || c == '_' ? c : '-');
		}
		return sb.ToString();
	}
}
