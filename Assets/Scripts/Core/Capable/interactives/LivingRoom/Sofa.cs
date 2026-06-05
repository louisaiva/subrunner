using UnityEngine;

public class Sofa : Container, Interactable, Sittable
{

    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.LivingRoom;

    [SerializeField] private Vector2 local_sitting_position;
    [SerializeField] private Vector2 local_standing_position;
    public Vector2 WorldSittingPosition { get { return transform.TransformPoint(local_sitting_position); } }
    public Vector2 WorldStandingPosition { get { return transform.TransformPoint(local_standing_position); } }

    // ON INTERACT
    public void OnInteract(Capable interactor)
    {
        // check if the interactor is the controlled one
        if (interactor != Controller.Capable) { return; }
        if (!interactor.TryGetCapacity(out SitCapacity sit_capacity)) { return; }


        // we check if we are already sitting on this sofa
        if (sit_capacity.CurrentSofa == this)
        {
            // if we are, we exit the sofa
            sit_capacity.ExitSofa();
            return;
        }

        // we sit on the sofa
        sit_capacity.Sit(this);
    }
    protected override void load_capable_accordingly(Capable capable)
    {
        // we sit on the sofa
        capable.GetCapacity<SitCapacity>()?.Sit(this, instant: true);
        // capable.AnimPlayer.Show();
    }

    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        if (data is not SofaData sofa_data) { Debug.LogError($"(Sofa) {name} cannot load data because it's not a SofaData"); base.LoadData(data); return; }

        // ! WE WANT TO SET THE LOCAL POSITIONS BEFORE calling base.LoadData()
        // because load data calls load_capable_accordingly which call SitCapacity and so
        // we need to have our position set :D
        local_sitting_position = sofa_data.local_sitting_position;
        local_standing_position = sofa_data.local_standing_position;

        base.LoadData(data);
    }
    public override ICapableData GetStaticData()
    {
        SofaData static_data = new SofaData((CapableData)base.GetStaticData())
        {
            local_sitting_position = this.local_sitting_position,
            local_standing_position = this.local_standing_position
        };

        return static_data;
    }
}

public class SofaData : ContainerData
{
    public Vector2 local_sitting_position;
    public Vector2 local_standing_position;
    // CONSTRUCTOR
    public SofaData() : base() { }
    public SofaData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new SofaData(base.Duplicate() as CapableData)
        {
            local_sitting_position = this.local_sitting_position,
            local_standing_position = this.local_standing_position
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - local_sitting_position: {local_sitting_position}\n";
        details += $"  - local_standing_position: {local_standing_position}\n";
        return details;
    }
}