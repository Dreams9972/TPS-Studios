using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Lang
{
	public static readonly string[] Codes = new string[7] { "fr", "en", "de", "es", "it", "pl", "pt" };

	public static readonly string[] Noms = new string[7] { "Français", "English", "Deutsch", "Español", "Italiano", "Polski", "Português" };

	private static string code;

	private static Dictionary<string, string> exact;

	private static Regex frag;

	private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();

	private static readonly object verrou = new object();

	private static bool enCours;

	public static string Code
	{
		get
		{
			if (code == null)
			{
				string value = Environment.GetEnvironmentVariable("TPS_LANGUE");
				if (string.IsNullOrEmpty(value))
				{
					value = Config.Lire("langue");
				}
				if (string.IsNullOrEmpty(value) || Array.IndexOf(Codes, value) < 0)
				{
					string twoLetterISOLanguageName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
					value = ((Array.IndexOf(Codes, twoLetterISOLanguageName) >= 0) ? twoLetterISOLanguageName : "fr");
				}
				code = value;
			}
			return code;
		}
	}

	public static bool Francais => Code == "fr";

	public static void Definir(string c)
	{
		if (Array.IndexOf(Codes, c) >= 0)
		{
			Config.Ecrire("langue", c);
		}
	}

	private static void Charger()
	{
		exact = new Dictionary<string, string>();
		try
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("langues.json");
			if (stream != null)
			{
				using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
				JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
				javaScriptSerializer.MaxJsonLength = 50000000;
				if (javaScriptSerializer.DeserializeObject(streamReader.ReadToEnd()) is Dictionary<string, object> dictionary && dictionary.TryGetValue(Code, out var value) && value is Dictionary<string, object>)
				{
					foreach (KeyValuePair<string, object> item in (Dictionary<string, object>)value)
					{
						exact[item.Key] = Convert.ToString(item.Value);
					}
				}
			}
		}
		catch
		{
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> item2 in exact)
		{
			if (item2.Key.Trim().Length >= 3 && item2.Key != item2.Value && item2.Key.IndexOf('{') < 0)
			{
				list.Add(item2.Key);
			}
		}
		list.Sort((string a, string b) => b.Length.CompareTo(a.Length));
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item3 in list)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('|');
			}
			string text = Regex.Escape(item3);
			if (char.IsLetterOrDigit(item3[0]))
			{
				text = "(?<![\\p{L}\\p{N}])" + text;
			}
			if (char.IsLetterOrDigit(item3[item3.Length - 1]))
			{
				text += "(?![\\p{L}\\p{N}])";
			}
			stringBuilder.Append(text);
		}
		try
		{
			frag = ((stringBuilder.Length > 0) ? new Regex(stringBuilder.ToString(), RegexOptions.Compiled | RegexOptions.CultureInvariant) : null);
		}
		catch
		{
			frag = null;
		}
	}

	public static string T(string s)
	{
		if (string.IsNullOrEmpty(s) || Code == "fr")
		{
			return s;
		}
		lock (verrou)
		{
			if (exact == null)
			{
				Charger();
			}
			if (exact.TryGetValue(s, out var value))
			{
				return value;
			}
			if (cache.TryGetValue(s, out value))
			{
				return value;
			}
			value = ((frag != null && s.Length <= 240) ? frag.Replace(s, (Match m) => (!exact.TryGetValue(m.Value, out var value2)) ? m.Value : value2) : s);
			if (cache.Count > 5000)
			{
				cache.Clear();
			}
			cache[s] = value;
			return value;
		}
	}

	public static void DrawText(IDeviceContext dc, string t, Font f, Point p, Color c)
	{
		TextRenderer.DrawText(dc, T(t), f, p, c);
	}

	public static void DrawText(IDeviceContext dc, string t, Font f, Point p, Color c, TextFormatFlags fl)
	{
		TextRenderer.DrawText(dc, T(t), f, p, c, fl);
	}

	public static void DrawText(IDeviceContext dc, string t, Font f, Point p, Color c, Color fond)
	{
		TextRenderer.DrawText(dc, T(t), f, p, c, fond);
	}

	public static void DrawText(IDeviceContext dc, string t, Font f, Rectangle r, Color c)
	{
		TextRenderer.DrawText(dc, T(t), f, r, c);
	}

	public static void DrawText(IDeviceContext dc, string t, Font f, Rectangle r, Color c, TextFormatFlags fl)
	{
		TextRenderer.DrawText(dc, T(t), f, r, c, fl);
	}

	public static void DrawText(IDeviceContext dc, string t, Font f, Rectangle r, Color c, Color fond, TextFormatFlags fl)
	{
		TextRenderer.DrawText(dc, T(t), f, r, c, fond, fl);
	}

	public static Size MeasureText(string t, Font f)
	{
		return TextRenderer.MeasureText(T(t), f);
	}

	public static Size MeasureText(string t, Font f, Size s)
	{
		return TextRenderer.MeasureText(T(t), f, s);
	}

	public static Size MeasureText(string t, Font f, Size s, TextFormatFlags fl)
	{
		return TextRenderer.MeasureText(T(t), f, s, fl);
	}

	public static Size MeasureText(IDeviceContext dc, string t, Font f)
	{
		return TextRenderer.MeasureText(dc, T(t), f);
	}

	public static Size MeasureText(IDeviceContext dc, string t, Font f, Size s, TextFormatFlags fl)
	{
		return TextRenderer.MeasureText(dc, T(t), f, s, fl);
	}

	public static void Traduire(Control c)
	{
		if (Francais || c == null)
		{
			return;
		}
		Label i = c as Label;
		if (i != null)
		{
			Etiquette(i);
			i.TextChanged += delegate
			{
				Etiquette(i);
			};
		}
		c.ControlAdded += delegate(object o, ControlEventArgs e)
		{
			Traduire(e.Control);
		};
		foreach (Control control in c.Controls)
		{
			Traduire(control);
		}
	}

	private static void Etiquette(Label l)
	{
		if (enCours)
		{
			return;
		}
		string text = T(l.Text);
		if (!(text != l.Text))
		{
			return;
		}
		enCours = true;
		try
		{
			l.Text = text;
		}
		finally
		{
			enCours = false;
		}
	}
}
