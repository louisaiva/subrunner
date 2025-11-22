using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this auto layout helper class determines which size we must put on this recttransform
/// based on MULTIPLE contents' sizes + relative positions + margins
/// </summary>
public class UI_AutoLayoutMerger : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private List<RectTransform> contents;
    private RectTransform rect_transform;

    [Header("Margins")]
    [SerializeField] private float margin_top;
    [SerializeField] private float margin_bottom;
    [SerializeField] private float margin_left;
    [SerializeField] private float margin_right;

    [Header("Logs")]
    [SerializeField] private bool log = false;

    // START
    private void Start()
    {
        // get the components
        rect_transform = GetComponent<RectTransform>();
        Update();
    }

    // UPDATE
    public void Update()
    {
        if (contents.Count == 0) { if (log) { Debug.LogError($"(UI_AutoLayoutResizer) {name} has no content set!"); } return; }
        
        // on calcule le min x/y et le max x/y des contents
        float min_x = float.MaxValue;
        float max_x = float.MinValue;
        float min_y = float.MaxValue;
        float max_y = float.MinValue;
        for (int i = 0; i < contents.Count; i++)
        {
            // checks if content gameobject is enabled
            RectTransform content = contents[i];
            if (!content.gameObject.activeSelf) { continue; }

            // gets corners and check if it is above limits
            Vector3[] corners = new Vector3[4];
            content.GetWorldCorners(corners);
            for (int j = 0; j < corners.Length; j++)
            {
                Vector3 local_pos = rect_transform.InverseTransformPoint(corners[j]);
                if (local_pos.x < min_x) { min_x = local_pos.x; }
                if (local_pos.x > max_x) { max_x = local_pos.x; }
                if (local_pos.y < min_y) { min_y = local_pos.y; }
                if (local_pos.y > max_y) { max_y = local_pos.y; }
            }
        }

        // apply margins
        float width = max_x - min_x + margin_left + margin_right;
        float height = max_y - min_y + margin_top + margin_bottom;
        rect_transform.sizeDelta = new Vector2(width, height);

        if (log) { Debug.Log($"(UI_AutoLayoutMerger) {name} resized to {rect_transform.sizeDelta}"); }
    }
}