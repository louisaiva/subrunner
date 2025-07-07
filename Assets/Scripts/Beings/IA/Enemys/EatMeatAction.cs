using UnityEngine;

public class EatMeatAction : CapacityAction
{
    [Header("Eat Meat Action")]
    public Meat target;
    // DOING
    public override void Do()
    {
        if (!ia.HasCapacity(capacity)) { return; }

        // we do the capacity
        if (debug) { Debug.Log("(EatMeatAction) " + name + " is eating meat: " + target.name); }
        EatCapacity capa = ia.GetCapacity<EatCapacity>();
        capa.SetFoodTarget(target);
        ia.Do(capacity);

        if (!wait_for_animation_to_finish)
        {
            // we succeed the action immediately
            base.succeed();
            return;
        }

        // we wait for the attack to finish
        StartCoroutine(WaitForAnim());
    }
}