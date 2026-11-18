using System;
using UnityEngine;

public class SpeedrunTimer : MonoBehaviour
{
    public bool is_running = false;

    private void Update()
    {
        if (!is_running) { return; }
        if (World.Instance == null) { return; }
        if (World.Instance.data == null) { return; }
        World.Instance.data.speedrun_clock += UnityEngine.Time.deltaTime;
    }
    public void StartTimer() { is_running = true; }
    public void StopTimer() { is_running = false; }
    public float Time => World.Instance?.data?.speedrun_clock ?? 0f;
    public string HMS_Time => convert_sec_to_hms(Time);
    public string GetColoredHMS_Time(Color highlight, Color base_color) => color_hms(HMS_Time, highlight, base_color);

    private string convert_sec_to_hms(float sec)
    {
        int min = (int)sec / 60;
        if (min <= 0) { return sec.ToString("F2"); } // only seconds

        // we have some minutes !
        sec -= min * 60;
        int hour = min / 60;
        if (hour <= 0) { return $"{min}min {sec:F2}s"; }

        // we have some hours !
        min -= hour * 60;
        return $"{hour}h {min}min {sec:F2}s";
    }
    private string color_hms(string hms, Color highlight, Color base_color)
    {
        // ex : 17h 36min 10,21s
        // we highlight 17 ; 36 & 10,21
        // we set base_color to the rest (h, min & s)
        string colored_hms = "";
        foreach (char c in hms)
        {
            if (c == ' ')
            {
                colored_hms += ' ';
                continue;
            }

            if (Char.IsNumber(c) || c == ',')
            {
                colored_hms += c.ToString().AddColor(highlight);
                continue;
            }

            // else we only have the rest
            colored_hms += c.ToString().AddColor(base_color);
        }
        return colored_hms;
    }
}