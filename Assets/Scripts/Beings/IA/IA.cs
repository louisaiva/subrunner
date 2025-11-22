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
    [SerializeField] private string base_tag = "IA";
    public string BaseTag => base_tag;


    [Header("Components")]
    private Detector _eyes;
    public Detector Eyes
    {
        get
        {
            if (_eyes == null) { _eyes = transform.Find("eyes")?.GetComponent<Detector>(); }
            return _eyes;
        }
    }
    private GoToBehaviour _mover;
    public GoToBehaviour Mover
    {
        get
        {
            if (_mover == null) { _mover = transform.Find("brain/goto")?.GetComponent<GoToBehaviour>(); }
            return _mover;
        }
    }
    private Brain _brain;
    public Brain Brain
    {
        get
        {
            if (_brain == null) { _brain = transform.Find("brain")?.GetComponent<Brain>(); }
            return _brain;
        }
    }


    [Header("Logs")]
    public bool log_actions = false;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we set the tag
        gameObject.tag = base_tag;
    }
}