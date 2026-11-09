using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TPSStudios;

internal static class Journal
{
	private static readonly Dictionary<string, DateTime> derniers = new Dictionary<string, DateTime>();

	public static void Une(string contexte, Exception e)
	{
		lock (derniers)
		{
			if (derniers.TryGetValue(contexte, out var value) && (DateTime.Now - value).TotalSeconds < 60.0)
			{
				return;
			}
			derniers[contexte] = DateTime.Now;
		}
		Ecrire(contexte, e);
	}

	public static void Ecrire(string contexte, Exception e)
	{
		try
		{
			string text = Path.Combine(Config.Dossier, "erreurs.log");
			if (File.Exists(text) && new FileInfo(text).Length > 524288)
			{
				try
				{
					File.Delete(text + ".old");
				}
				catch
				{
				}
				File.Move(text, text + ".old");
			}
			File.AppendAllText(text, string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "  v2.7.9  ", contexte, " : ", e, "\r\n\r\n"), Encoding.UTF8);
		}
		catch
		{
		}
	}
}
