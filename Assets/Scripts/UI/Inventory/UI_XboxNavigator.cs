#pragma warning disable 4014
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
/// <summary>
/// This class handles the navigation through UI
/// with a controller
/// </summary>
public class UI_XboxNavigator : Singleton<UI_XboxNavigator>
{
    
    [Header("Slottables & Slots")]
    [SerializeField] private List<GameObject> slottables = new List<GameObject>();
    [SerializeField] private List<GameObject> slots;
    [SerializeField] private int current_slot_index;
    

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
    public float angle_threshold = 45f;
    public float angle_multiplicator = 0f;
    // [SerializeField] public float angle_vs_distance_precision = 0f; // from 0 to 1, affine la prédiction de navigation

    // EVENTS
    public event Action<I_UI_Slot> OnSlotHoverEnter = delegate { }; // delegate that triggers when we navigate to a new slot
    public event Action<I_UI_Slot> OnSlotOutOfScreen = delegate { }; // delegate that triggers when we navigate to a position that is out of screen


    [Header("Moving Items")]
    [SerializeField] private UI_Item moving_ui_item = null; // the item that is currently being moved


    // todo : move all these inputs to UIC
    // todo : and keep only methods like OnNavigate(), OnActivate(), OnDrop(), OnMove() etc

    private InputManager input_manager;
    [Header("Input & Callbacks")]

    // NAVIGATE L
    [SerializeField] private InputActionReference navigateInput;
    private InputAction navigateAction;
    private event Action<InputAction.CallbackContext> navigateCallback; // for navigating through the UI -> LJoy

    // ACTIVATE
    [SerializeField] private InputActionReference activateInput;
    private InputAction activateAction;
    private event Action<InputAction.CallbackContext> activateCallback; // activate Callback is for activating the item/slot -> A

    // MOVING ITEM
    [SerializeField] private InputActionReference moveItemInput;
    private InputAction moveItemAction;
    private event Action<InputAction.CallbackContext> moveItemCallback; // moveItem Callback is for moving an item through the ui. -> Y
    public bool IsMovingItem { get => moving_ui_item != null; } // returns true if we are moving an item

    // NAVIGATE IN-GAME
    // [SerializeField] private bool navigateInGame = false; // if true, we use the navigateInGameAction instead of the navigateAction -> RJoy 
    [SerializeField] private InputActionReference navigateInGameInput;
    private InputAction navigateInGameAction;



    [Header("Logs")]
    public bool debug = false;
    public bool debug_navigation = false;
    public bool log_slot_position = false;
    public bool log_moving_items = false;
    public bool log_receivable_slots = false;
    public bool debug_gizmo = false;

    [Header("Gizmos")]
    private Vector2 gizmo_position = Vector2.negativeInfinity; // used to transmit the position to OnDrawGizmo
    public List<Vector2> gizmos_positions = new List<Vector2>(); // used to show gizmos, Vector2 in world space
    public List<float> gizmos_weights = new List<float>(); // used to show gizmos, weight from 0 to 1

    // START
    protected void Start()
    {
        if (navigateAction != null) { return; } // on ne fait rien si on a déjà Start()

        // we get the actions
        input_manager = GameObject.Find("/utils/input_manager").GetComponent<InputManager>();
        navigateAction = input_manager.GetAction(navigateInput);
        navigateInGameAction = input_manager.GetAction(navigateInGameInput);
        activateAction = input_manager.GetAction(activateInput);
        moveItemAction = input_manager.GetAction(moveItemInput);

        // we create the callbacks
        navigateCallback = ctx => HandleNavigateInput(ctx.ReadValue<Vector2>());
        activateCallback = ctx => HandleActivateInput(ctx.ReadValue<float>());
        moveItemCallback = ctx => HandleMoveItemInput(ctx.ReadValue<float>());

        if (debug) { Debug.Log("(UI_Navigator) started & callbacks created"); }

        // we reset the variables
        continuous_navigation_counter = float.MaxValue;
        current_slot_index = -1;
    }


