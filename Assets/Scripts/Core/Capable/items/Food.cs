using System;
using System.Collections;
using UnityEngine;

public class Food : Item, Usable
{
    [Header("Food parameters")]
    public float life_regen = 10f; // life regen of the food
    public int portions = 1; // number of portions left in the food
    public event System.Action<HealthCapacity> OnBeingBitten = delegate { };

    // BEING EATEN
    public void BeEaten(HealthCapacity eater)
    {
        OnBeingBitten?.Invoke(eater);
        
        // we reduce the number of portions
        portions--;
        if (AnimPlayer.HasCapacity("idle_eaten"))
        {
            AnimPlayer.Play("idle_eaten");
            if (TryGetCapacity(out HoverCapacity hover_capacity))
            {
                hover_capacity.ChangeAnimation("hover_eaten");
            }
        }
        if (portions > 0) { return; }

        // if there is no more portions, we despawn the food
        CapableEngine.Instance.DespawnCapable(this.data);
        if (Holder != null) { Holder.Inventory.Remove(this); } // we remove the item from the holder's inventory
    }
    public string UseLabel { get; } = "eat";
    public void Use(Capable user)
    {
        // we get the user's eat capacity
        EatCapacity eat_capacity = user.GetCapacity<EatCapacity>();
        if (eat_capacity == null) { return; }

        // we eat the food
        eat_capacity.SetFoodTarget(this);
        eat_capacity.Use(user);
    }

    // todo : make this more with multiple food portions


    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / GET DATA
    public override void LoadData(CapableData data)
    {
        base.LoadData(data);

        if (data is not FoodData fdata) { return; }
        this.life_regen = fdata.life_regen;
        this.portions = fdata.portions;
    }
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();

        if (data is not FoodData fdata) { return; }
        fdata.portions = this.portions;
    }
    public override ICapableData GetStaticData()
    {
        FoodData static_data = new FoodData((CapableData)base.GetStaticData())
        {
            life_regen = this.life_regen,
            portions = this.portions
        };

        return static_data;
    }
}


// ITEM DATA
[Serializable] public class FoodData : ItemData
{
    public float life_regen = 10f; // life regen of the food per portions
    public int portions = 1; // number of portions of the food


    // CONSTRUCTOR
    public FoodData() : base() { }
    public FoodData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        return new FoodData(base.Duplicate() as CapableData)
        {
            life_regen = this.life_regen,
            portions = this.portions
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = base.GetDetails();
        details += $"  - life_regen : {life_regen}\n";
        details += $"  - portions : {portions}\n";
        return details;
    }

}