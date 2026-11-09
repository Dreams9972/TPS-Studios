using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TPSStudios;

/// <summary>
/// Pseudos interdits (insultes, vulgarités, haine, violence) : mêmes règles que le serveur, pour
/// prévenir le joueur dès la saisie. Accents, chiffres « l33t », lettres répétées et séparées sont déjoués.
/// </summary>
internal static class ModerationPseudo
{
	private static readonly string[] Partout = new string[57]
	{
		"connard", "connasse", "salope", "encule", "enculer", "batard", "niquetamere", "niquetonpere", "niquer", "niqueta",
		"tagueule", "fermetagueule", "fils2pute", "filsdepute", "enfoire", "petasse", "pouffiasse", "branleur", "branlette", "trouduc",
		"couille", "bougnoul", "bicot", "youpin", "negro", "negre", "nazi", "hitler", "pedophil", "violeur",
		"terroris", "jihad", "daech", "fuck", "fucker", "motherfuck", "bitch", "whore", "nigger", "nigga",
		"faggot", "retarded", "porno", "pornhub", "penis", "vagin", "masturb", "sodomi", "zoophil", "trisomique",
		"mongolien", "tapette", "gouine", "suicid", "killyourself", "kys", "connar"
	};

	private static readonly HashSet<string> Seuls = new HashSet<string>
	{
		"con", "cons", "pute", "putes", "pd", "pede", "fdp", "ntm", "tg", "bite",
		"chatte", "cul", "merde", "foutre", "viol", "nique", "sex", "sexe", "shit", "slut",
		"cunt", "rape", "pedo", "abruti", "debile", "mongol", "isis", "kkk", "ss"
	};

	private static string Norm(string p)
	{
		string s = (p ?? "").Normalize(NormalizationForm.FormD);
		StringBuilder sb = new StringBuilder();
		foreach (char c in s)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
			{
				sb.Append(char.ToLowerInvariant(c));
			}
		}
		return sb.ToString().Replace('0', 'o').Replace('@', 'a').Replace('1', 'i').Replace('!', 'i').Replace('|', 'i').Replace('3', 'e').Replace('4', 'a').Replace('5', 's').Replace('$', 's').Replace('7', 't').Replace('8', 'b');
	}

	private static string SansRepetitions(string s)
	{
		return Regex.Replace(s, "(.)\\1+", "$1");
	}

	public static bool Interdit(string pseudo)
	{
		string n = Norm(pseudo);
		string compact = SansRepetitions(Regex.Replace(n, "[^a-z]", ""));
		foreach (string m in Partout)
		{
			if (compact.Contains(SansRepetitions(m)))
			{
				return true;
			}
		}
		List<string> mots = new List<string>();
		foreach (string m2 in Regex.Split(n, "[^a-z]+"))
		{
			if (m2.Length > 0)
			{
				mots.Add(SansRepetitions(m2));
			}
		}
		if (mots.Count > 1 && mots.TrueForAll((string m3) => m3.Length == 1) && Seuls.Contains(compact))
		{
			return true;
		}
		foreach (string m4 in mots)
		{
			if (Seuls.Contains(m4))
			{
				return true;
			}
		}
		return false;
	}
}
