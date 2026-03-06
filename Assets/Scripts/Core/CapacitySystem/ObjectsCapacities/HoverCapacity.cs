
using System;
using System.Collections.Generic;
using CrashKonijn.Goap.Editor;
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




    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        HoverCapacityData static_data = new HoverCapacityData
        {
            // set base data things
            id = this.name,
            local_position = this.transform.localPosition,

            // we set the kind
            kind = GetType().Name,

            // we get the hover collider data
            hover_collider_data = get_static_circle_data(GetComponentInChildren<CircleCollider2D>(includeInactive: true))
        };

        return static_data;
    }
    private CircleData get_static_circle_data(CircleCollider2D collider)
    {
        // if (log_static_data) { Debug.Log($"(Capable - GetStaticData - {name}) CircleCollider2D found with offset {collider.offset} and radius {collider.radius} and is_trigger = {collider.isTrigger}"); }
        return new CircleData
        {
            radius = collider.radius,
            local_position = collider.transform.localPosition,
            layerID = collider.gameObject.layer,
            offset = collider.offset,
            is_trigger = collider.isTrigger,
            used_for_pathfinding = false // hover colliders are never used for pathfinding
        };
    }
}

[Serializable] public class HoverCapacityData : CapacityData
{
    // need to store a collider data for the hover to work
    public CircleData hover_collider_data;


    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new HoverCapacityData()
        {
            id = this.id + "_copy",
            kind = this.kind,
            local_position = this.local_position,
            hover_collider_data = this.hover_collider_data != null ? this.hover_collider_data.Duplicate() as CircleData : null
        };
    }

    // GET DETAILS
    public override string GetDetails()
    {
        string details = "";
        if (hover_collider_data != null) { details += $"  - hover {hover_collider_data.GetDetails()}\n"; }
        else { details += $"  - no hover collider data\n"; }
        return base.GetDetails() + details;
    }
}