using UnityEngine;

/// <summary>
/// auto adjust its height & width to always fit perfectly the text
/// </summary>
public class UI_Message : MonoBehaviour
{

    [Header("Components")]
    public RectTransform rect_transform;
    public TMPro.TMP_Text text_mesh;

    [Header("Parameters")]
    public bool lock_width = false;
    public bool lock_height = false;

    [Header("Logs")]
    public bool debug = false;

    void Start()
    {
        if (rect_transform == null) { rect_transform = GetComponent<RectTransform>(); }
        if (text_mesh == null) { text_mesh = GetComponent<TMPro.TMP_Text>(); }

        if (rect_transform == null || text_mesh == null)
        {
            Debug.LogError("UI_Message: missing components");
            return;
        }

        adjust_tmptext_size();
    }
    void Update()
    {
        adjust_tmptext_size();
    }

    private void adjust_tmptext_size()
    {
        if (text_mesh == null || rect_transform == null) { return; }

        Vector2 size = rect_transform.sizeDelta;
        Vector2 text_size = new Vector2(text_mesh.preferredWidth * rect_transform.localScale.x, text_mesh.preferredHeight * rect_transform.localScale.y);

        if (!lock_width && size.x != text_size.x)
        {
            size.x = text_size.x;
            rect_transform.sizeDelta = size;
            if (debug) { Debug.Log("UI_Message: adjusted width to " + size.x); }
        }

        if (!lock_height && size.y != text_size.y)
        {
            size.y = text_size.y;
            rect_transform.sizeDelta = size;
            if (debug) { Debug.Log("UI_Message: adjusted height to " + size.y); }
        }
    }

}