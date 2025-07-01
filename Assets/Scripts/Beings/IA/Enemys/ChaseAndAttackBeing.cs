using UnityEngine;
using System.Collections.Generic;

public class ChaseAndAttackBeing : Goal
{

    [Header("Actions Prefabs")]
    public GameObject goto_prefab; // action to go to somewhere
    public GameObject attack_prefab; // action to attack the target


    [Header("Target detection")]
    public bool target_detected = false;
    protected Being target;
    public LayerMask target_layers;
    public List<string> excluded_tags; // tags to exclude from target detection

    private void Start()
    {
        // this goal is not doable at start, we need to detect a target first
        doable = false;
    }

    // PLANNING
    public override bool Plan()
    {
        // we need a target to plan
        if (!target_detected) { return false; }

        // checks if we have the required action prefab we need
        if (goto_prefab == null || attack_prefab == null)
        {
            Debug.LogError("(ChaseAndAttackBeing - Plan) " + name + " must have goto_prefab and attack_prefab set!");
            return false;
        }

        // we add the goto action to the plan
        GoToAction goto_action = Instantiate(goto_prefab, transform).GetComponent<GoToAction>();
        // we set the target of the goto action to the target
        goto_action.destination = target.transform.position;

        // we add the attack action to the plan
        Action attack_action = Instantiate(attack_prefab, transform).GetComponent<Action>();

        // we create the plan
        current_plan = new List<Action>() { goto_action, attack_action };

        return base.Plan();
    }

    // UPDATE
    public override void UpdateGoal()
    {
        base.UpdateGoal();

        // we update the GoToAction destination if we still have a target in sight !
        if (!target_detected) { return; }
        if (current_action is GoToAction goto_action)
        {
            goto_action.destination = target.transform.position;
        }

        // if we don't have any action in the plan it means we did everything (going & attacking once)
        // so we make a new plan
        else if (!current_action) { Plan(); }
    }

    // DETECTING TARGET
    private void OnTriggerEnter2D(Collider2D other)
    {
        // check if we already have a target
        if (target_detected) { return; }

        // checks if its on the right layer
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }

        // checks if the target is a Being
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }

        // checks if it is not in the excluded tags
        if (excluded_tags.Count > 0 && excluded_tags.Contains(being.transform.tag)) { return; }

        // we found a target
        if (debug) { Debug.Log("(ChaseAndAttackBeing) " + other.transform.name + " entered chasing & attacking " + transform.parent.parent.name + " perception !!"); }

        target_detected = true;
        target = being;
        doable = true; // we can chase and attack now
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!target_detected) { return; } // we don't have a target

        // checks if its on the right layer
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }

        // checks if it s the right target
        if (other.transform.parent.GetComponent<Being>() != target) { return; }

        if (debug) { Debug.Log("(ChaseAndAttackBeing) " + other.transform.name + " got out of chasing & attacking " + transform.parent.parent.name + " perception."); }

        // we lost the target
        target_detected = false;
        target = null;
        doable = false; // we can't chase and attack anymore
    }
}