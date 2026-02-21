
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Toggle capacity save a state. Which can be either off or on.
/// When on, multiple animations (idle, hover, interact) can be overriden so the right animations apply
/// </summary>

public class ToggleCapacity : Capacity
{

    [Header("Toggle")]
    [SerializeField] private bool TOGGLED_ON = false; // est-ce que le toggle est on ou off


    [Header("Animations parameters")]
    [SerializeField] private string idle_off_anim = "idle";
    [SerializeField] private string idle_on_anim = "idle_on";
    [SerializeField] private string hover_off_anim = "hover";
    [SerializeField] private string hover_on_anim = "hover_on";
    [SerializeField] private string interact_off_anim = "onnin";
    [SerializeField] private string interact_on_anim = "offin";

    [Header("Components")]
    [SerializeField] private AnimPlayer player;
    [SerializeField] private HoverCapacity hover;
    [SerializeField] private FalseInteractable interactable;

    [Header("Unity Event")]
    public UnityEngine.Events.UnityEvent OnToggleON;
    public UnityEngine.Events.UnityEvent OnToggleOFF;

    // AWAKE
    private void Start()
    {
        if (player == null) { player = capable.anim_player; }
        if (hover == null) { hover = capable.GetCapacity<HoverCapacity>(); }
        if (interactable == null)
        {
            if (capable is FalseInteractable) { interactable = (FalseInteractable)capable; }
        }
    }

    // TOGGLE ON / OFF
    public void Toggle() { 
        if (TOGGLED_ON) { TurnOFF(); }
        else { TurnON(); }
    }
    public void TurnON()
    {
        // we switch idle animation
        player.AddToPile(idle_on_anim);

        // we switch hover animation
        if (hover != null) { hover.ChangeAnimation(hover_on_anim); }

        // we switch interact animation
        if (interactable != null) { interactable.interact_animation = interact_on_anim; }

        TOGGLED_ON = true;
        OnToggleON?.Invoke();
    }
    public void TurnOFF()
    {
        // we switch idle animation
        player.StopPlaying(idle_on_anim);

        // we switch hover animation
        if (hover != null) { hover.ChangeAnimation(hover_off_anim); }

        // we switch interact animation
        if (interactable != null) { interactable.interact_animation = interact_off_anim; }

        TOGGLED_ON = false;
        OnToggleOFF?.Invoke();
    }

}