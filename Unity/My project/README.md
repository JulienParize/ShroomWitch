# ShroomWitch (titre provisoire : ShroomWitch ou SmallRain)

Roguelite 3D low poly sous Unity : on incarne une sorcière chatte qui part en balade cueillir des champignons, puis revient à sa hutte.

> Ce README reprend la feuille de route du projet Claude « Jeu Unity Champi » (synchronisée le 8 octobre 2026). C'est la référence de conception du jeu : à lire avant de coder, et à tenir à jour quand une décision change.

## Sommaire

- [Organisation du dossier](#organisation-du-dossier)
- [Concept et boucle de jeu](#concept-et-boucle-de-jeu)
- [Touches originales retenues](#touches-originales-retenues)
- [Champignons](#champignons)
- [La sorcière](#la-sorcière)
- [Histoire](#histoire)
- [Armes et sorts](#armes-et-sorts)
- [Compagnons animaux](#compagnons-animaux)
- [Le monde](#le-monde)
- [Ennemis](#ennemis)
- [Le refuge](#le-refuge)
- [Progression et méta](#progression-et-méta)
- [Technique Unity](#technique-unity)
- [Direction artistique et son](#direction-artistique-et-son)
- [Développement](#développement)
- [Questions ouvertes et prochaines étapes](#questions-ouvertes-et-prochaines-étapes)
- [Sources](#sources)

## Organisation du dossier

```
ShroomWitch/
├── The Witch/          modèle Blender de la sorcière (TheWitch.blend)
├── Unity/
│   └── My project/     projet Unity (ce README est ici)
│       ├── Assets/
│       ├── Packages/
│       └── ProjectSettings/
└── *.png, *.jpg        images de référence (allée de platanes, références de modélisation…)
```

Les dossiers `Library/`, `Logs/`, `UserSettings/` et les fichiers `.csproj` / `.sln` sont générés par Unity : ils ne font pas partie des sources.

## Concept et boucle de jeu

Un roguelite 3D sous Unity : on incarne une sorcière qui part en balade cueillir des champignons, puis revient à sa hutte.

### Session type

1. J'allume le jeu, je lance ma sauvegarde : je suis au refuge.
2. Je fais un tour et j'ouvre la Mycothèque des champignons et herbes : infos sur ceux déjà découverts, page blanche avec seulement la silhouette pour les autres.
3. Au râtelier d'armes et grimoires, je choisis la foudre.
4. Je sors de la hutte et je caresse ma vache.
5. Au carrefour, je choisis le biome marécage sur le panneau.
6. J'apparais dans le marécage, je me déplace légèrement et rapidement avec mes 3 sorts de foudre.
7. J'avance dans le niveau, je cueille, je combats, je trouve la salle au trésor, un marchand ou un pacte du diable.
8. Je bats le boss de fin pour accéder au niveau suivant.

## Touches originales retenues

Trois pistes pour distinguer le jeu, qui se renforcent entre elles.

| Piste | Principe |
| --- | --- |
| Manger ou garder | Chaque champignon est un dilemme : le manger pour un pouvoir immédiat pendant la run, ou le rapporter à la hutte pour progresser sur le long terme |
| Mycélium vivant | La forêt est reliée par un réseau souterrain qui réagit à la cueillette : trop ramasser la rend hostile, bien la traiter la fait prospérer |
| Hallucinations qui modifient le monde | Certains champignons changent réellement le niveau : chemins, ponts, comportement des ennemis |

Détails : à compléter.

## Champignons

Des champignons et des herbes à collecter tout au long du niveau, qui offrent des effets et bonus qui synergisent entre eux, avec plus ou moins de rareté. Certains ont aussi des effets négatifs. Modèle visé pour les synergies : The Binding of Isaac.

### Actions sur un champignon (système encore en réflexion)

- **Cueillir** : très léger buff passif.
- **Manger** : buff complet.
- **Laisser**.
- Une option qui ne donne rien sur le moment, mais de l'argent plus tard : à préciser.

Le nombre de champignons qu'on peut ramener à la hutte est limité.

### Faux-semblants

En début de partie, certains mauvais champignons ressemblent à des bons, avec une légère subtilité qui les distingue.

### Espèces et effets

À compléter.

### Génération procédurale

Une vingtaine d'espèces au départ. Chaque individu varie de façon procédurale : plus ou moins grand, avec plus ou moins de variations.

### Touffes

Une touffe trouvée (par exemple de girolles) compte comme un seul champignon, mais peut en contenir 2, 3, 4, 5… jusqu'à un nombre maximum.

### Rareté

À compléter.

## La sorcière

Une sorcière féline : chatte noire aux yeux jaunes, potentiellement personnalisable (à voir).

- **Vue** : troisième personne, dans l'esprit de Risk of Rain 2 ; première personne envisagée en plus.
- **Mouvements** : fluides, légers et rapides.
- **Combats** : fun.
- **Contrôles** : Shift pour sprinter, Alt pour esquiver, Espace pour sauter.
- **Capacités** : à compléter.

## Histoire

Abandonnée à sa naissance, elle a été recueillie par une sorcière. Détails à préciser plus tard.

Mot pour la mère adoptive (synonyme de sorcière) : à trouver.

### Fil conducteur

- **Départ** : la mentor a disparu. L'héroïne reprend son poste et, tout en cueillant des champignons pour des potions ou pour les vendre, elle la cherche.
- **La guerre** : certains champignons, dits « éveillés » (des petits hommes-champignons), sont en guerre contre les insectes. Ni l'un ni l'autre camp n'est entièrement bon ou mauvais.
- **La mentor** : ce qu'il est advenu d'elle reste à définir.

## Armes et sorts

Plusieurs types d'armes et de sorts dès le début :

- Épée à une main
- Masse d'arme
- Hache
- Sort de foudre
- Sort de feu
- Sort de nature

L'arme ou le grimoire se choisit au refuge avant la run. Chaque élément donne accès à 3 sorts.

| Touche | Foudre |
| --- | --- |
| Clic gauche | Éclair simple sur un ennemi |
| Clic droit | Fronde éclair |
| R | Éclair qui parcourt plusieurs ennemis |

Les autres armes et sorts : à compléter.

## Compagnons animaux

Des animaux rares à trouver pendant les runs, qui aident la sorcière.

| Animal | Aide |
| --- | --- |
| Cochon | Trouve les truffes |
| Chauve-souris | Régénère la vie sur les monstres |

Au refuge : une vache (nom à trouver), débloquée entre le début et le milieu du jeu, qu'on peut caresser. D'autres animaux se croisent pendant les runs et se débloquent grâce à certains objets rares à trouver.

## Le monde

Plusieurs biomes, dont 2 au début pour la démo, avec un environnement interactif et destructible.

- **Biomes** : marécage ; les autres à compléter. Décors avec plusieurs types d'arbres (épicéas, sapins, bouleaux…) et terrain généré procéduralement. Comme dans la réalité, certains champignons poussent sur certains types d'arbres.
- **Structure d'un niveau** : pas une grande carte ouverte comme Risk of Rain ; il faut avancer jusqu'à un boss de fin pour accéder au niveau suivant.
- **Génération** : procédurale, donc différente à chaque run.
- **Ennemis** : ils apparaissent dès le premier champignon cueilli, en continu, mais en nombre fini.
- **Salles spéciales** : une salle au trésor avec au moins un champignon ou une herbe rare, parfois un marchand ou une sorte de pacte du diable.

## Ennemis

Des ennemis originaux ; piste envisagée : les insectes.

## Le refuge

La hutte de sorcière de l'héroïne : une sorte de hub, qu'il sera possible de décorer.

- **La hutte** : petite, avec un toit inspiré de celui des Hospices de Beaune.
- **Mycothèque** : fiches des champignons et herbes déjà découverts, silhouette seule pour les autres. Un champignon rapporté d'une sortie met plusieurs jours à être identifié, et plus longtemps s'il est rare.
- **Râtelier d'armes et grimoires** : choix de l'arme ou de l'élément avant la run.
- **Les animaux** : la vache et les autres animaux débloqués vivent autour de la hutte.
- **L'allée de platanes** : une longue allée, avec au bout les pancartes pour choisir le niveau.
- **Les champs** : derrière la hutte, de hautes herbes vertes où l'on voit souffler le vent ; on peut s'y allonger paisiblement.
- **La forêt-mirage** : sur les autres côtés, une forêt où, si l'on s'enfonce, on revient à la hutte, sans que la transition soit perceptible.

## Progression et méta

À compléter.

## Technique Unity

Jeu en 3D sous Unity. Détails techniques : à compléter.

## Direction artistique et son

Jeu en 3D low poly. Pas un énième univers de sorcière fantasy : inspiration Morrowind pour les décors. Son : à compléter.

---

## Développement

### Le concept est-il original ?

Chaque ingrédient existe déjà (champignons, sorcière, chat, roguelite), mais leur combinaison reste rare : l'originalité viendra de l'exécution, pas de l'idée seule.

| Jeu | Proximité avec le projet |
| --- | --- |
| [Mushroom Witch Game](https://66saver.itch.io/mushroom-witch-game) | Très proche : roguelite cosy de cueillette de champignons et de potions, mais petit projet itch.io inachevé |
| [Shroom and Gloom](https://store.steampowered.com/app/3271280/Shroom_and_Gloom/) | Roguelike deckbuilder en vue subjective, ennemis champignons, édité par Devolver |
| [Mushroom Musume](https://store.steampowered.com/app/2484580/Mushroom_Musume/) | Simulation de vie autour d'une fille-champignon, avec un almanach de cueillette |
| [Whiskers and Wizardry](https://zillo-games.itch.io/whiskers-and-wizardry) | Roguelite d'action avec un chat magicien et des synergies de sorts |
| [Blade of Wiz](https://www.analogstickgaming.com/news/blade-of-wiz) | Roguelite à la Hades avec un chat noir spectral et des transformations |
| [Mewgenics](https://www.notebookcheck.net/Cat-themed-Roguelite-launches-on-Steam-with-over-3-500-overwhelmingly-positive-reviews-and-10-discount.1224316.0.html) | Roguelite sur le thème des chats, gros succès en 2026 |

Ce qui distingue le jeu : la cueillette comme cœur du jeu (touffes procédurales, rareté, synergies), la 3D, et une héroïne chatte orpheline. À surveiller de près : Mushroom Witch Game, le concurrent le plus proche.

### Facteurs clés de réussite

Un roguelite réussit s'il est agréable à jouer dès la première minute, donne envie de « refaire une run », et se fait connaître bien avant sa sortie.

1. **Les sensations de jeu** : déplacement, cueillette et combat doivent être réactifs et satisfaisants. Un jeu « à la Hades » qui ne se ressent pas bien ne tient pas.
2. **Les synergies** : le plaisir de découvrir des combos qui « cassent » le jeu. C'est l'argument central avec les champignons.
3. **Une identité forte** : une accroche lisible en une image (une chatte sorcière qui cueille des champignons) et un style visuel reconnaissable.
4. **La tranche verticale** : une portion courte du jeu, 100 % finie (art, son, sensations), qui sert de démo et prouve que le jeu est amusant.
5. **La visibilité en amont** : page Steam ouverte tôt, listes de souhaits (wishlists), démo publique, Discord. Mieux vaut sortir sa démo avant le Steam Next Fest plutôt que de la dévoiler pendant.
6. **Faire tester tôt et souvent** : l'équipe de Shroom and Gloom attribue une grande partie de ses progrès aux retours sur ses prototypes jouables.

### Ce qui est raisonnable en solo

Le projet est faisable seul, à condition de viser d'abord une démo courte et soignée plutôt que le jeu complet. La 3D et l'environnement destructible sont les deux postes les plus coûteux en temps.

| Poste | Pour la démo | Plus tard |
| --- | --- | --- |
| Biomes | 1 bien fini, puis le 2e | Autres biomes |
| Champignons et herbes | Une poignée d'espèces aux synergies claires | Les ~20 espèces |
| Armes et sorts | 2 ou 3 | Le reste de la liste |
| Ennemis | Quelques types + un boss | Variantes par biome |
| Compagnons | 1 (ou aucun) | Cochon, chauve-souris… |
| Destruction | Quelques objets destructibles | Environnement plus riche |
| Hutte | Fonctionnelle, sans décoration | Décoration |

**Bonnes pratiques solo**

- Prototyper avec des formes simples (cubes, capsules) avant de produire l'art.
- Utiliser des assets du Unity Asset Store pour ce qui n'est pas au cœur du jeu.
- Choisir un style 3D simple et stylisé (low poly par exemple), bien plus rapide à produire que du réaliste.
- Confier ce qui ne relève pas de ses forces (musique, par exemple) à d'autres.
- Garder une liste « plus tard » : toute nouvelle idée y va, pas dans la démo.

### Ordre de développement

D'abord prouver que cueillir est amusant avec des formes simples, puis construire la boucle, puis finir une petite portion avant de produire du contenu. Chaque phase se termine par un point de validation : on ne passe à la suivante que si le critère est atteint, sinon on corrige avant d'ajouter du contenu. L'art définitif n'arrive qu'en phase 3.

*(Le détail des 5 phases et 4 validations est dans la frise de l'artefact « Feuille de route » : à recopier ici.)*

### La B.O. par un élève d'école de musique

C'est envisageable et c'est une bonne idée : l'élève gagne un vrai projet pour son portfolio, le jeu une musique originale. Il faut cadrer les droits par écrit dès le départ, surtout si le jeu est vendu.

**Ce qu'il faut savoir en France**

- Le compositeur garde toujours son droit moral (être cité, respect de l'œuvre) : il ne peut pas être cédé, contrairement au « buy-out » américain.
- Pour utiliser la musique dans le jeu, il faut un contrat de cession de droits. Chaque droit cédé doit y être mentionné séparément (reproduction, diffusion, durée, territoires, supports).
- Si l'élève est ou devient membre de la Sacem, ses œuvres passent par elle : il faudra alors une licence et une déclaration. À vérifier avec lui dès le début.

**Démarche conseillée**

- [ ] Contacter l'école ou un professeur de composition, avec un dossier court : pitch, images, ambiance recherchée, jeux de référence.
- [ ] Demander à l'école si elle a des règles sur les droits des projets de fin d'année.
- [ ] Aligner les calendriers : l'année scolaire de l'élève et la phase où la musique sera nécessaire (tranche verticale).
- [ ] Prévoir une contrepartie : crédit visible, rémunération ou part des revenus.
- [ ] Demander les pistes séparées (stems) pour pouvoir adapter la musique en jeu.
- [ ] Faire relire le contrat par un juriste ou se renseigner auprès de la Sacem.

Ces points sont des repères, pas un avis juridique.

## Questions ouvertes et prochaines étapes

À compléter.

## Sources

- [Mushroom Witch Game (itch.io)](https://66saver.itch.io/mushroom-witch-game)
- [Shroom and Gloom (Steam)](https://store.steampowered.com/app/3271280/Shroom_and_Gloom/)
- [Mushroom Musume (Steam)](https://store.steampowered.com/app/2484580/Mushroom_Musume/)
- [Whiskers and Wizardry (itch.io)](https://zillo-games.itch.io/whiskers-and-wizardry)
- [Blade of Wiz (Analog Stick Gaming)](https://www.analogstickgaming.com/news/blade-of-wiz)
- [Mewgenics (Notebookcheck)](https://www.notebookcheck.net/Cat-themed-Roguelite-launches-on-Steam-with-over-3-500-overwhelmingly-positive-reviews-and-10-discount.1224316.0.html)
- [Conseils Steam Next Fest (Unity)](https://unity.com/blog/steam-next-fest-developer-tips-unity)
- [Réussir un jeu indé (Game Developers)](https://game-developers.org/how-to-create-successful-indie-games)
- [Musique de jeux vidéo et droits d'auteur (La revue des médias, INA)](https://larevuedesmedias.ina.fr/la-musique-de-jeux-video-cest-avant-tout-de-la-musique)
- [Droit d'auteur et jeu vidéo (Articlaw)](https://articlaw.net/publications/legal/droit-dauteur-et-jeu-video/)
- [Licences Sacem pour la musique en jeu (Deshoulières Avocats)](https://www.deshoulieres-avocats.com/musique-in-game-streaming-comprendre-les-licences-sacem/)
