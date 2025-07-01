using System.Collections;
using UnityEngine;
/// <summary>
/// simple action that triggers an attack animation
/// and waits for it to finish
/// </summary>
public class AttackAction : Action
{

    [Header("Attack Action")]
    public bool wait_for_animation_to_finish = true; // if we should wait for the animation to finish before succeeding the action

    // DOING
    public override void Do()
    {
        if (!ia.HasCapacity<AttackCapacity>()) { return; }

        // we attack
        if (debug) { Debug.Log("(IA) " + name + " is attacking !"); }
        ia.GetCapacity<AttackCapacity>().Use(ia);

        if (!wait_for_animation_to_finish)
        {
            // we succeed the action immediately
            base.succeed();
            return;
        }

        // we wait for the attack to finish
        StartCoroutine(WaitForAttack());
    }

    private IEnumerator WaitForAttack()
    {
        // wait for the attack to finish
        while (ia.anim_player.current_capacity == "attack") { yield return null; }

        // we succeed the action
        base.succeed();
    }
}