
using System;
using System.Collections.Generic;
using System.Reflection;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using subrunner.goap;
using UnityEngine;

[Serializable] public class MotorData : CapacityData
{
    public string agent_type;
    [RuntimeOnly] public string current_room_id;
    [/* InstanceSpecific,  */RuntimeOnly] public Dictionary<Type, string> room_destinations = new Dictionary<Type, string>(); // main room destinations per goal of the ia.
    


    // LOCAL WORLD DATA
    public SerializableLocalWorldData local_world_data = new SerializableLocalWorldData();

    // GOTO DATA
    public AvoidanceData avoidance_data = new AvoidanceData();

    // CONSTRUCTOR
    public MotorData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new MotorData(base.Duplicate() as CapacityData)
        {
            agent_type = this.agent_type,
            avoidance_data = this.avoidance_data.Duplicate(),
            local_position = this.local_position,

            // empty lists since when we duplicate, we spawn the entity so it is not populated for now
            local_world_data = new SerializableLocalWorldData(),
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - agent type : {agent_type}\n";
        details += $"  - {avoidance_data.GetDetails()}\n";
        details += $"  - local world data :\n{local_world_data.GetDetails()} \n";
        return base.GetDetails() + details;
    }



    // DESTINATIONS & CURRENT ROOM MANAGEMENT
    public bool SetDestination(Type goal, string destination)
    {
        if (string.IsNullOrEmpty(destination)) { return false; }
        if (goal == null) { return false; }

        if (room_destinations.ContainsKey(goal))
        {
            room_destinations[goal] = destination;
            return true;
        }

        room_destinations.Add(goal, destination);
        return true;
    }
    public void ClearDestination(Type goal)
    {
        if (goal == null) { return; }
        if (!room_destinations.ContainsKey(goal)) { return; }
        room_destinations.Remove(goal);
    }
    public bool TryGetDestination(Type goal, out string destination)
    {
        if (goal == null) { destination = null; return false; }
        if (!room_destinations.TryGetValue(goal, out destination)) { return false; }
        return !string.IsNullOrEmpty(destination);
    }
    public bool ExtractCurrentAndDestinationRooms(Type goal, out string current_room, out string destination)
    {
        current_room = "";
        destination = "";

        if (RoomEngine.DoorEngine == null) { return false; }
        if (goal == null) { return false; }
        if (string.IsNullOrEmpty(OwnerData?.room)) { return false; }
        current_room = OwnerData.room;
        TryGetDestination(goal, out destination);
        // even if no destination, we are good !
        return true;
    }
}



// LOCAL WORLD DATA

