using System;
using System.Threading;
using Windows.Foundation;
using Windows.Media.Control;

namespace TPSStudios;

internal static class MediaWinRT
{
	private static GlobalSystemMediaTransportControlsSessionManager gest;

	private static T Attendre<T>(IAsyncOperation<T> op)
	{
		int num = 0;
		while ((int)((IAsyncInfo)op).Status == 0 && num++ < 300)
		{
			Thread.Sleep(10);
		}
		if ((int)((IAsyncInfo)op).Status != 1)
		{
			return default(T);
		}
		return op.GetResults();
	}

	// Spotify ou Apple Music (iTunes) uniquement : les navigateurs, les bandes-annonces du lanceur et les autres sons sont ignorés
	public static bool EstLecteurMusique(string source, string mode)
	{
		string text = (source ?? "").ToLowerInvariant();
		bool flag = text.Contains("spotify");
		bool flag2 = text.Contains("applemusic") || text.Contains("apple music") || text.Contains("itunes") || text.Contains("appleinc.");
		if (mode == "spotify")
		{
			return flag;
		}
		if (mode == "apple")
		{
			return flag2;
		}
		return flag || flag2;
	}

	private static GlobalSystemMediaTransportControlsSession Session()
	{
		if (gest == null)
		{
			gest = Attendre<GlobalSystemMediaTransportControlsSessionManager>(GlobalSystemMediaTransportControlsSessionManager.RequestAsync());
		}
		if (gest == null)
		{
			return null;
		}
		string mode = Musique.Mode;
		GlobalSystemMediaTransportControlsSession result = null;
		foreach (GlobalSystemMediaTransportControlsSession session in gest.GetSessions())
		{
			if (!EstLecteurMusique(session.SourceAppUserModelId, mode))
			{
				continue;
			}
			if ((int)session.GetPlaybackInfo().PlaybackStatus == 4)
			{
				return session;   // celui qui joue en priorité
			}
			if (result == null)
			{
				result = session;
			}
		}
		return result;
	}

	public static void Lire()
	{
		GlobalSystemMediaTransportControlsSession val = Session();
		if (val == null)
		{
			MediaWin.Source = "";
			MediaWin.Titre = "";
			MediaWin.Artiste = "";
			MediaWin.EnLecture = false;
			MediaWin.Aleatoire = false;
			MediaWin.AleatoireDispo = false;
			return;
		}
		GlobalSystemMediaTransportControlsSessionMediaProperties val2 = Attendre<GlobalSystemMediaTransportControlsSessionMediaProperties>(val.TryGetMediaPropertiesAsync());
		GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo = val.GetPlaybackInfo();
		MediaWin.Source = val.SourceAppUserModelId ?? "";
		MediaWin.Titre = ((val2 != null) ? (val2.Title ?? "") : "");
		string artiste = ((val2 != null) ? (val2.Artist ?? "") : "");
		int num = artiste.IndexOf(" — ", StringComparison.Ordinal);   // Apple Music : « Artiste — Album - Single » : on garde l'artiste
		MediaWin.Artiste = ((num > 0) ? artiste.Substring(0, num) : artiste).Trim();
		MediaWin.EnLecture = (int)playbackInfo.PlaybackStatus == 4;
		MediaWin.Aleatoire = playbackInfo.IsShuffleActive.GetValueOrDefault();
		MediaWin.AleatoireDispo = playbackInfo.Controls.IsShuffleEnabled;
	}

	// commande envoyée à Spotify / Apple Music seulement ; false s'il n'y a pas de session de ces lecteurs
	public static bool Commande(string quoi)
	{
		GlobalSystemMediaTransportControlsSession val = Session();
		if (val == null)
		{
			return false;
		}
		switch (quoi)
		{
		case "lecture":
			return Attendre<bool>(val.TryTogglePlayPauseAsync());
		case "suivant":
			return Attendre<bool>(val.TrySkipNextAsync());
		case "precedent":
			return Attendre<bool>(val.TrySkipPreviousAsync());
		case "aleatoire":
		{
			GlobalSystemMediaTransportControlsSessionPlaybackInfo playbackInfo = val.GetPlaybackInfo();
			if (!playbackInfo.Controls.IsShuffleEnabled)
			{
				return false;
			}
			bool flag = !playbackInfo.IsShuffleActive.GetValueOrDefault();
			bool flag2 = Attendre<bool>(val.TryChangeShuffleActiveAsync(flag));
			if (flag2)
			{
				MediaWin.Aleatoire = flag;
			}
			return flag2;
		}
		default:
			return false;
		}
	}
}