    // ENABLE / DISABLE
    public void Enable(I_UI_Slottable slottable, bool ingame_navigation = false,bool navigate = true)
    {
        if (slottables.Contains(slottable.gameObject)) { return; } // on ne fait rien si le slottable est déjà dans la liste
        if (navigateAction == null || activateAction == null) { Start(); }

        // on active les inputs si besoin
        if (slottables.Count == 0) { enableInputs(ingame_navigation); }

        // on ajoute le slottable à la liste des slottables
        slottables.Add(slottable.gameObject);
        
        // log
        if (debug) { Debug.Log("(UI_Navigator) enabled slotabble : " + slottable.gameObject.name); }

        // on verifie si on utilise le clavier ou le controller
        if (!input_manager.isUsingGamepad()) { return; }
        if (!navigate) { return; }

        // ON NAVIGUE
        current_slot_index = -1;
        navigateToClosest(slottable.SavedPosition);
    }
    public void Disable(I_UI_Slottable slottable)
    {
        if (!slottables.Contains(slottable.gameObject)) { return; }

        // on désactive le moving_ui_item si on en a un
        if (moving_ui_item != null)
        {
            // on désactive le moving item
            moving_ui_item.OnPointerExit(null);
            moving_ui_item = null;
            disable_only_empty_slots();
        }

        // on enlève le slottable de la liste des slottables
        slottables.Remove(slottable.gameObject);

        if (debug) { Debug.Log("(UI_Navigator) disabling slottable : " + slottable.gameObject.name); }

        // on sauvegarde le slot actuel
        GameObject last_slot = null;

        // on désactive le slot
        if (current_slot_index != -1 && current_slot_index < slots.Count)
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
    private void enableInputs(bool ingame_navigation = false)
    {
        moveItemAction.performed += moveItemCallback;

        // on active le bon callback de navigation
        if (ingame_navigation)
        {
            navigateInGameAction.performed += navigateCallback;
        }
        else
        {
            navigateAction.performed += navigateCallback;
            activateAction.performed += activateCallback;
        }
        
        Controller.Instance.UIC.EnableInputs(ingame_navigation); // on active les inputs dans le UI_InputsController
    }
    private void disableInputs()
    {
        // on récupère les inputs
        navigateAction.performed -= navigateCallback;
        navigateInGameAction.performed -= navigateCallback;
        activateAction.performed -= activateCallback;
        moveItemAction.performed -= moveItemCallback;
        
        Controller.Instance.UIC.DisableInputs(); // on désactive les inputs dans le UI_InputsController
    }
    public void ToggleInput(string input_name, bool enable = true)
    {
        Controller.Instance.UIC.ToggleInput(input_name, enable);
        if (input_name == "activate")
        {
            if (enable) { activateAction.performed += activateCallback; }
            else { activateAction.performed -= activateCallback; }
        }
        else if (input_name == "move")
        {
            if (enable) { moveItemAction.performed += moveItemCallback; }
            else { moveItemAction.performed -= moveItemCallback; }
        }
    }

    // UPDATE
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
        if (debug) { Debug.Log("(UI_Navigator) navigating : " + direction); }

        // on récupère les slots
        update_slots();

        string s = "(UI_Navigator) NAVIGATE: \n\nparameters: \n\tangle_threshold : " + angle_threshold + "\n\tangle_multiplicator: " + angle_multiplicator + "\n\n";

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

        if (slots_in_angle.Count == 0)
        {
            // we show a cross in the gizmo to say that we had nothing in angle

            // we clear the gizmo data
            gizmos_positions.Clear();
            gizmos_weights.Clear();
            gizmo_position = Camera.main.ScreenToWorldPoint(current_slot_position);
            return;
        }

        int next_index = findClosestSlot(slots_in_angle, current_slot_position, ref s, direction, angle_multiplicator);

        // on navigue vers le slot si on en a un
        if (next_index == -1) {return; }
        hover_slot(next_index);

        // log
        s += "\n\nclosest : " + next_index + "\n";
        if (debug_navigation) { Debug.Log(s); }
    }
    private async void navigateToClosest(Vector2 position)
    {
        // we check if we have a slottable
        if (slottables.Count == 0) {return;}

        // we update the slots
        update_slots();

        await System.Threading.Tasks.Task.Yield(); // wait for a frame to let the UI update

        // on récupère le slot le plus proche
        string s = "(UI_Navigator) NAVIGATE TO CLOSEST: \n\nfrom position : " + position + "\n\n";
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
        string s = "(UI_Navigator) NAVIGATE TO FIRST: slot is : " + slots[0].gameObject.name + "\n";
        if (debug_navigation) { Debug.Log(s); }
    }

