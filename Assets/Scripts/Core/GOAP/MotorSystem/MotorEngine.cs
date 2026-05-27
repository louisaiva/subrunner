#pragma warning disable 1998

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using System.Threading.Tasks;

public class MotorEngine : BSOD_System<MotorEngine>
{
    // all the entities that went through at least one unload.
    // not cleared since we don't want to over and over create/destroy the IA_and_MotorData for the same entities.
    private Dictionary<string, EntityMotor> entities = new Dictionary<string, EntityMotor>();
 
    [Header("MotorData & IA relationship")]
    private HashSet<IAData> unloaded_ia = new HashSet<IAData>();
    private Dictionary<string, MotorData> motors_unloading = new Dictionary<string, MotorData>(); // motor data are stored when the Motor is unloading, so it can be retrieved AFTER the motor is unloaded



    [Header("SubSystems")]
    [SerializeField] private BatchActionProvider batch_provider;
    [SerializeField] private BatchActionAchiever batch_achiever;

    private static GoapBehaviour _goap; // main planner
    public static GoapBehaviour Goap
    {
        get
        {
            if (_goap == null) { _goap = GameObject.Find("/game/goap_manager").GetComponent<GoapBehaviour>(); }
            if (_goap == null) { Debug.LogError("(MotorCapacity) GoapBehaviour not found in the scene. Please add it to /game/goap_manager"); }
            return _goap;
        }
    }



    [Header("Logs")]
    [SerializeField] private bool log_loading = false;

    // LOAD / UNLOAD WORLD DATA
    public override async Task LoadWorldData(string world_id, bool log)
    {
        if (log) { Debug.Log($"(MotorEngine) Loading world data for world_id: {world_id}"); }


        // clear the sub systems
        batch_provider.ClearCache(log);
        // batch_achiever.ClearCache(log);
        // batch_goto.ClearCache(log);


        // we subscribe to the CapableBank's OnCapableLoading & OnCapableUnloaded events
        CapableBank.Instance.OnCapableLoading += on_capable_loading;
        CapableBank.Instance.OnCapableUnloaded += on_capable_unloaded;
        if (log) { Debug.Log($"(MotorEngine) Registered callbacks to CapableBank and RoomEngine events"); }

        // here we need to gather all the unloaded entities in the scene
        // todo


        if (log) { Debug.Log($"(MotorEngine) MOTOR ENGINE SUCCESSFULLY LOADED : {world_id}"); }
    }
    public override async Task UnloadWorldData(bool log)
    {
        // we unsubscribe to the CapableBank's OnCapableLoading & OnCapableUnloaded events
        CapableBank.Instance.OnCapableLoading -= on_capable_loading;
        CapableBank.Instance.OnCapableUnloaded -= on_capable_unloaded;
        if (log) { Debug.Log($"(MotorEngine) Registered callbacks to CapableBank and RoomEngine events"); }


        entities.Clear();
        unloaded_ia.Clear();
        motors_unloading.Clear();

        if (log) { Debug.Log($"(MotorEngine) MOTOR ENGINE SUCCESSFULLY UNLOADED"); }
    }


    // LOAD / UNLOAD CALLBACKS
    private void on_capable_loading(CapableData data)
    {
        if (data is not IAData ia_data) { return; }
        if (!unloaded_ia.Contains(ia_data)) { return; }

        // here we handle the action stopping of BatchActionAchiever & BatchGoTo
        if (entities.TryGetValue(ia_data.id, out EntityMotor entity)) { batch_provider.RemoveFromResolve(entity); }
        
        // we remove the IAData from the unloaded_ia set
        unloaded_ia.Remove(ia_data);
        if (log_loading) { Debug.Log($"(MotorEngine) Removed '{ia_data.id}' from unloaded entities."); }
    }
    private void on_capable_unloaded(CapableData data)
    {
        if (data is not IAData ia_data) { return; }
        EntityMotor entity = add_unloaded_entity(ia_data);
        if (entity is null) { return; } // if we could not add the unloaded entity, no action for it

        // here we handle the action launching of BatchActionAchiever & BatchGoTo
        batch_provider.RegisterForResolve(ia_and_motor_data: entity);
    }

    // low level add/remove unloaded entities
    private EntityMotor add_unloaded_entity(IAData ia_data)
    {
        string id = ia_data.id;

        // we check if we have a corresponding MotorData for this IAData in the motors_unloading dictionary
        if (!motors_unloading.TryGetValue(id, out MotorData motor_data))
        {
            Debug.LogError($"(MotorEngine) No MotorData found for IA '{id}' when adding to unloaded entities.");
            return null;
        }

        // we check if we have an EntityMotor already in the dictionary
        if (!entities.TryGetValue(id, out EntityMotor entity))
        {
            // else we create a new one
            entity = new EntityMotor()
            {
                ia_data = ia_data,
                motor_data = motor_data
            };
            entity.CreateWorldData();

            // and add it to the dictionary
            entities[id] = entity;
            if (log_loading) { Debug.Log($"(MotorEngine) Created new EntityMotor for IA '{id}'."); }
        }
        else { entity.PopulateWorldData(); }

        // and we remove the motor data from motors_unloading since it was unloaded with success
        motors_unloading.Remove(id);

        // and we also add the ia data
        unloaded_ia.Add(ia_data);

        if (log_loading) { Debug.Log($"(MotorEngine) Added '{id}' to unloaded entities."); }
        return entities[id];
    }

    // REGISTER MOTOR DATA BEFORE UNLOADED
    public void RegisterMotorUnloading(MotorData motor_data)
    {
        motors_unloading[motor_data.owner_id] = motor_data;
    }



    // GETTERS
    

}

public class EntityMotor
{
    // public string ia_id;
    public IAData ia_data;
    public MotorData motor_data;
    public LocalWorldData world_data;
    public string ID => ia_data.id;

    public void CreateWorldData()
    {
        if (motor_data is null) { return; }
        if (world_data is null) { world_data = new LocalWorldData(); }

        IAgentType agentType = MotorEngine.Goap.GetAgentType(motor_data.agent_type);
        world_data.SetParent(agentType.WorldData);

        // we clear the local world data and populate it with the runtime world data from the motor data
        motor_data.local_world_data.ClearAndPopulateRuntimeData(world_data);
    }
    public void PopulateWorldData()
    {
        if (world_data is null) { CreateWorldData(); return; }
        if (motor_data is null) { return; }
        
        // we simply populate the runtime world data from the motor data to the local world data
        motor_data.local_world_data.PopulateRuntimeData(world_data);
    }
}