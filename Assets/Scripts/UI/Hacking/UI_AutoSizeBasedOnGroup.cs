using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        // auto resize to match content size
        rect_transform = GetComponent<RectTransform>();
        if (content == null) { Debug.LogError($"(UI_AutoSizeBasedOnGroup) {name} has no content set!"); return; }
        UpdateSize();

        // we subscribe to content size changes
    }

    public void Update()
    {
        UpdateSize();
    }

    public void UpdateSize()
    {
        // update our size based on content's size & margins
        float width = content.rect.width + margin_left + margin_right;
        float height = content.rect.height + margin_top + margin_bottom;
        rect_transform.sizeDelta = new Vector2(width, height);
    }
}