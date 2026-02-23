using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI_OutlineSlot is a specific slot that outlines an UI_ItemPool (or UI_CompactItemPool).
/// It is used for 2 things :
/// - showing an "empty message" kind of when the pool has no items.
/// - receive moving ui_itemstack when on a UI_CompactItemPool (with regular UI_ItemPool we don't need it since the ui_itemstack can grab)
/// </summary>
public class UI_OutlineSlot : UI_ImageSlot, ItemReceivable
{
    public Sprite drag_hover_sprite;

    [Header("Drag here text")]
    [SerializeField] private TextMeshProUGUI drag_here_text;

    [Header("Icon image")]
    [SerializeField] protected Image icon_image;
    [SerializeField] protected string receive_icon_name;
    
    [Header("Outline helpers")]
    [SerializeField] private List<UI_OutlineHelper> outline_helpers;

    // EVENTS 
    public Action<Item> OnReceivedItem = delegate { };

    
    // START
    protected void Start()
    {
        // we find the outline helpers
        Transform outlines_transform = transform.Find("outlines");
        if (outlines_transform == null) { return; }
        for (int i = 0; i < outlines_transform.childCount; i++)
        {
            UI_OutlineHelper helper = outlines_transform.GetChild(i).GetComponent<UI_OutlineHelper>();
            if (helper != null) { outline_helpers.Add(helper); }
        }

        // we disable the slot at the start
        Disable();
    }

    // ON RECEIVED
    public void OnReceived(ItemStack moving_ui_item)
    {
        if (log) { Debug.Log($"(UI_OutlineSlot) received itemstack with {moving_ui_item.Quantity} items of type {moving_ui_item.ItemReference}, calling OnReceivedItem delegate"); }
        for (int i = 0; i < moving_ui_item.Quantity; i++)
        {
            Item item = moving_ui_item.Item;
            OnReceivedItem?.Invoke(item);
        }
    }

    // DISABLE
    public override void Enable()
    {
        if (!Disabled) { return; }

        // on active le drag here text
        drag_here_text.gameObject.SetActive(true);

        // on active "draggable" des outlines helpers
        for (int i = 0; i < outline_helpers.Count; i++) { outline_helpers[i].SetDraggable(); }

        base.Enable();
    }
    public override void Disable()
    {
        // we find the outline helpers if we don't have any yet
        // if (outline_helpers.Count == 0) { get_outline_helpers(); }

        if (Disabled) { return; }

        // on remet une icon null
        set_icon(null);

        // on desactive le drag here text
        drag_here_text.gameObject.SetActive(false);

        // on active "disabled" des outlines helpers
        for (int i = 0; i < outline_helpers.Count; i++) { outline_helpers[i].SetDisabled(); }

        base.Disable();
    }

    // DRAG
    public virtual void OnPointerDragEnter(UI_ItemStack moving_ui_item)
    {
        // check if disabled
        if (Disabled) { return; }
        if (log) { Debug.Log("OnPointerDragEnter on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = drag_hover_sprite;

        // on met une icon dans le holder
        set_icon(ItemBank.Instance.GetUI_Icon(receive_icon_name));

        // on desactive le drag here text (on a une icon déjà)
        drag_here_text.gameObject.SetActive(false);

        // on active "dragged hover" des outlines helpers
        for (int i = 0; i < outline_helpers.Count; i++) { outline_helpers[i].SetDraggedHover(); }

    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // on remet une icon null
        set_icon(null);

        // si on est pas disabled on active le drag here text
        if (!Disabled)
        {
            drag_here_text.gameObject.SetActive(true);

            // on active "draggable" des outlines helpers
            for (int i = 0; i < outline_helpers.Count; i++) { outline_helpers[i].SetDraggable(); }
        }
    }
    
    // SET ICON
    protected void set_icon(Sprite sprite)
    {
        if (icon_image == null) { return; }
        icon_image.sprite = sprite;
        icon_image.enabled = sprite != null;
    }

    // OUTLINE HELPERS
    private void get_outline_helpers()
    {
        Transform outlines_transform = transform.Find("outlines");
        if (outlines_transform == null) { return; }
        for (int i = 0; i < outlines_transform.childCount; i++)
        {
            UI_OutlineHelper helper = outlines_transform.GetChild(i).GetComponent<UI_OutlineHelper>();
            if (helper != null) { outline_helpers.Add(helper); }
        }
    }
}