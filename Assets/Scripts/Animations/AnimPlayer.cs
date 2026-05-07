using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;



#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class AnimPlayer : MonoBehaviour
{

    [Header("Components")]
    private SpriteRenderer _sr;
    public SpriteRenderer Renderer
    {
        get
        {
            if (_sr == null) { _sr = GetComponent<SpriteRenderer>(); }
            return _sr;
        }
    }
    private Capable _capable = null;
    public Capable Capable
    {
        get
        {
            if (_capable == null && transform.parent != null) { _capable = transform.parent.GetComponent<Capable>(); }
            if (_capable == null) { _capable = GetComponent<Capable>(); }
            return _capable;
        }
    }

    [Header("Skin")]
    [SerializeField] private string skin;
    public string Skin
    {
        get { return skin; }
        set
        {
            if (value == skin) { return; }
            skin = value;
            OnSkinChange.Invoke(skin);
            Play(current_capacity);
        }
    }
    public event Action<string> OnSkinChange = delegate { };

    [Header("Orientation")]
    [field:SerializeField] public string orientation { get; private set; } = "D";
    public Action<string> OnOrientationChanged = delegate { };


    [Header("Current Animation")]
    private string current_capacity = "";
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over
    [SerializeField] private bool update_each_frame = true; // if false, it means that the animation is a single frame anim, we don't want to check it each frame
    public Action<string, int, float> OnAnimPlayedAtFrame = delegate { };


    [Header("Anim Capacity Priorities")]
    [SerializeField] private List<AnimCapacityPriority> anim_capacity_priorities = new();
    private AnimCapacityPriority current_capacity_priority = null;

    [Header("Parameters")]
    [SerializeField] private bool never_flip = false;

    [Header("Logs")]
    public bool log = false;
    public bool log_orientation = false;
    public bool log_advanced = false;
    public bool log_frames = false;
    public bool log_pile = false;


    ///
    //
    /// AWAKE & START
    //
    ///

    // AWAKE & START
    private void Awake()
    {

        // todo maybe this won't work with CapableSystem because we load anim data after instantiating
        // the capable prefab with anim player.. so maybe it won't work -> if yes then we need to move it
        // to LoadPlayerData + another small little method for capable that are not in capable system + bool loaded_data
        // check how we did it in animlayer

        // we inform each anim capacity priority of its priority
        re_index_priorities();
    }
    private void re_index_priorities()
    {
        for (int i = 0; i < anim_capacity_priorities.Count; i++)
        {
            anim_capacity_priorities[i].priority = i;
        }
    }
    private void Start()
    {
        // if the capable is not in the capable system, we play idle
        if (!CapableBank.Instance.HasCapable(Capable)) { AddToPile("idle"); }
    }






    ///
    //
    /// MAIN PLAY / STOP METHODS
    //
    ///


    // PLAY ANIMATION
    public void VoidPlay(string capacity) { Play(capacity); }
    public Anim Play(string capacity, float duration_override = default)
    {
        if (AnimBank.Instance == null) { return null; }

        AnimCapacityPriority capacity_priority = getAnimCapacityPriority(capacity);
        if (capacity_priority == null)
        {
            if (log) { Debug.LogWarning($"(AnimPlayer - {Capable.name}) The capacity " + capacity + " doesn't exist in the anim_capacity_priorities list"); }
            return null;
        }

        // we check if the index is < than current prio we don't play it
        if (current_capacity_priority != null && capacity_priority.priority < current_capacity_priority.priority)
        {
            if (log_pile) { Debug.LogWarning($"(AnimPlayer - {Capable.name}) The capacity " + capacity + " has a lower priority than the current one (" + current_capacity_priority.priority + ")"); }
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
            Debug.Log($"(AnimPlayer - {Capable.name}) Bank found anim : " + anim.name
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
        else { anim.speed = 1f; }


        // we check if the animation is not actually playing
        if (current_anim != null && anim.name == current_anim.name && current_frame != -1)
        {
            if (log)
            {
                Debug.LogWarning($"(AnimPlayer - {Capable.name}) The animation " + anim.name + " is already playing at frame " + current_frame);
            }
            return current_anim;
        }

        // we play the animation
        OnAnimPlayedAtFrame.Invoke(anim_name, 0, duration_override);
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

        // verify that we have something
        if (capacity == "") { capacity = "idle"; capacity_priority = getAnimCapacityPriority("idle"); }

        // we get the animation from the bank
        string anim_name = skin + "." + capacity + "." + orientation;
        Anim anim = AnimBank.Instance.GetAnim(anim_name);
        if (log)
        {
            Debug.Log($"(AnimPlayer - {Capable.name}) Bank found anim : " + anim.name
                + (anim_name == anim.name
                ? ""
                : " (" + anim_name + " was asked)"));
        }

        // we check if this anim is a single frame anim
        update_each_frame = !(anim.sprites.Length == 1);

        // we play the animation
        OnAnimPlayedAtFrame.Invoke(anim_name, 0, 1f);
        play_now_at_frame(anim);

        // we set the current capacity
        current_capacity = capacity;
        current_capacity_priority = capacity_priority;
        current_capacity_priority.capacity_playing = capacity;
    }

    /// <summary>
    /// play the animation with the right orientation according to the look_at vector.
    /// After playing the anim the orientation will be reset to the previous orientation,
    /// so this look_at orientation is only an override for this animation.
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="look_at"></param>
    /// <returns></returns>
    public Anim PlayWithOrientation(string capacity, Vector2 look_at)
    {
        string old_orientation = this.orientation;
        SetOrientation(look_at);
        Anim anim = Play(capacity);
        setOrientation(old_orientation);
        return anim;
     }

    // STOP ANIMATION
    public void StopPlaying(string capacity) { StopPlaying(capacity, false); }
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






    ///
    //
    /// UPDATE, LOW LEVEL PLAYING & PILE MANAGEMENT
    //
    ///

    // PLAY ANIM LOW LEVEL
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
            Renderer.sprite = current_anim.sprites[current_frame];
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
        Renderer.sprite = anim.sprites[current_frame];

        // we flip the sprite renderer if needed
        if (never_flip) { Renderer.flipX = false; }
        else if (anim.flipX && !Renderer.flipX) { Renderer.flipX = true; }
        else if (!anim.flipX && Renderer.flipX) { Renderer.flipX = false; }

        if (log_advanced) { Debug.Log("(AnimPlayer) Playing " + anim.name + " at frame " + frame + " flipX: " + anim.flipX); }
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
    public void ClearIdles()
    {
        // we go through all the anim capa prio and check which anims she is playing
        for (int i = 0; i < anim_capacity_priorities.Count; i++)
        {
            AnimCapacityPriority priority = anim_capacity_priorities[i];
            if (priority.capacity_playing.StartsWith("idle")) { StopPlaying(priority.capacity_playing); }
        }
    }






    ///
    //
    /// GETTERS
    //
    ///

    /// <summary>
    /// returns true if the capacity is actually
    /// the main playing animation. This means
    /// it only returns true if current_capacity is set
    /// and equals to capacity
    /// </summary>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public bool IsShowing(string capacity)
    {
        if (string.IsNullOrEmpty(current_capacity)) { return false; }
        return current_capacity == capacity;
    }


    /// <summary>
    /// returns true if the capacity is somewhere inside
    /// the animcapacity priority list. If not found it returns false.
    /// This means that the method could return true even if the AnimPlayer.current_capacity
    /// is not the one passed in parameter, because the capacity can be playing
    /// but another capacity is playing on top of it so it's hidden beneath.
    /// </summary>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public bool IsPlaying(string capacity)
    {
        AnimCapacityPriority priority = getAnimCapacityPriority(capacity);
        if (priority == null)
        {
            if (log_pile) { Debug.LogWarning("(AnimPlayer - IsPlaying) The capacity " + capacity + " doesn't exist in the anim_capacity_priorities list"); }
            return false;
        }

        return priority.capacity_playing == capacity;
    }


    /// <summary>
    /// returns the current percentage done
    /// of the current animation. If no animation is playing, it returns -1f.
    /// </summary>
    /// <returns>the completed percentage of the current animation playing. equals to -1f if no anim is playing</returns>
    public float GetCurrentAnimPercentDone()
    {
        // we check if we are playing an animation
        if (current_anim == null || current_frame == -1) { return -1f; }

        // we get the total duration of the animation
        float anim_duration = current_anim.GetDuration();
        float time_played = current_anim.GetDurationUntilFrame(current_frame) + frame_timer;
        return time_played / anim_duration;
    }






    ///
    //
    /// SPECIFIC EDGE CASE METHODS
    //
    ///

    // ORIENTATION & FLIP
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

        // OnOrientationChange?.Invoke(look_at);
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
            if (log_orientation)
            {
                Debug.LogWarning("(AnimPlayer) Orientation change to " + orientation
                + " is locked by the current animation: " + current_capacity_priority.capacity_playing);
            }
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
        OnOrientationChanged?.Invoke(orientation);
    }
    /// <summary>
    /// this method is different from SetOrientation because
    /// it does not override the orientation. This means the
    /// flip will only affect the current animation ! Very
    /// useful for the 'hurted' anim for example; where we
    /// want the anim to face the opposite of knockabk direction even
    /// if we are looking in the direction of the knockback
    /// </summary>
    /// <param name="flipX">the direction to flip the animation</param>
    public void FlipCurrentAnim(bool flipX)
    {
        Renderer.flipX = flipX;

        // we flip all the anim layers renderers
        for (int i = 0; i < anim_layers.Count; i++)
        {
            anim_layers[i].Renderer.flipX = flipX;
        }
    }


    // LAYERS & SPRITE RENDERER MANAGEMENT
    private List<AnimLayer> anim_layers = new List<AnimLayer>();
    public void RegisterAnimLayer(AnimLayer anim_layer)
    {
        if (anim_layers.Contains(anim_layer)) { return; }
        anim_layers.Add(anim_layer);
    }
    public void UnregisterAnimLayer(AnimLayer anim_layer)
    {
        if (!anim_layers.Contains(anim_layer)) { return; }
        anim_layers.Remove(anim_layer);
    }
    /* public void DisableRenderer()
    {
        Renderer.enabled = false;

        // we disable all the anim layers renderers
        for (int i = 0; i < anim_layers.Count; i++)
        {
            anim_layers[i].DisableRenderer();
        }
    }
    public void EnableRenderer()
    {
        Renderer.enabled = true;

        // we enable all the anim layers renderers
        for (int i = 0; i < anim_layers.Count; i++)
        {
            anim_layers[i].EnableRenderer();
        }
    } */
    public List<AnimLayer> GetAnimLayers()
    {
        return new List<AnimLayer>(anim_layers);
    }
    public List<AnimLayer> GetStaticAnimLayers()
    {
        return transform.GetComponentsInChildren<AnimLayer>(includeInactive: true).ToList();
    }


    // RENDERER VISIBILITY
    public event Action OnHidden = delegate { };
    public event Action OnShown = delegate { };
    [Header("Visibility (debug only)")]
    [SerializeField] private bool visible_on = true; // RTO
    public void Hide()
    {
        if (!visible_on) { return; }

        // we set the material Visible bool to false
        Renderer.material.SetKeyword(visibleKeyword, false);
        OnHidden?.Invoke();
        visible_on = false;
        if (log) { Debug.Log("(AnimPlayer) " + Capable.ID + " is now hidden"); }
    }
    public void Show()
    {
        if (visible_on) { return; }

        // we set the material Visible bool to true
        Renderer.material.SetKeyword(visibleKeyword, true);
        OnShown?.Invoke();
        visible_on = true;
        if (log) { Debug.Log("(AnimPlayer) " + Capable.ID + " is now visible"); }
    }
    public bool IsVisible()
    {
        return Renderer.material.IsKeywordEnabled(visibleKeyword);
    }







    ///
    //
    /// DATA MANAGEMENT
    //
    ///


    // LOAD DATA
    private LocalKeyword visibleKeyword;
    public void LoadPlayerData(AnimPlayerData data)
    {
        if (CapableBank.Instance.LayerBank.log_anim_layers) { Debug.Log($"(AnimPlayer) {name}'s loading data : {(data != null ? data.GetDetails() : "null")}"); }

        transform.localPosition = data.local_position;

        // we clear runtime data
        _capable = null;
        current_anim = null;
        current_capacity = "";
        current_capacity_priority = null;
        current_frame = -1;
        frame_timer = 0f;

        // ! does not load layers !! but we don't want to it's inside CapableBank because we pool them
        if (data.anim_capacity_priorities == null || data.anim_capacity_priorities.Count == 0) { return; }
        skin = data.skin;
        current_capacity = data.current_capacity;
        anim_capacity_priorities = data.anim_capacity_priorities;

        // we load the sr data
        if (CapableBank.Instance.LayerBank.log_anim_player) { Debug.Log($"(AnimPlayer) {data.skin}'s data default material is {data.material_path}"); }
        Renderer.material = Resources.Load<Material>(data.material_path);
        Renderer.sortingLayerID = data.sorting_layer_id;
        Renderer.sortingOrder = data.order_in_layer;

        // and hide it by default
        visibleKeyword = new LocalKeyword(Renderer.material.shader, "_VISIBLE");
        visible_on = true;
        Hide();

        // and parameters
        never_flip = data.never_flip;

        // we inform each anim capacity priority of its priority
        re_index_priorities();

        // play current capacity
        if (!string.IsNullOrEmpty(current_capacity)) { Play(current_capacity); }
        else { AddToPile("idle"); }
    }

    // SAVE DATA
    public void SaveDynamicPlayerData(AnimPlayerData data)
    {
        // we save data
        data.current_capacity = current_capacity;
    }


    // GET STATIC DATA

    /// <summary>
    /// just as other GetStaticData() methods (ie Capable's one), this method
    /// is not meant to be run in a BUILD !!! IT WON T WORK because it does not
    /// update the anim_data, it creates a new data based from actual static
    /// variables states of the object. if run inside a build, it could overwrite
    /// some data such as material paths which would break the save.
    /// </summary>
    /// <returns></returns>
    public AnimPlayerData GetStaticAnimData()
    {
        // get basic player data
        AnimPlayerData data = new AnimPlayerData
        {
            skin = skin,
            anim_capacity_priorities = anim_capacity_priorities,

            sorting_layer_id = Renderer.sortingLayerID,
            order_in_layer = Renderer.sortingOrder,

            // and local position
            local_position = get_static_local_position(),

            // and parameters
            never_flip = never_flip
        };

        string material_path = get_material_path(Renderer);
        if (!string.IsNullOrEmpty(material_path))
        {
            data.material_path = material_path;
        }
        else if (Capable.data != null && Capable.data.anim_data != null && !string.IsNullOrEmpty(Capable.data.anim_data.material_path))
        {
            data.material_path = Capable.data.anim_data.material_path;
            if (CapableBank.Instance.LayerBank.log_anim_player) { Debug.LogWarning($"(AnimPlayer - {Capable.ID}) NO MATERIAL found on get_material_path(), we use the old data as FALLBACK : {data.material_path}"); }
        }
        else
        {
            data.material_path = "materials/objects";
            if (CapableBank.Instance.LayerBank.log_anim_player) { Debug.LogWarning($"(AnimPlayer - {Capable.ID}) NO MATERIAL found on get_material_path() and NO FALLBACK available, returning DEFAULT {data.material_path}"); }
        }


        // get the layers by going through the hierarchy (so we can do it even when not playing)
        List<AnimLayerData> layers_data = new List<AnimLayerData>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform layer_transform = transform.GetChild(i);
            AnimLayer anim_layer = layer_transform.GetComponent<AnimLayer>();
            if (anim_layer == null) { continue; }
            layers_data.Add(anim_layer.GetStaticData());
        }

        data.layers = layers_data;
        return data;
    }
    private Vector2 get_static_local_position()
    {
        if (GetComponent<Capable>() != null)
        {
            return Vector2.zero;
            // if we have a capable on it, it means we are at the top of the capable hierarchy,
            // so our local pos is a world pos in fact. that's why we return zero, because when
            // the capable will be constructed by the CapableBank, it will receive an "anim_player"
            // transform which is a direct child of the capable. and so if we return the world pos
            // it will move the anim player FFAAAAR AWAY from the capable, which is not what we want !
        }
        return transform.localPosition;
    }
    private string get_material_path(SpriteRenderer sr)
    {
        if (sr == null || sr.sharedMaterial == null)
        {
            if (CapableBank.Instance.LayerBank.log_anim_player) { Debug.LogWarning($"(AnimPlayer - {Capable.ID}) The SpriteRenderer or its material is null, returning empty material path"); }
            return "";
        }
        #if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(sr.sharedMaterial);
        if (CapableBank.Instance.LayerBank.log_anim_player) { Debug.Log($"(AnimPlayer - {Capable.ID}) get_material_path(UNITYEDITOR) found a material at path: {path}"); }
        #else
        string path = "";
        if (CapableBank.Instance.LayerBank.log_anim_player) { Debug.Log($"(AnimPlayer - {Capable.ID}) get_material_path(NO EDITOR) found no material path ://"); }
        #endif

        // we need to remove ".mat" from path
        path = path.Replace(".mat", "");
        path = path.Replace("Assets/Resources/", ""); // we also need to remove "Assets/Resources/" from the path

        return path;
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

    public AnimCapacityPriority Duplicate()
    {
        AnimCapacityPriority new_priority = new AnimCapacityPriority(new List<string>(this.capacities))
        {
            priority = this.priority,
            capacity_playing = "", // capacity playing is runtime only, means we don't duplicate it
            lock_orientation = this.lock_orientation,
            one_shot = this.one_shot
        };
        return new_priority;
    }
}


// ANIMATIONS
[Serializable] public class AnimPlayerData
{
    public string skin_kind;
    public string skin;
    public List<AnimCapacityPriority> anim_capacity_priorities;
    public string current_capacity; // runtime only

    // player sr data
    public string material_path;
    public int sorting_layer_id;
    public int order_in_layer;
    public bool never_flip;

    // layers
    public List<AnimLayerData> layers;

    // position
    public Vector2 local_position;


    // GET & DUPLICATE
    public AnimPlayerData Duplicate()
    {
        AnimPlayerData new_data = new AnimPlayerData
        {
            skin = this.skin,
            current_capacity = this.current_capacity,
            local_position = this.local_position,
            material_path = this.material_path,
            sorting_layer_id = this.sorting_layer_id,
            order_in_layer = this.order_in_layer,
            never_flip = this.never_flip,
            layers = new List<AnimLayerData>(this.layers)
        };

        // duplicate anim_capacity_priorities
        if (this.anim_capacity_priorities != null)
        {
            new_data.anim_capacity_priorities = new List<AnimCapacityPriority>();
            foreach (AnimCapacityPriority acp in this.anim_capacity_priorities)
            {
                new_data.anim_capacity_priorities.Add(acp.Duplicate());
            }
        }
        else { new_data.anim_capacity_priorities = null; }

        return new_data;
    }
    public string GetDetails()
    {
        string details = $"anim_data :\n";
        details += $"     - skin : {skin}\n";
        details += $"     - current_capacity : {current_capacity}\n";
        if (anim_capacity_priorities != null) { details += $"     - anim_capacity_priorities : {anim_capacity_priorities.Count} priorities\n"; }
        else { details += $"     - anim_capacity_priorities : null\n"; }
        if (layers != null) { details += $"     - layers : {layers.Count} layers"; }
        else { details += $"     - layers : null"; }
        details += $"     - local_position : {local_position}\n";
        details += $"     - material_path : {material_path}\n";
        details += $"     - sorting_layer_id : {sorting_layer_id}\n";
        details += $"     - order_in_layer : {order_in_layer}\n";
        details += $"     - never_flip : {never_flip}\n";
        return details;
    }
}