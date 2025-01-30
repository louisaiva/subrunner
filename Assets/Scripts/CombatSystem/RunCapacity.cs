
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RunCapacity is a capacity that allows a movable object to run.
/// Handles collisions with the world based on the velocity of the object.
/// the feet collider is used to detect collisions with the world.
/// </summary>

public class RunCapacity : Capacity
{
    // [SerializeField] private AnimationCurve accelerationCurve;
    [Header("Collisions")]
    public float collisionBuffer = 0.01f;
    private BoxCollider2D feet;
    public LayerMask world_layers; // layers du monde


    private void Awake()
    {
        feet = GetComponent<BoxCollider2D>();
    }
}