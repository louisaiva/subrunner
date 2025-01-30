using System;
using System.Collections.Generic;
using UnityEngine;

public class Movable : Capable
{
    [Header("MOVABLE")]
    public Vector2 velocity = Vector2.zero;
    // [SerializeField] private AnimationCurve accelerationCurve;

    [Header("Collisions")]
    [SerializeField] private float step_size = 0.1f;
    [SerializeField] private float collisionBuffer = 0.05f;
    [SerializeField] protected LayerMask world_layers; // layers du monde
    private BoxCollider2D feet;


    [Header("Forces")]
    public float weight = 1f; // poids du movable (pour le knockback)
    [SerializeField] private float friction = 0.1f; // You can tweak this value to control how fast the object slows down
    public float force_min_magnitude = 0.1f;
    public List<Force> forces = new List<Force>();

    [Header("Debug")]
    public bool debug = false;

    // START
    protected override void Start()
    {
        base.Start();

        // on ajoute la capacité de déplacement
        // AddCapacity("walk");
        // AddCapacity("run");

        // on récupère notre collider_box des pieds
        if (feet == null) { feet = transform.Find("run").GetComponent<BoxCollider2D>(); }

        // on défini les layers du monde
        world_layers = LayerMask.GetMask("Ground", "Walls", "Ceiling", "Doors", "Chests", "Computers", "Decoratives", "Interactives");

    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // velocity = Vector2.zero;

        // Update forces
        updateForces();

        // Update movement (Being does it later in its update, if not dead)
        // if (this is not Being || !(this as Being).Alive) { updateMovement(); }
    }


    // FORCES
    public void AddForce(Force force)
    {
        // on ajoute une force
        forces.Add(force);
    }
    public void ClearForces()
    {
        // on supprime toutes les forces
        forces.Clear();
    }
    protected void updateForces()
    {

        // checks if we have forces and if not, apply friction
        if (forces.Count == 0) { ApplyFriction(); return; }
        if (debug) { Debug.Log("(Movable - updateForces) Updating " + forces.Count + " Forces"); }

        // on met à jour les forces
        for (int i = forces.Count - 1; i >= 0; i--)
        {
            Force force = forces[i];
            if (force.expired /* && force.magnitude < force_min_magnitude */)
            {
                // si la force est nulle, on la supprime
                forces.RemoveAt(i);
                continue;
            }

            // on ajoute la force à la vélocité
            float force_velocity = force.magnitude * Time.deltaTime / weight;
            velocity += force.direction * force_velocity;
            if (debug) { Debug.Log("(Movable - updateForces) velocity gains "+ force_velocity + " from force " + force.direction + " with magnitude " + force.magnitude); }

            // on update la force
            force.Update();
        }
        
    }

    private void ApplyFriction()
    {
        // We apply a small drag force to slow the object down when no force is applied.
        if (velocity.magnitude > 0)
        {
            velocity *= 1 - friction * Time.deltaTime; // Apply friction over time
        }
    }


    // COLLISIONS & MOVEMENT FINAL
    protected void updateMovement()
    {
        // checks if we have a velocity
        if (velocity.magnitude <= new Vector2(0.01f, 0.01f).magnitude) { return; }

        // Move the object based on the current velocity
        Vector2 movement = velocity * Time.deltaTime;
        int steps = Mathf.CeilToInt(movement.magnitude / step_size); // Adjust step size as needed

        if (debug && movement.magnitude > 0)
        {
            Debug.Log("(Movable - updateMovement) Moving " + steps + " steps with velocity " + velocity + " and movement " + movement);
        }

        for (int i = 0; i < steps; i++)
        {
            Vector2 stepMove = movement / steps;
            bool step_success = move(stepMove);  // Call your existing move() method

            if (debug)
            {
                Debug.Log("(Movable - updateMovement) Step " + i + " with movement " + stepMove + " was " + (step_success ? "successful" : "blocked"));
            }

            if (!step_success)
            {
                // If we hit something, stop movement
                velocity = Vector2.zero;
                break;
            }
        }
    }
    protected bool move(Vector2 step)
    {
        Bounds feet_aabb = feet.bounds;

        // Check for obstacles
        RaycastHit2D hit = Physics2D.BoxCast(
            feet_aabb.center,
            feet_aabb.size,
            0f,
            step.normalized,
            step.magnitude,
            world_layers
        );

        if (hit.collider != null)
        {

            Vector2 old_velocity = velocity;
            // Project velocity onto the surface normal to get sliding motion
            velocity = Vector2.Reflect(velocity, hit.normal);
            if (debug) { Debug.Log("Hit " + hit.collider.name + " at " + hit.point + " with normal " + hit.normal
                        + " : reflecting velocity from "+ old_velocity +" to " + velocity); }

            // If the new velocity is too small, stop movement
            if (velocity.magnitude < 0.1f) { velocity = Vector2.zero; }

            return false; // Movement was blocked

            // If we hit something, adjust movement
            float safeDistance = hit.distance - collisionBuffer;
            Vector2 safeMove = step.normalized * safeDistance;
            transform.position += (Vector3)safeMove;
            return false;
        }

        if (debug) { Debug.Log("No collision detected, moving from" + transform.position
                    + " to " + (transform.position + (Vector3)step)); }

        // No collision, apply full step
        transform.position += (Vector3)step;
        return true;
    }
    protected void HandleCollision(Vector2 hitNormal)
    {
        // Reflect the velocity based on the normal to simulate a bounce
        velocity = Vector2.Reflect(velocity, hitNormal);

        // Optionally, you can also add friction or sliding behavior:
        float friction = 0.5f;  // This is an example, adjust it based on your needs
        velocity *= friction;   // Reduce the velocity based on friction

        // You can also add more complex behavior based on the angle of collision or surface type.
    }

    // gizmos
    protected virtual void OnDrawGizmos()
    {
        // on dessine le collider des pieds
        if (feet == null) { return; }

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(feet.bounds.center, feet.bounds.size);
    }

}

