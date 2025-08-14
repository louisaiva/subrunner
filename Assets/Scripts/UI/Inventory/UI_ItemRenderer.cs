using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemRenderer : MonoBehaviour
{
    [Header("UI_Item Target")]
    [SerializeField] private UI_Item target;

    [Header("Components")]
    [SerializeField] private Image item;
    [SerializeField] private TextMeshProUGUI qty;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("(UI_ItemRenderer) Target is not set on " + name);
            return;
        }

        // subscribe to the event
        target.OnItemChanged += UpdateItemDisplay;
    }

    private void UpdateItemDisplay(List<Item> items)
    {
        if (target == null) return;

        item.sprite = target.ItemSprite;
        if (item.sprite == null) { item.color = Color.clear; }
        else { item.color = Color.white; }

        // we show or hide the text
        qty.text = target.Quantity.ToString();
        qty.gameObject.SetActive(target.Quantity > 1);
    }
}