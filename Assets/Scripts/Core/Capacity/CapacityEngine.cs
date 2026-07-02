#pragma warning disable 1998

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class CapacityEngine : BSOD_System<CapacityEngine>
{


    ///
    //
    /// SUB SYSTEMS REFERENCES
    //
    ///

    private InputIndicationEngine _interaction_indication_engine;
    public InputIndicationEngine InteractionIndicationEngine
    {
        get
        {
            if (_interaction_indication_engine == null) { _interaction_indication_engine = GetComponentInChildren<InputIndicationEngine>(includeInactive: true); }
            return _interaction_indication_engine;
        }
    }
    public InputIndicationEngine II_Engine { get { return InteractionIndicationEngine; } }



    ///
    //
    /// VARIABLES
    //
    ///

    [Header("World Capacities data")]
    public Dictionary<string, CapacityData> world_capacities_data = new Dictionary<string, CapacityData>();

    [Header("Loaded Capacities data")]
    public Dictionary<string,CapacityData> loaded_capacities_data = new Dictionary<string,CapacityData>();

    [Header("State")]
    public bool worlddata_loaded_done = false;

    [Header("Settings")]
    public bool auto_repair_owner_links_on_load = false;


    [Header("Logs - Awake")]
    public bool log_templates_data_loading = false;
    public bool log_world_data_loading = false;
    public bool log_awake_data_extended = false;

    [Header("Logs - Spawning")]
    public bool log_spawning = false;

    [Header("Logs - Loading")]
    public bool log_loading = false;
    public bool log_loading_extended = false;
    public bool hide_log_no_data_found = false;

    [Header("Logs Saving")]
    public bool log_saving = false;

    ///
    //
    /// EVENTS
    //
    ///

    public Action<CapacityData> OnCapacitySpawned;
    public Action<CapacityData> OnCapacityDespawned;





    ///
    //
    /// LOAD / UNLOAD WORLD DATA
    //
    ///

    // LOAD / UNLOAD WORLD DATA
    public override async Task LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(CapacityEngine) Loading world data for world_id: {world_id}"); }

        // load templates capacities data
        if (!templates_loaded)
        {
            loadTemplatesCapacitiesData();
            if (log_templates_data_loading) { Debug.Log($"(CapacityEngine) Loaded {templates_capacities_data.Count} templates capacities data"); }
        }

        // load world capacities data
        LoadWorldCapacitiesData(world_id);
        if (log_world_data_loading) { Debug.Log($"(CapacityEngine) Loaded {world_capacities_data.Count} world capacities data"); }

        worlddata_loaded_done = true;


        // we can then call specific sub systems that require to gather some capacities
        CapableEngine.EcoEngine.GatherNests(log);
        if (log) { Debug.Log($"(CapacityEngine) Made EcoEngine grab the NestData I just loaded :D"); }


        if (log) { Debug.Log($"(CapacityEngine) CAPACITY ENGINE SUCCESSFULLY LOADED : {world_id}"); }
    }
    public override async Task UnloadWorldData(bool log)
    {
        CapacityBank.Instance.DestroyAllCapacitiesInstantly();
        loaded_capacities_data.Clear();
        world_capacities_data.Clear();

        worlddata_loaded_done = false;
        if (log) { Debug.Log($"(CapacityEngine) CAPACITY ENGINE SUCCESSFULLY UNLOADED"); }
    }



    // LOAD TEMPLATES & WORLD DATA
    private bool templates_loaded = false;
    private string templates_data_path = "data/templates/capacities/";
    private Dictionary<string, CapacityData> templates_capacities_data = new Dictionary<string, CapacityData>();
    protected void loadTemplatesCapacitiesData()
    {
        // we empty the capacities_data
        templates_capacities_data = new Dictionary<string, CapacityData>();
        string log_capacities_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = AppManager.LoadJsonsFromAssets(templates_data_path);
        foreach (string json in files)
        {
            CapacityData data = SaveEngine.LoadCapacityDataWithGoodKind(json);
            if (log_awake_data_extended) { Debug.Log($"(CapacityEngine) Loading capacity data template for '{data.id}' of type {data.GetType().Name}: \n{data.GetDetails()}"); }
            templates_capacities_data.Add(data.id, data);
            log_capacities_details += data.GetDetails() + "\n";
        }

        if (log_templates_data_loading) { Debug.Log("(CapacityEngine) TEMPLATES CAPACITIES DATA LOADED : " + templates_capacities_data.Count + log_capacities_details); }
        templates_loaded = true;
    }
    public void LoadWorldCapacitiesData(string world_id)
    {
        WorldSaveData world_save = SaveEngine.GetWorldSave(world_id);
        if (world_save == null)
        {
            Debug.LogWarning($"(CapableEngine) World save data not found for world_id: {world_id}. No world capables data loaded.");
            return;
        }

        // we empty the capacities_data
        world_capacities_data = new Dictionary<string,CapacityData>();
        string log_capacities_details = "\n\n";

        // we load all the json files in the data path and get their kind
        List<CapacityData> world_capacities = world_save.capacities;
        foreach (CapacityData data in world_capacities)
        {
            if (log_awake_data_extended) { Debug.Log($"(CapacityEngine) Loading world capacity data for '{data.id}' of type {data.GetType().Name}: \n{data.GetDetails()}"); }
            world_capacities_data.Add(data.id, data);
            log_capacities_details += data.GetDetails() + "\n";

            // we add the id to the world unique ids registry to avoid generating the same id for another data
            World.LazyInstance.RegisterUniqueID(data.id);
        }

        if (log_world_data_loading) { Debug.Log("(CapacityEngine) WORLD CAPACITIES DATA LOADED : " + world_capacities_data.Count + log_capacities_details); }
    }


    ///
    //
    /// TEMPLATE DUPLICATION & CAPACITY SPAWNING 
    //
    ///

    // DATA DUPLICATION
    private CapacityData DuplicateTemplate(string template)
    {
        // we get the base data
        if (!templates_capacities_data.ContainsKey(template))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - DuplicateTemplate) Template capacity data not found for id: " + template); }
            return null;
        }

        CapacityData base_data = templates_capacities_data[template];
        CapacityData new_data = base_data.Duplicate() as CapacityData;
        new_data.id = World.Instance.GenerateUniqueID(base_data.id); // automatically register the new id

        // we add the new_data to the data list
        world_capacities_data.Add(new_data.id, new_data);
        return new_data;
    }
    private CapacityData DuplicateExistingCapacity(string existing_id)
    {
        // we get the base data
        if (!world_capacities_data.ContainsKey(existing_id))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - DuplicateExistingCapacity) Existing capacity data not found for id: " + existing_id); }
            return null;
        }

        CapacityData base_data = world_capacities_data[existing_id];
        CapacityData new_data = base_data.Duplicate() as CapacityData;
        new_data.id = World.Instance.GenerateUniqueID(base_data.id); // automatically register the new id

        // we add the new_data to the data list
        world_capacities_data.Add(new_data.id, new_data);
        return new_data;
    }

    // SPAWN CAPACITIES
    public CapacityData SpawnCapacity(string template_id, CapableData cdata)
    {
        // we duplicate the data + generate unique id
        CapacityData data = DuplicateTemplate(template_id);
        if (data == null)
        {
            if (log_spawning) { Debug.LogWarning($"(CapacityEngine - Spawn) Failed to spawn capacity from template '{template_id}' for '{cdata.id}' because template not found"); }
            return null;
        }

        set_owner_and_capacity_ids(template_id, data, cdata);
        if (log_spawning) { Debug.Log($"(CapacityEngine - Spawn) New Capacity '{data.id}' was created from template '{template_id}' and assigned to '{cdata.id}'"); }

        OnCapacitySpawned?.Invoke(data);

        // we return the capacity data
        return data;
    }
    public CapacityData SpawnCapacityFromExistingOne(string cap_id, CapableData cdata)
    {
        // we duplicate the data + generate unique id
        CapacityData data = DuplicateExistingCapacity(cap_id);
        if (data == null)
        {
            if (log_spawning) { Debug.LogWarning($"(CapacityEngine - Spawn) Failed to spawn capacity from existing capacity '{cap_id}' for '{cdata.id}' because existing capacity was not found"); }
            return null;
        }

        set_owner_and_capacity_ids(cap_id, data, cdata);
        if (log_spawning) { Debug.Log($"(CapacityEngine - Spawn) New Capacity '{data.id}' was created from existing capacity '{cap_id}' and assigned to '{cdata.id}'"); }

        OnCapacitySpawned?.Invoke(data);

        // we return the capacity data
        return data;
    }
    private void set_owner_and_capacity_ids(string base_capacity_id, CapacityData data, CapableData cdata)
    {
        // we change the capacity id in the capable data
        for (int i = 0; i < cdata.capacities_ids.Count; i++)
        {
            // check if same id
            if (cdata.capacities_ids[i] != base_capacity_id) { continue; }

            // else change the id to new capacity id
            cdata.capacities_ids[i] = data.id;
            break;
        }

        // finally we set the capacity owner id to the capable id
        data.owner_id = cdata.id;
    }

    // DESPAWN CAPACITY
    public void DespawnCapacityClean(Capacity capacity)
    {
        if (capacity == null) { return; }
        DespawnCapacityAndCleanOwnerLink(capacity.data.id, capacity.data.owner_id);
    }
    public void DespawnCapacityAndCleanOwnerLink(string capacity_id, string capable_id)
    {
        // we ensure we have the data and that it is unloaded
        if (!world_capacities_data.ContainsKey(capacity_id)) { return; }
        if (loaded_capacities_data.ContainsKey(capacity_id))
        {
            Capacity capacity = CapacityBank.Instance.GetLoadedCapacity(capacity_id);
            Capable capable = CapableBank.Instance.GetLoadedCapable(capable_id);
            if (capacity != null && capable != null)
            {
                UnloadCapacities(new List<string>() { capacity_id }, capable);
            }
            else
            {
                // the capable (n capacity) is not loaded.
                // so we just ensure the capacity id is removed from the capable data
                // so it won't load next time the capable is loaded
                CapableData cdata = CapableEngine.Instance.GetCapableDataFromID(capable_id);
                if (cdata != null && cdata.capacities_ids != null && cdata.capacities_ids.Contains(capacity_id))
                {
                    cdata.capacities_ids.Remove(capacity_id);
                }
            }
        }

        // now the capacity is unloaded and unregistered from capable, we can safely remove the data
        OnCapacityDespawned?.Invoke(world_capacities_data[capacity_id]);
        world_capacities_data.Remove(capacity_id);
        World.Instance.UnregisterUniqueID(capacity_id);
        if (log_spawning) { Debug.Log($"(CapacityEngine - Despawn) Capacity '{capacity_id}' despawned and data removed from world data"); }
    }

    /// <summary>
    /// This method is the NOT CLEAN
    /// equivalent of above, which means
    /// we don't clean everything (owner link)
    /// because the owner CapableData is going
    /// to be despawned just after so no need.
    /// If you want to properly clean everything,
    /// use above methods
    /// </summary>
    /// <param name="capacity_id"></param>
    public void DespawnCapacity(string capacity_id)
    {
        if (!world_capacities_data.ContainsKey(capacity_id)) { return; }

        // unload the capacity if loaded
        if (loaded_capacities_data.ContainsKey(capacity_id))
        {
            Capacity capacity = CapacityBank.Instance.GetLoadedCapacity(capacity_id);
            string owner_id = world_capacities_data[capacity_id].owner_id;
            Capable owner = CapableBank.Instance.GetLoadedCapable(owner_id);
            UnloadCapacities(new List<string>() { capacity_id }, owner);
        }

        // now the capacity is unloaded, we remove the data
        OnCapacityDespawned?.Invoke(world_capacities_data[capacity_id]);
        world_capacities_data.Remove(capacity_id);
        World.Instance.UnregisterUniqueID(capacity_id);
        if (log_spawning) { Debug.Log($"(CapacityEngine - Despawn) Capacity '{capacity_id}' WAS DESPAWN VIOLENTLY (not clean)"); }
    }


    ///
    //
    /// LOAD / UNLOAD CAPACITIES / SAVE DYNAMIC DATA
    //
    ///


    // LOAD CAPACITIES
    public List<Capacity> LoadCapacities(List<string> capacities_ids, Capable capable)
    {
        List<Capacity> capacities = new List<Capacity>();
        for (int i = 0; i < capacities_ids.Count; i++)
        {
            string id = capacities_ids[i];
            Capacity capa = load_capacity(id,capable.data);

            if (capa == null) { continue; } // we may have skipped this capacity if it was already loaded
            
            // we register the capacity into the capable
            capacities.Add(capa);
            capable.RegisterCapacity(capa);

            // we set the parent & local pos
            capa.transform.SetParent(capable.transform);
            capa.transform.localPosition = capa.data.local_position;
        }
        return capacities;
    }
    private Capacity load_capacity(string id, CapableData capable_data)
    {
        if (!world_capacities_data.ContainsKey(id))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - Load) Capacity data not found for id: " + id); }
            return null;
        }
        CapacityData data = world_capacities_data[id];
        return load_capacity(data, capable_data);
    }
    private Capacity load_capacity(CapacityData data, CapableData capable_data)
    {
        // we check if we already have this data in our loaded data
        if (loaded_capacities_data.ContainsKey(data.id))
        {
            if (log_loading) { Debug.LogWarning($"(CapacityEngine - Load) Skipped capacity '{data.id}' for '{capable_data.id}' because already loaded"); }
            return null;            
        }

        // we check that the capacity owner id is the same as the capable id
        bool link_ok = CapableEngine.Instance.ValidateOwnershipLinks(capable_data, data, repair : auto_repair_owner_links_on_load, repair_removes_duplicates : false);
        if (!link_ok)
        {
            if (log_loading_extended) { Debug.LogWarning($"(CapacityEngine - Load) Capacity '{data.id}' owner id '{data.owner_id}' does not match capable id '{capable_data.id}' for '{capable_data.id}' (if they matches, it means there are some Duplicates)"); }
            // return null;
        }

        if (log_loading_extended) { Debug.Log($"(CapacityEngine - Load) Loading capacity '{data.id}' \n{data.GetDetails()}"); }

        Capacity capacity = CapacityBank.Instance.Load(data, capable_data);
        loaded_capacities_data.Add(data.id, data);
        if (log_loading) { Debug.Log("(CapacityEngine) Loaded " + data.id); }
        return capacity;
    }

    // UNLOAD CAPACITIES
    public void UnloadCapacities(List<string> capacities_ids, Capable capable)
    {
        for (int i = 0; i < capacities_ids.Count; i++)
        {
            string id = capacities_ids[i];
            Capacity capa = unload_capacity(id);

            // we unregister the capacity in the capable
            capable.UnregisterCapacity(capa);
        }
    }
    private Capacity unload_capacity(string id)
    {
        if (!loaded_capacities_data.ContainsKey(id))
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - Unload) Loaded capacity data not found for id: " + id); }
            return null;
        }

        CapacityData data = loaded_capacities_data[id];

        // we found the data, we unload it
        Capacity capacity = CapacityBank.Instance.Unload(data);
        loaded_capacities_data.Remove(id);
        if (log_loading) { Debug.Log("(CapacityEngine) Unloaded " + id); }
        return capacity;
    }


    // SAVE LOADED CAPACITIES DYNAMIC DATA
    public void SaveLoadedCapacitiesDynamicData()
    {
        List<Capacity> capacities = CapacityBank.Instance.GetAllLoadedCapacities();
        foreach (Capacity capacity in capacities)
        {
            if (capacity == null) { continue; }
            capacity.SaveDynamicData();
        }
        if (log_saving) { Debug.Log($"(CapacitySystem) Saved dynamic data for {capacities.Count} loaded capacities"); }
    }



    ///
    //
    /// GETTERS 
    //
    ///


    // GETTERS
    [Header("Item Static Capacities Kinds")]
    // these capacities kinds are loaded / unloaded along side with the item loading / unloading.
    // this means that the kinds that ARE NOT in this list will be dynamically loaded / unloaded
    // when the item is dropped / grabbed
    [SerializeField] private List<string> item_static_capacities_kinds = new List<string>() { "DodgeCapacity" };
    private Dictionary<string, string> item_dynamic_required_capacities_templates_per_kinds = new Dictionary<string, string>() {
        { "HoverCapacity", "hover" }
    };
    public List<string> GetDynamicItemCapacitiesIDs(ItemData idata, out List<string> static_ids)
    {
        List<string> capa_ids = new List<string>(idata.capacities_ids);
        static_ids = new List<string>();

        // 1. we get the base capable capacities ids
        List<string> dynamically_pooled_ids = new List<string>();

        // 2. we only check on items
        // if (capable is not Item) { return capable.data.capacities_ids; } ! no need for now bcz we only call method from Item

        List<string> required_kinds = new List<string>(item_dynamic_required_capacities_templates_per_kinds.Keys);

        // 3. we filter it with the list of static capacity kinds
        // -> means we dynamically handle ONLY the kinds that ARE NOT in this list
        for (int i = 0; i < capa_ids.Count; i++)
        {
            string capa_id = capa_ids[i];
            if (!world_capacities_data.ContainsKey(capa_id))
            {
                if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - GetCapacitiesIDsToPoolDynamically) Capacity data not found for id: " + capa_id); }
                continue;
            }
            CapacityData capa_data = world_capacities_data[capa_id];

            // if we have a static capacity kind, we don't add it
            if (item_static_capacities_kinds.Contains(capa_data.kind))
            {
                static_ids.Add(capa_id);
                continue;
            }

            // else it is a dynamic one
            // we check if it is a required one so we don't add it later
            if (required_kinds.Contains(capa_data.kind)) { required_kinds.Remove(capa_data.kind); }
            
            // finally we add it
            dynamically_pooled_ids.Add(capa_id);
        }

        // 4. we check if we have all required kinds, if not we spawn them with the template id
        foreach (string required_kind in required_kinds)
        {
            string template_id = item_dynamic_required_capacities_templates_per_kinds[required_kind];
            
            // spawn the capacity with the template id
            CapacityData data = SpawnCapacity(template_id, idata); // we don't have the capable id at this point but it doesn't matter because we will change the owner id later when we assign the capacity to the capable
            dynamically_pooled_ids.Add(data.id);
        }

        return dynamically_pooled_ids;
    }
    public CapacityData GetCapacityData(string id)
    {
        world_capacities_data.TryGetValue(id, out CapacityData data);
        if (data != null) { return data; }
        
        if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - GetCapacityData) Capacity data not found for id: " + id); }
        return null;
    }
    public List<CapacityData> GetCapacitiesDataFromIDs(List<string> ids)
    {
        List<CapacityData> datas = new List<CapacityData>();
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            CapacityData data = GetCapacityData(id);
            if (data != null) { datas.Add(data); }
        }
        return datas;
    }
    public CapacityData GetTemplateData(string id)
    {
        if (!templates_capacities_data.TryGetValue(id, out CapacityData data)) { return null; }
        return data;
    }
    public List<T> GetCapacitiesDataOfKind<T>() where T : CapacityData
    {
        // todo : cache this maybe ?
        List<T> gathered_capa = new List<T>();
        foreach (var kvp in world_capacities_data)
        {
            if (kvp.Value is not T capaciT) { continue; }
            gathered_capa.Add(capaciT);
        }
        return gathered_capa;
    }
    public T GetCapacityOfTypeOfCapable<T>(CapableData owner) where T : CapacityData
    {
        if (owner == null) { return null; }
        if (owner.capacities_ids == null) { return null; }
        if (owner.capacities_ids.Count == 0) { return null; }

        foreach (string cid in owner.capacities_ids)
        {
            CapacityData data = GetCapacityData(cid);
            if (data == null) { continue; }
            if (data is not T dataT) { continue; }
            return dataT;
        }
        return null;
    }
}