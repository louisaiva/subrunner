using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;

namespace subrunner.goap
{
    public class HungerSensor : LocalWorldSensorBase
    {
        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            // Get a cached reference to the IA on the agent
            var data = references.GetCachedComponent<Brain>();

            // We need to cast the float to an int, because the hunger is an int
            return (int)data.ia.hunger;
        }
    }
}