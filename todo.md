# TODO


- [ ] - faire les inventory context + Xbox compatibility
- [ ] - faire une classe Movable parent de Being et de Item afin d'appliquer des forces aux items
- [ ] - faire des plafonds respectifs à chaque area afin de pouvoir cacher les zones non découvertes
- [x] - (ui) faire des descriptions au sol qui indiquent la position d'une porte
- [ ] - faire des distributeurs de vie/bits
- [ ] - faire des coffres spéciaux à choix pour les sas
- [ ] - améliorer le dash : changer le sprite en fonction de la direction











# BUGS

- [ ] - quand on est loin de xp-provider ça arrete de générer de l'xp ?
- [ ] - bug graphique d'animations : les zombos n'attaquent pas










# LONG TERME

- [-] - faire un fichier json qui stocke les paramètres des différents beings

- [x] - faire une génération aléatoire des niveaux
    - [ ] - mettre en place le TK algorithm pour créer les salles
    - [ ] - créer une classe Room qui remplit les salles avec les tiles
    - [ ] - faire des plafonds qu'on peut passer dessous avec notre perso
            parce que ça rend bien ! genre dans les couloirs

- [x] - génération procédurale
    - [ ] - World.cs génère différents secteurs et les relie entre eux
    - [ ] - Sector.cs applique les bonnes textures (tiles) aux tilemaps de chaque Room
    - [ ] - Sector.cs dit aux Rooms adequates de generer *coffres*, doors, *computers*, *zombies*
    - [ ] - Room.cs génère les éléments de décor : objects, *posters*, tags, *lights*
    -> CF. gameplay.txt

- [ ] - mettre un drone de soutien qui est notre compagnie
    - [ ] - qui peut nous parler
    - [ ] - nous faire les tutos
    - [ ] - et qu'on peut améliorer
    - [ ] - au niv 2, ce drone sert de relai de hack

- [ ] - mettre des objets interactifs dans le niveau
    - [x] - mettre des coffres pour des items physiques
    - [ ] - mettre des disques durs pour des hacks
    - [ ] - mettre des lit pour sauvegarder la partie

- [ ] - faire des visus
    - [x] - des déchets
    - [x] - des murs ventilateurs
    - [x] - des flaques
    - [x] - des posters sur les murs
    - [x] - des poubelles
    - [x] - des tags
    - [ ] - des vieilles machines (type distributeur) cassées
    - [ ] - des déchets electroniques
    - [ ] - des vieux sacs de couchage
    - [ ] - des bornes electriques
    - [ ] - des rails désaffectés
    - [ ] - des roues de metros
    - [ ] - des vieux bureaux pétés


- [ ] - améliorer l'ia des ennemis
    - [ ] - faire ennemi intelligent cac comme ça https://www.youtube.com/watch?v=6BrZryMz-ac


- [ ] - mettre des modifieurs d'items
    - [ ] - pour les hacks : stat de propagation automatique à un ennemi proche
    - [ ] - acceleration de la vitesse de hack

- [ ] - améliorer l'UI
    - [ ] - faire un menu pause qui grossi les différents HUD (map, barre de vie)
    - [ ] - ajouter lecteur CD en bas à gauche ?
    - [ ] - ajouter visu clés RSA / HASH en bas à droite ?





















# DONE

- [x] - régler l'hoover description pour qu'il ne sorte pas de l'écran
- [x] - et qu'il aille avec l'xbox controller
- [x] - UI_XboxNavigator faire reselectionner automatiqement le plus proche quand on appuie sur un item

- [x] - bug ordi freeze à la fin de l'animation idle_on

- [x] - old
    - [x] - mettre une barre de vie de perso
    - [x] - mettre un compteur d'exploits (mana)
    - [x] - mettre une barre de hacker (xp)

    - [x] - faire des attaques cac des monstres
    - [x] - faire attaque cac de perso
    - [x] - changer le système d'inputs pour pas qu'on ait de la latence
    - [x] - faire un drop d'xp (avec anim) à la mort d'un being -> pour le perso

    - [x] - mettre des portes
    - [x] - faire zones hackables (portes)

    - [x] - faire des animations de hack
    - [x] - faire attaque à distance du perso (utilise des bits de hacker)

- [x] - bring some more light !
    - [x] - faire un shader full bloom qui applique à nos hacks, particles, etc
    - [x] - faire un shadergraph qui illumine certaines parties du sprite
            avec emission map
    - [x] - auto generer des lights à nos lumières

- [x] - transformer Item en Monobievor
    - [x] - l'inventory du perso est un canva fils de perso et parent des Items
    - [x] - meme principie pour les coffres
    - [ ] - faire un InventoryManager qui repère le nombre d'inventaires affichés et ajuste leurs positions + sert à basculer les items d'un inventaire à l'autre
    - [x] - (g pa fait mdr mais g fait autrement ça marche mm mieux)


- [x] - recalibrage : quand on attaque, on divise le nombre de dégats fait a
        chaque ennemis touchés par le nb d'ennemis touchés (logique)

- [x] - UX
    - [x] - ajouter des descriptions qui s'affichent au hoover de la souris sur
    les items & les skills
    - [x] - basculer le controle du hacking à la souris : on peut cliquer sur les zombis
    qui allument leur contour lors d'un hoover

- [x] - améliorer gameplay
    - [x] - mettre un sprint de vitesse
    - [x] - faire des "avancements souples", que le joueur puisse avancer
            meme si y'a colliding si c un peu (tah dead cells)
    - [x] - appliquer des forces (knockback etc)


- [x] - ajouter items
    - [x] - lunettes de vitesse : permettent de voir dans le noir
    - [x] - puce de hacking : permet de sélectionner un hack parmi plusieurs en maintenant la souris

- [x] - améliorer UX
    - [x] - ne pas changer material pour hoover de hack
    - [x] - afficher hack_ray blanc et faded qui devient actif quand on appuie