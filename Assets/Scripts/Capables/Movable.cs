using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// [RequireComponent(typeof(Rigidbody2D))] -> removed because we want to be able to use Movable without a Rigidbody2D
// -> when an item is picked up, it is not supposed to move
public class Movable : Capable
{
    [Header("MOVABLE")]
    public Rigidbody2D rb;  // Replace transform movement
    public float weight = 1f;
    public float friction = 7f;
    public List<Force> forces = new List<Force>();
    public float input_speed;
    public Collider2D feet_collider;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;  // No gravity in top-down games
        rb.freezeRotation = true; // Prevent unwanted rotation

        // Get the feet collider
        feet_collider = transform.Find("feet").GetComponent<Collider2D>();
    }

    protected override void Update()
    {
        base.Update();

        // checks if it is not carried by a Being
        if (HasEffect(Effect.BeingCarried)) { return; }

        // Update moving effects
        updateMovingEffects();

        // Update forces
        updateForces();
    }

    // EFFECTS ASSOCIATED TO MOVING
    protected void updateMovingEffects()
    {
        // we check if the Capable has the Ghost effect and if yes, we change the Layer of the feet collider to "Ghosts"
        if (HasEffect(Effect.Ghost) && !(feet_collider.gameObject.layer == LayerMask.NameToLayer("Ghosts")))
        {
            feet_collider.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }
        else if (!HasEffect(Effect.Ghost) && (feet_collider.gameObject.layer != LayerMask.NameToLayer("Feet")))
        {
            feet_collider.gameObject.layer = LayerMask.NameToLayer("Feet");
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
    protected void updateForces()
    {
        // Update input speed
        if (input_speed < 0.1f) { input_speed = 0f; }

        // Apply input velocity
        rb.linearVelocity = input_speed * Orientation;

        // Apply forces
        Vector2 totalForce = Vector2.zero;
        for (int i = forces.Count - 1; i >= 0; i--)
        {
            Force force = forces[i];

            if (force.expired) // Small threshold to remove force
            {
                forces.RemoveAt(i);
                continue;
            }

            totalForce += force.direction * force.magnitude / weight;
            force.Update();
        }

        // Apply force to Rigidbody2D
        rb.AddForce(totalForce, ForceMode2D.Force);

        // Apply friction when no force is applied
        if (totalForce == Vector2.zero && inputs == Vector2.zero)
        {
            // if (debug) { Debug.Log("Applying friction ("+ friction +") to " + gameObject.name + " with velocity " + rb.velocity); }
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, friction * Time.deltaTime);
        }
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
