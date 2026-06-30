using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class ReturnToNestAction : IA_Action<IA_ActionData>
    {
        public override IActionRunState Perform(IMonoAgent agent, IA_ActionData data, IActionContext context)
        {
            if (data.Target is not CapableTarget capable_target) { return ActionRunState.Completed; }
            if (capable_target.Capable == null || !capable_target.Capable.Loaded) { return ActionRunState.Completed; }
            if (capable_target.Capable is not Nester nest) { return ActionRunState.Completed; }
            if (!nest.CanHost(data.ia)) { return ActionRunState.Completed; }
            nest.Host(data.ia);

            return ActionRunState.Completed;
        }

        public override void End(IMonoAgent agent, IA_ActionData data)
        {
            // here we can reset the room destination in the motor data !
            if (data.ia.TryGetCapacity(out MotorCapacity mc))
            {
                mc.mdata.ClearDestination(typeof(ReturnToNestGoal));
            }
        }
    }
}