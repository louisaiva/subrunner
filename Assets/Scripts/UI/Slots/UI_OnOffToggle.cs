using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_OnOffToggle : UI_Toggle
{
    [Header("On/Off Toggle Settings")]
    [SerializeField] private Color onColor = Color.green;
    [SerializeField] private Color offColor = Color.red;
    [SerializeField] private Image power_icon;

    [Header("Power Animation Settings")]
    [SerializeField] private float transition_duration = 0.5f;
    [SerializeField] private float rotation_off = 180f;
    [SerializeField] private float rotation_on = 0f;
    private Coroutine transition = null;

    // AWAKE
    protected override void Awake()
    {
        base.Awake();

        // subscribe to our own events so we don't have to care about anything else
        this.OnOn += handle_turn_on;
        this.OnOff += handle_turn_off;

        // we check the state and directly apply rotation + color to the icon
        power_icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, is_on ? rotation_on : rotation_off);
        power_icon.color = is_on ? onColor : offColor;
    }

    // HANDLER FOR TURNING ON
    private void handle_turn_on()
    {
        power_icon.color = onColor;
        if (transition != null) { StopCoroutine(transition); }
        transition = StartCoroutine(power_on());
    }

    // HANDLER FOR TURNING OFF
    private void handle_turn_off()
    {
        power_icon.color = offColor;
        if (transition != null) { StopCoroutine(transition); }
        transition = StartCoroutine(power_off());
    }

    // POWER ON / POWER OFF
    private IEnumerator power_on()
    {
        // we check the current icon rotation to know from where we are starting
        float current_rotation = power_icon.rectTransform.eulerAngles.z;
        float angle_difference = Mathf.DeltaAngle(current_rotation, rotation_on);
        float duration = transition_duration * (Mathf.Abs(angle_difference) / Mathf.Abs(rotation_off - rotation_on));

        while (duration > 0f)
        {
            // animate rotation
            float step = (Time.deltaTime / transition_duration) * Mathf.Abs(rotation_off - rotation_on);
            current_rotation = Mathf.MoveTowardsAngle(current_rotation, rotation_on, step);
            power_icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, current_rotation);
            duration -= Time.deltaTime;

            yield return null;
        }

        // ensure final rotation
        power_icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, rotation_on);
        transition = null;
    }
    private IEnumerator power_off()
    {
        // we check the current icon rotation to know from where we are starting
        float current_rotation = power_icon.rectTransform.eulerAngles.z;
        float angle_difference = Mathf.DeltaAngle(current_rotation, rotation_off);
        float duration = transition_duration * (Mathf.Abs(angle_difference) / Mathf.Abs(rotation_off - rotation_on));

        while (duration > 0f)
        {
            // animate rotation
            float step = (Time.deltaTime / transition_duration) * Mathf.Abs(rotation_off - rotation_on);
            current_rotation = Mathf.MoveTowardsAngle(current_rotation, rotation_off, step);
            power_icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, current_rotation);
            duration -= Time.deltaTime;

            yield return null;
        }

        // ensure final rotation
        power_icon.rectTransform.rotation = Quaternion.Euler(0f, 0f, rotation_off);
        transition = null;
    }

}