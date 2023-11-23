using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberZombo : Attacker
{

    // player detection
    private float detection_radius = 10f;
    public bool target_detected = false;
    GameObject target;

    // player attacking
    // public float attack_radius = 1f; // attaque automatiquement si le joueur est dans ce rayon

    /*


    */

    // unity functions
    new void Start(){

        // on récupère le joueur
        // tar = GameObject.Find("/perso");

        // on start de d'habitude
        base.Start();

        // on défini les layers des ennemis
        enemy_layers = LayerMask.GetMask("Player");

        // on met à jour les différentes variables d'attaques pour le zombo
        max_vie = 25;
        vie = (float) max_vie;
        vitesse = 1f;
        damage = 20f;
        attack_range = 0.25f;
        damage_range = 0.25f;
        cooldown_attack = 0.6f;
        weight = 1.4f + Random.Range(-0.2f, 0.2f);

        // on met les bonnes animations
        anims.init("zombo");
    }

    public override void Events()
    {
        // treshold distance
        float treshold_distance = 0.1f;

        if (target_detected){
            // on se dirige vers le joueur
            inputs = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);

            // on regarde si on est pas TROP proche du joueur
            if (inputs.magnitude < treshold_distance){
                inputs = new Vector2(0, 0);
            }

            // on normalise les inputs
            inputs.Normalize();
        }
        else{
            // on fait un mouvement circulaire sur x
            inputs = simulate_circular_input_on_x(inputs);
            // inputs = new Vector2(0, 0);
        }
    }

    // update de d'habitude
    new void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        // on essaye de détecter le joueur
        detect_target(detection_radius);

        // update de d'habitude
        base.Update();

        print("target detected : " + target_detected);

        // on essaye d'attaquer le joueur si on le détecte
        if (target_detected){

            print(target);
            print("target is alive : " + target.GetComponent<Perso>().isAlive());

            if (target.GetComponent<Being>().isAlive())
            {

                try_to_attack_target();

            }
        }

    }

    // DETECTION DU JOUEUR

    private void detect_target(float radius){

        // on essaie de trouver le premier ennemi dans le rayon de détection
        Collider2D targetCollider = Physics2D.OverlapCircle(transform.position, radius, enemy_layers);
        target_detected = (targetCollider != null);
        if (target_detected){
            target = targetCollider.gameObject;
            print("assigned target " + target.name);
        }
        else{
            target = null;
        }

    }

    private void try_to_attack_target(){

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
