using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace TPSStudios;

/// <summary>Communauté : photos et vidéos publiées par les joueurs (visibles après validation par l'équipe).</summary>
internal static partial class Social
{
	internal class Publication
	{
		public long Id;

		public string Type = "photo";

		public string Titre = "";

		public string Mod = "";

		public string Mime = "";

		public long Taille;

		public string Statut = "ok";

		public long Cree;

		public string Auteur = "";

		public string AuteurId = "";

		public int Niveau = 1;

		public int Likes;

		public bool Aime;

		public bool Moi;

		public bool EstVideo => Type == "video";
	}

	internal class PageCommunaute
	{
		public List<Publication> Publications = new List<Publication>();

		public int Attente;

		public bool Modo;
	}

	/// <summary>Liste : « recentes », « populaires », « miennes » ou « attente » (équipe).</summary>
	public static PageCommunaute Communaute(string vue, long avant = 0, int limite = 30)
	{
		int st;
		Dictionary<string, object> r = AppelAuth("GET", "/v1/communaute?vue=" + vue + "&limite=" + limite + ((avant > 0) ? ("&avant=" + avant) : ""), null, out st);
		if (st != 200 || r == null)
		{
			throw new Exception(Msg(r, (st == 404) ? "la communauté n'est pas encore activée sur le serveur" : "communauté indisponible"));
		}
		PageCommunaute p = new PageCommunaute();
		if (r.TryGetValue("publications", out var v) && v is IEnumerable l && !(v is string))
		{
			foreach (object o in l)
			{
				if (o is Dictionary<string, object> d)
				{
					p.Publications.Add(new Publication
					{
						Id = Long(Val(d, "id")),
						Type = Convert.ToString(Val(d, "type") ?? "photo"),
						Titre = Convert.ToString(Val(d, "titre") ?? ""),
						Mod = Convert.ToString(Val(d, "mod") ?? ""),
						Mime = Convert.ToString(Val(d, "mime") ?? ""),
						Taille = Long(Val(d, "taille")),
						Statut = Convert.ToString(Val(d, "statut") ?? "ok"),
						Cree = Long(Val(d, "cree")),
						Auteur = Convert.ToString(Val(d, "auteur") ?? ""),
						AuteurId = Convert.ToString(Val(d, "auteur_id") ?? ""),
						Niveau = (int)Math.Max(1L, Long(Val(d, "niveau"))),
						Likes = (int)Long(Val(d, "likes")),
						Aime = Val(d, "aime") is bool b && b,
						Moi = Val(d, "moi") is bool b2 && b2
					});
				}
			}
		}
		p.Attente = (int)Long(Val(r, "attente"));
		p.Modo = Val(r, "modo") is bool m && m;
		return p;
	}

	private static object Val(Dictionary<string, object> d, string k)
	{
		if (d == null || !d.TryGetValue(k, out var v))
		{
			return null;
		}
		return v;
	}

	/// <summary>Adresse du fichier d'une publication (lisible sans connexion une fois validée).</summary>
	public static string MediaUrl(long id)
	{
		return Base + "/v1/communaute/media/" + id;
	}

	/// <summary>Télécharge le fichier d'une publication (avec le jeton : l'auteur et l'équipe voient aussi celles en attente).</summary>
	public static byte[] Media(long id, long max = 12L * 1024 * 1024)
	{
		HttpWebRequest q = (HttpWebRequest)WebRequest.Create(MediaUrl(id));
		q.Timeout = 20000;
		q.ReadWriteTimeout = 20000;
		q.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
		if (jeton.Length > 0)
		{
			q.Headers["Authorization"] = "Bearer " + jeton;
		}
		using HttpWebResponse rep = (HttpWebResponse)q.GetResponse();
		using Stream s = rep.GetResponseStream();
		using MemoryStream ms = new MemoryStream();
		byte[] tampon = new byte[65536];
		int n;
		while ((n = s.Read(tampon, 0, tampon.Length)) > 0)
		{
			ms.Write(tampon, 0, n);
			if (ms.Length > max)
			{
				throw new Exception("fichier trop lourd");
			}
		}
		return ms.ToArray();
	}

