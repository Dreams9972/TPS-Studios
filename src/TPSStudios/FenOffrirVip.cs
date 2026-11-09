using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenOffrirVip : Form
{
	private Champ t;

	private Label lErr;

	private Bouton ok;

	private Ami a;

	public FenOffrirVip(Ami ami)
	{
		a = ami;
		Text = "Offrir le VIP";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(600), Zx.P(400));
		Label value = new Label
		{
			Text = "\ud83c\udf81  Offrir le VIP à " + ami.Pseudo,
			UseMnemonic = false,
			Font = new Font("Segoe UI Semibold", Zx.P(16f)),
			ForeColor = C.Or,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(22)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Entre un code VIP (TPS-VIP-XXXX-XXXX-XXXX) : il sera activé sur le compte de " + ami.Pseudo + ", pas sur le tien. " + ami.Pseudo + " reçoit tous les avantages VIP (badge, bannières, accès anticipé, points, rôle Discord) et est prévenu dans son lanceur.",
			UseMnemonic = false,
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(66)),
			Size = new Size(Zx.P(544), Zx.P(76))
		};
		base.Controls.Add(value2);
		t = new Champ(multi: false, Zx.P(40));
		t.Location = new Point(Zx.P(28), Zx.P(152));
		t.Width = Zx.P(544);
		t.Indice = "TPS-VIP-XXXX-XXXX-XXXX";
		t.MaxLength = 30;
		base.Controls.Add(t);
		Label value3 = new Label
		{
			Text = "Pas encore de code ? Achète le VIP à 5 € : tu reçois un code à offrir.",
			Font = new Font("Segoe UI", Zx.P(9f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(206)),
			AutoSize = true
		};
		base.Controls.Add(value3);
		Bouton bouton = new Bouton("Acheter un code VIP (5 €)…")
		{
			Plein = false,
			Couleur = C.Or,
			Size = new Size(Zx.P(250), Zx.P(36)),
			Location = new Point(Zx.P(28), Zx.P(232)),
			Enabled = (Fenetre.LienVip.Length > 0)
		};
		base.Controls.Add(bouton);
		bouton.Click += delegate
		{
			if (Uri.TryCreate(Fenetre.LienVip, UriKind.Absolute, out var result) && result.Scheme == Uri.UriSchemeHttps)
			{
				try
				{
					Process.Start(result.AbsoluteUri);
				}
				catch
				{
				}
			}
		};
		lErr = new Label();
		lErr.Font = new Font("Segoe UI", Zx.P(9.5f));
		lErr.ForeColor = C.Rouge;
		lErr.BackColor = C.Fond;
		lErr.Location = new Point(Zx.P(28), Zx.P(282));
		lErr.Size = new Size(Zx.P(544), Zx.P(40));
		lErr.UseMnemonic = false;
		base.Controls.Add(lErr);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton2 = new Bouton("Annuler");
		bouton2.Plein = false;
		bouton2.Size = new Size(Zx.P(120), Zx.P(38));
		bouton2.Location = new Point(Zx.P(28), Zx.P(16));
		bouton2.Click += delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		panel.Controls.Add(bouton2);
		ok = new Bouton("Offrir le VIP");
		ok.Couleur = C.Or;
		ok.Size = new Size(Zx.P(170), Zx.P(38));
		ok.Location = new Point(Zx.P(600) - Zx.P(28) - Zx.P(170), Zx.P(16));
		panel.Controls.Add(ok);
		ok.Click += delegate
		{
			string code = t.Text.Trim();
			if (code.Length < 10)
			{
				lErr.Text = "Entre le code VIP à offrir.";
			}
			else
			{
				ok.Enabled = false;
				lErr.ForeColor = C.Gris;
				lErr.Text = "Activation du VIP pour " + a.Pseudo + "…";
				Thread thread = new Thread((ThreadStart)delegate
				{
					string e;
					try
					{
						e = Social.OffrirVip(a.Id, code);
					}
					catch (Exception ex)
					{
						e = ex.Message;
					}
					try
					{
						BeginInvoke((MethodInvoker)delegate
						{
							if (!base.IsDisposed)
							{
								if (e == null)
								{
									base.DialogResult = DialogResult.OK;
								}
								else
								{
									ok.Enabled = true;
									lErr.ForeColor = C.Rouge;
									lErr.Text = "✖  " + e;
								}
							}
						});
					}
					catch
					{
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		};
	}
}
