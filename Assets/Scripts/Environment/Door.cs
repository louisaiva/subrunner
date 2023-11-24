using UnityEngine;

[RequireComponent(typeof(AnimationHandler))]
public class Door : MonoBehaviour
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
    private int required_hack_lvl = 1;


    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
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

    public void hack(int lvl)
    {

        // on regarde si on a le bon niveau de hack
        if (lvl < required_hack_lvl) { return; }

        // on met à jour les animations
        if (!anim_handler.ChangeAnimTilEnd(anims.hackin)) { return; }

        // on hack la porte
        is_open = true;

        // on ferme la porte après un certain temps
        Invoke("close", auto_closin_delay);
    }


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