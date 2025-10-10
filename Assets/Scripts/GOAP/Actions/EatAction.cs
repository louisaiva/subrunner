using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
// using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class EatAction : GoapActionBase<EatAction.Data>
    {

        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            // we set the data
            data.anim_player = data.ia.anim_player;
            if (data.Target is not TransformTarget transformTarget) { return; }
            data.food_target = transformTarget.Transform.GetComponent<Food>();

            // set the eatCapa
            data.eatCapacity = data.ia.GetCapacity<EatCapacity>();
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            // verify that the food is still Eatable (maybe since the sensor sensed it it was eaten)
            if (data.food_target == null || !data.food_target.Eatable) { return; }

            // set the food target
            if (!data.eatCapacity) { return; }
            data.eatCapacity.SetFoodTarget(data.food_target);

            if (data.ia.log_actions)
            {
                Debug.Log($"(EatAction) {data.ia.name} is going to eat {data.food_target.name}"
                + $" with capacity {data.eatCapacity.GetType().Name}"
                + $" and animation {data.anim_player}");
            }

            // use the EatCapacity
            data.eatCapacity.Use(data.ia);
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (data.anim_player.current_capacity == "eat") { return ActionRunState.Continue; }

            // we try to take another bite of the food
            if (data.food_target != null && data.food_target.Eatable)
            {
                data.ia.Do("eat"); // we have to use the Do() method instead of Use() directly on
                                   // the eatCapacity because using the capacity directly does not check if the cooldown is over or not
                                   // which causes the Use() to be called every frame and so play the eat animation until the end of times
                return ActionRunState.Continue;
            }

            // if we are here, the food is not eatable anymore
            return ActionRunState.Completed;
        }

        // STOPPED
        public override void Stop(IMonoAgent agent, Data data)
        {
            data.eatCapacity.Cancel(data.ia);
            if (data.ia.log_actions) { Debug.Log($"(EatAction) {data.ia.name} stopped eating"); }
        }

        // DATA
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            // When using the GetComponent attribute, the system will automatically inject the reference
            [GetComponentInParent] public IA ia { get; set; }
            public Food food_target { get; set; }
            public AnimPlayer anim_player { get; set; }
            public EatCapacity eatCapacity { get; set; }
        }
    }
}