using System.Collections.Generic;
using UnityEngine;

public class PreyDetector : Detector
{

    [Header("Targets detection")]
    [SerializeField] private List<Being> waiting_targets = new List<Being>(); // list of potential targets, does not contains current_target !
    public LayerMask target_layers;
    public List<string> excluded_tags; // tags to exclude from current_target detection

    [Header("Logs")]
    [SerializeField] private bool log = false; // whether to log the detector's actions

    private IA ia;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();
        ia = transform.parent.GetComponent<IA>();
    }

    // GETTING CLOSEST TARGET
    public Being GetClosestTarget(IA ia)
    {
        // get the potential targets
        if (waiting_targets.Count == 0) { return null; }

        // we remove null targets
        waiting_targets.RemoveAll(target => target == null);

        // we find the closest target
        Being closest_target = null;
        float closest_distance = float.MaxValue; // Start with the largest possible distance

        foreach (Being target in waiting_targets)
        {
            float distance = Vector3.Distance(target.gameObject.transform.position, ia.transform.position);

            if (distance >= closest_distance) { continue; }

            closest_target = target;
            closest_distance = distance;
        }
        return closest_target;
    }


    // DETECTING TARGET
    private void OnTriggerEnter2D(Collider2D other)
    {
        // checks if the collider is a potential target
        if (!((target_layers.value & (1 << other.transform.gameObject.layer)) > 0)) { return; }
        Being being = other.transform.parent.GetComponent<Being>();
        if (being == null) { return; }
        if (excluded_tags.Count > 0 && excluded_tags.Contains(being.transform.tag)) { return; }
        if (waiting_targets.Contains(being)) { return; }

        // remove null targets
        waiting_targets.RemoveAll(target => target == null);

        // if we have no prey, we enable the goal
        if (waiting_targets.Count == 0) { brain.EnableGoal(goal); }

        // we add the being to the waiting targets
        waiting_targets.Add(being);

        if (log) { Debug.Log("(PreyDetector) " + being.name + " added to waiting targets of "+ ia.name); }
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

        // if we have no prey anymore, we disable the goal
        if (waiting_targets.Count == 0) { brain.DisableGoal(goal,false); } // we don't resolve so the ia will still finish its action
    }
}