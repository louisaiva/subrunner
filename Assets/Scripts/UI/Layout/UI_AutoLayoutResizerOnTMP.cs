using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// this auto layout helper class is kind of the same as UI_AutoLayoutResizer but
/// it works with textmeshprougui. It determines which size we must put on this recttransform
/// based on text's preferred size + margins (it's basically a content size filter + margins+ max size + min size param)
/// </summary>
public class UI_AutoLayoutResizerOnTMP : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI text;
    private RectTransform rect_transform;

    [Header("Margins")]
    [SerializeField] private float margin_top;
    [SerializeField] private float margin_bottom;
    [SerializeField] private float margin_left;
    [SerializeField] private float margin_right;

    [Header("Min Size")]
    [SerializeField] private Vector2 min_size = new Vector2(0, 0);

    [Header("Max Size")]
    [SerializeField] private Vector2 max_size = new Vector2(-10, -10); // -10 means no max size

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // START
    private void Start()
    {
        // get the components
        rect_transform = GetComponent<RectTransform>();
        if (text == null) { Debug.LogError($"(UI_AutoLayoutResizerOnTMP) {name} has no text set!"); return; }
        Update();
    }

    // UPDATE
    public void Update()
    {
        // get the text's preferred size
        float width = text.preferredWidth;
        float height = text.preferredHeight;

        // apply margins
        width += margin_left + margin_right;
        height += margin_top + margin_bottom;

        // apply min size
        if (width < min_size.x) { width = min_size.x; }
        if (height < min_size.y) { height = min_size.y; }

        // apply max size
        if (max_size.x >= 0 && width > max_size.x) { width = max_size.x; }
        if (max_size.y >= 0 && height > max_size.y) { height = max_size.y; }

        // apply the size
        rect_transform.sizeDelta = new Vector2(width, height);

        if (log)
        {
            Debug.Log($"(UI_AutoLayoutResizerOnTMP) {name} resized to {rect_transform.sizeDelta}");
        }
    }
}