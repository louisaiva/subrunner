using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI_Slot est la classe mère de tous les slots d'UI qu'on va être amené à travailler avec.
/// De cette classe dérive notamment 2 grandes classes, 
/// - UI_ImageSlot pour les slots ayant des images (ex UI_Button, UI_Toggle, UI_Item)
/// - UI_TextSlot pour les slots ayant du text (ex UI_Text)
/// </summary>
public class UI_Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler
{
    // hover
    [SerializeField] private bool _hovered = false;
    [SerializeField] private bool _disabled = false;
    public bool Hovered { get => _hovered; protected set => _hovered = value; }
    public bool Disabled { get => _disabled;
        protected set
        {
            if (_disabled == value) { return; }
            _disabled = value;
            if (_disabled)  { OnSlotDisabled?.Invoke(this); }
            else            { OnSlotEnabled?.Invoke(this); }
        }
    }

    // events
    public event Action<UI_Slot> OnSlotEnabled = delegate { };
    public event Action<UI_Slot> OnSlotDisabled = delegate { };

    [Header("Logs")]
    public bool log = false;

    // DISABLE
    public virtual void Enable()
    {
        Disabled = false;
        if (log) { Debug.Log("(UI_Slot) Enabled " + gameObject.name); }
    }
    public virtual void Disable()
    {
        if (Hovered) { OnPointerExit(null); } // on veut etre sur qu'on est pas hovered
        Disabled = true;
        if (log) { Debug.Log("(UI_Slot) Disabled " + gameObject.name); }
    }

    // POINTER HANDLERS
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_Slot) OnPointerEnter on " + gameObject.name); }

        // on met à jour le fait qu'on est survolé
        Hovered = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_Slot) OnPointerExit on " + gameObject.name); }

        // on met à jour le fait qu'on est survolé
        Hovered = false;
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_Slot) OnPointerDown on " + gameObject.name); }
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (log) { Debug.Log("(UI_Slot) OnPointerClick on " + gameObject.name); }
    }
}