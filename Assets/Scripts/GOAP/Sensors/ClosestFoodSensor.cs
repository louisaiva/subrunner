using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Goap.Core;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace subrunner.goap
{
    public class ClosestFoodSensor : LocalTargetSensorBase
    {
        private Food[] foods;
        private IA ia;
        private EatCapacity eatCapacity;

        public override void Created() { }
        public override void Update() { }
        
        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget target)
        {
            // Get a cached reference to the IA on the agent & EatCapacity
            if (ia == null) { this.ia = references.GetCachedComponent<Brain>().ia; }
            if (eatCapacity == null) { this.eatCapacity = ia.GetCapacity<EatCapacity>(); }
            if (eatCapacity == null) { return null; }

            this.foods = GameObject.FindObjectsByType<Food>(FindObjectsSortMode.None)
                .Where(food => !food.Grabbed && food.Eatable) // only food on the ground & eatable
                .Where(food => food.ValidateRule(eatCapacity.FoodRule)) // and that passes the food rule check of the ia
                .ToArray();
            
            // Debug.Log($"(ClosestFoodSensor) {ia.name} found {this.foods.Length} food to eat with rule {ia.FoodRule}");

            Food closestFood = this.Closest(this.foods, agent.Transform.position);

            if (closestFood == null) { return null; }

            // If the target is a transform target, set the target to the closest food
            if (target is TransformTarget transformTarget)
            {
                return transformTarget.SetTransform(closestFood.transform);
            }

            return new TransformTarget(closestFood.transform);
        }


        // Returns the closest item in a list
        private T Closest<T>(IEnumerable<T> list, Vector3 position)
            where T : MonoBehaviour
        {
            T closest = null;
            var closestDistance = float.MaxValue; // Start with the largest possible distance

            foreach (var item in list)
            {
                var distance = Vector3.Distance(item.gameObject.transform.position, position);

                if (!(distance < closestDistance))
                    continue;

                closest = item;
                closestDistance = distance;
            }

            return closest;
        }

    }
}