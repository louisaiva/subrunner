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
    // private CapableSpriteIcon sprite_icon;

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
        // sprite_icon = icon;
        image.sprite = icon.sprite;

        // set the position
        on_capable_position_changed(data);

        // we register to the capable data events to update the visu when the data changes
        capable_data.OnPositionChanged += on_capable_position_changed;
    }
    private void OnDestroy()
    {
        // we unregister from the capable data events to avoid memory leaks
        if (capable_data == null) { return; }
        capable_data.OnPositionChanged -= on_capable_position_changed;
    }

    // CAPABLE POSITION UPDATE
    private void on_capable_position_changed(CapableData data)
    {
        // when the capable changes position, we update the visu position
        image.rectTransform.anchoredPosition = UI_DevMap.GetUIPositionFromWorldPosition(data.position);
    }

    // DESCRIPTION
    public string Name => throw new System.NotImplementedException();
    public string Description => throw new System.NotImplementedException();
}