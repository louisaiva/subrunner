using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using Unity.Cinemachine;

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
    public float angle_threshold = 45f;
    public float angle_multiplicator = 0f;
    // [SerializeField] public float angle_vs_distance_precision = 0f; // from 0 to 1, affine la prédiction de navigation
    


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
    public bool debug_gizmo = false;

    [Header("Gizmos")]
    private Vector2 gizmo_position = Vector2.negativeInfinity; // used to transmit the position to OnDrawGizmo
    public List<Vector2> gizmos_positions = new List<Vector2>(); // used to show gizmos, Vector2 in world space
    public List<float> gizmos_weights = new List<float>(); // used to show gizmos, weight from 0 to 1
    // [Range(1, 10)] public double gizmo_dot_size = 1.0; // size of the dots in the gizmos

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
        current_slot_index = -1;

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

        // log
        if (debug) { Debug.Log("(XboxNavigator) enabled slotabble : " + slottable.gameObject.name); }

        // on verifie si on utilise le clavier ou le controller
        if (!input_manager.isUsingGamepad()) { return; }

        // on navigue vers le premier slot
        current_slot_index = -1;
        navigateToFirst();

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

        string s = "(XboxNavigator) NAVIGATE: \n\nparameters: \n\tangle_threshold : " + angle_threshold + "\n\tangle_multiplicator: " + angle_multiplicator + "\n\n";

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
        if (slottables.Count > 0)
        {
            // on récupère les slots
            slots = new List<GameObject>();
            foreach (GameObject slottable in slottables)
            {
                if (slottable == null) { continue; }
                
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

    // GET SLOT POSITION
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

        // on regarde si le slot est positionné dans un canvas world space or screen space
        if (slot.layer == LayerMask.NameToLayer("UI_World"))
        {
            // on le convertit en position
            position = Camera.main.WorldToScreenPoint(position);
        }

        return position;
    }


    // MOVING / DROPPING SLOTS
    private void HandleMoveInput(float input)
    {
        // do nothing if this input is mouse_based
        if (!input_manager.isUsingGamepad())
        {
            if (debug) { Debug.Log("(XboxNavigator - HandleActivateInput) activate input ignored because mouse based"); }
            return;
        }

        // cette fonction gère les inputs de navigation et décide si on doit naviguer ou non
        // si oui elle appelle alors drop() ou pressed()

        // 1 - on récupère le slot actuel
        if (current_slot_index == -1) { return; }
        GameObject go = slots[current_slot_index];
        if (go == null) { return; }
        I_UI_Slot slot = go.GetComponent<I_UI_Slot>();
        if (slot == null) { return; }

        // on handle le move seulement pour les UI_Item
        if (slot is not UI_Item)
        {
            if (debug)
            {
                Debug.Log("(XboxNavigator - HandleMoveInput) move input ignored for slot " + slot.gameObject.name + " because it is not a UI_Item (" + slot.GetType().Name + ")");
            }
            return;
        }

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

        // si on utilise la souris alors pas besoin de naviguer vers le plus proche
        if (!input_manager.isUsingGamepad()) { return; }

        // wait for a frame to let the click happen
        await System.Threading.Tasks.Task.Yield();

        // on navigue vers le slot le plus proche
        navigateToClosest(position);
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
        string s = "(XboxNavigator) GIZMOS WEIGHTS: " + gizmos_weights.Count + "\n";
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