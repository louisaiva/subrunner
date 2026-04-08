using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class ClosestBeingSensor : LocalTargetSensorBase
    {

        private static HealthDetector _health_detector;
        private static HealthDetector health_detector
        {
            get
            {
                if (_health_detector == null)
                {
                    _health_detector = GameObject.FindFirstObjectByType<OverlapHealthDetector>();
                }
                return _health_detector;
            }
        }

        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {
            // Get a cached reference to the IA on the agent
            IA ia = references.GetCachedComponentInParent<IA>();
            IAData iaData = ia.data as IAData;
            if (iaData == null) { return null; }
            HealthCapacity closestHealth = health_detector.FindClosestHealthCapacity(iaData);
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