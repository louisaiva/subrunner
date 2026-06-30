using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class BurnTrashAction : IA_Action<BurnTrashAction.Data>
    {
        public class Data : IA_ActionData
        {
            public InteractCapacity interactor { get; set; }
        }
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);
            data.interactor = data.ia.GetCapacity<InteractCapacity>();
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // checks that the item is still valid
            if (data.interactor == null) { return ActionRunState.Completed; }
            if (data.Target is not CapableTarget capable_target) { return ActionRunState.Completed; }
            if (capable_target.Capable == null || !capable_target.Capable.Loaded) { return ActionRunState.Completed; }
            if (capable_target.Capable is not Burner burner) { return ActionRunState.Completed; }
            if (!burner.CanReceiveFrom(data.ia)) { return ActionRunState.Completed; }

            // we give an item to the burner
            data.interactor.InteractWithInteractable(burner, endless:true);

            return ActionRunState.Continue;
        }
    }
}