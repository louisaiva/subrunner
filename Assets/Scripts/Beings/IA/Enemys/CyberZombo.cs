using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyberZombo : Enemy /*, I_Hackable */
{

    // meat detection
    [Header("Meat Detection")]
    [SerializeField] private bool meat_detected = false;
    [SerializeField] private LayerMask meat_layers;
    [SerializeField] private float meat_detection_radius = 20f;
    [SerializeField] private GameObject meat_target;

    // HACKING
    /* public string hack_type_self { get; set; }
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
    public HackUI hack_ui { get; set; } */

    // unity functions
    protected override void Start()
    {

        // on start de d'habitude
        base.Start();

        // on défini les layers des ennemis
        // target_layers = LayerMask.GetMask("Beings");
        // meat_layers = LayerMask.GetMask("Meat");

        // on met à jour les différentes variables d'attaques pour le zombo
        max_life = 25 + Random.Range(-5, 5);
        life = (float) max_life;

        // change speed
        GetCapacity<WalkCapacity>().max_speed = 1f + Random.Range(-0.2f, 0.2f);

        // change attack
        GetCapacity<AttackCapacity>().damage = 20f + Random.Range(-5f, 5f);
        weight = 1.4f + Random.Range(-0.2f, 0.2f);


        // on initialise le hackin
        // initHack();
    }

    // update de d'habitude
    /* protected override void Update()
    {
        // update des hacks
        /* if (is_getting_hacked) {
            updateHack();
        } 


        // update de d'habitude
        base.Update();

        // on vérifie si on a déjà une destination alors rien besoin de faire plus
        if (has_destination) { return; }

        // sinon on essaye de détecter de la viande
        detect_meat(meat_detection_radius);

        // si on en detecte on y va sinon on bouge pas
        if (meat_detected)
        {
            GoTo(meat_target.transform.position); // on se dirige vers la viande
        }
    } */

    protected override void IdleBehaviour()
    {
        // on essaye de détecter le joueur
        detectTarget(player_detection_radius);
        if (target_detected)
        {
            GoTo(target.transform.position);

            // on essaye d'attaquer le joueur si on le détecte
            try_to_attack_target();

            return;
        }

        // si on est ici on a pas de target
        // on essaie de detecter de la viande
        detect_meat(meat_detection_radius);
        if (meat_detected && Vector2.Distance(transform.position, meat_target.transform.position) > .5f)
        {
            GoTo(meat_target.transform.position); // on se dirige vers la viande (grand threshold parce que c ok)
            return;
        }

        // si on est ici on a rien detecté donc on fait rien mdr
        GoNowhere();

    }

    // TARGETS DETECTION
    protected override Collider2D[] get_targets_colliders(float radius)
    {
        Collider2D[] targets = base.get_targets_colliders(radius);

        // on enlève les zombies des targets
        targets = targets.Where(target => target.transform.parent.GetComponent<CyberZombo>() == null).ToArray();

        // Debug.Log("cyberzombo targets : " + targets.Length);
        return targets;
    }

    // MEAT DETECTION
    private void detect_meat(float radius)
    {
        Collider2D[] targets = get_meat_colliders(radius);

        // on regarde si on a trouvé de la viande
        meat_detected = targets.Length > 0;
        if (meat_detected)
        {
            meat_target = targets[0].transform.parent.gameObject;
        }
        else
        {
            meat_target = null;
        }
    }
    protected virtual Collider2D[] get_meat_colliders(float radius)
    {
        // on récupère toutes les viandes dans le rayon de détection
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, meat_layers);

        // on les trie par distance la plus proche
        targets = targets.OrderBy(target => Vector2.Distance(transform.position, target.transform.position)).ToArray();

        return targets;
    }


    // HACKIN
    /* public void initHack()
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
        // hack_ui = transform.Find("hack_ui").GetComponent<HackUI>();
    }

    public bool isHackable(string hack_type, int bits)
    {
        // on met à jour HackUI
        hack_ui.setMode("unhackable");

        // on regarde si on est pas mort
        if (!Alive) { return false; }

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
        // anim_handler.StopForcing();
        // anim_handler.ChangeAnimTilEnd(anims.hurted, hacking_current_duration);
        Do("hurted");

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
        // xp_provider.GetComponent<XPProvider>().EmitBits(bits_left, transform.position, 0.5f);

        // on met à jour les animations
        // anim_handler.StopForcing();
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
 */
    // DIE
    /* protected override void die(){
        // on arrête le hackin
        is_getting_hacked = false;
        hacking_end_time = -1;

        // on meurt
        base.die();
    } */

}
