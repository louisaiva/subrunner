using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CapacityEngine : BSOD_System<CapacityEngine>
{
    [Header("Capacities data")]
    private string data_path = "Assets/Resources/data/capacities/";
    public Hashtable capacities_data = new Hashtable();

    [Header("Loading / Unloading")]
    public Hashtable loaded_capacities_data = new Hashtable();

    [Header("State")]
    public bool awake_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;
    public bool log_loading = false;
    public bool hide_log_no_data_found = false;



    // AWAKE
    public override void Awake()
    {
        base.Awake();

        // load capacities data
        loadCapacitiesData();
    }

    // LOAD / UNLOAD DATA
    protected void loadCapacitiesData()
    {
        // we empty the capacities_data
        capacities_data = new Hashtable();
        string log_capacities_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = System.IO.Directory.GetFiles(data_path, "*.json");
        Dictionary<string, string> capacities_json_by_kind = new Dictionary<string, string>();
        foreach (string file in files)
        {
            string json = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
            CapacityData data = JsonUtility.FromJson<CapacityData>(json);
            capacities_json_by_kind.Add(data.kind, json);
        }

        // then we go through all json & kind and we load the json with the good type
        foreach (KeyValuePair<string, string> entry in capacities_json_by_kind)
        {
            string kind = entry.Key;
            string json = entry.Value;
            loadCapacityDataOfType(json, kind, ref log_capacities_details);
        }

        if (log_awake_data) { Debug.Log("(CapacityEngine) CAPACITIES DATA LOADED : " + capacities_data.Count + log_capacities_details); }
        awake_done = true;
    }
    private void loadCapacityDataOfType(string json, string kind, ref string log)
    {
        // if (log_awake_data) { Debug.Log($"(CapacityEngine - loadCapacityDataOfType) loading capacity of kind {kind} with json : {json}"); }

        Type type = Type.GetType(kind + "Data");
        if (type == null) { type = typeof(CapacityData); }
        CapacityData data = JsonUtility.FromJson(json, type) as CapacityData;
        capacities_data.Add(data.id, data);
        log += data.GetDetails() + "\n";
    }


    // LOAD CAPACITIES
    public List<Capacity> LoadCapacities(List<string> capacities_ids, Capable capable, bool skip_if_loaded=false)
    {
        List<Capacity> capacities = new List<Capacity>();
        for (int i = 0; i < capacities_ids.Count; i++)
        {
            string id = capacities_ids[i];
            Capacity capa = load_capacity(id,capable.data, skip_if_loaded);

            if (capa == null) { continue; }
            // we may have skipped this capacity for various reasons
            // ie. we load an Item which is grabbed, it does not need to have an hover
            // so we skipped hover
            
            capacities.Add(capa);
        }
        return capacities;
    }
    private Capacity load_capacity(string id, CapableData capable_data, bool skip_if_loaded = false)
    {
        CapacityData data = capacities_data[id] as CapacityData;
        if (data == null)
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - Load) Capacity data not found for id: " + id); }
            return null;
        }
        return load_capacity(data, capable_data,  skip_if_loaded);
    }
    private Capacity load_capacity(CapacityData data, CapableData capable_data, bool skip_if_loaded=false)
    {

        // we check if we REALLY want to load the capacity
        // if (skip_capacity_loading(data, capable_data)) { return null; }


        // we check if we already have this data in our loaded data
        if (loaded_capacities_data.ContainsKey(data.id))
        {
            if (skip_if_loaded)
            {
                if (log_loading) { Debug.LogWarning($"(CapacityEngine - Load) Skipped capacity '{data.id}' for '{capable_data.id}' because already loaded"); }
                return null;
            }

            // ? then we want to duplicate the data & change the capa_id & change the capa_id in capable.data.capacities
            string old_id = data.id;

            // we duplicate the data + generate unique id
            data = DuplicateData(data);

            // we change the capacity id in the capable data
            for (int i=0; i < capable_data.capacities_ids.Count; i++)
            {
                // check if same id
                if (capable_data.capacities_ids[i] != old_id) { continue; }

                // else change the id to new capacity id
                capable_data.capacities_ids[i] = data.id;
                break;
            }

            if (log_loading) { Debug.LogWarning($"(CapacityEngine - Load) Capacity '{old_id}' was already loaded, duplicated it to {data.id}"); }
        }

        Capacity capacity = CapacityBank.Instance.Load(data);
        loaded_capacities_data.Add(data.id, data);
        if (log_loading) { Debug.Log("(CapacityEngine) Loaded " + data.id); }
        return capacity;
    }
    /* private bool skip_capacity_loading(CapacityData data, CapableData capable_data)
    {
        // we only check on items
        if (!GameManager.Instance.IsKind(Type.GetType(capable_data.kind), typeof(Item))) { return false; }

        // and on grabbed items more specifically
        if (!(capable_data is ItemData item_data) || !item_data.is_grabbed) { return false; }

        // we only check for hover capacity (for now)
        if (!GameManager.Instance.IsKind(Type.GetType(data.kind), typeof(HoverCapacity))) { return false; }
        // todo inverse the check just above, we need to have a list<string> with kind
        // that don't need to be loaded when item is grabbed

        // we check if the capacity is hover and if the capable is grabbed, if yes we skip loading this capacity because it's not needed
        if (log_loading) { Debug.LogWarning($"(CapacityEngine - Load) Skipped loading capacity '{data.id}' for '{capable_data.id}' because hover & grabbed"); }
        return true;
    } */

    // UNLOAD CAPACITIES
    public void UnloadCapacities(List<string> capacities_ids)
    {
        for (int i = 0; i < capacities_ids.Count; i++)
        {
            string id = capacities_ids[i];
            unload_capacity(id);
        }
    }
    private Capacity unload_capacity(string id)
    {
        CapacityData data = loaded_capacities_data[id] as CapacityData;
        if (data == null)
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - Unload) Loaded capacity data not found for id: " + id); }
            return null;
        }

        // we found the data, we unload it
        Capacity capacity = CapacityBank.Instance.Unload(data);
        loaded_capacities_data.Remove(id);
        if (log_loading) { Debug.Log("(CapacityEngine) Unloaded " + id); }
        return capacity;
    }


    // GETTERS
    [Header("Item Static Capacities Kinds")]
    // these capacities kinds are loaded / unloaded along side with the item loading / unloading.
    // this means that the kinds that ARE NOT in this list will be dynamically loaded / unloaded
    // when the item is dropped / grabbed
    [SerializeField] private List<string> item_static_capacities_kinds = new List<string>() { "DodgeCapacity" };
    public List<string> GetDynamicItemCapacitiesIDs(List<string> capa_ids, ref List<string> static_ids)
    {
        // 1. we get the base capable capacities ids
        List<string> dynamically_pooled_ids = new List<string>();

        // 2. we only check on items
        // if (capable is not Item) { return capable.data.capacities_ids; } ! no need for now bcz we only call method from Item

        // 3. we filter it with the list of static capacity kinds
        // -> means we dynamically handle ONLY the kinds that ARE NOT in this list
        for (int i=0; i<capa_ids.Count; i++)
        {
            string capa_id = capa_ids[i];
            CapacityData capa_data = capacities_data[capa_id] as CapacityData;
            if (capa_data == null)
            {
                if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - GetCapacitiesIDsToPoolDynamically) Capacity data not found for id: " + capa_id); }
                continue;
            }

            // if we have a static capacity kind, we don't add it
            if (item_static_capacities_kinds.Contains(capa_data.kind))
            {
                static_ids.Add(capa_id);
                continue;
            }

            // else it is a dynamic one, we add it
            dynamically_pooled_ids.Add(capa_id);
        }
        return dynamically_pooled_ids;
    }

    // DATA DUPLICATION
    private CapacityData DuplicateData(CapacityData base_data)
    {
        CapacityData new_data = base_data.Duplicate() as CapacityData;
        new_data.id = GenerateUniqueId(base_data.id);

        // we add the new_data to the data list
        capacities_data.Add(new_data.id, new_data);
        return new_data;
    }
    private string GenerateUniqueId(string base_id)
    {
        // todo if we have perf issues we just need to have a static int that we increment so it's faster

        // we check if the base_id can be splitted with "_"
        string[] parts = base_id.Split('_');
        string suffix = parts.Length > 1 ? parts[parts.Length - 1] : "";
        string prefix = base_id.Substring(0, base_id.Length - suffix.Length);

        // we go through all capacities_data keys and memorize all the ids that have the same prefix and check the suffix int is greater or not
        int max_suffix = 0;
        foreach (string key in capacities_data.Keys)
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



}