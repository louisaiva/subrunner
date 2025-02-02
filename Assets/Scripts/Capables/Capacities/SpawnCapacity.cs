
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// SpawnCapacity is a capacity that creates a new entity at a given position.
/// </summary>

public class SpawnCapacity : Capacity
{
    [Header("Spawn parameters")]
    public GameObject entity;
    public Transform entity_parent;

    [Header("Spawn position")]
    public Vector2 local_spawn_position; // or the center of the spawn circle if spawn_radius > 0
    public float spawn_radius = 0.5f; // the spawn is randowmized in a circle of this radius
    
    [Header("Continuous spawn")]
    public float spawn_rate = 0f; // in seconds - needs to be > 0 to spawn continuously
    private float last_use_time = 0f;

    [Header("Debug")]
    public bool debug = false;

    // USE
    public override void Use(Capable capable)
    {
        base.Use(capable);

        // we get the spawn position
        Vector2 spawn_position = transform.parent.position + ((Vector3) local_spawn_position);
        if (spawn_radius > 0)
        {
            spawn_position += Random.insideUnitCircle * spawn_radius;
        }
        if (debug) {Debug.Log("(SpawnCapacity) " + name + " spawning entity at " + spawn_position);}

        if (entity == null) { return; }

        // we spawn the entity
        Instantiate(entity, spawn_position, Quaternion.identity);
        if (entity_parent != null)
        {
            entity.transform.parent = entity_parent;
        }

    }

    // UPDATE
    new void Update()
    {
        if (spawn_rate == 0f) { return; }

        // if the spawn rate is > 0, we spawn continuously
        if (Time.time - last_use_time > spawn_rate)
        {
            Use(transform.parent.GetComponent<Capable>());
            last_use_time = Time.time;
        }
    }
}