using System.Windows.Forms;

namespace TPSStudios;

internal static class MessageBox
{
	public static DialogResult Show(string texte)
	{
		return Show(null, texte, "TPS Groupe", MessageBoxButtons.OK, MessageBoxIcon.None);
	}

	public static DialogResult Show(string texte, string titre)
	{
		return Show(null, texte, titre, MessageBoxButtons.OK, MessageBoxIcon.None);
	}

	public static DialogResult Show(IWin32Window o, string texte)
	{
		return Show(o, texte, "TPS Groupe", MessageBoxButtons.OK, MessageBoxIcon.None);
	}

	public static DialogResult Show(IWin32Window o, string texte, string titre)
	{
		return Show(o, texte, titre, MessageBoxButtons.OK, MessageBoxIcon.None);
	}

	public static DialogResult Show(IWin32Window o, string texte, string titre, MessageBoxButtons b)
	{
		return Show(o, texte, titre, b, MessageBoxIcon.None);
	}

	public static DialogResult Show(IWin32Window o, string texte, string titre, MessageBoxButtons b, MessageBoxIcon i)
	{
		Fenetre principale = Fenetre.Principale;
		if (principale == null || principale.IsDisposed || !principale.IsHandleCreated || Fenetre.ModeCapture)
		{
			return System.Windows.Forms.MessageBox.Show(o, texte, titre, b, i);
		}
		using FenMessage f = new FenMessage(texte, titre, b, i);
		return principale.Integrer(f);
	}
}
