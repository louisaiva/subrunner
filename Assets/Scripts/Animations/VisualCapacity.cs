using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class VisualCapacity : Capacity, Visualizable
{

    // VISUALIZABLE
    public new virtual Visualizable Visual => null;
    public virtual string Skin { get => string.Empty; set { OnSkinChange?.Invoke(value); } }
    public event Action<string> OnSkinChange;

    public virtual void Hide() { }
    public virtual void Show() { }
    public virtual bool IsVisible() { return false; }
    public virtual SpriteRenderer Renderer => null;
}

public class VisualData : CapacityData
{

    public string skin;
    public string material_name;
    public int sorting_layer_id;
    public int order_in_layer;

    // CONSTRUCTOR
    public VisualData(CapacityData parent) : base(parent) { }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new VisualData(base.Duplicate() as CapacityData)
        {
            skin = this.skin,
            material_name = this.material_name,
            sorting_layer_id = this.sorting_layer_id,
            order_in_layer = this.order_in_layer
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - skin: {skin}\n";
        details += $"  - material_name: {material_name}\n";
        details += $"  - sorting_layer_id: {sorting_layer_id}\n";
        details += $"  - order_in_layer: {order_in_layer}\n";
        return base.GetDetails() + details;
    }

}
public interface Visualizable
{
    public Visualizable Visual { get; }

    // show hide
    public void Hide();
    public void Show();
    public bool IsVisible();
    public SpriteRenderer Renderer { get; }

    // skin
    public string Skin { get; set; }
    public event Action<string> OnSkinChange;

    // material
    public void SetMaterial(Material material)
    {
        if (Renderer == null) { return; }
        Renderer.material = material;
    }
}