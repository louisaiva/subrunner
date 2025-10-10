using UnityEngine;

/// <summary>
/// this UI_Colorer is a simple script that takes a graphic
/// and a color and can toggle its coloration (reverting back to previous color)
/// </summary>
public class UI_Colorer : MonoBehaviour
{

    [Header("Coloration")]
    [SerializeField] private Color color;
    private Color previous_color;
    private bool is_colored = false;
    private UnityEngine.UI.Graphic graphic;

    // START
    private void Start()
    {
        graphic = GetComponent<UnityEngine.UI.Graphic>();
        if (graphic == null)
        {
            Debug.LogError("UI_Colorer: no graphic found on " + gameObject.name);
        }
        previous_color = graphic.color;
    }

    // COLORER
    public void ApplyColor()
    {
        if (graphic == null) { return; }
        if (is_colored) { return; }
        previous_color = graphic.color;
        graphic.color = color;
        is_colored = true;
    }
    public void RevertColor()
    {
        if (graphic == null) { return; }
        if (!is_colored) { return; }
        graphic.color = previous_color;
        is_colored = false;
    }
}