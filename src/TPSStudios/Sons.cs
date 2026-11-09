using System;
using System.IO;
using System.Media;
using System.Text;

namespace TPSStudios;

internal static class Sons
{
	private static SoundPlayer clic;

	private static MemoryStream flux;

	public static bool Actifs => Config.Oui("sons", defaut: true);

	private static byte[] Wav(int ms, Func<double, double> onde)
	{
		int num = 44100;
		int num2 = num * ms / 1000;
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(Encoding.ASCII.GetBytes("RIFF"));
		binaryWriter.Write(36 + num2 * 2);
		binaryWriter.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
		binaryWriter.Write(16);
		binaryWriter.Write((short)1);
		binaryWriter.Write((short)1);
		binaryWriter.Write(num);
		binaryWriter.Write(num * 2);
		binaryWriter.Write((short)2);
		binaryWriter.Write((short)16);
		binaryWriter.Write(Encoding.ASCII.GetBytes("data"));
		binaryWriter.Write(num2 * 2);
		for (int i = 0; i < num2; i++)
		{
			double num3 = onde((double)i / (double)num);
			binaryWriter.Write((short)Math.Max(-32767.0, Math.Min(32767.0, num3 * 32767.0)));
		}
		binaryWriter.Flush();
		return memoryStream.ToArray();
	}

	public static void Clic()
	{
		if (Fenetre.ModeCapture || !Actifs)
		{
			return;
		}
		try
		{
			if (clic == null)
			{
				double ph = 0.0;
				double tPrec = 0.0;
				double ph2 = 0.0;
				byte[] buffer = Wav(34, delegate(double t)
				{
					double num = 620.0 + 520.0 * Math.Exp((0.0 - t) / 0.004);
					ph += Math.PI * 2.0 * num * (t - tPrec);
					ph2 += Math.PI * 360.0 * (t - tPrec);
					tPrec = t;
					double num2 = Math.Min(1.0, t / 0.0012) * Math.Exp((0.0 - t) / 0.0055);
					return 0.075 * num2 * (Math.Sin(ph) + 0.45 * Math.Sin(ph2));
				});
				flux = new MemoryStream(buffer);
				clic = new SoundPlayer(flux);
				clic.Load();
			}
			clic.Play();
		}
		catch
		{
		}
	}
}
