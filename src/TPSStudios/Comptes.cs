using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Comptes
{
	public static readonly Color[] Palette = new Color[8]
	{
		C.Jade,
		C.Bleu,
		C.Orange,
		C.Rouge,
		Color.FromArgb(150, 90, 220),
		Color.FromArgb(230, 90, 160),
		Color.FromArgb(90, 170, 60),
		Color.FromArgb(70, 190, 220)
	};

	private static DonneesComptes d;

	private static Dictionary<string, KeyValuePair<DateTime, Image>> imagesAvatar = new Dictionary<string, KeyValuePair<DateTime, Image>>();

	private static Dictionary<string, KeyValuePair<DateTime, Image>> imagesBanniere = new Dictionary<string, KeyValuePair<DateTime, Image>>();

	private static string Fichier
	{
		get
		{
			string environmentVariable = Environment.GetEnvironmentVariable("TPS_COMPTES");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				return environmentVariable;
			}
			return Path.Combine(Config.Dossier, "comptes.json");
		}
	}

	private static DonneesComptes D
	{
		get
		{
			if (d == null)
			{
				try
				{
					d = new JavaScriptSerializer().Deserialize<DonneesComptes>(File.ReadAllText(Fichier, Encoding.UTF8));
				}
				catch
				{
				}
				if (d == null || d.Comptes == null)
				{
					d = new DonneesComptes();
				}
			}
			return d;
		}
	}

	public static List<Compte> Tous => D.Comptes;

	public static bool EstAdmin
	{
		get
		{
			Compte actif = Actif;
			if (actif != null && actif.Admin)
			{
				return GitHub.JetonPresent;
			}
			return false;
		}
	}

	public static string ActifId => D.Actif;

	public static Compte Actif
	{
		get
		{
			foreach (Compte compte in D.Comptes)
			{
				if (compte.Id == D.Actif)
				{
					return compte;
				}
			}
			return null;
		}
	}

	private static void Sauver()
	{
		try
		{
			Disque.EcrireTexte(Fichier, new JavaScriptSerializer().Serialize(D));
		}
		catch (Exception e)
		{
			Journal.Une("profils", e);
		}
	}

	public static void DefinirAdmin(Compte c, bool oui)
	{
		c.Admin = oui;
		Sauver();
	}

	public static string Valider(string pseudo, string sauf)
	{
		pseudo = (pseudo ?? "").Trim();
		if (pseudo.Length < 2 || pseudo.Length > 20)
		{
			return "Le pseudo doit faire entre 2 et 20 caractères.";
		}
		if (!System.Text.RegularExpressions.Regex.IsMatch(pseudo, "[\\p{L}\\p{N}]"))
		{
			return "Le pseudo doit contenir au moins une lettre ou un chiffre.";
		}
		if (ModerationPseudo.Interdit(pseudo))
		{
			return "Ce pseudo n'est pas autorisé : insultes, vulgarités et propos haineux ou violents sont interdits.";
		}
		foreach (Compte compte in D.Comptes)
		{
			if (compte.Id != sauf && string.Equals(compte.Pseudo, pseudo, StringComparison.OrdinalIgnoreCase))
			{
				return "Ce pseudo existe déjà sur ce PC.";
			}
		}
		return null;
	}

	public static Compte Creer(string pseudo, int couleur)
	{
		Compte compte = new Compte();
		compte.Id = Guid.NewGuid().ToString("N");
		compte.Pseudo = pseudo.Trim();
		compte.Couleur = couleur;
		compte.Cree = DateTime.Now.ToString("dd/MM/yyyy");
		compte.Derniere = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
		D.Comptes.Add(compte);
		D.Actif = compte.Id;
		Sauver();
		return compte;
	}

	public static void Modifier(Compte c, string pseudo, int couleur)
	{
		c.Pseudo = pseudo.Trim();
		c.Couleur = couleur;
		Sauver();
	}

	public static void Choisir(string id)
	{
		foreach (Compte compte in D.Comptes)
		{
			if (compte.Id == id)
			{
				D.Actif = id;
				compte.Derniere = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
				Sauver();
				break;
			}
		}
	}

	public static void Supprimer(string id)
	{
		D.Comptes.RemoveAll((Compte c) => c.Id == id);
		if (D.Actif == id)
		{
			D.Actif = ((D.Comptes.Count > 0) ? D.Comptes[0].Id : "");
		}
		Sauver();
	}

	public static void Noter(string projet)
	{
		Compte actif = Actif;
		if (actif != null && !string.IsNullOrEmpty(projet))
		{
			actif.Lancements.TryGetValue(projet, out var value);
			actif.Lancements[projet] = value + 1;
			actif.Dernier[projet] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
			Sauver();
		}
	}

	public static void Sauvegarder()
	{
		Sauver();
	}

	public static string FichierAvatar(Compte c)
	{
		return Path.Combine(Config.Dossier, "avatars", c.Id + ".png");
	}

	public static Image ImageAvatar(Compte c)
	{
		if (c == null)
		{
			return null;
		}
		string path = FichierAvatar(c);
		if (!File.Exists(path))
		{
			OublierImage(c.Id);
			return null;
		}
		DateTime lastWriteTime = File.GetLastWriteTime(path);
		if (imagesAvatar.TryGetValue(c.Id, out var value) && value.Key == lastWriteTime)
		{
			return value.Value;
		}
		try
		{
			using FileStream stream = File.OpenRead(path);
			using Image original = Image.FromStream(stream);
			Bitmap bitmap = new Bitmap(original);
			OublierImage(c.Id);
			imagesAvatar[c.Id] = new KeyValuePair<DateTime, Image>(lastWriteTime, bitmap);
			return bitmap;
		}
		catch
		{
			return null;
		}
	}

	private static void OublierImage(string id)
	{
		if (imagesAvatar.TryGetValue(id, out var _))
		{
			imagesAvatar.Remove(id);
		}
	}

	public static void DefinirAvatar(Compte c, string source)
	{
		if (new FileInfo(source).Length > 10485760)
		{
			throw new Exception("image trop lourde (10 Mo maximum)");
		}
		using (Image image = Image.FromFile(source))
		{
			if (image.Width < 32 || image.Height < 32)
			{
				throw new Exception("image trop petite");
			}
			int num = Math.Min(image.Width, image.Height);
			Rectangle srcRect = new Rectangle((image.Width - num) / 2, (image.Height - num) / 2, num, num);
			string text = FichierAvatar(c);
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			string text2 = text + ".tmp";
			using (Bitmap bitmap = new Bitmap(256, 256))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
					graphics.DrawImage(image, new Rectangle(0, 0, Zx.P(256), Zx.P(256)), srcRect, GraphicsUnit.Pixel);
				}
				bitmap.Save(text2, ImageFormat.Png);
			}
			if (File.Exists(text))
			{
				File.Replace(text2, text, null);
			}
			else
			{
				File.Move(text2, text);
			}
		}
		OublierImage(c.Id);
	}

	public static string FichierBanniere(Compte c)
	{
		return Path.Combine(Config.Dossier, "avatars", c.Id + "_banniere.jpg");
	}

	public static Image ImageBanniere(Compte c)
	{
		if (c == null)
		{
			return null;
		}
		string path = FichierBanniere(c);
		if (!File.Exists(path))
		{
			imagesBanniere.Remove(c.Id);
			return null;
		}
		DateTime lastWriteTime = File.GetLastWriteTime(path);
		if (imagesBanniere.TryGetValue(c.Id, out var value) && value.Key == lastWriteTime)
		{
			return value.Value;
		}
		try
		{
			using FileStream stream = File.OpenRead(path);
			using Image original = Image.FromStream(stream);
			Bitmap bitmap = new Bitmap(original);
			imagesBanniere[c.Id] = new KeyValuePair<DateTime, Image>(lastWriteTime, bitmap);
			return bitmap;
		}
		catch
		{
			return null;
		}
	}

	public static Image ReduireBanniere(Image brut)
	{
		double num = Math.Min(1.0, 1600.0 / (double)brut.Width);
		int width = Math.Max(1, (int)((double)brut.Width * num));
		int height = Math.Max(1, (int)((double)brut.Height * num));
		Bitmap bitmap = new Bitmap(width, height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.SmoothingMode = SmoothingMode.HighQuality;
		graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
		graphics.DrawImage(brut, 0, 0, width, height);
		return bitmap;
	}

	public static void DefinirBanniereImage(Compte c, string source)
	{
		if (new FileInfo(source).Length > 15728640)
		{
			throw new Exception("image trop lourde (15 Mo maximum)");
		}
		using (Image image = Image.FromFile(source))
		{
			if (image.Width < 400 || image.Height < 100)
			{
				throw new Exception("image trop petite (400×100 minimum)");
			}
			string text = FichierBanniere(c);
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			string text2 = text + ".tmp";
			using (Image image2 = ReduireBanniere(image))
			{
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
				encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
				image2.Save(text2, encoder, encoderParameters);
			}
			if (File.Exists(text))
			{
				File.Replace(text2, text, null);
			}
			else
			{
				File.Move(text2, text);
			}
		}
		imagesBanniere.Remove(c.Id);
	}

	public static void RetirerBanniereImage(Compte c)
	{
		try
		{
			File.Delete(FichierBanniere(c));
		}
		catch
		{
		}
		imagesBanniere.Remove(c.Id);
	}

	public static List<Color> CouleursPhoto(Image im)
	{
		List<Color> list = new List<Color>();
		if (im == null)
		{
			return list;
		}
		double[] poids = new double[216];
		double[] array = new double[216];
		double[] array2 = new double[216];
		double[] array3 = new double[216];
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		using (Bitmap bitmap = new Bitmap(48, 48))
		{
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(im, 0, 0, Zx.P(48), Zx.P(48));
			}
			for (int i = 0; i < 48; i++)
			{
				for (int j = 0; j < 48; j++)
				{
					Color pixel = bitmap.GetPixel(j, i);
					if (pixel.A >= 128)
					{
						SelecteurCouleur.ToHsv(pixel, out var h, out var s, out var v);
						num += (double)(int)pixel.R;
						num2 += (double)(int)pixel.G;
						num3 += (double)(int)pixel.B;
						num4 += 1.0;
						if (!(v < 0.14) && (!(s < 0.12) || !(v > 0.88)))
						{
							double num5 = 0.25 + s * 1.2;
							int num6 = (int)(h / 15.0) % 24 * 9 + Math.Min(2, (int)(s * 3.0)) * 3 + Math.Min(2, (int)(v * 3.0));
							poids[num6] += num5;
							array[num6] += (double)(int)pixel.R * num5;
							array2[num6] += (double)(int)pixel.G * num5;
							array3[num6] += (double)(int)pixel.B * num5;
						}
					}
				}
			}
		}
		List<int> list2 = new List<int>();
		for (int k = 0; k < poids.Length; k++)
		{
			if (poids[k] > 0.0)
			{
				list2.Add(k);
			}
		}
		list2.Sort((int a, int c2) => poids[c2].CompareTo(poids[a]));
		foreach (int item2 in list2)
		{
			Color item = Color.FromArgb((int)(array[item2] / poids[item2]), (int)(array2[item2] / poids[item2]), (int)(array3[item2] / poids[item2]));
			bool flag = false;
			foreach (Color item3 in list)
			{
				int num7 = item.R - item3.R;
				int num8 = item.G - item3.G;
				int num9 = item.B - item3.B;
				if (num7 * num7 + num8 * num8 + num9 * num9 < 3600)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(item);
			}
			if (list.Count >= 5)
			{
				break;
			}
		}
		if (list.Count == 0 && num4 > 0.0)
		{
			list.Add(Color.FromArgb((int)(num / num4), (int)(num2 / num4), (int)(num3 / num4)));
		}
		return list;
	}

	public static Color Eclaircir(Color c)
	{
		SelecteurCouleur.ToHsv(c, out var h, out var s, out var v);
		return SelecteurCouleur.FromHsv(h, Math.Max(s, 0.45), Math.Max(0.62, Math.Min(0.95, v)));
	}

	public static void DegradeDepuis(List<Color> pal, out Color c1, out Color c2)
	{
		c1 = Eclaircir(pal[0]);
		double h;
		double s;
		double v;
		if (pal.Count > 1)
		{
			SelecteurCouleur.ToHsv(pal[1], out h, out s, out v);
			c2 = SelecteurCouleur.FromHsv(h, Math.Max(s, 0.5), Math.Min(v, 0.5));
		}
		else
		{
			SelecteurCouleur.ToHsv(c1, out h, out s, out v);
			c2 = SelecteurCouleur.FromHsv(h + 28.0, Math.Max(s, 0.55), 0.38);
		}
	}

	public static bool AppliquerCouleursPhoto(Compte c)
	{
		List<Color> list = CouleursPhoto(ImageAvatar(c));
		if (list.Count == 0)
		{
			return false;
		}
		DegradeDepuis(list, out var c2, out var c3);
		c.CouleurPerso = c2.ToArgb();
		if (c.BanType != "image")
		{
			c.BanType = "degrade";
			c.BanC1 = c2.ToArgb();
			c.BanC2 = c3.ToArgb();
			c.BanAngle = 45;
		}
		Sauver();
		return true;
	}

	public static void RetirerAvatar(Compte c)
	{
		try
		{
			File.Delete(FichierAvatar(c));
		}
		catch
		{
		}
		OublierImage(c.Id);
	}

	public static Color CouleurAvatar(Compte c)
	{
		if (c != null)
		{
			if (c.CouleurPerso == 0)
			{
				return Palette[Math.Abs(c.Couleur) % Palette.Length];
			}
			return Color.FromArgb(c.CouleurPerso);
		}
		return Palette[0];
	}

	public static void Avatar(Graphics g, Compte c, Rectangle r, float taille)
	{
		AvatarBrut(g, r, ImageAvatar(c), (c != null) ? c.Pseudo : "", CouleurAvatar(c), taille);
	}

	public static void AvatarBrut(Graphics g, Rectangle r, Image photo, string pseudo, Color couleur, float taille)
	{
		if (photo != null)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			GraphicsState gstate = g.Save();
			using (GraphicsPath graphicsPath = new GraphicsPath())
			{
				graphicsPath.AddEllipse(r);
				g.SetClip(graphicsPath, CombineMode.Intersect);
				g.DrawImage(photo, r);
			}
			g.Restore(gstate);
			return;
		}
		g.SmoothingMode = SmoothingMode.AntiAlias;
		using (SolidBrush brush = new SolidBrush(couleur))
		{
			g.FillEllipse(brush, r);
		}
		string t = ((!string.IsNullOrEmpty(pseudo)) ? pseudo.Substring(0, 1).ToUpperInvariant() : "?");
		using Font f = new Font("Segoe UI Semibold", taille);
		Lang.DrawText(g, t, f, r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
	}
}
