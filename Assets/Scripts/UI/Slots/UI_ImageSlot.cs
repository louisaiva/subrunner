using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ImageSlot : UI_Slot
{

    [Header("Components")]
    public Image image;
    
    [Header("Sprites")]
    public Sprite base_sprite;
    public Sprite hover_sprite;
    public Sprite down_sprite;
    public Sprite disabled_sprite;




    // DISABLE
    public override void Enable()
    {
        // on change le sprite du slot
        image.sprite = base_sprite;
        base.Enable();
    }
    public override void Disable()
    {
        base.Disable();
        
        // on change le sprite du slot
        image.sprite = disabled_sprite;
    }

    // POINTER HANDLERS
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_ImageSlot) OnPointerEnter on " + gameObject.name); }

        // on change le sprite du slot
        image.sprite = hover_sprite;

        // on met à jour le fait qu'on est survolé
        Hovered = true;
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_ImageSlot) OnPointerExit on " + gameObject.name); }

        // on change le sprite du slot
        if (!Disabled) { image.sprite = base_sprite; }

        // on met à jour le fait qu'on est survolé
        Hovered = false;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_ImageSlot) OnPointerDown on " + gameObject.name); }

        // on change le sprite du slot
        image.sprite = down_sprite;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_ImageSlot) OnPointerClick on " + gameObject.name); }

        // on change le sprite du slot
        image.sprite = Hovered ? hover_sprite : base_sprite;
    }
}