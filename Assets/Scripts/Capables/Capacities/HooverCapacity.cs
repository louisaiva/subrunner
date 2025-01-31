
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
    public Spawner capable;
    public Collider2D hoover_collider;

    [Header("INPUTS")]
    [SerializeField] private InputManager input_manager;
    public PlayerInputActions playerInputs;
    private event Action<InputAction.CallbackContext> interactCallback;
    public bool set_callback = false;


    // START
    private void Start()
    {
        // we get the hoover collider
        hoover_collider = GetComponent<Collider2D>();
        capable = transform.parent.GetComponent<Spawner>();

        // on récupère les inputs
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        playerInputs = input_manager.inputs;
    }

    // USE
    public override void Use(Capable capable)
    {
        base.Use(capable);

        if (!set_callback)
        {
            set_callback = true;
            interactCallback = ctx => this.capable.OnInteract();
            playerInputs.perso.interact.performed += interactCallback;
            Debug.Log("(HooverCapacity) " + name + " set callback OnInteract()");
        }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Use(capable);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }

        // we stop the animation
        stop_playing(capable.anim_player, name);

        // we remove the callback
        if (!set_callback) { return; }
        playerInputs.perso.interact.performed -= interactCallback;
        Debug.Log("(HooverCapacity) "+ name + " removed callback OnInteract()");
        set_callback = false;
    }

}