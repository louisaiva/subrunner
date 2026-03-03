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
    [SerializeField] protected Stack<GameObject> pooled_circle_colliders;
    [SerializeField] protected Stack<GameObject> pooled_box_colliders;


    // LOAD UNLOAD
    public BoxCollider2D LoadBoxCollider(Transform collider_parent, bool use_pathfinding)
    {
        // we first try to extract a collider from the pool
        BoxCollider2D collider;
        if (pooled_box_colliders != null && pooled_box_colliders.Count > 0)
        {
            GameObject go = pooled_box_colliders.Pop();
            go.SetActive(true);
            go.transform.SetParent(collider_parent);
            collider = go.GetComponent<BoxCollider2D>();
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            collider = Instantiate(box_collider_prefab, collider_parent).GetComponent<BoxCollider2D>();
            collider.name = "box_collider";
        }

        create_or_destroy_navmesh_modifier(collider.gameObject, use_pathfinding);

        return collider;
    }
    public CircleCollider2D LoadCircleCollider(Transform collider_parent, bool use_pathfinding)
    {
        CircleCollider2D collider;
        if (pooled_circle_colliders != null && pooled_circle_colliders.Count > 0)
        {
            GameObject go = pooled_circle_colliders.Pop();
            go.SetActive(true);
            go.transform.SetParent(collider_parent);
            collider = go.GetComponent<CircleCollider2D>();
        }
        else
        {
            // if we have no pooled anim layer we need to instantiate one
            collider = Instantiate(circle_collider_prefab, collider_parent).GetComponent<CircleCollider2D>();
            collider.name = "circle_collider";
        }

        create_or_destroy_navmesh_modifier(collider.gameObject, use_pathfinding);
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

    public void UnloadCollider(GameObject collider)
    {
        if (collider.GetComponent<BoxCollider2D>() is BoxCollider2D box_collider)
        {
            pooled_box_colliders.Push(box_collider.gameObject);
        }
        else if (collider.GetComponent<CircleCollider2D>() is CircleCollider2D circle_collider)
        {
            pooled_circle_colliders.Push(circle_collider.gameObject);
        }

        // we set the parent of the collider to the sleeping colliders parent to keep the hierarchy clean
        collider.transform.SetParent(sleeping_colliders_parent);
        collider.gameObject.SetActive(false);
    }
}