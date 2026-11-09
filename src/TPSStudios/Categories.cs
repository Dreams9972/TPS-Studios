using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TPSStudios;

/// <summary>
/// Catégories de tri des mods, propres à chaque jeu. Elles se règlent dans le mode admin
/// (fenêtre Jeux › « Catégories de tri », une par ligne) et sont publiées dans le catalogue
/// (jeux_liste › categories). OMSI 2 garde ses catégories historiques si rien n'est réglé.
/// </summary>
internal static class Categories
{
	/// <summary>Catégories historiques d'OMSI 2.</summary>
	public static readonly string[] Ids = new string[6] { "cartes", "livrees", "bus", "girouettes", "packs_ia", "autre" };

	public static readonly string[] Noms = new string[6] { "Cartes", "Livrées", "Bus", "Girouettes", "Packs IA", "Autre" };

	/// <summary>Catégorie unique d'un jeu qui n'a rien de réglé (hors OMSI 2).</summary>
	private static readonly string[] IdsVide = new string[1] { "mods" };

	private static readonly string[] NomsVide = new string[1] { "Mods" };

	private static readonly Dictionary<string, string[][]> parJeu = new Dictionary<string, string[][]>();

	private static string Jeu(string jeu)
	{
		if (!string.IsNullOrEmpty(jeu))
		{
			return jeu;
		}
		return "omsi2";
	}

	/// <summary>Règle les catégories d'un jeu depuis le texte de l'admin (une par ligne, « id|Nom » accepté).</summary>
	public static void Definir(string jeu, string texte)
	{
		jeu = Jeu(jeu);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string[] array = (texte ?? "").Replace("\r", "").Split('\n');
		foreach (string text in array)
		{
			string text2 = text.Trim();
			if (text2.Length == 0)
			{
				continue;
			}
			string text3;
			string text4;
			int num = text2.IndexOf('|');
			if (num > 0)
			{
				text3 = Slug(text2.Substring(0, num));
				text4 = text2.Substring(num + 1).Trim();
			}
			else
			{
				text3 = Slug(text2);
				text4 = text2;
			}
			if (text3.Length > 0 && text4.Length > 0 && !list.Contains(text3) && list.Count < 12)
			{
				list.Add(text3);
				list2.Add(text4);
			}
		}
		lock (parJeu)
		{
			if (list.Count == 0)
			{
				parJeu.Remove(jeu);
			}
			else
			{
				parJeu[jeu] = new string[2][]
				{
					list.ToArray(),
					list2.ToArray()
				};
			}
		}
	}

	public static string[] IdsDe(string jeu)
	{
		jeu = Jeu(jeu);
		lock (parJeu)
		{
			if (parJeu.TryGetValue(jeu, out var value))
			{
				return value[0];
			}
		}
		if (!(jeu == "omsi2"))
		{
			return IdsVide;
		}
		return Ids;
	}

	public static string[] NomsDe(string jeu)
	{
		jeu = Jeu(jeu);
		lock (parJeu)
		{
			if (parJeu.TryGetValue(jeu, out var value))
			{
				return value[1];
			}
		}
		if (!(jeu == "omsi2"))
		{
			return NomsVide;
		}
		return Noms;
	}

	/// <summary>Texte à afficher dans l'admin pour un jeu (une catégorie par ligne).</summary>
	public static string Texte(string jeu)
	{
		string[] array = IdsDe(jeu);
		string[] array2 = NomsDe(jeu);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append((Slug(array2[i]) == array[i]) ? array2[i] : (array[i] + "|" + array2[i])).Append("\r\n");
		}
		return stringBuilder.ToString().TrimEnd();
	}

	public static string Valide(string id)
	{
		return Valide(id, "omsi2");
	}

	/// <summary>Catégorie valable pour ce jeu (vide : la première ; inconnue : « autre » pour OMSI 2, gardée telle quelle sinon).</summary>
	public static string Valide(string id, string jeu)
	{
		id = (id ?? "").Trim().ToLowerInvariant();
		string[] array = IdsDe(jeu);
		if (Array.IndexOf(array, id) >= 0)
		{
			return id;
		}
		if (id.Length == 0)
		{
			return array[0];
		}
		if (Jeu(jeu) == "omsi2" && Array.IndexOf(array, "autre") >= 0)
		{
			return "autre";
		}
		return id;
	}

	public static string Nom(string id)
	{
		int num = Array.IndexOf(Ids, id);
		if (num >= 0)
		{
			return Noms[num];
		}
		lock (parJeu)
		{
			foreach (string[][] value in parJeu.Values)
			{
				num = Array.IndexOf(value[0], id);
				if (num >= 0)
				{
					return value[1][num];
				}
			}
		}
		if (id == "mods")
		{
			return "Mods";
		}
		if (string.IsNullOrEmpty(id))
		{
			return "Autre";
		}
		string text = id.Replace('_', ' ').Replace('-', ' ');
		return char.ToUpperInvariant(text[0]) + text.Substring(1);
	}

	/// <summary>« Packs IA » → « packs_ia ».</summary>
	public static string Slug(string s)
	{
		string text = (s ?? "").Trim().Normalize(NormalizationForm.FormD).ToLowerInvariant();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in text)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
			{
				continue;
			}
			if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
			{
				stringBuilder.Append(c);
			}
			else if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '_')
			{
				stringBuilder.Append('_');
			}
		}
		return stringBuilder.ToString().Trim('_');
	}
}
