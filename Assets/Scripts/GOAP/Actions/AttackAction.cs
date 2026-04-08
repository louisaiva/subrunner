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
            data.attack_capacity = data.ia.GetCapacity<AttackCapacity>();
            data.CapableTarget = data.Target is TransformTarget target ? target.Transform.GetComponent<Capable>() : null;
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            if (data.CapableTarget == null) { return; }

            // verify that the being is still Alive
            if (!data.CapableTarget.TryGetCapacity(out HealthCapacity health) || !health.Alive) { return; }

            // we turn over to face the target
            data.ia.OrientTowards(data.CapableTarget.transform.position);

            // Debug log only if enabled
            if (data.ia.log_actions)
            {
                Debug.Log($"(AttackAction) {data.ia.name} is trying to attack {data.CapableTarget.name}");
            }

            // use the attack capacity
            AttackCapacity attack_capacity = data.attack_capacity;
            if (attack_capacity.Able) { attack_capacity.Use(data.ia); }
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (data.ia.AnimPlayer.current_capacity == "attack") { return ActionRunState.Continue; }
            return ActionRunState.Completed;
        }

        // IS IN RANGE OVERRIDE
        public override bool IsInRange(IMonoAgent agent, float distance, Data data, IComponentReference references)
        {
            var actionData = data;

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
            public Capable CapableTarget { get; set; }

            // Direct access to IA and AnimPlayer
            [GetComponentInParent] public IA ia { get; set; }
            public AttackCapacity attack_capacity { get; set; }
        }
    }
}