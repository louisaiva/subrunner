using UnityEngine;
using System.Collections;


[RequireComponent(typeof(AnimationHandler))]
public class Door : MonoBehaviour
{


    // ANIMATION
    protected AnimationHandler anim_handler;
    protected DoorAnims anims = new DoorAnims();


    [Header("OPENING")]
    [SerializeField] protected bool is_open = false;
    [SerializeField] protected bool is_moving = false;
    [SerializeField] protected float openin_duration = 1f;
    [SerializeField] protected BoxCollider2D box_collider;


    [Header("CEILING")]
    [SerializeField] protected GameObject ceiling;


    [Header ("LABELS")]
    [SerializeField] protected GameObject label1;
    [SerializeField] protected GameObject label2;
    [SerializeField] protected float label_apparition_radius = 2f;
    [SerializeField] protected float offset_up_radius = 1f;
    [SerializeField] protected string door_axis = "vertical";


    // PERSO
    protected GameObject perso;

    // UNITY FUNCTIONS
    protected void Start()
    {
        // on récupère le perso
        perso = GameObject.Find("/perso");

        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();

        // on récupère le ceiling
        ceiling = transform.Find("ceiling").gameObject;

        // on récupère les labels
        label1 = transform.Find("labels/label1").gameObject;
        label2 = transform.Find("labels/label2").gameObject;

        // on close
        close();
    }

    protected void Update()
    {
        // met à jour les labels
        updateLabels();
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


    // LABELS
    protected void updateLabels()
    {
        // on check si le perso est dans le rayon d'ouverture
        if (Vector2.Distance(transform.position, perso.transform.position) > label_apparition_radius + offset_up_radius)
        {
            // on désactive les labels
            label1.SetActive(false);
            label2.SetActive(false);

            return;
        }

        if (Vector2.Distance(transform.position, perso.transform.position) > label_apparition_radius)
        {
            // si on est ici on est dans le rayon d'apparition du label up
            if (door_axis == "vertical" && perso.transform.position.y > transform.position.y)
            {
                // on active le label 2 (en haut)
                label1.SetActive(false);
                label2.SetActive(true);
            }
            else
            {
                // on désactive les labels
                label1.SetActive(false);
                label2.SetActive(false);
            }
            return;
        }

        // si on est ici, c'est qu'on est dans le rayon d'ouverture de tous les labels
        print("(door " + gameObject.name + ") active ses labels !");

        // on récupère la direction du perso
        if (door_axis == "vertical")
        {
            if (perso.transform.position.y <= transform.position.y)
            {
                // on active le label 1 (en bas)
                label1.SetActive(true);
                label2.SetActive(false);
            }
            else
            {
                // on active le label 2 (en haut)
                label1.SetActive(false);
                label2.SetActive(true);
            }
        }
        else
        {
            if (perso.transform.position.x <= transform.position.x)
            {
                // on active le label 1 (à gauche)
                label1.SetActive(true);
                label2.SetActive(false);
            }
            else
            {
                // on active le label 2 (à droite)
                label1.SetActive(false);
                label2.SetActive(true);
            }
        }
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