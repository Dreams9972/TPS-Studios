using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.Win32.SafeHandles;

namespace TPSStudios;

internal static class DiscordPresence
{
	private static Thread fil;

	private static volatile bool actif;

	private static string appId = "";

	private static string invite = "";

	private static readonly object verrou = new object();

	private static string details = "";

	private static string etat = "";

	private static DateTime? debut;

	private static bool aEnvoyer;

	public static volatile string Statut = "Désactivée";

	public static volatile string Utilisateur = "";

	public static bool EnMarche => actif;

	private static string Pipe
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("TPS_DISCORD_PIPE");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				return environmentVariable;
			}
			return "discord-ipc-";
		}
	}

	public static void Demarrer(string app, string lienInvitation)
	{
		lock (verrou)
		{
			invite = lienInvitation ?? "";
			if (actif && appId == app)
			{
				return;
			}
		}
		Arreter();
		appId = app;
		actif = true;
		Statut = "Connexion à Discord…";
		fil = new Thread(Boucle);
		fil.IsBackground = true;
		fil.Start();
	}

	public static void Arreter()
	{
		if (!actif)
		{
			return;
		}
		actif = false;
		Thread thread = fil;
		fil = null;
		if (thread != null)
		{
			try
			{
				thread.Join(2500);
			}
			catch
			{
			}
		}
		Statut = "Désactivée";
		Utilisateur = "";
	}

	public static void Definir(string d, string e, DateTime? deb)
	{
		lock (verrou)
		{
			details = d ?? "";
			etat = e ?? "";
			debut = deb;
			aEnvoyer = true;
		}
	}

	private static void Envoyer(Stream s, int op, string json)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		byte[] array = new byte[8];
		BitConverter.GetBytes(op).CopyTo(array, 0);
		BitConverter.GetBytes(bytes.Length).CopyTo(array, 4);
		s.Write(array, 0, 8);
		s.Write(bytes, 0, bytes.Length);
		s.Flush();
	}

	private static byte[] Exact(Stream s, int n)
	{
		byte[] array = new byte[n];
		Task<int> task;
		for (int i = 0; i < n; i += task.Result)
		{
			task = s.ReadAsync(array, i, n - i);
			if (!task.Wait(4000))
			{
				throw new TimeoutException("Discord ne répond pas");
			}
			if (task.Result <= 0)
			{
				throw new IOException("canal fermé");
			}
		}
		return array;
	}

	private static string Trame(Stream s, out int op)
	{
		byte[] value = Exact(s, 8);
		op = BitConverter.ToInt32(value, 0);
		int num = BitConverter.ToInt32(value, 4);
		if (num < 0 || num > 1048576)
		{
			throw new IOException("réponse invalide");
		}
		return Encoding.UTF8.GetString(Exact(s, num));
	}

	private static string Lire(Stream s)
	{
		for (int i = 0; i < 20; i++)
		{
			int op;
			string text = Trame(s, out op);
			switch (op)
			{
			case 3:
				break;
			case 2:
				throw new IOException("Discord a fermé la connexion : " + text);
			default:
				return text;
			}
			Envoyer(s, 4, text);
		}
		throw new IOException("trop de messages de Discord");
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool PeekNamedPipe(SafePipeHandle h, IntPtr buf, uint taille, IntPtr lus, out uint dispo, IntPtr reste);

	private static void Ecouter(NamedPipeClientStream p)
	{
		for (int i = 0; i < 10; i++)
		{
			if (!PeekNamedPipe(p.SafePipeHandle, IntPtr.Zero, 0u, IntPtr.Zero, out var dispo, IntPtr.Zero))
			{
				throw new IOException("canal fermé par Discord");
			}
			if (dispo < 8)
			{
				break;
			}
			int op;
			string text = Trame(p, out op);
			switch (op)
			{
			case 3:
				Envoyer(p, 4, text);
				break;
			case 2:
				throw new IOException("Discord a fermé la connexion : " + text);
			}
		}
	}

	private static string Cut(string t, int max)
	{
		t = (t ?? "").Trim();
		if (t.Length > max)
		{
			t = t.Substring(0, max - 1) + "…";
		}
		if (t.Length != 1)
		{
			return t;
		}
		return t + " ";
	}

	private static void Boucle()
	{
		NamedPipeClientStream namedPipeClientStream = null;
		DateTime dateTime = DateTime.MinValue;
		double num = 6.0;
		try
		{
			while (actif)
			{
				if (namedPipeClientStream == null)
				{
					if ((DateTime.Now - dateTime).TotalSeconds < num)
					{
						Thread.Sleep(300);
						continue;
					}
					dateTime = DateTime.Now;
					for (int i = 0; i < 10; i++)
					{
						if (namedPipeClientStream != null)
						{
							break;
						}
						try
						{
							NamedPipeClientStream namedPipeClientStream2 = new NamedPipeClientStream(".", Pipe + i, PipeDirection.InOut, PipeOptions.Asynchronous);
							namedPipeClientStream2.Connect(250);
							namedPipeClientStream = namedPipeClientStream2;
						}
						catch
						{
						}
					}
					if (namedPipeClientStream == null)
					{
						Statut = "Discord non détecté (ouvrez l'application Discord)";
						Utilisateur = "";
						num = Math.Min(60.0, num * 1.6);
						continue;
					}
					try
					{
						Envoyer(namedPipeClientStream, 0, "{\"v\":1,\"client_id\":\"" + appId + "\"}");
						string input = Lire(namedPipeClientStream);
						Dictionary<string, object> dictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(input);
						if (dictionary != null && dictionary.ContainsKey("evt") && dictionary["evt"] as string == "ERROR")
						{
							Statut = "Identifiant d'application Discord refusé";
							try
							{
								namedPipeClientStream.Dispose();
							}
							catch
							{
							}
							namedPipeClientStream = null;
							actif = false;
							break;
						}
						string text = "";
						Dictionary<string, object> dictionary2 = ((dictionary != null && dictionary.ContainsKey("data")) ? (dictionary["data"] as Dictionary<string, object>) : null);
						Dictionary<string, object> dictionary3 = ((dictionary2 != null && dictionary2.ContainsKey("user")) ? (dictionary2["user"] as Dictionary<string, object>) : null);
						if (dictionary3 != null)
						{
							text = ((dictionary3.TryGetValue("global_name", out var value) && value != null) ? value.ToString() : (dictionary3.ContainsKey("username") ? string.Concat(dictionary3["username"]) : ""));
						}
						Utilisateur = text;
						Statut = ((text.Length > 0) ? ("Connecté à Discord : " + text) : "Connecté à Discord");
						lock (verrou)
						{
							aEnvoyer = true;
						}
						num = 6.0;
					}
					catch (Exception ex)
					{
						try
						{
							namedPipeClientStream.Dispose();
						}
						catch
						{
						}
						namedPipeClientStream = null;
						if (ex.Message.IndexOf("4000", StringComparison.Ordinal) >= 0 || ex.Message.IndexOf("Invalid Client", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							Statut = "Identifiant d'application Discord refusé : copiez l'« Application ID » de discord.com/developers/applications";
							Utilisateur = "";
							Journal.Une("discord", ex);
							actif = false;
							break;
						}
						Journal.Une("discord", ex);
						Statut = "Discord ne répond pas";
						num = Math.Min(60.0, num * 1.6);
						continue;
					}
				}
				string text2 = null;
				string text3 = null;
				DateTime? dateTime2 = null;
				bool flag;
				lock (verrou)
				{
					flag = aEnvoyer;
					if (flag)
					{
						text2 = details;
						text3 = etat;
						dateTime2 = debut;
						aEnvoyer = false;
					}
				}
				if (flag)
				{
					try
					{
						Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
						if (text2.Length > 0)
						{
							dictionary4["details"] = Cut(text2, 128);
							if (text3.Length > 0)
							{
								dictionary4["state"] = Cut(text3, 128);
							}
						}
						if (dateTime2.HasValue)
						{
							Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
							dictionary5["start"] = (long)(dateTime2.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
							dictionary4["timestamps"] = dictionary5;
						}
						string text4;
						lock (verrou)
						{
							text4 = invite;
						}
						if (text4.StartsWith("https://") && text2.Length > 0)
						{
							Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
							dictionary6["label"] = "Rejoindre TPS Groupe";
							dictionary6["url"] = text4;
							dictionary4["buttons"] = new object[1] { dictionary6 };
						}
						Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
						dictionary7["pid"] = Process.GetCurrentProcess().Id;
						if (dictionary4.Count > 0)
						{
							dictionary7["activity"] = dictionary4;
						}
						else
						{
							dictionary7["activity"] = null;
						}
						Dictionary<string, object> dictionary8 = new Dictionary<string, object>();
						dictionary8["cmd"] = "SET_ACTIVITY";
						dictionary8["args"] = dictionary7;
						dictionary8["nonce"] = Guid.NewGuid().ToString();
						Envoyer(namedPipeClientStream, 1, new JavaScriptSerializer().Serialize(dictionary8));
						Lire(namedPipeClientStream);
						Thread.Sleep(1500);
					}
					catch (Exception e)
					{
						Journal.Une("discord", e);
						Statut = "Connexion à Discord perdue";
						try
						{
							namedPipeClientStream.Dispose();
						}
						catch
						{
						}
						namedPipeClientStream = null;
						lock (verrou)
						{
							aEnvoyer = true;
						}
					}
				}
				if (namedPipeClientStream != null)
				{
					try
					{
						Ecouter(namedPipeClientStream);
					}
					catch (Exception e2)
					{
						Journal.Une("discord", e2);
						Statut = "Reconnexion à Discord…";
						try
						{
							namedPipeClientStream.Dispose();
						}
						catch
						{
						}
						namedPipeClientStream = null;
						num = 2.0;
						lock (verrou)
						{
							aEnvoyer = true;
						}
					}
				}
				Thread.Sleep(300);
			}
			if (namedPipeClientStream != null)
			{
				try
				{
					Dictionary<string, object> dictionary9 = new Dictionary<string, object>();
					dictionary9["pid"] = Process.GetCurrentProcess().Id;
					dictionary9["activity"] = null;
					Dictionary<string, object> dictionary10 = new Dictionary<string, object>();
					dictionary10["cmd"] = "SET_ACTIVITY";
					dictionary10["args"] = dictionary9;
					dictionary10["nonce"] = Guid.NewGuid().ToString();
					Envoyer(namedPipeClientStream, 1, new JavaScriptSerializer().Serialize(dictionary10));
					Lire(namedPipeClientStream);
					return;
				}
				catch
				{
					return;
				}
			}
		}
		catch (Exception e3)
		{
			Journal.Une("discord", e3);
		}
		finally
		{
			if (namedPipeClientStream != null)
			{
				try
				{
					namedPipeClientStream.Dispose();
				}
				catch
				{
				}
			}
		}
	}
}
