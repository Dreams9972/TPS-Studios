using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace TPSStudios;

internal static class GitHub
{
	public const string DEPOT = "Dreams9972/TPS-Studios";

	private static readonly byte[] entropie = Encoding.UTF8.GetBytes("TPS-Studios-admin-v1");

	private static int compteurGalerie;

	public static string Api
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("TPS_GITHUB_API");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				return environmentVariable.TrimEnd('/');
			}
			return "https://api.github.com";
		}
	}

	private static string FichierJeton => Path.Combine(Config.Dossier, "admin.dat");

	public static bool JetonPresent => File.Exists(FichierJeton);

	private static string Jeton()
	{
		try
		{
			return Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(FichierJeton), entropie, DataProtectionScope.CurrentUser));
		}
		catch
		{
			return null;
		}
	}

	public static void EnregistrerJeton(string t)
	{
		Directory.CreateDirectory(Config.Dossier);
		File.WriteAllBytes(FichierJeton, ProtectedData.Protect(Encoding.UTF8.GetBytes(t), entropie, DataProtectionScope.CurrentUser));
	}

	public static void OublierJeton()
	{
		try
		{
			File.Delete(FichierJeton);
		}
		catch
		{
		}
	}

	private static string Appel(string methode, string url, string corps, string jeton)
	{
		if (string.IsNullOrEmpty(jeton))
		{
			throw new Exception("jeton administrateur introuvable : réactivez le mode administrateur");
		}
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
		httpWebRequest.Method = methode;
		httpWebRequest.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
		httpWebRequest.Accept = "application/vnd.github+json";
		httpWebRequest.Timeout = 25000;
		httpWebRequest.ReadWriteTimeout = 60000;
		httpWebRequest.Headers["Authorization"] = "Bearer " + jeton;
		httpWebRequest.Headers["X-GitHub-Api-Version"] = "2022-11-28";
		if (corps != null)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(corps);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.ContentLength = bytes.Length;
			using Stream stream = httpWebRequest.GetRequestStream();
			stream.Write(bytes, 0, bytes.Length);
		}
		try
		{
			using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
			return streamReader.ReadToEnd();
		}
		catch (WebException ex)
		{
			HttpWebResponse httpWebResponse2 = ex.Response as HttpWebResponse;
			string message = ex.Message;
			if (httpWebResponse2 != null)
			{
				int statusCode = (int)httpWebResponse2.StatusCode;
				object obj = statusCode;
				object obj2;
				switch (statusCode)
				{
				default:
					obj2 = "";
					break;
				case 409:
				case 422:
					obj2 = " (conflit : le catalogue a changé entre-temps, rechargez la liste)";
					break;
				case 404:
					obj2 = " (introuvable, ou jeton sans accès au dépôt)";
					break;
				case 403:
					obj2 = " (droits insuffisants ou limite atteinte)";
					break;
				case 401:
					obj2 = " (jeton refusé ou expiré)";
					break;
				}
				message = string.Concat("GitHub a répondu ", obj, obj2);
			}
			throw new Exception(message);
		}
	}

	private static JavaScriptSerializer JS()
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 16777216;
		return javaScriptSerializer;
	}

	public static string Verifier(string jeton)
	{
		try
		{
			Dictionary<string, object> dictionary = JS().Deserialize<Dictionary<string, object>>(Appel("GET", Api + "/repos/Dreams9972/TPS-Studios", null, jeton));
			if (dictionary != null && dictionary.TryGetValue("permissions", out var value) && value is Dictionary<string, object> && ((Dictionary<string, object>)value).TryGetValue("push", out var value2) && value2 is bool && (bool)value2)
			{
				return null;
			}
			return "Ce jeton n'a pas le droit d'écrire sur le dépôt Dreams9972/TPS-Studios (permission « Contents : Read and write » nécessaire).";
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	public static string JetonGhCli()
	{
		string[] array = new string[2] { "C:\\Program Files\\GitHub CLI\\gh.exe", "gh" };
		foreach (string fileName in array)
		{
			try
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName, "auth token");
				processStartInfo.UseShellExecute = false;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.RedirectStandardError = true;
				processStartInfo.CreateNoWindow = true;
				using Process process = Process.Start(processStartInfo);
				string text = process.StandardOutput.ReadToEnd().Trim();
				if (!process.WaitForExit(8000))
				{
					try
					{
						process.Kill();
					}
					catch
					{
					}
				}
				else if (process.ExitCode == 0 && text.Length > 20)
				{
					return text;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	public static Dictionary<string, object> LireCatalogue(out string sha)
	{
		Dictionary<string, object> dictionary = JS().Deserialize<Dictionary<string, object>>(Appel("GET", Api + "/repos/Dreams9972/TPS-Studios/contents/catalogue.json?ref=main", null, Jeton()));
		sha = dictionary["sha"] as string;
		string @string = Encoding.UTF8.GetString(Convert.FromBase64String(((string)dictionary["content"]).Replace("\n", "")));
		return JS().Deserialize<Dictionary<string, object>>(@string);
	}

	private static void Ecrire(string chemin, byte[] octets, string message, string sha)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["message"] = message;
		dictionary["content"] = Convert.ToBase64String(octets);
		dictionary["branch"] = "main";
		if (!string.IsNullOrEmpty(sha))
		{
			dictionary["sha"] = sha;
		}
		Appel("PUT", Api + "/repos/Dreams9972/TPS-Studios/contents/" + chemin, JS().Serialize(dictionary), Jeton());
	}

	public static void EcrireCatalogue(Dictionary<string, object> cat, string sha, string message)
	{
		Ecrire("catalogue.json", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(Json.Ecrire(cat)), message, sha);
	}

	public static string EnvoyerImageGalerie(string fichier, string id)
	{
		if (new FileInfo(fichier).Length > 20971520)
		{
			throw new Exception("photo trop lourde (20 Mo maximum)");
		}
		byte[] octets;
		using (Image image = Image.FromFile(fichier))
		{
			if (image.Width < 300 || image.Height < 150)
			{
				throw new Exception("photo trop petite (300×150 minimum)");
			}
			double num = Math.Min(1.0, 1920.0 / (double)image.Width);
			int width = Math.Max(1, (int)((double)image.Width * num));
			int height = Math.Max(1, (int)((double)image.Height * num));
			using Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.DrawImage(image, 0, 0, width, height);
			}
			ImageCodecInfo encoder = null;
			ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo imageCodecInfo in imageEncoders)
			{
				if (imageCodecInfo.MimeType == "image/jpeg")
				{
					encoder = imageCodecInfo;
				}
			}
			EncoderParameters encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 88L);
			using MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, encoder, encoderParameters);
			octets = memoryStream.ToArray();
		}
		string text = "images/galerie/" + id + "-" + DateTime.UtcNow.Ticks.ToString("x") + "-" + ++compteurGalerie + ".jpg";
		Ecrire(text, octets, "Photo de la page " + id, null);
		return "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/" + text;
	}

	public static string EnvoyerVideo(string fichier, string id, Action<string> etat)
	{
		string text = Jeton();
		JavaScriptSerializer javaScriptSerializer = JS();
		Dictionary<string, object> dictionary;
		try
		{
			dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(Appel("GET", Api + "/repos/Dreams9972/TPS-Studios/releases/tags/medias", null, text));
		}
		catch (Exception ex)
		{
			if (ex.Message.IndexOf("404") < 0)
			{
				throw;
			}
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2["tag_name"] = "medias";
			dictionary2["target_commitish"] = "main";
			dictionary2["name"] = "Médias des pages";
			dictionary2["prerelease"] = true;
			dictionary2["make_latest"] = "false";
			dictionary2["body"] = "Bandes-annonces des pages du lanceur TPS Groupe (envoyées depuis le mode administrateur).";
			dictionary = javaScriptSerializer.Deserialize<Dictionary<string, object>>(Appel("POST", Api + "/repos/Dreams9972/TPS-Studios/releases", javaScriptSerializer.Serialize(dictionary2), text));
		}
		string text2 = dictionary["upload_url"] as string;
		int num = text2.IndexOf('{');
		if (num > 0)
		{
			text2 = text2.Substring(0, num);
		}
		string stringToEscape = id + "-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".mp4";
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text2 + "?name=" + Uri.EscapeDataString(stringToEscape));
		httpWebRequest.Method = "POST";
		httpWebRequest.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
		httpWebRequest.Accept = "application/vnd.github+json";
		httpWebRequest.Headers["Authorization"] = "Bearer " + text;
		httpWebRequest.Headers["X-GitHub-Api-Version"] = "2022-11-28";
		httpWebRequest.ContentType = "video/mp4";
		httpWebRequest.AllowWriteStreamBuffering = false;
		httpWebRequest.SendChunked = false;
		httpWebRequest.Timeout = 60000;
		httpWebRequest.ReadWriteTimeout = 900000;
		long val = (httpWebRequest.ContentLength = new FileInfo(fichier).Length);
		using (FileStream fileStream = File.OpenRead(fichier))
		{
			using Stream stream = httpWebRequest.GetRequestStream();
			byte[] array = new byte[1048576];
			long num2 = 0L;
			int num3 = -1;
			int num4;
			while ((num4 = fileStream.Read(array, 0, array.Length)) > 0)
			{
				stream.Write(array, 0, num4);
				num2 += num4;
				int num5 = (int)(num2 * 100 / Math.Max(1L, val));
				if (num5 != num3 && etat != null)
				{
					num3 = num5;
					etat("Envoi de la bande-annonce… " + num5 + " %");
				}
			}
		}
		string input;
		try
		{
			using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
			input = streamReader.ReadToEnd();
		}
		catch (WebException ex2)
		{
			HttpWebResponse httpWebResponse2 = ex2.Response as HttpWebResponse;
			throw new Exception("envoi de la vidéo refusé" + ((httpWebResponse2 != null) ? (" (GitHub a répondu " + (int)httpWebResponse2.StatusCode + ")") : (" : " + ex2.Message)));
		}
		return javaScriptSerializer.Deserialize<Dictionary<string, object>>(input)["browser_download_url"] as string;
	}

	public static string EnvoyerImage(string fichier, string id)
	{
		string text = Path.GetExtension(fichier).ToLowerInvariant().TrimStart('.');
		if (text == "jpeg")
		{
			text = "jpg";
		}
		if (text != "jpg" && text != "png")
		{
			throw new Exception("image : format jpg ou png seulement");
		}
		FileInfo fileInfo = new FileInfo(fichier);
		if (fileInfo.Length > 5242880)
		{
			throw new Exception("image trop lourde (5 Mo maximum)");
		}
		using (Image image = Image.FromFile(fichier))
		{
			if (image.Width < 200 || image.Height < 100)
			{
				throw new Exception("image trop petite (200×100 minimum)");
			}
		}
		string text2 = "images/" + id + "." + text;
		string sha = null;
		try
		{
			sha = JS().Deserialize<Dictionary<string, object>>(Appel("GET", Api + "/repos/Dreams9972/TPS-Studios/contents/" + text2 + "?ref=main", null, Jeton()))["sha"] as string;
		}
		catch
		{
		}
		Ecrire(text2, File.ReadAllBytes(fichier), "Image de la page " + id, sha);
		return "https://raw.githubusercontent.com/Dreams9972/TPS-Studios/main/" + text2 + "?v=" + DateTime.UtcNow.Ticks;
	}

	// ---------- Paquets de mods (GitHub Releases) ----------

	/// <summary>Release portant ce tag, ou null si elle n'existe pas.</summary>
	public static Dictionary<string, object> ReleaseParTag(string tag)
	{
		try
		{
			return JS().Deserialize<Dictionary<string, object>>(Appel("GET", Api + "/repos/Dreams9972/TPS-Studios/releases/tags/" + Uri.EscapeDataString(tag), null, Jeton()));
		}
		catch (Exception ex)
		{
			if (ex.Message.IndexOf("404") >= 0)
			{
				return null;
			}
			throw;
		}
	}

	public static Dictionary<string, object> CreerRelease(string tag, string nom, string corps)
	{
		Dictionary<string, object> d = new Dictionary<string, object>();
		d["tag_name"] = tag;
		d["target_commitish"] = "main";
		d["name"] = nom;
		d["body"] = corps;
		d["make_latest"] = "false";
		return JS().Deserialize<Dictionary<string, object>>(Appel("POST", Api + "/repos/Dreams9972/TPS-Studios/releases", JS().Serialize(d), Jeton()));
	}

	/// <summary>Supprime de la release les fichiers portant ce nom (reprise après un envoi interrompu).</summary>
	public static void SupprimerAssetNomme(Dictionary<string, object> release, string nom)
	{
		Dictionary<string, object> frais = JS().Deserialize<Dictionary<string, object>>(Appel("GET", Api + "/repos/Dreams9972/TPS-Studios/releases/" + release["id"], null, Jeton()));
		if (!(frais.TryGetValue("assets", out var value) && value is System.Collections.ArrayList))
		{
			return;
		}
		foreach (object item in (System.Collections.ArrayList)value)
		{
			if (item is Dictionary<string, object> a && string.Equals(a["name"] as string, nom, StringComparison.OrdinalIgnoreCase))
			{
				Appel("DELETE", Api + "/repos/Dreams9972/TPS-Studios/releases/assets/" + a["id"], null, Jeton());
			}
		}
	}

	/// <summary>Envoie un fichier dans une release et vérifie qu'il est bien arrivé entier. Renvoie son adresse de téléchargement.</summary>
	public static string EnvoyerAsset(Dictionary<string, object> release, string fichier, string nom, string sha256, Action<long, long> progres, Func<bool> annule)
	{
		string text = Jeton();
		string url = release["upload_url"] as string;
		int i = url.IndexOf('{');
		if (i > 0)
		{
			url = url.Substring(0, i);
		}
		long total = new FileInfo(fichier).Length;
		HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url + "?name=" + Uri.EscapeDataString(nom));
		req.Method = "POST";
		req.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
		req.Accept = "application/vnd.github+json";
		req.Headers["Authorization"] = "Bearer " + text;
		req.Headers["X-GitHub-Api-Version"] = "2022-11-28";
		req.ContentType = "application/zip";
		req.AllowWriteStreamBuffering = false;
		req.SendChunked = false;
		req.Timeout = 120000;
		req.ReadWriteTimeout = 900000;
		req.ContentLength = total;
		using (FileStream fs = File.OpenRead(fichier))
		{
			using Stream st = req.GetRequestStream();
			byte[] buf = new byte[1048576];
			long envoye = 0L;
			int n;
			while ((n = fs.Read(buf, 0, buf.Length)) > 0)
			{
				if (annule != null && annule())
				{
					req.Abort();
					throw new OperationCanceledException();
				}
				st.Write(buf, 0, n);
				envoye += n;
				progres?.Invoke(envoye, total);
			}
		}
		string input;
		try
		{
			using HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			using StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
			input = sr.ReadToEnd();
		}
		catch (WebException ex)
		{
			HttpWebResponse r2 = ex.Response as HttpWebResponse;
			throw new Exception("envoi de " + nom + " refusé" + ((r2 != null) ? (" (GitHub a répondu " + (int)r2.StatusCode + ")") : (" : " + ex.Message)));
		}
		Dictionary<string, object> asset = JS().Deserialize<Dictionary<string, object>>(input);
		long taille;
		long.TryParse(Convert.ToString(asset.ContainsKey("size") ? asset["size"] : null), out taille);
		if (taille != total)
		{
			throw new Exception(nom + " est arrivé incomplet sur GitHub (" + taille + " octets sur " + total + ")");
		}
		string digest = asset.ContainsKey("digest") ? (asset["digest"] as string) : null;
		if (!string.IsNullOrEmpty(digest) && !string.Equals(digest, "sha256:" + sha256, StringComparison.OrdinalIgnoreCase))
		{
			throw new Exception(nom + " est arrivé abîmé sur GitHub (empreinte différente)");
		}
		return asset["browser_download_url"] as string;
	}
}
