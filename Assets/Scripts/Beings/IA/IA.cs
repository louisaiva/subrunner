using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using subrunner.goap;
using UnityEngine;

/// <summary>
/// IA is a class that represents an AI being in the game.
/// it is a Capable of course so it has Capacities & Effects,
/// BUT also have Behaviors which define how it behaves in the game world.
/// </summary>

public class IA : Being
{
    [Header("IA")]
    public float exploration_radius = 3f; // the radius of exploration for the IA
                                          // todo must be part of an IAData class or struct that influence a curiosity parameter
    public Transform eyes;
    public GoToBehaviour mover;
    public Brain Brain => transform.Find("brain").GetComponent<Brain>();
    [SerializeField] private string base_tag = "IA";
    public string BaseTag => base_tag;
    // public Seeker seeker { get; private set; } // the seeker component used for pathfinding

    [Header("Logs")]
    public bool log_actions = false;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we set the tag
        gameObject.tag = base_tag;

        // we get the eyes component
        eyes = transform.Find("eyes");
        mover = transform.Find("brain/goto").GetComponent<GoToBehaviour>();
    }
}