using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Module : UI_ItemStack
{
    [Header("Drag Module")]
    [SerializeField] private Image icon_image;
    [SerializeField] private TextMeshProUGUI helper_text;
    [SerializeField] private Color drag_hover_color = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    // public override int MaxQty => 1; // modules are not stackable when showed on a motherboard

    private UI_ModulePool _module_pool;
    public UI_ModulePool ModulePool
    {
        get
        {
            if (_module_pool == null) { _module_pool = UI_ItemPool as UI_ModulePool; }
            return _module_pool;
        }
    }

    // AWAKE
    public override void Init(ItemStack stack)
    {
        base.Init(stack);
        icon_image = transform.Find("help/icon").GetComponent<Image>();
        helper_text = transform.Find("help/helper_text").GetComponent<TextMeshProUGUI>();
    }

    // ENABLING
    public override void Enable()
    {
        // checks if we can enable
        if (!ModulePool.HasDevice) { return; }
        base.Enable();
    }

    // ITEM SETTING
    protected override void update_ui_item()
    {
        // on charge le sprite de l'image
        if (Stack.Quantity == 0)
        {
            clear_ui_item();
            return;
        }

        // on charge le sprite de l'image
        current_item_sprite = ItemBank.Instance.GetModuleSprite(Stack.Item.Reference);
        set_ui_item(current_item_sprite);

        // on change le nom du prefab
        name = "ui_" + Stack.ItemReference;

        // on enable le slot
        Enable();
    }
    protected override void set_ui_item(Sprite sprite)
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
        if (Disabled) { return; }
        if (log) { Debug.Log("OnPointerEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = hover_sprite;

        // on met à jour le fait qu'on est survolé
        Hovered = true;

        // on met à jour la description si y'en a une
        // update_description();

        // si on a un Item on baisse l'alpha à 0.5
        if (Stack.Item != null) { item_image.color = new Color(1, 1, 1, 0.5f); }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        // check if disabled
        if (Disabled) { return; }
        if (log) { Debug.Log("OnPointerExit on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        Hovered = false;

        // on remet l'alpha de l'image à 1
        if (Stack.Item != null) { item_image.color = new Color(1, 1, 1, 1); }

        hide_icon();
    }

    // DRAGGING
    public override void OnPointerDragEnter(UI_Item moving_ui_item)
    {
        // check if disabled
        if (Disabled) { return; }
        if (log) { Debug.Log("OnPointerDragEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_hover_sprite;

        // si on a un Item on baisse l'alpha à 0.5
        if (Stack.Item != null) { item_image.color = drag_hover_color; }

        // on récupère quelle reference d'icon on doit mettre
        string icon_ref = "";
        if (Stack.Item == null) { icon_ref = "screw"; }
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
        icon_image.sprite = ItemBank.Instance.GetUI_Icon(icon_ref);
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
    /* public override void SwitchItems(ItemStack new_stack)
    {
        base.SwitchItems(new_stack);
        // on met à jour la position dans le laptop inventory
        ModulePool.OnModuleMoved(this);
    } */
}