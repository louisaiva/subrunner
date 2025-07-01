using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
/// <summary>
/// this class manages all the debug elements by counting stuff
/// on awake, and nothing else for now !
/// </summary>
public class DebugManager : MonoBehaviour
{
    [Header("Debugs")]
    [SerializeField] private Transform debugs;
    bool debug_in_hud = false;

    [Header("Debugs - FPS")]
    [SerializeField] private TextMeshProUGUI fps;
    [SerializeField] private float smoothed_fps = 0f; // smoothed fps for the debug text
    private List<float> fps_samples = new List<float>(); // list of fps samples for smoothing
    [SerializeField] private int fps_sample_size = 60;
    [SerializeField] private string precision = "F0"; // precision of the fps text
    [SerializeField] private bool show_unscaled_fps = false; // if we want to show the unscaled fps or not (djizzi)


    // UPDATE
    private void Update()
    {
        if (!fps) { return; }

        // showing unscaled_fps if activated
        if (show_unscaled_fps)
        {
            fps.text = (1f / Time.unscaledDeltaTime).ToString(precision) + " FPS";
            return;
        }

        // else we smooth the fps
        if (fps_samples.Count >= fps_sample_size)
        {
            fps_samples.RemoveAt(0); // remove the oldest sample
        }
        fps_samples.Add(1f / Time.unscaledDeltaTime);

        // calculate the smoothed fps
        smoothed_fps = fps_samples.Average();

        // update the text
        fps.text = smoothed_fps.ToString(precision) + " FPS";
    }

    // ADD DEBUG TO HUD POOL
    public void ToggleDebug()
    {
        if (debug_in_hud) { HideDebug(); }
        else { ShowDebug(); }
    }
    private void ShowDebug()
    {
        UI_HUD hud = GameObject.Find("/ui/hud").GetComponent<UI_HUD>();
        hud.RegisterToPool(debugs.gameObject);
        debug_in_hud = true;
    }
    private void HideDebug()
    {
        UI_HUD hud = GameObject.Find("/ui/hud").GetComponent<UI_HUD>();
        hud.QuitPool(debugs.gameObject);
        debug_in_hud = false;
    }

}