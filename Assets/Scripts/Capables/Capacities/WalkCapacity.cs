using UnityEngine;

public class WalkCapacity : Capacity
{

    [Header("WALK CAPACITY")]
    public float walk_speed = 0f; // si c'est à 0 on marche pas, sinon on marche en conséquence !

    [Header("Walk Inputs")]
    public float walk_percentage_target = 0f; // walk_speed lerp continuellement jusqu'à walk_percentage_target*max_speed
    public float max_speed = 3f; // vitesse maximale de déplacement, atteinte quand walk_speed = 1f
    [SerializeField] private float random_speed_modifier_at_start = 0; // max_speed += random.range(-5,5) in the start method if this modifier = 5

    // START
    private void Start()
    {
        max_speed += Random.Range(-random_speed_modifier_at_start, random_speed_modifier_at_start);    
    }

    // FIXED UPDATE
    protected override void Update()
    {
        base.Update();
        
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

}