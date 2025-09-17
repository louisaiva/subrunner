
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
    public Interactable interactable
    {
        get
        {
            if (closest_hover is Interactable) { return closest_hover as Interactable; }
            return null;
        }
    }

    [Header("Waiting hovers")]
    [SerializeField] private List<Capable> waiting_hovers = new List<Capable>();

    [Header("Item Grab")]
    [SerializeField] private GrabCapacity grab_capacity;
    [SerializeField] private List<string> exclusion_item_rule = new List<string>() { }; // rule to check if an item is interactable with us
    public string ExclusionItemRule
    {
        get
        {
            // since it's an exclusion list we want to make sure that no item passes it if it's empty
            if (exclusion_item_rule.Count == 0) { return "none"; }
            return string.Join(",", exclusion_item_rule);
        }
    }

    // START
    private void Start()
    {
        // we get the grab capacity
        grab_capacity = capable.GetCapacity<GrabCapacity>();
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
    private void select_hover(Capable capable)
    {
        // we switch the current hover
        if (closest_hover != null) { unselect_hover(); }
        closest_hover = capable;
        if (debug) { Debug.Log("(InteractCapacity) " + capable.name + " selected as closest hover"); }

        // we play the hover animation
        closest_hover.GetCapacity<HoverCapacity>()?.Hover(this.capable);

        // if it's an item and we have a grab capacity, we select it
        if (closest_hover is Item item) { grab_capacity?.Select(item); }
    }
    private void unselect_hover()
    {
        // we chack if we have a current hover
        if (closest_hover == null) { return; }

        // we stop the hover animation
        closest_hover.GetCapacity<HoverCapacity>()?.Unhover(this.capable);

        // we remove the callback
        // remove_callbacks(closest_hover);

        // we deselect the grabbing if it's an item
        if (closest_hover is Item) { grab_capacity?.Deselect(); }

        // we reset the current hover
        if (debug) { Debug.Log("(InteractCapacity) " + closest_hover.name + " unselected as closest hover"); }
        closest_hover = null;
    }

    // HANDLE INTERACT INPUT
    public void HandleInteractInput(InputAction.CallbackContext context)
    {
        // if we release the button we direclty interact with it
        if (context.ReadValue<float>() < 0.5f)
        {
            if (interacting_endlessly || interacting_endlessly_waiting_threshold)
            {
                interacting_endlessly_waiting_threshold = false;
                interacting_endlessly = false;
            }

            // interact with interactable & select + grab items
            interact();

            return;
        }

        // else we launches endless interaction
        interact_endlessly();        
    }
    private void interact()
    {
        if (closest_hover == null) { return; }

        // interact with interactable & select + grab items
        if (closest_hover is Interactable interactable) { interactable.OnInteract(capable); }
        else if (closest_hover is Item item) { grab_capacity?.Use(capable); }
    }

    // ENDLESS INTERACT INPUT
    [Header("Interact Endlessly")]
    [SerializeField] private bool interacting_endlessly_waiting_threshold = false;
    [SerializeField] private bool interacting_endlessly = false;
    private async void interact_endlessly()
    {

        // threshold wait
        interacting_endlessly_waiting_threshold = true;
        float elapsed = 0f;
        while (elapsed < InputManager.Instance.BUTTON_ENDLESSLY_LONG_THRESHOLD && interacting_endlessly_waiting_threshold)
        {
            elapsed += Time.deltaTime;
            await System.Threading.Tasks.Task.Yield();
        }
        if (!interacting_endlessly_waiting_threshold) { return; }

        // endless interaction
        interacting_endlessly = true;
        interacting_endlessly_waiting_threshold = false;
        while (interacting_endlessly)
        {
            if (UI_Manager.Instance != null && !UI_Manager.Instance.InPools(new List<string> { "hud", "hacking" })) { break; }

            // interact endlessly if it's an item
            if (closest_hover != null
                && (closest_hover is Item
                || (closest_hover is Interactable interactable && interactable.AuthorizeEndlessInteraction)))
            { interact(); }

            // delay wait
            elapsed = 0f;
            while (elapsed < InputManager.Instance.BUTTON_ENDLESSLY_LONG_DELAY)
            {
                if (!interacting_endlessly) { break; }
                elapsed += Time.deltaTime;
                await System.Threading.Tasks.Task.Yield();
            }
        }

        // deactivate everything
        interacting_endlessly = false;
        interacting_endlessly_waiting_threshold = false;
    }


    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other has a HoverCapacity
        HoverCapacity hover = other.GetComponent<HoverCapacity>();
        if (hover == null) { return; }

        // we get the capable of the hover capacity
        Capable interactive = hover.capable;
        if (interactive == null) { return; }

        // we check if it's an Interactable or an Item
        if (interactive is not Interactable && interactive is not Item) { return; }
        if (interactive is Item item && item.ValidateRule(ExclusionItemRule)) { return; } // we check if the item is excluded by the rule
        // if (interactive is Hackable hackable && !hackable.CanInteract(capable)) { return; } // we check if the hackable can be interacted with

        // we check if the capable is already hovered
        if (interactive == closest_hover) { return; }

        // or if it's already in the waiting hovers
        if (waiting_hovers.Contains(interactive)) { return; }

        // we add the capable to the waiting hovers
        waiting_hovers.Add(interactive);

        if (debug) { Debug.Log("(InteractCapacity) " + interactive.name + " added to waiting hovers"); }
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

    // DESTROY
    /* private void OnDestroy()
    {
        // we remove all callbacks
        // if (closest_hover is Interactable || closest_hover is Item) { remove_callbacks(closest_hover); }
        interactAction.performed -= interactCallback;
    } */
}