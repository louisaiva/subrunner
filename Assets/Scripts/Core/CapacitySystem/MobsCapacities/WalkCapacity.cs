using FMOD.Studio;
using UnityEngine;

public class WalkCapacity : Capacity
{
    public bool log_footsteps_audio = false;
    public bool log_footsteps_repeating = false;


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
    public float delay_between_footsteps = 0.5f;
    private bool is_playing_footsteps = false;
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
        walk_sound = AudioEngine.Instance.CreateInstance("walk", capable);
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // update particles
        update_particles();

        // 1 - WALK_SPEED CALCULATION
        walk_speed = Mathf.Lerp(walk_speed, walk_percentage_target * max_speed, 10f * Time.deltaTime);

        // 2 - NOT MOVING CHECKS
        if (capable.HasEffect(Effect.Immobile)) { walk_speed = 0f; } // we check if immobile
        if (capable is Being being && !being.Alive) { walk_speed = 0f; } // we check if dead
        if (walk_speed < 0.1f) { walk_speed = 0f; } // we clamp if walk is too low

        // log final walk_speed
        if (debug) { Debug.Log("WalkCapacity: " + capable.name + " walk_speed is " + walk_speed); }

        // 3 - HANDLING ANIMATION & SOUND

        // update sound
        update_sound_parameters();

        if (walk_speed == 0f)
        {
            if (capable.anim_player.IsPlaying("walk")) { capable.anim_player.StopPlaying("walk"); }

            // then we want to stop walking
            if (is_playing_footsteps) { stop_footsteps(); }
            return;
        }

        // else we have a walk_speed, we want to enable walk animation if not playing
        if (!capable.anim_player.IsPlaying("walk")) { capable.anim_player.Play("walk"); }

        // and play the footsteps if not playing
        if (!is_playing_footsteps) { play_footsteps(); }
    }

    private void update_particles()
    {
        if (walk_particles == null) { return; } // if no particles, we return

        // we enable the particles emission if walk_speed > 0
        walk_particles_emission.enabled = walk_speed > 0f;

        // we set the rate of the particles emission based on the walk speed
        walk_particles_emission.rateOverTime = base_walk_particles_rate * (walk_speed / max_speed);
    }
    private void update_sound_parameters()
    {
        // update walk_sound speed with our velocity
        walk_sound.setParameterByName("speed", walk_speed / max_speed);

        /* if (((Movable)capable).Velocity.magnitude > 0.1f)
        {
            // check if the sound is playing
            PLAYBACK_STATE walk_state;
            walk_sound.getPlaybackState(out walk_state);
            if (!walk_state.Equals(PLAYBACK_STATE.PLAYING)) { walk_sound.start(); }
            return;
        }

        walk_sound.stop(STOP_MODE.ALLOWFADEOUT); */
    }

    // FOOTSTEPS
    private void play_footsteps()
    {
        CancelInvoke("play_one_footstep");
        if (log_footsteps_audio) { Debug.Log($"(WalkCapacity - log_footsteps) playing footsteps for '{capable.name}'"); }
        is_playing_footsteps = true;
        InvokeRepeating("play_one_footstep", 0f, delay_between_footsteps);
    }
    private void stop_footsteps()
    {
        CancelInvoke("play_one_footstep");
        if (log_footsteps_audio) { Debug.Log($"(WalkCapacity - log_footsteps) stopping footsteps for '{capable.name}'"); }
        walk_sound.stop(STOP_MODE.ALLOWFADEOUT);
        is_playing_footsteps = false;
    }
    private void play_one_footstep()
    {
        if (log_footsteps_repeating) { Debug.Log($"(WalkCapacity - log_footsteps_repeating) footstep '{capable.name}'"); }
        walk_sound.start();
    }
}