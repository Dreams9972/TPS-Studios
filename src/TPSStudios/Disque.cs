using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace TPSStudios;

internal static class Disque
{
	public static void Ecrire(string f, byte[] b)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(f));
		string text = f + "." + Process.GetCurrentProcess().Id + ".tmp";
		int num = 0;
		while (true)
		{
			try
			{
				File.WriteAllBytes(text, b);
				if (File.Exists(f))
				{
					File.Replace(text, f, null, ignoreMetadataErrors: true);
				}
				else
				{
					File.Move(text, f);
				}
				break;
			}
			catch (IOException)
			{
				if (num >= 9)
				{
					try
					{
						File.Delete(text);
					}
					catch
					{
					}
					throw;
				}
				Thread.Sleep(50 * (num + 1));
			}
			catch (UnauthorizedAccessException)
			{
				if (num >= 9)
				{
					try
					{
						File.Delete(text);
					}
					catch
					{
					}
					throw;
				}
				Thread.Sleep(50 * (num + 1));
			}
			num++;
		}
	}

	public static void EcrireTexte(string f, string t)
	{
		Ecrire(f, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(t));
	}

	public static void Deplacer(string de, string vers)
	{
		int num = 0;
		while (true)
		{
			try
			{
				if (File.Exists(vers))
				{
					File.Delete(vers);
				}
				File.Move(de, vers);
				break;
			}
			catch (IOException)
			{
				if (num >= 14)
				{
					throw;
				}
				Thread.Sleep(100 * (num + 1));
			}
			catch (UnauthorizedAccessException)
			{
				if (num >= 14)
				{
					throw;
				}
				Thread.Sleep(100 * (num + 1));
			}
			num++;
		}
	}
}
