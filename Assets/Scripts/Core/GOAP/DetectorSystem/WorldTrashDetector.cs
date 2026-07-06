using System.Collections.Generic;
using subrunner.goap;
using UnityEngine;
using UnityEngine.AI;

public class WorldTrashDetector : MonoBehaviour, TrashDetector
{
    [Header("Logs")]
    public bool log = false;
    public bool draw_gizmos = false;

    public T FindClosestCapableOfType<T>(CapableData looker_data) where T : Capable
    {
        throw new System.NotImplementedException();
    }


    // MAIN METHODS
    public ItemData FindClosestTrash(CapableData cdata, bool force_loaded = false)
    {
        List<ItemData> trashes = get_potential_trashes(cdata);
        if (trashes == null) { return null; }

        // then we filter the trashes to check if we have one that can be picked up by the capable
        ItemData closest_trash = null;
        float closest_distance = float.MaxValue;
        foreach (ItemData trash in trashes)
        {
            float distance = Vector3.Distance(trash.position, cdata.position);
            if (!(distance < closest_distance)) { continue; }

            // we filter the force_loaded & interact data type
            if (force_loaded && trash.Capable == null) { continue; }
            // if (!trash.ValidateRule(idata.item_rule)) { continue; }

            closest_trash = trash;
            closest_distance = distance;
        }
        return closest_trash;
    }
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
    public ItemData FindClosestInteractableTrash(CapableData cdata, InteractData idata, bool force_loaded = false)
    {
        if (!idata.interact_types.Contains(InteractType.Item)) { return null; }

        List<ItemData> trashes = get_potential_trashes(cdata);
        if (trashes == null) { return null; }

        // then we filter the trashes to check if we have one that can be picked up by the capable
        ItemData closest_trash = null;
        float closest_distance = float.MaxValue;
        foreach (ItemData trash in trashes)
        {
            // we filter the force_loaded & interact data type
            if (force_loaded && trash.Capable == null) { continue; }
            if (!trash.ValidateRule(idata.item_rule)) { continue; }

            // we also make sure we can reach the target
            if (!NavMesh.SamplePosition(trash.Position, out NavMeshHit hit, 0.3f, Filter))
            {
                // Debug.Log($"(WorldTrashDetector) {trash.id} is NOT reachable from {cdata.id}".AddColor(Color.red));
                continue;
            }

            // Debug.Log($"(WorldTrashDetector) {trash.id} IS reachable from {cdata.id}".AddColor(Color.green));
            float distance = Vector2.Distance(cdata.position,trash.position);
            if (!(distance < closest_distance)) { continue; }

            closest_trash = trash;
            closest_distance = distance;
        }
        return closest_trash;
    }
    /* private void draw_gizmo_along_path(NavMeshPath path, Color color)
    {
        if (!draw_gizmos) { return; }

        Vector3[] corners = path.corners;
        for (int i = 1; i < corners.Length; i++)
        {
            Debug.DrawLine(corners[i - 1], corners[i], color, 10f);
        }
    } */


    // LOWER LEVEL METHODS
    private List<ItemData> get_potential_trashes(CapableData cdata)
    {
        string level_id = "";

        // we get the level of the ia
        if (cdata.Capable != null) { level_id = LevelEngine.Instance.CurrentLevelID; }
        else if (LevelEngine.Instance.TryGetCapableLevel(cdata.id, out LevelData level)) { level_id = level.id; }
        else
        {
            Debug.LogError($"(WorldTrashDetector) Can't find closest trash for capable '{cdata.id}' bcz capable is not in any level ???");
            return null;
        }

        CapableEngine.TrashEngine.GetTrashesOfLevel(level_id, out List<ItemData> trashes);
        if (trashes == null)
        {
            if (log) { Debug.LogWarning($"(WorldTrashDetector) '{cdata.id}' found no trash in level '{level_id}'"); }
            return null;
        }
        return trashes;
    }

}