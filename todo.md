# TODO



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



- [ ] - améliorer gameplay
    - [ ] - mettre un sprint de vitesse
    - [ ] - faire des "avancements souples", que le joueur puisse avancer
            meme si y'a colliding si c un peu (tah dead cells)

- [ ] - ajouter items
    - [x] - lunettes de vitesse : permettent de voir dans le noir
    - [ ] - puce de hacking : permet de sélectionner un hack parmi plusieurs en maintenant la souris
    
- [ ] - mettre des modifieurs d'items
    - [ ] - pour les hacks : stat de propagation automatique à un ennemi proche
    - [ ] - acceleration de la vitesse de hack


- [ ] - faire une génération aléatoire des niveaux
    - [ ] - mettre en place le TK algorithm pour créer les salles
    - [ ] - créer une classe Room qui remplit les salles avec les tiles
    - [ ] - faire des plafonds qu'on peut passer dessous avec notre perso
            parce que ça rend bien ! genre dans les couloirs


- [ ] - UX
    - [x] - ajouter des descriptions qui s'affichent au hoover de la souris sur
    les items & les skills
    - [ ] - basculer le controle du hacking à la souris : on peut cliquer sur les zombis
    qui allument leur contour lors d'un hoover

# BUGS

- [ ] - quand on est loin de xp-provider ça arrete de générer de l'xp ?
- [ ] - bug graphique d'animations : les zombos n'attaquent pas
- [ ] - bug ordi freeze à la fin de l'animation idle_on

# DO IT LATER

- [-] - faire un fichier json qui stocke les paramètres des différents beings

# DONE

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
