
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HoverCapacity is a capacity that ONLY shows the hover animation of an Interactable.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class HoverCapacity : Capacity
{
    [Header("Hover Capacity")]
    private string played_animation = "hover";
    [SerializeField] private List<Capable> hoverers = new List<Capable>();
    public bool Hovered { get { return hoverers.Count > 0; } }

    [Header("Interact Key Feedback")]
    [SerializeField] private Transform canvas_kf;
    public Transform Canvas_kf { get { return canvas_kf; } }

    // DELEGATES
    public event Action<Capable> OnHover = delegate { };
    public event Action<Capable> OnHoverLost = delegate { };

    // AWAKE
    private void Awake()
    {
        // check if we have a canvas_kf
        canvas_kf = transform.Find("canvas_kf");
        if (canvas_kf != null)
        {
            canvas_kf.gameObject.SetActive(false);

            // sets some callbacks to dynamically show the interact key feedback
            OnHover += (capable) => toggle_key_feedback(capable, true);
            OnHoverLost += (capable) => toggle_key_feedback(capable, false);
        }
    }

    // HOVER
    public void Hover(Capable capable)
    {
        // we add the capable to the hoverers list
        if (hoverers.Contains(capable)) { return; }
        hoverers.Add(capable);

        OnHover?.Invoke(capable);

        // then we only play animation if the capable is the one controlled
        if (Controller.Instance == null || capable != Controller.Instance.Capable) { return; }
        
        // we play the animation
        this.capable.anim_player.Play(played_animation);

        if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " hovered " + this.capable.name + $", playing {played_animation}"); }
    }
    public void Unhover(Capable capable)
    {
        if (!hoverers.Contains(capable)) { return; }
        hoverers.Remove(capable);

        // then we only stop playing animation if the capable is the one controlled
        if (Controller.Instance != null && capable == Controller.Instance.Capable) { this.capable.anim_player.StopPlaying(played_animation); } // we stop the animation

        OnHoverLost?.Invoke(capable);
        if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " stop hovering " + this.capable.name + $", stopped playing {played_animation}"); }
    }

    // UPDATE HOVER ANIMATION
    public void ChangeAnimation(string animation = "hover")
    {
        if (!Hovered) { played_animation = animation; return; }

        // we stop the current animation
        capable.anim_player.StopPlaying(played_animation);

        // we play the new animation
        played_animation = animation;
        capable.anim_player.AddToPile(played_animation);
    }


    // HANDLE KEY FEEDBACK
    private void toggle_key_feedback(Capable hoverer, bool show)
    {
        if (canvas_kf == null) { return; }
        if (Controller.Instance == null) { return; }
        if (hoverer != Controller.Instance.Capable) { return; }
        canvas_kf.gameObject.SetActive(show);
    }

}