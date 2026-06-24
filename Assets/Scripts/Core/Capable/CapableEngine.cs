#pragma warning disable 1998

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class CapableEngine : BSOD_System<CapableEngine>
{

    // SUB SYSTEMS
    private static TrashEngine _trash_engine;
    public static TrashEngine TrashEngine
    {
        get
        {
            if (_trash_engine != null) { return _trash_engine; }
            _trash_engine = LazyInstance.GetComponentInChildren<TrashEngine>(includeInactive:true);
            return _trash_engine;
        }
    }




    [Header("Templates Capables data")]
    public Dictionary<string, CapableData> templates_capables_data = new Dictionary<string, CapableData>();

    [Header("World Capables data")]
    // private string world_data_path = "data/capables/";
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
    private bool world_data_loaded = false;
    private bool ownership_check_done = false;

    [Header("Logs Awake")]
    public bool log_templates_data_loading = false;
    public bool log_world_data_loading = false;
    public bool log_awake_data_extended = false;
    public bool log_start_capacity_ownership = false;

    [Header("Logs Spawning / Switching")]
    public bool log_duplicating = false;
    public bool log_spawning = false;
    public bool log_item_switching = false;
    public bool hide_log_ownership_validation = true;

    [Header("Logs Loading / Unloading")]
    public bool log_loading = false;
    public bool log_loading_extended = false;
    public bool log_visibility = false;
    public bool hide_log_no_data_found = false;

    [Header("Logs Saving")]
    public bool log_saving = false;
    


    // EVENTS
    public Action<CapableData> OnCapableAppear; // =/= spawned bcz it works for items too. a dropped item appears BUT is was not spawned !
    public Action<CapableData> OnCapableDisappear; // despawned capables + grabbed items
    public Action<CapableData> OnCapableSpawned; // only first time a capable appear
    public Action<CapableData> OnCapableDespawned; // after this we have no capable data stored anymore


    ///
    //
    /// 1. AWAKE & DATA LOADING
    //
    ///


    // LOAD / UNLOAD WORLD DATA
    public override async Task LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(CapableEngine) Loading world data for world_id: {world_id}"); }

        // load templates data
        if (!templates_loaded)
        {
            loadTemplatesCapablesData();
            if (log) { Debug.Log($"(CapableEngine) Loaded {templates_capables_data.Count} templates capables data"); }
        }

        // load world capables data
        LoadWorldCapablesData(world_id);
        if (log) { Debug.Log($"(CapableEngine) Loaded {world_capables_data.Count} world capables data"); }

        // detect capables that are not linked to a capable data in our system
        // and create an hash if it has an id
        detectCapablesOutsideOfSystem();
        if (log) { Debug.Log($"(CapableEngine) Detected {outsiders_data.Count} outsiders capables in the world"); }
        world_data_loaded = true;

        // finally we can load the sub systems
        await RoomEngine.Instance.DoorEngine.LoadWorldData(world_id, log);
        await MotorEngine.Instance.LoadWorldData(world_id, log);
        if (log) { Debug.Log($"(CapableEngine) Loaded DoorEngine & MotorEngine sub systems"); }


        await Task.Yield();

        // sanity check after loading all world data
        ValidateAllOwnershipLinks(repair: false); // log-only, no fixes
        ownership_check_done = true;
        if (log) { Debug.Log($"(CapableEngine) Ownership links validation check done for loaded world data. Total links={capables_hashs_by_ids.Count}, outsiders={outsiders_data.Count}"); }
        if (log) { Debug.Log($"(CapableEngine) CAPABLE SYSTEM SUCCESSFULLY LOADED : {world_id}"); }
    }
    public override async Task UnloadWorldData(bool log)
    {
        // we DON'T unload the templates data since it is valid for all worlds

        if (log) { Debug.Log($"(CapableEngine) clearing sub systems cache"); }
        CapableBank.Instance.ClearSubSystemsCache(log); // clears AnimLayerBank, ColliderBank
        TrashEngine.ClearCache(log);

        // we unload all loaded capables
        if (log) { Debug.Log($"(CapableEngine) clearing loaded capables data"); }
        loaded_capables_data.Clear();

        if (log) { Debug.Log($"(CapableEngine) destroying all loaded capables"); }
        CapableBank.Instance.DestroyAllCapablesInstantly(log);


        // we clear the world capables data, runtime ids, etc
        if (log) { Debug.Log($"(CapableEngine) clearing data"); }
        world_capables_data.Clear();
        capables_hashs_by_ids.Clear();
        capables_ids_by_hash.Clear();
        next_capable_hash = 1;

        // we clear the outsiders data (should be empty since they should not be in the world anymore, but just in case)
        outsiders_data.Clear();
        outsiders_ids.Clear();

        world_data_loaded = false;
        ownership_check_done = false;

        if (log) { Debug.Log($"(CapableEngine) CAPABLE SYSTEM SUCCESSFULLY UNLOADED"); }
    }


    // LOAD TEMPLATES & WORLD CAPABLES DATA
    private string templates_data_path = "data/templates/capables/";
    private bool templates_loaded = false;
    protected void loadTemplatesCapablesData()
    {
        // we empty the capables_data & runtime ids etc
        templates_capables_data = new Dictionary<string, CapableData>();
        string log_capables_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = AppManager.LoadJsonsFromAssets(templates_data_path);
        // Dictionary<string, List<string>> json_by_kind = new Dictionary<string, List<string>>();
        foreach (string json in files)
        {
            CapableData data = SaveEngine.LoadCapableDataWithGoodKind(json);
            if (log_awake_data_extended) { Debug.Log($"(CapableEngine) Loading capable data template for '{data.id}' of type {data.GetType().Name}: \n{data.GetDetails()}"); }
            templates_capables_data.Add(data.id, data);
            log_capables_details += data.GetDetails() + "\n";
        }

        if (log_templates_data_loading) { Debug.Log("(CapableEngine) TEMPLATES CAPABLES DATA LOADED : " + templates_capables_data.Count + log_capables_details); }
        templates_loaded = true;
    }
    public void LoadWorldCapablesData(string world_id)
    {
        WorldSaveData world_save = SaveEngine.GetWorldSave(world_id);
        if (world_save == null)
        {
            Debug.LogWarning($"(CapableEngine) World save data not found for world_id: {world_id}. No world capables data loaded.");
            return;
        }

        // we empty the capables_data & runtime ids etc
        world_capables_data = new Dictionary<string, CapableData>();
        capables_hashs_by_ids = new Dictionary<string, int>();
        capables_ids_by_hash = new Dictionary<int, string>();
        next_capable_hash = 1;
        string log_capables_details = "\n\n";

        // we load all the json files in the data path and get their kind
        List<CapableData> world_capables = world_save.capables;
        foreach (CapableData data in world_capables)
        {
            if (log_awake_data_extended) { Debug.Log($"(CapableEngine) Loading capable data for '{data.id}' of type {data.GetType().Name}: \n{data.GetDetails()}"); }
            world_capables_data.Add(data.id, data);
            generate_runtime_id(data.id);
            log_capables_details += data.GetDetails() + "\n";

            // we add the id to the world unique ids registry to avoid generating the same id for another data
            World.LazyInstance.RegisterUniqueID(data.id);
        }

        if (log_world_data_loading) { Debug.Log("(CapableEngine) WORLD CAPABLES DATA LOADED : " + world_capables_data.Count + log_capables_details); }
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

            // we set the data's runtime capable ref to this capable since it's the one that is in the world
            capable.data.OnLoaded(capable);
        }

        if (log_world_data_loading && enabled_outsiders.Count > 0) { Debug.Log("(CapableEngine) OUTSIDERS DETECTED : " + enabled_outsiders.Count + "\n - " + string.Join("\n - ",enabled_outsiders)); }


        // we remove maybe the data of some outsiders (disabled + cursor)
        string log_removed = "";
        int removed_count = 0;
        for (int i=0; i<to_remove_maybe.Count; i++)
        {
            if (!remove_outsider_from_world(to_remove_maybe[i])) { continue; }
            log_removed += to_remove_maybe[i].data.id + "\n";
            removed_count++;
        }
        if (log_world_data_loading && removed_count > 0) { Debug.Log("(CapableEngine) OUTSIDERS REMOVED : " + removed_count + "\n - " + log_removed); }
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
        if (ChunkEngine.Instance.IsInAChunk(data.id)) { return false; }

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





    ///
    //
    /// 2. SPAWNING / SWITCHING / DROPPING / GRABBING CAPABLES
    //
    ///


    // DATA MANAGMENT

    /// <summary>
    /// duplicates a data from a template
    /// (useful for spawning capables)
    /// </summary>
    /// <param name="base_data"></param>
    /// <returns></returns>
    public CapableData DuplicateTemplate(string template)
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
        new_data.id = World.Instance.GenerateUniqueID(base_data.id);

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

        if (log_duplicating) { Debug.Log($"(CapableEngine) Duplicated {template} data to {new_data.id} \n {new_data.GetDetails()}"); }


        return new_data;
    }
    public CapableData DuplicateExistingData(string existing_id)
    {
        // we get the base data
        if (!world_capables_data.ContainsKey(existing_id))
        {
            if (!hide_log_no_data_found || log_duplicating) { Debug.LogWarning("(CapableSystem - DuplicateExistingData) Existing capable data not found for id: " + existing_id); }
            return null;
        }

        // 1. we duplicate the data into new data
        ICapableData base_data = world_capables_data[existing_id];
        CapableData new_data = base_data.Duplicate() as CapableData;
        new_data.id = World.Instance.GenerateUniqueID(base_data.id);

        // generate a hash
        generate_runtime_id(new_data.id);

        // we add the new_data to the world data list
        world_capables_data.Add(new_data.id, new_data);

        // 2. we spawn capacities from this new data
        for (int i = 0; i < new_data.capacities_ids.Count; i++)
        {
            string capa_template = new_data.capacities_ids[i];
            CapacityEngine.Instance.SpawnCapacityFromExistingOne(capa_template, new_data); // this replace the capacity id in the entity data
        }

        if (log_duplicating) { Debug.Log($"(CapableEngine) Duplicated existing data for id {existing_id} to {new_data.id} \n {new_data.GetDetails()}"); }


        return new_data;
    }

    /// <summary>
    /// This method DO NOT add the capable data to the world !
    /// But np it duplicates the data
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    public CapableData GetTemplateData(string template)
    {
        // we get the base data
        if (!templates_capables_data.ContainsKey(template))
        {
            if (!hide_log_no_data_found || log_duplicating) { Debug.LogWarning("(CapableSystem - GetTemplateData) Template capable data not found for id: " + template); }
            return null;
        }

        // we duplicate the data into new data
        ICapableData base_data = templates_capables_data[template];
        return (CapableData) base_data.Duplicate();
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
        
        if (ownership_check_done || log_start_capacity_ownership) { Debug.Log($"(CapableEngine)      LINKS BROKEN : {broken_links}/{total_links} ({broken_percentage}%)   |   FIXED : {fixed_links}/{broken_links}   |   ORPHAN : {orphan_capacities}  \n{log_summary}"); }

        return final_broken_links == 0;
    }

    // SPAWNING CAPABLES
    public Capable SpawnCapable(string template)
    {
        return SpawnCapable(DuplicateTemplate(template));
    }
    public Capable SpawnCapable(CapableData data)
    {
        if (data == null)
        {
            if (log_spawning) { Debug.LogError($"(CapableSystem - SpawnCapable) Failed to spawn capable. Data is null."); }
            return null;
        }
        // if (log_spawning) { Debug.Log($"(CapableEngine) Spawning {data.id} entity"); }

        // 1. we load the new spawned capable & set position
        Capable spawned_capable = load_capable(data);

        // 2. we fire events
        OnCapableAppear?.Invoke(data);
        OnCapableSpawned?.Invoke(data);

        if (log_spawning) { Debug.Log($"(CapableEngine) Spawned {data.id}"); }

        // 3. we return the spawned capable
        return spawned_capable;
    }
    public void DespawnCapable(CapableData cdata)
    {
        // UNLOAD THE CAPABLE
        OnCapableDisappear?.Invoke(cdata);
        OnCapableDespawned?.Invoke(cdata);
        unload_capable(cdata.id);
    }

    // ITEMS EVENTS
    public void OnItemDropped(Item item) { OnCapableAppear?.Invoke(item.data); }
    public void OnItemGrabbed(Item item) { OnCapableDisappear?.Invoke(item.data); }

    // TURN CAPABLE TO ITEM
    public void TurnToItem(Capable capable, string item_template, string anim_capacity_to_play="idle")
    {
        // we check that this is a TurnableIntoItem capable
        TurnableIntoItem turnable = capable as TurnableIntoItem;

        // 1. DROP ALL ITEMS
        if (capable.Inventory != null && capable.Inventory.Count > 0)
        {
            // we make the capable drop all its items and we wait for it to be done
            capable.DropAllItems(turnable?.DropParameters);
        }

        // 2. SAVE CAPABLE DATA
        capable.SaveDynamicData();
        CapableData capable_data = capable.data;
        List<Force> forces = new List<Force>((capable as Movable)?.GetForces() ?? new List<Force>()); // duplicate the forces

        // 3. SPAWN THE ITEM DATA
        ItemData item_data = DuplicateTemplate(item_template) as ItemData;
        item_data.InitFromCapable(capable_data, turnable?.ItemDataInfo); // we transfer some of the capable data to the item data (ex : position, orientation, tag, skin if we have anim_data, etc)

        // 5. SPAWN THE ITEM
        Item item = SpawnCapable(item_data) as Item;
        if (log_item_switching) { Debug.Log($"(CapableSystem - Turning) Switched {capable.ID} to item {item.ID} \n - Capable data : \n{capable_data.GetDetails()} \n - Item data : \n{item_data.GetDetails()}"); }
        item.AnimPlayer.Play(anim_capacity_to_play);

        // 4. DESPAWN THE CAPABLE
        DespawnCapable(capable_data);

        // 6. TRANSFER FORCES
        item.SetForces(forces);
    }
    public void TurnToSomething(Capable capable, string template_for_new_capable)
    {
        // we check that this is a TurnableIntoSomething capable
        TurnableIntoSomething turnable = capable as TurnableIntoSomething;

        // 1. DROP ALL ITEMS
        if (capable.Inventory != null && capable.Inventory.Count > 0)
        {
            // we make the capable drop all its items and we wait for it to be done
            capable.DropAllItems(turnable?.DropParameters);
        }

        // 2. SAVE CAPABLE DATA
        capable.SaveDynamicData();
        CapableData capable_data = capable.data;
        List<Force> forces = new List<Force>((capable as Movable)?.GetForces() ?? new List<Force>()); // duplicate the forces

        // 3. CREATE THE SOMETHING DATA
        CapableData new_capable_data = DuplicateTemplate(template_for_new_capable);
        if (new_capable_data == null)
        {
            if (log_item_switching || log_spawning) { Debug.LogError($"(CapableSystem - Turning) Failed to switch {capable.ID} to {template_for_new_capable}. Template data not found."); }
            return;
        }
        new_capable_data.position = capable_data.Position;

        // 4. SPAWN THE SOMETHING
        Capable new_capable = SpawnCapable(new_capable_data);
        if (log_item_switching) { Debug.Log($"(CapableSystem - Turning) Switched {capable.ID} to something {new_capable.ID} \n - Capable data : \n{capable_data.GetDetails()} \n - Something data : \n{new_capable_data.GetDetails()}"); }

        // 5. DESPAWN THE CAPABLE
        DespawnCapable(capable_data);

        // 6. TRANSFER FORCES
        if (new_capable is Movable movable) { movable.SetForces(forces); }
    }





    ///
    //
    /// 3. DYNAMIC LOADING & UNLOADING OF CAPABLES
    //
    ///


    // LOAD CAPABLES
    public Capable LoadCapableInstantly(string id)
    {
        // if the capable is in the unloading queue, it means it is already loaded,
        // so we remove it from unloading queue and simply return it
        if (unloading_queue.Contains(id))
        {
            unloading_queue.Remove(id);
            Capable capable = CapableBank.Instance.GetLoadedCapable(id);

            if (log_loading) { Debug.Log("(CapableEngine) Already loaded " + id); }
            return capable;
        }

        // we remove the capable from loading queue
        if (loading_queue.Contains(id)) { loading_queue.Remove(id); }

        // and we finally load it
        return load_capable(id, duplicate_if_template: true);
    }

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
    private Capable load_capable(string id, bool duplicate_if_template = false)
    {
        if (!world_capables_data.ContainsKey(id))
        {
            if (duplicate_if_template)
            {
                // we check if it's a template, if yes we duplicate it and load the duplicate
                if (templates_capables_data.ContainsKey(id))
                {
                    CapableData new_data = DuplicateTemplate(id);
                    if (log_spawning) { Debug.Log($"(CapableEngine - Load) Capable id {id} is a template, we duplicated it to {new_data.id} and loading the duplicate"); }
                    Capable capable = load_capable(new_data);
                    OnCapableAppear?.Invoke(capable.data);
                    return capable;
                }
            }

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
        if (log_loading) { Debug.Log("(CapableEngine) Loaded " + data.id); }

        hide_show_capable_on_load(capable, data);

        return capable;        
    }
    
    /// <summary>
    /// this helper low level method is called when we load a capable,
    /// it checks if the capable's room is visible or not, and hide/show the capable accordingly.
    /// if the capable is a door it makes it visible if at least one of the 2 rooms is visible
    /// </summary>
    /// <param name="capable"></param>
    /// <param name="data"></param>
    private void hide_show_capable_on_load(Capable capable, CapableData data)
    {
        // check game state, if we are in GameState.Building then we always show
        if (GameManager.State == GameState.Building)
        {
            capable.AnimPlayer.Show();
            if (log_loading_extended) { Debug.Log($"(CapableSystem - Load) Game state is Building --> SHOWING CAPABLE {data.id}"); }
            return;
        }

        // ChunkData chunk;
        if (data is DoorData ddata)
        {
            // we get the 2 rooms of the door
            // chunk = ChunkEngine.Instance.GetChunkDataFromID(ddata.room1_id);
            if (RoomEngine.Instance.DoorEngine.IsRoomVisible(ddata.room1_id))
            {
                capable.AnimPlayer.Show();
                if (log_loading_extended) { Debug.Log($"(CapableSystem - Load) Door {data.id} ({ddata.room1_id} - {ddata.room2_id}) has room1 visible --> SHOWING DOOR"); }
                return;
            }
            // chunk = ChunkEngine.Instance.GetChunkDataFromID(ddata.room2_id);
            if (RoomEngine.Instance.DoorEngine.IsRoomVisible(ddata.room2_id))
            {
                capable.AnimPlayer.Show();
                if (log_loading_extended) { Debug.Log($"(CapableSystem - Load) Door {data.id} ({ddata.room1_id} - {ddata.room2_id}) has room2 visible --> SHOWING DOOR"); }
                return;
            }

            // else both rooms are not visible, we hide the door
            capable.AnimPlayer.Hide();
            if (log_loading_extended) { Debug.Log($"(CapableSystem - Load) Door {data.id} ({ddata.room1_id} - {ddata.room2_id}) has both rooms not visible --> HIDING DOOR"); }
            return;
        }


        // else the capable is not a door.

        // check if capable needs to be hidden bcz it is in a not visible room
        ChunkData chunk;
        ChunkEngine.Instance.TryGetCapableChunk(data.id, out chunk);
        if (chunk == null)
        {
            // we check if the capable has a sit capacity
            if (!capable.TryGetCapacity(out SitCapacity sit_capa))
            {
                if (log_visibility) { Debug.LogWarning($"(CapableSystem - Load) Capable {data.id} is not in any room ?! --> CANT SHOW / HIDE"); }
                return;
            }
            if (sit_capa.CurrentSofa == null)
            {
                if (log_visibility) { Debug.LogWarning($"(CapableSystem - Load) Capable {data.id} has a SitCapacity but is not currently sitting on any sofa ?! --> CANT SHOW / HIDE"); }
                return;
            }
            if (!ChunkEngine.Instance.TryGetCapableChunk(sit_capa.CurrentSofa.ID, out chunk))
            {
                if (log_visibility) { Debug.LogWarning($"(CapableSystem - Load) Capable {data.id} is sitting on sofa {sit_capa.CurrentSofa.ID} but we cant find the chunk of this sofa ?! --> CANT SHOW / HIDE"); }
                return;
            }
        }

        if (!RoomEngine.Instance.DoorEngine.IsRoomVisible(chunk.room_id))
        {
            capable.AnimPlayer.Hide();
            if (log_visibility) { Debug.Log($"(CapableSystem - Load) Capable {data.id} is in room {chunk.id} which is not visible --> HIDING CAPABLE"); }
        }
        else { capable.AnimPlayer.Show(); }
    }



    // UNLOAD CAPABLES
    public void UnloadCapableInstantly(string id)
    {
        // if the capable is in the loading queue, it means it is already unloaded,
        // so we remove it from loading queue
        if (loading_queue.Contains(id))
        {
            loading_queue.Remove(id);
            return;
        }

        // we remove the capable from unloading queue
        if (unloading_queue.Contains(id)) { unloading_queue.Remove(id); }

        // and we finally unload it
        unload_capable(id);
        return;
    }

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

            // verify that the capable is not controlled, if yes we skip it
            /* if (Controller.Capable != null && Controller.Capable.ID == id)
            {
                if (log_loading) { Debug.Log($"(CapableSystem - UnloadCapables) Skipping unloading of {id} since it is currently controlled by the player"); }
                continue;
            } */
            
            // we remove them from the loading_queue if they are inside it (so no need for unloading them)
            if (loading_queue.Contains(id)) { loading_queue.Remove(id); }

            // else we add them to unloading queue
            else if (!unloading_queue.Contains(id)) { unloading_queue.Add(id); }
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
        if (log_loading) { Debug.Log("(CapableEngine) Unloaded " + id); }
        return capable;
    }




    ///
    //
    /// UPDATE & UPDATING DATA
    //
    ///

    // UPDATE
    private float _update_positions_timer = 0f;
    private float update_positions_interval = 1f;
    private void Update()
    {
        if (!world_data_loaded) { return; }
        
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
            try
            {
                data.SetPosition(capable.transform.position);
            }
            catch (Exception e)
            {
                if (!hide_log_no_data_found) { Debug.LogWarning($"(CapableSystem - update_loaded_capables_positions) Failed to update position for capable {id}. Exception: {e}"); }
            }
        }
    }


    // SAVE LOADED CAPABLE DYNAMIC DATA
    public void SaveLoadedCapablesDynamicData()
    {
        List<Capable> capables = CapableBank.LazyInstance.GetAllLoadedCapables();
        foreach (Capable capable in capables)
        {
            if (capable == null) { continue; }
            capable.SaveDynamicData();
        }
        if (log_saving) { Debug.Log($"(CapableSystem) Saved dynamic data for {capables.Count} loaded capables"); }
    }





    ///
    //
    /// 4. GETTERS & OTHERS
    //
    ///


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
    public Dictionary<CapableData, Capable> GetOutsidersWorldCapablesData()
    {
        return outsiders_data;
    }
    public bool IsOutsider(string id) { return outsiders_ids.Contains(id); }
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
    public bool IsMovable(string id)
    {
        if (!world_capables_data.TryGetValue(id, out CapableData data))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - IsMovable) Capable data not found for id: " + id); }
            return false;
        }

        return GameManager.IsKind(data.kind,"Movable");
    }
    public CapableData GetCapableDataFromID(string id)
    {
        if (!world_capables_data.TryGetValue(id, out CapableData data))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapableSystem - GetCapableDataFromID) Capable data not found for id: " + id); }
            return null;
        }
        return data;
    }
    public Vector2 GetCapablePosition(string id)
    {
        // check if loaded then we return the position of the transform (more precise and up to date)
        if (HasLoadedCapableData(id))
        {
            Capable capable = CapableBank.Instance.GetLoadedCapable(id);
            if (capable != null) { return capable.transform.position; }
        }
        else if (TryGetOutsider(id, out Capable outsider)) // if loaded & outsider we can't use the bank, so we use this
        {
            return outsider.transform.position;
        }

        // else return the position from the data (may be outdated)
        CapableData data = GetCapableDataFromID(id);
        if (data == null) { return Vector2.zero; }
        return data.position;
    }

    // doors
    public List<DoorData> GetWorldDoorsData()
    {
        List<DoorData> doors_data = new List<DoorData>();
        foreach (KeyValuePair<string, CapableData> pair in world_capables_data)
        {
            CapableData data = pair.Value;
            if (data is not DoorData door_data) { continue; }
            doors_data.Add(door_data);
        }
        return doors_data;
    }


    // STATIC GETTERS
    public static List<CapableData> LoadWorldCapablesData(string world_id, List<string> capables_ids)
    {
        if (string.IsNullOrEmpty(world_id) || capables_ids == null || capables_ids.Count <= 0) { return new List<CapableData>(); }
        WorldSaveData save = SaveEngine.GetWorldSave(world_id);
        if (save == null) { return new List<CapableData>(); }

        List<CapableData> capables_data = new List<CapableData>();
        foreach (CapableData data in save.capables)
        {
            if (data == null) { continue; }
            if (!capables_ids.Contains(data.id)) { continue; }
            capables_data.Add(data);
        }
        return capables_data;
    }
    public static CapableData LoadWorldCapableData(string world_id, string item_id)
    {
        if (string.IsNullOrEmpty(world_id)) { return null; }
        WorldSaveData save = SaveEngine.GetWorldSave(world_id);
        if (save == null) { return null; }
        return save.capables.Find(c => c.id == item_id);
    }

}