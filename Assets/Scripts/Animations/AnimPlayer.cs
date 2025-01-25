using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimPlayer : MonoBehaviour
{
    [Header("Components")]
    private AnimBank bank;
    private SpriteRenderer sr;


    [Header("Skin")]
    public string skin;


    [Header("Current Animation")]
    public Anim current_anim = null;
    private Sprite[] current_sprites;
    private float frame_timer = 0f;
    private int current_frame = 0;

    // Start is called before the first frame update
    private void Start()
    {
        if (bank == null)
        {
            bank = GameObject.Find("/utils/bank").GetComponent<AnimBank>();
        }

        // we get the sprite renderer
        sr = GetComponent<SpriteRenderer>();

        // we play the idle animation
        Play("idle");
    }

    // EVENTS
    private void Events()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Play("run","U");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Play("run", "D");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Play("run", "L");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Play("run", "R");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Play("attack");
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Play("hurted");
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Play("dash");
        }
    }

    // Update
    private void Update()
    {
        Events();

        // we play the current animation
        if (current_anim != null) { UpdateAnim(); }

        // we play idle if no animation is playing
        if (current_anim == null) { Play("idle"); }

    }
    private void UpdateAnim()
    {
        frame_timer += Time.deltaTime;
        if (frame_timer >= current_anim.frame_times[current_frame] / current_anim.speed)
        {
            // we go to the next frame
            frame_timer = 0f;
            current_frame++;
            if (current_frame >= current_anim.frame_times.Length)
            {
                // we go to the next animation
                if (current_anim.loop)
                {
                    current_frame = 0;
                }
                else
                {
                    current_anim = null;
                    return;
                }
            }
            sr.sprite = current_sprites[current_frame];
        }
    }


    // PLAY ANIMATION
    public bool Play(string capacity, string orientation = "LR",float speed = default, bool? loop = null, int priority = default)
    {

        // 1 - WE CHECK IF WE CAN PLAY THE ANIMATION

        // we get the animation from the bank (it gives us the closest anim possible if the exact one doesn't exist)
        string anim_name = skin + "." + capacity + "." + (orientation == "L" || orientation == "R" ? "LR" : orientation);
        Anim anim = bank.GetAnim(anim_name);
        if (anim == null) { return false; }

        // we set the data of the animation
        anim.speed = speed == default ? anim.speed : speed;
        if (loop.HasValue) {anim.loop = loop.Value;}
        anim.priority = priority == default ? anim.priority : priority;

        // we check if we are already playing an animation with a higher priority
        if (current_anim != null && current_anim.priority > anim.priority) { return false; }



        // 2 - WE PLAY THE ANIMATION

        // we get the sprites and frame times
        current_sprites = bank.GetSprites(anim.sprites_paths);
        current_anim = anim;
        current_frame = 0;
        frame_timer = 0f;

        // we set the first sprite
        sr.sprite = current_sprites[current_frame];

        // we check the orientation
        if (orientation == "R") { sr.flipX = true; }
        else { sr.flipX = false; }

        return true;
    }
}