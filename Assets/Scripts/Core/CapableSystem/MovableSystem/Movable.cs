using System;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(Rigidbody2D))]
public class Movable : Capable
{



    public override void UnloadData()
    {
        base.UnloadData();

        // we clear the forces
        ClearForces();
    }









    public bool debug_velocity = false; // Show velocity in console
    public bool log_avoidance = false; // whether to log the avoidance force calculation

    [Header("MOVABLE")]
    private Rigidbody2D _rb;
    public Rigidbody2D Rb
    {
        get
        {
            if (_rb == null) { _rb = GetComponent<Rigidbody2D>(); }
            return _rb;
        }}
    public float weight = 1f;
    [SerializeField] private float random_weight_modifier_at_start = 0f; // weight += random.range(-5,5) in the start method if this modifier = 5
    public float friction = 7f;

    [Header("Forces")]
    public List<Force> forces = new List<Force>();
    public float input_speed;
    public Vector2 Velocity;



    // AWAKE
    protected virtual void Awake()
    {
        // base.Awake();

        if (Rb != null)
        {
            Rb.gravityScale = 0;  // No gravity in top-down games
            Rb.freezeRotation = true; // Prevent unwanted rotation
        }
        else { Debug.LogError("No Rigidbody2D found on " + gameObject.name); }
    }
    protected virtual void Start()
    {
        // random weight
        weight += UnityEngine.Random.Range(-random_weight_modifier_at_start, random_weight_modifier_at_start);
    }

    // ON ENABLE/DISABLE -> MOVABLE ENGINE REGISTERING
    protected override void OnEnable()
    {
        base.OnEnable();
        MovableEngine.Instance.Register(this);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (MovableEngine.Instance == null) { return; }

        // if we are in the bank, CapableSystem will handle MovableEngine unregisteration so we don't have to do it here
        if (CapableBank.Instance != null && CapableBank.Instance.HasCapable(this)) { return; }
        
        // if not we unregister ourselves like tall grown child
        MovableEngine.Instance.Unregister(this);
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
        else if (Rb == null) { return; }
        else if (FeetCollider == null) { return; }

        // Update moving effects
        updateMovingEffects();

        // Update forces
        updateForces();
    }

    // UPDATE FORCES
    protected virtual void updateForces()
    {
        // Stop movement if no speed or no orientation or no walk capacity
        Rb.linearVelocity = GetCapacity<WalkCapacity>()?.speed * Orientation ?? Vector2.zero;

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
        Rb.AddForce(totalForce * Time.timeScale, ForceMode2D.Force);

        // Apply friction when no force is applied
        if (totalForce == Vector2.zero && inputs == Vector2.zero)
        {
            // if (debug) { Debug.Log("Applying friction ("+ friction +") to " + gameObject.name + " with velocity " + rb.velocity); }
            Rb.linearVelocity = Vector2.Lerp(Rb.linearVelocity, Vector2.zero, friction * Time.deltaTime);
        }

    }

    // EFFECTS ASSOCIATED TO MOVING
    protected void updateMovingEffects()
    {

        // 1 - SEMI GHOST

        // update the semi ghost effect (passing through other feet by setting feet_collider layer to Ghosts)
        string feet_layer = LayerMask.LayerToName(FeetCollider.gameObject.layer);
        if (HasEffect(Effect.SemiGhost) && feet_layer == "Feet")
        {
            FeetCollider.gameObject.layer = LayerMask.NameToLayer("Ghosts");
        }
        else if (!HasEffect(Effect.SemiGhost) && feet_layer != "Feet")
        {
            FeetCollider.gameObject.layer = LayerMask.NameToLayer("Feet");
        }

        // 2 - GHOST

        // update the ghost effect (passing through everything by disabling the collider)
        if (HasEffect(Effect.Ghost) && FeetCollider.enabled)
        {
            FeetCollider.enabled = false; // if the ghost effect is applied, we disable the feet !! so we can go through everything
        }
        else if (!HasEffect(Effect.Ghost) && !FeetCollider.enabled)
        {
            FeetCollider.enabled = true; // if not, we re enable it
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
    public void SetForces(List<Force> new_forces)
    {
        // on remplace la liste des forces par la nouvelle liste
        forces.Clear();
        if (new_forces != null) { forces.AddRange(new_forces); }
    }
    public void ClearForces()
    {
        // on supprime toutes les forces
        forces.Clear();
    }
    public void ForceStop()
    {
        // on supprime les forces
        ClearForces();

        // on arrête le rb
        if (Rb != null) { Rb.linearVelocity = Vector2.zero; }
    }
    public List<Force> GetForces()
    {
        // on retourne la liste des forces
        return forces;
    }
    protected virtual void LateUpdate()
    {
        // check if we have a rigidbody
        if (Rb == null)
        {
            Velocity = Vector2.zero;
            return;
        }

        // we set the current velocity
        Velocity = Rb.linearVelocity / Time.fixedDeltaTime;

        // we log the current linear velocity
        if (debug_velocity) { Debug.Log("velocity : " + Velocity); }
    }

    public void DisableMovements()
    {
        if (this.HasEffect(Effect.BeingCarried)) { return; }

        ClearForces();
        Velocity = Vector2.zero;
        AddEffect(Effect.BeingCarried, -888f);

        if (FeetCollider != null) { FeetCollider.enabled = false; }
        if (Rb != null) { Rb.simulated = false; }
    }
    public void EnableMovements()
    {
        if (!this.HasEffect(Effect.BeingCarried)) { return; }

        RemoveEffect(Effect.BeingCarried);

        if (FeetCollider != null) { FeetCollider.enabled = true; }
        if (Rb != null) { Rb.simulated = true; }
    }

    // gizmos
    protected virtual void OnDrawGizmos()
    {
        // on dessine le collider des pieds
        if (FeetCollider == null) { return; }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(FeetCollider.bounds.center, FeetRadius);
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


    // CONSTRUCTORS
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

    // UPDATE
    public void Update()
    {
        // on diminue la force avec ma 2e méthode Update -> chat gpt one
        magnitude *= Mathf.Exp(-attenuation * Time.deltaTime * 5f);
    }

    // TO STRING
    public override string ToString()
    {
        return $"Force {name} : direction {direction}, magnitude {magnitude}, attenuation {attenuation}, expired {expired}";
    }
}
