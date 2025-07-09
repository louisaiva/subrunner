using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Core;
using UnityEngine;

namespace subrunner.goap
{
    public class Brain : MonoBehaviour
    {
        [Header("Agent Type")]
        [SerializeField] private string agent_type;

        [Header("GOAP Components")]
        protected AgentBehaviour agent; // handles action's doing
        protected GoapActionProvider provider; // handles goal's doing WHICH MEANS action's planning

        [Header("Logs")]
        [SerializeField] private bool log_checks = false; // whether to log the brain's actions

        // PROPERTIES
        public IA ia => transform.parent.GetComponent<IA>();


        // AWAKE
        private void Awake()
        {
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();

            // we set the provider's agent type
            GoapBehaviour goap = GameObject.Find("/utils/goap_manager").GetComponent<GoapBehaviour>();
            if (goap == null)
            {
                Debug.LogError("(Brain) GoapBehaviour not found in the scene. Please add it to /utils/goap_manager");
                return;
            }
            provider.AgentType = goap.GetAgentType(agent_type);
        }


        // ENABLING / DISABLING
        private void OnEnable()
        {
            // subscribe to the agent's events
            agent.Events.OnMove += check_distance_to_target;
        }
        private void OnDisable()
        {
            // unsubscribe to the agent's events
            agent.Events.OnMove -= check_distance_to_target;
        }

        // CHECKS
        private void check_distance_to_target(ITarget target)
        {
            // checks if the current action is an attack action
            if (agent.ActionState.Action is not AttackAction) { return; }
            if (target is not TransformTarget transformTarget) { return; }

            if (log_checks) { Debug.Log($"(AttackAction) {ia.name} checking distance to target {transformTarget.Transform.name}"); }

            AttackCapacity attack_capacity = ia.GetCapacity<AttackCapacity>();
            if (attack_capacity == null) { return; }

            if (Vector3.Distance(transformTarget.Transform.position, ia.transform.position)
                < attack_capacity.range_target_detection) { return; }

            // stop the action
            if (log_checks) { Debug.Log($"(AttackAction) {ia.name} stopped attacking {transformTarget.Transform.name} because it is too far away."); }

            agent.StopAction();
        }

    }
}