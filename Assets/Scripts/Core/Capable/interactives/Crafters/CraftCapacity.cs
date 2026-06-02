using System.Collections;
using UnityEngine;

public class CraftCapacity : Capacity
{
    [SerializeField] private float craft_duration = 2f;
    [SerializeField] private string craft_anim = "craft";

    // MAIN ENTRY POINT
    public void Craft()
    {
        // check if we are already crafting
        if ((Visual is AnimPlayer player) && player.IsPlaying(craft_anim)) { return; }

        if (log) { Debug.Log($"(CraftCapacity) {Capable.ID} is crafting with duration {craft_duration}"); }
        StartCoroutine(CraftCoroutine());
    }

    // LOW LEVEL CRAFTING
    private IEnumerator CraftCoroutine()
    {
        (Visual as AnimPlayer)?.Play(craft_anim);
        yield return new WaitForSeconds(craft_duration);
        craft_is_done();
    }
    private void craft_is_done()
    {
        if (Capable == null || !Capable.Loaded) { return; }
        if (log) { Debug.Log($"(CraftCapacity) {Capable.ID} is done crafting !"); }
        (Visual as AnimPlayer)?.StopPlaying(craft_anim);
    }




    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData owner)
    {
        base.LoadData(data, owner);

        if (data is not CraftCapacityData craft_data) { return; }
        
        craft_duration = craft_data.craft_duration;
        craft_anim = craft_data.craft_anim;
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        return new CraftCapacityData(base.GetStaticData())
        {
            craft_duration = this.craft_duration,
            craft_anim = this.craft_anim
        };
    }
}

public class CraftCapacityData : CapacityData
{
    public float craft_duration = 2f;
    public string craft_anim = "craft";


    // CONSTRUCTOR
    public CraftCapacityData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new CraftCapacityData(base.Duplicate() as CapacityData)
        {
            craft_duration = this.craft_duration,
            craft_anim = this.craft_anim
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - craft duration: {craft_duration}\n";
        details += $"  - craft anim: {craft_anim}\n";
        return base.GetDetails() + details;
    }

}