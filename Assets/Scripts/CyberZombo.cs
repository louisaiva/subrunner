using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberZombo : Attacker, I_Hackable
{

    // player detection
    private float player_detection_radius = 3f;
    public bool target_detected = false;
    GameObject target;

    // meat detection
    private float meat_detection_radius = 10f;
    public bool meat_detected = false;
    GameObject meat_target;
    private LayerMask meat_layers;

    // player attacking
    // public float attack_radius = 1f; // attaque automatiquement si le joueur est dans ce rayon

    // hackin
    public int required_hack_lvl { get; set;}
    public string hack_type_self { get; set;}
    public bool is_getting_hacked { get; set; }
    public float hacking_duration_base { get; set;}
    public float hacking_end_time { get; set;}


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
        meat_layers = LayerMask.GetMask("Meat");

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

        // on initialise le hackin
        initHack();
    }

    public override void Events()
    {
        // treshold distance "trop proche"
        float treshold_distance = 0.1f;


        // 1 - on essaye de détecter le joueur
        if (target_detected){
            // on se dirige vers le joueur
            inputs = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);

            // on regarde si on est pas TROP proche du joueur
            if (inputs.magnitude < treshold_distance){
                inputs = new Vector2(0, 0);
            }

            // on normalise les inputs
            inputs.Normalize();

            return;
        }
        
        // 2 - on essaye de détecter de la viande
        if (meat_detected){
            // on se dirige vers la viande
            inputs = new Vector2(meat_target.transform.position.x - transform.position.x, meat_target.transform.position.y - transform.position.y);

            // on regarde si on est pas TROP proche de la viande
            if (inputs.magnitude < treshold_distance){
                inputs = new Vector2(0, 0);
            }

            // on normalise les inputs
            inputs.Normalize();

            return;
        }

        // 3 - on se déplace aléatoirement circulairement en x
        inputs = simulate_circular_input_on_x(inputs);

    }

    // update de d'habitude
    new void Update()
    {
        // ! à mettre tjrs au début de la fonction update
        if (!isAlive()) { return; }

        // update des hacks
        updateHack();

        // on essaye de détecter le joueur
        detect_target(player_detection_radius);

        // on essaye de détecter de la viande
        detect_meat(meat_detection_radius);

        // update de d'habitude
        base.Update();


        // on essaye d'attaquer le joueur si on le détecte
        if (target_detected){
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

    // MEAT DETECTION

    private void detect_meat(float radius){

        // on essaie de trouver le premier gameobject meat dans le rayon de détection
        Collider2D meatCollider = Physics2D.OverlapCircle(transform.position, radius, meat_layers);
        meat_detected = (meatCollider != null);
        if (meat_detected){
            meat_target = meatCollider.gameObject;
        }
        else{
            meat_target = null;
        }
    }
    

    // HACKIN
    public void initHack(){
        required_hack_lvl = 1;
        hack_type_self = "zombo";
        is_getting_hacked = false;
        hacking_duration_base = 3f;
        hacking_end_time = -1;
    }

    bool I_Hackable.beHacked(int lvl){
        // on regarde si on est déjà en train de se faire hacker
        if (is_getting_hacked) { return false; }

        // on regarde si le hack est assez puissant
        if (lvl < required_hack_lvl) { return false; }

        // on calcule la durée du hack
        // chaque niveau de hack en plus réduit le temps de hack de 5% par rapport au niveau précédent
        // fonction qui tend vers 0 quand le niveau de hack augmente MAIS qui ne peut pas être négative
        // * FONCTIONNE JUSQU'AU NIVEAU 60 !! c super

        float hackin_duration = hacking_duration_base * Mathf.Pow(0.95f, lvl - required_hack_lvl);
        float hackin_speed = hacking_duration_base / hackin_duration;
        if (hackin_duration < 0.1f) { hackin_duration = 0.1f; }

        // on commence le hack
        is_getting_hacked = true;
        hacking_end_time = Time.time + hackin_duration;

        // on lance l'animation de hack
        anim_handler.ChangeAnimTilEnd(anims.hurted, hackin_speed);

        return true;
    }

    bool I_Hackable.isGettingHacked(){
        return is_getting_hacked;
    }

    public bool isHackable(string hack_type, int lvl = 1000)
    {
        // on regarde si on a le bon type de hack
        if (hack_type != hack_type_self) { return false; }

        // on regarde si on a le bon niveau de hack
        if (lvl < required_hack_lvl) { return false; }

        return true;
    }

    public void updateHack()
    {
        // on met à jour le hackin
        if (is_getting_hacked)
        {
            // on regarde si on a fini le hack
            if (Time.time > hacking_end_time)
            {
                is_getting_hacked = false;
                hacking_end_time = -1;
            }
        }
    }

    public void cancelHack()
    {
        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
    }
    // DIE

    protected override void die(){
        // on arrête le hackin
        is_getting_hacked = false;
        hacking_end_time = -1;

        // on meurt
        base.die();
    }

}
