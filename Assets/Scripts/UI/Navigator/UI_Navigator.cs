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
    public Navigator Navigator;
    private GamepadNavigator gamepad_navigator;
    private MouseNavigator mouse_navigator;
    public UI_ItemMover Mover
    {
        get
        {
            if (_mover != null) { return _mover; }
            _mover = GetComponent<UI_ItemMover>();
            return _mover;
        }
    }
    private UI_ItemMover _mover;

    [Header("Slottables & Slots")]
    public List<Slottable> Slottables = new List<Slottable>();
    public List<UI_Slot> Slots;
    public UI_Slot CurrentSlot;

    // events
    public event Action<UI_Slot> OnSlotHoverEnter = delegate { }; // delegate that triggers when we navigate to a new slot
    public event Action<UI_Slot> OnSlotOutOfScreen = delegate { }; // delegate that triggers when we navigate to a position that is out of screen


    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_hover = false;
    [SerializeField] private bool log_slot_position = false;
    [SerializeField] private bool log_inputs = false;
    [SerializeField] private bool log_update_slots = false;

    private void Start()
    {
        // on récupère les navigators
        gamepad_navigator = GetComponent<GamepadNavigator>();
        mouse_navigator = GetComponent<MouseNavigator>();
        handle_input_type_changed(InputManager.Instance.CurrentInputType);

        // register toggling Navigator when switching inputs
        InputManager.Instance.OnInputTypeChanged += handle_input_type_changed;
    }
    private void handle_input_type_changed(string input_type)
    {
        if (input_type == "gamepad")
        {
            // on navigue vers le slot le plus proche
            if (log) { Debug.Log("(UI_Navigator) switching to gamepad navigation"); }
            Navigator = gamepad_navigator;
            if (CurrentSlot == null) { Navigator.NavigateToClosest(Navigator.BasePosition); }
            return;
        }

        // on récupère le clavier/souris Navigator
        if (log) { Debug.Log("(UI_Navigator) switching to mouse navigation"); }
        Navigator = mouse_navigator;
        if (CurrentSlot != null) { UnhoverSlot(); }
    }

    // ENABLE / DISABLE
    public void AddSlottable(Slottable slottable, bool ingame_navigation = false)
    {
        if (Slottables.Contains(slottable)) { return; } // on ne fait rien si le slottable est déjà dans la liste

        // on active les inputs si besoin
        if (Slottables.Count == 0) { Controller.Instance?.UIC.EnableInputs(ingame_navigation); }

        // on ajoute le slottable à la liste des Slottables
        Slottables.Add(slottable);

        // log
        if (log) { Debug.Log("(UI_Navigator) enabled slottable : " + slottable.name); }

        // on active le plugin de navigation
        Navigator.ActivateSlottable(slottable);
    }
    public void RemoveSlottable(Slottable slottable)
    {
        if (!Slottables.Contains(slottable)) { return; }
        
        // on enlève le slottable de la liste des Slottables
        Slottables.Remove(slottable);

        if (log) { Debug.Log("(UI_Navigator) disabling slottable : " + slottable.name); }

        // on désactive le slot si il fait partie du slot qu'on desactive
        if (CurrentSlot != null && slottable.IsYourSlot(CurrentSlot)) { UnhoverSlot(); }

        // on regarde si on a encore des Slottables
        if (Slottables.Count == 0) { UnhoverSlot(); Controller.Instance?.UIC.DisableInputs(); }
    }


    // SLOTS MANAGEMENT LOW LEVEL
    public void UpdateSlots()
    {
        // we clear Slots
        Slots.Clear();

        // we check if we have a slottable
        if (Slottables.Count == 0)
        {
            CurrentSlot = null;
            if (log_update_slots) { Debug.LogWarning("(UI_Navigator) no Slottables to update Slots from"); }
            return;
        }

        // on récupère les Slots
        for (int i = 0; i < Slottables.Count; i++)
        {
            Slottable slottable = Slottables[i];
            if (slottable == null) { continue; }

            // on récupère les Slots du slottable
            List<UI_Slot> slottable_slots = slottable.GetSlots();
            Slots.AddRange(slottable_slots);
        }

        if (log_update_slots) { Debug.Log("(UI_Navigator) updated Slots: " + Slots.Count + " Slots"); }
    }
    public void HoverSlot(UI_Slot slot, bool prevent_same_slot = true)
    {
        // checks if we can hover the slot
        if (slot == null) { return; }
        if (slot.Disabled) { return; }
        if (slot == CurrentSlot && prevent_same_slot) { return; }

        // checks if we already have a slot
        if (CurrentSlot != null) { UnhoverSlot(); }

        // check dragging & moving items
        if (!Mover.IsMovingItem || slot is not UI_Item ui_item)
        {
            slot.OnPointerEnter(null);
        }
        else if (Mover.MovingUIItem != ui_item)
        {
            ui_item.OnPointerDragEnter(Mover.MovingUIItem); // si on est ici on drag
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

        // checks if we are moving an ui_item we don't unhover the moving item
        if (Mover.IsMovingItem && Mover.MovingUIItem == CurrentSlot) { return; }

        // on unhover le slot actuel
        if (!CurrentSlot.Disabled) { CurrentSlot.OnPointerExit(null); }
        CurrentSlot = null;
    }


    // GETTERS SLOTS
    public Vector2 GetPosition(UI_Slot slot)
    {
        if (slot == null) { return Navigator.BasePosition; }

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
    public UI_Slot GetClosestSlot(Vector2 position, ref List<UI_Slot> slots, ref string s,Type favorised_type=null)
    {
        return GetClosestSlot(position, ref slots, ref s, new Vector2(), 0f, favorised_type: favorised_type);
    }
    public UI_Slot GetClosestSlot(Vector2 position, ref List<UI_Slot> slots, ref string s, Vector2 direction, float local_angle_multiplicator, Type favorised_type = null, Type unfavorised_type = null)
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

            // favorised type check
            bool override_distance = false;
            if (unfavorised_type != null)
            {
                bool is_slot_unfavorised = slot.GetType() == unfavorised_type || slot.GetType().IsSubclassOf(unfavorised_type);
                bool is_next_slot_unfavorised = next_slot != null && (next_slot.GetType() == unfavorised_type || next_slot.GetType().IsSubclassOf(unfavorised_type));

                // si le slot est du type unfavorised on skip si on a déjà un slot qui n'est pas unfavorised
                if (!is_next_slot_unfavorised && is_slot_unfavorised) { continue; }
            }
            if (favorised_type != null)
            {
                bool is_slot_favorised = slot.GetType() == favorised_type || slot.GetType().IsSubclassOf(favorised_type);
                bool is_next_slot_favorised = next_slot != null && (next_slot.GetType() == favorised_type || next_slot.GetType().IsSubclassOf(favorised_type));

                // si on est pas du bon type et que le next slot est du bon type on quitte direct
                if (!is_slot_favorised && is_next_slot_favorised) { continue; }

                // on override la distance si :
                // - next slot n'est pas du bon type
                // - le slot actuel est du bon type
                override_distance = !is_next_slot_favorised && is_slot_favorised;
            }

            // on compare les distances
            if (!override_distance && distance >= closest_distance) { continue; }

            // finally we override the next slot it is the closest one yet !
            closest_distance = distance;
            next_slot = slot;
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
    public bool IsCurrentSlotTypeOf(Type type)
    {
        if (CurrentSlot == null) { return false; }
        return CurrentSlot.GetType() == type || CurrentSlot.GetType().IsSubclassOf(type);
    }


    // INPUTS HANDLING
    public void OnNavigate(Vector2 direction) => Navigator.Navigate(direction.normalized);
    public void OnActivate()
    {
        if (Mover.IsMovingItem)
        {
            Mover.FinishMovingItem();
            return;
        }

        if (CurrentSlot == null) { return; }


        // soit on activate le slot si on a pas d'ui_item moving
        Mover.FinishMovingItem();
        activate(CurrentSlot);
        return;
    }
    private async Awaitable activate(UI_Slot slot)
    {
        if (slot == null) { return; }

        // on récupère le type favorisé pour la navigation après le click
        Type navigating_to_closest_slot_type = slot.GetFavorisedNavigationType();

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
        Navigator.NavigateToClosest(position, favorised_type: navigating_to_closest_slot_type);
    }
    public async void OnDrop()
    {
        if (Mover.IsMovingItem)
        {
            Mover.FinishMovingItem();
            return;
        }

        if (CurrentSlot == null) { return; }
        if (CurrentSlot is not Droppable droppable) { return; }
        UI_Slot slot = CurrentSlot;

        // on récupère le type favorisé pour la navigation après le click
        Type navigating_to_closest_slot_type = slot.GetFavorisedNavigationType();

        // on retient la position du slot
        Vector2 position = GetPosition(slot);

        // on vide le slot
        // on appelle OnPointerDropped pour simuler un drop
        droppable.OnPointerDropped(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Dropped slot " + slot.name); }

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        Navigator.NavigateToClosest(position, favorised_type: navigating_to_closest_slot_type);
    }
    public void OnDown(bool for_drop = false)
    {
        if (CurrentSlot == null) { return; }

        // check si c pour drop on verifie que c'est un UI_Item
        if (for_drop && CurrentSlot is not Droppable) { return; }

        // on down le slot
        CurrentSlot.OnPointerDown(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Downed slot " + CurrentSlot.name); }
    }
    public void OnUp()
    {
        if (CurrentSlot == null) { return; }

        // on up le slot
        CurrentSlot.OnPointerEnter(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Uped slot " + CurrentSlot.name); }
    }
    public void StartMovingItemIfInputDown()
    {
        if (Controller.Instance == null || Controller.Instance.UIC == null) { return; }

        // on vérifie si l'input n'est pas downed on ne move pas
        if (!Controller.Instance.UIC.IsMovingInputDown()) { return; }

        // si on bouge déjà c'est déjà activé, donc pas besoin 
        if (Mover.IsMovingItem) { return; }

        // on vérifie si c'est un UI_Item pour le moving item
        if (CurrentSlot is not UI_Item ui_item) { return; }
        if (ui_item.Quantity == 0) { return; }

        // on annule le endless drop ingame si besoin
        if (Controller.Instance.UIC.InGame)
        {
            EndlessInput<float> endless_drop_input = Controller.Instance.UIC.get_endless_input<float>("ui_drop_ingame");
            endless_drop_input.Cancel();
        }

        // on set le moving item
        Mover.StartMovingItem(ui_item);
        return;
    }

    // ON EXIT
    public void OnExit()
    {
        // checks if we have a UI_ExitButton
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i] is not UI_ExitButton exit_button) { continue; }
            exit_button.OnPointerClick(null);
            if (log_inputs) { Debug.Log("(UI_Navigator) Exit button clicked: " + exit_button.name); }
            return;
        }
    }
    public void OnExitDown()
    {
        // checks if we have a UI_ExitButton
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i] is not UI_ExitButton exit_button) { continue; }
            exit_button.OnPointerDown(null);
            if (log_inputs) { Debug.Log("(UI_Navigator) Exit button downed: " + exit_button.name); }
            return;
        }
    }

}


public interface Navigator
{
    // HANDLE SLOTTABLE ACTIVATION
    void ActivateSlottable(Slottable slottable);

    // NAVIGATION
    void NavigateToClosest(Vector2 position, Type favorised_type = null);
    void Navigate(Vector2 direction);
    Vector2 BasePosition { get; }
}