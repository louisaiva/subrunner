
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// HooverCapacity is a capacity that allows a Capable to hoover.
/// It shows a hoover animation and sets the performed action in order
/// to launches the Interactable.OnInteract() method.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class HooverCapacity : Capacity
{
    [Header("Hoover parameters")]
    public Interactable capable;

    [Header("Callbacks")]
    public InputActionReference interactAction;
    private event Action<InputAction.CallbackContext> interactCallback;
    public bool set_callback = false;


    // START
    private void Start()
    {
        // we get the Capable
        capable = transform.parent.GetComponent<Interactable>();

        // we define the callback
        defineCallback();
    }

    // HOVER
    protected virtual void defineCallback()
    {
        // we define the interact action
        interactCallback = ctx => capable.OnInteract();
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
            Debug.Log("(HooverCapacity) " + name + " set callback OnInteract()");
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
            Debug.Log("(HooverCapacity) " + name + " removed callback OnInteract()");
        }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }
        
        // we hover
        hover((Capable) capable);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }

        // we unhover
        unhover((Capable) capable);
    }

}