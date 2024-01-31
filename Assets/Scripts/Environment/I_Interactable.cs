using UnityEngine;

public interface I_Interactable
{

    // fonctions
    bool isInteractable(); // est interactable maintenant ?
    void interact(GameObject target); // interaction
    void stopInteract(); // arrête l'interaction

}