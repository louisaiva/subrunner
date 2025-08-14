using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// this component must be a direct child of the perso (and ONLY the perso)
/// this is used by the UI_Hacking pool to navigate through the hackables in order to select the target to hack
/// it has a larger collider radius than the laptop's one because we want to hover out-of-range hackables
/// and warn the player it is not hackable rn in the UI_Hacking
/// also handles hover hackray & target hackable material
/// </summary>
public class HackableNavigator : MonoBehaviour
{

    [Header("Hackables selection")]
    [SerializeField] private GameObject current_hackable;
    [SerializeField] private List<GameObject> waiting_hackables = new List<GameObject>();
    [SerializeField] private LayerMask hackableLayerMask = default;
    [SerializeField] private float selection_radius = 10f; // the radius of the selection collider

    [Header("Hackrays")]
    public GameObject hackray_prefab;
    [SerializeField] private Color hackray_color = Color.yellow;
    [SerializeField] private Color hackable_color = Color.yellow;
    [SerializeField] private Color out_of_range_hackray_color = Color.yellow;
    [SerializeField] private Color no_vulnerability_hackray_color = Color.yellow;
    [SerializeField] private Color no_cores_hackray_color = Color.yellow;
    [SerializeField] private float hackray_width = 1.8f; // the x scale of the hackray's sr
    private Hackray hover_hackray; // this is the hackray that is used to hover the target


    [Header("Components")]
    [SerializeField] private HackCapacity hacker;
    [SerializeField] private ConnectCapacity connector;
    [SerializeField] private Transform cursor;
    [SerializeField] private Laptop laptop;
    // private CircleCollider2D selection_collider;

    [Header("Log")]
    [SerializeField] private bool log = false;
    // [SerializeField] private bool log_navigation_inputs = false;


    // START
    private void Start()
    {
        UI_LaptopItemSlot.Instance.OnItemChanged += ctx => OnLaptopChanged(ctx.Count > 0 ? ctx[0] as Laptop : null);

        // we get the navigation action & create the callbacks
        navigationAction = InputManager.Instance.GetAction(navigationInput);
        navigationCallback = ctx => HandleHackNavigationInput(ctx.ReadValue<Vector2>());

        if (log) { Debug.Log("(HackableNavigator) started & callbacks created"); }
    }

    private void OnDestroy()
    {
        Disable();
        UI_LaptopItemSlot.Instance.OnItemChanged -= ctx => OnLaptopChanged(ctx.Count > 0 ? ctx[0] as Laptop : null);
    }

    // UPDATE
    private void Update()
    {
        // on récupère le hackable
        if (current_hackable == null || connector == null || hover_hackray == null) { return; }
        if (current_hackable.GetComponent<Hackable>() == null) { unselect_target(); return; } // on vérifie si le hackable est toujours valide

        // on met à jour le hackable
        update_hackable(current_hackable.GetComponent<Hackable>());
    }
    private void update_hackable(Hackable hackable)
    {
        // check if we can't connect to the hackable
        if (!connector.IsConnectedTo(hackable))
        {
            hover_hackray.SetColor(out_of_range_hackray_color);
            return;
        }

        // check if we found any vulnerabilities
        Exploit exploit = hacker?.GetExploitVulnerabilities(hackable);
        if (exploit == null)
        {
            hover_hackray.SetColor(no_vulnerability_hackray_color);
            return;
        }

        // check if we have the required cores
        if (!laptop.HasFreeCores(exploit.cores_cost))
        {
            hover_hackray.SetColor(no_cores_hackray_color);
            return;
        }

        // otherwise we can hack it !!!!
        hover_hackray.SetColor(hackable_color);
    }

    // TARGET SELECTION
    private void select_target(Hackable hackable)
    {
        // we switch the current target
        if (current_hackable != null) { unselect_target(); }
        current_hackable = hackable.gameObject;
        if (log) { Debug.Log("(HackableNavigator) " + hackable.name + " selected as closest target"); }

        connector.Connect(hackable);

        // we set the hovered target material
        hackable.spriteRenderer.material = hackable.TargetMaterial;

        // we update the hackray
        hover_hackray.SetColor(connector.IsConnectedTo(hackable) ? hackray_color : out_of_range_hackray_color);
        hover_hackray.SetLaptopAndTarget(laptop, hackable.transform);
        hover_hackray.gameObject.SetActive(true);
    }
    private void unselect_target()
    {
        // we check if we have a current target
        if (current_hackable == null) { return; }

        connector.Disconnect();

        // we update the hackray
        hover_hackray?.SetColor(hackray_color);
        hover_hackray?.SetLaptopAndTarget(laptop, cursor);

        // we reset the hackable material
        Hackable hackable = current_hackable.GetComponent<Hackable>();
        if (hackable == null)
        {
            current_hackable = null;
            return;
        }
        hackable.spriteRenderer.material = hackable.DefaultMaterial;

        // we reset the current target
        if (log) { Debug.Log("(HackableNavigator) " + current_hackable.name + " unselected as closest target"); }
        current_hackable = null;
    }

