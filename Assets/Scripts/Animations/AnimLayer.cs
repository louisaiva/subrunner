using UnityEngine;

public class AnimLayer : MonoBehaviour
{

    [Header("ANIM LAYER")]
    public string skin;
    public AnimPlayer leader;

    [Header("Components")]
    private SpriteRenderer sr;

    [Header("Current Animation")]
    public Anim current_anim = null;
    private float frame_timer = 0f;
    private int current_frame = -1; // if -1, the animation is over
    [SerializeField] private bool update_each_frame = true; // if false, it means that the animation is a single frame anim, we don't want to check it each frame

    [Header("Logs")]
    public bool log = false;
    public bool log_frames = false;

    // AWAKE
    private void Awake()
    {
        // we get the sprite renderer
        sr = GetComponent<SpriteRenderer>();

        leader.OnAnimPlayedAtFrame += PlayAtFrame;
        leader.RegisterAnimLayer(this);
    }

    // PLAY ANIM
    private void PlayAtFrame(string anim_name, int frame)
    {
        // we replace our skin
        anim_name = anim_name.Replace(leader.Skin, skin);

        // we get the anim
        Anim anim = AnimBank.Instance.GetAnim(anim_name);

        // we play it at frame
        play_now_at_frame(anim, frame);
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


        // if we are here, it means that we reached the end of the animation
        // the animation is over
        /* current_frame = -1;

        // we check if it's looping or not
        if (current_capacity_priority.one_shot)
        {
            current_capacity_priority.capacity_playing = "";
        }

        // we play the highest animation in the pile
        playNextAnim(); */
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

        // we check if this anim is a single frame anim
        update_each_frame = !(anim.sprites.Length == 1);

        if (log) { Debug.Log("(AnimLayer) Playing " + anim.name + " at frame " + frame + " flipX: " + anim.flipX); }
    }



    // RENDERER MANAGEMENT
    public void DisableRenderer() { sr.enabled = false; }
    public void EnableRenderer() { sr.enabled = true; }
}