
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class InteractHackCapacity : Capacity
{
    [Header("Current target")]
    [SerializeField] private GameObject closest_target;

    [Header("Waiting targets")]
    [SerializeField] private List<GameObject> waiting_targets = new List<GameObject>();

    [Header("Hack selection")]
    [SerializeField] private HackCapacity hacker;

    [Header("Log")]
    [SerializeField] private bool log_targets = false;

    // START
    private void Start()
    {
        // we get the hack capacity
        hacker = capable.GetCapacity<HackCapacity>();
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // we remove null waiting_targets
        if (closest_target != null && closest_target.GetComponent<Hackable>() == null) { unselect_target(); }
        waiting_targets.RemoveAll(h => h.GetComponent<Hackable>() == null);
        if (log_targets)
        {
            string targets_info = "";
            int target_count = 0;
            if (closest_target != null)
            {
                targets_info += $"- closest target: {closest_target.name} (is null ? {closest_target == null})\n";
                target_count++;
            }
            else
            {
                targets_info += "- closest target: null\n";
            }

            foreach (var target in waiting_targets)
            {
                targets_info += $"- waiting target: {target} (is null ? {target == null})\n";
                target_count++;
            }
            Debug.Log($"(InteractHackCapacity) Current targets: {target_count}\n" + targets_info);
        }

        // we check if we have a something in the waiting targets
        if (waiting_targets.Count == 0) { return; }

        // we update the waiting targets by distance
        waiting_targets.Sort((a, b) => Vector2.Distance(a.transform.position, capable.transform.position).CompareTo(Vector2.Distance(b.transform.position, capable.transform.position)));

        // we check if we have a current target
        if (closest_target == null)
        {
            // we select the target of the first waiting target
            select_target(waiting_targets[0].GetComponent<Hackable>());
            waiting_targets.RemoveAt(0);

            return;
        }

        // we check if the current target is still the closest
        if (Vector2.Distance(closest_target.transform.position, capable.transform.position)
            < Vector2.Distance(waiting_targets[0].transform.position, capable.transform.position)) { return; }

        // we switch the current target
        waiting_targets.Add(closest_target);
        unselect_target();
        select_target(waiting_targets[0].GetComponent<Hackable>());
        waiting_targets.RemoveAt(0);
    }

    // INTERACTABLE SELECTION
    private void select_target(Hackable hackable)
    {
        // we switch the current target
        if (closest_target != null) { unselect_target(); }
        closest_target = hackable.gameObject;
        if (debug) { Debug.Log("(InteractHackCapacity) " + hackable.name + " selected as closest target"); }

        // we play the target animation
        // closest_target.GetComponent<Capable>().GetCapacity<HoverCapacity>()?.Hover(this.capable);

        hacker?.Select(hackable);
    }
    private void unselect_target()
    {
        // we check if we have a current target
        if (closest_target == null) { return; }

        // we stop the target animation
        // closest_target.gameObject.GetComponent<Capable>().GetCapacity<HoverCapacity>()?.Unhover(this.capable);

        hacker?.Deselect();

        // we reset the current target
        if (debug) { Debug.Log("(InteractHackCapacity) " + closest_target.name + " unselected as closest target"); }
        closest_target = null;
    }

    // TRIGGER ENTER
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if other is a capable
        Capable target = other.transform.parent.GetComponent<Capable>();
        if (target == null) { return; }

        // we get the hackable of the target capacity
        if (target is not Hackable hack_target) { return; }

        // we check if the hackable is already targeted
        if (target.gameObject == closest_target) { return; }

        // or if it's already in the waiting targets
        if (waiting_targets.Contains(target.gameObject)) { return; }

        // we add the hackable to the waiting targets
        waiting_targets.Add(target.gameObject);

        if (debug) { Debug.Log("(InteractHackCapacity) " + target.name + " added to waiting targets"); }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if other is a capable
        Capable target = other.transform.parent.GetComponent<Capable>();
        if (target == null) { return; }

        // we get the hackable of the target capacity
        if (target is not Hackable hack_target) { return; }

        // we check if the hackable is the current target
        if (target.gameObject == closest_target)
        {
            unselect_target();
            return;
        }

        // we check if the hack_target is in the waiting targets
        if (waiting_targets.Contains(target.gameObject))
        {
            waiting_targets.Remove(target.gameObject);
            if (debug) { Debug.Log("(InteractHackCapacity) " + hack_target.name + " removed from waiting targets"); }
        }
    }
}