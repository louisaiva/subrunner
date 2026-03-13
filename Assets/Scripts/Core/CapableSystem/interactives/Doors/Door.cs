using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class Door : Capable, Interactable, Openable
{

    public bool log_interact_kf = false;

    [Header("Door")]
    public bool is_vertical = false; // just for the editor
    public bool DontTouchSortingLayer = false;
    public Collider2D door_collider;
    public ShadowCaster2D shadow_caster;
    public bool is_open { get; set; }
    public bool is_moving { get; set; }


    [Header("ROOMS")]
    public Room2 room1; // always matchs the Orientation direction (Orientation == "right" => room1 is on the right)
    public Room2 room2; // always matchs the opposite of the Orientation direction (Orientation == "right" => room2 is on the left)

    [Header("Interact Key Feedback Vertical position")]
    private Vector2 closed_interact_kf_y = new Vector2(2.25f, 1.25f); // orientation up then down
    private Vector2 opened_interact_kf_y = new Vector2(0.9f, -0.4f); // orientation up then down
    private Transform interact_kf;

    [Header("Cached components")]
    protected OpenCapacity opener;
    protected CloseCapacity closer;
    public OpenCapacity Opener { get { return opener; }}
    public CloseCapacity Closer { get { return closer; }}

    // START
    protected virtual void Start()
    {
        // if vertical on set l'Orientaion à "up"
        if (is_vertical && (Orientation == Vector2.right || Orientation == Vector2.left)) { Orientation = Vector2.up; }
        else if (!is_vertical && (Orientation == Vector2.up || Orientation == Vector2.down)) { Orientation = Vector2.left; }

        // on récupère les composants
        door_collider = GetComponent<Collider2D>();
        shadow_caster = GetComponent<ShadowCaster2D>();

        // on récup l'interact kf
        interact_kf = GetCapacity<HoverCapacity>().Canvas_kf;

        // & les capacities
        opener = GetCapacity<OpenCapacity>();
        closer = GetCapacity<CloseCapacity>();

        // on close
        if (closer.Able) { close(); }
    }



    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get { return InteractType.Door; } }
    public virtual void OnInteract(Capable interactor)
    {
        // on set l'interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // on réagit à l'interaction
        if (opener.Able) { open(); }
        else if (closer.Able) { close(); }
    }


    // todo : à déplacer dans les Capacity ????
    // OPENABLE
    public void open()
    {
        // on désactive le collider
        door_collider.enabled = false;
        // on désactive le ShadowCaster2D
        shadow_caster.enabled = false;


        opener.Use(this);


        // on récupère la room du perso
        Room2 perso_room = Controller.Instance.current_room;

        // on vérifie que la room du perso est bien une des 2 rooms de la porte
        if (!(perso_room == room1 || perso_room == room2)) { return; }


        // on récupère la room qui s'ouvre
        Room2 room_to_open = perso_room == room1 ? room2 : room1;
        if (room_to_open == null)
        {
            Debug.LogWarning("(Door) Room2 to open is null!");
            return;
        }

        // on affiche les lights de la room qui s'ouvre
        room_to_open.Show();
    }
    public void close()
    {
        // on reactive le collider
        door_collider.enabled = true;
        // on reactive le ShadowCaster2D
        shadow_caster.enabled = true;

        closer.Use(this);


        // on récupère la room du perso
        Room2 perso_room = Controller.Instance.current_room;

        // on vérifie que la room du perso est bien une des 2 rooms de la porte
        if (!(perso_room == room1 || perso_room == room2)) { return; }

        // on récupère la room qui se ferme
        Room2 room_to_close = perso_room == room1 ? room2 : room1;

        // on cache les lights de la room qui se ferme
        room_to_close.Hide();

    }


    // UPDATE
    protected override void Update()
    {
        base.Update();

        if (Controller.Instance == null) { return; }

        // on met à jour l'orientation de la porte en fonction de la position du perso
        updateOrientation();
    }
    protected void updateOrientation()
    {
        // on récupère le vecteur entre la porte et le perso
        Vector2 perso_direction = Controller.Instance.transform.position - transform.position;

        // l'orientation de la porte tourne toujours le dos au perso !!
        // c'est pour avoir les flèches dans le bon sens
        // si le perso est en bas de la porte, la fleche doit indiquer le haut !

        // on regarde si la porte est verticale ou horizontale
        if (is_vertical)
        {
            // on set l'orientation de la porte à "up" ou "down"
            Orientation = perso_direction.y < 0 ? Vector2.up : Vector2.down;
            update_interact_kf_position(Orientation);
        }
        else
        {
            // on set l'orientation de la porte à "right" ou "left"
            Orientation = perso_direction.x < 0 ? Vector2.right : Vector2.left;
        }
    }

    // INTERACT KF
    private void update_interact_kf_position(Vector2 orientation)
    {
        if (interact_kf == null) { return; }
        if (!is_vertical) { return; } // on ne bouge le kf que si la porte est verticale
        float vertical_position;

        // d'abord on veut savoir si la porte est ouverte ou fermée
        if (is_open && !is_moving) { vertical_position = orientation == Vector2.up ? opened_interact_kf_y.x : opened_interact_kf_y.y; }
        else { vertical_position = orientation == Vector2.up ? closed_interact_kf_y.x : closed_interact_kf_y.y; }

        if (log_interact_kf) { Debug.Log("(Door) " + name + " orientation : " + orientation + ", is_open : " + is_open + ", vertical_position : " + vertical_position); }

        // puis on applique la position
        interact_kf.localPosition = new Vector2(interact_kf.localPosition.x, vertical_position);
    }

}