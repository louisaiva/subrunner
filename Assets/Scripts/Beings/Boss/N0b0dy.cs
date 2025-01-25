using UnityEngine;

public class N0b0dy : Attacker
{


    // player detection
    private float player_detection_radius = 3f;
    public bool target_detected = false;
    GameObject target;

    new void Start()
    {
        base.Start();
        anims = new N0b0dyAnims();
        anims.init("n0b0dy");
        // addCapacity("life_regen");
    }


    public override void Events()
    {


        // on vérifie qu'on est pas KO
        if (hasCapacity("knocked_out")) { return; }

        // life regen
        if (hasCapacity("life_regen"))
        {
            if (vie < max_vie)
            {
                vie += regen_vie * Time.deltaTime;
            }
        }

        // treshold distance "trop proche"
        float treshold_distance = 0.1f;


        // 1 - on essaye de détecter le joueur
        if (target_detected)
        {
            // on se dirige vers le joueur
            inputs = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);

            // on regarde si on est pas TROP proche du joueur
            if (inputs.magnitude < treshold_distance)
            {
                inputs = new Vector2(0, 0);
            }

            // on normalise les inputs
            inputs.Normalize();

            return;
        }

        // 2 - on fume notre clope oklm
        inputs = new Vector2(0, 0);

        // 3 - on se déplace aléatoirement circulairement en x
        // inputs = simulate_circular_input_on_x(inputs);

    }

    // update de d'habitude
    new void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        // on essaye de détecter le joueur
        detect_target(player_detection_radius);

        // update de d'habitude
        base.Update();


        // on essaye d'attaquer le joueur si on le détecte
        if (hasCapacity("hit"))
        {
            if (target_detected)
            {
                if (target.GetComponent<Being>().isAlive())
                {
                    try_to_attack_target();
                }
            }
        }

    }

    // DETECTION DU JOUEUR

    private void detect_target(float radius)
    {

        // on essaie de trouver le premier ennemi dans le rayon de détection
        Collider2D targetCollider = Physics2D.OverlapCircle(transform.position, radius, enemy_layers);
        target_detected = (targetCollider != null);
        if (target_detected)
        {
            target = targetCollider.gameObject;
        }
        else
        {
            target = null;
        }

    }

    private void try_to_attack_target()
    {

        // on recupère la distance entre le zombo et le joueur
        float distance = Vector2.Distance(transform.position, target.transform.position);

        // on regarde si la target est dans le cercle d'attaque + une marge
        float marge = Random.Range(-damage_range - 0.2f, damage_range + 0.2f);
        if (distance < attack_range + marge)
        {
            attack();
        }


    }

}


public class N0b0dyAnims : AttackerAnims
{
    public override void init(string name)
    {
        hurted = "hurted_LR";
        die = "dead";
        run_side = "run_LR";
        idle_side = "idle_LR";
        attack = "attack_LR";
        base.init(name);
    }
}