using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace subrunner.goap
{
    public class ClosestFoodSensor : LocalTargetSensorBase
    {

        public override void Created() { }
        public override void Update() { }
        
        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {

            // Get a cached reference to the IA on the agent & EatCapacity
            IA ia = references.GetCachedComponentInParent<IA>();
            // Debug.Log($"(ClosestFoodSensor) {ia.name} is sensing closest food...");
            EatCapacity eatCapacity = ia.GetCapacity<EatCapacity>();
            if (eatCapacity == null) { return null; }
            
            // find the closest food
            Capable closestFoodTarget = eatCapacity.GetClosestFoodTarget(ia);
            if (closestFoodTarget == null) { return null; }

            // If the target is a transform target, set the target to the closest food
            if (target is TransformTarget transformTarget)
            {
                return transformTarget.SetTransform(closestFoodTarget.transform);
            }
            return new TransformTarget(closestFoodTarget.transform);
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