[Serializable] public class SerializableLocalWorldData
{


    // ----------------------------    

    //     SERIALIZED WORLD DATA COLLECTIONS

    // ----------------------------

    public List<SerializableWorldState> local_world_states = new List<SerializableWorldState>();
    public List<SerializablePositionTarget> local_world_positions = new List<SerializablePositionTarget>();
    public List<SerializableCapableTarget> local_world_capables = new List<SerializableCapableTarget>();





    // ----------------------------    
    
    //     RUNTIME WORLD DATA -> SERIALIZED WORLD DATA

    // ----------------------------

    /// <summary>
    /// this method feeds or rebuilds the whole local world data from a
    /// runtime world data. It is used when we unload a MotorData.
    /// If the targets or state do not exist it creates them,
    /// otherwise it updates existing ones.
    /// </summary>
    /// <param name="world_data"></param>
    public void CreateOrPopulateSerializedData(ILocalWorldData world_data)
    {
        // feed states
        foreach (var entry in world_data.States)
        {
            if (entry.Key == null || entry.Value == null) { continue; }
            string key = serialize_type_name(entry.Key);
            SerializableWorldState state_data = find_state_by_key(key);
            if (state_data != null)
            {
                state_data.key_value = entry.Value.Value;
                continue;
            }
            
            // we add a new state data for this key
            state_data = new SerializableWorldState
            {
                key_name = key,
                key_value = entry.Value.Value,
            };
            local_world_states.Add(state_data);
        }

        // feed targets
        foreach (var entry in world_data.Targets)
        {
            if (entry.Key == null || entry.Value == null || entry.Value.Value == null) { continue; }
            string key = serialize_type_name(entry.Key);
            ITarget runtime_target = entry.Value.Value;

            // ? what if the runtime target is null ?
            // -> it means we still have an existing IWorldDataState<ITarget>
            // -> but its value is null, so we should not create anything yeah that's the best idea
            // -> we will save the data if we have an existing valid target, no need otherwise

            // check if position
            if (runtime_target is PositionTarget position_target)
            {
                // feed positions
                SerializablePositionTarget position_data = find_position_by_key(key);
                if (position_data != null)
                {
                    position_data.target_position = position_target.Position;
                    continue;
                }

                // we add a new position data for this key
                position_data = new SerializablePositionTarget
                {
                    key_name = key,
                    target_position = position_target.Position,
                };
                local_world_positions.Add(position_data);
                continue;
            }

            // check if capable
            if (runtime_target is CapableTarget cap_target)
            {
                // extract the capable data
                CapableData capable_data = cap_target.CapableData;
                if (capable_data == null) { continue; }

                // get existing data for this key
                SerializableCapableTarget target_data = find_capable_by_key(key);
                if (target_data != null)
                {
                    target_data.capable_id = capable_data.id;
                    continue;
                }

                // we add a new capable data for this key
                target_data = new SerializableCapableTarget
                {
                    key_name = key,
                    capable_id = capable_data.id,
                };
                local_world_capables.Add(target_data);
                continue;
            }
        }
    }







    // ----------------------------    

    //     SERIALIZED WORLD DATA -> RUNTIME WORLD DATA

    // ----------------------------

    /// <summary>
    /// This method is used when loading a MotorData AND the agent type is the same as last time.
    /// This does not create new runtime states from the serialized data. it takes
    /// the existing runtime world data states and update their values based
    /// on existing serialized data. This means that we iterate through existing
    /// RUNTIME data. And so if the serialized data has states/targets with no runtime equivalent,
    /// they sadly won't have a created runtime equivalent after.
    /// If the runtime world data is totally empty, it will still
    /// be empty after this method. If you want to create runtime states/targets,
    /// you should use the "ClearAndPopulateRuntimeData" method instead.
    /// </summary>
    /// <param name="world_data"></param>    
    public void PopulateRuntimeData(ILocalWorldData world_data)
    {
        LocalWorldData provider_world_data = world_data as LocalWorldData;
        if (provider_world_data == null)
        {
            Debug.LogError($"(MotorData) Failed to populate runtime world data, the runtime world data is of type '{serialize_type_name(world_data.GetType())}' instead of 'CrashKonijn.Goap.Runtime.LocalWorldData'.");
            return;
        }

        // feed states
        foreach (KeyValuePair<Type, IWorldDataState<int>> entry in world_data.States)
        {
            if (entry.Key == null || entry.Value == null) { continue; }
            Type key_type = entry.Key;
            IWorldDataState<int> state = entry.Value;

            SerializableWorldState state_data = find_state_by_key(serialize_type_name(key_type));
            if (state_data != null)
            {
                state.Value = state_data.key_value;
                continue;
            }

            // else we did not find any data for this state, we set it to 0
            state.Value = 0;
        }


        // feed targets
        foreach (KeyValuePair<Type, IWorldDataState<ITarget>> entry in world_data.Targets)
        {
            if (entry.Key == null) { continue; }
            IWorldDataState<ITarget> state = entry.Value;
            if (state == null) { continue; }
            feed_target_to_runtime_world_data(state);
        }
}

    /// <summary>
    /// this method feed a single existing runtime target state of a runtime world data,
    /// and tries to find a matching serialized target data to feed it. if the
    /// existing runtime target state value (the ITarget) is null, it will create
    /// a new ITarget based on the serialized data's type (either PositionTarget or CapableTarget)
    /// /!\ IT DOES NOT CREATE NEW RUNTIME IWorldDataState< ITarget > /!\ only the value of it which it a ITarget.
    /// </summary>
    /// <param name="state"></param>
    private void feed_target_to_runtime_world_data(IWorldDataState<ITarget> state)
    {

        string key_name = serialize_type_name(state.Key);
        string log_feeding = "";
        if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING)
        {
            log_feeding += $"(MotorData - feed_target_to_runtime_world_data) Trying to feed target state with key '{key_name}' to runtime world data.\n - Existing value : {state.Value}";
        }

        // we check if it's a position target
        SerializablePositionTarget position_data = find_position_by_key(key_name);
        if (position_data != null)
        {
            if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - Found matching serialized position target data with position {position_data.target_position}."; }
            
            if (state.Value is null)
            {
                if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - But State.Target is null. creating a new PositionTarget"; }
                state.Value = new PositionTarget(position_data.target_position);
            }
            else if (state.Value is PositionTarget pos_target)
            {
                pos_target.SetPosition(position_data.target_position);
                if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - And State.Target already exists ! Its position is now {pos_target.Position}."; }
            }

            if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { Debug.Log(log_feeding); }
            return;
        }

        // or a capable target
        SerializableCapableTarget capable_data = find_capable_by_key(key_name);
        if (capable_data != null)
        {
            if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - Found matching serialized capable target data with capable ID {capable_data.capable_id}."; }

            if (string.IsNullOrEmpty(capable_data.capable_id))
            {
                if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - But serialized capable target data has no capable ID. We cannot feed this target data to the runtime world data."; Debug.LogWarning(log_feeding); }
                return;
            }
            CapableData capdata = CapableEngine.Instance.GetCapableDataFromID(capable_data.capable_id);
            if (capdata is null)
            {
                if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - But no capable data found for this capable ID. We cannot feed this target data to the runtime world data."; Debug.LogWarning(log_feeding); }
                return;
            }
            
            if (state.Value is null)
            {
                if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - But State.Target is null. creating a new CapableTarget with capable id '{capdata.id}'"; }
                state.Value = new CapableTarget(capdata);
            }
            else if (state.Value is CapableTarget cap_target)
            {
                cap_target.SetCapableData(capdata);
                if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - And State.Target already exists ! Updated its capable data and now it is '{cap_target.CapableID}'."; }
            }
            if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { Debug.Log(log_feeding); }
            return;
        }

        // else we have no data for this target, we set it to null
        state.Value = null;
        if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { log_feeding += $"\n - No matching serialized target data found for this target state. We set it to null."; Debug.Log(log_feeding); }
    }


    /// <summary>
    /// This method is used when loading a MotorData BUT the new agent type is different than
    /// the last one. In this case we totally wipe out the runtime world data states & targets, and
    /// we rebuild them based on the existing serialized data. This means that we iterate through existing
    /// SERIALIZED data (instead of RUNTIME data), so all existing serialized data will have a runtime
    /// state/target equivalent at the end of it.
    /// </summary>
    /// <param name="state"></param>
    public void ClearAndPopulateRuntimeData(ILocalWorldData world_data)
    {

        // we clear the runtime world data
        world_data.States.Clear();
        world_data.Targets.Clear();

        LocalWorldData provider_world_data = world_data as LocalWorldData;
        if (provider_world_data == null)
        {
            Debug.LogError($"(MotorData) Failed to populate runtime world data, the runtime world data is of type '{serialize_type_name(world_data.GetType())}' instead of 'CrashKonijn.Goap.Runtime.LocalWorldData'.");
            return;
        }

        // feed states
        foreach (SerializableWorldState state_data in local_world_states)
        {
            Type key_type = resolve_type(state_data.key_name);
            if (key_type == null) { continue; }
            IWorldDataState<int> state = world_data.GetWorldState(key_type);
            if (state == null)
            {
                provider_world_data.SetState(key_type, state_data.key_value);
                continue;
            }

            // we feed the state value
            state.Value = state_data.key_value;
        }

        // feed targets
        foreach (SerializablePositionTarget position_data in local_world_positions)
        {
            Type key_type = resolve_type(position_data.key_name);
            if (key_type == null) { continue; }
            IWorldDataState<ITarget> state = world_data.GetTargetState(key_type);
            if (state == null)
            {
                provider_world_data.SetTarget(key_type, new PositionTarget(position_data.target_position));
                continue;
            }
            if (state.Value == null)
            {
                state.Value = new PositionTarget(position_data.target_position);
                continue;
            }
            ITarget target = state.Value;
            if (target is not PositionTarget pos_target) { continue; }

            // we feed the state value
            pos_target.SetPosition(position_data.target_position);
        }
        foreach (SerializableCapableTarget capable_data in local_world_capables)
        {
            Type key_type = resolve_type(capable_data.key_name);
            if (key_type == null) { continue; }

            // we try to get the capable data
            if (string.IsNullOrEmpty(capable_data.capable_id)) { continue; }
            CapableData capdata = CapableEngine.Instance.GetCapableDataFromID(capable_data.capable_id);
            if (capdata == null) { continue; }

            // we try to get the target state
            IWorldDataState<ITarget> state = world_data.GetTargetState(key_type);
            if (state == null)
            {
                provider_world_data.SetTarget(key_type, new CapableTarget(capdata));
                continue;
            }
            if (state.Value == null)
            {
                state.Value = new CapableTarget(capdata);
                continue;
            }
            ITarget target = state.Value;
            if (target is not CapableTarget cap_target) { continue; }
            cap_target.SetCapableData(capdata);
        }
    }








    // ----------------------------    


    //     GETTERS & TYPES HELPERS & FINDERS


    // ----------------------------



    // GET DETAILS
    public string GetDetails()
    {
        string details = "    - local world states : \n";
        foreach (var state in local_world_states)
        {
            details += $"      - {state.key_name} : {state.key_value}\n";
        }
        details += $"    - local world positions : \n";
        foreach (var position in local_world_positions)
        {
            details += $"      - {position.KeyName} : {position.Position}\n";
        }
        details += $"    - local world capables : \n";
        foreach (var target in local_world_capables)
        {
            details += $"      - {target.KeyName} : targeted capable id :'{target.capable_id}' at position {target.Position}\n";
        }
        return details;
    }

    // type helpers
    private static string serialize_type_name(Type type)
    {
        if (type == null) { return string.Empty; }
        return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
    }
    private static Type resolve_type(string type_name)
    {
        if (string.IsNullOrWhiteSpace(type_name)) { return null; }

        Type resolved = Type.GetType(type_name);
        if (resolved != null) { return resolved; }

        // legacy fallback: simple key names that were previously saved without namespace/assembly.
        resolved = Type.GetType($"subrunner.goap.{type_name}");
        if (resolved != null) { return resolved; }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            resolved = assemblies[i].GetType(type_name);
            if (resolved != null) { return resolved; }
        }

        for (int i = 0; i < assemblies.Length; i++)
        {
            Type[] types;
            try
            {
                types = assemblies[i].GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            if (types == null) { continue; }

            for (int j = 0; j < types.Length; j++)
            {
                Type type = types[j];
                if (type == null) { continue; }
                if (type.Name == type_name) { return type; }
            }
        }

        return null;
    }
    
    // data getters
    private SerializableWorldState find_state_by_key(string key)
    {
        SerializableWorldState state = local_world_states.Find(s => s.key_name == key);
        // if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { Debug.Log($"(MotorData - find_state_by_key) Looking for matching '{key}' among {local_world_states.Count} entries. Found: {state != null}"); }
        return state;
    }
    private SerializablePositionTarget find_position_by_key(string key)
    {
        SerializablePositionTarget position = local_world_positions.Find(p => p.key_name == key);
        // if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { Debug.Log($"(MotorData - find_position_by_key) Looking for matching '{key}' among {local_world_positions.Count} entries. Found: {position != null}"); }
        return position;
    }
    private SerializableCapableTarget find_capable_by_key(string key)
    {
        SerializableCapableTarget capable = local_world_capables.Find(c => c.key_name == key);
        // if (Logger.LazyInstance.LOG_SERIALIZABLE_WORLD_STATES_TARGETS_LOADING) { Debug.Log($"(MotorData - find_capable_by_key) Looking for matching '{key}' among {local_world_capables.Count} entries. Found: {capable != null}"); }
        return capable;
    }
}



