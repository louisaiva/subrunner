using UnityEngine;
using System.Collections.Generic;

public class ChaseAndAttackBeing : Goal
{
    public override bool Doable
    {
        get
        {
            // if we have a closest target than we can achieve this goal, otherwise noooo
            return current_target != null || waiting_targets.Count > 0;
        }
    }

    [Header("Actions Prefabs")]
    public GameObject goto_prefab; // action to go to somewhere
    public GameObject attack_prefab; // action to attack the current_target


    [Header("Targets detection")]
    [SerializeField] protected Being current_target;
    [SerializeField] private List<Being> waiting_targets = new List<Being>(); // list of potential targets, does not contains current_target !
    public LayerMask target_layers;
    public List<string> excluded_tags; // tags to exclude from current_target detection

    [Header("Attack & Chase")]
    [SerializeField] private bool always_chase_closest_target = false; // if true, we always chase the closest target, which means we call set_closest_target_to_current() each player loop, which can affect performance
    [SerializeField] private float distance_to_attack = 1f;

    // AWAKE
    private void Awake()
    {
        // checks if we have the required action prefab we need
        if (goto_prefab == null || attack_prefab == null)
        {
            Debug.LogError("(ChaseAndAttackBeing) " + name + " must have goto_prefab and attack_prefab set!");
        }
    }

    // PLANNING
    public override bool Plan()
    {
        // we need a current_target to plan
        if (!Doable) { return false; }

        // we update the closest target if we don't have any current_target
        if (!current_target) { set_closest_target_to_current(); }

        // we add the goto action to the plan
        GoToAction goto_action = Instantiate(goto_prefab, transform).GetComponent<GoToAction>();
        // we set the current_target of the goto action to the current_target
        goto_action.destination = current_target.transform.position;
        goto_action.threshold_distance = distance_to_attack; // we set the threshold distance to the distance to attack

        // we add the attack action to the plan
        Action attack_action = Instantiate(attack_prefab, transform).GetComponent<Action>();

        // we create the plan
        current_plan = new List<Action>() { goto_action, attack_action };

        return base.Plan();
    }
    private void set_closest_target_to_current()
    {
        // we can't set closest target if we don't have any target
        if (!current_target && waiting_targets.Count <= 0) { return; }

        // we remove all null waiting targets
        waiting_targets.RemoveAll(target => target == null);

        // we add the current_target to the waiting_targets if not null
        if (current_target) { waiting_targets.Add(current_target); }

        // we sort the waiting_targets by distance
        waiting_targets.Sort((a, b) => Vector2.Distance(a.transform.position, transform.position).CompareTo(Vector2.Distance(b.transform.position, transform.position)));

        // we set the current target as the first one
        current_target = waiting_targets[0];
        waiting_targets.RemoveAt(0);

        if (debug) { Debug.Log("(ChaseAndAttackBeing) Current target updated to " + current_target.name); }
    }


    // UPDATE
    public override void UpdateGoal()
    {
        base.UpdateGoal();

        // we check if we have a current_target
        if (!current_target || always_chase_closest_target) { set_closest_target_to_current(); }

        // we update the GoToAction destination if we still have a current_target in sight !
        if (current_action is GoToAction goto_action)
        {
            goto_action.destination = current_target.transform.position;
        }

        // if we don't have any action in the plan it means we did everything (going & attacking once)
        // so we make a new plan
        else if (!current_action) { Plan(); }
    }

    // DETECTING TARGET
    private void OnTriggerEnter2D(Collider2D other)
    {
        // checks if its on the right layer
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }

        // checks if the collider is a Being
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }

        // checks if it is not in the excluded tags
        if (excluded_tags.Count > 0 && excluded_tags.Contains(being.transform.tag)) { return; }

        // we check if the being is already targeted
        if (being == current_target) { return; }

        // or if it's already in the waiting targets
        if (waiting_targets.Contains(being)) { return; }

        // we add the being to the waiting targets
        waiting_targets.Add(being);

        if (debug) { Debug.Log("(ChaseAndAttackBeing) " + being.name + " added to waiting targets"); }

    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // checks if its on the right layer
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }

        // checks if the collider is a Being
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }

        // checks if it s the current_target
        if (being == current_target)
        {
            // we lost the current_target
            current_target = null;
            if (debug) { Debug.Log("(ChaseAndAttackBeing) " + being.name + " is no current target anymore ://"); }
            return;
        }

        // we check if the being is in the waiting targets
        if (waiting_targets.Contains(being))
        {
            waiting_targets.Remove(being);
            if (debug) { Debug.Log("(ChaseAndAttackBeing) " + being.name + " removed from waiting targets"); }
        }
    }
}