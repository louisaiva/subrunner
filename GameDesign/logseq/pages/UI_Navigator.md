### Singleton
-
- anciennement **UI_XboxNavigator**, devient désormais **UI_Navigator** et permet de naviguer à travers les différents **UI** qui sont basé sur des **Slots**
-
-
-
- # Rework
	- DONE faire 2 sous scripts qui gère le clavier souris / gamepad + navigation ?
	-
	- ## Inputs
		- DOING : transférer tous les **Inputs** sur le #[[UIC (UI_InputsController)]]
		  :LOGBOOK:
		  CLOCK: [2025-10-31 Fri 00:09:22]
		  :END:
		-
	- ## Current Slot
		- DONE on garde pas un int index en current slot index mais on garde direct le current slot
		  -> evite des galeres de remettre à jour etc
		  -> on peut verifier simplement si le current slot a changé / ne fait plus partie des slottables
		- DONE on register le current slot à l'event #UI_Slot .OnDisable comme ça on peut faire
		  -> navigate to closest direct
		  -> seulement pour gamepad !!!
		-
	- ### GamepadNavigator
	- ### MouseNavigator
		- souris + clavier ont pas besoin de method auto-navigate etc
		-
		- TODO quand on ouvre l'inventaire et qu'on a aucun item sur le panel principal ça ne va pas dans les shortcuts...
		  -> on doit naviguer direct ?
		- TODO faire que la molette / mouvement de la souris fasse naviguer les [[UI_Panel]] de l'inventaire
	-
		-
	- ### Moving Items
		- géré différemment en fonction du navigator choisi
-