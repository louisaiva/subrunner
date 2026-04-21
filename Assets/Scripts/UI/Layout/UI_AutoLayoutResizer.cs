using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this auto layout helper class determines which size we must put on this recttransform
/// based on content's size + margins
/// </summary>
public class UI_AutoLayoutResizer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RectTransform content;
    private RectTransform rect_transform;

    [Header("Margins")]
    [SerializeField] private float margin_top;
    [SerializeField] private float margin_bottom;
    [SerializeField] private float margin_left;
    [SerializeField] private float margin_right;

    [Header("Size Factor")]
    [SerializeField] private float size_factor = 1f;

    // START
    private void Start()
    {
        // get the components
        rect_transform = GetComponent<RectTransform>();
        if (content == null) { Debug.LogError($"(UI_AutoLayoutResizer) {name} has no content set!"); return; }
        Update();
    }

    // UPDATE
    public void Update()
    {
        // update our size based on content's size & margins
        float width = content.rect.width + margin_left + margin_right;
        float height = content.rect.height + margin_top + margin_bottom;
        rect_transform.sizeDelta = new Vector2(width*size_factor, height*size_factor);
    }
}