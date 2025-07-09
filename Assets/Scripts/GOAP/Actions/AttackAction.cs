using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
// using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class AttackAction : GoapActionBase<AttackAction.Data>
    {
        private IA ia;
        private AnimPlayer anim_player;
        private Being being_target;
        // private IMonoAgent current_agent;


        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            this.ia = data.Brain.ia;
            this.anim_player = ia.anim_player;
            // this.current_agent = agent;

            // sets the stopping distance before moving
            // this.Config.StoppingDistance = agen
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {

            if (data.Target is not TransformTarget transformTarget) { return; }
            this.being_target = transformTarget.Transform.GetComponent<Being>();

            // verify that the food is still Alive
            if (being_target == null || !being_target.Alive) { return; }

            // we look at the being target



            // if (ia is Cat cat) { cat.food_ready_to_be_eaten = true; }
            Debug.Log($"(AttackAction) {ia.name} is trying to attack {being_target.name}");

            // use the EatCapacity
            ia.Do("attack");
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (anim_player.current_capacity == "attack") { return ActionRunState.Continue; }
            return ActionRunState.Completed;
        }


        // DATA
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            // When using the GetComponent attribute, the system will automatically inject the reference
            [GetComponent] public Brain Brain { get; set; }
            [GetComponent] public AgentBehaviour AgentBehaviour { get; set; }
        }
    }
}