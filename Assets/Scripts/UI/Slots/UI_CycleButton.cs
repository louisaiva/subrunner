using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_CycleButton : UI_Button, Colorant
{


    [SerializeField] protected Image btn_icon;

    [Header("Cycle Settings")]
    [SerializeField]
    private List<CycleButtonData> cycle_datas = new List<CycleButtonData>();
    private int current_cycle_index = 0;
    private CycleButtonData current_cycle => cycle_datas[current_cycle_index];


    [Header("Colorers")]
    public List<UI_Colorer> colorers = new List<UI_Colorer>();
    public List<UI_Colorer> Colorers => colorers;

    [Header("Labels")]
    private List<TextMeshProUGUI> _labels = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> Labels
    {
        get
        {
            if (_labels.Count == 0)
            {
                foreach (UI_Colorer colorer in colorers)
                {
                    TextMeshProUGUI label = colorer.GetComponent<TextMeshProUGUI>();
                    if (label != null) { _labels.Add(label); }
                }
            }
            else
            {
                _labels.RemoveAll(label => label == null);
            }
            return _labels;
        }
    }


    [Header("Potential Setting")]
    [SerializeField] private string setting_name;
    [SerializeField] private Setting setting;




    // START
    private void Start()
    {
        // we check if we have a setting name
        if (string.IsNullOrEmpty(setting_name))
        {
            // we fire the last cycle event to initialize the button with the first cycle data
            cycle_datas.LastOrDefault()?.onClickedEvent.Invoke();
            SwitchToCycle(0);   // we initialize the button with the first cycle data
        }

        // we try to extract the setting from the setting name
        if (!string.IsNullOrEmpty(setting_name))
        {
            setting = SettingsManager.Instance?.GetSetting(setting_name);
        }
        if (setting != null) { setting.OnValueChanged += onSettingValueChanged; onSettingValueChanged(setting.Value); }
    }
    private void OnDestroy()
    {
        if (setting != null) { setting.OnValueChanged -= onSettingValueChanged; }
    }

    // SETTING VALUE CHANGED
    private void onSettingValueChanged(float new_value)
    {
        CycleButtonData cycle_data = null;
        int index = 0;

        // we check if it is a StringSetting
        if (setting is StringSetting string_setting)
        {
            // we try to find the cycle data with the value name corresponding to the new value
            cycle_data = cycle_datas.FirstOrDefault(c => c.name == string_setting.ToString());
            if (cycle_data == null) { return; }

            // we switch to the cycle data
            index = cycle_datas.IndexOf(cycle_data);
            SwitchToCycle(index);

            // and we fire the previous cycle event to update the button with the new cycle data
            int previous_cycle_index = index - 1;
            if (previous_cycle_index < 0) { previous_cycle_index = cycle_datas.Count - 1; }
            cycle_datas[previous_cycle_index].onClickedEvent.Invoke();
            return;
        }
    }

    // POINTER HANDLER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // we change the icon color
        btn_icon.color = current_cycle.iconHoverColor;
        image.color = current_cycle.baseColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(current_cycle.baseColor);
        }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // we change the icon color
        btn_icon.color = current_cycle.baseColor;
        image.color = current_cycle.hoverColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].RevertColor();
        }
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        // we invoke the event
        current_cycle.onClickedEvent.Invoke();

        // we switch to the next cycle
        SwitchToCycle(current_cycle_index + 1);
    }

    // CHANGE CYCLE
    public void SwitchToCycle(int index)
    {
        current_cycle_index = index % cycle_datas.Count;

        // we change the icon color
        btn_icon.color = current_cycle.iconHoverColor;
        image.color = Hovered ? current_cycle.baseColor : current_cycle.hoverColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            if (Hovered) { colorers[i].ApplyColor(current_cycle.baseColor); }
            else { colorers[i].RevertColor(); }
        }

        // we change the icon sprite
        btn_icon.sprite = current_cycle.iconSprite;

        // we change the labels
        for (int i = 0; i < Labels.Count; i++)
        {
            Labels[i].text = current_cycle.label;
        }

        // if we have a setting we change its value to the cycle name (only if it is a StringSetting)
        if (setting is not null && setting is StringSetting string_setting)
        {
            string_setting.SetValueWithoutNotify(current_cycle.name);
        }
    }

    // COLORANT
    public Color HoverColor => current_cycle.hoverColor;
    public void SetColors(Color base_color, Color clicked_color)
    {
        // special case for UI_EventButton the colors are inversed (the base color is the base color of the icon when not hovered -> means its the clicked one)
        current_cycle.baseColor = clicked_color;
        current_cycle.hoverColor = base_color;

        // we change the icon color
        btn_icon.color = current_cycle.iconHoverColor;
        image.color = Hovered ? current_cycle.baseColor : current_cycle.hoverColor; // and the main image color

        // we color all colorers
        if (!Hovered) { return; } // no need to update colorers if not hovered since colorers handle their own reset color
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(current_cycle.baseColor);
        }
    }
}

[Serializable] public class CycleButtonData
{
    public string name;
    public string label;
    public Color baseColor = Color.red;
    public Color hoverColor = Color.white;
    public Color iconHoverColor = Color.white;
    public Sprite iconSprite;
    public UnityEvent onClickedEvent;
}