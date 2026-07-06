using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// this component must be a direct child of the controller (and ONLY the controller)
/// this is used by the UI_Hacking pool to navigate through the vulnerables in order to select the target to hack
/// it has a larger collider radius than the device's one because we want to hover out-of-range vulnerables
/// and warn the player it is not hackable rn in the UI_Hacking
/// also handles hover hackray & target hackable material
/// </summary>
public class HackableNavigator : MonoBehaviour
{

    public bool IsSelecting = false;
    public bool IsConnected => targeted_connector != null;

    [Header("Hackables selection")]
    [SerializeField] private GameObject targeted_connector;
    public ConnectCapacity CurrentTarget => targeted_connector?.GetComponent<ConnectCapacity>();
    public Vulnerable CurrentVulnerable => targeted_connector?.GetComponent<Vulnerable>();
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
    private Device device => Controller.Perso.Device;
    public HackCapacity hacker => device?.Hacker;
    public ConnectionTree Tree => device?.Connector.Tree;
    private ConnectCapacity connector => Controller.Capable.Connector;
    [SerializeField] private ConnectCapacity cursor;
    private TextMeshProUGUI kf_exploit_text;

    [Header("Log")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_update = false;
    [SerializeField] private bool log_enabling = false;
    [SerializeField] private bool log_material_switching = false;

    // START
    void Start()
    {
        kf_exploit_text = UI_Manager.Instance.GetPool("hacking").transform.Find("details/hack_if/kf_exploit_text").GetComponent<TextMeshProUGUI>();
        if (kf_exploit_text == null)
        {
            Debug.LogWarning("(VulnerableNavigator) missing kf_exploit_text reference");
        }
    }
    
    // UPDATE
    private void Update()
    {
        // on vérifie qu'on est en mode sélection
        if (!IsSelecting) { return; }

        // on récupère le hackable
        if (targeted_connector == null || connector == null || hover_hackray == null) { return; }
        if (CurrentVulnerable == null) { unselect_target(); return; } // on vérifie si le hackable est toujours valide

        // on met à jour le hackable
        string log = update_connector(CurrentTarget);
        if (log_update && log != "") { Debug.Log(log); }
    }
    private string update_connector(ConnectCapacity target)
    {
        string logg = "(VulnerableNavigator) Updating vulnerable : " + target.Capable.ID + " --> ";


        // check if we can't connect to the vulnerable
        if (!connector.IsConnectedTo(target))
        {
            hover_hackray.SetColor(out_of_range_hackray_color);
            return logg + "(out of range)";
        }

        // check if we found any vulnerabilities
        Exploit exploit = hacker?.selected_exploit;
        if (exploit == null || !target.Vulnerable.IsVulnerableTo(exploit))
        {
            hover_hackray.SetColor(no_vulnerability_hackray_color);
            return logg + "(no vulnerability - exploit: " + (exploit != null ? exploit.name : "null") + ")";
        }

        // check if we have the required cores
        if (!(hacker.Capable as Device).Processor.HasFreeCores(exploit.cores_cost))
        {
            hover_hackray.SetColor(no_cores_hackray_color);
            return logg + "(no cores)";
        }

        // otherwise we can hack it !!!!
        hover_hackray.SetColor(hackable_color);
        return logg + "(ready to hack)";
    }

    // TARGET SELECTION
    private void select_target(ConnectCapacity target)
    {
        // we switch the current target
        if (targeted_connector != null) { unselect_target(); }
        targeted_connector = target.gameObject;
        if (log) { Debug.Log($"(VulnerableNavigator) ready to launch connection : {connector.Capable.ID} --> {target.Capable.ID}"); }

        connector.Connect(target,hacker);

        // we set the hovered target material
        target.Vulnerable.Renderer.material = target.Vulnerable.TargetMaterial;
        if (log_material_switching) { Debug.Log($"(VulnerableNavigator) switched material of {target.Capable.ID} to targeted material"); }

        // we update the hackray
        hover_hackray.SetColor(connector.IsConnectedTo(target) ? hackray_color : out_of_range_hackray_color);
        hover_hackray.SetConnectors(connector, target);
        hover_hackray.gameObject.SetActive(true);
    }
    private void unselect_target()
    {
        // we check if we have a current target
        if (targeted_connector == null) { return; }

        connector.Disconnect();
        // hacker?.DeselectExploit();

        // we update the hackray
        hover_hackray?.SetColor(hackray_color);
        hover_hackray.SetConnectors(connector, cursor);

        // we reset the vulnerable material
        Vulnerable vulnerable = targeted_connector.GetComponent<Vulnerable>();
        if (vulnerable == null)
        {
            targeted_connector = null;
            return;
        }
        vulnerable.Renderer.material = vulnerable.BaseMaterial;
        if (log_material_switching) { Debug.Log($"(VulnerableNavigator) resetted material of {vulnerable.Capable.ID} to base material"); }


        // we reset the current target
        if (log) { Debug.Log($"(VulnerableNavigator) resetted connection : {connector.Capable.ID} -x> {vulnerable.Capable.ID}"); }
        targeted_connector = null;
    }

    // ENABLE / DISABLE
    public void Enable()
    {
        if (IsSelecting) { return; }

        // we check if we have a connector
        if (connector == null)
        {
            if (log_enabling) { Debug.LogWarning("(VulnerableNavigator) no ConnectCapacity found, disabling navigator"); }
            return;
        }
        
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
        hover_hackray.SetConnectors(connector, cursor);

        // we activate the cursor
        cursor.gameObject.SetActive(false);

        IsSelecting = true;

        // we change the exploit KF text to "run hack"
        kf_exploit_text.text = "run hack";
        
        if (log_enabling) { Debug.Log("(VulnerableNavigator) enabled & callbacks set"); }

        // we select the last hackable if we still have some
        if (targeted_connector == null || hacker == null) { return; }
        select_target(targeted_connector.GetComponent<ConnectCapacity>());
    }
    public void Disable()
    {
        unselect_target();

        // we update the hackray & cursor
        hover_hackray?.gameObject.SetActive(false);
        cursor?.gameObject.SetActive(false);

        IsSelecting = false;

        // we change the exploit KF text to "open exploit wheel"
        kf_exploit_text.text = "open exploit wheel";

        if (log_enabling) { Debug.Log("(VulnerableNavigator) disabled"); }
    }
    private void OnDestroy() => Disable();


    // TARGET SELECTION HANDLE INPUT
    public void HandleHackNavigationInput(Vector2 input)
    {
        // update cursor & hackray
        update_cursor(input);
        hover_hackray.gameObject.SetActive(true);
        cursor.gameObject.SetActive(true);

        // get the closest hackable in range
        ConnectCapacity connector = raycast_closest_connector(input);


        // on vérifie qu'on a bien trouvé une target
        if (connector == null)
        {
            unselect_target();
            return;
        }

        // on regarde si c'est le même objet que le précédent
        if (targeted_connector != null && targeted_connector == connector.gameObject) { return; }

        // on sélectionne le nouveau connector
        select_target(connector);
    }
    private ConnectCapacity raycast_closest_connector(Vector2 input)
    {
        Vector2 direction = input.normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, selection_radius, hackableLayerMask);
        if (hits.Length == 0) { return null; }

        // we save the on-hacking processor
        ConnectCapacity being_hacked_connector = null;

        // we find the closest hackable to cursor
        float min_distance = float.MaxValue;
        ConnectCapacity closest_connector = null;
        foreach (RaycastHit2D hit in hits)
        {
            GameObject target_go = hit.collider.gameObject;

            ConnectCapacity target = target_go.GetComponent<ConnectCapacity>();
            if (target == null) { continue; }
            
            // we remove ourselves
            if (target.Capable == Controller.Capable) { continue; }
            if (target.Capable is Item item && item.Holder != null && item.Holder == Controller.Capable) { continue; }

            // we remove the target we are already hacking
            if (hacker?.IsHacking(target.Vulnerable) == true) { being_hacked_connector = target; continue; }

            // we check if this is a lockable unlocked we skip it
            if (target.Capable is Lockable lockable && !lockable.Locked) { continue; }

            // or a powered off Onnable
            if (target.Capable is Onnable onnable && !onnable.IsOn) { continue; }

            // we compare the distance
            float distance = Vector2.Distance(transform.position, target_go.transform.position);
            if (distance < min_distance)
            {
                min_distance = distance;
                closest_connector = target;
            }
        }
        if (closest_connector == null) { return being_hacked_connector; } // returns a current processor being hacked if hitted, null otherwise
        return closest_connector; // we return the parent hackable
    }

    // cursor update
    private void update_cursor(Vector2 input)
    {
        // on met à jour le curseur
        if (cursor == null || connector == null) { return; }
        cursor.Capable.transform.localPosition = new Vector3(input.x * connector.Radius, input.y * connector.Radius, 0f);
    }
}