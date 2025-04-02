using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : Capable, Interactable, Openable
{
    // OPENABLE
    public bool is_open { get; set; }
    public bool is_moving { get; set; }

    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        if (debug) { Debug.Log("(Chest) " + name + " was interacted by " + interactor.name); }

        // we react to the interaction
        if (Can("open")) { Do("open"); }
        else if (Can("close")) { Do("close"); }
    }

}