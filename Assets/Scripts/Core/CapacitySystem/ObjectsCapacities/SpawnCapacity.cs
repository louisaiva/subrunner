
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
    public string base_entity_id; // the id of the entity to spawn
    public Transform entity_parent; // the transform that will be the parent of the spawned entity
    private int entity_count = 0;

    [Header("Spawn Force")]
    public float spawn_force = 0f; // (optional) force applied to the spawned entity

    [Header("Spawn position")]
    public Vector2 local_spawn_position; // or the center of the spawn circle if spawn_radius > 0
    public float spawn_radius = 0.5f; // the spawn is randowmized in a circle of this radius

    [Header("Continuous spawn")]
    public float spawn_rate = 0f; // one entity is spawned each x seconds - needs to be > 0 to spawn continuously
    private float last_use_time = 0f;

    [Header("Spawn Animation")]
    [SerializeField] private bool spawn_after_animation = false; // if true, the entity will be spawned at the end of the animation, otherwise it will be spawned at the start of the animation
    [SerializeField] private string spawn_anim_name = "spawn"; // the name of the spawn animation in the AnimPlayer
    private AnimLayer entity_layer; // the animation layer of the entity spawning animation

    // START
    private void Start()
    {
        // cache the entity layer
        entity_layer = GetComponent<AnimLayer>();
    }

    // USE
    public override void Use(Capable capable)
    {
        // we spawn & load the entity
        GameObject entity = null;
        if (CapableSystem.Instance != null)
        {
            Capable entity_capable = CapableSystem.Instance.SpawnCapable(base_entity_id, this.capable);
            if (entity_capable != null) { entity = entity_capable.gameObject; }
            else if (log) { Debug.LogWarning("(SpawnCapacity) Could not spawn entity with id " + base_entity_id); }
        }
        else { entity = Instantiate(entity_prefab); }
        if (entity == null) { return; }

        // we apply spawn parameters to the entity (spawn force, etc)
        Spawn(entity);
    }
    public async void Spawn(GameObject entity)
    {
        entity.SetActive(false);

        // we find the entity_layer new skin name based on the capable skin + "_" + entity skin
        if (entity_layer != null) { set_entity_layer_skin(entity); }

        // we make the main capable play an animation
        capable.AnimPlayer.Play(spawn_anim_name);

        // if we spawn after the animation we wait for it to finish
        if (spawn_after_animation)
        {
            while (capable.AnimPlayer.IsPlaying(spawn_anim_name)) { await System.Threading.Tasks.Task.Yield(); }
        }

        entity.SetActive(true);

        // we get a random spawn position
        Vector2 spawn_position = transform.parent.position + ((Vector3)local_spawn_position);
        if (spawn_radius > 0)
        {
            spawn_position += Random.insideUnitCircle * spawn_radius;
        }

        // we apply the position & parent to entity
        entity.transform.position = spawn_position;
        if (entity_parent != null)
        {
            entity.transform.parent = entity_parent;
        }

        // we rename the entity
        entity.name = entity_prefab.name + "_" + entity_count;

        // we create a spawn force
        string force_debug = "";
        if (spawn_force > 0f && entity.GetComponent<Movable>() != null)
        {
            Vector2 force_direction = (spawn_position - (Vector2)capable.transform.position).normalized;
            Force spawn_force = new Force("spawn", force_direction, this.spawn_force);
            entity.GetComponent<Movable>().AddForce(spawn_force);
            force_debug = " with force " + spawn_force;
        }

        if (log) { Debug.Log("(SpawnCapacity) " + name + " spawning entity at " + spawn_position + force_debug); }
        entity_count++;
    }

    // low level spawning
    private void set_entity_layer_skin(GameObject entity)
    {
        // we get the entity anim player
        Capable entity_capable = entity.GetComponent<Capable>();
        if (entity_capable == null) { return; }

        // we get the skin name based on the capable skin + "_" + entity skin
        string skin_name = capable.Skin + "_" + entity_capable.Skin;

        // we set the skin of the entity layer to the new skin
        entity_layer.skin = skin_name;
    }


    // UPDATE
    protected override void Update()
    {
        if (spawn_rate == 0f) { return; }

        // if the spawn rate is > 0, we spawn continuously
        if (Time.time - last_use_time > spawn_rate)
        {
            Use(transform.parent.GetComponent<Capable>());
            last_use_time = Time.time;
        }
    }


    // GIZMOS
    private void OnDrawGizmosSelected()
    {
        // draw a circle to show the spawn zone
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + (Vector3) local_spawn_position, spawn_radius);
    }
}