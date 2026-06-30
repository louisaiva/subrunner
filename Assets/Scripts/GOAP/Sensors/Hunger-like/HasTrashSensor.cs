using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using UnityEngine;
using System.Collections.Generic;

namespace subrunner.goap
{
    public class HasTrashSensor : LocalWorldSensorBase
    {
        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            // Get a cached reference to the IA on the agent
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia == null) { return new SenseValue(0); }
            if (ia.Inventory == null) { return new SenseValue(0); }
            List<Item> trashes = ia.Inventory.GetItemsByRule("corpse,leftover");
            return trashes.Count;
        }
    }
}