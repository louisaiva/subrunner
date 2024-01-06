using UnityEngine;

public interface I_Interactable
{


    // interactions
    // bool is_interacting { get; set; } // est en train d'interagir


    // fonctions
    bool isInteractable(); // est interactable maintenant ?
    void interact(); // interaction
    void stopInteract(); // arrête l'interaction

}