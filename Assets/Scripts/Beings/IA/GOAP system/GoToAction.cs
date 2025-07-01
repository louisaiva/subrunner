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
    protected override void succeed(bool mark_as_done=true)
    {
        // we stop going anywhere
        has_destination = false;
        if (ia.HasCapacity<WalkCapacity>())
        {
            ia.GetCapacity<WalkCapacity>().walk_percentage_target = 0f; // we stop walking
        }

        // we mark the action as done
        base.succeed(mark_as_done);
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

    // BEHAVIORS
    /* protected IEnumerator GoToCoroutine(Vector2 position, float? threshold_distance = null)
    {
        // we want to go to a position

        // ! for now it goes in a straight line, but we could use NavMeshAgent to go around obstacles

        if (debug) { Debug.Log("(IA) " + name + " is going to transform: " + position); }

        // we set the destination
        destination = position;
        has_destination = true;
        if (HasCapacity<WalkCapacity>())
        {
            GetCapacity<WalkCapacity>().walk_percentage_target = 1f; // we start walking
        }

        // we set the threshold distance
        this.threshold_distance = threshold_distance ?? base_threshold_distance; // if no threshold distance is given, we use the base one

        // we wait until we reach the destination
        while (has_destination) { yield return null; }
        if (debug) { Debug.Log("(IA) " + name + " reached transform: " + position); }
    } */
    /* protected void GoTo(Vector2 position, float? threshold_distance = null)
    {
        if (has_destination)
        {
            // check if the destination is the same
            if (destination == position) { return; }

            // we already had a destination, we override it
            StopCoroutine("GoToCoroutine");
        }

        StartCoroutine(GoToCoroutine(position, threshold_distance));
    }
    protected void GoNowhere()
    {
        // we stop going anywhere
        has_destination = false;
        threshold_distance = base_threshold_distance; // reset the treshold distance to the base value
        if (HasCapacity<WalkCapacity>())
        {
            GetCapacity<WalkCapacity>().walk_percentage_target = 0f; // we stop walking
        }
    } */

}