using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;

using UnityEngine;

namespace subrunner.goap
{
    public class AttackAction : GoapActionBase<AttackAction.Data>
    {
        // dictionary that holds the last or current AttackActionResult for each IA performing this action, we will use it to update the distance to attack based on the last result (missed, took damage, hit)
        private Dictionary<IA, AttackActionResult> last_attack_results = new Dictionary<IA, AttackActionResult>();
        // ! WARNING !
        // this dictionary has IA as key, not string ia id. SO we may have weird behaviours sometimes in particular cases
        // where the prey target is the same but the loaded attacker inside the IA component is different,
        // then maybe the distance to attack will be kept, causing the new attacker to have a totally weird distance
        // BUT it is fun ahah, add some randomness which is great.
        // otherwise there is no issue with storing the IA instead of ID, bcz it is more performant, and
        // all the mobs doing this action are loaded mobs anyway, so we know everything is loaded.


        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            data.attack_capacity = data.ia.GetCapacity<AttackCapacity>();
            data.CapableTarget = data.Target is TransformTarget target ? target.Transform.GetComponent<Capable>() : null;
            if (data.CapableTarget == null)
            {
                if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.LogWarning($"(AttackAction) {data.ia.data.id} has no valid target for AttackAction"); }
                return;
            }

            // we check if we have a last_attack_result for this IA
            
            // if not we create a new one
            if (!last_attack_results.TryGetValue(data.ia, out AttackActionResult last_result))
            {
                last_result = new AttackActionResult(data.CapableTarget, data.attack_capacity.distance_to_attack);
                last_attack_results[data.ia] = last_result;

                // and we watch the attack
                last_result.WatchTheAttack(data.attack_capacity);

                // we cache the stop distance for the in range check
                data.this_action_stop_distance = data.attack_capacity.distance_to_attack;
                return;
            }

            // if we have one, we update its target if changed
            if (last_result.target == null || last_result.target.capable_id != data.CapableTarget.data.id)
            {
                // we changed target ! we assign a new target and start watching it
                last_result.WatchTheAttack(data.attack_capacity, data.CapableTarget, data.attack_capacity.distance_to_attack);

                // we cache the stop distance for the in range check
                data.this_action_stop_distance = data.attack_capacity.distance_to_attack;
                return;
            }
            

            // we have the same target as last time, we update the distance to attack
            // based on last attack status
            // -> if missed -> we go closer
            // -> if took damage -> we go further
            // -> if hit -> we keep the same distance
            // else we reset to default distance to attack
            // and we cache it into data.this_action_stop_distance to be used in the IsInRange override

            if (last_result.attack_status == AttackActionStatus.Missed)
            {
                data.this_action_stop_distance = Mathf.Max(0.25f, last_result.attack_distance * 0.8f); // we try to get 20% closer
            }
            else if (last_result.attack_status == AttackActionStatus.TookDamage)
            {
                data.this_action_stop_distance = last_result.attack_distance * 3f; // we try to attack from FAAR away
            }
            else if (last_result.attack_status == AttackActionStatus.Hit)
            {
                data.this_action_stop_distance = last_result.attack_distance;
            }
            else { data.this_action_stop_distance = data.attack_capacity.distance_to_attack; } // default value if we don't have a valid last result


            // we update the last result with the current target and distance, and we watch the attack
            last_result.WatchTheAttack(data.attack_capacity, data.CapableTarget, data.this_action_stop_distance);
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            if (data.CapableTarget == null) { return; }
            Capable capable_target = data.CapableTarget;

            // verify that the being is still Alive
            if (!capable_target.TryGetCapacity(out HealthCapacity health) || !health.Alive) { return; }

            // we turn over to face the target
            data.ia.OrientTowards(capable_target.transform.position);

            // Debug log only if enabled
            if (data.ia.log_actions)
            {
                Debug.Log($"(AttackAction) {data.ia.data.id} is trying to attack {capable_target.data.id}");
            }

            // use the attack capacity
            AttackCapacity attack_capacity = data.attack_capacity;
            if (attack_capacity.Able) { attack_capacity.Use(data.ia); }
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            // wait for the animation to finish
            if (data.ia.AnimPlayer.current_capacity == "attack") { return ActionRunState.Continue; }


            // we check if we have a last attack result for this IA to stop watching the attack
            if (last_attack_results.TryGetValue(data.ia, out AttackActionResult last_result))
            {
                // we check if we have a "hurted" animation playing to know if we took damage during the attack
                if (data.ia.AnimPlayer.IsPlaying("hurted"))
                {
                    last_result.StopWatchingTheAttack(data.attack_capacity, AttackActionStatus.TookDamage);
                }
                else
                {
                    last_result.StopWatchingTheAttack(data.attack_capacity);
                }
            }


