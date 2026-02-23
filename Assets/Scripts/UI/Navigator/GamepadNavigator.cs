using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GamepadNavigator is a navigation plugin for the UI_Navigator
/// that is responsible for caculating navigation with a gamepad
/// </summary>
public class GamepadNavigator : MonoBehaviour, Navigator
{

    [Header("Navigation Parameters")]
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 0f;
    public Vector2 BasePosition => new Vector2(Screen.width / 2f, Screen.height / 2f);
    public List<Type> UnwantedTypesAfterActivate => new List<Type>() { typeof(UI_OutlineSlot) };

    [Header("Dot Navigation Settings")]
    [SerializeField] private bool use_dot_navigation = false;
    // [SerializeField] private float distance_over_dot_preference = 100f;

    [Header("Components")]
    private UI_Navigator manager;
    public UI_Navigator Manager
    {
        get
        {
            if (manager == null) { manager = GetComponent<UI_Navigator>(); }
            return manager;
        }
    }


    [Header("Logs")]
    [SerializeField] private bool log = false;

    // HANDLE SLOTTABLE ACTIVATION
    public void ActivateSlottable(Slottable slottable)
    {
        // on navigue vers le plus proche
        if (log) { Debug.Log("(UI_GamepadNavigator) activating slottable : " + slottable.name + $" (starting slot is : {(slottable.StartingSlot != null ? slottable.StartingSlot.name : "null")})"); }
        if (slottable.StartingSlot == null) { NavigateToClosest(BasePosition); return; }

        // on navigue vers le slot souhaité
        if (log) { Debug.Log("(UI_GamepadNavigator) navigating to starting slot : " + slottable.StartingSlot.name); }

        // get slot position
        NavigateToSlot(slottable.StartingSlot);
        // Manager.HoverSlot(slottable.StartingSlot);
    }

    // NAVIGATION ALGORITHMS
    public void Navigate(Vector2 direction)
    {
        if (log) { Debug.Log("(UI_GamepadNavigator) navigating : " + direction); }

        // on ne navigue pas si on essaie de drop et qu'on a pas d'ui_item
        if (Manager.CurrentSlot != null
            && Controller.Instance != null
            && Controller.Instance.UIC.IsDropInputDown()
            && Manager.CurrentSlot is not UI_ItemStack) { return; }

        // on move item potentiellement
        Manager.StartMovingItemIfInputDown();

        // on récupère les Manager.Slots
        Manager.UpdateSlots();

        // on récupère le slot le plus proche dans la direction
        UI_Slot next_slot = null;
        if (use_dot_navigation) { next_slot = navigate_dot(direction); }
        else { next_slot = navigate_distance(direction); }
        if (next_slot == null) { return; }

        // on navigue vers le slot si on en a un
        Manager.HoverSlot(next_slot);
    }
    protected UI_Slot navigate_distance(Vector2 direction)
    {

        string s = "(UI_GamepadNavigator) NAVIGATE: \n\nparameters: \n\tangle_threshold : " + angle_threshold + "\n\tangle_multiplicator: " + angle_multiplicator + "\n\n";

        if (Controller.Instance != null) { s += "\n\ndropping :\n\tis_drop_input_down : " + Controller.Instance.UIC.IsDropInputDown() + "\n\tcurrent slot type : " + (Manager.CurrentSlot != null ? Manager.CurrentSlot.GetType().Name : "null") + "\n\n"; }

        // on récupère la position du slot actuel
        Vector2 current_slot_position = Manager.GetPosition(Manager.CurrentSlot);
        if (Manager.CurrentSlot != null) { s += "current slot : " + Manager.CurrentSlot.name + " / position : " + current_slot_position + "\n\n"; }
        else { s += "no current slot\n\n"; }

        // on récupère les Manager.Slots dans le bon angle
        List<UI_Slot> slots_in_angle = new List<UI_Slot>();
        for (int i = 0; i < Manager.Slots.Count; i++)
        {
            UI_Slot slot = Manager.Slots[i];
            // on vérifie que ce n'est pas le slot actuel
            if (slot == Manager.CurrentSlot) { continue; }

            // on récupère la position du slot
            Vector2 slot_position = Manager.GetPosition(slot);
            Vector2 direction_to_slot = (slot_position - current_slot_position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            if (angle < angle_threshold)
            {
                slots_in_angle.Add(slot);
            }
        }

        // on récupère le slot le plus proche dans cet angle
        UI_Slot closest_slot = Manager.GetClosestSlot(current_slot_position, ref slots_in_angle, ref s, direction, angle_multiplicator);

        // log
        if (closest_slot != null) { s += "\n\nclosest : " + closest_slot.name + "\n"; }
        else { s += "\n\nno closest slot found\n"; }
        if (log) { Debug.Log(s); }

        // return
        if (closest_slot == null) { return null; }
        return closest_slot;
    }
    protected UI_Slot navigate_dot(Vector2 direction)
    {
        // string s = "(UI_GamepadNavigator) NAVIGATE DOT: \n\n";

        // on récupère la position du slot actuel
        Vector2 current_slot_position = Manager.GetPosition(Manager.CurrentSlot);

        // on récupère les Manager.Slots dans le bon angle 45°
        float min_dot = float.MaxValue;
        UI_Slot best_slot = null;
        for (int i = 0; i < Manager.Slots.Count; i++)
        {
            UI_Slot slot = Manager.Slots[i];
            // on vérifie que ce n'est pas le slot actuel
            if (slot == Manager.CurrentSlot) { continue; }

            // on récupère la position du slot
            Vector2 slot_position = Manager.GetPosition(slot);
            Vector2 direction_to_slot = (slot_position - current_slot_position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            if (angle > 45f) { continue; }
            
            // on calcule le dot
            float dot = Vector2.Dot(direction, direction_to_slot);
            if (dot > min_dot) { continue; }

            // on retient le slot (c le meilleur candidat jusqu'à présent)
            min_dot = dot;
            best_slot = slot;
        }

        return best_slot;
    }

    // NAVIGATION CLOSEST & DIRECT
    public async void NavigateToClosest(Vector2 position, System.Type favorised_type = null, List<System.Type> unwanted_types = null)
    {
        // we check if we have a slottable
        if (Manager.Slottables.Count == 0) { return; }

        // we update the Manager.Slots
        Manager.UpdateSlots();

        // wait for a frame to let the UI update
        await System.Threading.Tasks.Task.Yield();

        // on récupère le slot le plus proche
        string s = "(UI_GamepadNavigator) NAVIGATE TO CLOSEST: \n\nfrom position : " + position + "\n\n";
        UI_Slot closest_slot = Manager.GetClosestSlot(position, ref Manager.Slots, ref s, favorised_type: favorised_type, unwanted_types: unwanted_types);

        // we navigate to the slot if we have one
        if (closest_slot == null) { return; }
        Manager.HoverSlot(closest_slot);

        // we log the result
        s += "\n\nclosest : " + closest_slot.name + "\n";
        if (log) { Debug.Log(s); }
    }
    public async void NavigateToSlot(UI_Slot slot)
    {
        // we check if we have a slottable
        if (Manager.Slottables.Count == 0) { return; }

        // we update the Manager.Slots
        Manager.UpdateSlots();

        // wait for a frame to let the UI update
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        
        // we update the Manager.Slots
        Manager.UpdateSlots();

        // we navigate to the slot if we have one
        if (slot == null) { return; }
        Manager.HoverSlot(slot);

        // we log the result
        if (log) { Debug.Log("(UI_GamepadNavigator) NAVIGATE TO SLOT: " + slot.name); }
    }
}