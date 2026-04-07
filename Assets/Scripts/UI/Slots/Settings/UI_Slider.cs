using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Slider : UI_ImageSlot, UI_SettingSlot
{
    [Header("Setting")]
    [SerializeField] protected string setting_name = "undefined";
    public string SettingName { get { return setting_name; } set 
        {
            setting_name = value;
            set_manager_setting();
        }
    }

    [Header("Slider settings")]
    [SerializeField] protected float minValue = 0f;
    [SerializeField] protected float maxValue = 1f;
    [SerializeField] protected float speed = 4f; // speed at which the slider moves when changing value (in percentage/click)
    public float CurrentValue = 0f;
    public float CurrentPercentage => calculate_percentage();

    [Header("Bar Color Settings")]
    [SerializeField] private Color baseBarColor = Color.white;
    [SerializeField] private Color hoverBarColor = Color.yellow;
    [SerializeField] private float bar_alpha = 1f; // alpha of the bar image
    [SerializeField] private Image bar_image;
    [SerializeField] private RectTransform input_rect;
    public float BarSize { get { return input_rect.rect.width; } }

    [Header("Colorers")]
    public List<UI_Colorer> colorers = new List<UI_Colorer>();
    public List<UI_Colorer> Colorers => colorers;

    // START
    protected virtual void Start()
    {
        set_manager_setting();
    }

    // POINTER HANDLER
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // we change the bar color
        bar_image.color = new Color(hoverBarColor.r, hoverBarColor.g, hoverBarColor.b, bar_alpha);
        image.color = hoverBarColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(hoverBarColor);
        }
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // we change the icon color
        bar_image.color = new Color(baseBarColor.r, baseBarColor.g, baseBarColor.b, bar_alpha);
        image.color = baseBarColor; // and the main image color

        // we color all colorers
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].RevertColor();
        }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (InputManager.Instance.UsingGamepad) { return; }
        OnClickWithMouse();
    }

    // VALUE CHANGED
    public virtual void OnSlide(float amount)
    {
        if (log) { Debug.Log("(UI_Slider) OnSlide on " + gameObject.name + " with amount " + amount); }

        // we convert in percentage
        float dp = amount * speed;

        // we convert dp (which is in percentage) to value-relative value
        float dv = dp * (maxValue - minValue) / 100f;
        set_value(CurrentValue + dv);
    }
    public void OnClickWithMouse()
    {
        if (log) { Debug.Log("(UI_Slider) OnClickWithMouse on " + gameObject.name); }

        // we get the mouse position relative to the bar
        Vector2 local_mouse_position;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            input_rect,
            Input.mousePosition,
            null,
            out local_mouse_position
        )) { return; } // we couldn't get the local position

        // we calculate the clicked percentage
        float clicked_percentage = (local_mouse_position.x + (BarSize / 2f)) / BarSize;
        clicked_percentage = Mathf.Clamp01(clicked_percentage);

        set_value(minValue + clicked_percentage * (maxValue - minValue));
    }
    private void Update()
    {
        // update the pointer position bcz when we scroll through the settings
        // sometimes BarSize changes which makes it fked up

        image.rectTransform.localPosition = new Vector3(
            calculate_pointer_position() - (BarSize / 2f),
            image.rectTransform.localPosition.y,
            image.rectTransform.localPosition.z
        );
    }

    // LOW SETTER
    protected virtual void set_value(float value)
    {
        CurrentValue = Mathf.Clamp(value, minValue, maxValue);

        if (log) { Debug.Log("(UI_Slider) New CurrentValue is " + CurrentValue + $" (not-clamped value is {value} , value per pixel is {ValuePerPixel} , percentage is {calculate_percentage()} , calculated position is then {calculate_pointer_position()} , bcz bar size is {BarSize})"); }

        // we update the image localposition relative to the current value
        image.rectTransform.localPosition = new Vector3(
            calculate_pointer_position() - (BarSize / 2f),
            image.rectTransform.localPosition.y,
            image.rectTransform.localPosition.z
        );

        // we change settings manager 's setting value
        SettingsManager.Instance?.SetSetting(settingName: SettingName, value:CurrentValue);
    }
    protected void set_manager_setting()
    {
        // we try to get the value from the settings manager
        Setting setting = SettingsManager.Instance?.GetSetting(settingName: SettingName);
        if (setting == null) { return; }

        CurrentValue = setting.Value;
        if (log) { Debug.Log("(UI_Slider) New CurrentValue from manager : " + CurrentValue); }

        // get maxValue minValue
        minValue = setting.min_value;
        maxValue = setting.max_value;

        // reset values that depends on max/min
        _value_per_px = 0f;

        // we update the image localposition relative to the current value
        image.rectTransform.localPosition = new Vector3(
            calculate_pointer_position() - (BarSize / 2f),
            image.rectTransform.localPosition.y,
            image.rectTransform.localPosition.z
        );
    }

    // VALUE PER PIXEL
    private float _value_per_px = 0f;
    public float ValuePerPixel
    {
        get
        {
            if (_value_per_px == 0f)
            {
                _value_per_px = (maxValue - minValue) / BarSize;
            }
            return _value_per_px;
        }
    }
    private float calculate_percentage()
    {
        return (CurrentValue - minValue) / (maxValue - minValue);
    }
    private float calculate_pointer_position()
    {
        return calculate_percentage() * BarSize;
    }

    // COLORANT
    public Color HoverColor => hoverBarColor;
    public void SetColors(Color baseColor, Color hoverColor)
    {
        this.baseBarColor = baseColor;
        this.hoverBarColor = hoverColor;

        // we apply the base color
        bar_image.color = Hovered ? new Color(hoverColor.r, hoverColor.g, hoverColor.b, bar_alpha) : new Color(baseColor.r, baseColor.g, baseColor.b, bar_alpha);
        image.color = Hovered ? hoverColor : baseColor;

        // we color all colorers
        if (!Hovered) { return; }
        for (int i = 0; i < colorers.Count; i++)
        {
            colorers[i].ApplyColor(hoverColor);
        }
    }
}

public interface UI_SettingSlot : Colorant
{
    public string SettingName { get; set;}
    public GameObject gameObject { get; }
}