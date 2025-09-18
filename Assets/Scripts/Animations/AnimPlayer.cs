using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimPlayer : MonoBehaviour
{

    [Header("Components")]
    private SpriteRenderer sr;


    [Header("Skin")]
    [SerializeField] private string skin;
    public string Skin
    {
        get { return skin; }
        set
        {
            if (value == skin) { return; }
            skin = value;
            OnSkillChange.Invoke(skin);
            Play(current_capacity);
        }
    }
    public event Action<string> OnSkillChange = delegate { };

    [Header("Orientation")]
    public string orientation { get; private set; } = "D";


    [Header("Current Animation")]
    public string current_capacity = "";
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over
    [SerializeField] private bool update_each_frame = true; // if false, it means that the animation is a single frame anim, we don't want to check it each frame



    [Header("Anim Capacity Priorities")]
    [SerializeField] private List<AnimCapacityPriority> anim_capacity_priorities = new();
    private AnimCapacityPriority current_capacity_priority = null;

    [Header("Logs")]
    public bool log = false;
    public bool log_orientation = false;
    public bool log_advanced = false;
    public bool log_frames = false;
    public bool log_pile = false;


    // AWAKE & START
    private void Awake()
    {
        // we get the sprite renderer
        sr = GetComponent<SpriteRenderer>();

        // we inform each anim capacity priority of its priority
        for (int i = 0; i < anim_capacity_priorities.Count; i++)
        {
            anim_capacity_priorities[i].priority = i;
        }
    }
    private void Start() { AddToPile("idle"); }


    // Update
    private void Update()
    {
        if (log_frames && current_anim != null)
        {
            Debug.Log("(AnimPlayer) Current frame: " + current_frame + " Current anim: " + current_anim.name);
        }

        // we play the current animation
        if (current_frame != -1 && update_each_frame) { updateAnim(); }
    }
    private void updateAnim()
    {
        // we update the timer
        frame_timer += Time.deltaTime;
        if (frame_timer < current_anim.sprites_durations[current_frame] / current_anim.speed) { return; }

        // we go to the next frame
        frame_timer = 0f;
        current_frame++;
        if (current_frame < current_anim.sprites_durations.Length)
        {
            sr.sprite = current_anim.sprites[current_frame];
            return;
        }


        // if we are here, it means that we reached the end of the animation
        // the animation is over
        current_frame = -1;

        // we check if it's looping or not
        if (current_capacity_priority.one_shot)
        {
            current_capacity_priority.capacity_playing = "";
        }
        
        // we play the highest animation in the pile
        playNextAnim();
    }


    // PLAY ANIMATION
    public Anim Play(string capacity, float duration_override = default)
    {
        if (AnimBank.Instance == null) { return null; }

        AnimCapacityPriority capacity_priority = getAnimCapacityPriority(capacity);
        if (capacity_priority == null)
        {
            if (log) { Debug.LogWarning("(AnimPlayer - Play) The capacity " + capacity + " doesn't exist in the anim_capacity_priorities list"); }
            return null;
        }

        // we check if the index is < than current prio we don't play it
        if (current_capacity_priority != null && capacity_priority.priority < current_capacity_priority.priority)
        {
            if (log_pile) { Debug.LogWarning("(AnimPlayer - Play) The capacity " + capacity + " has a lower priority than the current one (" + current_capacity_priority.priority + ")"); }
            return null;
        }

        // if the current playing animation is a one shot, we want to make sure it is properly stopped before playing a new animation
        if (current_capacity_priority != null && capacity_priority.priority > current_capacity_priority.priority && current_capacity_priority.one_shot)
        {
            current_capacity_priority.capacity_playing = "";
        }

        // we get the animation from the bank
        string anim_name = skin + "." + capacity + "." + orientation;
        Anim anim = AnimBank.Instance.GetAnim(anim_name);
        if (log)
        {
            Debug.Log("(AnimPlayer - Play) Bank found anim : " + anim.name
                + (anim_name == anim.name
                ? ""
                : " (" + anim_name + " was asked)"));
        }

        // we check if this anim is a single frame anim
        update_each_frame = !(anim.sprites.Length == 1);

        // we check if the duration is overriden
        if (duration_override != default)
        {
            // we get the duration of the animation
            float duration = anim.GetBaseDuration();

            // we calculate the resulting speed
            anim.speed = duration / (float)duration_override;
        } else { anim.speed = 1f; }

        // we check if the animation is not actually playing
        if (anim.name == current_anim.name && current_frame != -1)
        {
            if (log)
            {
                Debug.LogWarning("(AnimPlayer - Play) The animation " + anim.name + " is already playing at frame " + current_frame);
            }
            return current_anim;
        }
        
        // we play the animation    
        play_now_at_frame(anim);

        // we set the current capacity
        current_capacity = capacity;
        current_capacity_priority = capacity_priority;
        current_capacity_priority.capacity_playing = capacity;
        return anim;
    }
    private void playNextAnim()
    {
        string capacity = "";
        AnimCapacityPriority capacity_priority = null;

        /* // we check if we have a capacity in the pile
        if (capacity_pile != "")
        {
            capacity = capacity_pile.Split(',').FirstOrDefault();
            capacity_priority = getAnimCapacityPriority(capacity);
            capacity_pile = capacity_pile.Remove(0, capacity.Length + 1); // remove the first capacity and the comma after it
        }
        else
        { */
        // we go through the anim capacity priorities and we get the highest one playing a capacity
        int highest_priority = -1;
        foreach (AnimCapacityPriority priority in anim_capacity_priorities)
        {
            if (priority.priority < highest_priority) { continue; }
            if (priority.capacity_playing == "") { continue; }

            capacity = priority.capacity_playing;
            highest_priority = priority.priority;
            capacity_priority = priority;
        }
        // }

        // we get the animation from the bank
        string anim_name = skin + "." + capacity + "." + orientation;
        Anim anim = AnimBank.Instance.GetAnim(anim_name);
        if (log)
        {
            Debug.Log("(AnimPlayer - Play) Bank found anim : " + anim.name
                + (anim_name == anim.name
                ? ""
                : " (" + anim_name + " was asked)"));
        }

        // we check if this anim is a single frame anim
        update_each_frame = !(anim.sprites.Length == 1);

        // we play the animation    
        play_now_at_frame(anim);

        // we set the current capacity
        current_capacity = capacity;
        current_capacity_priority = capacity_priority;
        current_capacity_priority.capacity_playing = capacity;
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
        sr.sprite = anim.sprites[current_frame];

        // we flip the sprite renderer if needed
        if (anim.flipX && !sr.flipX) { sr.flipX = true; }
        else if (!anim.flipX && sr.flipX) { sr.flipX = false; }

        if (log_advanced) { Debug.Log("(AnimPlayer) Playing " + anim.name + " at frame " + frame + " flipX: " + anim.flipX); }
    }

    // STOP ANIMATION
    public void StopPlaying(string capacity, bool dont_stop_if_currently_playing = false)
    {

        // we get the anim capa prio
        AnimCapacityPriority priority = getAnimCapacityPriority(capacity);
        if (priority == null)
        {
            if (log_pile) { Debug.LogWarning("(AnimPlayer - StopPlaying) The capacity " + capacity + " doesn't exist in the anim_capacity_priorities list"); }
            return;
        }

        // we make it stop playing
        priority.capacity_playing = "";

        // we check if we have to brutally stop the current playing animation
        if (dont_stop_if_currently_playing || current_capacity != capacity) { return; }
        playNextAnim();
        
    }
    public void ClearPile()
    {
        // we remove ALL animations from the pile
        foreach (AnimCapacityPriority priority in anim_capacity_priorities)
        {
            priority.capacity_playing = "";
        }

        // we play the idle animation
        Play("idle");
    }

    // PILE MANAGEMENT
    private AnimCapacityPriority getAnimCapacityPriority(string capacity)
    {
        return anim_capacity_priorities.FirstOrDefault(p => p.capacities.Contains(capacity));
    }
    public void AddToPile(string capacity)
    {
        AnimCapacityPriority capacity_priority = getAnimCapacityPriority(capacity);
        if (capacity_priority == null)
        {
            if (log_pile) { Debug.LogWarning("(AnimPlayer - AddToPile) The capacity " + capacity + " doesn't exist in the anim_capacity_priorities list"); }
            return;
        }

        // if we don't have any ongoing anim, we simply play it
        if (current_capacity_priority == null) { Play(capacity); return; }

        // we check if the index is >= than current prio we play it also
        if (capacity_priority.priority >= current_capacity_priority.priority)
        {
            if (log_pile) { Debug.LogWarning("(AnimPlayer - AddToPile) The capacity " + capacity + " has a higher priority than the current one. we play it rn"); }
            Play(capacity);
            return;
        }

        // else we don't want to play it rn we simply add it to the pile for it to be played next
        capacity_priority.capacity_playing = capacity;
        if (log_pile) { Debug.Log("(AnimPlayer - AddToPile) Added " + capacity + " to the pile"); }
    }

    // ORIENTATION
    public void SetOrientation(Vector2 look_at)
    {
        if (log_orientation) { Debug.Log("(AnimPlayer) Changing " + name + " orientation to " + look_at); }


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
        this.orientation = orientation;

        // we check if we can interrupt the current animation to update orientation
        if (current_capacity_priority != null && current_capacity_priority.lock_orientation)
        {
            if (log_orientation) { Debug.LogWarning("(AnimPlayer) Orientation change to " + orientation
                + " is locked by the current animation: " + current_capacity_priority.capacity_playing); }
            return;
        }
        
        // we play the animation again with the right orientation
        Anim new_anim = Play(current_capacity);
        if (new_anim == null) { return; }
        if (log_orientation)
        {
            string s = "(AnimPlayer) Interrupted : Changing orientation to ";
            s += this.orientation + " | anim switched to ";
            s += new_anim.name;
            Debug.Log(s);
        }
    }
}


[Serializable] public class AnimCapacityPriority
{
    [HideInInspector] public int priority = 0;
    public string capacity_playing = "";
    public List<string> capacities;
    public bool lock_orientation = false;
    public bool one_shot = false;

    public AnimCapacityPriority(List<string> capacities)
    {
        this.capacities = capacities;
    }
}