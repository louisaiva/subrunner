using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("UI_ItemRenderer is deprecated, use UI_Item instead, which is kind of a renderer for ItemStack now")]
public class UI_ItemRenderer : MonoBehaviour
{
    [Header("UI_Item Target")]
    public UI_Item Target;

    [Header("Components")]
    [SerializeField] private Image item;
    [SerializeField] private TextMeshProUGUI qty;

    [Header("Colors & Feedbacks")]
    [SerializeField] private InputImageFeedback feedback; // optionnel
    private float color_shift = 60f;

    [Header("Log")]
    public bool log = false;

    private void Awake()
    {
        if (Target == null)
        {
            if (log) { Debug.LogWarning("(UI_ItemRenderer) Target is not set on " + name); }
            return;
        }

        // subscribe to the event
        // Target.OnItemChanged += UpdateItemDisplay;
    }

    // SETTING NEW TARGET
    /* public void SetTarget(UI_Item new_Target)
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
    } */

    // UPDATE DISPLAY
    /* private void UpdateItemDisplay(List<Item> items)
    {
        if (Target == null) { return; }

        item.sprite = Target.ItemSprite;
        if (item.sprite == null) { item.color = Color.clear; }
        else { item.color = Color.white; }

        // we show or hide the text
        qty.text = Target.Quantity.ToString();
        qty.gameObject.SetActive(Target.Quantity > 1);

        // we apply the color & color shift to the feedback
        if (feedback == null) { return; }

        Color base_color = Target.Item != null ? Target.Item.Color : Color.white;
        Color shifted_color = new Color(
                Mathf.Clamp01(base_color.r + (color_shift / 255f)),
                Mathf.Clamp01(base_color.g + (color_shift / 255f)),
                Mathf.Clamp01(base_color.b + (color_shift / 255f))
            );
        feedback.SetColors(base_color, shifted_color);
    } */
}