using UnityEngine;

public class WalkCapacity : Capacity
{

    [Header("WALK CAPACITY")]
    public float walk_speed = 0f; // si c'est à 0 on marche pas, sinon on marche en conséquence !

    [Header("Walk Inputs")]
    public float walk_percentage_target = 0f; // walk percentage lerp continuellement jusqu'à walk_percentage_target*max_speed
    public float max_speed = 3f; // vitesse maximale de déplacement, atteinte quand walk_percentage = 1f

    // FIXED UPDATE
    protected override void Update()
    {
        base.Update();

        // we lerp the speed to the target
        walk_speed = Mathf.Lerp(walk_speed, walk_percentage_target * max_speed, 10f * Time.deltaTime);

        // we check if immobile
        if (capable.HasEffect(Effect.Immobile)) { walk_speed = 0f; }

        // we check if dead
        if (capable is Being being && !being.Alive) { walk_speed = 0f; }

        // we clamp if walk is too low
        if (walk_speed < 0.1f) { walk_speed = 0f; }

        if (debug) { Debug.Log("WalkCapacity: " + capable.name + " walk_speed is " + walk_speed); }

        // we handle the anim
        if (walk_percentage_target == 0f)
        {
            capable.anim_player.StopPlaying("walk"); // we stop the anim
        }
        else
        {
            capable.anim_player.Play("walk"); // we play the anim
        }
    }

    // DEPLACEMENT
    /* protected void run(Vector2 direction, float inputs_magnitude = 1f)
    {
        // on calcule le mouvement sur X
        // float x_movement = direction.normalized.x * running_speed * Time.deltaTime * inputs_magnitude;

        // on calcule le mouvement sur Y
        // float y_movement = direction.normalized.y * running_speed * Time.deltaTime * inputs_magnitude;

        // on applique le mouvement au perso
        // move(new Vector2(x_movement, y_movement));
        input_speed = Mathf.Lerp(input_speed, running_speed * inputs_magnitude, 10f * Time.deltaTime);
        capable.anim_player.Play("run");
    } */

}