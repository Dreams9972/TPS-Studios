using System;
using System.Collections.Generic;

namespace TPSStudios;

internal class Ami
{
	public string Id = "";

	public string Pseudo = "";

	public string Statut = "";

	public string Lien = "";

	public int Niveau = 1;

	public int Communs;

	public bool Vip;

	public bool EnLigne;

	/// <summary>Date d'inscription sur le serveur (secondes Unix), 0 si inconnue.</summary>
	public long Cree;

	/// <summary>Apparence publique envoyée par son lanceur (bannière, couleurs, décor, bio…).</summary>
	public Dictionary<string, string> App = new Dictionary<string, string>();

	public string Texte(string k)
	{
		if (!App.TryGetValue(k, out var v) || v == null)
		{
			return "";
		}
		return v;
	}

	public int Nombre(string k, int defaut = 0)
	{
		if (!App.TryGetValue(k, out var v) || !int.TryParse(v, out var n))
		{
			return defaut;
		}
		return n;
	}

	/// <summary>Profil reconstitué à partir de l'apparence, pour dessiner sa bannière et son avatar comme un profil local.</summary>
	public Compte EnCompte()
	{
		Compte c = new Compte();
		c.Id = "ami-" + Id;
		c.Pseudo = Pseudo;
		c.Couleur = Math.Max(0, Nombre("couleur"));
		c.Banniere = Math.Max(0, Nombre("banniere"));
		c.CouleurPerso = Nombre("couleur_perso");
		string t = Texte("ban_type");
		c.BanType = ((t == "image") ? "" : t);
		c.BanMotif = Texte("ban_motif");
		c.BanC1 = Nombre("ban_c1");
		c.BanC2 = Nombre("ban_c2");
		c.BanAngle = Nombre("ban_angle");
		c.Deco = Texte("deco");
		c.Bio = Texte("bio");
		c.Statut = Texte("statut");
		c.JeuFavori = Texte("jeu_favori");
		c.Lieu = Texte("lieu");
		return c;
	}
}
