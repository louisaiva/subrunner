using UnityEngine;
using UnityEngine.InputSystem;

public class LootableMeat : Meat, Interactable, Openable
{
    // OPENABLE
    public bool is_open { get; set; }
    public bool is_moving { get; set; }

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public bool AuthorizeEndlessInteraction => true;
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        if (debug) { Debug.Log("(LootableMeat) " + name + " was interacted by " + interactor.name); }

        // we react to the interaction
        if (Can("open")) { Do("open"); }
    }

    public void TurnToMeat()
    {
        
    }
}