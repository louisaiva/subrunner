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
    

    [Header("Outsiders data")] // won't exist later 
    private Dictionary<CapableData, Capable> outsiders_data = new Dictionary<CapableData, Capable>(); // dictionary of capables that are in the world but not handled (loaded/unloaded) by our system. in the future there will be zero, but for now we still have some of these when we start the game
    private HashSet<string> outsiders_ids = new HashSet<string>();

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
    private bool start_done = false;

    [Header("Logs Awake")]
    public bool log_templates_data_loading = false;
    public bool log_world_data_loading = false;
    public bool log_awake_data_extended = false;
    public bool log_start_links = false;

    [Header("Logs Spawning / Switching")]
    public bool log_duplicating = false;
    public bool log_spawning = false;
    public bool log_corpse_switching = false;
    public bool hide_log_ownership_validation = false;

    [Header("Logs Loading / Unloading")]
    public bool log_loading = false;
    public bool log_loading_extended = false;
    public bool hide_log_no_data_found = false;
    


    // EVENTS
    public Action<CapableData> OnWorldCapableDataLoaded; // 
    public Action<Capable, string> OnCapableNeedRoom; // we pass the spawned capable's data and the spawner id (can be null)
    public Action<string> OnCapableNeedFreedom; // only the capable id we need to free
    public Action<CapableData> OnCapableSpawned; // we pass the spawned capable's data
    public Action<CapableData> OnCapableDespawned; // we pass the despawned capable's data


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
    public virtual void Start()
    {
        // sanity check after loading all world data
        ValidateAllOwnershipLinks(repair: false); // log-only, no fixes
        start_done = true;
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

        if (log_templates_data_loading) { Debug.Log("(CapableSystem) TEMPLATES CAPABLES DATA LOADED : " + templates_capables_data.Count + log_capables_details); }
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
            
            // verify that we are not loading a template
            if (templates_capables_data.ContainsKey(data.id))
            {
                if (log_world_data_loading) { Debug.LogWarning($"(CapableSystem - loadWorldCapablesData) Trying to load world capable data with id {data.id} but it already exists in templates data. Skipping it."); }
                continue;
            }

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

        if (log_world_data_loading) { Debug.Log("(CapableSystem) WORLD CAPABLES DATA LOADED : " + world_capables_data.Count + log_capables_details); }
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
        if (GameManager.IsKind(capable_type, typeof(Item))) { data_type = typeof(ItemData); }
        
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
        Capable[] already_existing_capables = FindObjectsByType<Capable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<string> enabled_outsiders = new List<string>();
        List<Capable> to_remove_maybe = new List<Capable>();
        outsiders_data = new Dictionary<CapableData, Capable>();
        for (int i=0; i<already_existing_capables.Length; i++)
        {
            Capable capable = already_existing_capables[i];

            // checks if enabled (we only care about enabled outsiders)
            if (!capable.gameObject.activeInHierarchy) { to_remove_maybe.Add(capable); continue; }

            // check if it has a data
            if (capable.data == null) { continue; }
            if (string.IsNullOrEmpty(capable.data.id)) { continue; }
            if (capable.data.id == "cursor-1") { to_remove_maybe.Add(capable); continue; } // special case for the hacking cursor because we HATE it

            // we have an outsider !!
            enabled_outsiders.Add(capable.data.id);
            if (GetCapableHashFromID(capable.data.id) == 0) { generate_runtime_id(capable.data.id); } // generate a runtime id if this capable data was not already in our loaded data
            outsiders_data.Add(capable.data, capable);
            outsiders_ids.Add(capable.data.id);
        }

        if (log_world_data_loading && enabled_outsiders.Count > 0) { Debug.Log("(CapableSystem) OUTSIDERS DETECTED : " + enabled_outsiders.Count + "\n - " + string.Join("\n - ",enabled_outsiders)); }


        // we remove maybe the data of some outsiders (disabled + cursor)
        string log_removed = "";
        int removed_count = 0;
        for (int i=0; i<to_remove_maybe.Count; i++)
        {
            if (!remove_outsider_from_world(to_remove_maybe[i])) { continue; }
            log_removed += to_remove_maybe[i].data.id + "\n";
            removed_count++;
        }
        if (log_world_data_loading && removed_count > 0) { Debug.Log("(CapableSystem) OUTSIDERS REMOVED : " + removed_count + "\n - " + log_removed); }
    }
    private bool remove_outsider_from_world(Capable capable)
    {
        if (capable.data == null) { return false; }
        if (string.IsNullOrEmpty(capable.data.id)) { return false; }
        
        // we check if we have the id in the system
        if (!world_capables_data.ContainsKey(capable.data.id)) { return false; }
        CapableData data = world_capables_data[capable.data.id];

        // we verify that the id is not already in a room, if yes we don't want to remove the data
        // since we will need it when the room are loaded.
        if (RoomSystem.Instance.IsInARoom(data.id)) { return false; }

        // we remove the grabbed items since they don't have a room
        if (data is ItemData item_data && item_data.is_grabbed) { return false; }

        // we remove the data from the world data, runtime ids, etc
        world_capables_data.Remove(data.id);
        int hash = GetCapableHashFromID(data.id);
        if (hash != 0)
        {
            capables_ids_by_hash.Remove(hash);
            capables_hashs_by_ids.Remove(data.id);
        }
        return true;
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

    /// <summary>
    /// verifies that the capable data and capacity data are correctly linked together
    /// (ex : the capacity data has the right capable_id,
    /// the capable data has the capacity id in its list, etc)
    /// </summary>
    /// <param name="entity_data">the capable data to validate</param>
    /// <param name="cap_data">the capacity data to validate</param>
    /// <param name="repair">whether to attempt to repair any issues found</param>
    /// <param name="repair_removes_duplicates">whether the repaire should remove capacity_id inside capable_data for duplicates (/!\\ could remove the capacity for those duplicates capables)</param>
    /// <returns>bool whether the links are valid</returns>
    public bool ValidateOwnershipLinks(CapableData entity_data, CapacityData cap_data, bool repair = true, bool repair_removes_duplicates = false)
    {
        // basic guards
        if (entity_data == null || cap_data == null)
        {
            if (!hide_log_ownership_validation) { Debug.LogWarning("(CapableSystem - ValidateOwnershipLinks) entity_data or cap_data is null."); }
            return false;
        }
        if (string.IsNullOrEmpty(entity_data.id) || string.IsNullOrEmpty(cap_data.id))
        {
            if (!hide_log_ownership_validation) { Debug.LogWarning($"(CapableSystem - ValidateOwnershipLinks) Invalid ids. entity='{entity_data?.id}', capacity='{cap_data?.id}'."); }
            return false;
        }

        // first we check if the links are ok as they are
        bool forward_ok = entity_data.capacities_ids != null && entity_data.capacities_ids.Contains(cap_data.id);
        bool reverse_ok = cap_data.owner_id == entity_data.id;

        // check for duplicates : we should not have any other capable owning this capacity
        List<CapableData> duplicate_owners = new List<CapableData>();
        foreach (KeyValuePair<string, CapableData> pair in world_capables_data)
        {
            CapableData other = pair.Value;
            if (other == null || other.id == entity_data.id || other.capacities_ids == null) { continue; }
            if (other.capacities_ids.Contains(cap_data.id)) { duplicate_owners.Add(other); }
        }
        bool duplicates_ok = duplicate_owners.Count == 0;

        // if everything is ok, we return true !!
        if (forward_ok && reverse_ok && duplicates_ok) { return true; }
        if (!repair)
        {
            if (!hide_log_ownership_validation) { Debug.LogWarning($"(CapableSystem - ValidateOwnershipLinks) Ownership links are not valid for capacity '{cap_data.id}' and capable '{entity_data.id}', and repair is disabled. forward_ok={forward_ok}, reverse_ok={reverse_ok}, duplicates_ok={duplicates_ok}, duplicate_owners=[{string.Join(", ", duplicate_owners.ConvertAll(d => d.id))}]\n{entity_data.GetDetails()}\n{cap_data.GetDetails()}"); }
            return false;
        }

        // else we try to repair

        // repairs
        if (entity_data.capacities_ids == null) { entity_data.capacities_ids = new List<string>(); }
        if (!entity_data.capacities_ids.Contains(cap_data.id)) { entity_data.capacities_ids.Add(cap_data.id); }
        if (cap_data.owner_id != entity_data.id) { cap_data.owner_id = entity_data.id; }

        // remove duplicate ownership from other capables
        if (repair_removes_duplicates) { for(int i = 0; i < duplicate_owners.Count; i++) { duplicate_owners[i].capacities_ids.Remove(cap_data.id); } }

        // final check after repair
        bool is_fixed = ValidateOwnershipLinks(entity_data, cap_data, repair: false); // repair is false so no infinite loop !
        if (!is_fixed && !hide_log_ownership_validation) { Debug.LogWarning($"(CapableSystem - ValidateOwnershipLinks) Failed to repair ownership for capacity '{cap_data.id}' and capable '{entity_data.id}'.\n{entity_data.GetDetails()}\n{cap_data.GetDetails()}"); }
        return is_fixed;
    }
    public bool ValidateAllOwnershipLinks(bool repair = false)
    {
        // we prepare for logging
        bool old_hide_log = hide_log_ownership_validation;
        hide_log_ownership_validation = true; // disable validation logs since we will log them all at the end in a summary
        string log_summary = "";

        // initialize counters for summary log
        int total_links = 0;
        int broken_links = 0;
        int fixed_links = 0;
        int orphan_capacities = 0;

        // we go through all capables and their capacities and validate the links
        List<CapableData> capables_inspected = new List<CapableData>();
        List<CapacityData> capacities_inspected = new List<CapacityData>();
        foreach (KeyValuePair<string, CapableData> pair in world_capables_data)
        {
            CapableData capable_data = pair.Value;
            string entity_id = capable_data.id;
            if (capable_data == null || capable_data.capacities_ids == null) { continue; }
            capables_inspected.Add(capable_data);
            for (int i = 0; i < capable_data.capacities_ids.Count; i++)
            {
                total_links++; // we have a link ! we don't know if it's valid or not yet but we count it for the summary log

                // try to get the data
                string cap_id = capable_data.capacities_ids[i];
                CapacityData cap_data = CapacityEngine.Instance.GetCapacityData(cap_id);
                if (cap_data == null)
                {
                    // no capacity data found, so it's a broken link
                    broken_links++;
                    log_summary += $"\n  - (broken) {entity_id} -> {cap_id} : capacity data not found";
                    continue;
                }

                // we have the data, we can validate the links
                capacities_inspected.Add(cap_data);
                if (ValidateOwnershipLinks(capable_data, cap_data, repair: false)) { continue; } // if the links are ok, we do nothing

                // else we have a broken link
                broken_links++;
                bool forward_ok = capable_data.capacities_ids.Contains(cap_id);
                bool reverse_ok = cap_data.owner_id == entity_id;
                if (!repair)
                {
                    log_summary += $"\n  - (broken) {entity_id} -> {cap_id} : forward_ok={forward_ok}, reverse_ok={reverse_ok}{((forward_ok && reverse_ok) ? ", DUPLICATES FOUND !!! " : "")} - (not fixed since repair is disabled)";
                    continue;
                }
                
                // we try to repair the link
                bool is_fixed = ValidateOwnershipLinks(capable_data, cap_data, repair: true);
                forward_ok = capable_data.capacities_ids.Contains(cap_id);
                reverse_ok = cap_data.owner_id == entity_id;
                if (is_fixed)
                {
                    fixed_links++;
                    log_summary += $"\n  - (fixed) {entity_id} -> {cap_id} : forward_ok={forward_ok}, reverse_ok={reverse_ok}{((forward_ok && reverse_ok) ? ", DUPLICATES FOUND !!! " : "")}";
                }
                else
                {
                    log_summary += $"\n  - (broken) {entity_id} -> {cap_id} : forward_ok={forward_ok}, reverse_ok={reverse_ok}{((forward_ok && reverse_ok) ? ", DUPLICATES FOUND !!! " : "")} - (FAILED TO FIX !!!)";
                }
            }
        }

        // we go through all capacities in CapacityEngine world's capacities to check if we have orphan capacities
        foreach (KeyValuePair<string, CapacityData> pair in CapacityEngine.Instance.world_capacities_data)
        {
            CapacityData cap_data = pair.Value;
            string cap_id = cap_data.id;
            if (cap_data == null) { continue; }
            if (capacities_inspected.Contains(cap_data)) { continue; } // we already inspected this capacity through its capable ownership, so we skip it

            // we have an orphan capacity that is not owned by any capable, which is a broken link
            broken_links++;
            orphan_capacities++;
            log_summary += $"\n  - (orphan) ORPHAN CAPACITY {cap_id} : no capable owns this capacity (their capable is {cap_data.owner_id}, is it in the world ? {(world_capables_data.ContainsKey(cap_data.owner_id) ? "YES" : "NO")})";
        }

        // we restore the log state
        hide_log_ownership_validation = old_hide_log;

        // we log the summary
        int broken_percentage = (total_links > 0) ? (broken_links * 100 / total_links) : 0;
        int final_broken_links = broken_links - fixed_links; // we count the fixed links as valid for the summary
        
        if (start_done || log_start_links) { Debug.Log($"(CapableSystem)      LINKS BROKEN : {broken_links}/{total_links} ({broken_percentage}%)   |   FIXED : {fixed_links}/{broken_links}   |   ORPHAN : {orphan_capacities}  \n{log_summary}"); }

        return final_broken_links == 0;
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

        // 2. we fire events
        OnCapableNeedRoom?.Invoke(spawned_capable, spawner_id); // we alert the RoomSystem that we just spawned a capable, for it to assign a room to it
        OnCapableSpawned?.Invoke(data);

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
        List<Force> forces = new List<Force>((capable as Movable)?.GetForces() ?? new List<Force>()); // duplicate the forces

        // 3. SPAWN THE CORPSE DATA
        CorpseData corpse_data = DuplicateTemplate("corpse") as CorpseData;
        corpse_data.Init(capable_data); // we transfer some of the capable data to the corpse data (ex : position, orientation, tag, skin if we have anim_data, etc)
        
        // todo here we should put some meat items inside corpse data inventory so they auto load when spawning the corpse
        // and with the right meat reference

        // 4. UNLOAD THE CAPABLE
        unload_capable(capable_data.id);
        OnCapableDespawned?.Invoke(capable_data);

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
    private float _update_positions_timer = 0f;
    private float update_positions_interval = 1f;


    private void Update()
    {
        if (!awake_done) { return; }
        
        // we load / unload in queue
        int unloaded = unload_in_queue(load_x_capables_per_frame);
        load_in_queue(load_x_capables_per_frame - unloaded);

        // we update the positions of the loaded capables
        _update_positions_timer += Time.deltaTime;
        if (_update_positions_timer < update_positions_interval) { return; }
        _update_positions_timer = 0f;
        update_loaded_capables_positions();
    }
    private void update_loaded_capables_positions()
    {
        foreach (KeyValuePair<string, CapableData> pair in loaded_capables_data)
        {
            string id = pair.Key;
            CapableData data = pair.Value;
            Capable capable = CapableBank.Instance.GetLoadedCapable(id);
            if (capable == null) { continue; }
            data.SetPosition(capable.transform.position);
        }
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
    public List<CapableData> GetInsidersWorldCapablesData()
    {
        List<CapableData> insiders = new List<CapableData>();
        foreach (KeyValuePair<string, CapableData> pair in world_capables_data)
        {
            CapableData data = pair.Value;
            
            // check if inside the system or not
            if (IsOutsider(data.id)) { continue; }

            // otherwise we add it !
            insiders.Add(data);
        }
        return insiders;
    }
    public Dictionary<CapableData, Capable> GetOutsidersWorldCapablesData()
    {
        return outsiders_data;
    }
    public bool IsOutsider(string id)
    {
        return outsiders_ids.Contains(id);
    }
    public bool TryGetOutsider(string id, out Capable capable)
    {
        capable = null;
        if (!IsOutsider(id)) { return false; }
        foreach (KeyValuePair<CapableData, Capable> pair in outsiders_data)
        {
            CapableData data = pair.Key;
            if (data.id == id)
            {
                capable = pair.Value;
                return true;
            }
        }
        return false;
    }
    public List<CapableData> GetCapablesDataFromIDs(List<string> ids)
    {
        List<CapableData> data_list = new List<CapableData>();
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            if (!world_capables_data.ContainsKey(id))
            {
                if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - GetCapablesDataFromIDs) Capable data not found for id: " + id); }
                continue;
            }
            data_list.Add(world_capables_data[id]);
        }
        return data_list;
    }
}