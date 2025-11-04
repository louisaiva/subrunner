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
    public Navigator navigator;
    private GamepadNavigator gamepad_navigator;
    private MouseNavigator mouse_navigator;

    [Header("Slottables & Slots")]
    public List<Slottable> Slottables = new List<Slottable>();
    public List<UI_Slot> Slots;
    public UI_Slot CurrentSlot;

    // events
    public event Action<UI_Slot> OnSlotHoverEnter = delegate { }; // delegate that triggers when we navigate to a new slot
    public event Action<UI_Slot> OnSlotOutOfScreen = delegate { }; // delegate that triggers when we navigate to a position that is out of screen

    [Header("Moving Item")]
    [SerializeField] private UI_Item moving_ui_item = null;
    public bool IsMovingItem { get { return moving_ui_item != null; } }



    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_hover = false;
    // [SerializeField] private bool log_moving_items = false;
    [SerializeField] private bool log_slot_position = false;
    [SerializeField] private bool log_inputs = false;

    private void Start()
    {
        // on récupère les navigators
        gamepad_navigator = GetComponent<GamepadNavigator>();
        mouse_navigator = GetComponent<MouseNavigator>();
        handle_input_type_changed(InputManager.Instance.CurrentInputType);

        // register toggling navigator when switching inputs
        InputManager.Instance.OnInputTypeChanged += handle_input_type_changed;
    }
    private void handle_input_type_changed(string input_type)
    {
        if (input_type == "gamepad")
        {
            // on navigue vers le slot le plus proche
            if (log) { Debug.Log("(UI_Navigator) switching to gamepad navigation"); }
            navigator = gamepad_navigator;
            if (CurrentSlot == null) { navigator.NavigateToClosest(navigator.BasePosition); }
            return;
        }

        // on récupère le clavier/souris navigator
        if (log) { Debug.Log("(UI_Navigator) switching to mouse navigation"); }
        navigator = mouse_navigator;
        if (CurrentSlot != null) { UnhoverSlot(); }
    }

    // ENABLE / DISABLE
    public void AddSlottable(Slottable slottable, bool ingame_navigation = false, UI_Slot starting_slot = null)
    {
        if (Slottables.Contains(slottable)) { return; } // on ne fait rien si le slottable est déjà dans la liste

        // on active les inputs si besoin
        if (Slottables.Count == 0) { Controller.Instance.UIC.EnableInputs(ingame_navigation); }

        // on ajoute le slottable à la liste des Slottables
        Slottables.Add(slottable);

        // log
        if (log) { Debug.Log("(UI_Navigator) enabled slottable : " + slottable.name); }

        // on active le plugin de navigation
        navigator.ActivateSlottable(slottable);
    }
    public void RemoveSlottable(Slottable slottable)
    {
        if (!Slottables.Contains(slottable)) { return; }

        // disable moving item

        // on enlève le slottable de la liste des Slottables
        Slottables.Remove(slottable);

        if (log) { Debug.Log("(UI_Navigator) disabling slottable : " + slottable.name); }

        // on désactive le slot si il fait partie du slot qu'on desactive
        if (CurrentSlot != null && slottable.IsYourSlot(CurrentSlot)) { UnhoverSlot(); }

        // on met à jour les Slots
        // update_slots();
        // HoverSlot(last_slot != null ? Slots.IndexOf(last_slot) : -1);

        // on regarde si on a encore des Slottables
        if (Slottables.Count == 0) { Controller.Instance.UIC.DisableInputs(); }
        /* {
            disableInputs();

            // on reset les variables de navigation
            navigate_continuously = false;
            can_navigate = true;
            continuous_navigation_counter = float.MaxValue;
            last_input = Vector2.zero;
        } */
    }


    // SLOTS MANAGEMENT LOW LEVEL
    public void UpdateSlots()
    {
        // we clear Slots
        Slots.Clear();

        // we check if we have a slottable
        if (Slottables.Count == 0) { CurrentSlot = null; return; }

        // on récupère les Slots
        for (int i = 0; i < Slottables.Count; i++)
        {
            Slottable slottable = Slottables[i];
            if (slottable == null) { continue; }

            // on récupère les Slots du slottable
            List<UI_Slot> slottable_slots = slottable.GetSlots();
            Slots.AddRange(slottable_slots);
        }

        if (log) { Debug.Log("(UI_Navigator) updated Slots: " + Slots.Count + " Slots"); }
    }
    public void HoverSlot(UI_Slot slot)
    {
        // checks if we can hover the slot
        if (slot == null) { return; }
        if (slot.Disabled) { return; }
        if (slot == CurrentSlot) { return; }

        // checks if we already have a slot
        if (CurrentSlot != null) { UnhoverSlot(); }

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
        if (IsSlotOutOfScreen(slot)) { OnSlotOutOfScreen?.Invoke(slot); }

        // on déclenche l'event OnSlotHoverEnter
        OnSlotHoverEnter?.Invoke(slot);
        CurrentSlot = slot;
        if (log_hover) { Debug.Log("(UI_Navigator) hovered slot : " + slot.name); }
    }
    public void UnhoverSlot()
    {
        if (CurrentSlot == null) { return; }

        // on check le drag & moving
        /* if (Slots.Count > CurrentSlot_index && CurrentSlot_index != -1
            && (moving_ui_item == null || moving_ui_item != Slots[CurrentSlot_index].GetComponent<UI_Item>()))
        {
            Slots[CurrentSlot_index].GetComponent<I_UI_Slot>().OnPointerExit(null);
        } */

        // on unhover le slot actuel
        if (!CurrentSlot.Disabled) { CurrentSlot.OnPointerExit(null); }
        CurrentSlot = null;
    }

    // GETTERS SLOTS
    public Vector2 GetPosition(UI_Slot slot)
    {
        if (slot == null) { return navigator.BasePosition; }

        // if we are here we have a canvas slot -> means we have a recttransform
        RectTransform rect_transform = slot.GetComponent<RectTransform>();
        Vector2 position = rect_transform.TransformPoint(rect_transform.rect.center);
        string s = "(UI_Navigator) GetPosition: slot " + slot.name + " position : " + position;

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
        if (CurrentSlot == null) { return null; }
        return CurrentSlot;
    }
    public UI_Slot GetClosestSlot(Vector2 position, ref List<UI_Slot> slots, ref string s)
    {
        return GetClosestSlot(position, ref slots, ref s, new Vector2(), 0f);
    }
    public UI_Slot GetClosestSlot(Vector2 position, ref List<UI_Slot> slots, ref string s, Vector2 direction = new Vector2(), float local_angle_multiplicator = 0f)
    {
        // find the closest slot to the given position
        // if direction & local_angle_multiplicator are given, we will find the closest slot in the direction
        // s is a string to debug the found slots

        // on récupère le slot le plus proche
        UI_Slot next_slot = null;
        float closest_distance = float.MaxValue;
        for (int i = 0; i < slots.Count; i++)
        {
            UI_Slot slot = slots[i];

            // on récupère la distance entre la position et la position du slot
            Vector2 slot_position = GetPosition(slot);
            float distance;

            // si on a une direction & un local_angle_multiplicator, on applique la modification d'angle
            if (local_angle_multiplicator != 0f)
            {
                Vector2 direction_to_slot = (slot_position - position).normalized;
                float angle = Vector2.Angle(direction, direction_to_slot);
                distance = Vector2.Distance(position, slot_position - direction * local_angle_multiplicator);
                s += slot.name + " : " + slot_position + " / angle : " + angle + " /  distance : " + distance + "\n";
            }
            else
            {
                distance = Vector2.Distance(position, slot_position);
                s += slot.name + " : " + slot_position + " / distance : " + distance + "\n";
            }


            // on compare les distances
            if (distance < closest_distance)
            {
                closest_distance = distance;
                next_slot = slot;
            }
        }
        return next_slot;
    }
    public bool IsSlotOutOfScreen(UI_Slot slot)
    {
        Vector2 position = GetPosition(slot);
        if (position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height)
        {
            return true;
        }
        return false;
    }
    public bool IsAllSlotsOutOfScreen()
    {
        if (Slots.Count == 0) { return false; }

        for (int i = 0; i < Slots.Count; i++)
        {
            if (!IsSlotOutOfScreen(Slots[i]))
            {
                return false;
            }
        }
        return true;
    }

    // INPUTS HANDLING
    public void OnNavigate(Vector2 direction) => navigator.Navigate(direction.normalized);
    public void OnActivate()
    {
        if (CurrentSlot == null) { return; }
        activate(CurrentSlot);
    }
    private async Awaitable activate(UI_Slot slot)
    {
        if (slot == null) { return; }

        // on retient la position du slot
        Vector2 position = GetPosition(slot);

        // on clique sur le slot
        slot.OnPointerClick(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Activated slot " + slot.name); }

        // wait for a few frame to let the click happen
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();

        // verify that we still are enabled
        if (AppManager.Instance.IsQuitting) { return; }

        // on navigue vers le slot le plus proche
        navigator.NavigateToClosest(position);
    }
    public async void OnDrop()
    {
        if (CurrentSlot == null) { return; }
        UI_Item slot = CurrentSlot as UI_Item;
        if (slot == null) { return; }

        // on retient la position du slot
        Vector2 position = GetPosition(slot);

        // on vide le slot
        // on appelle OnPointerDropped pour simuler un drop
        slot.OnPointerDropped(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Dropped slot " + slot.name); }

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        navigator.NavigateToClosest(position);
    }
    public void OnDown()
    {
        if (CurrentSlot == null) { return; }

        // on down le slot
        CurrentSlot.OnPointerDown(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Downed slot " + CurrentSlot.name); }
    }
}


public interface Navigator
{

    // HANDLE SLOTTABLE ACTIVATION
    void ActivateSlottable(Slottable slottable);

    // NAVIGATION
    void NavigateToClosest(Vector2 position);
    void Navigate(Vector2 direction);
    // void Scroll(float scroll_value);

    Vector2 BasePosition { get; }
}