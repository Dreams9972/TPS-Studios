using System;
using System.IO;
using System.Reflection;

namespace TPSStudios;

internal static class Video
{
	public const int DureeMax = 180;

	public static string Verifier(string f, out double secondes, out int largeur, out int hauteur)
	{
		secondes = 0.0;
		largeur = 0;
		hauteur = 0;
		if (!string.Equals(Path.GetExtension(f), ".mp4", StringComparison.OrdinalIgnoreCase))
		{
			return "La vidéo doit être un fichier MP4 (H.264).";
		}
		long length = new FileInfo(f).Length;
		if (length > 1992294400)
		{
			return "Fichier trop lourd (1,9 Go au plus).";
		}
		try
		{
			Type typeFromProgID = Type.GetTypeFromProgID("Shell.Application");
			object target = Activator.CreateInstance(typeFromProgID);
			object obj = typeFromProgID.InvokeMember("NameSpace", BindingFlags.InvokeMethod, null, target, new object[1] { Path.GetDirectoryName(Path.GetFullPath(f)) });
			object it = obj.GetType().InvokeMember("ParseName", BindingFlags.InvokeMethod, null, obj, new object[1] { Path.GetFileName(f) });
			Func<string, object> func = (string nom) => it.GetType().InvokeMember("ExtendedProperty", BindingFlags.InvokeMethod, null, it, new object[1] { nom });
			object obj2 = func("System.Media.Duration");
			object obj3 = func("System.Video.FrameWidth");
			object obj4 = func("System.Video.FrameHeight");
			if (obj2 != null)
			{
				secondes = Convert.ToDouble(obj2) / 10000000.0;
			}
			if (obj3 != null)
			{
				largeur = Convert.ToInt32(obj3);
			}
			if (obj4 != null)
			{
				hauteur = Convert.ToInt32(obj4);
			}
		}
		catch
		{
		}
		if (secondes <= 0.0 || largeur <= 0 || hauteur <= 0)
		{
			return "Impossible de lire la durée ou la définition de la vidéo (fichier MP4 H.264 attendu).";
		}
		if (secondes > 181.0)
		{
			return "Vidéo trop longue : " + (int)secondes / 60 + " min " + (int)secondes % 60 + " s (3 min au plus).";
		}
		int num = Math.Max(largeur, hauteur);
		int num2 = Math.Min(largeur, hauteur);
		if (num2 < 1080 || num < 1920)
		{
			return "Définition trop faible : " + largeur + "×" + hauteur + " (1080p au minimum, soit 1920×1080).";
		}
		if (num > 4096 || num2 > 2160)
		{
			return "Définition trop élevée : " + largeur + "×" + hauteur + " (4K au maximum, soit 3840×2160).";
		}
		return null;
	}

	public static string Resume(double s, int w, int h)
	{
		int num = ((Math.Max(w, h) >= 3800) ? 2160 : Math.Min(w, h));
		return (int)s / 60 + " min " + ((int)s % 60).ToString("00") + " s  ·  " + w + "×" + h + "  (" + ((num >= 2160) ? "4K" : (num + "p")) + ")";
	}
}
