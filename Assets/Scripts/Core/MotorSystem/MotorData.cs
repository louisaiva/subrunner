
using System;
using subrunner.goap;

[Serializable] public class MotorData : CapacityData
{
    public string agent_type;

    // GOALS
    public string current_goal; // todo : transfer this to BrainData

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
            current_goal = this.current_goal,
            avoidance_data = this.avoidance_data.Duplicate()
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - agent type : {agent_type}\n";
        details += $"  - current goal : {current_goal}\n";
        details += $"  - {avoidance_data.GetDetails()}\n";
        return base.GetDetails() + details;
    }
}