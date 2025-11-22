using System.Collections.Generic;
using UnityEngine;

public class FalseInteractable : Capable, EndlessInteractable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get; set; } = InteractType.Other;

    // INTERACTION
    public void OnEndlessInteract(Capable interactor) { OnInteract(interactor); }
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        if (debug) { Debug.Log("(FalseInteractable) " + name + " was interacted by " + interactor.name); }

        anim_player.Play("interact");
    }
}