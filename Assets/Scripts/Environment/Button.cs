using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationHandler))]
public class Button : MonoBehaviour, I_Interactable
{

    // BUTTON
    [SerializeField] protected bool is_pressed = false;
    [SerializeField] protected bool is_pressing = false;

    // ANIMATION
    protected AnimationHandler anim_handler;
    protected ButtonAnims anims = new ButtonAnims();
    protected float pressin_duration = 0.25f;

    // BUTTONABLE
    [SerializeField] protected GameObject buttonable = null;

    // unity functions
    protected virtual void Awake()
    {
        // on récupère l'animation handler
        anim_handler = GetComponent<AnimationHandler>();
        
        // on ferme le bouton
        is_pressed = false;
    }

    void Start()
    {
        success_switch_off();
    }

    // PRESSING
    protected void switch_on()
    {
        CancelInvoke();

        // on joue l'animation
        anim_handler.ChangeAnim(anims.switch_on, pressin_duration);

        // on invoque la fonction de succès
        Invoke("success_switch_on", pressin_duration);

        // on appuie sur le bouton
        is_pressing = true;
    }

    protected void success_switch_on()
    {
        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_on);

        // on appuie sur le bouton
        is_pressed = true;
        is_pressing = false;

        // on active le boutonable
        if (buttonable != null)
        {
            buttonable.GetComponent<I_Buttonable>().buttonDown();
        }
    }

    protected void switch_off()
    {
        CancelInvoke();

        // on joue l'animation
        anim_handler.ChangeAnim(anims.switch_off, pressin_duration);

        // on invoque la fonction de succès
        Invoke("success_switch_off", pressin_duration);

        // on appuie sur le bouton
        is_pressing = true;
    }

    protected void success_switch_off()
    {
        // on joue l'animation
        anim_handler.ChangeAnim(anims.idle_off);

        // on appuie sur le bouton
        is_pressed = false;
        is_pressing = false;

        // on desactive le boutonable
        if (buttonable != null)
        {
            buttonable.GetComponent<I_Buttonable>().buttonUp();
        }
    }

    // INTERACTION
    public bool isInteractable()
    {
        return !is_pressing;
    }

    public void interact()
    {
        if (!isInteractable()) { return; }
        
        // on appuie sur le bouton
        if (is_pressed)
        {
            switch_off();
        }
        else
        {
            switch_on();
        }
    }

    public void stopInteract()
    {
        if (!is_pressing) { return; }

        CancelInvoke();

        // on cancel l'interaction
        if (is_pressed)
        {
            success_switch_on();
        }
        else
        {
            success_switch_off();
        }
    }

}

public class ButtonAnims
{

    // ANIMATIONS
    public string idle_on = "button_idle_on";
    public string idle_off = "button_idle_off";
    public string switch_on = "button_switch_on";
    public string switch_off = "button_switch_off";

}