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
    [SerializeField] private float clamp_speed = 10f;
    [SerializeField] private bool limit_to_bounds = false;
    // if true, the scroller will ensure :
    // -      the min_y < screen_y + min_offset
    // -      the max_y > screen_y + max_offset
    // (for vertical, if horizontal we check x instead of y)
    // this way we ensure the scroller always has content to show and doesn't scroll too much
    [SerializeField] private float min_offset = -100f;
    [SerializeField] private float max_offset = 100f;

    [Header("Logs")]
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

    public void Scroll(float movement)
    {
        if (invert_scroll) { movement = -movement; }

        // we calculate the new target position
        float new_position = RectTransform.anchoredPosition[scroll_axis == ScrollAxis.Vertical ? 1 : 0] + movement * scroll_speed;

        // we set the position (no bounds check for now)
        Vector2 pos = RectTransform.anchoredPosition;
        if (scroll_axis == ScrollAxis.Vertical)
            pos.y = new_position;
        else
            pos.x = new_position;
        RectTransform.anchoredPosition = pos;
    }
    public void Update()
    {
        if (!limit_to_bounds) { return; }

        int axis = scroll_axis == ScrollAxis.Vertical ? 1 : 0;
        float rect_min = RectTransform.rect.yMin;
        float rect_max = RectTransform.rect.yMax;
        float current_offset = RectTransform.anchoredPosition[axis];

        // Current edges in space
        float content_min = rect_min + current_offset;
        float content_max = rect_max + current_offset;


        if (log_clamp)
        {
            Debug.Log($"--- Scroll Frame ---");
            Debug.Log($"rect_min: {rect_min}, rect_max: {rect_max}");
            Debug.Log($"current_offset (localPos[{axis}]): {current_offset}");
            Debug.Log($"content_min: {content_min}, content_max: {content_max}");
            Debug.Log($"bounds: [{min_offset}, {max_offset}]");
        }

        // Clamp
        float clamped_offset = current_offset;
        if (content_min > min_offset)
        {
            float adjustment = content_min - min_offset;
            if (log_clamp) Debug.Log($"content_min {content_min} > min_offset {min_offset}, adjusting by {-adjustment}");
            clamped_offset -= adjustment;
        }
        if (content_max < max_offset)
        {
            float adjustment = max_offset - content_max;
            if (log_clamp) Debug.Log($"content_max {content_max} < max_offset {max_offset}, adjusting by {adjustment}");
            clamped_offset += adjustment;
        }

        if (log_clamp) Debug.Log($"target clamped_offset: {clamped_offset}");

        // Lerp towards clamped position
        Vector2 pos = RectTransform.anchoredPosition;
        pos[axis] = Mathf.Lerp(pos[axis], clamped_offset, Time.unscaledDeltaTime * clamp_speed);
        RectTransform.anchoredPosition = pos;
    }
    public void ResetPosition()
    {
        Vector2 pos = RectTransform.anchoredPosition;
        pos[scroll_axis == ScrollAxis.Vertical ? 1 : 0] = 0f;
        RectTransform.anchoredPosition = pos;
    }
}

public enum ScrollAxis
{
    Horizontal,
    Vertical
}