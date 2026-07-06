
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using subrunner.goap;
using UnityEngine;

/// <summary>
/// MotorCapacity is a capacity that ONLY shows the hover animation of an Interactable.
/// </summary>
public class MotorCapacity : Capacity
{
    public IA IA
    {
        get
        {
            if (Capable == null) { return null; }
            return (IA)Capable;
        }
    }

    [Header("Components")]
    private AgentBehaviour _agent; // doer
    public AgentBehaviour Agent
    {
        get
        {
            if (_agent == null) { _agent = this.GetComponent<AgentBehaviour>(); }
            return _agent;
        }
    }
    private GoapActionProvider _provider; // planner
    public GoapActionProvider Provider
    {
        get
        {
            if (_provider == null) { _provider = this.GetComponent<GoapActionProvider>(); }
            return _provider;
        }
    }

    private GoToBehaviour _goto; // go to
    private GoToBehaviour mover
    {
        get
        {
            if (_goto == null) { _goto = GetComponentInChildren<GoToBehaviour>(includeInactive: true); }
            return _goto;
        }
    }

    public MotorData mdata => (MotorData)data;
    public IActionData currentActionData => Agent?.ActionState?.Data;
    private string last_agent_type = "";


    // pending actions
    private List<IGoapAction> pending_actions = new List<IGoapAction>();
    private IGoapAction current_action = null;


    [Header("Logs")]
    [SerializeField] private bool log_agent_type = false;
    [SerializeField] private bool log_world_state_loading = false;
    [SerializeField] private bool log_goals = false;
    [SerializeField] private bool log_pending_actions = false;


    // AWAKE
    private void Awake()
    {
        // the only thing we do here is assign the goap action provider to the "none" agent
        Provider.AgentType = MotorEngine.Goap.GetAgentType("none");
        if (log_agent_type) { Debug.Log($"(MotorCapacity) Assigned GoapActionProvider to agent type 'none'"); }
        // provider & agent are always the same on a MotorCapacity bc it is instantiated together etc so we can register/unregister callbacks during the awake / on destroy
        register_callbacks();
    }
    private void OnDestroy() { unregister_callbacks(); } 

    // ON ENABLE / DISABLE
    private void register_callbacks()
    {
        // subscribe to the agent's events
        // agent.Events.OnMove += this.check_distance_to_target;
        Provider.Events.OnNoActionFound += this.OnNoActionFound;
        Provider.Events.OnGoalCompleted += this.OnGoalCompleted;
        Agent.Events.OnActionStart += this.OnActionStart;
        Agent.Events.OnActionEnd += this.OnActionEnd;
    }
    private void unregister_callbacks()
    {
        // unsubscribe to the agent's events
        // agent.Events.OnMove -= this.check_distance_to_target;
        Provider.Events.OnNoActionFound -= this.OnNoActionFound;
        Provider.Events.OnGoalCompleted -= this.OnGoalCompleted;
        Agent.Events.OnActionStart -= this.OnActionStart;
        Agent.Events.OnActionEnd -= this.OnActionEnd;
    }



    // IA ACTION DELEGATES
    private void OnNoActionFound(IGoalRequest request) { IA.OnNoActionFound(request); }
    private void OnActionStart(IAction action)
    {
        if (log_pending_actions) { Debug.Log($"(MotorCapacity) Current action started : {action?.GetType().GetFriendlyName() ?? "null"}"); }
        update_pending_actions(action as IGoapAction);
    }
    private void OnActionEnd(IAction action)
    {
        if (current_action == action)
        {
            current_action = null;
            if (log_pending_actions) { Debug.Log($"(MotorCapacity) Current action ended : {action?.GetType().GetFriendlyName() ?? "null"}"); }
        }

        IA.OnActionEnd(action);
    }
    private void OnGoalCompleted(IGoal goal) { IA.OnGoalCompleted(goal); }


    // DETERMINE GOAL
    public void RequestSuitedGoal()
    {
        if (mdata == null) { return; }

        // we request the goal
        RequestGoals(Provider.AgentType.GetGoals());
    }
    public void RequestGoal<T>() where T : GoalBase
    {
        if (log_goals) { Debug.Log($"(MotorCapacity) {data.owner_id} is requesting goal of type '{typeof(T).Name}'"); }
        Provider.RequestGoal<T>();
    }
    public void RequestGoals(List<IGoal> goals)
    {
        Type[] goal_types = new Type[goals.Count];
        for (int i = 0; i < goals.Count; i++)
        {
            goal_types[i] = goals[i].GetType();
        }
        if (log_goals) { Debug.Log($"(MotorCapacity) {data.owner_id} is requesting goals of types '{string.Join(", ", goal_types.Select(t => t.Name))}'"); }
        Provider.RequestGoal(goal_types);
    }



