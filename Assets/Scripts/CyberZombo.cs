using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberZombo : Attacker
{

    // player detection
    private float detection_radius = 10f;
    public bool player_detected = false;
    GameObject player;

    // player attacking
    // public float attack_radius = 1f; // attaque automatiquement si le joueur est dans ce rayon

    /*


    */

    // unity functions
    new void Start(){

        // on récupère le joueur
        player = GameObject.Find("/perso");

        // on start de d'habitude
        base.Start();

        // on donne une vitesse de déplacement
        vitesse = 1f;

        // on défini les layers des ennemis
        enemy_layers = LayerMask.GetMask("Player");

        // on met à jour les différentes variables d'attaques pour le zombo
        damage = 10f;
        attack_range = 0.25f;
        damage_range = 0.25f;
    }

    public override void Events()
    {
        if (player_detected){
            // on se dirige vers le joueur
            inputs = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.y - transform.position.y);
            inputs.Normalize();
        }
        else{
            // on fait un mouvement circulaire sur x
            inputs = simulate_circular_input_on_x(inputs);
        }
    }

    // update de d'habitude
    new void Update()
    {

        // on essaye de détecter le joueur
        player_detected = detect_player(detection_radius);

        // update de d'habitude
        base.Update();

    }

    // DETECTION DU JOUEUR

    private bool detect_player(float radius){

        // on récupère la distance entre le joueur et le zombo
        float distance = Vector2.Distance(player.transform.position, transform.position);

        // on regarde si le joueur est dans le cercle d'attaque + une marge
        float marge = Random.Range(-damage_range-0.2f, damage_range+0.2f);
        if (distance < attack_range + marge)
        {
            attack();
        }

        // on regarde si le joueur est dans le rayon de détection
        if (distance < radius){
            return true;
        }
        else{
            return false;
        }

    }

}
