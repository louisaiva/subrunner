
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HoverCapacity is a capacity that ONLY shows the hover animation of an Interactable.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class HoverCapacity : Capacity
{
    private string played_animation = "hover";
    // [SerializeField] private bool hovered = false;
    [SerializeField] private List<Capable> hoverers = new List<Capable>();
    public bool Hovered { get { return hoverers.Count > 0; } }

    // HOVER
    public void Hover(Capable capable)
    {
        // we add the capable to the hoverers list
        if (hoverers.Contains(capable)) { return; }
        hoverers.Add(capable);

        // then we only play animation if the capable is the one controlled
        if (capable != PersoInputsController.Instance.Capable) { return; }

        // we check if the capable is locked or not
        played_animation = "hover";
        if (this.capable is Lockable lockable && lockable.Locked) { played_animation = "hover_locked"; }

        // we play the animation
        this.capable.anim_player.Play(played_animation);

        if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " hovered " + this.capable.name + $", playing {played_animation}"); }
    }
    public void Unhover(Capable capable)
    {
        if (!hoverers.Contains(capable)) { return; }
        hoverers.Remove(capable);

        // we stop the animation
        this.capable.anim_player.StopPlaying(played_animation);

        if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " stop hovering " + this.capable.name + $", stopped playing {played_animation}"); }


        // we check if the capable is a Chest, and if its opened than we close it
        // we force (dont check the Can()) it so even if it is still opening it will close
        // todo : move this to close capacity
        if (this.capable is not Chest chest) { return; }
        if (chest.is_open && !chest.is_moving || chest.is_moving)
        {
            if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " called close() on the chest " + this.capable.name); }
            chest.Do("close");
        }
    }

    // UPDATE HOVER ANIMATION
    public void ChangeAnimation(string animation = "hover")
    {
        if (!Hovered) { return; }

        // we stop the current animation
        capable.anim_player.StopPlaying(played_animation);

        // we play the new animation
        played_animation = animation;
        capable.anim_player.AddToPile(played_animation);
    }
}