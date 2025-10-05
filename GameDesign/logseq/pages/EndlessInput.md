## Description
- Cette classe permet de créer nos propres events d'inputs qui peuvent ensuite être lus par le #[[PIC (PersoInputsController)]] & le #[[UIC (UI_InputsController)]]
- Cette classe permet de régler les bugs d'**Inputs** que possèdent l'actuelle version des endless inputs sur le #[[PIC (PersoInputsController)]] & le #[[UIC (UI_InputsController)]] qui ne cancel pas proprement leurs values ce qui fait que des fois quand on mitraille ça saute un cancel
-
- de plus c'est bcp mieux de faire une seule classe **EndlessInput** avec des **events** auquel le **PIC** peut souscrire plutôt que de réimplementer à chaque fois le systeme des endless inputs dans le **PIC**
-
-
- ## Fonctionnement
-