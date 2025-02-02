using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TriggerTuto : MonoBehaviour
{

    // The UI prefab to instantiate
    public GameObject ui_tuto_prefab;
    public Transform ui_tuto_parent;

    // The UI instance
    public GameObject ui_tuto;
    public bool triggered = false;

    // Collisions
    // private Collider2D trigger_collider;
    // public LayerMask player_layer;
    // private ContactFilter2D player_contact_filter;

    /* private void Start()
    {
        // Get the trigger_collider
        // trigger_collider = GetComponent<Collider2D>();

        // Set the contact filter
        player_contact_filter = new ContactFilter2D();
        player_contact_filter.useLayerMask = true;
        player_contact_filter.layerMask = player_layer;
    } */

    /* private void Update()
    {
        // 1 - we check if the trigger_collider collides with the player
        
        // we get all the player collisions
        List<Collider2D> collisions = new();
        Physics2D.OverlapCollider(trigger_collider, player_contact_filter,collisions);

        // we verify if we actually collided with the player
        bool player_is_colliding = false;
        foreach (Collider2D collision in collisions)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                player_is_colliding = true;
                break;
            }
        }

        // 2 - we check if we need to trigger the tutorial
        if (!player_is_colliding && triggered)
        {
            // we just got out of the trigger
            OnTriggerExit2D();
        }
        else if (player_is_colliding && !triggered)
        {
            // we just got in the trigger
            OnTriggerEnter2D();
        }
    } */

    private void OnTriggerEnter2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }
        if (triggered) { return; }

        Debug.Log("OMG "+ name +" is Triggered by " + other.name);

        // Create the tutorial UI
        ui_tuto = Instantiate(ui_tuto_prefab, ui_tuto_parent);
        triggered = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // we check if the other is the player
        if (!other.gameObject.CompareTag("Player")) { return; }
        if (!triggered) { return; }

        Debug.Log("OMG " + name + " is ended Triggered by " + other.name);

        // Destroy the tutorial UI
        Destroy(ui_tuto);
        triggered = false;
    }

}