using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Zx
{
	public static float Z = 1f;

	public static readonly string[] Ids = new string[6] { "auto", "100", "115", "125", "150", "175" };

	public static readonly string[] Noms = new string[6] { "Automatique", "100 %", "115 %", "125 %", "150 %", "175 %" };

	public static int P(int v)
	{
		if (Z != 1f)
		{
			return (int)Math.Round((float)v * Z);
		}
		return v;
	}

	public static float P(float v)
	{
		return v * Z;
	}

	public static double P(double v)
	{
		return v * (double)Z;
	}

	public static float Auto()
	{
		try
		{
			Rectangle bounds = Screen.PrimaryScreen.Bounds;
			float val = Math.Min((float)bounds.Width / 1920f, (float)bounds.Height / 1080f);
			return (float)Math.Round(Math.Max(1f, Math.Min(1.75f, val)) * 20f) / 20f;
		}
		catch
		{
			return 1f;
		}
	}

	public static void Init()
	{
		string text = Environment.GetEnvironmentVariable("TPS_ZOOM");
		if (string.IsNullOrEmpty(text))
		{
			try
			{
				text = Config.Lire("taille_interface");
			}
			catch
			{
				text = "";
			}
		}
		Z = ((!string.IsNullOrEmpty(text) && text != "auto" && int.TryParse(text, out var result) && result >= 80 && result <= 200) ? ((float)result / 100f) : Auto());
	}
}
