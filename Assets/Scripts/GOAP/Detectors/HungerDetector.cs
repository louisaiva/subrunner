using UnityEngine;
using subrunner.goap;


public class HungerDetector : Detector
{
    // calls EatGoal when health <= x
    // or hunger <= y
    [Header("Hunger Detector Settings")]
    public float healthPercentageThreshold = 30f; // between 0 & 100
    public float hungerThreshold = 50f;
    [SerializeField] protected float detectionInterval = 0.5f; // How often to check for conditions
    protected float lastDetectionTime = 0f;

    private EatCapacity eatCapacity;

    protected override void Start()
    {
        base.Start();
        eatCapacity = ia.GetCapacity<EatCapacity>();

        lastDetectionTime = Time.time; // initialize the timer
    }

    private void Update()
    {
        // check if has health capacity
        if (!ia.HasCapacity<HealthCapacity>()) { return; }

        // update timer
        if (Time.time - lastDetectionTime <= detectionInterval) { return; }
        lastDetectionTime = Time.time;

        // si le goal est inactif et (qu'on a faim ou pas assez de vie)
        if (!goal.enabled
            && (ia.GetCapacity<HealthCapacity>().LifePourcent <= healthPercentageThreshold * 0.01f
            || eatCapacity.hunger >= hungerThreshold))
        {
            brain.EnableGoal(goal);
            return;
        }

        // si le goal est actif et qu'on a assez de vie et pas faim
        if (goal.enabled
            && ia.GetCapacity<HealthCapacity>().LifePourcent > healthPercentageThreshold * 0.01f
            && eatCapacity.hunger < hungerThreshold)
        {
            brain.DisableGoal(goal);
        }
    }
}