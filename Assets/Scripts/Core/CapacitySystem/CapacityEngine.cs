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
    public List<string> loading_queue = new List<string>();
    public List<string> unloading_queue = new List<string>();

    [Header("State")]
    public bool awake_done = false;

    [Header("Logs")]
    public bool log_awake_data = false;



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

        if (log_awake_data) { Debug.Log("(CapacitySystem) CAPACITIES DATA LOADED : " + capacities_data.Count + log_capacities_details); }
        awake_done = true;
    }
    private void loadCapacityDataOfType(string json, string kind, ref string log)
    {
        // if (log_awake_data) { Debug.Log($"(CapacitySystem - loadCapacityDataOfType) loading capacity of kind {kind} with json : {json}"); }

        Type type = Type.GetType(kind + "Data");
        if (type == null) { type = typeof(CapacityData); }
        CapacityData data = JsonUtility.FromJson(json, type) as CapacityData;
        capacities_data.Add(data.id, data);
        log += data.GetDetails() + "\n";
    }
}