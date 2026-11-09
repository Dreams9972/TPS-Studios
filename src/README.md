# Code source du lanceur TPS Studios

Projet C# WinForms (.NET Framework 4.8) du lanceur `TPS.Studios.exe`.

> Ce code a été reconstruit à partir du lanceur publié **v2.7.9** (décompilation),
> l'original n'étant pas dans le dépôt. Il compile à l'identique, mais les
> commentaires et certains noms de variables internes (`text2`, `num`…) d'origine sont perdus.

## Compiler

Avec le SDK .NET (8 ou plus récent) installé :

```
cd src
dotnet build -c Release
```

L'exécutable sort dans `src/bin/Release/net48/TPS.Studios.exe`.

## Repères

- `TPSStudios/Programme.cs` : point d'entrée, mise à jour du lanceur, commandes admin.
- `TPSStudios/Fenetre.cs` : fenêtre principale (bibliothèque, téléchargement, installation).
- `TPSStudios/Omsi.cs` : détection d'OMSI 2 / openOMSI, installation, désactivation, désinstallation.
- `TPSStudios/Catalogue.cs`, `Projet.cs` : lecture de `catalogue.json`.
- `TPSStudios/Config.cs` : réglages et dossiers (`%LocalAppData%\TPS Studios`).

## Publier le paquet d'un mod (mode administrateur, v2.8.0)

Dans le mode administrateur, sélectionnez une page puis **Publier le paquet…** :

1. Choisissez le dossier du mod. Soit il contient `jeu\` (et éventuellement `plugins\`, `fonts\`),
   soit c'est directement le contenu à copier dans le dossier d'OMSI 2 (`maps\`, `Vehicles\`…).
2. Indiquez la nouvelle version et les notes de version.
3. Le lanceur découpe le mod en zips d'environ 250 Mo, envoie chaque partie dans une release
   GitHub (`<id>-<version>`), vérifie sa taille et son empreinte, puis **l'efface du PC** :
   il n'y a jamais plus d'une partie sur le disque.
4. À la fin, le catalogue est mis à jour (version, date, empreintes, tailles) :
   les joueurs voient « METTRE À JOUR ».

En cas de coupure, relancez avec le même numéro de version : la release est réutilisée.
Le code est dans `TPSStudios/FenPaquet.cs`, `TPSStudios/Paquet.cs` et `TPSStudios/GitHub.cs`.
