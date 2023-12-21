using UnityEngine;

public interface I_LongInteractable : I_Interactable
{


    // interactions
    bool is_being_activated { get; set; } // est en train de se faire activer
    float activation_time { get; set; } // temps d'activation

    // fonctions
    void startActivating(); // commence l'activation
    void quitActivating(); // arrête l'activation
    void succeedActivating(); // activation lorsqu'on a fini d'activer
}