    // NAVIGATION LOW LEVEL
    private int findClosestSlot(List<GameObject> slots, Vector2 position, ref string s, Vector2 direction = new Vector2(), float local_angle_multiplicator = 0f)
    {
        // find the closest slot to the given position
        // if direction & local_angle_multiplicator are given, we will find the closest slot in the direction
        // s is a string to debug the slots

        // we clear the gizmo data
        gizmo_position = Camera.main.ScreenToWorldPoint(position);
        gizmos_positions.Clear();
        gizmos_weights.Clear();

        // on récupère le slot le plus proche
        int next_index = -1;
        float closest_distance = float.MaxValue;
        foreach (GameObject slot in slots)
        {
            // on récupère la position du slot
            Vector2 slot_position = get_position(slot);
            Vector2 direction_to_slot = (slot_position - position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            float distance = Vector2.Distance(position, slot_position - direction * local_angle_multiplicator);

            // on ajoute les données de navigation aux gizmos
            // gizmos_positions.Add(Camera.main.ScreenToWorldPoint(slot_position));

            s += this.slots.IndexOf(slot) + " : " + slot.name + " : " + slot_position + " / angle : " + angle + " /  distance : " + distance + "\n";

            if (distance < closest_distance)
            {
                closest_distance = distance;
                next_index = this.slots.IndexOf(slot);
            }
        }

        // now that we have the next index (based on the closest_distance)
        // we can calculate slot navigation weight
        foreach (GameObject slot in slots)
        {
            /* // on récupère la position du slot
            Vector2 slot_position = get_position(slot);
            Vector2 direction_to_slot = (slot_position - position).normalized;
            float angle = Vector2.Angle(direction, direction_to_slot);
            float distance = Vector2.Distance(position, slot_position - direction * local_angle_multiplicator);

            // we add the weight to the slot
            float weight = Mathf.Clamp01(distance / closest_distance);*/


            // on récupère la position du slot
            Vector2 slot_position = get_position(slot);
            Vector2 movement = slot_position - position; // ce vecteur est le vecteur PM où P est la position du slot actuelle et M la position du slot qu'on regarde
            float weight = Vector2.Dot(direction, movement.normalized);


            if (debug_navigation)
            {
                s += "weight for " + slot.name + " : " + weight + "\n";
            }

            // on applique un filtre pour mieux voir le gizmo (on veut voir en vert petant le slot choisi, et les autres NO)
            // weight = (float)Math.Pow(weight, gizmo_dot_size);

            // we add a gizmo
            Vector2 gizmo_slot = Camera.main.ScreenToWorldPoint(slot_position);
            // Vector3 gizmo_data = new Vector3(gizmo_slot.x, gizmo_slot.y, weight); // we add the weight as z value
            gizmos_positions.Add(gizmo_slot);
            gizmos_weights.Add(weight);
        }

        return next_index;
    }
    private void update_slots()
    {
        // we check if we have a slottable
        if (slottables.Count == 0)
        {
            slots.Clear();
            current_slot_index = -1;
            return;
        }

        // on récupère les slots
        slots = new List<GameObject>();
        foreach (GameObject slottable in slottables)
        {
            if (slottable == null) { continue; }
            
            // on récupère les slots du slottable
            List<GameObject> slottable_slots = slottable.GetComponent<I_UI_Slottable>().GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);
            slottable_slots.RemoveAll(slot => slot.GetComponent<UI_Item>() != null && slot.GetComponent<UI_Item>().is_disabled); // we filter the disabled ones
            slots.AddRange(slottable_slots);
        }
        if (slots.Count == 0) { current_slot_index = -1; }
    }
    private void hover_slot(int index)
    {
        // on unhover le slot actuel
        if (slots.Count > current_slot_index && current_slot_index != -1
            && (moving_ui_item == null || moving_ui_item != slots[current_slot_index].GetComponent<UI_Item>()))
        {
            slots[current_slot_index].GetComponent<I_UI_Slot>().OnPointerExit(null);
        }

        // on vérifie si on a rien à hover
        if (index == -1) { current_slot_index = -1; return; }

        // on vérifie si on ne drag pas
        if (moving_ui_item == null || slots[index].GetComponent<UI_Item>() == null)
        {
            slots[index].GetComponent<I_UI_Slot>().OnPointerEnter(null);
        }
        else if (moving_ui_item != slots[index].GetComponent<UI_Item>())
        {
            slots[index].GetComponent<UI_Item>().OnPointerDragEnter(moving_ui_item); // si on est ici on drag
        }

        // on vérifie si la position du slot est en dehors de l'écran
        Vector2 position = get_position(index);
        if (position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height)
        {
            // on déclenche l'event OnSlotOutOfScreen
            if (debug) { Debug.Log("(UI_Navigator) slot " + index + " is out of screen"); }
            OnSlotOutOfScreen?.Invoke(slots[index].GetComponent<I_UI_Slot>());
        }

        // on déclenche l'event OnSlotHoverEnter
        OnSlotHoverEnter?.Invoke(slots[index].GetComponent<I_UI_Slot>());

        // on change le current slot index
        current_slot_index = index;
    }

