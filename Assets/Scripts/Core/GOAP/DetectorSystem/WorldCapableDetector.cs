using System.Collections.Generic;
using subrunner.goap;
using UnityEngine;
using UnityEngine.AI;

public class WorldCapableDetector : MonoBehaviour, CapableDetector
{
    // MAIN METHODS
    private NavMeshQueryFilter? _filter;
    public NavMeshQueryFilter Filter
    {
        get
        {
            if (_filter == null)
            {
                _filter = new NavMeshQueryFilter { areaMask = NavMesh.AllAreas, agentTypeID = GoToBehaviour.GetNavMeshAgentTypeID("humanoid") };
            }
            return _filter.Value;
        }
    }

    public CapableData ComputeClosest(CapableData cdata, List<CapableData> capables, bool force_loaded = false)
    {
        CapableData closest = null;
        float closest_distance = float.MaxValue;
        foreach (CapableData potential_target in capables)
        {
            // we filter the force_loaded
            if (force_loaded && potential_target.Capable == null) { continue; }

            // we also make sure we can reach the target
            if (!NavMesh.SamplePosition(potential_target.Position, out NavMeshHit hit, 0.3f, Filter)) { continue; }

            // Debug.Log($"(WorldCapableDetector) {trash.id} IS reachable from {cdata.id}".AddColor(Color.green));
            float distance = Vector2.Distance(cdata.position, potential_target.position);
            if (!(distance < closest_distance)) { continue; }

            closest = potential_target;
            closest_distance = distance;
        }
        return closest;
    }
    public List<CapableData> FindAll<T>(CapableData cdata, bool force_loaded = false) where T : Capable
    {
        return CapableEngine.Instance.GetWorldCapableByKind(typeof(T));
    }
    public CapableData FindClosest<T>(CapableData cdata, bool force_loaded = false) where T : Capable
    {
        List<CapableData> capables = CapableEngine.Instance.GetWorldCapableByKind(typeof(T));
        if (capables == null) { return null; }
        return ComputeClosest(cdata, capables, force_loaded);
    }
}