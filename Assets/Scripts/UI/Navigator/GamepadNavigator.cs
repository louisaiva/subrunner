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

    [Header("Components")]
    public UI_Navigator manager;


    [Header("Logs")]
    [SerializeField] private bool log = false;

    // START
    private void Start()
    {
        manager = GetComponent<UI_Navigator>();
    }

    // HANDLE SLOTTABLE ACTIVATION
    public void ActivateSlottable(Slottable slottable)
    {
        // on navigue vers le plus proche
        if (slottable.StartingSlot == null) { NavigateToClosest(BasePosition); return; }

        // on navigue vers le slot souhaité
        if (log) { Debug.Log("(UI_GamepadNavigator) navigating to starting slot : " + slottable.StartingSlot.name); }
        manager.HoverSlot(slottable.StartingSlot);
    }

    // NAVIGATION
    public void Navigate(Vector2 direction)
    {
        if (log) { Debug.Log("(UI_Navigator) navigating : " + direction); }

        // on move item potentiellement
        manager.StartMovingItemIfInputDown();
        
        // on récupère les manager.Slots
        manager.UpdateSlots();

        string s = "(UI_Navigator) NAVIGATE: \n\nparameters: \n\tangle_threshold : " + angle_threshold + "\n\tangle_multiplicator: " + angle_multiplicator + "\n\n";

        // on récupère la position du slot actuel
        Vector2 current_slot_position = manager.GetPosition(manager.CurrentSlot);
        if (manager.CurrentSlot != null) { s += "current slot : " + manager.CurrentSlot.name + " / position : " + current_slot_position + "\n\n"; }
        else { s += "no current slot\n\n"; }

        // on récupère les manager.Slots dans le bon angle
        List<UI_Slot> slots_in_angle = new List<UI_Slot>();
        for (int i = 0; i < manager.Slots.Count; i++)
        {
            UI_Slot slot = manager.Slots[i];
            // on vérifie que ce n'est pas le slot actuel
            if (slot == manager.CurrentSlot) { continue; }

            // on récupère la position du slot
            Vector2 slot_position = manager.GetPosition(slot);
            Vector2 direction_to_slot = (slot_position - current_slot_position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            if (angle < angle_threshold)
            {
                slots_in_angle.Add(slot);
            }
        }

        // on récupère le slot le plus proche dans cet angle
        UI_Slot closest_slot = manager.GetClosestSlot(current_slot_position, ref slots_in_angle, ref s, direction, angle_multiplicator);
        if (closest_slot == null) { return; }


        // on navigue vers le slot si on en a un
        manager.HoverSlot(closest_slot);


        // log
        s += "\n\nclosest : " + closest_slot.name + "\n";
        if (log) { Debug.Log(s); }
    }
    public async void NavigateToClosest(Vector2 position)
    {
        // we check if we have a slottable
        if (manager.Slottables.Count == 0) { return; }

        // we update the manager.Slots
        manager.UpdateSlots();

        // wait for a frame to let the UI update
        await System.Threading.Tasks.Task.Yield();

        // on récupère le slot le plus proche
        string s = "(UI_Navigator) NAVIGATE TO CLOSEST: \n\nfrom position : " + position + "\n\n";
        UI_Slot closest_slot = manager.GetClosestSlot(position, ref manager.Slots, ref s);

        // we navigate to the slot if we have one
        if (closest_slot == null) { return; }
        manager.HoverSlot(closest_slot);

        // we log the result
        s += "\n\nclosest : " + closest_slot.name + "\n";
        if (log) { Debug.Log(s); }
    }

}