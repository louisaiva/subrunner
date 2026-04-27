using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;


public class Door2 : Capable, Interactable, Openable
{

    public bool log_interact_kf = false;

    [Header("Door")]
    public bool is_vertical = false; // just for the editor
    public bool DontTouchSortingLayer = false;
    private Collider2D _door_collider;
    public Collider2D door_collider
    {
        get
        {
            if (_door_collider == null)
            {
                _door_collider = GetComponent<Collider2D>();
                if (_door_collider == null)
                {
                    _door_collider = FeetCollider; // fallback to feet collider if no collider is found on the door
                }
            }
            return _door_collider;
        }
    }
    public ShadowCaster2D shadow_caster;
    public bool is_open { get; set; }
    public bool is_moving { get; set; }


    [Header("ROOMS")]
    public Room2 room1; // always matchs the Orientation direction (Orientation == "right" => room1 is on the right)
    public Room2 room2; // always matchs the opposite of the Orientation direction (Orientation == "right" => room2 is on the left)

    [Header("Interact Key Feedback Vertical position")]
    [SerializeField] private IFPositionSwitcher if_switcher;
    private Transform interact_kf;

    [Header("Cached components")]
    private OpenCapacity _opener;
    private CloseCapacity _closer;
    public OpenCapacity Opener
    {
        get
        {
            if (_opener == null) { _opener = GetCapacity<OpenCapacity>(); }
            return _opener;
        }
    }
    public CloseCapacity Closer
    {
        get
        {
            if (_closer == null) { _closer = GetCapacity<CloseCapacity>(); }
            return _closer;
        }
    }


    // START
    protected virtual void Start()
    {
        // check if we are an insider we don't even start
        if (CapableSystem.Instance.IsOutsider(this.ID)) { return; }

        // if vertical on set l'Orientaion à "up"
        if (is_vertical && (Orientation == Vector2.right || Orientation == Vector2.left)) { Orientation = Vector2.up; }
        else if (!is_vertical && (Orientation == Vector2.up || Orientation == Vector2.down)) { Orientation = Vector2.left; }

        // on récupère les composants
        // door_collider = GetComponent<Collider2D>();
        shadow_caster = door_collider.GetComponent<ShadowCaster2D>();

        // on récup l'interact kf
        interact_kf = GetCapacity<HoverCapacity>().Canvas_kf;

        // on close
        if (Closer is not null && Closer.Able) { close(); }
    }



    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get { return InteractType.Door; } }
    public virtual void OnInteract(Capable interactor)
    {
        // on set l'interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // on réagit à l'interaction
        if (Opener.Able) { open(); }
        else if (Closer.Able) { close(); }
    }


    // todo : à déplacer dans les Capacity ????
    // OPENABLE
    public void open()
    {
        // on désactive le collider
        door_collider.enabled = false;
        // on désactive le ShadowCaster2D
        if (shadow_caster != null) { shadow_caster.enabled = false; }


        Opener.Use(this);


        // on récupère la room du perso
        Room2 perso_room = Controller.Instance.current_room;

        // on vérifie que la room du perso est bien une des 2 rooms de la porte
        if (!(perso_room == room1 || perso_room == room2)) { return; }


        // on récupère la room qui s'ouvre
        Room2 room_to_open = perso_room == room1 ? room2 : room1;
        if (room_to_open == null)
        {
            if (log) { Debug.LogWarning("(Door) Room2 to open is null!"); }
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
        if (shadow_caster != null) { shadow_caster.enabled = true; }

        Closer.Use(this);


        // on récupère la room du perso
        Room2 perso_room = Controller.Instance.current_room;

        // on vérifie que la room du perso est bien une des 2 rooms de la porte
        if (!(perso_room == room1 || perso_room == room2)) { return; }

        // on récupère la room qui se ferme
        Room2 room_to_close = perso_room == room1 ? room2 : room1;
        if (room_to_close == null)
        {
            if (log) { Debug.LogWarning("(Door) Room2 to close is null!"); }
            return;
        }

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

        Vector2 new_orientation;

        // on regarde si la porte est verticale ou horizontale
        if (is_vertical)
        {
            // on set l'orientation de la porte à "up" ou "down"
            new_orientation = perso_direction.y < 0 ? Vector2.up : Vector2.down;
        }
        else
        {
            // on set l'orientation de la porte à "right" ou "left"
            new_orientation = perso_direction.x < 0 ? Vector2.right : Vector2.left;
        }

        // on regarde si on a déjà la bonne orientation
        if (Orientation == new_orientation) { return; }

        // on set la nouvelle orientation
        Orientation = new_orientation;
        update_interact_kf_position(new_orientation);
    }

    // INTERACT KF
    private void update_interact_kf_position(Vector2 orientation)
    {
        if (interact_kf == null) { return; }
        if (!is_vertical) { return; } // on ne bouge le kf que si la porte est verticale
        float vertical_position;

        // d'abord on veut savoir si la porte est ouverte ou fermée
        if (is_open && !is_moving) { vertical_position = orientation == Vector2.up ? if_switcher.opened_interact_kf_y.x : if_switcher.opened_interact_kf_y.y; }
        else { vertical_position = orientation == Vector2.up ? if_switcher.closed_interact_kf_y.x : if_switcher.closed_interact_kf_y.y; }

        if (log_interact_kf) { Debug.Log("(Door) " + name + " orientation : " + orientation + ", is_open : " + is_open + ", vertical_position : " + vertical_position); }

        // puis on applique la position
        interact_kf.localPosition = new Vector2(interact_kf.localPosition.x, vertical_position);
    }




    // LOAD / UNLOAD DATA
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);

        // we get the shadow caster reference
        shadow_caster = door_collider.GetComponent<ShadowCaster2D>();

        // on récup l'interact kf
        interact_kf = GetCapacity<HoverCapacity>().Canvas_kf;

    }
    public override void UnloadData()
    {
        base.UnloadData();

        // we clear the door collider & shadow caster references
        _door_collider = null;
        shadow_caster = null;

        // and other references
        _opener = null;
        _closer = null;
    }

}