using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;


public class Door : Capable, Interactable, Openable
{

    public bool log_interact_kf = false;

    [Header("Door")]
    public bool is_vertical = false;
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
    public string room1_id; // always matchs the Orientation direction (Orientation == "right" => room1 is on the right)
    public string room2_id; // always matchs the opposite of the Orientation direction (Orientation == "right" => room2 is on the left)



    [Header("Interact Key Feedback Vertical position")]
    [SerializeField] private IFPositionSwitcher if_switcher;
    /* private Transform _interact_kf;
    protected Transform interact_kf
    {
        get
        {
            if (_interact_kf == null)
            {
                HoverCapacity hover_capacity = GetCapacity<HoverCapacity>();
                if (hover_capacity != null) { _interact_kf = hover_capacity.Canvas_kf; }
            }
            return _interact_kf;
        }
    } */

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


    // EVENTS
    public event Action<Door> OnDoorOpen;
    public event Action<Door> OnDoorClose;


    // INTERACTABLE
    public InteractCapacity Interactor { get; set; }
    public InteractType InteractionType { get { return InteractType.Door; } }
    public virtual void OnInteract(Capable interactor)
    {
        // on set l'interactor
        Interactor = interactor.GetCapacity<InteractCapacity>();

        // on réagit à l'interaction
        if (Opener.Able) { Open(); }
        else if (Closer.Able) { Close(); }
    }

    // OPENABLE
    public void Open()
    {
        // on désactive le collider
        door_collider.enabled = false;
        if (shadow_caster != null) { shadow_caster.enabled = false; }

        Opener.Open();
        OnDoorOpen?.Invoke(this);
    }
    public void Close()
    {
        // on reactive le collider
        door_collider.enabled = true;
        if (shadow_caster != null) { shadow_caster.enabled = true; }

        Closer.Close();
        OnDoorClose?.Invoke(this);
    }
    public void OpenInstantly()
    {
        // on désactive le collider & ShadowCaster2D
        door_collider.enabled = false;
        if (shadow_caster != null) { shadow_caster.enabled = false; }

        // on ouvre direct
        Opener.OpenInstantly();
    }
    public void CloseInstantly()
    {
        // on reactive le collider & ShadowCaster2D
        door_collider.enabled = true;
        if (shadow_caster != null) { shadow_caster.enabled = true; }

        // on ferme direct
        Closer.CloseInstantly();
    }

    // UPDATE
    protected override void Update()
    {
        base.Update();

        // on met à jour l'orientation de la porte en fonction de la position du perso
        updateOrientation();
    }
    protected void updateOrientation()
    {
        if (Controller.Capable == null) { return; }
        
        // on récupère le vecteur entre la porte et le perso
        Vector2 perso_direction = Controller.Capable.transform.position - transform.position;

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
        UpdateIF(new_orientation);
    }

    // INTERACT KF
    public void UpdateIF(Vector2 orientation)
    {
        // if (interact_kf == null) { return; }
        if (!TryGetCapacity(out InputIndicationCapacity iic)) { return; }


        float vertical_position;
        Vector2 main_door_direction = is_vertical ? Vector2.up : Vector2.right;

        // d'abord on veut savoir si la porte est ouverte ou fermée
        if (!is_moving && is_open) // ouverte seulement
        {
            vertical_position =

                orientation == main_door_direction
                ? if_switcher.opened_interact_kf_y.x
                : if_switcher.opened_interact_kf_y.y;
        }
        else
        {
            vertical_position =

                orientation == main_door_direction
                ? if_switcher.closed_interact_kf_y.x
                : if_switcher.closed_interact_kf_y.y;
        }

        if (log_interact_kf) { Debug.Log("(Door) " + name + " orientation : " + orientation + ", is_open : " + is_open + ", vertical_position : " + vertical_position); }

        // puis on applique la position
        iic.SetOffset(new Vector2(iic.transform.localPosition.x, vertical_position));
        // interact_kf.localPosition = new Vector2(interact_kf.localPosition.x, vertical_position);
    }


