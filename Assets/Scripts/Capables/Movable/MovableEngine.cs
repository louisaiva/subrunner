using UnityEngine;
using System.Collections.Generic;

public class MovableEngine : MonoBehaviour
{
    [Header("MOVABLES")]
    public List<Movable> movables = new List<Movable>();

    // on stocke une matrice contenant les differentes distances entre les agents
    private HalfMatrix<float> distances_matrix = new HalfMatrix<float>();


    [Header("Logs")]
    public bool log_movables = false;
    public bool log_neighbours = false;

    // AWAKE
    public static MovableEngine Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
        Debug.Log("(MovableEngine) Initialized");
    }
    public void Register(Movable movable)
    {
        if (movables.Contains(movable)) { return; }

        movables.Add(movable);

        // on resize la matrice de distances
        distances_matrix.AddAgent();

        if (log_movables) { Debug.Log($"(MovableEngine) Registered movable: {movable.name}. Total movables: {movables.Count}"); }
    }
    public void Unregister(Movable movable)
    {
        if (!movables.Contains(movable)) { return; }

        // on resize la matrice de distances
        string log = "";
        distances_matrix.RemoveAgent(movables.IndexOf(movable), ref log);

        // on supprime le movable
        movables.Remove(movable);

        if (log_movables) { Debug.Log($"(MovableEngine) Unregistered movable: {movable.name}. Total movables: {movables.Count} + \n\n{log}"); }
    }


    // UPDATE
    private void Update()
    {
        // on met a jour la matrice des distances
        for (int i = 0; i < movables.Count; i++)
        {
            for (int j = i + 1; j < movables.Count; j++)
            {
                float distance = Vector2.Distance(movables[i].transform.position, movables[j].transform.position);
                distances_matrix[i, j] = distance;
            }
        }
    }

    // GETTERS
    public float GetDistance(Movable a, Movable b)
    {
        int indexA = movables.IndexOf(a);
        int indexB = movables.IndexOf(b);
        if (indexA == -1 || indexB == -1)
        {
            Debug.LogError($"(MovableEngine) GetDistance: One of the movables {a.name} or {b.name} not registered in MovableEngine");
            return float.MaxValue;
        }
        return distances_matrix[indexA, indexB];
    }
    public List<Movable> GetNeighbours(Movable agent, float maxDistance = 1f)
    {
        List<Movable> neighbours = new List<Movable>();
        foreach (var movable in movables)
        {
            if (movable == agent) { continue; }

            // we add this movable to the agent's neighbours if it's inside the circle of radius maxDistance
            if (GetDistance(agent, movable) <= maxDistance) { neighbours.Add(movable); }
        }

        if (log_neighbours)
        {
            string neighbour_names = string.Join(", ", neighbours.ConvertAll(n => n.name));
            Debug.Log($"(MovableEngine) Neighbours of {agent.name} within {maxDistance} units: ({neighbours.Count}) {neighbour_names}");
        }

        return neighbours;
    }
}

