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
		- DONE re-set JF for ui:
		  :LOGBOOK:
		  CLOCK: [2025-11-04 Tue 20:03:22]--[2025-11-04 Tue 21:01:58] =>  00:58:36
		  :END:
			- DONE exploit_wheel
			- DONE gamepad controls x2 (L & R)
			  :LOGBOOK:
			  CLOCK: [2025-11-04 Tue 20:03:15]--[2025-11-04 Tue 20:03:16] =>  00:00:01
			  :END:
			- DONE hack instructions ??
		-
		- TODO transformer les inputs d'activation en [[EndlessInput]] ou fin en fait plutot des **HoldInput** pcq on a pas besoin du endless, juste du hold. quand on hold ça déclenche [[UI_ItemMover]] .SelectPotentialMovingItem(), ce qui se déclenche aussi si on navigate avant que le Hold arrive (mais qu'on maintient quand même)
	-
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
		  -> comment récupérer le panel vers lequel naviguer ?
		- DONE faire que la molette / mouvement de la souris fasse naviguer les [[UI_Panel]] de l'inventaire
	-
		-
	- ### Moving Items
		- géré grâce au [[UI_ItemMover]]
		- permet de bouger des [[UI_Item]] à travers un/plusieurs [[UI_Inventory]]
		- DONE retracer bug qui remet les ui_module slots en tant que slots dans le **Navigator** après avoir move un item pour la 1e fois
		  -> peut-etre ça vient du **GetSlots()** de [[UI_Inventory]]
		  -> et [[UI_InventoryMenu]] .RefreshItemPools() qui active le gameobject de la motherboard alors qu'on a pas de laptop
		- DONE encore avec la motherboard qui s'affiche pas quand on remet un laptop
		- DONE bug quand on move un [[UI_Module]] sur la motherboard sur un empty_slot bah ça sauvegarde pas correctement la position dans le [[LaptopInventory]]
		  :LOGBOOK:
		  CLOCK: [2025-11-06 Thu 13:31:11]
		  CLOCK: [2025-11-06 Thu 13:31:15]--[2025-11-06 Thu 14:03:44] =>  00:32:29
		  :END:
		-
	-
	-
	-
	- ### KeyFeedback d'interaction
		- TODO rework en faisant un seul [[KeyFeedback]] d'interaction et les [[UI_Slottable]] ont une méthode qui détermine la position adéquate en fonction du [[UI_Slot]] actuel du navigator
		- TODO les [[Interactable]] ont d'ailleurs aussi cette méthode **GetInteractKFPosition()** cad que le KF est vraiment single et déconnecté des capables et des ui_chest, ça se fait que au niveau du ui_manager ?
		- TODO basic chests placent mal leur KF interact