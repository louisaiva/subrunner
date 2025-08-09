using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// this component must be a direct child of the perso (and ONLY the perso)
/// this is used by the UI_Hacking pool to navigate through the hackables in order to select the target to hack
/// it has a larger collider radius than the laptop's one because we want to hover out-of-range hackables
/// and warn the player it is not hackable rn in the UI_Hacking
/// also handles hover hackray ?
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HackNavigator : MonoBehaviour
{
    [Header("Current hackable")]
    [SerializeField] private GameObject current_hackable;

    [Header("Waiting hackables")]
    [SerializeField] private List<GameObject> waiting_hackables = new List<GameObject>();

    [Header("Hackrays")]
    public GameObject hackray_prefab;
    [SerializeField] private Color hackray_color = Color.yellow;
    [SerializeField] private Color out_of_range_hackray_color = Color.yellow;
    private Hackray hover_hackray; // this is the hackray that is used to hover the target


    [Header("Components")]
    [SerializeField] private HackCapacity hacker;
    private Laptop laptop;

    [Header("Log")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_targets = false;

    // START
    private void Start()
    {
        UI_LaptopItemSlot.Instance.OnItemChanged += ctx => OnLaptopChanged(ctx.Count > 0 ? ctx[0] as Laptop : null);
    }

    // UPDATE
    private void Update()
    {
        // we remove null waiting_hackables
        if (current_hackable != null && current_hackable.GetComponent<Hackable>() == null) { unselect_target(); }
        waiting_hackables.RemoveAll(h => h.GetComponent<Hackable>() == null);
        if (log_targets)
        {
            string targets_info = "";
            int target_count = 0;
            if (current_hackable != null)
            {
                targets_info += $"- closest target: {current_hackable.name} (is null ? {current_hackable == null})\n";
                target_count++;
            }
            else
            {
                targets_info += "- closest target: null\n";
            }

            foreach (var target in waiting_hackables)
            {
                targets_info += $"- waiting target: {target} (is null ? {target == null})\n";
                target_count++;
            }
            Debug.Log($"(HackNavigator) Current targets: {target_count}\n" + targets_info);
        }

        // on essaie de se connecter au current hackable si on en a un
        if (current_hackable == null || hacker == null || hover_hackray == null) { return; }
        bool connected = hacker.Connect(current_hackable.GetComponent<Hackable>());

        // on met à jour le hackray
        hover_hackray.SetColor(connected ? hackray_color : out_of_range_hackray_color);
        if (connected) { hacker.Select(current_hackable.GetComponent<Hackable>()); }
        else { hacker.Deselect(); }
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
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

        if (log) { Debug.Log("(HackNavigator) " + target.name + " added to waiting targets"); }
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
            if (log) { Debug.Log("(HackNavigator) " + hack_target.name + " removed from waiting targets"); }
        }
    }




    // TARGET SELECTION
    private void select_target(Hackable hackable)
    {
        if (hacker == null) { return; }

        // we switch the current target
        if (current_hackable != null) { unselect_target(); }
        current_hackable = hackable.gameObject;
        if (log) { Debug.Log("(HackNavigator) " + hackable.name + " selected as closest target"); }

        hacker.Select(hackable);

        // we update the hackray
        if (hover_hackray == null)
        {
            hover_hackray = Instantiate(hackray_prefab, transform).GetComponent<Hackray>();
            hover_hackray.name = "hover_hackray";
        }
        hover_hackray.gameObject.SetActive(true);
        hover_hackray.SetColor(hacker.Connect(hackable) ? hackray_color : out_of_range_hackray_color);
        hover_hackray.SetLaptopAndHackable(laptop, hackable);

        // we set the hovered target material
        hackable.spriteRenderer.material = hackable.TargetMaterial;
    }
    private void unselect_target()
    {
        // we check if we have a current target
        if (current_hackable == null || hacker == null) { return; }

        hacker?.Deselect();

        // we reset the hackable material
        Hackable hackable = current_hackable.GetComponent<Hackable>();
        if (hackable == null) { return; }
        hackable.spriteRenderer.material = hackable.DefaultMaterial;

        // we reset the current target
        if (log) { Debug.Log("(HackNavigator) " + current_hackable.name + " unselected as closest target"); }
        current_hackable = null;


        // we update the hackray
        if (hover_hackray != null) { hover_hackray.gameObject.SetActive(false); }
    }

    // TARGET SELECTION HANDLE INPUT
    public void HandleHackNavigationInput(Vector2 input)
    {

        // on regarde si on a un gros input (sinon on return)
        if (input.magnitude < 0.95f) { return; }

        // on vérifie qu'on a des hackables
        List<GameObject> targets = new List<GameObject>();
        if (current_hackable != null) { targets.Add(current_hackable); }
        targets.AddRange(waiting_hackables);
        if (targets.Count == 0) { return; }

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


    // ENABLE / DISABLE
    public void Enable()
    {
        if (current_hackable == null || hacker == null) { return; }
        select_target(current_hackable.GetComponent<Hackable>());
    }
    public void Disable() { unselect_target(); }


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
}