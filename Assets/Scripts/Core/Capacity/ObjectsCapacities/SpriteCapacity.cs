using System;
using System.Collections.Generic;
using UnityEngine;


public class SpriteCapacity : Capacity
{
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer sr
    {
        get
        {
            if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
            return _spriteRenderer;
        }
    }

    [Header("Randoms sprites")]
    [SerializeField] private List<Sprite> random_sprites = new List<Sprite>();

    ///
    //
    /// DATA MANAGEMENT
    //
    ///

    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data, CapableData owner)
    {
        base.LoadData(data, owner);
        if (data is not SpriteData sdata) { return; }

        // load sprite
        sr.material = MaterialBank.GetMaterial(sdata.material_name);
        sr.sortingLayerID = sdata.sorting_layer_id;
        sr.sortingOrder = sdata.order_in_layer;
        random_sprites = new List<Sprite>(sdata.random_sprites);

        // assign the sprite
        sr.sprite = sdata.sprite;
        if (random_sprites.Count > 0)
        {
            int index = UnityEngine.Random.Range(-1, random_sprites.Count);
            if (index >= 0) { sr.sprite = random_sprites[UnityEngine.Random.Range(0, random_sprites.Count)]; }
        }
    }

    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        SpriteData static_data = new SpriteData(base.GetStaticData())
        {
            sprite = sr.sprite,
            material_name = MaterialBank.GetMaterialName(sr, Capable.ID),
            sorting_layer_id = sr.sortingLayerID,
            order_in_layer = sr.sortingOrder
        };
        if (random_sprites.Count > 0)
        {
            static_data.random_sprites = new List<Sprite>(random_sprites);
        }
        return static_data;
    }
}


[Serializable] public class SpriteData : CapacityData
{
    public Sprite sprite;
    public List<Sprite> random_sprites;
    public string material_name;
    public int sorting_layer_id;
    public int order_in_layer;

    public SpriteData(CapacityData data) : base(data)
    {
        random_sprites = new List<Sprite>();
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new SpriteData(base.Duplicate() as CapacityData)
        {
            sprite = this.sprite,
            random_sprites = new List<Sprite>(this.random_sprites),
            material_name = this.material_name,
            sorting_layer_id = this.sorting_layer_id,
            order_in_layer = this.order_in_layer
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        details += $"  - sprite : {sprite}\n";
        details += $"  - random_sprites : {string.Join(", ", random_sprites)}\n";
        details += $"  - material_name : {material_name}\n";
        details += $"  - sorting_layer_id : {sorting_layer_id}\n";
        details += $"  - order_in_layer : {order_in_layer}\n";
        return base.GetDetails() + details;
    }
}
