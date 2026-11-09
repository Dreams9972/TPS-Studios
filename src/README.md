# Code source du lanceur TPS Groupe

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

## Bibliothèque (v2.8.0)

Code dans `TPSStudios/FenetreBiblio.cs` (partie de la classe `Fenetre`). Deux onglets :

- **Bibliothèque** : page du jeu façon Steam (bannière `library_hero.jpg` de Steam si présente,
  bouton JOUER, statistiques, à propos, actualités des mods) et, à droite, **Mes mods** :
  les mods installés avec un interrupteur activer / désactiver, « ▶ Jouer » pour lancer
  openOMSI directement sur une carte, et « Mettre à jour » si besoin.
- **Mods** : tout le catalogue du jeu (recherche, filtres, catégories) avec un tri
  par défaut / nom / récents / populaires.

## Aperçu pour le développement

`TPS.Studios.exe /apercu biblio` (ou `/apercu projet <id>`, `/apercu jeux`) ouvre directement
une page, sans les fenêtres de démarrage. Option : `/taille 1280x720`.

## Bandes-annonces (v2.8.2)

- Fichier vidéo direct (mp4…) : lecteur Windows Media intégré.
- YouTube : WebView2 (moteur d'Edge), embarqué dans l'exe (`WebView2Embarque.cs`,
  `LecteurWeb.cs`). WebView2Loader.dll est déposé dans `%LocalAppData%\TPS Studios\webview2`.
- Mods sans vidéo : le lanceur cherche sur la chaîne YouTube TPS Groupe (flux public des
  dernières vidéos, `YouTube.cs`) une vidéo dont le titre contient le nom du mod, de
  préférence « trailer », « bande-annonce » ou « teaser ». Chaîne modifiable dans le
  catalogue : `communaute.youtube`.

## Nom : TPS Groupe (anciennement TPS Studios)

Tous les textes visibles disent « TPS Groupe ». Restent volontairement « TPS Studios » :
le dossier des données (`%LocalAppData%\TPS Studios`), le fichier installé `TPS Studios.exe`,
le dossier `_TPS Studios - mods désactivés`, le dépôt GitHub, le nom `TPS.Studios.exe`
du téléchargement, le verrou `TPSStudiosLanceur` et la clé de démarrage `TPSStudios`.
Les changer ferait perdre leurs réglages aux joueurs ou casserait la mise à jour automatique.
Les raccourcis « TPS Studios » sont renommés « TPS Groupe » au premier lancement.
