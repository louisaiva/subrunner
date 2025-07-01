using System;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("Goal")]
    public float priority = 0f; // Priority of the goal, higher means more important
    public virtual bool Doable { get { return true; } } // if the goal is Doable, if not we don't plan it
    // private IA ia;
    // public string goal_name = "Idle";

    [Header("Plans & Actions")]

    [Header("Plans")]
    public bool has_plan = false; // if we have a plan to achieve the goal
    public List<Action> current_plan; // the current plan that is being executed
    public Action current_action { get { return current_plan.Count > 0 ? current_plan[0] : null; } } // the current action that is being executed

    [Header("Debug")]
    public bool debug = false;

    // UPDATE
    public virtual void UpdateGoal()
    {
        if (current_plan == null) { return; }
        if (current_action == null)
        {
            // we have a plan but nothing in it -> we achieved the goal
            ClearPlan();
            return;
        }

        // check if the current action is done
        if (current_action.done)
        {
            destroy_current_action();

            // si on a une action suivante alors on la fait
            if (current_action) { current_action.Do(); }
        }

        // update the current action
        current_action?.UpdateAction();
    }

    // PLANNING
    public virtual bool Plan()
    {
        // try to find a way to achieve the goal
        // if there are multiple ways we do the one with the smallest cost

        // check if we have a plan
        if (current_plan == null || current_plan.Count <= 0) { return false; }

        // we have a plan, we launch the first action
        has_plan = true;
        current_action.Do();
        return true;
    }
    public void ClearPlan()
    {
        // we check if we have a plan
        if (current_plan == null) { has_plan = false; return; }

        // we clear the plan by deleting all actions in it
        while (current_plan.Count > 0)
        {
            destroy_current_action();
        }

        has_plan = false; // we don't have a plan anymore
    }

    // ACTION MANAGEMENT LOW LEVEL
    private void destroy_current_action()
    {
        if (!current_action) { return; }

        // we destroy the action
        Destroy(current_action.gameObject);
        current_plan.RemoveAt(0);
    }
}