using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class WaitAction : IA_Action<WaitAction.Data>
    {
        // START
        Dictionary<IMonoAgent, float> wait_times = new Dictionary<IMonoAgent, float>();
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);

            // we set the wait time for this agent to 1 second
            wait_times[agent] = Time.time;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (!wait_times.ContainsKey(agent))
            {
                if (Logger.LazyInstance.LOG_IA_ACTION) { Debug.LogWarning($"(WaitAction) No wait time found for agent {agent}, cannot perform WaitAction"); }
                return ActionRunState.Stop;
            }

            if (Time.time - wait_times[agent] > 10) { return ActionRunState.Completed; }
            else { return ActionRunState.Continue; }
        }

        public class Data : IA_ActionData { }
    }
}