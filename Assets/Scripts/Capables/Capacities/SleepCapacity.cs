using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SleepCapacity is a capacity for cats that allows a being to sleep
/// </summary>
public class SleepCapacity : Capacity
{
    private Cat cat => capable as Cat; // we cast the capable to a Cat

    [Header("Sleeping parameters")]
    public bool asleep = false; // Is the cat asleep?
    public float sleep_timer = Mathf.Infinity; // Timer for how long the cat has been asleep
    public float lick_per_second = 0.05f; // for calculating licking_to_do
    public int licking_to_do = 0; // Number of times the cat will lick itself after sleeping

    public override void Use(Capable capable)
    {

        // we check if the capable is a Cat
        if (!(capable is Cat))
        {
            if (debug) { Debug.LogWarning("(SleepCapacity) " + capable.name + " is not a Cat"); }
            return;
        }

        // if the cat is already asleep, we wake it up
        if (asleep)
        {
            WakeUp();
        }
        else
        {
            // if the cat is not asleep, we make it sleep
            Sleep();
        }

    }

    // SLEEPING
    public void Sleep()
    {
        // we play the sleep animation
        Anim anim = cat.anim_player.Play("fell_asleep");
        if (anim == null)
        {
            if (debug) { Debug.LogWarning("(SleepCapacity) " + cat.name + " could not play fell_asleep animation"); }
            return;
        }

        if (debug) { Debug.Log("(SleepCapacity) " + cat.name + " is falling asleep"); }

        // and we put the idle_sleep animation in the queue
        cat.anim_player.Play("idle_sleep");

        // and we set the cat as asleep
        asleep = true;
        sleep_timer = Time.time;

        // we stop talking during sleep
        if (capable.GetCapacity<TalkCapacity>() != null)
        {
            capable.GetCapacity<TalkCapacity>().StopTalking();
            capable.GetCapacity<TalkCapacity>().Say("z/.z/.z/."); // we say we are sleeping
        }

        // // wait until the falling asleep animation is finished
        // while (cat.anim_player.current_capacity == "fell_asleep") { yield return null; }
    }

    // WAKING UP
    public void WakeUp()
    {
        // we play the wake up animation
        Anim anim = cat.anim_player.Play("wake_up");
        if (anim == null)
        {
            if (debug) { Debug.LogWarning("(SleepCapacity) " + cat.name + " could not play wake_up animation"); }
            return;
        }

        // we stop playing the idle_sleep animation
        cat.anim_player.StopPlaying("idle_sleep");

        // we calculate the number of licks to do
        float time_spent_asleep = Time.time - sleep_timer;
        if (time_spent_asleep <= 0f) { return; } // if no time has passed, we do nothing
        licking_to_do = Mathf.RoundToInt(time_spent_asleep * lick_per_second) + 1;

        // we rate the cat's sleeping spot
        cat.RateNap(time_spent_asleep);
        cat.timer_before_next_sleeping = cat.seconds_per_lick_stay_up * licking_to_do;
        // calculate the next time the cat will sleep based on the time spent asleep and the number of licks to do (+ 1 licking always so there is a fixed time)

        // we set the cat as not asleep
        asleep = false;
        sleep_timer = Mathf.Infinity;


        // we start talking again
        if (capable.GetCapacity<TalkCapacity>() != null)
        {
            capable.GetCapacity<TalkCapacity>().StartTalking();
        }

        if (debug) { Debug.Log("(SleepCapacity) " + cat.name + " woke up after a " + time_spent_asleep + " seconds nap : means " + licking_to_do + " licks to do !"); }

        // we lick our feet
        StartCoroutine(LickFeet());

    }
    public IEnumerator LickFeet()
    {
        // if we are still playing wake_up animation, we wait for it to finish
        while (cat.anim_player.current_capacity == "wake_up") { yield return null; }

        // we lick our foot for the number of licks to do
        while (licking_to_do > 0)
        {
            if (debug) { Debug.Log("(SleepCapacity) " + cat.name + " is licking its foot, licks left (including this one): " + licking_to_do); }

            // we play the lick foot animation
            Anim anim = cat.anim_player.Play("lick_foot");
            if (anim == null)
            {
                if (debug) { Debug.LogWarning("(SleepCapacity) " + cat.name + " could not play lick foot animation"); }
                yield break;
            }
            while (cat.anim_player.current_capacity == "lick_foot") { yield return null; }
            // yield return new WaitForSeconds(anim.GetDuration());

            // we decrease the number of licks to do
            licking_to_do--;
        }
        
    }


    // COLLISION ENTER
    private void OnTriggerStay2D(Collider2D other)
    {
        // check if we are a Cat
        if (!(capable is Cat)) { return; }

        // we check if the other is on the Beings layer
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Beings"))) { return; }
        Capable other_capable = other.transform.parent.GetComponent<Capable>();
        if (other_capable == null) { return; }
        if (other_capable == capable) { return; } // we don't disturb ourselves

        // if we are asleep, we wake up
        if (asleep)
        {
            if (debug) { Debug.Log("(SleepCapacity) " + other_capable.name + " disturbed the nap of " + capable.name); }
            WakeUp();
        }
    }
}