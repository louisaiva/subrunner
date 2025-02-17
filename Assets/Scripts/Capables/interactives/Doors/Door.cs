using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class Door : Capable, Interactable, Openable
{


    [Header("Door")]
    public bool is_vertical = false; // just for the editor
    public Collider2D door_collider;
    public ShadowCaster2D shadow_caster;
    public bool is_open { get; set;}
    public bool is_moving { get; set;}


    [Header("ROOMS")]
    public Room room1; // always matchs the Orientation direction (Orientation == "right" => room1 is on the right)
    public Room room2; // always matchs the opposite of the Orientation direction (Orientation == "right" => room2 is on the left)

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

        // on close
        if (Can("close")) { Do("close"); }
    }

    protected override void Update()
    {
        base.Update();

        // on met à jour l'orientation de la porte en fonction de la position du perso
        updateOrientation();
    }



    // ON INTERACT
    public virtual void OnInteract()
    {
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

}