using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimPlayer : MonoBehaviour
{



    [Header("Components")]
    // private AnimBank bank;
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

    /* [Header("Animation Pile")]
    public List<string> anim_pile = new();
    private Dictionary<string, int> capacity_priorities = new()
            {
            {"idle",0},
            {"walk",1},
            {"run",1},
            {"hover",1},
            {"idle_open",2},
            {"idle_sleep",2},
            {"hack",3},
            {"attack",4},
            {"spawn",4},
            {"open",4},
            {"close",4},
            {"dodge",4},
            {"hurted",5},
            {"eat",5},
            {"fell_asleep",5},
            {"wake_up",5},
            {"lick_foot",5},
            {"throw",6},
            {"die",6}};
    // todo à transformer en List<CapacityPriority> sans MonoBehaviour pour pouvoir les éditer dans l'éditeur

    [SerializeField] private List<int> animation_priorities_with_no_loop = new() { 4, 5 };
    // we never loop the animation if it's in this priority (attack, dodge, hurted) -> always play once
    [SerializeField] private List<int> animation_priorities_with_static_orientation = new() { 4, 5 }; */
    // we can't interrupt the animation for switching orientation if it's in this list (wait the end of the anim before changing orientation)
    // only for orientation, not for switching to another animation (ex we can interrupt dodge to attack bcz they have the same priority 3)

    [Header("Animation Pile")]
    [SerializeField] private string capacity_pile = ""; // a string with the capacities separated by commas, ex: "walk,run,idle"
    // todo is there any bugs with this transition system ?

    [Header("Logs")]
    public bool log = false;
    public bool log_orientation = false;
    public bool log_advanced = false;
    public bool log_frames = false;
    public bool log_pile = false;




    // Start is called before the first frame update
    private void Start()
    {
        // we get the sprite renderer
        sr = GetComponent<SpriteRenderer>();

        // we inspect the capacity_priorities and we create the anim_pile
        /* float highest_priority = capacity_priorities.Values.Max();
        for (int i = 0; i <= highest_priority; i++)
        {
            anim_pile.Add("");
        } */

        // we inform each anim capacity priority of its priority
        for (int i = 0; i < anim_capacity_priorities.Count; i++)
        {
            anim_capacity_priorities[i].priority = i;
        }

        // we play the idle animation
        Play("idle");
    }


    // Update
    private void Update()
    {
        if (log_frames && current_anim != null)
        {
            Debug.Log("(AnimPlayer) Current frame: " + current_frame + " Current anim: " + current_anim.name);
        }

        // we play the current animation
        if (current_frame != -1)
        {
            if (update_each_frame) { updateAnim(); }
            // return;
        }

        // we play the highest animation in the pile if it's not the current one
        // playFromPile();
        // playNextAnim();
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
        // current_anim = null;
        // current_capacity = "";

        // we check if it's looping or not
        if (current_capacity_priority.one_shot)
        {
            current_capacity_priority.capacity_playing = "";
        }

        // we check if the priority of the current animation is in the list of priorities with no loop
        // int current_anim_priority = capacity_priorities[current_capacity];
        // if (animation_priorities_with_no_loop.Contains(current_anim_priority)) { removeFromPile(current_capacity); }
        // we check if it is looping or not (if not, we remove it from the pile)
        // else if (!current_anim.loop) { removeFromPile(current_capacity); }

        // we play the highest animation in the pile
        playNextAnim();
    }


    // PLAY ANIMATION
    public Anim Play(string capacity/* , int? priority_override = null */, float duration_override = default)
    {
        if (AnimBank.Instance == null) { return null; }

        /* // we get the priority of the capacity
        int priority = 1;
        if (priority_override != null)
        {
            priority = (int)priority_override;
            if (log_pile) { Debug.Log("(AnimPlayer - Play) The capacity " + capacity + " has a priority override: " + priority); }
            capacity_priorities[capacity] = priority;
        }
        else if (priority_override == null && capacity_priorities.ContainsKey(capacity))
        {
            priority = capacity_priorities[capacity];
            if (log_pile) { Debug.Log("(AnimPlayer - Play) The capacity " + capacity + " has a priority: " + priority + " from the capacity_priorities dictionnary"); }
        }
        else
        {
            if (log_pile) { Debug.Log("(AnimPlayer - Play) The capacity " + capacity + " doesn't exist in the capacity_priorities dictionnary. Priority 1 applied by default"); }
            capacity_priorities[capacity] = priority;
        } */

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
        }

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
        


        /* // we check if we can play the animation RIGHT NOW
        if (priority >= getPileMaxPriority())
        {
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

            // we check if the duration is overriden
            if (duration_override != null)
            {
                // we get the duration of the animation
                float duration = anim.GetBaseDuration();

                // we calculate the resulting speed
                anim.speed = duration / (float)duration_override;
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

        // we can't play the animation right now BUT we add the animation to the pile
        // anim_pile[priority] = capacity;
        // if (log_pile) { Debug.LogWarning("(AnimPlayer - Play) Adding " + capacity + " to the pile at priority " + priority); }

        // we didn't play the animation so we return null
        return null; */
    }
    private void playNextAnim()
    {
        string capacity = "";
        AnimCapacityPriority capacity_priority = null;

        // we check if we have a capacity in the pile
        if (capacity_pile != "")
        {
            capacity = capacity_pile.Split(',').FirstOrDefault();
            capacity_priority = getAnimCapacityPriority(capacity);
            capacity_pile = capacity_pile.Remove(0, capacity.Length + 1); // remove the first capacity and the comma after it
        }
        else
        {
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

        // we play the animation    
        play_now_at_frame(anim);

        // we set the current capacity
        current_capacity = capacity;
        current_capacity_priority = capacity_priority;
        current_capacity_priority.capacity_playing = capacity;

    }
    /* private void playFromPile()
    {
        if (AnimBank.Instance == null) { return; }

        for (int i = anim_pile.Count - 1; i >= 0; i--)
        {
            // we check if there is an animation to play
            if (anim_pile[i] == "") { continue; }

            // we get the closest animation from the bank
            string anim_name = skin + "." + anim_pile[i] + "." + orientation;
            Anim anim = AnimBank.Instance.GetAnim(anim_name);
            if (log_pile)
            {
                Debug.Log("(AnimPlayer - playFromPile) Bank found anim : " + anim.name
                    + (anim_name == anim.name
                    ? ""
                    : " (" + anim_name + " was asked)"));
            }

            // we set the current capacity
            current_capacity = anim_pile[i];

            // we play the animation
            play_now_at_frame(anim);
            return;
        }
    } */
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
        // we check if the capacity is in the pile
        // if (!anim_pile.Contains(capacity)) { return; }

        // we remove the capacity from the pile
        // removeFromPile(capacity);

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
        // if we were playing the capacity, we stop it
        // except if dont_stop_if_currently_playing == true

        // if we are here, it means we need to stop the animation from playing directly
        // current_capacity_priority = null;

        // playFromPile();
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
    /* private int getPileMaxPriority()
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
                if (log_pile) { Debug.Log("(AnimPlayer) Removing " + capacity + " from the pile"); }
                return;
            }
        }
    } */

    private AnimCapacityPriority getAnimCapacityPriority(string capacity)
    {
        return anim_capacity_priorities.FirstOrDefault(p => p.capacities.Contains(capacity));
    }
    public void AddToPile(string capacity)
    {
        // we add the capacity to the pile
        capacity_pile += capacity + ",";

        if (log_pile) { Debug.Log("(AnimPlayer - AddToPile) Adding " + capacity + " to the pile"); }
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
        //if (new List<string> { "U", "D", "L", "R" }.Contains(orientation)){
        this.orientation = orientation;//}

        // we check if we can interrupt the current animation to update orientation
        if (current_capacity_priority != null && current_capacity_priority.lock_orientation)
        {
            if (log_orientation) { Debug.LogWarning("(AnimPlayer) Orientation change to " + orientation
                + " is locked by the current animation: " + current_capacity_priority.capacity_playing); }
            return;
        }
        /* if (current_capacity == "") { return; }
        int current_anim_priority = capacity_priorities[current_capacity];
        if (animation_priorities_with_static_orientation.Contains(current_anim_priority)) { return; } */

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