    // GETTERS SLOTS
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

        // if we are here we have a canvas slot -> means we have a recttransform
        Vector2 position = slot.GetComponent<RectTransform>().TransformPoint(slot.GetComponent<RectTransform>().rect.center);
        string s = "(UI_Navigator) get_position: slot " + slot.name + " position : " + position;

        // on regarde si le slot est positionné dans un canvas world space or screen space
        if (slot.layer == LayerMask.NameToLayer("UI_World"))
        {
            // on le convertit en position
            position = Camera.main.WorldToScreenPoint(position);
            s += " position (screen) : " + position + "\n";
        }
        if (log_slot_position) { Debug.Log(s); }

        return position;
    }
    public Vector2 GetCurrentSlotPosition()
    {
        // get the position of the current slot
        if (current_slot_index == -1) { return new Vector2(Screen.width / 2f, Screen.height / 2f); }
        return get_position(current_slot_index);
    }





    // NAVIGATION INPUTS & UPDATE
    private void HandleNavigateInput(Vector2 input)
    {
        // do nothing if this input is mouse_based
        if (!input_manager.isUsingGamepad())
        {
            if (debug) { Debug.Log("(XboxNavigator - HandleActivateInput) activate input ignored because mouse based"); }
            return;
        }

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
        // 4 - on regarde si on a un input proche de zéro (pour naviguer à nouveau quand on revient à 1)
        if (magnitude < 0.5f)
        {
            can_navigate = true;
            navigate_continuously = false; // on reset la navigation continue
            continuous_navigation_counter = float.MaxValue; // on reset le compteur de navigation continue
        }
    }

    // ACTIVATE SLOT
    private void HandleActivateInput(float input)
    {
        // do nothing if this input is mouse_based
        if (!input_manager.isUsingGamepad())
        {
            if (debug) { Debug.Log("(XboxNavigator - HandleActivateInput) activate input ignored because mouse based"); }
            return;
        }

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
            slot.OnPointerDown(null);
            if (debug) { Debug.Log("(UI_Navigator) pressed slot " + slot.gameObject.name); }
        }
        else
        {
            // on relache le slot
            activate(slot);
            if (debug) { Debug.Log("(UI_Navigator) activated slot " + slot.gameObject.name); }
        }
    }
    private async void activate(I_UI_Slot slot)
    {
        if (slots.Count == 0 || current_slot_index == -1) { return; }

        // on retient la position du slot
        Vector2 position = get_position(slot.gameObject);

        // on clique sur le slot
        slot.OnPointerClick(null);

        // si on utilise la souris alors pas besoin de naviguer vers le plus proche
        if (!input_manager.isUsingGamepad()) { return; }

        // wait for a few frame to let the click happen
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();

        if (AppManager.Instance.IsQuitting) { return; }

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
    }

    // DROP
    public async void OnDrop()
    {
        if (slots.Count == 0 || current_slot_index == -1) { return; }
        UI_Item slot = slots[current_slot_index].GetComponent<UI_Item>();
        if (slot == null) { return; }

        // on retient la position du slot
        Vector2 position = get_position(slot.gameObject);

        // on vide le slot
        // on appelle OnPointerDropped pour simuler un drop
        slot.OnPointerDropped(null);

        // si on utilise la souris alors pas besoin de naviguer vers le plus proche
        if (!input_manager.isUsingGamepad()) { return; }

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
    }
    

    // MOVING ITEM THROUGH INVENTORY
    private void HandleMoveItemInput(float input)
    {
        // do nothing if this input is mouse_based
        if (!input_manager.isUsingGamepad())
        {
            if (debug) { Debug.Log("(XboxNavigator - HandleMoveItemInput) move input ignored because mouse based"); }
            return;
        }

        // cette fonction gère le déplacement des items dans l'inventaire

        // 1 - on récupère le slot actuel
        if (current_slot_index == -1) { return; }
        GameObject go = slots[current_slot_index];
        if (go == null) { return; }
        I_UI_Slot slot = go.GetComponent<I_UI_Slot>();
        if (slot == null) { return; }

        // 2 - on regarde si le slot est un UI_Item
        if (slot is not UI_Item)
        {
            if (debug)
            {
                Debug.Log("(XboxNavigator - HandleMoveItemInput) move input ignored for slot " + slot.gameObject.name
                + " because it is not a UI_Item (" + slot.GetType().Name + ")");
            }
            return;
        }

        // 3 - on regarde si on a down ou up
        if (input > 0.5f) { start_moving_ui_item(slot as UI_Item); }
        else if (moving_ui_item != null) { finish_moving_ui_item(slot as UI_Item); }
    }
    private void start_moving_ui_item(UI_Item potential_ui_item)
    {
        // on check si on a bien un item & un itempool
        if (potential_ui_item.ItemPool == null || potential_ui_item.Item == null)
        {
            if (debug) { Debug.LogWarning("(UI_Navigator) moving item " + potential_ui_item.gameObject.name + " has no ItemPool or Item. cannot move it."); }
            return;
        }

        // on active le moving item
        moving_ui_item = potential_ui_item;
        enable_only_recevable_slots();
        update_slots(); // on met à jour les slots

        // on remet l'index à l'index du ui_moving_item
        current_slot_index = slots.IndexOf(moving_ui_item.gameObject);
        if (debug) { Debug.Log("(UI_Navigator) updated slots for moving item. current_slot_index is now : " + current_slot_index); }

        // on down le slot
        moving_ui_item.OnPointerDragDown();
        if (debug) { Debug.Log("(UI_Navigator) drag downed item " + potential_ui_item.gameObject.name); }
    }
    private void finish_moving_ui_item(UI_Item destination)
    {
        // on release les 2 slos
        moving_ui_item.OnPointerExit(null);
        destination.OnPointerExit(null);

        // on choisit le mode d'action qu'il faut pour echanger les items
        if (destination == moving_ui_item) { /* we do nothing -> will just avoid moving them */ } 
        else if (destination != moving_ui_item && destination.CanStore(moving_ui_item.GetItems()))
        {
            merge_items(moving_ui_item, destination); // ce sont les mêmes items, on peut alors les merge ensemble
        }
        else if (destination is UI_Module ui_module
            && moving_ui_item is not UI_Module
            && moving_ui_item.Quantity > 1)
        {
            split_items(ui_module, moving_ui_item); // on split l'item
        }
        else if (moving_ui_item is UI_Module ui_module2
            && destination is not UI_Module
            && destination.Quantity > 1)
        {
            split_items(ui_module2, destination); // on split l'item
        }
        else { switch_items(moving_ui_item, destination); }

        // on met à jour les slots
        disable_only_empty_slots(true);
        update_slots();

        // et on renavigue vers destination
        int destination_index = slots.IndexOf(destination.gameObject);
        if (destination_index == -1)
        {
            destination_index = slots.IndexOf(moving_ui_item.gameObject);
            destination = moving_ui_item; // on remet le destination à l'ui_item en cours de drag
        }
        current_slot_index = destination_index;
        destination.OnPointerEnter(null); // on hover le slot de destination
        OnSlotHoverEnter?.Invoke(destination); // on déclenche l'event OnSlotHoverEnter

        // on relache le drag
        if (debug) { Debug.Log($"(UI_Navigator) item {moving_ui_item.gameObject.name} switched position with {destination.gameObject.name}"); }
        moving_ui_item = null;
    }

    // MOVING ITEM LOW LEVEL
    private void enable_only_recevable_slots()
    {
        UI_ItemPool moving_pool = moving_ui_item.ItemPool;
        Item moving_item = moving_ui_item.Item;
        if (log_receivable_slots) { Debug.Log($"(UI_Navigator) enabling only receivable slots for item {moving_item.name} in pool {moving_pool.name}"); }

        List<UI_ItemPool> item_pools = new List<UI_ItemPool>();

        // on récupère les slots
        foreach (GameObject slottable in slottables)
        {
            if (slottable == null) { continue; }

            // si le slottable est ni un ui_inventory ni un ui_inventorymenu pas la peine de continuer y'a pas d'ui_items
            UI_Inventory inventory = get_inventory_for_slottable(slottable);
            if (inventory == null) { continue; }
            
            // on récupère tous les UI_ItemPool du slottable si on a un UI_Inventory
            for (int i = 0; i < inventory.pools.Count; i++)
            {
                UI_ItemPool item_pool = inventory.pools[i];
                if (!item_pools.Contains(item_pool)) { item_pools.Add(item_pool); }
            }
            

            // on récupère les slots du slottable
            List<GameObject> slottable_slots = slottable.GetComponent<I_UI_Slottable>().GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);
            foreach (GameObject slot in slottable_slots)
            {
                I_UI_Slot ui_slot = slot.GetComponent<I_UI_Slot>();
                if (ui_slot is not UI_Item ui_item) { continue; }
                if (ui_item == moving_ui_item) { continue; } // on ne désactive pas le slot en cours de drag

                // on regarde si le slot peut recevoir le moving item
                if (ui_item.ItemPool == null || !ui_item.ItemPool.CanStore(moving_item))
                {
                    ui_item.Disable(); // on désactive le slot
                    continue;
                }

                // on regarde si le slot a un item qui peut etre recu par le moving_ui_item_pool
                if (ui_item.Item != null && !moving_pool.CanStore(ui_item.Item))
                {
                    ui_item.Disable(); // on désactive le slot
                    continue;
                }

                // sinon on active le slot
                ui_item.Enable();
            }
        }

        if (log_receivable_slots) { Debug.Log($"(UI_Navigator) now trying to add empty slots to ({item_pools.Count}) scalable item pools : {string.Join(", ", item_pools)} "); }

        // on parcourt les item pools et on ajoute un ui_item vide si c'est un item pool scalable
        // utile pour pouvoir déposer des ui_items dans une item pool scalable
        foreach (UI_ItemPool item_pool in item_pools)
        {
            if (item_pool == moving_pool) { continue; }
            if (!item_pool.Scalable) { continue; }
            if (!item_pool.CanStore(moving_item)) { continue; }
            if (item_pool.EmptyCount > 0) { continue; }
            GameObject empty_slot = item_pool.CreateItemSlot();
            empty_slot.GetComponent<UI_Item>().Enable();
            if (log_receivable_slots) { Debug.Log($"(UI_Navigator) added empty slot to item pool {item_pool.name}"); }
        }

        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        if (UI_Manager.Instance.CurrentPool == "inventory")
        {
            UI_InventoryMenu inventory_menu = UI_Manager.Instance.GetPool("inventory") as UI_InventoryMenu;
            if (inventory_menu != null)
            {
                inventory_menu.RefreshItemPools();
            }
        }

    }
    private void disable_only_empty_slots(bool except_modules = false)
    {
        // on sauvegarde les item pools qu'on trouve
        List<UI_ItemPool> item_pools = new List<UI_ItemPool>();

        foreach (GameObject slottable in slottables)
        {
            if (slottable == null) { continue; }

            // si le slottable est ni un ui_inventory ni un ui_inventorymenu pas la peine de continuer y'a pas d'ui_items
            UI_Inventory inventory = get_inventory_for_slottable(slottable);
            if (inventory == null) { continue; }

            // on ajoute les item pools du slottable
            foreach (UI_ItemPool item_pool in inventory.pools)
            {
                if (!item_pools.Contains(item_pool)) { item_pools.Add(item_pool); }
            }

            // on récupère les slots du slottable
            List<GameObject> slottable_slots = slottable.GetComponent<I_UI_Slottable>().GetSlots(ref base_position, ref angle_threshold, ref angle_multiplicator);
            foreach (GameObject slot in slottable_slots)
            {
                I_UI_Slot ui_slot = slot.GetComponent<I_UI_Slot>();
                if (ui_slot is not UI_Item ui_item) { continue; }

                // on regarde si le slot n'a pas d'item on le désactive
                if (ui_item.Item != null) { ui_item.Enable(); continue; }
                if (except_modules && ui_item is UI_Module) { ui_item.Enable(); continue; } // on ne désactive pas les modules
                if (ui_item.ItemPool != null && ui_item.ItemPool.DoNotDisableEmptySlots) { ui_item.Enable(); continue; }
                ui_item.Disable(); // on désactive le slot
            }
        }

        // on parcourt les item pools et on supprime les ui_item vide si c'est un item pool scalable
        foreach (UI_ItemPool item_pool in item_pools)
        {
            if (item_pool.Scalable) { item_pool.DestroyEmptySlots(); }
        }


        // si on a un UI_InventoryMenu dans nos uis alors on refresh ses UI_ItemPools
        if (GetComponent<UI_Manager>().CurrentPool == "inventory")
        {
            UI_InventoryMenu inventory_menu = GetComponent<UI_Manager>().GetPool("inventory") as UI_InventoryMenu;
            if (inventory_menu != null)
            {
                inventory_menu.RefreshItemPools();
            }
        }
    }
    private UI_Inventory get_inventory_for_slottable(GameObject slottable)
    {
        // on récupère l'UI_Inventory si on en trouve un
        I_UI_Slottable slottable_interface = slottable.GetComponent<I_UI_Slottable>();
        UI_Inventory inventory = null;
        if (slottable_interface is UI_Inventory) { inventory = slottable_interface as UI_Inventory; }
        else if (slottable_interface is UI_InventoryMenu inventory_menu) { inventory = inventory_menu.UI_Inventory; }

        return inventory;
    }
    private void switch_items(UI_Item item1, UI_Item item2)
    {
        // on échange les items entre les deux UI_Items
        if (item1 == null || item2 == null) { return; }

        if (log_moving_items) { Debug.Log($"(UI_Navigator) switching items between {item1.gameObject.name} and {item2.gameObject.name}"); }

        // on sauvegarde les items
        List<Item> items1 = item1.GetItems();
        List<Item> items2 = item2.GetItems();

        // on echange les items
        item1.SwitchItems(items2);
        item2.SwitchItems(items1);

        // on regarde si on est dans deux inventaires différents
        Inventory inventory1 = item1.Inventory;
        Inventory inventory2 = item2.Inventory;
        if (inventory1 == null || inventory2 == null)
        {
            if (debug)
            {
                Debug.LogWarning($"(UI_Navigator) switched items between {item1.gameObject.name} "
            + $"and {item2.gameObject.name} but at least one inventory is null : {inventory1?.capable.name} and {inventory2?.capable.name}");
            }
            return;
        }
        if (inventory1 == inventory2) { return; } // we stay inside the same inventory so no need to update Items's inventories

        List<UI_Inventory> uis_to_ignore = new List<UI_Inventory>() { item1.ItemPool.UI_Inventory, item2.ItemPool.UI_Inventory };

        // on met à jour les inventories des items
        foreach (Item item in items1)
        {
            inventory2.Grab(item, uis_to_ignore); // on ignore les ui_inventory parce qu'ils ont déjà été grab dans ces UI_Inventory
        }
        foreach (Item item in items2)
        {
            inventory1.Grab(item, uis_to_ignore); // pareil
        }
    }
    private void split_items(UI_Module ui_module, UI_Item ui_item)
    {
        if (log_moving_items) { Debug.Log($"(UI_Navigator) splitting items between {ui_item.gameObject.name} and {ui_module.gameObject.name}"); }


        // on vérifie que y'a pas déjà un module installé (sinon ça va tout kc)
        // todo : faire en sorte que si un module est déjà installé il est juste drop dans l'inventaire et ça
        // todo : switch quand mm le 1er module
        if (ui_module.Item != null)
        {
            if (debug)
            {
                Debug.LogWarning($"(UI_Navigator) cannot split items from {ui_item.gameObject.name} to {ui_module.gameObject.name} because it already has a module installed.");
            }
            return;
        }

        // on récupère le 1er item de ui_item sous la forme d'une liste
        Item item_to_move = ui_item.Item;
        List<Item> remaining_items = ui_item.GetItems();
        remaining_items.Remove(item_to_move);

        // on echange les items
        ui_module.SwitchItems(new List<Item>() { item_to_move });
        ui_item.SwitchItems(remaining_items);

        // on regarde si on est dans deux inventaires différents
        Inventory ui_item_inv = ui_item.Inventory;
        Inventory ui_module_inv = ui_module.Inventory;
        if (ui_item_inv == null || ui_module_inv == null)
        {
            if (debug)
            {
                Debug.Log($"(UI_Navigator) switched items between {ui_item.gameObject.name} "
            + $"and {ui_module.gameObject.name} but at least one inventory is null : {ui_item_inv?.capable.name} and {ui_module_inv?.capable.name}");
            }
            return;
        }
        if (ui_item_inv == ui_module_inv) { return; } // we stay inside the same inventory so no need to update Items's inventories

        List<UI_Inventory> uis_to_ignore = new List<UI_Inventory>() { ui_item.ItemPool.UI_Inventory, ui_module.ItemPool.UI_Inventory };
        ui_module_inv.Grab(item_to_move, uis_to_ignore); // on ignore les ui_inventory parce qu'ils ont déjà été grab dans ces UI_Inventory
    }
    private void merge_items(UI_Item item1, UI_Item item2)
    {
        if (log_moving_items) { Debug.Log($"(UI_Navigator) merging items between {item1.gameObject.name} and {item2.gameObject.name}"); }

        // on merge les items de item1 dans item2
        List<Item> items = item1.GetItems();
        List<Item> transfered_items = new List<Item>();
        while (item2.Store(items[0]))
        {
            transfered_items.Add(items[0]); // on ajoute l'item à la liste des items transférés
            items.RemoveAt(0);
            if (items.Count == 0) { break; } // si on a plus d'items on sort de la boucle
        }

        // on vide le slot de item1
        item1.SwitchItems(items);

        // on regarde si on est dans deux inventaires différents
        Inventory inventory1 = item1.Inventory;
        Inventory inventory2 = item2.Inventory;
        if (inventory1 == null || inventory2 == null)
        {
            if (debug)
            {
                Debug.Log($"(UI_Navigator) merged items between {item1.gameObject.name} "
            + $"and {item2.gameObject.name} but at least one inventory is null : {inventory1?.capable.name} and {inventory2?.capable.name}");
            }
            return;
        }
        if (inventory1 == inventory2) { return; } // we stay inside the same inventory so no need to update Items's inventories

        if (debug)
        {
            Debug.Log($"(UI_Navigator) merged items between {item1.ItemPool.UI_Inventory.name} "
            + $"and {item2.ItemPool.UI_Inventory.name} with {items.Count} items left in {item1.gameObject.name}");
        }

        // on met à jour les inventories des items
        List<UI_Inventory> uis_to_ignore = new List<UI_Inventory>() { item1.ItemPool.UI_Inventory, item2.ItemPool.UI_Inventory };
        foreach (Item item in transfered_items)
        {
            inventory2.Grab(item, uis_to_ignore); // on ignore les ui_inventory parce qu'ils ont déjà été grab dans ces UI_Inventory
            // inventory1.Drop(item, uis_to_ignore); // on drop l'item de l'inventaire 1
        }
    }


    // GIZMOS
    private void OnDrawGizmos()
    {
        if (gizmo_position == Vector2.negativeInfinity) { return; }
        if (!debug_navigation) { return; }

        // we get the last navigation slot position from the gizmos list
        Vector2 position = gizmo_position;

        if (gizmos_positions.Count == 0)
        {
            // we draw a cross to say that we had nothing in angle
            Gizmos.color = Color.red; // Set the color to red
            Gizmos.DrawLine(position + Vector2.up * 0.1f, position - Vector2.up * 0.1f);
            Gizmos.DrawLine(position + Vector2.right * 0.1f, position - Vector2.right * 0.1f);
            return;
        }

        /* // we split the rest of the gizmos list in little portions of 2 (one for each slot)
        for (int slot = 1; slot < gizmos_positions.Count; slot += 2)
        {
            // we show the distance
            Gizmos.color = Color.red; // Set the color to red
            Gizmos.DrawLine(position, gizmos_positions[slot]);

            // we show the distance influenced by the angle
            Gizmos.color = Color.green; // Set the color to green
            Gizmos.DrawLine(gizmos_positions[slot], gizmos_positions[slot + 1]);
        } */

        // we normalize the weights so the minimum is at 0.25 and max is at 1
        string s = "(UI_Navigator) GIZMOS WEIGHTS: " + gizmos_weights.Count + "\n";
        List<float> gizmos_weights_normalised = new List<float>(gizmos_weights); // we copy the weights to avoid modifying the original list

        if (gizmos_weights.Count > 1)
        {
            float min_weight = Mathf.Min(gizmos_weights.ToArray());
            float max_weight = Mathf.Max(gizmos_weights.ToArray());

            for (int i = 0; i < gizmos_weights.Count; i++)
            {
                // normalize the weight
                gizmos_weights_normalised[i] = (gizmos_weights[i] - min_weight) / (max_weight - min_weight);
                gizmos_weights_normalised[i] = (gizmos_weights_normalised[i] + 0.25f) / 1.25f; // we shift the weight to be between 0.25 and 1

                // we log
                s += "\n\t" + i + " : " + gizmos_weights[i] + " -> " + gizmos_weights_normalised[i];
            }
        }

        if (debug_gizmo) { Debug.Log(s); }

        // we show the rest of the gizmo with a color on a scale from red to green
        for (int i = 0; i < gizmos_positions.Count; i++)
        {
            // get position & weight
            Vector2 position_slot = gizmos_positions[i];
            float weight = gizmos_weights_normalised[i];

            // we draw the slot position in green if weight = 1f, red if not
            if (weight == 1f || gizmos_weights.Count == 1) { Gizmos.color = Color.green; }
            else { Gizmos.color = Color.red; }
            Gizmos.DrawSphere(position_slot, 0.10f); // Draw a sphere at the slot position

            // we change the color
            Color color = Color.Lerp(Color.red, Color.green, weight);
            Gizmos.color = color; // Set the color based on the weight

            // we get the direction vector between the position and the slot
            Vector2 direction = position_slot - position;
            float magnitude = direction.magnitude * weight;
            Vector2 dot_position = direction.normalized * magnitude + position;
            Gizmos.DrawLine(position, dot_position);
        }
    }
}