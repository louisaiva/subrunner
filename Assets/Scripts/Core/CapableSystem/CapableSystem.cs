using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CapableSystem : BSOD_System<CapableSystem>
{

    [Header("Capables data")]
    private string data_path = "Assets/Resources/data/capables/";
    public Hashtable capables_data = new Hashtable();

    [Header("Loading / Unloading")]
    public Hashtable loaded_capables_data = new Hashtable();
    public List<string> loading_queue = new List<string>();
    public List<string> unloading_queue = new List<string>();

    [Header("State")]
    public bool awake_done = false;
    public bool start_loading_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;
    public bool log_loading = false;
    public bool hide_log_no_data_found = false;
    public bool log_spawning = false;
    public int load_x_capables_per_frame = 1;


    // EVENTS
    public Action<Capable, Capable> OnCapableSpawned; // we pass the spawned capable's data and the spawner capable (can be null)




    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load capables data
        loadCapablesData();
    }

    // LOAD / UNLOAD DATA
    protected void loadCapablesData()
    {
        // we empty the capables_data
        capables_data = new Hashtable();
        string log_capables_details = "\n\n";

        // we load all the json files in the data path and convert them to CapableData objects
        string[] files = System.IO.Directory.GetFiles(data_path, "*.json");
        foreach (string file in files)
        {
            string json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
            CapableData data = JsonUtility.FromJson<CapableData>(json);
            capables_data.Add(data.id, data);
            log_capables_details += data.GetDetails() + "\n";
        }

        if (log_awake_data) { Debug.Log("(CapableSystem) CAPABLES DATA LOADED : " + capables_data.Count + log_capables_details); }
        awake_done = true;
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
        CapableData data = capables_data[id] as CapableData;
        if (data == null)
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - Load) Capable data not found for id: " + id); }
            return null;
        }
        return load_capable(data);
    }
    private Capable load_capable(CapableData data)
    {
        Capable capable = CapableBank.Instance.Load(data);
        loaded_capables_data.Add(data.id, data);
        if (log_loading) { Debug.Log("(CapableSystem) Loaded " + data.id); }
        return capable;
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
        MovableEngine.Instance.UnregisterInBatch(movables_to_unregister);
        
        return count;
    }
    private Capable unload_capable(string id)
    {
        CapableData data = loaded_capables_data[id] as CapableData;
        if (data == null) { Debug.LogWarning("(CapableSystem - Unload) Loaded capable data not found for id: " + id); return null; }
        Capable capable = CapableBank.Instance.Unload(data);
        loaded_capables_data.Remove(id);
        if (log_loading) { Debug.Log("(CapableSystem) Unloaded " + id); }
        return capable;
    }


    // SPAWNING
    public Capable SpawnCapable(string base_id, Capable spawner)
    {
        if (log_spawning) { Debug.Log($"(CapableSystem) Spawning {base_id} entity"); }

        // 1. we find base_id data & duplicates it
        CapableData base_data = capables_data[base_id] as CapableData;
        if (base_data == null) { Debug.LogWarning("(CapableSystem - SpawnCapable) Capable data not found for id: " + base_id); return null; }
        CapableData spawn_data = DuplicateData(base_data);

        if (log_spawning) { Debug.Log($"(CapableSystem) Duplicated {base_id} data to {spawn_data.id} \n {spawn_data.GetDetails()}"); }

        // 2. we load the new spawned capable
        Capable spawned_capable = load_capable(spawn_data);

        // 3. we alert the RoomSystem that we just spawned a capable, for it to assign a room to it
        OnCapableSpawned?.Invoke(spawned_capable, spawner);

        if (log_spawning) { Debug.Log($"(CapableSystem) Spawned {base_id} (new id : {spawn_data.id})"); }

        // 4. we return the spawned capable
        return spawned_capable;
    }
    private CapableData DuplicateData(CapableData base_data)
    {
        CapableData new_data = base_data.Duplicate();
        new_data.id = GenerateUniqueId(base_data.id);

        // we add the new_data to the data list
        capables_data.Add(new_data.id, new_data);
        return new_data;
    }
    private string GenerateUniqueId(string base_id)
    {
        // todo if we have perf issues when spawning capables this can be the issue
        // then we just need to have a static int that we increment so it's faster

        // we check if the base_id can be splitted with "_"
        string[] parts = base_id.Split('_');
        string suffix = parts.Length > 1 ? parts[parts.Length - 1] : "";
        string prefix = base_id.Substring(0, base_id.Length - suffix.Length);

        // we go through all capables_data keys and memorize all the ids that have the same prefix and check the suffix int is greater or not
        int max_suffix = 0;
        foreach (string key in capables_data.Keys)
        {
            if (key.StartsWith(prefix))
            {
                string key_suffix = key.Substring(prefix.Length);
                if (int.TryParse(key_suffix, out int key_suffix_int))
                {
                    if (key_suffix_int > max_suffix)
                    {
                        max_suffix = key_suffix_int;
                    }
                }
            }
        }

        return prefix + (max_suffix + 1);
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
    public bool HasCapable(Capable capable)
    {
        if (capable == null) { return false; }
        if (capable.data == null) { return false; }
        if (capable.data.id == "") { return false; }
        return loaded_capables_data.ContainsKey(capable.data.id);
    }

}