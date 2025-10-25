using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using subrunner.goap;
using UnityEngine;

public class PreyDetector : Detector
{
    [Header("Target selection")]
    [SerializeField] private bool always_select_closest = true; // whether to always select the closest target or not
    [SerializeField] protected float selectionInterval = 0.5f; // How often to check for conditions
    protected float lastSelectionTime = 0f;

    [Header("Targets detection")]
    [SerializeField] private List<Being> waiting_targets = new List<Being>(); // list of potential targets, does not contains current_target !
    public LayerMask target_layers;
    [SerializeField] private List<string> target_skins; // skins to include from current_target detection

    [Header("Logs")]
    [SerializeField] private bool log = false; // whether to log the detector's actions


    [Header("Components")]
    private CircleCollider2D eyes;
    public float Range => eyes.radius; // the range of the detector, used to determine if the target is in range

    // AWAKE
    protected override void Awake()
    {
        base.Awake();
        eyes = GetComponent<CircleCollider2D>();
    }

    // GETTING CLOSEST TARGET
    public Being GetClosestTarget(IA ia)
    {
        // get the potential targets
        if (waiting_targets.Count == 0) { return null; }

        // we remove null targets
        waiting_targets.RemoveAll(target => target == null || !target.Alive);

        // we find the closest target
        Being closest_target = null;
        float closest_distance = float.MaxValue; // Start with the largest possible distance

        foreach (Being target in waiting_targets)
        {
            float distance = Vector3.Distance(target.gameObject.transform.position, ia.transform.position);

            if (!target_skins.Contains(target.Skin)) { continue; }
            if (distance >= closest_distance) { continue; }

            closest_target = target;
            closest_distance = distance;
        }
        return closest_target;
    }

    // UPDATE
    private void Update()
    {
        if (!always_select_closest) { return; }

        // update timer
        if (Time.time - lastSelectionTime <= selectionInterval) { return; }
        lastSelectionTime = Time.time;

        // find the closest target
        Being closest_target = GetClosestTarget(ia);

        // checks if the goal is the active one
        if (!goal.enabled)
        {
            // if we have a closest target, we enable the goal
            if (closest_target != null) { brain.EnableGoal(goal); }
            return;
        }
        else if (brain.CurrentGoal != goal.type) { return; }

        // here we are on the Attack Goal : if we have no closest target, we stop the goal
        if (closest_target == null)
        {
            brain.DisableGoal(goal);
            if (log) { Debug.Log($"(PreyDetector) {ia.name} is disabling the goal because it has no valid target."); }
            return;
        }

        // we get the current action state target
        IActionData actionData = brain.currentActionData;
        if (actionData is not AttackAction.Data attackData) { return; } // we only care about attack actions
        if (attackData.BeingTarget == null) { return; }

        // if the target is different than the current closest one, we stop the action
        // (will request a new attack action with the right closest target)
        if (attackData.BeingTarget == closest_target) { return; }
        brain.agent.StopAction();
        if (log) { Debug.Log($"(PreyDetector) {ia.name} is stopping current action because the target {attackData.BeingTarget.name} is not the closest one."); }
    }

    // DETECTING TARGET
    private void OnTriggerEnter2D(Collider2D other)
    {
        // checks if the collider is a potential target
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }
        
        if (waiting_targets.Contains(being)) { return; }

        // remove null targets
        waiting_targets.RemoveAll(target => target == null);

        // we add the being to the waiting targets
        waiting_targets.Add(being);

        // if we have no prey, we enable the goal
        if (target_skins.Contains(being.Skin) && !goal.enabled) { brain.EnableGoal(goal); }

        if (log) { Debug.Log("(PreyDetector) " + being.name + " added to waiting targets of " + ia.name); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }

        // we check if the being is in the waiting targets of 
        if (waiting_targets.Contains(being))
        {
            waiting_targets.Remove(being);
            if (log) { Debug.Log("(PreyDetector) " + being.name + " removed from waiting targets of " + ia.name); }
        }

        // we remove null targets
        waiting_targets.RemoveAll(target => target == null);
        
        // if we have no more prey, we disable the goal
        if (waiting_targets.Count == 0 && goal.enabled) { brain.DisableGoal(goal); if (log) { Debug.Log($"(PreyDetector) {ia.name} is disabling the goal because it has no more waiting targets."); } }
    }
}