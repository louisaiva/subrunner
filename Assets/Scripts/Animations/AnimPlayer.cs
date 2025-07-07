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



    [Header("Skin")]
    public string skin;

    [Header("Orientation")]
    public string orientation { get; private set; } = "D";





    [Header("Current Animation")]
    public string current_capacity = "";
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over





    [Header("Animation Pile")]
    public List<string> anim_pile = new();
    private Dictionary<string, int> capacity_priorities = new()
            {
            {"idle",0},
            {"walk",1},
            {"run",1},
            {"hover",1},
            {"idle_open",2},
            {"idle_sleep",2},
            {"attack",3},
            {"spawn",3},
            {"open",3},
            {"close",3},
            {"dodge",3},
            {"hurted",3},
            {"eat",4},
            {"fell_asleep",4},
            {"wake_up",4},
            {"lick_foot",4},
            {"throw",5},
            {"die",5}};
    // todo à transformer en List<CapacityPriority> sans MonoBehaviour pour pouvoir les éditer dans l'éditeur

    public List<int> animation_priorities_with_no_loop = new() { 3,4 };
        // we never loop the animation if it's in this priority (attack, dodge, hurted) -> always play once
    public List<int> animation_priorities_with_static_orientation = new() { 3,4 };
        // we can't interrupt the animation for switching orientation if it's in this list (wait the end of the anim before changing orientation)
        // only for orientation, not for switching to another animation (ex we can interrupt dodge to attack bcz they have the same priority 3)



    [Header("Debug")]
    public bool debug = false;
    public bool debug_orientation = false;
    public bool debug_advanced = false;
    public bool debug_frames = false;
    public bool debug_pile = false;




    // Start is called before the first frame update
    private void Start()
    {
        if (bank == null)
        {
            bank = GameObject.Find("/utils/bank").GetComponent<AnimBank>();
        }

        // we get the sprite renderer
        sr = GetComponent<SpriteRenderer>();

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
        if (debug_frames)
        {
            if (current_anim != null) { Debug.Log("(AnimPlayer) Current frame: " + current_frame + " Current anim: " + current_anim.name); }
            // else { Debug.Log("(AnimPlayer) Current frame: " + current_frame + " Current anim: null"); }
        }

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
        }
    }


    // PLAY ANIMATION
    public Anim Play(string capacity, int? priority_override=null, float? duration_override=null)
    {
        // we get the priority of the capacity
        int priority = 1;
        if (priority_override != null)
        {
            priority = (int)priority_override;
            if (debug_pile) { Debug.Log("(AnimPlayer - Play) The capacity " + capacity + " has a priority override: " + priority); }
            capacity_priorities[capacity] = priority;
        }
        else if (priority_override == null && capacity_priorities.ContainsKey(capacity))
        {
            priority = capacity_priorities[capacity];
            if (debug_pile) { Debug.Log("(AnimPlayer - Play) The capacity " + capacity + " has a priority: " + priority + " from the capacity_priorities dictionnary"); }
        }
        else
        {
            if (debug_pile) { Debug.Log("(AnimPlayer - Play) The capacity " + capacity + " doesn't exist in the capacity_priorities dictionnary. Priority 1 applied by default"); }
            capacity_priorities[capacity] = priority;
        }

        // we check if we can play the animation
        if (priority >= getPileMaxPriority())
        {
            // we get the animation from the bank
            string anim_name = skin + "." + capacity + "." + orientation;
            Anim anim = bank.GetAnim(anim_name);
            if (debug)
            {
                Debug.Log("(AnimPlayer - Play) Bank found anim : " + anim.name
                    + (anim_name == anim.name
                    ? ""
                    : " (" + anim_name + " was asked)"));
            }

            // we check if the duration is overriden
            if (duration_override != null)
            {
                // we get the duration of the animation
                float duration = anim.GetBaseDuration();
                
                // we calculate the resulting speed
                anim.speed = duration / (float) duration_override;
            }


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
        if (debug_pile) {Debug.LogWarning("(AnimPlayer - Play) Adding " + capacity + " to the pile at priority " + priority);}

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
            string anim_name = skin + "." + anim_pile[i] + "." + orientation;
            Anim anim = bank.GetAnim(anim_name);
            if (debug_pile)
            {
                Debug.Log("(AnimPlayer - playFromPile) Bank found anim : " + anim.name
                    + (anim_name == anim.name
                    ? ""
                    : " (" + anim_name + " was asked)"));
            }
            /* if (anim == null)
            {
                if (debug) {Debug.LogWarning("(AnimPlayer - playFromPile) No animation " + skin + "." + anim_pile[i] + "." + orientation + " found to play in the bank");}
                continue;
            }
            if (debug_advanced) {Debug.Log("(AnimPlayer - playFromPile) Found an animation to play: " + anim.name + " for capacity " + anim_pile[i]);} */

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

        if (debug_advanced) {Debug.Log("(AnimPlayer) Playing " + anim.name + " at frame " + frame + " flipX: " + anim.flipX);}
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
    public void ClearPile()
    {
        // we remove ALL animations from the pile
        for (int i = 0; i < anim_pile.Count; i++)
        {
            anim_pile[i] = "";
        }

        // we play the idle animation
        Play("idle");
    }

    // PILE MANAGEMENT
    private int getPileMaxPriority()
    {
        for (int i = anim_pile.Count - 1; i >= 0; i--)
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
                if (debug_pile) { Debug.Log("(AnimPlayer) Removing " + capacity + " from the pile"); }
                return;
            }
        }
    }


    // ORIENTATION
    public void SetOrientation(Vector2 look_at)
    {
        if (debug_orientation) { Debug.Log("(AnimPlayer) Changing " + name + " orientation to " + look_at); }


        // todo - next step is to work with 12 orientations instead of 8
        // todo - L,LU,UL,U,UR,RU,R,RD,DR,D,DL,LD
        // todo - bcz for now if we're at 65° the AnimBank returns a L animation even if we
        // todo - are closer to the D one


        // we get the angle from the normalized vector (between 0 & 360f)
        float angle = Mathf.Atan2(look_at.y, look_at.x) * Mathf.Rad2Deg + 180f;
        if (angle >= 337.5f || angle < 22.5f) { setOrientation("L"); }
        else if (angle >= 292.5f) { setOrientation("LU"); }
        else if (angle >= 247.5f) { setOrientation("U"); }
        else if (angle >= 202.5f) { setOrientation("RU"); }
        else if (angle >= 157.5f) { setOrientation("R"); }
        else if (angle >= 112.5f) { setOrientation("RD"); }
        else if (angle >= 67.5f) { setOrientation("D"); }
        else { setOrientation("LD"); }
    }
    private void setOrientation(string orientation)
    {
        // we check if the orientation is different
        if (orientation == this.orientation) { return; }

        // we set the orientation
        //if (new List<string> { "U", "D", "L", "R" }.Contains(orientation)){
        this.orientation = orientation;//}

        // we check if we can interrupt the current animation to update orientation
        if (current_capacity == "") { return; }
        int current_anim_priority = capacity_priorities[current_capacity];
        if (animation_priorities_with_static_orientation.Contains(current_anim_priority)) { return; }

        // we check if the current animation is in the pile
        Anim new_anim = Play(current_capacity);
        if (new_anim == null) { return; }
        if (debug_orientation)
        {
            string s = "(AnimPlayer) Interrupted : Changing orientation to ";
            s += this.orientation + " | anim switched to ";
            s += new_anim.name;
            Debug.Log(s);
        }
    }
}