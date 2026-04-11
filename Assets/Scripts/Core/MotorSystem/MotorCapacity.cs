
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
    private GoapBehaviour _goap; // main planner
    private GoapBehaviour goap
    {
        get
        {
            if (_goap == null) { _goap = GameObject.Find("/game/goap_manager").GetComponent<GoapBehaviour>(); }
            if (_goap == null) { if (log) { Debug.LogError("(MotorCapacity) GoapBehaviour not found in the scene. Please add it to /game/goap_manager"); } }
            return _goap;
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

    private MotorData mdata => (MotorData)data;
    public IActionData currentActionData => Agent?.ActionState?.Data;
    private string last_agent_type = "";


    [Header("Logs")]
    [SerializeField] private bool log_agent_type = false;
    [SerializeField] private bool log_goals = false;
    [SerializeField] private bool log_world_state_loading = false;


    // AWAKE
    private void Awake()
    {
        // the only thing we do here is assign the goap action provider to the "none" agent
        Provider.AgentType = goap.GetAgentType("none");
        if (log_agent_type) { Debug.Log($"(MotorCapacity) Assigned GoapActionProvider to agent type 'none'"); }
    }


    // ON ENABLE / DISABLE
    private void OnEnable()
    {
        // subscribe to the agent's events
        // agent.Events.OnMove += this.check_distance_to_target;
        Provider.Events.OnNoActionFound += this.OnNoActionFound;
        Provider.Events.OnActionEnd += this.OnActionEnd;
        Provider.Events.OnGoalCompleted += this.OnGoalCompleted;
    }
    private void OnDisable()
    {
        // unsubscribe to the agent's events
        // agent.Events.OnMove -= this.check_distance_to_target;
        Provider.Events.OnNoActionFound -= this.OnNoActionFound;
        Provider.Events.OnActionEnd -= this.OnActionEnd;
        Provider.Events.OnGoalCompleted -= this.OnGoalCompleted;
    }

    // GOAL DELEGATES
    private void OnNoActionFound(IGoalRequest request) { request_suited_goal(); }
    private void OnActionEnd(IAction action) { request_suited_goal(); }
    private void OnGoalCompleted(IGoal goal) { request_suited_goal(); }

    // DETERMINE GOAL
    protected void request_suited_goal()
    {
        if (mdata == null) { return; }

        // we request the goal
        request_goal(Provider.AgentType.GetGoals());
    }
    protected void request_goal<T>() where T : GoalBase
    {
        if (log_goals) { Debug.Log($"(MotorCapacity) {data.owner_id} is requesting goal of type '{typeof(T).Name}'"); }
        Provider.RequestGoal<T>();
    }
    protected void request_goal(string goal_type)
    {
        Type goal = convert_string_to_goals(goal_type);

        if (goal == null)
        {
            if (log_goals) { Debug.LogWarning($"(MotorCapacity - request_goal) Goal {goal_type} not found for {data.owner_id}"); }
            return;
        }

        if (log_goals) { Debug.Log($"(MotorCapacity) {data.owner_id} is requesting goal of type '{goal_type}'"); }
        Provider.RequestGoal(goal);
    }
    protected void request_goal(List<IGoal> goals)
    {
        Type[] goal_types = new Type[goals.Count];
        for (int i = 0; i < goals.Count; i++)
        {
            goal_types[i] = goals[i].GetType();
        }
        if (log_goals) { Debug.Log($"(MotorCapacity) {data.owner_id} is requesting goals of types '{string.Join(", ", goal_types.Select(t => t.Name))}'"); }
        Provider.RequestGoal(goal_types);
    }
    protected Type convert_string_to_goals(string goal_type)
    {
        // todo : debug why the Type.GetType(goal_type) does not work and delete this very not convenient method
        switch (goal_type)
        {
            case "WanderGoal":
                return typeof(WanderGoal);
            case "KillBeingGoal":
                return typeof(KillBeingGoal);
            case "EatGoal":
                return typeof(EatGoal);
        }
        return null;
    }

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        if (data is not MotorData motor_data) { return; }

        // we load the goto
        mover.Load(motor_data.avoidance_data);

        // we set the provider's agent type
        Provider.AgentType = goap.GetAgentType(motor_data.agent_type);
        if (log_agent_type) { Debug.Log($"(MotorCapacity) {data.owner_id} set GoapActionProvider agent type to '{motor_data.agent_type}' from data"); }

        Agent.Initialize(); // we refresh the injected data for the agent

        base.LoadData(data); // set this before the rest so the data is set


        // we log
        if (log_world_state_loading) { Debug.Log($"(MotorCapacity) Loading serialized world data for '{data.owner_id} :\n{motor_data.local_world_data.GetDetails()}"); }
        if (last_agent_type == motor_data.agent_type)
        {
            // we have the same agent type as before, no need to clear the data, we simply repopulate the existing runtime world data
            motor_data.local_world_data.PopulateRuntimeData(Provider.WorldData);
        }
        else
        {
            // clear the current runtime world data in the provider and then feed the serialized data to avoid conflicts
            // restore local world snapshots from the motor data to the provider's world data
            motor_data.local_world_data.ClearAndPopulateRuntimeData(Provider.WorldData);
        }
        
        request_suited_goal();
    }
    public override void UnloadData()
    {
        string owner_id = data != null ? data.owner_id : "unknown";

        // this saves the dynamic data (we need to save it before stopping things)
        base.UnloadData();

        // we unload the goto
        mover.Unload();

        // stop agent action
        Agent.StopAction(resolveAction: false);
        Agent.ActionState.Reset();

        // we set the last agent type to check if same on next load
        last_agent_type = Provider.AgentType.Id;

        // we reset the provider's agent type
        Provider.AgentType = goap.GetAgentType("none");
        if (log_agent_type) { Debug.Log($"(MotorCapacity) {owner_id} reset GoapActionProvider agent type to 'none' from unload"); }
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