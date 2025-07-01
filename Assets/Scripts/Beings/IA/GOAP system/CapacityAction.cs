using System.Collections;
using UnityEngine;
/// <summary>
/// simple action that triggers a capacity
/// and waits for it to finish / or not
/// </summary>
public class CapacityAction : Action
{

    [Header("Capacity Action")]
    public string capacity = "idle"; // the capacity to use for this action
    public string animation_name = "idle"; // the capacity to use for this action (sometimes its different from capacity name)
    public bool wait_for_animation_to_finish = true; // if we should wait for the animation to finish before succeeding the action

    // DOING
    public override void Do()
    {
        if (!ia.HasCapacity(capacity)) { return; }

        // we do the capacity
        if (debug) { Debug.Log("(IA) " + name + " is "+ capacity + "ing !"); }
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

    protected IEnumerator WaitForAnim()
    {
        // wait for the animation to finish
        while (ia.anim_player.current_capacity == animation_name) { yield return null; }

        // we succeed the action
        base.succeed();
    }
}