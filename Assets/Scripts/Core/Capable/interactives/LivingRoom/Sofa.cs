using UnityEngine;

public class Sofa : Capable, Interactable, Sittable
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

        // we sit on the sofa
        interactor.GetCapacity<SitCapacity>()?.Sit(this);
    }




    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);
        if (data is not SofaData sofa_data) { Debug.LogError($"(Sofa) {name} cannot load data because it's not a SofaData"); return; }

        local_sitting_position = sofa_data.local_sitting_position;
        local_standing_position = sofa_data.local_standing_position;
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

public class SofaData : CapableData
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
        details += $"local_sitting_position: {local_sitting_position}\n";
        details += $"local_standing_position: {local_standing_position}\n";
        return details;
    }
}