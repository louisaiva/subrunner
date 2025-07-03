
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RunCapacity is a capacity that allows a movable object to run.
/// needs to have a WalkCapacity on the capable
/// the feet_collider collider is used to detect collisions with the world.
/// </summary>

public class RunCapacity : Capacity
{
    [Header("Running parameters")]
    [SerializeField] private bool IsRunning = false; // est-ce qu'on est en train de courir
    public float max_run_speed = 3f; // vitesse maximale de déplacement lors de la course
    [SerializeField] private float max_walk_speed = 0f; // vitesse maximale de déplacement lors de la marche

    public void EnableRun()
    {
        // si on court déjà on return
        if (IsRunning) { return; }

        // on check si on a un walk capacity
        if (!capable.HasCapacity<WalkCapacity>()) { Debug.LogError("(RunCapacity) needs a WalkCapacity to work!"); return; }

        // on active la course
        IsRunning = true;

        // on récupère la WalkCapacity
        WalkCapacity walk_capacity = capable.GetCapacity<WalkCapacity>();
        max_walk_speed = walk_capacity.max_speed; // on récupère la vitesse de marche de base
        walk_capacity.max_speed = max_run_speed; // on change la vitesse maximale de marche

        if (debug) { Debug.Log("(RunCapacity) " + capable.name + " is now running at " + max_run_speed + " speed!"); }
    }
    public void DisableRun()
    {
        // si on court déjà on return
        if (!IsRunning) { return; }

        // on check si on a un walk capacity
        if (!capable.HasCapacity<WalkCapacity>()) { Debug.LogError("(RunCapacity) needs a WalkCapacity to work!"); return; }

        // on active la course
        IsRunning = false;

        // on récupère la WalkCapacity
        WalkCapacity walk_capacity = capable.GetCapacity<WalkCapacity>();
        walk_capacity.max_speed = max_walk_speed; // on change la vitesse maximale de marche

        if (debug) { Debug.Log("(RunCapacity) " + capable.name + " is now walking at " + max_walk_speed + " speed!"); }
    }

}