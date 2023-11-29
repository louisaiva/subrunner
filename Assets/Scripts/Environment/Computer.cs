using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AnimationHandler))]
public class Computer : MonoBehaviour, I_Hackable
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
    public float time_to_live_base = 60f; // si on ne l'utilise pas pendant ce temps, il s'éteint

    // ANIMATION
    private AnimationHandler anim_handler;
    private ComputerAnims anims = new ComputerAnims();

    // HACKIN
    public int required_hack_lvl { get; set; }
    public string hack_type_self { get; set; }
    public bool is_getting_hacked { get; set; }
    public float hacking_duration_base { get; set; }
    public float hacking_end_time { get; set; }


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

        // on met à jour le hackin
        updateHack();

        // on met à jour les animations
        if (is_on)
        {
            print("is on, current anim: " + anim_handler.current_anim + ", is forcing: " + anim_handler.IsForcing());

            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_on);
        }
        else
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_off);
        }

        // on regarde si on a été utilisé depuis la dernière frame
        if (is_getting_hacked)
        {
            time_to_live = time_to_live_base;
        }

        // on regarde si on doit s'éteindre
        if (is_on && time_to_live >= 0f)
        {
            time_to_live -= Time.deltaTime;
            if (time_to_live <= 0f)
            {
                is_on = false;
            }
        }
    }

    // MAIN FUNCTIONS
    public void turnOn()
    {
        // on calcule le temps de vie
        time_to_live = time_to_live_base;

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
        // on initialise le hackin
        required_hack_lvl = 1;
        hack_type_self = "computer";
        hacking_end_time = -1;
        hacking_duration_base = 5f;
        is_getting_hacked = false;
    }

    bool I_Hackable.beHacked(int lvl)
    {

        // on regarde si l'ordi est allumé
        if (!is_on) { return false; }

        // on regarde si on est déjà en train de se faire hacker
        if (is_getting_hacked) { return false; }

        // on regarde si on a le bon niveau de hack
        if (lvl < required_hack_lvl) { return false; }

        // on calcule la durée du hack
        // chaque niveau de hack en plus réduit le temps de hack de 5% par rapport au niveau précédent
        // fonction qui tend vers 0 quand le niveau de hack augmente MAIS qui ne peut pas être négative
        // * FONCTIONNE JUSQU'AU NIVEAU 60 !! c super

        float hackin_duration = hacking_duration_base * Mathf.Pow(0.95f, lvl - required_hack_lvl);
        float hackin_speed = hacking_duration_base / hackin_duration;
        if (hackin_duration < 0.1f) { hackin_duration = 0.1f; }

        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.hackin, hackin_speed)) { return false; }

        // on hack la porte
        is_getting_hacked = true;
        hacking_end_time = Time.time + hackin_duration;

        return true;
    }

    bool I_Hackable.isGettingHacked()
    {
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
                // on réussit le hack
                succeedHack();

                // on arrête le hack
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

        // on ferme la porte
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(is_on ? anims.idle_on : anims.idle_off);
    }

    private void succeedHack()
    {
        // on affiche le physical tree du perso
        tree.physicalLevelUp();
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