using System;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenMessage : Form
{
	public FenMessage(string texte, string titre, MessageBoxButtons btns, MessageBoxIcon icone)
	{
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.StartPosition = FormStartPosition.Manual;
		base.ShowInTaskbar = false;
		Color backColor = icone switch
		{
			MessageBoxIcon.Question => C.Bleu, 
			MessageBoxIcon.Hand => C.Rouge, 
			MessageBoxIcon.Exclamation => C.Orange, 
			_ => C.Jade, 
		};
		int num = Zx.P(560);
		int num2 = 32;
		int num3 = Math.Min(Lang.MeasureText(texte, Font, new Size(num - 2 * num2, Zx.P(4000)), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height + Zx.P(6), Zx.P(420));
		base.ClientSize = new Size(num, Zx.P(4) + Zx.P(26) + Zx.P(34) + Zx.P(12) + num3 + Zx.P(26) + Zx.P(70));
		Panel value = new Panel
		{
			Dock = DockStyle.Top,
			Height = Zx.P(4),
			BackColor = backColor
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = titre,
			Font = new Font("Segoe UI Semibold", Zx.P(15f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(num2, Zx.P(30)),
			Size = new Size(num - 2 * num2, Zx.P(34))
		};
		base.Controls.Add(value2);
		Label value3 = new Label
		{
			Text = texte,
			Font = Font,
			ForeColor = Color.FromArgb(205, 214, 228),
			BackColor = C.Fond,
			Location = new Point(num2, Zx.P(76)),
			Size = new Size(num - 2 * num2, num3)
		};
		base.Controls.Add(value3);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		string[] array;
		DialogResult[] array2;
		switch (btns)
		{
		case MessageBoxButtons.YesNo:
			array = new string[2] { "Oui", "Non" };
			array2 = new DialogResult[2]
			{
				DialogResult.Yes,
				DialogResult.No
			};
			break;
		case MessageBoxButtons.YesNoCancel:
			array = new string[3] { "Oui", "Non", "Annuler" };
			array2 = new DialogResult[3]
			{
				DialogResult.Yes,
				DialogResult.No,
				DialogResult.Cancel
			};
			break;
		case MessageBoxButtons.OKCancel:
			array = new string[2] { "OK", "Annuler" };
			array2 = new DialogResult[2]
			{
				DialogResult.OK,
				DialogResult.Cancel
			};
			break;
		default:
			array = new string[1] { "OK" };
			array2 = new DialogResult[1] { DialogResult.OK };
			break;
		}
		Zx.P(28);
		Bouton premier = null;
		for (int num4 = array.Length - 1; num4 >= 0; num4--)
		{
			Bouton bouton = new Bouton(array[num4])
			{
				Plein = (num4 == 0),
				Size = new Size(Zx.P(120), Zx.P(38)),
				Location = ((num4 == 0) ? new Point(num - Zx.P(28) - Zx.P(120), Zx.P(16)) : new Point(Zx.P(28) + (num4 - 1) * Zx.P(132), Zx.P(16)))
			};
			DialogResult r = array2[num4];
			bouton.Click += delegate
			{
				base.DialogResult = r;
			};
			panel.Controls.Add(bouton);
			if (num4 == 0)
			{
				premier = bouton;
			}
		}
		base.Shown += delegate
		{
			if (premier != null)
			{
				premier.Focus();
			}
		};
	}
}
