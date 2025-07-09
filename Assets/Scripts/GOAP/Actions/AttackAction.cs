using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;

using UnityEngine;

namespace subrunner.goap
{
    public class AttackAction : GoapActionBase<AttackAction.Data>
    {
        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            data.anim_player = data.ia.anim_player;
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            if (data.Target is not TransformTarget transformTarget) { return; }
            Being being_target = transformTarget.Transform.GetComponent<Being>();

            // verify that the being is still Alive
            if (being_target == null || !being_target.Alive) { return; }

            // Debug log only if enabled
            if (data.ia.log_actions) 
            {
                Debug.Log($"(AttackAction) {data.ia.name} is trying to attack {being_target.name}");
            }

            // use the attack capacity
            data.ia.Do("attack");
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (data.anim_player.current_capacity == "attack") { return ActionRunState.Continue; }
            return ActionRunState.Completed;
        }


        // DATA
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            // Direct access to IA and AnimPlayer
            [GetComponentInParent] public IA ia { get; set; }
            public AnimPlayer anim_player { get; set; }
        }
    }
}