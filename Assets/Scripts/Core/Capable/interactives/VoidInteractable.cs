using System.Collections.Generic;
using UnityEngine;

public class VoidInteractable : Capable, Interactable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get; set; } = InteractType.Other;

    // INTERACTION
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();
        if (log) { Debug.Log("(VoidInteractable) " + name + " was interacted by " + interactor.name); }
    }
}