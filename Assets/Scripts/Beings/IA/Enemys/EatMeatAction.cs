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
        EatMeatCapacity capa = ia.GetCapacity<EatMeatCapacity>();
        capa.meat_target = target; // set the meat target for the capacity
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