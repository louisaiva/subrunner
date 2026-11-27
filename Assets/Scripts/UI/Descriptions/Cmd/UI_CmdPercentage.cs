using System;
using UnityEngine;

public class UI_CmdPercentage : UI_CmdLine
{
    public override bool IsWorking => percentage < 1f;
    public float percentage = 0f;

    [Header("Visuals")]
    public string left_border = "]";
    public string right_border = "[";
    public string no_charac = ".";
    public string yes_charac = "|";
    public int bar_size = 10; // character count that forms the inside of the bar
    public string perc_suffix = " %";

    [Header("Simulate progress")]
    public bool simulate = false; // if false, the progress will never change, and so the cmd will always be working ???
    public float simulation_time = 2f; // duration in seconds of the simulation animation

    protected override void Update()
    {
        base.Update();
        if (IsWriting) { return; }


        if (simulate) { update_simulation(); }

        update_text();
    }
    private void update_text()
    {
        int yes_count = (int)(percentage * bar_size);
        // int no_count = bar_size - yes_count;

        string bar = left_border;
        for (int i=0;i<bar_size;i++)
        {
            if (i<yes_count) { bar += yes_charac; continue; }
            bar += no_charac;
        }
        bar += right_border;

        lefter.Write(bar);

        string perc = ((int)(percentage * 100f)).ToString() + perc_suffix;
        righter.Write(perc);
    }
    private void update_simulation()
    {
        float delta_p = Time.unscaledDeltaTime / simulation_time;
        percentage += delta_p;
        if (percentage > 1f) { percentage = 1f; }
    }
}