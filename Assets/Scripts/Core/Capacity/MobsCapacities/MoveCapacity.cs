using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a simpler version of GoToBehaviour,
/// that controls a WalkCapacity to move an entity
/// to a specific Vector2 destination. This does NOT
/// handle any path related logic, any avoidance logic, neither
/// does it handle any pathfinding logic. Destination
/// must be calculated before calling MoveTo, and it is the responsibility of the caller to make sure the
/// destination can be reached
/// </summary>
public class MoveCapacity : Capacity
{
    private const float DESTINATION_REACHED_THRESHOLD = 0.1f; // distance to the destination under which we consider we reached it 

    // update data
    private bool has_destination = false;
    private Vector2 destination;
    private WalkCapacity walker;
    [SerializeField] private float speed_percentage = 1f;

    // events
    private System.Action OnDestinationReachedCallback = null;

    ///
    //
    /// MAIN ENTRY POINTS
    //
    ///

    public void WalkTo(Vector2 destination, float? speed_percentage = null)
    {
        MoveTo(destination, speed_percentage, run: false);
    }
    public void RunTo(Vector2 destination, float? speed_percentage = null)
    {
        MoveTo(destination, speed_percentage, run: true);
    }
    public void MoveTo(Vector2 destination, float? speed_percentage = null, bool run = false)
    {
        if (walker == null)
        {
            if (!TryGetSiblingCapacity(out walker))
            {
                Debug.LogError("(MoveCapacity) " + Capable.ID + " doesn't have a WalkCapacity, which is required for MoveCapacity to work.");
                return;
            }
        }

        // we set the destination and we start updating our movement til the destination is reached
        this.destination = destination;
        if (speed_percentage != null) { this.speed_percentage = speed_percentage.Value; }
        this.has_destination = true;
        if (run) { walker.EnableRun(); }
        else { walker.DisableRun(); }
    }
    public void SetOnDestinationReachedCallback(System.Action callback)
    {
        this.OnDestinationReachedCallback = callback;
    }
    public void SetSpeedPercentage(float speed_percentage)
    {
        this.speed_percentage = speed_percentage;
    }
    public void SetRun(bool run)
    {
        if (walker == null) { return; }
        if (run) { walker.EnableRun(); }
        else { walker.DisableRun(); }
    }


    ///
    //
    /// UPDATE
    //
    ///

    // UPDATE
    public void Update()
    {
        if (!has_destination) { return; }
        // here we always have a destination and a walker

        if (has_reached_destination())
        {
            if (log) { Debug.Log("(MoveCapacity) " + Capable.ID + " has reached the destination."); }
            stop_movement();
            OnDestinationReachedCallback?.Invoke();
            return;
        }

        // here we need to move towards destination
        Vector2 movement_direction = (destination - (Vector2)Capable.transform.position).normalized;
        Capable.Orientation = movement_direction; // rotate towards the destination
        walker.walk_percentage_target = speed_percentage;
    }
    private bool has_reached_destination(float threshold = DESTINATION_REACHED_THRESHOLD)
    {
        if (Vector2.Distance(Capable.transform.position, destination) > threshold) { return false; }
        
        // if we are here we reached destination for this specific threshold !
        return true;
    }
    private void stop_movement()
    {
        walker.walk_percentage_target = 0f; // we stop walking
        walker.DisableRun();
        if (log) { Debug.Log("(MoveCapacity) " + Capable.ID + " has stopped moving."); }
        
        has_destination = false;
        // speed_percentage = 1f; // don't reset speed bcz we want to remember which speed we were using for next time
    }




    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / UNLOAD DATA
    /* public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);

        if (data is not MoveData specific_data) { return; }
        // load custom data here
    } */
    public override void UnloadData()
    {
        // unload custom data here
        if (has_destination) { stop_movement(); }
        walker = null;

        base.UnloadData();
    }

    // GET STATIC DATA
    /* public override CapacityData GetStaticData()
    {
        MoveData static_data = new MoveData(base.GetStaticData())
        {
            // set custom data fields here
        };

        // set more complicated data fields check here if needed

        return static_data;
    } */
}





// [Serializable] public class MoveData : CapacityData
// {
//     // declare custom data fields here
//     public List<Vector2> path = new List<Vector2>();
//     public int current_point_index = 0;
//     public float threshold_distance = 0.1f;



//     // CONSTRUCTOR
//     public MoveData(CapacityData parent) : base(parent) { }

//     // DUPLICATE
//     public override ICapacityData Duplicate()
//     {
//         return new MoveData(base.Duplicate() as CapacityData)
//         {
//             // copy fields here
//         };
//     }

//     // GET DETAILS
//     public override string GetDetails()
//     {
//         string details = "";
//         // add details to the string here
//         // ex : details += $"  - slot color : {slot_color}\n";
//         return base.GetDetails() + details;
//     }
// }