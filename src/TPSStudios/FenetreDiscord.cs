using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace TPSStudios;

/// <summary>
/// Bouton Discord de l'accueil : nombre de membres et de membres en ligne du serveur,
/// lus sur l'invitation publique (API Discord, sans connexion), rafraîchis toutes les
/// 45 secondes. Quand un chiffre change, il défile jusqu'à sa nouvelle valeur et une
/// flèche ▲ (verte) ou ▼ (rouge) apparaît quelques secondes.
/// </summary>
internal partial class Fenetre
{
	private int dcMembres = -1;

	private int dcEnLigne = -1;

	private double affMembres;

	private double affEnLigne;

	private int sensMembres;

	private int sensEnLigne;

	private DateTime quandMembres = DateTime.MinValue;

	private DateTime quandEnLigne = DateTime.MinValue;

	private System.Windows.Forms.Timer tSondageDc;

	private System.Windows.Forms.Timer tAnimDc;

	private bool sondageEnCours;

	private static readonly CultureInfo frFR = CultureInfo.GetCultureInfo("fr-FR");

	private void DemarrerCompteDiscord()
	{
		if (tAnimDc != null)
		{
			return;
		}
		tAnimDc = new System.Windows.Forms.Timer();
		tAnimDc.Interval = 16;
		tAnimDc.Tick += delegate
		{
			if (!tAnimDc.Enabled)
			{
				return;
			}
			AnimerCompteDiscord();
		};
		if (ModeCapture)
		{
			return;
		}
		tSondageDc = new System.Windows.Forms.Timer();
		tSondageDc.Interval = 45000;
		tSondageDc.Tick += delegate
		{
			SonderDiscord();
		};
		tSondageDc.Start();
		SonderDiscord();
	}

	private void SonderDiscord()
	{
		if (sondageEnCours || base.WindowState == FormWindowState.Minimized)
		{
			return;
		}
		sondageEnCours = true;
		string text = InviteDiscord();
		if (!Regex.IsMatch(text ?? "", "^https://(discord\\.gg|discord\\.com/invite)/[A-Za-z0-9-]{2,32}$"))
		{
			text = "https://discord.gg/dA77S5nXVV";
		}
		string code = text.Substring(text.LastIndexOf('/') + 1);
		Thread thread = new Thread((ThreadStart)delegate
		{
			int m = -1;
			int o = -1;
			try
			{
				using WebClient webClient = Web.Client();
				webClient.Encoding = Encoding.UTF8;
				string input = webClient.DownloadString("https://discord.com/api/v10/invites/" + code + "?with_counts=true");
				Dictionary<string, object> dictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(input);
				if (dictionary != null && dictionary.TryGetValue("approximate_member_count", out var value))
				{
					m = Convert.ToInt32(value);
				}
				if (dictionary != null && dictionary.TryGetValue("approximate_presence_count", out var value2))
				{
					o = Convert.ToInt32(value2);
				}
			}
			catch
			{
			}
			Sur(delegate
			{
				sondageEnCours = false;
				if (m >= 0)
				{
					MajCompteDiscord(m, o);
				}
			});
		});
		thread.IsBackground = true;
		thread.Start();
	}

	private void MajCompteDiscord(int m, int o)
	{
		DateTime now = DateTime.Now;
		if (dcMembres < 0)
		{
			affMembres = m;
			affEnLigne = Math.Max(0, o);
		}
		else
		{
			if (m != dcMembres)
			{
				sensMembres = Math.Sign(m - dcMembres);
				quandMembres = now;
			}
			if (o >= 0 && o != dcEnLigne)
			{
				sensEnLigne = Math.Sign(o - dcEnLigne);
				quandEnLigne = now;
			}
		}
		dcMembres = m;
		if (o >= 0)
		{
			dcEnLigne = o;
		}
		if (tAnimDc != null && !tAnimDc.Enabled)
		{
			derDc = chronoDc.ElapsedMilliseconds;
		}
		tAnimDc?.Start();
		InvaliderDiscord();
	}

	private readonly System.Diagnostics.Stopwatch chronoDc = System.Diagnostics.Stopwatch.StartNew();

	private long derDc;

	private void AnimerCompteDiscord()
	{
		bool flag = false;
		long ms = chronoDc.ElapsedMilliseconds;
		double k = Math.Max(0.2, Math.Min(4.0, (double)(ms - derDc) / 30.0));
		derDc = ms;
		affMembres = Rapprocher(affMembres, dcMembres, k, ref flag);
		affEnLigne = Rapprocher(affEnLigne, Math.Max(0, dcEnLigne), k, ref flag);
		DateTime now = DateTime.Now;
		bool flag2 = (now - quandMembres).TotalSeconds < 4.0 || (now - quandEnLigne).TotalSeconds < 4.0;
		if (!flag && !flag2)
		{
			tAnimDc.Stop();
		}
		InvaliderDiscord();
	}

	private static double Rapprocher(double v, int cible, double k, ref bool bouge)
	{
		double num = (double)cible - v;
		if (Math.Abs(num) < 0.5)
		{
			return cible;
		}
		bouge = true;
		double num2 = num * (1.0 - Math.Pow(0.86, k));
		if (Math.Abs(num2) < 0.35 * k)
		{
			num2 = Math.Sign(num) * Math.Min(Math.Abs(num), 0.35 * k);
		}
		return v + num2;
	}

	private void InvaliderDiscord()
	{
		if (vue == Vue.Accueil && rDiscord.Width > 0)
		{
			pAccueil.Invalidate(Rectangle.Inflate(rDiscord, Zx.P(8), Zx.P(30)));
		}
	}

	/// <summary>Chiffre animé + flèche de tendance, dessinés dans le bouton Discord.</summary>
	private void DessinerCompte(Graphics g, Rectangle r, double valeur, string suffixe, int sens, DateTime quand, Font f, Color c, bool centre)
	{
		string text = ((int)Math.Round(valeur)).ToString("N0", frFR) + suffixe;
		double totalSeconds = (DateTime.Now - quand).TotalSeconds;
		string text2 = "";
		Color color = c;
		if (sens != 0 && totalSeconds < 4.0)
		{
			text2 = ((sens > 0) ? " ▲" : " ▼");
			int num = (int)(255.0 * Math.Min(1.0, (4.0 - totalSeconds) / 1.2));
			color = Color.FromArgb(num, (sens > 0) ? Color.FromArgb(87, 242, 135) : Color.FromArgb(255, 110, 110));
			if (totalSeconds < 1.2)
			{
				c = Color.FromArgb((int)(c.R + (color.R - c.R) * (1.0 - totalSeconds / 1.2)), (int)(c.G + (color.G - c.G) * (1.0 - totalSeconds / 1.2)), (int)(c.B + (color.B - c.B) * (1.0 - totalSeconds / 1.2)));
			}
		}
		Size size = Lang.MeasureText(text, f);
		Size size2 = ((text2.Length > 0) ? Lang.MeasureText(text2, f) : Size.Empty);
		int num2 = (centre ? (r.X + (r.Width - size.Width - size2.Width) / 2) : r.X);
		int num3 = 0;
		if (sens != 0 && totalSeconds < 0.6)
		{
			num3 = (int)((double)(-sens * Zx.P(6)) * (1.0 - totalSeconds / 0.6));
		}
		Lang.DrawText(g, text, f, new Point(num2, r.Y + num3), c, TextFormatFlags.NoPadding);
		if (text2.Length > 0)
		{
			Lang.DrawText(g, text2, f, new Point(num2 + size.Width, r.Y + num3), color, TextFormatFlags.NoPadding);
		}
	}
}
