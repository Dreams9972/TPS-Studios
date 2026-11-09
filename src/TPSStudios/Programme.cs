using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal static class Programme
{
	private static Mutex Verrou;

	private static bool verrouPris;

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int RegisterWindowMessage(string nom);

	[DllImport("user32.dll")]
	private static extern bool PostMessage(IntPtr h, int msg, IntPtr w, IntPtr l);

	public static void Liberer()
	{
		if (Verrou != null && verrouPris)
		{
			try
			{
				Verrou.ReleaseMutex();
			}
			catch
			{
			}
			verrouPris = false;
		}
	}

	private static bool SInstaller()
	{
		string text = Path.Combine(Config.Dossier, "TPS Studios.exe");
		if (string.Equals(Path.GetFullPath(Application.ExecutablePath), Path.GetFullPath(text), StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		Directory.CreateDirectory(Config.Dossier);
		if (File.Exists(text))
		{
			try
			{
				if (File.Exists(text + ".old"))
				{
					File.Delete(text + ".old");
				}
				File.Move(text, text + ".old");
			}
			catch
			{
			}
		}
		File.Copy(Application.ExecutablePath, text, overwrite: true);
		Raccourci(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "TPS Studios.lnk"), text);
		Raccourci(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "TPS Studios.lnk"), text);
		Process.Start(text, "/portable");
		return true;
	}

	private static void Raccourci(string lnkF, string cible)
	{
		try
		{
			Type typeFromProgID = Type.GetTypeFromProgID("WScript.Shell");
			object target = Activator.CreateInstance(typeFromProgID);
			object obj = typeFromProgID.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, target, new object[1] { lnkF });
			Type type = obj.GetType();
			type.InvokeMember("TargetPath", BindingFlags.SetProperty, null, obj, new object[1] { cible });
			type.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, obj, new object[1] { Path.GetDirectoryName(cible) });
			type.InvokeMember("Description", BindingFlags.SetProperty, null, obj, new object[1] { "TPS Studios : projets, mises à jour et lancement" });
			type.InvokeMember("IconLocation", BindingFlags.SetProperty, null, obj, new object[1] { cible + ",0" });
			type.InvokeMember("Save", BindingFlags.InvokeMethod, null, obj, null);
		}
		catch
		{
		}
	}

	[STAThread]
	private static int Main(string[] args)
	{
		WebView2Embarque.Init();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Zx.Init();
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		bool prevenu = false;
		Application.ThreadException += delegate(object o, ThreadExceptionEventArgs e)
		{
			Journal.Ecrire("interface", e.Exception);
			if (!prevenu && !Fenetre.ModeCapture)
			{
				prevenu = true;
				MessageBox.Show("Une erreur inattendue s'est produite. Le lanceur continue de fonctionner.\n\nLe détail est enregistré dans :\n" + Path.Combine(Config.Dossier, "erreurs.log"), "TPS Studios");
			}
		};
		AppDomain.CurrentDomain.UnhandledException += delegate(object o, UnhandledExceptionEventArgs e)
		{
			Journal.Ecrire("fatale", e.ExceptionObject as Exception);
		};
		try
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
			ServicePointManager.DefaultConnectionLimit = 8;
			ServicePointManager.Expect100Continue = false;
		}
		catch
		{
		}
		List<string> a = new List<string>(args);
		int num = a.IndexOf("/catalogue");
		if (num >= 0 && num + 1 < a.Count)
		{
			Config.Catalogue = a[num + 1];
			a.RemoveRange(num, 2);
		}
		string environmentVariable = Environment.GetEnvironmentVariable("TPS_CATALOGUE");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			Config.Catalogue = environmentVariable;
		}
		num = a.IndexOf("/action");
		if (num >= 0 && num + 1 < a.Count)
		{
			Fenetre.ActionAuto = a[num + 1];
			a.RemoveRange(num, 2);
		}
		num = a.IndexOf("/rouvrir");
		if (num >= 0 && num + 1 < a.Count)
		{
			if (int.TryParse(a[num + 1], out var result))
			{
				Fenetre.Rouvrir = result;
			}
			a.RemoveRange(num, 2);
		}
		num = a.IndexOf("/plein");
		if (num >= 0)
		{
			Fenetre.PleinAuDemarrage = true;
			a.RemoveAt(num);
		}
		try
		{
			string path = Application.ExecutablePath + ".old";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch
		{
		}
		if (a.Count >= 7 && a[0] == "/installer-admin")
		{
			string fullPath = Path.GetFullPath(Config.Cache);
			try
			{
				string[] array = a[1].Split('|');
				string[] array2 = a[6].Split('|');
				if (array.Length != array2.Length || array.Length > 40)
				{
					throw new Exception("paquet invalide");
				}
				string[] array3 = array;
				foreach (string chemin in array3)
				{
					if (!Omsi.Sous(chemin, fullPath))
					{
						throw new Exception("fichier hors du cache du lanceur");
					}
				}
				if (!Omsi.Sous(a[5], fullPath))
				{
					throw new Exception("fichier hors du cache du lanceur");
				}
				if (!File.Exists(Path.Combine(a[2], "Omsi.exe")))
				{
					throw new Exception("dossier d'OMSI 2 invalide");
				}
				if (a[3] != "-" && !File.Exists(Path.Combine(a[3], "openomsi.exe")))
				{
					throw new Exception("dossier d'openOMSI invalide");
				}
				if (!a[4].EndsWith("_fichiers_installes.txt", StringComparison.OrdinalIgnoreCase) || a[4].IndexOfAny(new char[3] { '\\', '/', ':' }) >= 0)
				{
					throw new Exception("liste invalide");
				}
				for (int j = 0; j < array.Length; j++)
				{
					if (array2[j].Length != 64 || !string.Equals(Web.Sha256(array[j]), array2[j], StringComparison.OrdinalIgnoreCase))
					{
						throw new Exception("empreinte du fichier incorrecte");
					}
				}
				Projet projet = new Projet();
				projet.Liste = a[4];
				Omsi.Installer(projet, array, a[2], (a[3] == "-") ? null : a[3], null);
				File.WriteAllText(a[5], "OK", Encoding.UTF8);
				return 0;
			}
			catch (Exception ex)
			{
				Journal.Ecrire("installer-admin", ex);
				try
				{
					if (Omsi.Sous(a[5], fullPath))
					{
						File.WriteAllText(a[5], "L'installation a échoué : " + ex.Message, Encoding.UTF8);
					}
				}
				catch
				{
				}
				return 1;
			}
		}
		if (a.Count >= 4 && a[0] == "/desinstaller-admin")
		{
			try
			{
				if (!Regex.IsMatch(a[1], "^[a-z0-9_-]{1,40}$"))
				{
					throw new Exception("identifiant invalide");
				}
				if (!File.Exists(Path.Combine(a[3], "Omsi.exe")))
				{
					throw new Exception("dossier d'OMSI 2 invalide");
				}
				if (!a[2].EndsWith("_fichiers_installes.txt", StringComparison.OrdinalIgnoreCase) || a[2].IndexOfAny(new char[3] { '\\', '/', ':' }) >= 0)
				{
					throw new Exception("liste invalide");
				}
				Projet projet2 = new Projet();
				projet2.Id = a[1];
				projet2.Liste = a[2];
				Omsi.Desinstaller(projet2, a[3]);
				return 0;
			}
			catch (Exception e2)
			{
				Journal.Ecrire("desinstaller-admin", e2);
				return 1;
			}
		}
		if (a.Count >= 4 && a[0] == "/installer")
		{
			string path2 = Path.Combine(a[2], "_resultat_test.txt");
			try
			{
				string erreur;
				Catalogue catalogue = Catalogue.Charger(out erreur);
				Projet projet3 = catalogue.Projets.Find((Projet x) => x.Id == a[1]);
				string text = Path.Combine(Config.Cache, projet3.Id + "_test.zip");
				if (File.Exists(projet3.Zip))
				{
					File.Copy(projet3.Zip, text, overwrite: true);
				}
				else
				{
					Web.Telecharger(projet3.Zip, text);
				}
				if (projet3.Sha256.Length > 0 && Web.Sha256(text) != projet3.Sha256)
				{
					throw new Exception("sha256 different");
				}
				Omsi.Installer(projet3, text, a[2], (a[3] == "-") ? null : a[3], null);
				File.Delete(text);
				string[] array4 = Omsi.Installee(projet3, a[2]);
				File.WriteAllText(path2, "installe, nettoyes " + Omsi.Nettoyes + ", version " + array4[0] + " / " + (Omsi.Perimee(projet3, array4) ? "DIFFERENTE" : "a jour"), Encoding.UTF8);
				return 0;
			}
			catch (Exception ex2)
			{
				File.WriteAllText(path2, "ERREUR " + ex2.Message, Encoding.UTF8);
				return 1;
			}
		}
		if (a.Count >= 3 && a[0] == "/desinstaller")
		{
			string path3 = Path.Combine(a[2], "_resultat_test.txt");
			try
			{
				string erreur2;
				Catalogue catalogue2 = Catalogue.Charger(out erreur2);
				Projet p = catalogue2.Projets.Find((Projet x) => x.Id == a[1]);
				File.WriteAllText(path3, "desinstalle " + Omsi.Desinstaller(p, a[2]));
				return 0;
			}
			catch (Exception ex3)
			{
				File.WriteAllText(path3, "ERREUR " + ex3.Message);
				return 1;
			}
		}
		if (a.Count >= 2 && a[0] == "/preparer")
		{
			string erreur3;
			Catalogue catalogue3 = Catalogue.Charger(out erreur3);
			Projet projet4 = catalogue3.Projets.Find((Projet x) => x.Id == a[1]);
			Fenetre obj4 = new Fenetre();
			string text2 = (string)typeof(Fenetre).GetMethod("Preparer", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj4, new object[1] { projet4 });
			File.WriteAllText(Environment.GetEnvironmentVariable("TPS_DUTY") + ".resultat.txt", "ok" + text2, Encoding.UTF8);
			return 0;
		}
		if (a.Count >= 2 && a[0] == "/apercu")
		{
			Fenetre.ModeCapture = true;
			Fenetre fenetre0 = new Fenetre();
			fenetre0.StartPosition = FormStartPosition.Manual;
			fenetre0.Location = new Point(0, 0);
			int num0 = a.IndexOf("/taille");
			if (num0 >= 0 && num0 + 1 < a.Count)
			{
				string[] array0 = a[num0 + 1].Split('x');
				fenetre0.ClientSize = new Size(int.Parse(array0[0]), int.Parse(array0[1]));
				a.RemoveRange(num0, 2);
			}
			fenetre0.Apercu(a[1], (a.Count >= 3) ? a[2] : null);
			Application.Run(fenetre0);
			return 0;
		}
		if (a.Count >= 2 && a[0] == "/capture")
		{
			Fenetre.ModeCapture = true;
			Fenetre fenetre = new Fenetre();
			fenetre.StartPosition = FormStartPosition.Manual;
			fenetre.Location = new Point(-Zx.P(4000), -Zx.P(4000));
			fenetre.ShowInTaskbar = false;
			if (a.Contains("/min"))
			{
				fenetre.Size = fenetre.MinimumSize;
			}
			int num2 = a.IndexOf("/taille");
			if (num2 >= 0 && num2 + 1 < a.Count)
			{
				string[] array5 = a[num2 + 1].Split('x');
				fenetre.ClientSize = new Size(int.Parse(array5[0]), int.Parse(array5[1]));
				a.RemoveRange(num2, 2);
			}
			if (Environment.GetEnvironmentVariable("TPS_AMIS") == "1")
			{
				try
				{
					Social.Configurer(null);
					Social.ChargerAmis();
				}
				catch
				{
				}
			}
			fenetre.Capture(a[1]);
			if (a.Contains("/vues"))
			{
				fenetre.CaptureVues(a[1]);
			}
			if (a.Contains("/intro"))
			{
				fenetre.CaptureIntro(a[1]);
			}
			if (a.Contains("/trans"))
			{
				fenetre.CaptureTransitions();
			}
			if (a.Contains("/social"))
			{
				Social.Configurer(null);
				fenetre.CaptureSocial(a[1]);
			}
			if (a.Contains("/bannieres"))
			{
				fenetre.CaptureBannieres(a[1]);
			}
			if (a.Contains("/noel"))
			{
				fenetre.CaptureNoel(a[1]);
			}
			if (a.Contains("/integre"))
			{
				fenetre.CaptureIntegre(a[1], a.Contains("/anim"));
			}
			if (a.Contains("/bench"))
			{
				fenetre.Benchmark(a[1]);
			}
			if (a.Contains("/fps"))
			{
				fenetre.MesureReelle(a[1]);
			}
			if (a.Contains("/admin"))
			{
				FenAdmin fenAdmin = new FenAdmin();
				fenAdmin.StartPosition = FormStartPosition.Manual;
				fenAdmin.Location = new Point(-Zx.P(4000), -Zx.P(4000));
				fenAdmin.ShowInTaskbar = false;
				fenAdmin.Show();
				for (int k = 0; k < 20; k++)
				{
					Thread.Sleep(100);
					Application.DoEvents();
				}
				fenAdmin.Refresh();
				Application.DoEvents();
				using (Bitmap bitmap = new Bitmap(fenAdmin.Width, fenAdmin.Height))
				{
					fenAdmin.DrawToBitmap(bitmap, new Rectangle(0, 0, fenAdmin.Width, fenAdmin.Height));
					bitmap.Save(Path.Combine(a[1], "admin.png"));
				}
				Queue<Control> queue = new Queue<Control>();
				queue.Enqueue(fenAdmin);
				Panel panel = null;
				while (queue.Count > 0 && panel == null)
				{
					Control control = queue.Dequeue();
					if (control is Panel { AutoScroll: not false, Width: >600 } panel2)
					{
						panel = panel2;
					}
					foreach (Control control2 in control.Controls)
					{
						queue.Enqueue(control2);
					}
				}
				if (panel != null)
				{
					int[] array6 = new int[2] { 560, 1060 };
					for (int l = 0; l < array6.Length; l++)
					{
						panel.AutoScrollPosition = new Point(0, array6[l]);
						for (int m = 0; m < 5; m++)
						{
							Thread.Sleep(80);
							Application.DoEvents();
						}
						fenAdmin.Refresh();
						Application.DoEvents();
						using Bitmap bitmap2 = new Bitmap(fenAdmin.Width, fenAdmin.Height);
						fenAdmin.DrawToBitmap(bitmap2, new Rectangle(0, 0, fenAdmin.Width, fenAdmin.Height));
						bitmap2.Save(Path.Combine(a[1], (l == 0) ? "admin_bas.png" : "admin_bas2.png"));
					}
				}
				fenAdmin.Close();
			}
			if (a.Contains("/fenetres"))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["paris25"] = "Paris 25";
				dictionary["parisview"] = "ParisView";
				Form[] array7 = new Form[4]
				{
					new FenParametres(pleinActuel: false, "9.9.9"),
					new FenCompte(nouveau: false, dictionary),
					new FenJeton(),
					new FenBanniere(Comptes.Actif)
				};
				foreach (Form form in array7)
				{
					form.StartPosition = FormStartPosition.Manual;
					form.Location = new Point(-Zx.P(4000), -Zx.P(4000));
					form.ShowInTaskbar = false;
					form.Show();
					Application.DoEvents();
					form.Refresh();
					using (Bitmap bitmap3 = new Bitmap(form.Width, form.Height))
					{
						form.DrawToBitmap(bitmap3, new Rectangle(0, 0, form.Width, form.Height));
						bitmap3.Save(Path.Combine(a[1], ((form is FenParametres) ? "parametres" : ((form is FenJeton) ? "jeton" : ((form is FenBanniere) ? "banniere" : "compte"))) + ".png"));
					}
					form.Close();
				}
			}
			if (a.Contains("/dossiers"))
			{
				FenDossiers fenDossiers = new FenDossiers("Bienvenue ! Indiquez où sont installés OMSI 2 et openOMSI sur ce PC (une seule fois).");
				fenDossiers.StartPosition = FormStartPosition.Manual;
				fenDossiers.Location = new Point(-Zx.P(4000), -Zx.P(4000));
				fenDossiers.ShowInTaskbar = false;
				fenDossiers.Show();
				Application.DoEvents();
				fenDossiers.Refresh();
				using (Bitmap bitmap4 = new Bitmap(fenDossiers.Width, fenDossiers.Height))
				{
					fenDossiers.DrawToBitmap(bitmap4, new Rectangle(0, 0, fenDossiers.Width, fenDossiers.Height));
					bitmap4.Save(Path.Combine(a[1], "dossiers.png"));
				}
				fenDossiers.Close();
			}
			fenetre.Close();
			return 0;
		}
		if (!a.Contains("/portable"))
		{
			try
			{
				if (SInstaller())
				{
					return 0;
				}
			}
			catch
			{
			}
		}
		string text3 = Environment.GetEnvironmentVariable("TPS_MUTEX");
		if (string.IsNullOrEmpty(text3))
		{
			text3 = "TPSStudiosLanceur";
		}
		using (Mutex mutex = new Mutex(initiallyOwned: false, text3))
		{
			bool flag;
			try
			{
				flag = mutex.WaitOne(a.Contains("/apres") ? 20000 : 0, exitContext: false);
			}
			catch (AbandonedMutexException)
			{
				flag = true;
			}
			if (!flag)
			{
				try
				{
					int num3 = RegisterWindowMessage("TPSStudios_Montrer");
					if (num3 != 0)
					{
						PostMessage((IntPtr)65535, num3, IntPtr.Zero, IntPtr.Zero);
					}
				}
				catch
				{
				}
				return 0;
			}
			Verrou = mutex;
			verrouPris = true;
			try
			{
				Fenetre.MsgMontrer = RegisterWindowMessage("TPSStudios_Montrer");
			}
			catch
			{
			}
			try
			{
				Application.Run(new Fenetre());
			}
			finally
			{
				Liberer();
			}
		}
		return 0;
	}
}
