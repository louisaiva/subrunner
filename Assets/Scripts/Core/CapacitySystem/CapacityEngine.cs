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
    public List<Capacity> LoadCapacities(List<string> capacities_ids/* , Capable capable */)
    {
        List<Capacity> capacities = new List<Capacity>();
        for (int i = 0; i < capacities_ids.Count; i++)
        {
            string id = capacities_ids[i];
            Capacity capa = load_capacity(id);
            
            capacities.Add(capa);
        }
        return capacities;
    }
    private Capacity load_capacity(string id)
    {
        CapacityData data = capacities_data[id] as CapacityData;
        if (data == null)
        {
            if (!hide_log_no_data_found) { Debug.LogWarning("(CapacityEngine - Load) Capacity data not found for id: " + id); }
            return null;
        }
        return load_capacity(data);
    }
    private Capacity load_capacity(CapacityData data)
    {
        // we check if we already have this data in our loaded data
        if (loaded_capacities_data.ContainsKey(data.id))
        {
            // ? then we want to duplicate the data
            // if (log_loading) { Debug.LogWarning($"(CapacityEngine - Load) Capacity with id {data.id} is already loaded, we will duplicate it"); }
            if (log_loading) { Debug.LogWarning($"(CapacityEngine - Load) Capacity with id {data.id} is already loaded, we return null for now"); }
            return null;
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

}