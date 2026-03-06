


using System;
using UnityEngine;

public interface IColliderData : IData
{
    IColliderData Duplicate();
}




[Serializable]
public class ColliderData : IColliderData
{
    // collider data
    public Vector2 local_position;
    public int layerID;
    public bool used_for_pathfinding;
    public bool is_trigger;
    public Vector2 offset;


    // DUPLICATE
    public virtual IColliderData Duplicate()
    {
        ColliderData new_data = new ColliderData()
        {
            local_position = this.local_position,
            layerID = this.layerID,
            used_for_pathfinding = this.used_for_pathfinding,
            is_trigger = this.is_trigger,
            offset = this.offset
        };

        return new_data;
    }


    // GET DETAILS
    public virtual string GetDetails()
    {
        string details = $"collider data :\n";
        details += $"     - local_position : {local_position}\n";
        details += $"     - layerID : {layerID} ({LayerMask.LayerToName(layerID)})\n";
        details += $"     - used_for_pathfinding : {used_for_pathfinding}\n";
        details += $"     - is_trigger : {is_trigger}\n";
        return details;
    }
}
[Serializable]
public class CircleData : ColliderData
{
    public float radius;


    // DUPLICATE
    public override IColliderData Duplicate()
    {
        return new CircleData()
        {
            local_position = this.local_position,
            layerID = this.layerID,
            used_for_pathfinding = this.used_for_pathfinding,
            is_trigger = this.is_trigger,
            offset = this.offset,
            radius = this.radius
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = $"     - radius : {radius}\n";
        return base.GetDetails() + details;
    }
}
[Serializable]
public class BoxData : ColliderData
{
    public Vector2 size;

    // DUPLICATE
    public override IColliderData Duplicate()
    {
        return new BoxData()
        {
            local_position = this.local_position,
            layerID = this.layerID,
            used_for_pathfinding = this.used_for_pathfinding,
            is_trigger = this.is_trigger,
            offset = this.offset,
            size = this.size
        };
    }
    // GET DETAILS
    public override string GetDetails()
    {
        string details = $"     - size : {size}\n";
        return base.GetDetails() + details;
    }
}
