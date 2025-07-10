using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Agent.Core;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace subrunner.goap
{
    public class Brain : MonoBehaviour
    {
        [Header("Agent Type")]
        [SerializeField] private string agent_type;
        protected IA ia;

        [Header("Goal Selection")]
        [SerializeField] protected List<GoalPriority> goals = new List<GoalPriority>();
        [SerializeField] protected GoalType current_goal; // The current goal type being pursued

        [Header("GOAP Components")]
        protected AgentBehaviour agent; // handles action's doing
        protected GoapActionProvider provider; // handles goal's doing WHICH MEANS action's planning


        [Header("Logs")]
        [SerializeField] private bool log_checks = false; // whether to log the brain's actions
        [SerializeField] private bool log_goals = false; // whether to log the brain's actions
        [SerializeField] private bool log_goals_update = false; // whether to log the brain's actions


        // AWAKE
        private void Awake()
        {
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();
            this.ia = this.transform.parent.GetComponent<IA>();

            // we set the provider's agent type
            GoapBehaviour goap = GameObject.Find("/utils/goap_manager").GetComponent<GoapBehaviour>();
            if (goap == null)
            {
                Debug.LogError("(Brain) GoapBehaviour not found in the scene. Please add it to /utils/goap_manager");
                return;
            }
            provider.AgentType = goap.GetAgentType(agent_type);


            // we initialize the goals
            if (goals.Count == 0)
            {
                Debug.LogError($"(Brain) no goal found for {ia.name}, please create them in the inspector");
            }
        }


        // ENABLING / DISABLING
        private void OnEnable()
        {
            // subscribe to the agent's events
            agent.Events.OnMove += this.check_distance_to_target;
            provider.Events.OnNoActionFound += this.OnNoActionFound;
            provider.Events.OnActionEnd += this.OnActionEnd;
            provider.Events.OnGoalCompleted += this.OnGoalCompleted;
        }
        private void OnDisable()
        {
            // unsubscribe to the agent's events
            agent.Events.OnMove -= this.check_distance_to_target;
            provider.Events.OnNoActionFound -= this.OnNoActionFound;
            provider.Events.OnActionEnd -= this.OnActionEnd;
            provider.Events.OnGoalCompleted -= this.OnGoalCompleted;
        }

        // START
        private void Start() { DetermineGoal(); }


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


        // GOAL SELECTION LOGIC
        public void DetermineGoal(bool resolve = true)
        {
            // we cycle through all the goals priorities & we find the highest one
            GoalPriority highestGoal = null;
            int highestPriority = int.MinValue;

            string log = "(Brain) " + ia.name + "'s goals : \n\t";
            for (int i = 0; i < goals.Count; i++)
            {

                if (!goals[i].enabled) { continue; } // skip non-enabled goals

                log += goals[i].type + " (priority: " + goals[i].priority + ", enabled: " + goals[i].enabled + ") \n\t";
                if (goals[i].priority > highestPriority)
                {
                    highestGoal = goals[i];
                    highestPriority = goals[i].priority;
                }
            }
            if (log_goals_update) { Debug.Log(log); }
            if (highestGoal == null || highestGoal.type == current_goal) { return; }
            if (log_goals) { Debug.Log("(Brain) " + ia.name + " is requesting " + highestGoal.type); }

            // we request the goal
            request_goal(highestGoal.type,resolve);
        }
        private void request_goal(GoalType goal, bool resolve = true)
        {
            switch (goal)
            {
                case GoalType.None:
                    break;
                case GoalType.KillBeingGoal:
                    provider.RequestGoal<KillBeingGoal>(resolve);
                    break;
                case GoalType.EatGoal:
                    provider.RequestGoal<EatGoal>(resolve);
                    break;
                case GoalType.WanderGoal:
                    provider.RequestGoal<WanderGoal>(resolve);
                    break;
                default:
                    Debug.LogWarning($"(Brain) Unknown goal type: {goal}");
                    return;
            }

            // we set the current goal
            current_goal = goal;
            if (log_goals) { Debug.Log($"(Brain) {ia.name} is now pursuing goal: {current_goal}"); }
        }

        // GOAL DELEGATES
        private void OnNoActionFound(IGoalRequest request) { request_goal(GoalType.WanderGoal); }
        private void OnActionEnd(IAction action) { DetermineGoal(); }
        private void OnGoalCompleted(IGoal goal) { DetermineGoal(); }

        // GOAL MANAGEMENT
        public void EnableGoal(GoalPriority goal,bool resolve = true)
        {
            goal.enabled = true;
            if (goal.type != current_goal) { DetermineGoal(resolve); }
        }
        public void DisableGoal(GoalPriority goal, bool resolve = true)
        {
            goal.enabled = false;
            if (goal.type == current_goal) { DetermineGoal(resolve); }
        }
        public GoalPriority GetGoal(GoalType goalType)
        {
            foreach (var goal in goals)
            {
                if (goal.type == goalType)
                {
                    return goal;
                }
            }
            Debug.LogError($"(Brain) GoalType '{goalType}' not found in Brain's goals.");
            return null;
        }

    }

    [Serializable] public class GoalPriority
    {
        public GoalType type; // "KillBeingGoal", "EatGoal", "WanderGoal"
        public int priority; // Higher number = higher priority
        public bool enabled; // used by brain for excluding non enabled goals
    }

    [Serializable] public enum GoalType
    {
        None,
        WanderGoal,
        KillBeingGoal,
        EatGoal
    }
}