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
    [SerializeField] private bool log_handle_slot_disabled = false;
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
    public void HoverSlot(UI_Slot slot)
    {
        // checks if we can hover the slot
        if (slot == null) { return; }
        if (slot.Disabled) { return; }

        // on joue le son seulement si on hover pas le même slot
        if (slot != CurrentSlot) { AudioEngine.Instance.PlayUI("hover"); }

        // checks if we already have a slot
        if (CurrentSlot != null) { UnhoverSlot(); }

        // check dragging & moving items
        if (!Mover.IsMovingItem || slot is not ItemReceivable ui_item)
        {
            slot.OnPointerEnter(null);
        }
        else if ((Mover.MovingUIItem as ItemReceivable) != ui_item)
        {
            ui_item.OnPointerDragEnter(Mover.MovingUIItem); // si on est ici on drag
        }

        // on vérifie si la position du slot est en dehors de l'écran
        if (IsSlotOutOfScreen(slot)) { OnSlotOutOfScreen?.Invoke(slot); }

        // on register le callback pour disabling
        slot.OnSlotDisabled += handle_slot_disabled_while_hovering;

        // on déclenche l'event OnSlotHoverEnter
        OnSlotHoverEnter?.Invoke(slot);
        CurrentSlot = slot;
        if (log_hover) { Debug.Log("(UI_Navigator) hovered slot : " + slot.name); }

    }
    public void UnhoverSlot()
    {
        if (CurrentSlot == null) { return; }

        // on unregister le callback pour disabling
        CurrentSlot.OnSlotDisabled -= handle_slot_disabled_while_hovering;

        if (log_hover) { Debug.Log("(UI_Navigator) unhovered slot : " + CurrentSlot.name); }

        // checks if we are moving an ui_item we don't unhover the moving item
        if (Mover.IsMovingItem && Mover.MovingUIItem == CurrentSlot) { return; }

        // on unhover le slot actuel
        if (!CurrentSlot.Disabled) { CurrentSlot.OnPointerExit(null); }
        CurrentSlot = null;
    }
    private async void handle_slot_disabled_while_hovering(UI_Slot slot)
    {
        // register the position of the slot while we still have a slot (might be deleted the frame after)
        Vector2 position = GetPosition(slot);

        await System.Threading.Tasks.Task.Yield();

        if (CurrentSlot != slot) { return; }
        if (log_handle_slot_disabled) { Debug.Log($"(UI_Navigator) hovered slot {(slot != null ? slot.name : "null")} got disabled, navigating to closest"); }
        Navigator.NavigateToClosest(position, unwanted_types: Navigator.UnwantedTypesAfterActivate);
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
    public UI_Slot GetClosestSlot(Vector2 position, ref List<UI_Slot> slots, ref string s,Type favorised_type=null, List<Type> unwanted_types = null)
    {
        return GetClosestSlot(position, ref slots, ref s, new Vector2(), 0f, favorised_type: favorised_type, unwanted_types: unwanted_types);
    }
    public UI_Slot GetClosestSlot(Vector2 position, ref List<UI_Slot> slots, ref string s, Vector2 direction, float local_angle_multiplicator, Type favorised_type = null, Type unfavorised_type = null, List<Type> unwanted_types = null)
    {
        // find the closest slot to the given position
        // if direction & local_angle_multiplicator are given, we will find the closest slot in the direction
        // s is a string to debug the found slots

        
        // global can't be set to closest slot type (specific ui_slot types that can't be closest, never)
        // List<Type> global_unfavorised_types = new List<Type>() { typeof(UI_OutlineSlot) };


        // on récupère le slot le plus proche
        UI_Slot next_slot = null;
        float closest_distance = float.MaxValue;
        for (int i = 0; i < slots.Count; i++)
        {
            UI_Slot slot = slots[i];

            // global unfavorised type check - not even need to check distance for this type of slot
            bool is_slot_unwanted = false;
            if (unwanted_types != null)
            {
                for (int j = 0; j < unwanted_types.Count; j++)
                {
                    Type unwanted_type = unwanted_types[j];
                    if (slot.GetType() == unwanted_type || slot.GetType().IsSubclassOf(unwanted_type))
                    {
                        is_slot_unwanted = true;
                        break;
                    }
                }
                if (is_slot_unwanted) { continue; }
            }



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

            // si on a une distance nulle, alors on skip parce que c'est qu'on est déjà dessus
            if (distance <= 0.01f) { continue; }

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
        List<Type> unwanted_types = Navigator.UnwantedTypesAfterActivate;

        // on retient la position du slot
        Vector2 position = GetPosition(slot);

        // on clique sur le slot
        slot.OnPointerClick(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Activated slot " + slot.name); }

        // on joue le son activate / cant activate
        if (slot.IsActivable()) { AudioEngine.Instance.PlayUI("activate"); }
        else { AudioEngine.Instance.PlayUI("cant_activate"); }

        // wait for a few frame to let the click happen
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();

        // verify that we still are enabled
        if (AppManager.Instance.IsQuitting) { return; }

        // on navigue vers le slot le plus proche
        // Navigator.NavigateToClosest(position, favorised_type: navigating_to_closest_slot_type, unwanted_types: unwanted_types);
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
        List<Type> unwanted_types = Navigator.UnwantedTypesAfterActivate;

        // on retient la position du slot
        Vector2 position = GetPosition(slot);

        // on vide le slot
        // on appelle OnPointerDropped pour simuler un drop
        droppable.OnPointerDropped(null);
        if (log_inputs) { Debug.Log("(UI_Navigator) Dropped slot " + slot.name); }

        // on joue le son drop
        AudioEngine.Instance.PlayUI("drop");

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche (seulement si c pas null ni disabled parce que sinon handle_slot_disabled_while_hovering a été appelé)
        if (slot != null && !slot.Disabled)
        {
            if (log_inputs) { Debug.Log("(UI_Navigator - OnDrop) Navigating to closest after drop"); }
            Navigator.NavigateToClosest(position, favorised_type: navigating_to_closest_slot_type, unwanted_types: unwanted_types);
        }

        // on refresh les item pools de l'inventory menu
        UI_Manager.Instance.RefreshInventoryMenu();
    }
    public void OnDown(bool for_drop = false)
    {
        if (CurrentSlot == null) { return; }

        // check si c pour drop on verifie que c'est un UI_ItemStack
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

        // on vérifie si c'est un UI_ItemStack pour le moving item
        if (CurrentSlot is not UI_ItemStack ui_item) { return; }
        if (ui_item.Stack.Quantity == 0) { return; }

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


    // ON DESTROY
    private void OnDestroy()
    {
        if (InputManager.Instance == null) { return; }
        InputManager.Instance.OnInputTypeChanged -= handle_input_type_changed;
    }
}


public interface Navigator
{

    // GAMEOBJECT
    GameObject gameObject { get; }

    // HANDLE SLOTTABLE ACTIVATION
    void ActivateSlottable(Slottable slottable);

    // NAVIGATION
    void NavigateToClosest(Vector2 position, Type favorised_type = null, List<System.Type> unwanted_types = null);
    void Navigate(Vector2 direction);
    Vector2 BasePosition { get; }
    List<Type> UnwantedTypesAfterActivate { get; }
}