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
    [SerializeField] private GameObject targeted_connector;
    public GameObject CurrentTarget => targeted_connector;
    public Vulnerable CurrentVulnerable => targeted_connector.GetComponent<Vulnerable>();
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
    public HackCapacity hacker => Perso.Instance.Laptop?.GetCapacity<HackCapacity>();
    // public ConnectCapacity connector;
    private ConnectCapacity connector => Controller.Instance.Capable.Connector;
    private Laptop laptop => hacker.capable as Laptop;
    [SerializeField] private ConnectCapacity cursor;
    // public Laptop laptop;

    [Header("Log")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_update = false;
    [SerializeField] private bool log_enabling = false;


    // START
    private void Start()
    {
        // UI_LaptopItemSlot.Instance.OnItemChanged += ctx => OnLaptopChanged(ctx.Count > 0 ? ctx[0] as Laptop : null);

        // we get the navigation action & create the callbacks
        navigationAction = InputManager.Instance.GetAction(navigationInput);
        navigationCallback = ctx => HandleHackNavigationInput(ctx.ReadValue<Vector2>());

        if (log_enabling) { Debug.Log("(VulnerableNavigator) started & callbacks created"); }
    }

    private void OnDestroy()
    {
        Disable();
        // UI_LaptopItemSlot.Instance.OnItemChanged -= ctx => OnLaptopChanged(ctx.Count > 0 ? ctx[0] as Laptop : null);
    }

    // UPDATE
    private void Update()
    {
        // on récupère le hackable
        if (targeted_connector == null || connector == null || hover_hackray == null) { return; }
        if (CurrentVulnerable == null) { unselect_target(); return; } // on vérifie si le hackable est toujours valide

        // on met à jour le hackable
        string log = update_vulnerable(CurrentVulnerable);
        if (log_update && log != "") { Debug.Log(log); }
    }
    private string update_vulnerable(Vulnerable vulnerable)
    {
        string logg = "(VulnerableNavigator) Updating vulnerable : " + vulnerable.name + " --> ";


        // check if we can't connect to the vulnerable
        if (!connector.IsConnectedTo(vulnerable))
        {
            hover_hackray.SetColor(out_of_range_hackray_color);
            return logg + "(out of range)";
        }

        // check if we found any vulnerabilities
        Exploit exploit = hacker?.selected_exploit;
        if (exploit == null || !vulnerable.IsVulnerableTo(exploit))
        {
            hover_hackray.SetColor(no_vulnerability_hackray_color);
            return logg + "(no vulnerability - exploit: " + (exploit != null ? exploit.name : "null") + ")";
        }

        // check if we have the required cores
        if (!(hacker.capable as Laptop).Processor.HasFreeCores(exploit.cores_cost))
        {
            hover_hackray.SetColor(no_cores_hackray_color);
            return logg + "(no cores)";
        }

        // otherwise we can hack it !!!!
        hover_hackray.SetColor(hackable_color);
        return logg + "(ready to hack)";
    }

    // TARGET SELECTION
    private void select_target(Vulnerable vulnerable)
    {
        // we switch the current target
        if (targeted_connector != null) { unselect_target(); }
        targeted_connector = vulnerable.gameObject;
        if (log) { Debug.Log($"(VulnerableNavigator) ready to launch connection : {connector.capable.name} --> {vulnerable.capable.name}"); }

        connector.Connect(vulnerable,hacker);

        // we set the hovered target material
        vulnerable.Renderer.material = vulnerable.TargetMaterial;

        // we update the hackray
        hover_hackray.SetColor(connector.IsConnectedTo(vulnerable) ? hackray_color : out_of_range_hackray_color);
        // hover_hackray.SetLaptopAndTarget(laptop, vulnerable.transform);
        hover_hackray.SetConnectors(connector, vulnerable.Connector);
        hover_hackray.gameObject.SetActive(true);
    }
    private void unselect_target()
    {
        // we check if we have a current target
        if (targeted_connector == null) { return; }

        connector.Disconnect();
        hacker?.DeselectExploit();

        // we update the hackray
        hover_hackray?.SetColor(hackray_color);
        // hover_hackray?.SetLaptopAndTarget(laptop, cursor);
        hover_hackray.SetConnectors(connector, cursor);

        // we reset the vulnerable material
        Vulnerable vulnerable = targeted_connector.GetComponent<Vulnerable>();
        if (vulnerable == null)
        {
            targeted_connector = null;
            return;
        }
        vulnerable.Renderer.material = vulnerable.DefaultMaterial;

        // we reset the current target
        if (log) { Debug.Log($"(VulnerableNavigator) resetted connection : {connector.capable.name} -x> {vulnerable.capable.name}"); }
        targeted_connector = null;
    }

    // ENABLE / DISABLE
    public void Enable()
    {
        // we check if we have a connector
        if (connector == null)
        {
            if (log_enabling) { Debug.LogWarning("(VulnerableNavigator) no ConnectCapacity found, disabling navigator"); }
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
        // hover_hackray.SetLaptopAndTarget(laptop, cursor);
        hover_hackray.SetConnectors(connector, cursor);

        // we activate the cursor
        cursor.gameObject.SetActive(false);


        if (log_enabling) { Debug.Log("(VulnerableNavigator) enabled & callbacks set"); }

        // we select the last hackable if we still have some
        if (targeted_connector == null || hacker == null) { return; }
        select_target(targeted_connector.GetComponent<Vulnerable>());
    }
    public void Disable()
    {
        // we deactivate the callbacks
        if (log_enabling) Debug.Log("(VulnerableNavigator) about to remove navigation callback");
        if (navigationAction != null) { navigationAction.performed -= navigationCallback; }
        if (log_enabling) Debug.Log("(VulnerableNavigator) removed navigation callback");

        unselect_target();

        // we update the hackray & cursor
        hover_hackray?.gameObject.SetActive(false);
        cursor.gameObject.SetActive(false);

        if (log_enabling) { Debug.Log("(VulnerableNavigator) disabled & callbacks removed"); }
    }

    // ON LAPTOP CHANGED
    /* private void OnLaptopChanged(Laptop new_laptop)
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
        // connector = new_laptop.GetCapacity<ConnectCapacity>();
    } */
    /* private void OnLaptopModuleChanged(Item item)
    {
        hacker = laptop.GetCapacity<HackCapacity>();
        connector = laptop.GetCapacity<ConnectCapacity>();
    } */
    /* public void SetConnector(ConnectCapacity new_connector)
    {
        connector = new_connector;
        // hacker.SetConnector(new_connector);
    } */

    [Header("Inputs")]
    [SerializeField] private InputActionReference navigationInput;
    private InputAction navigationAction;
    private event System.Action<InputAction.CallbackContext> navigationCallback;


    // TARGET SELECTION HANDLE INPUT
    public void HandleHackNavigationInput(Vector2 input)
    {
        // on vérifie qu'on est au moins dans le threshold min
        if (input.magnitude < InputManager.Instance.JOYSTICK_MIN_THRESHOLD) { return; }

        // update cursor & hackray
        update_cursor(input);
        hover_hackray.gameObject.SetActive(true);
        cursor.gameObject.SetActive(true);

        // get the closest hackable in range
        Vulnerable vulnerable = raycast_closest_vulnerable(input);


        // on vérifie qu'on a bien trouvé une target
        if (vulnerable == null)
        {
            unselect_target();
            return;
        }

        // on regarde si c'est le même objet que le précédent
        if (targeted_connector != null && targeted_connector == vulnerable.gameObject) { return; }

        // on sélectionne le nouveau vulnerable
        select_target(vulnerable);
    }
    private Vulnerable raycast_closest_vulnerable(Vector2 input)
    {
        Vector2 direction = input.normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, selection_radius, hackableLayerMask);
        if (hits.Length == 0) { return null; }

        // we save the on-hacking processor
        Vulnerable being_hacked_vulnerable = null;

        // we find the closest hackable to cursor
        float min_distance = float.MaxValue;
        Vulnerable closest_vulnerable = null;
        foreach (RaycastHit2D hit in hits)
        {
            GameObject target_go = hit.collider.gameObject;

            Vulnerable target = target_go.GetComponent<Vulnerable>();
            if (target == null) { continue; }
            
            // we remove ourselves
            if (target.capable == Controller.Instance.Capable) { continue; }
            if (target.capable is Item item && item.Holder != null && item.Holder == Controller.Instance.Capable) { continue; }

            // we remove the target we are already hacking
            if (hacker?.IsHacking(target) == true) { being_hacked_vulnerable = target; continue; }

            // we check if this is a lockable unlocked we skip it
            if (target.capable is Lockable lockable && !lockable.Locked) { continue; }

            // we compare the distance
            float distance = Vector2.Distance(transform.position, target_go.transform.position);
            if (distance < min_distance)
            {
                min_distance = distance;
                closest_vulnerable = target;
            }
        }
        if (closest_vulnerable == null) { return being_hacked_vulnerable; } // returns a current processor being hacked if hitted, null otherwise
        return closest_vulnerable; // we return the parent hackable
    }

    // cursor update
    private void update_cursor(Vector2 input)
    {
        // on met à jour le curseur
        if (cursor == null || connector == null) { return; }
        cursor.capable.transform.localPosition = new Vector3(input.x * connector.Radius, input.y * connector.Radius, 0f);
    }
}