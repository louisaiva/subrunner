using System;
using System.Collections;
using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;

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
    [SerializeField] protected Stack<GameObject> pooled_circle_colliders;
    [SerializeField] protected Stack<GameObject> pooled_box_colliders;


    // LOAD UNLOAD
    public BoxCollider2D LoadBoxCollider(BoxData data, Transform parent)
    {
        // we first try to extract a collider from the pool
        BoxCollider2D collider;
        if (pooled_box_colliders != null && pooled_box_colliders.Count > 0)
        {
            GameObject go = pooled_box_colliders.Pop();
            go.SetActive(true);
            go.transform.SetParent(parent);
            collider = go.GetComponent<BoxCollider2D>();
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            collider = Instantiate(box_collider_prefab, parent).GetComponent<BoxCollider2D>();
            collider.name = "box_collider";
        }

        // we load navmesh data
        create_or_destroy_navmesh_modifier(collider.gameObject, data.used_for_pathfinding);

        // we load the collider data
        load_collider_data(collider, data);
        collider.size = data.size;
        return collider;
    }
    public CircleCollider2D LoadCircleCollider(CircleData data, Transform parent)
    {
        CircleCollider2D collider;
        if (pooled_circle_colliders != null && pooled_circle_colliders.Count > 0)
        {
            GameObject go = pooled_circle_colliders.Pop();
            go.SetActive(true);
            go.transform.SetParent(parent);
            collider = go.GetComponent<CircleCollider2D>();
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            collider = Instantiate(circle_collider_prefab, parent).GetComponent<CircleCollider2D>();
            collider.name = "circle_collider";
        }

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
        // we set gameobject data
        collider.gameObject.layer = collider_data.layerID;
        collider.transform.localPosition = collider_data.local_position;

        // we set the collider data
        collider.offset = collider_data.offset;
        collider.isTrigger = collider_data.is_trigger;
    }


    public void UnloadCollider(GameObject collider_go)
    {
        Collider2D collider = collider_go.GetComponent<Collider2D>();
        if (collider == null) { return; }


        if (collider is BoxCollider2D box_collider)
        {
            pooled_box_colliders.Push(box_collider.gameObject);
        }
        else if (collider is CircleCollider2D circle_collider)
        {
            pooled_circle_colliders.Push(circle_collider.gameObject);
        }

        // we set the parent of the collider to the sleeping colliders parent to keep the hierarchy clean
        collider_go.SetActive(false);
        collider_go.layer = sleeping_layer;
        collider_go.transform.SetParent(sleeping_colliders_parent);
    }
}