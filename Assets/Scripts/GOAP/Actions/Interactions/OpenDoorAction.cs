using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class OpenDoorAction : IA_Action<OpenDoorAction.Data>
    {
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
            if (capable_target.Capable is not Door door) { return ActionRunState.Completed; }
            if (((Openable)door).IsOpenOrOpening()) { return ActionRunState.Completed; } // if the door is already open/opening, no need to open it again !

            // else the door is either closing either steady close, so we continue to perform !
            // we interact with the door
            if (Logger.LazyInstance.LOG_INTERACT_ACTION)
            {
                Debug.Log($"(OpenDoorAction) '{data.ia.ID}' is going to open door '{door.ID}'"
                + $" with capacity '{data.interactor.ID}'");
            }
            data.interactor.InteractWithInteractable(door);

            return ActionRunState.WaitThenComplete(0.5f);
        }

        public class Data : IA_ActionData
        {
            public InteractCapacity interactor { get; set; }
        }



        // OVERRIDES
        public override bool IsInRange(IMonoAgent agent, float distance, Data data, IComponentReference references)
        {
            return distance <= .75f; // doors can be big sometimes we don't want to get that close to them berk
        }

    }
}