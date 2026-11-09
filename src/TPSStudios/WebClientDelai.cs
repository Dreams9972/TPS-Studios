using System;
using System.Net;

namespace TPSStudios;

internal class WebClientDelai : WebClient
{
	protected override WebRequest GetWebRequest(Uri adresse)
	{
		WebRequest webRequest = base.GetWebRequest(adresse);
		if (webRequest != null)
		{
			webRequest.Timeout = 20000;
			if (webRequest is HttpWebRequest httpWebRequest)
			{
				httpWebRequest.ReadWriteTimeout = 60000;
			}
		}
		return webRequest;
	}
}
