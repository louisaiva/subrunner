using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class InteractAction<T> : IA_Action<InteractAction<T>.Data> where T : Interactable
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

            // todo : maybe here we can wait a little ???

            return ActionRunState.Completed;
        }

        // DATA
        public class Data : IA_ActionData
        {
            public InteractCapacity interactor { get; set; }
        }
    }
}