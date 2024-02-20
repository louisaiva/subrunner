


# SUBRUNNER

## STORY

subrunner is a 2D top-down view rogue-lite video game set in the underground of a cyberpunk city.
You play as a small ninja hacker trying to get back to the surface to buy some noodles.

Beware, your path may cross the road of cyberzombies and other dangerous creatures.
You'll cross paths with items to help you unlock abilities that will allow you to explore more and more.

The averall goal is to unlock the access to the elevator and defeat the final boss.
In order to do so, you'll have to hack firewalls and other devices to unlock the path.

![welcome to subrunner](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/welcome.png?raw=true)

## GAMEPLAY

### CONTROLS

#### Keyboard

*W/S/A/D* - Move
*Maj (hold)* - Sprint
*Space* - Attack
*E* - Interact with objects
*F* - Open inventory
*Mouse movement* - Select hack target
*Mouse click* - Hack target
*Esc* - Pause
*Tab* - Open map
*Alt* - Use the dash ability
*R* - Use the consumable

#### Gamepad

![gamepad](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/manette2.png?raw=true)


### ITEMS

Items are the core of the game. They unlock abilities that let you explore the level. They are divided in several categories:

#### Legendary items

These are the most powerful items of the game. They are unique and can only be found in rooms that hold a LegendaryItem Zone. They unlock unique abilities that can be used to unlock the path to the elevator.

They are always the same between games, and are the same for all players.

Each Legendary item has its own specificities, abilities tree and may interact with other items.

They are 6 legendary items in the game :

- The **Katana** : a katana that allows you to attack enemies. Can be forged with other items to create different weapons
- The **Glasses** : the glasses allow you to see in the dark of the underground. Allows you to sprint when you upgrade it, and more.
- The **Shoes** : the shoes allow you to dash. Can be upgraded to allow you to get an upgraded dash.
- The **Gyroscope** : records the rooms you've been in. Unlock the map. Can be upgraded to see the enemies on the map.
- The **NOOD_OS** : the main hackin tool of the game. Can be upgraded to hack more efficiently, and more. Uses bits and hacks to hack things.
- The **Recorder** : can record and play sounds. Uses CDs to play musics that can have different effects on the enemies.

Each legendary item plays a sound and an animation when you get close to it.
![legendary zone](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/legendary_item.png?raw=true)


#### Hacks

The hacks are virtual items that can be found in computers or other devices. They are used to unlock doors, hack cyberzombies, hack computers, ...
You need to find the NOOD_OS to use them.

#### Consumables

The consumables are items that can be used to heal (water), to explode doors (dynamite), to attack (shurikens)...
They can be equipped and used with the *R* key.


### HACKING

When you find the NOOD_OS, you can use hacks to hack devices. This gameplay system replaces the classic "mana & spells" system of a lot of games. It is a core mechanic of the game, but you don't have to use it to finish the game.

A lot of hacks can be found in the game and can be used to hack & attack cyborgs and other electronic machines.
For now the game only has one simple damaging hack that deals damage to the target.
![hacking](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/hackin.png?raw=true)

In order to hack something, you need to have the right hack and enough bits. Bits regenerate over time and you can upgrade your NOOD_OS to regenerate them faster. Hacking a cyberzombie costs 1 bit for now, but future enemies will cost more bits.
You can definitely hack serveral targets at the same time if you have enough bits.

### SKILLTREES

The game has a skilltree system that allows you to upgrade your character and several items. For now the game has 2 skilltrees, one for the character and one for the NOOD_OS.

#### Character skilltree

You can earn XP by killing enemies and opening XP chests. When you level up, you can upgrade a specific skill :
![hacking](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/skilltree_xp.png?raw=true)

#### NOOD_OS skilltree

When you find a Computer station in the game, you will be able to upgrade your NOOD_OS with this skilltree :
![hacking](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/skilltree_hacking.png?raw=true)


## LEVEL DESIGN

The whole game is set in a 2D top-down view. It is for now composed of one only level, divided in several sectors, each divided in several rooms. In the future more levels will be added.
The rooms are handcrafted, however their position in their sector is procedurally generated, as well as the position of the sectors in the level.

### Level generation

The level is composed of several sectors, disposed procedurally view the algorithm of the TinyKeep game (https://www.gamedeveloper.com/programming/procedural-dungeon-generation-algorithm). The sectors are connected by doors, unlocked by hacking a firewall or using the right ability (for this you need to find items in the rooms).

Here is an example of the averall look of a level generated by the game:
![macro map](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/procedural_sectors_macro.png?raw=true)

Dark links represent the links between the sectors, and the blue sectors are handcrafted sectors (see below).


### Sector generation

Each sector is procedurally generated by assembling rooms. The game use a Random Walk algorithm to generate the rooms.

Here you can see the same level as above, but zoomed in to see the rooms:
![sector map](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/procedural_sectors.png?raw=true)


#### Handcrafted sectors

Some sectors needs to be handcrafted to fit the story and the gameplay. Here is an example of a handcrafted sector:
![HQ](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/handmade_sector.png?raw=true)

This is the spawn sector, where the player starts the game. It is the home of the player, and will upgrade as the player progresses in the game.

#### Rooms skins

To make the game more interesting, the rooms are skinned with different skins. Here are some examples of skins:
![base skin](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/skin_1.png?raw=true)
![server skin](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/skin_2.png?raw=true)
![labo skin](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/skin_3.png?raw=true)
![mecha skin](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/skin_4.png?raw=true)


#### Zones

In order to diversify the game even more, the rooms can sometimes hold a zone. Zones are a combination of objects that could be only decorative, or could be interactive. Here are some examples of zones:
![zone 1](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/zone.png?raw=true)
![zone 2](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/zone_2.png?raw=true)

Some Zones are uniques, such as LegendaryItem Zones that hold a legendary item, or Boss Zones that hold the boss of the sector.
![legendary zone](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/legendary_item2.png?raw=true)


## UI

The game has several UI panels that will be displayed on the screen, such as :

### In-game HUD

The In-game HUD is the main UI panel of the game. It displays the health of the player, the xp, the bits and the mini-map (if you have the according items).
![hacking](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/in_game_hud.png?raw=true)

### Inventory

The inventory is the panel that displays the items you have collected. You can equip and use the consumables from this panel.
![hacking](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/inventory.png?raw=true)

### Map

The map is a panel that displays the map of the level. It is unlocked by the Gyroscope item. It displays only the rooms you've been in.
![hacking](https://github.com/louisaiva/subrunner/blob/master/Assets/Resources/exports/map.png?raw=true)