using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Slot : MonoBehaviour, I_UI_Slot
{

    // hover
    [SerializeField] public bool is_hovered { get; set; }
    [SerializeField] public bool is_disabled { get; set; }

    [Header("Logs")]
    public bool log = false;

    [Header("Sprites")]
    public Sprite base_sprite;
    public Sprite hover_sprite;
    public Sprite down_sprite;
    public Sprite disabled_sprite;

    [Header("Components")]
    public Image image;


    // DISABLE
    public virtual void Enable()
    {
        if ( image == null ) { image = GetComponent<Image>(); }
        is_disabled = false;

        // on change le sprite du slot
        image.sprite = base_sprite;

        if (log) { Debug.Log("(UI_Item) Enabled " + gameObject.name); }
    }
    public virtual void Disable()
    {
        if (image == null) { image = GetComponent<Image>(); }
        is_disabled = true;

        // on change le sprite du slot
        image.sprite = disabled_sprite;

        if (log) { Debug.Log("(UI_Item) Disabled " + gameObject.name); }
    }

    // POINTER HANDLERS
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        if (log) { Debug.Log("OnPointerEnter on " + gameObject.name); }
        // on change le sprite du slot
        image.sprite = hover_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        if (log) { Debug.Log("OnPointerExit on " + gameObject.name); }

        // on change le sprite du slot
        image.sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = false;
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        if (log) { Debug.Log("OnPointerDown on " + gameObject.name); }

        // on change le sprite du slot
        image.sprite = down_sprite;
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        image.sprite = is_hovered ? hover_sprite : base_sprite;
    }

}