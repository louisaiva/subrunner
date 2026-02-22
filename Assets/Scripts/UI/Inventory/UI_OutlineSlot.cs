using TMPro;
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
    
    
    // DISABLE
    public override void Enable()
    {
        if (!Disabled) { return; }

        // on active le drag here text
        drag_here_text.gameObject.SetActive(true);

        base.Enable();
    }
    public override void Disable()
    {
        if (Disabled) { return; }

        // on remet une icon null
        set_icon(null);

        // on desactive le drag here text
        drag_here_text.gameObject.SetActive(false);

        base.Disable();
    }

    // RECEIVE
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
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // on remet une icon null
        set_icon(null);

        // si on est pas disabled on active le drag here text
        if (!Disabled) { drag_here_text.gameObject.SetActive(true); }
    }
    
    // SET ICON
    protected void set_icon(Sprite sprite)
    {
        if (icon_image == null) { return; }
        icon_image.sprite = sprite;
        icon_image.enabled = sprite != null;
    }
}