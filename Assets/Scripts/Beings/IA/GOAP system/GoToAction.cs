using UnityEngine;

public class GoToAction : Action
{

    [Header("GOTO")]
    public bool has_destination = false;
    public Vector2 destination;
    public float threshold_distance = 0.2f; // current distance to the destination to consider it reached

    // DOING
    public override void Do()
    {
        if (debug) { Debug.Log("(IA) " + name + " is going to transform: " + destination); }

        // we set the destination
        has_destination = true;
        if (ia.HasCapacity<WalkCapacity>())
        {
            ia.GetCapacity<WalkCapacity>().walk_percentage_target = 1f; // we start walking
        }
    }

    // UPDATE
    public override void UpdateAction()
    {
        // GOTO BEHAVIOR
        if (!has_destination) { ia.Orientation = new Vector2(0, 0); return; } // we have no destination, we stop moving

        // we update the cost
        cost = CalculateCost();

        // on regarde si on est arrivé à la destination
        if (Vector2.Distance(ia.transform.position, destination) <= threshold_distance)
        {
            succeed(); // we reached the destination, we succeed the action
            return; // we stop the action
        }

        // sinon on se dirige vers la destination
        Vector2 global_movement = new Vector2(destination.x - transform.position.x, destination.y - transform.position.y);
        ia.Orientation = global_movement.normalized;
    }
   
    // SUCCEEDING
    protected override void succeed()
    {
        // we stop going anywhere
        has_destination = false;
        if (ia.HasCapacity<WalkCapacity>())
        {
            ia.GetCapacity<WalkCapacity>().walk_percentage_target = 0f; // we stop walking
        }

        // we mark the action as done
        base.succeed();
    }


    // COST
    public float CalculateCost()
    {
        if (!has_destination) { return 0f; } // no destination, no cost

        // we calculate the cost based on the distance to the destination
        float distance_to_destination = Vector2.Distance(transform.position, destination);

        // we multiply by a factor to adjust the cost
        return distance_to_destination * 0.1f;
    }
}