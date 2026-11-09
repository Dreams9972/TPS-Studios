using System;

namespace TPSStudios;

internal static class Categories
{
	public static readonly string[] Ids = new string[6] { "cartes", "livrees", "bus", "girouettes", "packs_ia", "autre" };

	public static readonly string[] Noms = new string[6] { "Cartes", "Livrées", "Bus", "Girouettes", "Packs IA", "Autre" };

	public static string Valide(string id)
	{
		id = (id ?? "").Trim().ToLowerInvariant();
		if (Array.IndexOf(Ids, id) < 0)
		{
			if (id.Length != 0)
			{
				return "autre";
			}
			return "cartes";
		}
		return id;
	}

	public static string Nom(string id)
	{
		int num = Array.IndexOf(Ids, id);
		if (num < 0)
		{
			return "Autre";
		}
		return Noms[num];
	}
}
