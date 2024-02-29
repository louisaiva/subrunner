using UnityEngine;

public interface I_Interactable
{
    // propriétés
    // string interact_tuto_label { get; set; }
    Transform interact_tuto_label { get; set; }

    // fonctions
    bool isInteractable(); // est interactable maintenant ?
    void interact(GameObject target); // interaction
    void stopInteract(); // arrête l'interaction

    void OnPlayerInteractRangeEnter(); // quand le joueur entre dans la range d'interaction
    void OnPlayerInteractRangeExit(); // quand le joueur sort de la range d'interaction

}