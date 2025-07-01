using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Goals")]
    [SerializeField] private Transform goal_parent;
    public List<Goal> goals = new List<Goal>(); // list of goals that the IA can achieve
    public Goal current_goal; // the current goal that the IA is trying to achieve

    [Header("Debug")]
    public bool debug_goals = false;
    public bool debug_doable_goals = false;


    // [Header("Behaviours - GOTO")]
    // public bool has_destination = false;
    // public Vector2 destination;
    // private float base_threshold_distance = 0.2f; // distance to the destination to consider it reached (base)
    // private float threshold_distance; // current distance to the destination to consider it reached (because sometimes we can't have a very precise distance)

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        if (goal_parent == null)
        {
            Debug.LogError("(IA) " + name + " has no goal parent set! Please set a goal parent in the inspector.");
            return;
        }
        
        // we get the goals from the goal parent
        goals = goal_parent.GetComponentsInChildren<Goal>().ToList();
        if (goals.Count == 0)
        {
            Debug.LogError("(IA) " + name + " has no goals set! Please add at least the default idle goal in the inspector");
        }
    }

    protected override void UpdateGOAP()
    {
        // 1 - detect if one of the goal have a higher priority than the actual goal
        Goal max_priority_goal = get_highest_priority_doable_goal();
        if (max_priority_goal == null) { return; } // no goal to switch to

        // checks if the current goal is the max priority goal
        if (max_priority_goal != current_goal)
        {
            // we switch to the new goal
            SwitchGoal(max_priority_goal);
        }

        // 2 - update current goal
        current_goal.UpdateGoal();


        // GOTO BEHAVIOR
        /* if (has_destination)
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
        else { Orientation = new Vector2(0, 0); /* on bouge pas } */
    }

    // GOALS MANAGEMENT HIGH LEVEL
    protected virtual void SwitchGoal(Goal new_goal)
    {
        // we stop the current goal
        if (current_goal != null)
        {
            if (debug_goals) { Debug.Log("(IA) " + name + " stopped goal: " + current_goal.GetType()); }
            current_goal.ClearPlan(); // we clear the plan of the current goal
        }

        // we start the new goal
        current_goal = new_goal;
        current_goal.Plan(); // we plan the new goal
        if (debug_goals) { Debug.Log("(IA) " + name + " switched to goal: " + current_goal.GetType()); }
    }

    // GOALS LOW LEVEL
    private Goal get_highest_priority_doable_goal()
    {
        // we filter the goals to get only the Doable ones
        var doable_goals = goals.Where(g => g.Doable).ToList();

        if (doable_goals.Count == 0)
        {
            if (debug_doable_goals) { Debug.Log("(IA) " + name + " has no Doable goals"); }
            return null;
        }

        // we return the highest priority Doable goal
        return doable_goals.OrderByDescending(g => g.priority).FirstOrDefault();
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
    } */
}