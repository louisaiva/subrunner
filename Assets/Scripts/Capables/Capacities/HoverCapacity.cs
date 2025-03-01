
using UnityEngine;

/// <summary>
/// HoverCapacity is a capacity that ONLY shows the hover animation of an Interactable.
/// </summary>

[RequireComponent(typeof(Collider2D))]
public class HoverCapacity : Capacity
{
    // HOVER
    public void Hover(Capable capable)
    {
        // we play the animation
        Use(this.capable);

        if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " hovered " + this.capable.name); }
    }
    public void Unhover(Capable capable)
    {
        // we stop the animation
        this.capable.anim_player.StopPlaying(name);

        // we check if the capable is a Chest, and if its opened than we close it
        // we force (dont check the Can()) it so even if it is still opening it will close

        if (this.capable is Chest /* && capable.Can("close") */)
        {
            Chest chest = this.capable as Chest;
            if (chest.is_open && !chest.is_moving || chest.is_moving)
            {
                if (debug) { Debug.Log("(HoverCapacity) " + capable.name + " called close() on the chest " + this.capable.name); }
                chest.Do("close");
            }
        }
    }
}