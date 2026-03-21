
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class MotorData : CapacityData
{
    public string agent_type;

    // GOALS
    // todo : transfer these to BrainData
    public string current_goal;
    public List<string> goals = new List<string>();

    // ACTIONS
    public string current_action;
    public List<string> action_queue = new List<string>();

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
            current_goal = this.current_goal,
            goals = new List<string>(this.goals),
            current_action = this.current_action,
            action_queue = new List<string>(this.action_queue)
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"\n- Agent Type : {agent_type}";
        details += $"\n- Current Goal : {current_goal}";
        details += $"\n- Goals : {string.Join(", ", goals)}";
        details += $"\n- Current Action : {current_action}";
        details += $"\n- Action Queue : {string.Join(", ", action_queue)}";
        return base.GetDetails() + details;
    }
}