using UnityEngine;

/// <summary>
/// this UI_Colorer is a simple script that takes a graphic
/// and a color and can toggle its coloration (reverting back to previous color)
/// </summary>
public class UI_Colorer : MonoBehaviour
{

    [Header("Coloration")]
    [SerializeField] private Color color;
    private Color base_color;
    private bool is_colored = false;
    private UnityEngine.UI.Graphic graphic;

    [Header("Parameters")]
    public bool force_our_color = false;
    public bool disable_on_revert = false;
    public bool disable_on_color = false;

    [Header("Logs")]
    public bool log = false;

    // START
    private void Start()
    {
        if (graphic != null) { return; }
        graphic = GetComponent<UnityEngine.UI.Graphic>();
        if (graphic == null) { Debug.LogError("UI_Colorer: no graphic found on " + gameObject.name); }
        base_color = graphic.color;
    }

    // COLORER
    public void ApplyColor(Color? color = null)
    {
        // ensure we have everything + save the base color
        if (graphic == null) { Start(); }
        if (!is_colored) { base_color = graphic.color; }

        // find the color to apply
        if (force_our_color) { color = this.color; }
        else if (color == null) { color = this.color; }

        // apply the color
        graphic.color = color.Value;
        is_colored = true;
        if (log) Debug.Log("(UI_Colorer) Applied color " + graphic.color + " to " + gameObject.name);

        // we disable or enable the gameObject if needed
        if (disable_on_revert) { gameObject.SetActive(true); }
        if (disable_on_color) { gameObject.SetActive(false); }
    }
    public void RevertColor()
    {
        if (graphic == null) { return; }
        if (!is_colored) { return; }
        graphic.color = base_color;
        is_colored = false;
        if (log) Debug.Log("(UI_Colorer) Reverted color to " + graphic.color + " on " + gameObject.name);

        if (disable_on_revert) { gameObject.SetActive(false); }
        if (disable_on_color) { gameObject.SetActive(true); }
    }
}