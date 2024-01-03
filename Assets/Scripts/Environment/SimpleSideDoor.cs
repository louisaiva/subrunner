using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class SimpleSideDoor : MonoBehaviour
{

    // PERSO
    private GameObject perso;

    // ANIMATION
    private AnimationHandler anim_handler;
    private SimpleSideDoorAnims anims = new SimpleSideDoorAnims();

    [Header("OPENING")]
    public bool is_open = false;
    public bool is_moving = false;
    [SerializeField] private  float auto_openin_radius = 2f;
    [SerializeField] private float openin_duration = 1f;
    [SerializeField] private BoxCollider2D box_collider;

    [Header("CEILING")]
    [SerializeField] private GameObject ceiling;

    // UNITY FUNCTIONS
    void Start()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();

        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère le ceiling
        ceiling = transform.Find("ceiling").gameObject;
    }

    void Update()
    {

        // on check si le perso est dans le rayon d'ouverture
        if (!is_open && !is_moving)
        {
            if (Vector2.Distance(transform.position, perso.transform.position) < auto_openin_radius)
            {
                open();
            }
        }
        else if (is_open && !is_moving)
        {
            if (Vector2.Distance(transform.position, perso.transform.position) > auto_openin_radius)
            {
                close();
            }
        }

    }

    // MAIN FUNCTIONS
    void open()
    {
        CancelInvoke();

        // on joue l'animation
        anim_handler.ChangeAnim(anims.openin,openin_duration);
        
        // on ouvre la porte
        Invoke("success_open",openin_duration);

        is_moving = true;

        // on met à jour le box_collider
        box_collider.enabled = false;
        ceiling.SetActive(false);
    }

    void success_open()
    {
        // on ouvre la porte
        is_open = true;
        is_moving = false;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_open);
    }

    void close()
    {
        CancelInvoke();

        // on ferme la porte
        is_moving = true;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.closin,openin_duration);
        Invoke("success_close",openin_duration);
    }

    void success_close()
    {
        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_closed);

        // on ferme la porte
        is_open = false;
        is_moving = false;

        // on met à jour le box_collider
        box_collider.enabled = true;
        ceiling.SetActive(true);
    }

}

public class SimpleSideDoorAnims
{

    // ANIMATIONS
    public string idle_open = "ssdoor_idle_open";
    public string idle_closed = "ssdoor_idle_closed";
    public string openin = "ssdoor_openin";
    public string closin = "ssdoor_closin";

}