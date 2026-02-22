using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// MouseNavigator is a navigation plugin for the UI_Navigator
/// that is responsible for handling navigation with mouse & keyboard
/// </summary>
public class MouseNavigator : MonoBehaviour, Navigator
{

    [Header("Mouse navigation parameters")]
    public LayerMask ui_slot_layer;
    public Vector2 BasePosition => new Vector2(Screen.width / 2f, Screen.height / 2f);
    private float move_threshold = 15f;
    [SerializeField] private Vector2 last_mouse_position = Vector2.zero;

    [Header("Components")]
    public UI_Navigator manager;

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_hover = false;
    [SerializeField] private bool log_closest = false;

    // HANDLE SLOTTABLE ACTIVATION
    public void ActivateSlottable(Slottable slottable)
    {
        // we update the slots
        manager.UpdateSlots();

        // if we have some slots but they are out of screen we automatically go to the closest' slot panel
        if (manager.IsAllSlotsOutOfScreen())
        {
            if (log) { Debug.LogWarning("(UI_MouseNavigator) all slots are out of screen"); }

            // we want to roll panel to show a panel with slots ?
        }

        // si on a des slots on navigue tout simplement
        Navigate(start_moving_item: false);
    }
    public async void NavigateToClosest(Vector2 position, System.Type favorised_type = null)
    {
        // wait a frame for ui to update it self
        await System.Threading.Tasks.Task.Yield();
        if (log_closest) { Debug.Log($"(UI_MouseNavigator) navigatig to closest"); }
        Navigate(start_moving_item: false, navigating_to_closest: true);
    }


    // START
    private void Start()
    {
        manager = GetComponent<UI_Navigator>();
    }


    // NAVIGATION
    public void Navigate(Vector2 position) { Navigate(true); }
    public void Navigate(bool start_moving_item = true,bool navigating_to_closest = false)
    {
        // checks if we are navigating enough
        if (!navigating_to_closest && Vector2.Distance(last_mouse_position, Input.mousePosition) < move_threshold) { return; }
        last_mouse_position = Input.mousePosition;
        if (log_hover) { Debug.Log($"(UI_MouseNavigator) navigating with mouse position : {Input.mousePosition}"); }


        // checks if we are holding ui_drop_ingame (and so waiting for endless drop) we cancel it
        // -> because it means we are going to move items
        /* if (start_moving_item && Controller.Instance.UIC.InGame)
        {
            EndlessInput<float> endless_drop_input = Controller.Instance.UIC.get_endless_input<float>("ui_drop_ingame");
            // if (!IsEndlessInputDown<float>("ui_drop_ingame")) { return; }
            // get_endless_input<float>("ui_drop_ingame").Cancel();
            endless_drop_input.Cancel();
        } */


        // on move item potentiellement
        if (start_moving_item)
        {
            
            manager.StartMovingItemIfInputDown();
        }

        // on récupère le slot raycasted
        UI_Slot hovered_slot = raycast_mouse_slot();

        // on vérifie si le slot est disabled
        if (hovered_slot == null || hovered_slot.Disabled)
        {
            if (log_hover) { Debug.Log($"(UI_MouseNavigator) hovered slot is null or disabled, unhovering slot."); }
            manager.UnhoverSlot();
            return;
        }

        // on hover le slot
        manager.HoverSlot(hovered_slot);
    }
    public void UnhoverIfNotHovering(UI_Slot slot)
    {
        UI_Slot hovered_slot = raycast_mouse_slot();
        if (hovered_slot == slot) { return; }
        manager.UnhoverSlot();
    }
    private UI_Slot raycast_mouse_slot()
    {
        // prepare UI_Slot result list
        List<UI_Slot> hovered_slots = new List<UI_Slot>();
        UI_Slot slot;

        // get the mouse position
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        string log_results = "(UI_MouseNavigator) raycast results : " + results.Count;

        // we filter the results to keep only slots on "UI_Slot" layers & that have a UI_Slot on them
        for (int i = 0; i < results.Count; i++)
        {
            log_results += $"\n\t- {results[i].gameObject.name} (layer {results[i].gameObject.layer})";

            slot = results[i].gameObject.GetComponent<UI_Slot>();
            if (slot == null) { continue; }
            if ((ui_slot_layer & (1 << results[i].gameObject.layer)) == 0) { continue; }
            hovered_slots.Add(slot);
        }

        if (log_hover) { Debug.Log(log_results); }

        // if we have no slots hovered, we unhover & return
        if (hovered_slots.Count == 0) { return null; }

        // on récupère le premier résultat
        slot = hovered_slots[0];

        // special cases when it's an OutlinerSlot and is Disabled we skip it
        if (slot is UI_OutlineSlot outline_slot && outline_slot.Disabled)
        {
            if (hovered_slots.Count > 1)
            {
                slot = hovered_slots[1];
                if (log_hover) { Debug.Log($"(UI_MouseNavigator) skipped disabled outline slot, hovered another slot : {slot.name}"); }
            }
            else
            {
                if (log_hover) { Debug.Log($"(UI_MouseNavigator) only hovered slot is a disabled outline slot, unhovering."); }
                return null;
            }
        }
        if (log_hover) { Debug.Log($"(UI_MouseNavigator) hovered {hovered_slots.Count} ui_slots ! chosen one is {slot.name}"); }

        return slot;
    }
}