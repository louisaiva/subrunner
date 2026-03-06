using System;
using System.Collections;
using System.Collections.Generic;
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
    public List<Capacity> LoadCapacities(List<string> capacities_ids, Capable capable)
    {
        List<Capacity> capacities = new List<Capacity>();
        for (int i = 0; i < capacities_ids.Count; i++)
        {
            string id = capacities_ids[i];
            Capacity capa = load_capacity(id,capable.data);
            
            capacities.Add(capa);
        }
        return capacities;
    }
    private Capacity load_capacity(string id, CapableData capable_data)
    {
        CapacityData data = capacities_data[id] as CapacityData;
        if (data == null)
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - Load) Capacity data not found for id: " + id); }
            return null;
        }
        return load_capacity(data, capable_data);
    }
    private Capacity load_capacity(CapacityData data, CapableData capable_data)
    {
        // we check if we already have this data in our loaded data
        if (loaded_capacities_data.ContainsKey(data.id))
        {
            string old_id = data.id;
            // ? then we want to duplicate the data & change the capa_id & change the capa_id in capable.data.capacities

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
        if (data == null) { Debug.LogWarning("(CapacityEngine - Unload) Loaded capacity data not found for id: " + id); return null; }
        Capacity capacity = CapacityBank.Instance.Unload(data);
        loaded_capacities_data.Remove(id);
        if (log_loading) { Debug.Log("(CapacityEngine) Unloaded " + id); }
        return capacity;
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