using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

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
    protected static bool log_shadows = true;


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
        create_or_destroy_navmesh_modifier(collider.gameObject, data.used_for_pathfinding);

        // we load the collider data
        load_collider_data(collider, data);
        collider.size = data.size;
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
        create_or_destroy_navmesh_modifier(collider.gameObject, data.used_for_pathfinding);

        // we load the collider data
        load_collider_data(collider, data);
        collider.radius = data.radius;
        return collider;
    }

    private void create_or_destroy_navmesh_modifier(GameObject gameObject, bool use_pathfinding)
    {
        NavMeshModifier modifier = gameObject.GetComponent<NavMeshModifier>();
        
        // if we use pathfinding we verify that we have a nav mesh modifier
        if (use_pathfinding)
        {
            if (modifier == null) { gameObject.AddComponent<NavMeshModifier>(); }
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

        // we set the shadow caster data
        if (collider_data.shadow_caster_data is null) { return; }
        load_shadow_caster_data(collider.gameObject, collider_data.shadow_caster_data);
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
            used_for_pathfinding = is_used_for_pathfinding(collider),
            shadow_caster_data = get_static_shadow_caster_data(collider)
        };

        // check if circle
        if (collider is CircleCollider2D circle)
        {
            return new CircleData(data)
            {
                radius = circle.radius
            };
        }

        // check if box
        if (collider is BoxCollider2D box)
        {
            return new BoxData(data)
            {
                size = box.size
            };
        }

        return data;
    }
    protected static bool is_used_for_pathfinding(Collider2D collider)
    {
        NavMeshModifier modifier = collider.GetComponent<NavMeshModifier>();
        if (modifier != null && modifier.enabled) { return true; }
        return false;
    }



    // SHADOW CASTER DATA
    protected static ShadowCasterData get_static_shadow_caster_data(Collider2D collider)
    {
        ShadowCaster2D shadow_caster = collider.GetComponent<ShadowCaster2D>();
        if (shadow_caster == null || !shadow_caster.enabled)
        {
            if (log_shadows) { Debug.Log($"(ColliderBank - get_static_shadow_caster_data) No shadow caster found on collider {collider.gameObject.name}"); }
            return null;
        }
        ShadowCasterData data = new ShadowCasterData();
        if (shadow_caster.selfShadows) { data.cast_and_self = true; }
        else { data.cast_and_self = false; }
        data.used_layers = get_static_shadow_used_layers(shadow_caster);
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
        ShadowCaster2D shadow_caster = go.GetComponent<ShadowCaster2D>();
        if (shadow_caster == null) { shadow_caster = go.AddComponent<ShadowCaster2D>(); }
        shadow_caster.enabled = true;
        shadow_caster.castingOption = data.cast_and_self ? ShadowCaster2D.ShadowCastingOptions.CastAndSelfShadow : ShadowCaster2D.ShadowCastingOptions.CastShadow;
        
        // convert the used layers from string to int and set them to the shadow caster
        List<int> used_layers_int = new List<int>();
        foreach (var layer_name in data.used_layers)
        {
            int layer_id = SortingLayer.NameToID(layer_name);
            used_layers_int.Add(layer_id);
        }
        sorting_layers_field.SetValue(shadow_caster, used_layers_int.ToArray());
    }
}