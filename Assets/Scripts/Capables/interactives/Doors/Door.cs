using UnityEngine;
using System.Collections;


public class Door : Capable, Interactable, Openable
{


    // ANIMATION
    // protected AnimationHandler anim_handler;
    // protected DoorAnims anims = new DoorAnims();


    [Header("Door")]
    public bool is_vertical = false; // just for the editor
    public BoxCollider2D box_collider;
    public Transform ceiling;
    public bool is_open { get; set;}
    public bool is_moving { get; set;}
    public float openin_duration = 0.5f;


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
        if (is_vertical) { Orientation = Vector2.down; }
        else { Orientation = Vector2.left; }

        // on récupère le perso
        perso = GameObject.Find("/perso").GetComponent<Perso>();

        // on récupère l'animation handler
        // anim_handler = GetComponent<AnimationHandler>();

        // on récupère le box_collider
        box_collider = GetComponent<BoxCollider2D>();

        // on récupère le ceiling
        ceiling = transform.Find("ceiling");

        // on récupère les labels
        // label1 = transform.Find("labels/label1").gameObject;
        // label2 = transform.Find("labels/label2").gameObject;

        // on close
        if (Can("close")) { Do("close"); }
    }

    /* protected override void Update()
    {
        base.Update();

        // met à jour les labels
        // updateLabels();

        // on met à jour la position du perso par rapport à la porte
    } */



    // MAIN FUNCTIONS
    /* protected virtual void open()
    {
        CancelInvoke();

        // on joue l'animation
        // anim_handler.ChangeAnim(anims.openin,openin_duration);
        
        // on ouvre la porte
        Invoke("success_open",openin_duration);

        is_moving = true;

        // on met à jour le box_collider
        box_collider.enabled = false;

        // on affiche les 2 rooms
        room1?.gameObject.SetActive(true);
        room2?.gameObject.SetActive(true);
    }

    protected virtual void success_open()
    {
        // on ouvre la porte
        is_open = true;
        is_moving = false;

        // on joue l'animation
        // anim_handler.ChangeAnim(anims.idle_open);
    }

    protected virtual void close()
    {
        CancelInvoke();

        // on ferme la porte
        is_moving = true;

        // on joue l'animation
        // anim_handler.ChangeAnim(anims.closin,openin_duration);
        Invoke("success_close",openin_duration);
    }

    protected void success_close()
    {
        // on joue l'animation
        // anim_handler.ChangeAnim(anims.idle_closed);

        // on ferme la porte
        is_open = false;
        is_moving = false;

        // on met à jour le box_collider
        box_collider.enabled = true;

        // on cache la 2ème room
        room2?.gameObject.SetActive(false);
    } */

    // ON INTERACT
    public void OnInteract()
    {
        if (Can("open"))
        {
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
        else if (Can("close"))
        {
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

// public class DoorAnims
// {

//     // ANIMATIONS
//     public string idle_open = "door_idle_open";
//     public string idle_closed = "door_idle_closed";
//     public string openin = "door_openin";
//     public string closin = "door_closin";
//     public string hackin = "door_hacked";

//     public void addGlobalModif(string s)
//     {
//         idle_open = s + idle_open;
//         idle_closed = s + idle_closed;
//         openin = s + openin;
//         closin = s + closin;
//         hackin = s + hackin;
//     }

// }