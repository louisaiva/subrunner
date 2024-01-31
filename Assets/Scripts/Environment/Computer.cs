using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Computer : MonoBehaviour, I_Hackable, I_Interactable
{

    // la classe CHEST sert à créer des coffres
    // peut contenir des objets physiques
    // pas obligatoirement alignée avec les tiles

    // mettre un champ de force qui attire les objets vers le coffre,
    // comme ça on peut juste drop les objets à côté du coffre

    // perso
    private SkillTree tree;

    // ORDI
    public bool is_on = false;
    public float time_to_live = 0f; // si on ne l'utilise pas pendant ce temps, il s'éteint
    [SerializeField] private float time_to_live_base = 30f; // si on ne l'utilise pas pendant ce temps, il s'éteint

    public int niveau = 1;

    // ANIMATION
    private AnimationHandler anim_handler;
    private ComputerAnims anims = new ComputerAnims();

    // HACKIN
    public string hack_type_self { get; set; }
    public int required_bits { get; set; }
    public int required_bits_base { get; set; }
    public int security_lvl { get; set; }
    public bool is_getting_hacked { get; set; }
    public float hacking_duration_base { get; set; }
    public float hacking_end_time { get; set; }
    public float hacking_current_duration { get; set; } // temps de hack actuel

    // bit_provider
    public GameObject bit_provider { get; set; }

    // hackable outline
    public Material outline_material { get; set; }
    public Material default_material { get; set; }

    // HackUI
    public HackUI hack_ui { get; set; }

    // interactions
    public bool is_interacting { get; set; } // est en train d'interagir


    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le skill tree
        tree = GameObject.Find("/perso/skills_tree").GetComponent<SkillTree>();

        // on initialise le hackin
        initHack();
    }

    void Update()
    {

        // on met à jour les bits nécessaires pour hacker
        required_bits = (int) (required_bits_base * Mathf.Pow(2, security_lvl - 1));


        // on update selon si l'ordi est allumé ou pas
        if (is_on)
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_on);

            // on update le hackin
            if (is_getting_hacked)
            {
                // on met à jour le hackin
                updateHack();
            }

            // on update le ttl
            if (is_interacting || is_getting_hacked)
            {
                // on est utilisé -> on reset le temps de vie
                time_to_live = time_to_live_base;
            }

            // on regarde si on doit s'éteindre
            if (time_to_live >= 0f)
            {
                time_to_live -= Time.deltaTime;
                if (time_to_live <= 0f)
                {
                    is_on = false;
                }
            }
            
        }
        else
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_off);
        }
    }

    // MAIN FUNCTIONS
    public void turnOn()
    {
        // on regarde si on est déjà allumé
        if (is_on) { return; }
        
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.onin)) { return; }

        // on allume l'ordi
        is_on = true;
    }

    // HACKIN
    public void initHack()
    {

        // on récupère le bit_provider
        bit_provider = GameObject.Find("/particles/xp_provider");

        // on initialise le hackin
        hack_type_self = "computer";
        is_getting_hacked = false;
        hacking_duration_base = 5f;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // bits necessaires pour hacker
        security_lvl = niveau;
        required_bits_base = 4;
        required_bits = (int) (required_bits_base * Mathf.Pow(2, security_lvl - 1));

        // on met à jour le material
        default_material = GetComponent<SpriteRenderer>().material;
        outline_material = Resources.Load<Material>("materials/targeted/hack_computer");

        // on récupère le hack_ui
        hack_ui = transform.Find("hack_ui").GetComponent<HackUI>();
    }

    public bool isHackable(string hack_type, int bits)
    {
        // on change le mode de l'UI
        hack_ui.setMode("unhackable");

        // on regarde si l'ordi est allumé
        if (!is_on) { return false; }
        if (anim_handler.IsForcing()) { return false; }

        // on regarde si on a le bon type de hack
        if (hack_type != hack_type_self) { return false; }

        // on regarde si on a assez de bits
        if (bits < required_bits) { return false; }

        // on change le mode de l'UI
        hack_ui.setMode("hackable");

        return true;
    }

    int I_Hackable.beHacked()
    {

        // on regarde si l'ordi est allumé
        if (!is_on) { return 0; }

        // on regarde si on est déjà en train de se faire hacker
        if (is_getting_hacked) { return 0; }

        // on calcule la durée du hack
        // chaque niveau de hack en plus réduit le temps de hack de 5% par rapport au niveau précédent
        // fonction qui tend vers 0 quand le niveau de hack augmente MAIS qui ne peut pas être négative
        // * FONCTIONNE JUSQU'AU NIVEAU 60 !! c super

        // todo : envoyer le Hack dans la fonction beHacked et appliquer les dégats en fonction du hack
        // todo : le hack influe sur la vitesse de hack, PAS sur le nombre de bits nécessaires pour hacker

        /* float hackin_duration = hacking_duration_base * Mathf.Pow(0.95f, lvl - required_hack_lvl); */

        hacking_current_duration = hacking_duration_base;
        // float hackin_speed = hacking_duration_base / hacking_current_duration;
        if (hacking_current_duration < 0.1f) { hacking_current_duration = 0.1f; }

        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.hackin, hacking_current_duration)) { return 0; }

        // on change le mode de l'UI
        hack_ui.setMode("hacked");

        // on commence le hack
        is_getting_hacked = true;
        hacking_end_time = Time.time + hacking_current_duration;

        return required_bits;
    }

    bool I_Hackable.isGettingHacked()
    {
        return is_getting_hacked;
    }

    public void updateHack()
    {
        // on regarde si on a fini le hack
        if (Time.time > hacking_end_time)
        {
            // on réussit le hack
            succeedHack();
        }
    }

    public void cancelHack()
    {
        // on calcule le temps restant
        float time_left = hacking_end_time - Time.time;

        // on calcule le nombre de bits restants et on en drop la moitié
        int bits_left = Mathf.RoundToInt((required_bits * time_left / hacking_duration_base)/2);

        // on arrête le hack
        is_getting_hacked = false;
        hacking_end_time = -1;
        hacking_current_duration = 0f;

        // on drop les bits restants
        bit_provider.GetComponent<XPProvider>().EmitBits(bits_left, transform.position, 0.5f);

        // on met à jour les animations
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(is_on ? anims.idle_on : anims.idle_off);

        // on met à jour HackUI
        hack_ui.setMode("unhackable");
    }

    public void succeedHack()
    {

        // on affiche le virtual tree du perso
        tree.virtualLevelUp(this);

        // on augmente le niveau de l'ordi
        niveau++;
        security_lvl = niveau;

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
        hack_ui.show();
    }
    public void hideHackUI()
    {

        // on le montre
        hack_ui.hide();
    }

    // INTERACTIONS
    public bool isInteractable()
    {
        // l'interaction porte juste sur l'allumage de l'ordi,
        // donc on return false si l'ordi est déjà allumé
        if (is_on) { return false; }

        // si on est déjà en train d'interagir, on return false
        if (is_interacting) { return false; }

        return true;
    }
    public void interact(GameObject target)
    {
        // on allume l'ordi
        turnOn();

        // on commence l'interaction
        is_interacting = true;

        // on lance le timer d'exctinction automatique
        time_to_live = time_to_live_base;
    }
    public void stopInteract()
    {
        // on arrête l'interaction
        is_interacting = false;
    }
}

public class ComputerAnims
{

    // ANIMATIONS
    public string idle_on = "computer_idle_on";
    public string idle_off = "computer_idle_off";
    public string onin = "computer_powerin_on";
    public string hackin = "computer_hacked";

}