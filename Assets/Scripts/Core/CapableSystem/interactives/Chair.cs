using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chair : Capable, EndlessInteractable
{
    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.Other;

    // coroutine
    Coroutine small_turn_coroutine = null;

    // ON INTERACT
    public void OnInteract(Capable interactor)
    {
        if (log) { Debug.Log(name + " has been interacted by " + interactor.name); }
        small_turn_coroutine = StartCoroutine(small_turn(Random.Range(1, 4)));
    }
    public void OnEndlessInteract(Capable interactor)
    {
        if (small_turn_coroutine != null) { StopCoroutine(small_turn_coroutine); }
        OnInteract(interactor);
    }

    // COROUTINES
    private IEnumerator small_turn(int turns)
    {
        Anim turn_anim = AnimPlayer.Play("turn");
        if (turn_anim == null) { yield break; }
        AnimPlayer.Play("start_turn");

        // get the duration of the anim
        float anim_duration = turn_anim.GetDuration();
        float duration = turns * anim_duration;

        // we choose a random orientation for the turn to finish
        string[] orientations = new string[] { "U", "D", "L", "R" };
        string chosen_orientation = orientations[Random.Range(0, orientations.Length)];
        string capacity = "stop_at_" + chosen_orientation;
        AnimPlayer.AddToPile(capacity);

        // we wait for a small amount of time & then stop playing turn
        yield return new WaitForSeconds(duration);
        AnimPlayer.StopPlaying("turn");
        while (AnimPlayer.current_capacity != capacity) { yield return null; }

        // we set the orientation
        Orient(chosen_orientation);
    }
}