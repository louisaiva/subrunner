using System;
using System.Linq;
using SingularityGroup.HotReload;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI_Scroller : MonoBehaviour
{
    [Header("Scroll parameters")]
    [SerializeField] private ScrollAxis scroll_axis = ScrollAxis.Vertical;
    [SerializeField] private bool invert_scroll = false;
    [SerializeField] private float scroll_speed = 1f;
    // [SerializeField] private float scroll_sensitivity = 1f;
    

    [Header("Scroll Bounds")]
    [SerializeField] private bool limit_to_bounds = false;
    // if true, the scroller will ensure :
    // -      the min_y < screen_y + min_offset
    // -      the max_y > screen_y + max_offset
    // (for vertical, if horizontal we check x instead of y)
    // this way we ensure the scroller always has content to show and doesn't scroll too much
    [SerializeField] private float min_offset = -100f;
    [SerializeField] private float max_offset = 100f;

    [Header("Fine Tuning Scroll Feeling")]
    [SerializeField] private float lerp_time_offset = 10f;
    [SerializeField] private float lerp_velocity;
    [SerializeField] private float clamp_speed = 10f;
    private Vector2? target_position = null;

    [Header("Logs")]
    [SerializeField] private bool log_scroll = false;
    [SerializeField] private bool log_lerp = false;
    [SerializeField] private bool log_clamp = false;

    // COMPONENTS
    private RectTransform _rect_transform;
    private RectTransform RectTransform
    {
        get
        {
            if (_rect_transform == null) { _rect_transform = GetComponent<RectTransform>(); }
            if (_rect_transform == null) { Debug.LogError($"(Scroller) No RectTransform found on the {name} game object. Please add one to the scene."); }
            return _rect_transform;
        }
    }

    private void Start()
    {
        UI_Navigator.Instance.OnSlotOutOfScreen += OnSlotOutOfScreen;
    }
    private void OnDestroy()
    {
        if (UI_Navigator.Instance == null) { return; }
        UI_Navigator.Instance.OnSlotOutOfScreen -= OnSlotOutOfScreen;
    }

    // MAIN METHODS
    public void Scroll(float movement)
    {
        if (invert_scroll) { movement = -movement; }
        int axis = scroll_axis == ScrollAxis.Vertical ? 1 : 0;

        // we calculate the new target position
        float new_position = RectTransform.anchoredPosition[axis] + (movement * scroll_speed);

        // we set the position (no bounds check for now)
        Vector2 pos = RectTransform.anchoredPosition;
        pos[axis] = new_position;
        
        // RectTransform.anchoredPosition = pos;
        target_position = pos;
        if (log_scroll) { Debug.Log($"(UI_Scroller) {name} is scrolling to {pos}"); }
    }
    public void ResetPosition()
    {
        Vector2 pos = RectTransform.anchoredPosition;
        pos[scroll_axis == ScrollAxis.Vertical ? 1 : 0] = 0f;
        target_position = pos;
        if (log_scroll) { Debug.Log($"(UI_Scroller) {name} resetted scroll to {pos}"); }
    }

    // UPDATE
    public void Update()
    {
        if (target_position != null)
        {
            lerp_to_target();
            return;
        }

        if (!limit_to_bounds) { return; }
        lerp_to_clamp();
    }

    private void lerp_to_clamp()
    {
        Vector2 pos = RectTransform.anchoredPosition;
        int axis = scroll_axis == ScrollAxis.Vertical ? 1 : 0;

        // getting
        float rect_min = RectTransform.rect.yMin;
        float rect_max = RectTransform.rect.yMax;
        float current_offset = pos[axis];

        // Current edges in space
        float content_min = rect_min + current_offset;
        float content_max = rect_max + current_offset;


        string debug_clamp = "";
        if (log_clamp)
        {
            debug_clamp += $"--- Scroll Frame ---";
            debug_clamp += $"\nrect_min: {rect_min}, rect_max: {rect_max}";
            debug_clamp += $"\ncurrent_offset (localPos[{axis}]): {current_offset}";
            debug_clamp += $"\ncurrent_target_position (localPos[{axis}]): {target_position}";
            debug_clamp += $"\ncurrent_next_position (localPos[{axis}]): {pos}";
            debug_clamp += $"\ncontent_min: {content_min}, content_max: {content_max}";
            debug_clamp += $"\nbounds: [{min_offset}, {max_offset}]";
        }

        // Clamp
        float clamped_offset = current_offset;
        if (content_min > min_offset)
        {
            float adjustment = content_min - min_offset;
            if (log_clamp) { debug_clamp += $"\ncontent_min {content_min} > min_offset {min_offset}, adjusting by {-adjustment}"; }
            clamped_offset -= adjustment;
        }
        if (content_max < max_offset)
        {
            float adjustment = max_offset - content_max;
            if (log_clamp) { debug_clamp += $"\ncontent_max {content_max} < max_offset {max_offset}, adjusting by {adjustment}"; }
            clamped_offset += adjustment;
        }

        if (log_clamp)
        {
            debug_clamp += $"\ntarget clamped_offset: {clamped_offset}";
            Debug.Log(debug_clamp);
        }

        // Lerp towards clamped position
        pos[axis] = Mathf.Lerp(pos[axis], clamped_offset, Time.unscaledDeltaTime * clamp_speed);
        RectTransform.anchoredPosition = pos;
    }
    private void lerp_to_target()
    {
        int axis = scroll_axis == ScrollAxis.Vertical ? 1 : 0;

        // updating current position & clear last target_position if needed
        Vector2 pos = target_position.Value;
        pos[axis] = Mathf.SmoothDamp(RectTransform.anchoredPosition[axis], pos[axis], ref lerp_velocity, lerp_time_offset, maxSpeed:10000, deltaTime:Time.unscaledDeltaTime);
        if (log_lerp) { Debug.Log($"(UI_Scroller) lerping from {RectTransform.anchoredPosition} to {pos}"); }
        RectTransform.anchoredPosition = pos;

        if (Mathf.Abs(target_position.Value[axis] - RectTransform.anchoredPosition[axis]) < 1)
        {
            target_position = null;
        }
    }

    // ON SLOT OUT OF SCREEN
    private void OnSlotOutOfScreen(UI_Slot slot)
    {
        // we only handle if the slot is a child of us ?
        if (!IsSlotInScroller(slot)) { return; }

        // we get the diff position between the slot and the center of screen
        // but ONLY the float diff along the scroll axis
        float diff = 0f;
        if (scroll_axis == ScrollAxis.Vertical) { diff = Screen.height / 2 - slot.GetPosition().y; }
        else if (scroll_axis == ScrollAxis.Horizontal) { diff = Screen.width / 2 - slot.GetPosition().x; }

        if (log_scroll) { Debug.Log($"(UI_Scroller) {name} : {slot} was out of screen. scrolling {diff}"); }
        // Scroll(-diff/scroll_speed);

        Vector2 pos = RectTransform.anchoredPosition;
        pos[scroll_axis == ScrollAxis.Vertical ? 1 : 0] += diff;
        target_position = pos;
    }

    /// <summary>
    /// holds the truth verity about if the slot
    /// is in the scroller. Override to apply custom
    /// logic. For now it only returns true if the slot is
    /// in their children and enabled
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected virtual bool IsSlotInScroller(UI_Slot slot)
    {
        return GetComponentsInChildren<UI_Slot>(includeInactive:false).Contains(slot);
    }
}

public enum ScrollAxis
{
    Horizontal,
    Vertical
}