
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AnimCapacity is the link between the capable & AnimPlayer basically
/// </summary>

public class AnimCapacity : VisualCapacity
{
    [SerializeField] private AnimPlayer _anim_player;
    public AnimPlayer Player
    {
        get
        {
            if (_anim_player == null) { _anim_player = GetComponent<AnimPlayer>(); }
            return _anim_player;
        }
    }
    public override Visualizable Visual => Player;
    public override string Skin
    {
        get => Player?.Skin ?? string.Empty;
        set
        {
            if (Player != null) { Player.Skin = value; }
        }
    }
    public override void Hide() => Player?.Hide();
    public override void Show() => Player?.Show();
    public override bool IsVisible() => Player?.IsVisible() ?? false;
    public override SpriteRenderer Renderer => Player?.Renderer;



    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData owner)
    {
        base.LoadData(data, owner);
        if (data is not AnimData adata) { return; }
        CapableBank.Instance.LayerBank.LoadAnimData(Player, adata);
        Player.SetOrientation(owner.orientation);
    }
    public override void UnloadData()
    {
        CapableBank.Instance.LayerBank.UnloadAnimData(Player);
        base.UnloadData();
    }

    // SAVE DYNAMIC DATA
    public override void SaveDynamicData()
    {
        base.SaveDynamicData();
        if (this.data is not AnimData adata) { return; }
        Player.SaveDynamicPlayerData(adata);
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        // get basic player data
        AnimData data = new AnimData(base.GetStaticData())
        {
            skin = Player?.Skin,
            anim_capacity_priorities = Player?.ACPs,

            sorting_layer_id = Renderer.sortingLayerID,
            order_in_layer = Renderer.sortingOrder,
            material_name = MaterialBank.GetMaterialName(Renderer, ID),

            // and parameters
            never_flip = Player?.NeverFlip ?? false
        };

        // overwrite local position to make sure we have the good one
        data.local_position = get_static_local_position();

        // get the layers by going through the hierarchy (so we can do it even when not playing)
        List<AnimLayerData> layers_data = new List<AnimLayerData>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform layer_transform = transform.GetChild(i);
            AnimLayer anim_layer = layer_transform.GetComponent<AnimLayer>();
            if (anim_layer == null) { continue; }
            layers_data.Add(anim_layer.GetStaticData());
        }

        data.layers = layers_data;
        return data;
    }
    private Vector2 get_static_local_position()
    {
        if (GetComponent<Capable>() != null)
        {
            return Vector2.zero;
            // if we have a capable on it, it means we are at the top of the capable hierarchy,
            // so our local pos is a world pos in fact. that's why we return zero, because when
            // the capable will be constructed by the CapableBank, it will receive an "anim_player"
            // transform which is a direct child of the capable. and so if we return the world pos
            // it will move the anim player FFAAAAR AWAY from the capable, which is not what we want !
        }
        return transform.localPosition;
    }
}

// ANIMATIONS
[Serializable] public class AnimData : VisualData
{
    public List<AnimCapacityPriority> anim_capacity_priorities;
    public string current_capacity; // runtime only
    public bool never_flip;

    // layers
    public List<AnimLayerData> layers;

    // CONSTRUCTOR
    public AnimData(CapacityData parent) : base(parent) { }

    // GET & DUPLICATE
    public override ICapacityData Duplicate()
    {
        AnimData new_data = new AnimData(base.Duplicate() as CapacityData)
        {
            current_capacity = this.current_capacity,
            never_flip = this.never_flip,
            anim_capacity_priorities = DuplicateACPs()
        };

        // duplicate layers
        if (this.layers != null)
        {
            new_data.layers = new List<AnimLayerData>();
            foreach (AnimLayerData ald in this.layers)
            {
                new_data.layers.Add(ald.Duplicate());
            }
        }
        else { new_data.layers = null; }

        return new_data;
    }
    public List<AnimCapacityPriority> DuplicateACPs()
    {
        if (this.anim_capacity_priorities == null) { return null; }
        List<AnimCapacityPriority> new_acps = new List<AnimCapacityPriority>();
        foreach (AnimCapacityPriority acp in this.anim_capacity_priorities)
        {
            new_acps.Add(acp.Duplicate());
        }
        return new_acps;
    }
    public override string GetDetails()
    {
        string details = $"anim_data :\n";
        details += $"     - current_capacity : {current_capacity}\n";
        if (anim_capacity_priorities != null) { details += $"     - anim_capacity_priorities : {anim_capacity_priorities.Count} priorities\n"; }
        else { details += $"     - anim_capacity_priorities : null\n"; }
        details += $"     - never_flip : {never_flip}\n";

        if (layers == null || layers.Count == 0) { details += $"     - layers : no layers\n"; }
        else
        {
            details += $"     - layers : {layers.Count} layers\n";
            for (int i = 0; i < layers.Count; i++)
            {
                details += $"          - layer {i} : " + layers[i].GetDetails() + "\n";
            }
        }
        return base.GetDetails() + details;
    }

    // REPLACE WITH
    public void ReplaceWith(AnimData new_data)
    {
        // capacity data
        this.id = new_data.id;
        this.owner_id = new_data.owner_id;
        this.kind = new_data.kind;
        this.local_position = new_data.local_position;
        this.layer = new_data.layer;
        this.tag = new_data.tag;

        // visual
        this.skin = new_data.skin;
        this.material_name = new_data.material_name;
        this.sorting_layer_id = new_data.sorting_layer_id;
        this.order_in_layer = new_data.order_in_layer;
        
        // anim
        this.current_capacity = new_data.current_capacity;
        this.never_flip = new_data.never_flip;
        this.anim_capacity_priorities = new_data.anim_capacity_priorities;

        // replace layers
        if (new_data.layers != null) { this.layers = new_data.layers; }
        else { this.layers = null; }
    }
}