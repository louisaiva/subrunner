using UnityEngine;
using UnityEngine.UI;

public class UI_OutlineHelper : MonoBehaviour
{
    [Header("Image")]
    private Image _image;
    protected Image image
    {
        get
        {
            if (_image == null) { _image = GetComponent<Image>(); }
            return _image;
        }
    }

    [Header("Sprites")]
    [SerializeField] private Sprite base_sprite;
    [SerializeField] private Sprite disabled;
    [SerializeField] private Sprite draggable;
    [SerializeField] private Sprite dragged_hover;

    // SET SPRITE
    public void SetBase() { set_sprite(base_sprite); }
    public void SetDisabled() { set_sprite(disabled); }
    public void SetDraggable() { set_sprite(draggable); }
    public void SetDraggedHover() { set_sprite(dragged_hover); }

    protected void set_sprite(Sprite sprite)
    {
        if (image == null) { return; }
        image.sprite = sprite;
    }
}