
using System;
using System.Collections.Generic;
using System.Reflection;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using subrunner.goap;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[Serializable] public class MotorData : CapacityData
{
    public string agent_type;
    
    // LOCAL WORLD DATA
    public LocalWorldData local_world_data = new LocalWorldData();

    // GOTO DATA
    public AvoidanceData avoidance_data = new AvoidanceData();

    // CONSTRUCTOR
    public MotorData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new MotorData(base.Duplicate() as CapacityData)
        {
            agent_type = this.agent_type,
            avoidance_data = this.avoidance_data.Duplicate(),
            local_position = this.local_position,

            // empty lists since when we duplicate, we spawn the entity so it is not populated for now
            local_world_data = new LocalWorldData(),
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

    // GETTERS
    public List<ILocalWorldTarget> GetLocalWorldTargets() { return local_world_data.GetLocalWorldTargets(); }
}



// LOCAL WORLD DATA

[Serializable] public class LocalWorldData
{
    public List<LocalWorldStateData> local_world_states = new List<LocalWorldStateData>();
    public List<LocalWorldPositionData> local_world_positions = new List<LocalWorldPositionData>();
    public List<LocalWorldCapableData> local_world_capables = new List<LocalWorldCapableData>();

    // GETTERS
    public List<ILocalWorldTarget> GetLocalWorldTargets()
    {
        List<ILocalWorldTarget> targets = new List<ILocalWorldTarget>();
        targets.AddRange(local_world_positions);
        targets.AddRange(local_world_capables);
        return targets;
    }

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
            details += $"      - {target.KeyName} : {target.capable_id} @ {target.Position}\n";
        }
        return details;
    }



    // RUNTIME WORLD DATA -> LOCAL WORLD DATA
    /// <summary>
    /// this method feed or rebuilds the whole local world data from a
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
            LocalWorldStateData state_data = find_state_by_key(key);
            if (state_data != null)
            {
                state_data.key_value = entry.Value.Value;
                continue;
            }
            
            // we add a new state data for this key
            state_data = new LocalWorldStateData
            {
                key_name = key,
                key_value = entry.Value.Value,
            };
            local_world_states.Add(state_data);
        }


        foreach (var entry in world_data.Targets)
        {
            if (entry.Key == null || entry.Value == null || entry.Value.Value == null) { continue; }
            string key = serialize_type_name(entry.Key);
            ITarget runtime_target = entry.Value.Value;

            // check if position
            if (runtime_target is PositionTarget position_target)
            {
                // feed positions
                LocalWorldPositionData position_data = find_position_by_key(key);
                if (position_data != null)
                {
                    position_data.target_position = position_target.Position;
                    continue;
                }

                // we add a new position data for this key
                position_data = new LocalWorldPositionData
                {
                    key_name = key,
                    target_position = position_target.Position,
                };
                local_world_positions.Add(position_data);
                continue;
            }

            // check for unknown target types
            if (runtime_target is not TransformTarget transform_target) { continue; }

            // extract the capable
            Transform target_transform = transform_target.Transform;
            if (target_transform is null) { continue; }
            Capable capable = target_transform.GetComponent<Capable>();
            if (capable == null && target_transform.parent != null)
            {
                capable = target_transform.parent.GetComponent<Capable>(); // only direct parent otherwise if we are in an inventory we could get the higher capable
            }
            if (capable == null || !capable.Loaded) { continue; }

            // feed capables
            LocalWorldCapableData target_data = find_capable_by_key(key);
            if (target_data != null)
            {
                target_data.capable_id = capable.data.id;
                continue;
            }

            // we add a new capable data for this key
            target_data = new LocalWorldCapableData
            {
                key_name = key,
                capable_id = capable.data.id,
            };
            local_world_capables.Add(target_data);
            continue;
        }
    }

    // LOCAL WORLD DATA -> RUNTIME WORLD DATA
    public void PopulateRuntimeData(ILocalWorldData world_data)
    {
        CrashKonijn.Goap.Runtime.LocalWorldData provider_world_data = world_data as CrashKonijn.Goap.Runtime.LocalWorldData;
        if (provider_world_data == null)
        {
            Debug.LogError($"(MotorData) Failed to populate runtime world data, the runtime world data is of type '{serialize_type_name(world_data.GetType())}' instead of 'CrashKonijn.Goap.Runtime.LocalWorldData'.");
            return;
        }

        // feed states
        foreach (var state_data in local_world_states)
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
        foreach (var position_data in local_world_positions)
        {
            Type key_type = resolve_type(position_data.key_name);
            if (key_type == null) { continue; }
            IWorldDataState<ITarget> state = world_data.GetTargetState(key_type);
            if (state == null)
            {
                provider_world_data.SetTarget(key_type, new PositionTarget(position_data.target_position));
                continue;
            }
            ITarget target = state.Value;
            if (target == null || target is not PositionTarget pos_target) { continue; }

            // we feed the state value
            pos_target.SetPosition(position_data.target_position);
        }
        foreach (var capable_data in local_world_capables)
        {
            Type key_type = resolve_type(capable_data.key_name);
            if (key_type == null) { continue; }

            // we try to get the transform of the capable
            Capable capable = CapableBank.Instance.GetLoadedCapable(capable_data.capable_id);
            if (capable == null) { continue; }
            Transform target_transform = capable.transform;

            // we try to get the target state
            IWorldDataState<ITarget> state = world_data.GetTargetState(key_type);
            if (state == null)
            {
                provider_world_data.SetTarget(key_type, new TransformTarget(target_transform));
                continue;
            }
            ITarget target = state.Value;
            if (target == null || target is not TransformTarget trans_target) { continue; }
            trans_target.SetTransform(target_transform);
        }
    }
    public void ClearAndPopulateRuntimeData(ILocalWorldData world_data)
    {
        // we clear the runtime world data
        world_data.States.Clear();
        world_data.Targets.Clear();

        // we feed the local world data to the runtime world data
        PopulateRuntimeData(world_data);
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
    private LocalWorldStateData find_state_by_key(string key)
    {
        return local_world_states.Find(s => s.key_name == key);
    }
    private LocalWorldPositionData find_position_by_key(string key)
    {
        return local_world_positions.Find(p => p.key_name == key);
    }
    private LocalWorldCapableData find_capable_by_key(string key)
    {
        return local_world_capables.Find(c => c.key_name == key);
    }
}



// LOCAL WORLD DATA CLASSES


[Serializable] public class LocalWorldStateData
{
    public string key_name;
    public int key_value;
}

public interface ILocalWorldTarget
{
    public string KeyName { get; }
    public Vector2 Position { get; }
}

[Serializable] public class LocalWorldPositionData : ILocalWorldTarget
{
    public string key_name;
    public Vector2 target_position;

    public string KeyName { get { return key_name; } }
    public Vector2 Position { get { return target_position; } }
}
[Serializable] public class LocalWorldCapableData : ILocalWorldTarget
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
            _target_capable = CapableSystem.Instance.GetCapableDataFromID(_capable_id);
            return _target_capable;
        }
    }

    public string KeyName { get { return key_name; } }
    public Vector2 Position
    {
        get
        {
            if (target_capable == null) { return default(Vector2); }
            return _target_capable.position;
        }
    }
}