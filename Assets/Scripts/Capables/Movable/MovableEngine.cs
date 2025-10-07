using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Linq;
using subrunner.goap;

public class MovableEngine : MonoBehaviour
{
    [Header("MOVABLES")]
    public List<Movable> movables = new List<Movable>();

    // on stocke une matrice contenant les differentes distances entre les agents
    private HalfMatrix<float> distances_matrix;

    [Header("Job Data")]
    private NativeArray<float3> movablePositions;
    private NativeArray<float> movableDistances;
    private NativeList<int> neighbours_indexes;
    private NativeList<MovableStruct> neighbours_structs;


    [Header("Logs")]
    public bool log_movables = false;
    public bool log_arrays = false;
    public bool log_neighbours = false;

    // AWAKE
    public static MovableEngine Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
        Debug.Log("(MovableEngine) Initialized");

        // on initialise les matrices
        distances_matrix = new HalfMatrix<float>();
        movablePositions = new NativeArray<float3>(0, Allocator.Persistent);
        movableDistances = new NativeArray<float>(0, Allocator.Persistent);
        neighbours_indexes = new NativeList<int>(Allocator.Persistent);
        neighbours_structs = new NativeList<MovableStruct>(Allocator.Persistent);
    }


    // REGISTER / UNREGISTER MOVABLES
    public void Register(Movable movable)
    {
        if (movables.Contains(movable)) { return; }

        movables.Add(movable);
        if (log_movables) { Debug.Log($"(MovableEngine) Registered movable: {movable.name}. Total movables: {movables.Count}"); }

        // on resize les matrices
        distances_matrix.AddAgent();
        movablePositions.ResizeArray(movables.Count);
        movableDistances.ResizeArray(distances_matrix.DataCount);
        if (log_arrays) { Debug.Log($"(MovableEngine) Resized arrays: movablePositions length {movablePositions.Length}, movableDistances length {movableDistances.Length}, distances_matrix data count {distances_matrix.DataCount}"); }

    }
    public void Unregister(Movable movable)
    {
        if (!movables.Contains(movable)) { return; }
        // on verifie qu'on a pas déjà disposé la mémoire (ce qui veut dire qu'on quitte le jeu)
        if (!movableDistances.IsCreated || !movablePositions.IsCreated || distances_matrix == null) { return; }

        // on resize la matrice de distances
        string log = "";
        distances_matrix.RemoveAgent(movables.IndexOf(movable), ref log);

        // on supprime le movable
        movables.Remove(movable);
        if (log_movables) { Debug.Log($"(MovableEngine) Unregistered movable: {movable.name}. Total movables: {movables.Count} + \n\n{log}"); }

        // on resize les arrays
        movablePositions.Dispose();
        movablePositions = new NativeArray<float3>(movables.Count, Allocator.Persistent);
        movableDistances.Dispose();
        movableDistances = new NativeArray<float>(distances_matrix.DataCount, Allocator.Persistent);
        if (log_arrays) { Debug.Log($"(MovableEngine) Resized arrays: movablePositions length {movablePositions.Length}, movableDistances length {movableDistances.Length}, distances_matrix data count {distances_matrix.DataCount}"); }

    }

    // UPDATE
    private void Update()
    {
        // on copie les données dans le movablePositions
        for (int i = 0; i < movables.Count; i++)
        {
            movablePositions[i] = movables[i].transform.position;
        }

        // we go through all the movables we have, and if they have a brain (ia), then we calculate their avoidance force
        foreach (Movable movable in movables)
        {
            if (movable is not IA ia) { continue; }
            GoToBehaviour mover = ia.Mover;
            if (mover == null) { continue; }
            Vector2 avoidance_force = CalculateAvoidanceForce(movable, mover.neighbour_radius, mover.ttc_treshold);
            mover.SetAvoidanceForce(avoidance_force);
            if (movable.log_avoidance) { Debug.Log($"(MovableEngine) {movable.name} has avoidance force {avoidance_force}"); }
        }

    }

    // AVOIDANCE FORCE CALCULATION
    public Vector2 CalculateAvoidanceForce(Movable agent, float neighbour_radius, float ttc_treshold = 3f)
    {
        // checks if the agent has a brain and has an attack target -> we exclude the target from the avoidance force calculation
        // since we want to collide with it

        NativeList<int> excludeIndexes = new NativeList<int>(Allocator.TempJob);
        if (agent is IA ia && ia.Brain.currentActionData is AttackAction.Data attackData && attackData.BeingTarget != null)
        {
            int targetIndex = movables.IndexOf(attackData.BeingTarget);
            if (targetIndex != -1) { excludeIndexes.Add(targetIndex); }
            if (agent.log_avoidance) { Debug.Log($"(MovableEngine) {agent.name} tried excluding {attackData.BeingTarget.name} from ttc (and {((targetIndex != -1) ? "succeeded" : "failed")})"); }
        }

        // get the neighbours
        cache_neighbours_indexes(agent, neighbour_radius, excludeIndexes);
        cache_neighbours_movable_structs();

        // then we put all the values to the job
        NativeArray<float2> output = new NativeArray<float2>(1, Allocator.TempJob);

        // log
        if (agent.log_avoidance)
        {
            string log = $"(MovableEngine) Calculating avoidance force for {agent.name} with {neighbours_structs.Length} neighbours: ";
            foreach (MovableStruct ms in neighbours_structs) { log += $"\n - Neighbour {movables[ms.id].name} at {ms.position} with velocity {ms.velocity}"; }
            Debug.Log(log);
        }

        // create the job
        CalculateAvoidanceForceJob avoidJob = new CalculateAvoidanceForceJob
        {
            movable = new MovableStruct
            {
                position = (float2)(Vector2)agent.transform.position,
                velocity = (float2)agent.Velocity,
                feet_radius = agent.feet_radius,
                is_item = agent is Item
            },
            neighbours = neighbours_structs,
            ttc_treshold = ttc_treshold,
            avoidanceForces = output
        };

        // launches the job
        JobHandle avoidHandle = avoidJob.Schedule();
        avoidHandle.Complete();

        // get the final avoidance force
        float2 avoidance_force = output[0];

        // Dispose to free memory
        output.Dispose();
        excludeIndexes.Dispose();

        // return
        return (Vector2)avoidance_force;
    }

    // NEIGHBOURS CALCULATION
    public List<Movable> GetNeighbours(Movable agent, float maxDistance = 1f)
    {
        // we cache the neighbours indexes in the neighbour_indexes array
        cache_neighbours_indexes(agent, maxDistance);

        // check that we have an array
        if (!neighbours_indexes.IsCreated) { return new List<Movable>(); }

        // convert back the indexes to a list
        List<Movable> neighbours = new List<Movable>();
        for (int i = 0; i < neighbours_indexes.Length; i++)
        {
            neighbours.Add(movables[neighbours_indexes[i]]);
        }
        return neighbours;
    }
    private void cache_neighbours_indexes(Movable agent, float maxDistance = 1f, NativeList<int> excludeIndexes = default)
    {
        if (!neighbours_indexes.IsCreated) { return; }
        neighbours_indexes.Clear();
        int agentIndex = movables.IndexOf(agent);

        // create a new job
        FindNeighboursJob neighJob = new FindNeighboursJob
        {
            MovablePositions = movablePositions,
            excludedIndexes = excludeIndexes,
            neighbours = neighbours_indexes,
            movableIndex = agentIndex,
            maxDistanceSq = maxDistance * maxDistance,
        };

        // launches it
        JobHandle neighHandle = neighJob.Schedule();
        neighHandle.Complete();
    }
    private void cache_neighbours_movable_structs()
    {
        if (!neighbours_structs.IsCreated) { return; }
        neighbours_structs.Clear();
        foreach (int neighbourIndex in neighbours_indexes)
        {
            Movable neighbour = movables[neighbourIndex];
            if (neighbour.feet_collider == null) { continue; } // we only consider movables with feet colliders
            if (neighbour is Item item && item.Grabbed) { continue; } // we skip held items
            neighbours_structs.Add(new MovableStruct
            {
                id = neighbourIndex,
                position = (float2)(Vector2)neighbour.transform.position,
                velocity = (float2)neighbour.Velocity,
                feet_radius = neighbour.feet_radius,
                is_item = neighbour is Item
            });
        }
    }

    // ON DESTROY
    public void OnDestroy()
    {
        movablePositions.Dispose();
        neighbours_indexes.Dispose();
        movableDistances.Dispose();
        neighbours_structs.Dispose();
        distances_matrix = null;
    }
}

