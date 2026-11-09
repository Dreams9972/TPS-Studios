using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TPSStudios;

internal class FluxSocial
{
	private string mod;

	private Action<Dictionary<string, object>> surMsg;

	private Action<bool> surEtat;

	private volatile bool stop;

	private Thread t;

	private ClientWebSocket ws;

	public FluxSocial(string mod, Action<Dictionary<string, object>> surMsg, Action<bool> surEtat)
	{
		this.mod = mod;
		this.surMsg = surMsg;
		this.surEtat = surEtat;
		t = new Thread(Boucle);
		t.IsBackground = true;
		t.Start();
	}

	public void Fermer()
	{
		stop = true;
		try
		{
			if (ws != null)
			{
				ws.Abort();
			}
		}
		catch
		{
		}
	}

	private void Boucle()
	{
		int num = 1000;
		while (!stop)
		{
			try
			{
				using ClientWebSocket clientWebSocket = new ClientWebSocket();
				ws = clientWebSocket;
				Uri uri = new Uri((Social.Base.StartsWith("https") ? "wss" : "ws") + Social.Base.Substring(Social.Base.IndexOf("://")) + "/v1/ws/" + mod);
				clientWebSocket.ConnectAsync(uri, CancellationToken.None).Wait(10000);
				if (clientWebSocket.State != WebSocketState.Open)
				{
					throw new Exception("connexion impossible");
				}
				surEtat(obj: true);
				num = 1000;
				DateTime now = DateTime.Now;
				byte[] array = new byte[16384];
				while (!stop && clientWebSocket.State == WebSocketState.Open)
				{
					Task<WebSocketReceiveResult> task = clientWebSocket.ReceiveAsync(new ArraySegment<byte>(array), CancellationToken.None);
					while (!task.Wait(1000) && !stop)
					{
						if ((DateTime.Now - now).TotalSeconds > 25.0)
						{
							clientWebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("ping")), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None).Wait(3000);
							now = DateTime.Now;
						}
					}
					if (stop || task.Result.MessageType == WebSocketMessageType.Close)
					{
						break;
					}
					string @string = Encoding.UTF8.GetString(array, 0, task.Result.Count);
					if (@string == "pong")
					{
						continue;
					}
					try
					{
						if (new JavaScriptSerializer().DeserializeObject(@string) is Dictionary<string, object> obj)
						{
							surMsg(obj);
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			if (stop)
			{
				break;
			}
			surEtat(obj: false);
			Thread.Sleep(num);
			num = Math.Min(num * 2, 30000);
		}
	}
}
