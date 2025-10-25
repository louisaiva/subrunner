using UnityEngine;
using subrunner.goap;

public class Detector : MonoBehaviour
{
    [Header("Detector Settings")]
    protected Brain brain;
    [SerializeField] protected GoalType goal_type = GoalType.None; // The type of goal this detector is looking for
    protected GoalPriority goal;

    [Header("Components")]
    protected IA ia;
    protected virtual void Awake()
    {
        ia = transform.parent.GetComponent<IA>();
        brain = ia.Brain;
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