    // ENABLE / DISABLE
    public void Enable()
    {
        // we check if we have a connector
        if (connector == null)
        {
            if (log) { Debug.LogWarning("(HackableNavigator) no ConnectCapacity found, disabling navigator"); }
            return;
        }

        // we activate the callbacks
        navigationAction.performed += navigationCallback;

        // we update the hackray
        if (hover_hackray == null)
        {
            hover_hackray = Instantiate(hackray_prefab, transform).GetComponent<Hackray>();
            hover_hackray.name = "hover_hackray";
            Transform sr = hover_hackray.transform.Find("sr");
            sr.localScale = new Vector3(hackray_width, sr.localScale.y, sr.localScale.z);
        }
        hover_hackray.gameObject.SetActive(false);
        hover_hackray.SetColor(hackray_color);
        hover_hackray.SetLaptopAndTarget(laptop, cursor);

        // we activate the cursor
        cursor.gameObject.SetActive(false);


        if (log) { Debug.Log("(HackableNavigator) enabled & callbacks set"); }

        // we select the last hackable if we still have some
        if (current_hackable == null || hacker == null) { return; }
        select_target(current_hackable.GetComponent<Hackable>());
    }
    public void Disable()
    {
        // we deactivate the callbacks
        if (log) Debug.Log("(HackableNavigator) about to remove navigation callback");
        if (navigationAction != null) { navigationAction.performed -= navigationCallback; }
        if (log) Debug.Log("(HackableNavigator) removed navigation callback");

        unselect_target();

        // we update the hackray & cursor
        hover_hackray?.gameObject.SetActive(false);
        cursor.gameObject.SetActive(false);

        if (log) { Debug.Log("(HackableNavigator) disabled & callbacks removed"); }
    }

    // ON LAPTOP CHANGED
    private void OnLaptopChanged(Laptop new_laptop)
    {
        if (new_laptop == null)
        {
            unselect_target();
            hacker = null;
            connector = null;
            if (laptop != null) { (laptop.Inventory as LaptopInventory).OnModuleChanged -= OnLaptopModuleChanged; }
            laptop = null;
            return;
        }

        // we assign the hacker as the new laptop hack capacity
        laptop = new_laptop;
        (laptop.Inventory as LaptopInventory).OnModuleChanged += OnLaptopModuleChanged;
        hacker = new_laptop.GetCapacity<HackCapacity>();
        connector = new_laptop.GetCapacity<ConnectCapacity>();
    }
    private void OnLaptopModuleChanged(Item item)
    {
        hacker = laptop.GetCapacity<HackCapacity>();
        connector = laptop.GetCapacity<ConnectCapacity>();
    }


    [Header("Inputs")]
    [SerializeField] private InputActionReference navigationInput;
    private InputAction navigationAction;
    private event System.Action<InputAction.CallbackContext> navigationCallback;


    // TARGET SELECTION HANDLE INPUT
    public void HandleHackNavigationInput(Vector2 input)
    {
        // on vérifie qu'on est au moins dans le threshold min
        if (input.magnitude < InputManager.Instance.JOYSTICK_MIN_THRESHOLD) { return; }

        // on crée un vector qui clamp la magnitude entre JOY_MIN & JOY_MAX -> 0 & 1 (et conserve le signe)
        // Vector2 clean_inputs = calculate_clean_inputs(input);
        // if (log_navigation_inputs) { Debug.Log($"(HackableNavigator) navigation inputs : raw {input} , clean {clean_inputs}"); }

        // update cursor & hackray
        update_cursor(input);
        hover_hackray.gameObject.SetActive(true);
        cursor.gameObject.SetActive(true);

        // get the closest hackable in range
        GameObject hackable = raycast_closest_processor(input);


        // on vérifie qu'on a bien trouvé une target
        if (hackable == null)
        {
            unselect_target();
            return;
        }

        // on regarde si c'est le même objet que le précédent
        if (current_hackable != null && current_hackable == hackable) { return; }

        // on sélectionne le nouveau hackable
        select_target(hackable.GetComponent<Hackable>());
    }
    private GameObject raycast_closest_processor(Vector2 input)
    {
        Vector2 direction = input.normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, selection_radius, hackableLayerMask);
        if (hits.Length == 0) { return null; }

        // we save the on-hacking processor
        GameObject being_hacked_hackable = null;

        // we find the closest hackable to cursor
        float min_distance = float.MaxValue;
        GameObject closest_processor = null;
        foreach (RaycastHit2D hit in hits)
        {
            GameObject processor = hit.collider.gameObject;

            // we remove the target we are already hacking
            Hackable hackable = processor.transform.parent.GetComponent<Hackable>();
            if (hackable == null || hacker?.IsHacking(hackable) == true) { being_hacked_hackable = hackable.gameObject; continue; }

            // we compare the distance
            float distance = Vector2.Distance(transform.position, processor.transform.position);
            if (distance < min_distance)
            {
                min_distance = distance;
                closest_processor = processor;
            }
        }
        if (closest_processor == null) { return being_hacked_hackable; } // returns a current processor being hacked if hitted, null otherwise
        return closest_processor.transform.parent.gameObject; // we return the parent hackable
    }

    // cursor update
    private void update_cursor(Vector2 input)
    {
        // on met à jour le curseur
        if (cursor == null || connector == null) { return; }
        cursor.localPosition = new Vector3(input.x * connector.Radius, input.y * connector.Radius, 0f);
    }
}