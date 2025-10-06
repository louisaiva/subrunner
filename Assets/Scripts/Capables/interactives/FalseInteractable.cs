using System.Collections.Generic;
using UnityEngine;

public class FalseInteractable : Capable, Interactable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public bool AuthorizeEndlessInteraction => true;


    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        if (debug) { Debug.Log("(FalseInteractable) " + name + " was interacted by " + interactor.name); }

        anim_player.Play("interact");
    }
}