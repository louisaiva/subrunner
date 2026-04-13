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
    public class CapableTarget : ITarget, IDisposable
    {

        // unloaded capable data ref
        private CapableData capable_data;
        public CapableData CapableData { get { return capable_data; } }

        // loaded capable ref
        private Capable capable;
        public Capable Capable { get { return capable; } }

        // position
        public Vector3 Position
        {
            get
            {
                if (capable != null && capable.Loaded) { return capable.transform.position; }
                return capable_data != null ? capable_data.position : Vector3.zero;
            }
        }

        // is valid
        public bool IsValid() { return capable_data != null; }


        // CONSTRUCTORS
        public CapableTarget(Capable capable)
        {
            if (capable == null || !capable.Loaded) { return; }
            this.capable = capable;
            this.capable_data = capable.data;

            // we register to the capable_data events to dynamically update the loaded capable ref
            capable_data.OnCapableLoaded += on_capable_loaded;
            capable_data.OnCapableUnloaded += on_capable_unloaded;
        }

        // DESTRUCTOR
        public void Dispose()
        {
            unsubscribe_data_events();

            // null everything
            capable = null;
            capable_data = null;
        }
        private void unsubscribe_data_events()
        {
            if (capable_data == null) { return; }
            capable_data.OnCapableLoaded -= on_capable_loaded;
            capable_data.OnCapableUnloaded -= on_capable_unloaded;
        }

        // CAPABLE UPDATE
        public ITarget SetCapable(Capable capable)
        {
            if (capable == null || !capable.Loaded) { return null; }
            if (capable_data == capable.data) { return this; } // we are already set to this capable or the capable data is the same as the current one, we do nothing

            // we unregister from the old capable_data events to avoid memory leaks
            unsubscribe_data_events();

            // reassign the capable and capable_data refs
            this.capable = capable;
            this.capable_data = capable.data;

            // we register to the capable_data events to dynamically update the loaded capable ref
            capable_data.OnCapableLoaded += on_capable_loaded;
            capable_data.OnCapableUnloaded += on_capable_unloaded;
            return this;
        }

        // CAPABLE DATA EVENTS
        private void on_capable_unloaded(Capable capable, CapableData data)
        {
            this.capable = null;
        }
        private void on_capable_loaded(Capable capable, CapableData data)
        {
            this.capable = capable;
        }
    }
}
