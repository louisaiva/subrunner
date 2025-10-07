using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this auto layout helper class is kind of the same as UI_AutoLayoutResizer but
/// it works with groups. It determines which size we must put on this recttransform
/// based on group's preferred size + margins (it's basically a content size filter + margins)
/// </summary>
public class UI_AutoLayoutResizerOnGroup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private LayoutGroup group;
    private RectTransform rect_transform;

    [Header("Margins")]
    [SerializeField] private float margin_top;
    [SerializeField] private float margin_bottom;
    [SerializeField] private float margin_left;
    [SerializeField] private float margin_right;

    // START
    private void Start()
    {
        // get the components
        rect_transform = GetComponent<RectTransform>();
        if (group == null) { Debug.LogError($"(UI_AutoLayoutResizerOnGroup) {name} has no group set!"); return; }
        Update();
    }

    // UPDATE
    public void Update()
    {
        // get the group's preferred size
        float width = group.preferredWidth;
        float height = group.preferredHeight;

        // apply margins
        width += margin_left + margin_right;
        height += margin_top + margin_bottom;
        rect_transform.sizeDelta = new Vector2(width, height);
    }
}