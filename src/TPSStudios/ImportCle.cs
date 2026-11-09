using System;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal static class ImportCle
{
	public static void Lancer(Form f, Label etat)
	{
		string text = null;
		using (OpenFileDialog openFileDialog = new OpenFileDialog())
		{
			openFileDialog.Title = "Fichier de la clé administrateur (cle-admin.txt, dans le dossier Serveur)";
			openFileDialog.Filter = "Clé administrateur (cle-admin.txt)|cle-admin.txt|Fichiers texte (*.txt)|*.txt";
			if (openFileDialog.ShowDialog(f) != DialogResult.OK)
			{
				return;
			}
			try
			{
				text = Social.CleDepuisFichier(openFileDialog.FileName);
			}
			catch (Exception ex)
			{
				etat.ForeColor = C.Rouge;
				etat.Text = "✖  Lecture impossible : " + ex.Message;
				return;
			}
		}
		if (text == null)
		{
			etat.ForeColor = C.Rouge;
			etat.Text = "✖  Aucune clé trouvée dans ce fichier.";
			return;
		}
		Social.DefinirCleAdmin(text);
		etat.ForeColor = C.Gris;
		etat.Text = "Vérification de la clé auprès du serveur…";
		Thread thread = new Thread((ThreadStart)delegate
		{
			string e = Social.VerifierCleAdmin();
			try
			{
				f.BeginInvoke((MethodInvoker)delegate
				{
					etat.ForeColor = ((e == null) ? C.Vert : C.Rouge);
					etat.Text = ((e == null) ? "✔  Clé administrateur reconnue par le serveur." : ("✖  " + e));
				});
			}
			catch
			{
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}
}
