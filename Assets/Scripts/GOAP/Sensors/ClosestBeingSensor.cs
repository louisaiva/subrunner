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
            // Get a cached reference to the IA on the agent & attack capacity
            IA ia = references.GetCachedComponentInParent<IA>();
            AttackCapacity attackCapacity = ia.GetCapacity<AttackCapacity>();
            if (attackCapacity == null)
            {
                Debug.LogWarning($"(ClosestBeingSensor) {ia.name} has no AttackCapacity, cannot sense closest being.");
                return null;
            }

            // gets the closest being
            Being closestBeing = attackCapacity.GetClosestTarget(ia);
            if (closestBeing == null) { return null; }
            
            // If the target is a transform target, set the target to the closest being
            if (target is TransformTarget transformTarget)
            {
                return transformTarget.SetTransform(closestBeing.transform);
            }
            return new TransformTarget(closestBeing.transform);
        }
    }
}