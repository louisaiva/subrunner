using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemRenderer : MonoBehaviour
{
    [Header("UI_Item Target")]
    public UI_Item Target;

    [Header("Components")]
    [SerializeField] private Image item;
    [SerializeField] private TextMeshProUGUI qty;

    private void Awake()
    {
        if (Target == null)
        {
            Debug.LogWarning("(UI_ItemRenderer) Target is not set on " + name);
            return;
        }

        // subscribe to the event
        Target.OnItemChanged += UpdateItemDisplay;
    }
    private void UpdateItemDisplay(List<Item> items)
    {
        if (Target == null) { return; }

        item.sprite = Target.ItemSprite;
        if (item.sprite == null) { item.color = Color.clear; }
        else { item.color = Color.white; }

        // we show or hide the text
        qty.text = Target.Quantity.ToString();
        qty.gameObject.SetActive(Target.Quantity > 1);
    }

    // SETTING NEW TARGET
    public void SetTarget(UI_Item new_Target)
    {
        // we unsubscribe from the old Target
        if (Target != null)
        {
            Target.OnItemChanged -= UpdateItemDisplay;
        }

        // we set the new Target
        Target = new_Target;
        if (Target == null) { return; }

        // we subscribe to the new Target
        Target.OnItemChanged += UpdateItemDisplay;
        UpdateItemDisplay(Target.GetItems()); // we update the display
    }
}