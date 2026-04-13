using CrashKonijn.Agent.Core;
// using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
// using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class EatAction : IA_Action<EatAction.Data>
    {

        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);

            // set the eatCapa
            data.eatCapacity = data.ia.GetCapacity<EatCapacity>();
        }

        // PERFORM
        private bool target_food(Data data)
        {
            // checks that the food is still valid
            if (data.eatCapacity == null) { return false; }
            if (data.Target is not CapableTarget capable_target) { return false; }
            if (capable_target.Capable == null || !capable_target.Capable.Loaded) { return false; }
            if (capable_target.Capable is not Food food) { return false; }

            // we set the food target into the capacity
            data.eatCapacity.SetFoodTarget(food);
            if (Logger.Instance.LOG_EAT_ACTION)
            {
                Debug.Log($"(EatAction) {data.ia.ID} is going to eat {food.ID}"
                + $" with capacity {data.eatCapacity.GetType().Name}");
            }
            return true;
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (data.eatCapacity.IsEating) { return ActionRunState.Continue; }

            // eating one bite is over, we find another food to bite into (maybe the same if it still has bites in it)
            bool food_targeted = target_food(data);
            if (!food_targeted) { return ActionRunState.Completed; }

            // we have found a food, we eat it
            data.eatCapacity.Use(data.ia);
            return ActionRunState.Continue;
        }

        // STOPPED
        public override void Stop(IMonoAgent agent, Data data)
        {
            base.Stop(agent, data);

            data.eatCapacity.Cancel();
            if (Logger.Instance.LOG_EAT_ACTION) { Debug.Log($"(EatAction) {data.ia.ID} stopped eating"); }
        }

        // DATA
        public class Data : IA_ActionData
        {
            public EatCapacity eatCapacity { get; set; }
        }
    }
}