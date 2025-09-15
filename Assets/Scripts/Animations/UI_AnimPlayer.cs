using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// UI_AnimPlayer is a AnimPlayer fork for UI Elements.
/// Doesn't handle animation capacity pile, it is only very simple for now
/// used mainly in the title screen
/// always loop
/// </summary>
[RequireComponent(typeof(Image))]
public class UI_AnimPlayer : MonoBehaviour
{

    [Header("UI Related Parameters")]
    public bool resize_to_native_size = true;
    public bool play_on_start = true;
    public bool unscaled_time = false;

    [Header("Components")]
    public AnimBank Bank
    {
        get
        {
            if (bank == null)
            {
                bank = AnimBank.Instance;
            }
            return bank;
        }
    }
    private AnimBank bank;
    private Image img;


    [Header("Skin")]
    public string skin;

    [Header("Orientation")]
    public string orientation = "L";



    [Header("Current Animation")]
    public string current_capacity = "";
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over
    public bool IsPlaying => current_frame != -1;



    [Header("Logs")]
    public bool debug = false;
    public bool debug_orientation = false;
    public bool debug_advanced = false;
    public bool debug_frames = false;


    // Awake & Start
    private void Awake()
    {
        img = GetComponent<Image>();
    }
    private void Start()
    {
        // we play the idle animation
        if (play_on_start) { Play("idle"); }
    }


    // Update
    private void Update()
    {
        if (current_frame == -1 || current_anim == null) { return; }

        // update timer
        frame_timer += unscaled_time ? Time.unscaledDeltaTime : Time.deltaTime;

        // checks if timer reach the next anim frame
        if (!(frame_timer >= current_anim.sprites_durations[current_frame] / current_anim.speed)) { return; }

        // we go to the next frame
        frame_timer = 0f;
        current_frame++;
        if (current_frame >= current_anim.sprites_durations.Length)
        {
            // if the animation is not looping, we stop it
            if (!current_anim.loop)
            {
                current_frame = -1;
                if (debug_frames) { Debug.Log($"(UI_AnimPlayer) Animation {current_anim.name} ended."); }
                return;
            }
            // we loop the animation
            current_frame = 0;
        }

        // we set the sprite
        img.sprite = current_anim.sprites[current_frame];
        if (resize_to_native_size && img.sprite != null)
        {
            img.SetNativeSize();
        }
    }


    // PLAY ANIMATION
    public Anim Play(string capacity,bool? loop_override=null)
    {
        // we get the animation from the bank
        string anim_name = skin + "." + capacity + "." + orientation;
        Anim anim = Bank.GetAnim(anim_name);
        if (anim == null)
        {
            if (debug) { Debug.Log("(UI_AnimPlayer - Play) could not Play() : " + anim_name + " no contact with bank"); }
            return null;
        }
        if (debug) { Debug.Log("(UI_AnimPlayer - Play) Bank found anim : " + anim.name
                + (anim_name == anim.name
                ? ""
                : " (" + anim_name + " was asked)")); }

        // we check if we have a loop override
        if (loop_override != null)
        {
            // we set the loop of the animation
            anim.loop = (bool)loop_override;
        }

        // we check if the animation is not actually playing
        if (anim.name != current_anim.name)
        {
            // we play the animation
            play_now_at_frame(anim);

            // we set the current capacity
            current_capacity = capacity;
            return anim;
        }

        // we didn't play the animation so we return null
        return null;

    }
    private void play_now_at_frame(Anim anim, int frame = 0)
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
        img.sprite = anim.sprites[current_frame];

        // we set the right size to the rect transform
        if (resize_to_native_size && img.sprite != null)
        {
            img.SetNativeSize();
        }

        // we flip the sprite renderer if needed
        RectTransform rt = img.rectTransform;
        rt.localScale = new Vector3(anim.flipX ? -1 : 1, 1, 1);

        // we check if its a one frame animation we instantly stop it
        if (anim.sprites.Length == 1) { current_frame = -1; }


        if (debug_advanced) { Debug.Log("(AnimPlayer) Playing " + anim.name + " at frame " + frame); }
    }

    // ORIENTATION
    public void SetOrientation(Vector2 look_at)
    {
        if (debug_orientation) { Debug.Log("(AnimPlayer) Changing " + name + " orientation to " + look_at); }

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

        // we check if we have a current_animation playing
        if (current_capacity == "") { return; }

        // we play the animation with the new orientation
        Anim new_anim = Play(current_capacity);
        if (new_anim == null)
        {
            if (debug_orientation) { Debug.LogWarning("(AnimPlayer) Could not change orientation to " + orientation + " for " + current_capacity); }
            return;
        }
        if (debug_orientation)
            {
                string s = "(AnimPlayer) Interrupted : Changing orientation to ";
                s += this.orientation + " | anim switched to ";
                s += new_anim.name;
                Debug.Log(s);
            }
    }
}