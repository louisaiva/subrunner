using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_Colorant : MonoBehaviour
{

    [Header("Coloration")]
    [SerializeField] private Color base_color;
    [SerializeField] private Color current_color;
    [SerializeField] private bool is_colored = false;

    [Header("Targets")]
    [SerializeField] private List<UnityEngine.UI.Graphic> graphics;

    [Header("Logs")]
    public bool log = false;

    // COLORER
    public void ApplyColor(Color? color = null)
    {
        // find the color to apply
        if (color == null) { color = this.base_color; }

        // apply the color
        foreach (Graphic graphic in graphics)
        {
            graphic.color = color.Value;
            if (log) Debug.Log("(UI_Colorant) Applied color " + graphic.color + " to " + graphic.gameObject.name);
        }
        is_colored = true;
        current_color = color.Value;
    }
    public void RevertColor()
    {
        if (!is_colored) { return; }
        ApplyColor();
        is_colored = false;
        if (log) Debug.Log("(UI_Colorant) Succesfully reverted color to " + base_color + " for " + graphics.Count + " colored graphics.\n\n" + string.Join("\n  -", graphics.Where(g => g != null).Select(g => g.name)));
    }
}