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

    // HACKING

    public string hack_type_self { get; set; }
    public int required_bits { get; set; }
    public int required_bits_base { get; set; }
    public int security_lvl { get; set; }
    public bool is_getting_hacked { get; set; }
    public float hacking_duration_base { get; set; }
    public float hacking_end_time { get; set; }
    public float hacking_current_duration { get; set; } // temps de hack actuel

    // xp_provider
    public GameObject bit_provider { get; set; }

    // hackable outline
    public Material outline_material { get; set; }
    public Material default_material { get; set; }

    // HackUI
    public HackUI hack_ui { get; set; }

    /*


    */

    // unity functions
    new void Start(){

        // on récupère le joueur
        // tar = GameObject.Find("/perso");
        cooldown_attack = 0.6f;

        // on start de d'habitude
        base.Start();

        // on défini les layers des ennemis
        enemy_layers = LayerMask.GetMask("Player","Beings");
        meat_layers = LayerMask.GetMask("Meat","Beings");

        // on met à jour les différentes variables d'attaques pour le zombo
        max_vie = 25 + Random.Range(-5, 5);
        vie = (float) max_vie;
        speed = 1f + Random.Range(-0.2f, 0.2f);
        running_speed = 2f;
        damage = 20f + Random.Range(-5f, 5f);
        attack_range = 0.25f + Random.Range(-0.05f, 0.05f);
        damage_range = 0.25f + Random.Range(-0.05f, 0.05f);
        weight = 1.4f + Random.Range(-0.2f, 0.2f);

        // on met les bonnes animations
        anims = new AttackerAnims();
        anims.init("zombo");

        // on met les bons sons
        sounds = new ZomboSounds();
        // audio_manager.LoadSoundsFromPath("audio/zombo");


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
        if (is_getting_hacked) {
            updateHack();
        }

        // on essaye de détecter le joueur
        detect_target(player_detection_radius);

        // on essaye de détecter de la viande
        detect_meat(meat_detection_radius);

        // update de d'habitude
        base.Update();


        // on essaye d'attaquer le joueur si on le détecte
        if(hasCapacity("hit"))
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
    public void initHack()
    {

        // on récupère le xp_provider
        bit_provider = GameObject.Find("/particles/xp_provider");

        // on initialise le hackin
        hack_type_self = "zombo";
        is_getting_hacked = false;
        hacking_duration_base = 3f;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // bits necessaires pour hacker
        security_lvl = 1;
        required_bits_base = 1;
        required_bits = (int) (required_bits_base * Mathf.Pow(2, security_lvl - 1));

        // on met à jour le material
        default_material = GetComponent<SpriteRenderer>().material;
        outline_material = Resources.Load<Material>("materials/targeted/hack_enemy");

        // on récupère le hack_ui
        hack_ui = transform.Find("hack_ui").GetComponent<HackUI>();
    }

    public bool isHackable(string hack_type, int bits)
    {
        // on met à jour HackUI
        hack_ui.setMode("unhackable");

        // on regarde si on est pas mort
        if (!isAlive()) { return false; }

        // on regarde si on a le bon type de hack
        if (hack_type != hack_type_self) { return false; }

        // on regarde si on a assez de bits
        if (bits < required_bits) { return false; }

        // on met à jour HackUI
        hack_ui.setMode("hackable");

        return true;
    }

    int I_Hackable.beHacked()
    {
        // on regarde si on est déjà en train de se faire hacker
        if (is_getting_hacked) { return 0; }

        // on calcule la durée du hack
        // todo voir Computer.cs

        hacking_current_duration = hacking_duration_base;
        if (hacking_current_duration < 0.1f) { hacking_current_duration = 0.1f; }

        // on met à jour les animations
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(anims.hurted, hacking_current_duration);

        // on commence le hack
        is_getting_hacked = true;
        hacking_end_time = Time.time + hacking_current_duration;

        // on met à jour HackUI
        hack_ui.setMode("hacked");


        return required_bits;
    }

    bool I_Hackable.isGettingHacked(){
        return is_getting_hacked;
    }

    public void updateHack()
    {
        // on regarde si on a fini le hack
        if (Time.time > hacking_end_time)
        {
            // on reussit le hack
            succeedHack();
        }
    }

    public void cancelHack()
    {
        // on calcule le temps restant
        float time_left = hacking_end_time - Time.time;

        // on calcule le nombre de bits restants et on en drop la moitié
        int bits_left = Mathf.RoundToInt((required_bits * time_left / hacking_duration_base) / 2);

        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on met à jour HackUI
        hack_ui.setMode("unhackable");

        // on drop les bits restants
        xp_provider.GetComponent<XPProvider>().EmitBits(bits_left, transform.position, 0.5f);

        // on met à jour les animations
        anim_handler.StopForcing();
    }

    public void succeedHack(){

        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on met à jour HackUI
        hack_ui.setMode("unhackable");
    }

    // outlines
    public void outlineMe()
    {
        // on change le material
        GetComponent<SpriteRenderer>().material = outline_material;
        // print(gameObject.name + " just got outlined");
    }
    public void unOutlineMe()
    {
        // on change le material
        GetComponent<SpriteRenderer>().material = default_material;
    }

    // HackUI
    public void showHackUI()
    {
        // on le montre
        // hack_ui.show();
    }
    public void hideHackUI()
    {

        // on le montre
        // hack_ui.hide();
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


public class ZomboSounds : AttackerSounds
{
    public ZomboSounds()
    {
        s_hurted = new List<string>() { "hurted - 1", "hurted - 2", "hurted - 3", "hurted - 4" };
        s_idle = new List<string>() { "idle - 1", "idle - 2", "idle - 3"};
        base.init("zombo");
    }
}