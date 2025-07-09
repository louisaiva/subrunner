using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
// using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class EatAction : GoapActionBase<EatAction.Data>
    {
        private IA ia;
        private AnimPlayer anim_player;
        private Food food_target;


        /* /// <summary>
        /// called when the action is created. inject the EatCapacity into the action data
        /// </summary>
        public override void Created()
        {
            EatAction.Data data = this.CreateData();
            data.EatCapacity = new EatCapacity();
        } */

        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            this.ia = data.Brain.ia;
            this.anim_player = ia.anim_player;

            if (data.Target is not TransformTarget transformTarget) { return; }
            this.food_target = transformTarget.Transform.GetComponent<Food>();

            // verify that the food is still Eatable (maybe since the sensor sensed it it was eaten)
            if (food_target == null || !food_target.Eatable) { return; }

            // set the food target
            if (!ia.HasCapacity<EatCapacity>()) { return; }
            ia.GetCapacity<EatCapacity>().SetFoodTarget(food_target);


            // if (ia is Cat cat) { cat.food_ready_to_be_eaten = true; }
            Debug.Log($"(EatAction) {agent.name} is going to eat {food_target.name}"
                + $" with capacity {ia.GetCapacity<EatCapacity>().GetType().Name}"
                + $" and animation {anim_player}");

            // use the EatCapacity
            ia.Do("eat");
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (anim_player.current_capacity == "eat") { return ActionRunState.Continue; }

            // we try to take another bite of the food
            if (food_target != null && food_target.Eatable)
            {
                // we use the EatCapacity again
                ia.Do("eat");
                return ActionRunState.Continue;
            }

            // if we are here, the food is not eatable anymore
            return ActionRunState.Completed;
        }

        // This method is called when the action is completed
        /* public override void Complete(IMonoAgent agent, Data data)
        {
            if (ia == null || food_target == null) { return; }

        } */

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            // When using the GetComponent attribute, the system will automatically inject the reference
            [GetComponent] public Brain Brain { get; set; }
            // public EatCapacity EatCapacity { get; set; }
        }
    }
}