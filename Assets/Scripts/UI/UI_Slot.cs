using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Slot : MonoBehaviour, I_UI_Slot
{

    // hover
    public bool is_hovered { get; set; }
    public bool is_disabled { get; set; }

    [Header("Sprites")]
    public Sprite base_sprite;
    public Sprite hover_sprite;
    public Sprite down_sprite;
    public Sprite disabled_sprite;

    [Header("Logs")]
    public bool debug = false;

    // DISABLE
    public void Enable()
    {
        is_disabled = false;

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;
    }
    public void Disable()
    {
        is_disabled = true;

        // on change le sprite du slot
        GetComponent<Image>().sprite = disabled_sprite;
    }

    // POINTER HANDLERS
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = hover_sprite;


        // on met à jour le fait qu'on est survolé
        is_hovered = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = base_sprite;

        // on met à jour le fait qu'on est survolé
        is_hovered = false;
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        if (debug) { Debug.Log("OnPointerDown on " + gameObject.name); }

        // on change le sprite du slot
        GetComponent<Image>().sprite = down_sprite;
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // check if disabled
        if (is_disabled) { return; }

        // on change le sprite du slot
        GetComponent<Image>().sprite = is_hovered ? hover_sprite : base_sprite;
    }


    // ! DEPRECATED

    public string getDescription()
    {
        throw new NotImplementedException();
    }
    public bool shouldDescriptionBeShown()
    {
        throw new NotImplementedException();
    }
}