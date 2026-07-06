using UnityEngine;

public class Note : Capable, Interactable
{
    [SerializeField] private string ui_pool_to_open = "";
    public InteractCapacity Interactor => null;
    public InteractType InteractionType => InteractType.LivingRoom;
    public void OnInteract(Capable interactor)
    {
        if (string.IsNullOrEmpty(ui_pool_to_open)) { return; }
        if (interactor != Controller.Capable) { return; }
        UI_Manager.Instance.SwitchTo(ui_pool_to_open);
    }

    // DATA MANAGEMENT
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);
        if (data is not NoteData nd) { return; }
        this.ui_pool_to_open = nd.ui_pool_to_open;
    }
    public override ICapableData GetStaticData()
    {
        NoteData static_data = new NoteData((CapableData)base.GetStaticData())
        {
            ui_pool_to_open = this.ui_pool_to_open,
        };

        return static_data;
    }
}

public class NoteData : CapableData
{
    public string ui_pool_to_open = "";

    // CONSTRUCTOR
    public NoteData() : base() { }
    public NoteData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new NoteData(base.Duplicate() as CapableData)
        {
            ui_pool_to_open = this.ui_pool_to_open
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - ui_pool_to_open: {ui_pool_to_open}\n";
        return details;
    }
}