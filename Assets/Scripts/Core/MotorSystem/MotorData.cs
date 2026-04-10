
using System;
using System.Collections.Generic;
using subrunner.goap;
using UnityEngine;

[Serializable] public class MotorData : CapacityData
{
    public string agent_type;
    
    // LOCAL WORLD DATA
    public List<LocalWorldStateData> local_world_state = new List<LocalWorldStateData>();
    public List<LocalWorldTargetData> local_world_targets = new List<LocalWorldTargetData>();

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

            // deep copy local world data to avoid shared references
            local_world_state = duplicate_states(this.local_world_state),
            local_world_targets = duplicate_targets(this.local_world_targets),
        };
    }

    private static List<LocalWorldStateData> duplicate_states(List<LocalWorldStateData> source)
    {
        if (source == null) { return new List<LocalWorldStateData>(); }

        List<LocalWorldStateData> copy = new List<LocalWorldStateData>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            LocalWorldStateData state = source[i];
            if (state == null) { continue; }

            copy.Add(new LocalWorldStateData
            {
                key_name = state.key_name,
                key_value = state.key_value,
            });
        }
        return copy;
    }

    private static List<LocalWorldTargetData> duplicate_targets(List<LocalWorldTargetData> source)
    {
        if (source == null) { return new List<LocalWorldTargetData>(); }

        List<LocalWorldTargetData> copy = new List<LocalWorldTargetData>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            LocalWorldTargetData target = source[i];
            if (target == null) { continue; }

            copy.Add(new LocalWorldTargetData
            {
                key_name = target.key_name,
                target_type = target.target_type,
                target_capable_id = target.target_capable_id,
                target_position = target.target_position,
            });
        }
        return copy;
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
        details += $"  - local world targets : \n";
        foreach (var target in local_world_targets)
        {
            details += $"    - {target.key_name} : {target.target_capable_id} @ {target.target_position}\n";
        }
        return base.GetDetails() + details;
    }
}

[Serializable] public class LocalWorldStateData
{
    public string key_name;
    public int key_value;
}

[Serializable] public class LocalWorldTargetData
{
    public string key_name;
    public string target_type;
    public string target_capable_id;
    public Vector2 target_position;
}