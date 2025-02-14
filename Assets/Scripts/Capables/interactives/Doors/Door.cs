using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class Door : Capable, Interactable, Openable
{


    // ANIMATION
    // protected AnimationHandler anim_handler;
    // protected DoorAnims anims = new DoorAnims();

    // inputs actions
    /*public PlayerInputActions input_actions
    {
        get => GameObject.Find("/utils/input_manager").GetComponent<InputManager>().inputs;
    }*/

    [Header("Door")]
    public bool is_vertical = false; // just for the editor
    public Collider2D door_collider;
    public ShadowCaster2D shadow_caster;
    public bool is_open { get; set;}
    public bool is_moving { get; set;}
    // public float openin_duration = 0.5f;


    /* [Header ("LABELS")]
    [SerializeField] protected GameObject label1;
    [SerializeField] protected GameObject label2;
    [SerializeField] protected float label_apparition_radius = 2f;
    [SerializeField] protected float offset_up_radius = 1f; */

    [Header("ROOMS")]
    public Room room1; // always matchs the Orientation direction (Orientation == "right" => room1 is on the right)
    public Room room2; // always matchs the opposite of the Orientation direction (Orientation == "right" => room2 is on the left)
    // public Room player_room; // the room where the player is.

    // PERSO
    protected Perso perso;

    // UNITY FUNCTIONS
    protected override void Start()
    {
        base.Start();

        // if vertical on set l'Orientaion à "up"
        if (is_vertical && (Orientation == Vector2.right || Orientation == Vector2.left))
        {
            Orientation = Vector2.up;
        }
        else if (!is_vertical && (Orientation == Vector2.up || Orientation == Vector2.down))
        {
            Orientation = Vector2.left;
        }

        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère le door_collider
        door_collider = GetComponent<Collider2D>();

        // on récup le shadow caster
        shadow_caster = GetComponent<ShadowCaster2D>();

        // on récupère les labels
        // label1 = transform.Find("labels/label1").gameObject;
        // label2 = transform.Find("labels/label2").gameObject;

        // on close
        if (Can("close")) { Do("close"); }
    }

    protected override void Update()
    {
        base.Update();

        // met à jour les labels
        // updateLabels();

        // on met à jour l'orientation de la porte en fonction de la position du perso
        updateOrientation();
    }



    // ON INTERACT
    public virtual void OnInteract()
    {
        // if (!input_actions.perso.enabled) { return; }
        if (Can("open")) { open(); }
        else if (Can("close")) { close(); }
    }

    // todo : à déplacer dans les Capacity ????
    public void open()
    {
        // on désactive le collider
        door_collider.enabled = false;
        // on désactive le ShadowCaster2D
        shadow_caster.enabled = false;


        Do("open");


        // on récupère la room du perso
        Room perso_room = perso.current_room;

        // on vérifie que la room du perso est bien une des 2 rooms de la porte
        if (!(perso_room == room1 || perso_room == room2)) { return; }

        // on récupère la room qui s'ouvre
        Room room_to_open = perso_room == room1 ? room2 : room1;

        // on affiche les lights de la room qui s'ouvre
        room_to_open.Show();
        
    }

    public void close()
    {
        // on reactive le collider
        door_collider.enabled = true;
        // on reactive le ShadowCaster2D
        shadow_caster.enabled = true;

        Do("close");


        // on récupère la room du perso
        Room perso_room = perso.current_room;

        // on vérifie que la room du perso est bien une des 2 rooms de la porte
        if (!(perso_room == room1 || perso_room == room2)) { return; }

        // on récupère la room qui se ferme
        Room room_to_close = perso_room == room1 ? room2 : room1;

        // on cache les lights de la room qui se ferme
        room_to_close.Hide();

    }


    protected void updateOrientation()
    {
        // on récupère le vecteur entre la porte et le perso
        Vector2 perso_direction = perso.transform.position - transform.position;
        
        // l'orientation de la porte tourne toujours le dos au perso !!
        // c'est pour avoir les flèches dans le bon sens
        // si le perso est en bas de la porte, la fleche doit indiquer le haut !

        // on regarde si la porte est verticale ou horizontale
        if (is_vertical)
        {
            // on set l'orientation de la porte à "up" ou "down"
            Orientation = perso_direction.y < 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            // on set l'orientation de la porte à "right" ou "left"
            Orientation = perso_direction.x < 0 ? Vector2.right : Vector2.left;
        }
    }

    // LABELS
    /* protected void updateLabels()
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
        // print("(door " + gameObject.name + ") active ses labels !");

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
    } */

}