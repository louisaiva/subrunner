using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using UnityEngine;

namespace subrunner.goap
{
    public class HungerSensor : LocalWorldSensorBase
    {
        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            // Get a cached reference to the IA on the agent
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia == null)
            {
                if (Logger.LazyInstance.LOG_HUNGER_SENSOR) { Debug.LogWarning($"(HungerSensor) IA component not found on agent {agent}"); }
                return new SenseValue(0);
            }

            EatCapacity eat_capacity = ia.GetCapacity<EatCapacity>();
            if (eat_capacity == null)
            {
                if (Logger.LazyInstance.LOG_HUNGER_SENSOR) { Debug.LogWarning($"(HungerSensor) EatCapacity not found on ia {ia.GetStaticID()}"); } // get static id bcz it may be destroyed if we did not find the capacity
                return new SenseValue(0);
            }

            // We need to cast the float to an int, because the hunger is an int
            return (int)eat_capacity.hunger;
        }
    }
}