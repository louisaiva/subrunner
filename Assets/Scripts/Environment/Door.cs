using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class Door : MonoBehaviour
{


    // ANIMATION
    private AnimationHandler anim_handler;
    protected DoorAnims anims = new DoorAnims();


    [Header("OPENING")]
    [SerializeField] protected bool is_open = false;
    [SerializeField] protected bool is_moving = false;
    [SerializeField] protected float openin_duration = 1f;
    [SerializeField] protected BoxCollider2D box_collider;



    [Header("CEILING")]
    [SerializeField] protected GameObject ceiling;


    // todo -> label qui indique la direction de la porte
    // todo [Header("LABEL")]

    // UNITY FUNCTIONS
    protected void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();

        // on récupère le ceiling
        ceiling = transform.Find("ceiling").gameObject;
    }

    // MAIN FUNCTIONS
    protected void open()
    {
        CancelInvoke();

        // on joue l'animation
        anim_handler.ChangeAnim(anims.openin,openin_duration);
        
        // on ouvre la porte
        Invoke("success_open",openin_duration);

        is_moving = true;

        // on met à jour le box_collider
        box_collider.enabled = false;
        // ceiling.SetActive(false);
    }

    protected void success_open()
    {
        // on ouvre la porte
        is_open = true;
        is_moving = false;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_open);
    }

    protected void close()
    {
        CancelInvoke();

        // on ferme la porte
        is_moving = true;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.closin,openin_duration);
        Invoke("success_close",openin_duration);
    }

    protected void success_close()
    {
        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_closed);

        // on ferme la porte
        is_open = false;
        is_moving = false;

        // on met à jour le box_collider
        box_collider.enabled = true;
        // ceiling.SetActive(true);
    }

}

public class DoorAnims
{

    // ANIMATIONS
    public string idle_open = "door_idle_open";
    public string idle_closed = "door_idle_closed";
    public string openin = "door_openin";
    public string closin = "door_closin";
    public string hackin = "door_hacked";

    public void addGlobalModif(string s)
    {
        idle_open = s + idle_open;
        idle_closed = s + idle_closed;
        openin = s + openin;
        closin = s + closin;
        hackin = s + hackin;
    }

}