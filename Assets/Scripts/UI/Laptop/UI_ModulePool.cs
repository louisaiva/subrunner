using PrimeTween;
using UnityEngine;

/// <summary>
/// UI_ModulePool is a helper class to manage the module pool in the UI_Laptop
/// it directly inherits from UI_ItemPool
/// and is used to manage the modules in the laptop
/// </summary>
public class UI_ModulePool : UI_ItemPool
{
    [Header("Motherboard Transition")]
    [SerializeField] private RectTransform motherboard;
    [SerializeField] private Vector2 motherboard_LR_when_highlighted = new Vector2(0.25f, 0.85f);
    [SerializeField] private Vector2 motherboard_LR_base = new Vector2(0.75f, 1.35f);

    [Header("Inventory Transition")]
    [SerializeField] private RectTransform item_pools;
    [SerializeField] private Vector2 inventory_LR_base = new Vector2(0, 1f);
    [SerializeField] private Vector2 inventory_LR_when_mb = new Vector2(-0.75f, 1f);

    public void DropOverheadSlots()
    {
        // we check if we have too many slots
        if (transform.childCount <= MaxSlots) { return; }

        // we first try to remove the empty slots
        DestroyEmptySlots();
        if (transform.childCount <= MaxSlots)
        {
            // if we are not scalable we create back some empty slots to match max slots
            if (!Scalable) { CreateEmptySlots(MaxSlots - transform.childCount); }
            return;
        }

        // we only have full slots, but we still have too many slots
        // so we drop the last slots items and remove their slots

        // todo do this bcz for now we only remove them

        for (int i = transform.childCount; i > MaxSlots; --i)
        {
            // we get the last slot
            Transform last_slot = transform.GetChild(i - 1);
            UI_Item ui_item = last_slot.GetComponent<UI_Item>();
            if (ui_item == null) { continue; }

            // we unstore the item
            if (ui_item.Quantity > 0)
            {
                Item item = ui_item.Item;
                ui_item.Unstore(item);
            }

            // we destroy the slot
            Destroy(last_slot.gameObject);
        }
    }
    public override GameObject CreateItemSlot(Item item = null)
    {
        // we create the item
        GameObject ui_slot = bank.CreateUI_Module();
        ui_slot.transform.SetParent(transform);

        // reset the scale to 1
        ui_slot.transform.localScale = Vector3.one;

        // we change the layer of the slot to the same as the pool
        ui_slot.layer = gameObject.layer;

        UI_Item ui_item = ui_slot.GetComponent<UI_Item>();
        ui_item.Init();

        // we assign the item to the UI_Item
        if (item != null) { ui_item.Store(item); }
        else { ui_item.ClearUI(); }

        // we add the item to the list
        ui_items.Add(ui_item);

        if (debug) { Debug.Log($"(UI_ModulePool) created an ui_module with item {(item == null ? "null" : item.Reference)}"); }

        return ui_slot;
    }

    public async override void Fade(float duration = 0.1f, bool fade_in = true)
    {
        // we get our sibling and make it fade
        // TextMeshProUGUI sibling = transform.parent.Find("title").GetComponent<TextMeshProUGUI>();

        // get screen
        // Vector2 right_destination = fade_in ? motherboard_LR_when_highlighted*Screen.width : Vector2.zero;
        // Vector2 left_destination = fade_in ? motherboard_LR_when_highlighted*Screen.width : Vector2.zero;


        // CanvasGroup group = GetComponentInParent<CanvasGroup>();
        await Sequence.Create(useUnscaledTime: true)
            .Group(Tween.Custom(fade_in ? 0f : 1f, fade_in ? 1f : 0f, duration: duration,
                onValueChange: ctx => group.alpha = ctx))
            .Group(Tween.Custom(fade_in ? motherboard_LR_base.x : motherboard_LR_when_highlighted.x,
                                fade_in ? motherboard_LR_when_highlighted.x : motherboard_LR_base.x,
                                duration: duration,
                onValueChange: ctx => motherboard.anchorMin = new Vector2(ctx, motherboard.anchorMin.y)))
            .Group(Tween.Custom(fade_in ? motherboard_LR_base.y : motherboard_LR_when_highlighted.y,
                                fade_in ? motherboard_LR_when_highlighted.y : motherboard_LR_base.y,
                                duration: duration,
                onValueChange: ctx => motherboard.anchorMax = new Vector2(ctx, motherboard.anchorMax.y)))
            .Group(Tween.Custom(fade_in ? inventory_LR_base.x : inventory_LR_when_mb.x,
                                fade_in ? inventory_LR_when_mb.x : inventory_LR_base.x,
                                duration: duration,
                onValueChange: ctx => item_pools.anchorMin = new Vector2(ctx, item_pools.anchorMin.y)))
            .Group(Tween.Custom(fade_in ? inventory_LR_base.y : inventory_LR_when_mb.y,
                                fade_in ? inventory_LR_when_mb.y : inventory_LR_base.y,
                                duration: duration,
                onValueChange: ctx => item_pools.anchorMax = new Vector2(ctx, item_pools.anchorMax.y)));
    }
}