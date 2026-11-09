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
