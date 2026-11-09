using System;

namespace TPSStudios;

internal class DefSucces
{
	public string Id;

	public string Nom;

	public string Desc;

	public string Glyphe;

	public string Famille = "";

	/// <summary>Rubrique de la fenêtre Succès : "lanceur", l'identifiant d'un jeu, ou "mods".</summary>
	public string Groupe = "lanceur";

	public string Prec;

	public int Xp;

	public int Objectif;

	public int Rang;

	public int NbRangs;

	public Func<Compte, int> Valeur;
}
