using System;
using System.Collections.Generic;
using UnityEngine;

public class CapableSystem : BSOD_System<CapableSystem>
{

    [Header("Templates Capables data")]
    private string templates_data_path = "data/templates/capables/";
    public Dictionary<string, CapableData> templates_capables_data = new Dictionary<string, CapableData>();

    [Header("World Capables data")]
    private string world_data_path = "data/capables/";
    public Dictionary<string, CapableData> world_capables_data = new Dictionary<string, CapableData>();

    [Header("Runtime IDs (hashs)")]
    private Dictionary<string, int> capables_hashs_by_ids = new Dictionary<string, int>();
    private Dictionary<int, string> capables_ids_by_hash = new Dictionary<int, string>();
    private int next_capable_hash = 1; // we start at 1 because 0 is the default value for non hashables (null, empty id, etc)

    [Header("Loaded Capables data")]
    public Dictionary<string,CapableData> loaded_capables_data = new Dictionary<string,CapableData>();
    public List<string> loading_queue = new List<string>();
    public List<string> unloading_queue = new List<string>();

    [Header("System parameters")]
    public int load_x_capables_per_frame = 1;

    [Header("State")]
    private bool awake_done = false;

    [Header("Logs Awake")]
    public bool log_templates_data_loading = false;
    public bool log_world_data_loading = false;
    public bool log_awake_data_extended = false;

    [Header("Logs Spawning / Switching")]
    public bool log_duplicating = false;
    public bool log_spawning = false;
    public bool log_corpse_switching = false;

    [Header("Logs Loading / Unloading")]
    public bool log_loading = false;
    public bool log_loading_extended = false;
    public bool hide_log_no_data_found = false;
    


    // EVENTS
    public Action<Capable, string> OnCapableNeedRoom; // we pass the spawned capable's data and the spawner id (can be null)
    public Action<string> OnCapableNeedFreedom; // only the capable id we need to free



    /* -------------------------------------

     1. AWAKE & DATA LOADING

    ------------------------------------- */


    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load templates data
        loadTemplatesCapablesData();

        // load world capables data
        loadWorldCapablesData();

        // detect capables that are not linked to a capable data in our system
        // and create an hash if it has an id
        detectCapablesOutsideOfSystem();

