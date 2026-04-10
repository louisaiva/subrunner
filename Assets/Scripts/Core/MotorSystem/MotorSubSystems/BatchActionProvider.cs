using System;
using System.Collections.Generic;
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


    // resolve handles
    private readonly List<RunningResolveHandle> resolveHandles = new();



    [Header("Logs")]
    [SerializeField] private bool hide_log_resolves_still_pending = false;


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



    // ########################

    //     UPDATE & RUN / RESOLVE

    // ########################

    // UPDATE
    private void Update()
    {
        // verify that we have no leftover resolve handles that are not completed
        if (resolveHandles.Count > 0)
        {
            if (!hide_log_resolves_still_pending) { Debug.LogWarning($"(BatchActionProvider) There are {resolveHandles.Count} resolve handles that are not completed. waiting next frame to Run()"); }
            return;
        }

        // here we could check timers (tick-based update)
        
        // and select the entities we need to resolve (dirty-based batching + max batch size)

        // we then call the run method to resolve for the selected batch this frame
        Run();
    }

    // RUN (RESOLVE ACTION)
    public void Run()
    {
        resolveHandles.Clear();

        // here we will call all the Resolve() methods for each unloaded entity in the game
    }
    private readonly int[] goalIndexes = new int[20]; // we reuse this array to avoid allocations in get_goals_indexes
    public void Resolve(MotorData mdata, Vector2 position)
    {
        // here we only resolve the action for one entity
        if (!agents_resolvers.ContainsKey(mdata.agent_type)) { return; }

        // we get the resolver for the agent type
        AgentResolver resolver = agents_resolvers[mdata.agent_type];

        // we fill the builders with the current world data and motor data
        fill_builders(resolver, null, mdata, position);

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
            AgentPosition = new float3(position, 0f),
            IsEnabled = new NativeArray<bool>(resolver.enabled_builder.Build(), Allocator.TempJob),
            IsExecutable = new NativeArray<bool>(resolver.executable_builder.Build(), Allocator.TempJob),
            Positions = new NativeArray<float3>(resolver.position_builder.Build(), Allocator.TempJob),
            Costs = new NativeArray<float>(resolver.cost_builder.Build(), Allocator.TempJob),
            ConditionsMet = new NativeArray<bool>(resolver.condition_builder.Build(), Allocator.TempJob),
            DistanceMultiplier = 1f
        };

        // next we create the resolve handle for the resolver
        IResolveHandle handle = resolver.graph.StartResolve(run_data);
        resolveHandles.Add(new RunningResolveHandle { handle = handle, mdata = mdata });
    }

    // get goals indexes
    private void fill_builders(AgentResolver resolver, IWorldData wdata, MotorData mdata, Vector2 position)
    {
        var conditionObserver = resolver.agent_type.GoapConfig.ConditionObserver;
        conditionObserver.SetWorldData(wdata);

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

            var target = wdata.GetTarget(node);

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
            resolveHandle.handle.Complete();

            // here we will get the result of the resolve and pass it to the BatchActionAchiever to achieve the actions for the entity
        }
        resolveHandles.Clear();
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
    public MotorData mdata;
}