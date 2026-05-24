using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Corpse is a Movable that appears when a being
/// dies. it contains a certain amount of meat,
/// a meat type & bones.
/// cats & zombies & rats can eat the corpse directly
/// (which eats the meat actually) and when there is
/// not meat it turns to bones
/// robot can directly extract all meat & bones
/// </summary>

public class Corpse : Food
{
    [Header("CORPSE SETTINGS")]
    [SerializeField] private int random_meat_modifier_at_start = 5; // meat_amount += random.range(-5,5) in the start method if this modifier = 5
    [SerializeField] private bool log_bites = false;

    // INIT
    public void Initialize(Capable capable)
    {
        // we calculate how much meat we want to put inside the meat
        int meat_qty = 2 + UnityEngine.Random.Range(-random_meat_modifier_at_start, random_meat_modifier_at_start);
        if (meat_qty < 1) { meat_qty = 1; }
        int bones_qty = 2 + UnityEngine.Random.Range(-1, 1);
        if (bones_qty < 1) { bones_qty = 1; }

        // we understand which meat type we want
        string meat_reference = "food:meat";
        if (capable is Zombo) { meat_reference = "food:meat_zombo"; }

        // we instantiate & grab x meat
        for (int i = 0; i < meat_qty; i++)
        {
            Food meat = ItemBank.Instance.CreateItem(meat_reference) as Food;
            if (meat == null) { continue; }
            meat.OnBeingBitten += being_bitten;
            Inventory.Grab(meat);
        }

        // and x bones
        for (int i = 0; i < bones_qty; i++)
        {
            Item bone = ItemBank.Instance.CreateItem("other:bone");
            if (bone == null) { continue; }
            Inventory.Grab(bone);
        }

        if (log_bites) { Debug.Log($"(Corpse) Initialized corpse of {capable.name} with {meat_qty} meat & {bones_qty} bones."); }
    }

    // EXTRACT MEAT
    /* public List<Item> ExtractMeatAndBones()
    {
        List<Item> extracted_items = new List<Item>();
        extracted_items.AddRange(Inventory.GetItemsByType<Food>());
        // extracted_items.AddRange(Inventory.GetItemsByType<Bone>());
        return extracted_items;
    } */
    public Food GetPortion(string food_rule)
    {
        // we check if we still have some food
        List<Item> meats = Inventory.GetItemsByRule(food_rule);
        if (meats.Count == 0) { return null; }

        // we eat the first meat
        Item meat_to_eat = meats[0];
        return meat_to_eat as Food;
    }


    // ON BEING BITTEN & BECOME BONES
    private async void being_bitten(HealthCapacity eater)
    {
        if (log_bites) { Debug.Log("(Corpse) " + name + " has been bitten by " + eater.name); }

        // we wait a frame for the bite to happen
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();

        int food_left = Inventory.GetItemsByType<Food>().Count;
        int bones_left = Inventory.GetItemsByRule("other:bone").Count;

        if (log_bites) { Debug.Log($"(Corpse) {name} has {food_left} food & {bones_left} bones left after being bitten by {eater.name}"); }

        // we check how many bones we have left
        if (bones_left == 0)
        {
            // we do not exist anymore... we delete ourselves
            Destroy(gameObject);
            return;
        }

        // if we still have food we good !
        if (food_left > 0) { return; }

        // else we have no more food, we become bones
        become_bones();
    }
    private void become_bones()
    {
        if (log_bites) { Debug.Log("(Corpse) " + name + " becoming bones!"); }
        if (Skin.Contains("bone")) { return; } // if we are already bones, we do nothing
        
        // StopAllCoroutines(); // we stop all coroutines to avoid any issues

        // we destroy the body child
        Destroy(transform.Find("feet").gameObject);

        // and change our skin to bones
        AnimPlayer.Skin = "bones";
        AnimPlayer.ClearPile();
        // Reference = "food:bones";
        // ItemDescription = "just some bones./. nothing special here.";
    }
}


// CORPSE DATA
[Serializable] public class CorpseData : ItemData
{

    private static ItemData item_info = new ItemData()
    {
        reference = "corpse:unknown",
        color = Color.softRed,
        max_qty = 12,
        item_description = "a corpse of unknown. it seems to contain some meat and bones. smells bad. beurk -o-",
        is_grabbed = false,
    };

    // INIT FROM CAPABLE
    public override void InitFromCapable(CapableData capdata, ItemData item_data = null)
    {
        string entity_type = capdata.kind.ToLowerInvariant();
        // modify the item info with out entity type
        item_info.reference = "corpse:" + entity_type;
        item_info.item_description = item_info.item_description.Replace("unknown", entity_type);

        base.InitFromCapable(capdata, item_info);
        
        // reset item info reference for future uses
        item_info.reference = "corpse:unknown";
        item_info.item_description = item_info.item_description.Replace(entity_type, "unknown");
    }

    // CONSTRUCTOR
    public CorpseData(CapableData parent) : base(parent) { }

    // DUPLICATE
    public override ICapableData Duplicate()
    {
        ItemData duplicated = base.Duplicate() as ItemData;
        if (duplicated == null) { return null; }

        return new CorpseData(duplicated)
        {
            // we don't need to duplicate anything else for now, but if we add corpse specific data in the future we will need to duplicate it here
        };
    }
}