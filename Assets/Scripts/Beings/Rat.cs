using UnityEngine;

public class Rat : Being
{

    [SerializeField] private float cooldown_sprint = 4f; // temps entre chaque déplacement
    [SerializeField] private float cooldown_waiting = 10f; // temps entre chaque déplacement
    [SerializeField] private string state = "idle"; // état du rat (idle, sprint, little_movement)
    [SerializeField] private float current_state_left_time = 0f; // temps depuis le début de l'état
    [SerializeField] private Vector2 destination = new Vector2(0, 0); // destination du rat


    // unity functions

    new void Start()
    {

        // on start de d'habitude
        base.Start();

        // on met à jour les différentes variables d'attaques pour le rat
        max_vie = 5 + Random.Range(-3, 3);
        vie = (float)max_vie;
        speed = 3f + Random.Range(-0.5f, 0.5f);
        running_speed = 4f;
        // damage = 5f + Random.Range(-2f, 2f);
        // attack_range = 0.15f + Random.Range(-0.05f, 0.05f);
        // damage_range = 0.15f + Random.Range(-0.05f, 0.05f);
        weight = 0.3f + Random.Range(-0.05f, 0.05f);

        // on met les bonnes animations
        anims.init("rat");
    }

    public override void Events()
    {
        base.Events();

        // walk
        if (hasCapacity("walk"))
        {
            inputs = rat_movement();
        }
    }

    // MOVEMENT
    protected Vector2 rat_movement()
    {
        // simule des inputs de rat.

        // attend longtemps sans rien faire
        // puis tape des grosses sprints

        if (state == "idle")
        {
            // on attend
            current_state_left_time -= Time.deltaTime;
            if (current_state_left_time <= 0)
            {
                // on passe à l'état sprint
                state = "sprint";
                destination = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)) + (Vector2)transform.position;
                current_state_left_time = cooldown_sprint + Random.Range(-2f, 2f);
            }

            // on ne bouge pas
            /* if (Random.Range(0, 100) <= 2)
            {
                // on fait un petit mouvement aléatoire
                return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            } */
            return new Vector2(0, 0);
        }
        else if (state == "sprint")
        {
            // on sprint
            current_state_left_time -= Time.deltaTime;
            if (current_state_left_time <= 0 || Mathf.Abs(destination.x - transform.position.x) < 0.1f || Mathf.Abs(destination.y - transform.position.y) < 0.1f)
            {
                // on passe à l'état idle
                state = "idle";
                current_state_left_time = cooldown_waiting + Random.Range(-2f, 2f);
            }
            else
            {
                // on sprint en direction de la destination
                return (destination - (Vector2)transform.position).normalized;
            }
        }

        return new Vector2(0, 0);
    }

}