            return ActionRunState.Completed;
        }

        // OVERRIDES
        public override bool IsInRange(IMonoAgent agent, float distance, Data data, IComponentReference references)
        {
            if (data.ia.log_actions) { Debug.Log($"(AttackAction) {data.ia.name} IsInRange check: distance={distance:F2}, stopping_distance={data.this_action_stop_distance:F2}, in_range={distance <= data.this_action_stop_distance}"); }
            return distance <= data.this_action_stop_distance;
        }
        public override void Stop(IMonoAgent agent, Data data)
        {
            // we check if we have a last attack result for this IA to stop watching the attack
            if (last_attack_results.TryGetValue(data.ia, out AttackActionResult last_result))
            {
                last_result.StopWatchingTheAttack(data.attack_capacity, AttackActionStatus.Canceled);
            }

            base.Stop(agent, data);
        }


        // DATA
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public Capable CapableTarget { get; set; }

            // Direct access to IA and AnimPlayer
            [GetComponentInParent] public IA ia { get; set; }
            public AttackCapacity attack_capacity { get; set; }
            public float this_action_stop_distance { get; set; } // we cache this at start to be used in the IsInRange override
        }
    }
}

public class AttackActionResult
{
    public CapableTarget target;
    public AttackActionStatus attack_status;
    public float attack_distance;

    public AttackActionResult(CapableTarget target, AttackActionStatus attack_status, float attack_distance)
    {
        this.target = target;
        this.attack_status = attack_status;
        this.attack_distance = attack_distance;
    }
    public AttackActionResult(Capable target, float attack_distance)
    {
        this.target = new CapableTarget(target);
        this.attack_distance = attack_distance;
    }

    // WATCH THE ATTACK
    public void WatchTheAttack(AttackCapacity attacker, Capable target, float distance)
    {
        // we set the new target and distance (in case the target moved during the attack)
        if (this.target == null) { this.target = new CapableTarget(target); }
        else if (this.target.capable_id != target.data.id) { this.target.SetTarget(target); }
        this.attack_distance = distance;

        WatchTheAttack(attacker);
    }
    public void WatchTheAttack(AttackCapacity attacker)
    {
        // we register to AttackCapacity's OnHealthHit event to know if we hit the target or not
        attacker.OnDamageDealt += on_health_hit;

        // we set the status to Processing since we are currently performing the attack and we don't know the result yet
        this.attack_status = AttackActionStatus.Processing;


        // we log
        if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackActionResult) Started watching attack from {attacker.data.id} ({attacker.Capable.data.id}), our target is {target.capable_id} and distance is {attack_distance:F2}"); }
    }

    // STOP WATCHING THE ATTACK
    public void StopWatchingTheAttack(AttackCapacity attacker, AttackActionStatus status)
    {
        // update the status
        this.attack_status = status;
        StopWatchingTheAttack(attacker);
    }
    public void StopWatchingTheAttack(AttackCapacity attacker)
    {
        // we unregister from the AttackCapacity's OnHealthHit event
        attacker.OnDamageDealt -= on_health_hit;

        // if we are still in Processing status here, it means we completed the attack
        // without taking damage, without hitting, without canceling, 
        // so it is a missed one.
        if (this.attack_status != AttackActionStatus.Processing) { return; }
        this.attack_status = AttackActionStatus.Missed;
    }

    // ON HEALTH HIT
    private void on_health_hit(HealthCapacity hit_target, float damage)
    {
        // we log
        if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackActionResult) Target {hit_target.Capable.data.id} took {damage} damage, our target is {target.capable_id}"); }

        // we check if the hit target is the one we are watching
        if (target.capable_id != hit_target.Capable.data.id) { return; }

        // we got a hit on the target, we set the attack status to Hit
        attack_status = AttackActionStatus.Hit;
    }
}

public enum AttackActionStatus
{
    Processing, // we are currently performing the attack action, we don't know the result yet
    Canceled, // we stopped the action before completing it
    TookDamage, // we started the action but we receive damage before dealing some -> maybe we were too close, next we try further away
    Missed, // we completed the action but missed the target (for example because it moved out of range during the attack) -> we may be too far, next we try closer
    Hit // we completed the action and hit the target -> good, we keep the same distance for the next attack
}