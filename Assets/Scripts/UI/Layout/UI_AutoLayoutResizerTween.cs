using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this auto layout helper class determines which size we must put on this recttransform
/// based on content's size + margins
/// </summary>
[ExecuteAlways]
public class UI_AutoLayoutResizerTween : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RectTransform content;
    private RectTransform _rect_transform;
    private RectTransform rect_transform
    {
        get
        {
            if (_rect_transform != null) { return _rect_transform; }
            _rect_transform = GetComponent<RectTransform>();
            if (_rect_transform == null) { Debug.LogError($"(UI_AutoLayoutResizerTween) {name} has no rect transform component!"); }
            return _rect_transform;            
        }
    }

    [Header("Settings")]
    [SerializeField] private ResizeAxis resize_axis = ResizeAxis.Both;
    [SerializeField] private bool execute_in_editor = false;
    [SerializeField, Range(0.1f, 10f)] private float tween_speed = 1f;

    [Header("Margins")]
    [SerializeField] private float margin_top;
    [SerializeField] private float margin_bottom;
    [SerializeField] private float margin_left;
    [SerializeField] private float margin_right;

    [Header("Size Factor")]
    [SerializeField] private float size_factor = 1f;

    // UPDATE
    public void Update()
    {
        if (Application.isPlaying) { RefreshSize(); }
        else if (execute_in_editor) { RefreshSize(); }
    }


    // SIZE CALCULATOR
    private bool isHorizontal => resize_axis == ResizeAxis.Horizontal;
    private bool isVertical => resize_axis == ResizeAxis.Vertical;
    private bool handleHorizontal => isHorizontal || resize_axis == ResizeAxis.Both;
    private bool handleVertical => isVertical || resize_axis == ResizeAxis.Both;
    public void RefreshSize()
    {
        if (content == null || rect_transform == null) { return; }

        // update our size based on content's size & margins
        float width = 1;
        float height = 1;

        // calculate target size
        if (handleHorizontal)
        {
            width = content.rect.width + margin_left + margin_right;
        }
        if (handleVertical)
        {
            height = content.rect.height + margin_top + margin_bottom;
        }

        Vector2 target_size = new Vector2
        (
            isHorizontal ? width * size_factor : rect_transform.sizeDelta.x,
            isVertical ? height * size_factor : rect_transform.sizeDelta.y
        );

        // check size distance to avoid tweening if the size is already correct
        if (Vector2.Distance(rect_transform.sizeDelta, target_size) < 0.1f)
        {
            rect_transform.sizeDelta = target_size;
            return;
        }

        // lerp to the target size
        rect_transform.sizeDelta = Vector2.Lerp(rect_transform.sizeDelta, target_size, Time.deltaTime * tween_speed);
    }
}