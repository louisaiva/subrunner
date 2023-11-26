using UnityEngine;

[RequireComponent(typeof(AnimationHandler))]
public class Door : MonoBehaviour, I_Hackable
{
    // La classe DOOR sert à créer des portes et à les ouvrir
    // comme elle n'est pas alignée avec les tiles, il faut faire attention à la position de la porte
    // elle doit être alignée avec les tiles

    // ANIMATION
    private AnimationHandler anim_handler;
    private DoorAnims anims = new DoorAnims();

    // OPENING
    public bool is_open = false;
    private BoxCollider2D box_collider;

    // closing
    private float auto_closin_delay = 8f;

    // HACKIN
    public int required_hack_lvl { get; set;}
    public string hack_type_self { get; set;}
    public bool is_getting_hacked { get; set; }
    public float hacking_duration_base { get; set;}
    public float hacking_end_time { get; set;}


    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();

        // on initialise le hackin
        initHack();
    }

    void Update()
    {

        // on met à jour le hackin
        updateHack();

        // on met à jour les animations
        if (is_open)
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_open);
        }
        else
        {
            // on regarde si on a fini l'animation
            if (anim_handler.IsForcing()) { return; }

            // on met à jour les animations
            anim_handler.ChangeAnim(anims.idle_closed);
        }

        // on met à jour le collider
        box_collider.enabled = !is_open;
    }

    public void open()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.openin)) { return; }

        // on ouvre la porte
        is_open = true;

        // on ferme la porte après un certain temps
        Invoke("close", auto_closin_delay);
    }

    public void close()
    {
        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.closin)) { return; }

        // on ferme la porte
        is_open = false;
    }

    // HACKIN
    public void initHack()
    {
        // on initialise le hackin
        required_hack_lvl = 1;
        hack_type_self = "door";
        hacking_end_time = -1;
        hacking_duration_base = 2f;
        is_getting_hacked = false;
    }

    bool I_Hackable.beHacked(int lvl)
    {

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
        if (!anim_handler.ChangeAnimTilEnd(anims.hackin,hackin_speed)) { return false; }

        // on hack la porte
        is_open = true;
        is_getting_hacked = true;
        hacking_end_time = Time.time + hackin_duration;

        // print("je vais me faire hacker pendant " + hackin_duration + " secondes");

        // on ferme la porte après un certain temps
        Invoke("close", auto_closin_delay);

        return true;
    }

    bool I_Hackable.isGettingHacked()
    {
        return is_getting_hacked;
    }

    public bool isHackable(string hack_type, int lvl=1000){

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

        // on ferme la porte
        anim_handler.StopForcing();
        anim_handler.ChangeAnimTilEnd(anims.idle_closed);
        is_open = false;
    }


    /* // GETTERS

    public float GetHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }

    public float getWidth()
    {
        return GetComponent<SpriteRenderer>().bounds.size.x;
    }

    public float getRadius()
    {
        return Mathf.Max(GetHeight(), getWidth()) / 2f;
    }

    */
}


public class DoorAnims
{

    // ANIMATIONS
    public string idle_open = "door_open_idle";
    public string idle_closed = "door_closed_idle";
    public string openin = "door_openin";
    public string closin = "door_closin";
    public string hackin = "door_hacked";

}