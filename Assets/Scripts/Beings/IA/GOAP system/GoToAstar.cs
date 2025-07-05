using Pathfinding;
using UnityEngine;

public class GoToAstar : Action
{

    [Header("GOTO ASTAR")]
    public bool HasTarget => target != null; // check if we have a target to go to
    public Transform target;
    public float threshold_distance = 0.2f; // current distance to the destination to consider it reached
    public float update_path_interval = 0.5f; // interval to update the pathfinding

    [Header("Pathfinding")]
    [SerializeField] private Vector2 current_waypoint_destination;
    private Path path; // the current path
    [SerializeField] private int current_waypoint = 0; // the current waypoint on the path
    [SerializeField] private bool path_done = false;

    [Header("Components")]
    private WalkCapacity walker;

    public void Start()
    {
        // get the walk capacity of the ia
        walker = ia.GetCapacity<WalkCapacity>();
        if (walker == null)
        {
            Debug.LogError("(IA) " + name + " must have a WalkCapacity to go to a target!");
        }
    }

    // DOING
    public override void Do()
    {
        if (!HasTarget)
        {
            Debug.LogWarning("(IA) " + name + " has no target to go to.");
            return;
        }

        // we calculate the path to follow
        InvokeRepeating(nameof(CalculatePath), 0f, update_path_interval); // we calculate the path every 0.5 seconds
    }


    // PATH MANAGEMENT
    public void CalculatePath()
    {
        // we find a path to follow
        ia.seeker.StartPath(ia.transform.position, target.position, OnPathComplete);
    }
    public void OnPathComplete(Path path)
    {
        if (path.error)
        {
            if (debug) { Debug.LogError("(IA) " + name + " failed to find a path to " + target.name + ": " + path.errorLog); }
            return;
        }

        // we initialize the path & waypoints variables
        this.path = path;
        current_waypoint = 0;
        path_done = false;
        current_waypoint_destination = path.vectorPath[0];

        // we start walking
        walker.walk_percentage_target = 1f;
        if (debug) { Debug.Log("(IA) " + name + " found a path to " + target.name + " with " + path.vectorPath.Count + " waypoints."); }
    }

    // UPDATE
    public override void UpdateAction()
    {
        // if we already finished this action we return
        if (done) { return; }

        // check if we have a path & waypoints
        if (path == null || path_done) { return; }

        // on regarde si on est arrivé au prochain point
        if (Vector2.Distance(ia.transform.position, current_waypoint_destination) <= threshold_distance)
        {
            // we go to next point
            current_waypoint++;
            if (current_waypoint >= path.vectorPath.Count)
            {
                // if we reached the end of the path, we succeed
                path_done = true;
                succeed();
                return;
            }

            // we update the current waypoint destination
            current_waypoint_destination = path.vectorPath[current_waypoint];
        }

        // we calculate the direction of the movement towards the waypoint
        Vector2 direction_to_waypoint = (current_waypoint_destination - (Vector2)ia.transform.position).normalized;
        ia.Orientation = direction_to_waypoint;
    }

    // SUCCEEDING
    protected override void succeed()
    {
        CancelInvoke(nameof(CalculatePath));
        walker.walk_percentage_target = 0f;

        // mark the action as done
        done = true;
        if (debug) { Debug.Log("(Action) " + name + " is done!"); }
    }

    // QUITTING
    public override void Quit()
    {
        CancelInvoke(nameof(CalculatePath));

        walker.walk_percentage_target = 0f;
        if (debug) { Debug.Log("(Action) " + name + " is not done but finished"); }
    }
}