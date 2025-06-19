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
    private float treshold_distance = 1f; // distance to the destination to consider it reached


    public override void Events()
    {
        base.Events();

        // GOTO BEHAVIOR
        if (has_destination)
        {
            // on regarde si on est pas TROP proche du joueur
            if (Vector2.Distance(transform.position, destination) < treshold_distance)
            {
                Orientation = new Vector2(0, 0);
                destination = new Vector2(0, 0);
                has_destination = false; // we reached the destination
                inputs_magnitude = 0f; // we stop moving
                return;
            }

            // on se dirige vers le joueur
            Vector2 global_movement = new Vector2(destination.x - transform.position.x, destination.y - transform.position.y);
            Orientation = global_movement.normalized;
            return;
        }
        else { Orientation = new Vector2(0, 0); /* on bouge pas*/ }
    }

    // BEHAVIORS
    protected IEnumerator GoTo(Vector2 position)
    {
        // we want to go to a position

        // ! for now it goes in a straight line, but we could use NavMeshAgent to go around obstacles

        if (debug) { Debug.Log("(IA) " + name + " is going to transform: " + position); }

        // we set the destination
        destination = position;
        has_destination = true;
        inputs_magnitude = 1f; // we start moving

        // we wait until we reach the destination
        while (has_destination) { yield return null; }
        if (debug) { Debug.Log("(IA) " + name + " reached transform: " + position); }
    }
}