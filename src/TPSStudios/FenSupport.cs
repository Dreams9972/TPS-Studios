using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace TPSStudios;

internal class FenSupport : Form
{
	private Segments sg;

	private Champ tMsg;

	private Champ tContact;

	private Label lErr;

	private Bouton ok;

	private static readonly string[] Sujets = new string[5] { "Installation", "Bug en jeu", "Mise à jour", "Achat / accès", "Autre" };

	public FenSupport(Projet p, string infos)
	{
		FenSupport fenSupport = this;
		Text = "Support";
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = C.Fond;
		ForeColor = C.Texte;
		Font = new Font("Segoe UI", Zx.P(10f));
		base.ClientSize = new Size(Zx.P(640), Zx.P(560));
		Label value = new Label
		{
			Text = "Support  ·  " + p.Nom,
			UseMnemonic = false,
			Font = new Font("Segoe UI Semibold", Zx.P(16f)),
			ForeColor = C.Texte,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(20)),
			AutoSize = true
		};
		base.Controls.Add(value);
		Label value2 = new Label
		{
			Text = "Décris ton souci : l'équipe TPS Groupe reçoit un ticket sur son Discord et te répond au plus vite.",
			Font = new Font("Segoe UI", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(58)),
			Size = new Size(Zx.P(584), Zx.P(22))
		};
		base.Controls.Add(value2);
		Label value3 = new Label
		{
			Text = "Quel est le souci ?",
			Font = new Font("Segoe UI Semibold", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(92)),
			AutoSize = true
		};
		base.Controls.Add(value3);
		sg = new Segments();
		sg.Items = Sujets;
		sg.Etire = true;
		sg.Location = new Point(Zx.P(28), Zx.P(116));
		sg.Width = Zx.P(584);
		sg.Index = 1;
		base.Controls.Add(sg);
		Label value4 = new Label
		{
			Text = "Explique ce qui se passe (quand, quel message, ce que tu as déjà essayé)",
			Font = new Font("Segoe UI Semibold", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(166)),
			AutoSize = true
		};
		base.Controls.Add(value4);
		tMsg = new Champ(multi: true, Zx.P(150));
		tMsg.Location = new Point(Zx.P(28), Zx.P(190));
		tMsg.Width = Zx.P(584);
		tMsg.MaxLength = 1000;
		base.Controls.Add(tMsg);
		Label value5 = new Label
		{
			Text = "Ton pseudo Discord (facultatif, pour te répondre)",
			Font = new Font("Segoe UI Semibold", Zx.P(9.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(354)),
			AutoSize = true
		};
		base.Controls.Add(value5);
		tContact = new Champ(multi: false, Zx.P(38));
		tContact.Location = new Point(Zx.P(28), Zx.P(378));
		tContact.Width = Zx.P(300);
		tContact.MaxLength = 40;
		tContact.Indice = "ex. evens_tps";
		base.Controls.Add(tContact);
		Label value6 = new Label
		{
			Text = "Joint automatiquement : " + infos,
			UseMnemonic = false,
			Font = new Font("Segoe UI", Zx.P(8.5f)),
			ForeColor = C.Gris,
			BackColor = C.Fond,
			Location = new Point(Zx.P(28), Zx.P(426)),
			Size = new Size(Zx.P(584), Zx.P(34))
		};
		base.Controls.Add(value6);
		lErr = new Label();
		lErr.Font = new Font("Segoe UI", Zx.P(9.5f));
		lErr.ForeColor = C.Rouge;
		lErr.BackColor = C.Fond;
		lErr.Location = new Point(Zx.P(28), Zx.P(462));
		lErr.Size = new Size(Zx.P(584), Zx.P(22));
		lErr.UseMnemonic = false;
		base.Controls.Add(lErr);
		Panel panel = new Panel
		{
			Dock = DockStyle.Bottom,
			Height = Zx.P(70),
			BackColor = C.Cote
		};
		base.Controls.Add(panel);
		Bouton bouton = new Bouton("Annuler")
		{
			Plein = false,
			Size = new Size(Zx.P(120), Zx.P(38)),
			Location = new Point(Zx.P(28), Zx.P(16))
		};
		EventHandler value7 = delegate
		{
			base.DialogResult = DialogResult.Cancel;
		};
		bouton.Click += value7;
		panel.Controls.Add(bouton);
		ok = new Bouton("Envoyer le ticket");
		ok.Size = new Size(Zx.P(190), Zx.P(38));
		ok.Location = new Point(Zx.P(640) - Zx.P(28) - Zx.P(190), Zx.P(16));
		panel.Controls.Add(ok);
		ok.Click += delegate
		{
			string text = fenSupport.tMsg.Text.Trim();
			if (text.Length < 10)
			{
				fenSupport.lErr.ForeColor = C.Rouge;
				fenSupport.lErr.Text = "Décris le souci en quelques mots (10 caractères au moins).";
			}
			else
			{
				Dictionary<string, object> t = new Dictionary<string, object>();
				t["mod"] = p.Id;
				t["nom_mod"] = p.Nom;
				t["sujet"] = Sujets[Math.Max(0, fenSupport.sg.Index)];
				t["message"] = text;
				t["contact"] = fenSupport.tContact.Text.Trim();
				t["infos"] = infos;
				fenSupport.ok.Enabled = false;
				fenSupport.lErr.ForeColor = C.Gris;
				fenSupport.lErr.Text = "Envoi du ticket…";
				Thread thread = new Thread((ThreadStart)delegate
				{
					string e;
					try
					{
						e = Social.EnvoyerSupport(t);
					}
					catch (Exception ex)
					{
						e = ex.Message;
					}
					try
					{
						fenSupport.BeginInvoke((MethodInvoker)delegate
						{
							if (!fenSupport.IsDisposed)
							{
								if (e == null)
								{
									fenSupport.lErr.ForeColor = C.Vert;
									fenSupport.lErr.Text = "✔  Ticket envoyé à l'équipe TPS Groupe. Merci !";
									fenSupport.ok.Text = "Envoyé ✔";
									System.Windows.Forms.Timer tf = new System.Windows.Forms.Timer();
									tf.Interval = 1600;
									tf.Tick += delegate
									{
										tf.Stop();
										fenSupport.DialogResult = DialogResult.OK;
									};
									tf.Start();
								}
								else
								{
									fenSupport.ok.Enabled = true;
									fenSupport.lErr.ForeColor = C.Rouge;
									fenSupport.lErr.Text = "✖  " + e;
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
