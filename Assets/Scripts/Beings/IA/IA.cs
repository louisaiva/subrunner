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
    public float hunger = 0f; // Hunger level of the cat, the less the better
    [SerializeField] private string[] items_eatable = new string[] { "apple" }; // List of items that the IA can eat
    public string FoodRule { get
        {
            // return the total item_rule to see if the ia can eat a precise item
            if (items_eatable.Length == 0) { return "food"; }
            string rule = "";
            for (int i = 0; i < items_eatable.Length; i++)
            {
                rule += "food:" + items_eatable[i] + ",";
            }
            rule = rule.TrimEnd(','); // we remove the last comma
            return rule;
        }
    }

    // public bool food_ready_to_be_eaten = false; // If the food is ready to be eaten

    // [SerializeField] private Transform goal_parent;
    // public List<Goal> goals = new List<Goal>(); // list of goals that the IA can achieve
    // public Goal current_goal; // the current goal that the IA is trying to achieve

    [Header("Pathfinding")]
    public Seeker seeker; // the seeker component used for pathfinding

    [Header("Logs")]
    public bool debug_goals = false;
    public bool debug_doable_goals = false;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // we get the seeker component
        seeker = GetComponent<Seeker>();

        /* if (goal_parent == null)
        {
            Debug.LogError("(IA) " + name + " has no goal parent set! Please set a goal parent in the inspector.");
            return;
        }

        // we get the goals from the goal parent
        goals = goal_parent.GetComponentsInChildren<Goal>().ToList();
        if (goals.Count == 0)
        {
            Debug.LogError("(IA) " + name + " has no goals set! Please add at least the default idle goal in the inspector");
        } */

        // we sort the goals by descending priority order (so the highest priority is in first)
        // goals = goals.OrderByDescending(g => g.priority).ToList();
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        hunger += Time.deltaTime * 1f;
    }
    // GOALS MANAGEMENT
    /* protected override void Update()
    {
        base.Update();

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
    } */
    /* protected virtual void SwitchGoal(Goal new_goal)
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
    private Goal get_highest_priority_doable_goal()
    {

        // we go through the goals from 0 to Count and we return the first doable one
        for (int i = 0; i < goals.Count; i++)
        {
            if (goals[i].Doable)
            {
                if (debug_doable_goals) { Debug.Log("(IA) " + name + " has Doable goal: " + goals[i].GetType()); }
                return goals[i];
            }
        }

        if (debug_doable_goals) { Debug.Log("(IA) " + name + " has no Doable goals"); }
        return null;


        /* // we filter the goals to get only the Doable ones
        var doable_goals = goals.Where(g => g.Doable).ToList();

        if (doable_goals.Count == 0)
        {
        }

        // we return the highest priority Doable goal
        return doable_goals.OrderByDescending(g => g.priority).FirstOrDefault(); 
    } */
}