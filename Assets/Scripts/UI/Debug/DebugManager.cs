using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// this class manages all the debug elements by counting stuff
/// on awake, and nothing else for now !
/// </summary>
public class DebugManager : Singleton<DebugManager>
{
    [Header("Debug")]
    [SerializeField] private Transform debug;
    private List<SingleDebugger> debugs = new List<SingleDebugger>();

    [Header("FPS Debug")]
    [SerializeField] private TextMeshProUGUI fps;
    [SerializeField] private float smoothed_fps = 0f; // smoothed fps for the debug text
    private List<float> fps_samples = new List<float>(); // list of fps samples for smoothing
    [SerializeField] private int fps_sample_size = 60;
    [SerializeField] private string precision = "F0"; // precision of the fps text
    [SerializeField] private bool show_unscaled_fps = false; // if we want to show the unscaled fps or not (djizzi)

    [Header("Settings")]
    private Setting debug_setting;

    [Header("Logs")]
    [SerializeField] private bool log = false; // if we want to log warnings when

    // START
    protected override void Awake()
    {
        base.Awake();

        // find all debugs in children
        debugs = debug.GetComponentsInChildren<SingleDebugger>(includeInactive: true).ToList();
        debugs = debugs.Where(d => d.gameObject.activeSelf).ToList(); // we get only active debugs
        foreach (SingleDebugger d in debugs)
        {
            d.gameObject.SetActive(false); // disable all debugs at start
        }
    }
    protected void Start()
    {
        // on met le skin en fonction du settings skin
        debug_setting = SettingsManager.Instance.GetSetting("debug");
        if (debug_setting == null) { return; }
        toggle_debug(debug_setting.value);
        debug_setting.OnValueChanged += toggle_debug;
    }
    void OnDestroy()
    {
        if (debug_setting != null) { debug_setting.OnValueChanged -= toggle_debug; }
    }

    // UPDATE + FPS
    private void Update() { update_fps(); }
    private void update_fps()
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

    // TOGGLE DEBUG
    private void toggle_debug(float value)
    {
        bool enable = (value > 0.5f) ? true : false;
        debug.gameObject.SetActive(enable);
    }

    // ADD DEBUGGABLE TO DEBUGGER
    public void AddDebuggable(Debuggable debuggable,string name)
    {
        // find the debug with the right name
        SingleDebugger debug = debugs.Find(d => d.name == name);
        if (debug == null)
        {
            if (log) { Debug.LogWarning("(DebugManager) No debug found with name " + name); }
            return;
        }

        // add the debuggable to the debug
        debug.SetDebuggable(debuggable);
        debug.gameObject.SetActive(true); // enable the debug
    }
}