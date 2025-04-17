using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public class UI_XboxNavigator : MonoBehaviour
{
    
    // this class handles how the UI reacts to the xbox controller

    [Header("Slottables & Slots")]
    [SerializeField] private List<GameObject> slottables = new List<GameObject>();
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private int current_slot_index;
    [SerializeField] private UI_Inventory perso_quick_inventory;
    // this quick inventory is showed when another inventory (chess, etc.) is opened
    // to allow the player to transfer items between inventories
    private bool perso_quick_inventory_was_shown = true;

    [Header("Navigation")]
    [SerializeField] private bool can_navigate = true; // devient true lorsque la magnitude de l'input revient à 0, et false lorsque la magnitude de l'input est supérieure à 0.95f
    [SerializeField] private Vector2 last_input = Vector2.zero; // dernier input reçu

    [Header("Continuous Navigation")]
    [SerializeField] private bool navigate_continuously = false; // devient true lorsque la magnitude de l'input est maintenue à 1 pendant continuous_navigation_threshold
    [SerializeField] private float continuous_navigation_threshold = 0.4f; // delai avant activation de la navigation continue
    [SerializeField] private float continuous_navigation_cooldown = 0.05f; // vitesse de navigation continue -> délai entre chaque activation de la navigation continue
    [SerializeField] private float continuous_navigation_counter = float.MaxValue; // compteur de temps avant activation de la navigation continue & compteur de la navigation continue

    [Header("Navigation Parameters")]
    [SerializeField] private Vector2 base_position = Vector2.zero;
    [SerializeField] private float angle_threshold = 45f;
    [SerializeField] private float angle_multiplicator = 0f;


    [Header("Input & Callbacks")]
    private InputManager input_manager;
    [SerializeField] private InputActionReference navigateInput;
    private InputAction navigateAction;
    private event Action<InputAction.CallbackContext> navigateCallback;
    [SerializeField] private InputActionReference moveInput;
    private InputAction moveAction;
    private event Action<InputAction.CallbackContext> moveCallback; // move Callback is for dropping items, or, when dragged, move it through the ui, on Y

    [SerializeField] private InputActionReference activateInput;
    private InputAction activateAction;
    private event Action<InputAction.CallbackContext> activateCallback; // activate Callback is for activating the item/slot -> quit game, mostly on A


    [Header("Debug")]
    public bool debug = false;
    public bool debug_navigation = false;


    // START
    protected void Start()
    {
        if (navigateAction != null) { return; } // on ne fait rien si on a déjà Start()

        // we get the actions
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        activateAction = input_manager.GetAction(activateInput);
        navigateAction = input_manager.GetAction(navigateInput);
        moveAction = input_manager.GetAction(moveInput);

        // we create the callbacks
        navigateCallback = ctx => HandleNavigateInput(ctx.ReadValue<Vector2>());
        moveCallback = ctx => HandleMoveInput(ctx.ReadValue<float>());
        activateCallback = ctx => HandleActivateInput(ctx.ReadValue<float>());

        if (debug) { Debug.Log("(XboxNavigator) started & callbacks created"); }

        // we reset the variables
        continuous_navigation_counter = float.MaxValue;

        // we check if the perso quick inventory is shown
        if (perso_quick_inventory == null)
        {
            Debug.LogError("(XboxNavigator) perso_quick_inventory is not set. please set it in the inspector");
        }
    }


    // ENABLE / DISABLE
    public void Enable(I_UI_Slottable slottable)
    {
        if (slottables.Contains(slottable.gameObject)) { return; } // on ne fait rien si le slottable est déjà dans la liste

        if (navigateAction == null || activateAction == null) { Start(); }

        // on active les inputs si besoin
        if (slottables.Count == 0) { enableInputs(); }

        // on ajoute le slottable à la liste des slottables
        slottables.Add(slottable.gameObject);

        // si c'est l'ui d'un coffre on affiche le quick inventory
        if (slottable is UI_Inventory && slottable.gameObject != perso_quick_inventory.gameObject)
        {
            // on regarde si c'est un coffre
            UI_Inventory inventory = slottable as UI_Inventory;
            if (inventory.inventory != null && inventory.inventory.capable != null && inventory.inventory.capable is Chest)
            {
                // on affiche le perso quick inventory si besoin
                perso_quick_inventory_was_shown = perso_quick_inventory.gameObject.activeSelf;
                if (!perso_quick_inventory_was_shown) { perso_quick_inventory.Show(); }

                // on enable le slottable
                Enable(perso_quick_inventory);
            }
        }

        // on navigue vers le premier slot
        current_slot_index = -1;
        navigateToFirst();

        if (debug) { Debug.Log("(XboxNavigator) enabled slotabble : " + slottable.gameObject.name);}
    }
    public void Disable(I_UI_Slottable slottable)
    {
        if (!slottables.Contains(slottable.gameObject)) { return; }

        // on enlève le slottable de la liste des slottables
        slottables.Remove(slottable.gameObject);

        /* // on desactive le perso quick inventory si c'etait un coffre
        if (slottable != )
        {
            // on cache le perso quick inventory si besoin
            if (!perso_quick_inventory_was_shown) { perso_quick_inventory.Hide(); }

            // on disable le slottable
            Disable(perso_quick_inventory);
        } */

        if (debug) { Debug.Log("(XboxNavigator) disabling slottable : " + slottable.gameObject.name); }

        // on sauvegarde le slot actuel
        GameObject last_slot = null;

        // on désactive le slot
        if (current_slot_index != -1)
        {
            // on vérifie si le slot est encore présent et dans le slottable qu'on vient de désactiver
            GameObject slot = slots[current_slot_index];
            if (slot != null && slottable.IsYourSlot(slot))
            {
                // on désactive le slot
                slot.GetComponent<I_UI_Slot>().OnPointerExit(null);
            }
            else if (slot != null)
            {
                // on sauvegarde le slot
                last_slot = slot;
            }
        }

        // on met à jour les slots
        update_slots();
        hover_slot(last_slot != null ? slots.IndexOf(last_slot) : -1);

        // on cache le perso quick inventory si c'est le seul survivant
        if (slottables.Count == 1 && slottables[0] == perso_quick_inventory.gameObject)
        {
            // on cache le perso quick inventory si besoin
            if (!perso_quick_inventory_was_shown) { perso_quick_inventory.Hide(); }

            // on disable le slottable
            Disable(perso_quick_inventory);
        }
        // on regarde si on a encore des slottables
        if (slottables.Count == 0)
        {
            disableInputs();

            // on reset les variables de navigation
            navigate_continuously = false;
            can_navigate = true;
            continuous_navigation_counter = float.MaxValue;
            last_input = Vector2.zero;
        }
    }
    
    // INPUTS
    public void enableInputs()
    {
        // on récupère les inputs
        navigateAction.performed += navigateCallback;
        activateAction.performed += activateCallback;
        moveAction.performed += moveCallback;
    }
    private void disableInputs()
    {
        // on récupère les inputs
        navigateAction.performed -= navigateCallback;
        activateAction.performed -= activateCallback;
        moveAction.performed -= moveCallback;
    }


    // NAVIGATION INPUTS & UPDATE
    private void HandleNavigateInput(Vector2 input)
    {
        // cette fonction gère les inputs de navigation et décide si on doit naviguer ou non
        // si oui elle appelle alors navigate()

        // 1 - on gère l'input
        float magnitude = input.magnitude;
        // on garde l'input précédent
        last_input = input;

        // 2 - on regarde si on est avec un gros input (pour naviguer !!!)
        if (magnitude > 0.95f)
        {
            // on regarde si c'est la première fois qu'on navigue
            if (can_navigate)
            {
                // on lance le counter de navigation continue
                continuous_navigation_counter = Time.realtimeSinceStartup;
                can_navigate = false;

                // on navigue
                navigate(input.normalized);
            }
            return;
        }
        
        // 3 - si on est là c'est qu'on navigue pas
        navigate_continuously = false; // on reset la navigation continue
        continuous_navigation_counter = float.MaxValue; // on reset le compteur de navigation continue

        // 4 - on regarde si on a un input proche de zéro (pour naviguer à nouveau quand on revient à 1)
        if (magnitude < 0.5f) { can_navigate = true; }
    }
    private void Update()
    {
        // gère les timings de navigation continue

        // on regarde si on essaie d'activer la navigation continue
        if (continuous_navigation_counter == float.MaxValue) { return; }

        // cas 1 - on ne navigue pas encore en continu mais on essaie !
        if (!navigate_continuously && Time.realtimeSinceStartup - continuous_navigation_counter > continuous_navigation_threshold)
        {
            // on lance la navigation continue
            navigate_continuously = true;

            // on navigue
            continuous_navigation_counter = Time.realtimeSinceStartup;
            navigate(last_input.normalized);
        }

        // cas 2 - on navigue en continu
        else if (navigate_continuously && Time.realtimeSinceStartup - continuous_navigation_counter > continuous_navigation_cooldown)
        {
            // on navigue en continu
            continuous_navigation_counter = Time.realtimeSinceStartup;
            navigate(last_input.normalized);
        }
    }

    // NAVIGATION HIGH LEVEL
    private void navigate(Vector2 direction)
    {
        if (debug) { Debug.Log("(XboxNavigator) navigating : " + direction); }

        // on récupère les slots
        update_slots();

        string s = "(XboxNavigator) NAVIGATE: \n\n";

        // on récupère la position du slot actuel
        Vector2 current_slot_position = get_position(current_slot_index);
        s += "current slot : " + current_slot_index + " / position : " + current_slot_position + "\n\n";

        // on récupère les slots dans le bon angle
        List<GameObject> slots_in_angle = new List<GameObject>();
        foreach (GameObject slot in slots)
        {
            // on vérifie que ce n'est pas le slot actuel
            if (slots.IndexOf(slot) == current_slot_index) {continue;}

            // on récupère la position du slot
            Vector2 slot_position = get_position(slot);
            Vector2 direction_to_slot = (slot_position - current_slot_position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            if (angle < angle_threshold)
            {
                slots_in_angle.Add(slot);
            }
        }
        if (slots_in_angle.Count == 0) {return;}

        int next_index = findClosestSlot(slots_in_angle, current_slot_position, ref s, direction, angle_multiplicator);

        // on navigue vers le slot si on en a un
        if (next_index == -1) {return; }
        hover_slot(next_index);

        // log
        s += "\n\nclosest : " + next_index + "\n";
        if (debug_navigation) { Debug.Log(s); }
    }
    private void navigateToClosest(Vector2 position)
    {
        // we check if we have a slottable
        if (slottables.Count == 0) {return;}

        // we update the slots
        update_slots();

        // on récupère le slot le plus proche
        string s = "(XboxNavigator) NAVIGATE TO CLOSEST: \n\nfrom position : " + position + "\n\n";
        int next_index = findClosestSlot(slots, position,ref s);

        // we navigate to the slot if we have one
        if (next_index == -1) { return; }
        hover_slot(next_index);

        // we log the result
        s += "\n\nclosest : " + next_index + "\n";
        if (debug_navigation) { Debug.Log(s); }
    }
    private void navigateToFirst()
    {
        // we check if we have a slottable
        if (slottables.Count == 0) { return; }

        // we update the slots
        update_slots();

        // we check if we have a slot
        if (slots.Count == 0) {return;}

        // we hover the first slot
        hover_slot(0);

        // we log the result
        string s = "(XboxNavigator) NAVIGATE TO FIRST: slot is : " + slots[0].gameObject.name + "\n";
        if (debug_navigation) { Debug.Log(s); }
    }

    // NAVIGATION LOW LEVEL
    private int findClosestSlot(List<GameObject> slots, Vector2 position, ref string s, Vector2 direction = new Vector2(), float local_angle_multiplicator = 0f)
    {
        // find the closest slot to the given position
        // if direction & local_angle_multiplicator are given, we will find the closest slot in the direction
        // s is a string to debug the slots

        // on récupère le slot le plus proche
        int next_index = -1;
        float closest_distance = float.MaxValue;
        foreach (GameObject slot in slots)
        {
            // on récupère la position du slot
            Vector2 slot_position = get_position(slot);
            float angle = Vector2.Angle(direction, (slot_position - position).normalized);
            float distance = Vector2.Distance(position, slot_position - direction * local_angle_multiplicator);

            s += this.slots.IndexOf(slot) + " : " + slot.name + " : " + slot_position + " / angle : " + angle + " /  distance : " + distance + "\n";

            if (distance < closest_distance)
            {
                closest_distance = distance;
                next_index = this.slots.IndexOf(slot);
            }
        }
        return next_index;
    }
    private void update_slots()
    {
        // we check if we have a slottable
        if (slottables.Count > 0)
        {
            // on récupère les slots
            slots = new List<GameObject>();
            foreach (GameObject slottable in slottables)
            {
                // on récupère les slots du slottable
                List<GameObject> slottable_slots = slottable.GetComponent<I_UI_Slottable>().GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);
                slots.AddRange(slottable_slots);
            }
        }
        else { slots.Clear(); }

        if (slots.Count == 0)
        {
            current_slot_index = -1;
            return;
        }
    }
    private void hover_slot(int index)
    {

        // on unhover le slot actuel
        if (slots.Count > current_slot_index && current_slot_index != -1)
        {
            slots[current_slot_index].GetComponent<I_UI_Slot>().OnPointerExit(null);
        }

        // on hover le nouveau slot
        if (index != -1)
        {
            slots[index].GetComponent<I_UI_Slot>().OnPointerEnter(null);
        }

        // on met à jour l'index
        current_slot_index = index;
    }

    // SLOT POSITION LOW LEVEL
    private Vector2 get_position(int index)
    {
        // get the position of the slot
        Vector2 position = base_position;
        if (index == -1) { return position; }

        return get_position(slots[index]);
    }
    private Vector2 get_position(GameObject slot)
    {
        if (slot == null) { return base_position; }

        Vector2 position = slot.GetComponent<RectTransform>().TransformPoint(slot.GetComponent<RectTransform>().rect.center);
        return position;
    }


    // MOVING / DROPPING SLOTS
    private void HandleMoveInput(float input)
    {
        // cette fonction gère les inputs de navigation et décide si on doit naviguer ou non
        // si oui elle appelle alors drop() ou pressed()

        // 1 - on récupère le slot actuel
        if (current_slot_index == -1) { return; }
        GameObject go = slots[current_slot_index];
        if (go == null) { return; }
        I_UI_Slot slot = go.GetComponent<I_UI_Slot>();
        if (slot == null) { return; }

        // on handle le move seulement pour les UI_Item
        if (slot is not UI_Item) { return; }

        // 2 - on regarde si on a down ou up
        if (input > 0.5f)
        {
            // on down le slot
            (slot as UI_Slot).OnPointerDown(null);
            if (debug) { Debug.Log("(XboxNavigator) pressed ui_item " + slot.gameObject.name); }
        }
        else
        {
            // on relache le slot
            drop(slot as UI_Item);
            if (debug) { Debug.Log("(XboxNavigator) dropped ui_item " + slot.gameObject.name); }
        }

    }
    private async void drop(UI_Item slot)
    {
        if (slots.Count == 0 || current_slot_index == -1) { return; }

        // on retient la position du slot
        Vector2 position = get_position(slot.gameObject);

        // on clique sur le slot
        slot.OnPointerDropped(null);

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
    }
    
    // ACTIVATE SLOT
    private void HandleActivateInput(float input)
    {
        // cette fonction gère les inputs d'activation et décide si on peut activer ou non
        // si oui elle appelle alors activate() ou pressed()

        // 1 - on récupère le slot actuel
        if (current_slot_index == -1) { return; }
        GameObject go = slots[current_slot_index];
        if (go == null) { return; }
        I_UI_Slot slot = go.GetComponent<I_UI_Slot>();
        if (slot == null) { return; }

        // 2 - on regarde si on a down ou up
        if (input > 0.5f)
        {
            // on down le slot
            (slot as UI_Slot).OnPointerDown(null);
            if (debug) { Debug.Log("(XboxNavigator) pressed slot " + slot.gameObject.name); }
        }
        else
        {
            // on relache le slot
            activate(slot);
            if (debug) { Debug.Log("(XboxNavigator) activated slot " + slot.gameObject.name); }
        }
    }
    private async void activate(I_UI_Slot slot)
    {
        if (slots.Count == 0 || current_slot_index == -1) { return; }

        // on retient la position du slot
        Vector2 position = get_position(slot.gameObject);

        // on clique sur le slot
        slot.OnPointerClick(null);

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
    }

    // UPDATE
    /* public void updateWhileShowed()
    {
        if (slottable == null) {return;}
        if (current_slot_index == -1)
        {
            navigateToClosest();
            return;
        }

        // on conserve le slot actuel
        Vector2 position = get_position(current_slot_index);

        // on met à jour les slots en gardant le slot actuel
        navigateToClosest(position);
    } */
    /* public Vector2 getCursorPosition()
    {
        if (slottable == null) {return base_position;}
        if (current_slot_index == -1) {return base_position;}
        
        // on récupère le slot actuel
        return get_position(current_slot_index);
    } */
}