using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ToggleSetting : UI_Toggle, UI_SettingSlot
{
    [Header("Setting")]
    [SerializeField] protected string setting_name = "undefined";
    public string SettingName => setting_name;

    [Header("On Off Sprites")]
    [SerializeField] private Image icon;
    [SerializeField] private Sprite on_sprite;
    [SerializeField] private Sprite on_hover_sprite;
    [SerializeField] private Sprite off_sprite;
    [SerializeField] private Sprite off_hover_sprite;

    [Header("Color Settings")]
    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;

    [Header("Colorers")]
    public List<UI_Colorer> colorers = new List<UI_Colorer>();


    // START
    protected virtual void Start()
    {
        // register on toggled event
        this.OnOn += on_toggled;
        this.OnOff += on_toggled;
        set_manager_setting();
    }

    // POINTER HANDLER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // we change the bar color
        image.color = hoverColor;
        icon.color = hoverColor; // and the main icon color
        icon.sprite = is_on ? on_hover_sprite : off_hover_sprite;

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(hoverColor);
        }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // we change the icon color
        image.color = baseColor;
        icon.color = baseColor; // and the main icon color
        icon.sprite = is_on ? on_sprite : off_sprite;

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].RevertColor();
        }
    }

    // ON TOGGLED
    private void on_toggled()
    {
        SettingsManager.Instance?.SetSetting(settingName: SettingName, value: is_on ? 1 : 0);

        // change icon based on is_on
        icon.sprite = is_on ? Hovered ? on_hover_sprite : on_sprite
                            : Hovered ? off_hover_sprite : off_sprite;
    }

    // LOW SETTER
    protected void set_manager_setting()
    {
        // we try to get the value from the settings manager
        Setting setting = SettingsManager.Instance?.GetSetting(settingName: SettingName);
        if (setting == null) { return; }

        // set current value
        is_on = setting.value > 0;
        if (log) { Debug.Log("(UI_ToggleSetting) New boolean from manager : " + is_on); }

        // change icon based on is_on
        icon.sprite = is_on ? Hovered ? on_hover_sprite : on_sprite
                            : Hovered ? off_hover_sprite : off_sprite;
    }
}