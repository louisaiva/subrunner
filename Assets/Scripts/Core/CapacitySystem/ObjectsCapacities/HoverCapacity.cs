
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HoverCapacity is a capacity that ONLY shows the hover animation of an Interactable.
/// </summary>

public class HoverCapacity : Capacity
{
    [Header("Hover Capacity")]
    private string played_animation = "hover";
    [SerializeField] private List<Capable> hoverers = new List<Capable>();
    public bool Hovered { get { return hoverers.Count > 0; } }

    [Header("Interact Key Feedback")]
    [SerializeField] private Transform canvas_kf;
    public Transform Canvas_kf { get { return canvas_kf; } }


    // HOVER COLLIDER
    private CircleCollider2D _hover_collider = null;
    public CircleCollider2D HoverCollider
    {
        get
        {
            if (_hover_collider == null) { _hover_collider = transform.GetComponentInChildren<CircleCollider2D>(includeInactive:true); }
            return _hover_collider;
        }
    }


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
        this.Capable.AnimPlayer.Play(played_animation);

        if (log) { Debug.Log("(HoverCapacity) " + capable.name + " hovered " + this.Capable.name + $", playing {played_animation}"); }
    }
    public void Unhover(Capable capable)
    {
        if (!hoverers.Contains(capable)) { return; }
        hoverers.Remove(capable);
        
        if (this.Capable == null) { Debug.LogWarning($"(HoverCapacity) this.capable is null on {name}"); }

        // then we only stop playing animation if the capable is the one controlled
        if (Controller.Instance != null && capable == Controller.Instance.Capable) { this.Capable.AnimPlayer.StopPlaying(played_animation); } // we stop the animation

        OnHoverLost?.Invoke(capable);
        if (log) { Debug.Log("(HoverCapacity) " + capable.name + " stop hovering " + this.Capable.name + $", stopped playing {played_animation}"); }
    }

    // UPDATE HOVER ANIMATION
    public void ChangeAnimation(string animation = "hover")
    {
        if (!Hovered) { played_animation = animation; return; }

        // we stop the current animation
        Capable.AnimPlayer.StopPlaying(played_animation);

        // we play the new animation
        played_animation = animation;
        Capable.AnimPlayer.AddToPile(played_animation);
    }


    // HANDLE KEY FEEDBACK
    private void toggle_key_feedback(Capable hoverer, bool show)
    {
        if (canvas_kf == null) { return; }
        if (Controller.Instance == null) { return; }
        if (hoverer != Controller.Instance.Capable) { return; }
        canvas_kf.gameObject.SetActive(show);
    }



    // LOAD / UNLOAD DATA
    public override void LoadData(CapacityData data)
    {
        base.LoadData(data);

        if (data is not HoverCapacityData hover_data) { return; }
        if (hover_data.hover_collider_data == null) { return; }

        // then we load the collider
        _hover_collider = ColliderBank.Instance.LoadCollider(hover_data.hover_collider_data, this.transform) as CircleCollider2D;
    }
    public override void UnloadData()
    {
        // we tell the hoverers we don't exist anymore
        while (hoverers.Count > 0)
        {
            hoverers[0].GetCapacity<InteractCapacity>()?.HoverLostItself(this);
        }
        this.hoverers.Clear();

        // we unload the collider
        if (this.HoverCollider != null) { ColliderBank.Instance.UnloadCollider(this.HoverCollider.gameObject); }
        this._hover_collider = null;

        base.UnloadData();

    }


    // GET STATIC DATA
    public override CapacityData GetStaticData()
    {
        HoverCapacityData static_data = new HoverCapacityData(base.GetStaticData())
        {
            // we get the hover collider data
            hover_collider_data = get_static_circle_data(GetComponentInChildren<CircleCollider2D>(includeInactive: true))
        };

        return static_data;
    }
    private CircleData get_static_circle_data(CircleCollider2D collider)
    {
        // if (collider == null) { return null; }
        // if (log_static_data) { Debug.Log($"(Capable - GetStaticData - {name}) CircleCollider2D found with offset {collider.offset} and radius {collider.radius} and is_trigger = {collider.isTrigger}"); }

        // setup basic data
        ColliderData data = new ColliderData
        {
            local_position = collider.transform.localPosition,
            layerID = collider.gameObject.layer,
            offset = collider.offset,
            is_trigger = collider.isTrigger,
            used_for_pathfinding = false // hover colliders are never used for pathfinding
        };
        return new CircleData(data)
        {
            radius = collider.radius
        };
    }
}

[Serializable] public class HoverCapacityData : CapacityData
{
    // need to store a collider data for the hover to work
    public CircleData hover_collider_data;

    // CONSTRUCTOR
    public HoverCapacityData(CapacityData parent)
    {
        foreach (var prop in parent.GetType().GetProperties()) { prop.SetValue(this, prop.GetValue(parent)); }
        foreach (var prop in parent.GetType().GetFields()) { prop.SetValue(this, prop.GetValue(parent)); }
    }

    // DUPLICATE
    public override ICapacityData Duplicate()
    {
        return new HoverCapacityData(base.Duplicate() as CapacityData)
        {
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