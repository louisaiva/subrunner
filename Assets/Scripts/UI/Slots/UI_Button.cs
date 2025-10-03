using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Button : UI_Slot
{

    [Header("Button Settings")]
    [SerializeField] protected TextMeshProUGUI label;
    [SerializeField] protected Vector2 text_movement = new Vector2(0f, -2f); // how much to move the text when pressed

    /* public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // we reset the text position
        label.rectTransform.anchoredPosition = Vector2.zero;
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // we reset the text position
        label.rectTransform.anchoredPosition = Vector2.zero;
    } */
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // we move the text position
        label.rectTransform.anchoredPosition += text_movement;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        // Call the base class method
        base.OnPointerClick(eventData);

        // we reset the text position
        label.rectTransform.anchoredPosition = Vector2.zero;

        // Additional button-specific logic
        Debug.Log("Button clicked: " + gameObject.name);
    }
}