using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

/// <summary>
/// IA is a class that represents an AI being in the game.
/// it is a Capable of course so it has Capacities & Effects,
/// BUT also have Behaviors which define how it behaves in the game world.
/// </summary>

[RequireComponent(typeof(Seeker))]
public class IA : Being
{
    [Header("IA")]
    public float exploration_radius = 3f; // the radius of exploration for the IA
    // todo must be part of an IAData class or struct that influence a curiosity parameter
    public Seeker seeker { get; private set; } // the seeker component used for pathfinding

    [Header("Logs")]
    public bool debug_goals = false;
    public bool debug_doable_goals = false;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we get the seeker component
        seeker = GetComponent<Seeker>();
    }
}