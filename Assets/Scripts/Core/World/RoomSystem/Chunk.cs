using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{

    [Header("Chunk data")]
    public ChunkData data;
    public bool Loaded { get { return data != null; } }
    private bool _unloading = false;
    public string ID { get { return GetStaticID(); } }

    [Header("Components")]
    private PolygonCollider2D _chunk_collider;
    public PolygonCollider2D ChunkCollider
    {
        get
        {
            if (_chunk_collider == null) { _chunk_collider = GetComponent<PolygonCollider2D>(); }
            return _chunk_collider;
        }
    }

    private Transform _lights_parent;
    public Transform LightsParent
    {
        get
        {
            if (_lights_parent == null)
            {
                _lights_parent = transform.Find("Lights");
                if (_lights_parent == null)
                {
                    GameObject lights_go = new GameObject("Lights");
                    lights_go.transform.SetParent(transform);
                    _lights_parent = lights_go.transform;
                }
            }
            return _lights_parent;
        }
    }

    // LOAD / UNLOAD
    public void LoadData(ChunkData data)
    {
        this.data = data;
        this.name = data.id;
        this.transform.position = data.position;

        // load the colliders in the composite collider
        ChunkCollider.SetPath(0, data.collider_points.ToArray());
        ChunkCollider.enabled = true;

        // load the lights
        ChunkEngine.Instance.LightsEngine.LoadLights(data.lights_data, data.id);

        // show the tilemaps
        // RoomEngine.Instance.DoorEngine.ShowRoom(data.room_id);

        // here we need to load all the capables that we hold in data.capables_ids
        if ((data.capables_ids != null && data.capables_ids.Count > 0) || (data.movables_ids != null && data.movables_ids.Count > 0))
        {
            if (CapableEngine.Instance != null)
            {
                CapableEngine.Instance.LoadCapables(data.capables_ids);
                CapableEngine.Instance.LoadCapables(data.movables_ids);
            }
        }

        // fire the event
        data.OnChunkLoaded?.Invoke(data);
    }
    public void UnloadData()
    {
        _unloading = true;
        
        // unload the collider
        ChunkCollider.enabled = false;

        // here we need to unload all the capables that we hold
        // -> interacts with CapableSystem
        if (CapableEngine.Instance != null)
        {
            CapableEngine.Instance.UnloadCapables(data.capables_ids);
            CapableEngine.Instance.UnloadCapables(data.movables_ids);
        }

        // fire the event
        data.OnChunkUnloaded?.Invoke(data);
        this.data = null;
        _unloading = false;
    }


    /// <summary>
    /// just as other GetStaticData() methods (ie Capable's one), this method
    /// is not meant to be run in a BUILD !!! IT WON T WORK because it does not
    /// update this.data . it creates a new data based from actual static
    /// variables states of the object. if run inside a build, it could overwrite
    /// some data such as tilebases_used paths which would break the save.
    /// </summary>
    /// <returns></returns>
    public ChunkData GetStaticData()
    {
        ChunkData new_data = new ChunkData
        {
            // set base data things
            id = GetStaticID(),
            room_id = get_static_room_id(),
            position = this.transform.position,

            // set collider data
            collider_points = get_static_collider_points(),

            // set neighbours data
            neighbours_ids = data.neighbours_ids ?? new List<string>(),

            // set capables data
            capables_ids = data.capables_ids ?? new List<string>(),
            movables_ids = data.movables_ids ?? new List<string>(),

            // set lights data
            lights_data = get_static_light_data(),
        };

        return new_data;
    }
    public string get_static_room_id()
    {
        if (data != null && !string.IsNullOrEmpty(data.room_id)) { return data.room_id; }
        // else we can quickly check if we have a room above us in the hierarchy (in AIO loaded mode this happens)
        Room room = GetComponentInParent<Room>(includeInactive: true);
        if (room != null) { return room.ID; }
        return null;
    }
    public string GetStaticID()
    {
        if (data == null) { return name; }
        if (string.IsNullOrEmpty(data.id)) { return name; }
        return data.id;
    }
    protected List<LightData> get_static_light_data()
    {
        List<LightData> lights_data = new List<LightData>();
        if (LightsParent == null) { return lights_data; }

        // we go through our LightsTransform
        foreach (Transform light_transform in LightsParent)
        {
            // we get the Light2D component on it
            Light2D light = light_transform.GetComponent<Light2D>();
            if (light == null) { continue; }

            // we create a new LightData with the data of the visu and we add it to the list
            LightData light_data = new LightData()
            {
                position = light.transform.position,
                color = light.color,
                intensity = light.intensity,
                radius = new Vector2(light.pointLightInnerRadius, light.pointLightOuterRadius),
                falloff = light.falloffIntensity,
            };
            lights_data.Add(light_data);
        }
        return lights_data;
    }
    protected List<Vector2> get_static_collider_points()
    {
        List<Vector2> points = new List<Vector2>();
        if (ChunkCollider == null) { return points; }
        if (ChunkCollider.pathCount == 0) { return points; }
        points = new List<Vector2>(ChunkCollider.GetPath(0));
        
        // we go through all the points and apply the collider' offset to get them real pos
        for (int i = 0; i < points.Count; i++)
        {
            points[i] += ChunkCollider.offset;
        }

        return points;
    }
    public void AddStaticNeighbor(Chunk neighbor)
    {
        // if (data == null) { return; }
        if (data.neighbours_ids == null) { data.neighbours_ids = new List<string>(); }
        if (data.neighbours_ids.Contains(neighbor.GetStaticID())) { return; }
        data.neighbours_ids.Add(neighbor.GetStaticID());
    }
    public void RemoveStaticNeighbor(Chunk neighbor)
    {
        if (data.neighbours_ids == null) { return; }
        if (!data.neighbours_ids.Contains(neighbor.GetStaticID())) { return; }
        data.neighbours_ids.Remove(neighbor.GetStaticID());
    }
    public void ClearStaticNeighbors()
    {
        if (data.neighbours_ids == null) { return; }
        data.neighbours_ids.Clear();
    }


    // COLLIDERS EVENTS
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        Capable capable = collider.GetComponent<Capable>(); // some old objects have feet collider directly on them
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); } // some old movables have feet collider on feet -> child of the capable
        if (capable == null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); } // new obj have feet collider as child of feet -> grand child of the capable
        if (capable == null) { return; }
        if (capable.data == null) { return; } // if we don't have data, we can't do anything with it, so we ignore the trigger

        // check if capable is not the controlled one and not in the capable system
        // we just ignore the trigger
        if (Controller.Capable != capable && !CapableBank.Instance.HasCapable(capable)) { return; }
        string id = capable.data.id;

        if (ChunkEngine.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) {id} - IN -"); }
        
        // check if we should ignore the trigger bcz the capable just attached to something in the room
        if (ChunkEngine.Instance.ShouldIgnoreRoomTrigger(id)) { return; }

        // directly call RoomEngine.OnRoomEnter
        ChunkEngine.Instance.OnChunkEnter(this.data, capable);

    }
    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (data == null) { return; }
        if (AppManager.Instance.IsQuitting) { return; }
        // if (GameManager.IsClosingGame) { return; }
        if (ChunkEngine.Instance == null) { return; }
        if (_unloading) { return; } // if we are unloading the room we don't want any trigger event

        Capable capable = collider.GetComponent<Capable>();
        if (capable == null) { capable = collider.transform.parent.GetComponent<Capable>(); }
        if (capable == null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); }
        if (capable == null) { return; }

        if (Controller.Capable != capable && !CapableBank.Instance.HasCapable(capable)) { return; }
        string id = capable.data.id;
        if (ChunkEngine.Instance.log_colliders) { Debug.Log($"(Room - {this.name}) {id} - OUT -"); }

        // ! is data.id null ? if we unload the capable, could be

        // check if we should ignore the trigger bcz the capable just attached/detached to something in the room
        if (ChunkEngine.Instance.ShouldIgnoreRoomTrigger(id)) { return; }

        // if this is a grabbed item then we do nothing (was freed when grabbed)
        // if (capable is Item item && item.Grabbed) { return; }

        // directly call RoomEngine.OnRoomExit
        ChunkEngine.Instance.OnChunkExit(this.data, capable);

    }


    // COLLIDER OVERLAP
    private ContactFilter2D? _contact_filter = null;
    private ContactFilter2D contact_filter
    {
        get
        {
            if (_contact_filter != null) { return _contact_filter.Value; }
            ContactFilter2D filter = new ContactFilter2D();
            filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Objects", "Feet"));
            filter.useTriggers = true;
            _contact_filter = filter;
            return filter;
        }
    }
    public void GetOverlappingCapablesIDs(out List<string> overlapping_capables, out List<string> overlapping_movables)
    {
        overlapping_capables = new List<string>();
        overlapping_movables = new List<string>();

        // we get all the colliders that are currently overlapping with the room collider
        Collider2D[] colliders = new Collider2D[30];
        int count = Physics2D.OverlapCollider(ChunkCollider, contact_filter, colliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = colliders[i];
            Capable capable = collider.GetComponent<Capable>();
            if (capable == null) { capable = collider.GetComponentInParent<Capable>(); }
            if (capable == null) { continue; }

            // we found a capable !
            if (capable is Movable) { overlapping_movables.Add(capable.GetStaticID()); }
            else { overlapping_capables.Add(capable.GetStaticID()); }
        }
    }
    public List<Capable> GetStaticOverlappingCapables<T>(Loggable<T> grab_log = null) where T : MonoBehaviour
    {
        List<Capable> overlapping_capables = new List<Capable>();

        if (grab_log != null && grab_log.Verbose >= Verbosity.Extended)
        {
            grab_log.LogExtended($"(Chunk - {this.ID}) - checking overlapping capables, chunk bounds : {ChunkCollider.bounds}, chunk position : {transform.position}, path : {string.Join(", ", ChunkCollider.GetPath(0))}\n");
        }

        // we get all the colliders that are currently overlapping with the room collider
        Collider2D[] colliders = new Collider2D[100];
        int count = Physics2D.OverlapCollider(ChunkCollider, contact_filter, colliders);
        for (int i = 0; i < count; i++)
        {
            Collider2D collider = colliders[i];
            Capable capable = collider.GetComponent<Capable>();

            if (grab_log != null && grab_log.Verbose >= Verbosity.Extended)
            {
                string capable_id = capable != null ? capable.GetStaticID() : "null";
                grab_log.LogExtended($"(Chunk - {this.ID}) Checking overlapping collider : {collider.name} (capable: {capable_id}), collider is at {collider.transform.position} and it is a {collider.GetType().Name}\n");
            }

            // if (RoomEngine.LazyInstance.log_grab) 
            if (capable == null && collider.transform.parent != null) { capable = collider.transform.parent.GetComponent<Capable>(); }
            if (capable == null && collider.transform.parent != null && collider.transform.parent.parent != null) { capable = collider.transform.parent.parent.GetComponent<Capable>(); }
            if (capable == null) { continue; }
            
            // we found a capable !
            overlapping_capables.Add(capable);

            grab_log?.Log($"(Chunk - {this.ID}) Found overlapping capable : {capable.ID} (at {capable.transform.position})\n");
        }
        return overlapping_capables;
    }
    public Bounds GetStaticBounds() { return ChunkCollider.bounds; }

    public bool OverlapPoint(Vector2 point)
    {
        return ChunkCollider.OverlapPoint(point);
    }


}