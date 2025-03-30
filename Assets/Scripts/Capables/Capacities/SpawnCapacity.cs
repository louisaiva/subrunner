
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

/// <summary>
/// SpawnCapacity is a capacity that creates a new entity at a given position.
/// </summary>

public class SpawnCapacity : Capacity
{
    [Header("Spawn parameters")]
    public GameObject entity_prefab;
    public Transform entity_parent; // the transform that will be the parent of the spawned entity

    [Header("Spawn Force")]
    public float spawn_force = 0f; // (optional) force applied to the spawned entity

    [Header("Spawn position")]
    public Vector2 local_spawn_position; // or the center of the spawn circle if spawn_radius > 0
    public float spawn_radius = 0.5f; // the spawn is randowmized in a circle of this radius
    
    [Header("Continuous spawn")]
    public float spawn_rate = 0f; // in seconds - needs to be > 0 to spawn continuously
    private float last_use_time = 0f;

    // USE
    public override void Use(Capable capable)
    {
        base.Use(capable);
        if (entity_prefab == null) { return; }

        // we get the spawn position
        Vector2 spawn_position = transform.parent.position + ((Vector3) local_spawn_position);
        if (spawn_radius > 0)
        {
            spawn_position += Random.insideUnitCircle * spawn_radius;
        }

        // we spawn the entity
        GameObject entity = Instantiate(entity_prefab, spawn_position, Quaternion.identity);
        if (entity_parent != null)
        {
            entity.transform.parent = entity_parent;
        }

        // we create a spawn force
        string force_debug = "";
        if (spawn_force > 0f && entity.GetComponent<Movable>() != null)
        {
            Force spawn_force = new Force("spawn", capable.Orientation, this.spawn_force);
            entity.GetComponent<Movable>().AddForce(spawn_force);
            force_debug = " with force " + spawn_force;
        }

        if (debug) { Debug.Log("(SpawnCapacity) " + name + " spawning entity at " + spawn_position + force_debug); }

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