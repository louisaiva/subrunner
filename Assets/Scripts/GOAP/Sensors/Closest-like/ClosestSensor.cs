using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class ClosestSensor<TCap,TGoal> : LocalTargetSensorBase where TCap : Capable where TGoal : IGoal
    {

        private static CapableDetector _detector;
        private static CapableDetector detector
        {
            get
            {
                if (_detector == null)
                {
                    _detector = GameObject.FindFirstObjectByType<WorldCapableDetector>(FindObjectsInactive.Include);
                }
                return _detector;
            }
        }

        public override void Created() { }
        public override void Update() { }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {
            System.Type goal = typeof(TGoal);

            // Get a cached reference to the IA on the agent & Capacity
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia.JustLoaded)
            {
                // we check that we have a valid target, if yes we return it (it was loaded when the MotorCapacity loaded the MotorData' local world data)
                if (target != null && target is CapableTarget)
                {
                    if (Logger.LazyInstance.LOG_CLOSEST_SENSOR) { Debug.Log($"(ClosestSensor - {goal.Name}) {ia.data.id} just loaded and has an existing target : {target}. We keep it."); }
                    return target;
                }
            }

            // ensure that the agent has no left actions in the plan
            if (!ia.TryGetCapacity(out MotorCapacity mc)) { return null; }
            if (mc.mdata.TryGetDestination(goal, out string destination))
            {
                if (Logger.LazyInstance.LOG_CLOSEST_SENSOR) { Debug.Log($"(ClosestSensor - {goal.Name}) {ia.data.id} already has a destination room set : {destination}. We don't sense a new trash target."); }
                return target;
            }

            CapableData closest_capable = detector.FindClosest<TCap>(ia.data);
            if (closest_capable == null) { return null; }
            mc.mdata.SetDestination(goal, closest_capable.room);

            // If the target is already set, we update it
            if (target is CapableTarget captarg) { return captarg.SetCapableData(closest_capable); }
            return new CapableTarget(closest_capable);
        }
    }
}