using System.Collections.Generic;
using UnityEngine;

public class OverlapFoodDetector : MonoBehaviour, FoodDetector
{
    [Header("Logs")]
    public bool logs_detection = false;

    public T FindClosestCapableOfType<T>(CapableData looker_data) where T : Capable
    {
        throw new System.NotImplementedException();
    }
    public Food FindClosestFood(CapableData cdata, EatData edata)
    {
        // get the potential foods
        List<Food> potential_foods = DetectPotentialFoods(cdata.position, cdata.id, edata);
        if (potential_foods.Count == 0) { return null; }

        // we find the closest food
        Food closest_food = null;
        float closest_distance = float.MaxValue;
        foreach (Food food in potential_foods)
        {
            float distance = Vector3.Distance(food.gameObject.transform.position, cdata.position);

            if (!(distance < closest_distance))
                continue;

            closest_food = food;
            closest_distance = distance;
        }
        return closest_food;
    }


    // OLD OVERLAPPING DETECTION
    public List<Food> DetectPotentialFoods(Vector2 position, string ia_id, EatData edata)
    {
        // we do an overlap to detect foods
        Collider2D[] results = Physics2D.OverlapCircleAll(position,
            edata.range_food_detection,
            LayerMask.GetMask("Interactives"));
        if (results.Length == 0) { return new List<Food>(); }

        if (logs_detection)
        {
            Debug.Log("(EatCapacity) " + ia_id + " detected " + results.Length + " potential foods in range of " + edata.range_food_detection);
        }

        // we convert those into foods & check few things
        List<Food> potential_foods = new List<Food>();
        foreach (Collider2D collider in results)
        {
            // we check if the parent capable has a Capable component
            Food food = collider.transform.parent.GetComponent<Food>();
            if (food == null) { continue; }

            if (!food.ValidateRule(edata.food_rule)) { continue; }
            
            // we add the food to the list of potential foods
            potential_foods.Add(food);
            
            /* else if (capable is Corpse corpse && corpse.EatableBy(edata.food_rule))
            {
                // we add the corpse to the list of potential foods
                potential_foods.Add(corpse);
            } */
        }
        return potential_foods;
    }
}