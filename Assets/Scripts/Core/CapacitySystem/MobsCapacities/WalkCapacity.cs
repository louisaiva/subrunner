using FMOD.Studio;
using UnityEngine;

public class WalkCapacity : Capacity
{

    [Header("WALK CAPACITY")]
    public float walk_speed = 0f; // si c'est à 0 on marche pas, sinon on marche en conséquence !

    [Header("Walk Inputs")]
    public float walk_percentage_target = 0f; // walk_speed lerp continuellement jusqu'à walk_percentage_target*max_speed
    public float max_speed = 3f; // vitesse maximale de déplacement, atteinte quand walk_speed = 1f
    [SerializeField] private float random_speed_modifier_at_start = 0; // max_speed += random.range(-5,5) in the start method if this modifier = 5

    [Header("Walk Particles")]
    [SerializeField] private ParticleSystem walk_particles; // particles to play when walking
    ParticleSystem.EmissionModule walk_particles_emission; // emission module of the particles
    private float base_walk_particles_rate = 10f; // base rate of the particles emission

    [Header("Walk sound instance")]
    public EventInstance walk_sound;

    private void Awake()
    {
        walk_particles = GetComponent<ParticleSystem>();
        if (walk_particles == null) { return; }
        walk_particles_emission = walk_particles.emission;
        walk_particles_emission.enabled = false; // we disable the particles by default
        base_walk_particles_rate = walk_particles_emission.rateOverTime.constant; // we get the base rate of the particles emission

    }

    // START
    private void Start()
    {
        max_speed += Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start);

        // we get a walk sound instance from the audio engine
        walk_sound = AudioEngine.Instance.CreateSound(AudioBank.Instance.bob_walk);
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // update particles
        update_particles();

        // update sound
        update_sound();

        // 1 - WALK_SPEED CALCULATION
        walk_speed = Mathf.Lerp(walk_speed, walk_percentage_target * max_speed, 10f * Time.deltaTime);

        // 2 - NOT MOVING CHECKS
        if (capable.HasEffect(Effect.Immobile)) { walk_speed = 0f; } // we check if immobile
        if (capable is Being being && !being.Alive) { walk_speed = 0f; } // we check if dead
        if (walk_speed < 0.1f) { walk_speed = 0f; } // we clamp if walk is too low

        // log final walk_speed
        if (debug) { Debug.Log("WalkCapacity: " + capable.name + " walk_speed is " + walk_speed); }

        // 3 - HANDLING ANIMATION
        if (walk_speed == 0f)
        {
            capable.anim_player.StopPlaying("walk");
            return;
        }
        // we play the anim if it's not already playing
        if (capable.anim_player.current_capacity != "walk")
        {
            capable.anim_player.Play("walk");
        }
    }

    private void update_particles()
    {
        if (walk_particles == null) { return; } // if no particles, we return

        // we enable the particles emission if walk_speed > 0
        walk_particles_emission.enabled = walk_speed > 0f;

        // we set the rate of the particles emission based on the walk speed
        walk_particles_emission.rateOverTime = base_walk_particles_rate * (walk_speed / max_speed);
    }
    private void update_sound()
    {
        // update walk_sound speed with our velocity
        walk_sound.setParameterByName("speed", walk_speed / max_speed);

        if (((Movable)capable).Velocity.magnitude > 0.1f)
        {
            // check if the sound is playing
            PLAYBACK_STATE walk_state;
            walk_sound.getPlaybackState(out walk_state);
            if (!walk_state.Equals(PLAYBACK_STATE.PLAYING)) { walk_sound.start(); }
            return;
        }

        walk_sound.stop(STOP_MODE.ALLOWFADEOUT);
    }
}