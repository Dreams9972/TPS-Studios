using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TPSStudios;

internal static class MediaWin
{
	public static string Titre = "";

	public static string Artiste = "";

	public static string Source = "";

	public static bool EnLecture;

	public static bool Dispo = true;

	private static Thread th;

	public static void Demarrer()
	{
		if (th != null || Fenetre.ModeCapture)
		{
			return;
		}
		th = new Thread((ThreadStart)delegate
		{
			int num = 0;
			while (true)
			{
				if (Dispo)
				{
					try
					{
						Lire();
					}
					catch (Exception e)
					{
						Dispo = false;
						Journal.Une("media-windows", e);
					}
				}
				if (num++ % 2 == 0 && (Source.Length == 0 || !Dispo))
				{
					Musique.ScanFenetres();
				}
				Thread.Sleep(1000);
			}
		});
		th.IsBackground = true;
		th.Start();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Lire()
	{
		MediaWinRT.Lire();
	}
}
