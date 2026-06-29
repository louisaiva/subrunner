using System.Collections.Generic;
using UnityEngine;

public class TrashEngine : MonoBehaviour
{

    [SerializeField] private string trash_rule = "corpse,leftover";

    [Header("Logs")]
    [SerializeField] private bool log_trash = false;

    private Dictionary<string,List<ItemData>> on_floor_trashes = new Dictionary<string, List<ItemData>>();


    // LOAD & CLEAR CACHE
    public void GatherExistingTrashes(ref Dictionary<string, CapableData> world_capables, bool log)
    {
        foreach (KeyValuePair<string, CapableData> kvp in world_capables)
        {
            if (kvp.Value is not ItemData item) { continue; }
            if (item.is_grabbed) { continue; }
            if (!is_item_a_trash(item)) { return; }

            // try to extract the level of it
            if (!LevelEngine.LazyInstance.TryGetCapableLevel(item.id, out LevelData level)) { continue; }
            add_trash_to_level(item, level.id);
        }
    }
    public void ClearCache(bool log)
    {
        on_floor_trashes.Clear();
        if (log) { Debug.Log($"(TrashEngine) Cache cleared"); }
    }


    // ON ENABLE / DISABLE
    private void OnEnable()
    {
        ChunkEngine.LazyInstance.OnCapableAddedToChunk += on_capable_appeared;
        ChunkEngine.LazyInstance.OnCapableRemovedFromChunk += on_capable_disappeared;
    }
    private void OnDisable()
    {
        ChunkEngine.LazyInstance.OnCapableAddedToChunk -= on_capable_appeared;
        ChunkEngine.LazyInstance.OnCapableRemovedFromChunk -= on_capable_disappeared;
    }


    // CALLBACKS
    private void on_capable_appeared(string id, ChunkData chunk)
    {
        // if (log_trash) { Debug.Log($"(TrashEngine) Detected appearing capable '{id}' in chunk {chunk.id}"); }
        CapableData cdata = CapableEngine.LazyInstance.GetCapableDataFromID(id);
        if (cdata == null) { return; }
        if (cdata is not ItemData item) { return; }
        
        if (!is_item_a_trash(item)) { return; }

        // we get the level of the cdata
        if (!LevelEngine.Instance.TryGetRoomLevel(chunk.room_id, out LevelData level)) { return; }

        // we add the item to the level item list
        add_trash_to_level(item, level.id);
    }
    private void add_trash_to_level(ItemData item, string level_id)
    {
        if (!on_floor_trashes.ContainsKey(level_id))
        {
            on_floor_trashes[level_id] = new List<ItemData>();
        }
        else if (on_floor_trashes[level_id].Contains(item)) { return; }
        on_floor_trashes[level_id].Add(item);
        if (log_trash) { Debug.Log($"(TrashEngine) Trash '{item.id} just appeared on '{level_id}' level"); }
    }
    private void on_capable_disappeared(string id, ChunkData chunk)
    {
        // if (log_trash) { Debug.Log($"(TrashEngine) Detected disappearing capable '{cdata.id}' : {cdata.GetDetails()}"); }
        CapableData cdata = CapableEngine.LazyInstance.GetCapableDataFromID(id);
        if (cdata == null) { return; }
        if (cdata is not ItemData item) { return; }
        if (!LevelEngine.Instance.TryGetRoomLevel(chunk.room_id, out LevelData level)) { return; }
        if (!on_floor_trashes.ContainsKey(level.id)) { return; }
        if (!on_floor_trashes[level.id].Contains(item)) { return; }

        on_floor_trashes[level.id].Remove(item);
        if (log_trash) { Debug.Log($"(TrashEngine) Trash '{item.id}' was cleaned on '{level.id}' level"); }

        if (on_floor_trashes[level.id].Count > 0) { return; }
        on_floor_trashes.Remove(level.id);
        if (log_trash) { Debug.Log($"(TrashEngine) Level '{level.id}' is now fully clean !!! good job npc"); }
    }



    // GETTERS
    private bool is_item_a_trash(ItemData item)
    {
        return item.ValidateRule(trash_rule);
    }
    public int GetQuantityOfTrash(string level_id)
    {
        if (!on_floor_trashes.TryGetValue(level_id, out List<ItemData> trashes)) { return 0; }
        return trashes.Count;
    }
    public void GetTrashesOfLevel(string level_id, out List<ItemData> trashes)
    {
        if (!on_floor_trashes.ContainsKey(level_id))
        {
            trashes = null;
            return;
        }
        trashes = on_floor_trashes[level_id];
    }
}