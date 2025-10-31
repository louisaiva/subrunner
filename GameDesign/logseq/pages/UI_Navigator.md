### Singleton
-
- anciennement **UI_XboxNavigator**, devient désormais **UI_Navigator** et permet de naviguer à travers les différents **UI** qui sont basé sur des **Slots**
-
-
-
- # Rework
	- ## Inputs
		- DOING : transférer tous les **Inputs** sur le #[[UIC (UI_InputsController)]]
		  :LOGBOOK:
		  CLOCK: [2025-10-31 Fri 00:09:22]
		  :END:
	- ## Current Slot
		- on garde pas un int index en current slot index mais on garde direct le current slot
		  -> evite des galeres de remettre à jour etc
		  -> on peut verifier simplement si le current slot a changé / ne fait plus partie des slottables
		- on register le current slot à l'event #UI_Slot .OnDisable comme ça on peut faire -> navigate to closest direct
	- ### Souris & clavier
		- souris + clavier ont pas besoin de method navigate etc
		  -> peut-etre faire 2 sous scripts qui gère le clavier souris / gamepad + navigation ?
	- ### Moving Items
		- faire un script séparé lol evidemment
-