using System;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class WalkCapacity : Capacity
{

    private WalkData wdata => data as WalkData;


    public bool log_footsteps_audio = false;
    public bool log_footsteps_repeating = false;

    [Header("WALK CAPACITY")]
    public float speed = 0f; // si c'est à 0 on marche pas, sinon on marche en conséquence !

    [Header("Walk Inputs")]
    public float walk_percentage_target = 0f; // walk_speed lerp continuellement jusqu'à walk_percentage_target*max_speed
    public float max_walk_speed = 3f; // vitesse maximale de déplacement, atteinte quand walk_speed = 1f
    public float max_run_speed = 3f; // vitesse maximale de déplacement, atteinte quand walk_speed = 1f
    [SerializeField] private float random_speed_modifier_at_start = 0; // max_speed += random.range(-5,5) in the start method if this modifier = 5

    [Header("Walk Particles")]
    private ParticleSystem _walk_particles; // particles to play when walking
    private ParticleSystem walk_particles
    {
        get
        {
            if (_walk_particles == null) { _walk_particles = GetComponent<ParticleSystem>(); }
            return _walk_particles;
        }
    }
    private ParticleSystem.EmissionModule walk_particles_emission; // emission module of the particles
    private float base_walk_particles_rate = 10f; // base rate of the particles emission

    [Header("Walk sound instance")]
    public float delay_between_footsteps = 0.5f;
    private bool is_playing_footsteps = false;
    public EventInstance walk_sound;

    private void Awake()
    {
        if (walk_particles == null) { return; }
        walk_particles_emission = walk_particles.emission;
        walk_particles_emission.enabled = false; // we disable the particles by default
        base_walk_particles_rate = walk_particles_emission.rateOverTime.constant; // we get the base rate of the particles emission
    }

    // START
    /* private void Start()
    {
        // max_speed += UnityEngine.Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start);

        // we get a walk sound instance from the audio engine
        walk_sound = AudioEngine.Instance.CreateInstance("walk", Capable);
    } */

    // UPDATE
    protected void Update()
    {
        
        // update particles
        update_particles();

        // 1 - SPEED CALCULATION
        float target_speed = walk_percentage_target * (is_running ? max_run_speed : max_walk_speed); // we calculate the target speed based on the walk percentage and the max speed (walk or run)
        speed = Mathf.Lerp(speed, target_speed, 10f * Time.deltaTime);

        // 2 - NOT MOVING CHECKS
        if (Capable.HasEffect(Effect.Immobile)) { speed = 0f; } // we check if immobile
        if (Capable.TryGetCapacity(out HealthCapacity being) && !being.Alive) { speed = 0f; } // we check if dead
        if (speed < 0.1f) { speed = 0f; } // we clamp if walk is too low

        // log final walk_speed
        if (log) { Debug.Log("(WalkCapacity) " + Capable.data.id + " speed is " + speed); }

        // 3 - HANDLING ANIMATION & SOUND

        // update sound
        update_sound_parameters();

        if (speed == 0f)
        {
            Capable.AnimPlayer.StopPlaying("walk");
            Capable.AnimPlayer.StopPlaying("run");

            // then we want to stop walking
            if (is_playing_footsteps) { stop_footsteps(); }
            return;
        }

        // else we have a walk_speed, we want to enable walk animation if not playing
        string anim_to_play = is_running ? "run" : "walk";
        if (!Capable.AnimPlayer.IsPlaying(anim_to_play)) { Capable.AnimPlayer.Play(anim_to_play); }
        // will automatically switch between walk & run (needs to be set in the same ACP priority)

        // and play the footsteps if not playing
        if (!is_playing_footsteps) { play_footsteps(); }
    }

    private void update_particles()
    {
        if (walk_particles == null) { return; } // if no particles, we return

        // we enable the particles emission if walk_speed > 0
        walk_particles_emission.enabled = speed > 0f;

        // we set the rate of the particles emission based on the walk speed
        walk_particles_emission.rateOverTime = base_walk_particles_rate * (speed / max_run_speed);
    }
    private void update_sound_parameters()
    {
        // update walk_sound speed with our velocity
        walk_sound.setParameterByName("speed", speed / max_run_speed);
    }

    // FOOTSTEPS
    private void play_footsteps()
    {
        CancelInvoke("play_one_footstep");
        if (log_footsteps_audio) { Debug.Log($"(WalkCapacity - log_footsteps) playing footsteps for '{Capable.name}'"); }
        is_playing_footsteps = true;
        InvokeRepeating("play_one_footstep", 0f, delay_between_footsteps);
    }
    private void stop_footsteps()
    {
        CancelInvoke("play_one_footstep");
        if (log_footsteps_audio) { Debug.Log($"(WalkCapacity - log_footsteps) stopping footsteps for '{Capable.name}'"); }
        walk_sound.stop(STOP_MODE.ALLOWFADEOUT);
        is_playing_footsteps = false;
    }
    private void play_one_footstep()
    {
        if (log_footsteps_repeating) { Debug.Log($"(WalkCapacity - log_footsteps_repeating) footstep '{Capable.name}'"); }
        walk_sound.start();
    }


    // RUNNING
    private bool is_running = false;
    public void EnableRun()
    {
        // si on court déjà on return
        if (is_running) { return; }
        
        // on met à jour max_speed avec la vitesse de run
        // max_run_speed = wdata.max_run_speed;
        is_running = true;

        // on applique le changement au son
        if (walk_sound.isValid()) { walk_sound.setParameterByName("running", 1); }
        if (log) { Debug.Log("(WalkCapacity) " + Capable.data.id + $" is now running (up to {max_run_speed})"); }
    }
    public void DisableRun()
    {
        // si on court déjà on return
        if (!is_running) { return; }

        // on met à jour max_speed avec la vitesse de walk
        // max_run_speed = wdata.max_walk_speed;
        is_running = false;

        // on applique le changement au son
        if (walk_sound.isValid()) { walk_sound.setParameterByName("running", 0); }
        if (log) { Debug.Log("(WalkCapacity) " + Capable.data.id + $" is now walking (up to {max_walk_speed})"); }
    }




    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        if (data is not WalkData wdata) { return; }

        // load walk parameters
        walk_percentage_target = wdata.walk_percentage_target;
        max_walk_speed = wdata.max_walk_speed;
        max_run_speed = wdata.max_run_speed;

        // load particles parameters
        if (walk_particles != null)
        {
            walk_particles_emission.rateOverTime = wdata.particles_rate;
            var main = walk_particles.main;
            main.startColor = wdata.particles_color;
        }

        // reset walk speed
        speed = 0f;

        base.LoadData(data);

        // reset audio
        if (walk_sound.isValid())
        {
            walk_sound.stop(STOP_MODE.IMMEDIATE);
            walk_sound.release();
        }
        walk_sound = AudioEngine.Instance.CreateInstanceFromHolderID("walk", data.owner_id); // we get a walk sound instance from the audio engine

        // apply run state
        is_running = !wdata.is_running; // we set the opposite so we can call the enable/disable run methods
        if (wdata.is_running) { EnableRun(); } else { DisableRun(); } // will handle the sound parameters accordingly
    }
    public override void UnloadData()
    {
        // reset audio
        if (walk_sound.isValid())
        {
            walk_sound.stop(STOP_MODE.IMMEDIATE);
            walk_sound.release();
        }

        if (walk_particles != null)
        {
            walk_particles_emission.enabled = false;
        }

        // this saves the dynamic health data
        base.UnloadData();
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        // update the dynamic fields data with the current values of the capacity
        if (this.data == null) { return; }
        if (this.data is not WalkData wdata) { return; }
        wdata.walk_percentage_target = this.walk_percentage_target;
        wdata.is_running = this.is_running;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        WalkData static_data = new WalkData(base.GetStaticData())
        {
            random_speed_modifier_at_start = this.random_speed_modifier_at_start,
            template_max_walk_speed = this.max_walk_speed,
            template_max_run_speed = this.max_run_speed,
            walk_percentage_target = 0f,
            is_running = false,
        };

        // we calculate random modifiers for the speeds
        static_data.max_walk_speed = static_data.template_max_walk_speed + UnityEngine.Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start);
        static_data.max_run_speed = static_data.template_max_run_speed + UnityEngine.Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start);

        // we set particles parameters
        if (walk_particles != null)
        {
            static_data.particles_color = walk_particles.main.startColor.color;
            static_data.particles_rate = walk_particles.emission.rateOverTime.constant;
        }
        else
        {
            static_data.particles_color = new Color(0, 0, 0, 0);
            static_data.particles_rate = 0f;
        }

        return static_data;
    }
}

