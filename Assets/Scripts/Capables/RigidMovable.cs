using System;
using System.Collections.Generic;
using UnityEngine;

public class RigidMovable : Capable
{
    [Header("MOVABLE")]
    public Rigidbody2D rb;  // Replace transform movement
    public float weight = 1f;
    private float friction = 7f;
    public List<Force> forces = new List<Force>();
    // public List<InputForce> input_forces = new List<InputForce>();
    public float input_speed;
    private BoxCollider2D feet;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;  // No gravity in top-down games
        rb.freezeRotation = true; // Prevent unwanted rotation
    }

    protected override void Update()
    {
        base.Update();
        updateForces();
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

    protected void updateForces()
    {
        // Update input speed
        if (input_speed < 0.1f) { input_speed = 0f; }

        // Apply input velocity
        rb.velocity = input_speed * Orientation;

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
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, friction * Time.deltaTime);
        }
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

// [Serializable] public class InputForce
// {
//     public Vector2 direction;
//     public float magnitude;
//     public float speed;

//     public InputForce(float speed)
//     {
//         this.speed = speed;
//     }

//     /* public void SetDirection(Vector2 normalized_inputs)
//     {
//         direction = normalized_inputs;
//     }
//     public void SetMagnitude(float inputs_magnitude)
//     {
//         magnitude = inputs_magnitude;
//     } */

//     public void UpdateInput(Vector2 inputDir, float inputMagnitude)
//     {
//         if (inputMagnitude < 0.1f || inputDir.magnitude < 0.1f) // If there is no input
//         {
//             magnitude = 0; // Stop force if no input
//             return;
//         }

//         // Update force direction and magnitude
//         direction = inputDir.normalized;
//         magnitude = inputMagnitude * speed;
//     }
// }