	/// <summary>Télécharge une vidéo dans un fichier (aperçu d'une vidéo en attente, lecture hors ligne).</summary>
	public static void MediaVersFichier(long id, string chemin, Action<double> progression)
	{
		HttpWebRequest q = (HttpWebRequest)WebRequest.Create(MediaUrl(id));
		q.Timeout = 30000;
		q.ReadWriteTimeout = 60000;
		q.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
		if (jeton.Length > 0)
		{
			q.Headers["Authorization"] = "Bearer " + jeton;
		}
		using HttpWebResponse rep = (HttpWebResponse)q.GetResponse();
		long total = rep.ContentLength;
		using Stream s = rep.GetResponseStream();
		using FileStream f = File.Create(chemin);
		byte[] tampon = new byte[131072];
		long lu = 0L;
		int n;
		while ((n = s.Read(tampon, 0, tampon.Length)) > 0)
		{
			f.Write(tampon, 0, n);
			lu += n;
			if (total > 0)
			{
				progression?.Invoke((double)lu / (double)total);
			}
		}
	}

	public const long PhotoMax = 10L * 1024 * 1024;

	public const long VideoMax = 90L * 1024 * 1024;

	/// <summary>Type MIME accepté d'après l'extension (null : format refusé).</summary>
	public static string MimeDe(string chemin)
	{
		switch (Path.GetExtension(chemin ?? "").ToLowerInvariant())
		{
		case ".jpg":
		case ".jpeg":
			return "image/jpeg";
		case ".png":
			return "image/png";
		case ".webp":
			return "image/webp";
		case ".mp4":
		case ".m4v":
			return "video/mp4";
		case ".webm":
			return "video/webm";
		default:
			return null;
		}
	}

	/// <summary>
	/// Photo prête à envoyer : réduite à 2560 px au plus et enregistrée en JPEG (qualité 90) si elle est grande ou lourde ;
	/// les photos légères (moins de 3 Mo et 2560 px) partent telles quelles.
	/// </summary>
	public static byte[] PreparerPhoto(string chemin, out string mime)
	{
		mime = MimeDe(chemin);
		byte[] brut = File.ReadAllBytes(chemin);
		if (mime == "image/webp")
		{
			return brut;
		}
		using MemoryStream ms0 = new MemoryStream(brut);
		using Image im = Image.FromStream(ms0);
		int max = 2560;
		if (brut.Length <= 3 * 1024 * 1024 && im.Width <= max && im.Height <= max)
		{
			return brut;
		}
		double k = Math.Min(1.0, (double)max / (double)Math.Max(im.Width, im.Height));
		int w = Math.Max(1, (int)(im.Width * k));
		int h = Math.Max(1, (int)(im.Height * k));
		using Bitmap b = new Bitmap(w, h, PixelFormat.Format24bppRgb);
		using (Graphics g = Graphics.FromImage(b))
		{
			g.Clear(Color.Black);
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.DrawImage(im, 0, 0, w, h);
		}
		ImageCodecInfo codec = Array.Find(ImageCodecInfo.GetImageEncoders(), (ImageCodecInfo c) => c.MimeType == "image/jpeg");
		using EncoderParameters ep = new EncoderParameters(1);
		ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
		using MemoryStream ms = new MemoryStream();
		b.Save(ms, codec, ep);
		mime = "image/jpeg";
		return ms.ToArray();
	}

