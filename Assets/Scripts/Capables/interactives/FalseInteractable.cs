using System.Collections.Generic;
using UnityEngine;

public class FalseInteractable : Capable, EndlessInteractable
{
    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get; set; } = InteractType.Other;

    [Header("Interactions parameters")]
    public string interact_animation = "interact";
    [SerializeField] private bool endless_interact = true;

    [Header("Unity Events")]
    public UnityEngine.Events.UnityEvent OnInteractEvent;
    public UnityEngine.Events.UnityEvent OnEndlessInteractEvent;

    // INTERACTION
    public void OnEndlessInteract(Capable interactor) { if (!endless_interact) { return; } OnInteract(interactor); OnEndlessInteractEvent?.Invoke(); }
    public void OnInteract(Capable interactor)
    {
        // we set the interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        if (debug) { Debug.Log("(FalseInteractable) " + name + " was interacted by " + interactor.name); }

        anim_player.Play(interact_animation);
        OnInteractEvent?.Invoke();
    }
}