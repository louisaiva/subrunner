using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MotorEngine : BSOD_System<MotorEngine>
{
    private HashSet<IAData> unloaded_entities = new HashSet<IAData>();
    public HashSet<IAData> UnloadedEntities { get { return unloaded_entities; } }

    [Header("SubSystems")]
    [SerializeField] private BatchActionProvider batch_provider;
    [SerializeField] private BatchActionAchiever batch_achiever;

    [Header("Logs")]
    [SerializeField] private bool log_loading = false;

    private void Start()
    {
        // we subscribe to the CapableBank's OnCapableLoading & OnCapableUnloaded events
        CapableBank.Instance.OnCapableLoading += on_capable_load;
        CapableBank.Instance.OnCapableUnloaded += on_capable_unload;

        // here we need to gather all the unloaded entities in the scene
        // todo
    }

    // LOAD / UNLOAD CALLBACKS
    private void on_capable_load(CapableData data)
    {
        if (data is not IAData ia_data) { return; }
        if (!unloaded_entities.Contains(ia_data)) { return; }

        // here we handle the action stopping of BatchActionAchiever & BatchGoTo

        // we remove the IAData from the unloaded_entities set
        unloaded_entities.Remove(ia_data);
        if (log_loading) { Debug.Log($"(MotorEngine) Removed '{ia_data.id}' from unloaded entities."); }
    }
    private void on_capable_unload(CapableData data)
    {
        if (data is not IAData ia_data) { return; }

        // we add the IAData to the unloaded_entities set
        unloaded_entities.Add(ia_data);
        if (log_loading) { Debug.Log($"(MotorEngine) Added '{ia_data.id}' to unloaded entities."); }

        // here we handle the action launching of BatchActionAchiever & BatchGoTo
    }

}