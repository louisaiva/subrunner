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
    private Hackray hover_hackray; // this is the hackray that is used to hover the target


    [Header("Components")]
    [SerializeField] private HackCapacity hacker;
    [SerializeField] private Transform cursor;
    private Laptop laptop;
    // private CircleCollider2D selection_collider;

    [Header("Log")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_navigation_inputs = false;


    // START
    private void Start()
    {
        UI_LaptopItemSlot.Instance.OnItemChanged += ctx => OnLaptopChanged(ctx.Count > 0 ? ctx[0] as Laptop : null);

        // we get the navigation action & create the callbacks
        navigationAction = InputManager.Instance.GetAction(navigationInput);
        navigationCallback = ctx => HandleHackNavigationInput(ctx.ReadValue<Vector2>());

        if (log) { Debug.Log("(HackableNavigator) started & callbacks created"); }
    }

    // UPDATE
    private void Update()
    {
        // on récupère le hackable
        if (current_hackable == null || hacker == null || hover_hackray == null) { return; }
        if (current_hackable.GetComponent<Hackable>() == null) { unselect_target(); return; } // on vérifie si le hackable est toujours valide

        // on met à jour le hackable
        update_hackable(current_hackable.GetComponent<Hackable>());
    }
    private void update_hackable(Hackable hackable)
    {
        // check if we can't connect to the hackable
        if (!hacker.Connect(hackable))
        {
            hover_hackray.SetColor(out_of_range_hackray_color);
            hacker.Deselect();
            return;
        }

        // check if we found any vulnerabilities
        Exploit exploit = hacker.GetExploitVulnerabilities(hackable);
        if (exploit == null)
        {
            hover_hackray.SetColor(no_vulnerability_hackray_color);
            hacker.Deselect();
            return;
        }

        // check if we have the required cores
        if (!laptop.HasFreeCores(exploit.cores_cost))
        {
            hover_hackray.SetColor(no_cores_hackray_color);
            hacker.Deselect();
            return;
        }

        // checks if this is nmap -> if yes we can hack it to check for vulnerabilities
        if (exploit == Exploit.Nmap)
        {
            hover_hackray.SetColor(hackray_color);
            hacker.Select(hackable);
            return;
        }

        // otherwise we can hack it !!!!
        hover_hackray.SetColor(hackable_color);
        hacker.Select(hackable);
    }

    // TRIGGER ENTER
    /* private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if other is a capable
        Capable target = other.transform.parent.GetComponent<Capable>();
        if (target == null) { return; }

        // we get the hackable of the target capacity
        if (target is not Hackable hack_target) { return; }

        // we check if the hackable is already targeted
        if (target.gameObject == current_hackable) { return; }

        // or if it's already in the waiting targets
        if (waiting_hackables.Contains(target.gameObject)) { return; }

        // we add the hackable to the waiting targets
        waiting_hackables.Add(target.gameObject);

        if (log) { Debug.Log("(HackableNavigator) " + target.name + " added to waiting targets"); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if other is a capable
        Capable target = other.transform.parent.GetComponent<Capable>();
        if (target == null) { return; }

        // we get the hackable of the target capacity
        if (target is not Hackable hack_target) { return; }

        // we check if the hackable is the current target
        if (target.gameObject == current_hackable)
        {
            unselect_target();
            return;
        }

        // we check if the hack_target is in the waiting targets
        if (waiting_hackables.Contains(target.gameObject))
        {
            waiting_hackables.Remove(target.gameObject);
            if (log) { Debug.Log("(HackableNavigator) " + hack_target.name + " removed from waiting targets"); }
        }
    } */


    // TARGET SELECTION
    private void select_target(Hackable hackable)
    {
        if (hacker == null) { return; }

        // we switch the current target
        if (current_hackable != null) { unselect_target(); }
        current_hackable = hackable.gameObject;
        if (log) { Debug.Log("(HackableNavigator) " + hackable.name + " selected as closest target"); }

        hacker.Select(hackable);

        // we set the hovered target material
        hackable.spriteRenderer.material = hackable.TargetMaterial;

        // we update the hackray
        hover_hackray.SetColor(hacker.Connect(hackable) ? hackray_color : out_of_range_hackray_color);
        hover_hackray.SetLaptopAndTarget(laptop, hackable.transform);
        hover_hackray.gameObject.SetActive(true);
    }
    private void unselect_target()
    {
        // we check if we have a current target
        if (current_hackable == null || hacker == null) { return; }

        hacker?.Deselect();

        // we update the hackray
        hover_hackray?.SetColor(hackray_color);
        hover_hackray?.SetLaptopAndTarget(laptop, cursor);

        // we reset the hackable material
        Hackable hackable = current_hackable.GetComponent<Hackable>();
        if (hackable == null) { return; }
        hackable.spriteRenderer.material = hackable.DefaultMaterial;

        // we reset the current target
        if (log) { Debug.Log("(HackableNavigator) " + current_hackable.name + " unselected as closest target"); }
        current_hackable = null;
    }

    // ENABLE / DISABLE
    public async void Enable()
    {
        // we activate the callbacks
        navigationAction.performed += navigationCallback;

        // we update the hackray
        if (hover_hackray == null)
        {
            hover_hackray = Instantiate(hackray_prefab, transform).GetComponent<Hackray>();
            hover_hackray.name = "hover_hackray";
        }
        hover_hackray.gameObject.SetActive(false);
        hover_hackray.SetColor(hackray_color);
        hover_hackray.SetLaptopAndTarget(laptop, cursor);

        // we activate the cursor
        cursor.gameObject.SetActive(false);
        HandleHackNavigationInput(InputManager.Instance.inputs.perso.select_hackable.ReadValue<Vector2>());
        await System.Threading.Tasks.Task.Yield(); // wait for cursor to be inactive & instantiated


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
            laptop = null;
            return;
        }

        // we assign the hacker as the new laptop hack capacity
        laptop = new_laptop;
        hacker = new_laptop.GetCapacity<HackCapacity>();
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
        Vector2 clean_inputs = calculate_clean_inputs(input);
        if (log_navigation_inputs) { Debug.Log($"(HackableNavigator) navigation inputs : raw {input} , clean {clean_inputs}"); }

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

    // simple closest angle selection
    private GameObject get_closest_angled_hackable(Vector2 input)
    {
        // on vérifie qu'on a des hackables
        List<GameObject> targets = new List<GameObject>();
        if (current_hackable != null) { targets.Add(current_hackable); }
        targets.AddRange(waiting_hackables);
        if (targets.Count == 0) { return null; }

        // on parcourt tous les hackable et on trouve celui avec le plus petit angle
        float min_angle = 1000f;
        GameObject hackable = null;
        foreach (GameObject target in targets)
        {
            // on vérifie que le hackable est valide
            if (target == null || target.GetComponent<Hackable>() == null) { continue; }

            // on calcule l'angle entre le perso et l'objet
            Vector2 direction = (target.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(input, direction);
            if (angle < min_angle)
            {
                // on sélectionne le hackable
                hackable = target;
                min_angle = angle;
            }
        }
        return hackable;
    }
    private void update_cursor(Vector2 input)
    {
        // on met à jour le curseur
        if (cursor == null || hacker == null) { return; }
        cursor.localPosition = new Vector3(input.x * hacker.Radius, input.y * hacker.Radius, 0f);
    }
    private Vector2 calculate_clean_inputs(Vector2 raw)
    {
        // store signs
        float x_sign = Mathf.Sign(raw.x);
        float y_sign = Mathf.Sign(raw.y);

        // clamp base value between MIN_JOY & MAX_JOY
        float clamped_x = Mathf.Clamp(Mathf.Abs(raw.x), InputManager.Instance.JOYSTICK_MIN_THRESHOLD, InputManager.Instance.JOYSTICK_MAX_THRESHOLD);
        float clamped_y = Mathf.Clamp(Mathf.Abs(raw.y), InputManager.Instance.JOYSTICK_MIN_THRESHOLD, InputManager.Instance.JOYSTICK_MAX_THRESHOLD);

        // substract MIN_JOY to recenter the vector to zero
        clamped_x -= InputManager.Instance.JOYSTICK_MIN_THRESHOLD;
        clamped_y -= InputManager.Instance.JOYSTICK_MIN_THRESHOLD;

        // multiply by 1/(MAX_JOY - MIN_JOY) to normalize the vector
        clamped_x *= 1f / (InputManager.Instance.JOYSTICK_MAX_THRESHOLD - InputManager.Instance.JOYSTICK_MIN_THRESHOLD);
        clamped_y *= 1f / (InputManager.Instance.JOYSTICK_MAX_THRESHOLD - InputManager.Instance.JOYSTICK_MIN_THRESHOLD);

        // return the re-signed version
        return new Vector2(x_sign * clamped_x, y_sign * clamped_y);
    }

    // raycast selection with closest hitted
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
            if (hackable == null || hacker.IsHacking(hackable)) { being_hacked_hackable = hackable.gameObject; continue; }

            // we compare the distance
                float distance = Vector2.Distance(transform.position, processor.transform.position);
            if (distance < min_distance)
            {
                min_distance = distance;
                closest_processor = processor;
            }
        }
        if (closest_processor == null){ return being_hacked_hackable; } // returns a current processor being hacked if hitted, null otherwise
        return closest_processor.transform.parent.gameObject; // we return the parent hackable
    }

}