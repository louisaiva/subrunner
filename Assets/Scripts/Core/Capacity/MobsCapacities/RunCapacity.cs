
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RunCapacity is a capacity that allows a movable object to run.
/// needs to have a WalkCapacity on the capable
/// the feet_collider collider is used to detect collisions with the world.
/// </summary>

[Obsolete("RunCapacity is deprecated, use WalkCapacity instead (it now handles both walking and running)")]
public class RunCapacity : Capacity
{
    [Header("Running parameters")]
    [SerializeField] private bool IsRunning = false; // est-ce qu'on est en train de courir
    public float max_run_speed = 3f; // vitesse maximale de déplacement lors de la course
    [SerializeField] private float max_walk_speed = 0f; // vitesse maximale de déplacement lors de la marche
    [SerializeField] private float run_multiplier = 1f; // multiplicateur de vitesse lors de la course -> s'applique sur max_walk_speed pour calculer max_run_speed

    [Header("Components")]
    private WalkCapacity _walker;
    public WalkCapacity Walker
    {
        get
        {
            if (_walker == null)
            {
                if (Capable.HasCapacity<WalkCapacity>())
                {
                    _walker = Capable.GetCapacity<WalkCapacity>();
                }
                else
                {
                    Debug.LogError("(RunCapacity) " + Capable.name + " has no WalkCapacity! RunCapacity won't work!");
                    return null;
                }
            }
            return _walker;
        }
    }


    public void EnableRun()
    {
        // si on court déjà on return
        if (IsRunning) { return; }

        // on récupère la WalkCapacity
        WalkCapacity walker = Walker;
        if (walker == null) { return; } // on check si on a un walk capacity

        max_walk_speed = walker.max_run_speed; // on récupère la vitesse de marche de base

        // si on a un multiplier alors on utilise le multiplier plutot que la vitesse de run
        if (run_multiplier > 1f) { max_run_speed = max_walk_speed * run_multiplier; }
        walker.max_run_speed = max_run_speed; // on met la vitesse de course
        IsRunning = true;

        // on applique le changement au son
        walker.walk_sound.setParameterByName("running", 1);

        if (log) { Debug.Log("(RunCapacity) " + Capable.name + " is now running at " + max_run_speed + " speed!"); }
    }
    public void DisableRun()
    {
        // si on court déjà on return
        if (!IsRunning) { return; }

        // on récupère la WalkCapacity
        WalkCapacity walker = Walker;
        if (walker == null) { return; }
        walker.max_run_speed = max_walk_speed; // on change la vitesse maximale de marche
        IsRunning = false; // on desactive la course

        // on applique le changement au son
        walker.walk_sound.setParameterByName("running", 0);

        if (log) { Debug.Log("(RunCapacity) " + Capable.name + " is now walking at " + max_walk_speed + " speed!"); }
    }

}