    // ACTION MANAGEMENT
    public void StopCurrentAction()
    {
        if (Agent == null) { return; }
        Agent.StopAction(resolveAction: true);
    }
    private void update_pending_actions(IGoapAction action)
    {
        // updates pending & current actions
        IConnectable[] plan = Provider?.CurrentPlan?.Plan;
        if (plan == null || plan.Length == 0)
        {
            pending_actions.Clear();
            current_action = action;
            Debug.Log($"(MotorCapacity) No plan found, pending actions cleared and current action set to {action?.GetType().Name ?? "null"}");
            return;
        }

        // we update the pending actions list
        pending_actions.Clear();
        bool found_current = false;
        for (int i = 0; i < plan.Length; i++)
        {
            if (plan[i] is not IGoapAction i_action) { continue; }
            if (i_action == action)
            {
                found_current = true;
                current_action = i_action;
                continue;
            }
            if (!found_current) { continue; } // we only want NOT done actions
            pending_actions.Add(i_action);
        }
        if (log_pending_actions) { Debug.Log($"(MotorCapacity) Updated pending actions : {GetPendingActionsDetails()}"); }
    }
    public bool StillHasPendingActions()
    {
        return pending_actions.Count > 0;
    }
    public string GetPendingActionsDetails()
    {
        string debug = $"[{current_action?.GetType().GetFriendlyName() ?? "null"}] >> ";
        debug += pending_actions.Count > 0 ? string.Join(" >> ", pending_actions.Select(a => a.GetType().GetFriendlyName())) : "none";
        return debug;
    }
    public Type GetCurrentGoal()
    {
        if (Provider.CurrentPlan == null) { return null; }
        if (Provider.CurrentPlan.Goal == null) { return null; }
        return Provider.CurrentPlan.Goal.GetType();
    }










    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        if (data is not MotorData motor_data) { return; }

        // we load the goto
        mover.Load(motor_data.avoidance_data);

        // we set the provider's agent type
        Provider.AgentType = MotorEngine.Goap.GetAgentType(motor_data.agent_type);
        if (log_agent_type) { Debug.Log($"(MotorCapacity) {data.owner_id} set GoapActionProvider agent type to '{motor_data.agent_type}' from data"); }
        Agent.Initialize(); // we refresh the injected data for the agent
        // Provider.Receiver = Agent;


        // set this before the rest so the data is set
        // ? yes but is there a reason why we don't set it at the top of method ?
            // -> yes I think the Provider must be initialized properly before we have a data
        base.LoadData(data, capable_data); 


        // we log
        if (last_agent_type == motor_data.agent_type)
        {
            if (log_world_state_loading) { Debug.Log($"(MotorCapacity) Loading serialized world data for '{data.owner_id}' (same type) :\n{motor_data.local_world_data.GetDetails()}"); }
            
            // we have the same agent type as before, no need to clear the data, we simply repopulate the existing runtime world data
            motor_data.local_world_data.PopulateRuntimeData(Provider.WorldData);
        }
        else
        {
            if (log_world_state_loading) { Debug.Log($"(MotorCapacity) Loading serialized world data for '{data.owner_id}' (different type) :\n{motor_data.local_world_data.GetDetails()}"); }
            // clear the current runtime world data in the provider and then feed the serialized data to avoid conflicts
            // restore local world snapshots from the motor data to the provider's world data
            motor_data.local_world_data.ClearAndPopulateRuntimeData(Provider.WorldData);
        }
        
        RequestSuitedGoal();
    }
    public override void UnloadData()
    {
        string owner_id = data != null ? data.owner_id : "unknown";

        // we inform the MotorEngine that we are unloading this motor data
        if (data != null) { MotorEngine.Instance.RegisterMotorUnloading(mdata); }

        // we unload the goto
        mover.Unload();

        // stop agent action
        Agent.StopAction(resolveAction: false);
        Agent.ActionState.Reset();

        // we set the last agent type to check if same on next load
        last_agent_type = Provider.AgentType.Id;

        // we reset the provider's agent type
        Provider.AgentType = MotorEngine.Goap.GetAgentType("none");
        if (log_agent_type) { Debug.Log($"(MotorCapacity) {owner_id} reset GoapActionProvider agent type to 'none' from unload"); }

        // we unload the data -> will save the Provider's world data to the motor data
        base.UnloadData();
    }



    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (data is not MotorData motor_data) { return; }
        
        // we save the runtime world data from the provider to the motor data
        motor_data.local_world_data.CreateOrPopulateSerializedData(Provider.WorldData);

        // we log
        if (log_world_state_loading) { Debug.Log($"(MotorCapacity) Saved dynamic world data for '{data.owner_id} :\n{motor_data.local_world_data.GetDetails()}"); }
    }


    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        MotorData static_data = new MotorData(base.GetStaticData())
        {
            agent_type = get_static_agent_type(),
            avoidance_data = mover.GetStaticAvoidanceData()
        };

        return static_data;
    }
    private string get_static_agent_type()
    {
        // the agent type is the capable's type to lowercase
        Capable capable = transform.parent.GetComponent<Capable>();
        if (capable == null) { return ""; }
        return capable.GetType().Name.ToLower();
    }
}