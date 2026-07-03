
using System;
using UnityEngine;

/// <summary>
/// SpawnCapacity is a capacity that creates a new entity at a given position.
/// </summary>

public class SpawnCapacity : Capacity
{
    [Header("Spawn parameters")]
    // public GameObject entity_prefab;
    public string base_entity_id; // the id of the entity to spawn
    // public Transform entity_parent; // the transform that will be the parent of the spawned entity
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
    private AnimLayer _entity_layer; // the animation layer of the entity spawning animation
    private AnimLayer entity_layer
    {
        get
        {
            if (_entity_layer != null) { return _entity_layer; }
            _entity_layer = GetComponent<AnimLayer>();
            return _entity_layer;
        }
    }
    private bool entity_layer_connected = false;

    // USE
    public override void Use(Capable capable)
    {
        // we apply spawn parameters to the entity (spawn force, etc)
        Spawn(base_entity_id);
    }
    public async void Spawn(string id_or_template)
    {
        if (string.IsNullOrEmpty(id_or_template))
        {
            if (log) { Debug.LogError("(SpawnCapacity - Spawn) id_or_template is null or empty, canot spawn entity."); }
            return;
        }
        
        // we find the entity_layer new skin name based on the capable skin + "_" + entity skin
        if (entity_layer_connected)
        {
            set_entity_layer_skin(id_or_template.GetPrefix()); // get prefix will return the template or the template (in both cases the template)
            entity_layer.EnableRenderer();
        }

        // we make the main capable play an animation
        is_spawning = true;
        Capable.AnimPlayer.Play(spawn_anim_name);
        if (spawn_after_animation && entity_layer_connected)
        {
            while (Capable.Loaded && Capable.AnimPlayer.IsPlaying(spawn_anim_name)) { await System.Threading.Tasks.Task.Yield(); }
            entity_layer.DisableRenderer();
        }


        // we spawn & load the entity
        // ! IMPORTANT ! We call LoadCapableInstantly instead of SpawnCapable bcz this way
        // ! we can have a real world id as parameter, which means we can spawn entities that
        // ! were in the spawner (you need to figure the spawner as a burrow)
        Capable entity = CapableEngine.Instance.LoadCapableInstantly(id_or_template);
        if (entity == null)
        {
            if (log) { Debug.LogWarning("(SpawnCapacity) Could not spawn entity with id or template " + id_or_template); }
            return;
        }


        // we get a random spawn position
        Vector2 spawn_position = transform.parent.position + ((Vector3)local_spawn_position);
        if (spawn_radius > 0)
        {
            spawn_position += UnityEngine.Random.insideUnitCircle * spawn_radius;
        }
        entity.transform.position = spawn_position;

        // we create a spawn force
        string force_debug = "";
        if (spawn_force > 0f && entity.GetComponent<Movable>() != null)
        {
            Vector2 force_direction = (spawn_position - (Vector2)Capable.transform.position).normalized;
            Force spawn_force = new Force("spawn", force_direction, this.spawn_force);
            entity.GetComponent<Movable>().AddForce(spawn_force);
            force_debug = " with force " + spawn_force;
        }

        if (log) { Debug.Log($"(SpawnCapacity) {data.owner_id} spawning entity {entity.ID} at " + spawn_position + force_debug); }
        entity_count++;

        await System.Threading.Tasks.Task.Yield(); // wait a little frame to be sure
        is_spawning = false;
    }

    // low level spawning
    private void set_entity_layer_skin(string entity_skin)
    {
        // we get the skin name based on the capable skin + "_" + entity skin
        string skin_name = Capable.Skin + "_" + entity_skin;

        // we set the skin of the entity layer to the new skin
        entity_layer.skin = skin_name;
    }
    private bool is_spawning = false;
    public bool IsSpawning { get { return is_spawning; } }

    // UPDATE
    protected void Update()
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



    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData capable_data)
    {
        base.LoadData(data, capable_data);

        if (data is not SpawnCapacityData spawn_data) { return; }

        entity_layer_connected = false;
        if (entity_layer != null)
        {
            entity_layer.DisableRenderer();
            Capable leader = capable_data.Capable;

            if (leader == null || leader.AnimPlayer == null)
            {
                Debug.LogError($"(SpawnCapacity) could not connect entity_layer to animplayer leader cause {(leader == null ? "leader" : "leader's AnimPlayer")} is null");
            }
            else
            {
                entity_layer.AssignLeader(leader.AnimPlayer);
                entity_layer_connected = true;
                // Debug.Log($"(SpawnCapacity) successfully connected entity_layer of '{ID}' to the leader animplayer of '{leader.ID}'");
            }
        }

        // we set all the spawn parameters
        this.base_entity_id = spawn_data.base_entity_id;
        this.spawn_force = spawn_data.spawn_force;
        this.local_spawn_position = spawn_data.local_spawn_position;
        this.spawn_radius = spawn_data.spawn_radius;
        this.spawn_rate = spawn_data.spawn_rate;
        this.spawn_anim_name = spawn_data.spawn_anim_name;
        this.spawn_after_animation = spawn_data.spawn_after_animation;

    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        SpawnCapacityData static_data = new SpawnCapacityData(base.GetStaticData())
        {
            base_entity_id = this.base_entity_id,
            spawn_force = this.spawn_force,
            local_spawn_position = this.local_spawn_position,
            spawn_radius = this.spawn_radius,
            spawn_rate = this.spawn_rate,
            spawn_anim_name = this.spawn_anim_name,
            spawn_after_animation = this.spawn_after_animation
        };

        return static_data;
    }
}


[Serializable] public class SpawnCapacityData : CapacityData
{


    // template variables
    public string spawn_anim_name = "spawn"; // the name of the spawn animation in the AnimPlayer
    public string base_entity_id; // the id of the entity to spawn
    public float spawn_force = 0f; // (optional) force applied to the spawned entity
    public Vector2 local_spawn_position; // or the center of the spawn circle if spawn_radius > 0
    public float spawn_radius = 0.5f; // the spawn is randowmized in a circle of this radius
    public float spawn_rate = 0f; // one entity is spawned each x seconds - needs to be > 0 to spawn continuously
    public bool spawn_after_animation = false;


    // CONSTRUCTOR
    public SpawnCapacityData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new SpawnCapacityData(base.Duplicate() as CapacityData)
        {
            base_entity_id = this.base_entity_id,
            spawn_force = this.spawn_force,
            local_spawn_position = this.local_spawn_position,
            spawn_radius = this.spawn_radius,
            spawn_rate = this.spawn_rate,

            spawn_anim_name = this.spawn_anim_name,
            spawn_after_animation = this.spawn_after_animation

        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";

        details += $"  - entity id to spawn: {base_entity_id} \n";
        details += $"  - spawn force: {spawn_force} \n";
        details += $"  - local spawn position: {local_spawn_position} \n";
        details += $"  - spawn radius: {spawn_radius} \n";
        details += $"  - spawn rate: {spawn_rate} \n";
        details += $"  - spawn animation name: {spawn_anim_name} \n";
        details += $"  - spawn after animation: {spawn_after_animation} \n";

        return base.GetDetails() + details;
    }
}