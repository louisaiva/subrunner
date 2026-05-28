using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using System.Collections.Generic;
using System;

public class UI_GraphicColorSweeper : MonoBehaviour
{
    
    [SerializeField] private Graphic graphic;
    [SerializeField] private ColorSweep sweep;
    [SerializeField] private bool one_shot = false; // disables itself after first sweep if true
    private Color _base_color;
    private Sequence? sequence = null;

    [SerializeField] private Loggable<UI_GraphicColorSweeper> log = new Loggable<UI_GraphicColorSweeper>();

    private void OnEnable()
    {
        _base_color = graphic.color;
    }
    private void OnDisable()
    {
        // we stop the sequence if we have one
        if (sequence != null) { sequence.Value.Stop(); }

        time_until_next_sweep = 0f;
        sequence = null;
        graphic.color = _base_color;
    }


    // update : do the sweep
    private float time_until_next_sweep = 0f;
    private void Update()
    {
        if (sweep == null) { return; }

        log.LogVerySpecific("Update called. Time until next sweep: " + time_until_next_sweep + ". Sequence is null: " + (sequence == null) + ". Sequence is alive: " + (sequence != null ? sequence.Value.isAlive.ToString() : "N/A"));

        // if we are waiting between sweeps, we check if we can start a new one
        if (time_until_next_sweep > 0f)
        {
            // we decrease the time until next sweep
            time_until_next_sweep -= sweep.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            return;
        }

        if (sequence == null)
        {
            sequence = sweep.RunSweep(_base_color, graphic, ref log);
            time_until_next_sweep = 0f;
            return;
        }

        // else we have a running tween.
        // we check if it is done
        if (!sequence.Value.isAlive)
        {
            sequence = null;
            time_until_next_sweep = sweep.DelayBetweenSweeps;
            if (one_shot) { enabled = false; }
            return;
        }
    }
}

[Serializable] public class ColorSweep
{
    [SerializeField] private List<Color> colors;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float pause_between = 0f; // if 0 we have no pause, we sweep continuously
    public float DelayBetweenSweeps { get { return pause_between; } }
    [SerializeField] private Ease ease = Ease.Default;
    [SerializeField] private bool unscaled_time = false;
    public bool UnscaledTime { get { return unscaled_time; } }
    [SerializeField] private bool reversed = false;
    [SerializeField] private bool doubled = false;

    public ColorSweep()
    {
        colors = new List<Color>() { Color.white, Color.black };
    }
    public ColorSweep(List<Color> colors, float duration, float pause_between, Ease ease)
    {
        this.colors = colors;
        this.duration = duration;
        this.pause_between = pause_between;
        this.ease = ease;
    }


    public Sequence RunSweep(Color default_color, Graphic graphic, ref Loggable<UI_GraphicColorSweeper> log)
    {
        // gather the color list for the sweep
        List<Color> this_sweep_colors = new List<Color>() { default_color };
        if (reversed && !doubled)
        {
            for (int i = colors.Count - 1; i >= 0; i--) // we reverse the colors
            {
                this_sweep_colors.Add(colors[i]);
            }
        }
        else { this_sweep_colors.AddRange(colors); }
        if (doubled && !reversed) { this_sweep_colors.AddRange(colors); } // we add a normal sweep if double
        else if (doubled && reversed)
        {
            for (int i = colors.Count - 1; i >= 0; i--) // we reverse the colors
            {
                this_sweep_colors.Add(colors[i]);
            }
        }
        this_sweep_colors.Add(default_color);

        // creating the tweens
        Sequence sequence = Sequence.Create(useUnscaledTime: unscaled_time);
        for (int i = 0; i < this_sweep_colors.Count; i++)
        {
            Color start_color = this_sweep_colors[i];
            Color end_color = this_sweep_colors[(i + 1) % this_sweep_colors.Count]; // we loop back to the first color at the end of the list

            sequence.Chain(Tween.Custom(0f, 1f, duration,
                onValueChange: ctx =>
                {
                    apply_color(graphic, start_color, end_color, ctx);
                }, ease: ease, useUnscaledTime: unscaled_time));
        }

        if (log.Verbose >= Verbosity.Specific)
        {
            string logg = $"(ColorSweep) Running sweep with colors: ";
            foreach (Color c in this_sweep_colors)
            {
                logg += $"\n  - {c} ";
            }
            logg += $"\nTotal duration planned for the sweep: {duration * this_sweep_colors.Count} seconds. Sequence has {sequence.durationTotal} children.";
            log.LogSpecific(logg);
        }


        return sequence;
    }

    private void apply_color(Graphic graphic, Color start_color, Color end_color, float ctx)
    {
        Color color = Color.Lerp(start_color, end_color, ctx);
        graphic.color = color;
    }
}