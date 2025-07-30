
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class InteractHackCapacity : Capacity
{
    [Header("Current Hover")]
    [SerializeField] private Hackable closest_hover;

    [Header("Waiting hovers")]
    [SerializeField] private List<Hackable> waiting_hovers = new List<Hackable>();

    [Header("Hack selection")]
    [SerializeField] private HackCapacity hacker;

    // START
    private void Start()
    {
        // we get the hack capacity
        hacker = capable.GetCapacity<HackCapacity>();
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
        if (Vector2.Distance(closest_hover.transform.position, capable.transform.position)
            < Vector2.Distance(waiting_hovers[0].transform.position, capable.transform.position)) { return; }

        // we switch the current hover
        waiting_hovers.Add(closest_hover);
        unselect_hover();
        select_hover(waiting_hovers[0]);
        waiting_hovers.RemoveAt(0);
    }

    // INTERACTABLE SELECTION
    private void select_hover(Hackable hackable)
    {
        // we switch the current hover
        if (closest_hover != null) { unselect_hover(); }
        closest_hover = hackable;
        if (debug) { Debug.Log("(InteractHackCapacity) " + hackable.name + " selected as closest hover"); }

        // we play the hover animation
        closest_hover.gameObject.GetComponent<Capable>().GetCapacity<HoverCapacity>()?.Hover(this.capable);

        hacker?.Select(closest_hover);
    }
    private void unselect_hover()
    {
        // we chack if we have a current hover
        if (closest_hover == null) { return; }

        // we stop the hover animation
        closest_hover.gameObject.GetComponent<Capable>().GetCapacity<HoverCapacity>()?.Unhover(this.capable);

        hacker?.Deselect();

        // we reset the current hover
        if (debug) { Debug.Log("(InteractHackCapacity) " + closest_hover.name + " unselected as closest hover"); }
        closest_hover = null;
    }

    // CALLBACKS
    /* public void set_callbacks(Hackable hackable)
    {
        // we define the interact action
        interactCallback = ctx =>
        {
            if (ctx.ReadValue<float>() > 0.5f) { return; } // we verify that the button was released
            hackable.OnInteract(capable);
        };

        // we set the callback
        interactAction.performed += interactCallback;

        // we set the callback as set
        callback_is_set = true;

        if (debug) { Debug.Log("(InteractHackCapacity) " + capable.name + " set callback OnInteract() on " + (hackable as Capable).name); }
    }
    public void remove_callbacks(Hackable hackable)
    {
        // we remove the callback
        interactAction.performed -= interactCallback;

        // we set the callback as not set
        callback_is_set = false;

        if (debug) { Debug.Log("(InteractHackCapacity) " + capable.name + " removed callback OnInteract() on " + (hackable as Capable).name); }
    } */

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponent<HoverCapacity>();
        if (hover == null) { hover = other.GetComponentInParent<HoverCapacity>(); }
        if (hover == null) { return; }

        // we get the hackable of the hover capacity
        Hackable hack_target = hover.capable as Hackable;
        if (hack_target == null) { return; }

        // we check if the hackable is already hovered
        if (hack_target == closest_hover) { return; }

        // or if it's already in the waiting hovers
        if (waiting_hovers.Contains(hack_target)) { return; }

        // we add the hackable to the waiting hovers
        waiting_hovers.Add(hack_target);

        if (debug) { Debug.Log("(InteractHackCapacity) " + hack_target.name + " added to waiting hovers"); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponentInParent<HoverCapacity>();
        if (hover == null) { return; }

        // we get the hackable of the hover capacity
        Hackable hack_target = hover.capable as Hackable;
        if (hack_target == null) { return; }

        // we check if the hackable is the current hover
        if (hack_target == closest_hover)
        {
            unselect_hover();
            return;
        }

        // we check if the hack_target is in the waiting hovers
        if (waiting_hovers.Contains(hack_target))
        {
            waiting_hovers.Remove(hack_target);
            if (debug) { Debug.Log("(InteractHackCapacity) " + hack_target.name + " removed from waiting hovers"); }
        }
    }

    // DESTROY
    /* private void OnDestroy()
    {
        // we remove all callbacks
        if (closest_hover is Hackable hackable) { remove_callbacks(hackable); }
    } */
}