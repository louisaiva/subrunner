using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class InteractAction<T, GoalT> : IA_Action<InteractAction<T, GoalT>.Data> where T : Interactable where GoalT : IGoal
    {

        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);
            data.interactor = data.ia.GetCapacity<InteractCapacity>();
        }

        // PERFORM
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // checks that the item is still valid
            if (data.interactor == null) { return ActionRunState.Completed; }
            if (data.Target is not CapableTarget capable_target) { return ActionRunState.Completed; }
            if (capable_target.Capable == null || !capable_target.Capable.Loaded) { return ActionRunState.Completed; }
            if (capable_target.Capable is not T interactive) { return ActionRunState.Completed; }

            // we interact with the item
            if (Logger.LazyInstance.LOG_INTERACT_ACTION)
            {
                Debug.Log($"(InteractAction) '{data.ia.ID}' is going to interact with '{interactive.ID}', of type {typeof(T).Name}"
                + $" with capacity '{data.interactor.ID}'");
            }
            data.interactor.InteractWithInteractable(interactive);

            return ActionRunState.WaitThenComplete(.5f);
        }

        // OVERRIDES
        public override void End(IMonoAgent agent, Data data)
        {
            // here we can reset the room destination in the motor data !
            if (data.ia.TryGetCapacity(out MotorCapacity mc))
            {
                mc.mdata.ClearDestination(typeof(GoalT));
            }
        }

        // DATA
        public class Data : IA_ActionData
        {
            public InteractCapacity interactor { get; set; }
        }
    }
}