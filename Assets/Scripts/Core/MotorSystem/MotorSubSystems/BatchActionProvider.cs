using System;
using System.Collections.Generic;
using System.Linq;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Resolver;
using CrashKonijn.Goap.Runtime;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BatchActionProvider : MonoBehaviour
{
    private GoapBehaviour _goap_behaviour;
    private GoapBehaviour GoapBehaviour
    {
        get
        {
            if (_goap_behaviour is null)
            {
                _goap_behaviour = GameObject.FindFirstObjectByType<GoapBehaviour>();
            }
            return _goap_behaviour;
        }
    }

    // CACHED AGENT GRAPHS RESOLVERS & BUILDERS
    private Dictionary<string, AgentResolver> agents_resolvers = new();


    [Header("Entities waiting for resolve")]
    [SerializeField] private float resolve_interval = 0.5f;
    private float time_since_last_resolve = 0f;
    [SerializeField] private int max_entities_to_resolve_per_frame = 5;
    private HashSet<EntityMotor> pending_entities = new HashSet<EntityMotor>();
    private readonly List<RunningResolveHandle> resolveHandles = new();



    [Header("Logs - Tick")]
    [SerializeField] private bool log_pending_entities_management = false;
    [SerializeField] private bool log_tick_running_entities = false;
    [SerializeField] private bool hide_log_resolves_still_pending = false;

    [Header("Logs - Resolve Result")]
    [SerializeField] private bool log_action_found = false;
    [SerializeField] private bool hide_log_no_action_found = false;



    // ########################

    //     START & PENDING ENTITIES MANAGEMENT

    // ########################

    // START
    private void Start()
    {
        foreach (IAgentType type in GoapBehaviour.AgentTypes)
        {
            build_agent_graph(type);
        }
    }
    private void build_agent_graph(IAgentType type)
    {
        if (agents_resolvers.ContainsKey(type.Id)) { return; }

        // create a new resolver for the agent type
        AgentResolver resolver = new AgentResolver()
        {
            agent_type = type,
            graph = new GraphResolver(type.GetAllNodes().ToArray(), type.GoapConfig.KeyResolver),
        };

        // we also need to create the builders for the resolver
        resolver.enabled_builder = resolver.graph.GetEnabledBuilder();
        resolver.executable_builder = resolver.graph.GetExecutableBuilder();
        resolver.cost_builder = resolver.graph.GetCostBuilder();
        resolver.position_builder = resolver.graph.GetPositionBuilder();
        resolver.condition_builder = resolver.graph.GetConditionBuilder();

        // add the resolver to the cache
        agents_resolvers.Add(type.Id, resolver);
    }


    // PENDING ENTITIES MANAGEMENT
    public void RegisterForResolve(EntityMotor ia_and_motor_data)
    {
        pending_entities.Add(ia_and_motor_data);
        if (log_pending_entities_management) { Debug.Log($"(BatchActionProvider) Registered '{ia_and_motor_data.ia_data.id}' for resolve. Total pending entities: {pending_entities.Count}"); }
    }
    public void RemoveFromResolve(EntityMotor ia_and_motor_data)
    {
        pending_entities.Remove(ia_and_motor_data);
        if (log_pending_entities_management) { Debug.Log($"(BatchActionProvider) Removed '{ia_and_motor_data.ia_data.id}' from resolve. Total pending entities: {pending_entities.Count}"); }
    }

    // ########################

    //     UPDATE & RUN / RESOLVE

    // ########################

    // UPDATE
    private List<EntityMotor> running_entities = new();
    private void Update()
    {
        // verify that we have no leftover resolve handles that are not completed
        if (resolveHandles.Count > 0)
        {
            if (!hide_log_resolves_still_pending) { Debug.LogWarning($"(BatchActionProvider) There are {resolveHandles.Count} resolve handles that are not completed. waiting next frame to Run()"); }
            return;
        }

        // check timers (tick-based update)
        time_since_last_resolve += Time.deltaTime;
        if (time_since_last_resolve < resolve_interval) { return; }
        time_since_last_resolve = 0f;

        // and select the entities we need to resolve (dirty-based batching + max batch size)
        running_entities.Clear();
        foreach (EntityMotor item in pending_entities)
        {
            if (running_entities.Count >= max_entities_to_resolve_per_frame) { break; }
            running_entities.Add(item);
        }
        if (running_entities.Count == 0) { return; }

        // log
        if (log_tick_running_entities)
        {
            Debug.Log($"(BatchActionProvider) Running resolve for {running_entities.Count} entities this frame: " +
                $"{string.Join(", ", running_entities.Select(e => e.ia_data.id))}");
        }

        // we then call the run method to resolve for the selected entities batch this frame
        Run();
    }

    // RUN (RESOLVE ACTION)
    public void Run()
    {
        resolveHandles.Clear();

        // here we will call all the Resolve() methods for each unloaded entity in the game
        foreach (EntityMotor entity in running_entities)
        {
            Resolve(entity);
        }
    }
    private readonly int[] goalIndexes = new int[40]; // we reuse this array to avoid allocations in get_goals_indexes
    public void Resolve(EntityMotor entity)
    {
        MotorData mdata = entity.motor_data;
        IAData iadata = entity.ia_data;

        if (!agents_resolvers.ContainsKey(mdata.agent_type)) { return; }

        // we get the resolver for the agent type
        AgentResolver resolver = agents_resolvers[mdata.agent_type];

        // we fill the builders with the current world data and motor data
        fill_builders(resolver, entity);

        // we fill the goal indexes for the resolver
        int goalCount = get_goals_indexes(resolver.agent_type.GetGoals(), resolver, goalIndexes);
        if (goalCount <= 0) { return; }

        // we create the run data for the resolver

        // start indexes
        var startIndex = new NativeArray<int>(goalCount, Allocator.TempJob);
        for (int i = 0; i < goalCount; i++)
        {
            startIndex[i] = goalIndexes[i];
        }

        RunData run_data = new RunData
        {
            StartIndex = startIndex,
            AgentPosition = new float3(iadata.Position, 0f),
            IsEnabled = new NativeArray<bool>(resolver.enabled_builder.Build(), Allocator.TempJob),
            IsExecutable = new NativeArray<bool>(resolver.executable_builder.Build(), Allocator.TempJob),
            Positions = new NativeArray<float3>(resolver.position_builder.Build(), Allocator.TempJob),
            Costs = new NativeArray<float>(resolver.cost_builder.Build(), Allocator.TempJob),
            ConditionsMet = new NativeArray<bool>(resolver.condition_builder.Build(), Allocator.TempJob),
            DistanceMultiplier = 1f
        };

        // next we create the resolve handle for the resolver
        IResolveHandle handle = resolver.graph.StartResolve(run_data);
        resolveHandles.Add(new RunningResolveHandle { handle = handle, entity = entity });
    }


    // LOW LEVEL RESOLVE HELPER METHODS
    private void fill_builders(AgentResolver resolver, EntityMotor entity)
    {
        var conditionObserver = resolver.agent_type.GoapConfig.ConditionObserver;
        conditionObserver.SetWorldData(entity.world_data);

        resolver.enabled_builder.Clear();
        resolver.executable_builder.Clear();
        resolver.position_builder.Clear();
        resolver.condition_builder.Clear();

        foreach (var goal in resolver.agent_type.GetGoals())
        {
            // BrainEngine will determine the goal priority, so we don't need to set it here
            resolver.cost_builder.SetCost(goal, 1f/* goal.GetCost(actionProvider.Receiver, actionProvider.Receiver.Injector) */);

            foreach (var condition in goal.Conditions)
            {
                resolver.condition_builder.SetConditionMet(condition, conditionObserver.IsMet(condition));
            }
        }

        foreach (var node in resolver.agent_type.GetActions())
        {
            var allMet = true;

            foreach (var condition in node.Conditions)
            {
                if (!conditionObserver.IsMet(condition))
                {
                    allMet = false;
                    continue;
                }

                resolver.condition_builder.SetConditionMet(condition, true);
            }

            var target = entity.world_data.GetTarget(node);

            // for now we only check if the conditions are met to determine if we can execute the action,
            // but later we need to check if the action needs a target AND if so if the target is valid
            bool executable = allMet ? true /* node.IsExecutable(actionProvider.Receiver, allMet) */ : false;
            resolver.executable_builder.SetExecutable(node, executable);

            // we will have the disabled actions in the MotorData/BrainData later
            var isEnabled = true /* node.IsEnabled(actionProvider.Receiver) */;

            // same with costs
            var cost = isEnabled ? 1f /* node.GetCost(actionProvider.Receiver, actionProvider.Receiver.Injector, target) */ : 0f;

            resolver.enabled_builder.SetEnabled(node, isEnabled);
            resolver.cost_builder.SetCost(node, cost);

            resolver.position_builder.SetPosition(node, target?.GetValidPosition());
        }
    }
    private int get_goals_indexes(List<IGoal> goals, AgentResolver resolver, int[] goals_indexes)
    {
        Array.Clear(goals_indexes, 0, goals_indexes.Length);

        int count = 0;
        for (int i = 0; i < goals.Count; i++)
        {
            // we need to check wheter the goal is completed or not
            /* if (this.IsGoalCompleted(actionProvider, goals[i]))
                continue; */

            if (i >= goals_indexes.Length) { break; }

            int index = resolver.graph.GetIndex(goals[i]);
            if (index < 0) { continue; }

            goals_indexes[count] = index;
            count++;
        }
        return count;
    }







    // ########################

    //     LATEUPDATE & COMPLETE

    // ########################

    // LATE UPDATE
    private void LateUpdate() { Complete(); }

    // COMPLETE
    public void Complete()
    {
        foreach (var resolveHandle in resolveHandles)
        {
            // here we will get the result of the resolve and pass it to the BatchActionAchiever to achieve the actions for the entity
         
            var result = resolveHandle.handle.Complete();

            // check resulting goal
            var goal = result.Goal;
            if (goal == null)
            {
                no_action_found(resolveHandle);
                continue;
            }

            // check resulting action
            var action = result.Actions.FirstOrDefault() as IGoapAction;
            if (action is null)
            {
                no_action_found(resolveHandle);
                continue;
            }

            // we got a valid goal and action -> we can pass it to the BatchActionAchiever
            action_found(resolveHandle, goal, action);/* 
            if (action != resolveHandle.ActionProvider.Receiver.ActionState.Action)
                resolveHandle.ActionProvider.SetAction(new GoalResult
                {
                    Goal = goal,
                    Plan = result.Actions,
                    Action = action
                }); */
        }
        resolveHandles.Clear();
    }


    // ACTIONS FOUND / NOT FOUND
    private void no_action_found(RunningResolveHandle resolveHandle)
    {
        if (!hide_log_no_action_found) { Debug.LogWarning($"(BatchActionProvider) No action found for entity '{resolveHandle.entity.ia_data.id}' with agent type '{resolveHandle.entity.motor_data.agent_type}'"); }
        
        // we do nothing (will try to resolve again and again)
    }

    private void action_found(RunningResolveHandle resolveHandle, IGoal goal, IGoapAction action)
    {
        if (log_action_found) { Debug.Log($"(BatchActionProvider) Action '{action}' found for entity '{resolveHandle.entity.ia_data.id}' with agent type '{resolveHandle.entity.motor_data.agent_type}' to achieve goal '{goal}'"); }

        // we pass the action & goal to the BatchActionAchiever
        // blablabla

        // we remove the entity from the pending list since we found an action for it
        RemoveFromResolve(resolveHandle.entity);
    }



    // ########################

    //     ONDESTROY/DISABLE & DISPOSE

    // ########################


    // ONDISABLE & ONDESTROY
    private void OnDisable() { Dispose(); }
    private void OnDestroy() { Dispose(dispose_graphs: true); }

    // DISPOSE
    public void Dispose(bool dispose_graphs = false)
    {
        foreach (var resolveHandle in resolveHandles)
        {
            resolveHandle.handle.Complete();
        }
        resolveHandles.Clear();


        // we also dispose the graphs if needed
        if (!dispose_graphs) { return; }
        foreach (var resolver in agents_resolvers.Values)
        {
            resolver.graph.Dispose();
        }
    }
}

public class AgentResolver
{
    public IAgentType agent_type;
    public IGraphResolver graph;

    // run data builders
    public IEnabledBuilder enabled_builder;
    public IExecutableBuilder executable_builder;
    public ICostBuilder cost_builder;
    public IPositionBuilder position_builder;
    public IConditionBuilder condition_builder;    
}

public class RunningResolveHandle
{
    public IResolveHandle handle;
    public EntityMotor entity;
}