using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TPSStudios;

internal static class Demarrage
{
	private const string CLE = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	private const string NOM = "TPSStudios";

	public static bool Actif
	{
		get
		{
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
				return registryKey != null && registryKey.GetValue("TPSStudios") != null;
			}
			catch
			{
				return false;
			}
		}
	}

	public static void Definir(bool oui)
	{
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
			if (oui)
			{
				string text = Path.Combine(Config.Dossier, "TPS Studios.exe");
				if (!File.Exists(text))
				{
					text = Application.ExecutablePath;
				}
				registryKey.SetValue("TPSStudios", "\"" + text + "\" /portable");
			}
			else if (registryKey.GetValue("TPSStudios") != null)
			{
				registryKey.DeleteValue("TPSStudios", throwOnMissingValue: false);
			}
		}
		catch
		{
		}
	}
}
