
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// HoverCapacity is a capacity that allows a Capable to hoover.
/// It shows a hoover animation and sets the performed action in order
/// to launches the Interactable.OnInteract() method.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class HoverCapacity : Capacity
{

    [Header("Callbacks")]
    public InputActionReference interactAction;
    private event Action<InputAction.CallbackContext> interactCallback;
    public bool set_callback = false;

    [Header("Debug")]
    public bool debug = false;


    // START
    private void Start()
    {

        // we define the callback
        defineCallback();
    }

    // HOVER
    protected virtual void defineCallback()
    {
        // we define the interact action
        interactCallback = ctx => (capable as Interactable).OnInteract();
    }
    protected virtual void hover(Capable capable)
    {
        // we play the animation
        Use(capable);

        // we set the callback
        if (!set_callback)
        {
            set_callback = true;
            interactAction.action.performed += interactCallback;
            if (debug) { Debug.Log("(HoverCapacity) " + name + " set callback OnInteract() on " + capable.name); }
        }
    }
    protected virtual void unhover(Capable capable)
    {
        // we stop the animation
        stop_playing(capable.anim_player, name);

        // we remove the callback
        if (set_callback)
        {
            set_callback = false;
            interactAction.action.performed -= interactCallback;
            if (debug) { Debug.Log("(HoverCapacity) " + name + " removed callback OnInteract() on " + capable.name); }
        }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }
        
        // we hover
        hover(capable);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }

        // we unhover
        unhover(capable);
    }

}