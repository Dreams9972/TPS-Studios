using System;
using System.Runtime.InteropServices;

namespace TPSStudios;

internal static class VolumeWin
{
	[ComImport]
	[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
	private class MMDeviceEnumerator
	{
	}

	[ComImport]
	[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IMMDeviceEnumerator
	{
		int NonUtilise();

		[PreserveSig]
		int GetDefaultAudioEndpoint(int flux, int role, out IMMDevice appareil);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
	private interface IMMDevice
	{
		[PreserveSig]
		int Activate(ref Guid iid, int ctx, IntPtr param, [MarshalAs(UnmanagedType.IUnknown)] out object itf);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
	private interface IAudioEndpointVolume
	{
		int RegisterControlChangeNotify(IntPtr p);

		int UnregisterControlChangeNotify(IntPtr p);

		int GetChannelCount(out int n);

		int SetMasterVolumeLevel(float db, ref Guid ctx);

		int SetMasterVolumeLevelScalar(float v, ref Guid ctx);

		int GetMasterVolumeLevel(out float db);

		int GetMasterVolumeLevelScalar(out float v);

		int SetChannelVolumeLevel(uint c, float db, ref Guid ctx);

		int SetChannelVolumeLevelScalar(uint c, float v, ref Guid ctx);

		int GetChannelVolumeLevel(uint c, out float db);

		int GetChannelVolumeLevelScalar(uint c, out float v);

		int SetMute([MarshalAs(UnmanagedType.Bool)] bool m, ref Guid ctx);

		int GetMute([MarshalAs(UnmanagedType.Bool)] out bool m);
	}

	private static float cache = -1f;

	private static DateTime lu = DateTime.MinValue;

	private static IAudioEndpointVolume Ep()
	{
		IMMDeviceEnumerator iMMDeviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
		if (iMMDeviceEnumerator.GetDefaultAudioEndpoint(0, 1, out var appareil) != 0 || appareil == null)
		{
			return null;
		}
		Guid iid = typeof(IAudioEndpointVolume).GUID;
		if (appareil.Activate(ref iid, 23, IntPtr.Zero, out var itf) != 0)
		{
			return null;
		}
		return itf as IAudioEndpointVolume;
	}

	public static float Lire()
	{
		if ((DateTime.Now - lu).TotalMilliseconds < 300.0)
		{
			return cache;
		}
		lu = DateTime.Now;
		try
		{
			IAudioEndpointVolume audioEndpointVolume = Ep();
			if (audioEndpointVolume != null && audioEndpointVolume.GetMasterVolumeLevelScalar(out var v) == 0)
			{
				cache = v;
			}
		}
		catch
		{
		}
		return cache;
	}

	public static void Ecrire(float v)
	{
		v = Math.Max(0f, Math.Min(1f, v));
		try
		{
			IAudioEndpointVolume audioEndpointVolume = Ep();
			Guid ctx = Guid.Empty;
			if (audioEndpointVolume != null)
			{
				audioEndpointVolume.SetMasterVolumeLevelScalar(v, ref ctx);
				if (v > 0f)
				{
					audioEndpointVolume.SetMute(m: false, ref ctx);
				}
			}
			cache = v;
			lu = DateTime.Now;
		}
		catch
		{
		}
	}
}
