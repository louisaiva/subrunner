#pragma warning disable 4014
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI_Slottable is the highest UI representation of any Slottable UI.
/// can handle the showing of multiple ui_slots gameobjects
/// it is the mother of the UI_Inventory.
/// </summary>
public class UI_Navigator : Singleton<UI_Navigator>
{
    [Header("Slottables & Slots")]
    private List<Slottable> slottables = new List<Slottable>();
    [SerializeField] private List<UI_Slot> slots;
    [SerializeField] private UI_Slot current_slot;


    [Header("Navigation Parameters")]
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 0f;
    private Vector2 base_position => new Vector2(Screen.width / 2f, Screen.height / 2f);

    // events
    public event Action<UI_Slot> OnSlotHoverEnter = delegate { }; // delegate that triggers when we navigate to a new slot
    public event Action<UI_Slot> OnSlotOutOfScreen = delegate { }; // delegate that triggers when we navigate to a position that is out of screen

    [Header("Moving Item")]
    [SerializeField] private UI_Item moving_ui_item = null;
    public bool IsMovingItem { get { return moving_ui_item != null; } }



    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_navigation = false;
    [SerializeField] private bool log_hover = false;
    [SerializeField] private bool log_moving_items = false;
    [SerializeField] private bool log_slot_position = false;
    [SerializeField] private bool log_inputs = false;

    // ENABLE / DISABLE
    public void AddSlottable(Slottable slottable, bool ingame_navigation = false, UI_Slot starting_slot = null)
    {
        if (slottables.Contains(slottable)) { return; } // on ne fait rien si le slottable est déjà dans la liste

        // on active les inputs si besoin
        if (slottables.Count == 0) { Controller.Instance.UIC.EnableInputs(ingame_navigation); }

        // on ajoute le slottable à la liste des slottables
        slottables.Add(slottable);

        // log
        if (log) { Debug.Log("(UI_Navigator) enabled slottable : " + slottable.name); }

        // on verifie si on utilise le clavier ou le controller
        if (!InputManager.Instance.isUsingGamepad()) { return; }


        // ON NAVIGUE
        if (starting_slot != null)
        {
            if (log) { Debug.Log("(UI_Navigator) navigating to starting slot : " + starting_slot.name); }
            hover_slot(starting_slot);
            return;
        }
        navigateToClosest(base_position);
    }
    public void RemoveSlottable(Slottable slottable)
    {
        if (!slottables.Contains(slottable)) { return; }

        // disable moving item

        // on enlève le slottable de la liste des slottables
        slottables.Remove(slottable);

        if (log) { Debug.Log("(UI_Navigator) disabling slottable : " + slottable.name); }

        // on désactive le slot si il fait partie du slot qu'on desactive
        if (current_slot != null && slottable.IsYourSlot(current_slot)) { unhover_slot(); }

        // on met à jour les slots
        // update_slots();
        // hover_slot(last_slot != null ? slots.IndexOf(last_slot) : -1);

        // on regarde si on a encore des slottables
        if (slottables.Count == 0) { Controller.Instance.UIC.DisableInputs(); }
        /* {
            disableInputs();

            // on reset les variables de navigation
            navigate_continuously = false;
            can_navigate = true;
            continuous_navigation_counter = float.MaxValue;
            last_input = Vector2.zero;
        } */
    }


