
using System;
using System.Collections.Generic;
using subrunner.goap;
using UnityEngine;

[Serializable] public class MotorData : CapacityData
{
    public string agent_type;
    
    // LOCAL WORLD DATA
    public List<LocalWorldStateData> local_world_state = new List<LocalWorldStateData>();
    public List<LocalWorldPositionData> local_world_positions = new List<LocalWorldPositionData>();
    public List<LocalWorldCapableData> local_world_capables = new List<LocalWorldCapableData>();

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
            local_world_state = new List<LocalWorldStateData>(),
            local_world_positions = new List<LocalWorldPositionData>(),
            local_world_capables = new List<LocalWorldCapableData>(),
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - agent type : {agent_type}\n";
        details += $"  - {avoidance_data.GetDetails()}\n";
        details += $"  - local world state : \n";
        foreach (var state in local_world_state)        {
            details += $"    - {state.key_name} : {state.key_value}\n";
        }
        details += $"  - local world positions : \n";
        foreach (var position in local_world_positions)        {
            details += $"    - {position.KeyName} : {position.Position}\n";
        }
        details += $"  - local world capables : \n";
        foreach (var target in local_world_capables)
        {
            details += $"    - {target.KeyName} : {target.target_capable.id} @ {target.Position}\n";
        }
        return base.GetDetails() + details;
    }


    // GETTERS
    public List<ILocalWorldTarget> GetLocalWorldTargets()
    {
        List<ILocalWorldTarget> targets = new List<ILocalWorldTarget>();
        targets.AddRange(local_world_positions);
        targets.AddRange(local_world_capables);
        return targets;
    }
}

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
    public CapableData target_capable;

    public string KeyName { get { return key_name; } }
    public Vector2 Position { get { return target_capable.position; } }
}