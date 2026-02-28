using System.Collections.Generic;
using UnityEngine;

public class CapableSystem : BSOD_System<CapableSystem>
{
    // todo : RECODE THIS ENTIRELY WITH [ CAPABLE BANK ]
    // follow RoomSystem example

    Dictionary<string, Capable> loaded_capables = new Dictionary<string, Capable>();
    
    [Header("Logs")]
    public bool log_entity_loading = true;

    public void Start()
    {
        // for now only find all capables in scene
        Capable[] capables = FindObjectsByType<Capable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Capable capable in capables)
        {
            loaded_capables[capable.ID] = capable;
        }
    }

    public void LoadCapables(List<string> capables_ids)
    {
        foreach (string capable_id in capables_ids) { Load(capable_id); }
    }
    public void UnloadCapables(List<string> capables_ids)
    {
        foreach (string capable_id in capables_ids) { Unload(capable_id); }
    }


    // todo these methods should be in bank & work with pooling + stack
    public Capable Load(string id)
    {
        if (log_entity_loading) { Debug.Log($"(CapableSystem) Loading {id}"); }
        if (!loaded_capables.ContainsKey(id))
        {
            Debug.LogError($"(CapableSystem) Load - Capable with id {id} not found in loaded_capables");
            return null;
        }
        Capable capable = loaded_capables[id];
        capable.gameObject.SetActive(true);
        return capable;
    }
    public void Unload(string id)
    {
        if (log_entity_loading) { Debug.Log($"(CapableSystem) Unloading {id}"); }
        if (!loaded_capables.ContainsKey(id))
        {
            Debug.LogError($"(CapableSystem) Unload - Capable with id {id} not found in loaded_capables");
            return;
        }
        Capable capable = loaded_capables[id];
        capable.gameObject.SetActive(false);
    }

}