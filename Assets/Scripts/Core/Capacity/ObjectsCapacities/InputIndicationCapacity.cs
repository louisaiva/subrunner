using System;
using UnityEngine;


/// <summary>
/// Specific capacity helper of HoverCapacity that store a reference to a position, a color and an input
/// to show an InputFeedback (IF) on top of the capable when hovered.
/// This is what allow us to show little interactive [E] on top of chests.
/// </summary>
public class InputIndicationCapacity : Capacity
{

    [Header("Parameters to save in data")]
    [SerializeField] private Color color;
    [SerializeField] private string input_name;


    private InputIndicationData iidata { get { return (InputIndicationData) data; } }
    public Color Color { get { return iidata.color; } }
    public string InputName { get { return iidata.input_name; } }

    // events
    public event Action<Capable> OnOffsetSet = delegate { };
    public event Action<Capable> OnOffsetReset = delegate { };
    
    private Vector2 loaded_position;
    public void SetOffset(Vector2 position)
    {
        transform.localPosition = position;
        OnOffsetSet?.Invoke(Capable);
    }
    public void ResetOffset()
    {
        transform.localPosition = loaded_position;
        OnOffsetReset?.Invoke(Capable);
    }


    ///
    //
    /// DATA MANAGEMENT
    //
    ///


    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData owner)
    {
        base.LoadData(data, owner);
        if (data is not InputIndicationData iidata) { return; }
        loaded_position = iidata.local_position;

        // no need to set offset directly bcz it is already set inside the base.LoadData :D
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        InputIndicationData new_data = new InputIndicationData(base.GetStaticData())
        {
            color = this.color,
            input_name = this.input_name
        };
        return new_data;
    }
}


[Serializable] public class InputIndicationData : CapacityData
{
    public Color color;
    public string input_name;

    // CONSTRUCTOR
    public InputIndicationData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new InputIndicationData(base.Duplicate() as CapacityData)
        {
            color = this.color,
            input_name = this.input_name
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - IF offset: {local_position}\n";
        details += $"  - color: {color}\n";
        details += $"  - input name: {input_name}\n";
        return base.GetDetails() + details;
    }
}