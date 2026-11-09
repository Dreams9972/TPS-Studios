using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenVideo : Form, IPage
{
	private LecteurWmp wmp;

	private string url;

	private bool parti;

	private int essais;

	private Timer tDem;

	public FenVideo(string titre, string url)
	{
		this.url = url;
		Text = titre;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = Color.Black;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = Integre.Hote;
		try
		{
			wmp = new LecteurWmp();
			((ISupportInitialize)wmp).BeginInit();
			wmp.Dock = DockStyle.Fill;
			base.Controls.Add(wmp);
			((ISupportInitialize)wmp).EndInit();
		}
		catch (Exception e2)
		{
			Journal.Une("video-lecteur", e2);
			wmp = null;
		}
		Panel panel = new Panel
		{
			Dock = DockStyle.Top,
			Height = Zx.P(74),
			BackColor = C.Fond
		};
		base.Controls.Add(panel);
		panel.Controls.Add(Integre.Retour(delegate
		{
			base.DialogResult = DialogResult.OK;
		}, Zx.P(24), Zx.P(13)));
		Label value = new Label
		{
			Text = titre,
			Font = new Font("Segoe UI Semibold", Zx.P(16f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(84), Zx.P(22)),
			AutoSize = true
		};
		panel.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Bande-annonce  ·  Échap pour revenir",
			Font = new Font("Segoe UI", Zx.P(9f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			AutoSize = true,
			Anchor = (AnchorStyles.Top | AnchorStyles.Right),
			Location = new Point(base.ClientSize.Width - Zx.P(260), Zx.P(30))
		};
		panel.Controls.Add(value2);
		base.KeyPreview = true;
		base.KeyDown += delegate(object o, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				base.DialogResult = DialogResult.OK;
			}
		};
		tDem = new Timer();
		tDem.Interval = 150;
		tDem.Tick += delegate
		{
			Demarrer();
		};
		tDem.Start();
		base.FormClosing += delegate
		{
			tDem.Stop();
			try
			{
				if (wmp != null && wmp.Ocx != null)
				{
					object o2 = LecteurWmp.Lire(wmp.Ocx, "controls");
					LecteurWmp.Appeler(o2, "stop");
				}
			}
			catch
			{
			}
		};
	}

	private void Demarrer()
	{
		if (parti)
		{
			return;
		}
		object obj = ((wmp != null && wmp.IsHandleCreated) ? wmp.Ocx : null);
		if (obj == null)
		{
			if (++essais >= 30)
			{
				parti = true;
				tDem.Stop();
				Secours();
			}
			return;
		}
		parti = true;
		tDem.Stop();
		try
		{
			LecteurWmp.Fixer(obj, "uiMode", "full");
			try
			{
				LecteurWmp.Fixer(obj, "stretchToFit", true);
				LecteurWmp.Fixer(obj, "enableContextMenu", false);
			}
			catch
			{
			}
			object o = LecteurWmp.Lire(obj, "settings");
			LecteurWmp.Fixer(o, "autoStart", true);
			LecteurWmp.Fixer(o, "volume", 80);
			LecteurWmp.Fixer(obj, "URL", url);
		}
		catch (Exception e)
		{
			Journal.Une("video", e);
			Secours();
		}
	}

	private void Secours()
	{
		try
		{
			Process.Start(url);
		}
		catch
		{
		}
		BeginInvoke((MethodInvoker)delegate
		{
			base.DialogResult = DialogResult.OK;
		});
	}
}
