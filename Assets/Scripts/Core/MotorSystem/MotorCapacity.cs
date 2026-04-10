
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


    [Header("Logs")]
    [SerializeField] private bool log_agent_type = false;
    [SerializeField] private bool log_goals = false;


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

        // restore local world snapshots before asking for a new plan
        hydrate_provider_world_data(motor_data);

        // we request the current goal
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

        // we reset the provider's agent type
        Provider.AgentType = goap.GetAgentType("none");
        if (log_agent_type) { Debug.Log($"(MotorCapacity) {owner_id} reset GoapActionProvider agent type to 'none' from unload"); }
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (data is not MotorData motor_data) { return; }
        if (Provider == null || Provider.WorldData == null) { return; }

        motor_data.local_world_state.Clear();
        motor_data.local_world_targets.Clear();

        foreach (var entry in Provider.WorldData.States)
        {
            if (entry.Key == null || entry.Value == null) { continue; }

            motor_data.local_world_state.Add(new LocalWorldStateData
            {
                key_name = serialize_type_name(entry.Key),
                key_value = entry.Value.Value,
            });
        }

        foreach (var entry in Provider.WorldData.Targets)
        {
            if (entry.Key == null || entry.Value == null || entry.Value.Value == null) { continue; }

            ITarget runtime_target = entry.Value.Value;
            LocalWorldTargetData target_data = new LocalWorldTargetData
            {
                key_name = serialize_type_name(entry.Key),
                target_type = serialize_type_name(runtime_target.GetType()),
                target_position = runtime_target.Position,
            };

            // If the runtime target points to a loaded capable, keep the id for better restore fidelity.
            if (runtime_target is TransformTarget transform_target && transform_target.Transform != null)
            {
                Capable capable = transform_target.Transform.GetComponentInParent<Capable>(includeInactive: true);
                if (capable != null && capable.data != null)
                {
                    target_data.target_capable_id = capable.data.id;
                }
            }

            motor_data.local_world_targets.Add(target_data);
        }
    }

    private void hydrate_provider_world_data(MotorData motor_data)
    {
        if (motor_data == null) { return; }
        if (Provider == null || Provider.WorldData == null) { return; }

        for (int i = 0; i < motor_data.local_world_state.Count; i++)
        {
            LocalWorldStateData saved_state = motor_data.local_world_state[i];
            if (saved_state == null || string.IsNullOrWhiteSpace(saved_state.key_name)) { continue; }

            Type world_key_type = resolve_type(saved_state.key_name);
            if (world_key_type == null)
            {
                if (log) { Debug.LogWarning($"(MotorCapacity) Failed to hydrate world state key '{saved_state.key_name}' for {data.owner_id}"); }
                continue;
            }

            Provider.WorldData.SetState(world_key_type, saved_state.key_value);
        }

        for (int i = 0; i < motor_data.local_world_targets.Count; i++)
        {
            LocalWorldTargetData saved_target = motor_data.local_world_targets[i];
            if (saved_target == null || string.IsNullOrWhiteSpace(saved_target.key_name)) { continue; }

            Type target_key_type = resolve_type(saved_target.key_name);
            if (target_key_type == null)
            {
                if (log) { Debug.LogWarning($"(MotorCapacity) Failed to hydrate target key '{saved_target.key_name}' for {data.owner_id}"); }
                continue;
            }

            if (!typeof(ITargetKey).IsAssignableFrom(target_key_type))
            {
                if (log) { Debug.LogWarning($"(MotorCapacity) Target key type '{target_key_type.FullName}' is not an ITargetKey for {data.owner_id}"); }
                continue;
            }

            ITargetKey key_instance = Activator.CreateInstance(target_key_type) as ITargetKey;
            if (key_instance == null)
            {
                if (log) { Debug.LogWarning($"(MotorCapacity) Failed to instantiate target key '{target_key_type.FullName}' for {data.owner_id}"); }
                continue;
            }

            ITarget runtime_target = build_runtime_target(saved_target);
            if (runtime_target == null) { continue; }

            Provider.WorldData.SetTarget(key_instance, runtime_target);
        }
    }

    private ITarget build_runtime_target(LocalWorldTargetData saved_target)
    {
        if (saved_target == null) { return null; }

        if (!string.IsNullOrWhiteSpace(saved_target.target_capable_id))
        {
            Capable capable = CapableBank.Instance?.GetLoadedCapable(saved_target.target_capable_id);
            if (capable != null)
            {
                return new TransformTarget(capable.transform);
            }
        }

        return new PositionTarget(saved_target.target_position);
    }

    private static string serialize_type_name(Type type)
    {
        if (type == null) { return string.Empty; }
        return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
    }

    private static Type resolve_type(string type_name)
    {
        if (string.IsNullOrWhiteSpace(type_name)) { return null; }

        Type resolved = Type.GetType(type_name);
        if (resolved != null) { return resolved; }

        // legacy fallback: simple key names that were previously saved without namespace/assembly.
        resolved = Type.GetType($"subrunner.goap.{type_name}");
        if (resolved != null) { return resolved; }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            resolved = assemblies[i].GetType(type_name);
            if (resolved != null) { return resolved; }
        }

        for (int i = 0; i < assemblies.Length; i++)
        {
            Type[] types;
            try
            {
                types = assemblies[i].GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            if (types == null) { continue; }

            for (int j = 0; j < types.Length; j++)
            {
                Type type = types[j];
                if (type == null) { continue; }
                if (type.Name == type_name) { return type; }
            }
        }

        return null;
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