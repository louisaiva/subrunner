using System;
using System.Collections.Generic;
using UnityEngine;

public class Movable : Capable
{
    [Header("MOVABLE")]
    public Rigidbody2D rb;  // Replace transform movement
    public float weight = 1f;
    public float friction = 7f;
    public bool debug_velocity = false; // Show velocity in console

    [Header("Forces")]
    public List<Force> forces = new List<Force>();
    public float input_speed;
    public Vector2 Velocity;
    

    [Header("Collisions")]
    public Collider2D feet_collider;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;  // No gravity in top-down games
            rb.freezeRotation = true; // Prevent unwanted rotation
        }
        else { Debug.LogError("No Rigidbody2D found on " + gameObject.name); }

        // Get the feet collider
        feet_collider = transform.Find("feet").GetComponent<Collider2D>();
    }

    // UPDATE
    protected virtual void FixedUpdate()
    {
        // base.Update();

        // checks if it is not carried by a Being
        if (HasEffect(Effect.BeingCarried))
        {
            // we remove all the forces
            ClearForces();
            return;
        }
        else if (rb == null) { return; }

        // Update moving effects
        updateMovingEffects();

        // Update forces
        updateForces();
    }

    // UPDATE FORCES
    protected virtual void updateForces()
    {
        // Apply input velocity from capacities
        if (HasCapacity<WalkCapacity>())
        {
            rb.linearVelocity = GetCapacity<WalkCapacity>().walk_speed * Orientation;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop movement if no speed
        }

        // Apply forces
        Vector2 totalForce = Vector2.zero;
        for (int i = forces.Count - 1; i >= 0; i--)
        {
            Force force = forces[i];

            // Remove expired forces
            if (force.expired)
            {
                forces.RemoveAt(i);
                continue;
            }

            totalForce += force.direction * force.magnitude / weight;
            force.Update();
        }

        // Apply force to Rigidbody2D
        rb.AddForce(totalForce * Time.timeScale, ForceMode2D.Force);

        // Apply friction when no force is applied
        if (totalForce == Vector2.zero && inputs == Vector2.zero)
        {
            // if (debug) { Debug.Log("Applying friction ("+ friction +") to " + gameObject.name + " with velocity " + rb.velocity); }
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, friction * Time.deltaTime);
        }

    }

    // EFFECTS ASSOCIATED TO MOVING
    protected void updateMovingEffects()
    {
        // we check if the Capable has the Ghost effect and if yes, we change the Layer of the feet collider to "Ghosts"

        // update the feet layer
        string feet_layer = feet_collider.gameObject.layer.ToString();
        if (feet_layer != "Feet" && !HasEffect(Effect.SemiGhost) && !HasEffect(Effect.Ghost))
        {
            // if no ghost effect applied than we switch back to normal
            feet_collider.gameObject.layer = LayerMask.NameToLayer("Feet");

        }
        else if (feet_layer != "Ghosts" && (HasEffect(Effect.SemiGhost) || HasEffect(Effect.Ghost)))
        {
            // if a ghost effect is applied than we switch to Ghosts
            feet_collider.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }

        // update the feet is trigger
        if (HasEffect(Effect.Ghost))
        {
            feet_collider.isTrigger = true; // if the ghost effect is applied, we set the feet collider as trigger
        }
        else
        {
            feet_collider.isTrigger = false; // if not, we set it as not trigger
        }
    }

    // FORCES
    public void AddForce(Force force)
    {
        // checks if the force already exists
        foreach (Force f in forces)
        {
            if (f.name == force.name)
            {
                // if it does, we update it
                f.direction = force.direction;
                f.magnitude = force.magnitude;
                f.attenuation = force.attenuation;
                return;
            }
        }

        // if the force doesn't exist, we add it
        forces.Add(force);
    }
    public void ClearForces()
    {
        // on supprime toutes les forces
        forces.Clear();
    }
    protected void LateUpdate()
    {
        // check if we have a rigidbody
        if (rb == null)
        {
            Velocity = Vector2.zero;
            return;
        }

        // we set the current velocity
        Velocity = rb.linearVelocity / Time.fixedDeltaTime;

        // we log the current linear velocity
        if (debug_velocity) { Debug.Log("velocity : " + Velocity); }
    }

    // gizmos
    protected virtual void OnDrawGizmos()
    {
        // on dessine le collider des pieds
        if (feet_collider == null) { return; }

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(feet_collider.bounds.center, feet_collider.bounds.size);
    }

}


[Serializable] public class Force
{
    public static int id_copies = 0;

    // Force parameters
    public string name = "null";
    public Vector2 direction;
    public float magnitude;
    public float attenuation;
    public bool expired
    {
        get { return magnitude < 0.05f; }
    }


    // Constructors
    public Force(string name, Vector2 direction, float magnitude, float attenuation = 1f)
    {
        this.name = name;
        this.direction = direction;
        this.magnitude = magnitude;
        this.attenuation = attenuation;
    }

    public Force(Force force)
    {
        name = force.name + " (copy " + id_copies++ + ")";
        direction = force.direction;
        magnitude = force.magnitude;
        attenuation = force.attenuation;
    }

    public void Update()
    {
        // on diminue la force avec ma 2e méthode Update -> chat gpt one
        magnitude *= Mathf.Exp(-attenuation * Time.deltaTime * 5f);
    }
}
