using System.Collections.Generic;
using UnityEngine;

public class WorldTrashDetector : MonoBehaviour, TrashDetector
{
    [Header("Logs")]
    public bool log = false;

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
            float distance = Vector3.Distance(trash.position, cdata.position);
            if (!(distance < closest_distance)) { continue; }

            // we filter the force_loaded & interact data type
            if (force_loaded && trash.Capable == null) { continue; }
            if (!trash.ValidateRule(idata.item_rule)) { continue; }

            closest_trash = trash;
            closest_distance = distance;
        }
        return closest_trash;
    }



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