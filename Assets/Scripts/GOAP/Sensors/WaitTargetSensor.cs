using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

namespace subrunner.goap
{
    public class WaitTargetSensor : LocalTargetSensorBase, LoadedSensor
    {
        public override void Created() { }
        public override void Update() { }

        // SENSE
        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            if (Logger.Instance.LOG_WANDER_TARGET_SENSOR) { Debug.Log($"(WaitSensor - Sense) {agent} senses a flemmardise that makes them just wait"); }
            // get the ia & exploration range
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia.JustLoaded)
            {
                // we check that we have a valid target, if yes we return it (it was loaded when the MotorCapacity loaded the MotorData' local world data)
                if (existingTarget != null && existingTarget is PositionTarget)
                {
                    if (Logger.Instance.LOG_WANDER_TARGET_SENSOR) { Debug.Log($"(WaitSensor - Sense) {ia.data.id} just loaded and has an existing target : {existingTarget}. We keep it."); }
                    return existingTarget;
                }
                else
                {
                    if (Logger.Instance.LOG_WANDER_TARGET_SENSOR) { Debug.Log($"(WaitSensor - Sense) {ia.data.id} just loaded but has no existing target : {existingTarget}"); }
                }
            }

            // else we return the actual position of the ia as a PositionTarget
            if (existingTarget is PositionTarget existingTargetPosition)
            {
                existingTargetPosition.SetPosition(ia.transform.position);
                return existingTargetPosition;
            }

            return new PositionTarget(ia.transform.position);
        }
    }
}