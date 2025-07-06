using UnityEngine;

public class Cat : IA
{
    [Header("Cat Parameters")]
    public float hunger = 0f; // Hunger level of the cat, the less the better
    public bool food_ready_to_be_eaten = false; // If the food is ready to be eaten

    protected override void Update()
    {
        base.Update();
        
        hunger += Time.deltaTime * 0.1f;
    }    
}