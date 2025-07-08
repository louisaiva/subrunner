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
        private List<Being> potential_targets;
        private IA ia;

        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {

            // Get a cached reference to the IA on the agent & attack capacity
            this.ia = references.GetCachedComponent<Brain>().ia;
            AttackCapacity attackCapacity = ia.GetCapacity<AttackCapacity>();
            if (attackCapacity == null)
            {
                Debug.LogWarning($"(ClosestBeingSensor) {ia.name} has no AttackCapacity, cannot sense closest being.");
                return null;
            }


            // we do an overlap to detect targets
            Collider2D[] results = Physics2D.OverlapCircleAll(ia.transform.position,
                attackCapacity.range_target_detection,
                attackCapacity.target_layers);
            if (results.Length == 0) { return null; }


            // we convert those into beings and filter their tags
            if (potential_targets == null)
            {
                potential_targets = new List<Being>();
            }
            else
            {
                // Clear the list to avoid duplicates
                potential_targets.Clear();
            }
            foreach (Collider2D collider in results)
            {
                // we check if the collider has a Being component
                Being being = collider.transform.parent.GetComponent<Being>();
                if (being == null) { continue; }
                else if (being == ia) { continue; } // we don't want to target ourselves

                // we check if the being is excluded by the tags
                if (attackCapacity.excluded_tags.Contains(being.gameObject.tag)) { continue; }

                // we add the being to the list of potential targets
                potential_targets.Add(being);
            }
            if (potential_targets.Count == 0) { return null; }


            // Debug.Log($"(ClosestBeingSensor) {ia.name} found {potential_targets.Count} potential targets to attack");
            Being closestTarget = this.Closest(this.potential_targets, agent.Transform.position);


            // If the target is a transform target, set the target to the closest being
            if (target is TransformTarget transformTarget)
            {
                return transformTarget.SetTransform(closestTarget.transform);
            }
            return new TransformTarget(closestTarget.transform);
        }

        // Returns the closest item in a list
        private T Closest<T>(IEnumerable<T> list, Vector3 position)
            where T : MonoBehaviour
        {
            T closest = null;
            var closestDistance = float.MaxValue; // Start with the largest possible distance

            foreach (var item in list)
            {
                var distance = Vector3.Distance(item.gameObject.transform.position, position);

                if (!(distance < closestDistance))
                    continue;

                closest = item;
                closestDistance = distance;
            }

            return closest;
        }

    }
}