using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CapableSystem : BSOD_System<CapableSystem>
{

    [Header("Capables data")]
    private string data_path = "data/capables/";
    public Dictionary<string, CapableData> capables_data = new Dictionary<string, CapableData>();
    
    [Header("Runtime IDs (hashs)")]
    private Dictionary<string, int> capables_hashs_by_ids = new Dictionary<string, int>();
    private Dictionary<int, string> capables_ids_by_hash = new Dictionary<int, string>();
    private int next_capable_hash = 1; // we start at 1 because 0 is the default value for non hashables (null, empty id, etc)

    [Header("Loading / Unloading")]
    public Dictionary<string,CapableData> loaded_capables_data = new Dictionary<string,CapableData>();
    public List<string> loading_queue = new List<string>();
    public List<string> unloading_queue = new List<string>();

    [Header("State")]
    public bool awake_done = false;
    public bool start_loading_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;
    public bool log_awake_data_extended = false;
    public bool log_loading = false;
    public bool log_loading_extended = false;
    public bool hide_log_no_data_found = false;
    public bool log_spawning = false;
    public int load_x_capables_per_frame = 1;


    // EVENTS
    public Action<Capable, Capable> OnCapableNeedRoom; // we pass the spawned capable's data and the spawner capable (can be null)
    public Action<Capable, Capable> OnCapableNeedFreedom;



    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load capables data
        loadCapablesData();

        // detect capables that are not linked to a capable data in our system
        // and create an hash if it has an id
        detectCapablesOutsideOfSystem();
    }

    // LOAD / UNLOAD DATA
    protected void loadCapablesData()
    {
        // we empty the capables_data & runtime ids etc
        capables_data = new Dictionary<string,CapableData>();
        capables_hashs_by_ids = new Dictionary<string, int>();
        capables_ids_by_hash = new Dictionary<int, string>();
        next_capable_hash = 1;
        string log_capables_details = "\n\n";


        // we load all the json files in the data path and get their kind
        string[] files = GameManager.Instance.LoadJsons(data_path);
        Dictionary<string, List<string>> json_by_kind = new Dictionary<string, List<string>>();
        foreach (string json in files)
        {
            CapableData data = JsonUtility.FromJson<CapableData>(json);

            if (json_by_kind.ContainsKey(data.kind))
            {
                json_by_kind[data.kind].Add(json);
            }
            else
            {
                json_by_kind.Add(data.kind, new List<string> { json });
            }
        }

        // then we go through all json & kind and we load the json with the good type
        foreach (KeyValuePair<string, List<string>> entry in json_by_kind)
        {
            string kind = entry.Key;
            List<string> json_list = entry.Value;
            foreach (string json in json_list)
            {
                loadCapableDataOfType(json, kind, ref log_capables_details);
            }
        }

        if (log_awake_data) { Debug.Log("(CapableSystem) CAPABLES DATA LOADED : " + capables_data.Count + log_capables_details); }
        awake_done = true;
    }
    private void loadCapableDataOfType(string json, string kind, ref string log)
    {
        // find the data type suited for this capable_type
        // and extracts the json as this data type
        CapableData data;

        // first we check if we have a data for this precise kind
        Type data_type = Type.GetType(kind + "Data");
        if (data_type != null)
        {
            data = JsonUtility.FromJson(json, data_type) as CapableData;
            if (log_awake_data_extended) { Debug.Log($"(CapableSystem) Loading capable data : \n{data.GetDetails()}\n\n{json}"); }
            capables_data.Add(data.id, data);
            generate_runtime_id(data.id);

            log += data.GetDetails() + "\n";
            return;
        }
        
        // we found no precise data type ://
        // we check if we have an intermediary type
        // ex : ItemData
        // (insert in the list below)
        Type capable_type = Type.GetType(kind);
        
        // ItemData
        if (GameManager.Instance.IsKind(capable_type, typeof(Item))) { data_type = typeof(ItemData); }
        
        // no intermediary type -> we give a CapableData, basic
        else { data_type = typeof(CapableData); }


        // we finally extract the data
        data = JsonUtility.FromJson(json, data_type) as CapableData;
        if (log_awake_data_extended) { Debug.Log($"(CapableSystem) Loading capable data : \n{data.GetDetails()}\n\n{json}"); }
        capables_data.Add(data.id, data);
        generate_runtime_id(data.id);
        log += data.GetDetails() + "\n";
    }
    private int generate_runtime_id(string id)
    {
        
        if (string.IsNullOrEmpty(id))
        {
            return 0;
        }

        // If already assigned, return existing value
        if (capables_hashs_by_ids.TryGetValue(id, out int existing))
        {
            return existing;
        }

        // Allocate new unique runtime id
        int new_hash = next_capable_hash++;
        capables_ids_by_hash[new_hash] = id;
        capables_hashs_by_ids[id] = new_hash;
        return new_hash;
        
    }

    // outside of system capable detection
    private void detectCapablesOutsideOfSystem()
    {
        // we check for all active capables in hierarchy
        Capable[] already_existing_capables = FindObjectsByType<Capable>(FindObjectsSortMode.None);
        List<string> detected_outsiders = new List<string>();
        for (int i=0; i<already_existing_capables.Length; i++)
        {
            Capable capable = already_existing_capables[i];

            // check if it has a data
            if (capable.data == null) { continue; }
            if (string.IsNullOrEmpty(capable.data.id)) { continue; }

            // checks if we have the data
            if (GetCapableHashFromID(capable.data.id) != 0) { continue; }

            // we have an outsider !!
            detected_outsiders.Add(capable.data.id);
            generate_runtime_id(capable.data.id);
        }

        if (log_awake_data && detected_outsiders.Count > 0) { Debug.Log("(CapableSystem) OUTSIDERS DETECTED : " + detected_outsiders.Count + "\n - " + string.Join("\n - ",detected_outsiders)); }
    }


    // LOAD CAPABLES
    /// <summary>
    /// this method is not loading the capables directly, but it adds them to
    /// the loading queue. then, update will cycle through the loading queue and load
    /// the capables :D
    /// </summary>
    /// <param name="capables_ids">the IDs of the capables to load</param>
    public void LoadCapables(List<string> capables_ids)
    {
        for (int i = 0; i < capables_ids.Count; i++)
        {
            string id = capables_ids[i];
            
            // we remove them from the unloading_queue if they are inside it (so no need for loading them, already loaded)
            if (unloading_queue.Contains(id)) { unloading_queue.Remove(id); }

            // else we add them to loading queue
            else { loading_queue.Add(id); }
        }
    }
    private void load_in_queue(int count)
    {
        if (loading_queue.Count <= 0) { return; }
        for (int i = 0; i < count; i++)
        {
            if (loading_queue.Count <= 0) { break; }
            string capable_id = loading_queue[0];
            load_capable(capable_id);
            loading_queue.RemoveAt(0);
        }
    }
    private Capable load_capable(string id)
    {
        if (!capables_data.ContainsKey(id))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - Load) Capable data not found for id: " + id); }
            return null;
        }
        CapableData data = capables_data[id];
        return load_capable(data);
    }
    private Capable load_capable(CapableData data)
    {
        Capable capable = CapableBank.Instance.Load(data);
        loaded_capables_data.Add(data.id, data);
        if (log_loading) { Debug.Log("(CapableSystem) Loaded " + data.id); }
        return capable;
    }
    public Capable LoadCapableInstantly(string id)
    {
        // if the capable is in the unloading queue, it means it is already loaded,
        // so we remove it from unloading queue and simply return it
        if (unloading_queue.Contains(id))
        {
            unloading_queue.Remove(id);
            Capable capable = CapableBank.Instance.GetLoadedCapable(id);
            if (log_loading) { Debug.Log("(CapableSystem) Already loaded " + id); }
            return capable;
        }

        // we remove the capable from loading queue
        if (loading_queue.Contains(id)) { loading_queue.Remove(id); }

        // and we finally load it
        return load_capable(id);
    }

    // UNLOAD CAPABLES
    /// <summary>
    /// same as LoadCapables(), this method is not unloading the capables directly, but it adds them to
    /// the unloading queue. then, update will cycle through the unloading queue and unload
    /// the capables :D
    /// </summary>
    /// <param name="capables_ids">the IDs of the capables to unload</param>
    public void UnloadCapables(List<string> capables_ids)
    {
        for (int i = 0; i < capables_ids.Count; i++)
        {
            string id = capables_ids[i];
            
            // we remove them from the loading_queue if they are inside it (so no need for unloading them)
            if (loading_queue.Contains(id)) { loading_queue.Remove(id); }

            // else we add them to unloading queue
            else { unloading_queue.Add(id); }
        }
    }
    private int unload_in_queue(int count)
    {
        if (unloading_queue.Count <= 0) { return 0; }
        List<Movable> movables_to_unregister = new List<Movable>();
        for (int i = 0; i < count; i++)
        {
            if (unloading_queue.Count <= 0) { return i; }
            string capable_id = unloading_queue[0];
            Capable capable = unload_capable(capable_id);
            unloading_queue.RemoveAt(0);

            // add to the unregistering list if movable
            if (capable != null && capable is Movable)
            {
                movables_to_unregister.Add(capable as Movable);
            }
        }

        // we call MovableEngine.UnregisterInBatch to remove all the capable we just disabled
        if (log_loading_extended && movables_to_unregister.Count > 0) { Debug.Log($"(CapableSystem - unload_in_queue) Calling MovableEngine.UnregisterInBatch for {movables_to_unregister.Count} entities : \n  - {(string.Join("\n  - ", movables_to_unregister))}"); }
        MovableEngine.Instance.UnregisterInBatch(movables_to_unregister);
        
        return count;
    }
    private Capable unload_capable(string id)
    {
        if (!loaded_capables_data.ContainsKey(id)) { Debug.LogWarning("(CapableSystem - Unload) Loaded capable data not found for id: " + id); return null; }
        CapableData data = loaded_capables_data[id];
        Capable capable = CapableBank.Instance.Unload(data);
        loaded_capables_data.Remove(id);
        if (log_loading) { Debug.Log("(CapableSystem) Unloaded " + id); }
        return capable;
    }


    // SPAWNING / DROPPING ITEMS
    public Capable SpawnCapable(string base_id, Capable spawner)
    {
        if (log_spawning) { Debug.Log($"(CapableSystem) Spawning {base_id} entity"); }

        // 1. we find base_id data & duplicates it
        if (!capables_data.ContainsKey(base_id)) { Debug.LogWarning("(CapableSystem - SpawnCapable) Capable data not found for id: " + base_id); return null; }
        CapableData base_data = capables_data[base_id];
        CapableData spawn_data = DuplicateData(base_data);

        if (log_spawning) { Debug.Log($"(CapableSystem) Duplicated {base_id} data to {spawn_data.id} \n {spawn_data.GetDetails()}"); }

        // 2. we load the new spawned capable
        Capable spawned_capable = load_capable(spawn_data);

        // 3. we alert the RoomSystem that we just spawned a capable, for it to assign a room to it
        OnCapableNeedRoom?.Invoke(spawned_capable, spawner);

        if (log_spawning) { Debug.Log($"(CapableSystem) Spawned {base_id} (new id : {spawn_data.id})"); }

        // 4. we return the spawned capable
        return spawned_capable;
    }
    public void OnItemDropped(Item item, Capable dropper)
    {
        // we simply inform the room system that we need a room for the item
        OnCapableNeedRoom?.Invoke(item, dropper);
    }
    public void OnItemGrabbed(Item item, Capable grabber)
    {
        // we simply inform the room system that we need to detach the item from the room
        OnCapableNeedFreedom?.Invoke(item, grabber);
    }


    // UPDATE
    private void Update()
    {
        if (!awake_done) { return; }
        
        // we load / unload in queue
        int unloaded = unload_in_queue(load_x_capables_per_frame);
        load_in_queue(load_x_capables_per_frame - unloaded);
    }



    // GETTERS
    public int GetCapableHashFromID(string id)
    {
        return capables_hashs_by_ids.TryGetValue(id, out int hash) ? hash : 0;
    }
    public string GetCapableIDFromHash(int hash)
    {
        return capables_ids_by_hash.TryGetValue(hash, out string id) ? id : null;
    }


    // DATA MANAGMENT
    private CapableData DuplicateData(ICapableData base_data)
    {
        ICapableData new_data = base_data.Duplicate();
        new_data.id = GameManager.Instance.GenerateUniqueID(base_data.id);

        // generate a hash
        generate_runtime_id(new_data.id);

        // we add the new_data to the data list
        capables_data.Add(new_data.id, new_data as CapableData);
        return new_data as CapableData;
    }
}