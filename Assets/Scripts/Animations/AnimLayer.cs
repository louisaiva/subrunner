using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimLayer : MonoBehaviour
{

    [Header("ANIM LAYER")]
    public string skin;
    public AnimPlayer leader;

    [Header("Components")]
    private SpriteRenderer _sr;
    private SpriteRenderer sr
    {
        get
        {
            if (_sr == null) { _sr = GetComponent<SpriteRenderer>(); }
            return _sr;
        }
    }
    public SpriteRenderer Renderer { get { return sr; } }

    [Header("Current Animation")]
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over
    private bool update_each_frame = true; // if false, it means that the animation is a single frame anim, we don't want to check it each frame
    [SerializeField] private bool follow_duration = false; // if true, the layer will sync its speed with the leader's one
    [SerializeField] private bool never_flip = false;
    private bool choose_perfect_orientation_if_available =  true;
    // if true, when the leader changes Orientation, the layer will
    // try to stick to the perfect new orientation instead of following
    // the leader's one. this is useful when the leader's skin does not
    // have the orientation (ex: steel_door has no U, only D because 
    // it is the same, but we need the door_icon to be able to switch
    // to U even if the leader sticks to D). if false we always
    // follow the leader's current anim orientation.




    [Header("Logs")]
    public bool log = false;
    public bool log_frames = false;
    public bool log_assign = false;

    private void Awake()
    {
        if (leader != null) { AssignLeader(leader); }
        bool a = visible_on;
    }

    // ASSING LEADER
    [SerializeField] private bool already_assigned = false;
    public void AssignLeader(AnimPlayer leader)
    {
        if (already_assigned) { return; }
        this.leader = leader;
        leader.OnAnimPlayedAtFrame += PlayAtFrame;
        leader.OnOrientationChanged += SetOrientation;
        leader.OnHidden += Hide;
        leader.OnShown += Show;
        leader.RegisterAnimLayer(this);
        already_assigned = true;
        if (log_assign) { Debug.Log("(AnimLayer) Assigned leader " + leader.name + " to layer " + name); }
    }
    public void UnassignLeader()
    {
        if (!already_assigned) { return; }
        leader.OnAnimPlayedAtFrame -= PlayAtFrame;
        leader.OnOrientationChanged -= SetOrientation;
        leader.OnHidden -= Hide;
        leader.OnShown -= Show;
        leader.UnregisterAnimLayer(this);
        this.leader = null;
        already_assigned = false;
        if (log_assign) { Debug.Log("(AnimLayer) Unassigned leader from layer " + name); }
    }


    // PLAY ANIM
    private void PlayAtFrame(string anim_name, int frame, float duration_override)
    {
        // we replace our skin
        string anim_skin = anim_name.Split('.')[0]; // we get the skin of the anim name, we split by dot and we take the first word (which is the skin)
        anim_name = skin + anim_name[anim_skin.Length..]; // replace the current anim skin by our layer skin

        // we get the anim
        Anim anim = AnimBank.Instance.GetAnim(anim_name, return_empty_if_not_found: true);

        // we play it at frame
        play_now_at_frame_with_duration(anim, frame, duration_override);
    }


    // PLAY ANIM LOW LEVEL
    private void Update()
    {
        if (log_frames && current_anim != null)
        {
            Debug.Log("(AnimLayer) Current frame: " + current_frame + " Current anim: " + current_anim.name);
        }

        // we play the current animation
        if (current_frame == -1) { return; }
        if (current_frame >= current_anim.sprites.Length) { return; }
        if (!update_each_frame) { return; }
        updateAnim();
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
        
    }
    private void play_now_at_frame_with_duration(Anim anim, int frame = 0, float duration_override = -1f)
    {
        // we check if the duration is overriden
        if (follow_duration && duration_override != 1f)
        {
            // we get the duration of the animation
            float duration = anim.GetBaseDuration();

            // we calculate the resulting speed
            anim.speed = duration / (float)duration_override;
        }
        else { anim.speed = 1f; }

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
        if (never_flip) { sr.flipX = false; }
        else if (anim.flipX && !sr.flipX) { sr.flipX = true; }
        else if (!anim.flipX && sr.flipX) { sr.flipX = false; }

        // we check if this anim is a single frame anim
        update_each_frame = !(anim.sprites.Length == 1);

        if (log) { Debug.Log("(AnimLayer) Playing " + anim.name + " at frame " + frame + " flipX: " + anim.flipX + $" (never_flip: {never_flip})"); }
    }



    // RENDERER MANAGEMENT / VISIBILITY
    public void DisableRenderer() { sr.enabled = false; } // ? obsolete ???
    public void EnableRenderer() { sr.enabled = true; } // ? obsolete ???

    [Header("Visibility (debug only)")]
    [SerializeField] private bool visible_on; // RTO
    public void Hide()
    {
        visible_on = false;
        if (visibleKeyword == null) { return; }
        material.SetKeyword(visibleKeyword.Value, false);
    }
    public void Show()
    {
        visible_on = true;
        if (visibleKeyword == null) { return; }
        material.SetKeyword(visibleKeyword.Value, true);
    }



    // ORIENTATION MANAGEMENT
    public void SetOrientation(string orientation)
    {
        if (!choose_perfect_orientation_if_available) { return; } // we don't care about orientation, we will have the new anim's one, so we do nothing

        float duration_override = current_anim == null ? -1f : current_anim.GetDuration();
        string current_capacity = current_anim == null ? "" : current_anim.capacity;

        PlayAtFrame($"_.{current_capacity}.{orientation}", current_frame, duration_override); // we try to play the idle anim of the new orientation, if it exists it means that we have a perfect orientation for this new direction and we will switch to it, otherwise we will keep the current anim which is the closest one to the leader's one

        if (log) { Debug.Log($"(AnimLayer) Set orientation to {orientation} (perfect orientation: {current_anim.name})"); }
    }





    // LOAD DATA
    private Material material;
    private LocalKeyword? visibleKeyword;
    public void LoadData(AnimLayerData layer_data)
    {
        // load main layer data
        name = $"layer_{layer_data.skin}";
        skin = layer_data.skin;
        transform.localPosition = layer_data.local_position;
        transform.localEulerAngles = layer_data.local_rotation;

        // load sr data
        Material mat = MaterialBank.GetMaterial(layer_data.material_name);
        bool is_see_through = mat.name.Contains("see_through");
        if (is_see_through)
        {
            Debug.LogWarning("(AnimLayer) See through material (" + mat.name + ") detected on layer " + name + " of skin " + skin + " on capable '" + (leader?.Capable == null ? "null" : leader.Capable.ID) + "'");
            sr.SetSharedMaterials(new List<Material> { mat });
        }
        else { sr.material = mat; }
        sr.sortingLayerID = layer_data.sorting_layer_id;
        sr.sortingOrder = layer_data.order_in_layer;

        // load never flip & follow duration
        never_flip = layer_data.never_flip;
        follow_duration = layer_data.follow_duration;

        if (is_see_through)
        {
            material = null;
            visibleKeyword = null;
            return;
        }

        
        // setup visibility keyword
        material = sr.material;
        if (has_material_keyword(mat, "_VISIBLE"))
        {
            visibleKeyword = new LocalKeyword(Renderer.material.shader, "_VISIBLE");
        }
        else { visibleKeyword = null; }
        visible_on = true;
    }
    private bool has_material_keyword(Material material, string name)
    {
        Shader shader = material.shader;
        LocalKeywordSpace keywordSpace = shader.keywordSpace;
        foreach (LocalKeyword localKeyword in keywordSpace.keywords)
        {
            if (localKeyword.name == name) { return true; }
        }
        return false;
    }

    // GET STATIC DATA
    public AnimLayerData GetStaticData()
    {
        AnimLayerData data = new AnimLayerData
        {
            // load basic layer data
            skin = skin,
            local_position = transform.localPosition,
            never_flip = never_flip,
            follow_duration = follow_duration,
            local_rotation = transform.localEulerAngles,

            // load sr data
            sorting_layer_id = sr.sortingLayerID,
            order_in_layer = sr.sortingOrder
        };

        data.material_name = MaterialBank.GetMaterialName(Renderer, name);

        return data;
    }

}


[Serializable] public class AnimLayerData
{
    public string skin;
    public Vector2 local_position;
    public Vector3 local_rotation;

    // layer sr data
    public string material_name;
    public int sorting_layer_id;
    public int order_in_layer;
    public bool never_flip;
    public bool follow_duration;

    public AnimLayerData Duplicate()
    {

        AnimLayerData new_data = new AnimLayerData
        {
            skin = this.skin,
            local_position = this.local_position,
            local_rotation = this.local_rotation,
            material_name = this.material_name,
            sorting_layer_id = this.sorting_layer_id,
            order_in_layer = this.order_in_layer,
            never_flip = this.never_flip,
            follow_duration = this.follow_duration
        };
        
        return new_data;
    }

    public string GetDetails()
    {
        string details = $"layer skin {skin} :\n";
        details += $"          - local_position : {local_position}\n";
        details += $"          - local_rotation : {local_rotation}\n";
        details += $"          - material_name : {material_name}\n";
        details += $"          - sorting_layer_id : {sorting_layer_id}\n";
        details += $"          - order_in_layer : {order_in_layer}\n";
        details += $"          - never_flip : {never_flip}\n";
        details += $"          - follow_duration : {follow_duration}\n";
        return details;
    }
}