    // NAVIGATION HIGH LEVEL
    private void navigate(Vector2 direction)
    {
        if (log) { Debug.Log("(UI_Navigator) navigating : " + direction); }

        // on récupère les slots
        update_slots();

        string s = "(UI_Navigator) NAVIGATE: \n\nparameters: \n\tangle_threshold : " + angle_threshold + "\n\tangle_multiplicator: " + angle_multiplicator + "\n\n";

        // on récupère la position du slot actuel
        Vector2 current_slot_position = get_position(current_slot);
        if (current_slot != null) { s += "current slot : " + current_slot.name + " / position : " + current_slot_position + "\n\n"; }
        else { s += "no current slot\n\n"; }

        // on récupère les slots dans le bon angle
        List<UI_Slot> slots_in_angle = new List<UI_Slot>();
        for (int i = 0; i < slots.Count; i++)
        {
            UI_Slot slot = slots[i];
            // on vérifie que ce n'est pas le slot actuel
            if (slot == current_slot) { continue; }

            // on récupère la position du slot
            Vector2 slot_position = get_position(slot);
            Vector2 direction_to_slot = (slot_position - current_slot_position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            if (angle < angle_threshold)
            {
                slots_in_angle.Add(slot);
            }
        }

        // on récupère le slot le plus proche dans cet angle
        UI_Slot closest_slot = findClosestSlot(slots_in_angle, current_slot_position, ref s, direction, angle_multiplicator);

        // on navigue vers le slot si on en a un
        if (closest_slot == null) { return; }
        hover_slot(closest_slot);

        // log
        s += "\n\nclosest : " + closest_slot.name + "\n";
        if (log_navigation) { Debug.Log(s); }
    }
    private async void navigateToClosest(Vector2 position)
    {
        // we check if we have a slottable
        if (slottables.Count == 0) { return; }

        // we update the slots
        update_slots();

        // wait for a frame to let the UI update
        await System.Threading.Tasks.Task.Yield();

        // on récupère le slot le plus proche
        string s = "(UI_Navigator) NAVIGATE TO CLOSEST: \n\nfrom position : " + position + "\n\n";
        UI_Slot closest_slot = findClosestSlot(slots, position, ref s);

        // we navigate to the slot if we have one
        if (closest_slot == null) { return; }
        hover_slot(closest_slot);

        // we log the result
        s += "\n\nclosest : " + closest_slot.name + "\n";
        if (log_navigation) { Debug.Log(s); }
    }
    private UI_Slot findClosestSlot(List<UI_Slot> slots, Vector2 position, ref string s, Vector2 direction = new Vector2(), float local_angle_multiplicator = 0f)
    {
        // find the closest slot to the given position
        // if direction & local_angle_multiplicator are given, we will find the closest slot in the direction
        // s is a string to debug the slots

        // on récupère le slot le plus proche
        UI_Slot next_slot = null;
        float closest_distance = float.MaxValue;
        for (int i = 0; i < slots.Count; i++)
        {
            UI_Slot slot = slots[i];

            // on récupère la distance entre la position et la position du slot
            Vector2 slot_position = get_position(slot);
            Vector2 direction_to_slot = (slot_position - position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            float distance = Vector2.Distance(position, slot_position - direction * local_angle_multiplicator);

            s += slot.name + " : " + slot_position + " / angle : " + angle + " /  distance : " + distance + "\n";

            // on compare les distances
            if (distance < closest_distance)
            {
                closest_distance = distance;
                next_slot = slot;
            }
        }
        return next_slot;
    }


    // SLOTS MANAGEMENT LOW LEVEL
    private void update_slots()
    {
        // we clear slots
        slots.Clear();

        // we check if we have a slottable
        if (slottables.Count == 0) { current_slot = null; return; }

        // on récupère les slots
        for (int i = 0; i < slottables.Count; i++)
        {
            Slottable slottable = slottables[i];
            if (slottable == null) { continue; }

            // on récupère les slots du slottable
            List<UI_Slot> slottable_slots = slottable.GetSlots();
            // slottable_slots.RemoveAll(slot => slot.Disabled); // we filter the disabled ones
            slots.AddRange(slottable_slots);
        }

        if (log) { Debug.Log("(UI_Navigator) updated slots: " + slots.Count + " slots"); }
    }
    private void hover_slot(UI_Slot slot)
    {
        // checks if we can hover the slot
        if (slot == null) { return; }
        if (slot == current_slot) { return; }

        // checks if we already have a slot
        if (current_slot != null) { unhover_slot(); }

        // check dragging & moving items
        if (moving_ui_item == null || slot is not UI_Item ui_item)
        {
            slot.OnPointerEnter(null);
        }
        else if (moving_ui_item != ui_item)
        {
            ui_item.OnPointerDragEnter(moving_ui_item); // si on est ici on drag
        }

        // on vérifie si la position du slot est en dehors de l'écran
        // // todo fix bug declenche l'event meme pour des canvas UI_World
        Vector2 position = get_position(slot);
        if (position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height)
        {
            // on déclenche l'event OnSlotOutOfScreen
            if (log_slot_position) { Debug.Log("(UI_Navigator) slot " + slot.name + " is out of screen"); }
            OnSlotOutOfScreen?.Invoke(slot);
        }

        // on déclenche l'event OnSlotHoverEnter
        OnSlotHoverEnter?.Invoke(slot);
        current_slot = slot;
        if (log_hover) { Debug.Log("(UI_Navigator) hovered slot : " + slot.name); }
    }
    private void unhover_slot()
    {
        if (current_slot == null) { return; }

        // on check le drag & moving
        /* if (slots.Count > current_slot_index && current_slot_index != -1
            && (moving_ui_item == null || moving_ui_item != slots[current_slot_index].GetComponent<UI_Item>()))
        {
            slots[current_slot_index].GetComponent<I_UI_Slot>().OnPointerExit(null);
        } */

        // on unhover le slot actuel
        if (!current_slot.Disabled) { current_slot.OnPointerExit(null); }
        current_slot = null;
    }

    // GETTERS SLOTS
    private Vector2 get_position(UI_Slot slot)
    {
        if (slot == null) { return base_position; }

        // if we are here we have a canvas slot -> means we have a recttransform
        RectTransform rect_transform = slot.GetComponent<RectTransform>();
        Vector2 position = rect_transform.TransformPoint(rect_transform.rect.center);
        string s = "(UI_Navigator) get_position: slot " + slot.name + " position : " + position;

        // on regarde si le slot est positionné dans un canvas world space or screen space
        if (slot.gameObject.layer == LayerMask.NameToLayer("UI_World"))
        {
            // on le convertit en position
            position = Camera.main.WorldToScreenPoint(position);
            s += " position (screen) : " + position + "\n";
        }
        if (log_slot_position) { Debug.Log(s); }

        return position;
    }
    public UI_Slot GetCurrentSlot()
    {
        // get the current slot
        if (current_slot == null) { return null; }
        return current_slot;
    }



    // INPUTS HANDLING
    public void OnNavigate(Vector2 direction) => navigate(direction.normalized);
    public void OnActivate()
    {
        if (current_slot == null) { return; }
        activate(current_slot);
    }
    private async Awaitable activate(UI_Slot slot)
    {
        if (slot == null) { return; }

        // on retient la position du slot
        Vector2 position = get_position(slot);

        // on clique sur le slot
        slot.OnPointerClick(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Activated slot " + slot.name); }

        // si on utilise la souris alors pas besoin de naviguer vers le plus proche
        if (!InputManager.Instance.isUsingGamepad()) { return; }

        // wait for a few frame to let the click happen
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();

        if (AppManager.Instance.IsQuitting) { return; }

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
    }
    public async void OnDrop()
    {
        if (current_slot == null) { return; }
        UI_Item slot = current_slot as UI_Item;
        if (slot == null)
        {
            // on a pas d'ui_item, alors on active tout simplement le i_ui_slot
            if (log_inputs) { Debug.Log("(UI_Navigator - OnDrop) current slot is not a UI_Item, cannot drop. activating instead."); }
            await activate(current_slot);
            return;
        }

        // on retient la position du slot
        Vector2 position = get_position(slot);

        // on vide le slot
        // on appelle OnPointerDropped pour simuler un drop
        slot.OnPointerDropped(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Dropped slot " + slot.name); }

        // si on utilise la souris alors pas besoin de naviguer vers le plus proche
        if (!InputManager.Instance.isUsingGamepad()) { return; }

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
    }
    public void OnDown()
    {
        if (current_slot == null) { return; }
        // I_UI_Slot slot = current_slot.GetComponent<I_UI_Slot>();
        // UI_Slot slot = current_slot;
        // if (slot == null) { return; }

        // on down le slot
        current_slot.OnPointerDown(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Downed slot " + current_slot.name); }
    }

}