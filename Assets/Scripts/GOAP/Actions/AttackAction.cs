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
            data.attack_capacity = data.ia.GetCapacity<AttackCapacity>();
            data.Being = data.ia as Being;
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            if (data.Target is not TransformTarget transformTarget) { return; }
            Being being_target = transformTarget.Transform.GetComponent<Being>();

            // verify that the being is still Alive
            if (being_target == null || !being_target.Alive) { return; }

            // we turn over to face the target
            data.ia.OrientTowards(being_target.transform.position);

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
        
        // IS IN RANGE OVERRIDE
        public override bool IsInRange(IMonoAgent agent, float distance, IActionData data, IComponentReference references)
        {
            var actionData = (Data)data;

            // Fallback to default behavior if no AttackCapacity
            if (actionData.attack_capacity == null) { return base.IsInRange(agent, distance, data, references); }

            // Use cached attack_capacity for performance
            float agentStoppingDistance = actionData.attack_capacity.distance_to_attack;
            if (actionData.ia.log_actions) { Debug.Log($"(AttackAction) {actionData.ia.name} IsInRange check: distance={distance:F2}, stopping_distance={agentStoppingDistance:F2}, in_range={distance <= agentStoppingDistance}"); }

            return distance <= agentStoppingDistance;
        }


        // DATA
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public Being Being { get; set; }

            // Direct access to IA and AnimPlayer
            [GetComponentInParent] public IA ia { get; set; }
            public AnimPlayer anim_player { get; set; }
            public AttackCapacity attack_capacity { get; set; }
        }
    }
}