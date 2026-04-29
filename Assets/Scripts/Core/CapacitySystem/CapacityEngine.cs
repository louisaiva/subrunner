using System;
using System.Collections.Generic;
using UnityEngine;

public class CapacityEngine : BSOD_System<CapacityEngine>
{

    [Header("Templates Capacities data")]
    private string templates_data_path = "data/templates/capacities/";
    private Dictionary<string, CapacityData> templates_capacities_data = new Dictionary<string, CapacityData>();

    [Header("World Capacities data")]
    // private string world_data_path = "data/capacities/";
    public Dictionary<string, CapacityData> world_capacities_data = new Dictionary<string, CapacityData>();

    [Header("Loaded Capacities data")]
    public Dictionary<string,CapacityData> loaded_capacities_data = new Dictionary<string,CapacityData>();

    [Header("State")]
    public bool awake_done = false;

    [Header("Settings")]
    public bool auto_repair_owner_links_on_load = false;


    [Header("Logs - Awake")]
    public bool log_templates_data_loading = false;
    public bool log_world_data_loading = false;

    [Header("Logs - Spawning")]
    public bool log_spawning = false;

    [Header("Logs - Loading")]
    public bool log_loading = false;
    public bool log_loading_extended = false;
    public bool hide_log_no_data_found = false;



    // AWAKE
    public void Init()
    {
        // load templates capacities data
        loadTemplatesCapacitiesData();

        // load world capacities data
        loadWorldCapacitiesData();

        awake_done = true;
    }

    // LOAD TEMPLATES & WORLD DATA
    protected void loadTemplatesCapacitiesData()
    {
        // we empty the capacities_data
        templates_capacities_data = new Dictionary<string, CapacityData>();
        string log_capacities_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = AppManager.LoadJsonsFromAssets(templates_data_path);
        Dictionary<string, List<string>> json_by_kind = new Dictionary<string, List<string>>();
        foreach (string json in files)
        {
            CapacityData data = JsonUtility.FromJson<CapacityData>(json);

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
                loadCapacityDataOfType(json, kind, ref log_capacities_details, ref templates_capacities_data);
            }
        }

        if (log_templates_data_loading) { Debug.Log("(CapacityEngine) TEMPLATES CAPACITIES DATA LOADED : " + templates_capacities_data.Count + log_capacities_details); }
    }
    protected void loadWorldCapacitiesData()
    {
        // we empty the capacities_data
        world_capacities_data = new Dictionary<string,CapacityData>();
        string log_capacities_details = "\n\n";

        // we load all the json files in the data path and get their kind
        string[] files = AppManager.LoadJsonsFromWorldDataPath("capacities");
        Dictionary<string, List<string>> json_by_kind = new Dictionary<string, List<string>>();
        foreach (string json in files)
        {
            CapacityData data = JsonUtility.FromJson<CapacityData>(json);

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
            CapacityData data = null;
            foreach (string json in json_list)
            {
                data = loadCapacityDataOfType(json, kind, ref log_capacities_details, ref world_capacities_data);

                // we add the id to the world unique ids registry to avoid generating the same id for another data
                World.StaticInstance.RegisterUniqueID(data.id);
            }
        }

        if (log_world_data_loading) { Debug.Log("(CapacityEngine) WORLD CAPACITIES DATA LOADED : " + world_capacities_data.Count + log_capacities_details); }
    }
    private CapacityData loadCapacityDataOfType(string json, string kind, ref string log, ref Dictionary<string, CapacityData> data_by_id)
    {
        // if (log_awake_data) { Debug.Log($"(CapacityEngine - loadCapacityDataOfType) loading capacity of kind {kind} with json : {json}"); }

        Type type = Type.GetType(kind + "Data");
        if (type == null) { type = Type.GetType(kind.Replace("Capacity","Data")); }
        if (type == null) { type = typeof(CapacityData); }
        CapacityData data = JsonUtility.FromJson(json, type) as CapacityData;
        data_by_id.Add(data.id, data);
        log += data.GetDetails() + "\n";
        return data;
    }

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

        // we change the capacity id in the capable data
        for (int i = 0; i < cdata.capacities_ids.Count; i++)
        {
            // check if same id
            if (cdata.capacities_ids[i] != template_id) { continue; }

            // else change the id to new capacity id
            cdata.capacities_ids[i] = data.id;
            break;
        }

        // finally we set the capacity owner id to the capable id
        data.owner_id = cdata.id;

        if (log_spawning) { Debug.Log($"(CapacityEngine - Spawn) New Capacity '{data.id}' was created from template '{template_id}' and assigned to '{cdata.id}'"); }

        // we return the capacity data
        return data;
    }

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
        bool link_ok = CapableSystem.Instance.ValidateOwnershipLinks(capable_data, data, repair : auto_repair_owner_links_on_load, repair_removes_duplicates : false);
        if (!link_ok)
        {
            if (log_loading_extended) { Debug.LogWarning($"(CapacityEngine - Load) Capacity '{data.id}' owner id '{data.owner_id}' does not match capable id '{capable_data.id}' for '{capable_data.id}' (if they matches, it means there are some Duplicates)"); }
            // return null;
        }

        if (log_loading_extended) { Debug.Log($"(CapacityEngine - Load) Loading capacity '{data.id}' \n{data.GetDetails()}"); }

        Capacity capacity = CapacityBank.Instance.Load(data);
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

            // else it is a dynamic one, we add it
            dynamically_pooled_ids.Add(capa_id);
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
}