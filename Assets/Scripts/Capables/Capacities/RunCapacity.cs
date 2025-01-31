
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RunCapacity is a capacity that allows a movable object to run.
/// Handles collisions with the world based on the velocity of the object.
/// the feet_collider collider is used to detect collisions with the world.
/// </summary>

public class RunCapacity : Capacity
{
    [Header("Capable & Feet")]
    private Movable movable;
    private BoxCollider2D feet_collider;



    private void Awake()
    {
        // we get the feet collider
        feet_collider = GetComponent<BoxCollider2D>();

        // we get the movable component
        movable = transform.parent.GetComponent<Movable>();
    }

    protected override void Update()
    {
        base.Update();

        if (movable == null) { return; }

        // we check if the Capable has the Ghost effect and if yes, we change the Layer of the feet collider to "Ghosts"
        if (movable.HasEffect(Effect.Ghost) && !(gameObject.layer == LayerMask.NameToLayer("Ghosts")))
        {
            gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }
        else if (!movable.HasEffect(Effect.Ghost) && (gameObject.layer != LayerMask.NameToLayer("Feet")))
        {
            gameObject.layer = LayerMask.NameToLayer("Feet");
        }
    }
}