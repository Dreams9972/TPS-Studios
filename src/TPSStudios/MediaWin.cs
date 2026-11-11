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

	public static bool Aleatoire;

	public static bool AleatoireDispo;

	// commande à Spotify / Apple Music, hors de l'interface (jusqu'à 3 s d'attente) ; touches multimédia seulement si l'API de Windows manque
	public static void Envoyer(string quoi, Action<bool> fini = null)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			bool ok = false;
			if (Dispo)
			{
				try
				{
					ok = Commander(quoi);
				}
				catch (Exception e)
				{
					Journal.Une("media-commande", e);
				}
			}
			else if (quoi != "aleatoire")
			{
				Musique.Touche((quoi == "lecture") ? Musique.LectPause : ((quoi == "suivant") ? Musique.Suivant : Musique.Precedent));
				ok = true;
			}
			fini?.Invoke(ok);
		});
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool Commander(string quoi)
	{
		return MediaWinRT.Commande(quoi);
	}

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