// LOCAL WORLD DATA CLASSES


[Serializable] public class SerializableWorldState
{
    public string key_name;
    public int key_value;
}

public interface ISerialiableTarget
{
    public string KeyName { get; }
    public Vector2 Position { get; }
}

[Serializable] public class SerializablePositionTarget : ISerialiableTarget
{
    public string key_name;
    public Vector2 target_position;

    public string KeyName { get { return key_name; } }
    public Vector2 Position { get { return target_position; } }
}
[Serializable] public class SerializableCapableTarget : ISerialiableTarget
{
    public string key_name;
    private string _capable_id;
    public string capable_id
    {
        get { return _capable_id; }
        set
        {
            _capable_id = value;
            _target_capable = null; // reset the cached capable data so it will be reloaded on next access
        }
    }
    [NonSerialized] private CapableData _target_capable = null; // we only store the capable id, the capable data is loaded at runtime from the CapableBank using the id
    private CapableData target_capable
    {
        get
        {
            if (_target_capable != null) { return _target_capable; }
            if (string.IsNullOrEmpty(_capable_id)) { return null; }
            _target_capable = CapableEngine.Instance.GetCapableDataFromID(_capable_id);
            return _target_capable;
        }
    }

    public string KeyName { get { return key_name; } }
    public Vector2 Position
    {
        get
        {
            if (target_capable == null) { return default(Vector2); }
            return _target_capable.Position;
        }
    }
}