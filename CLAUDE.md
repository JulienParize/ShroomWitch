# ShroomWitch – Instructions pour Claude Code

Jeu : roguelite 3D low poly sous Unity. Une sorcière chatte cueille des champignons, combat et revient à sa hutte.
Réponds toujours en français et explique simplement les termes techniques.

## Où trouver quoi
- Conception du jeu : `Unity/My project/README.md`. À lire avant toute tâche de gameplay (champignons, sorts, ennemis, refuge).
- Projet Unity : `Unity/My project/` (Unity 6, URP, Input System, AI Navigation).
- Mes fichiers Unity : `Assets/_Projet/`, scripts dans `Assets/_Projet/Scripts/`.
- Modèle 3D de la sorcière : `The Witch/TheWitch.blend`.

## Règles Unity (IMPORTANT)
- Ne jamais lire ni modifier `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- Chaque fichier garde son `.meta` : le déplacer ou le renommer avec lui, ne jamais le supprimer.
- Ne pas modifier à la main les scènes, prefabs, matériaux et assets (`.unity`, `.prefab`, `.mat`, `.asset`) : écrire un script d'éditeur, ou me dire quoi faire dans l'éditeur.
- Ne pas installer de paquet sans me demander.

## Code
- C#, une classe par fichier, namespace `ShroomWitch`.
- Noms en anglais, commentaires en français.
- Réglages modifiables dans l'inspecteur (`[SerializeField]`) ou dans des ScriptableObject, pas de valeurs en dur.

## Façon de travailler
- Un seul système à la fois, en suivant la feuille de route du projet.
- Pour un gros changement, proposer d'abord un plan.
- À la fin de chaque tâche : me dire comment la tester dans Unity, puis proposer un commit.
