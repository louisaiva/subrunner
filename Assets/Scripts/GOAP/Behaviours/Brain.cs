using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class Brain : MonoBehaviour
    {
        [Header("Agent Type")]
        [SerializeField] private string agent_type;

        [Header("GOAP Components")]
        private AgentBehaviour agent; // handles action's doing
        private GoapActionProvider provider; // handles goal's doing WHICH MEANS action's planning

        // PROPERTIES
        public IA ia => transform.parent.GetComponent<IA>();

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

        private void Start()
        {
            this.provider.RequestGoal<WanderGoal,SmellFoodGoal>();
        }
    }
}