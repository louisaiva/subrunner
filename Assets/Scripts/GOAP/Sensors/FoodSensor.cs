using System.Collections.Generic;
using System.Linq;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace subrunner.goap
{
    public class FoodSensor : MultiSensorBase, IInjectable
    {
        private Food[] foods;
        private IA ia;

        public FoodSensor()
        {
            /* this.AddLocalWorldSensor<FoodReadyToBeEaten>((agent, references) =>
            {
                // Get a cached reference to the Cat on the agent
                var brain = references.GetCachedComponent<Brain>();

                return brain.ia.food_ready_to_be_eaten ? 1 : 0;
            }); */

            this.AddLocalWorldSensor<Hunger>((agent, references) =>
            {
                // Get a cached reference to the IA on the agent
                var data = references.GetCachedComponent<Brain>();

                // this.ia = data.ia;

                // string log = "(FoodSensor) " + ia.name + " is creating the hunger local food sensor"
                //     + "\n\tFood rule is: " + this.ia.FoodRule;
                // if (ia.debug_goals) { Debug.Log(log); }
                
                // We need to cast the float to an int, because the hunger is an int
                // We will lose the decimal values, but we don't need them for this example
                return (int)ia.hunger;
            });

            this.AddLocalTargetSensor<ClosestFood>((agent, references, target) =>
            {
                // Use the cashed foods list to find the closest food
                var closestFood = this.Closest(this.foods, agent.Transform.position);

                if (closestFood == null)
                    return null;

                // If the target is a transform target, set the target to the closest food
                if (target is TransformTarget transformTarget)
                    return transformTarget.SetTransform(closestFood.transform);

                return new TransformTarget(closestFood.transform);
            });
        }

        public override void Created() {}

        public void Inject(DependencyInjector injector)
        {
            // throw new System.NotImplementedException();
        }


        // UPDATE
        public override void Update()
        {
            Debug.Log("(FoodSensor) updating the food sensor, ia is " + ia);
            string log = "(FoodSensor) " + ia.name + " is updating the food sensor"
                + "\n\tFood rule is: " + this.ia.FoodRule
                + "\n\tFoods found: " + this.foods.Length;
            if (ia.debug_goals) { Debug.Log(log); }

            this.foods = GameObject.FindObjectsByType<Food>(FindObjectsSortMode.None)
                .Where(food => !food.Grabbed && food.Eatable) // only food on the ground & eatable
                .Where(food => food.ValidateRule(ia.FoodRule)) // and that passes the food rule check of the ia
                .ToArray();
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