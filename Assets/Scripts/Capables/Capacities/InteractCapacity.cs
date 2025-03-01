
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InteractCapacity is a capacity that allows a Capable to interact with (Interactable/Item) objects.
/// When approaching an (Interactable/Item), the capacity make the object play hover.
/// Then, if it s an Item and if our capable has a GrabCapacity, it select it (for preparing the grab)
/// Otherwise if it's an Interactable, it sets the performed action in order to launches the Interactable.OnInteract() method.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class InteractCapacity : Capacity
{


    [Header("Current Hover")]
    [SerializeField] private Capable closest_hover;

    [Header("Waiting hovers")]
    [SerializeField] private List<Capable> waiting_hovers = new List<Capable>();

    [Header("Input & Callbacks")]
    [SerializeField] private InputActionReference interactInput;
    private InputAction interactAction;
    private event Action<InputAction.CallbackContext> interactCallback;
    [SerializeField] private bool callback_is_set = false;

    // START
    private void Start()
    {
        // we get the interact action
        interactAction = GameObject.Find("/utils/input_manager").GetComponent<InputManager>().GetAction(interactInput);
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // we check if we have a something in the waiting hovers
        if (waiting_hovers.Count == 0) { return; }

        // we update the waiting hovers by distance
        waiting_hovers.Sort((a, b) => Vector2.Distance(a.transform.position, capable.transform.position).CompareTo(Vector2.Distance(b.transform.position, capable.transform.position)));

        // we check if we have a current hover
        if (closest_hover == null)
        {
            // we select the hover of the first waiting hover
            select_hover(waiting_hovers[0]);
            waiting_hovers.RemoveAt(0);

            return;
        }

        // we check if the current hover is still the closest
        if (Vector2.Distance(closest_hover.transform.position, capable.transform.position) < Vector2.Distance(waiting_hovers[0].transform.position, capable.transform.position))
        {
            return;
        }

        // we switch the current hover
        waiting_hovers.Add(closest_hover);
        unselect_hover();
        select_hover(waiting_hovers[0]);
        waiting_hovers.RemoveAt(0);
    }

    // INTERACTABLE SELECTION
    private void select_hover(Capable capable)
    {
        // we switch the current hover
        if (closest_hover != null) { unselect_hover(); }
        closest_hover = capable;
        if (debug) { Debug.Log("(InteractCapacity) " + capable.name + " selected as closest hover"); }

        // we play the hover animation
        closest_hover.GetCapacity<HoverCapacity>()?.Hover(this.capable);
        // if (closest_hover.Can("hover")) { closest_hover.Do("hover"); }

        // we check if it's an Item or an Interactable
        if (closest_hover is Interactable)
        {
            // we set the callback
            set_callbacks(closest_hover as Interactable);
        }
    }
    private void unselect_hover()
    {
        // we chack if we have a current hover
        if (closest_hover == null) { return; }

        // we stop the hover animation
        closest_hover.GetCapacity<HoverCapacity>()?.Unhover(this.capable);

        // we remove the callback
        if (closest_hover is Interactable)
        {
            remove_callbacks(closest_hover as Interactable);
        }

        // we reset the current hover
        if (debug) { Debug.Log("(InteractCapacity) " + closest_hover.name + " unselected as closest hover"); }
        closest_hover = null;
    }

    // CALLBACKS
    public void set_callbacks(Interactable interactable)
    {
        // we define the interact action
        interactCallback = ctx => interactable.OnInteract();

        // we set the callback
        interactAction.performed += interactCallback;

        // we set the callback as set
        callback_is_set = true;

        if (debug) { Debug.Log("(InteractCapacity) " + capable.name + " set callback OnInteract() on " + (interactable as Capable).name); }
    }
    public void remove_callbacks(Interactable interactable)
    {
        // we remove the callback
        interactAction.performed -= interactCallback;

        // we set the callback as not set
        callback_is_set = false;

        if (debug) { Debug.Log("(InteractCapacity) " + capable.name + " removed callback OnInteract() on " + (interactable as Capable).name); }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponent<HoverCapacity>();
        if (hover == null) { return; }
        
        // we get the capable of the hover capacity
        Capable capable = hover.capable;
        if (capable == null) { return; }

        // we check if it's an Interactable or an Item
        if (capable is not Interactable && capable is not Item) { return; }
        
        // we check if the capable is already hovered
        if (capable == closest_hover) { return; }

        // or if it's already in the waiting hovers
        if (waiting_hovers.Contains(capable)) { return; }

        // we add the capable to the waiting hovers
        waiting_hovers.Add(capable);

        if (debug) { Debug.Log("(InteractCapacity) " + capable.name + " added to waiting hovers"); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponent<HoverCapacity>();
        if (hover == null) { return; }
        
        // we get the capable of the hover capacity
        Capable capable = hover.capable;
        if (capable == null) { return; }

        // we check if it's an Interactable or an Item
        if (capable is not Interactable && capable is not Item) { return; }
        
        // we check if the capable is the current hover
        if (capable == closest_hover)
        {
            unselect_hover();
            return;
        }

        // we check if the capable is in the waiting hovers
        if (waiting_hovers.Contains(capable))
        {
            waiting_hovers.Remove(capable);
            if (debug) { Debug.Log("(InteractCapacity) " + capable.name + " removed from waiting hovers"); }
        }
    }
}