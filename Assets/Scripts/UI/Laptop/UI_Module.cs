using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Module : UI_Item
{
    [Header("Drag Module")]
    [SerializeField] private Image icon_image;
    [SerializeField] private TextMeshProUGUI helper_text;
    [SerializeField] private Color drag_hover_color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    public override int MaxQty => 1; // modules are not stackable when showed on a motherboard

    // AWAKE
    public override void Init()
    {
        base.Init();
        icon_image = transform.Find("help/icon").GetComponent<Image>();
        helper_text = transform.Find("help/helper_text").GetComponent<TextMeshProUGUI>();
    }

    // ITEM SETTING
    protected override void setItem(Item item)
    {
        // on charge le sprite de l'image
        current_item_sprite = bank.GetModuleSprite(item.Reference);
        set_ui(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Reference;

        // on enable le slot
        Enable();
    }
    protected override void set_ui(Sprite sprite)
    {
        if (item_image == null)
        {
            Debug.LogWarning($"(UI_Module) item_image of {name} is null, cannot set UI");
            return;
        }
        item_image.sprite = sprite;
        item_image.color
                = sprite != null
                ? new Color(1, 1, 1, 1)
                : new Color(0, 0, 0, 0);
        if (sprite == null) { return; }
    }

    // ON POINTER ENTER/EXIT
    public override void OnPointerEnter(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }
        if (log) { Debug.Log("OnPointerEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = hover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = true;

        // on met à jour la description si y'en a une
        update_description();

        // si on a un Item on baisse l'alpha à 0.5
        if (Item != null) { item_image.color = new Color(1, 1, 1, 0.5f); }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }
        if (log) { Debug.Log("OnPointerExit on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = false;

        // on remet l'alpha de l'image à 1
        if (Item != null) { item_image.color = new Color(1, 1, 1, 1); }

        hide_icon();
    }

    // ON POINTER CLICK
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (Item == null) { return; }
        if (Item.Reference == "module:hdd")
        {
            // we open the HDD info
            StoreCapacity disk = Item.GetCapacity<StoreCapacity>();
            if (disk == null) { return; }
            if (log) { Debug.Log($"(UI_Module) {name} clicked, opening HDD info"); }
            UI_Manager.Instance.GetPool("hdd").gameObject.GetComponent<UI_HDD>().SetDisk(disk);
            UI_Manager.Instance.StackPool("hdd");
            return;
        }
    }

    // DRAGGING
    public override void OnPointerDragEnter(UI_Item moving_ui_item)
    {
        // check if disabled
        if (is_disabled) { return; }
        if (log) { Debug.Log("OnPointerDragEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_hover_sprite;

        // si on a un Item on baisse l'alpha à 0.5
        if (Item != null) { item_image.color = drag_hover_color; }

        // on récupère quelle reference d'icon on doit mettre
        string icon_ref = "";
        if (Item == null) { icon_ref = "screw"; }
        // else if (Reference == moving_ui_item.Reference && Quantity < MaxQty) { icon_ref = "upgrade"; }
        else { icon_ref = "switch"; }
        set_icon(icon_ref);
    }
    private void set_icon(string icon_ref)
    {
        if (icon_image == null)
        {
            Debug.LogWarning($"(UI_Module) icon_image of {name} is null, cannot set UI Icon {icon_ref}");
            return;
        }
        icon_image.sprite = bank.GetUI_Icon(icon_ref);
        icon_image.color = new Color(1, 1, 1, 1);

        // sets the text
        helper_text.text = icon_ref switch
        {
            "screw" => "install\nmodule",
            "switch" => "switch\nmodules",
            "upgrade" => "upgrade\nmodule",
            _ => ""
        };
    }
    private void hide_icon()
    {
        if (icon_image == null)
        {
            Debug.LogWarning($"(UI_Module) icon_image of {name} is null, cannot hide icon");
            return;
        }
        icon_image.sprite = null;
        icon_image.color = new Color(0, 0, 0, 0);
        helper_text.text = "";
    }
    public override void SwitchItems(List<Item> items, bool items_moved = true)
    {
        base.SwitchItems(items, items_moved);
        if (!items_moved) { return; }

        // on met à jour la position dans le laptop inventory
        (ItemPool as UI_ModulePool)?.OnModuleMoved(this);
    }

}