[Serializable] public class WalkData : CapacityData
{
    // template walk parameters
    public float template_max_walk_speed = 3f;
    public float template_max_run_speed = 3f;
    public float random_speed_modifier_at_start = 0.5f;
    public Color particles_color = Color.white;
    public float particles_rate = 1f;


    // instance walk parameters
    public float max_walk_speed = 3f;
    public float max_run_speed = 3f;
    public float walk_percentage_target = 0f;
    public bool is_running = false;
    


    // CONSTRUCTOR
    public WalkData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new WalkData(base.Duplicate() as CapacityData)
        {
            template_max_walk_speed = this.template_max_walk_speed,
            template_max_run_speed = this.template_max_run_speed,
            random_speed_modifier_at_start = this.random_speed_modifier_at_start,
            max_walk_speed = template_max_walk_speed + UnityEngine.Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start),
            max_run_speed = template_max_run_speed   + UnityEngine.Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start),
            is_running = this.is_running,
            walk_percentage_target = this.walk_percentage_target,

            // particles
            particles_color = this.particles_color,
            particles_rate = this.particles_rate
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - template_max_walk_speed : {template_max_walk_speed}\n";
        details += $"  - template_max_run_speed : {template_max_run_speed}\n";
        details += $"  - random_speed_modifier_at_start : {random_speed_modifier_at_start}\n";
        details += $"  - max_walk_speed : {max_walk_speed}\n";
        details += $"  - max_run_speed : {max_run_speed}\n";
        details += $"  - walk_percentage_target : {walk_percentage_target}\n";
        details += $"  - is_running : {is_running}\n";
        details += $"  - particles_color : {particles_color}\n";
        details += $"  - particles_rate : {particles_rate}\n";
        return base.GetDetails() + details;
    }
}