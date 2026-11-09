using System;

namespace TPSStudios;

internal static class Themes
{
	public static readonly string[] Ids = new string[10] { "auto", "aucun", "nouvelan", "valentin", "carnaval", "paques", "ete", "halloween", "blackfriday", "noel" };

	public static readonly string[] Noms = new string[10] { "Automatique", "Aucun", "Nouvel An", "Saint-Valentin", "Carnaval", "Pâques", "Été", "Halloween", "Black Friday", "Noël" };

	public static bool CoupeAdmin;

	private static DateTime Paques(int y)
	{
		int num = y % 19;
		int num2 = y / 100;
		int num3 = y % 100;
		int num4 = num2 / 4;
		int num5 = num2 % 4;
		int num6 = (num2 + 8) / 25;
		int num7 = (num2 - num6 + 1) / 3;
		int num8 = (Zx.P(19) * num + num2 - num4 - num7 + Zx.P(15)) % 30;
		int num9 = num3 / 4;
		int num10 = num3 % 4;
		int num11 = (32 + 2 * num5 + 2 * num9 - num8 - num10) % 7;
		int num12 = (num + 11 * num8 + 22 * num11) / 451;
		int month = (num8 + num11 - 7 * num12 + 114) / 31;
		int day = (num8 + num11 - 7 * num12 + 114) % 31 + 1;
		return new DateTime(y, month, day);
	}

	public static string Calendrier(DateTime d)
	{
		d = d.Date;
		int year = d.Year;
		DateTime dateTime = new DateTime(year, 11, 1);
		DateTime dateTime2 = dateTime.AddDays((int)(4 - dateTime.DayOfWeek + 7) % 7 + 21).AddDays(1.0);
		if (d >= dateTime2.AddDays(-6.0) && d <= new DateTime(year, 12, 1))
		{
			return "blackfriday";
		}
		if (d >= new DateTime(year, 11, 25) && d.Month >= 11)
		{
			return "noel";
		}
		if (d.Month == 1 && d.Day <= 10)
		{
			return "nouvelan";
		}
		if (d.Month == 2 && d.Day <= 15)
		{
			return "valentin";
		}
		DateTime dateTime3 = Paques(year);
		DateTime dateTime4 = dateTime3.AddDays(-47.0);
		if (d >= dateTime4.AddDays(-7.0) && d <= dateTime4.AddDays(1.0))
		{
			return "carnaval";
		}
		if (d >= dateTime3.AddDays(-14.0) && d <= dateTime3.AddDays(7.0))
		{
			return "paques";
		}
		if (d.Month == 7 && d.Day <= 20)
		{
			return "ete";
		}
		if ((d.Month == 10 && d.Day >= 10) || (d.Month == 11 && d.Day == 1))
		{
			return "halloween";
		}
		return null;
	}

	public static string CalendrierOuProchain(DateTime d)
	{
		string text = Calendrier(d);
		if (text != null)
		{
			return text;
		}
		for (int i = 1; i <= 120; i++)
		{
			string text2 = Calendrier(d.AddDays(i));
			if (text2 != null)
			{
				return text2;
			}
		}
		return null;
	}

	public static string Courant()
	{
		if (CoupeAdmin)
		{
			return null;
		}
		string text = Environment.GetEnvironmentVariable("TPS_THEME");
		if (string.IsNullOrEmpty(text))
		{
			text = Config.Lire("theme") ?? "auto";
		}
		if (text == "aucun")
		{
			return null;
		}
		if (text != "auto" && Array.IndexOf(Ids, text) >= 2)
		{
			return text;
		}
		return CalendrierOuProchain(DateTime.Now);
	}

	public static ThemeBase Creer(string id, int page)
	{
		if (id == null)
		{
			return null;
		}
		ThemeBase themeBase = ((id == "noel") ? ((ThemeBase)new ThemeNoel(page)) : ((ThemeBase)new ThemeFete(id, page)));
		themeBase.Id = id;
		return themeBase;
	}
}
