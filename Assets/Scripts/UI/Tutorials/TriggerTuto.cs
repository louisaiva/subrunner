using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerTuto : MonoBehaviour
{

    [Header("UI Tuto Trigger")]
    [SerializeField] private bool triggered = false;
    [SerializeField] private float wait_time_before_showing = 0f;
    [SerializeField] private float hide_and_disable_after = float.MaxValue;


    [Header("UI Tuto Prefab")]
    [SerializeField] private GameObject ui_tuto_prefab;
    [SerializeField] private Transform ui_tuto_parent;
    [SerializeField] private GameObject ui_tuto;


    [Header("Debug")]
    [SerializeField] private bool log = false;

    // START
    private void Start()
    {
        // set the ui tuto parent
        if (ui_tuto_parent == null)
        {
            ui_tuto_parent = GameObject.Find("/ui/hud/tutorials").transform;
        }

        /* // Get the trigger_collider
        trigger_collider = GetComponent<Collider2D>();

        // Set the contact filter
        player_contact_filter = new ContactFilter2D();
        player_contact_filter.useLayerMask = true;
        player_contact_filter.layerMask = player_layer;

        // Check if the player is already inside the trigger
        if (isPlayerInsideTuto())
        {
            // Create the tutorial UI
            ui_tuto = Instantiate(ui_tuto_prefab, ui_tuto_parent);
            triggered = true;
        } */
    }

    // TRIGGERS
    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }
        if (triggered) { return; }

        if (log) { Debug.Log("(TriggerTuto) " + other.transform.parent.name + " <- enter -> " + name); }

        triggered = true;

        StartCoroutine(show_ui_tuto());
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (AppManager.Instance.IsQuitting) { return; }

        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }
        if (!triggered) { return; }

        if (log) { Debug.Log("(TriggerTuto) " + other.transform.parent.name + " <- exit -> " + name); }

        triggered = false;

        StopAllCoroutines();

        if (ui_tuto == null) { return; }
        if (ui_tuto.GetComponent<Transitioner>() == null) { return; }

        StartCoroutine(hide_ui_tuto());
    }


    // SHOW / HIDE
    private IEnumerator show_ui_tuto()
    {
        if (log) { Debug.Log("(TriggerTuto) " + name + " got triggered, preparing for showing ui tuto"); }

        if (ui_tuto != null)
        {
            yield return new WaitUntil(() => ui_tuto == null);
        }

        // Wait for the specified time before showing the UI
        if (wait_time_before_showing > 0f)
        {
            yield return new WaitForSeconds(wait_time_before_showing);

            // checks if we are still triggered if not return
            if (!triggered) { yield break; }
        }

        // Create the tutorial UI
        ui_tuto = Instantiate(ui_tuto_prefab, ui_tuto_parent);

        // we wait for the showing transition to happen
        yield return new WaitUntil(() => !ui_tuto.GetComponent<Transitioner>().Transitioning);
        Awaitable task = ui_tuto.GetComponent<Transitioner>().Show();
        while (!task.IsCompleted) { yield return null; }
        if (log) { Debug.Log("(TriggerTuto) " + name + " ui tuto instantiated and shown !"); }


        // we wait for the hiding time to finish
        yield return new WaitForSeconds(hide_and_disable_after);

        // we disable ourself
        yield return hide_ui_tuto();
        Destroy(gameObject);
        if (log) { Debug.Log("(TriggerTuto) " + name + " IS DONE !"); }
        yield return null;

    }
    private IEnumerator hide_ui_tuto()
    {
        if (ui_tuto == null) { yield break; }
        if (ui_tuto.GetComponent<Transitioner>() == null) { yield break; }

        yield return new WaitUntil(() => !ui_tuto.GetComponent<Transitioner>().Transitioning);

        // we wait for the hiding transition to happen
        Awaitable task = ui_tuto.GetComponent<Transitioner>().Hide();
        while (!task.IsCompleted) { yield return null; }

        if (log) { Debug.Log("(TriggerTuto) " + name + " hid the UI Tuto (and is going to destroy it)"); }

        yield return null;
        // we destroy the tutorial UI
        Destroy(ui_tuto);
        yield return null; // wait a frame to make sure destroy has happened

    }
}