using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class Brain : MonoBehaviour
    {
        private AgentBehaviour agent;
        private GoapActionProvider provider;
        [Header("GOAP")]
        [SerializeField] private GoapBehaviour goap;
        [SerializeField] private string agent_type;

        private void Awake()
        {
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();

            // we get the goap manager & the agent type
            goap = GameObject.Find("/utils/goap_manager").GetComponent<GoapBehaviour>();
            AgentTypeBehaviour agent_type_behaviour = goap.transform.Find(agent_type).GetComponent<AgentTypeBehaviour>();

            // we set the provider's agent behaviour
            // provider.AgentTypeBehaviour = agent_type_behaviour;
            provider.AgentType = agent_type_behaviour.AgentType;
        }

        private void Start()
        {
            this.provider.RequestGoal<IdleGoal>();
        }
    }
}