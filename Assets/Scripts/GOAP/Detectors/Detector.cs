using UnityEngine;
using subrunner.goap;

public class Detector : MonoBehaviour
{
    [Header("Detector Settings")]
    [SerializeField] protected Brain brain;
    [SerializeField] protected GoalType goal_type = GoalType.None; // The type of goal this detector is looking for
    protected GoalPriority goal;

    protected virtual void Awake()
    {
        if (brain == null)
        {
            Debug.LogError($"(Detector) {name} requires a Brain component. Please assign it in the inspector");
        }
    }

    protected virtual void Start()
    {
        // Find the goal by name in the brain's goals
        goal = brain.GetGoal(goal_type);
        if (goal == null)
        {
            Debug.LogError($"(Detector) Goal '{goal_type}' not found in Brain's goals. Please ensure it is defined.");
        }
    }
}