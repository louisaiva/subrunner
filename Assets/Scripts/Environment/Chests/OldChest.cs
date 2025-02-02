using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationHandler))]
public class OldChest : MonoBehaviour
{

    // ANIMATION
    protected AnimationHandler anim_handler;
    protected ChestAnims anims = new ChestAnims();

    [Header("OPENING")]
    [SerializeField] protected bool is_open = false;
    [SerializeField] protected bool is_moving = false;
    [SerializeField] protected float openin_duration = 0.5f;

    // unity functions
    protected virtual void Awake()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
    }

    // OPENING
    protected virtual void open()
    {
        CancelInvoke();

        // on joue l'animation
        anim_handler.ChangeAnim(anims.openin, openin_duration);

        // on ouvre le coffre
        Invoke("success_open", openin_duration);

        is_moving = true;
    }

    protected virtual void success_open()
    {
        // on ouvre le coffre
        is_open = true;
        is_moving = false;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_open);
    }

    protected virtual void close()
    {
        CancelInvoke();

        // on ferme le coffre
        is_moving = true;

        // on joue l'animation
        anim_handler.ChangeAnim(anims.closin, openin_duration);
        Invoke("success_close", openin_duration);

    }

    protected virtual void success_close()
    {
        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_closed);

        // on ferme le coffre
        is_open = false;
        is_moving = false;
    }

    // GETTERS
    public bool is_opened()
    {
        return is_open;
    }

}


public class ChestAnims
{

    // ANIMATIONS
    public string idle_open = "chest_idle_open";
    public string idle_closed = "chest_idle_closed";
    public string openin = "chest_openin";
    public string closin = "chest_closin";

}