	/// <summary>Envoie une photo ou une vidéo. Renvoie le statut (« attente » ou « ok » pour l'équipe).</summary>
	public static string Publier(string chemin, string titre, string mod, Action<double> progression)
	{
		string mime = MimeDe(chemin);
		if (mime == null)
		{
			throw new Exception("format non accepté : photos JPG, PNG ou WEBP, vidéos MP4 ou WEBM");
		}
		bool video = mime.StartsWith("video/");
		byte[] photo = null;
		long taille;
		if (video)
		{
			taille = new FileInfo(chemin).Length;
			if (taille > VideoMax)
			{
				throw new Exception("vidéo trop lourde : 90 Mo au plus");
			}
		}
		else
		{
			photo = PreparerPhoto(chemin, out mime);
			taille = photo.Length;
			if (taille > PhotoMax)
			{
				throw new Exception("photo trop lourde : 10 Mo au plus");
			}
		}
		for (int essai = 0; essai < 2; essai++)
		{
			HttpWebRequest q = (HttpWebRequest)WebRequest.Create(Base + "/v1/communaute?titre=" + Uri.EscapeDataString(titre ?? "") + "&mod=" + Uri.EscapeDataString(mod ?? ""));
			q.Method = "POST";
			q.ContentType = mime;
			q.ContentLength = taille;
			q.AllowWriteStreamBuffering = false;
			q.SendChunked = false;
			q.Timeout = 60000;
			q.ReadWriteTimeout = 120000;
			q.UserAgent = "TPS-Studios-Lanceur/" + Config.VERSION;
			q.Headers["Authorization"] = "Bearer " + jeton;
			using (Stream sortie = q.GetRequestStream())
			{
				Stream entree = (photo != null) ? (Stream)new MemoryStream(photo) : File.OpenRead(chemin);
				using (entree)
				{
					byte[] tampon = new byte[131072];
					long envoye = 0L;
					int n;
					while ((n = entree.Read(tampon, 0, tampon.Length)) > 0)
					{
						sortie.Write(tampon, 0, n);
						envoye += n;
						progression?.Invoke((double)envoye / (double)taille);
					}
				}
			}
			int st;
			string texte;
			try
			{
				using HttpWebResponse rep = (HttpWebResponse)q.GetResponse();
				st = (int)rep.StatusCode;
				texte = new StreamReader(rep.GetResponseStream(), Encoding.UTF8).ReadToEnd();
			}
			catch (WebException ex) when (ex.Response is HttpWebResponse r2)
			{
				st = (int)r2.StatusCode;
				texte = new StreamReader(r2.GetResponseStream(), Encoding.UTF8).ReadToEnd();
			}
			Dictionary<string, object> d = null;
			try
			{
				d = new JavaScriptSerializer().DeserializeObject(texte) as Dictionary<string, object>;
			}
			catch
			{
			}
			if (st == 401 && essai == 0 && pseudoSouhaite.Length > 0)
			{
				jeton = "";
				string err = Inscrire(pseudoSouhaite);
				if (err != null)
				{
					throw new Exception(err);
				}
				continue;
			}
			if (st != 201)
			{
				throw new Exception(Msg(d, (st == 404) ? "la communauté n'est pas encore activée sur le serveur" : "publication impossible"));
			}
			return Convert.ToString(Val(d, "statut") ?? "attente");
		}
		throw new Exception("publication impossible");
	}

	public static int AimerPublication(long id, out bool aime)
	{
		int st;
		Dictionary<string, object> d = AppelAuth("POST", "/v1/communaute/" + id + "/aimer", new Dictionary<string, object>(), out st);
		if (st != 200 || d == null)
		{
			throw new Exception(Msg(d, "J'aime impossible"));
		}
		aime = Val(d, "aime") is bool b && b;
		return (int)Long(Val(d, "likes"));
	}

	/// <summary>Équipe : « valider » ou « refuser » une publication en attente.</summary>
	public static string Moderer(long id, string action)
	{
		int st;
		Dictionary<string, object> d = AppelAuth("POST", "/v1/communaute/" + id + "/moderer", new Dictionary<string, object> { { "action", action } }, out st);
		if (st != 200)
		{
			return Msg(d, "action impossible");
		}
		return null;
	}

	public static string SupprimerPublication(long id)
	{
		int st;
		Dictionary<string, object> d = AppelAuth("DELETE", "/v1/communaute/" + id, null, out st);
		if (st != 200)
		{
			return Msg(d, "suppression impossible");
		}
		return null;
	}
}