    // GETTERS
    public bool IsOpenOrOpening()
    {
        return (is_open && !is_moving) || (!is_open && is_moving);
    }
    public bool IsCloseOrClosing()
    {
        return (!is_open && !is_moving) || (is_open && is_moving);
    }



    // LOAD / UNLOAD DATA
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);

        // we get the shadow caster reference
        shadow_caster = door_collider.GetComponent<ShadowCaster2D>();

        if (data is not DoorData door_data) { return; }
        
        // on met les paramètres de la porte
        this.is_vertical = door_data.is_vertical;
        this.DontTouchSortingLayer = door_data.dont_touch_sorting_layer;
        this.room1_id = door_data.room1_id;
        this.room2_id = door_data.room2_id;

        // on met à jour la position du kf d'interaction
        this.if_switcher = door_data.if_switcher;
        updateOrientation();

        // on ouvre / ferme la porte en fonction des données
        if (door_data.is_open) { OpenInstantly(); }
        else { CloseInstantly(); }
    }
    public override void UnloadData()
    {
        base.UnloadData();

        // we clear the door collider & shadow caster references
        _door_collider = null;
        shadow_caster = null;

        // we clear the interact kf reference
        // _interact_kf = null;

        // and other references
        _opener = null;
        _closer = null;
    }
    public override ICapableData GetStaticData()
    {
        DoorData static_data = new DoorData((CapableData)base.GetStaticData())
        {
            is_vertical = this.is_vertical,
            dont_touch_sorting_layer = this.DontTouchSortingLayer,
            room1_id = this.room1_id,
            room2_id = this.room2_id,
            if_switcher = this.if_switcher.Duplicate(),
            is_open = false // we close every door by default when getting static data
        };

        return static_data;
    }
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (data is not DoorData door_data) { return; }
        door_data.is_open = this.is_open;
    }
}



// DOOR DATA
[Serializable] public class DoorData : CapableData
{

    // INSTANCE DATA
    public bool is_vertical;
    public bool dont_touch_sorting_layer;
    public string room1_id;
    public string room2_id;
    public bool is_open;
    public IFPositionSwitcher if_switcher;


    // CONSTRUCTOR
    public DoorData() : base() { }
    public DoorData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new DoorData(base.Duplicate() as CapableData)
        {
            is_vertical = this.is_vertical,
            room1_id = this.room1_id,
            room2_id = this.room2_id,
            is_open = this.is_open,
            if_switcher = this.if_switcher.Duplicate()
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - is vertical : {is_vertical}\n";
        details += $"  - room1 id : {room1_id}\n";
        details += $"  - room2 id : {room2_id}\n";
        details += $"  - is open : {is_open}\n";
        details += $"  - IFPositionSwitcher : \n{if_switcher.GetDetails()}\n";
        return details;
    }

    // RUNTIME HELP METHODS
    public bool GetPositionInsideRoom(string room, out Vector2 position)
    {
        position = Position;
        if (room != room1_id && room != room2_id) { return false; }
        if (room == room1_id)
        {
            position += .5f * (is_vertical ? Vector2.up : Vector2.right);
        }
        else
        {
            position += .5f * (is_vertical ? Vector2.down : Vector2.left);
        }
        return true;
    }
}

[Serializable] public class IFPositionSwitcher
{
    public Vector2 closed_interact_kf_y = new Vector2(2.25f, 1.25f); // orientation up then down
    public Vector2 opened_interact_kf_y = new Vector2(0.9f, -0.4f); // orientation up then down

    // DUPLICATE
    public IFPositionSwitcher Duplicate()
    {
        return new IFPositionSwitcher()
        {
            closed_interact_kf_y = this.closed_interact_kf_y,
            opened_interact_kf_y = this.opened_interact_kf_y
        };
    }

    // GET DETAILS
    public string GetDetails()
    {
        string details = $"    - closed interact kf y : {closed_interact_kf_y}\n";
        details += $"    - opened interact kf y : {opened_interact_kf_y}\n";
        return details;
    }
}
