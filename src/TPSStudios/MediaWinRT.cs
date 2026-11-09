using System.Threading;
using Windows.Foundation;
using Windows.Media.Control;

namespace TPSStudios;

internal static class MediaWinRT
{
	private static GlobalSystemMediaTransportControlsSessionManager gest;

	private static T Attendre<T>(IAsyncOperation<T> op)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
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

	public static void Lire()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Invalid comparison between Unknown and I4
		if (gest == null)
		{
			gest = Attendre<GlobalSystemMediaTransportControlsSessionManager>(GlobalSystemMediaTransportControlsSessionManager.RequestAsync());
		}
		GlobalSystemMediaTransportControlsSession val = ((gest != null) ? gest.GetCurrentSession() : null);
		if (val == null)
		{
			MediaWin.Source = "";
			MediaWin.Titre = "";
			MediaWin.Artiste = "";
			MediaWin.EnLecture = false;
		}
		else
		{
			GlobalSystemMediaTransportControlsSessionMediaProperties val2 = Attendre<GlobalSystemMediaTransportControlsSessionMediaProperties>(val.TryGetMediaPropertiesAsync());
			MediaWin.Source = val.SourceAppUserModelId ?? "";
			MediaWin.Titre = ((val2 != null) ? (val2.Title ?? "") : "");
			MediaWin.Artiste = ((val2 != null) ? (val2.Artist ?? "") : "");
			MediaWin.EnLecture = (int)val.GetPlaybackInfo().PlaybackStatus == 4;
		}
	}
}
