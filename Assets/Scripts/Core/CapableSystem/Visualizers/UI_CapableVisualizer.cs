using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This UI component is a visualizer for CapableData.
/// It is a UI_Slot (which allows us to navigate through capables with a controller/mouse)
/// and also a Descriptable, which means it can show a description when hovered or selected.
/// It also stores a reference to the CapableData it is visualizing, to be able to update the description and other info when needed.
/// </summary>
public class UI_CapableVisualizer : UI_Slot, Descriptable
{
    // capable data
    [HideInInspector] public CapableData capable_data;
    private CapableSpriteIcon sprite_icon;

    // UI refs
    private Image _image;
    private Image image
    {
        get
        {
            if (_image == null) { _image = GetComponent<Image>(); }
            return _image;
        }
    }

    // INIT / ON DESTROY
    public void Init(CapableData data, CapableSpriteIcon icon)
    {
        capable_data = data;
        sprite_icon = icon;
        image.sprite = icon.sprite;

        // set the position
        on_capable_position_changed(data);

        // we register to the capable data events to update the visu when the data changes
        capable_data.OnPositionChanged += on_capable_position_changed;


        // we check if we are an item (so we have an ItemData), we handle the grab/drop thing
        // 1. should not appear if is grabbed
        // 2. should update when the item is grabbed/dropped
        if (capable_data is not ItemData item_data) { return; }
        if (item_data.is_grabbed)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f); // make the image invisible
            Disable();
        }

        item_data.OnGrabbed += handle_item_grabbed;
        item_data.OnDropped += handle_item_dropped;
    }
    private void OnDestroy()
    {
        // we unregister from the capable data events to avoid memory leaks
        if (capable_data == null) { return; }
        capable_data.OnPositionChanged -= on_capable_position_changed;

        if (capable_data is not ItemData item_data) { return; }
        item_data.OnGrabbed -= handle_item_grabbed;
        item_data.OnDropped -= handle_item_dropped;
    }

    // CAPABLE POSITION UPDATE
    private void on_capable_position_changed(CapableData data)
    {
        // when the capable changes position, we update the visu position
        image.rectTransform.anchoredPosition = UI_DevMap.GetUIPositionFromWorldPosition(data.position);
    }

    // ITEM GRAB/DROP HANDLERS
    private void handle_item_grabbed(ItemData item_data)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f); // make the image invisible
        Disable();
    }
    private void handle_item_dropped(ItemData item_data)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f); // make the image visible
        Enable();
    }

    // DESCRIPTION
    public string Name => throw new System.NotImplementedException();
    public string Description => throw new System.NotImplementedException();
}