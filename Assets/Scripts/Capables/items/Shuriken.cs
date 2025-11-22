using System.Collections;
using UnityEngine;

public class Shuriken : Item, Usable
{

    [Header("Shuriken parameters")]
    public float throw_force_magnitude = 100f; // the force of the shuriken when thrown
    public ParticleSystem shuriken_particle; // the particle system of the shuriken
    private ParticleSystem.EmissionModule emission; // the emission module of the particle system
    private float particle_rotation_offset = 180f; // the rotation offset of the particle system

    [Header("Speed")]
    public float current_speed = 1f; // the current speed of the shuriken

    protected override void Awake()
    {
        base.Awake();

        // we start the particle system
        shuriken_particle.gameObject.SetActive(false);
        emission = shuriken_particle.emission; // we get the emission module of the particle system
    }

    // we stop throwing the shuriken when it is stopped
    private IEnumerator AdjustAnim(Force throw_force)
    {
        // we wait until the shuriken is thrown
        yield return new WaitUntil(() => Velocity.magnitude > 0f);

        // we wait for the shuriken to stop
        while (Velocity.magnitude > 25f && GetComponent<AnimPlayer>().current_anim.capacity == "throw")
        {
            // we adjust the animation speed to the shuriken speed
            float speed = Velocity.magnitude;

            if (speed > 1000f)
            {
                GetComponent<AnimPlayer>().current_anim.speed = 3f;
                emission.rateOverTime = 30f;
                current_speed = 30f;
            }
            else if (speed > 200f)
            {
                GetComponent<AnimPlayer>().current_anim.speed = 2f;
                emission.rateOverTime = 20f;
                current_speed = 20f;
            }
            else
            {
                GetComponent<AnimPlayer>().current_anim.speed = 1f;
                emission.rateOverTime = 10f;
                current_speed = 10f;
            }

            if (debug_velocity) { Debug.Log("(Shuriken) shuriken animation speed : " + GetComponent<AnimPlayer>().current_anim.speed); }

            // we orient the shuriken particle system
            orient_particle(Velocity.normalized);

            // we wait for a bit
            yield return null;
        }

        if (debug) { Debug.Log("(Shuriken) shuriken stopped"); }

        // we stop the animation
        GetComponent<AnimPlayer>().current_anim.speed = 1f; // we set the speed to 1
        GetComponent<AnimPlayer>().StopPlaying("throw");

        // we remove the ghost effect
        RemoveEffect(Effect.SemiGhost);

        // we stop the particle system
        shuriken_particle.gameObject.SetActive(false);
        emission.rateOverTime = 0f;
    }
    private void orient_particle(Vector2 direction)
    {
        // we get the angle of the shuriken
        float angle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);

        // we set the rotation of the particle system
        shuriken_particle.transform.rotation = Quaternion.Euler(0f, 0f, particle_rotation_offset + angle);
    }


    // USABLE
    public string UseLabel { get; } = "throw";
    public void Use(Capable user)
    {
        // the shuriken can be thrown only if it is grabbed
        if (!Grabbed) { return; }

        // we check if we have an attack capacity
        AttackCapacity attack_capacity = GetCapacity<AttackCapacity>();
        if (attack_capacity == null) { return; }

        // we find the holder of the item
        /* Capable holder = transform.parent.GetComponent<Inventory>().capable;
        if (holder == null) { return; } */

        // we whitelist our holder as the user of the attack
        attack_capacity.WhiteListTagShortly(user.tag, 3f); // we whitelist the holder for 0.5s

        // we use the attack capacity
        attack_capacity.Use(this);

        // we throw the shuriken
        Force throw_force = new Force("throw", user.Orientation, throw_force_magnitude, 0.8f);
        AddForce(throw_force); // we add the force to the shuriken

        // we add a ghost effect to the shuriken
        AddEffect(Effect.SemiGhost, -888f); // we add the ghost effect for infinite time

        // we start the particle system
        shuriken_particle.gameObject.SetActive(true);
        emission.rateOverTime = 30f;
        orient_particle(throw_force.direction.normalized); // we orient the shuriken particle system

        // we launch the coroutine for removing the ghost effect & the animation when stopped
        StartCoroutine(AdjustAnim(throw_force)); // we start the coroutine for stopping the shuriken

        if (debug) { Debug.Log("(Shuriken) shuriken throwed by " + user.name); }
    }

}