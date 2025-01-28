using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimPlayer : MonoBehaviour
{
    [Header("Components")]
    private AnimBank bank;
    private SpriteRenderer sr;
    // private PolygonCollider2D pc;

    [Header("Skin")]
    public string skin;

    [Header("Orientation")]
    public string orientation = "L";


    [Header("Current Animation")]
    public string current_capacity = "";
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over

    [Header("Animation Pile")]
    public List<string> anim_pile = new();
    public Dictionary<string, int> capacity_priorities = new() {
            {"idle",0},
            {"walk",1},
            {"run",1},
            {"attack",2},
            {"dodge",2},
            {"hurted",2},
            {"die",3}};

    public List<int> animation_priorities_with_no_loop = new() { 2 };
        // we never loop the animation if it's in this priority (attack, dodge, hurted) -> always play once
    public List<int> animation_priorities_with_no_interrupt = new() { 2 };
        // we can't interrupt the animation if it's in this list (wait the end of the anim before changing orientation by example)



    [Header("Debug")]
    public bool debug = false;

    // Start is called before the first frame update
    private void Start()
    {
        if (bank == null)
        {
            bank = GameObject.Find("/utils/bank").GetComponent<AnimBank>();
        }

        // we get the sprite renderer
        sr = GetComponent<SpriteRenderer>();

        // we get the polygon collider
        // pc = GetComponent<PolygonCollider2D>();

        // we inspect the capacity_priorities and we create the anim_pile
        float highest_priority = capacity_priorities.Values.Max();
        for (int i = 0; i <= highest_priority; i++)
        {
            anim_pile.Add("");
        }

        // we play the idle animation
        Play("idle");
    }


    // Update
    private void Update()
    {
        // Debug.Log("Current frame: " + current_frame + " Current anim: " + current_anim.name);

        // we play the current animation
        if (current_frame != -1)
        {
            updateAnim();
        }
        // we play the highest animation in the pile if it's not the current one
        else
        {
            playFromPile();
        }

    }
    private void updateAnim()
    {
        frame_timer += Time.deltaTime;
        if (frame_timer >= current_anim.sprites_durations[current_frame] / current_anim.speed)
        {
            // we go to the next frame
            frame_timer = 0f;
            current_frame++;
            if (current_frame >= current_anim.sprites_durations.Length)
            {
                // the animation is over
                current_frame = -1;

                // we check if the priority of the current animation is in the list of priorities with no loop
                int current_anim_priority = capacity_priorities[current_capacity];
                if (animation_priorities_with_no_loop.Contains(current_anim_priority)) { removeFromPile(current_capacity); }
                // we check if it is looping or not (if not, we remove it from the pile)
                else if (!current_anim.loop) { removeFromPile(current_capacity); }

                // we play the highest animation in the pile
                playFromPile();

                return;
            }

            // we set the sprite
            sr.sprite = current_anim.sprites[current_frame];

            // we try to update the polygon collider
            // if (pc != null) { pc.TryUpdateShapeToAttachedSprite();}
        }
    }


    // PLAY ANIMATION
    public Anim Play(string capacity)
    {
        // we get the priority of the capacity
        int priority = 1;
        if (capacity_priorities.ContainsKey(capacity)) { priority = capacity_priorities[capacity]; }
        else { Debug.LogWarning("The capacity " + capacity + " doesn't exist in the capacity_priorities dictionnary. Priority 1 by default applied");}

        // we check if we can play the animation
        if (priority >= getPileMaxPriority())
        {
            // we get the animation from the bank
            string anim_name = skin + "." + capacity + "." + orientation;
            Anim anim = bank.GetAnim(anim_name);

            // we check if the animation is not actually playing
            if (anim.name != current_anim.name)
            {
                // we play the animation
                play_now_at_frame(anim);

                // we set the current capacity
                current_capacity = capacity;

                // we add the animation to the pile
                anim_pile[priority] = capacity;
                return anim;
            }
        }

        // we add the animation to the pile
        anim_pile[priority] = capacity;
        if (debug) {Debug.Log("Adding " + capacity + " to the pile at priority " + priority);}

        // we didn't play the animation so we return null
        return null;

    }
    private void playFromPile()
    {
        for (int i = anim_pile.Count - 1; i >= 0; i--)
        {
            // we check if there is an animation to play
            if (anim_pile[i] == "") { continue; }

            // we get the closest animation from the bank
            Anim anim = bank.GetAnim(skin + "." + anim_pile[i] + "." + orientation);
            /* if (anim == null)
            {
                Debug.LogWarning("The animation " + skin + "." + anim_pile[i] + "." + orientation + " doesn't exist in the bank");
                continue;
            } */
            if (debug) {Debug.Log("Found an animation to play: " + anim.name + " for capacity " + anim_pile[i]);}

            // we set the current capacity
            current_capacity = anim_pile[i];

            // we play the animation
            play_now_at_frame(anim);
            return;
        }
    }
    private void play_now_at_frame(Anim anim, int frame=0)
    {
        // we saturate the frame
        if (frame < 0) { frame = 0; }
        else if (frame >= anim.sprites.Length) { frame = 0; }

        // we play the animation
        if (current_anim != anim)
        {
            current_anim = anim;
        }
        current_frame = frame;
        frame_timer = 0f;

        // we set the sprite
        sr.sprite = anim.sprites[current_frame];

        // we flip the sprite renderer if needed
        if (anim.flipX && !sr.flipX) { sr.flipX = true; }
        else if (!anim.flipX && sr.flipX) { sr.flipX = false; }

        if (debug) {Debug.Log("Playing " + anim.name + " at frame " + frame + " flipX: " + anim.flipX);}
    }

    // STOP ANIMATION
    public void StopPlaying(string capacity)
    {
        // we check if the capacity is in the pile
        if (!anim_pile.Contains(capacity)) { return; }

        // we remove the capacity from the pile
        removeFromPile(capacity);

        // if we were playing the capacity, we stop it
        if (current_capacity == capacity)
        {
            playFromPile();
        }
    }

    // PILE MANAGEMENT
    private int getPileMaxPriority()
    {
        for (int i = anim_pile.Count-1; i >= 0; i--)
        {
            if (anim_pile[i] != "") { return i; }
        }
        return -1;
    }
    private void removeFromPile(string capacity)
    {
        for (int i = 0; i < anim_pile.Count; i++)
        {
            if (anim_pile[i] == capacity)
            {
                anim_pile[i] = "";
                if (debug) { Debug.Log("Removing " + capacity + " from the pile"); }
                return;
            }
        }
    }

    // ORIENTATION
    public void SetOrientation(Vector2 look_at)
    {
        // we separate the 360° in 4 directions (up, down, left, right)
        if (look_at.y > 0.5) { SetOrientation("U"); }
        else if (look_at.y < -0.5) { SetOrientation("D"); }
        else if (look_at.x > 0.5) { SetOrientation("R"); }
        else if (look_at.x < -0.5) { SetOrientation("L"); }
    }
    public void SetOrientation(string orientation)
    {
        // we check if the orientation is different
        if (orientation == this.orientation) { return; }

        // we set the orientation
        if (new List<string> { "U", "D", "L", "R" }.Contains(orientation))
        { this.orientation = orientation; }

        // we check if we can interrupt the current animation
        int current_anim_priority = current_capacity == "" ? 0 : capacity_priorities[current_capacity];
        if (animation_priorities_with_no_interrupt.Contains(current_anim_priority)) { return; }

        // we check if the current animation is in the pile
        Anim new_anim = Play(current_capacity);
        if (new_anim == null) { return; }
        if (debug)
        {
            string s = "(AnimPlayer) Interrupted : Changing orientation to ";
            s += this.orientation + " | anim switched to ";
            s += new_anim.name;
            Debug.Log(s);
        }
    }
}