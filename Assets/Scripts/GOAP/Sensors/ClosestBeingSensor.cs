using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace subrunner.goap
{
    public class ClosestBeingSensor : LocalTargetSensorBase
    {
        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {
            // Get a cached reference to the IA on the agent
            IA ia = references.GetCachedComponentInParent<IA>();
            PreyDetector preyDetector = ia.Eyes as PreyDetector;
            if (preyDetector == null) { return null; }
            HealthCapacity closestHealth = preyDetector.GetClosestTarget(ia);
            if (closestHealth == null) { return null; }

            // If the target is a transform target, set the target to the closest being
            if (target is TransformTarget transformTarget)
            {
                return transformTarget.SetTransform(closestHealth.Capable.transform);
            }
            return new TransformTarget(closestHealth.Capable.transform);
        }
    }
}


/* IMPLEMENTATION FOR IF WE NEED TO PUT IT BACK


// 1 - TARGET DETECTION // todo change this to work with colliders
public List<Being> DetectPotentialTargets(IA ia)
{
    // we do an overlap to detect targets
    Collider2D[] results = Physics2D.OverlapCircleAll(ia.transform.position,
        range_target_detection,
        target_layers);
    if (results.Length == 0) { return new List<Being>(); }

    // we convert those into beings and filter their tags
    List<Being> potential_targets = new List<Being>();
    foreach (Collider2D collider in results)
    {
        // we check if the collider has a Being component
        Being being = collider.transform.parent.GetComponent<Being>();
        if (being == null) { continue; }
        else if (being == ia) { continue; } // we don't want to target ourselves

        // we check if the being is excluded by the tags
        if (excluded_tags.Contains(being.gameObject.tag)) { continue; }

        // we add the being to the list of potential targets
        potential_targets.Add(being);
    }
    return potential_targets;
}
public Being GetClosestTarget(IA ia)
{
    // get the potential targets
    List<Being> potential_targets = DetectPotentialTargets(ia);
    if (potential_targets.Count == 0) { return null; }

    // we find the closest target
    Being closest_target = null;
    float closest_distance = float.MaxValue; // Start with the largest possible distance

    foreach (Being target in potential_targets)
    {
        float distance = Vector3.Distance(target.gameObject.transform.position, ia.transform.position);

        if (!(distance < closest_distance))
            continue;

        closest_target = target;
        closest_distance = distance;
    }
    return closest_target;
}
 */