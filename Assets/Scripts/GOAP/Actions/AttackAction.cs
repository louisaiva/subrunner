using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;

using UnityEngine;

namespace subrunner.goap
{
    public class AttackAction : IA_Action<AttackAction.Data>
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

        [Header("Logs")]
        public bool log_start_target_info = false;


        // START
        public override void Start(IMonoAgent agent, Data data)
        {
            base.Start(agent, data);

            if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackAction) {data.ia.data.id} is starting AttackAction on target {data.Target}"); }

            data.attack_capacity = data.ia.GetCapacity<AttackCapacity>();
            // data.CapableTarget = data.Target is CapableTarget target ? target.Capable : null;
            if (data.CapableTarget == null || !data.CapableTarget.IsValid())
            {
                if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.LogWarning($"(AttackAction) {data.ia.ID} has no valid target for AttackAction"); }
                agent.StopAction(resolveAction: true);
                return;
            }
            if (!data.CapableTarget.Loaded)
            {
                // we have a target but it is not loaded
                // we don't care about checking the attack distance, because
                // the attack will happen later when this agent will be unloaded. so we just need to return
                // and it will go to the target, which will make this agent be unloaded, so it is perfect like this
                if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackAction) {data.ia.ID} target {data.CapableTarget.CapableID} is not loaded, we chase it"); }
                data.this_action_stop_distance = 0.5f;
                return;
            }



            // we check if we have a last_attack_result for this IA
            
            // if not we create a new one
            if (!last_attack_results.TryGetValue(data.ia, out AttackActionResult last_result)/*  || last_result == null */)
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
            /* if (log_start_target_info)
            {
                string log = $"(AttackAction) line 59 : last_result.target is null ? {last_result.target == null}";
                log += $" | last_result.target.capable_id : {(last_result.target != null ? last_result.target.capable_id : "null")}";
                log += $" | data.CapableTarget is null ? {data.CapableTarget == null}";
                log += $" | data.CapableTarget.data is null ? {(data.CapableTarget != null ? data.CapableTarget.data == null : "null")}";
                log += $" | data.CapableTarget.data.id : {(data.CapableTarget.data != null ? data.CapableTarget.data.id : "null")}";
                Debug.Log(log);
            } */
            if (last_result.Target == null || last_result.Target.CapableID != data.CapableTarget.CapableID)
            {
                // we changed target ! we assign a new target and start watching it
                last_result.Reset(data.CapableTarget, data.attack_capacity.distance_to_attack);
                last_result.WatchTheAttack(data.attack_capacity);

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

            if (last_result.Status == AttackActionStatus.Missed)
            {
                data.this_action_stop_distance = Mathf.Max(0.25f, last_result.AttackDistance * 0.8f); // we try to get 20% closer
            }
            else if (last_result.Status == AttackActionStatus.TookDamage)
            {
                data.this_action_stop_distance = last_result.AttackDistance * 1.5f; // we try to attack from 80% further away
            }
            else if (last_result.Status == AttackActionStatus.Hit)
            {
                data.this_action_stop_distance = last_result.AttackDistance;
            }
            else { data.this_action_stop_distance = data.attack_capacity.distance_to_attack; } // default value if we don't have a valid last result


            // we update the last result with the current target and distance, and we watch the attack
            last_result.Reset(data.CapableTarget, data.this_action_stop_distance);
            last_result.WatchTheAttack(data.attack_capacity);
        }

        // PERFORM
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
            if (data.CapableTarget is null) { return; }
            if (!data.CapableTarget.Loaded) { return; }
            Capable capable_target = data.CapableTarget.Capable;

            // verify that the being is still Alive
            if (!capable_target.TryGetCapacity(out HealthCapacity health) || !health.Alive) { return; }

            // we turn over to face the target
            data.ia.OrientTowards(capable_target.transform.position);

            // Debug log only if enabled
            if (Logger.Instance.LOG_ATTACK_ACTION)
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
                last_result.StopWatchingTheAttack(data.attack_capacity);
            }

            return ActionRunState.Completed;
        }


        // OVERRIDES
        public override bool IsInRange(IMonoAgent agent, float distance, Data data, IComponentReference references)
        {
            if (data.this_action_stop_distance <= 0f)
            {
                if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.LogWarning($"(AttackAction) {data.ia.ID} has invalid stop distance {data.this_action_stop_distance:F2}, we consider it is not in range"); }
                data.this_action_stop_distance = 0.5f;
                return false;
            }

            if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackAction) {data.ia.name} IsInRange check: distance={distance:F2}, stopping_distance={data.this_action_stop_distance:F2}, in_range={distance <= data.this_action_stop_distance}"); }
            return distance <= data.this_action_stop_distance;
        }

        // TAKE DAMAGE
        public override void TakeDamage(IMonoAgent agent, Data data)
        {
            base.TakeDamage(agent, data);

            // we set the last attack result as TookDamage
            if (last_attack_results.TryGetValue(data.ia, out AttackActionResult last_result))
            {
                last_result.StopWatchingTheAttack(data.attack_capacity, AttackActionStatus.TookDamage);
            }
        }
        public override void Stop(IMonoAgent agent, Data data)
        {
            base.Stop(agent, data);

            // we check if we have a last attack result for this IA to stop watching the attack
            if (last_attack_results.TryGetValue(data.ia, out AttackActionResult last_result))
            {
                last_result.StopWatchingTheAttack(data.attack_capacity, AttackActionStatus.Canceled);
            }
        }

        // DATA
        public class Data : IA_ActionData
        {
            public CapableTarget CapableTarget
            {
                get
                {
                    if (Target is CapableTarget capable_target) { return capable_target; }
                    return null;
                }
            }

            public AttackCapacity attack_capacity { get; set; }
            public float this_action_stop_distance { get; set; } // we cache this at start to be used in the IsInRange override
        }
    }

    public class AttackActionResult
    {
        private CapableTarget target;
        public CapableTarget Target { get { return target; } }
        private AttackActionStatus attack_status;
        public AttackActionStatus Status { get { return attack_status; } }
        private float attack_distance;
        public float AttackDistance { get { return attack_distance; } }
        private bool watching_attack = false;


        // CONSTRUCTORS
        /* public AttackActionResult(CapableTarget target, AttackActionStatus attack_status, float attack_distance)
        {
            this.target = target;
            this.attack_status = attack_status;
            this.attack_distance = attack_distance;
        } */
        public AttackActionResult(CapableTarget base_target, float attack_distance)
        {
            this.target = new CapableTarget(base_target.Capable); // we deep copy the target bcz we want to keep the same target until the next attack
            // and base_target will be updated next sensor's update, so we need to copy it
            this.attack_distance = attack_distance;
            this.attack_status = AttackActionStatus.NotStarted;
        }

        // RESETTORS
        public void Reset(CapableTarget new_target, float new_attack_distance)
        {
            this.target.SetCapableData(new_target.CapableData);
            this.attack_distance = new_attack_distance;
            this.attack_status = AttackActionStatus.NotStarted;
        }


        // WATCH THE ATTACK
        public void WatchTheAttack(AttackCapacity attacker)
        {
            // we register to AttackCapacity's OnHealthHit event to know if we hit the target or not
            attacker.OnDamageDealt += on_health_hit;

            // we set the status to Watching since we are now watching the attack perform
            this.attack_status = AttackActionStatus.Watching;

            watching_attack = true;

            // we log
            if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackActionResult) Started watching attack from {attacker.ID} ({attacker.Capable.ID}), our target is {target.CapableID} and distance is {attack_distance:F2}"); }
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
            if (!watching_attack) { return; }

            // we unregister from the AttackCapacity's OnHealthHit event
            attacker.OnDamageDealt -= on_health_hit;

            watching_attack = false;

            // if we are still in Processing status here, it means we completed the attack
            // without taking damage, without hitting, without canceling, 
            // so it is a missed one.
            if (this.attack_status != AttackActionStatus.Watching) { return; }
            this.attack_status = AttackActionStatus.Missed;
        }

        // ON HEALTH HIT
        private void on_health_hit(HealthCapacity hit_target, float damage)
        {
            // we log
            if (Logger.Instance.LOG_ATTACK_ACTION) { Debug.Log($"(AttackActionResult) Target {hit_target.Capable.data.id} took {damage} damage, our target is {target.CapableID}"); }

            // we check if the hit target is the one we are watching
            if (target.CapableID != hit_target.OwnerID) { return; }

            // we got a hit on the target, we set the attack status to Hit
            attack_status = AttackActionStatus.Hit;
        }
    }

    public enum AttackActionStatus
    {
        NotStarted, // the attack has not started yet
        Watching, // we are currently performing the attack action, we don't know the result yet
        Canceled, // we stopped the action before completing it
        TookDamage, // we started the action but we receive damage before dealing some -> maybe we were too close, next we try further away
        Missed, // we completed the action but missed the target (for example because it moved out of range during the attack) -> we may be too far, next we try closer
        Hit // we completed the action and hit the target -> good, we keep the same distance for the next attack
    }
}