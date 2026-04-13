using System;
using System.Collections.Generic;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;


namespace subrunner.goap
{
    public class IA_Action<TData> : GoapActionBase<TData> where TData : IA_ActionData, new()
    {

        private Dictionary<CapableData, Action<float, Force>> damage_callbacks = new Dictionary<CapableData, Action<float, Force>>(); // we store the damage callback for each IA to be able to remove it when we stop the action


        // START
        public override void Start(IMonoAgent agent, TData data)
        {
            // we register the damage callback for this IA
            register_damage_callback(agent, data);
        }

        // DAMAGE CALLBACKS
        private void register_damage_callback(IMonoAgent agent, TData data)
        {
            // we get the IA's HealthCapacity to register to its OnTakeDamage event
            if (!data.ia.Loaded)
            {
                if (Logger.Instance.LOG_IA_ACTION) { Debug.LogWarning($"(IA_Action) {data.ia} is not loaded, cannot register damage callback for AttackAction"); }
                return;
            }

            // we get the IA's HealthCapacity to register to its OnTakeDamage event
            if (!data.ia.TryGetCapacity(out HealthCapacity health_capacity))
            {
                if (Logger.Instance.LOG_IA_ACTION) { Debug.LogWarning($"(IA_Action) {data.ia.ID} has no HealthCapacity, cannot register damage callback for AttackAction"); }
                return;
            }

            // we create the callback
            Action<float, Force> damage_callback = (d, f) => TakeDamage(agent, data);

            // we register it to the IA's HealthCapacity.OnTakeDamage event
            health_capacity.OnTakeDamage += damage_callback;

            // we store it in the dictionary
            damage_callbacks[data.ia.data] = damage_callback;
        }
        private void unregister_damage_callback(IMonoAgent agent, TData data)
        {
            if (!data.ia.Loaded)
            {
                if (Logger.Instance.LOG_IA_ACTION) { Debug.LogWarning($"(IA_Action) {data.ia.ID} is not loaded, cannot unregister damage callback for AttackAction"); }
                return;
            }

            // we get the IA's HealthCapacity to unregister from its OnTakeDamage event
            if (!data.ia.TryGetCapacity(out HealthCapacity health_capacity)) { return; }

            // we get the callback from the dictionary
            if (!damage_callbacks.TryGetValue(data.ia.data, out Action<float, Force> damage_callback))
            {
                if (Logger.Instance.LOG_IA_ACTION) { Debug.LogWarning($"(IA_Action) No damage callback found for {data.ia.ID}, cannot unregister damage callback for AttackAction"); }
                return;
            }

            // we unregister it from the IA's HealthCapacity.OnTakeDamage event
            health_capacity.OnTakeDamage -= damage_callback;

            // we remove it from the dictionary
            damage_callbacks.Remove(data.ia.data);
        }

        // PERFORM
        public override IActionRunState Perform(IMonoAgent agent, TData data, IActionContext context)
        {
            return ActionRunState.Completed;
        }

        // COMPLETE
        public override void Complete(IMonoAgent agent, TData data)
        {
            // we remove the damage callback
            unregister_damage_callback(agent, data);
        }

        // TAKE DAMAGE
        public virtual void TakeDamage(IMonoAgent agent, TData data)
        {
            if (Logger.Instance.LOG_IA_ACTION) { Debug.Log($"(IA_Action) {data.ia.ID} took damage during an action, we stop it"); }

            // we stop the action
            agent.StopAction();

            // finally reset the action state
            agent.ActionState.Reset();
        }
        
        // STOP
        public override void Stop(IMonoAgent agent, TData data)
        {
            // we remove the damage callback
            unregister_damage_callback(agent, data);
        }
    }

    public class IA_ActionData : IActionData
    {
        public ITarget Target { get; set; }

        // Direct access to IA
        [GetComponentInParent] public IA ia { get; set; }
    }

    /// <summary>
    /// This class is only runtime based. it means that it can successfully cache a CapableData ref when
    /// the capable is set or changed
    /// </summary>
    public class CapableTarget : ITarget
    {

        // unloaded capable data ref
        private CapableData capable_data;
        public CapableData CapableData { get { return capable_data; } }
        public string CapableID { get { return capable_data?.id; } }

        // loaded capable ref
        public Capable Capable { get { return capable_data?.Capable; } }
        public bool Loaded { get { return Capable is not null && Capable.Loaded; } }

        // position
        public Vector3 Position { get { return capable_data?.Position ?? Vector2.zero; } }

        // is valid
        public bool IsValid() { return capable_data is not null; }


        // CONSTRUCTORS
        public CapableTarget(Capable capable)
        {
            if (capable == null || !capable.Loaded) { return; }
            this.capable_data = capable.data;
        }
        public CapableTarget(CapableData capable_data)
        {
            if (capable_data == null) { return; }
            this.capable_data = capable_data;
        }
        public CapableTarget(CapableTarget other)
        {
            if (other == null) { return; }
            this.capable_data = other.capable_data;
        }

        // CAPABLE UPDATE
        public ITarget SetCapable(Capable capable)
        {
            if (capable == null || !capable.Loaded) { return null; }
            if (capable_data == capable.data) { return this; } // we are already set to this capable or the capable data is the same as the current one, we do nothing

            // reassign the capable and capable_data refs
            this.capable_data = capable.data;

            return this;
        }
        public ITarget SetCapableData(CapableData capable_data)
        {
            if (capable_data == null) { return null; }
            if (this.capable_data == capable_data) { return this; } // we are already set to this capable data, we do nothing

            // reassign the capable and capable_data refs
            this.capable_data = capable_data;
            return this;
        }
    }
}
