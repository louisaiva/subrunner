using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// IA is a class that represents an AI being in the game.
/// it is a Capable of course so it has Capacities & Effects,
/// BUT also have Behaviors which define how it behaves in the game world.
/// </summary>

// [RequireComponent(typeof(NavMeshAgent))]
public class IA : Being
{
    [Header("Behaviours - GOTO")]
    public bool has_destination = false;
    public Vector2 destination;
    private float base_threshold_distance = 0.2f; // distance to the destination to consider it reached (base)
    private float threshold_distance; // current distance to the destination to consider it reached (because sometimes we can't have a very precise distance)


    public override void Events()
    {
        base.Events();

        // GOTO BEHAVIOR
        if (has_destination)
        {
            // on regarde si on est pas TROP proche de la destination
            if (Vector2.Distance(transform.position, destination) < threshold_distance)
            {
                Orientation = new Vector2(0, 0);
                destination = new Vector2(0, 0);
                has_destination = false; // we reached the destination
                                         // inputs_magnitude = 0f; // we stop moving
                if (HasCapacity<WalkCapacity>())
                {
                    GetCapacity<WalkCapacity>().walk_percentage_target = 0f; // we stop walking
                }
                return;
            }

            // on se dirige vers la destination
            Vector2 global_movement = new Vector2(destination.x - transform.position.x, destination.y - transform.position.y);
            Orientation = global_movement.normalized;
            return;
        }
        else { Orientation = new Vector2(0, 0); /* on bouge pas*/ }
    }

    // BEHAVIORS
    protected IEnumerator GoToCoroutine(Vector2 position, float? threshold_distance = null)
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
    }
    protected void GoTo(Vector2 position,float? threshold_distance = null)
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
    }
}