        awake_done = true;
    }

    // LOAD TEMPLATES & WORLD CAPABLES DATA
    protected void loadTemplatesCapablesData()
    {
        // we empty the capables_data & runtime ids etc
        templates_capables_data = new Dictionary<string, CapableData>();
        string log_capables_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = GameManager.Instance.LoadJsons(templates_data_path);
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
                loadCapableDataOfType(json, kind, ref log_capables_details, ref templates_capables_data, generate_runtime: false);
            }
        }

        if (log_templates_data_loading) { Debug.Log("(CapableSystem) CAPABLES DATA LOADED : " + templates_capables_data.Count + log_capables_details); }
    }
    protected void loadWorldCapablesData()
    {
        // we empty the capables_data & runtime ids etc
        world_capables_data = new Dictionary<string, CapableData>();
        capables_hashs_by_ids = new Dictionary<string, int>();
        capables_ids_by_hash = new Dictionary<int, string>();
        next_capable_hash = 1;
        string log_capables_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = GameManager.Instance.LoadJsons(world_data_path);
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
                loadCapableDataOfType(json, kind, ref log_capables_details, ref world_capables_data);
            }
        }

        if (log_world_data_loading) { Debug.Log("(CapableSystem) CAPABLES DATA LOADED : " + world_capables_data.Count + log_capables_details); }
    }
    private void loadCapableDataOfType(string json, string kind, ref string log, ref Dictionary<string, CapableData> data_by_id, bool generate_runtime = true)
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
            data_by_id.Add(data.id, data);
            if (generate_runtime) { generate_runtime_id(data.id); }

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
        data_by_id.Add(data.id, data);
        if (generate_runtime) { generate_runtime_id(data.id); }
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

        if (log_world_data_loading && detected_outsiders.Count > 0) { Debug.Log("(CapableSystem) OUTSIDERS DETECTED : " + detected_outsiders.Count + "\n - " + string.Join("\n - ",detected_outsiders)); }
    }



    /* -------------------------------------

     2. SPAWNING / SWITCHING / DROPPING / GRABBING CAPABLES

    ------------------------------------- */



    // DATA MANAGMENT

    /// <summary>
    /// duplicates a data from a template
    /// (useful for spawning capables)
    /// </summary>
    /// <param name="base_data"></param>
    /// <returns></returns>
    private CapableData DuplicateTemplate(string template)
    {
        // we get the base data
        if (!templates_capables_data.ContainsKey(template))
        {
            if (!hide_log_no_data_found || log_duplicating) { Debug.LogWarning("(CapableSystem - DuplicateTemplate) Template capable data not found for id: " + template); }
            return null;
        }

        // 1. we duplicate the data into new data
        ICapableData base_data = templates_capables_data[template];
        CapableData new_data = base_data.Duplicate() as CapableData;
        new_data.id = GameManager.Instance.GenerateUniqueID(base_data.id);

        // generate a hash
        generate_runtime_id(new_data.id);

        // we add the new_data to the world data list
        world_capables_data.Add(new_data.id, new_data);


        // 2. we spawn capacities from this new data
        for (int i = 0; i < new_data.capacities_ids.Count; i++)
        {
            string capa_template = new_data.capacities_ids[i];
            CapacityEngine.Instance.SpawnCapacity(capa_template, new_data); // this replace the capacity id in the entity data
        }

        if (log_duplicating) { Debug.Log($"(CapableSystem) Duplicated {template} data to {new_data.id} \n {new_data.GetDetails()}"); }


        return new_data;
    }

    // SPAWNING / DROPPING CAPABLES & ITEMS
    public Capable SpawnCapable(string template, string spawner_id = "")
    {
        return SpawnCapable(DuplicateTemplate(template), spawner_id);
    }
    public Capable SpawnCapable(CapableData data, string spawner_id = "")
    {
        if (data == null)
        {
            if (log_spawning) { Debug.LogError($"(CapableSystem - SpawnCapable) Failed to spawn capable. Data is null."); }
            return null;
        }
        if (log_spawning) { Debug.Log($"(CapableSystem) Spawning {data.id} entity"); }

        // 1. we load the new spawned capable
        Capable spawned_capable = load_capable(data);

        // 2. we alert the RoomSystem that we just spawned a capable, for it to assign a room to it
        OnCapableNeedRoom?.Invoke(spawned_capable, spawner_id);

        if (log_spawning) { Debug.Log($"(CapableSystem) Spawned {data.id}"); }

        // 3. we return the spawned capable
        return spawned_capable;
    }
    public void OnItemDropped(Item item, Capable dropper)
    {
        // we simply inform the room system that we need a room for the item
        OnCapableNeedRoom?.Invoke(item, dropper?.data?.id ?? "");
    }
    public void OnItemGrabbed(Item item, Capable grabber)
    {
        // we simply inform the room system that we need to detach the item from the room
        OnCapableNeedFreedom?.Invoke(item.data.id);
    }

    // SWITCH CAPABLE TO CORPSE
    private CorpseData base_corpse_data;
    public async void SwitchToCorpse(Capable capable)
    {
        // 1. DROP ALL ITEMS
        if (capable.Inventory != null && capable.Inventory.Count > 0)
        {
            // we make the capable drop all its items and we wait for it to be done
            await capable.DropAllItems();
        }

        // 2. SAVE CAPABLE DATA
        capable.SaveDynamicData();
        CapableData capable_data = capable.data;
        List<Force> forces = new List<Force>((capable as Movable)?.GetForces());

        // 3. SPAWN THE CORPSE DATA
        CorpseData corpse_data = DuplicateTemplate("corpse") as CorpseData;
        corpse_data.Init(capable_data); // we transfer some of the capable data to the corpse data (ex : position, orientation, tag, skin if we have anim_data, etc)
        // todo here we should put some meat items inside corpse data inventory so they auto load when spawning the corpse
        // and with the right meat reference

        // 4. UNLOAD THE CAPABLE
        unload_capable(capable_data.id);

        // 5. SPAWN THE CORPSE
        Corpse corpse = SpawnCapable(corpse_data, capable_data.id) as Corpse; // (will assign the corpse to the same room as the capable since we pass the capable as spawner_id)
        OnCapableNeedFreedom?.Invoke(capable_data.id); // then we need to free the old capable data from the room system since we don't want it to be loaded in the room anymore
        if (log_corpse_switching) { Debug.Log($"(CapableSystem - SwitchToCorpse) Switched {capable.name} to corpse {corpse.name} \n - Capable data : \n{capable_data.GetDetails()} \n - Corpse data : \n{corpse_data.GetDetails()}"); }
        corpse.AnimPlayer.Play("die");

        // 6. TRANSFER FORCES
        corpse.SetForces(forces);
    }




    /* -------------------------------------

     3. DYNAMIC LOADING & UNLOADING OF CAPABLES

    ------------------------------------- */



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

            // if already in loading queue, we do nothing
            if (loading_queue.Contains(id)) { continue; }

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
        if (!world_capables_data.ContainsKey(id))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - Load) Capable data not found for id: " + id); }
            return null;
        }
        CapableData data = world_capables_data[id];
        return load_capable(data);
    }
    private Capable load_capable(CapableData data)
    {
        // safe guards
        if (data == null)
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - Load) Capable data is null"); }
            return null;
        }
        if (HasLoadedCapableData(data.id))
        {
            if (log_loading) { Debug.Log("(CapableSystem - Load) Capable " + data.id + " is already loaded"); }
            return CapableBank.Instance.GetLoadedCapable(data.id);
        }

        // we load the capable from the data
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
        int capables_unloaded = 0;
        for (int i = 0; i < count; i++)
        {
            if (unloading_queue.Count <= 0) { break; }
            string capable_id = unloading_queue[0];
            Capable capable = unload_capable(capable_id);
            unloading_queue.RemoveAt(0);
            capables_unloaded++;

            // add to the unregistering list if movable
            if (capable != null && capable is Movable)
            {
                movables_to_unregister.Add(capable as Movable);
            }
        }

        // we call MovableEngine.UnregisterInBatch to remove all the capable we just disabled
        if (log_loading_extended && movables_to_unregister.Count > 0) { Debug.Log($"(CapableSystem - unload_in_queue) Calling MovableEngine.UnregisterInBatch for {movables_to_unregister.Count} entities : \n  - {(string.Join("\n  - ", movables_to_unregister))}"); }
        MovableEngine.Instance.UnregisterInBatch(movables_to_unregister);
        
        return capables_unloaded;
    }
    private Capable unload_capable(string id)
    {
        if (!loaded_capables_data.ContainsKey(id))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning($"(CapableSystem - Unload) Capable {id} is not loaded (or inexistant)"); }
            return null;
        }
        CapableData data = loaded_capables_data[id];
        Capable capable = CapableBank.Instance.Unload(data);
        loaded_capables_data.Remove(id);
        if (log_loading) { Debug.Log("(CapableSystem) Unloaded " + id); }
        return capable;
    }


    // UPDATE
    private void Update()
    {
        if (!awake_done) { return; }
        
        // we load / unload in queue
        int unloaded = unload_in_queue(load_x_capables_per_frame);
        load_in_queue(load_x_capables_per_frame - unloaded);
    }




    /* -------------------------------------

     4. GETTERS & OTHERS

    ------------------------------------- */


    // GETTERS
    public bool HasLoadedCapableData(string id) { return loaded_capables_data.ContainsKey(id); }
    public int GetCapableHashFromID(string id)
    {
        return capables_hashs_by_ids.TryGetValue(id, out int hash) ? hash : 0;
    }
    public string GetCapableIDFromHash(int hash)
    {
        return capables_ids_by_hash.TryGetValue(hash, out string id) ? id : null;
    }
}