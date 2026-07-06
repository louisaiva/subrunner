using System.Collections.Generic;
using System.Reflection;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ColliderBank : MonoBehaviour
{

    // AWAKE & SINGLETON LOGIC
    public static ColliderBank Instance { get; private set; }
    public void Awake()
    {
        // singleton logic
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        // initialize the pools
        pooled_circle_colliders = new Stack<GameObject>();
        pooled_box_colliders = new Stack<GameObject>();
        pooled_shadowed_circle_colliders = new Dictionary<List<string>, Stack<GameObject>>();
        pooled_shadowed_box_colliders = new Dictionary<List<string>, Stack<GameObject>>();
    }


    // COLLIDERS POOLING
    [Header("Colliders prefabs")]
    [SerializeField] protected GameObject circle_collider_prefab;
    [SerializeField] protected GameObject box_collider_prefab;

    [Header("Sleeping colliders")]
    [SerializeField] protected Transform sleeping_colliders_parent;
    [SerializeField] protected string sleeping_layer_name;
    private int _sleeping_layer = -999;
    private int sleeping_layer
    {
        get
        {
            if (_sleeping_layer == -999) { _sleeping_layer = LayerMask.NameToLayer(sleeping_layer_name); }
            return _sleeping_layer;
        }
    }


    // SAVING SHADOW DATA FOR UNLOADING
    protected Dictionary<GameObject, ShadowCasterData> shadow_caster_datas = new Dictionary<GameObject, ShadowCasterData>();

    // POOLS
    protected Stack<GameObject> pooled_circle_colliders; // null shadowdata
    protected Stack<GameObject> pooled_box_colliders; // null shadowdata
    protected Dictionary<List<string>, Stack<GameObject>> pooled_shadowed_circle_colliders;
    protected Dictionary<List<string>, Stack<GameObject>> pooled_shadowed_box_colliders;



    [Header("Logs")]
    [SerializeField] protected bool log_body_data;
    [SerializeField] protected bool hide_log_load_collider_not_found;
    protected static bool log_shadows = false;
    protected static bool log_shadows_shapes = false;
    [SerializeField] protected Loggable<ColliderBank> log;
    protected static Loggable<ColliderBank> slog => Instance != null ? Instance.log : null;



    ///
    //
    ///  CLEAR CACHE
    //
    ///
    public void ClearCache(bool log)
    {
        // we clear all the pools
        pooled_circle_colliders.Clear();
        pooled_box_colliders.Clear();
        pooled_shadowed_circle_colliders.Clear();
        pooled_shadowed_box_colliders.Clear();

        // we clear the shadow caster data
        shadow_caster_datas.Clear();

        if (log) { Debug.Log($"(ColliderBank) Cache cleared"); }
    }



    ///
    //
    ///  LOAD / UNLOAD COLLIDERS
    //
    ///


    // LOAD UNLOAD
    public Collider2D LoadCollider(ColliderData data, Transform parent)
    {
        if (data is BoxData bdata) { return load_box_collider(bdata, parent); }
        if (data is CircleData cdata) { return load_circle_collider(cdata, parent); }
        if (!hide_log_load_collider_not_found) { Debug.LogWarning($"(ColliderBank - LoadCollider) data is nor BoxData nor CircleData, can't load collider : {data}\n{data.GetDetails()}"); }
        return null;
    }
    private BoxCollider2D load_box_collider(BoxData data, Transform parent)
    {
        // we get the pool to extract to
        Stack<GameObject> pool = pooled_box_colliders;
        if (data.shadow_caster_data != null)
        {
            if (!pooled_shadowed_box_colliders.ContainsKey(data.shadow_caster_data.used_layers)) { pooled_shadowed_box_colliders[data.shadow_caster_data.used_layers] = new Stack<GameObject>(); }
            pool = pooled_shadowed_box_colliders[data.shadow_caster_data.used_layers];
        }

        // we try to extract a collider from the pool
        BoxCollider2D collider;
        if (pool != null && pool.Count > 0)
        {
            GameObject go = pool.Pop();
            go.SetActive(true);
            go.transform.SetParent(parent);
            collider = go.GetComponent<BoxCollider2D>();
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            collider = Instantiate(box_collider_prefab, parent).GetComponent<BoxCollider2D>();
            collider.name = "box_collider";
            shadow_caster_datas[collider.gameObject] = data.shadow_caster_data; // we save the shadow caster data for unloading
        }

        // we activate the collider
        collider.enabled = true;

        // we load navmesh data
        create_or_destroy_navmesh_modifier(collider.gameObject, data.pathfinding_area);

        // we load the collider data
        load_collider_data(collider, data);
        collider.size = data.size;

        // we load the shadow caster data
        if (data.shadow_caster_data is not null) { load_shadow_caster_data(collider.gameObject, data.shadow_caster_data); }        

        return collider;
    }
    private CircleCollider2D load_circle_collider(CircleData data, Transform parent)
    {
        // we get the pool to extract to
        Stack<GameObject> pool = pooled_circle_colliders;
        if (data.shadow_caster_data != null)
        {
            if (!pooled_shadowed_circle_colliders.ContainsKey(data.shadow_caster_data.used_layers)) { pooled_shadowed_circle_colliders[data.shadow_caster_data.used_layers] = new Stack<GameObject>(); }
            pool = pooled_shadowed_circle_colliders[data.shadow_caster_data.used_layers];
        }

        // we try to extract from the pool
        CircleCollider2D collider;
        if (pool != null && pool.Count > 0)
        {
            GameObject go = pool.Pop();
            go.SetActive(true);
            go.transform.SetParent(parent);
            collider = go.GetComponent<CircleCollider2D>();
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            collider = Instantiate(circle_collider_prefab, parent).GetComponent<CircleCollider2D>();
            collider.name = "circle_collider";
            shadow_caster_datas[collider.gameObject] = data.shadow_caster_data; // we save the shadow caster data for unloading
        }

        // we activate the collider
        collider.enabled = true;

        // we load navmesh data
        create_or_destroy_navmesh_modifier(collider.gameObject, data.pathfinding_area);

        // we load the collider data
        load_collider_data(collider, data);
        collider.radius = data.radius;
        
        // we load the shadow caster data
        if (data.shadow_caster_data is not null) { load_shadow_caster_data(collider.gameObject, data.shadow_caster_data); }
        return collider;
    }

    private void create_or_destroy_navmesh_modifier(GameObject gameObject, int pathfinding_area)
    {
        NavMeshModifier modifier = gameObject.GetComponent<NavMeshModifier>();
        
        // if we use pathfinding we verify that we have a nav mesh modifier
        if (pathfinding_area != -1)
        {
            if (modifier == null) { modifier = gameObject.AddComponent<NavMeshModifier>(); }
            modifier.area = pathfinding_area;
            return;
        }

        // if we are not using pathfinding we make sure we don't have any
        if (modifier != null) { Destroy(modifier); }
    }
    private void load_collider_data(Collider2D collider, ColliderData collider_data)
    {
        if (log_body_data) { Debug.Log($"(CapableBank - load_collider_data) Loading collider data : {(collider_data == null ? "null" : collider_data.GetDetails())}"); }

        // we set gameobject data
        collider.gameObject.layer = collider_data.layerID;
        collider.transform.localPosition = collider_data.local_position;

        // we set the collider data
        collider.offset = collider_data.offset;
        collider.isTrigger = collider_data.is_trigger;

        // if for navmesh, we check if we are world building or not
        // ! this is commented bcz we don't do this here after all,
        // ! this is the responsability of the capacity that uses the collider
        // ! if feet we never disable the collider for example
        // but ColliderCapacity yes
        /* if (collider_data.UsedForPathfinding)
        {
            collider.isTrigger = !WorldBuilder.IsWorking; // if we are world building we want it to be active, if not world building we don't want it
        } */
    }
    public void UnloadCollider(GameObject collider_go)
    {
        Collider2D collider = collider_go.GetComponent<Collider2D>();
        if (collider == null) { return; }

        // we check if we have shadow caster data for this collider
        ShadowCasterData shadow_data = null;
        if (shadow_caster_datas.ContainsKey(collider_go)) { shadow_data = shadow_caster_datas[collider_go]; }

        // we push back to the right pool
        if (collider is BoxCollider2D box_collider)
        {
            if (shadow_data is null) { pooled_box_colliders.Push(box_collider.gameObject); }
            else
            {
                if (!pooled_shadowed_box_colliders.ContainsKey(shadow_data.used_layers)) { pooled_shadowed_box_colliders[shadow_data.used_layers] = new Stack<GameObject>(); }
                pooled_shadowed_box_colliders[shadow_data.used_layers].Push(box_collider.gameObject);
            }
        }
        else if (collider is CircleCollider2D circle_collider)
        {
            if (shadow_data is null) { pooled_circle_colliders.Push(circle_collider.gameObject); }
            else
            {
                if (!pooled_shadowed_circle_colliders.ContainsKey(shadow_data.used_layers)) { pooled_shadowed_circle_colliders[shadow_data.used_layers] = new Stack<GameObject>(); }
                pooled_shadowed_circle_colliders[shadow_data.used_layers].Push(circle_collider.gameObject);
            }
        }

        // we set the parent of the collider to the sleeping colliders parent to keep the hierarchy clean
        collider_go.SetActive(false);
        collider_go.layer = sleeping_layer;
        collider_go.transform.SetParent(sleeping_colliders_parent);
    }
    public void UnloadColliders(HashSet<GameObject> collider_gos)
    {
        foreach (GameObject go in collider_gos) { UnloadCollider(go); }
    }


    // USEFUL STATIC METHODS
    public static IColliderData GetColliderData(Collider2D collider)
    {
        // setup basic data
        ColliderData data = new ColliderData
        {
            local_position = collider.transform.localPosition,
            layerID = collider.gameObject.layer,
            offset = collider.offset,
            is_trigger = collider.isTrigger,
            shadow_caster_data = get_static_shadow_caster_data(collider)
        };

        slog?.LogExtended($"Basic collider data gathered for {collider.gameObject.name} of type {collider.GetType().Name} with offset {collider.offset} and isTrigger {collider.isTrigger}");
        data.pathfinding_area = get_pathfinding_area(collider);

        slog?.LogExtended($"Pathfinding area for collider {collider.gameObject.name} is {data.pathfinding_area}. Getting circle/box data if applicable.");

        // check if circle
        if (collider is CircleCollider2D circle)
        {
            slog?.LogExtended($"Collider {collider.gameObject.name} is a CircleCollider2D with radius {circle.radius}. Gathering circle collider data.");
            return new CircleData(data)
            {
                radius = circle.radius
            };
        }

        // check if box
        if (collider is BoxCollider2D box)
        {
            slog?.LogExtended($"Collider {collider.gameObject.name} is a BoxCollider2D with size {box.size}. Gathering box collider data.");
            return new BoxData(data)
            {
                size = box.size
            };
        }

        slog?.Warning($"Collider {collider.gameObject.name} is of type {collider.GetType().Name} which is not supported by ColliderBank, returning basic collider data without size/radius info");
        return data;
    }
    protected static int get_pathfinding_area(Collider2D collider)
    {
        slog?.LogSpecific($"Getting pathfinding area for collider {collider.gameObject.name}");
        NavMeshModifier modifier = collider.GetComponent<NavMeshModifier>();
        if (modifier == null) { slog?.LogSpecific($"no nav mesh modifier found"); return -1; }
        if (!modifier.enabled) { slog?.LogSpecific($"nav mesh modifier found but not enabled"); return -1; }
        slog?.LogSpecific($"Pathfinding area is {modifier.area}");
        return modifier.area;
    }
    public static void LoadColliderData(Collider2D c, ColliderData data)
    {
        // Debug.Log($"(ColliderBank - LoadColliderData) Loading collider data : {(data == null ? "null" : data.GetDetails())} to collider {c.gameObject.name}");

        // we set gameobject data
        c.gameObject.layer = data.layerID;
        c.transform.localPosition = data.local_position;

        // we set the collider data
        c.offset = data.offset;
        c.isTrigger = data.is_trigger;

        if (c is BoxCollider2D box_collider)
        {
            if (data is BoxData bdata) { box_collider.size = bdata.size; }
        }
        else if (c is CircleCollider2D circle_collider)
        {
            if (data is CircleData cdata) { circle_collider.radius = cdata.radius; }
        }
    }


    ///
    //
    ///  SHADOW CASTER DATA
    //
    ///

    // SHADOW CASTER DATA
    protected static ShadowCasterData get_static_shadow_caster_data(Collider2D collider)
    {
        ShadowCaster2D shadow_caster = collider.GetComponent<ShadowCaster2D>();
        if (shadow_caster == null || !shadow_caster.enabled)
        {
            if (log_shadows) { Debug.Log($"(ColliderBank - get_static_shadow_caster_data) No shadow caster found on collider {collider.gameObject.name}"); }
            return null;
        }

        // get the layers
        ShadowCasterData data = new ShadowCasterData();
        data.used_layers = get_static_shadow_used_layers(shadow_caster);
        if (data.used_layers == null || data.used_layers.Count == 0)
        {
            if (log_shadows) { Debug.Log($"(ColliderBank - get_static_shadow_caster_data) Shadow caster on collider {collider.gameObject.name} has no used layers, ignoring shadow caster data"); }
            return null;
        }

        // get the cast and self data
        if (shadow_caster.selfShadows) { data.cast_and_self = true; }
        else { data.cast_and_self = false; }

        // return the data
        if (log_shadows) { Debug.Log($"(ColliderBank - get_static_shadow_caster_data) Found shadow caster on collider {collider.gameObject.name} with data : \n{data.GetDetails()}"); }
        return data;
    }
    private static FieldInfo sorting_layers_field = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers", BindingFlags.Instance | BindingFlags.NonPublic);
    protected static List<string> get_static_shadow_used_layers(ShadowCaster2D shadow_caster)
    {
        // return new List<int>((int[]) sorting_layers_field.GetValue(shadow_caster));
        int[] sorting_layers = (int[]) sorting_layers_field.GetValue(shadow_caster);
        List<string> used_layers = new List<string>();
        for (int i = 0; i < sorting_layers.Length; i++) { used_layers.Add(SortingLayer.IDToName(sorting_layers[i])); }
        return used_layers;
    }
    protected static void load_shadow_caster_data(GameObject go, ShadowCasterData data)
    {
        if (data == null) { return; }
        if (data.used_layers == null || data.used_layers.Count == 0) { return; }

        Collider2D collider = go.GetComponent<Collider2D>();
        ShadowCaster2D shadow_caster = go.GetComponent<ShadowCaster2D>();
        if (shadow_caster == null)
        {
            shadow_caster = go.AddComponent<ShadowCaster2D>();
            // var probe = go.AddComponent<DebugShadowProber>(); probe.Init(shadow_caster, collider, "load_shadow_caster_data");
            // if (log_shadows_shapes) { Debug.Log($"(ColliderBank - load_shadow_caster_data) Added shadow caster to gameobject {go.name}. shape is {string.Join(", ", shadow_caster.shapePath)}"); }
        }

        // Get the collider for debugging
        if (log_shadows_shapes && collider != null)
        {
            Debug.Log($"(ColliderBank - load_shadow_caster_data) Before setup - Collider offset: {collider.offset}, size: {(collider is BoxCollider2D ? ((BoxCollider2D)collider).size : "N/A")}");
        }

        shadow_caster.enabled = false;
        shadow_caster.castingOption = data.cast_and_self ? ShadowCaster2D.ShadowCastingOptions.CastAndSelfShadow : ShadowCaster2D.ShadowCastingOptions.CastShadow;

        // convert the used layers from string to int and set them to the shadow caster
        List<int> used_layers_int = new List<int>();
        foreach (var layer_name in data.used_layers)
        {
            int layer_id = SortingLayer.NameToID(layer_name);
            used_layers_int.Add(layer_id);
        }
        sorting_layers_field.SetValue(shadow_caster, used_layers_int.ToArray());

        shadow_caster.enabled = true;
        ApplyShadowShapeFromCollider(shadow_caster, collider);

        if (log_shadows_shapes)
        {
            Debug.Log($"(ColliderBank - load_shadow_caster_data) After setup - {go.name}. Collider: {collider?.GetType().Name}, ShapePath points: {shadow_caster.shapePath?.Length ?? 0}, Shape: {string.Join(", ", shadow_caster.shapePath ?? System.Array.Empty<Vector3>())}");
        }
        // i)f (log_shadows_shapes) { Debug.Log($"(ColliderBank - load_shadow_caster_data) Loaded shadow caster data to {go.name}. shape is {string.Join(", ", shadow_caster.shapePath)}"); }
    }

    private static FieldInfo shape_path = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.Instance | BindingFlags.NonPublic);
    protected static void ApplyShadowShapeFromCollider(ShadowCaster2D shadowCaster, Collider2D collider)
    {
        if (shadowCaster == null || collider == null) return;

        if (collider is BoxCollider2D box)
        {
            Vector2 size = box.size;
            Vector2 offset = box.offset;
            float hx = size.x * 0.5f;
            float hy = size.y * 0.5f;
            Vector3[] pts = new Vector3[4];
            pts[0] = new Vector3(offset.x - hx, offset.y - hy, 0f);
            pts[1] = new Vector3(offset.x - hx, offset.y + hy, 0f);
            pts[2] = new Vector3(offset.x + hx, offset.y + hy, 0f);
            pts[3] = new Vector3(offset.x + hx, offset.y - hy, 0f);
            shape_path.SetValue(shadowCaster, pts);
            return;
        }

        if (collider is CircleCollider2D c)
        {
            int segments = 12;
            float r = c.radius;
            Vector2 offset = c.offset;
            Vector3[] pts = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float a = (2f * Mathf.PI * i) / segments;
                pts[i] = new Vector3(offset.x + Mathf.Cos(a) * r, offset.y + Mathf.Sin(a) * r, 0f);
            }
            shape_path.SetValue(shadowCaster, pts);
            return;
        }

        // fallback: use collider.bounds converted to local space
        Bounds b = collider.bounds;
        Vector3 centerLocal = collider.transform.InverseTransformPoint(b.center);
        Vector3 ext = b.extents;
        Vector3[] fallback = new Vector3[4];
        fallback[0] = new Vector3(centerLocal.x - ext.x, centerLocal.y - ext.y, 0f);
        fallback[1] = new Vector3(centerLocal.x - ext.x, centerLocal.y + ext.y, 0f);
        fallback[2] = new Vector3(centerLocal.x + ext.x, centerLocal.y + ext.y, 0f);
        fallback[3] = new Vector3(centerLocal.x + ext.x, centerLocal.y - ext.y, 0f);
        shape_path.SetValue(shadowCaster, fallback);
    }
}