using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class ClosestFoodSensor : LocalTargetSensorBase
    {

        private static FoodDetector _food_detector;
        private static FoodDetector food_detector
        {
            get
            {
                if (_food_detector == null)
                {
                    _food_detector = GameObject.FindFirstObjectByType<OverlapFoodDetector>();
                }
                return _food_detector;
            }
        }

        public override void Created() { }
        public override void Update() { }
        
        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {
            // Get a cached reference to the IA on the agent & EatCapacity
            IA ia = references.GetCachedComponentInParent<IA>();
            if (ia.JustLoaded)
            {
                // we check that we have a valid target, if yes we return it (it was loaded when the MotorCapacity loaded the MotorData' local world data)
                if (target != null && target is CapableTarget)
                {
                    if (Logger.LazyInstance.LOG_CLOSEST_FOOD_SENSOR) { Debug.Log($"(ClosestFoodSensor - Sense) {ia.data.id} just loaded and has an existing target : {target}. We keep it."); }
                    return target;
                }
            }



            // Debug.Log($"(ClosestFoodSensor) {ia.name} is sensing closest food...");
            EatCapacity eatCapacity = ia.GetCapacity<EatCapacity>();
            EatData eatData = eatCapacity?.data as EatData;
            if (eatData == null) { return null; }
            
            // find the closest food
            Capable closestFoodTarget = food_detector.FindClosestFood(ia.data, eatData);
            if (closestFoodTarget == null) { return null; }

            // If the target is already set, we update it
            if (target is CapableTarget transformTarget)
            {
                return transformTarget.SetCapable(closestFoodTarget);
            }
            return new CapableTarget(closestFoodTarget);
        }
    }
}